using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum Shape
{
    CUBE,
    PYRAMID,
    TETRAHEDRON,
    HEXAGONAL_PRISM,
    OTCAHEDRON
}

public class Grid_Builder_3D : MonoBehaviour
{
    private List<Point> gridPoints = new List<Point>();
    private List<Point> subPoints = new List<Point>();
    private List<Point> boundingPoints = new List<Point>();

    [SerializeField] private int GRID_LENGTH = 10;
    [SerializeField] private int GRID_HEIGHT = 10;
    [SerializeField] private int GRID_DEPTH = 10;
    [SerializeField] private float SUB_POINT_CHANCE = 0.35f;
    [SerializeField] private float SUB_POINT_RIGIDITY_CHANCE = 0.35f;
    [SerializeField] private int MIN_CONNECTION_COUNT = 3;
    [SerializeField] private float MIN_POINT_DISTANCE = 0.35f;

    [SerializeField] private Shape currentShape;

    [InspectorButton("CreatePoints", ButtonWidth = 250)]
    public bool CreatePointsButton;
    [InspectorButton("CreateConnections", ButtonWidth = 250)]
    public bool CreateConnectionsButton;
    [InspectorButton("CreateSubPoints", ButtonWidth = 250)]
    public bool CreateSubPointsButton;
    [InspectorButton("AddSubPointConnections", ButtonWidth = 250)]
    public bool AddSubPointConnectionsButton;
    [InspectorButton("RemoveRandomConnections", ButtonWidth = 250)]
    public bool RemoveRandomConnectionsButton;
    [InspectorButton("RebalanceGrid", ButtonWidth = 250)]
    public bool RebalanceGridButton;
    [InspectorButton("ClearGrid", ButtonWidth = 250)]
    public bool ClearGridButton;

    private void ClearGrid()
    {
        gridPoints.Clear();
        subPoints.Clear();
    }

    private void CreatePoints()
    {
        switch(currentShape)
        {
            case Shape.CUBE:
                CreateCube();
                break;
            case Shape.PYRAMID:
                CreatePyramid();
                break;
            case Shape.TETRAHEDRON:
                CreateTetrahedron();
                break;
            case Shape.HEXAGONAL_PRISM:
                Debug.LogWarning("NOT HERE YET");
                break;
            case Shape.OTCAHEDRON:
                Debug.LogWarning("NOT HERE YET");
                break;
            default:
                Debug.LogError("THIS SHOULD NEVER FIRE");
                break;
        }
    }

    private void CreateCube()
    {
        /* cube corners
         * (0,0,0)
         * (max, 0, 0)
         * (max, 0, max)
         * (0, 0, max)
         * 
         * (0, max, 0)
         * (max, max, 0)
         * (max, max, max)
         * (0, max, max)
         */

        gridPoints.Add(new Point(0, 0, 0, false));
        gridPoints.Add(new Point(GRID_LENGTH - 1, 0, 0, false));
        gridPoints.Add(new Point(GRID_LENGTH - 1, 0, GRID_DEPTH - 1, false));
        gridPoints.Add(new Point(0, 0, GRID_DEPTH - 1, false));
        gridPoints.Add(new Point(0, GRID_HEIGHT - 1, 0, false));
        gridPoints.Add(new Point(GRID_LENGTH - 1, GRID_HEIGHT - 1, 0, false));
        gridPoints.Add(new Point(GRID_LENGTH - 1, GRID_HEIGHT - 1, GRID_DEPTH - 1, false));
        gridPoints.Add(new Point(0, GRID_HEIGHT - 1, GRID_DEPTH - 1, false));

        boundingPoints.Add(new Point(0, 0, 0, false));
        boundingPoints.Add(new Point(GRID_LENGTH - 1, 0, 0, false));
        boundingPoints.Add(new Point(GRID_LENGTH - 1, 0, GRID_DEPTH - 1, false));
        boundingPoints.Add(new Point(0, 0, GRID_DEPTH - 1, false));
        boundingPoints.Add(new Point(0, GRID_HEIGHT - 1, 0, false));
        boundingPoints.Add(new Point(GRID_LENGTH - 1, GRID_HEIGHT - 1, 0, false));
        boundingPoints.Add(new Point(GRID_LENGTH - 1, GRID_HEIGHT - 1, GRID_DEPTH - 1, false));
        boundingPoints.Add(new Point(0, GRID_HEIGHT - 1, GRID_DEPTH - 1, false));

        boundingPoints[0].AddConnection(boundingPoints[1]);
        boundingPoints[0].AddConnection(boundingPoints[3]);
        boundingPoints[0].AddConnection(boundingPoints[4]);

        boundingPoints[1].AddConnection(boundingPoints[2]);
        boundingPoints[1].AddConnection(boundingPoints[5]);

        boundingPoints[2].AddConnection(boundingPoints[3]);
        boundingPoints[2].AddConnection(boundingPoints[6]);

        boundingPoints[3].AddConnection(boundingPoints[7]);

        boundingPoints[4].AddConnection(boundingPoints[5]);
        boundingPoints[4].AddConnection(boundingPoints[7]);

        boundingPoints[5].AddConnection(boundingPoints[6]);

        boundingPoints[6].AddConnection(boundingPoints[7]);

        for (int x = 0; x < GRID_LENGTH; x++)
        {
            for (int y = 0; y < GRID_HEIGHT; y++)
            {
                for(int z = 0; z < GRID_DEPTH; z++)
                {
                    if(x == 0 || x == GRID_LENGTH - 1 || y == 0 || y == GRID_HEIGHT - 1 || z == 0 || z == GRID_DEPTH - 1)
                    {
                        Point temp = new Point(x, y, z);
                        //Debug.Log(gridPoints.Find(i => i.Position == temp.Position) == null);
                        if (gridPoints.Find(i => i.Position == temp.Position) == null)
                        {
                            gridPoints.Add(temp);
                        }
                    }
                    else
                    {
                        gridPoints.Add(new Point(x, y, z));
                    }   
                }
            }
        }

        //for(int i = 8; i < gridPoints.Count; i++)
        //{
        //    if(DistanceLineSegmentPoint(gri))
        //}
        //Debug.Log(DistanceLineSegmentPoint(gridPoints[0].Position, gridPoints[1].Position, gridPoints[104].Position) + "::" + gridPoints[104].Position);

    }

    private void CreatePyramid()
    {
        /* corner points
         * (0,0,0)
         * (max, 0, 0)
         * (max, 0, max)
         * (0, 0, max)
         * (max/2, height, max/2)
         */

        gridPoints.Add(new Point(0, 0, 0, false));
        gridPoints.Add(new Point(GRID_LENGTH - 1, 0, 0, false));
        gridPoints.Add(new Point(GRID_LENGTH - 1, 0, GRID_DEPTH - 1, false));
        gridPoints.Add(new Point(0, 0, GRID_DEPTH - 1, false));
        gridPoints.Add(new Point((GRID_LENGTH-1) / 2f, (GRID_HEIGHT-1)/2f, (GRID_DEPTH-1) / 2f, false));

        boundingPoints.Add(new Point(0, 0, 0, false));
        boundingPoints.Add(new Point(GRID_LENGTH - 1, 0, 0, false));
        boundingPoints.Add(new Point(GRID_LENGTH - 1, 0, GRID_DEPTH - 1, false));
        boundingPoints.Add(new Point(0, 0, GRID_DEPTH - 1, false));
        boundingPoints.Add(new Point((GRID_LENGTH - 1) / 2f, (GRID_HEIGHT - 1) / 2f, (GRID_DEPTH - 1) / 2f, false));

        boundingPoints[0].AddConnection(boundingPoints[1]);
        boundingPoints[0].AddConnection(boundingPoints[3]);
        boundingPoints[0].AddConnection(boundingPoints[4]);

        boundingPoints[1].AddConnection(boundingPoints[2]);
        boundingPoints[1].AddConnection(boundingPoints[4]);

        boundingPoints[2].AddConnection(boundingPoints[3]);
        boundingPoints[2].AddConnection(boundingPoints[4]);

        boundingPoints[3].AddConnection(boundingPoints[4]);

        for (int y = 0; y < GRID_HEIGHT; y++)
        {
            int length = GRID_LENGTH - y;
            int depth = GRID_DEPTH - y;
            for(int x = y; x < length; x++)
            {
                for(int z = y; z < depth; z++)
                {
                    Point temp = new Point(x, y, z);
                    if (gridPoints.Find(i => i.Position == temp.Position) == null)
                    {
                        gridPoints.Add(temp);
                    }
                }
            }
        }
    }

    private void CreateTetrahedron()
    {
        Debug.LogWarning("NOT DONE YET");

        /* corner points
         * (0, 0, 0)
         * FILL IN THE REST
         */

        float height = Mathf.Sqrt(2f / 3f) * GRID_HEIGHT;

        boundingPoints.Add(new Point(0, 0, 0, false));
        boundingPoints.Add(new Point(GRID_LENGTH, 0, 0, false));
        boundingPoints.Add(new Point(GRID_LENGTH / 2f, 0, GRID_DEPTH * (Mathf.Sqrt(3f) / 2f), false));
        boundingPoints.Add(new Point((boundingPoints[0].Position.x + boundingPoints[1].Position.x + boundingPoints[2].Position.x) / 3f,
            height, (boundingPoints[0].Position.z + boundingPoints[1].Position.z + boundingPoints[2].Position.z) / 3f, false));

        boundingPoints[0].AddConnection(boundingPoints[1]);
        boundingPoints[0].AddConnection(boundingPoints[2]);
        boundingPoints[0].AddConnection(boundingPoints[3]);

        boundingPoints[1].AddConnection(boundingPoints[2]);
        boundingPoints[1].AddConnection(boundingPoints[3]);

        boundingPoints[2].AddConnection(boundingPoints[3]);


        gridPoints.Add(new Point(0, 0, 0, false));
        gridPoints.Add(new Point(GRID_LENGTH, 0, 0, false));
        gridPoints.Add(new Point(GRID_LENGTH / 2f, 0, GRID_DEPTH * (Mathf.Sqrt(3f) / 2f), false));
        gridPoints.Add(new Point((gridPoints[0].Position.x + gridPoints[1].Position.x + gridPoints[2].Position.x) / 3f,
            height, (gridPoints[0].Position.z + gridPoints[1].Position.z + gridPoints[2].Position.z) / 3f, false));

        /* for x in 0 to gridlength
         *  for z in 0 to griddepth
         *   for y in 0 to height
         *    if proposed point is inside tetrahedron
         *     create point
         */
        for (int y = 0; y < GRID_HEIGHT; y++)
        {
            for (int x = 0; x < GRID_LENGTH; x++)
            {
                for (int z = 0; z < GRID_DEPTH; z++)
                {
                    Vector3 proposedPoint = new Vector3(x, y, z);
                    Debug.Log(proposedPoint);
                    if (IsPointInTetrahedron(gridPoints[0].Position, gridPoints[1].Position, gridPoints[2].Position, gridPoints[3].Position, proposedPoint))//todo le busted
                    {
                        gridPoints.Add(new Point(x,y,y));
                    }
                }
            }
        }
    }

    private bool IsPointInTetrahedron(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Vector3 p)
    {
        return IsSameSide(v1, v2, v3, v4, p) && IsSameSide(v2, v3, v4, v1, p) && IsSameSide(v3, v4, v1, v2, p) && IsSameSide(v4, v1, v2, v3, p);
    }

    private bool IsSameSide(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Vector3 p)
    {
        Vector3 normal = Vector3.Cross(v2 - v1, v3 - v1);
        float v4Dot = Vector3.Dot(normal, v4 - v1);
        float pDot = Vector3.Dot(normal, p - v1);
        return Mathf.Sign(v4Dot) == Mathf.Sign(pDot);
    }

    private void CreateConnections()
    {
        switch (currentShape)
        {
            case Shape.CUBE:
                CreateConnectionsCube();
                break;
            case Shape.PYRAMID:
                CreateConnectionsPyramid();
                break;
            case Shape.TETRAHEDRON:
                Debug.LogWarning("NOT HERE YET");
                break;
            case Shape.HEXAGONAL_PRISM:
                Debug.LogWarning("NOT HERE YET");
                break;
            case Shape.OTCAHEDRON:
                Debug.LogWarning("NOT HERE YET");
                break;
            default:
                Debug.LogError("THIS SHOULD NEVER FIRE");
                break;
        }
    }

    private void CreateConnectionsCube()
    {
        for (int x = 0; x < GRID_LENGTH; x++)
        {
            for (int y = 0; y < GRID_HEIGHT; y++)
            {
                for (int z = 0; z < GRID_DEPTH; z++)
                {
                    Point point = GetPoint(x, y, z);

                    if (x == 0 || x == GRID_LENGTH - 1)
                    {
                        if (y - 1 >= 0)
                        {
                            point.AddConnection(GetPoint(x, y - 1, z));
                        }
                        if (y + 1 < GRID_HEIGHT)
                        {
                            point.AddConnection(GetPoint(x, y + 1, z));
                        }
                        if (z - 1 >= 0)
                        {
                            point.AddConnection(GetPoint(x, y, z - 1));
                        }
                        if (z + 1 < GRID_DEPTH)
                        {
                            point.AddConnection(GetPoint(x, y, z + 1));
                        }
                    }

                    if (y == 0 || y == GRID_HEIGHT - 1)
                    {
                        if (x - 1 >= 0)
                        {
                            point.AddConnection(GetPoint(x - 1, y, z));
                        }
                        if (x + 1 < GRID_LENGTH)
                        {
                            point.AddConnection(GetPoint(x + 1, y, z));
                        }
                        if (z - 1 >= 0)
                        {
                            point.AddConnection(GetPoint(x, y, z - 1));
                        }
                        if (z + 1 < GRID_DEPTH)
                        {
                            point.AddConnection(GetPoint(x, y, z + 1));
                        }
                    }

                    if (z == 0 || z == GRID_DEPTH - 1)
                    {
                        if (x - 1 >= 0)
                        {
                            point.AddConnection(GetPoint(x - 1, y, z));
                        }
                        if (x + 1 < GRID_LENGTH)
                        {
                            point.AddConnection(GetPoint(x + 1, y, z));
                        }
                        if (y - 1 >= 0)
                        {
                            point.AddConnection(GetPoint(x, y - 1, z));
                        }
                        if (y + 1 < GRID_HEIGHT)
                        {
                            point.AddConnection(GetPoint(x, y + 1, z));
                        }
                    }

                    if (x + 1 < GRID_LENGTH)
                    {
                        point.AddConnection(GetPoint(x + 1, y, z));
                    }
                    if (x - 1 >= 0)
                    {
                        point.AddConnection(GetPoint(x - 1, y, z));
                    }

                    if (y + 1 < GRID_HEIGHT)
                    {
                        point.AddConnection(GetPoint(x, y + 1, z));
                    }
                    if (y - 1 >= 0)
                    {
                        point.AddConnection(GetPoint(x, y - 1, z));
                    }

                    if (z - 1 >= 0)
                    {
                        point.AddConnection(GetPoint(x, y, z - 1));
                    }
                    if (z + 1 < GRID_DEPTH)
                    {
                        point.AddConnection(GetPoint(x, y, z + 1));
                    }

                    if (x + 1 < GRID_LENGTH && y + 1 < GRID_HEIGHT)
                    {
                        point.AddConnection(GetPoint(x + 1, y + 1, z));
                    }

                    if (x + 1 < GRID_LENGTH && z + 1 < GRID_DEPTH)
                    {
                        point.AddConnection(GetPoint(x + 1, y, z + 1));
                    }

                    if (y + 1 < GRID_HEIGHT && z + 1 < GRID_DEPTH)
                    {
                        point.AddConnection(GetPoint(x, y + 1, z + 1));
                    }
                }
            }
        }
        Debug.Log(Point.EDGE_COUNT);
    }

    private void CreateConnectionsPyramid()
    {
        for (int y = 0; y < GRID_HEIGHT; y++)
        {
            int length = GRID_LENGTH - y;
            int depth = GRID_DEPTH - y;

            //Debug.Log(y + " " + length + " " + depth);

            if (y == length && y == depth)
            {
                break;
            }

            for (int x = y; x < length; x++)
            {
                for (int z = y; z < depth; z++)
                {
                    Point point = GetPoint(x, y, z);

                    if(x == y)
                    {
                        if (z - 1 >= y)
                        {
                            point.AddConnection(GetPoint(x, y, z - 1));
                        }
                        if (z + 1 < depth)
                        {
                            point.AddConnection(GetPoint(x, y, z + 1));
                        }

                        if(y - 1 >= 0)
                        {
                            point.AddConnection(GetPoint(x - 1, y - 1, z));
                        }
                    }
                    else if (x == length - 1)
                    {
                        if (z - 1 >= y)
                        {
                            point.AddConnection(GetPoint(x, y, z - 1));
                        }
                        if (z + 1 < depth)
                        {
                            point.AddConnection(GetPoint(x, y, z + 1));
                        }

                        if (y - 1 >= 0)
                        {
                            point.AddConnection(GetPoint(x + 1, y - 1, z));
                        }
                    }

                    if (z == y)
                    {
                        if (x - 1 >= y)
                        {
                            point.AddConnection(GetPoint(x - 1, y, z));
                        }
                        if (x + 1 < length)
                        {
                            point.AddConnection(GetPoint(x + 1, y, z));
                        }

                        if (y - 1 >= 0)
                        {
                            point.AddConnection(GetPoint(x, y - 1, z - 1));
                        }
                    }
                    else if (z == depth - 1)
                    {
                        if (x - 1 >= y)
                        {
                            point.AddConnection(GetPoint(x - 1, y, z));
                        }
                        if (x + 1 < length)
                        {
                            point.AddConnection(GetPoint(x + 1, y, z));
                        }

                        if (y - 1 >= 0)
                        {
                            point.AddConnection(GetPoint(x, y - 1, z + 1));
                        }
                    }

                    if (x + 1 < length)
                    {
                        point.AddConnection(GetPoint(x + 1, y, z));
                    }
                    if (x - 1 >= y)
                    {
                        point.AddConnection(GetPoint(x - 1, y, z));
                    }

                    if (z + 1 < depth)
                    {
                        point.AddConnection(GetPoint(x, y, z + 1));
                    }
                    if (z - 1 >= y)
                    {
                        point.AddConnection(GetPoint(x, y, z - 1));
                    }

                    //diagonals
                    if (x + 1 < length && y + 1 < GRID_HEIGHT && GetPoint(x + 1, y + 1, z) != null)
                    {
                        point.AddConnection(GetPoint(x + 1, y + 1, z));
                    }

                    if (x + 1 < length && z + 1 < depth)
                    {
                        point.AddConnection(GetPoint(x + 1, y, z + 1));
                    }

                    if (z + 1 < depth && y + 1 < GRID_HEIGHT && GetPoint(x, y + 1, z + 1) != null)
                    {
                        point.AddConnection(GetPoint(x, y + 1, z + 1));
                    }

                    //vertical interior connections
                    if (y + 1 <= GRID_HEIGHT && GetPoint(x, y+1, z) != null)
                    {
                        point.AddConnection(GetPoint(x, y + 1, z));
                    }

                    //outer edge diagonal connections
                    if(x == y && z == y && GetPoint(x + 1, y + 1, z + 1) != null)
                    {
                        point.AddConnection(GetPoint(x + 1, y + 1, z + 1));
                    }

                    if (x == length-1 && z == y && GetPoint(x - 1, y + 1, z + 1) != null)
                    {
                        point.AddConnection(GetPoint(x - 1, y + 1, z + 1));
                    }

                    if (x == y && z == depth-1 && GetPoint(x + 1, y + 1, z - 1) != null)
                    {
                        point.AddConnection(GetPoint(x + 1, y + 1, z - 1));
                    }

                    if (x == length-1 && z == depth-1 && GetPoint(x - 1, y + 1, z - 1) != null)
                    {
                        point.AddConnection(GetPoint(x - 1, y + 1, z - 1));
                    }

                    //apex connections
                    //Debug.Log(y + " " + length + " " + (y + 1 == length - 1) + " :: " + (GetPoint((GRID_LENGTH - 1) / 2f, (GRID_HEIGHT - 1) / 2f, (GRID_DEPTH - 1) / 2f) != null));
                    if(y + 1 == length - 1 && GetPoint((GRID_LENGTH - 1) / 2f, (GRID_HEIGHT - 1) / 2f, (GRID_DEPTH - 1) / 2f) != null)
                    {
                        point.AddConnection(GetPoint((GRID_LENGTH - 1) / 2f, (GRID_HEIGHT - 1) / 2f, (GRID_DEPTH - 1) / 2f));
                    }
                }
            }
            
        }
    }

    private void CreateSubPoints()
    {
        ShuffleGridPoints();

        foreach (Point start in gridPoints)
        {
            //Debug.Log("start");
            bool subPointAdded = false;

            foreach (Point neighbor in start.Connections)
            {
                if(subPointAdded)
                {
                    subPointAdded = false;
                    break;
                }
                //Debug.Log("neighbor");

                foreach (Point localLeft in neighbor.Connections)
                {
                    if (localLeft == start)
                    {
                        continue;
                    }
                    //Debug.Log("local left");

                    Vector3 proposedPosition = new Vector3((start.Position.x + neighbor.Position.x + localLeft.Position.x) / 3, 
                        (start.Position.y + neighbor.Position.y + localLeft.Position.y) / 3, 
                        (start.Position.z + neighbor.Position.z + localLeft.Position.z) / 3);

                    if (localLeft.Connections.Contains(start) && IsLeft(start.Position, neighbor.Position, localLeft.Position)
                        //&& !SubPointExists(proposedPosition.x, proposedPosition.y, proposedPosition.z)
                        && Random.Range(0f, 1f) <= SUB_POINT_CHANCE)
                    {
                        Point newPoint = new Point(proposedPosition.x, proposedPosition.y, proposedPosition.z, (Random.Range(0, 1f) >= SUB_POINT_RIGIDITY_CHANCE));
                        subPoints.Add(newPoint);
                        newPoint.AddConnection(start, false);
                        newPoint.AddConnection(neighbor, false);
                        newPoint.AddConnection(localLeft, false);
                        subPointAdded = true;
                        //Debug.Log("sub point added");
                        break;
                    }
                }
            }
        }

        for (int i = 0; i < subPoints.Count; i++)
        {
            for (int j = i + 1; j < subPoints.Count; j++)
            {
                //Debug.Log(i + "::" + j + "::" + test.Count);
                if (subPoints[i].IsNearEnough(subPoints[j]))
                {
                    subPoints.RemoveAt(j);
                    j--;
                }
            }
        }
    }

    private void AddSubPointConnections()
    {
        foreach (Point point in subPoints)
        {
            point.SolidifyConnections();
        }

        gridPoints.AddRange(subPoints);
        subPoints.Clear();
    }

    private void RemoveRandomConnections()
    {
        ShuffleGridPoints();

        List<Point> connsToRemove = new List<Point>();

        foreach (Point point in gridPoints)
        {
            if (point.Connections.Count <= MIN_CONNECTION_COUNT)
            {
                continue;
            }

            foreach (Point connection in point.Connections)
            {
                if (!point.isModifiable && !connection.isModifiable)
                {
                    continue;
                }

                if (point.Connections.Count - connsToRemove.Count > MIN_CONNECTION_COUNT
                    && connection.Connections.Count > MIN_CONNECTION_COUNT)
                {
                    connsToRemove.Add(connection);
                }
            }

            foreach (Point removed in connsToRemove)
            {
                point.RemoveConnectionsMutual(removed);
            }

            connsToRemove.Clear();

            if (point.Connections.Count < MIN_CONNECTION_COUNT)
            {
                Debug.LogError(point.Connections.Count);
            }
        }
    }

    private void ShuffleGridPoints()
    {
        for (int i = 0; i < gridPoints.Count; i++)
        {
            Point temp = gridPoints[i];
            int randIndex = Random.Range(i, gridPoints.Count);

            //temp.ShuffleConnections();

            gridPoints[i] = gridPoints[randIndex];
            gridPoints[randIndex] = temp;
        }
    }

    private void RebalanceGrid()
    {
        foreach (Point point in gridPoints)
        {
            if (point.isModifiable)
            {
                Vector3 newPos = Vector3.zero;

                foreach (Point neighbor in point.Connections)
                {
                    newPos += neighbor.Position;
                }

                newPos = newPos / point.Connections.Count;

                point.Connections = point.Connections.OrderBy(x => Vector3.Distance(newPos, x.Position)).ToList();

                if (Vector3.Distance(newPos, point.Connections[0].Position) > MIN_POINT_DISTANCE)
                {
                    point.Position = newPos;
                }
            }
        }
    }

    private void UpdateConnections()
    {
        foreach (Point core in gridPoints)
        {
            foreach (Point neighbor in core.Connections)
            {
                if (!neighbor.Connections.Contains(core))
                {
                    neighbor.AddConnection(core);
                }
            }
        }
    }

    private Point GetPoint(float x, float y, float z)
    {
        return gridPoints.Find(point => point.IsNearEnough(x, y, z));
    }

    private bool SubPointExists(float x, float y, float z)
    {
        if(subPoints.Count > 0)
        {
            return (subPoints.Find(point => point.IsNearEnough(x, y, z)) != null);
        }

        return false;
    }

    private bool IsLeft(Vector3 a, Vector3 b, Vector3 c)
    {
        Vector3 dir = Vector3.Cross(b - a, b - c);
        Vector3 norm = dir.normalized;
        return Vector3.Dot(dir, norm) > 0;
    }

    private float DistanceLineSegmentPoint(Vector3 a, Vector3 b, Vector3 p)
    {
        if(a == b)
        {
            return Vector3.Distance(a, p);
        }

        Vector3 ba = b - a;
        Vector3 pa = a - p;
        return (pa - ba * (Vector3.Dot(pa, ba) / Vector3.Dot(ba, ba))).magnitude;
    }

    [SerializeField] private bool DrawPoints = true;
    [SerializeField] private bool DrawConnections = true;
    [SerializeField] private bool DrawSubPoints = true;
    [SerializeField] private bool DrawBoundingLines = true;

    public void OnDrawGizmos()
    {
        if(DrawConnections)
        {
            Gizmos.color = Color.green;
            foreach (Point point in gridPoints)
            {
                foreach (Point connection in point.Connections)
                {
                    Gizmos.DrawLine(point.Position, connection.Position);
                }
            }
        }

        if(DrawPoints)
        {
            foreach (Point point in gridPoints)
            {
                if (point.isModifiable)
                {
                    Gizmos.color = Color.blue;
                }
                else
                {
                    Gizmos.color = Color.red;
                }

                Gizmos.DrawSphere(point.Position, 0.1f);
            }
        }

        if(DrawSubPoints)
        {
            foreach (Point point in subPoints)
            {
                if (point.isModifiable)
                {
                    Gizmos.color = Color.yellow;
                }
                else
                {
                    Gizmos.color = Color.red;
                }

                Gizmos.DrawSphere(point.Position, 0.1f);
            }
        }

        if(DrawBoundingLines)
        {
            Gizmos.color = Color.magenta;
            foreach (Point point in boundingPoints)
            {
                foreach (Point connection in point.Connections)
                {
                    Gizmos.DrawLine(point.Position, connection.Position);
                }
            }
        }
    }
}
