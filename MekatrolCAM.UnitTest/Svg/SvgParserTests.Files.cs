using Mekatrol.CAM.Core.Parsers.Svg;

namespace MekatrolCAM.UnitTest.Svg;

public partial class SvgParserTests
{
    [TestMethod]
    public void ParseFiles()
    {
        var files = Directory.GetFiles(@"TestFiles\svg", "*.svg");

        Assert.IsTrue(files.Length > 0);

        var svgParser = _services.GetRequiredService<ISvgParser>();

        foreach (var fileName in files)
        {
            using var fileStream = File.OpenRead(fileName);
            using var streamReader = new StreamReader(fileStream);

            try
            {
                svgParser.Parse(streamReader);
            }
            catch (Exception ex)
            {
                Assert.Fail($"File '{fileName}' failed with message {ex.Message}");
            }
        }
    }
}
