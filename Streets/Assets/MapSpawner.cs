using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;

public class MapSpawner : MonoBehaviour
{
    public int seed;
    public MapGenerator mapGenerator;
    public GameObject BuildingTest;
    public float tileSize = 1f;
    private char[,] roadmap;

    private Vector3[] verts;  // the vertices of the mesh
	private int[] tris;       // the triangles of the mesh (triplets of integer references to vertices)
	private int ntris = 0;    // the number of triangles that have been created so far
    private int count = 0;

    enum Dir { Down = 0, Right = 1, Up = 2, Left = 3 }

    Dir OffsetToDir(Vector2Int offset)
    {
        if (offset == new Vector2Int(1, 0)) return Dir.Down;
        if (offset == new Vector2Int(0, 1)) return Dir.Right;
        if (offset == new Vector2Int(-1, 0)) return Dir.Up;
        return Dir.Left;
    }

    // Start is called before the first frame update
    void Start()
    {
        Random.InitState(seed);
        SpawnMapObjects();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void SpawnMapObjects()
    {
        roadmap = mapGenerator.getRoadmap();
        int rows = roadmap.GetLength(0);
        int cols = roadmap.GetLength(1);

        for (int x = 0; x < rows; x++)
        {
            for (int y = 0; y < cols; y++)
            {
                Vector3 pos = new Vector3(x * tileSize, 0, y * tileSize);
                ntris = 0;

                // Debug.Log("creating object at " + pos);
                Vector2Int coords = new Vector2Int(x, y);
                switch (roadmap[x, y])
                {
                    // terrain  cases
                    case 'X':
                        // Instantiate(RoadTest, pos, Quaternion.identity);
                        CreateRoad(pos, coords);
                        ntris = 0;
                        CreateGround(pos);
                        break;
                    case 'A':
                        Instantiate(BuildingTest, pos, Quaternion.identity);
                        CreateGround(pos);
                        break;
                    case '_':
                        CreateGround(pos);
                        break;
                    // aquatic cases
                    case 'B':
                        // Instantiate(BridgeTest, pos, Quaternion.identity);
                        CreateBridge(pos, coords);
                        ntris = 0;
                        CreateRiver(pos);
                        break;
                    case '~':
                        // Instantiate(RiverTest, pos, Quaternion.identity);
                        CreateRiver(pos);
                        break;
                }
                count++;
            }
        }
    }

    void CreateRoad(Vector3 pos, Vector2Int coords)
    {
        // check surroundings to determine what road to make
        Vector2Int[] offsets = {
            new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(-1, 0), new Vector2Int(0, -1),
        };
        List<Dir> adjRoads = new List<Dir>();
        foreach (var offset in offsets)
        {
            Vector2Int newPos = coords + offset;
            if (mapGenerator.InsideMap(newPos) && (mapGenerator.getRoadmap()[newPos.x, newPos.y] == 'X' || mapGenerator.getRoadmap()[newPos.x, newPos.y] == 'B'))
            {
                adjRoads.Add(OffsetToDir(offset));
            }
        }
        adjRoads.Sort();
        if (adjRoads.Count == 1)
        {
            createEndRoad(pos, adjRoads[0]);
        }
        else if (adjRoads.Count == 2) // either straight or corner
        {
            Dir d1 = adjRoads[0];
            Dir d2 = adjRoads[1];

            bool opposite = (d1 == Dir.Right && d2 == Dir.Left) || (d1 == Dir.Down && d2 == Dir.Up);
            if (opposite)
            {
                createStraightRoad(pos, d1);
            }
            else
            {
                Dir d;
                if (d1 == Dir.Down && d2 == Dir.Left) d = d2; // the only possible way d is not d1 is if d1 is down and d2 is left
                else d = d1;
                createCornerRoad(pos, d);
            }
        }
        else if (adjRoads.Count == 3)
        {
            Dir d1 = adjRoads[0];
            Dir d2 = adjRoads[1];
            Dir d3 = adjRoads[2];
            Dir d; //refers to the middle road direction
            if (d1 == Dir.Down)
            {
                if (d2 == Dir.Up) d = Dir.Left;
                else if (d3 == Dir.Up) d = Dir.Right;
                else d = Dir.Down; // otherwise down right left
            }
            else
            {
                //otherwise all are constrained to be right up left
                d = Dir.Up;
            }
            createTRoad(pos, d);
        }
        else if (adjRoads.Count == 4)
        {
            createCrossRoad(pos);
        }
    }

    void createEndRoad(Vector3 pos, Dir direction)
    {
        Mesh mesh = new Mesh();
        int num_verts = 10;

        verts = new Vector3[num_verts];
        verts[0] = new Vector3(0.0f, 0.01f, -1.0f);
        for (int i = 1; i < num_verts; i++)
        {
            float theta = Mathf.PI * (i - 1) / 8f;
            float xcoord = 0.0f + 0.8f * Mathf.Cos(theta);
            float ycoord = -1.0f + 0.8f * Mathf.Sin(theta);
            verts[i] = new Vector3(xcoord, 0.01f, ycoord);
        }

        tris = new int[8 * 3];
        for (int i = 1; i < 9; i++)
        {
            MakeTri(0, i + 1, i);
        }

        mesh.vertices = verts;
        mesh.triangles = tris;

        mesh.RecalculateNormals();

        GameObject s = new GameObject("End Road " + count);
        s.AddComponent<MeshFilter>();
        s.AddComponent<MeshRenderer>();

        float x = pos.x;
        float y = pos.y;
        float z = pos.z;
        s.transform.position = new Vector3(x, y, z);
        s.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        s.GetComponent<MeshFilter>().mesh = mesh;
        Renderer rend = s.GetComponent<Renderer>();
        float rand = Random.value;
        rend.material.color = new Color(0.1f + 0.2f * rand, 0.1f + 0.2f * rand, 0.1f + 0.2f * rand, 1.0f);
        if (direction == Dir.Up)
            s.transform.Rotate(Vector3.up, 90);
        else if (direction == Dir.Right)
            s.transform.Rotate(Vector3.up, 180);
        else if (direction == Dir.Down)
            s.transform.Rotate(Vector3.up, 270);
    }

    void createStraightRoad(Vector3 pos, Dir direction)
    {
        Mesh mesh = new Mesh();
        int num_verts = 4;

        verts = new Vector3[num_verts];
        verts[0] = new Vector3(0.8f, 0.01f, 1);
        verts[1] = new Vector3(0.8f, 0.01f, -1);
        verts[2] = new Vector3(-0.8f, 0.01f, -1);
        verts[3] = new Vector3(-0.8f, 0.01f, 1);

        tris = new int[2 * 3];
        MakeQuad(0, 1, 2, 3);

        mesh.vertices = verts;
        mesh.triangles = tris;

        mesh.RecalculateNormals();

        GameObject s = new GameObject("Straight Road " + count);
        s.AddComponent<MeshFilter>();
        s.AddComponent<MeshRenderer>();

        float x = pos.x;
        float y = pos.y;
        float z = pos.z;
        s.transform.position = new Vector3(x, y, z);
        s.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        s.GetComponent<MeshFilter>().mesh = mesh;
        Renderer rend = s.GetComponent<Renderer>();
        float rand = Random.value;
        rend.material.color = new Color(0.1f + 0.2f * rand, 0.1f + 0.2f * rand, 0.1f + 0.2f * rand, 1.0f);
        if (direction == Dir.Up || direction == Dir.Down)
            s.transform.Rotate(Vector3.up, 90);

        //yellow line
        mesh = new Mesh();
        int line_num_verts = 8;
        Vector3[] line_verts = new Vector3[line_num_verts];

        line_verts[0] = new Vector3(0.1f, 0.03f, 1);
        line_verts[1] = new Vector3(0.1f, 0.03f, -1);
        line_verts[2] = new Vector3(0.025f, 0.03f, -1);
        line_verts[3] = new Vector3(0.025f, 0.03f, 1);

        line_verts[4] = new Vector3(-0.025f, 0.03f, 1);
        line_verts[5] = new Vector3(-0.025f, 0.03f, -1);
        line_verts[6] = new Vector3(-0.1f, 0.03f, -1);
        line_verts[7] = new Vector3(-0.1f, 0.03f, 1);

        ntris = 0;
        tris = new int[3 * 4];
        MakeQuad(0, 1, 2, 3);
        MakeQuad(4, 5, 6, 7);

        mesh.vertices = line_verts;
        mesh.triangles = tris;

        mesh.RecalculateNormals();

        GameObject l = new GameObject("line" + count);
        l.AddComponent<MeshFilter>();
        l.AddComponent<MeshRenderer>();
        l.transform.parent = s.transform;


        x = pos.x;
        y = pos.y;
        z = pos.z;
        l.transform.position = new Vector3(x, y, z);
        l.transform.localScale = new Vector3(1, 1, 1);
        l.GetComponent<MeshFilter>().mesh = mesh;
        Renderer r = l.GetComponent<Renderer>();
        r.material.color = new Color(0.9f, 0.9f, 0.2f, 1.0f);
        if (direction == Dir.Up || direction == Dir.Down)
            l.transform.Rotate(Vector3.up, 90);
    }

    void createCornerRoad(Vector3 pos, Dir direction) // dir is from the perspective of the right turner
    {
        Mesh mesh = new Mesh();
        int num_verts = 18;

        verts = new Vector3[num_verts];
        for (int i = 0; i < num_verts / 2; i++)
        {
            float theta = Mathf.PI / 2f * i / 8f;
            float xcoord = -1.0f + 0.2f * Mathf.Sin(theta);
            float ycoord = -1.0f + 0.2f * Mathf.Cos(theta);
            verts[2 * i] = new Vector3(xcoord, 0.01f, ycoord);

            xcoord = -1.0f + 1.8f * Mathf.Sin(theta);
            ycoord = -1.0f + 1.8f * Mathf.Cos(theta);
            verts[2 * i + 1] = new Vector3(xcoord, 0.01f, ycoord);
        }

        tris = new int[8 * 3 * 2]; // 8 segments, with 2 triangles each
        for (int i = 0; i < num_verts / 2 - 1; i++)
        {
            MakeQuad(2 * i, 2 * i + 1, 2 * (i + 1) + 1, 2 * (i + 1));
        }

        mesh.vertices = verts;
        mesh.triangles = tris;

        mesh.RecalculateNormals();

        GameObject s = new GameObject("Corner Road " + count);
        s.AddComponent<MeshFilter>();
        s.AddComponent<MeshRenderer>();

        float x = pos.x;
        float y = pos.y;
        float z = pos.z;
        s.transform.position = new Vector3(x, y, z);
        s.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        s.GetComponent<MeshFilter>().mesh = mesh;
        Renderer rend = s.GetComponent<Renderer>();
        float rand = Random.value;
        rend.material.color = new Color(0.1f + 0.2f * rand, 0.1f + 0.2f * rand, 0.1f + 0.2f * rand, 1.0f);
        if (direction == Dir.Left)
            s.transform.Rotate(Vector3.up, -90);
        else if (direction == Dir.Right)
            s.transform.Rotate(Vector3.up, 90);
        else if (direction == Dir.Down)
            s.transform.Rotate(Vector3.up, 180);
    }

    void createTRoad(Vector3 pos, Dir direction)
    {
        Mesh mesh = new Mesh();
        int num_verts = 21;

        verts = new Vector3[num_verts];
        verts[0] = new Vector3(-1.0f, 0.01f, 0.8f);
        verts[1] = new Vector3(1.0f, 0.01f, 0.8f);
        verts[2] = new Vector3(0.0f, 0.01f, 0.0f);
        for (int i = 3; i < 3 + 9; i++)
        {
            float theta = Mathf.PI / 2f * (i - 3) / 8f;
            float xcoord = -1.0f + 0.2f * Mathf.Cos(theta);
            float ycoord = -1.0f + 0.2f * Mathf.Sin(theta);
            verts[i] = new Vector3(xcoord, 0.01f, ycoord);
        }
        for (int i = 3 + 9; i < 3 + 18; i++)
        {
            float theta = Mathf.PI / 2f * (i - 12) / 8f;
            float xcoord = 1.0f - 0.2f * Mathf.Sin(theta);
            float ycoord = -1.0f + 0.2f * Mathf.Cos(theta);
            verts[i] = new Vector3(xcoord, 0.01f, ycoord);
        }

        tris = new int[(4 * 2 + 4 + 4 + 4) * 3]; // 4 quads at bottom, 4 on both sides connecting to center, 4 large
        //4 large
        MakeTri(0, 1, 2);
        MakeTri(0, 2, 11);
        MakeTri(2, 1, 12);
        MakeTri(7, 2, 16);

        //4 quads
        for (int i = 0; i < 4; i++)
        {
            int lefti = i + 3;
            int righti = num_verts - 1 - i;
            MakeQuad(lefti, lefti + 1, righti - 1, righti);
        }

        // 4 on left side
        for (int i = 7; i < 7 + 4; i++)
        {
            MakeTri(i, i + 1, 2);
        }

        // 4 on right side
        for (int i = 12; i < 12 + 4; i++)
        {
            MakeTri(i, i + 1, 2);
        }

        mesh.vertices = verts;
        mesh.triangles = tris;

        mesh.RecalculateNormals();

        GameObject s = new GameObject("T Road " + count);
        s.AddComponent<MeshFilter>();
        s.AddComponent<MeshRenderer>();

        float x = pos.x;
        float y = pos.y;
        float z = pos.z;
        s.transform.position = new Vector3(x, y, z);
        s.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        s.GetComponent<MeshFilter>().mesh = mesh;
        Renderer rend = s.GetComponent<Renderer>();
        float rand = Random.value;
        rend.material.color = new Color(0.1f + 0.2f * rand, 0.1f + 0.2f * rand, 0.1f + 0.2f * rand, 1.0f);
        if (direction == Dir.Up)
            s.transform.Rotate(Vector3.up, 90);
        else if (direction == Dir.Right)
            s.transform.Rotate(Vector3.up, 180);
        else if (direction == Dir.Down)
            s.transform.Rotate(Vector3.up, 270);
    }

    void createCrossRoad(Vector3 pos)
    {
        Mesh mesh = new Mesh();
        int num_verts = 1 + 4 * 9;

        verts = new Vector3[num_verts];
        verts[0] = verts[0] = new Vector3(0, 0, 0);

        for (int i = 0; i < 9; i++) // bottom left, 1 to 9
        {
            int ind = i + 1;
            float theta = Mathf.PI / 2f * i / 8f;
            float xcoord = -1.0f + 0.2f * Mathf.Cos(theta);
            float ycoord = -1.0f + 0.2f * Mathf.Sin(theta);
            verts[ind] = new Vector3(xcoord, 0.01f, ycoord);
        }

        for (int i = 0; i < 9; i++) // bottom right, 10 to 18
        {
            int ind = i + 1 + 9;
            float theta = Mathf.PI / 2f * i / 8f;
            float xcoord = 1.0f - 0.2f * Mathf.Sin(theta);
            float ycoord = -1.0f + 0.2f * Mathf.Cos(theta);
            verts[ind] = new Vector3(xcoord, 0.01f, ycoord);
        }

        for (int i = 0; i < 9; i++) // upper right, 19 to 27
        {
            int ind = i + 1 + 9 * 2;
            float theta = Mathf.PI / 2f * i / 8f;
            float xcoord = 1.0f - 0.2f * Mathf.Cos(theta);
            float ycoord = 1.0f - 0.2f * Mathf.Sin(theta);
            verts[ind] = new Vector3(xcoord, 0.01f, ycoord);
        }

        for (int i = 0; i < 9; i++) // upper left, 28 to 36
        {
            int ind = i + 1 + 9 * 3;
            float theta = Mathf.PI / 2f * i / 8f;
            float xcoord = -1.0f + 0.2f * Mathf.Cos(theta);
            float ycoord = 1.0f - 0.2f * Mathf.Sin(theta);
            verts[ind] = new Vector3(xcoord, 0.01f, ycoord);
        }

        tris = new int[(4 + 4 * 9) * 3]; // 4 large, 4 groups of 9
        MakeTri(0, 18, 1);
        MakeTri(0, 27, 10);
        MakeTri(0, 28, 19);
        MakeTri(0, 9, 36);

        for (int i = 0; i < 9 - 1; i++)
        {
            int ind = i + 1;
            MakeTri(ind, ind + 1, 0);
        }
        for (int i = 0; i < 9 - 1; i++)
        {
            int ind = i + 1 + 9;
            MakeTri(ind, ind + 1, 0);
        }
        for (int i = 0; i < 9 - 1; i++)
        {
            int ind = i + 1 + 9 * 2;
            MakeTri(ind, ind + 1, 0);
        }
        for (int i = 0; i < 9 - 1; i++)
        {
            int ind = i + 1 + 9 * 3;
            MakeTri(ind, 0, ind + 1);
        }

        mesh.vertices = verts;
        mesh.triangles = tris;

        mesh.RecalculateNormals();

        GameObject s = new GameObject("Cross Road " + count);
        s.AddComponent<MeshFilter>();
        s.AddComponent<MeshRenderer>();

        float x = pos.x;
        float y = pos.y;
        float z = pos.z;
        s.transform.position = new Vector3(x, y, z);
        s.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        s.GetComponent<MeshFilter>().mesh = mesh;
        Renderer rend = s.GetComponent<Renderer>();
        float rand = Random.value;
        rend.material.color = new Color(0.1f + 0.2f * rand, 0.1f + 0.2f * rand, 0.1f + 0.2f * rand, 1.0f);
    }

    void CreateBridge(Vector3 pos, Vector2Int coords)
    {
        // check surroundings to determine what road to make
        Vector2Int[] offsets = {
            new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(-1, 0), new Vector2Int(0, -1),
        };
        List<Dir> adjRoads = new List<Dir>();
        foreach (var offset in offsets)
        {
            Vector2Int newPos = coords + offset;
            if (mapGenerator.InsideMap(newPos) && (mapGenerator.getRoadmap()[newPos.x, newPos.y] == 'X'))
            {
                adjRoads.Add(OffsetToDir(offset));
            }
        }
        Dir d;
        if (adjRoads[0] == Dir.Down || adjRoads[0] == Dir.Up)
        {
            d = Dir.Down;
        }
        else
        {
            d = Dir.Right;
        }

        Mesh mesh = new Mesh();
        int num_verts = 12 * 3;
        // top surface 8 verts - 12 for smooth
        // inside will mirror

        verts = new Vector3[num_verts];
        float rise = 0.4f;
        // top, south side
        verts[0] = new Vector3(0.8f, 0.01f, -1);
        verts[1] = new Vector3(-0.8f, 0.01f, -1);
        verts[2] = new Vector3(0.8f, rise, -0.5f);
        verts[3] = new Vector3(-0.8f, rise, -0.5f);
        // middle
        verts[4] = new Vector3(0.8f, rise, -0.5f);
        verts[5] = new Vector3(-0.8f, rise, -0.5f);
        verts[6] = new Vector3(0.8f, rise, 0.5f);
        verts[7] = new Vector3(-0.8f, rise, 0.5f);
        // north
        verts[8] = new Vector3(0.8f, rise, 0.5f);
        verts[9] = new Vector3(-0.8f, rise, 0.5f);
        verts[10] = new Vector3(0.8f, 0.01f, 1);
        verts[11] = new Vector3(-0.8f, 0.01f, 1);

        // left, south side
        verts[12] = new Vector3(-0.8f, 0.01f, -1);
        verts[13] = new Vector3(-0.8f, 0.01f, -0.6f);
        verts[14] = new Vector3(-0.8f, rise, -0.5f);
        verts[15] = new Vector3(-0.8f, rise/2, -0.35f);
        //middle
        verts[16] = new Vector3(-0.8f, rise/2, -0.35f);
        verts[17] = new Vector3(-0.8f, rise/2, 0.35f);
        verts[18] = new Vector3(-0.8f, rise, 0.5f);
        verts[19] = new Vector3(-0.8f, rise, -0.5f);
        //north
        verts[20] = new Vector3(-0.8f, rise/2, 0.35f);
        verts[21] = new Vector3(-0.8f, 0.01f, 0.6f);
        verts[22] = new Vector3(-0.8f, 0.01f, 1f);
        verts[23] = new Vector3(-0.8f, rise, 0.5f);

        // right, south side
        verts[24] = new Vector3(0.8f, 0.01f, -1);
        verts[25] = new Vector3(0.8f, rise, -0.5f);
        verts[26] = new Vector3(0.8f, rise/2, -0.35f);
        verts[27] = new Vector3(0.8f, 0.01f, -0.6f);
        //middle
        verts[28] = new Vector3(0.8f, rise/2, -0.35f);
        verts[29] = new Vector3(0.8f, rise, -0.5f);
        verts[30] = new Vector3(0.8f, rise, 0.5f);
        verts[31] = new Vector3(0.8f, rise/2, 0.35f);
        //north
        verts[32] = new Vector3(0.8f, rise/2, 0.35f);
        verts[33] = new Vector3(0.8f, rise, 0.5f);
        verts[34] = new Vector3(0.8f, 0.01f, 1f);
        verts[35] = new Vector3(0.8f, 0.01f, 0.6f);

        tris = new int[8 * 3 * 3];
        MakeQuad(0, 1, 3, 2);
        MakeQuad(4, 5, 7, 6);
        MakeQuad(8, 9, 11, 10);

        MakeQuad(12, 13, 15, 14);
        MakeQuad(16, 17, 18, 19);
        MakeQuad(20, 21, 22, 23);

        MakeQuad(24, 25, 26, 27);
        MakeQuad(28, 29, 30, 31);
        MakeQuad(32, 33, 34, 35);

        mesh.vertices = verts;
        mesh.triangles = tris;

        mesh.RecalculateNormals();

        GameObject s = new GameObject("Bridge " + count);
        s.AddComponent<MeshFilter>();
        s.AddComponent<MeshRenderer>();

        float x = pos.x;
        float y = pos.y;
        float z = pos.z;
        s.transform.position = new Vector3(x, y, z);
        s.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        s.GetComponent<MeshFilter>().mesh = mesh;
        Renderer rend = s.GetComponent<Renderer>();
        float rand = Random.value;
        rend.material.color = new Color(0.1f + 0.2f * rand, 0.1f + 0.2f * rand, 0.1f + 0.2f * rand, 1.0f);
        if (d == Dir.Down)
            s.transform.Rotate(Vector3.up, 90);
    }

    void CreateRiver(Vector3 pos)
    {
        Mesh mesh = new Mesh();
        int num_verts = 4;

        verts = new Vector3[num_verts];
        verts[0] = new Vector3(1, 0, 1);
        verts[1] = new Vector3(1, 0, -1);
        verts[2] = new Vector3(-1, 0, -1);
        verts[3] = new Vector3(-1, 0, 1);

        tris = new int[2 * 3];
        MakeQuad(0, 1, 2, 3);

        mesh.vertices = verts;
        mesh.triangles = tris;

        mesh.RecalculateNormals();

        GameObject s = new GameObject("River " + count);
        s.AddComponent<MeshFilter>();
        s.AddComponent<MeshRenderer>();

        float x = pos.x;
        float y = pos.y;
        float z = pos.z;
        s.transform.position = new Vector3(x, y, z);
        s.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        s.GetComponent<MeshFilter>().mesh = mesh;
        Renderer rend = s.GetComponent<Renderer>();
        rend.material.color = new Color(0.2f, 0.3f, 0.8f, 1.0f);
    }

    void CreateGround(Vector3 pos)
    {
        Mesh mesh = new Mesh();
        int num_verts = 4;

        verts = new Vector3[num_verts];
        verts[0] = new Vector3(1, 0, 1);
        verts[1] = new Vector3(1, 0, -1);
        verts[2] = new Vector3(-1, 0, -1);
        verts[3] = new Vector3(-1, 0, 1);

        tris = new int[2 * 3];
        MakeQuad(0, 1, 2, 3);

        mesh.vertices = verts;
        mesh.triangles = tris;

        mesh.RecalculateNormals();

        GameObject s = new GameObject("Ground " + count);
        s.AddComponent<MeshFilter>();
        s.AddComponent<MeshRenderer>();

        float x = pos.x;
        float y = pos.y;
        float z = pos.z;
        s.transform.position = new Vector3(x, y, z);
        s.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        s.GetComponent<MeshFilter>().mesh = mesh;
        Renderer rend = s.GetComponent<Renderer>();
        rend.material.color = new Color(0.2f, 0.8f, 0.3f, 1.0f);
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
