using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tree
{
    // defines the main backing structure of the tree
    // start with a bud a grow out
    // we get things like growth chance, death chance, etc, from the buds
    // random growth is all done in this class
    private int cycles;
    private int initCycles;
    private List<List<Branch>> branches;
    private List<Bud> buds;
    private List<Leaf> leaves;
    private int currBranch;
    private bool init;

    private float[] o1params, o2params, o3params, o4params; // order 1, 2, 3, and 4 parameters; growth, death, branch, length, thickness, variation, branch angle 

    public Tree(int seed, int cycles, int initCycles, float[] o1params, float[] o2params, float[] o3params, float[] o4params)
    {
        Random.InitState(seed);
        this.cycles = cycles;
        this.initCycles = initCycles;
        branches = new List<List<Branch>>();
        currBranch = 0;
        buds = new List<Bud>();
        leaves = new List<Leaf>();
        this.o1params = o1params;
        this.o2params = o2params;
        this.o3params = o3params;
        this.o4params = o4params;
        init = true;
    }

    // initial growth based on tree type, to prevent branches to being too low
    private void initialGrowth()
    {
        Bud initialBud = new Bud(Vector3.zero, Vector3.up, Vector3.forward, Vector3.right, currBranch++, 1, o1params);
        buds.Add(initialBud);
        branches.Add(new List<Branch>()); // add initial branch list
        float rand = Random.Range(0f, 1f);
        for (int i = 0; i < initCycles; i++)
        {
            if (rand < initialBud.getGrowthChance())
            {
                // Debug.Log("bud vectors: T " + initialBud.GetT().ToString() + " B " + initialBud.GetB().ToString() + " N " + initialBud.GetN().ToString());
                growBud(initialBud);

            }
            // else do nothing, no death or branching in initial growth
        }
        init = false;
    }

    private void cycle()
    {
        for (int i = buds.Count - 1; i >= 0; i--)
        {
            Bud bud = buds[i];
            (float growthChance, float deathChance, float branchChance, float leafChance) = bud.getChances();


            // first check for a leaf
            float leafRand = Random.Range(0f, 1f);
            if (leafRand < leafChance)
            {
                // create leaf at bud position
                // first, we need a random 360 rotation composed of B and N
                Vector3 position = bud.GetPosition();
                Vector3 B = bud.GetB();
                Vector3 N = bud.GetN();
                float theta = Random.Range(0f, 360f) * Mathf.PI / 180f;
                Vector3 leafDir = Mathf.Cos(theta) * B + Mathf.Sin(theta) * N;
                leafDir.Normalize();
                leaves.Add(new Leaf(position, leafDir, bud.GetT()));
            }


            // check for growth or death
            float rand = Random.Range(0f, 1f);
            if (rand < growthChance)
            {
                growBud(bud);
                float branchRand = Random.Range(0f, 1f);
                while (branchRand < branchChance)
                {
                    createNewBranch(bud);
                    branchRand = Random.Range(0f, 1f);
                }
            }
            else if (rand < growthChance + deathChance)
            {
                buds.RemoveAt(i);
            }
            
            // otherwise do nothing
        }
    }

    // grow the bud out by creating a new branch and updating the bud position
    // the bud's direction is modified by the variation angle
    private void growBud(Bud bud)
    {
        Vector3 position = bud.GetPosition();
        Vector3 T = bud.GetT();
        Vector3 B = bud.GetB();
        Vector3 N = bud.GetN();
        float length = bud.getLength();
        float thickness = bud.getThickness();
        float variationAngle = bud.getVariation();

        // we will set the actual direction to be some variation of T based on variation angle
        Vector3 randomVector = Random.onUnitSphere;
        Vector3 axis = Vector3.Cross(T, randomVector);
        axis.Normalize();
        float randomAngle = Random.Range(-variationAngle, variationAngle);
        Quaternion rotation = Quaternion.AngleAxis(randomAngle, axis);
        Vector3 newDir = rotation * T;
        newDir.Normalize();

        // now take this new direction and bias it
        // note that if upBias is not zero, we will not bias to the side at all
        float sideBias = bud.GetSideBias();
        float upBias = bud.GetUpBias();
        if (!init)
        {
            if (upBias != 0f)
            {
                // Debug.Log("Applying up bias");
                // Debug.Log("Before: " + newDir.ToString());
                newDir = newDir + Vector3.up * upBias;
                newDir.Normalize();
                // Debug.Log("After: " + newDir.ToString());
            }
            else if (sideBias != 0f)
            {
                Vector3 sideDir = new Vector3(newDir.x, 0f, newDir.z);
                newDir = newDir + sideDir * sideBias;
                newDir.Normalize();
            }
        }
        

        // now we will create a new branch and change the bud position
        //update bud
        Vector3 newPosition = position + newDir * length;
        bud.SetPosition(newPosition);
        bud.SetT(newDir);
        // re-calculate B and N to be orthogonal to T
        Vector3 newB = Vector3.Cross(N, newDir);
        newB.Normalize();
        bud.SetB(newB);
        Vector3 newN = Vector3.Cross(newDir, newB);
        newN.Normalize();
        bud.SetN(newN);

        //create branch
        Branch newBranch = new Branch(position, B, N, newPosition, newB, newN, thickness, bud.getOrder());
        // add branch to list
        branches[bud.getBranch()].Add(newBranch);
    }

    // start a new branch from the given bud with angle based on branch angle parameter
    // note that this doesn't create a new Branch object, just a new bud to grow from
    private void createNewBranch(Bud parentBud)
    {
        // get parent bud info
        Vector3 position = parentBud.GetPosition();
        Vector3 T = parentBud.GetT();
        Vector3 B = parentBud.GetB();
        Vector3 N = parentBud.GetN();
        float branchAngle = parentBud.getBranchAngle();

        // calculate new direction based on branch angle
        Vector3 randomVector = Random.onUnitSphere;
        Vector3 axis = Vector3.Cross(T, randomVector);
        axis.Normalize();
        Quaternion rotation = Quaternion.AngleAxis(branchAngle, axis);
        Vector3 newDir = rotation * T;
        newDir.Normalize();
        Vector3 newB = Vector3.Cross(newDir, N);
        newB.Normalize();
        Vector3 newN = Vector3.Cross(newDir, newB);
        newN.Normalize();

        // determine order and set params accordingly
        int parentOrder = parentBud.getOrder();
        float[] orderParams;
        if (parentOrder == 1)
        {
            orderParams = o2params;
        }
        else if (parentOrder == 2)
        {
            orderParams = o3params;
        }
        else //if (parentOrder >= 3)
        {
            orderParams = o4params; // max order is 4
        }
        // create new bud and set a new branch list
        Bud newBud = new Bud(position, newDir, newB, newN, currBranch++, ++parentOrder, orderParams);
        buds.Add(newBud);
        branches.Add(new List<Branch>()); // add new branch list
    }

    public List<List<Branch>> startGrowth()
    {
        initialGrowth();
        for (int i = 0; i < cycles; i++)
        {
            cycle();
        }
        processThickness();
        return branches;
    }

    // Slightly tapers the thickness of the tree towards the tips
    // The smallest thickness that a branch will taper to is 0.1 bigger than the next order's thickness
    // For the final order, the smallest is 0.1
    // Ignore above, will just taper by constant amount
    private void processThickness()
    {
        for (int i = 0; i < branches.Count; i++)
        {
            List<Branch> branchList = branches[i];
            if (branchList.Count <= 1) // no point in tapering a 1 length branch list
                continue;
            float baseThickness = branchList[0].getThickness();
            float endThickness = baseThickness - 0.3f;
            if (endThickness < 0.1f)
                endThickness = 0.1f;
            for (int j = 0; j < branchList.Count; j++)
            {
                Branch branch = branchList[j];
                float t = (float)j / (branchList.Count - 1); // guaranteed not to divide by 0 due to earlier check of Count
                float newThickness = Mathf.Lerp(baseThickness, endThickness, t);
                branch.setThickness(newThickness);
            }
        }
    }

    public List<Leaf> getLeaves()
    {
        return leaves;
    }
}
