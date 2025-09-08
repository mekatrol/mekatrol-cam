using Mekatrol.CAM.Core.Geometry;
using Mekatrol.CAM.Core.Geometry.Entities;
using Mekatrol.CAM.Core.Render;

namespace MekatrolCAM.UnitTest.Geometry;

[TestClass]
public class PointInPolygonRectangleTests
{
    [TestMethod]
    public void TestRectAtOrigin()
    {
        var rect = new RectangleEntity(0, 0, 10, 10, 0, 0, new GeometryTransform());
        rect.InitializeState(new GeometryTransform());
        var points = rect.TransformedPolylines[0];

        var minX = rect.MinUntransformed.X;
        var minY = rect.MinUntransformed.Y;
        var maxX = rect.MaxUntransformed.X;
        var maxY = rect.MaxUntransformed.Y;

        /**********************************************************************************************************
         * Vertices
         **********************************************************************************************************/
        var result = GeometryUtils.PointInPolygon(new PointDouble(minX, minY), points);
        Assert.AreEqual(PointInPolgygonResult.Vertex, result);

        result = GeometryUtils.PointInPolygon(new PointDouble(maxX, minY), points);
        Assert.AreEqual(PointInPolgygonResult.Vertex, result);

        result = GeometryUtils.PointInPolygon(new PointDouble(maxX, maxY), points);
        Assert.AreEqual(PointInPolgygonResult.Vertex, result);

        result = GeometryUtils.PointInPolygon(new PointDouble(minX, maxY), points);
        Assert.AreEqual(PointInPolgygonResult.Vertex, result);

        /**********************************************************************************************************
         * Top left outside
         **********************************************************************************************************/
        result = GeometryUtils.PointInPolygon(new PointDouble(minX - 1, minY - 1), points);
        Assert.AreEqual(PointInPolgygonResult.Outside, result);

        result = GeometryUtils.PointInPolygon(new PointDouble(minX - 1, minY), points);
        Assert.AreEqual(PointInPolgygonResult.Outside, result);

        result = GeometryUtils.PointInPolygon(new PointDouble(minX, minY - 1), points);
        Assert.AreEqual(PointInPolgygonResult.Outside, result);

        /**********************************************************************************************************
         * Top right outside
         **********************************************************************************************************/
        result = GeometryUtils.PointInPolygon(new PointDouble(maxX + 1, minY - 1), points);
        Assert.AreEqual(PointInPolgygonResult.Outside, result);

        result = GeometryUtils.PointInPolygon(new PointDouble(maxX + 1, minY), points);
        Assert.AreEqual(PointInPolgygonResult.Outside, result);

        result = GeometryUtils.PointInPolygon(new PointDouble(maxX, minY - 1), points);
        Assert.AreEqual(PointInPolgygonResult.Outside, result);

        /**********************************************************************************************************
         * Bottom right outside
         **********************************************************************************************************/
        result = GeometryUtils.PointInPolygon(new PointDouble(maxX + 1, maxY + 1), points);
        Assert.AreEqual(PointInPolgygonResult.Outside, result);

        result = GeometryUtils.PointInPolygon(new PointDouble(maxX + 1, maxY), points);
        Assert.AreEqual(PointInPolgygonResult.Outside, result);

        result = GeometryUtils.PointInPolygon(new PointDouble(maxX, maxY + 1), points);
        Assert.AreEqual(PointInPolgygonResult.Outside, result);

        /**********************************************************************************************************
         * Bottom left outside
         **********************************************************************************************************/
        result = GeometryUtils.PointInPolygon(new PointDouble(minX - 1, maxY + 1), points);
        Assert.AreEqual(PointInPolgygonResult.Outside, result);

        result = GeometryUtils.PointInPolygon(new PointDouble(minX - 1, maxY), points);
        Assert.AreEqual(PointInPolgygonResult.Outside, result);

        result = GeometryUtils.PointInPolygon(new PointDouble(minX, maxY + 1), points);
        Assert.AreEqual(PointInPolgygonResult.Outside, result);

        /**********************************************************************************************************
         * Horizontal edges
         **********************************************************************************************************/
        for (var x = minX + 1; x < maxX; x++)
        {
            result = GeometryUtils.PointInPolygon(new PointDouble(x, minY), points);
            Assert.AreEqual(PointInPolgygonResult.Edge, result);

            result = GeometryUtils.PointInPolygon(new PointDouble(x, maxY), points);
            Assert.AreEqual(PointInPolgygonResult.Edge, result);
        }

        /**********************************************************************************************************
         * Vertical edges
         **********************************************************************************************************/
        for (var y = minY + 1; y < maxY; y++)
        {
            result = GeometryUtils.PointInPolygon(new PointDouble(minX, y), points);
            Assert.AreEqual(PointInPolgygonResult.Edge, result);

            result = GeometryUtils.PointInPolygon(new PointDouble(maxX, y), points);
            Assert.AreEqual(PointInPolgygonResult.Edge, result);
        }

        /**********************************************************************************************************
         * Inside poly
         **********************************************************************************************************/
        for (var x = minX + 1; x < maxX; x++)
        {
            for (var y = minY + 1; y < maxY; y++)
            {
                result = GeometryUtils.PointInPolygon(new PointDouble(x, y), points);
                Assert.AreEqual(PointInPolgygonResult.Inside, result);
            }
        }

    }

    [TestMethod]
    public void TestRectAroundOrigin()
    {
        var rect = new RectangleEntity(-10, -10, 20, 20, 0, 0, new GeometryTransform());
        rect.InitializeState(new GeometryTransform());
        var points = rect.TransformedPolylines;

        var minX = rect.MinUntransformed.X;
        var minY = rect.MinUntransformed.Y;
        var maxX = rect.MaxUntransformed.X;
        var maxY = rect.MaxUntransformed.Y;

        /**********************************************************************************************************
         * Vertices
         **********************************************************************************************************/
        var result = GeometryUtils.PointInPolygon(new PointDouble(minX, minY), points);
        Assert.AreEqual(PointInPolgygonResult.Vertex, result);

        result = GeometryUtils.PointInPolygon(new PointDouble(maxX, minY), points);
        Assert.AreEqual(PointInPolgygonResult.Vertex, result);

        result = GeometryUtils.PointInPolygon(new PointDouble(maxX, maxY), points);
        Assert.AreEqual(PointInPolgygonResult.Vertex, result);

        result = GeometryUtils.PointInPolygon(new PointDouble(minX, maxY), points);
        Assert.AreEqual(PointInPolgygonResult.Vertex, result);

        /**********************************************************************************************************
         * Top left outside
         **********************************************************************************************************/
        result = GeometryUtils.PointInPolygon(new PointDouble(minX - 1, minY - 1), points);
        Assert.AreEqual(PointInPolgygonResult.Outside, result);

        result = GeometryUtils.PointInPolygon(new PointDouble(minX - 1, minY), points);
        Assert.AreEqual(PointInPolgygonResult.Outside, result);

        result = GeometryUtils.PointInPolygon(new PointDouble(minX, minY - 1), points);
        Assert.AreEqual(PointInPolgygonResult.Outside, result);

        /**********************************************************************************************************
         * Top right outside
         **********************************************************************************************************/
        result = GeometryUtils.PointInPolygon(new PointDouble(maxX + 1, minY - 1), points);
        Assert.AreEqual(PointInPolgygonResult.Outside, result);

        result = GeometryUtils.PointInPolygon(new PointDouble(maxX + 1, minY), points);
        Assert.AreEqual(PointInPolgygonResult.Outside, result);

        result = GeometryUtils.PointInPolygon(new PointDouble(maxX, minY - 1), points);
        Assert.AreEqual(PointInPolgygonResult.Outside, result);

        /**********************************************************************************************************
         * Bottom right outside
         **********************************************************************************************************/
        result = GeometryUtils.PointInPolygon(new PointDouble(maxX + 1, maxY + 1), points);
        Assert.AreEqual(PointInPolgygonResult.Outside, result);

        result = GeometryUtils.PointInPolygon(new PointDouble(maxX + 1, maxY), points);
        Assert.AreEqual(PointInPolgygonResult.Outside, result);

        result = GeometryUtils.PointInPolygon(new PointDouble(maxX, maxY + 1), points);
        Assert.AreEqual(PointInPolgygonResult.Outside, result);

        /**********************************************************************************************************
         * Bottom left outside
         **********************************************************************************************************/
        result = GeometryUtils.PointInPolygon(new PointDouble(minX - 1, maxY + 1), points);
        Assert.AreEqual(PointInPolgygonResult.Outside, result);

        result = GeometryUtils.PointInPolygon(new PointDouble(minX - 1, maxY), points);
        Assert.AreEqual(PointInPolgygonResult.Outside, result);

        result = GeometryUtils.PointInPolygon(new PointDouble(minX, maxY + 1), points);
        Assert.AreEqual(PointInPolgygonResult.Outside, result);

        /**********************************************************************************************************
         * Horizontal edges
         **********************************************************************************************************/
        for (var x = minX + 1; x < maxX; x++)
        {
            result = GeometryUtils.PointInPolygon(new PointDouble(x, minY), points);
            Assert.AreEqual(PointInPolgygonResult.Edge, result);

            result = GeometryUtils.PointInPolygon(new PointDouble(x, maxY), points);
            Assert.AreEqual(PointInPolgygonResult.Edge, result);
        }

        /**********************************************************************************************************
         * Vertical edges
         **********************************************************************************************************/
        for (var y = minY + 1; y < maxY; y++)
        {
            result = GeometryUtils.PointInPolygon(new PointDouble(minX, y), points);
            Assert.AreEqual(PointInPolgygonResult.Edge, result);

            result = GeometryUtils.PointInPolygon(new PointDouble(maxX, y), points);
            Assert.AreEqual(PointInPolgygonResult.Edge, result);
        }

        /**********************************************************************************************************
         * Inside poly
         **********************************************************************************************************/
        for (var x = minX + 1; x < maxX; x++)
        {
            for (var y = minY + 1; y < maxY; y++)
            {
                result = GeometryUtils.PointInPolygon(new PointDouble(x, y), points);
                Assert.AreEqual(PointInPolgygonResult.Inside, result);
            }
        }

    }

    [TestMethod]
    public void TestRectRotatedAboutOrigin()
    {
        var transform = new GeometryTransform() { Rotate = new GeometryRotate { Angle = 45 } };

        var rect = new RectangleEntity(-10, -10, 20, 20, 0, 0, transform);
        rect.InitializeState(new GeometryTransform());
        var points = rect.TransformedPolylines;

        var result = GeometryUtils.PointInPolygon(new PointDouble(0, -14.142135623730951), points);
        Assert.AreEqual(PointInPolgygonResult.Vertex, result);

        result = GeometryUtils.PointInPolygon(new PointDouble(-14.142135623730951, 0), points);
        Assert.AreEqual(PointInPolgygonResult.Vertex, result);

        result = GeometryUtils.PointInPolygon(new PointDouble(0, 14.142135623730951), points);
        Assert.AreEqual(PointInPolgygonResult.Vertex, result);

        result = GeometryUtils.PointInPolygon(new PointDouble(14.142135623730951, 0), points);
        Assert.AreEqual(PointInPolgygonResult.Vertex, result);
    }

    [TestMethod]
    public void TestRectRotatedAbout_100_100()
    {
        var transform = new GeometryTransform() { Rotate = new GeometryRotate { Angle = 45 } };

        var rect = new RectangleEntity(90, 90, 20, 20, 0, 0, transform);
        rect.InitializeState(new GeometryTransform());
        var points = rect.TransformedPolylines;

        var result = GeometryUtils.PointInPolygon(new PointDouble(0, 127.27922061357856), points);
        Assert.AreEqual(PointInPolgygonResult.Vertex, result);

        result = GeometryUtils.PointInPolygon(new PointDouble(14.142135623730951, 141.42135623730951), points);
        Assert.AreEqual(PointInPolgygonResult.Vertex, result);

        result = GeometryUtils.PointInPolygon(new PointDouble(0, 155.56349186104046), points);
        Assert.AreEqual(PointInPolgygonResult.Vertex, result);

        result = GeometryUtils.PointInPolygon(new PointDouble(-14.142135623730951, 141.42135623730951), points);
        Assert.AreEqual(PointInPolgygonResult.Vertex, result);
    }

    [TestMethod]
    public void TestRectRotatedAbout_Minus100_Minus100()
    {
        var transform = new GeometryTransform() { Rotate = new GeometryRotate { Angle = 45 } };

        var rect = new RectangleEntity(-110, -110, 20, 20, 0, 0, transform);
        rect.InitializeState(new GeometryTransform());
        var points = rect.TransformedPolylines[0];

        var result = GeometryUtils.PointInPolygon(new PointDouble(0, -155.56349186104046), points);
        Assert.AreEqual(PointInPolgygonResult.Vertex, result);

        result = GeometryUtils.PointInPolygon(new PointDouble(14.142135623730951, -141.42135623730951), points);
        Assert.AreEqual(PointInPolgygonResult.Vertex, result);

        result = GeometryUtils.PointInPolygon(new PointDouble(0, -127.27922061357856), points);
        Assert.AreEqual(PointInPolgygonResult.Vertex, result);

        result = GeometryUtils.PointInPolygon(new PointDouble(-14.142135623730951, -141.42135623730951), points);
        Assert.AreEqual(PointInPolgygonResult.Vertex, result);
    }
}

