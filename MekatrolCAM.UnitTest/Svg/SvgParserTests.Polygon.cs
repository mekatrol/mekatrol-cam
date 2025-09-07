using Mekatrol.CAM.Core.Geometry.Entities;
using Mekatrol.CAM.Core.Parsers.Svg;
using System.Text;
using System.Xml;

namespace MekatrolCAM.UnitTest.Svg;

public partial class SvgParserTests
{
    [TestMethod]
    public void ParsePolygonElementBadPoints()
    {
        const string polygonSvg = @"
                <svg height=""100"" width=""100"">
                  <polygon points=""0z,100 50,25 50,75 100,0"" />
                </svg>
                ";

        var svgParser = _services.GetRequiredService<ISvgParser>();
        var byteArray = Encoding.ASCII.GetBytes(polygonSvg);
        using var stream = new MemoryStream(byteArray);
        using var streamReader = new StreamReader(stream);

        try
        {
            svgParser.Parse(streamReader);
            Assert.Fail("Expected exception");
        }
        catch (XmlException ex)
        {
            Assert.AreEqual("Invalid polygon points value. Line 3, position 20.", ex.Message);
        }
    }

    [TestMethod]
    public void ParsePolygon1()
    {
        const string polygonSvg = @"
                <svg height=""100"" width=""100"">
                  <polygon points=""12,34 56,78 90,11 22,33"" />
                </svg>
                ";

        var svgParser = _services.GetRequiredService<ISvgParser>();
        var byteArray = Encoding.ASCII.GetBytes(polygonSvg);
        using var stream = new MemoryStream(byteArray);
        using var streamReader = new StreamReader(stream);

        var list = svgParser.Parse(streamReader);
        Assert.IsNotNull(list);
        Assert.AreEqual(1, list.Count);

        var geometry = list[0];
        Assert.AreEqual(GeometricEntityType.Polygon, geometry.Type);

        var polygon = (PolygonEntity)geometry;
        Assert.AreEqual(4, polygon.Points.Count);
        AssertEx.WithinTolerance(12, polygon.Location.X);
        AssertEx.WithinTolerance(34, polygon.Location.Y);

        AssertEx.WithinTolerance(12, polygon.Points[0].X);
        AssertEx.WithinTolerance(34, polygon.Points[0].Y);
        AssertEx.WithinTolerance(56, polygon.Points[1].X);
        AssertEx.WithinTolerance(78, polygon.Points[1].Y);
        AssertEx.WithinTolerance(90, polygon.Points[2].X);
        AssertEx.WithinTolerance(11, polygon.Points[2].Y);
        AssertEx.WithinTolerance(22, polygon.Points[3].X);
        AssertEx.WithinTolerance(33, polygon.Points[3].Y);
    }

    [TestMethod]
    public void ParsePolygon2()
    {
        const string polygonSvg = @"
                <svg height=""100"" width=""100"">
                  <polygon points=""1E1,2E2 3E3,4E4 5E5,6E6"" />
                </svg>
                ";

        var svgParser = _services.GetRequiredService<ISvgParser>();
        var byteArray = Encoding.ASCII.GetBytes(polygonSvg);
        using var stream = new MemoryStream(byteArray);
        using var streamReader = new StreamReader(stream);

        var list = svgParser.Parse(streamReader);
        Assert.IsNotNull(list);
        Assert.AreEqual(1, list.Count);

        var geometry = list[0];
        Assert.AreEqual(GeometricEntityType.Polygon, geometry.Type);

        var polygon = (PolygonEntity)geometry;
        Assert.AreEqual(3, polygon.Points.Count);
        AssertEx.WithinTolerance(1e1, polygon.Location.X);
        AssertEx.WithinTolerance(2e2, polygon.Location.Y);

        AssertEx.WithinTolerance(1E1, polygon.Points[0].X);
        AssertEx.WithinTolerance(2E2, polygon.Points[0].Y);
        AssertEx.WithinTolerance(3E3, polygon.Points[1].X);
        AssertEx.WithinTolerance(4E4, polygon.Points[1].Y);
        AssertEx.WithinTolerance(5E5, polygon.Points[2].X);
        AssertEx.WithinTolerance(6E6, polygon.Points[2].Y);
    }

    [TestMethod]
    public void ParsePolygon3()
    {
        const string polygonSvg = @"
                <svg height=""100"" width=""100"">
                  <polygon points=""1.2,3.4 5.6,7.8"" />
                </svg>
                ";

        var svgParser = _services.GetRequiredService<ISvgParser>();
        var byteArray = Encoding.ASCII.GetBytes(polygonSvg);
        using var stream = new MemoryStream(byteArray);
        using var streamReader = new StreamReader(stream);

        var list = svgParser.Parse(streamReader);
        Assert.IsNotNull(list);
        Assert.AreEqual(1, list.Count);

        var geometry = list[0];
        Assert.AreEqual(GeometricEntityType.Polygon, geometry.Type);

        var polygon = (PolygonEntity)geometry;
        Assert.AreEqual(2, polygon.Points.Count);
        AssertEx.WithinTolerance(1.2, polygon.Location.X);
        AssertEx.WithinTolerance(3.4, polygon.Location.Y);

        AssertEx.WithinTolerance(1.2, polygon.Points[0].X);
        AssertEx.WithinTolerance(3.4, polygon.Points[0].Y);
        AssertEx.WithinTolerance(5.6, polygon.Points[1].X);
        AssertEx.WithinTolerance(7.8, polygon.Points[1].Y);
    }

    [TestMethod]
    public void ParsePolygon4()
    {
        const string polygonSvg = @"
                <svg height=""100"" width=""100"">
                  <polygon id=""polygon-01"" fill=""none"" stroke=""#000000""  points=""59,45,95,63,108,105,82,139,39,140,11,107,19,65""/>
                </svg>
                ";

        var svgParser = _services.GetRequiredService<ISvgParser>();
        var byteArray = Encoding.ASCII.GetBytes(polygonSvg);
        using var stream = new MemoryStream(byteArray);
        using var streamReader = new StreamReader(stream);

        var list = svgParser.Parse(streamReader);
        Assert.IsNotNull(list);
        Assert.AreEqual(1, list.Count);

        var geometry = list[0];
        Assert.AreEqual(GeometricEntityType.Polygon, geometry.Type);

        var polygon = (PolygonEntity)geometry;
        Assert.AreEqual(7, polygon.Points.Count);
        AssertEx.WithinTolerance(59, polygon.Location.X);
        AssertEx.WithinTolerance(45, polygon.Location.Y);

        AssertEx.WithinTolerance(59, polygon.Points[0].X);
        AssertEx.WithinTolerance(45, polygon.Points[0].Y);
        AssertEx.WithinTolerance(95, polygon.Points[1].X);
        AssertEx.WithinTolerance(63, polygon.Points[1].Y);
        AssertEx.WithinTolerance(108, polygon.Points[2].X);
        AssertEx.WithinTolerance(105, polygon.Points[2].Y);
        AssertEx.WithinTolerance(82, polygon.Points[3].X);
        AssertEx.WithinTolerance(139, polygon.Points[3].Y);
        AssertEx.WithinTolerance(39, polygon.Points[4].X);
        AssertEx.WithinTolerance(140, polygon.Points[4].Y);
        AssertEx.WithinTolerance(11, polygon.Points[5].X);
        AssertEx.WithinTolerance(107, polygon.Points[5].Y);
        AssertEx.WithinTolerance(19, polygon.Points[6].X);
        AssertEx.WithinTolerance(65, polygon.Points[6].Y);
    }

    [TestMethod]
    public void ParsePolygonNoPoints()
    {
        const string polygonSvg = @"
                <svg height=""100"" width=""100"">
                  <polygon points="""" />
                </svg>
                ";

        var svgParser = _services.GetRequiredService<ISvgParser>();
        var byteArray = Encoding.ASCII.GetBytes(polygonSvg);
        using var stream = new MemoryStream(byteArray);
        using var streamReader = new StreamReader(stream);

        var list = svgParser.Parse(streamReader);
        Assert.IsNotNull(list);
        Assert.AreEqual(1, list.Count);

        var geometry = list[0];
        Assert.AreEqual(GeometricEntityType.Polygon, geometry.Type);

        var polygon = (PolygonEntity)geometry;
        Assert.AreEqual(0, polygon.Points.Count);
    }
}
