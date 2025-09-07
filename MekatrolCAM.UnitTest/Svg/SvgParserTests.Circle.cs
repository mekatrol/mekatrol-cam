using Mekatrol.CAM.Core.Geometry.Entities;
using Mekatrol.CAM.Core.Parsers.Svg;
using System.Text;
using System.Xml;

namespace MekatrolCAM.UnitTest.Svg;

public partial class SvgParserTests
{
    [TestMethod]
    public void ParseCircleElementBadCx()
    {
        const string circle = @"
                <svg height=""100"" width=""100"">
                  <circle cx=""F"" cy=""50"" r=""40"" stroke=""black"" stroke-width=""3"" fill=""red"" />
                </svg>
                ";

        var svgParser = _services.GetRequiredService<ISvgParser>();
        var byteArray = Encoding.ASCII.GetBytes(circle);
        using var stream = new MemoryStream(byteArray);
        using var streamReader = new StreamReader(stream);

        try
        {
            svgParser.Parse(streamReader);
            Assert.Fail("Expected exception");
        }
        catch (XmlException ex)
        {
            Assert.AreEqual("circle 'cx' attribute must be a valid floating point number Line 3, position 20.", ex.Message);
        }
    }

    [TestMethod]
    public void ParseCircleElementBadCy()
    {
        const string circle = @"
                <svg height=""100"" width=""100"">
                  <circle cx=""5"" cy=""!@#"" r=""40"" stroke=""black"" stroke-width=""3"" fill=""red"" />
                </svg>
                ";

        var svgParser = _services.GetRequiredService<ISvgParser>();
        var byteArray = Encoding.ASCII.GetBytes(circle);
        using var stream = new MemoryStream(byteArray);
        using var streamReader = new StreamReader(stream);

        try
        {
            svgParser.Parse(streamReader);
            Assert.Fail("Expected exception");
        }
        catch (XmlException ex)
        {
            Assert.AreEqual("circle 'cy' attribute must be a valid floating point number Line 3, position 20.", ex.Message);
        }
    }

    [TestMethod]
    public void ParseCircleElementBadR()
    {
        const string circle = @"
                <svg height=""100"" width=""100"">
                  <circle cx=""50"" cy=""50"" r=""3_3"" stroke=""black"" stroke-width=""3"" fill=""red"" />
                </svg>
                ";

        var svgParser = _services.GetRequiredService<ISvgParser>();
        var byteArray = Encoding.ASCII.GetBytes(circle);
        using var stream = new MemoryStream(byteArray);
        using var streamReader = new StreamReader(stream);

        try
        {
            svgParser.Parse(streamReader);
            Assert.Fail("Expected exception");
        }
        catch (XmlException ex)
        {
            Assert.AreEqual("circle 'r' attribute must be a valid floating point number Line 3, position 20.", ex.Message);
        }
    }

    [TestMethod]
    public void ParseCircleElementMissingR()
    {
        const string circle = @"
                <svg height=""100"" width=""100"">
                  <circle cx=""50"" cy=""50"" stroke=""black"" stroke-width=""3"" fill=""red"" />
                </svg>
                ";

        var svgParser = _services.GetRequiredService<ISvgParser>();
        var byteArray = Encoding.ASCII.GetBytes(circle);
        using var stream = new MemoryStream(byteArray);
        using var streamReader = new StreamReader(stream);

        try
        {
            svgParser.Parse(streamReader);
            Assert.Fail("Expected exception");
        }
        catch (XmlException ex)
        {
            Assert.AreEqual("circle 'r' attribute must be provided Line 3, position 20.", ex.Message);
        }
    }

    [TestMethod]
    public void ParseCircle()
    {
        const string circleSvg = @"
                <svg height=""100"" width=""100"">
                  <circle cx=""60"" cy=""50"" r=""40"" stroke=""black"" stroke-width=""3"" fill=""red"" />
                </svg>
                ";

        var svgParser = _services.GetRequiredService<ISvgParser>();
        var byteArray = Encoding.ASCII.GetBytes(circleSvg);
        using var stream = new MemoryStream(byteArray);
        using var streamReader = new StreamReader(stream);

        var list = svgParser.Parse(streamReader);
        Assert.IsNotNull(list);
        Assert.AreEqual(1, list.Count);

        var geometry = list[0];
        Assert.AreEqual(GeometricEntityType.Circle, geometry.Type);

        var circle = (CircleEntity)geometry;
        AssertEx.WithinTolerance(60, circle.Location.X);
        AssertEx.WithinTolerance(50, circle.Location.Y);
        AssertEx.WithinTolerance(40, circle.Radius);
    }
}
