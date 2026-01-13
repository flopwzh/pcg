using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public int seed = 0;
    private const int W = 51;
    private const int H = 51;

    private char[,] roadmap = new char[H, W];
    private List<Vector2Int> roadPositions = new List<Vector2Int>();
    // Start is called before the first frame update
    void Awake()
    {
        Random.InitState(seed);
        GenerateMap();
        PrintMap();
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    void GenerateMap()
    {
        // note that x will refer to the vertical direction cuz i was getting confused
        int numWalks = 20;
        int walkLen = 200;
        int numBuildings = 50;

        for (int x = 0; x < H; x++)
        {
            for (int y = 0; y < W; y++)
            {
                roadmap[x, y] = '_';
            }
        }

        //center
        Vector2Int center = new Vector2Int(IntRound(H / 2f), IntRound(W / 2f));
        roadmap[center.x, center.y] = 'X';
        roadPositions.Add(center);

        //river
        RandomRiver();

        //roads
        for (int i = 0; i < numWalks; i++)
        {
            RandomWalk(center, walkLen, Vector2Int.zero, 'X');
        }

        //buildings
        for (int i = 0; i < numBuildings; i++)
        {
            GenerateBuilding();
        }
    }

    int IntRound(float num)
    {
        return Mathf.FloorToInt(num + 0.5f);
    }

    void PrintMap()
    {
        Debug.Log("Printing map");
        string mapString = "";
        for (int x = 0; x < H; x++)
        {
            for (int y = 0; y < W; y++)
            {
                mapString += roadmap[x, y];
                mapString += " ";
            }
            mapString += "\n";
        }

        Debug.Log(mapString);
    }

    void RandomRiver()
    {
        float rand = Random.value;
        if (rand < 0.5f) // horizontal
        {
            int x = Random.Range(0, H);
            for (int y = 0; y < W; y++)
                roadmap[x, y] = '~';
        }
        else // vertical
        {
            int y = Random.Range(0, W);
            for (int x = 0; x < H; x++)
                roadmap[x, y] = '~';
        }
    }

    void RandomWalk(Vector2Int pos, int walkLen, Vector2Int lastDir, char c)
    {
        if (walkLen == 0) return;

        // Debug.Log("walking with remaing length " + walkLen);

        if (lastDir == Vector2Int.zero)
        {
            lastDir = RandomDirection();
        }

        var turn = RandomTurn(pos, lastDir, 0);
        if (turn == null) return;

        Vector2Int newPos = turn.Value.Item1;
        Vector2Int newDir = turn.Value.Item2;

        // place road or bridge if crossing water
        if (c == 'X' && (roadmap[newPos.x, newPos.y] == '~' || roadmap[newPos.x, newPos.y] == 'B'))
        {
            roadmap[newPos.x, newPos.y] = 'B';
        }
        else
        {
            roadmap[newPos.x, newPos.y] = c;
            if (c == 'X')
            {
                roadPositions.Add(newPos);
            }
        }

        RandomWalk(newPos, walkLen - 1, newDir, c);
    }

    Vector2Int RandomDirection()
    {
        int dir = Random.Range(1, 5);
        switch (dir)
        {
            case 1: return new Vector2Int(0, -1);   // west
            case 2: return new Vector2Int(1, 0);    // south
            case 3: return new Vector2Int(0, 1);    // east
            default: return new Vector2Int(-1, 0);  // north
        }
    }

    (Vector2Int, Vector2Int)? RandomTurn(Vector2Int pos, Vector2Int lastDir, int tries)
    {
        if (tries > 10) return null;

        int turn = Random.Range(1, 9);
        Vector2Int newDir;

        switch (turn)
        {
            case 1: newDir = new Vector2Int(-lastDir.y, lastDir.x); break; //left
            case 2: newDir = new Vector2Int(lastDir.y, -lastDir.x); break;
            default: newDir = lastDir; break;
        }

        Vector2Int newPos = pos + newDir;

        if (ValidPlacement(pos, newPos))
        {
            return (newPos, newDir);
        }
        else
        {
            return RandomTurn(pos, lastDir, tries + 1);
        }
    }

    bool ValidPlacement(Vector2Int lastPos, Vector2Int placement)
    {
        if (!InsideMap(placement)) return false;

        int x = placement.x;
        int y = placement.y;

        //first check if there is a river and we last place a bridge (this means we are going long the river)
        if (roadmap[lastPos.x, lastPos.y] == 'B' && roadmap[placement.x, placement.y] == '~')
        {
            return false;
        }

        Vector2Int[] offsets = {
            new Vector2Int(-1,-1), new Vector2Int(-1,0), new Vector2Int(-1,1), //upper row
            new Vector2Int(0,-1),                        new Vector2Int(0,1),  //middle row
            new Vector2Int(1,-1),  new Vector2Int(1,0),  new Vector2Int(1,1)   //bottom row
        };

        // collect corners (if they exist)
        Dictionary<Vector2Int, char> neighbors = new Dictionary<Vector2Int, char>();
        foreach (var offset in offsets)
        {
            Vector2Int n = new Vector2Int(x + offset.x, y + offset.y);
            if (InsideMap(n))
            {
                neighbors[offset] = roadmap[n.x, n.y];
            }
        }

        char[] roads = { 'X', 'B' };

        // top-left corner: (y-1,x-1), (y-1,x), (y,x-1)
        if (InsideMap(new Vector2Int(x - 1, y - 1)) && InsideMap(new Vector2Int(x, y - 1)) && InsideMap(new Vector2Int(x - 1, y)))
        {
            if (System.Array.IndexOf(roads, roadmap[x - 1, y - 1]) >= 0 &&
                System.Array.IndexOf(roads, roadmap[x - 1, y]) >= 0 &&
                System.Array.IndexOf(roads, roadmap[x, y - 1]) >= 0)
                return false;
        }

        // top-right corner: (y-1,x+1), (y-1,x), (y,x+1)
        if (InsideMap(new Vector2Int(x - 1, y + 1)) && InsideMap(new Vector2Int(x - 1, y)) && InsideMap(new Vector2Int(x, y + 1)))
        {
            if (System.Array.IndexOf(roads, roadmap[x - 1, y + 1]) >= 0 &&
                System.Array.IndexOf(roads, roadmap[x - 1, y]) >= 0 &&
                System.Array.IndexOf(roads, roadmap[x, y + 1]) >= 0)
                return false;
        }

        // bottom-left: (y+1,x-1), (y+1,x), (y,x-1)
        if (InsideMap(new Vector2Int(x + 1, y - 1)) && InsideMap(new Vector2Int(x, y - 1)) && InsideMap(new Vector2Int(x + 1, y)))
        {
            if (System.Array.IndexOf(roads, roadmap[x + 1, y - 1]) >= 0 &&
                System.Array.IndexOf(roads, roadmap[x + 1, y]) >= 0 &&
                System.Array.IndexOf(roads, roadmap[x, y - 1]) >= 0)
                return false;
        }

        // bottom-right: (y+1,x+1), (y+1,x), (y,x+1)
        if (InsideMap(new Vector2Int(x + 1, y + 1)) && InsideMap(new Vector2Int(x, y + 1)) && InsideMap(new Vector2Int(x + 1, y)))
        {
            if (System.Array.IndexOf(roads, roadmap[x + 1, y + 1]) >= 0 &&
                System.Array.IndexOf(roads, roadmap[x + 1, y]) >= 0 &&
                System.Array.IndexOf(roads, roadmap[x, y + 1]) >= 0)
                return false;
        }

        return true;
    }

    public bool InsideMap(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < H && pos.y >= 0 && pos.y < W;
    }

    void GenerateBuilding()
    {
        if (roadPositions.Count == 0) return;

        Vector2Int randRoadPos = roadPositions[Random.Range(0, roadPositions.Count)];
        Vector2Int[] directions = { new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(-1, 0), new Vector2Int(0, -1) };
        Vector2Int dir = directions[Random.Range(0, directions.Length)];

        Vector2Int newPos = randRoadPos + dir;
        // roadmap[randRoadPos.x, randRoadPos.y] = '*';
        // Debug.Log("chosen road at " + randRoadPos + " with building position " + newPos);
        // if (InsideMap(newPos))
        //     Debug.Log((roadmap[newPos.x, newPos.y] == '_'));
        if (InsideMap(newPos) && roadmap[newPos.x, newPos.y] == '_')
        {
            roadmap[newPos.x, newPos.y] = 'A';
            return;
        }
        else
        {
            GenerateBuilding();
        }
    }

    public char[,] getRoadmap()
    {
        return roadmap;
    }
}
