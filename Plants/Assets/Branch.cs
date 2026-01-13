using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;

public class Branch
{
    private Vector3 startPosition;
    private Vector3 startB;
    private Vector3 startN;
    private Vector3 endPosition;
    private Vector3 endB;
    private Vector3 endN;
    private float thickness;
    private int order;
    public Branch(Vector3 startPosition, Vector3 startB, Vector3 startN, Vector3 endPosition, Vector3 endB, Vector3 endN, float thickness, int order)
    {
        this.startPosition = startPosition;
        this.startB = startB;
        this.startN = startN;
        this.endPosition = endPosition;
        this.endB = endB;
        this.endN = endN;
        this.thickness = thickness;
        this.order = order;
    }


    public override String ToString()
    {
        return "Start: " + startPosition.ToString() + " End: " + endPosition.ToString() + " Thickness: " + thickness.ToString();
    }

    public Vector3 getStartPosition()
    {
        return startPosition;
    }
    public Vector3 getEndPosition()
    {
        return endPosition;
    }
    public float getThickness()
    {
        return thickness;
    }
    public Vector3 getStartB()
    {
        return startB;
    }
    public Vector3 getStartN()
    {
        return startN;
    }
    public Vector3 getEndB()
    {
        return endB;
    }
    public Vector3 getEndN()
    {
        return endN;
    }

    public void setThickness(float thickness)
    {
        this.thickness = thickness;
    }
    public int getOrder()
    {
        return order;
    }
}
