using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Point
{
    public static float COMPARISON_TOLERANCE = 0.1f;

    public Vector3 Position;
    public List<Point> Connections;
    public bool isModifiable = true;

    public Point(float x, float y, float z, bool modifiable = true)
    {
        Position = new Vector3(x, y, z);
        isModifiable = modifiable;
        Connections = new List<Point>();
    }

    public void AddConnection(Point other, bool mutual = true)
    {
        if(!this.Connections.Contains(other))
        {
            if(mutual)
            {
                this.Connections.Add(other);
                other.AddConnection(this, false);
            }
            else
            {
                this.Connections.Add(other);
            }
        }
    }

    public void RemoveConnection(Point other)
    {
        if(this.Connections.Contains(other))
        {
            this.Connections.Remove(other);
        }
    }

    public void RemoveConnectionsMutual(Point other)
    {
        RemoveConnection(other);
        other.RemoveConnection(this);
    }

    public void ShuffleConnections()
    {
        for (int i = 0; i < Connections.Count; i++)
        {
            Point temp = Connections[i];
            int randIndex = Random.Range(i, Connections.Count);

            Connections[i] = Connections[randIndex];
            Connections[randIndex] = temp;
        }
    }

    public void SolidifyConnections()
    {
        foreach(Point connection in Connections)
        {
            connection.AddConnection(this, false);
        }
    }

    public virtual bool Equals(Point other)
    {
        if(other.Position.x == this.Position.x
            && other.Position.y == this.Position.y
            && other.Position.z == this.Position.z)
        {
            return true;
        }

        return false;
    }

    public bool IsNearEnough(float x, float y, float z)
    {
        if(Mathf.Abs(this.Position.x - x) <= COMPARISON_TOLERANCE
            && Mathf.Abs(this.Position.y - y) <= COMPARISON_TOLERANCE
            && Mathf.Abs(this.Position.z - z) <= COMPARISON_TOLERANCE)
        {
            return true;
        }

        return false;
    }

    public bool IsNearEnough(Point other)
    {
        if (Mathf.Abs(this.Position.x - other.Position.x) <= COMPARISON_TOLERANCE
            && Mathf.Abs(this.Position.y - other.Position.y) <= COMPARISON_TOLERANCE
            && Mathf.Abs(this.Position.z - other.Position.z) <= COMPARISON_TOLERANCE)
        {
            return true;
        }

        return false;
    }
}
