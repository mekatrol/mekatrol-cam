using ExCSS;
using Mekatrol.CAM.Core.Converter;
using Mekatrol.CAM.Core.Parsers.Svg;
using System.Text;
using System.Xml.Linq;

namespace MekatrolCAM.UnitTest.Svg;

[TestClass]
public class ViewBoxParserTest
{
    private const double _tolerance = 1E-6;

    [TestMethod]
    public void ValidViewBoxTest()
    {
        const string svg = @"
                <svg viewBox=""0 0 1200 600"" height=""100"" width=""80"">
                </svg>
                ";

        var byteArray = Encoding.ASCII.GetBytes(svg);

        using (var stream = new MemoryStream(byteArray))
        {
            using var streamReader = new StreamReader(stream);

            var xmlDocument = XDocument.Load(streamReader, LoadOptions.SetLineInfo);
            var viewBoxInfo = ViewBoxParser.ReadViewBox(xmlDocument.Root!, OutputUnit.Millimeter);

            Assert.AreEqual(0, viewBoxInfo.ViewBox.MinX);
            Assert.AreEqual(0, viewBoxInfo.ViewBox.MinX);
            Assert.AreEqual(1200, viewBoxInfo.ViewBox.Width);
            Assert.AreEqual(600, viewBoxInfo.ViewBox.Height);

            Assert.IsTrue(Math.Abs(UnitSizeConverter.ConvertGraphicSizeToMM(80, "px") - viewBoxInfo.PhysicalSize.Width) < _tolerance);
            Assert.IsTrue(Math.Abs(UnitSizeConverter.ConvertGraphicSizeToMM(100, "px") - viewBoxInfo.PhysicalSize.Height) < _tolerance);
        }

        using (var stream = new MemoryStream(byteArray))
        {
            using var streamReader = new StreamReader(stream);

            var xmlDocument = XDocument.Load(streamReader, LoadOptions.SetLineInfo);
            var viewBoxInfo = ViewBoxParser.ReadViewBox(xmlDocument.Root!, OutputUnit.Inch);

            Assert.AreEqual(0, viewBoxInfo.ViewBox.MinX);
            Assert.AreEqual(0, viewBoxInfo.ViewBox.MinX);
            Assert.AreEqual(1200, viewBoxInfo.ViewBox.Width);
            Assert.AreEqual(600, viewBoxInfo.ViewBox.Height);

            Assert.IsTrue(Math.Abs(UnitSizeConverter.ConvertGraphicSizeToMM(80, "px") / 25.4 - viewBoxInfo.PhysicalSize.Width) < _tolerance);
            Assert.IsTrue(Math.Abs(UnitSizeConverter.ConvertGraphicSizeToMM(100, "px") / 25.4 - viewBoxInfo.PhysicalSize.Height) < _tolerance);
        }
    }

    [TestMethod]
    public void InvalidViewBoxTest()
    {
        const string svg = @"
                <svg viewBox=""0 0 a 600"" height=""100"" width=""80"">
                </svg>
                ";

        var byteArray = Encoding.ASCII.GetBytes(svg);

        using var stream = new MemoryStream(byteArray);
        using var streamReader = new StreamReader(stream);

        var xmlDocument = XDocument.Load(streamReader, LoadOptions.SetLineInfo);
        var viewBoxInfo = ViewBoxParser.ReadViewBox(xmlDocument.Root!, OutputUnit.Millimeter);

        Assert.AreEqual(0, viewBoxInfo.ViewBox.MinX);
        Assert.AreEqual(0, viewBoxInfo.ViewBox.MinX);
        Assert.IsTrue(Math.Abs(UnitSizeConverter.ConvertGraphicSizeToMM(80, "px") - viewBoxInfo.ViewBox.Width) < _tolerance);
        Assert.IsTrue(Math.Abs(UnitSizeConverter.ConvertGraphicSizeToMM(100, "px") - viewBoxInfo.ViewBox.Height) < _tolerance);

        Assert.IsTrue(Math.Abs(UnitSizeConverter.ConvertGraphicSizeToMM(80, "px") - viewBoxInfo.PhysicalSize.Width) < _tolerance);
        Assert.IsTrue(Math.Abs(UnitSizeConverter.ConvertGraphicSizeToMM(100, "px") - viewBoxInfo.PhysicalSize.Height) < _tolerance);
    }

    [TestMethod]
    public void InvalidViewBoxAndSizeTest()
    {
        const string svg = @"
                <svg viewBox=""0 0 a 600"" height1=""100"" width2=""80"">
                </svg>
                ";

        var byteArray = Encoding.ASCII.GetBytes(svg);

        using var stream = new MemoryStream(byteArray);
        using var streamReader = new StreamReader(stream);

        var xmlDocument = XDocument.Load(streamReader, LoadOptions.SetLineInfo);
        var viewBoxInfo = ViewBoxParser.ReadViewBox(xmlDocument.Root!, OutputUnit.Millimeter);

        Assert.AreEqual(0, viewBoxInfo.ViewBox.MinX);
        Assert.AreEqual(0, viewBoxInfo.ViewBox.MinX);
        Assert.IsTrue(Math.Abs(UnitSizeConverter.ConvertGraphicSizeToMM(300, "px") - viewBoxInfo.ViewBox.Width) < _tolerance);
        Assert.IsTrue(Math.Abs(UnitSizeConverter.ConvertGraphicSizeToMM(150, "px") - viewBoxInfo.ViewBox.Height) < _tolerance);

        Assert.IsTrue(Math.Abs(UnitSizeConverter.ConvertGraphicSizeToMM(300, "px") - viewBoxInfo.PhysicalSize.Width) < _tolerance);
        Assert.IsTrue(Math.Abs(UnitSizeConverter.ConvertGraphicSizeToMM(150, "px") - viewBoxInfo.PhysicalSize.Height) < _tolerance);
    }
}
