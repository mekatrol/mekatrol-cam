use eframe::egui::{self, Button, RichText};
use rfd::FileDialog;

pub struct CamApp {
    scale: f32,
    offset: egui::Vec2,

    original_size: egui::Vec2,
    display_size: egui::Vec2,

    raster_resolution: f32,

    svg_texture: Option<egui::TextureHandle>,
    svg_data: Vec<u8>,
}

impl Default for CamApp {
    fn default() -> Self {
        Self {
            scale: 1.0,
            offset: egui::Vec2::ZERO,

            raster_resolution: 8.0,

            original_size: egui::Vec2::ZERO,
            display_size: egui::Vec2::ZERO,

            svg_texture: None,
            svg_data: Vec::new(),
        }
    }
}

impl CamApp {
    /// Called once before the first frame.
    pub fn new(_: &eframe::CreationContext<'_>) -> Self {
        // This is also where you can customize the look and feel of egui using
        // `cc.egui_ctx.set_visuals()` and `cc.egui_ctx.set_fonts()`

        // Load the default app state
        Default::default()
    }

    fn load_svg_data(&mut self, file_path: std::path::PathBuf, ctx: &egui::Context) {
        self.svg_data = std::fs::read(file_path).unwrap_or_else(|_| {
            eprintln!("Failed to read SVG file");
            Vec::new()
        });

        self.update_texture(ctx);
    }

    fn open_file(&mut self, ctx: &egui::Context) {
        if let Some(path) = FileDialog::new()
            .add_filter("SVG", &["svg"])
            .set_directory("/home")
            .pick_file()
        {
            self.load_svg_data(path, ctx);
        }
    }

    fn load_svg_texture(&mut self, ctx: &egui::Context) -> Result<(), Box<dyn std::error::Error>> {
        if self.svg_data.is_empty() {
            return Ok(());
        }

        let opt = resvg::usvg::Options::default();
        let rtree = resvg::usvg::Tree::from_data(&self.svg_data, &opt)?;

        let original_size = rtree.size();
        self.original_size = egui::Vec2::new(original_size.width(), original_size.height());

        let raster_size = self.original_size * self.raster_resolution;

        let mut pixmap = resvg::tiny_skia::Pixmap::new(raster_size.x as u32, raster_size.y as u32)
            .ok_or("Failed to create pixmap")?;

        let transform =
            resvg::tiny_skia::Transform::from_scale(self.raster_resolution, self.raster_resolution);

        let mut target = pixmap.as_mut();
        resvg::render(&rtree, transform, &mut target);

        // Convert to egui texture
        let rgba_data = pixmap.take();
        let color_image = egui::ColorImage::from_rgba_unmultiplied(
            [raster_size.x as usize, raster_size.y as usize],
            &rgba_data,
        );

        // Create texture with linear filtering for smooth scaling
        let texture_options = egui::TextureOptions {
            magnification: egui::TextureFilter::Linear,
            minification: egui::TextureFilter::Linear,
            wrap_mode: egui::TextureWrapMode::ClampToEdge,
            mipmap_mode: None,
        };

        self.svg_texture = Some(ctx.load_texture("svg_cache", color_image, texture_options));

        Ok(())
    }

    fn update_texture(&mut self, ctx: &egui::Context) {
        if let Err(e) = self.load_svg_texture(ctx) {
            eprintln!("Failed to load SVG: {}", e);
        }
    }

    fn set_scale(&mut self, scale: f32) {
        self.scale = scale.clamp(0.1, 8.0);
    }
}

impl eframe::App for CamApp {
    fn update(&mut self, ctx: &egui::Context, _frame: &mut eframe::Frame) {
        egui::TopBottomPanel::top("top_panel").show(ctx, |ui| {
            let rect = ui.max_rect();
            let id = ui.make_persistent_id("top_panel_drag-area");

            let drag_response = ui.interact(rect, id, egui::Sense::click_and_drag());

            if drag_response.drag_started_by(egui::PointerButton::Primary) {
                ctx.send_viewport_cmd(egui::ViewportCommand::StartDrag);
            }

            ui.horizontal(|ui| {
                if ui
                    .button("Open File")
                    .on_hover_text("Open SVG file")
                    .clicked()
                {
                    self.open_file(ctx);
                }

                ui.add_space(16.0);

                egui::widgets::global_theme_preference_buttons(ui);

                ui.with_layout(egui::Layout::right_to_left(egui::Align::Center), |ui| {
                    let button_height = 12.0;

                    let close_response = ui
                        .add(Button::new(RichText::new("❌").size(button_height)))
                        .on_hover_text("Close window");

                    if close_response.clicked() {
                        ctx.send_viewport_cmd(egui::ViewportCommand::Close);
                    }

                    let is_maximized = ui.input(|i| i.viewport().maximized.unwrap_or(false));
                    if is_maximized {
                        let maximized_response = ui
                            .add(Button::new(RichText::new("🗖").size(button_height)))
                            .on_hover_text("Restore window");
                        if maximized_response.clicked() {
                            ctx.send_viewport_cmd(egui::ViewportCommand::Maximized(false));
                        }
                    } else {
                        let maximized_response = ui
                            .add(Button::new(RichText::new("🗗").size(button_height)))
                            .on_hover_text("Maximize window");
                        if maximized_response.clicked() {
                            ctx.send_viewport_cmd(egui::ViewportCommand::Maximized(true));
                        }
                    }

                    let minimized_response = ui
                        .add(Button::new(RichText::new("🗕").size(button_height)))
                        .on_hover_text("Minimize the window");
                    if minimized_response.clicked() {
                        ctx.send_viewport_cmd(egui::ViewportCommand::Minimized(true));
                    }
                });
            });
        });

        egui::SidePanel::right("right_panel")
            .resizable(false)
            .default_width(290.0)
            .show(ctx, |ui| {
                ui.add_space(12.0);

                egui::ScrollArea::vertical().show(ui, |ui| {
                    ui.add(egui::Slider::new(&mut self.scale, 0.1..=8.0).text("Zoom"));
                });
            });

        egui::TopBottomPanel::bottom("bottom_panel")
            .resizable(false)
            .min_height(0.0)
            .show(ctx, |ui| {
                ui.label(format!("Scale: {:.2}x", self.scale));
            });

        egui::CentralPanel::default().show(ctx, |ui| {
            let central_panel_rect =
                ui.allocate_rect(ui.available_rect_before_wrap(), egui::Sense::drag());

            if let Some(texture) = &self.svg_texture {
                self.display_size = self.original_size * self.scale;

                if self.offset == egui::Vec2::ZERO {
                    let panel_center = central_panel_rect.rect.center();
                    let image_size = self.original_size * self.scale;
                    self.offset = panel_center.to_vec2()
                        - ((image_size + egui::Vec2 { x: 290.0, y: 0.0 }) / 2.0);
                }

                // compute destination rect (top-left at panel.min + offset)
                let dest_rect =
                    egui::Rect::from_min_size(ui.min_rect().min + self.offset, self.display_size);

                let rect_response = ui.allocate_rect(dest_rect, egui::Sense::drag());

                // full UV (draw the whole texture)
                let uv = egui::Rect::from_min_max(egui::Pos2::ZERO, egui::Pos2::new(1.0, 1.0));

                ui.painter()
                    .image(texture.id(), dest_rect, uv, egui::Color32::WHITE);

                if central_panel_rect.hovered() || rect_response.hovered() {
                    ctx.input(|i| {
                        let scroll_delta = i.raw_scroll_delta.y;
                        if scroll_delta != 0.0 {
                            let zoom_speed = 0.007;
                            let zoom_factor = 1.0 + (scroll_delta * zoom_speed);
                            self.set_scale(self.scale * zoom_factor);
                        }
                    });
                }

                if central_panel_rect.dragged() {
                    self.offset += central_panel_rect.drag_delta();
                }

                if rect_response.dragged() {
                    self.offset += rect_response.drag_delta();
                }
            }
        });
    }
}

fn main() -> eframe::Result<()> {
    let native_options = eframe::NativeOptions {
        viewport: egui::ViewportBuilder::default()
            .with_decorations(false)
            .with_inner_size([1920.0, 800.0])
            .with_min_inner_size([800.0, 600.0])
            .with_transparent(true)
            .with_icon(
                eframe::icon_data::from_png_bytes(&include_bytes!("../assets/app-icon.png")[..])
                    .expect("Failed to load icon"),
            ),
        ..Default::default()
    };

    eframe::run_native(
        "Mekatrol CAM",
        native_options,
        Box::new(|cc| {
            // This gives us image support:
            egui_extras::install_image_loaders(&cc.egui_ctx);

            Ok(Box::new(CamApp::new(cc)))
        }),
    )
}
