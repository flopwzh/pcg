using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Leaf
{
    private Vector3 position;
    private Vector3 direction;
    private Vector3 normal;

    public Leaf(Vector3 position, Vector3 direction, Vector3 normal)
    {
        this.position = position;
        this.direction = direction;
        this.normal = normal;
    }
    public Vector3 GetPosition()
    {
        return position;
    }
    public Vector3 GetDirection()
    {
        return direction;
    }
    public Vector3 GetNormal()
    {
        return normal;
    }
}
