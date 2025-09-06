namespace Mekatrol.CAM.Core.Geometry.Algorithms;

internal class GrahamsHull
{
    // Start with capacity for 1000 points (can grow thereafter)
    private readonly List<PointLinkedListItem> _points = new(1000);
    private int _activePointCount = 0;
    private bool _hasDeletedItems = false;

    public IList<PointLong> GetHull(IList<PointLong> points)
    {
        // Make copy of list so it is not destructive on original 
        points = points.ToList();

        // Reset the number of active points in list
        _activePointCount = points.Count;

        // Clear any existing points
        _points.Clear();

        // Create point list
        for (var i = 0; i < _activePointCount; i++)
        {
            _points.Add(new PointLinkedListItem { OriginalIndex = i, Point = points[i], Delete = false });
        }

        // Move lowest point to index 0
        FindLowest();

        // Sort the remainder by angle
        Sort();

        // Delete any marked for deletion
        if (_hasDeletedItems)
        {
            Squash();
        }

        // Generate hull linked list
        var top = GenerateHullList();

        // Construct hull points from linked list
        var hullPoints = new List<PointLong>();

        while (top != null)
        {
            hullPoints.Add(top.Item!.Point);
            top = top.Next;
        }

        // Return the hull points
        return hullPoints;
    }

    private void FindLowest()
    {
        int i;
        var m = 0; /* Index of lowest so far. */

        for (i = 1; i < _activePointCount; i++)
        {
            if ((_points[i].Point.Y < _points[m].Point.Y) ||
                ((_points[i].Point.Y == _points[m].Point.Y) && (_points[i].Point.X > _points[m].Point.X)))
            {
                m = i;
            }
        }

        /* Swap P[0] and P[m] */
        (_points[0], _points[m]) = (_points[m], _points[0]);
    }

    private PointLinkedList? GenerateHullList()
    {
        PointLinkedList? top = null;
        int i;
        PointLinkedListItem? p1, p2;

        top = Push(_points[0], top);
        top = Push(_points[1], top);

        /* Bottom two elements will never be removed. */
        i = 2;

        while (i < _activePointCount)
        {
            // printf("Stack at top of while loop, i=%d, vnum=%d:\n", i, P[i].vnum);

            if (top == null || top.Next == null)
            {
                throw new Exception(); //printf("Error\n"), exit(EXIT_FAILURE);
            }

            p1 = top.Next.Item;
            p2 = top.Item;

            if (Left(p1!.Point, p2!.Point, _points[i].Point))
            {
                top = Push(_points[i], top);
                i++;
            }
            else
            {
                top = Pop(top);
            }

            // printf("Stack at bot of while loop, i=%d, vnum=%d:\n", i, P[i].vnum);
        }

        return top;
    }

    private void Sort()
    {
        // Create a point structure comparer
        var comparer = new PointComparer(_points);

        // Sort the point structures
        _points.Sort(1, _activePointCount - 1, comparer);

        // Record if the comparer deleted any points
        _hasDeletedItems = comparer.DeletedCount > 0;
    }

    private void Squash()
    {
        var i = 0;
        var j = 0;

        while (i < _activePointCount)
        {
            if (!_points[i].Delete)
            {
                /* if not marked for deletion */
                /* Copy P[i] to P[j]. */
                _points[j] = _points[i];

                j++;
            }

            /* else do nothing: delete by skipping. */
            i++;
        }

        _activePointCount = j;
    }

    private static int AreaSign(PointLong a, PointLong b, PointLong c)
    {
        double area2;

        area2 = (b.X - a.X) * (double)(c.Y - a.Y) -
                (c.X - a.X) * (double)(b.Y - a.Y);

        /* The area should be an integer. */
        if (area2 > 0.5)
        {
            return 1;
        }
        else if (area2 < -0.5)
        {
            return -1;
        }
        else
        {
            return 0;
        }
    }

    private static PointLinkedList? Pop(PointLinkedList s)
    {
        var top = s.Next;
        return top;
    }

    private static PointLinkedList? Push(PointLinkedListItem p, PointLinkedList? top)
    {
        var s = new PointLinkedList
        {
            Item = p,
            Next = top
        };

        return s;
    }

    private static bool Left(PointLong a, PointLong b, PointLong c)
    {
        return AreaSign(a, b, c) > 0;
    }

    private class PointLinkedListItem
    {
        public int OriginalIndex;
        public PointLong Point;
        public bool Delete;

        public override string ToString()
        {
            return $"{OriginalIndex} del={Delete} => {Point}";
        }
    }

    private class PointLinkedList
    {
        public PointLinkedListItem? Item;
        public PointLinkedList? Next;
    }

    private class PointComparer(IList<PointLinkedListItem> points) : IComparer<PointLinkedListItem>
    {
        private readonly IList<PointLinkedListItem> _points = points;
        private int _deletedCount = 0;

        public int DeletedCount
        {
            get
            {
                return _deletedCount;
            }
        }

        public int Compare(PointLinkedListItem? pi, PointLinkedListItem? pj)
        {
            long a;    /* area */
            long x, y; /* projections of point i &  point j in 1st quadrant */

            a = AreaSign(_points[0].Point, pi!.Point, pj!.Point);

            if (a > 0)
            {
                return -1;
            }
            else if (a < 0)
            {
                return 1;
            }
            else
            {
                /* Collinear with P[0] */
                x = Math.Abs(pi.Point.X - _points[0].Point.X) - Math.Abs(pj.Point.X - _points[0].Point.X);
                y = Math.Abs(pi.Point.Y - _points[0].Point.Y) - Math.Abs(pj.Point.Y - _points[0].Point.Y);

                _deletedCount++;
                if ((x < 0) || (y < 0))
                {
                    pi.Delete = true;
                    return -1;
                }
                else if ((x > 0) || (y > 0))
                {
                    pj.Delete = true;
                    return 1;
                }
                else
                {
                    /* points are coincident */
                    if (pi.OriginalIndex > pj.OriginalIndex)
                    {
                        pj.Delete = true;
                    }
                    else
                    {
                        pi.Delete = true;
                    }
                    return 0;
                }
            }
        }
    }
}
