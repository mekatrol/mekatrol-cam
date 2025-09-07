using Mekatrol.CAM.Core.Geometry.Entities;
using Mekatrol.CAM.Core.Parsers.Svg;
using System.Text;
using System.Xml;

namespace MekatrolCAM.UnitTest.Svg;

public partial class SvgParserTests
{
    [TestMethod]
    public void ParseRectangleElementBadX()
    {
        const string rectSvg = @"
                <svg height=""100"" width=""100"">
                  <rect x=""F"" y=""50"" width=""40"" height=""10"" stroke=""black"" stroke-width=""3"" fill=""red"" />
                </svg>
                ";

        var svgParser = _services.GetRequiredService<ISvgParser>();
        var byteArray = Encoding.ASCII.GetBytes(rectSvg);
        using var stream = new MemoryStream(byteArray);
        using var streamReader = new StreamReader(stream);

        try
        {
            svgParser.Parse(streamReader);
            Assert.Fail("Expected exception");
        }
        catch (XmlException ex)
        {
            Assert.AreEqual("rect 'x' attribute must be a valid floating point number Line 3, position 20.", ex.Message);
        }
    }

    [TestMethod]
    public void ParseRectangleElementBadY()
    {
        const string rectangleSvg = @"
                <svg height=""100"" width=""100"">
                  <rect x=""5"" y=""!@#"" stroke=""black"" stroke-width=""3"" fill=""red"" />
                </svg>
                ";

        var svgParser = _services.GetRequiredService<ISvgParser>();
        var byteArray = Encoding.ASCII.GetBytes(rectangleSvg);
        using var stream = new MemoryStream(byteArray);
        using var streamReader = new StreamReader(stream);

        try
        {
            svgParser.Parse(streamReader);
            Assert.Fail("Expected exception");
        }
        catch (XmlException ex)
        {
            Assert.AreEqual("rect 'y' attribute must be a valid floating point number Line 3, position 20.", ex.Message);
        }
    }

    [TestMethod]
    public void ParseRectangleElementBadWidth()
    {
        const string rectangleSvg = @"
                <svg height=""100"" width=""100"">
                  <rect x=""50"" y=""50"" width=""3_3"" height=""1"" stroke=""black"" stroke-width=""3"" fill=""red"" />
                </svg>
                ";

        var svgParser = _services.GetRequiredService<ISvgParser>();
        var byteArray = Encoding.ASCII.GetBytes(rectangleSvg);
        using var stream = new MemoryStream(byteArray);
        using var streamReader = new StreamReader(stream);

        try
        {
            svgParser.Parse(streamReader);
            Assert.Fail("Expected exception");
        }
        catch (XmlException ex)
        {
            Assert.AreEqual("rect 'width' attribute must be a valid floating point number Line 3, position 20.", ex.Message);
        }
    }

    [TestMethod]
    public void ParseRectangleElementBadHeight()
    {
        const string rectangleSvg = @"
                <svg height=""100"" width=""100"">
                  <rect x=""50"" y=""50"" width=""33"" height=""1_"" stroke=""black"" stroke-width=""3"" fill=""red"" />
                </svg>
                ";

        var svgParser = _services.GetRequiredService<ISvgParser>();
        var byteArray = Encoding.ASCII.GetBytes(rectangleSvg);
        using var stream = new MemoryStream(byteArray);
        using var streamReader = new StreamReader(stream);

        try
        {
            svgParser.Parse(streamReader);
            Assert.Fail("Expected exception");
        }
        catch (XmlException ex)
        {
            Assert.AreEqual("rect 'height' attribute must be a valid floating point number Line 3, position 20.", ex.Message);
        }
    }

    [TestMethod]
    public void ParseRectangleElementBadRx()
    {
        const string rectangleSvg = @"
                <svg height=""100"" width=""100"">
                  <rect cx=""50"" cy=""50"" rx=""t0"" ry=""33"" stroke=""black"" stroke-width=""3"" fill=""red"" />
                </svg>
                ";

        var svgParser = _services.GetRequiredService<ISvgParser>();
        var byteArray = Encoding.ASCII.GetBytes(rectangleSvg);
        using var stream = new MemoryStream(byteArray);
        using var streamReader = new StreamReader(stream);

        try
        {
            svgParser.Parse(streamReader);
            Assert.Fail("Expected exception");
        }
        catch (XmlException ex)
        {
            Assert.AreEqual("rect 'rx' attribute must be a valid floating point number Line 3, position 20.", ex.Message);
        }
    }

    [TestMethod]
    public void ParseRectangleElementBadRy()
    {
        const string rectangleSvg = @"
                <svg height=""100"" width=""100"">
                  <rect cx=""50"" cy=""50"" rx=""0.0"" ry=""ss"" stroke=""black"" stroke-width=""3"" fill=""red"" />
                </svg>
                ";

        var svgParser = _services.GetRequiredService<ISvgParser>();
        var byteArray = Encoding.ASCII.GetBytes(rectangleSvg);
        using var stream = new MemoryStream(byteArray);
        using var streamReader = new StreamReader(stream);

        try
        {
            svgParser.Parse(streamReader);
            Assert.Fail("Expected exception");
        }
        catch (XmlException ex)
        {
            Assert.AreEqual("rect 'ry' attribute must be a valid floating point number Line 3, position 20.", ex.Message);
        }
    }

    [TestMethod]
    public void ParseRectangle()
    {
        const string rectSvg = @"
                <svg height=""100"" width=""100"">
                  <rect x=""99"" y=""11"" width=""33"" height=""22"" rx=""45"" ry=""66"" stroke=""black"" stroke-width=""3"" fill=""red"" />
                </svg>
                ";

        var svgParser = _services.GetRequiredService<ISvgParser>();
        var byteArray = Encoding.ASCII.GetBytes(rectSvg);
        using var stream = new MemoryStream(byteArray);
        using var streamReader = new StreamReader(stream);

        var list = svgParser.Parse(streamReader);
        Assert.IsNotNull(list);
        Assert.AreEqual(1, list.Count);

        var geometry = list[0];
        Assert.AreEqual(GeometricEntityType.Rectangle, geometry.Type);

        var rectangle = (RectangleEntity)geometry;
        AssertEx.WithinTolerance(99, rectangle.Location.X);
        AssertEx.WithinTolerance(11, rectangle.Location.Y);
        AssertEx.WithinTolerance(33, rectangle.Size.X);
        AssertEx.WithinTolerance(22, rectangle.Size.Y);
        AssertEx.WithinTolerance(45, rectangle.CornerRounding.X);
        AssertEx.WithinTolerance(66, rectangle.CornerRounding.Y);
    }

    [TestMethod]
    public void ParseRectangleDefaultRy()
    {
        const string rectSvg = @"
                <svg height=""100"" width=""100"">
                  <rect x=""12"" y=""34"" width=""56"" height=""78"" rx=""90"" stroke=""black"" stroke-width=""3"" fill=""red"" />
                </svg>
                ";

        var svgParser = _services.GetRequiredService<ISvgParser>();
        var byteArray = Encoding.ASCII.GetBytes(rectSvg);
        using var stream = new MemoryStream(byteArray);
        using var streamReader = new StreamReader(stream);

        var list = svgParser.Parse(streamReader);
        Assert.IsNotNull(list);
        Assert.AreEqual(1, list.Count);

        var geometry = list[0];
        Assert.AreEqual(GeometricEntityType.Rectangle, geometry.Type);

        var rectangle = (RectangleEntity)geometry;
        AssertEx.WithinTolerance(12, rectangle.Location.X);
        AssertEx.WithinTolerance(34, rectangle.Location.Y);
        AssertEx.WithinTolerance(56, rectangle.Size.X);
        AssertEx.WithinTolerance(78, rectangle.Size.Y);
        AssertEx.WithinTolerance(90, rectangle.CornerRounding.X);
        AssertEx.WithinTolerance(90, rectangle.CornerRounding.Y);
    }

    [TestMethod]
    public void ParseRectangleDefaultRx()
    {
        const string rectSvg = @"
                <svg height=""100"" width=""100"">
                  <rect x=""12"" y=""34"" width=""56"" height=""78"" ry=""90"" stroke=""black"" stroke-width=""3"" fill=""red"" />
                </svg>
                ";

        var svgParser = _services.GetRequiredService<ISvgParser>();
        var byteArray = Encoding.ASCII.GetBytes(rectSvg);
        using var stream = new MemoryStream(byteArray);
        using var streamReader = new StreamReader(stream);

        var list = svgParser.Parse(streamReader);
        Assert.IsNotNull(list);
        Assert.AreEqual(1, list.Count);

        var geometry = list[0];
        Assert.AreEqual(GeometricEntityType.Rectangle, geometry.Type);

        var rectangle = (RectangleEntity)geometry;
        AssertEx.WithinTolerance(12, rectangle.Location.X);
        AssertEx.WithinTolerance(34, rectangle.Location.Y);
        AssertEx.WithinTolerance(56, rectangle.Size.X);
        AssertEx.WithinTolerance(78, rectangle.Size.Y);
        AssertEx.WithinTolerance(90, rectangle.CornerRounding.X);
        AssertEx.WithinTolerance(90, rectangle.CornerRounding.Y);
    }

    [TestMethod]
    public void ParseRectangleNoValues()
    {
        const string rectSvg = @"
                <svg height=""100"" width=""100"">
                  <rect />
                </svg>
                ";

        var svgParser = _services.GetRequiredService<ISvgParser>();
        var byteArray = Encoding.ASCII.GetBytes(rectSvg);
        using var stream = new MemoryStream(byteArray);
        using var streamReader = new StreamReader(stream);

        var list = svgParser.Parse(streamReader);
        Assert.IsNotNull(list);
        Assert.AreEqual(1, list.Count);

        var geometry = list[0];
        Assert.AreEqual(GeometricEntityType.Rectangle, geometry.Type);

        var rectangle = (RectangleEntity)geometry;
        AssertEx.WithinTolerance(0, rectangle.Location.X);
        AssertEx.WithinTolerance(0, rectangle.Location.Y);
        AssertEx.WithinTolerance(1, rectangle.Size.X);
        AssertEx.WithinTolerance(1, rectangle.Size.Y);
        AssertEx.WithinTolerance(0, rectangle.CornerRounding.X);
        AssertEx.WithinTolerance(0, rectangle.CornerRounding.Y);
    }
}
