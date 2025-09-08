using Mekatrol.CAM.Core.Geometry.Entities;
using Mekatrol.CAM.Core.Parsers.Svg;
using System.Text;
using System.Xml;

namespace MekatrolCAM.UnitTest.Svg;

public partial class SvgParserTests
{
    [TestMethod]
    public void ParseEllipseElementBadCx()
    {
        const string ellipseSvg = @"
                <svg height=""100"" width=""100"">
                  <ellipse cx=""F"" cy=""50"" rx="""" ry=""_"" stroke=""black"" stroke-width=""3"" fill=""red"" />
                </svg>
                ";

        var svgParser = _services.GetRequiredService<ISvgParser>();
        var byteArray = Encoding.ASCII.GetBytes(ellipseSvg);
        using var stream = new MemoryStream(byteArray);
        using var streamReader = new StreamReader(stream);

        try
        {
            svgParser.Parse(streamReader);
            Assert.Fail();
        }
        catch (XmlException ex)
        {
            Assert.AreEqual("ellipse 'cx' attribute must be a valid floating point number Line 3, position 20.", ex.Message);
        }
    }

    [TestMethod]
    public void ParseEllipseElementBadCy()
    {
        const string ellipseSvg = @"
                <svg height=""100"" width=""100"">
                  <ellipse cx=""5"" cy=""!@#"" rx=""40"" ry=""33"" stroke=""black"" stroke-width=""3"" fill=""red"" />
                </svg>
                ";

        var svgParser = _services.GetRequiredService<ISvgParser>();
        var byteArray = Encoding.ASCII.GetBytes(ellipseSvg);
        using var stream = new MemoryStream(byteArray);
        using var streamReader = new StreamReader(stream);

        try
        {
            svgParser.Parse(streamReader);
            Assert.Fail("Expected exception");
        }
        catch (XmlException ex)
        {
            Assert.AreEqual("ellipse 'cy' attribute must be a valid floating point number Line 3, position 20.", ex.Message);
        }
    }

    [TestMethod]
    public void ParseEllipseElementBadRx()
    {
        const string ellipseSvg = @"
                <svg height=""100"" width=""100"">
                  <ellipse cx=""50"" cy=""50"" rx=""++"" stroke=""black"" stroke-width=""3"" fill=""red"" />
                </svg>
                ";

        var svgParser = _services.GetRequiredService<ISvgParser>();
        var byteArray = Encoding.ASCII.GetBytes(ellipseSvg);
        using var stream = new MemoryStream(byteArray);
        using var streamReader = new StreamReader(stream);

        try
        {
            svgParser.Parse(streamReader);
            Assert.Fail("Expected exception");
        }
        catch (XmlException ex)
        {
            Assert.AreEqual("ellipse 'rx' attribute must be a valid floating point number Line 3, position 20.", ex.Message);
        }
    }

    [TestMethod]
    public void ParseEllipseElementBadRy()
    {
        const string ellipseSvg = @"
                <svg height=""100"" width=""100"">
                  <ellipse cx=""50"" cy=""50"" ry=""tt"" stroke=""black"" stroke-width=""3"" fill=""red"" />
                </svg>
                ";

        var svgParser = _services.GetRequiredService<ISvgParser>();
        var byteArray = Encoding.ASCII.GetBytes(ellipseSvg);
        using var stream = new MemoryStream(byteArray);
        using var streamReader = new StreamReader(stream);

        try
        {
            svgParser.Parse(streamReader);
            Assert.Fail("Expected exception");
        }
        catch (XmlException ex)
        {
            Assert.AreEqual("ellipse 'ry' attribute must be a valid floating point number Line 3, position 20.", ex.Message);
        }
    }

    [TestMethod]
    public void ParseEllipse()
    {
        const string ellipseSvg = @"
                <svg height=""100"" width=""100"">
                  <ellipse cx=""99"" cy=""11"" rx=""45"" ry=""66"" stroke=""black"" stroke-width=""3"" fill=""red"" />
                </svg>
                ";

        var svgParser = _services.GetRequiredService<ISvgParser>();
        var byteArray = Encoding.ASCII.GetBytes(ellipseSvg);
        using var stream = new MemoryStream(byteArray);
        using var streamReader = new StreamReader(stream);

        var svgPath = svgParser.Parse(streamReader);
        Assert.IsNotNull(svgPath);
        Assert.AreEqual(1, svgPath.Entities.Count);

        var geometry = svgPath.Entities[0];
        Assert.AreEqual(GeometricEntityType.Ellipse, geometry.Type);

        var ellipse = (EllipseEntity)geometry;
        AssertEx.WithinTolerance(99, ellipse.Location.X);
        AssertEx.WithinTolerance(11, ellipse.Location.Y);
        AssertEx.WithinTolerance(45, ellipse.Radius.X);
        AssertEx.WithinTolerance(66, ellipse.Radius.Y);
    }

    [TestMethod]
    public void ParseEllipseDefaultRy()
    {
        const string ellipseSvg = @"
                <svg height=""100"" width=""100"">
                  <ellipse cx=""12"" cy=""34"" rx=""56"" stroke=""black"" stroke-width=""3"" fill=""red"" />
                </svg>
                ";

        var svgParser = _services.GetRequiredService<ISvgParser>();
        var byteArray = Encoding.ASCII.GetBytes(ellipseSvg);
        using var stream = new MemoryStream(byteArray);
        using var streamReader = new StreamReader(stream);

        var svgPath = svgParser.Parse(streamReader);
        Assert.IsNotNull(svgPath);
        Assert.AreEqual(1, svgPath.Entities.Count);

        var geometry = svgPath.Entities[0];
        Assert.AreEqual(GeometricEntityType.Ellipse, geometry.Type);

        var ellipse = (EllipseEntity)geometry;
        AssertEx.WithinTolerance(12, ellipse.Location.X);
        AssertEx.WithinTolerance(34, ellipse.Location.Y);
        AssertEx.WithinTolerance(56, ellipse.Radius.X);
        AssertEx.WithinTolerance(1, ellipse.Radius.Y);
    }

    [TestMethod]
    public void ParseEllipseDefaultRx()
    {
        const string ellipseSvg = @"
                <svg height=""100"" width=""100"">
                  <ellipse cx=""12"" cy=""34"" ry=""78"" stroke=""black"" stroke-width=""3"" fill=""red"" />
                </svg>
                ";

        var svgParser = _services.GetRequiredService<ISvgParser>();
        var byteArray = Encoding.ASCII.GetBytes(ellipseSvg);
        using var stream = new MemoryStream(byteArray);
        using var streamReader = new StreamReader(stream);

        var svgPath = svgParser.Parse(streamReader);
        Assert.IsNotNull(svgPath);
        Assert.AreEqual(1, svgPath.Entities.Count);

        var geometry = svgPath.Entities[0];
        Assert.AreEqual(GeometricEntityType.Ellipse, geometry.Type);

        var ellipse = (EllipseEntity)geometry;
        AssertEx.WithinTolerance(12, ellipse.Location.X);
        AssertEx.WithinTolerance(34, ellipse.Location.Y);
        AssertEx.WithinTolerance(1, ellipse.Radius.X);
        AssertEx.WithinTolerance(78, ellipse.Radius.Y);
    }

    [TestMethod]
    public void ParseEllipseNoValues()
    {
        const string ellipseSvg = @"
                <svg height=""100"" width=""100"">
                  <ellipse />
                </svg>
                ";

        var svgParser = _services.GetRequiredService<ISvgParser>();
        var byteArray = Encoding.ASCII.GetBytes(ellipseSvg);
        using var stream = new MemoryStream(byteArray);
        using var streamReader = new StreamReader(stream);

        var svgPath = svgParser.Parse(streamReader);
        Assert.IsNotNull(svgPath);
        Assert.AreEqual(1, svgPath.Entities.Count);

        var geometry = svgPath.Entities[0];
        Assert.AreEqual(GeometricEntityType.Ellipse, geometry.Type);

        var ellipse = (EllipseEntity)geometry;
        AssertEx.WithinTolerance(0, ellipse.Location.X);
        AssertEx.WithinTolerance(0, ellipse.Location.Y);
        AssertEx.WithinTolerance(1, ellipse.Radius.X);
        AssertEx.WithinTolerance(1, ellipse.Radius.Y);
    }
}
