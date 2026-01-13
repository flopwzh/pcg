using System.Collections;
using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;

public class Bud
{

    private Vector3 position;
    private Vector3 T;
    private Vector3 B;
    private Vector3 N;
    private float growthChance;
    private float deathChance;
    private float branchChance;
    private float leafChance;
    private float length;
    private float thickness;
    private float variation;
    private float branchAngle;
    private float upBias;
    private float sideBias;
    private int branch;
    private int order;

    public Bud(Vector3 position, Vector3 T, Vector3 B, Vector3 N, int branch, int order, float[] parameters)
    {
        this.position = position;
        this.T = T;
        this.B = B;
        this.N = N;
        this.branch = branch;
        this.order = order;
        this.T.Normalize();
        this.B.Normalize();
        this.N.Normalize();
        growthChance = parameters[0];
        deathChance = parameters[1];
        branchChance = parameters[2];
        leafChance = parameters[3];
        length = parameters[4];
        thickness = parameters[5];
        variation = parameters[6];
        branchAngle = parameters[7];
        upBias = parameters[8];
        sideBias = parameters[9];
    }

    public Vector3 GetPosition()
    {
        return position;
    }
    public Vector3 GetT()
    {
        return T;
    }
    public Vector3 GetB()
    {
        return B;
    }
    public Vector3 GetN()
    {
        return N;
    }

    public void SetPosition(Vector3 position)
    {
        this.position = position;
    }
    public void SetT(Vector3 T)
    {
        this.T = T;
        this.T.Normalize();
    }
    public void SetB(Vector3 B)
    {
        this.B = B;
        this.B.Normalize();
    }
    public void SetN(Vector3 N)
    {
        this.N = N;
        this.N.Normalize();
    }

    public (float, float, float, float) getChances()
    {
        return (growthChance, deathChance, branchChance, leafChance);
    }

    public float getGrowthChance()
    {
        return growthChance;
    }
    public float getVariation()
    {
        return variation;
    }
    public float getLength()
    {
        return length;
    }
    public float getThickness()
    {
        return thickness;
    }
    public int getBranch()
    {
        return branch;
    }
    public float getBranchAngle()
    {
        return branchAngle;
    }
    public int getOrder()
    {
        return order;
    }
    public float GetUpBias()
    {
        return upBias;
    }
    public float GetSideBias()
    {
        return sideBias;
    }
}
