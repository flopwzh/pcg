using System;
using System.Collections.Generic;
using UnityEngine;

public class BuildTree : MonoBehaviour
{
    public int seed;
    public int numQuads = 8;
    public int treeType = 0;
    public int initCycles = 3;
    public int cycles = 10;
    public float o1UpBias = 0.1f;
    public float o1SideBias = 0f;
    public float o2UpBias = 0.1f;
    public float o2SideBias = 0f;
    public float o3UpBias = 0.1f;
    public float o3SideBias = 0f;
    public float o4UpBias = 0.1f;
    public float o4SideBias = 0f;

    private float[] o1params;
    private float[] o2params;
    private float[] o3params;
    private float[] o4params;

    private Vector3[] verts;
    private int[] tris;
    private int ntris = 0;
    private int count = 0;

    void Start()
    {
        // check type and set parameters
        // parameters are:
        // 0: growth chance
        // 1: death chance
        // 2: branch chance
        // 3: leaf chance
        // 4: length
        // 5: thickness
        // 6: variation
        // 7: branch angle
        // growth chance and dead should add between 0 and 1 (they are exclusive)
        // however, branch chance and leaf chance are each between 0 and 1
        // branch chance is a subset of growth chance - ie if the tree doesn't grow then it won't branch
        // leaf chance is completely indpenedent from the others
        // variation and branch angle both define angles of the branches
        // variation defines the randomness within a single branch (ie like -5 to 5 degrees)
        // branch angle defines the angle at which new branches grow from the current branch
        if (treeType == 0)
        {
            o1params = new float[] { 0.7f, 0.00f, 0.8f, 0.0f, 1.0f, 0.8f, 5f, 60f, o1UpBias, o1SideBias };
            o2params = new float[] { 0.8f, 0.10f, 0.3f, 0.3f, 1.0f, 0.5f, 15f, 30f, o2UpBias, o2SideBias };
            o3params = new float[] { 0.4f, 0.20f, 0.0f, 0.8f, 1.0f, 0.2f, 20f, 60f, o3UpBias, o3SideBias };
            o4params = new float[] { 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, o4UpBias, o4SideBias }; // no 4th order
        }
        else if (treeType == 1)
        {
            o1params = new float[] { 0.5f, 0.05f, 0.8f, 0.0f, 1.0f, 0.7f, 10f, 45f, o1UpBias, o1SideBias };
            o2params = new float[] { 0.7f, 0.10f, 0.6f, 0.5f, 0.8f, 0.4f, 15f, 30f, o2UpBias, o2SideBias };
            o3params = new float[] { 0.7f, 0.20f, 0.0f, 1.0f, 0.5f, 0.2f, 15f, 0f, o3UpBias, o3SideBias };
            o4params = new float[] { 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, o4UpBias, o4SideBias }; // no 4th order
        }
        else if (treeType == 2)
        {
            o1params = new float[] { 1.0f, 0.00f, 0.8f, 0.0f, 1.5f, 1.5f, 3f, 45f, o1UpBias, o1SideBias };
            o2params = new float[] { 0.9f, 0.1f, 0.5f, 0.5f, 1.0f, 0.9f, 10f, 30f, o2UpBias, o2SideBias };
            o3params = new float[] { 0.6f, 0.2f, 0.5f, 0.9f, 0.8f, 0.6f, 20f, 60f, o3UpBias, o3SideBias };
            o4params = new float[] { 0.1f, 0.1f, 0f, 1.0f, 0.5f, 0.3f, 30f, 0f, o4UpBias, o4SideBias }; // mainly to spawn more leaves
        }
        else // an explosion of colors
        {
            o1params = new float[] { 0.8f, 0.20f, 0.5f, 0.8f, 1.0f, 1.0f, 5f, 10f, o1UpBias, o1SideBias };
            o2params = new float[] { 0.8f, 0.10f, 0.5f, 0.8f, 1.0f, 0.7f, 15f, 10f, o2UpBias, o2SideBias };
            o3params = new float[] { 0.8f, 0.20f, 0.5f, 0.8f, 1.0f, 0.5f, 30f, 30f, o3UpBias, o3SideBias };
            o4params = new float[] { 0.7f, 0.30f, 0.5f, 0.8f, 1.0f, 0.2f, 45f, 0f, o4UpBias, o4SideBias };
        }
        
        Tree tree = new Tree(seed, cycles, initCycles, o1params, o2params, o3params, o4params);
        List<List<Branch>> branches = tree.startGrowth();
        for (int i = 0; i < branches.Count; i++)
        {
            List<Branch> branchList = branches[i];
            createBranch(branchList);
        }

        List<Leaf> leaves = tree.getLeaves();
        foreach (Leaf leaf in leaves)
        {
            // create leaf game object
            createLeaf(leaf);
        }
    }

    void createBranch(List<Branch> branchList)
    {
        if (branchList.Count == 0)
            return;
        // print(branchList);
        // one branch is one mesh
        Mesh mesh = new Mesh();
        // reset data structures
        int numVerts = (branchList.Count + 1) * numQuads + 1; // +1 for end cap center
        verts = new Vector3[numVerts];
        int numTris = branchList.Count * numQuads * 2 + numQuads; // 2 tris per quad per segment + end cap
        tris = new int[numTris * 3];
        ntris = 0;
        // we need to make the first vertices at the start of the first segment
        Branch firstBranch = branchList[0];
        Vector3 startPos = firstBranch.getStartPosition();
        float startThickness = firstBranch.getThickness();
        Vector3 sB = firstBranch.getStartB();
        Vector3 sN = firstBranch.getStartN();
        Vector3[] lastVerts = new Vector3[numQuads];
        for (int i = 0; i < numQuads; i++)
        {
            float theta = Mathf.PI * 2f * i / numQuads;
            Vector3 vertPos = startPos + (startThickness / 2f) * (sN * Mathf.Cos(theta) + sB * Mathf.Sin(theta));
            verts[i] = vertPos;
            lastVerts[i] = vertPos;
        }
        // loop through each branch segment and add their vertices and triangles
        for (int i = 0; i < branchList.Count; i++)
        {
            int index = numQuads * (i + 1);
            Branch branch = branchList[i];
            // print("Branch with N, B: " + branch.getEndN().ToString() + ", " + branch.getEndB().ToString());
            Vector3 endPos = branch.getEndPosition();
            float endThickness = branch.getThickness();
            Vector3 endB = branch.getEndB();
            Vector3 endN = branch.getEndN();
            Vector3[] currVerts = new Vector3[numQuads];
            // create end vertices
            for (int j = 0; j < numQuads; j++)
            {
                float theta = Mathf.PI * 2f * j / numQuads;
                Vector3 vertPos = endPos + (endThickness / 2f) * (endN * Mathf.Cos(theta) + endB * Mathf.Sin(theta));
                verts[index + j] = vertPos;
                currVerts[j] = vertPos;
            }
            // create quads between last verts and curr verts
            // since they should be aligned, our calculations are easier
            for (int j = 0; j < numQuads; j++)
            {
                int nextJ = (j + 1) % numQuads;
                // print("Creating quad between " + currVerts[j] + ", " + currVerts[nextJ] + ", " + lastVerts[nextJ] + ", " + lastVerts[j]);
                MakeQuad(
                    Array.IndexOf(verts, currVerts[j]),
                    Array.IndexOf(verts, currVerts[nextJ]),
                    Array.IndexOf(verts, lastVerts[nextJ]),
                    Array.IndexOf(verts, lastVerts[j])
                );
            }
            lastVerts = currVerts; // update lastVerts for next iteration
        }
        // we need to cap off the end of the branch
        Branch lastBranch = branchList[branchList.Count - 1];
        Vector3 direction = (lastBranch.getEndPosition() - lastBranch.getStartPosition()).normalized;
        Vector3 endCapCenter = lastBranch.getEndPosition() + direction * 0.5f;
        verts[verts.Length - 1] = endCapCenter;
        for (int i = 0; i < numQuads; i++)
        {
            int nextI = (i + 1) % numQuads;
            MakeTri(
                Array.IndexOf(verts, endCapCenter),
                Array.IndexOf(verts, lastVerts[nextI]),
                Array.IndexOf(verts, lastVerts[i])
            );
        }

        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.RecalculateNormals();

        GameObject b = new GameObject("branch" + count++);
        b.AddComponent<MeshFilter>();
        b.AddComponent<MeshRenderer>();

        b.transform.position = transform.position;
        b.GetComponent<MeshFilter>().mesh = mesh;
        // color based on order and type
        Renderer renderer = b.GetComponent<Renderer>();
        renderer.material.color = getBranchColor(branchList[0]);
        b.transform.parent = this.transform;
    }

    void createLeaf(Leaf leaf)
    {
        Mesh mesh = new Mesh();
        //reset data strcutures
        int numVerts = 12;
        verts = new Vector3[numVerts];
        int numTris = 8;
        tris = new int[numTris * 3];
        ntris = 0;

        // get relavant data
        Vector3 position = leaf.GetPosition();
        Vector3 direction = leaf.GetDirection();
        Vector3 normal = leaf.GetNormal(); // this is always up

        verts[0] = new Vector3(0, 0, 1); // tip
        verts[1] = new Vector3(-0.2f, 0, 0.7f);
        verts[2] = new Vector3(0.2f, 0, 0.7f); // left and right upper
        verts[3] = new Vector3(-0.3f, 0, 0.2f);
        verts[4] = new Vector3(0.3f, 0, 0.2f); // left and right mid
        verts[5] = new Vector3(-0.05f, 0, 0.0f);

        verts[6] = new Vector3(0, 0, 1); // tip
        verts[7] = new Vector3(-0.2f, 0, 0.7f);
        verts[8] = new Vector3(0.2f, 0, 0.7f); // left and right upper
        verts[9] = new Vector3(-0.3f, 0, 0.2f);
        verts[10] = new Vector3(0.3f, 0, 0.2f); // left and right mid
        verts[11] = new Vector3(-0.05f, 0, 0.0f);

        MakeTri(0, 2, 1);
        MakeTri(1, 2, 4);
        MakeTri(1, 4, 3);
        MakeTri(3, 4, 5);

        MakeTri(6, 8, 7);
        MakeTri(7, 8, 10);
        MakeTri(7, 10, 9);
        MakeTri(9, 10, 11);

        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.RecalculateNormals();
        GameObject l = new GameObject("leaf" + count++);
        l.AddComponent<MeshFilter>();
        l.AddComponent<MeshRenderer>();

        l.transform.position = position + transform.position;
        l.GetComponent<MeshFilter>().mesh = mesh;
        l.transform.rotation = Quaternion.LookRotation(direction, normal);
        Renderer renderer = l.GetComponent<Renderer>();
        renderer.material.color = getLeafColor();
        l.transform.parent = this.transform;
        l.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
    }

    // Update is called once per frame
    void Update()
    {

    }

    private Color getBranchColor(Branch branch)
    {
        if (treeType == 0) //shades of darker and darker brown
        {
            if (branch.getOrder() == 1)
            {
                return new Color(0.35f, 0.2f, 0f);
            }
            else if (branch.getOrder() == 2)
            {
                return new Color(0.4f, 0.25f, 0.0f);
            }
            else
            {
                return new Color(0.4f, 0.3f, 0.0f);
            }
        }
        else if (treeType == 1)
        {
            if (branch.getOrder() <= 2) // grayish colors
            {
                return new Color(0.3f, 0.3f, 0.27f);
            }
            else
            {
                return new Color(0.45f, 0.4f, 0.35f);
            }
        }
        else if (treeType == 2)
        {
            if (branch.getOrder() == 1)
            {
                return new Color(0.2f, 0.1f, 0f);
            }
            else if (branch.getOrder() == 2)
            {
                return new Color(0.25f, 0.15f, 0.0f);
            }
            else if (branch.getOrder() == 3)
            {
                return new Color(0.28f, 0.18f, 0.0f);
            }
            else
            {
                return new Color(0.3f, 0.2f, 0.0f);
            }
        }
        else
        {
            return new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f));
        }
    }
    private Color getLeafColor()
    {
        float random = UnityEngine.Random.Range(0f, 1f);
        if (random < 0.1f)
        {
            return new Color(0.3f, 0.55f, 0.2f); // green
        }
        else if (random < 0.4f)
        {
            return new Color(0.6f, 0.55f, 0.1f); // yellow
        }
        else if (random < 0.8f)
        {
            return new Color(0.6f, 0.3f, 0.1f); // orange
        }
        else
        {
            return new Color(0.5f, 0.15f, 0f); // red
        }
    }
    
    // make a triangle from three vertex indices (clockwise order)
    void MakeTri(int i1, int i2, int i3) {
		int index = ntris * 3;  // figure out the base index for storing triangle indices
		ntris++;

		tris[index]     = i1;
		tris[index + 1] = i2;
		tris[index + 2] = i3;
	}

	// make a quadrilateral from four vertex indices (clockwise order)
	void MakeQuad(int i1, int i2, int i3, int i4) {
		MakeTri (i1, i2, i3);
		MakeTri (i1, i3, i4);
	}
}
