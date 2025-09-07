using Mekatrol.CAM.Core.Parsers.Svg;
using System.Text;
using System.Xml;

namespace MekatrolCAM.UnitTest.Svg;

[TestClass]
public partial class SvgParserTests
{
    private readonly ApplicationServices _services;

    public SvgParserTests()
    {
        _services = ApplicationServices.Instance;
    }

    [TestMethod]
    public void ServiceAvailable()
    {
        var svgParser = _services.GetService<ISvgParser>();
        Assert.IsNotNull(svgParser);
    }

    [TestMethod]
    public void ParseNull()
    {
        var svgParser = _services.GetRequiredService<ISvgParser>();
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        var list = svgParser.Parse(null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        Assert.IsNotNull(list);
        Assert.AreEqual(0, list.Count);
    }

    [TestMethod]
    public void ParseEmpty()
    {
        var svgParser = _services.GetRequiredService<ISvgParser>();
        var byteArray = Encoding.ASCII.GetBytes(string.Empty);
        using var stream = new MemoryStream(byteArray);
        using var streamReader = new StreamReader(stream);

        var list = svgParser.Parse(streamReader);
        Assert.IsNotNull(list);
        Assert.AreEqual(0, list.Count);
    }

    [TestMethod]
    public void MalformedHeader()
    {
        var svgParser = _services.GetRequiredService<ISvgParser>();
        var byteArray = Encoding.ASCII.GetBytes(@"<?xml version=""1.0"" encoding=""utf-8""");
        using var stream = new MemoryStream(byteArray);
        using var streamReader = new StreamReader(stream);

        try
        {
            var list = svgParser.Parse(streamReader);
            Assert.Fail("Expected exception");
        }
        catch (XmlException ex)
        {
            Assert.AreEqual("Unexpected end of file has occurred. Line 1, position 37.", ex.Message);
        }
    }

    [TestMethod]
    public void RootElementMissing()
    {
        var svgParser = _services.GetRequiredService<ISvgParser>();
        var byteArray = Encoding.ASCII.GetBytes(@"<?xml version=""1.0"" encoding=""utf-8""?>");
        using var stream = new MemoryStream(byteArray);
        using var streamReader = new StreamReader(stream);

        try
        {
            var list = svgParser.Parse(streamReader);
            Assert.Fail("Expected exception");
        }
        catch (XmlException ex)
        {
            Assert.AreEqual("Root element is missing.", ex.Message);
        }
    }

    [TestMethod]
    public void MissingSvgTag()
    {
        var svgParser = _services.GetRequiredService<ISvgParser>();
        var byteArray = Encoding.ASCII.GetBytes(@"<bob></bob>");
        using var stream = new MemoryStream(byteArray);
        using var streamReader = new StreamReader(stream);

        try
        {
            var list = svgParser.Parse(streamReader);
            Assert.Fail("Expected exception");
        }
        catch (XmlException ex)
        {
            Assert.AreEqual("Expecting <svg> element Line 1, position 2.", ex.Message);
        }
    }
}

#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
