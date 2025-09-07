using Mekatrol.CAM.Core.Geometry.Entities;
using Mekatrol.CAM.Core.Parsers.Svg;
using System.Text;
using System.Xml;

namespace MekatrolCAM.UnitTest.Svg;

public partial class SvgParserTests
{
    [TestMethod]
    public void ParsePolylineElementBadPoints()
    {
        const string polylineSvg = @"
                <svg height=""100"" width=""100"">
                  <polyline points=""0z,100 50,25 50,75 100,0"" />
                </svg>
                ";

        var svgParser = _services.GetRequiredService<ISvgParser>();
        var byteArray = Encoding.ASCII.GetBytes(polylineSvg);
        using var stream = new MemoryStream(byteArray);
        using var streamReader = new StreamReader(stream);

        try
        {
            svgParser.Parse(streamReader);
            Assert.Fail("Expected exception");
        }
        catch (XmlException ex)
        {
            Assert.AreEqual("Invalid polyline points value. Line 3, position 20.", ex.Message);
        }
    }

    [TestMethod]
    public void ParsePolyline1()
    {
        const string polylineSvg = @"
                <svg height=""100"" width=""100"">
                  <polyline points=""12,34 56,78 90,11 22,33"" />
                </svg>
                ";

        var svgParser = _services.GetRequiredService<ISvgParser>();
        var byteArray = Encoding.ASCII.GetBytes(polylineSvg);
        using var stream = new MemoryStream(byteArray);
        using var streamReader = new StreamReader(stream);

        var list = svgParser.Parse(streamReader);
        Assert.IsNotNull(list);
        Assert.AreEqual(1, list.Count);

        var geometry = list[0];
        Assert.AreEqual(GeometricEntityType.Polyline, geometry.Type);

        var polyline = (PolylineEntity)geometry;
        Assert.AreEqual(4, polyline.Points.Count);
        AssertEx.WithinTolerance(12, polyline.Location.X);
        AssertEx.WithinTolerance(34, polyline.Location.Y);

        AssertEx.WithinTolerance(12, polyline.Points[0].X);
        AssertEx.WithinTolerance(34, polyline.Points[0].Y);
        AssertEx.WithinTolerance(56, polyline.Points[1].X);
        AssertEx.WithinTolerance(78, polyline.Points[1].Y);
        AssertEx.WithinTolerance(90, polyline.Points[2].X);
        AssertEx.WithinTolerance(11, polyline.Points[2].Y);
        AssertEx.WithinTolerance(22, polyline.Points[3].X);
        AssertEx.WithinTolerance(33, polyline.Points[3].Y);
    }

    [TestMethod]
    public void ParsePolyline2()
    {
        const string polylineSvg = @"
                <svg height=""100"" width=""100"">
                  <polyline points=""1E1,2E2 3E3,4E4 5E5,6E6"" />
                </svg>
                ";

        var svgParser = _services.GetRequiredService<ISvgParser>();
        var byteArray = Encoding.ASCII.GetBytes(polylineSvg);
        using var stream = new MemoryStream(byteArray);
        using var streamReader = new StreamReader(stream);

        var list = svgParser.Parse(streamReader);
        Assert.IsNotNull(list);
        Assert.AreEqual(1, list.Count);

        var geometry = list[0];
        Assert.AreEqual(GeometricEntityType.Polyline, geometry.Type);

        var polyline = (PolylineEntity)geometry;
        Assert.AreEqual(3, polyline.Points.Count);
        AssertEx.WithinTolerance(1e1, polyline.Location.X);
        AssertEx.WithinTolerance(2e2, polyline.Location.Y);

        AssertEx.WithinTolerance(1E1, polyline.Points[0].X);
        AssertEx.WithinTolerance(2E2, polyline.Points[0].Y);
        AssertEx.WithinTolerance(3E3, polyline.Points[1].X);
        AssertEx.WithinTolerance(4E4, polyline.Points[1].Y);
        AssertEx.WithinTolerance(5E5, polyline.Points[2].X);
        AssertEx.WithinTolerance(6E6, polyline.Points[2].Y);
    }

    [TestMethod]
    public void ParsePolyline3()
    {
        const string polylineSvg = @"
                <svg height=""100"" width=""100"">
                  <polyline points=""1.2,3.4 5.6,7.8"" />
                </svg>
                ";

        var svgParser = _services.GetRequiredService<ISvgParser>();
        var byteArray = Encoding.ASCII.GetBytes(polylineSvg);
        using var stream = new MemoryStream(byteArray);
        using var streamReader = new StreamReader(stream);

        var list = svgParser.Parse(streamReader);
        Assert.IsNotNull(list);
        Assert.AreEqual(1, list.Count);

        var geometry = list[0];
        Assert.AreEqual(GeometricEntityType.Polyline, geometry.Type);

        var polyline = (PolylineEntity)geometry;
        Assert.AreEqual(2, polyline.Points.Count);
        AssertEx.WithinTolerance(1.2, polyline.Location.X);
        AssertEx.WithinTolerance(3.4, polyline.Location.Y);

        AssertEx.WithinTolerance(1.2, polyline.Points[0].X);
        AssertEx.WithinTolerance(3.4, polyline.Points[0].Y);
        AssertEx.WithinTolerance(5.6, polyline.Points[1].X);
        AssertEx.WithinTolerance(7.8, polyline.Points[1].Y);
    }

    [TestMethod]
    public void ParsePolyline4()
    {
        const string polylineSvg = @"
                <svg height=""100"" width=""100"">
                  <polyline id=""polyline-01"" fill=""none"" stroke=""#000000""  points=""59,45,95,63,108,105,82,139,39,140,11,107,19,65""/>
                </svg>
                ";

        var svgParser = _services.GetRequiredService<ISvgParser>();
        var byteArray = Encoding.ASCII.GetBytes(polylineSvg);
        using var stream = new MemoryStream(byteArray);
        using var streamReader = new StreamReader(stream);

        var list = svgParser.Parse(streamReader);
        Assert.IsNotNull(list);
        Assert.AreEqual(1, list.Count);

        var geometry = list[0];
        Assert.AreEqual(GeometricEntityType.Polyline, geometry.Type);

        var polyline = (PolylineEntity)geometry;
        Assert.AreEqual(7, polyline.Points.Count);
        AssertEx.WithinTolerance(59, polyline.Location.X);
        AssertEx.WithinTolerance(45, polyline.Location.Y);

        AssertEx.WithinTolerance(59, polyline.Points[0].X);
        AssertEx.WithinTolerance(45, polyline.Points[0].Y);
        AssertEx.WithinTolerance(95, polyline.Points[1].X);
        AssertEx.WithinTolerance(63, polyline.Points[1].Y);
        AssertEx.WithinTolerance(108, polyline.Points[2].X);
        AssertEx.WithinTolerance(105, polyline.Points[2].Y);
        AssertEx.WithinTolerance(82, polyline.Points[3].X);
        AssertEx.WithinTolerance(139, polyline.Points[3].Y);
        AssertEx.WithinTolerance(39, polyline.Points[4].X);
        AssertEx.WithinTolerance(140, polyline.Points[4].Y);
        AssertEx.WithinTolerance(11, polyline.Points[5].X);
        AssertEx.WithinTolerance(107, polyline.Points[5].Y);
        AssertEx.WithinTolerance(19, polyline.Points[6].X);
        AssertEx.WithinTolerance(65, polyline.Points[6].Y);
    }

    [TestMethod]
    public void ParsePolylineNoPoints()
    {
        const string polylineSvg = @"
                <svg height=""100"" width=""100"">
                  <polyline points="""" />
                </svg>
                ";

        var svgParser = _services.GetRequiredService<ISvgParser>();
        var byteArray = Encoding.ASCII.GetBytes(polylineSvg);
        using var stream = new MemoryStream(byteArray);
        using var streamReader = new StreamReader(stream);

        var list = svgParser.Parse(streamReader);
        Assert.IsNotNull(list);
        Assert.AreEqual(1, list.Count);

        var geometry = list[0];
        Assert.AreEqual(GeometricEntityType.Polyline, geometry.Type);

        var polyline = (PolylineEntity)geometry;
        Assert.AreEqual(0, polyline.Points.Count);
    }
}
