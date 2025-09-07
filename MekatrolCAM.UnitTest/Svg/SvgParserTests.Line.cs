using Mekatrol.CAM.Core.Geometry.Entities;
using Mekatrol.CAM.Core.Parsers.Svg;
using System.Text;
using System.Xml;

namespace MekatrolCAM.UnitTest.Svg;

public partial class SvgParserTests
{
    [TestMethod]
    public void ParseLineElementBadX1()
    {
        const string lineSvg = @"
                <svg height=""100"" width=""100"">
                  <line x1=""F"" y1=""50"" x2=""40"" y2=""10"" stroke=""black"" stroke-width=""3"" fill=""red"" />
                </svg>
                ";

        var svgParser = _services.GetRequiredService<ISvgParser>();
        var byteArray = Encoding.ASCII.GetBytes(lineSvg);
        using var stream = new MemoryStream(byteArray);
        using var streamReader = new StreamReader(stream);

        try
        {
            svgParser.Parse(streamReader);
            Assert.Fail("Expected exception");
        }
        catch (XmlException ex)
        {
            Assert.AreEqual("line 'x1' attribute must be a valid floating point number Line 3, position 20.", ex.Message);
        }
    }

    [TestMethod]
    public void ParseLineElementBadY1()
    {
        const string rectangleSvg = @"
                <svg height=""100"" width=""100"">
                  <line x1=""5"" y1=""!@#"" stroke=""black"" stroke-width=""3"" fill=""red"" />
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
            Assert.AreEqual("line 'y1' attribute must be a valid floating point number Line 3, position 20.", ex.Message);
        }
    }

    [TestMethod]
    public void ParseLineElementBadX2()
    {
        const string rectangleSvg = @"
                <svg height=""100"" width=""100"">
                  <line x1=""50"" y1=""50"" x2=""3_3"" y2=""1"" stroke=""black"" stroke-width=""3"" fill=""red"" />
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
            Assert.AreEqual("line 'x2' attribute must be a valid floating point number Line 3, position 20.", ex.Message);
        }
    }

    [TestMethod]
    public void ParseLineElementBadY2()
    {
        const string rectangleSvg = @"
                <svg height=""100"" width=""100"">
                  <line x1=""50"" y1=""50"" x2=""33"" y2=""1_"" stroke=""black"" stroke-width=""3"" fill=""red"" />
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
            Assert.AreEqual("line 'y2' attribute must be a valid floating point number Line 3, position 20.", ex.Message);
        }
    }

    [TestMethod]
    public void ParseLine()
    {
        const string lineSvg = @"
                <svg height=""100"" width=""100"">
                  <line x1=""12"" y1=""34"" x2=""56"" y2=""78"" stroke=""black"" stroke-width=""3"" fill=""red"" />
                </svg>
                ";

        var svgParser = _services.GetRequiredService<ISvgParser>();
        var byteArray = Encoding.ASCII.GetBytes(lineSvg);
        using var stream = new MemoryStream(byteArray);
        using var streamReader = new StreamReader(stream);

        var svgPath = svgParser.Parse(streamReader);
        Assert.IsNotNull(svgPath);
        Assert.AreEqual(1, svgPath.Entities.Count);

        var geometry = svgPath.Entities[0];
        Assert.AreEqual(GeometricEntityType.Line, geometry.Type);

        var line = (LineEntity)geometry;
        AssertEx.WithinTolerance(12, line.Location.X);
        AssertEx.WithinTolerance(34, line.Location.Y);
        AssertEx.WithinTolerance(56, line.EndLocation.X);
        AssertEx.WithinTolerance(78, line.EndLocation.Y);
    }

    [TestMethod]
    public void ParseLineDefaultX1()
    {
        const string lineSvg = @"
                <svg height=""100"" width=""100"">
                  <line y1=""34"" x2=""56"" y2=""78"" stroke=""black"" stroke-width=""3"" fill=""red"" />
                </svg>
                ";

        var svgParser = _services.GetRequiredService<ISvgParser>();
        var byteArray = Encoding.ASCII.GetBytes(lineSvg);
        using var stream = new MemoryStream(byteArray);
        using var streamReader = new StreamReader(stream);

        var svgPath = svgParser.Parse(streamReader);
        Assert.IsNotNull(svgPath);
        Assert.AreEqual(1, svgPath.Entities.Count);

        var geometry = svgPath.Entities[0];
        Assert.AreEqual(GeometricEntityType.Line, geometry.Type);

        var line = (LineEntity)geometry;
        AssertEx.WithinTolerance(0, line.Location.X);
        AssertEx.WithinTolerance(34, line.Location.Y);
        AssertEx.WithinTolerance(56, line.EndLocation.X);
        AssertEx.WithinTolerance(78, line.EndLocation.Y);
    }

    [TestMethod]
    public void ParseLineDefaultY1()
    {
        const string lineSvg = @"
                <svg height=""100"" width=""100"">
                  <line x1=""12"" x2=""56"" y2=""78"" stroke=""black"" stroke-width=""3"" fill=""red"" />
                </svg>
                ";

        var svgParser = _services.GetRequiredService<ISvgParser>();
        var byteArray = Encoding.ASCII.GetBytes(lineSvg);
        using var stream = new MemoryStream(byteArray);
        using var streamReader = new StreamReader(stream);

        var svgPath = svgParser.Parse(streamReader);
        Assert.IsNotNull(svgPath);
        Assert.AreEqual(1, svgPath.Entities.Count);

        var geometry = svgPath.Entities[0];
        Assert.AreEqual(GeometricEntityType.Line, geometry.Type);

        var line = (LineEntity)geometry;
        AssertEx.WithinTolerance(12, line.Location.X);
        AssertEx.WithinTolerance(0, line.Location.Y);
        AssertEx.WithinTolerance(56, line.EndLocation.X);
        AssertEx.WithinTolerance(78, line.EndLocation.Y);
    }

    [TestMethod]
    public void ParseLineDefaultX2()
    {
        const string lineSvg = @"
                <svg height=""100"" width=""100"">
                  <line x1=""12"" y1=""34"" y2=""78"" stroke=""black"" stroke-width=""3"" fill=""red"" />
                </svg>
                ";

        var svgParser = _services.GetRequiredService<ISvgParser>();
        var byteArray = Encoding.ASCII.GetBytes(lineSvg);
        using var stream = new MemoryStream(byteArray);
        using var streamReader = new StreamReader(stream);

        var svgPath = svgParser.Parse(streamReader);
        Assert.IsNotNull(svgPath);
        Assert.AreEqual(1, svgPath.Entities.Count);

        var geometry = svgPath.Entities[0];
        Assert.AreEqual(GeometricEntityType.Line, geometry.Type);

        var line = (LineEntity)geometry;
        AssertEx.WithinTolerance(12, line.Location.X);
        AssertEx.WithinTolerance(34, line.Location.Y);
        AssertEx.WithinTolerance(0, line.EndLocation.X);
        AssertEx.WithinTolerance(78, line.EndLocation.Y);
    }

    [TestMethod]
    public void ParseLineDefaultY2()
    {
        const string lineSvg = @"
                <svg height=""100"" width=""100"">
                  <line x1=""12"" y1=""34"" x2=""56"" stroke=""black"" stroke-width=""3"" fill=""red"" />
                </svg>
                ";

        var svgParser = _services.GetRequiredService<ISvgParser>();
        var byteArray = Encoding.ASCII.GetBytes(lineSvg);
        using var stream = new MemoryStream(byteArray);
        using var streamReader = new StreamReader(stream);

        var svgPath = svgParser.Parse(streamReader);
        Assert.IsNotNull(svgPath);
        Assert.AreEqual(1, svgPath.Entities.Count);

        var geometry = svgPath.Entities[0];
        Assert.AreEqual(GeometricEntityType.Line, geometry.Type);

        var line = (LineEntity)geometry;
        AssertEx.WithinTolerance(12, line.Location.X);
        AssertEx.WithinTolerance(34, line.Location.Y);
        AssertEx.WithinTolerance(56, line.EndLocation.X);
        AssertEx.WithinTolerance(0, line.EndLocation.Y);
    }

    [TestMethod]
    public void ParseLineNoValues()
    {
        const string lineSvg = @"
                <svg height=""100"" width=""100"">
                  <line />
                </svg>
                ";

        var svgParser = _services.GetRequiredService<ISvgParser>();
        var byteArray = Encoding.ASCII.GetBytes(lineSvg);
        using var stream = new MemoryStream(byteArray);
        using var streamReader = new StreamReader(stream);

        var svgPath = svgParser.Parse(streamReader);
        Assert.IsNotNull(svgPath);
        Assert.AreEqual(1, svgPath.Entities.Count);

        var geometry = svgPath.Entities[0];
        Assert.AreEqual(GeometricEntityType.Line, geometry.Type);

        var line = (LineEntity)geometry;
        AssertEx.WithinTolerance(0, line.Location.X);
        AssertEx.WithinTolerance(0, line.Location.Y);
        AssertEx.WithinTolerance(0, line.Location.X);
        AssertEx.WithinTolerance(0, line.Location.Y);
    }
}
