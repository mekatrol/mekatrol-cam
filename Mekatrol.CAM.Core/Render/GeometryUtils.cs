using Avalonia.Media;
using Mekatrol.CAM.Core.Geometry;
using Mekatrol.CAM.Core.Geometry.Entities;
using SkiaSharp;
using System.Runtime.CompilerServices;

namespace Mekatrol.CAM.Core.Render;

public static class GeometryUtils
{
    const double DegreesToRadiansFactor = Math.PI / 180.0;
    const double RadiansToDegreesFactor = 180.0 / Math.PI;
    const double PI2 = 2.0 * Math.PI;

    public static PointDouble GetReflectedPoint(PointDouble pointToReflect, PointDouble reflectionLinePoint1, PointDouble reflectionLinePoint2)
    {
        // Special case: Vertical line (reflect -ve twice the distance to the line line X)
        if (NearEqual(reflectionLinePoint1.X, reflectionLinePoint2.X))
        {
            // Line is vertical, so return point with negated X
            return new PointDouble(-pointToReflect.X + 2 * reflectionLinePoint1.X, pointToReflect.Y);
        }

        // Special case: Horizontal line (reflect -ve twice the distance to the line line Y)
        if (NearEqual(reflectionLinePoint1.Y, reflectionLinePoint2.Y))
        {
            // Line is vertical, so return point with negated Y
            return new PointDouble(pointToReflect.X, -pointToReflect.Y + 2 * reflectionLinePoint1.Y);
        }

        // Working with the line y = mx + x
        // Where m is the slope (gradient) and c is the Y intercept
        var m = (reflectionLinePoint2.Y - reflectionLinePoint1.Y) / (reflectionLinePoint2.X - reflectionLinePoint1.X);
        var c = (reflectionLinePoint2.X * reflectionLinePoint1.Y - reflectionLinePoint1.X * reflectionLinePoint2.Y) / (reflectionLinePoint2.X - reflectionLinePoint1.X);

        // Calculate the distance to the reflecting line
        var d = (pointToReflect.X + (pointToReflect.Y - c) * m) / (1 + m * m);

        // Calculate the reflected point
        var x4 = 2 * d - pointToReflect.X;
        var y4 = 2 * d * m - pointToReflect.Y + 2 * c;

        // Return the reflected point
        return new PointDouble(x4, y4);
    }

    public static PointDouble GetReflectedPoint(PointDouble pointToReflect, PointDouble reflectionPoint)
    {
        // The reflected point is the 'point to reflect' rotated 180°
        // about the 'reflection point'.
        var reflect =
            Matrix3.CreateTranslate(-reflectionPoint) * // Translate so that we rotate about origin
            Matrix3.CreateRotation(Math.PI) *           // Rotate 180°
            Matrix3.CreateTranslate(reflectionPoint);   // Translate relative back relative to reflectionPoint

        return pointToReflect * reflect;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double DegreesToRadians(double degrees)
    {
        return degrees * DegreesToRadiansFactor;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double RadiansToDegrees(double radians)
    {
        return radians * RadiansToDegreesFactor;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double NormaliseAngleRadians(double a)
    {
        a %= Math.PI * 2;
        if (a < 0)
        {
            a += Math.PI * 2;
        }

        return a;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double NormaliseAngleDegrees(double a)
    {
        a %= 360.0;
        if (a < 0)
        {
            a += 360;
        }

        return a;
    }

    public static bool NearZero(double value, double tolerance = GeometryConstants.Tolerance)
    {
        return Math.Abs(value) <= tolerance;
    }

    public static bool NearEqual(double value1, double value2, double tolerance = GeometryConstants.Tolerance)
    {
        return Math.Abs(value1 - value2) <= tolerance;
    }

    /// <summary>
    /// SEE: https://www.w3.org/TR/SVG11/implnote.html#ArcConversionEndpointToCenter
    /// F.6 Elliptical arc implementation notes
    /// An elliptical arc, as represented in the SVG path command, is described by the following parameters in order:
    /// </summary>
    /// <param name="x1">(x1, y1) are the absolute coordinates of the start point of the arc.</param>
    /// <param name="y1">(x1, y1) are the absolute coordinates of the start point of the arc</param>
    /// <param name="x2">(x2, y2) are the absolute coordinates of the end point of the arc.</param>
    /// <param name="y2">(x2, y2) are the absolute coordinates of the end point of the arc.</param>
    /// <param name="rx">(rx and ry) are the radii of the ellipse(also known as its semi - major and semi - minor axes).</param>
    /// <param name="ry">(rx and ry) are the radii of the ellipse(also known as its semi - major and semi - minor axes).</param>
    /// <param name="ellipseRotation">φ is the angle from the x-axis of the current coordinate system to the x-axis of the ellipse.</param>
    /// <param name="isLargeArc">the large arc flag, and is false if an arc spanning less than or equal to 180 degrees is chosen, or true if an arc spanning greater than 180 degrees is chosen.</param>
    /// <param name="isClockwise">the sweep flag, and is false if the line joining center to arc sweeps through decreasing angles, or true if it sweeps through increasing angles.</param>
    /// <returns>The generated arc OR null if arc not well defined OR a line if rx and ry are zero</returns>
    public static IGeometricEntity? GenerateArc(
        double x1, double y1,   // The start point X and Y
        double x2, double y2,   // The end point X and Y
        double rx, double ry,   // The elliptical arc radii X and Y
        double ellipseRotation, // The ellipse rotation (the arc sits on this ellipse's boundary)
        bool isLargeArc,        // the points are connected by a large arc (fA in SVG doco)
        bool isClockwise)       // the arc sweeps clockwise (fS in SVG doco)
    {
        // Get modulus so that we only sweep within 360 deg
        // φ is taken mod 360 degrees.
        ellipseRotation %= 360;

        // Convert angle from degrees to radians
        var phi = DegreesToRadians(ellipseRotation);

        if (NearZero(x2 - x1) &&
            NearZero(y2 - y1))
        {
            // F.6.2 Out-of-range parameters
            // If the endpoints (x1, y1) and (x2, y2) are identical, then this is equivalent to omitting the elliptical arc segment entirely.
            return null;
        }

        if (NearZero(rx) || NearZero(ry))
        {
            // F.6.6 Correction of out-of-range radii
            // Step 1: Ensure radii are non-zero
            // If rx = 0 or ry = 0, then treat this as a straight line from (x1, y1) to (x2, y2) and stop.
            return new LineEntity(x1, y1, x2, y2, new GeometryTransform());
        }

        // F.6.6 Correction of out-of-range radii
        // If rx or ry have negative signs, these are dropped; the absolute value is used instead.
        // Step 2: Ensure radii are positive
        rx = Math.Abs(rx);
        ry = Math.Abs(ry);

        // Cosine of phi
        var cosPhi = Math.Cos(phi);

        // Sin of phi
        var sinPhi = Math.Sin(phi);

        // Delta X halfed
        var halfDx = (x1 - x2) / 2.0;

        // Delta Y halfed
        var halfDy = (y1 - y2) / 2.0;

        // F.6.5 Conversion from endpoint to center parameterization
        // Equation F.6.5.1
        // Step 1: Compute (x1′, y1′)

        // X1 prime
        var x1p = cosPhi * halfDx + sinPhi * halfDy;

        // Y1 prime
        var y1p = cosPhi * halfDy - sinPhi * halfDx;

        // X1 prime squared
        var x1p2 = x1p * x1p;

        // Y1 prime squared
        var y1p2 = y1p * y1p;

        // X radii squared
        var rx2 = rx * rx;

        // Y radii squared
        var ry2 = ry * ry;

        // F.6.6 Correction of out-of-range radii
        // Step 3: Ensure radii are large enough
        // Using the primed coordinate values of equation (F.6.5.1), compute
        var lambda = x1p2 / rx2 + y1p2 / ry2;  // Equation F.6.6.2

        // Equation F.6.6.3
        // If the result of the above equation is less than or equal to 1,
        // then no further change need be made to rx and ry.
        // If the result of the above equation is greater than 1, then make the replacements
        if (lambda > 1.0)
        {
            var lambdaSquareroot = Math.Sqrt(lambda);
            rx *= lambdaSquareroot;
            ry *= lambdaSquareroot;

            // Update squared values
            rx2 = rx * rx;
            ry2 = ry * ry;
        }

        // F.6.6 Correction of out-of-range radii
        // Step 4: Proceed with computations

        // F.6.5 Conversion from endpoint to center parameterization
        // Equation F.6.5.2 - variables
        // Step 2: Compute (cx′, cy′)

        // Center X prime
        double cxp;

        // Center Y prime
        double cyp;

        // Center X
        double cx;

        // Center Y
        double cy;

        // X,Y average (used in equation F6.5.2)
        var xAv = (x1 + x2) / 2.0;
        var yAv = (y1 + y2) / 2.0;

        // Equation F.6.5.2 denominator used in sqrt
        var denominator = rx2 * y1p2 + ry2 * x1p2;

        // Equation F.6.5.2 numerator used in sqrt
        // The denominator is used in the form: rx2 * ry2 - (rx2 * y1p2 + ry2 * x1p2)
        var numerator = rx2 * ry2 - denominator;

        // If the denominator is near zero then there is no solution
        if (NearZero(denominator))
        {
            return null;
        }

        // Equation F.6.5.2 
        // Step 2: Compute (cx′, cy′)
        // Calculate the squareroot result and
        // get absolute value for simpler inversion in next operation
        var c = Math.Sqrt(Math.Abs(numerator / denominator));

        // Equation F.6.5.2 
        // Step 2: Compute (cx′, cy′)
        // where the + sign is chosen if fA ≠ fS, and the − sign is chosen if fA = fS.
        if (isLargeArc == isClockwise) { c = -c; }

        // Equation F.6.5.2 
        // Step 2: Compute (cx′, cy′)
        // Calculate center prime values
        cxp = c * (rx * y1p) / ry;
        cyp = -c * (ry * x1p) / rx;

        // Equation F.6.5.3 
        // Step 3: Compute (cx, cy) from (cx′, cy′)
        cx = cosPhi * cxp - sinPhi * cyp + xAv;
        cy = sinPhi * cxp + cosPhi * cyp + yAv;

        // Equation F.6.5.4
        // Calculate components
        var xcr1 = (x1p - cxp) / rx;
        var xcr2 = (x1p + cxp) / rx;
        var ycr1 = (y1p - cyp) / ry;
        var ycr2 = (y1p + cyp) / ry;

        // Equation F.6.5.5
        // Calculate start angle
        var startAngle = AngleBetween(1.0, 0.0, xcr1, ycr1);

        // We only want positive start angles
        while (startAngle < 0) { startAngle += PI2; }

        // Equation F.6.5.6
        // Calculate delta (sweep) angle
        var sweepAngle = AngleBetween(xcr1, ycr1, -xcr2, -ycr2);
        while (sweepAngle > PI2) { sweepAngle -= PI2; }
        while (sweepAngle < 0.0) { sweepAngle += PI2; }

        // Equation F.6.5.6
        // where θ1 is fixed in the range −360° < Δθ < 360° such that:
        //  if fS = 0, then Δθ < 0,
        //  else if fS = 1, then Δθ > 0.
        if (isClockwise == false) { sweepAngle -= PI2; }

        // Convert radians to degrees
        startAngle = RadiansToDegrees(startAngle);
        sweepAngle = RadiansToDegrees(sweepAngle);

        // Limit sweep to first rotation
        sweepAngle %= 360.0;

        // Create and return the arc based on the calculated parameters
        var arc = new ArcEntity(
            new PointDouble(cx, cy),
            new PointDouble(x1, y1), // Center
            new PointDouble(x2, y2), // Start
            new PointDouble(rx, ry), // End
            startAngle, // Radii
            sweepAngle,
            ellipseRotation,
            new GeometryTransform());

        return arc;
    }

    /// <summary>
    /// Calculate the angle between two vectors.
    /// See:
    /// https://www.w3.org/TR/SVG11/implnote.html#ArcConversionEndpointToCenter
    /// F.6.5 Conversion from endpoint to center parameterization
    /// Step 4: Compute θ1 and Δθ
    /// In general, the angle between two vectors (ux, uy) and (vx, vy) can be computed as
    /// </summary>
    /// <param name="ux">Vector u x component</param>
    /// <param name="uy">Vector u y component</param>
    /// <param name="vx">Vector v x component</param>
    /// <param name="vy">Vector v y component</param>
    /// <returns>The angle between the two vectors (in radians)</returns>
    public static double AngleBetween(double ux, double uy, double vx, double vy)
    {
        var dot = ux * vx + uy * vy;
        var mod = Math.Sqrt((ux * ux + uy * uy) * (vx * vx + vy * vy));
        var theta = Math.Acos(dot / mod);
        if (ux * vy - uy * vx < 0.0)
        {
            theta = -theta;
        }
        return theta;
    }

    public static (IList<PointDouble> points, double minX, double minY, double maxX, double maxY) PlotEllipse(
        double cx, double cy,
        double rx, double ry,
        double startAngle = 0.0, double sweepAngle = Math.PI * 2,
        double ellipsRotationAngle = 0.0,
        double step = Math.PI / 90.0)
    {
        // If there is no sweep then return empty points
        if (NearZero(sweepAngle))
        {
            return (new List<PointDouble>(), 0.0, 0.0, 0.0, 0.0);
        }

        var minX = double.MaxValue;
        var maxX = double.MinValue;
        var minY = double.MaxValue;
        var maxY = double.MinValue;

        var ellipseRotationCosTheta = (float)Math.Cos(ellipsRotationAngle);
        var ellipseRotationSinTheta = (float)Math.Sin(ellipsRotationAngle);

        (double x, double y) CalculateAndRotate(double pointAngleTheta)
        {
            // Calculate x and y prime, these are the values before they
            // are rotated for the ellipse rotation angle
            var xp = rx * Math.Cos(pointAngleTheta);
            var yp = ry * Math.Sin(pointAngleTheta);

            // Rotate the point by the ellipse rotation about the origin
            var x = xp * ellipseRotationCosTheta - yp * ellipseRotationSinTheta;
            var y = xp * ellipseRotationSinTheta + yp * ellipseRotationCosTheta;

            // Offset the center location
            x += cx;
            y += cy;

            if (x < minX)
            {
                minX = x;
            }

            if (x > maxX)
            {
                maxX = x;
            }

            if (y < minY)
            {
                minY = y;
            }

            if (y > maxY)
            {
                maxY = y;
            }

            return (x, y);
        }

        var points = new List<PointDouble>();

        if (sweepAngle > 0)
        {
            for (var theta = startAngle; theta < startAngle + sweepAngle; theta += step)
            {
                var (x, y) = CalculateAndRotate(theta);
                points.Add(new PointDouble(x, y));
            }
        }
        else
        {
            for (var theta = startAngle; theta > startAngle + sweepAngle; theta -= step)
            {
                var (x, y) = CalculateAndRotate(theta);
                points.Add(new PointDouble(x, y));
            }
        }

        // Make sure we include a point at the very end angle (the step may have missed this)
        var (xEnd, yEnd) = CalculateAndRotate(startAngle + sweepAngle);

        // Only add it if the loop did not already add it
        var last = points[^1];
        if (!NearEqual(last.X, xEnd) || !NearEqual(last.Y, yEnd))
        {
            points.Add(new PointDouble(xEnd, yEnd));
        }

        return (points, minX, minY, maxX, maxY);
    }

    public static (List<PointDouble> points, List<PointType> types) PlotText(
        string text,
        FontDescription fontDescription,
        TextAlignment alignment,   // horizontal alignment of the whole run
        float x,                   // anchor X (before alignment shift)
        float y,                   // baseline Y
        Matrix3 transform)         // model transform applied to each output point
    {
        // 1) Build font + paint
        using var tf = SKTypeface.FromFamilyName(
            fontDescription.FamilyName,
            SKFontStyleWeight.Normal,
            SKFontStyleWidth.Normal,
            SKFontStyleSlant.Upright);

        using var paint = new SKPaint
        {
            Typeface = tf,
            TextSize = fontDescription.Size, // units = pixels (DIPs≈px at 96 DPI)
            IsAntialias = true,
            // Keep Left align; we do our own pre-shift below for deterministic geometry.
            TextAlign = SKTextAlign.Left
        };

        // 2) Horizontal alignment: shift X by the run advance
        //    - Center: anchor sits at run center
        //    - Right:  anchor sits at run right edge
        var advance = paint.MeasureText(text); // glyph advance width (no shaping)
        var xAligned = alignment switch
        {
            TextAlignment.Center => x - advance / 2f,
            TextAlignment.Right => x - advance,
            _ => x // Left/Justify
        };

        // 3) Build vector outlines for the text at (xAligned, y) baseline
        using var path = paint.GetTextPath(text, xAligned, y);
        if (path == null || path.IsEmpty)
        {
            return (new List<PointDouble>(), new List<PointType>());
        }

        // 4) Decompose SKPath -> point/type lists compatible with your geometry pipeline
        var points = new List<PointDouble>();
        var types = new List<PointType>();

        var rawPts = new SKPoint[4];
        using var it = path.CreateRawIterator();

        SKPathVerb verb;
        var figureStartIndex = -1;

        while ((verb = it.Next(rawPts)) != SKPathVerb.Done)
        {
            switch (verb)
            {
                case SKPathVerb.Move:
                    {
                        // Start of a new contour. rawPts[0] = move target
                        var p = rawPts[0];
                        points.Add(new PointDouble(p.X, p.Y));
                        types.Add(PointType.StartOfFigure);
                        figureStartIndex = points.Count - 1;
                        break;
                    }
                case SKPathVerb.Line:
                    {
                        // Line segment. rawPts[1] = line end
                        var p = rawPts[1];
                        points.Add(new PointDouble(p.X, p.Y));
                        types.Add(PointType.LinePoint);
                        break;
                    }
                case SKPathVerb.Quad:
                    {
                        // Quadratic Bézier. rawPts: [p0, c, e]
                        // Emit control then end as BezierPoint like System.Drawing.GraphicsPath does.
                        var c = rawPts[1];
                        var e = rawPts[2];
                        points.Add(new PointDouble(c.X, c.Y)); types.Add(PointType.BezierPoint);
                        points.Add(new PointDouble(e.X, e.Y)); types.Add(PointType.BezierPoint);
                        break;
                    }
                case SKPathVerb.Conic:
                    {
                        // Conic (weighted quadratic). Treat as quadratic control+end.
                        // If you need exact conic handling, read it.ConicWeight and convert to quad/cubic.
                        var c = rawPts[1];
                        var e = rawPts[2];
                        points.Add(new PointDouble(c.X, c.Y)); types.Add(PointType.BezierPoint);
                        points.Add(new PointDouble(e.X, e.Y)); types.Add(PointType.BezierPoint);
                        break;
                    }
                case SKPathVerb.Cubic:
                    {
                        // Cubic Bézier. rawPts: [p0, c1, c2, e]
                        var c1 = rawPts[1];
                        var c2 = rawPts[2];
                        var e = rawPts[3];
                        points.Add(new PointDouble(c1.X, c1.Y)); types.Add(PointType.BezierPoint);
                        points.Add(new PointDouble(c2.X, c2.Y)); types.Add(PointType.BezierPoint);
                        points.Add(new PointDouble(e.X, e.Y)); types.Add(PointType.BezierPoint);
                        break;
                    }
                case SKPathVerb.Close:
                    {
                        // Close current contour. Mark last point with ClosePoint flag.
                        if (figureStartIndex >= 0 && points.Count > figureStartIndex)
                        {
                            types[^1] |= PointType.ClosePoint;
                        }

                        figureStartIndex = -1;
                        break;
                    }
            }
        }

        if (points.Count == 0)
        {
            return (new List<PointDouble>(), new List<PointType>());
        }

        // 5) Optional vertical centering around the local bbox (matches your prior behavior).
        //    If you prefer true typographic centering, use SKFontMetrics instead.
        var minY = points.Min(p => p.Y);
        var maxY = points.Max(p => p.Y);
        var yCenterOffset = (maxY - minY) / 2.0;

        for (var i = 0; i < points.Count; i++)
        {
            // Apply vertical centering, then the caller-supplied transform.
            var centered = new PointDouble(points[i].X, points[i].Y - yCenterOffset);
            points[i] = centered * transform;
        }

        return (points, types);
    }

    public static PointDouble MeasureText(
        string text,
        FontDescription fontDescription,
        TextAlignment alignment,
        float x,
        float y,
        Matrix3 transform)
    {
        var (points, _) = PlotText(text, fontDescription, alignment, x, y, transform);

        if (points.Count == 0)
        {
            return new PointDouble();
        }

        var minX = double.MaxValue;
        var minY = double.MaxValue;
        var maxX = double.MinValue;
        var maxY = double.MinValue;

        foreach (var point in points)
        {
            minX = Math.Min(minX, point.X);
            minY = Math.Min(minX, point.X);
            maxX = Math.Max(maxX, point.X);
            maxY = Math.Max(maxX, point.X);
        }

        return new PointDouble(maxX - minX, maxY - minY); ;
    }

    public static IBoundary GetBoundary(double x1, double y1, double x2, double y2)
    {
        var minX = Math.Min(x1, x2);
        var minY = Math.Min(y1, y2);
        var maxX = Math.Max(x1, x2);
        var maxY = Math.Max(y1, y2);

        var location = new PointDouble(minX, minY);
        var size = new PointDouble(maxX - minX, maxY - minY).Abs();

        if (size.X <= 2)
        {
            location.X--;
            size.X = 2;
        }

        if (size.Y <= 2)
        {
            location.Y--;
            size.Y = 2;
        }

        return new Boundary(location, size);
    }

    public static IBoundary GetBoundary(IList<PointDouble> points)
    {
        if (points.Count == 0)
        {
            return new Boundary(new PointDouble(), new PointDouble());
        }

        var (min, max) = GetMinMax(points);

        return GetBoundary(min.X, min.Y, max.X, max.Y);
    }

    public static (PointDouble min, PointDouble max) GetMinMax(IEnumerable<PointDouble> points)
    {
        var minX = double.MaxValue;
        var minY = double.MaxValue;
        var maxX = double.MinValue;
        var maxY = double.MinValue;

        foreach (var point in points)
        {
            minX = Math.Min(minX, point.X);
            minY = Math.Min(minY, point.Y);
            maxX = Math.Max(maxX, point.X);
            maxY = Math.Max(maxY, point.Y);
        }

        return (new PointDouble(minX, minY), new PointDouble(maxX, maxY));
    }

    public static PointInPolgygonResult PointInPolygon(PointDouble point, IList<PointDouble[]> polygons)
    {
        var overallResult = PointInPolgygonResult.Outside;

        foreach (var polygon in polygons)
        {
            // Get result for this polygon
            var result = PointInPolygon(point, polygon);

            // If outside then continue to next polygon
            if (result == PointInPolgygonResult.Outside)
            {
                continue;
            }

            // If inside that is as good as it gets so return result
            if (result == PointInPolgygonResult.Inside)
            {
                return PointInPolgygonResult.Inside;
            }

            // Edge and vertex same same so just set it
            overallResult = result;
        }

        return overallResult;
    }

    public static PointInPolgygonResult PointInPolygon(PointDouble point, PointDouble[] polyPoints)
    {
        var n = polyPoints.Length;  // number of vertices
        int i, i1;                  // point index; i1 = i-1 mod n
        double x;                   // x intersection of e with ray
        var rCross = 0;             // number of right edge/ray crossings
        var lCross = 0;             // number of left edge/ray crossings

        /* Shift polygon so that 'point' is the origin. */
        var polygon = polyPoints.Select(p => new PointDouble(p.X - point.X, p.Y - point.Y)).ToArray();

        /* For each edge e=(i-1,i), see if crosses ray. */
        for (i = 0; i < n; i++)
        {
            /* First see if point=(0,0) is a vertex. */
            if (NearZero(polygon[i].X) && NearZero(polygon[i].Y))
            {
                return PointInPolgygonResult.Vertex;
            }

            i1 = (i + n - 1) % n;
            // printf("e=(%d,%d)\t", i1, i);

            /* if e "straddles" the x-axis... */
            /* The commented-out statement is logically equivalent to the one
               following. */
            /* if( ( ( P[i].Y > 0 ) && ( P[i1].Y <= 0 ) ) ||
               ( ( P[i1].Y > 0 ) && ( P[i] [Y] <= 0 ) ) ) { */

            if (polygon[i].Y > 0 != polygon[i1].Y > 0)
            {

                /* e straddles ray, so compute intersection with ray. */
                x = (polygon[i].X * (double)polygon[i1].Y - polygon[i1].X * (double)polygon[i].Y) / (double)(polygon[i1].Y - polygon[i].Y);
                // printf("straddles: x = %g\t", x);

                /* crosses ray if strictly positive intersection. */
                if (x > 0)
                {
                    rCross++;
                }
            }
            // printf("Right cross=%d\t", Rcross);

            /* if e straddles the x-axis when reversed... */
            /* if( ( ( P[i] [Y] < 0 ) && ( P[i1].Y >= 0 ) ) ||
               ( ( P[i1].Y < 0 ) && ( P[i] [Y] >= 0 ) ) )  { */

            if (polygon[i].Y < 0 != polygon[i1].Y < 0)
            {

                /* e straddles ray, so compute intersection with ray. */
                x = (polygon[i].X * (double)polygon[i1].Y - polygon[i1].X * (double)polygon[i].Y) / (double)(polygon[i1].Y - polygon[i].Y);
                // printf("straddles: x = %g\t", x);

                /* crosses ray if strictly positive intersection. */
                if (x < 0)
                {
                    lCross++;
                }
            }
            // printf("Left cross=%d\n", Lcross);
        }

        /* q on the edge if left and right cross are not the same parity. */
        if (rCross % 2 != lCross % 2)
        {
            return PointInPolgygonResult.Edge;
        }

        /* q inside iff an odd number of crossings. */
        if (rCross % 2 == 1)
        {
            return PointInPolgygonResult.Inside;
        }
        else
        {
            return PointInPolgygonResult.Outside;
        }
    }

    public static IList<PointDouble> RotatePoints(IList<PointDouble> points, double angleInRadians)
    {
        // Get min / max locations
        var (min, max) = GetMinMax(points);

        // Calculate center of min max
        var deltaX = min.X + (max.X - min.X) / 2.0;
        var deltaY = min.Y + (max.Y - min.Y) / 2.0;

        // 1. Center points about origin
        // 2. Rotate points
        // 3. Move back to original location
        var m =
            Matrix3.CreateTranslate(new PointDouble(-deltaX, -deltaY)) *
            Matrix3.CreateRotation(angleInRadians) *
            Matrix3.CreateTranslate(new PointDouble(deltaX, deltaY));

        points = points.Select(p => p * m).ToArray();

        return points;
    }
}
