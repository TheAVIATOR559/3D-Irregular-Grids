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

    private float sin0;
    private float sin60;
    private float sin120;
    private float sin180;
    private float sin240;
    private float sin300;

    private float cos0;
    private float cos60;
    private float cos120;
    private float cos180;
    private float cos240;
    private float cos300;

    private void ClearGrid()
    {
        gridPoints.Clear();
        subPoints.Clear();
        boundingPoints.Clear();
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
                CreateHexagonalPrism();
                break;
            case Shape.OTCAHEDRON:
                CreateOctahedron();
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
        /* corner points
         * (0, 0, 0)
         * (GRID_LENGTH, 0, 0)
         * (GRID_LENGTH / 2f, 0, GRID_DEPTH * (Mathf.Sqrt(3f) / 2f))
         * ((boundingPoints[0].Position.x + boundingPoints[1].Position.x + boundingPoints[2].Position.x) / 3f,
            Mathf.Sqrt(2f / 3f) * GRID_HEIGHT, (boundingPoints[0].Position.z + boundingPoints[1].Position.z + boundingPoints[2].Position.z) / 3f)
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
        for (int y = 0; y < height-1; y++)
        {
            for (int x = 0; x < GRID_LENGTH; x++)
            {
                for (int z = 0; z < GRID_DEPTH; z++)
                {
                    if(x==0 && z==0)
                    {
                        continue;
                    }
                    Vector3 proposedPoint = new Vector3(x, y, z);

                    if (/*IsPointInTetrahedron(gridPoints[0].Position, gridPoints[1].Position, gridPoints[2].Position, proposedPoint)
                        &&*/ IsPointInTetrahedron(gridPoints[0].Position, gridPoints[1].Position, gridPoints[3].Position, proposedPoint)
                        && IsPointInTetrahedron(gridPoints[1].Position, gridPoints[2].Position, gridPoints[3].Position, proposedPoint)
                        && IsPointInTetrahedron(gridPoints[2].Position, gridPoints[0].Position, gridPoints[3].Position, proposedPoint))
                    {
                        //Debug.Log(proposedPoint);
                        gridPoints.Add(new Point(x, y, z));
                    }
                }
            }
        }
    }

    /* ax + by + cz + d = 0
     * -d = ax + by + cz
     * d = -(ax + by + cz)
     */

    private bool IsPointInTetrahedron(Vector3 a, Vector3 b, Vector3 c, Vector3 p)
    {
        Vector3 normal = Vector3.Cross(c - a, b - a);
        float k = -((normal.x * c.x) + (normal.y * c.y) + (normal.z * c.z));
        //Debug.Log(k + "=-(" + normal.x + "*" + c.x + "+" + normal.y + "*" + c.y + "+" + normal.z + "*" + c.z);
        float result = (normal.x * p.x) + (normal.y * p.y) + (normal.z * p.z) + k;
        //Debug.Log(result + "=" + (normal.x * p.x) + "+" + (normal.y * p.y) + "+" + (normal.z * p.z) + "+" + k + "::" + normal);
        return result <= 0f;
    }

    private void CreateHexagonalPrism()
    {
        sin0 = Mathf.Sin(0);
        sin60 = Mathf.Sin(1.0472f);
        sin120 = Mathf.Sin(2.0994f);
        sin180 = Mathf.Sin(3.14159f);
        sin240 = Mathf.Sin(4.18879f);
        sin300 = Mathf.Sin(5.23599f);

        cos0 = Mathf.Cos(0);
        cos60 = Mathf.Cos(1.0472f);
        cos120 = Mathf.Cos(2.0994f);
        cos180 = Mathf.Cos(3.14159f);
        cos240 = Mathf.Cos(4.18879f);
        cos300 = Mathf.Cos(5.23599f);

        List<Point> mainPoints = new List<Point>();
        List<Point> secondaryPoints = new List<Point>();

        boundingPoints.Add(new Point((GRID_LENGTH / 2) * sin0, 0, (GRID_LENGTH / 2) * cos0, false));
        boundingPoints.Add(new Point((GRID_LENGTH / 2) * sin60, 0, (GRID_LENGTH / 2) * cos60, false));
        boundingPoints.Add(new Point((GRID_LENGTH / 2) * sin120, 0, (GRID_LENGTH / 2) * cos120, false));
        boundingPoints.Add(new Point((GRID_LENGTH / 2) * sin180, 0, (GRID_LENGTH / 2) * cos180, false));
        boundingPoints.Add(new Point((GRID_LENGTH / 2) * sin240, 0, (GRID_LENGTH / 2) * cos240, false));
        boundingPoints.Add(new Point((GRID_LENGTH / 2) * sin300, 0, (GRID_LENGTH / 2) * cos300, false));

        boundingPoints.Add(new Point((GRID_LENGTH / 2) * sin0, GRID_HEIGHT-1, (GRID_LENGTH / 2) * cos0, false));
        boundingPoints.Add(new Point((GRID_LENGTH / 2) * sin60, GRID_HEIGHT - 1, (GRID_LENGTH / 2) * cos60, false));
        boundingPoints.Add(new Point((GRID_LENGTH / 2) * sin120, GRID_HEIGHT - 1, (GRID_LENGTH / 2) * cos120, false));
        boundingPoints.Add(new Point((GRID_LENGTH / 2) * sin180, GRID_HEIGHT - 1, (GRID_LENGTH / 2) * cos180, false));
        boundingPoints.Add(new Point((GRID_LENGTH / 2) * sin240, GRID_HEIGHT - 1, (GRID_LENGTH / 2) * cos240, false));
        boundingPoints.Add(new Point((GRID_LENGTH / 2) * sin300, GRID_HEIGHT - 1, (GRID_LENGTH / 2) * cos300, false));

        boundingPoints[0].AddConnection(boundingPoints[1]);
        boundingPoints[1].AddConnection(boundingPoints[2]);
        boundingPoints[2].AddConnection(boundingPoints[3]);
        boundingPoints[3].AddConnection(boundingPoints[4]);
        boundingPoints[4].AddConnection(boundingPoints[5]);
        boundingPoints[5].AddConnection(boundingPoints[0]);

        boundingPoints[6].AddConnection(boundingPoints[7]);
        boundingPoints[7].AddConnection(boundingPoints[8]);
        boundingPoints[8].AddConnection(boundingPoints[9]);
        boundingPoints[9].AddConnection(boundingPoints[10]);
        boundingPoints[10].AddConnection(boundingPoints[11]);
        boundingPoints[11].AddConnection(boundingPoints[6]);

        boundingPoints[0].AddConnection(boundingPoints[6]);
        boundingPoints[1].AddConnection(boundingPoints[7]);
        boundingPoints[2].AddConnection(boundingPoints[8]);
        boundingPoints[3].AddConnection(boundingPoints[9]);
        boundingPoints[4].AddConnection(boundingPoints[10]);
        boundingPoints[5].AddConnection(boundingPoints[11]);

        for(int y = 0; y <GRID_HEIGHT; y++)
        {
            for (int i = 1; i <= GRID_LENGTH / 2; i++)
            {
                if (i == GRID_LENGTH / 2 && (y == 0 || y == GRID_HEIGHT-1))
                {
                    mainPoints.Add(new Point(i * sin0, y, i * cos0, false));
                    mainPoints.Add(new Point(i * sin60, y, i * cos60, false));
                    mainPoints.Add(new Point(i * sin120, y, i * cos120, false));
                    mainPoints.Add(new Point(i * sin180, y, i * cos180, false));
                    mainPoints.Add(new Point(i * sin240, y, i * cos240, false));
                    mainPoints.Add(new Point(i * sin300, y, i * cos300, false));
                }
                else
                {
                    mainPoints.Add(new Point(i * sin0, y, i * cos0));
                    mainPoints.Add(new Point(i * sin60, y, i * cos60));
                    mainPoints.Add(new Point(i * sin120, y, i * cos120));
                    mainPoints.Add(new Point(i * sin180, y, i * cos180));
                    mainPoints.Add(new Point(i * sin240, y, i * cos240));
                    mainPoints.Add(new Point(i * sin300, y, i * cos300));
                }

                if (i < 2)
                {
                    continue;
                }

                for (int k = (i * 6) - 5; k <= i * 6; k++)
                {
                    for (float j = 1; j < i; j++)
                    {
                        //(x1 + k(x2 - x1), y1 + k(y2 - y1))
                        //Debug.Log(k + "::" + j + "/" + i + "=" + (j / i));
                        if (k == (i * 6))
                        {
                            if (i == 6)
                            {
                                //Debug.Log(k + "::" + mainPoints[k-1]);
                                secondaryPoints.Add(new Point(mainPoints[k - 1].Position.x + (j / i) * (mainPoints[k - 6].Position.x - mainPoints[k - 1].Position.x), y, mainPoints[k - 1].Position.z + (j / i) * (mainPoints[k - 6].Position.z - mainPoints[k - 1].Position.z), false));
                            }
                            else
                            {
                                //Debug.Log(k + "::" + mainPoints[k-1]);
                                secondaryPoints.Add(new Point(mainPoints[k - 1].Position.x + (j / i) * (mainPoints[k - 6].Position.x - mainPoints[k - 1].Position.x), y, mainPoints[k - 1].Position.z + (j / i) * (mainPoints[k - 6].Position.z - mainPoints[k - 1].Position.z)));
                            }
                        }
                        else
                        {
                            if (i == 6)
                            {
                                secondaryPoints.Add(new Point(mainPoints[k - 1].Position.x + (j / i) * (mainPoints[k].Position.x - mainPoints[k - 1].Position.x), y, mainPoints[k - 1].Position.z + (j / i) * (mainPoints[k].Position.z - mainPoints[k - 1].Position.z), false));
                            }
                            else
                            {
                                secondaryPoints.Add(new Point(mainPoints[k - 1].Position.x + (j / i) * (mainPoints[k].Position.x - mainPoints[k - 1].Position.x), y, mainPoints[k - 1].Position.z + (j / i) * (mainPoints[k].Position.z - mainPoints[k - 1].Position.z)));
                            }
                        }
                    }
                }
            }

            gridPoints.Add(new Point(0, y, 0));
        }
        
        gridPoints.AddRange(mainPoints);
        gridPoints.AddRange(secondaryPoints);
    }

    private void CreateOctahedron()
    {
        /* corner points
         * (0,0,0)
         * (max, 0, 0)
         * (max, 0, max)
         * (0, 0, max)
         * (max/2, height, max/2)
         * (max/2, -height, max/2)
         */

        gridPoints.Add(new Point(0, 0, 0, false));
        gridPoints.Add(new Point(GRID_LENGTH - 1, 0, 0, false));
        gridPoints.Add(new Point(GRID_LENGTH - 1, 0, GRID_DEPTH - 1, false));
        gridPoints.Add(new Point(0, 0, GRID_DEPTH - 1, false));
        gridPoints.Add(new Point((GRID_LENGTH - 1) / 2f, (GRID_HEIGHT - 1) / 2f, (GRID_DEPTH - 1) / 2f, false));
        gridPoints.Add(new Point((GRID_LENGTH - 1) / 2f, -(GRID_HEIGHT - 1) / 2f, (GRID_DEPTH - 1) / 2f, false));

        boundingPoints.Add(new Point(0, 0, 0, false));
        boundingPoints.Add(new Point(GRID_LENGTH - 1, 0, 0, false));
        boundingPoints.Add(new Point(GRID_LENGTH - 1, 0, GRID_DEPTH - 1, false));
        boundingPoints.Add(new Point(0, 0, GRID_DEPTH - 1, false));
        boundingPoints.Add(new Point((GRID_LENGTH - 1) / 2f, (GRID_HEIGHT - 1) / 2f, (GRID_DEPTH - 1) / 2f, false));
        boundingPoints.Add(new Point((GRID_LENGTH - 1) / 2f, -(GRID_HEIGHT - 1) / 2f, (GRID_DEPTH - 1) / 2f, false));

        boundingPoints[0].AddConnection(boundingPoints[1]);
        boundingPoints[0].AddConnection(boundingPoints[3]);
        boundingPoints[0].AddConnection(boundingPoints[4]);
        boundingPoints[0].AddConnection(boundingPoints[5]);

        boundingPoints[1].AddConnection(boundingPoints[2]);
        boundingPoints[1].AddConnection(boundingPoints[4]);
        boundingPoints[1].AddConnection(boundingPoints[5]);

        boundingPoints[2].AddConnection(boundingPoints[3]);
        boundingPoints[2].AddConnection(boundingPoints[4]);
        boundingPoints[2].AddConnection(boundingPoints[5]);

        boundingPoints[3].AddConnection(boundingPoints[4]);
        boundingPoints[3].AddConnection(boundingPoints[5]);

        for (int y = 0; y < GRID_HEIGHT; y++)
        {
            int length = GRID_LENGTH - y;
            int depth = GRID_DEPTH - y;
            for (int x = y; x < length; x++)
            {
                for (int z = y; z < depth; z++)
                {
                    Point temp = new Point(x, y, z);
                    if (gridPoints.Find(i => i.Position == temp.Position) == null)
                    {
                        gridPoints.Add(temp);
                    }
                }
            }
        }

        for (int y = 0; y < GRID_HEIGHT; y++)
        {
            int length = GRID_LENGTH - y;
            int depth = GRID_DEPTH - y;
            for (int x = y; x < length; x++)
            {
                for (int z = y; z < depth; z++)
                {
                    Point temp = new Point(x, -y, z);
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
                CreateConnectionsTetrahedron();
                break;
            case Shape.HEXAGONAL_PRISM:
                CreateConnectionsHexagonalPrism();
                break;
            case Shape.OTCAHEDRON:
                CreateConnectionsOctahedron();
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

    private void CreateConnectionsOctahedron()
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

                    if (x == y)
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
                    if (y + 1 <= GRID_HEIGHT && GetPoint(x, y + 1, z) != null)
                    {
                        point.AddConnection(GetPoint(x, y + 1, z));
                    }

                    //outer edge diagonal connections
                    if (x == y && z == y && GetPoint(x + 1, y + 1, z + 1) != null)
                    {
                        point.AddConnection(GetPoint(x + 1, y + 1, z + 1));
                    }

                    if (x == length - 1 && z == y && GetPoint(x - 1, y + 1, z + 1) != null)
                    {
                        point.AddConnection(GetPoint(x - 1, y + 1, z + 1));
                    }

                    if (x == y && z == depth - 1 && GetPoint(x + 1, y + 1, z - 1) != null)
                    {
                        point.AddConnection(GetPoint(x + 1, y + 1, z - 1));
                    }

                    if (x == length - 1 && z == depth - 1 && GetPoint(x - 1, y + 1, z - 1) != null)
                    {
                        point.AddConnection(GetPoint(x - 1, y + 1, z - 1));
                    }

                    //apex connections
                    //Debug.Log(y + " " + length + " " + (y + 1 == length - 1) + " :: " + (GetPoint((GRID_LENGTH - 1) / 2f, (GRID_HEIGHT - 1) / 2f, (GRID_DEPTH - 1) / 2f) != null));
                    if (y + 1 == length - 1 && GetPoint((GRID_LENGTH - 1) / 2f, (GRID_HEIGHT - 1) / 2f, (GRID_DEPTH - 1) / 2f) != null)
                    {
                        point.AddConnection(GetPoint((GRID_LENGTH - 1) / 2f, (GRID_HEIGHT - 1) / 2f, (GRID_DEPTH - 1) / 2f));
                    }
                }
            }

        }

        for (int y = 0; y > -GRID_HEIGHT; y--)
        {
            int length = GRID_LENGTH + y;
            int depth = GRID_DEPTH + y;

            //Debug.Log(y + " " + length + " " + depth);

            if (y == length && y == depth)
            {
                break;
            }

            int absY = Mathf.Abs(y);
            //Debug.Log(y + "::" + absY);
            for (int x = absY; x < length; x++)
            {
                for (int z = absY; z < depth; z++)
                {
                    //Debug.Log(x + " " + y + " " + z);
                    Point point = GetPoint(x, y, z);

                    if (x == absY)
                    {
                        if (z - 1 >= absY)
                        {
                            point.AddConnection(GetPoint(x, y, z - 1));
                        }
                        if (z + 1 < depth)
                        {
                            point.AddConnection(GetPoint(x, y, z + 1));
                        }

                        if (y + 1 <= 0)
                        {
                            point.AddConnection(GetPoint(x - 1, y + 1, z));
                        }
                    }
                    else if (x == length - 1)
                    {
                        if (z - 1 >= absY)
                        {
                            point.AddConnection(GetPoint(x, y, z - 1));
                        }
                        if (z + 1 < depth)
                        {
                            point.AddConnection(GetPoint(x, y, z + 1));
                        }

                        if (y + 1 <= 0)
                        {
                            point.AddConnection(GetPoint(x + 1, y + 1, z));
                        }
                    }

                    if (z == absY)
                    {
                        if (x - 1 >= absY)
                        {
                            point.AddConnection(GetPoint(x - 1, y, z));
                        }
                        if (x + 1 < length)
                        {
                            point.AddConnection(GetPoint(x + 1, y, z));
                        }

                        if (y + 1 <= 0)
                        {
                            point.AddConnection(GetPoint(x, y + 1, z - 1));
                        }
                    }
                    else if (z == depth - 1)
                    {
                        if (x - 1 >= absY)
                        {
                            point.AddConnection(GetPoint(x - 1, y, z));
                        }
                        if (x + 1 < length)
                        {
                            point.AddConnection(GetPoint(x + 1, y, z));
                        }

                        if (y + 1 <= 0)
                        {
                            point.AddConnection(GetPoint(x, y + 1, z + 1));
                        }
                    }

                    if (x + 1 < length)
                    {
                        point.AddConnection(GetPoint(x + 1, y, z));
                    }
                    if (x - 1 >= absY)
                    {
                        point.AddConnection(GetPoint(x - 1, y, z));
                    }

                    if (z + 1 < depth)
                    {
                        point.AddConnection(GetPoint(x, y, z + 1));
                    }
                    if (z - 1 >= absY)
                    {
                        point.AddConnection(GetPoint(x, y, z - 1));
                    }

                    //diagonals
                    if (x + 1 < length && y - 1 > -GRID_HEIGHT && GetPoint(x + 1, y - 1, z) != null)
                    {
                        point.AddConnection(GetPoint(x + 1, y - 1, z));
                    }

                    if (x + 1 < length && z + 1 < depth)
                    {
                        point.AddConnection(GetPoint(x + 1, y, z + 1));
                    }

                    if (z + 1 < depth && y - 1 > -GRID_HEIGHT && GetPoint(x, y - 1, z + 1) != null)
                    {
                        point.AddConnection(GetPoint(x, y - 1, z + 1));
                    }

                    //outer edge diagonal connections
                    if (x == absY && z == absY && GetPoint(x + 1, y - 1, z + 1) != null)
                    {
                        point.AddConnection(GetPoint(x + 1, y - 1, z + 1));
                    }

                    if (x == length - 1 && z == absY && GetPoint(x - 1, y - 1, z + 1) != null)
                    {
                        point.AddConnection(GetPoint(x - 1, y - 1, z + 1));
                    }

                    if (x == absY && z == depth - 1 && GetPoint(x + 1, y - 1, z - 1) != null)
                    {
                        point.AddConnection(GetPoint(x + 1, y - 1, z - 1));
                    }

                    if (x == length - 1 && z == depth - 1 && GetPoint(x - 1, y - 1, z - 1) != null)
                    {
                        point.AddConnection(GetPoint(x - 1, y - 1, z - 1));
                    }

                    //apex connections
                    //Debug.Log(y + " " + length + " " + (y + 1 == length - 1) + " :: " + (GetPoint((GRID_LENGTH - 1) / 2f, (GRID_HEIGHT - 1) / 2f, (GRID_DEPTH - 1) / 2f) != null));
                    if (absY + 1 == length - 1 && GetPoint((GRID_LENGTH - 1) / 2f, -(GRID_HEIGHT - 1) / 2f, (GRID_DEPTH - 1) / 2f) != null)
                    {
                        point.AddConnection(GetPoint((GRID_LENGTH - 1) / 2f, -(GRID_HEIGHT - 1) / 2f, (GRID_DEPTH - 1) / 2f));
                    }
                }
            }
        }
    }

    private void CreateConnectionsHexagonalPrism()
    {
        //List<Vector3> offsets = new List<Vector3>()
        //{
        //    //new Vector3(0, 1, 0),
        //    new Vector3(sin0, 0, cos0),
        //    //new Vector3(sin0, 1, cos0),
        //    new Vector3(sin60, 0, cos60),
        //    new Vector3(sin120, 0, cos120),
        //    new Vector3(sin180, 0, cos180),
        //    new Vector3(sin240, 0, cos240),
        //    new Vector3(sin300, 0, cos300),
        //};

        foreach (Point point in gridPoints)
        {
            //foreach(Vector3 offset in offsets)
            //{

            //    Point connection = GetPoint(point.Position.x + offset.x, point.Position.z + offset.z, point.Position.x + offset.z);
            //    Debug.Log(point.Position + "::" + offset + "::" + (point.Position + offset) + "::" + connection);
            //    if (connection != null)
            //    {
            //        point.AddConnection(connection);
            //    }
            //}

            for (int i = 0; i < gridPoints.Count; i++)
            {
                if (gridPoints[i] == point)
                {
                    continue;
                }

                if (gridPoints[i].IsNearEnough(point.Position.x, point.Position.y + 1, point.Position.z))
                {
                    point.AddConnection(gridPoints[i]);
                }
                else if (gridPoints[i].IsNearEnough(point.Position.x + sin0, point.Position.y, point.Position.z + cos0))
                {
                    point.AddConnection(gridPoints[i]);
                }
                else if (gridPoints[i].IsNearEnough(point.Position.x + sin0, point.Position.y + 1, point.Position.z + cos0))
                {
                    point.AddConnection(gridPoints[i]);
                }
                else if (gridPoints[i].IsNearEnough(point.Position.x + sin60, point.Position.y, point.Position.z + cos60))
                {
                    point.AddConnection(gridPoints[i]);
                }
                else if (gridPoints[i].IsNearEnough(point.Position.x + sin60, point.Position.y + 1, point.Position.z + cos60))
                {
                    point.AddConnection(gridPoints[i]);
                }
                else if (gridPoints[i].IsNearEnough(point.Position.x + sin300, point.Position.y, point.Position.z + cos300))
                {
                    point.AddConnection(gridPoints[i]);
                }
                else if (gridPoints[i].IsNearEnough(point.Position.x + sin300, point.Position.y + 1, point.Position.z + cos300))
                {
                    point.AddConnection(gridPoints[i]);
                }
            }
        }
    }

    private void CreateConnectionsTetrahedron()
    {
        Point connection = null;
        foreach (Point point in gridPoints)
        {
            connection = GetPoint(point.Position.x + 1, point.Position.y, point.Position.z);
            if(connection != null)
            {
                point.AddConnection(connection);
            }

            connection = GetPoint(point.Position.x, point.Position.y, point.Position.z + 1);
            if (connection != null)
            {
                point.AddConnection(connection);
            }

            connection = GetPoint(point.Position.x, point.Position.y + 1, point.Position.z);
            if (connection != null)
            {
                point.AddConnection(connection);
            }

            connection = GetPoint(point.Position.x + 1, point.Position.y, point.Position.z + 1);
            if (connection != null)
            {
                point.AddConnection(connection);
            }

            connection = GetPoint(point.Position.x + 1, point.Position.y + 1, point.Position.z);
            if (connection != null)
            {
                point.AddConnection(connection);
            }

            connection = GetPoint(point.Position.x, point.Position.y + 1, point.Position.z + 1);
            if (connection != null)
            {
                point.AddConnection(connection);
            }

            connection = GetPoint(point.Position.x, point.Position.y, point.Position.z + 1);
            Point connection2 = GetPoint(point.Position.x, point.Position.y + 1, point.Position.z);
            Point connection3 = GetPoint(point.Position.x, point.Position.y + 1, point.Position.z + 1);
            if (connection3 == null && connection != null && connection2 != null)
            {
                connection.AddConnection(connection2);
            }

            connection = GetPoint(point.Position.x + 1, point.Position.y, point.Position.z);
            connection2 = GetPoint(point.Position.x, point.Position.y + 1, point.Position.z);
            connection3 = GetPoint(point.Position.x + 1, point.Position.y + 1, point.Position.z);
            if (connection3 == null && connection != null && connection2 != null)
            {
                connection.AddConnection(connection2);
            }

            connection = GetPoint(point.Position.x + 1, point.Position.y, point.Position.z);
            connection2 = GetPoint(point.Position.x, point.Position.y, point.Position.z + 1);
            connection3 = GetPoint(point.Position.x + 1, point.Position.y, point.Position.z + 1);
            if (connection3 == null && connection != null && connection2 != null)
            {
                connection.AddConnection(connection2);
            }
        }

        

        connection = GetNearestPoint(gridPoints[3]);
        if(connection != null)
        {
            gridPoints[3].AddConnection(connection);
        }

        connection = null;
        connection = GetNearestPoint(gridPoints[2]);
        if (connection != null)
        {
            gridPoints[2].AddConnection(connection);
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

    private Point GetNearestPoint(Point core)
    {
        float distance = float.MaxValue;
        Point nearestPoint = null;
        foreach(Point point in gridPoints)
        {
            if(point == core)
            {
                continue;
            }

            if(Vector3.Distance(core.Position, point.Position) < distance)
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
