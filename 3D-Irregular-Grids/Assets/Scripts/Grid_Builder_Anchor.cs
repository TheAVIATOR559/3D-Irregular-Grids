using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Grid_Builder_Anchor : MonoBehaviour
{
    [SerializeField] private List<Vector3> anchors = new List<Vector3>();

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
        Vector3 origin = new Vector3();

        foreach (Transform child in transform)
        {
            anchors.Add(new Vector3(child.position.x, child.position.y, child.position.z));
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
            origin += child.position;
        }

        origin = origin / transform.childCount;

        //sort points in counter clockwise order
        anchors.Sort(new ClockwiseComparer(origin));

        upperbound = new Vector3Int(Mathf.FloorToInt(knownUpperX), Mathf.FloorToInt(knownUpperY), Mathf.FloorToInt(knownUpperZ));
        lowerbound = new Vector3Int(Mathf.FloorToInt(knownLowerX), Mathf.FloorToInt(knownLowerY), Mathf.FloorToInt(knownLowerZ));

        for (int y = lowerbound.y; y <= upperbound.y; y++)
        {
            for (int x = lowerbound.x; x <= upperbound.x; x++)
            {
                for (int z = lowerbound.z; z <= upperbound.z; z++)
                {
                    //Debug.Log("adding point");
                    if(GetPoint(x,y,z) == null /* and point(x,y,z) is inside the 2d polygon defined by the anchors*/)
                    {
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
        Vector3 dir = Vector3.Cross(b - a, b - c);
        Vector3 norm = dir.normalized;
        return Vector3.Dot(dir, norm) > 0;
    }

    private float DistanceLineSegmentPoint(Vector3 a, Vector3 b, Vector3 p)
    {
        if (a == b)
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

        //if (DrawBoundingLines)
        //{
        //    Gizmos.color = Color.magenta;
        //    foreach (Point point in boundingPoints)
        //    {
        //        foreach (Point connection in point.Connections)
        //        {
        //            Gizmos.DrawLine(point.Position, connection.Position);
        //        }
        //    }
        //}
    }
}
