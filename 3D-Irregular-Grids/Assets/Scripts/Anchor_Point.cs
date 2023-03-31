using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Anchor_Point : MonoBehaviour
{
    public Vector3 Position;
    public List<Anchor_Point> Connections;

    public void Initialize()
    {
        Position = transform.position;
    }
}
