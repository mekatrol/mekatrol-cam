using Mekatrol.CAM.Core.Geometry;
using Mekatrol.CAM.Core.Geometry.Entities;
using Mekatrol.CAM.Core.Parsers.Svg;
using System.Text;

namespace MekatrolCAM.UnitTest.Svg;

public partial class SvgParserTests
{
    [TestMethod]
    public void ParsePathLineNoClose()
    {
        const string pathSvg = @"
                <svg height=""100"" width=""100"">
                  <path d=""m 54.170965,204.5469 L-0.540415-1.35062-0.540004-2.94263,0.0013-5.0665 "" />
                </svg>
                ";

        var svgParser = _services.GetRequiredService<ISvgParser>();
        var byteArray = Encoding.ASCII.GetBytes(pathSvg);
        using var stream = new MemoryStream(byteArray);
        using var streamReader = new StreamReader(stream);

        var list = svgParser.Parse(streamReader);
        Assert.IsNotNull(list);
        Assert.AreEqual(1, list.Count);

        var geometry = list[0];
        Assert.AreEqual(GeometricEntityType.Path, geometry.Type);

        var path = (PathEntity)geometry;
        Assert.AreEqual(3, path.Entities.Count);
        AssertEx.WithinTolerance(54.170965, path.Location.X);
        AssertEx.WithinTolerance(204.5469, path.Location.Y);
        Assert.IsFalse(path.IsClosed);

        Assert.AreEqual(GeometricEntityType.Line, path.Entities[0].Type);
        var line = (LineEntity)path.Entities[0];
        AssertEx.WithinTolerance(54.170965, line.Location.X);
        AssertEx.WithinTolerance(204.5469, line.Location.Y);
        AssertEx.WithinTolerance(-0.540415, line.EndLocation.X);
        AssertEx.WithinTolerance(-1.35062, line.EndLocation.Y);

        Assert.AreEqual(GeometricEntityType.Line, path.Entities[1].Type);
        line = (LineEntity)path.Entities[1];
        AssertEx.WithinTolerance(-0.540415, line.Location.X);
        AssertEx.WithinTolerance(-1.35062, line.Location.Y);
        AssertEx.WithinTolerance(-0.540004, line.EndLocation.X);
        AssertEx.WithinTolerance(-2.94263, line.EndLocation.Y);

        Assert.AreEqual(GeometricEntityType.Line, path.Entities[2].Type);
        line = (LineEntity)path.Entities[2];
        AssertEx.WithinTolerance(-0.540004, line.Location.X);
        AssertEx.WithinTolerance(-2.94263, line.Location.Y);
        AssertEx.WithinTolerance(0.0013, line.EndLocation.X);
        AssertEx.WithinTolerance(-5.0665, line.EndLocation.Y);

        AssertEx.WithinTolerance(54.170965, path.Location.X);
        AssertEx.WithinTolerance(204.5469, path.Location.Y);
    }

    [TestMethod]
    public void ParsePathLineWithClose()
    {
        const string pathSvg = @"
                <svg height=""100"" width=""100"">
                  <path d=""m 54.170965,204.5469 l-0.540415-1.35062-0.540004-2.94263,0.0013-5.0665z"" />
                </svg>
                ";

        var svgParser = _services.GetRequiredService<ISvgParser>();
        var byteArray = Encoding.ASCII.GetBytes(pathSvg);
        using var stream = new MemoryStream(byteArray);
        using var streamReader = new StreamReader(stream);

        var list = svgParser.Parse(streamReader);
        Assert.IsNotNull(list);
        Assert.AreEqual(1, list.Count);

        var geometry = list[0];
        Assert.AreEqual(GeometricEntityType.Path, geometry.Type);

        var path = (PathEntity)geometry;
        Assert.AreEqual(4, path.Entities.Count);
        AssertEx.WithinTolerance(54.170965, path.Location.X);
        AssertEx.WithinTolerance(204.5469, path.Location.Y);
        Assert.IsTrue(path.IsClosed);

        Assert.AreEqual(GeometricEntityType.Line, path.Entities[0].Type);
        var line = (LineEntity)path.Entities[0];
        AssertEx.WithinTolerance(54.170965, line.Location.X);
        AssertEx.WithinTolerance(204.5469, line.Location.Y);
        AssertEx.WithinTolerance(53.63055, line.EndLocation.X);
        AssertEx.WithinTolerance(203.19628, line.EndLocation.Y);

        Assert.AreEqual(GeometricEntityType.Line, path.Entities[1].Type);
        line = (LineEntity)path.Entities[1];
        AssertEx.WithinTolerance(53.63055, line.Location.X);
        AssertEx.WithinTolerance(203.19628, line.Location.Y);
        AssertEx.WithinTolerance(53.090546, line.EndLocation.X);
        AssertEx.WithinTolerance(200.25365, line.EndLocation.Y);

        Assert.AreEqual(GeometricEntityType.Line, path.Entities[2].Type);
        line = (LineEntity)path.Entities[2];
        AssertEx.WithinTolerance(53.090546, line.Location.X);
        AssertEx.WithinTolerance(200.25365, line.Location.Y);
        AssertEx.WithinTolerance(53.091846, line.EndLocation.X);
        AssertEx.WithinTolerance(195.18715, line.EndLocation.Y);

        Assert.AreEqual(GeometricEntityType.Line, path.Entities[3].Type);
        line = (LineEntity)path.Entities[3];
        AssertEx.WithinTolerance(53.091846, line.Location.X);
        AssertEx.WithinTolerance(195.18715, line.Location.Y);
        AssertEx.WithinTolerance(54.170965, line.EndLocation.X);
        AssertEx.WithinTolerance(204.5469, line.EndLocation.Y);

        AssertEx.WithinTolerance(54.170965, path.Location.X);
        AssertEx.WithinTolerance(204.5469, path.Location.Y);
    }

    [TestMethod]
    public void ParseHorizontalLine()
    {
        const string pathSvg = @"
                <svg height=""100"" width=""100"">
                  <path d=""M 10,5 h20 m -10,-5 h-20 h+5 M 5, 6 H100 H-50 H0"" />
                </svg>
                ";

        var svgParser = _services.GetRequiredService<ISvgParser>();
        var byteArray = Encoding.ASCII.GetBytes(pathSvg);
        using var stream = new MemoryStream(byteArray);
        using var streamReader = new StreamReader(stream);

        var list = svgParser.Parse(streamReader);
        Assert.IsNotNull(list);
        Assert.AreEqual(1, list.Count);

        var geometry = list[0];
        Assert.AreEqual(GeometricEntityType.Path, geometry.Type);

        var path = (PathEntity)geometry;
        Assert.AreEqual(6, path.Entities.Count);
        AssertEx.WithinTolerance(10, path.Location.X);
        AssertEx.WithinTolerance(5, path.Location.Y);
        Assert.IsFalse(path.IsClosed);

        Assert.AreEqual(GeometricEntityType.Line, path.Entities[0].Type);
        var line = (LineEntity)path.Entities[0];
        AssertEx.WithinTolerance(10, line.Location.X);
        AssertEx.WithinTolerance(5, line.Location.Y);
        AssertEx.WithinTolerance(30, line.EndLocation.X);
        AssertEx.WithinTolerance(5, line.EndLocation.Y);

        Assert.AreEqual(GeometricEntityType.Line, path.Entities[1].Type);
        line = (LineEntity)path.Entities[1];
        AssertEx.WithinTolerance(20, line.Location.X);
        AssertEx.WithinTolerance(0, line.Location.Y);
        AssertEx.WithinTolerance(0, line.EndLocation.X);
        AssertEx.WithinTolerance(0, line.EndLocation.Y);

        Assert.AreEqual(GeometricEntityType.Line, path.Entities[2].Type);
        line = (LineEntity)path.Entities[2];
        AssertEx.WithinTolerance(0, line.Location.X);
        AssertEx.WithinTolerance(0, line.Location.Y);
        AssertEx.WithinTolerance(5, line.EndLocation.X);
        AssertEx.WithinTolerance(0, line.EndLocation.Y);

        Assert.AreEqual(GeometricEntityType.Line, path.Entities[3].Type);
        line = (LineEntity)path.Entities[3];
        AssertEx.WithinTolerance(5, line.Location.X);
        AssertEx.WithinTolerance(6, line.Location.Y);
        AssertEx.WithinTolerance(100, line.EndLocation.X);
        AssertEx.WithinTolerance(6, line.EndLocation.Y);

        Assert.AreEqual(GeometricEntityType.Line, path.Entities[4].Type);
        line = (LineEntity)path.Entities[4];
        AssertEx.WithinTolerance(100, line.Location.X);
        AssertEx.WithinTolerance(6, line.Location.Y);
        AssertEx.WithinTolerance(-50, line.EndLocation.X);
        AssertEx.WithinTolerance(6, line.EndLocation.Y);

        Assert.AreEqual(GeometricEntityType.Line, path.Entities[5].Type);
        line = (LineEntity)path.Entities[5];
        AssertEx.WithinTolerance(-50, line.Location.X);
        AssertEx.WithinTolerance(6, line.Location.Y);
        AssertEx.WithinTolerance(0, line.EndLocation.X);
        AssertEx.WithinTolerance(6, line.EndLocation.Y);
    }

    [TestMethod]
    public void ParsePathLine()
    {
        const string pathSvg = @"
                <svg height=""100"" width=""100"">
                  <path d=""M 10,5 l20,6 m -10,-5 l-20,-2 l+5,1 M 5, 6 L100,101 L-50,51 L0,0"" />
                </svg>
                ";

        var svgParser = _services.GetRequiredService<ISvgParser>();
        var byteArray = Encoding.ASCII.GetBytes(pathSvg);
        using var stream = new MemoryStream(byteArray);
        using var streamReader = new StreamReader(stream);

        var list = svgParser.Parse(streamReader);
        Assert.IsNotNull(list);
        Assert.AreEqual(1, list.Count);

        var geometry = list[0];
        Assert.AreEqual(GeometricEntityType.Path, geometry.Type);

        var path = (PathEntity)geometry;
        Assert.AreEqual(6, path.Entities.Count);
        AssertEx.WithinTolerance(10, path.Location.X);
        AssertEx.WithinTolerance(5, path.Location.Y);
        Assert.IsFalse(path.IsClosed);

        Assert.AreEqual(GeometricEntityType.Line, path.Entities[0].Type);
        var line = (LineEntity)path.Entities[0];
        AssertEx.WithinTolerance(10, line.Location.X);
        AssertEx.WithinTolerance(5, line.Location.Y);
        AssertEx.WithinTolerance(30, line.EndLocation.X);
        AssertEx.WithinTolerance(11, line.EndLocation.Y);

        Assert.AreEqual(GeometricEntityType.Line, path.Entities[1].Type);
        line = (LineEntity)path.Entities[1];
        AssertEx.WithinTolerance(20, line.Location.X);
        AssertEx.WithinTolerance(6, line.Location.Y);
        AssertEx.WithinTolerance(0, line.EndLocation.X);
        AssertEx.WithinTolerance(4, line.EndLocation.Y);

        Assert.AreEqual(GeometricEntityType.Line, path.Entities[2].Type);
        line = (LineEntity)path.Entities[2];
        AssertEx.WithinTolerance(0, line.Location.X);
        AssertEx.WithinTolerance(4, line.Location.Y);
        AssertEx.WithinTolerance(5, line.EndLocation.X);
        AssertEx.WithinTolerance(5, line.EndLocation.Y);

        Assert.AreEqual(GeometricEntityType.Line, path.Entities[3].Type);
        line = (LineEntity)path.Entities[3];
        AssertEx.WithinTolerance(5, line.Location.X);
        AssertEx.WithinTolerance(6, line.Location.Y);
        AssertEx.WithinTolerance(100, line.EndLocation.X);
        AssertEx.WithinTolerance(101, line.EndLocation.Y);

        Assert.AreEqual(GeometricEntityType.Line, path.Entities[4].Type);
        line = (LineEntity)path.Entities[4];
        AssertEx.WithinTolerance(100, line.Location.X);
        AssertEx.WithinTolerance(101, line.Location.Y);
        AssertEx.WithinTolerance(-50, line.EndLocation.X);
        AssertEx.WithinTolerance(51, line.EndLocation.Y);

        Assert.AreEqual(GeometricEntityType.Line, path.Entities[5].Type);
        line = (LineEntity)path.Entities[5];
        AssertEx.WithinTolerance(-50, line.Location.X);
        AssertEx.WithinTolerance(51, line.Location.Y);
        AssertEx.WithinTolerance(0, line.EndLocation.X);
        AssertEx.WithinTolerance(0, line.EndLocation.Y);
    }

    [TestMethod]
    public void ParseVerticalLine()
    {
        const string pathSvg = @"
                <svg height=""100"" width=""100"">
                  <path d=""M 10,5 v20 m -10,-5 v-20 v+5 M 5, 6 V100 V-50 V0"" />
                </svg>
                ";

        var svgParser = _services.GetRequiredService<ISvgParser>();
        var byteArray = Encoding.ASCII.GetBytes(pathSvg);
        using var stream = new MemoryStream(byteArray);
        using var streamReader = new StreamReader(stream);

        var list = svgParser.Parse(streamReader);
        Assert.IsNotNull(list);
        Assert.AreEqual(1, list.Count);

        var geometry = list[0];
        Assert.AreEqual(GeometricEntityType.Path, geometry.Type);

        var path = (PathEntity)geometry;
        Assert.AreEqual(6, path.Entities.Count);
        AssertEx.WithinTolerance(10, path.Location.X);
        AssertEx.WithinTolerance(5, path.Location.Y);
        Assert.IsFalse(path.IsClosed);

        Assert.AreEqual(GeometricEntityType.Line, path.Entities[0].Type);
        var line = (LineEntity)path.Entities[0];
        AssertEx.WithinTolerance(10, line.Location.X);
        AssertEx.WithinTolerance(5, line.Location.Y);
        AssertEx.WithinTolerance(10, line.EndLocation.X);
        AssertEx.WithinTolerance(25, line.EndLocation.Y);

        Assert.AreEqual(GeometricEntityType.Line, path.Entities[1].Type);
        line = (LineEntity)path.Entities[1];
        AssertEx.WithinTolerance(0, line.Location.X);
        AssertEx.WithinTolerance(20, line.Location.Y);
        AssertEx.WithinTolerance(0, line.EndLocation.X);
        AssertEx.WithinTolerance(0, line.EndLocation.Y);

        Assert.AreEqual(GeometricEntityType.Line, path.Entities[2].Type);
        line = (LineEntity)path.Entities[2];
        AssertEx.WithinTolerance(0, line.Location.X);
        AssertEx.WithinTolerance(0, line.Location.Y);
        AssertEx.WithinTolerance(0, line.EndLocation.X);
        AssertEx.WithinTolerance(5, line.EndLocation.Y);

        Assert.AreEqual(GeometricEntityType.Line, path.Entities[3].Type);
        line = (LineEntity)path.Entities[3];
        AssertEx.WithinTolerance(5, line.Location.X);
        AssertEx.WithinTolerance(6, line.Location.Y);
        AssertEx.WithinTolerance(5, line.EndLocation.X);
        AssertEx.WithinTolerance(100, line.EndLocation.Y);

        Assert.AreEqual(GeometricEntityType.Line, path.Entities[4].Type);
        line = (LineEntity)path.Entities[4];
        AssertEx.WithinTolerance(5, line.Location.X);
        AssertEx.WithinTolerance(100, line.Location.Y);
        AssertEx.WithinTolerance(5, line.EndLocation.X);
        AssertEx.WithinTolerance(-50, line.EndLocation.Y);

        Assert.AreEqual(GeometricEntityType.Line, path.Entities[5].Type);
        line = (LineEntity)path.Entities[5];
        AssertEx.WithinTolerance(5, line.Location.X);
        AssertEx.WithinTolerance(-50, line.Location.Y);
        AssertEx.WithinTolerance(5, line.EndLocation.X);
        AssertEx.WithinTolerance(0, line.EndLocation.Y);
    }

    [TestMethod]
    public void ParsePath1()
    {
        const string pathSvg = @"
                <svg height=""100"" width=""100"">
                  <path d=""m 54.170965,204.5469 c -0.540415,-1.35062 -0.540004,-2.94263 0.0013,-5.0665 0.663191,-2.60208 1.013154,-3.13023 2.074162,-3.13023 0.71794,0 1.065707,-0.20069 1.71928,-0.99218 2.124835,-2.57323 4.744138,-8.88471 7.151331,-17.23187 1.802865,-6.2516 1.880017,-6.36902 5.525098,-8.40902 2.983563,-1.66978 4.007108,-2.62331 5.140026,-4.78844 1.137703,-2.17427 1.79689,-4.33266 2.824066,-9.24692 0.553077,-2.64603 1.062575,-4.30881 1.720755,-5.61579 1.125781,-2.23549 1.156086,-2.7236 0.207433,-3.34078 -1.752967,-1.14046 -3.987498,-3.61166 -4.698636,-5.19628 -0.623923,-1.39029 -0.706979,-1.86062 -0.597866,-3.38561 0.195207,-2.72827 2.188284,-6.61602 3.391738,-6.61602 0.405127,0 0.516019,0.18438 0.517178,0.8599 0.0024,1.42751 0.53825,2.90679 1.312177,3.62275 0.402336,0.3722 1.491748,0.96119 2.420912,1.30886 3.965919,1.48398 6.319602,2.90311 6.319602,3.81034 0,0.23347 0.948232,0.32402 3.505729,0.33479 2.994755,0.0126 4.045625,0.14088 7.209896,0.87998 9.414374,2.19899 10.360534,2.33917 13.009124,1.92741 3.32506,-0.51694 8.36243,-2.50287 10.58729,-4.17393 0.46323,-0.34793 0.9226,-0.6326 1.02081,-0.6326 0.36251,0 3.5698,-2.71028 4.71621,-3.98537 2.68428,-2.98555 2.95614,-4.86667 1.43153,-9.90526 -1.07261,-3.54478 -1.41951,-5.20165 -1.67065,-7.97916 -0.18626,-2.05993 -0.31712,-2.19564 -2.12494,-2.20368 -1.49844,-0.007 -3.55798,-0.80262 -4.66917,-1.80448 -1.72167,-1.5523 -3.69801,-6.45612 -2.91679,-7.23734 0.39592,-0.39592 5.45377,-0.37638 6.91382,0.0267 0.639,0.17641 1.6189,0.60929 2.17755,0.96196 1.07547,0.67891 1.68923,0.80227 1.94363,0.39064 0.1723,-0.27879 -2.69416,-1.68294 -4.34072,-2.12632 -0.58254,-0.15686 -2.83494,-0.51381 -5.00533,-0.79323 -3.99712,-0.51459 -6.61411,-1.20746 -8.16963,-2.16299 -1.39309,-0.85575 -3.2729,-3.174789 -4.93679,-6.090294 -0.85748,-1.502498 -1.63157,-2.791345 -1.72021,-2.864106 -0.0886,-0.07276 -0.31592,-0.429948 -0.50507,-0.79375 -0.18915,-0.363802 -1.36499,-2.090208 -2.61298,-3.836458 -3.0106,-4.212584 -3.886043,-6.069201 -3.992244,-8.466667 -0.109144,-2.463844 0.257651,-3.175 1.637564,-3.175 1.48193,0 1.96363,0.783202 2.39904,3.900685 0.19431,1.391187 0.5075,2.809062 0.69598,3.150832 0.32827,0.595242 0.35058,0.568645 0.53,-0.631829 0.62156,-4.158807 2.5663,-6.33453 4.20093,-4.699896 0.71059,0.710586 0.668,1.436657 -0.22788,3.884555 -0.41638,1.137713 -0.83994,2.522762 -0.94124,3.077886 -0.23569,1.291465 0.22314,3.506118 0.97083,4.685924 l 0.58262,0.919343 0.17261,-2.547069 c 0.17999,-2.655925 0.66655,-4.527031 1.5565,-5.985743 0.45037,-0.738199 0.67363,-0.859896 1.57753,-0.859896 1.79346,0 2.03938,1.314826 0.94279,5.040672 -2.56923,8.729333 -0.79912,12.618848 6.3253,13.898771 l 1.03718,0.18633 -0.54065,-0.76549 c -0.86145,-1.219704 -2.09903,-3.817055 -2.56229,-5.377576 -0.57291,-1.929836 -0.53748,-4.720088 0.10174,-8.012388 0.2906,-1.496748 0.52872,-3.576059 0.52916,-4.620691 9e-4,-2.137401 0.34381,-2.730878 1.57789,-2.730878 0.41005,0 1.02662,0.228109 1.37017,0.506909 0.57869,0.469635 0.62316,0.688503 0.60481,2.976562 -0.0147,1.827612 -0.20377,3.123106 -0.72741,4.983196 -0.82583,2.933462 -0.87588,4.134587 -0.25435,6.10354 l 0.45363,1.437085 0.15769,-2.513542 c 0.12003,-1.913199 0.32323,-2.874889 0.85078,-4.026446 1.75485,-3.830589 6.23671,-6.562776 8.34413,-5.086676 1.31922,0.924014 0.31197,2.640289 -2.70197,4.603947 -0.93607,0.609872 -1.89504,1.40356 -2.13104,1.763751 -0.236,0.360191 -0.59837,1.617675 -0.80526,2.794409 -0.38415,2.184935 -0.26632,4.157298 0.42675,7.143228 0.28377,1.22253 0.3094,1.24603 1.9191,1.75983 0.89733,0.28641 1.83019,0.60105 2.07302,0.6992 0.39515,0.15971 0.38457,0.003 -0.10073,-1.49525 -0.36616,-1.13018 -0.58426,-2.674588 -0.67161,-4.755776 -0.12166,-2.898632 -0.0886,-3.171151 0.55581,-4.578696 0.78887,-1.723119 3.58927,-4.799805 4.85974,-5.33921 1.19275,-0.506404 2.10246,-0.45033 2.72048,0.167689 1.05735,1.057346 0.46546,3.209815 -1.15864,4.213567 -1.1733,0.725141 -2.64387,2.360827 -3.32782,3.70147 -0.7922,1.552854 -0.69283,3.231639 0.3863,6.525916 1.27952,3.90605 1.99421,4.95342 3.7613,5.5122 l 0.58718,0.18568 -0.58718,-1.27202 c -0.52142,-1.12956 -0.58696,-1.70168 -0.58521,-5.10848 0.002,-4.2769 0.29523,-5.656711 1.86879,-8.799994 1.58137,-3.158875 2.72112,-4.195678 4.12901,-3.756073 0.79721,0.248926 1.00268,0.587299 1.00764,1.659399 0.005,1.050317 -0.29146,1.648251 -1.59753,3.223752 -1.05104,1.267849 -1.91864,2.853888 -2.1983,4.018658 -0.15615,0.650346 -0.0594,0.602825 1.12829,-0.553985 1.5017,-1.462701 3.14859,-2.085097 4.32938,-1.636164 0.63521,0.241508 0.72346,0.415468 0.72346,1.426113 0,1.253744 -0.36188,1.72038 -2.17516,2.804841 -1.34805,0.806223 -2.99816,2.741083 -3.53377,4.143563 -0.41711,1.09218 -0.41668,1.2056 0.007,1.8523 1.0707,1.63409 4.2538,1.88226 7.15709,0.558 1.19994,-0.54732 2.11727,-1.28398 3.75749,-3.01746 1.95834,-2.06968 2.26459,-2.29337 3.13986,-2.29337 1.27099,0 1.70161,0.61159 1.70161,2.4167 0,1.32221 -0.0824,1.50403 -1.24387,2.74576 -1.27641,1.36459 -4.51617,3.15939 -6.40731,3.54959 -0.68841,0.14204 -0.80737,0.24084 -0.4983,0.4138 0.22876,0.12803 2.39027,0.10483 4.95131,-0.0531 5.0097,-0.30897 5.31483,-0.244 5.31483,1.13176 0,1.13797 -1.2935,4.98944 -1.98824,5.92009 -0.3306,0.44287 -1.91608,1.61593 -3.5233,2.60682 -1.60722,0.99089 -3.09754,1.99777 -3.31183,2.23751 -0.74619,0.83483 -1.61686,4.72033 -1.9949,8.90256 -0.2186,2.41834 -0.17035,3.00982 0.5611,6.87916 1.0701,5.66071 1.29022,11.78415 0.52056,14.48088 l -0.50827,1.78088 0.95525,0.18167 c 0.52539,0.0999 2.32448,0.0386 3.99797,-0.13621 8.54931,-0.89317 9.47407,-0.94577 10.16131,-0.57797 0.96371,0.51577 1.48005,1.82462 1.48237,3.75761 0.002,1.63788 -0.0426,1.74585 -1.78594,4.32564 -0.98338,1.45521 -1.9225,2.78888 -2.08694,2.96372 -0.16445,0.17484 -1.2498,1.72266 -2.4119,3.43959 -2.22602,3.2888 -3.21543,5.07944 -3.77164,6.82586 -0.63165,1.98329 -1.82856,4.04188 -3.09858,5.32932 -1.23606,1.253 -5.10961,3.53422 -6.00118,3.53422 -0.24103,0 -1.11574,0.23812 -1.94381,0.52916 -2.09353,0.73582 -2.71454,0.7036 -2.71454,-0.14082 0,-0.82303 1.42596,-4.61149 2.31449,-6.14907 0.8116,-1.40447 1.34967,-1.70257 2.68713,-1.4887 1.00969,0.16146 1.06582,0.13171 1.43964,-0.76297 0.21395,-0.51204 1.13431,-2.03942 2.04524,-3.39418 1.60741,-2.39056 5.05294,-9.14816 5.41486,-10.61999 0.15706,-0.63868 0.10267,-0.72757 -0.44507,-0.72738 -1.48669,5.3e-4 -10.16537,2.29481 -10.93981,2.89204 -0.28255,0.2179 -0.48394,0.27886 -3.47632,1.05226 -1.29699,0.33521 -2.32975,0.82232 -3.30729,1.55992 -1.88279,1.42064 -4.20456,2.54932 -6.74968,3.28121 -1.84637,0.53096 -2.67458,0.59867 -7.37172,0.60261 -6.04514,0.005 -9.47954,-0.52029 -17.33021,-2.65105 -3.94219,-1.06995 -5.555517,-0.86202 -7.587173,0.97786 -3.247199,2.94068 -7.579352,5.32064 -15.166996,8.33232 -4.058555,1.61092 -5.038889,2.10544 -5.738238,2.89465 -1.479683,1.6698 -5.017752,8.87932 -7.789058,15.87175 l -1.625223,4.10069 -2.366401,2.35965 c -2.659732,2.65215 -3.256576,2.98333 -3.887785,2.15724 l -0.421421,-0.55152 -0.661458,0.69521 c -1.026574,1.07896 -2.158549,1.66662 -3.212418,1.6677 -0.878209,7.9e-4 -1.001118,-0.0931 -1.389062,-1.0627 z m 2.617104,0.0428 c 0.925775,-0.57124 2.248959,-1.85983 2.248959,-2.19016 0,-0.50755 0.825047,-0.18471 1.106475,0.43296 0.16576,0.3638 0.386909,0.66145 0.491445,0.66145 0.104535,0 1.268821,-1.07568 2.587302,-2.3904 2.315494,-2.30889 2.434504,-2.48707 3.490124,-5.22552 2.336212,-6.0605 6.078318,-14.01473 7.706877,-16.38176 1.039764,-1.51124 1.366495,-1.70254 5.820224,-3.4076 7.053755,-2.70045 12.604959,-5.70887 15.524954,-8.4136 0.68217,-0.63187 1.500373,-1.28456 1.818227,-1.45041 1.12689,-0.58798 4.308084,-0.34871 8.177484,0.61506 10.23398,2.54903 15.77671,3.07724 20.99422,2.00071 3.19108,-0.65841 4.99277,-1.37814 7.2435,-2.89358 2.52405,-1.69948 2.68885,-1.77846 5.20792,-2.49581 1.23692,-0.35224 3.32052,-1.01971 4.6302,-1.48327 5.23088,-1.85146 10.26219,-2.66549 10.62535,-1.71911 0.32114,0.83686 -3.29163,8.39726 -5.78374,12.10356 -0.87078,1.29503 -1.75242,2.75952 -1.9592,3.25442 -0.36267,0.86799 -0.42719,0.89981 -1.82431,0.89981 -1.40827,0 -1.46731,0.0301 -2.13429,1.08663 -0.73471,1.16385 -2.60067,5.77574 -2.41285,5.96356 0.0621,0.0621 0.72815,-0.12953 1.48007,-0.42591 0.75191,-0.29637 1.61646,-0.53886 1.92122,-0.53886 0.9888,0 4.80661,-2.20579 6.11707,-3.53422 1.27001,-1.28743 2.46692,-3.34602 3.09857,-5.32932 0.55621,-1.74642 1.54563,-3.53705 3.77164,-6.82586 1.1621,-1.71693 2.24745,-3.26474 2.4119,-3.43958 0.16444,-0.17485 1.11033,-1.51647 2.10196,-2.9814 1.78516,-2.63718 1.80173,-2.67804 1.67692,-4.13351 -0.26641,-3.10678 -0.75043,-3.28876 -6.91947,-2.60163 -4.80226,0.5349 -7.96264,0.66293 -9.06602,0.36729 -0.50949,-0.13652 -0.58884,-0.31445 -0.52917,-1.18663 0.0386,-0.56431 0.14321,-1.0262 0.23247,-1.02641 0.45641,-0.001 0.81254,-3.32482 0.67001,-6.2531 -0.19158,-3.93609 -0.18464,-3.85459 -0.36862,-4.33063 -0.0844,-0.21828 -0.18341,-0.69453 -0.22009,-1.05833 -0.0367,-0.3638 -0.12634,-1.01865 -0.19921,-1.45521 -0.0729,-0.43656 -0.20516,-1.27 -0.29397,-1.85208 -0.0888,-0.58209 -0.24265,-1.29646 -0.34187,-1.5875 -0.52296,-1.53394 -0.45367,-4.32415 0.20038,-8.06979 0.16517,-0.94589 0.33629,-2.01745 0.38027,-2.38125 0.044,-0.36381 0.15231,-0.84006 0.24075,-1.05834 0.0884,-0.21828 0.2035,-0.635 0.25571,-0.92604 0.31575,-1.76007 0.64091,-2.08602 4.37932,-4.38993 1.51156,-0.93155 3.01784,-2.05131 3.34729,-2.48835 0.57123,-0.75781 1.66815,-3.8201 1.82815,-5.10371 0.11552,-0.92679 -0.69059,-1.22215 -2.46795,-0.90426 -1.87914,0.33609 -7.45534,0.40007 -7.92921,0.091 -0.79692,-0.51982 -0.17114,-1.15993 1.82067,-1.86236 3.8188,-1.34673 6.34498,-3.37756 6.49102,-5.21822 0.10061,-1.26788 -0.26656,-1.95226 -1.04738,-1.95226 -0.43119,0 -1.17127,0.61668 -2.48143,2.06765 -2.53739,2.81012 -4.39768,3.81169 -7.36959,3.96776 -2.61338,0.13723 -3.84622,-0.22371 -4.73237,-1.38552 -0.8183,-1.07285 -0.81803,-1.30983 0.003,-3.01273 0.82228,-1.7045 2.25729,-3.317094 3.70876,-4.167711 1.44877,-0.84904 1.92728,-1.423055 1.86059,-2.231954 -0.12124,-1.470404 -3.00746,-0.540304 -4.53401,1.461107 -1.2928,1.694949 -2.22257,0.888125 -1.49174,-1.294492 0.525,-1.567935 1.08371,-2.567953 2.1626,-3.870782 1.10682,-1.336566 1.50161,-2.062262 1.43934,-2.645833 -0.0233,-0.218282 -0.0561,-0.545703 -0.0729,-0.727604 -0.0591,-0.640702 -1.36783,-0.347634 -2.11869,0.474453 -1.40566,1.539002 -3.78822,7.194013 -3.33794,7.92259 0.0911,0.147434 0.0826,0.402429 -0.0189,0.566657 -0.1015,0.164228 -0.18239,1.573162 -0.17977,3.130969 0.004,2.30372 0.10949,3.06908 0.5658,4.10066 0.76559,1.73072 0.73597,2.25936 -0.12659,2.25936 -1.0999,0 -2.48382,-0.79958 -3.16269,-1.82729 -1.05864,-1.60263 -2.55354,-6.53102 -2.57566,-8.491462 -0.0288,-2.547423 1.32445,-4.834003 3.91594,-6.616999 2.06253,-1.419057 2.19525,-3.437168 0.22604,-3.437168 -0.8037,0 -1.2127,0.260892 -2.77795,1.772014 -1.21463,1.172629 -2.0967,2.313695 -2.60774,3.373437 -0.72368,1.500665 -0.76639,1.767897 -0.67888,4.247258 0.061,1.729662 0.28464,3.241344 0.64569,4.36563 0.60866,1.8953 0.70679,2.91041 0.28134,2.91041 -0.15279,0 -1.33242,-0.36827 -2.62142,-0.81839 -2.29023,-0.79975 -2.64544,-1.09164 -2.76458,-2.27179 -0.0173,-0.17163 -0.11711,-0.39766 -0.22175,-0.5023 -0.10464,-0.10463 -0.11997,-0.470225 -0.0341,-0.812423 0.10071,-0.401295 0.0403,-0.622179 -0.17018,-0.622179 -0.2215,0 -0.27533,-0.255039 -0.16757,-0.793864 0.0873,-0.436625 0.0416,-1.101948 -0.10153,-1.478495 -0.316,-0.831149 0.55863,-5.57592 1.21152,-6.572358 0.23101,-0.352559 1.16239,-1.124696 2.06975,-1.71586 3.27203,-2.131813 3.9535,-3.521538 1.80294,-3.676754 -2.44942,-0.176787 -6.02339,3.207534 -6.82852,6.466179 -0.17282,0.699443 -0.25161,1.835063 -0.33266,4.794589 -0.025,0.912065 -0.12449,1.12448 -0.52678,1.12448 -0.38228,0 -0.63823,-0.439794 -1.11639,-1.918229 -0.65647,-2.029773 -0.71904,-3.656053 -0.22271,-5.788852 0.15676,-0.673634 0.30971,-1.388009 0.33989,-1.5875 0.0302,-0.199491 0.14436,-0.505899 0.25373,-0.680906 0.51964,-0.831451 0.65519,-5.463007 0.17676,-6.039487 -0.32901,-0.396423 -1.84016,-0.552032 -1.85248,-0.190756 -0.004,0.109141 -0.0781,1.210469 -0.16528,2.447396 -0.0872,1.236927 -0.17423,2.487084 -0.19343,2.778125 -0.0192,0.291042 -0.13045,0.677995 -0.24721,0.859896 -0.11676,0.181901 -0.13548,0.455007 -0.0416,0.606903 0.0939,0.151895 0.0469,0.425273 -0.1043,0.607506 -0.15124,0.182233 -0.28237,0.563971 -0.29141,0.848306 -0.0831,2.612193 -0.0408,4.88716 0.10301,5.544056 0.2165,0.989063 1.81791,4.417612 2.53142,5.419637 0.29273,0.41111 0.53224,1.0142 0.53224,1.3402 0,0.56718 -0.0713,0.58607 -1.65423,0.43825 -2.01692,-0.18834 -4.32595,-1.0395 -5.54426,-2.043748 -0.49899,-0.411306 -1.21284,-1.336838 -1.58634,-2.056739 -0.60934,-1.17448 -0.67826,-1.604568 -0.67102,-4.18763 0.004,-1.583296 0.10031,-3.116845 0.21307,-3.407887 0.11275,-0.291041 0.53198,-1.747855 0.93163,-3.237363 0.60463,-2.25351 0.67435,-2.822919 0.41529,-3.391504 -0.46007,-1.009739 -1.34899,-0.769007 -2.08289,0.564076 -0.32671,0.593427 -0.63803,1.307802 -0.69184,1.5875 -0.0538,0.279698 -0.16526,0.687135 -0.24767,0.905416 -0.17809,0.471707 -0.1887,0.600295 -0.26634,3.227929 -0.0654,2.212987 -0.40605,2.910627 -1.19195,2.440925 -0.49789,-0.29757 -1.93777,-3.030158 -1.75365,-3.328065 0.0627,-0.101457 -0.0198,-0.47806 -0.18326,-0.836894 -0.31983,-0.701971 0.13739,-3.038783 1.06945,-5.465705 0.25295,-0.658661 0.44907,-1.614283 0.43582,-2.123606 -0.0219,-0.84293 -0.0939,-0.926042 -0.80215,-0.926042 -0.65821,0 -0.87514,0.188691 -1.40851,1.225117 -0.34676,0.673815 -0.67977,1.626315 -0.74002,2.116667 -0.27858,2.267042 -0.41872,2.611341 -1.06289,2.611341 -0.96475,0 -1.40191,-1.248737 -1.79056,-5.114753 -0.12049,-1.198474 -0.95484,-2.425872 -1.64904,-2.425872 -0.27104,0 -0.64339,0.297656 -0.827445,0.661458 -0.847354,1.674894 -0.06437,4.381548 2.246285,7.765066 4.17661,6.115838 5.3446,7.91012 6.5402,10.047189 1.6895,3.01986 3.7032,5.244824 5.52823,6.108194 1.78152,0.8428 2.93208,1.09767 7.76892,1.72095 2.53514,0.32668 4.54714,0.74695 5.74926,1.20091 4.11779,1.55499 5.26166,2.84099 2.6734,3.00555 -1.04427,0.0664 -1.59724,-0.0797 -2.87113,-0.75871 -1.29123,-0.68821 -1.9752,-0.86573 -3.76608,-0.97746 -1.20357,-0.0751 -2.61157,-0.0571 -3.12887,0.0399 l -0.94056,0.17645 0.4095,1.38024 c 1.26366,4.25922 3.3007,6.101 7.14485,6.45997 2.38462,0.22268 2.77317,0.66791 2.72486,3.12235 -0.0201,1.01865 0.0363,2.32834 0.12516,2.91042 0.13801,0.90373 1.39709,5.45687 2.28201,8.25231 0.35691,1.12747 -0.31491,3.75169 -1.39379,5.44433 -1.59324,2.49958 -5.56519,5.63657 -9.88232,7.80488 -3.35486,1.68501 -6.17523,2.47519 -9.38153,2.62841 -2.62038,0.12522 -3.11617,0.0571 -8.73125,-1.1989 -7.751285,-1.7339 -10.196778,-2.07677 -11.940861,-1.67419 -1.875486,0.43292 -2.076842,0.3972 -2.372855,-0.4209 -0.396605,-1.09611 -1.826538,-2.03066 -5.033164,-3.28948 -1.59309,-0.6254 -3.196344,-1.37639 -3.562784,-1.66886 -0.732046,-0.58428 -1.762882,-3.30766 -1.531027,-4.04485 0.304681,-0.96875 -1.057475,0.91217 -1.799785,2.48521 -0.894917,1.89643 -1.249227,4.12693 -0.750929,4.72734 0.183303,0.22087 0.257162,0.40157 0.164132,0.40157 -0.09303,0 0.123653,0.57781 0.481517,1.28402 0.594388,1.17297 4.126298,4.8014 4.673678,4.8014 0.123158,0 0.389229,0.3155 0.59127,0.7011 0.340995,0.65082 0.285075,0.86888 -0.779651,3.04028 -0.630846,1.28654 -1.207828,2.60815 -1.282181,2.93692 -0.184042,0.81375 -0.424619,2.04812 -0.554815,2.8467 -0.200025,1.22688 -0.305523,1.71809 -0.454602,2.11667 -0.08164,0.21828 -0.285192,1.00756 -0.452329,1.75395 -0.167138,0.7464 -0.342757,1.5203 -0.390266,1.71979 -0.492385,2.06751 -2.353418,5.57277 -3.51671,6.62373 -0.545038,0.49242 -1.921228,1.40585 -3.058199,2.02985 -3.544863,1.94554 -4.358288,2.9558 -5.065138,6.29085 -0.795795,3.7547 -3.991749,12.65455 -5.801276,16.15496 -1.68594,3.26133 -2.604414,4.32061 -3.74629,4.32061 -0.736037,0 -0.97615,0.14729 -1.318095,0.80854 -0.229961,0.44469 -0.469728,1.1293 -0.532815,1.52135 -0.06309,0.39205 -0.230498,1.27253 -0.372022,1.95663 -0.195673,0.94584 -0.162279,1.56384 0.139396,2.57969 0.44373,1.4942 0.806009,1.6401 2.096004,0.84412 z"" />
                </svg>
                ";

        var svgParser = _services.GetRequiredService<ISvgParser>();
        var byteArray = Encoding.ASCII.GetBytes(pathSvg);
        using var stream = new MemoryStream(byteArray);
        using var streamReader = new StreamReader(stream);

        var list = svgParser.Parse(streamReader);
        Assert.IsNotNull(list);
        Assert.AreEqual(1, list.Count);

        var geometry = list[0];
        Assert.AreEqual(GeometricEntityType.Path, geometry.Type);

        var path = (PathEntity)geometry;
        Assert.AreEqual(28, path.Entities.Count);
        AssertEx.WithinTolerance(54.170965, path.Location.X);
        AssertEx.WithinTolerance(204.5469, path.Location.Y);
        Assert.IsTrue(path.IsClosed);
    }

    [TestMethod]
    public void ParsePath2()
    {
        const string pathSvg = @"
                <svg height=""100"" width=""100"">
                  <path d=""m 54.170965,204.5469 l-0.540415-1.35062-0.540004-2.94263,0.0013-5.0665 m0.663191,-2.60208l1.013154,-3.13023 2.074162,-3.13023 0.71794,0 1.065707,-0.20069 1.71928,-0.99218 2.124835,-2.57323 4.744138,-8.88471 7.151331,-17.23187 1.802865,-6.2516 1.880017,-6.36902 5.525098,-8.40902 2.983563,-1.66978 4.007108,-2.62331 5.140026,-4.78844 1.137703,-2.17427 1.79689,-4.33266 2.824066,-9.24692 0.553077,-2.64603 1.062575,-4.30881 1.720755,-5.61579 1.125781,-2.23549 1.156086,-2.7236 0.207433,-3.34078 -1.752967,-1.14046 -3.987498,-3.61166 -4.698636,-5.19628 -0.623923,-1.39029 -0.706979,-1.86062 -0.597866,-3.38561 0.195207,-2.72827 2.188284,-6.61602 3.391738,-6.61602 0.405127,0 0.516019,0.18438 0.517178,0.8599 0.0024,1.42751 0.53825,2.90679 1.312177,3.62275 0.402336,0.3722 1.491748,0.96119 2.420912,1.30886 3.965919,1.48398 6.319602,2.90311 6.319602,3.81034 0,0.23347 0.948232,0.32402 3.505729,0.33479 2.994755,0.0126 4.045625,0.14088 7.209896,0.87998 9.414374,2.19899 10.360534,2.33917 13.009124,1.92741 3.32506,-0.51694 8.36243,-2.50287 10.58729,-4.17393 0.46323,-0.34793 0.9226,-0.6326 1.02081,-0.6326 0.36251,0 3.5698,-2.71028 4.71621,-3.98537 2.68428,-2.98555 2.95614,-4.86667 1.43153,-9.90526 -1.07261,-3.54478 -1.41951,-5.20165 -1.67065,-7.97916 -0.18626,-2.05993 -0.31712,-2.19564 -2.12494,-2.20368 -1.49844,-0.007 -3.55798,-0.80262 -4.66917,-1.80448 -1.72167,-1.5523 -3.69801,-6.45612 -2.91679,-7.23734 0.39592,-0.39592 5.45377,-0.37638 6.91382,0.0267 0.639,0.17641 1.6189,0.60929 2.17755,0.96196 1.07547,0.67891 1.68923,0.80227 1.94363,0.39064 0.1723,-0.27879 -2.69416,-1.68294 -4.34072,-2.12632 -0.58254,-0.15686 -2.83494,-0.51381 -5.00533,-0.79323 -3.99712,-0.51459 -6.61411,-1.20746 -8.16963,-2.16299 -1.39309,-0.85575 -3.2729,-3.174789 -4.93679,-6.090294 -0.85748,-1.502498 -1.63157,-2.791345 -1.72021,-2.864106 -0.0886,-0.07276 -0.31592,-0.429948 -0.50507,-0.79375 -0.18915,-0.363802 -1.36499,-2.090208 -2.61298,-3.836458 -3.0106,-4.212584 -3.886043,-6.069201 -3.992244,-8.466667 -0.109144,-2.463844 0.257651,-3.175 1.637564,-3.175 1.48193,0 1.96363,0.783202 2.39904,3.900685 0.19431,1.391187 0.5075,2.809062 0.69598,3.150832 0.32827,0.595242 0.35058,0.568645 0.53,-0.631829 0.62156,-4.158807 2.5663,-6.33453 4.20093,-4.699896 0.71059,0.710586 0.668,1.436657 -0.22788,3.884555 -0.41638,1.137713 -0.83994,2.522762 -0.94124,3.077886 -0.23569,1.291465 0.22314,3.506118 0.97083,4.685924 l 0.58262,0.919343 0.17261,-2.547069z"" />
                </svg>
                ";

        var svgParser = _services.GetRequiredService<ISvgParser>();
        var byteArray = Encoding.ASCII.GetBytes(pathSvg);
        using var stream = new MemoryStream(byteArray);
        using var streamReader = new StreamReader(stream);

        var list = svgParser.Parse(streamReader);
        Assert.IsNotNull(list);
        Assert.AreEqual(1, list.Count);

        var geometry = list[0];
        Assert.AreEqual(GeometricEntityType.Path, geometry.Type);

        var path = (PathEntity)geometry;
        Assert.AreEqual(140, path.Entities.Count);
    }

    [TestMethod]
    public void ParsePath3()
    {
        const string pathSvg = @"
                <svg height=""100"" width=""100"">
                    <path
                       id=""_141515936""
                       class=""fil102""
                       d=""M 237.64635,-231.36918 C 237.64635,-231.36918 237.66325,-231.36918 237.68005,-231.36918 L 237.64635,-231.36918 z""
                       style=""fill:#5d5c5b"" /><polygon
                       id=""_141558336""
                       class=""fil114""
                       points=""101.783,130.393 101.783,130.461 101.783,130.393 ""
                       style=""fill:#2f2a27""
                       transform=""translate(160.25645,-352.34518)"" /><polygon
                       id=""_141559792""
                       class=""fil115""
                       points=""76.9309,127.265 76.9309,127.214 76.9309,127.265 ""
                       style=""fill:#272727""
                       transform=""translate(160.25645,-352.34518)"" /><polygon
                       id=""_142063432""
                       class=""fil212""
                       points=""89.3398,110.725 89.3229,110.725 89.3398,110.725 ""
                       style=""fill:#332e2c""
                       transform=""translate(160.25645,-352.34518)"" /><polygon
                       id=""_142253976""
                       class=""fil114""
                       points=""101.783,130.461 101.783,130.393 101.783,130.461 ""
                       style=""fill:#2f2a27""
                       transform=""translate(160.25645,-352.34518)"" />				</svg>
                ";

        var svgParser = _services.GetRequiredService<ISvgParser>();
        var byteArray = Encoding.ASCII.GetBytes(pathSvg);
        using var stream = new MemoryStream(byteArray);
        using var streamReader = new StreamReader(stream);

        var list = svgParser.Parse(streamReader);
        Assert.IsNotNull(list);
        Assert.AreEqual(5, list.Count);

        var geometry = list[0];
        Assert.AreEqual(GeometricEntityType.Path, geometry.Type);

        var path = (PathEntity)geometry;
        Assert.AreEqual(3, path.Entities.Count);
        Assert.IsTrue(path.IsClosed);

        geometry = list[1];
        Assert.AreEqual(GeometricEntityType.Polygon, geometry.Type);
        var poly = (PolygonEntity)geometry;
        Assert.AreEqual(3, poly.Points.Count);

        geometry = list[2];
        Assert.AreEqual(GeometricEntityType.Polygon, geometry.Type);
        poly = (PolygonEntity)geometry;
        Assert.AreEqual(3, poly.Points.Count);

        geometry = list[3];
        Assert.AreEqual(GeometricEntityType.Polygon, geometry.Type);
        poly = (PolygonEntity)geometry;
        Assert.AreEqual(3, poly.Points.Count);
    }

    [TestMethod]
    public void ParsePath4()
    {
        const string pathSvg = @"
                <svg height=""100"" width=""100"">
                    <path class=""ChromeOther"" d=""M0,0 L0,1937 1,2621 2,2535 3,3007 4,3137 5,3558 6,3133 7,3260 8,3365 9,3789 10,3472 11,3398 12,3238 13,3032 14,3093 15,3218 16,3555 17,3319 18,3510 19,6141 20,2619 21,2578 22,2836 23,2856 24,2943 25,3138 26,3042 27,2605 28,2437 29,2667 30,2566 31,2727 32,2296 33,2427 34,2350 35,2351 36,2421 37,3388 38,3474 39,3242 40,3888 41,8005 42,2811 43,3027 44,3195 45,4718 46,3825 47,2445 48,2677 49,3071 50,2980 51,3036 52,2869 53,2424 54,2682 55,2836 56,2763 57,2747 58,2527 59,2376 60,2439 61,2746 62,2737 63,2569 64,2487 65,2471 66,2497 67,2396 68,2444 69,2863 70,2440 71,2033 72,2082 73,1958 74,1935 75,1921 76,1889 77,2421 78,2134 79,2052 80,2421 81,2395 82,2551 83,2462 84,3175 85,3918 86,2477 87,2546 88,2388 89,2776 90,2419 91,2283 92,1896 93,2007 94,2290 95,2408 96,2007 97,1932 98,1904 99,2031 100,2028 101,1981 102,2098 103,2265 104,2356 105,2041 106,1455 107,1976 108,2130 109,2314 110,2255 111,2005 112,2310 113,2180 114,2573 115,3419 116,2742 117,1846 118,1833 119,1858 120,1795 121,1848 122,1854 123,2092 124,2131 125,1746 126,2240 127,2349 128,2381 129,2358 130,1973 131,1794 132,2552 133,2705 134,1986 135,2412 136,1985 137,1905 138,1868 139,1779 140,1727 141,2041 142,1682 143,1884 144,1848 145,2134 146,2544 147,2426 148,2374 149,2309 150,2240 151,2581 152,2261 153,2268 154,3430 155,2173 156,2215 157,1895 158,2313 159,2677 160,2767 161,1133 162,1128 163,1104 164,1713 165,956 166,1220 167,279 168,286 169,211 170,232 171,302 172,294 173,462 174,501 175,560 176,643 177,656 177,0 Z"" fill=""rgb(100,100,100)""><title>Chrome/Other</title></path><path class=""Chrome20x"" d=""M0,0 L0,1937 1,2621 2,2535 3,3007 4,3137 5,3558 6,3132 7,3260 8,3365 9,3789 10,3472 11,3398 12,3238 13,3032 14,3092 15,3217 16,3555 17,3319 18,3510 19,6141 20,2619 21,2578 22,2836 23,2856 24,2943 25,3138 26,3042 27,2605 28,2437 29,2667 30,2566 31,2727 32,2296 33,2427 34,2350 35,2351 36,2421 37,3388 38,3474 39,3242 40,3888 41,8005 42,2811 43,3027 44,3195 45,4718 46,3825 47,2445 48,2677 49,3071 50,2980 51,3036 52,2869 53,2424 54,2682 55,2836 56,2763 57,2747 58,2527 59,2376 60,2439 61,2746 62,2737 63,2569 64,2487 65,2471 66,2497 67,2396 68,2444 69,2863 70,2440 71,2033 72,2082 73,1958 74,1935 75,1921 76,1889 77,2421 78,2134 79,2052 80,2421 81,2395 82,2551 83,2462 84,3175 85,3918 86,2477 87,2546 88,2388 89,2776 90,2419 91,2283 92,1896 93,2007 94,2290 95,2408 96,2007 97,1932 98,1904 99,2031 100,2028 101,1981 102,2098 103,2265 104,2356 105,2041 106,1455 107,1976 108,2130 109,2314 110,2255 111,2005 112,2310 113,2180 114,2573 115,3419 116,2742 117,1846 118,1833 119,1858 120,1795 121,1848 122,1854 123,2092 124,2131 125,1746 126,2240 127,2349 128,2381 129,2358 130,1973 131,1794 132,2552 133,2705 134,1986 135,2412 136,1985 137,1905 138,1868 139,1779 140,1727 141,2041 142,1682 143,1884 144,1848 145,2134 146,2544 147,2426 148,2374 149,2309 150,2240 151,2581 152,2261 153,2268 154,3430 155,2173 156,2215 157,1895 158,2313 159,2677 160,2767 161,1133 162,1128 163,1104 164,1713 165,956 166,1220 167,279 168,286 169,211 170,232 171,302 172,294 173,462 174,501 175,560 176,643 177,656 177,0 Z"" fill=""rgb(120,120,120)""><title>Chrome/2.0.x</title></path><path class=""Chrome10x"" d=""M0,0 L0,1936 1,2621 2,2535 3,3007 4,3137 5,3558 6,3132 7,3260 8,3365 9,3789 10,3472 11,3398 12,3238 13,3032 14,3092 15,3217 16,3555 17,3319 18,3510 19,6141 20,2619 21,2578 22,2836 23,2856 24,2943 25,3138 26,3042 27,2605 28,2437 29,2667 30,2566 31,2727 32,2296 33,2427 34,2350 35,2351 36,2421 37,3388 38,3474 39,3242 40,3888 41,8005 42,2811 43,3027 44,3195 45,4718 46,3825 47,2445 48,2677 49,3071 50,2980 51,3036 52,2869 53,2424 54,2682 55,2836 56,2763 57,2747 58,2527 59,2376 60,2439 61,2746 62,2737 63,2569 64,2487 65,2471 66,2497 67,2396 68,2444 69,2863 70,2440 71,2033 72,2082 73,1958 74,1935 75,1921 76,1889 77,2421 78,2134 79,2052 80,2421 81,2395 82,2551 83,2462 84,3175 85,3918 86,2477 87,2546 88,2388 89,2776 90,2419 91,2283 92,1896 93,2007 94,2290 95,2408 96,2007 97,1932 98,1904 99,2031 100,2028 101,1981 102,2098 103,2265 104,2356 105,2041 106,1455 107,1976 108,2130 109,2314 110,2255 111,2005 112,2310 113,2180 114,2573 115,3419 116,2742 117,1846 118,1833 119,1858 120,1795 121,1848 122,1854 123,2092 124,2131 125,1746 126,2240 127,2349 128,2381 129,2358 130,1973 131,1794 132,2552 133,2705 134,1986 135,2412 136,1985 137,1905 138,1868 139,1779 140,1727 141,2041 142,1682 143,1884 144,1848 145,2134 146,2544 147,2426 148,2374 149,2309 150,2240 151,2581 152,2261 153,2268 154,3430 155,2173 156,2215 157,1895 158,2313 159,2677 160,2767 161,1133 162,1128 163,1104 164,1713 165,956 166,1220 167,279 168,286 169,211 170,232 171,302 172,294 173,462 174,501 175,560 176,643 177,656 177,0 Z"" fill=""rgb(140,140,140)""><title>Chrome/1.0.x</title></path><path class=""Chrome05"" d=""M0,0 L0,1907 1,2583 2,2494 3,2974 4,3128 5,3558 6,3132 7,3260 8,3365 9,3789 10,3472 11,3398 12,3238 13,3032 14,3092 15,3217 16,3555 17,3319 18,3510 19,6141 20,2619 21,2578 22,2836 23,2856 24,2943 25,3138 26,3042 27,2605 28,2437 29,2667 30,2566 31,2727 32,2296 33,2427 34,2350 35,2351 36,2421 37,3388 38,3474 39,3242 40,3888 41,8005 42,2811 43,3027 44,3195 45,4718 46,3825 47,2445 48,2677 49,3071 50,2980 51,3036 52,2869 53,2424 54,2682 55,2836 56,2763 57,2747 58,2527 59,2376 60,2439 61,2746 62,2737 63,2569 64,2487 65,2471 66,2497 67,2396 68,2444 69,2863 70,2440 71,2033 72,2082 73,1958 74,1935 75,1921 76,1889 77,2421 78,2134 79,2052 80,2421 81,2395 82,2551 83,2462 84,3175 85,3918 86,2477 87,2546 88,2388 89,2776 90,2419 91,2283 92,1896 93,2007 94,2290 95,2408 96,2007 97,1932 98,1904 99,2031 100,2028 101,1981 102,2098 103,2265 104,2356 105,2041 106,1455 107,1976 108,2130 109,2314 110,2255 111,2005 112,2310 113,2180 114,2573 115,3419 116,2742 117,1846 118,1833 119,1858 120,1795 121,1848 122,1854 123,2092 124,2131 125,1746 126,2240 127,2349 128,2381 129,2358 130,1973 131,1794 132,2552 133,2705 134,1986 135,2412 136,1985 137,1905 138,1868 139,1779 140,1727 141,2041 142,1682 143,1884 144,1848 145,2134 146,2544 147,2426 148,2374 149,2309 150,2240 151,2581 152,2261 153,2268 154,3430 155,2173 156,2215 157,1895 158,2313 159,2677 160,2767 161,1133 162,1128 163,1104 164,1713 165,956 166,1220 167,279 168,286 169,211 170,232 171,302 172,294 173,462 174,501 175,560 176,643 177,656 177,0 Z"" fill=""rgb(160,160,160)""><title>Chrome/0.5-</title></path><path class=""IEOther"" d=""M0,0 L0,1906 1,2581 2,2492 3,2964 4,3103 5,3509 6,3105 7,3239 8,3312 9,3740 10,3428 11,3385 12,3216 13,3002 14,3052 15,3173 16,3474 17,3242 18,3331 19,6141 20,2619 21,2578 22,2836 23,2856 24,2943 25,3138 26,3042 27,2605 28,2437 29,2667 30,2566 31,2727 32,2296 33,2427 34,2350 35,2351 36,2421 37,3388 38,3474 39,3242 40,3888 41,8005 42,2811 43,3027 44,3195 45,4718 46,3825 47,2445 48,2677 49,3071 50,2980 51,3036 52,2869 53,2424 54,2682 55,2836 56,2763 57,2747 58,2527 59,2376 60,2439 61,2746 62,2737 63,2569 64,2487 65,2471 66,2497 67,2396 68,2444 69,2863 70,2440 71,2033 72,2082 73,1958 74,1935 75,1921 76,1889 77,2421 78,2134 79,2052 80,2421 81,2395 82,2551 83,2462 84,3175 85,3918 86,2477 87,2546 88,2388 89,2776 90,2419 91,2283 92,1896 93,2007 94,2290 95,2408 96,2007 97,1932 98,1904 99,2031 100,2028 101,1981 102,2098 103,2265 104,2356 105,2041 106,1455 107,1976 108,2130 109,2314 110,2255 111,2005 112,2310 113,2180 114,2573 115,3419 116,2742 117,1846 118,1833 119,1858 120,1795 121,1848 122,1854 123,2092 124,2131 125,1746 126,2240 127,2349 128,2381 129,2358 130,1973 131,1794 132,2552 133,2705 134,1986 135,2412 136,1985 137,1905 138,1868 139,1779 140,1727 141,2041 142,1682 143,1884 144,1848 145,2134 146,2544 147,2426 148,2374 149,2309 150,2240 151,2581 152,2261 153,2268 154,3430 155,2173 156,2215 157,1895 158,2313 159,2677 160,2767 161,1133 162,1128 163,1104 164,1713 165,956 166,1220 167,279 168,286 169,211 170,232 171,302 172,294 173,462 174,501 175,560 176,643 177,656 177,0 Z"" fill=""rgb(0,60,100)""><title>IE/Other</title></path><path class=""IE80x"" d=""M0,0 L0,1900 1,2575 2,2483 3,2956 4,3094 5,3496 6,3095 7,3220 8,3305 9,3728 10,3416 11,3377 12,3201 13,2997 14,3043 15,3165 16,3462 17,3232 18,3326 19,6133 20,2611 21,2570 22,2832 23,2850 24,2940 25,3134 26,3037 27,2602 28,2426 29,2653 30,2561 31,2722 32,2293 33,2421 34,2346 35,2341 36,2408 37,3377 38,3459 39,3222 40,3871 41,7993 42,2799 43,3014 44,3178 45,4702 46,3817 47,2440 48,2665 49,3064 50,2974 51,3035 52,2852 53,2419 54,2678 55,2825 56,2757 57,2735 58,2516 59,2367 60,2429 61,2741 62,2730 63,2562 64,2479 65,2467 66,2491 67,2393 68,2440 69,2857 70,2429 71,2021 72,2074 73,1949 74,1929 75,1916 76,1881 77,2417 78,2131 79,2045 80,2414 81,2392 82,2546 83,2451 84,3165 85,3912 86,2472 87,2542 88,2385 89,2770 90,2415 91,2280 92,1894 93,2001 94,2287 95,2406 96,2005 97,1925 98,1901 99,2026 100,2021 101,1978 102,2095 103,2255 104,2354 105,2029 106,1454 107,1973 108,2127 109,2307 110,2249 111,2001 112,2302 113,2171 114,2561 115,3410 116,2736 117,1841 118,1826 119,1854 120,1792 121,1843 122,1848 123,2081 124,2117 125,1729 126,2221 127,2344 128,2373 129,2350 130,1968 131,1793 132,2540 133,2700 134,1978 135,2403 136,1979 137,1900 138,1861 139,1773 140,1725 141,2028 142,1675 143,1869 144,1838 145,2127 146,2541 147,2423 148,2370 149,2305 150,2240 151,2575 152,2233 153,2222 154,3378 155,2092 156,2150 157,1854 158,2287 159,2603 160,2695 161,1133 162,1127 163,1104 164,1711 165,956 166,1220 167,279 168,285 169,211 170,231 171,302 172,294 173,461 174,501 175,557 176,643 177,656 177,0 Z"" fill=""rgb(0,79,131)""><title>IE/8.0x</title></path><path class=""IE70x"" d=""M0,0 L0,1897 1,2568 2,2473 3,2945 4,3081 5,3485 6,3085 7,3213 8,3296 9,3718 10,3404 11,3363 12,3194 13,2986 14,3037 15,3161 16,3454 17,3223 18,3321 19,6111 20,2610 21,2570 22,2831 23,2850 24,2939 25,3134 26,3037 27,2600 28,2425 29,2651 30,2559 31,2721 32,2293 33,2420 34,2344 35,2340 36,2406 37,3375 38,3454 39,3218 40,3868 41,7978 42,2792 43,3004 44,3159 45,4701 46,3817 47,2440 48,2665 49,3064 50,2974 51,3035 52,2852 53,2419 54,2678 55,2825 56,2757 57,2735 58,2516 59,2367 60,2429 61,2741 62,2730 63,2562 64,2479 65,2467 66,2491 67,2393 68,2440 69,2857 70,2429 71,2021 72,2074 73,1949 74,1929 75,1916 76,1881 77,2417 78,2131 79,2045 80,2414 81,2392 82,2546 83,2451 84,3165 85,3912 86,2472 87,2542 88,2385 89,2770 90,2415 91,2280 92,1894 93,2001 94,2287 95,2406 96,2005 97,1925 98,1901 99,2026 100,2021 101,1978 102,2095 103,2255 104,2354 105,2029 106,1454 107,1973 108,2127 109,2307 110,2249 111,2001 112,2302 113,2171 114,2561 115,3410 116,2736 117,1841 118,1826 119,1854 120,1792 121,1843 122,1848 123,2081 124,2117 125,1729 126,2221 127,2344 128,2373 129,2350 130,1968 131,1793 132,2540 133,2700 134,1978 135,2403 136,1979 137,1900 138,1861 139,1773 140,1725 141,2028 142,1675 143,1869 144,1838 145,2127 146,2541 147,2423 148,2370 149,2305 150,2240 151,2575 152,2233 153,2222 154,3378 155,2092 156,2150 157,1854 158,2287 159,2603 160,2695 161,1133 162,1127 163,1104 164,1711 165,956 166,1220 167,279 168,285 169,211 170,231 171,302 172,294 173,461 174,501 175,557 176,643 177,656 177,0 Z"" fill=""rgb(0,90,151)""><title>IE/7.0x</title></path><path class=""IE60x"" d=""M0,0 L0,1705 1,2336 2,2243 3,2691 4,2813 5,3149 6,2828 7,2886 8,2982 9,3403 10,3091 11,3053 12,2909 13,2682 14,2757 15,2853 16,3151 17,2907 18,3018 19,5773 20,2366 21,2252 22,2426 23,2418 24,2557 25,2765 26,2580 27,2226 28,1952 29,2137 30,2033 31,2130 32,1781 33,1914 34,1882 35,1888 36,1967 37,2917 38,2960 39,2715 40,3329 41,7322 42,2365 43,2563 44,2690 45,4149 46,3262 47,1888 48,2072 49,2404 50,2378 51,2407 52,2271 53,1919 54,2140 55,2154 56,2121 57,2165 58,1986 59,1877 60,1867 61,2132 62,2113 63,1943 64,1883 65,1826 66,1889 67,1796 68,1923 69,2476 70,2097 71,1681 72,1721 73,1616 74,1621 75,1626 76,1614 77,2031 78,1820 79,1737 80,2111 81,2085 82,2216 83,2146 84,2625 85,3268 86,2129 87,2158 88,1999 89,2413 90,2079 91,1956 92,1629 93,1730 94,2003 95,2148 96,1752 97,1659 98,1647 99,1786 100,1767 101,1756 102,1862 103,1987 104,2115 105,1838 106,1302 107,1770 108,1963 109,2078 110,2025 111,1810 112,2111 113,2045 114,2454 115,3290 116,2648 117,1781 118,1776 119,1792 120,1750 121,1792 122,1806 123,2030 124,2072 125,1690 126,2160 127,2297 128,2325 129,2298 130,1920 131,1750 132,2503 133,2636 134,1933 135,2359 136,1943 137,1870 138,1843 139,1744 140,1700 141,2017 142,1662 143,1842 144,1824 145,2114 146,2518 147,2404 148,2345 149,2289 150,2217 151,2555 152,2224 153,2213 154,3374 155,2090 156,2145 157,1853 158,2284 159,2602 160,2693 161,1131 162,1126 163,1102 164,1708 165,953 166,1219 167,279 168,285 169,209 170,231 171,301 172,294 173,461 174,501 175,557 176,642 177,655 177,0 Z"" fill=""rgb(0,115,191)""><title>IE/6.0x</title></path><path class=""IE55"" d=""M0,0 L0,1360 1,1822 2,1789 3,2185 4,2206 5,2598 6,2293 7,2391 8,2339 9,2575 10,2338 11,2192 12,2282 13,2154 14,2181 15,2292 16,2534 17,2373 18,2442 19,5125 20,1874 21,1817 22,1838 23,1841 24,1859 25,2052 26,1760 27,1300 28,1184 29,1372 30,1178 31,1261 32,981 33,1252 34,1181 35,1236 36,1281 37,2071 38,2144 39,1941 40,2505 41,6249 42,1342 43,1455 44,1681 45,3175 46,2627 47,1272 48,1369 49,1550 50,1643 51,1535 52,1373 53,1123 54,1614 55,1367 56,1328 57,1409 58,1299 59,1223 60,1166 61,1262 62,1198 63,1134 64,1146 65,1041 66,1037 67,1089 68,1179 69,1647 70,1403 71,1192 72,1178 73,1063 74,1071 75,1049 76,986 77,1348 78,1012 79,958 80,1199 81,1211 82,1196 83,1063 84,1429 85,1943 86,1202 87,1279 88,1247 89,1633 90,1333 91,1192 92,993 93,1001 94,1063 95,1230 96,956 97,900 98,888 99,971 100,906 101,931 102,929 103,1053 104,1031 105,902 106,652 107,1029 108,985 109,1066 110,1055 111,863 112,1011 113,1131 114,1229 115,1741 116,1317 117,794 118,817 119,816 120,813 121,852 122,778 123,883 124,886 125,718 126,1044 127,1115 128,1041 129,1025 130,880 131,842 132,920 133,1049 134,892 135,1075 136,923 137,920 138,899 139,840 140,840 141,945 142,797 143,899 144,814 145,1077 146,1410 147,1308 148,1304 149,1356 150,1378 151,1617 152,1228 153,1258 154,2198 155,1052 156,1186 157,1086 158,1181 159,1493 160,1476 161,651 162,600 163,554 164,1112 165,554 166,795 167,56 168,64 169,63 170,64 171,96 172,110 173,214 174,246.99999999999997 175,297 176,313 177,328 177,0 Z"" fill=""rgb(0,133,221)""><title>IE/5.5-</title></path><path class=""FirefoxOther"" d=""M0,0 L0,650 1,742 2,793 3,1060 4,1065 5,1294 6,924 7,1044 8,998 9,1301 10,1080 11,1033 12,1093 13,1040 14,1056 15,1078 16,1221 17,1287 18,1461 19,4220 20,1009 21,918 22,1018 23,1022 24,1096 25,1205 26,1327 27,1239 28,1112 29,1319 30,1123 31,1200 32,934 33,1231 34,1154 35,1216 36,1201 37,1881 38,1938 39,1721 40,2281 41,6072 42,1232 43,1320 44,1585 45,3078 46,2531 47,1201 48,1290 49,1469 50,1574 51,1478 52,1317 53,1066 54,1561 55,1308 56,1281 57,1346 58,1233 59,1174 60,1117 61,1212 62,1171 63,1109 64,1118 65,1018 66,1001 67,1048 68,1134 69,1604 70,1370 71,1170 72,1167 73,1051 74,1062 75,1039 76,955 77,1296 78,970 79,927 80,1164 81,1180 82,1155 83,1029 84,1384 85,1905 86,1176 87,1257 88,1222 89,1608 90,1304 91,1171 92,972 93,977 94,1043 95,1212 96,931 97,869 98,871 99,950 100,886 101,896 102,904 103,1003 104,1007 105,872 106,632 107,1001 108,958 109,1048 110,1035 111,849 112,998 113,1034 114,992 115,1162 116,873 117,768 118,717 119,735 120,791 121,822 122,746 123,858 124,858 125,696 126,836 127,939 128,894 129,829 130,858 131,794 132,896 133,1018 134,859 135,1040 136,862 137,893 138,869 139,814 140,804 141,898 142,764 143,870 144,799 145,994 146,985 147,915 148,933 149,983 150,1030 151,1254 152,891 153,900 154,1766 155,698 156,727 157,680 158,816 159,995.9999999999999 160,954 161,303 162,263 163,254 164,823 165,332 166,618 167,49 168,58 169,54 170,58 171,89 172,101 173,204 174,239.99999999999997 175,286 176,300 177,312 177,0 Z"" fill=""rgb(232,100,0)""><title>Firefox/Other</title></path><path class=""Firefox31x"" d=""M0,0 L0,639 1,723 2,776 3,1045 4,1044 5,1276 6,909 7,1030 8,979 9,1291 10,1070 11,1021 12,1084 13,1011 14,1039 15,1068 16,1183 17,1265 18,1440 19,4203 20,1006 21,905 22,1007 23,1018 24,1090 25,1176 26,1304 27,1228 28,1107 29,1313 30,1100 31,1188 32,908 33,1214 34,1151 35,1205 36,1193 37,1873 38,1923 39,1708 40,2257 41,6058 42,1222 43,1306 44,1570 45,3070 46,2516 47,1195 48,1284 49,1461 50,1566 51,1469 52,1311 53,1057 54,1554 55,1296 56,1271 57,1338 58,1231 59,1165 60,1106 61,1211 62,1162 63,1105 64,1116 65,1014 66,999 67,1043 68,1131 69,1598 70,1364 71,1164 72,1162 73,1049 74,1058 75,1036 76,953 77,1292 78,968 79,921 80,1159 81,1171 82,1154 83,1024 84,1379 85,1902 86,1169 87,1253 88,1220 89,1600 90,1301 91,1167 92,970 93,969 94,1036 95,1208 96,924 97,861 98,866 99,943 100,883 101,893 102,901 103,1002 104,1001 105,866 106,631 107,1000 108,954 109,1047 110,1032 111,847 112,994 113,1030 114,990 115,1153 116,873 117,767 118,710 119,724 120,789 121,820 122,746 123,855 124,858 125,695 126,834 127,936 128,892 129,827 130,854 131,792 132,894 133,1017 134,855 135,1038 136,860 137,892 138,867 139,813 140,801 141,895 142,763 143,867 144,793 145,989 146,980 147,910 148,931 149,981 150,1024 151,1251 152,886 153,883 154,1759 155,694 156,726 157,674 158,815 159,988.9999999999999 160,946 161,297 162,256 163,250 164,815 165,330 166,614 167,49 168,55 169,49 170,58 171,89 172,100 173,203 174,235.99999999999997 175,286 176,294 177,309 177,0 Z"" fill=""rgb(244,120,0)""><title>Firefox/3.1.x</title></path><path class=""Firefox30x"" d=""M0,0 L0,624 1,709 2,758 3,1026 4,1015 5,1252 6,881 7,1007 8,943 9,1244 10,1031 11,1000 12,1038 13,987 14,1011 15,1051 16,1164 17,1239 18,1418 19,4155 20,983 21,882 22,974 23,991 24,1064 25,1155 26,1284 27,1220 28,1091 29,1289 30,1084 31,1183 32,907 33,1214 34,1151 35,1205 36,1193 37,1873 38,1923 39,1708 40,2257 41,6058 42,1222 43,1306 44,1570 45,3070 46,2516 47,1195 48,1284 49,1461 50,1566 51,1469 52,1311 53,1057 54,1554 55,1296 56,1271 57,1338 58,1231 59,1165 60,1106 61,1211 62,1162 63,1105 64,1116 65,1014 66,999 67,1043 68,1131 69,1598 70,1364 71,1164 72,1162 73,1049 74,1058 75,1036 76,953 77,1292 78,968 79,921 80,1159 81,1171 82,1154 83,1024 84,1379 85,1902 86,1169 87,1253 88,1220 89,1600 90,1301 91,1167 92,970 93,969 94,1036 95,1208 96,924 97,861 98,866 99,943 100,883 101,893 102,901 103,1002 104,1001 105,866 106,631 107,1000 108,954 109,1047 110,1032 111,847 112,994 113,1030 114,990 115,1153 116,873 117,767 118,710 119,724 120,789 121,820 122,746 123,855 124,858 125,695 126,834 127,936 128,892 129,827 130,854 131,792 132,894 133,1017 134,855 135,1038 136,860 137,892 138,867 139,813 140,801 141,895 142,763 143,867 144,793 145,989 146,980 147,910 148,931 149,981 150,1024 151,1251 152,886 153,883 154,1759 155,694 156,726 157,674 158,815 159,988.9999999999999 160,946 161,297 162,256 163,250 164,815 165,330 166,614 167,49 168,55 169,49 170,58 171,89 172,100 173,203 174,235.99999999999997 175,286 176,294 177,309 177,0 Z"" fill=""rgb(255,140,20)""><title>Firefox/3.0.x</title></path><path class=""Firefox20x"" d=""M0,0 L0,211 1,245 2,269 3,360 4,372 5,494 6,311 7,365 8,376 9,468 10,373 11,374 12,421 13,379 14,344 15,402 16,473 17,512 18,594 19,1337 20,521 21,419 22,470 23,478 24,520 25,562 26,675 27,730 28,625 29,775 30,852 31,918 32,705 33,925 34,941 35,1001 36,991 37,1651 38,1587 39,1356 40,1847 41,4878 42,1065 43,1135 44,1389 45,2821 46,2248 47,1107 48,1216 49,1389 50,1505 51,1406 52,1260 53,1008 54,1479 55,1218 56,1238 57,1294 58,1195 59,1129 60,1087 61,1199 62,1139 63,1088 64,1096 65,996 66,974 67,1020 68,1113 69,1572 70,1346 71,1143 72,1132 73,1025 74,1038 75,1016 76,911 77,1278 78,953 79,905 80,1138 81,1160 82,1132 83,1015 84,1369 85,1882 86,1153 87,1232 88,1209 89,1569 90,1284 91,1159 92,965 93,953 94,1025 95,1204 96,922 97,856 98,856 99,941 100,881 101,889 102,895 103,992 104,994 105,857 106,628 107,994 108,935 109,1041 110,1024 111,845 112,993 113,1023 114,986 115,1150 116,869 117,762 118,710 119,721 120,787 121,818 122,743 123,848 124,856 125,693 126,831 127,933 128,890 129,823 130,847 131,790 132,890 133,1003 134,847 135,1023 136,853 137,885 138,852 139,812 140,796 141,892 142,760 143,867 144,793 145,989 146,980 147,910 148,931 149,981 150,1024 151,1251 152,886 153,883 154,1759 155,694 156,726 157,674 158,815 159,988.9999999999999 160,946 161,297 162,256 163,250 164,815 165,330 166,614 167,49 168,55 169,49 170,58 171,89 172,100 173,203 174,235.99999999999997 175,286 176,294 177,309 177,0 Z"" fill=""rgb(255,160,50)""><title>Firefox/2.0.x</title></path><path class=""Firefox15"" d=""M0,0 L0,145 1,157 2,177 3,284 4,281 5,374 6,213 7,238 8,248 9,324 10,237 11,228 12,274 13,231 14,222 15,264 16,310 17,326 18,366 19,797 20,258 21,191 22,172 23,193 24,216 25,235 26,356 27,391 28,300 29,338 30,298 31,353 32,216 33,310 34,351 35,347 36,347 37,751 38,642 39,444 40,629 41,1901 42,318 43,311 44,453 45,973 46,732 47,232 48,278 49,321 50,377 51,283 52,309 53,252 54,203 55,306 56,289 57,310 58,254 59,282 60,249 61,278 62,257 63,232 64,226 65,237 66,234 67,268 68,268 69,661 70,538 71,237 72,250 73,206 74,208 75,216 76,184 77,501 78,234 79,226 80,376 81,411 82,357 83,282 84,382 85,620 86,342 87,388 88,308 89,642 90,494 91,348 92,319 93,318 94,351 95,451 96,350 97,307 98,323 99,360 100,346 101,343 102,365 103,419 104,407 105,412 106,300 107,434 108,449 109,515 110,538 111,472 112,575 113,577 114,663 115,851 116,774 117,713 118,665 119,675 120,756 121,779 122,716 123,819 124,832 125,680 126,811 127,911 128,865 129,791 130,824 131,782 132,881 133,996 134,830 135,1006 136,833 137,865 138,835 139,805 140,789 141,886 142,747 143,854 144,775 145,962 146,963 147,898 148,924 149,962 150,1014 151,1239 152,877 153,879 154,1751 155,690 156,720 157,666 158,809 159,981.9999999999999 160,936 161,294 162,255 163,250 164,803 165,319 166,599 167,49 168,52 169,47 170,52 171,81 172,97 173,195 174,230.99999999999997 175,280 176,294 177,309 177,0 Z"" fill=""rgb(255,180,85)""><title>Firefox/1.5-</title></path><path class=""SafariOther"" d=""M0,0 L0,136 1,152 2,165 3,272 4,265 5,361 6,204 7,224 8,229 9,298 10,217 11,217 12,264 13,210 14,207 15,246 16,274 17,285 18,351 19,768 20,219 21,164 22,147 23,170 24,196 25,205 26,302 27,270 28,248 29,296 30,258 31,258 32,180 33,229 34,275 35,320 36,266 37,596 38,495 39,360 40,568 41,1798 42,269 43,256 44,357 45,829 46,624 47,206 48,234 49,266 50,293 51,223 52,247 53,221 54,166 55,250 56,242 57,254 58,219 59,233 60,209 61,240 62,221 63,188 64,175 65,204 66,190 67,216 68,228 69,625 70,473 71,186 72,202 73,155 74,166 75,151 76,134 77,431 78,184 79,156 80,277 81,284 82,254 83,146 84,198 85,313 86,153 87,220 88,164 89,443 90,305 91,173.00000000000003 92,140 93,153 94,163 95,223 96,144 97,126 98,135 99,146 100,135 101,147 102,165 103,154 104,142 105,133 106,97 107,169 108,171 109,155 110,154 111,127 112,141 113,143 114,139 115,129 116,116 117,121 118,109 119,106 120,119 121,110 122,115 123,115 124,142 125,108 126,134 127,145 128,119 129,125 130,146 131,123 132,143 133,173 134,110 135,145 136,149 137,129 138,134 139,143 140,123 141,156 142,134 143,171 144,134 145,184 146,152 147,163 148,174 149,230 150,271 151,445 152,162 153,176 154,329 155,127 156,142 157,154 158,175 159,162 160,168 161,78 162,100 163,97 164,195 165,159 166,482 167,23 168,23 169,21 170,25 171,27 172,22 173,33 174,30 175,31 176,46 177,37 177,0 Z"" fill=""rgb(0,160,240)""><title>Safari/Other</title></path><path class=""Safari40x"" d=""M0,0 L0,95 1,112 2,124 3,205 4,205 5,298 6,157 7,165 8,208 9,287 10,214 11,210 12,256 13,205 14,198 15,234 16,269 17,279 18,348 19,760 20,214 21,160 22,147 23,170 24,196 25,202 26,298 27,263 28,244 29,293 30,255 31,255 32,178 33,228 34,273 35,317 36,258 37,585 38,487 39,354 40,557 41,1787 42,268 43,252 44,353 45,822 46,620 47,205 48,232 49,265 50,293 51,223 52,247 53,220 54,165 55,248 56,240 57,252 58,216 59,232 60,207 61,240 62,221 63,188 64,174 65,204 66,190 67,213 68,228 69,625 70,473 71,185 72,199 73,155 74,166 75,151 76,134 77,428 78,183 79,156 80,277 81,284 82,253 83,146 84,195 85,313 86,153 87,220 88,164 89,443 90,305 91,172.00000000000003 92,140 93,151 94,163 95,222 96,144 97,125 98,135 99,145 100,133 101,146 102,165 103,154 104,140 105,133 106,90 107,163 108,166 109,154 110,154 111,123 112,135 113,143 114,132 115,129 116,110 117,116 118,103 119,106 120,119 121,110 122,102 123,114 124,132 125,107 126,134 127,140 128,108 129,124 130,146 131,121 132,143 133,172 134,110 135,145 136,148 137,128 138,133 139,143 140,123 141,156 142,134 143,171 144,134 145,183 146,151 147,163 148,174 149,230 150,271 151,445 152,162 153,176 154,329 155,126 156,142 157,154 158,175 159,162 160,168 161,78 162,100 163,97 164,195 165,159 166,482 167,23 168,23 169,21 170,25 171,27 172,22 173,33 174,30 175,31 176,46 177,37 177,0 Z"" fill=""rgb(20,170,241)""><title>Safari/4.0.x</title></path><path class=""Safari31x"" d=""M0,0 L0,91 1,107 2,121 3,202 4,204 5,294 6,150 7,165 8,205 9,281 10,209 11,209 12,244 13,201 14,192 15,219 16,264 17,268 18,333 19,716 20,202 21,153 22,143 23,164 24,192 25,193 26,290 27,258 28,238 29,274 30,242 31,255 32,178 33,228 34,273 35,317 36,258 37,585 38,487 39,354 40,557 41,1787 42,268 43,252 44,353 45,822 46,620 47,205 48,232 49,265 50,293 51,223 52,247 53,220 54,165 55,248 56,240 57,252 58,216 59,232 60,207 61,240 62,221 63,188 64,174 65,204 66,190 67,213 68,228 69,625 70,473 71,185 72,199 73,155 74,166 75,151 76,134 77,428 78,183 79,156 80,277 81,284 82,253 83,146 84,195 85,313 86,153 87,220 88,164 89,443 90,305 91,172.00000000000003 92,140 93,151 94,163 95,222 96,144 97,125 98,135 99,145 100,133 101,146 102,165 103,154 104,140 105,133 106,90 107,163 108,166 109,154 110,154 111,123 112,135 113,143 114,132 115,129 116,110 117,116 118,103 119,106 120,119 121,110 122,102 123,114 124,132 125,107 126,134 127,140 128,108 129,124 130,146 131,121 132,143 133,172 134,110 135,145 136,148 137,128 138,133 139,143 140,123 141,156 142,134 143,171 144,134 145,183 146,151 147,163 148,174 149,230 150,271 151,445 152,162 153,176 154,329 155,126 156,142 157,154 158,175 159,162 160,168 161,78 162,100 163,97 164,195 165,159 166,482 167,23 168,23 169,21 170,25 171,27 172,22 173,33 174,30 175,31 176,46 177,37 177,0 Z"" fill=""rgb(50,190,243)""><title>Safari/3.1.x</title></path><path class=""Safari30x"" d=""M0,0 L0,81 1,99 2,110 3,185 4,182 5,266 6,118 7,135 8,133 9,180 10,127 11,136 12,162 13,117 14,123 15,137 16,177 17,198 18,245 19,329 20,126 21,88 22,89 23,99 24,101 25,115 26,215 27,182 28,145 29,178 30,149 31,151 32,101 33,130 34,174 35,134 36,132 37,241 38,328 39,216 40,379 41,977 42,210 43,249 44,342 45,820 46,612 47,203 48,230 49,265 50,293 51,223 52,247 53,220 54,165 55,248 56,240 57,252 58,216 59,232 60,207 61,240 62,221 63,188 64,174 65,204 66,190 67,213 68,228 69,625 70,473 71,185 72,199 73,155 74,166 75,151 76,134 77,428 78,183 79,156 80,277 81,284 82,253 83,146 84,195 85,313 86,153 87,220 88,164 89,443 90,305 91,172.00000000000003 92,140 93,151 94,163 95,222 96,144 97,125 98,135 99,145 100,133 101,146 102,165 103,154 104,140 105,133 106,90 107,163 108,166 109,154 110,154 111,123 112,135 113,143 114,132 115,129 116,110 117,116 118,103 119,106 120,119 121,110 122,102 123,114 124,132 125,107 126,134 127,140 128,108 129,124 130,146 131,121 132,143 133,172 134,110 135,145 136,148 137,128 138,133 139,143 140,123 141,156 142,134 143,171 144,134 145,183 146,151 147,163 148,174 149,230 150,271 151,445 152,162 153,176 154,329 155,126 156,142 157,154 158,175 159,162 160,168 161,78 162,100 163,97 164,195 165,159 166,482 167,23 168,23 169,21 170,25 171,27 172,22 173,33 174,30 175,31 176,46 177,37 177,0 Z"" fill=""rgb(90,202,245)""><title>Safari/3.0.x</title></path><path class=""Safari20"" d=""M0,0 L0,77 1,97 2,106 3,184 4,180 5,265 6,117 7,133 8,131 9,174 10,125 11,132 12,161 13,113 14,121 15,133 16,175 17,194 18,243 19,321 20,125 21,86 22,87 23,94 24,99 25,109 26,209 27,176 28,142 29,173 30,140 31,146 32,92 33,124 34,162 35,118 36,123 37,212 38,307 39,201 40,349 41,879 42,158 43,151 44,209 45,620 46,346 47,118 48,132 49,159 50,165 51,145 52,135 53,157 54,99 55,134 56,162 57,157 58,139 59,155 60,164 61,198 62,182 63,171 64,154 65,187 66,164 67,191 68,202 69,596 70,448 71,164 72,172 73,130 74,139 75,126 76,113 77,408 78,161 79,127 80,241 81,235 82,199 83,146 84,195 85,310 86,153 87,218 88,164 89,434 90,304 91,172.00000000000003 92,139 93,151 94,163 95,222 96,144 97,125 98,135 99,145 100,133 101,146 102,165 103,154 104,140 105,133 106,90 107,163 108,166 109,154 110,154 111,123 112,135 113,143 114,132 115,129 116,110 117,116 118,103 119,106 120,119 121,110 122,102 123,114 124,132 125,107 126,134 127,140 128,108 129,124 130,146 131,121 132,143 133,172 134,110 135,145 136,148 137,128 138,133 139,143 140,123 141,156 142,134 143,171 144,134 145,183 146,151 147,163 148,174 149,230 150,271 151,445 152,162 153,176 154,329 155,126 156,142 157,154 158,175 159,162 160,168 161,78 162,100 163,97 164,195 165,159 166,482 167,23 168,23 169,21 170,25 171,27 172,22 173,33 174,30 175,31 176,46 177,37 177,0 Z"" fill=""rgb(150,224,255)""><title>Safari/2.0-</title></path><path class=""OperaOther"" d=""M0,0 L0,77 1,96 2,105 3,182 4,179 5,263 6,116 7,131 8,129 9,173 10,123 11,132 12,152 13,109 14,116 15,131 16,173 17,192 18,240 19,315 20,122 21,85 22,82 23,94 24,95 25,105 26,207 27,173 28,138 29,168 30,135 31,141 32,83 33,112 34,154 35,110 36,116 37,197 38,293 39,188 40,335 41,844 42,152 43,141 44,190 45,588 46,319 47,108 48,124 49,139 50,146 51,129 52,113 53,140 54,83 55,117 56,131 57,131 58,109 59,126 60,124 61,159 62,130 63,129 64,109 65,152 66,112 67,148 68,160 69,543 70,401 71,114 72,128 73,81 74,105 75,90 76,76 77,194 78,99 79,95 80,188 81,183 82,142 83,85 84,105 85,140 86,104 87,151 88,103 89,337 90,252 91,117.00000000000001 92,90 93,108 94,122.99999999999999 95,159 96,102 97,81 98,98 99,101 100,98 101,86 102,112 103,101 104,105 105,93 106,62 107,136 108,122.99999999999999 109,115 110,106 111,73 112,85 113,105 114,86 115,87 116,73 117,79 118,65 119,71 120,69 121,74 122,77 123,71 124,83 125,77 126,79 127,99 128,67 129,77 130,106 131,73 132,91 133,114 134,61 135,95 136,102 137,80 138,83 139,93 140,96 141,105 142,81 143,103 144,90 145,122 146,98 147,116 148,135 149,158 150,199 151,386 152,91 153,118 154,127 155,78 156,89 157,89 158,87 159,109 160,121 161,60 162,82 163,72 164,167 165,145 166,459 167,16 168,17 169,11 170,17 171,19 172,13 173,19 174,21 175,22 176,34 177,28.000000000000004 177,0 Z"" fill=""rgb(135,20,23)""><title>Opera/Other</title></path><path class=""Opera100"" d=""M0,0 L0,77 1,96 2,105 3,182 4,179 5,263 6,116 7,131 8,129 9,172 10,123 11,131 12,150 13,108 14,116 15,130 16,173 17,191 18,239 19,313 20,115 21,83 22,81 23,92 24,93 25,103 26,205 27,171 28,137 29,168 30,135 31,140 32,82 33,112 34,153 35,109 36,113 37,194 38,290 39,188 40,332 41,839 42,151 43,141 44,189 45,587 46,318 47,106 48,123 49,138 50,146 51,127 52,112 53,138 54,82 55,115 56,131 57,130 58,107 59,124 60,122 61,159 62,129 63,127 64,109 65,152 66,111 67,148 68,160 69,543 70,399 71,114 72,128 73,79 74,105 75,90 76,76 77,194 78,99 79,94 80,188 81,183 82,142 83,84 84,105 85,139 86,104 87,151 88,103 89,336 90,251 91,117.00000000000001 92,87 93,108 94,122.99999999999999 95,157 96,102 97,81 98,98 99,101 100,98 101,86 102,112 103,101 104,105 105,92 106,62 107,135 108,122.99999999999999 109,115 110,106 111,73 112,85 113,105 114,86 115,87 116,73 117,79 118,65 119,69 120,69 121,74 122,77 123,71 124,83 125,77 126,79 127,99 128,67 129,76 130,106 131,73 132,91 133,114 134,60 135,94 136,101 137,80 138,83 139,93 140,96 141,105 142,81 143,103 144,90 145,122 146,96 147,110 148,129 149,152 150,196 151,384 152,89 153,117 154,127 155,77 156,88 157,87 158,87 159,107 160,120 161,60 162,82 163,71 164,167 165,143 166,458 167,14.000000000000002 168,17 169,11 170,17 171,18 172,12 173,19 174,18 175,20 176,31 177,28.000000000000004 177,0 Z"" fill=""rgb(165,20,25)""><title>Opera/10.0</title></path><path class=""Opera95x"" d=""M0,0 L0,61 1,76 2,90 3,151 4,137 5,207 6,114 7,131 8,129 9,169 10,123 11,131 12,150 13,108 14,116 15,130 16,173 17,191 18,239 19,313 20,115 21,83 22,81 23,92 24,93 25,103 26,205 27,171 28,137 29,168 30,135 31,140 32,82 33,112 34,153 35,109 36,113 37,194 38,290 39,188 40,332 41,839 42,151 43,141 44,189 45,587 46,318 47,106 48,123 49,138 50,146 51,127 52,112 53,138 54,82 55,115 56,131 57,130 58,107 59,124 60,122 61,159 62,129 63,127 64,109 65,152 66,111 67,148 68,160 69,543 70,399 71,114 72,128 73,79 74,105 75,90 76,76 77,194 78,99 79,94 80,188 81,183 82,142 83,84 84,105 85,139 86,104 87,151 88,103 89,336 90,251 91,117.00000000000001 92,87 93,108 94,122.99999999999999 95,157 96,102 97,81 98,98 99,101 100,98 101,86 102,112 103,101 104,105 105,92 106,62 107,135 108,122.99999999999999 109,115 110,106 111,73 112,85 113,105 114,86 115,87 116,73 117,79 118,65 119,69 120,69 121,74 122,77 123,71 124,83 125,77 126,79 127,99 128,67 129,76 130,106 131,73 132,91 133,114 134,60 135,94 136,101 137,80 138,83 139,93 140,96 141,105 142,81 143,103 144,90 145,122 146,96 147,110 148,129 149,152 150,196 151,384 152,89 153,117 154,127 155,77 156,88 157,87 158,87 159,107 160,120 161,60 162,82 163,71 164,167 165,143 166,458 167,14.000000000000002 168,17 169,11 170,17 171,18 172,12 173,19 174,18 175,20 176,31 177,28.000000000000004 177,0 Z"" fill=""rgb(195,23,25)""><title>Opera/9.5x</title></path><path class=""Opera90x"" d=""M0,0 L0,26 1,33 2,34 3,52 4,31 5,39 6,25 7,33 8,33 9,49 10,39 11,46 12,49 13,36 14,25 15,42 16,48 17,36 18,45 19,48 20,27 21,27 22,25 23,22 24,40 25,28 26,30 27,30 28,33 29,40 30,48 31,92 32,46 33,72 34,90 35,69 36,77 37,109 38,173 39,122.00000000000001 40,191 41,444 42,87 43,95 44,122.99999999999999 45,410 46,194 47,79 48,94 49,99 50,113 51,100 52,79 53,90 54,62 55,80 56,104 57,100 58,74 59,88 60,86 61,92 62,84 63,89 64,83 65,118.00000000000001 66,87 67,113 68,125 69,316 70,207 71,113 72,128 73,79 74,104 75,89 76,76 77,194 78,99 79,94 80,187 81,181 82,142 83,84 84,105 85,139 86,104 87,151 88,103 89,334 90,251 91,117.00000000000001 92,87 93,108 94,122.99999999999999 95,157 96,102 97,81 98,98 99,101 100,98 101,86 102,112 103,101 104,105 105,92 106,62 107,135 108,122.99999999999999 109,115 110,106 111,73 112,85 113,105 114,86 115,87 116,73 117,79 118,65 119,69 120,69 121,74 122,77 123,71 124,83 125,77 126,79 127,99 128,67 129,76 130,106 131,73 132,91 133,114 134,60 135,94 136,101 137,80 138,83 139,93 140,96 141,105 142,81 143,103 144,90 145,122 146,96 147,110 148,129 149,152 150,196 151,384 152,89 153,117 154,127 155,77 156,88 157,87 158,87 159,107 160,120 161,60 162,82 163,71 164,167 165,143 166,458 167,14.000000000000002 168,17 169,11 170,17 171,18 172,12 173,19 174,18 175,20 176,31 177,28.000000000000004 177,0 Z"" fill=""rgb(225,45,48)""><title>Opera/9.0x</title></path><path class=""Opera85"" d=""M0,0 L0,7.000000000000001 1,10 2,19 3,18 4,15 5,17 6,11 7,14.000000000000002 8,12 9,15 10,14.000000000000002 11,20 12,12 13,14.000000000000002 14,12 15,17 16,15 17,9 18,9 19,12 20,11 21,9 22,11 23,9 24,8 25,9 26,9 27,7.000000000000001 28,9 29,10 30,9 31,8 32,9 33,11 34,10 35,9 36,11 37,14.000000000000002 38,16 39,12 40,15 41,18 42,13 43,8 44,9 45,12 46,8 47,5 48,8 49,13 50,12 51,13 52,6 53,10 54,16 55,14.000000000000002 56,12 57,12 58,10 59,15 60,9 61,8 62,9 63,7.000000000000001 64,12 65,7.000000000000001 66,9 67,8 68,17 69,20 70,8 71,8 72,11 73,8 74,7.000000000000001 75,11 76,6 77,124 78,31 79,6 80,15 81,12 82,10 83,13 84,12 85,12 86,15 87,19 88,9 89,22 90,9 91,7.000000000000001 92,13 93,14.000000000000002 94,10 95,10 96,16 97,7.000000000000001 98,15 99,8 100,10 101,12 102,11 103,11 104,10 105,10 106,11 107,12 108,10 109,9 110,10 111,7.000000000000001 112,7.000000000000001 113,10 114,9 115,12 116,10 117,7.000000000000001 118,9 119,9 120,9 121,9 122,15 123,12 124,16 125,15 126,19 127,26 128,13 129,17 130,22 131,22 132,23 133,31 134,27 135,57.99999999999999 136,49 137,38 138,42 139,51 140,59 141,61 142,48 143,59 144,46 145,77 146,56.99999999999999 147,73 148,76 149,61 150,94 151,139 152,63 153,89 154,100 155,52 156,62 157,53 158,63 159,81 160,89 161,41 162,56.00000000000001 163,53 164,108 165,81 166,216 167,14.000000000000002 168,17 169,11 170,17 171,18 172,12 173,19 174,18 175,20 176,31 177,28.000000000000004 177,0 Z"" fill=""rgb(255,68,72)""><title>Opera/8.5-</title></path>
                </svg>
                ";

        var svgParser = _services.GetRequiredService<ISvgParser>();
        var byteArray = Encoding.ASCII.GetBytes(pathSvg);
        using var stream = new MemoryStream(byteArray);
        using var streamReader = new StreamReader(stream);

        var list = svgParser.Parse(streamReader);
        Assert.IsNotNull(list);
        Assert.AreEqual(24, list.Count);

        var geometry = list[0];
        Assert.AreEqual(GeometricEntityType.Path, geometry.Type);

        var path = (PathEntity)geometry;
        Assert.AreEqual(180, path.Entities.Count);
        Assert.IsTrue(path.IsClosed);
    }

    [TestMethod]
    public void TestDoublePairs()
    {
        string value1, value2;

        const string testValue1 = "+123,456";
        (value1, value2) = SvgPathParser.GetSplitValue(testValue1);
        Assert.AreEqual("+123", value1);
        Assert.AreEqual("456", value2);

        const string testValue2 = "-123-456";
        (value1, value2) = SvgPathParser.GetSplitValue(testValue2);
        Assert.AreEqual("-123", value1);
        Assert.AreEqual("-456", value2);

        const string testValue3 = "+123+456";
        (value1, value2) = SvgPathParser.GetSplitValue(testValue3);
        Assert.AreEqual("+123", value1);
        Assert.AreEqual("+456", value2);

        const string testValue4 = "123,-456";
        (value1, value2) = SvgPathParser.GetSplitValue(testValue4);
        Assert.AreEqual("123", value1);
        Assert.AreEqual("-456", value2);

        const string testValue5 = "123,+456";
        (value1, value2) = SvgPathParser.GetSplitValue(testValue5);
        Assert.AreEqual("123", value1);
        Assert.AreEqual("+456", value2);

        const string testValue6 = "123.054E-5+23";
        (value1, value2) = SvgPathParser.GetSplitValue(testValue6);
        Assert.AreEqual("123.054E-5", value1);
        Assert.AreEqual("+23", value2);
    }

    [TestMethod]
    public void ParseArcPath1()
    {
        const string pathSvg = @"
                <svg height=""100"" width=""100"">
                    <path
                       d=""M 995.9277 476.82574 A 446.66661860238554 154.99472636156321 1.23 1 0  104.09415,476.82574 A 446.04079626679186 154.99472636156321 0.5 0 1  995.9277 476.82574 z""
                       transform=""matrix(0.983314,-0.181917,0.181917,0.983314,-175.1608,6.48194)"" />				
				</svg>
                ";

        var svgParser = _services.GetRequiredService<ISvgParser>();
        var byteArray = Encoding.ASCII.GetBytes(pathSvg);
        using var stream = new MemoryStream(byteArray);
        using var streamReader = new StreamReader(stream);

        var list = svgParser.Parse(streamReader);
        Assert.IsNotNull(list);
        Assert.AreEqual(1, list.Count);

        var geometry = list[0];
        Assert.AreEqual(GeometricEntityType.Path, geometry.Type);

        var path = (PathEntity)geometry;
        Assert.AreEqual(3, path.Entities.Count);
        Assert.IsTrue(path.IsClosed);

        geometry = path.Entities[0];
        Assert.AreEqual(GeometricEntityType.Arc, geometry.Type);
        var arc = (ArcEntity)geometry;
        AssertEx.WithinTolerance(550.00903619421524, arc.Location.X);
        AssertEx.WithinTolerance(476.81365115555968, arc.Location.Y);
        AssertEx.WithinTolerance(356.46379423047722, arc.StartAngle);
        AssertEx.WithinTolerance(-180.00892258740043, arc.SweepAngle);
        AssertEx.WithinTolerance(446.66661860238554, arc.Radii.X);
        AssertEx.WithinTolerance(154.99472636156321, arc.Radii.Y);

        geometry = path.Entities[1];
        Assert.AreEqual(GeometricEntityType.Arc, geometry.Type);
        arc = (ArcEntity)geometry;
        AssertEx.WithinTolerance(550.02419413444466, arc.Location.X);
        AssertEx.WithinTolerance(477.03468404135208, arc.Location.Y);
        AssertEx.WithinTolerance(178.63859309096193, arc.StartAngle);
        AssertEx.WithinTolerance(179.84556510017802, arc.SweepAngle);
        AssertEx.WithinTolerance(446.04079626679186, arc.Radii.X);
        AssertEx.WithinTolerance(154.99472636156321, arc.Radii.Y);

        geometry = path.Entities[2];
        Assert.AreEqual(GeometricEntityType.Line, geometry.Type);
        var line = (LineEntity)geometry;
        AssertEx.WithinTolerance(995.9277, line.Location.X);
        AssertEx.WithinTolerance(476.82574, line.Location.Y);
        AssertEx.WithinTolerance(995.9277, line.EndLocation.X);
        AssertEx.WithinTolerance(476.82574, line.EndLocation.Y);
    }

    [TestMethod]
    public void ParseArcPath2()
    {
        const string pathSvg = @"
                <svg height=""100"" width=""100"" xmlns:sodipodi=""http://sodipodi.sourceforge.net/DTD/sodipodi-0.dtd"">
                    <path
                       d=""M -5 -4 a -9 -8 -3 0 0 -1 -2 a -6 -7 0 1 1 1 2"" />
				</svg>
                ";

        var svgParser = _services.GetRequiredService<ISvgParser>();
        var byteArray = Encoding.ASCII.GetBytes(pathSvg);
        using var stream = new MemoryStream(byteArray);
        using var streamReader = new StreamReader(stream);

        var list = svgParser.Parse(streamReader);
        Assert.IsNotNull(list);
        Assert.AreEqual(1, list.Count);

        var geometry = list[0];
        Assert.AreEqual(GeometricEntityType.Path, geometry.Type);

        var path = (PathEntity)geometry;
        Assert.AreEqual(2, path.Entities.Count);
        Assert.IsFalse(path.IsClosed);

        geometry = path.Entities[0];
        Assert.AreEqual(GeometricEntityType.Arc, geometry.Type);
        var arc = (ArcEntity)geometry;
        AssertEx.WithinTolerance(-13.650525978360967, arc.Location.X);
        AssertEx.WithinTolerance(-1.7044728895243222, arc.Location.Y);
        AssertEx.WithinTolerance(346.705521489089, arc.StartAngle);
        AssertEx.WithinTolerance(-15.793715794919688, arc.SweepAngle);
        AssertEx.WithinTolerance(9, arc.Radii.X);
        AssertEx.WithinTolerance(8, arc.Radii.Y);

        geometry = path.Entities[1];
        Assert.AreEqual(GeometricEntityType.Arc, geometry.Type);
        arc = (ArcEntity)geometry;
        AssertEx.WithinTolerance(-0.38869784209620573, arc.Location.X);
        AssertEx.WithinTolerance(-8.4785250796845268, arc.Location.Y);
        AssertEx.WithinTolerance(159.26323538204574, arc.StartAngle);
        AssertEx.WithinTolerance(340.96065490885, arc.SweepAngle);
        AssertEx.WithinTolerance(6, arc.Radii.X);
        AssertEx.WithinTolerance(7, arc.Radii.Y);
    }

    [TestMethod]
    public void ParseArcPath3()
    {
        const string pathSvg = @"
                    <svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 100 100"">
	                    <path d=""M10,50 A25,25 -30 0,1 60,25z""/>
                    </svg>
                ";

        var svgParser = _services.GetRequiredService<ISvgParser>();
        var byteArray = Encoding.ASCII.GetBytes(pathSvg);
        using var stream = new MemoryStream(byteArray);
        using var streamReader = new StreamReader(stream);

        var list = svgParser.Parse(streamReader);
        Assert.IsNotNull(list);
        Assert.AreEqual(1, list.Count);

        var geometry = list[0];
        Assert.AreEqual(GeometricEntityType.Path, geometry.Type);

        var path = (PathEntity)geometry;
        Assert.AreEqual(2, path.Entities.Count);
        Assert.IsTrue(path.IsClosed);

        geometry = path.Entities[0];
        Assert.AreEqual(GeometricEntityType.Arc, geometry.Type);
        var arc = (ArcEntity)geometry;
        AssertEx.WithinTolerance(35.000000172633492, arc.Location.X);
        AssertEx.WithinTolerance(37.500000345266983, arc.Location.Y);
        AssertEx.WithinTolerance(183.43494961421553, arc.StartAngle);
        AssertEx.WithinTolerance(179.99999829245272, arc.SweepAngle);
        AssertEx.WithinTolerance(27.950849718747371, arc.Radii.X);
        AssertEx.WithinTolerance(27.950849718747371, arc.Radii.Y);

        geometry = path.Entities[1];
        Assert.AreEqual(GeometricEntityType.Line, geometry.Type);
        var line = (LineEntity)geometry;
        AssertEx.WithinTolerance(60, line.Location.X);
        AssertEx.WithinTolerance(25, line.Location.Y);
        AssertEx.WithinTolerance(10, line.EndLocation.X);
        AssertEx.WithinTolerance(50, line.EndLocation.Y);
    }

    [TestMethod]
    public void ParseArcPath4()
    {
        // Path format:
        // M - start point
        //   - radix
        //   - start angle
        //   - is large arc
        //   - is increasng sweep (increasing in angle)
        //   - end point
        var testPaths = new[]
        {                
            /* 
            /*  Start at 0 deg
             */

            // Small arc - cw
            new ArcTest(@"<path d=""M  10,  0 A 10,10   0 0 1   0, 10"" />",
            new ArcEntity(new PointDouble(  0,   0), new PointDouble(10,  0), new PointDouble( 0,  10), new PointDouble(10, 10), 0, 90, 0, new GeometryTransform())),

            // Large arc - cw  
            new ArcTest(@"<path d=""M  10,  0 A 10,10   0 1 1   0, 10"" />",
            new ArcEntity(new PointDouble( 10,  10), new PointDouble(10,  0), new PointDouble( 0,  10), new PointDouble(10, 10), 270, 270, 0, new GeometryTransform())),

            // Small arc - ccw 
            new ArcTest(@"<path d=""M  10,  0 A 10,10   0 0 0   0, 10"" />",
            new ArcEntity(new PointDouble( 10,  10), new PointDouble(10,  0), new PointDouble( 0,  10), new PointDouble(10, 10), 270, -90, 0, new GeometryTransform())),

            // Large arc - ccw
            new ArcTest(@"<path d=""M  10,  0 A 10,10   0 1 0   0, 10"" />",
            new ArcEntity(new PointDouble(  0,   0), new PointDouble(10,  0), new PointDouble( 0,  10), new PointDouble(10, 10), 0, -270, 0, new GeometryTransform())),
               
            /* 
            /*  Start at 90 deg
             */ 

            // Small arc - cw
            new ArcTest(@"<path d=""M  10,  0 A 10,10  90 0 1  20, 10"" />",
            new ArcEntity(new PointDouble( 10,  10), new PointDouble(10,  0), new PointDouble(20, 10), new PointDouble(10, 10),   180,  90, 90, new GeometryTransform())),

            // Large arc - cw  
            new ArcTest(@"<path d=""M  10,  0 A 10,10  90 1 1  20, 10"" />",
            new ArcEntity(new PointDouble( 20,   0), new PointDouble(10,  0), new PointDouble(20, 10),  new PointDouble(10, 10),  90,  270, 90, new GeometryTransform())),

            // Small arc - ccw 
            new ArcTest(@"<path d=""M  10,  0 A 10,10  90 0 0  20, 10"" />",
            new ArcEntity(new PointDouble( 20,   0), new PointDouble(10,  0), new PointDouble(20, 10),  new PointDouble(10, 10),  90,  -90, 90, new GeometryTransform())),

            // Large arc - ccw
            new ArcTest(@"<path d=""M  10,  0 A 10,10  90 1 0  20, 10"" />",
            new ArcEntity(new PointDouble( 10,  10), new PointDouble(10,  0), new PointDouble(20, 10), new PointDouble(10, 10), 180,  -270, 90, new GeometryTransform())),
                                                     
            /* 
            /* Start at 180 deg
             */

            // Small arc - cw
            new ArcTest(@"<path d=""M  10,  0 A 10,10 180 0 1  20,-10"" />",
            new ArcEntity(new PointDouble( 20,   0), new PointDouble(10,  0), new PointDouble(20, -10),   new PointDouble(10, 10),   0, 90, 180, new GeometryTransform())),

            // Large arc - cw  
            new ArcTest(@"<path d=""M  10,  0 A 10,10 180 1 1  20,-10"" />",
            new ArcEntity(new PointDouble( 10, -10), new PointDouble(10,  0), new PointDouble(20, -10), new PointDouble(10, 10),  270, 270, 180, new GeometryTransform())),

             // Small arc - ccw
            new ArcTest(@"<path d=""M  10,  0 A 10,10 180 0 0  20,-10"" />",
            new ArcEntity(new PointDouble( 10, -10), new PointDouble(10,  0), new PointDouble(20, -10), new PointDouble(10, 10),  270, -90, 180, new GeometryTransform())),

            // Large arc - ccw
            new ArcTest(@"<path d=""M  10,  0 A 10,10 180 1 0  20,-10"" />",
            new ArcEntity(new PointDouble( 20,   0), new PointDouble(10,  0), new PointDouble(20, -10),   new PointDouble(10, 10), 0, -270, 180, new GeometryTransform())),
                                                     
            /* 
             * Start at 270 deg
             */

            // Small arc - cw
            new ArcTest(@"<path d=""M  10,  0 A 10,10 270 0 1  20,-10"" />",
            new ArcEntity(new PointDouble( 20,   0), new PointDouble(10,  0), new PointDouble(20, -10), new PointDouble(10, 10),   270, 90, 270, new GeometryTransform())),

            // Large arc - cw
            new ArcTest(@"<path d=""M  10,  0 A 10,10 270 1 1  20,-10"" />",
            new ArcEntity(new PointDouble( 10, -10), new PointDouble(10,  0), new PointDouble(20, -10), new PointDouble(10, 10),  180, 270, 270, new GeometryTransform())),

            // Small arc - ccw 
            new ArcTest(@"<path d=""M  10,  0 A 10,10 270 0 0  20,-10"" />",
            new ArcEntity(new PointDouble( 10, -10), new PointDouble(10,  0), new PointDouble(20, -10), new PointDouble(10, 10),  180, -90, 270, new GeometryTransform())),

            // Large arc - ccw
            new ArcTest(@"<path d=""M  10,  0 A 10,10 270 1 0  20,-10"" />",
            new ArcEntity(new PointDouble( 20,   0), new PointDouble(10,  0), new PointDouble(20, -10), new PointDouble(10, 10), 270, -270, 270, new GeometryTransform()))
        };

        const string pathSvg = @"<svg>{0}</svg>";

        for (var i = 0; i < testPaths.Length; i++)
        {
            var testPath = testPaths[i].SvgTestPath;
            var expectantArc = testPaths[i].ExpectedArc;

            var svgParser = _services.GetRequiredService<ISvgParser>();

            var xmlString = string.Format(pathSvg, testPath);

            var byteArray = Encoding.ASCII.GetBytes(xmlString);
            using var stream = new MemoryStream(byteArray);
            using var streamReader = new StreamReader(stream);

            var list = svgParser.Parse(streamReader);
            Assert.IsNotNull(list);
            Assert.AreEqual(1, list.Count);

            var geometry = list[0];
            Assert.AreEqual(GeometricEntityType.Path, geometry.Type);

            var path = (PathEntity)geometry;
            Assert.AreEqual(1, path.Entities.Count);
            Assert.IsFalse(path.IsClosed);

            geometry = path.Entities[0];
            Assert.AreEqual(GeometricEntityType.Arc, geometry.Type);
            var arc = (ArcEntity)geometry;
            AssertEx.WithinTolerance(expectantArc.Location.X, arc.Location.X);
            AssertEx.WithinTolerance(expectantArc.Location.Y, arc.Location.Y);
            AssertEx.WithinTolerance(expectantArc.StartAngle, arc.StartAngle);
            AssertEx.WithinTolerance(expectantArc.SweepAngle, arc.SweepAngle);
            AssertEx.WithinTolerance(expectantArc.Radii.X, arc.Radii.X);
            AssertEx.WithinTolerance(expectantArc.Radii.Y, arc.Radii.Y);
            AssertEx.WithinTolerance(expectantArc.StartLocation.X, arc.StartLocation.X);
            AssertEx.WithinTolerance(expectantArc.StartLocation.Y, arc.StartLocation.Y);
            AssertEx.WithinTolerance(expectantArc.EndLocation.X, arc.EndLocation.X);
            AssertEx.WithinTolerance(expectantArc.EndLocation.Y, arc.EndLocation.Y);

            System.Diagnostics.Trace.WriteLine($"Completed test for {i}");
        }
    }

    private class ArcTest
    {
        public ArcTest(string path, ArcEntity expected)
        {
            SvgTestPath = path;
            ExpectedArc = expected;
        }

        public string SvgTestPath { get; }

        public ArcEntity ExpectedArc { get; }
    }
}
