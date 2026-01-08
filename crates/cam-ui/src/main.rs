use cam_core::parse_svg;
use cam_core::usvg::{Group, Node, Tree};
use eframe::egui;
use rfd::FileDialog;

struct CamApp {
    svg_path: String,
    tree: Option<Tree>,
    error: Option<String>,
    texture: Option<egui::TextureHandle>,
    needs_render: bool,
    render_size: Option<(u32, u32)>,
}

impl CamApp {
    fn new(svg_path: impl Into<String>) -> Self {
        let app = Self {
            svg_path: svg_path.into(),
            tree: None,
            error: None,
            texture: None,
            needs_render: false,
            render_size: None,
        };
        app
    }

    fn reload(&mut self) {
        self.error = None;
        let svg_bytes = match std::fs::read(&self.svg_path) {
            Ok(bytes) => bytes,
            Err(err) => {
                self.tree = None;
                self.error = Some(format!("Failed to read SVG: {err}"));
                return;
            }
        };

        match parse_svg(&svg_bytes) {
            Ok(tree) => {
                self.tree = Some(tree);
                self.needs_render = true;
            }
            Err(err) => {
                self.tree = None;
                self.error = Some(format!("Failed to parse SVG: {err}"));
            }
        }
    }

    fn render_svg(&mut self, ctx: &egui::Context) {
        self.needs_render = false;

        let tree = match &self.tree {
            Some(tree) => tree,
            None => return,
        };

        let size = tree.size().to_int_size();
        let (width, height) = (size.width(), size.height());
        if width == 0 || height == 0 {
            self.texture = None;
            self.error = Some("SVG has zero size.".to_string());
            return;
        }

        let mut pixmap = match tiny_skia::Pixmap::new(width, height) {
            Some(pixmap) => pixmap,
            None => {
                self.texture = None;
                self.error = Some("Failed to allocate render target.".to_string());
                return;
            }
        };

        let mut target = pixmap.as_mut();
        draw_checkerboard(&mut target);
        resvg::render(tree, tiny_skia::Transform::default(), &mut target);

        let image = egui::ColorImage::from_rgba_premultiplied(
            [width as usize, height as usize],
            pixmap.data(),
        );
        let texture = ctx.load_texture(
            "svg_render",
            image,
            egui::TextureOptions::LINEAR,
        );
        self.texture = Some(texture);
        self.render_size = Some((width, height));
    }
}

fn draw_checkerboard(pixmap: &mut tiny_skia::PixmapMut<'_>) {
    let tile = 16u32;
    let w = pixmap.width();
    let h = pixmap.height();

    let mut paint = tiny_skia::Paint::default();
    paint.anti_alias = false;

    let light = tiny_skia::Color::from_rgba8(230, 230, 230, 255);
    let dark = tiny_skia::Color::from_rgba8(200, 200, 200, 255);

    let tiles_x = (w + tile - 1) / tile;
    let tiles_y = (h + tile - 1) / tile;

    for y in 0..tiles_y {
        for x in 0..tiles_x {
            let use_light = (x + y) % 2 == 0;
            paint.set_color(if use_light { light } else { dark });

            let rect_x = (x * tile) as f32;
            let rect_y = (y * tile) as f32;
            let rect_w = (tile.min(w - x * tile)) as f32;
            let rect_h = (tile.min(h - y * tile)) as f32;

            if let Some(rect) = tiny_skia::Rect::from_xywh(rect_x, rect_y, rect_w, rect_h) {
                pixmap.fill_rect(rect, &paint, tiny_skia::Transform::identity(), None);
            }
        }
    }
}

impl eframe::App for CamApp {
    fn update(&mut self, ctx: &egui::Context, _frame: &mut eframe::Frame) {
        egui::TopBottomPanel::top("top_bar").show(ctx, |ui| {
            ui.horizontal(|ui| {
                ui.label("SVG path:");
                ui.text_edit_singleline(&mut self.svg_path);
                if ui.button("Open...").clicked() {
                    if let Some(path) = FileDialog::new()
                        .add_filter("SVG", &["svg"])
                        .pick_file()
                    {
                        self.svg_path = path.display().to_string();
                        self.reload();
                    }
                }
                if ui.button("Reload").clicked() {
                    self.reload();
                }
            });
        });

        egui::CentralPanel::default().show(ctx, |ui| {
            if self.needs_render {
                self.render_svg(ctx);
            }

            if let Some(err) = &self.error {
                ui.colored_label(egui::Color32::RED, err);
                return;
            }

            let tree = match &self.tree {
                Some(tree) => tree,
                None => {
                    ui.label("No SVG loaded.");
                    return;
                }
            };

            ui.label(format!("SVG size: {:?}", tree.size()));
            if let Some((width, height)) = self.render_size {
                ui.label(format!("Rendered size: {width}x{height}px"));
            }

            if let Some(texture) = &self.texture {
                ui.add_space(8.0);
                egui::ScrollArea::both().show(ui, |ui| {
                    let image =
                        egui::Image::new(texture).fit_to_exact_size(texture.size_vec2());
                    ui.add(image);
                });
            }

            ui.separator();
            egui::CollapsingHeader::new("SVG tree")
                .default_open(false)
                .show(ui, |ui| {
                    ui_group(ui, tree.root());
                });
        });
    }
}

fn ui_group(ui: &mut egui::Ui, group: &Group) {
    egui::CollapsingHeader::new("Group")
        .default_open(true)
        .show(ui, |ui| {
            ui.label(format!("transform: {:?}", group.transform()));
            ui.label(format!("opacity: {:?}", group.opacity().get()));

            for child in group.children() {
                ui_node(ui, child);
            }
        });
}

fn ui_node(ui: &mut egui::Ui, node: &Node) {
    match node {
        Node::Group(g) => ui_group(ui, g),
        Node::Path(p) => {
            egui::CollapsingHeader::new("Path").show(ui, |ui| {
                ui.label(format!("abs_transform: {:?}", p.abs_transform()));
                ui.label(format!("visible: {:?}", p.is_visible()));
                ui.label(format!("fill: {}", p.fill().is_some()));
                ui.label(format!("stroke: {}", p.stroke().is_some()));
                ui.label(format!("segments: {}", p.data().segments().count()));
            });
        }
        Node::Text(t) => {
            egui::CollapsingHeader::new("Text").show(ui, |ui| {
                ui.label(format!("abs_transform: {:?}", t.abs_transform()));
                ui.label(format!("chunks: {}", t.chunks().len()));
            });
        }
        Node::Image(i) => {
            egui::CollapsingHeader::new("Image").show(ui, |ui| {
                ui.label(format!("abs_transform: {:?}", i.abs_transform()));
                ui.label(format!("size: {:?}", i.size()));
            });
        }
    }
}

fn main() -> eframe::Result<()> {
    let options = eframe::NativeOptions::default();
    eframe::run_native(
        "Mekatrol CAM",
        options,
        Box::new(|_cc| Box::new(CamApp::new(String::new()))),
    )
}
