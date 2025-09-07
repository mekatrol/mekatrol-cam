using Avalonia;
using Avalonia.Headless;
using Avalonia.Media;
using Mekatrol.CAM.Core.Parsers.Svg;
using Mekatrol.CAM.Core.Render;

namespace MekatrolCAM.UnitTest.Svg;

[STATestClass]
public class CssParserTests
{
    private static int _inited;    

    [AssemblyInitialize]
    public static void Init(TestContext _)
    {
        if (Interlocked.Exchange(ref _inited, 1) == 1)
        {
            return;
        }

        // Registers IFontManagerImpl
        AppBuilder.Configure<Application>()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false })
            .UseSkia()
            .SetupWithoutStarting();
    }

    [TestMethod]
    public void TestFont()
    {
        var fontLine = "font: 1.2em \"Fira Sans\", sans-serif;";
        var fontDescription = CssParser.ExtractFont(fontLine);
        Assert.IsNotNull(fontDescription);
        Assert.AreEqual("Microsoft Sans Serif", fontDescription.FamilyName);
        AssertEx.WithinTolerance(1.2f * 4.21752f, fontDescription.Size);
        Assert.AreEqual(FontStyle.Normal, fontDescription.Style);

        fontLine = "font: italic 1.2mm \"Fira Sans\", serif;";
        fontDescription = CssParser.ExtractFont(fontLine);
        Assert.IsNotNull(fontDescription);
        Assert.AreEqual("Microsoft Sans Serif", fontDescription.FamilyName);
        AssertEx.WithinTolerance(1.2f, fontDescription.Size);
        Assert.AreEqual(FontStyle.Italic, fontDescription.Style);

        fontLine = "font: italic bold small-caps bold 16px/2 cursive;";
        fontDescription = CssParser.ExtractFont(fontLine);
        Assert.IsNotNull(fontDescription);
        Assert.AreEqual(RenderExtensions.DefaultFontFamilyName, fontDescription.FamilyName);
        AssertEx.WithinTolerance(16f * 25.4f / 96.0f, fontDescription.Size);
        Assert.AreEqual(FontStyle.Italic, fontDescription.Style);
        Assert.AreEqual(FontWeight.Bold, fontDescription.Weight);

        fontLine = "font: small-caps bold 25.4mm/1 Consolas;";
        fontDescription = CssParser.ExtractFont(fontLine);
        Assert.IsNotNull(fontDescription);
        Assert.AreEqual("Consolas", fontDescription.FamilyName);
        AssertEx.WithinTolerance(25.4f, fontDescription.Size);
        Assert.AreEqual(FontWeight.Bold, fontDescription.Weight);

        fontLine = "font: small-caps italic 2cm Consolas;";
        fontDescription = CssParser.ExtractFont(fontLine);
        Assert.IsNotNull(fontDescription);
        Assert.AreEqual("Consolas", fontDescription.FamilyName);
        AssertEx.WithinTolerance(20f, fontDescription.Size);
        Assert.AreEqual(FontStyle.Italic, fontDescription.Style);

        fontLine = "font: small-caps italic 1in Consolas;";
        fontDescription = CssParser.ExtractFont(fontLine);
        Assert.IsNotNull(fontDescription);
        Assert.AreEqual("Consolas", fontDescription.FamilyName);
        AssertEx.WithinTolerance(25.4f, fontDescription.Size);
        Assert.AreEqual(FontStyle.Italic, fontDescription.Style);

        fontLine = "font: 80% sans-serif;";
        fontDescription = CssParser.ExtractFont(fontLine);
        Assert.IsNotNull(fontDescription);
        Assert.AreEqual("Microsoft Sans Serif", fontDescription.FamilyName);
        AssertEx.WithinTolerance(RenderExtensions.DefaultFontSize * 0.8, fontDescription.Size);
        Assert.AreEqual(FontStyle.Normal, fontDescription.Style);

        fontLine = "font: bold italic large 1.2mm serif;";
        fontDescription = CssParser.ExtractFont(fontLine);
        Assert.IsNotNull(fontDescription);
        Assert.AreEqual("Microsoft Sans Serif", fontDescription.FamilyName);
        AssertEx.WithinTolerance(1.2f, fontDescription.Size);
        Assert.AreEqual(FontStyle.Italic, fontDescription.Style);
        Assert.AreEqual(FontWeight.Bold, fontDescription.Weight);

        fontLine = "font: 100 \"Helvetica Neue\", \"Arial\", sans-serif; ";
        fontDescription = CssParser.ExtractFont(fontLine);
        Assert.IsNotNull(fontDescription);
        Assert.AreEqual(RenderExtensions.DefaultFontFamilyName, fontDescription.FamilyName);
        AssertEx.WithinTolerance(100f, fontDescription.Size);
        Assert.AreEqual(FontStyle.Normal, fontDescription.Style);

        fontLine = "font: caption;";
        fontDescription = CssParser.ExtractFont(fontLine);
        Assert.IsNull(fontDescription);

        fontLine = "font: ";
        fontDescription = CssParser.ExtractFont(fontLine);
        Assert.IsNull(fontDescription);

        fontLine = "";
        fontDescription = CssParser.ExtractFont(fontLine);
        Assert.IsNull(fontDescription);
    }
}
