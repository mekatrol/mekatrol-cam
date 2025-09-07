using Mekatrol.CAM.Core.Geometry.Entities;
using Mekatrol.CAM.Core.Parsers.Svg;
using System.Text;

namespace MekatrolCAM.UnitTest.Svg;

public partial class SvgParserTests
{
    [TestMethod]
    public void ParseCubicBezier()
    {
        const string cubicBezierSvg = @"<?xml version=""1.0"" standalone=""no""?>
                <svg width=""5cm"" height=""4cm"" viewBox=""0 0 500 400""
                     xmlns=""http://www.w3.org/2000/svg"" version=""1.1"">
                  <title>Example cubic01- cubic Bézier commands in path data</title>
                  <desc>Picture showing a simple example of path data
                        using both a ""C"" and an ""S"" command,
                        along with annotations showing the control points
                        and end points</desc>
                  <style type=""text/css""><![CDATA[
                    .Border { fill:none; stroke:blue; stroke-width:1 }
                    .Connect { fill:none; stroke:#888888; stroke-width:2 }
                    .SamplePath { fill:none; stroke:red; stroke-width:5 }
                    .EndPoint { fill:none; stroke:#888888; stroke-width:2 }
                    .CtlPoint { fill:#888888; stroke:none }
                    .AutoCtlPoint { fill:none; stroke:blue; stroke-width:4 }
                    .Label { font-size:22; font-family:Verdana }
                  ]]></style>

                  <rect class=""Border"" x=""1"" y=""1"" width=""498"" height=""398"" />

                  <polyline class=""Connect"" points=""100,200 100,100"" />
                  <polyline class=""Connect"" points=""250,100 250,200"" />
                  <polyline class=""Connect"" points=""250,200 250,300"" />
                  <polyline class=""Connect"" points=""400,300 400,200"" />
                  <path class=""SamplePath"" d=""M100,200 C100,100 250,100 250,200 S400,300 400,200"" />
                  <circle class=""EndPoint"" cx=""100"" cy=""200"" r=""10"" />
                  <circle class=""EndPoint"" cx=""250"" cy=""200"" r=""10"" />
                  <circle class=""EndPoint"" cx=""400"" cy=""200"" r=""10"" />
                  <circle class=""CtlPoint"" cx=""100"" cy=""100"" r=""10"" />
                  <circle class=""CtlPoint"" cx=""250"" cy=""100"" r=""10"" />
                  <circle class=""CtlPoint"" cx=""400"" cy=""300"" r=""10"" />
                  <circle class=""AutoCtlPoint"" cx=""250"" cy=""300"" r=""9"" />
                  <text class=""Label"" x=""25"" y=""70"">M100,200 C100,100 250,100 250,200</text>
                  <text class=""Label"" x=""325"" y=""350""
                        style=""text-anchor:middle"">S400,300 400,200</text>
                </svg>";

        var svgParser = _services.GetRequiredService<ISvgParser>();
        var byteArray = Encoding.ASCII.GetBytes(cubicBezierSvg);
        using var stream = new MemoryStream(byteArray);
        using var streamReader = new StreamReader(stream);

        var svgPath = svgParser.Parse(streamReader);
        Assert.IsNotNull(svgPath);
        Assert.AreEqual(15, svgPath.Entities.Count);

        var geometry = svgPath.Entities[5];
        Assert.AreEqual(GeometricEntityType.Path, geometry.Type);

        var path = (PathEntity)geometry;
        Assert.AreEqual(2, path.Entities.Count);
        AssertEx.WithinTolerance(100, path.Location.X);
        AssertEx.WithinTolerance(200, path.Location.Y);

        geometry = path.Entities[0];
        Assert.AreEqual(GeometricEntityType.CubicBezier, geometry.Type);
        var cubicBezier = (CubicBezierEntity)geometry;
        AssertEx.WithinTolerance(100, cubicBezier.Location.X);
        AssertEx.WithinTolerance(200, cubicBezier.Location.Y);
        AssertEx.WithinTolerance(100, cubicBezier.Control1.X);
        AssertEx.WithinTolerance(100, cubicBezier.Control1.Y);
        AssertEx.WithinTolerance(250, cubicBezier.Control2.X);
        AssertEx.WithinTolerance(100, cubicBezier.Control2.Y);
        AssertEx.WithinTolerance(250, cubicBezier.EndLocation.X);
        AssertEx.WithinTolerance(200, cubicBezier.EndLocation.Y);

        geometry = path.Entities[1];
        Assert.AreEqual(GeometricEntityType.CubicBezier, geometry.Type);
        cubicBezier = (CubicBezierEntity)geometry;
        AssertEx.WithinTolerance(250, cubicBezier.Location.X);
        AssertEx.WithinTolerance(200, cubicBezier.Location.Y);
        AssertEx.WithinTolerance(250, cubicBezier.Control1.X);
        AssertEx.WithinTolerance(300, cubicBezier.Control1.Y);
        AssertEx.WithinTolerance(400, cubicBezier.Control2.X);
        AssertEx.WithinTolerance(300, cubicBezier.Control2.Y);
        AssertEx.WithinTolerance(400, cubicBezier.EndLocation.X);
        AssertEx.WithinTolerance(200, cubicBezier.EndLocation.Y);
    }

    [TestMethod]
    public void ParseCubicBezierSNotFollowingC()
    {
        const string cubicBezierSvg = @"<?xml version=""1.0"" standalone=""no""?>
                <svg width=""5cm"" height=""4cm"" viewBox=""0 0 500 400""
                     xmlns=""http://www.w3.org/2000/svg"" version=""1.1"">
                  <path d=""M100,200 S400,300 400,200"" />
                </svg>";

        var svgParser = _services.GetRequiredService<ISvgParser>();
        var byteArray = Encoding.ASCII.GetBytes(cubicBezierSvg);
        using var stream = new MemoryStream(byteArray);
        using var streamReader = new StreamReader(stream);

        var svgPath = svgParser.Parse(streamReader);
        Assert.IsNotNull(svgPath);
        Assert.AreEqual(1, svgPath.Entities.Count);

        var geometry = svgPath.Entities[0];
        Assert.AreEqual(GeometricEntityType.Path, geometry.Type);

        var path = (PathEntity)geometry;
        Assert.AreEqual(1, path.Entities.Count);
        AssertEx.WithinTolerance(100, path.Location.X);
        AssertEx.WithinTolerance(200, path.Location.Y);

        geometry = path.Entities[0];
        Assert.AreEqual(GeometricEntityType.CubicBezier, geometry.Type);
        var cubicBezier = (CubicBezierEntity)geometry;
        AssertEx.WithinTolerance(100, cubicBezier.Location.X);
        AssertEx.WithinTolerance(200, cubicBezier.Location.Y);
        AssertEx.WithinTolerance(100, cubicBezier.Control1.X);
        AssertEx.WithinTolerance(200, cubicBezier.Control1.Y);
        AssertEx.WithinTolerance(400, cubicBezier.Control2.X);
        AssertEx.WithinTolerance(300, cubicBezier.Control2.Y);
        AssertEx.WithinTolerance(400, cubicBezier.EndLocation.X);
        AssertEx.WithinTolerance(200, cubicBezier.EndLocation.Y);
    }
}
