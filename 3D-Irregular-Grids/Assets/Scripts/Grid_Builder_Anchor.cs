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
    
    private void ClearGrid()
    {
        anchors.Clear();
        gridPoints.Clear();
        subPoints.Clear();
    }

    private void CreatePoints()
    {
        Debug.LogWarning("AINT DONE YET");

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

        upperbound = new Vector3Int(Mathf.FloorToInt(knownUpperX), Mathf.FloorToInt(knownUpperY), Mathf.FloorToInt(knownUpperZ));
        lowerbound = new Vector3Int(Mathf.FloorToInt(knownLowerX), Mathf.FloorToInt(knownLowerY), Mathf.FloorToInt(knownLowerZ));

        for (int y = lowerbound.y; y <= upperbound.y; y++)
        {
            for (int x = lowerbound.x; x <= upperbound.x; x++)
            {
                for (int z = lowerbound.z; z <= upperbound.z; z++)
                {
                    Debug.Log("attempting point creation");
                    if (GetPoint(x, y, z) == null && IsPointInPolygon(anchors, new Vector3(x,y,z)))
                    {
                        Debug.Log("point creation");
                        gridPoints.Add(new Point(x, y, z));
                    }
                }
            }
        }
    }

    private void CreateConnections()
    {
        Debug.LogWarning("AINT HERE YET");
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

    public bool IsPointInPolygon(List<Anchor_Point> polygonPoints, Vector3 point)
    {
        Vector3 xMax = polygonPoints[0].Position;

        foreach(Anchor_Point anchor in polygonPoints)
        {
            if(anchor.Position.x > xMax.x)
            {
                xMax = anchor.Position;
            }
        }

        Vector3 pointOutside = xMax + new Vector3(10f, 0, 0);

        Vector3 l1P1 = point;
        Vector3 l1P2 = pointOutside;

        int intersectionCount = 0;

        for (int i = 0; i < polygonPoints.Count; i++)
        {
            Vector3 l2P1 = polygonPoints[i].Position;

            int iPlusOne = ClampListIndex(i + 1, polygonPoints.Count);

            Vector3 l2P2 = polygonPoints[iPlusOne].Position;

            if (DoLinesIntersect(l1P1, l1P2, l2P1, l2P2, true))
            {
                intersectionCount += 1;
            }
        }

        bool isInside = true;

        if(intersectionCount == 0 || intersectionCount % 2 == 0)
        {
            isInside = false;
        }

        return isInside;
    }

    //provided by https://www.habrador.com/tutorials/math/9-useful-algorithms/
    private int ClampListIndex(int index, int listSize)
    {
        return ((index % listSize) + listSize) % listSize;
    }

    //adapted from http://thirdpartyninjas.com/blog/2008/10/07/line-segment-intersection/
    private bool DoLinesIntersect(Vector3 l1p1, Vector3 l1p2, Vector3 l2p1, Vector3 l2p2, bool includeEndPoints)//todo adapt to 3d see idea for adaptation
    {
        bool isIntersecting = false;

        float denom = (l2p2.y - l2p1.y) * (l1p2.x - l1p1.x) - (l2p2.x - l2p1.x) * (l1p2.y - l1p1.y);

        if(denom != 0f)
        {
            float uA = ((l2p2.x - l2p1.x) * (l1p1.y - l2p1.y) - (l2p2.y - l2p1.y) * (l1p1.x - l2p1.x)) / denom;
            float uB = ((l1p2.x - l1p1.x) * (l1p1.y - l2p1.y) - (l1p2.y - l1p1.y) * (l1p1.x - l2p1.x)) / denom;

            if(includeEndPoints)
            {
                if(uA >= 0f && uA <= 1f && uB >= 0f && uB <= 1f)
                {
                    isIntersecting = true;
                }
            }
            else
            {
                if (uA > 0f && uA < 1f && uB > 0f && uB < 1f)
                {
                    isIntersecting = true;
                }
            }
        }

        return isIntersecting;
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
}

//// IDEA FOR ADAPTATION
/// get faces from anchors, get their inward normals(use centroid to verify)
///     for each proposed point, if magnitude between point and each face matches the sign of the inward normal or is 0
///         add point
/// reference
/// https://www.codeproject.com/Articles/1065730/Point-Inside-Convex-Polygon-in-Cplusplus