using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Grid_Builder_Anchor : MonoBehaviour
{
    [SerializeField] private List<Anchor_Point> anchors = new List<Anchor_Point>();

    private List<Point> gridPoints = new List<Point>();
    private List<Point> subPoints = new List<Point>();

    [SerializeField] private float SUB_POINT_CHANCE = 0.35f;
    [SerializeField] private float SUB_POINT_RIGIDITY_CHANCE = 0.35f;
    [SerializeField] private int MIN_CONNECTION_COUNT = 3;
    [SerializeField] private float MIN_POINT_DISTANCE = 0.35f;
    [SerializeField] private int ANCHOR_INTERIOR_CONNECTION_COUNT = 3;

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

    [SerializeField] private Vector3Int upperbound;
    [SerializeField] private Vector3Int lowerbound;

    [SerializeField] private List<Vector4> facePlanes = new List<Vector4>();
    
    private void ClearGrid()
    {
        anchors.Clear();
        gridPoints.Clear();
        subPoints.Clear();
        facePlanes.Clear();
    }

    private void CreatePoints()
    {
        float knownLowerX = float.MaxValue;
        float knownUpperX = float.MinValue;
        float knownLowerY = float.MaxValue;
        float knownUpperY = float.MinValue;
        float knownLowerZ = float.MaxValue;
        float knownUpperZ = float.MinValue;

        foreach (Transform child in transform)
        {
            anchors.Add(child.GetComponent<Anchor_Point>());
            child.GetComponent<Anchor_Point>().Initialize();
            gridPoints.Add(new Point(child.position.x, child.position.y, child.position.z, false));

            if (child.position.x < knownLowerX)
            {
                knownLowerX = child.position.x;
            }
            if (child.position.x > knownUpperX)
            {
                knownUpperX = child.position.x;
            }

            if (child.position.y < knownLowerY)
            {
                knownLowerY = child.position.y;
            }
            if (child.position.y > knownUpperY)
            {
                knownUpperY = child.position.y;
            }

            if (child.position.z < knownLowerZ)
            {
                knownLowerZ = child.position.z;
            }
            if (child.position.z > knownUpperZ)
            {
                knownUpperZ = child.position.z;
            }
        }

        GetPolyFaces();

        upperbound = new Vector3Int(Mathf.FloorToInt(knownUpperX), Mathf.FloorToInt(knownUpperY), Mathf.FloorToInt(knownUpperZ));
        lowerbound = new Vector3Int(Mathf.FloorToInt(knownLowerX), Mathf.FloorToInt(knownLowerY), Mathf.FloorToInt(knownLowerZ));

        for (int y = lowerbound.y; y <= upperbound.y; y++)
        {
            for (int x = lowerbound.x; x <= upperbound.x; x++)
            {
                for (int z = lowerbound.z; z <= upperbound.z; z++)
                {
                    //Debug.Log("attempting point creation");
                    if (GetPoint(x, y, z) == null && IsPointInPolygon(x, y, z))
                    {
                        //Debug.Log("point creation");
                        gridPoints.Add(new Point(x, y, z));
                    }
                }
            }
        }
    }

    private void CreateConnections()
    {
        for(int i = 0; i < anchors.Count; i++)
        {
            gridPoints[i].AddConnections(GetNearestPoints(gridPoints[i], ANCHOR_INTERIOR_CONNECTION_COUNT));
        }

        foreach(Point point in gridPoints)
        {
            Point neighbor = GetPoint(point, Direction.NORTH);

            if(neighbor != null)
            {
                point.AddConnection(neighbor);
            }

            neighbor = GetPoint(point, Direction.SOUTH);

            if (neighbor != null)
            {
                point.AddConnection(neighbor);
            }

            neighbor = GetPoint(point, Direction.EAST);

            if (neighbor != null)
            {
                point.AddConnection(neighbor);
            }

            neighbor = GetPoint(point, Direction.WEST);

            if (neighbor != null)
            {
                point.AddConnection(neighbor);
            }

            neighbor = GetPoint(point, Direction.UP);

            if (neighbor != null)
            {
                point.AddConnection(neighbor);
            }

            neighbor = GetPoint(point, Direction.DOWN);

            if (neighbor != null)
            {
                point.AddConnection(neighbor);
            }

            neighbor = GetPoint(point.Position.x+1, point.Position.y, point.Position.z+1);
            if(neighbor != null)
            {
                point.AddConnection(neighbor);
            }

            neighbor = GetPoint(point.Position.x + 1, point.Position.y + 1, point.Position.z);
            if (neighbor != null)
            {
                point.AddConnection(neighbor);
            }

            neighbor = GetPoint(point.Position.x, point.Position.y + 1, point.Position.z + 1);
            if (neighbor != null)
            {
                point.AddConnection(neighbor);
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
                if (subPointAdded)
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

    private enum Direction
    {
        NORTH,
        SOUTH,
        EAST,
        WEST,
        UP,
        DOWN
    }

    private Point GetPoint(Point center, Direction dir)
    {
        switch(dir)
        {
            case Direction.NORTH:
                return GetPoint(center.Position.x, center.Position.y, center.Position.z + 1);
            case Direction.SOUTH:
                return GetPoint(center.Position.x, center.Position.y, center.Position.z - 1);
            case Direction.EAST:
                return GetPoint(center.Position.x + 1, center.Position.y, center.Position.z);
            case Direction.WEST:
                return GetPoint(center.Position.x - 1, center.Position.y, center.Position.z);
            case Direction.UP:
                return GetPoint(center.Position.x, center.Position.y + 1, center.Position.z);
            case Direction.DOWN:
                return GetPoint(center.Position.x, center.Position.y - 1, center.Position.z);
            default:
                Debug.LogError("SWITCH FALLTHROUGH ERROR");
                return null;
        }
    }

    private bool SubPointExists(float x, float y, float z)
    {
        if (subPoints.Count > 0)
        {
            return (subPoints.Find(point => point.IsNearEnough(x, y, z)) != null);
        }

        return false;
    }

    private Point GetNearestPoint(Point core)
    {
        float distance = float.MaxValue;
        Point nearestPoint = null;
        foreach (Point point in gridPoints)
        {
            if (point == core)
            {
                continue;
            }

            if (Vector3.Distance(core.Position, point.Position) < distance)
            {
                distance = Vector3.Distance(core.Position, point.Position);
                nearestPoint = point;
            }
        }

        return nearestPoint;
    }

    private List<Point> GetNearestPoints(Point core, int n)
    {
        //sort points by distance from core
        //take the first n results

        //Debug.Log(gridPoints[0].Position);
        List<Point> temp = gridPoints.OrderBy(x => Vector3.Distance(core.Position, x.Position)).ToList();
        //Debug.Log(gridPoints[0].Position + "::" + temp[0].Position);
        return temp.Take(n).ToList();
    }

    private bool IsLeft(Vector3 a, Vector3 b, Vector3 c)
    {
        //Vector3 dir = Vector3.Cross(b - a, b - c);
        //Vector3 norm = dir.normalized;
        //return Vector3.Dot(dir, norm) > 0;
        Vector3 vec = b - a;
        Vector3 dir = c - a;

        Vector3 cross = Vector3.Cross(vec, dir);
        return cross.magnitude > 0f;
    }

    private bool IsPointInPolygon(float x, float y, float z)
    {
        Vector3 point = new Vector3(x, y, z);

        for(int i = 0; i < facePlanes.Count; i++)
        {
            float distance = GetPlaneDistance(facePlanes[i], point);

            if(distance > 0f)
            {
                return false;
            }
        }

        return true;
    }

    /// adapted from
    /// https://www.codeproject.com/Articles/1065730/Point-Inside-Convex-Polygon-in-Cplusplus
    private void GetPolyFaces()
    {
        int vertCount = anchors.Count;

        //x,y,z,w => a,b,c,d
        List<Vector4> faceOutwards = new List<Vector4>();

        List<List<int>> faceVerticesIndex = new List<List<int>>();

        for(int i = 0; i < vertCount; i++)
        {
            Vector3 p0 = anchors[i].Position;

            for(int j = i+1; j < vertCount; j++)
            {
                Vector3 p1 = anchors[j].Position;

                for(int k = j+1; k < vertCount; k++)
                {
                    Vector3 p2 = anchors[k].Position;

                    Vector4 trianglePlane = GetPlane(p0, p1, p2);

                    int onLeftCount = 0;
                    int onRightCount = 0;

                    List<int> pointsInSamePlane = new List<int>();

                    for(int l = 0; l < vertCount; l++)
                    {
                        if(l != i && l != j && l != k)
                        {
                            Vector3 point = anchors[l].Position;

                            float distance = GetPlaneDistance(trianglePlane, point);

                            if(distance == 0f)
                            {
                                pointsInSamePlane.Add(l);
                            }
                            else
                            {
                                if(distance < 0)
                                {
                                    onLeftCount++;
                                }
                                else
                                {
                                    onRightCount++;
                                }
                            }
                        }
                    }

                    if(onLeftCount == 0 || onRightCount == 0)
                    {
                        List<int> faceVertsInOneFace = new List<int>();

                        faceVertsInOneFace.Add(i);
                        faceVertsInOneFace.Add(j);
                        faceVertsInOneFace.Add(k);

                        for(int p = 0; p < pointsInSamePlane.Count; p++)
                        {
                            faceVertsInOneFace.Add(pointsInSamePlane[p]);
                        }

                        if(!ContainsList(faceVerticesIndex, faceVertsInOneFace))
                        {
                            faceVerticesIndex.Add(faceVertsInOneFace);

                            if(onRightCount == 0)
                            {
                                faceOutwards.Add(trianglePlane);
                            }
                            else if(onLeftCount == 0)
                            {
                                faceOutwards.Add(-trianglePlane);
                            }
                        }
                    }
                    else
                    {
                        Debug.LogWarning("POLYGON MAY NOT BE CONVEX");
                    }
                }
            }
        }

        for(int i = 0; i < faceVerticesIndex.Count; i++)
        {
            facePlanes.Add(new Vector4(faceOutwards[i].x, faceOutwards[i].y, faceOutwards[i].z, faceOutwards[i].w));

            List<Vector3> verts = new List<Vector3>();
            List<int> indices = new List<int>();
            int count = faceVerticesIndex[i].Count;

            for(int j = 0; j < count; j++)
            {
                indices.Add(faceVerticesIndex[i][j]);
                verts.Add(new Vector3(anchors[indices[j]].Position.x, anchors[indices[j]].Position.y, anchors[indices[j]].Position.z));
            }
        }
    }

    private Vector4 GetPlane(Vector3 p0, Vector3 p1, Vector3 p2)
    {
        Vector4 plane = Vector3.Cross(p2 - p0, p1 - p0);
        plane.w = -((plane.x * p0.x) + (plane.y * p0.y) + (plane.z * p0.z));
        return plane;
    }

    private float GetPlaneDistance(Vector4 plane, Vector3 point)
    {
        //point.x * plane.a + point.y * plane.b + point.z * plane.c + plane.d
        return ((point.x * plane.x) + (point.y * plane.y) + (point.z * plane.z) + plane.w);
    }

    [SerializeField] private bool DrawPoints = true;
    [SerializeField] private bool DrawConnections = true;
    [SerializeField] private bool DrawSubPoints = true;
    [SerializeField] private bool DrawBoundingLines = true;

    public void OnDrawGizmos()
    {
        if (DrawConnections)
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

        if (DrawPoints)
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

        if (DrawSubPoints)
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

        if (DrawBoundingLines)
        {
            Gizmos.color = Color.magenta;
            foreach (Anchor_Point point in anchors)
            {
                foreach (Anchor_Point connection in point.Connections)
                {
                    Gizmos.DrawLine(point.Position, connection.Position);
                }
            }
        }
    }

    private bool ContainsList(List<List<int>> list, List<int> item)
    {
        /* sort item
         * for each sub list in list
         * sort it
         * compare to item using linq sequenceequals
         */

        item.Sort();

        foreach(List<int> sublist in list)
        {
            sublist.Sort();

            if(sublist.SequenceEqual(item))
            {
                return true;
            }
        }

        return false;
    }
}