using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurveTest : MonoBehaviour
{
    public GameObject pointPrefab;
    public GameObject curvePointPrefab;

    // Vector3 getPoint(Vector3[,] controlPoints, float u, float v)
    // {
    //     Vector3[] temp = new Vector3[4];
    //     for (int i = 0; i < 4; i++)
    //     {
    //         // calculate a point on each of the 4 bezier curves in u direction
    //         temp[i] = EvaluateBezierSurface(controlPoints, u, v);
    //     }
    //     return EvaluateBezierSurface(temp, u, v);
    // }

    Vector3 EvaluateBezierSurface(Vector3[,] controlPoints, float u, float v)
    {
        // Cubic Bezier basis functions
        float[] Bu = new float[4];
        float[] Bv = new float[4];

        Bu[0] = Mathf.Pow(1 - u, 3);
        Bu[1] = 3 * u * Mathf.Pow(1 - u, 2);
        Bu[2] = 3 * Mathf.Pow(u, 2) * (1 - u);
        Bu[3] = Mathf.Pow(u, 3);

        Bv[0] = Mathf.Pow(1 - v, 3);
        Bv[1] = 3 * v * Mathf.Pow(1 - v, 2);
        Bv[2] = 3 * Mathf.Pow(v, 2) * (1 - v);
        Bv[3] = Mathf.Pow(v, 3);

        Vector3 point = Vector3.zero;

        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                point += Bu[i] * Bv[j] * controlPoints[i, j];
            }
        }

        return point;
    }

    Mesh GenerateBezierPatchMesh(Vector3[,] controlPoints, int resolutionU, int resolutionV)
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        // Sample vertices
        for (int i = 0; i <= resolutionU; i++)
        {
            float u = (float)i / resolutionU;
            for (int j = 0; j <= resolutionV; j++)
            {
                float v = (float)j / resolutionV;
                vertices.Add(EvaluateBezierSurface(controlPoints, u, v));
            }
        }

        // Triangles (grid-based)
        for (int i = 0; i < resolutionU; i++)
        {
            for (int j = 0; j < resolutionV; j++)
            {
                int idx = i * (resolutionV + 1) + j;
                triangles.Add(idx);
                triangles.Add(idx + resolutionV + 1);
                triangles.Add(idx + 1);

                triangles.Add(idx + 1);
                triangles.Add(idx + resolutionV + 1);
                triangles.Add(idx + resolutionV + 2);
            }
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        return mesh;
    }

    // Start is called before the first frame update
    void Start()
    {
        Vector3[,] controlPoints = new Vector3[5,4];
        controlPoints[0, 0] = new Vector3(0.0f, 0.0f, 0.0f);
        controlPoints[0, 1] = new Vector3(1.0f, 0.0f, 0.0f);
        controlPoints[0, 2] = new Vector3(2.0f, 0.0f, 0.0f);
        controlPoints[0, 3] = new Vector3(3.0f, 0.0f, 0.0f);

        controlPoints[1, 0] = new Vector3(0.0f, 1.0f, 1.0f);
        controlPoints[1, 1] = new Vector3(1.0f, 1.0f, 1.0f);
        controlPoints[1, 2] = new Vector3(2.0f, 1.0f, 1.0f);
        controlPoints[1, 3] = new Vector3(3.0f, 1.0f, 1.0f);

        controlPoints[2, 0] = new Vector3(0.0f, 2.0f, 2.0f);
        controlPoints[2, 1] = new Vector3(1.0f, 2.0f, 2.0f);
        controlPoints[2, 2] = new Vector3(2.0f, 2.0f, 2.0f);
        controlPoints[2, 3] = new Vector3(3.0f, 2.0f, 2.0f);

        controlPoints[3, 0] = new Vector3(0.0f, 3.0f, 0.0f);
        controlPoints[3, 1] = new Vector3(1.0f, 3.0f, 0.0f);
        controlPoints[3, 2] = new Vector3(2.0f, 3.0f, 0.0f);
        controlPoints[3, 3] = new Vector3(3.0f, 3.0f, 0.0f);

        controlPoints[4, 0] = new Vector3(0.0f, 4.0f, -1.0f);
        controlPoints[4, 1] = new Vector3(1.0f, 4.0f, -1.0f);
        controlPoints[4, 2] = new Vector3(2.0f, 4.0f, -1.0f);
        controlPoints[4, 3] = new Vector3(3.0f, 4.0f, -1.0f);

        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                Instantiate(pointPrefab, controlPoints[i, j], Quaternion.identity);
            }
        }

        // calculate points on curve
        Mesh bezierMesh = GenerateBezierPatchMesh(controlPoints, 10, 10);
        GameObject bezierObject = new GameObject("BezierPatch");
        MeshFilter mf = bezierObject.AddComponent<MeshFilter>();
        mf.mesh = bezierMesh;
        MeshRenderer mr = bezierObject.AddComponent<MeshRenderer>();

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
