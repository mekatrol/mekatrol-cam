use usvg::{Options, Tree};

pub use usvg;

pub fn parse_svg(svg_bytes: &[u8]) -> Result<Tree, usvg::Error> {
    let mut opt = Options::default();

    if let Err(e) = opt
        .fontdb_mut()
        .load_font_file("./test-files/LiberationMono-Bold.ttf")
    {
        eprintln!("Warning: failed to load font: {e}");
    }

    Tree::from_data(svg_bytes, &opt)
}
