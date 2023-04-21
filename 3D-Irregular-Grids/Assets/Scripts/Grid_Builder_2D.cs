using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Grid_Builder_2D : MonoBehaviour
{
    //master list of points
    private List<Point> gridPoints = new List<Point>();
    private List<Point> subPoints = new List<Point>();

    //control variables
    [SerializeField] private int HORIZONTAL_GRID_SIZE = 10;
    [SerializeField] private int VERTICAL_GRID_SIZE = 10;
    [SerializeField] private float DIAGONAL_FLIP_CHANCE = 0.35f;
    [SerializeField] private float SUB_POINT_CHANCE = 0.35f;
    [SerializeField] private float SUB_POINT_RIGIDITY_CHANCE = 0.35f;
    [SerializeField] private int MIN_CONNECTION_COUNT = 3;
    [SerializeField] private float MIN_POINT_DISTANCE = 0.35f;

    //utilites draw inspector buttons
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

    //clear and reset grid
    private void ClearGrid()
    {
        gridPoints.Clear();
    }

    //loop over space and create points
    //set corner points to immovable
    private void CreatePoints()
    {
        for(int x = 0; x < HORIZONTAL_GRID_SIZE; x++)
        {
            for(int y = 0; y < VERTICAL_GRID_SIZE; y++)
            {
                if((x == 0 || y == 0)
                    && (x == HORIZONTAL_GRID_SIZE - 1 || y == VERTICAL_GRID_SIZE - 1))
                {
                    gridPoints.Add(new Point(x, 0, y, false));
                }
                else if ((x == 0 || y == VERTICAL_GRID_SIZE - 1)
                    && (x == HORIZONTAL_GRID_SIZE - 1 || y == 0))
                {
                    gridPoints.Add(new Point(x, 0, y, false));
                }
                else
                {
                    gridPoints.Add(new Point(x, 0, y));
                }
            }
        }
    }

    //loop over grid and set up connection btwn points
    //randomly flip a percentage of diagonal connections
    private void CreateConnections()
    {
        List<Vector3> diagonalsToFlip = new List<Vector3>();

        for (int x = 0; x < HORIZONTAL_GRID_SIZE; x++)
        {
            for (int y = 0; y < VERTICAL_GRID_SIZE; y++)
            {
                Point point = GetPoint(x, 0, y);

                if(x == 0 || x == HORIZONTAL_GRID_SIZE - 1)
                {
                    if(y - 1 >= 0)
                    {
                        point.AddConnection(GetPoint(x, 0, y - 1));
                    }
                    if(y + 1 < VERTICAL_GRID_SIZE)
                    {
                        point.AddConnection(GetPoint(x, 0, y + 1));
                    }
                }

                if (y == 0 || y == VERTICAL_GRID_SIZE - 1)
                {
                    if (x - 1 >= 0)
                    {
                        point.AddConnection(GetPoint(x - 1, 0, y));
                    }
                    if (x + 1 < HORIZONTAL_GRID_SIZE)
                    {
                        point.AddConnection(GetPoint(x + 1, 0, y));
                    }
                }

                if(x + 1 < HORIZONTAL_GRID_SIZE)
                {
                    point.AddConnection(GetPoint(x + 1, 0, y));
                }

                if(x - 1 >= 0)
                {
                    point.AddConnection(GetPoint(x - 1, 0, y));
                }

                if (y + 1 < VERTICAL_GRID_SIZE)
                {
                    point.AddConnection(GetPoint(x, 0, y + 1));
                }

                if (y - 1 >= 0)
                {
                    point.AddConnection(GetPoint(x, 0, y - 1));
                }

                if(Random.Range(0f, 1f) <= DIAGONAL_FLIP_CHANCE)
                {
                    diagonalsToFlip.Add(new Vector3(x, 0, y));
                }
                else
                {
                    if (x + 1 < HORIZONTAL_GRID_SIZE && y + 1 < VERTICAL_GRID_SIZE)
                    {
                        point.AddConnection(GetPoint(x + 1, 0, y + 1));
                    }
                }
            }
        }

        foreach(Vector3 diagonal in diagonalsToFlip)
        {
            if (diagonal.x + 1 < HORIZONTAL_GRID_SIZE && diagonal.z + 1 < VERTICAL_GRID_SIZE)
            {
                /*
                 * get point at (x,y+1)
                 * add connection to point at (x+1,y)
                 */
                //Debug.Log("diagonal :: " + diagonal);
                GetPoint(diagonal.x, 0, diagonal.z + 1).AddConnection(GetPoint(diagonal.x + 1, 0, diagonal.z));
            }
        }
    }

    //create subpoints within existing grid cells
    private void CreateSubPoints()
    {
        ShuffleGridPoints();

        //for each point
        // for each neighbor of point
        //  for each local left neigbor of neighbor
        //   if that point is left of point and neighbor
        //    create new point, set up connections
        foreach(Point start in gridPoints)
        {
            foreach(Point neighbor in start.Connections)
            {
                foreach(Point localLeft in neighbor.Connections)
                {
                    if(localLeft == start)
                    {
                        continue;
                    }

                    if(localLeft.Connections.Contains(start) && IsLeft(start.Position, neighbor.Position, localLeft.Position)
                        && Random.Range(0f, 1f) <= SUB_POINT_CHANCE)
                    {
                        Point newPoint = new Point((start.Position.x + neighbor.Position.x + localLeft.Position.x) / 3, 0, (start.Position.z + neighbor.Position.z + localLeft.Position.z) / 3, (Random.Range(0, 1f) >= SUB_POINT_RIGIDITY_CHANCE));
                        subPoints.Add(newPoint);
                        newPoint.AddConnection(start, false);
                        newPoint.AddConnection(neighbor, false);
                        newPoint.AddConnection(localLeft, false);
                    }
                }
            }
        }

        //remove any potential duplicate subpoints
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

    //fold subpoints into master list
    private void AddSubPointConnections()
    {
        foreach(Point point in subPoints)
        {
            foreach(Point connection in point.Connections)
            {
                connection.AddConnection(point);
            }
        }

        gridPoints.AddRange(subPoints);
        subPoints.Clear();
    }

    //remove edges from points until threshold is reached
    private void RemoveRandomConnections()
    {
        ShuffleGridPoints();

        List<Point> connsToRemove = new List<Point>();

        //for each point
        // if point has more than minimum
        //  for each neighbor point
        //   if both points are movable & have more than minimum threshold
        //    remove connection btwn point and neighbor
        foreach(Point point in gridPoints)
        {
            if(point.Connections.Count <= MIN_CONNECTION_COUNT)
            {
                continue;
            }

            foreach(Point connection in point.Connections)
            {
                if(!point.isModifiable && !connection.isModifiable)
                {
                    continue;
                }

                if(point.Connections.Count - connsToRemove.Count > MIN_CONNECTION_COUNT
                    && connection.Connections.Count > MIN_CONNECTION_COUNT)
                {
                    connsToRemove.Add(connection);
                }
            }

            foreach(Point removed in connsToRemove)
            {
                point.RemoveConnectionsMutual(removed);
            }

            connsToRemove.Clear();

            if(point.Connections.Count < MIN_CONNECTION_COUNT)
            {
                Debug.LogError(point.Connections.Count);
            }
        }
    }

    //randomly move points around within master list
    private void ShuffleGridPoints()
    {
        for(int i = 0; i < gridPoints.Count; i++)
        {
            Point temp = gridPoints[i];
            int randIndex = Random.Range(i, gridPoints.Count);

            gridPoints[i] = gridPoints[randIndex];
            gridPoints[randIndex] = temp;
        }
    }

    //apply laplacian smoothing with min distance check
    //NEEDS TO BE APPLIED MULTIPLE TIMES
    private void RebalanceGrid()
    {
        foreach(Point point in gridPoints)
        {
            if(point.isModifiable)
            {
                Vector3 newPos = Vector3.zero;

                foreach(Point neighbor in point.Connections)
                {
                    newPos += neighbor.Position;
                }

                newPos = newPos / point.Connections.Count;

                point.Connections = point.Connections.OrderBy(x => Vector3.Distance(newPos, x.Position)).ToList();

                if(Vector3.Distance(newPos, point.Connections[0].Position) > MIN_POINT_DISTANCE)
                {
                    point.Position = newPos;
                }
            }
        }
    }

    private Point GetPoint(float x, float y, float z)
    {
        return gridPoints.Find(point => point.Position == new Vector3(x, y, z));
    }

    private bool IsLeft(Vector3 a, Vector3 b, Vector3 c)
    {
        return ((b.x - a.x) * (c.z - a.z) - (b.z - a.z) * (c.x - a.x)) > 0;
    }

    //unity specific method for drawing points/edges
    public void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        foreach(Point point in gridPoints)
        {
            foreach(Point connection in point.Connections)
            {
                Gizmos.DrawLine(point.Position, connection.Position);
            }
        }

        foreach(Point point in gridPoints)
        {
            if(point.isModifiable)
            {
                Gizmos.color = Color.blue;
            }
            else
            {
                Gizmos.color = Color.red;
            }

            Gizmos.DrawSphere(point.Position, 0.1f);
        }

        foreach(Point point in subPoints)
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
