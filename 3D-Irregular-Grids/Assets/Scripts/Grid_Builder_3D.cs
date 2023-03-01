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
    }

    private void CreateConnectionsPyramid()
    {
        Debug.Log("AINT FINISHED YET");

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

    private void CreateSubPoints()//todo this only works on the xz plane
    {
        ShuffleGridPoints();

        foreach (Point start in gridPoints)
        {
            foreach (Point neighbor in start.Connections)
            {
                foreach (Point localLeft in neighbor.Connections)
                {
                    if (localLeft == start)
                    {
                        continue;
                    }

                    if (localLeft.Connections.Contains(start) && IsLeft(start.Position, neighbor.Position, localLeft.Position)
                        && Random.Range(0f, 1f) <= SUB_POINT_CHANCE)
                    {
                        Point newPoint = new Point((start.Position.x + neighbor.Position.x + localLeft.Position.x) / 3, (start.Position.y + neighbor.Position.y + localLeft.Position.y) / 3, (start.Position.z + neighbor.Position.z + localLeft.Position.z) / 3, (Random.Range(0, 1f) >= SUB_POINT_RIGIDITY_CHANCE));
                        subPoints.Add(newPoint);
                        newPoint.AddConnection(start, false);
                        newPoint.AddConnection(neighbor, false);
                        newPoint.AddConnection(localLeft, false);
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

            temp.ShuffleConnections();

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

    private bool IsLeft(Vector3 a, Vector3 b, Vector3 c)//todo this doesnt work in 3d
    {
        return ((b.x - a.x) * (c.z - a.z) - (b.z - a.z) * (c.x - a.x)) > 0;
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
    }
}