using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

/*
 * Generate a single ship based on the random seed passed in
 */
public class ShipGenerator : MonoBehaviour
{
    public int seed = 0;
    private HullType hullType;
    private float hullLengthMult;
    private float hullWidthMult;
    private float bowLengthMult;
    private float sternLengthMult;
    private SuperstructureType superstructureType;
    private float superstructureLengthMult;
    private float superstructureHeightMult;
    private Color ssColor;
    private Materials deckMaterial;
    private Materials hullMaterial;
    private SailType sailType;
    private float sailHeightMult;
    private float sailSizeMult; // for width/length
    private Color sailStructureColor;

    private GameObject shipObject;
    private GameObject deck;
    private GameObject sail;
    private Vector3 sailPivotPoint;
    private GameObject[] props;
    private Vector3[] propPivotPoints;

    private float lastAngle = 0f;
    private float sailFrequency;

    private void generateShip()
    {
        // hull type 1 = two curved sheets meet at keel
        // hull type 2 = single curved sheet wraps around
        // hull type 3 = very boxy hull

        // type 1 hull control points
        // hullType = HullType.pointed; // placeholder
        if (hullType == HullType.pointed)
        {
            generateHull1();
        }
        else if (hullType == HullType.curved)
        {
            generateHull2();
        }
        else if (hullType == HullType.boxy)
        {
            generateHull3();
        }
        generateSuperstructure();
        generateSail();
        generateProps();
        textureShip();
    }

    // generates hull of type 1
    // note that all the hull generation functions create the bow, stern, and main sections
    private void generateHull1()
    {
        // starboard side of straight section
        Vector3[,] controlPointsL = new Vector3[4, 4];
        controlPointsL[0, 0] = new Vector3(0.0f, 0.0f, 0.0f);
        controlPointsL[0, 1] = new Vector3(0.0f, 0.0f, 0.6f * hullLengthMult);
        controlPointsL[0, 2] = new Vector3(0.0f, 0.0f, 1.4f * hullLengthMult);
        controlPointsL[0, 3] = new Vector3(0.0f, 0.0f, 2.0f * hullLengthMult);

        controlPointsL[1, 0] = new Vector3(0.1f * hullWidthMult, -0.5f, 0.0f);
        controlPointsL[1, 1] = new Vector3(0.1f * hullWidthMult, -0.5f, 0.6f * hullLengthMult);
        controlPointsL[1, 2] = new Vector3(0.1f * hullWidthMult, -0.5f, 1.4f * hullLengthMult);
        controlPointsL[1, 3] = new Vector3(0.1f * hullWidthMult, -0.5f, 2.0f * hullLengthMult);

        controlPointsL[2, 0] = new Vector3(0.3f * hullWidthMult, -0.7f, 0.0f);
        controlPointsL[2, 1] = new Vector3(0.3f * hullWidthMult, -0.7f, 0.6f * hullLengthMult);
        controlPointsL[2, 2] = new Vector3(0.3f * hullWidthMult, -0.7f, 1.4f * hullLengthMult);
        controlPointsL[2, 3] = new Vector3(0.3f * hullWidthMult, -0.7f, 2.0f * hullLengthMult);

        controlPointsL[3, 0] = new Vector3(0.5f * hullWidthMult, -0.8f, 0.0f);
        controlPointsL[3, 1] = new Vector3(0.5f * hullWidthMult, -0.8f, 0.6f * hullLengthMult);
        controlPointsL[3, 2] = new Vector3(0.5f * hullWidthMult, -0.8f, 1.4f * hullLengthMult);
        controlPointsL[3, 3] = new Vector3(0.5f * hullWidthMult, -0.8f, 2.0f * hullLengthMult);

        Mesh hullStraightMeshR = getBezierPatchMesh(controlPointsL, 10, 10);

        // port side of straight section
        Vector3[,] controlPointsR = new Vector3[4, 4];
        // we can just copy right side and reverse the x
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                controlPointsR[3-i, j] = new Vector3( // negative for correct normal direction
                    -controlPointsL[i, j].x + hullWidthMult,
                    controlPointsL[i, j].y,
                    controlPointsL[i, j].z);
            }
        }

        Mesh hullStraightMeshL = getBezierPatchMesh(controlPointsR, 10, 10);

        CombineInstance[] combinedStraight = new CombineInstance[2];
        combinedStraight[0].mesh = hullStraightMeshL;
        combinedStraight[0].transform = Matrix4x4.identity;
        combinedStraight[1].mesh = hullStraightMeshR;
        combinedStraight[1].transform = Matrix4x4.identity;
        Mesh combinedStraightMesh = new Mesh();
        combinedStraightMesh.CombineMeshes(combinedStraight);
        combinedStraightMesh.RecalculateNormals();



        // bow section
        Vector3[,] controlPointsBowL = new Vector3[4, 4];
        float bowOffset = 2.0f * hullLengthMult;
        // to make it a smooth join, we need to be aligned with the straight section
        controlPointsBowL[0, 0] = new Vector3(0.0f, 0.0f, bowOffset);
        controlPointsBowL[0, 1] = new Vector3(0.0f, 0.0f, bowOffset + 0.3f * bowLengthMult);
        controlPointsBowL[0, 2] = new Vector3(0.2f * hullWidthMult, 0.0f, bowOffset + 0.8f * bowLengthMult);
        controlPointsBowL[0, 3] = new Vector3(0.5f * hullWidthMult, 0.0f, bowOffset + 1.0f * bowLengthMult);

        controlPointsBowL[1, 0] = new Vector3(0.1f * hullWidthMult, -0.5f, bowOffset);
        controlPointsBowL[1, 1] = new Vector3(0.1f * hullWidthMult, -0.5f, bowOffset + .2f * bowLengthMult);
        controlPointsBowL[1, 2] = new Vector3(0.3f * hullWidthMult, -0.5f, bowOffset + .6f * bowLengthMult);
        controlPointsBowL[1, 3] = new Vector3(0.5f * hullWidthMult, -0.5f, bowOffset + .8f * bowLengthMult);

        controlPointsBowL[2, 0] = new Vector3(0.3f * hullWidthMult, -0.7f, bowOffset);
        controlPointsBowL[2, 1] = new Vector3(0.3f * hullWidthMult, -0.7f, bowOffset + .2f * bowLengthMult);
        controlPointsBowL[2, 2] = new Vector3(0.4f * hullWidthMult, -0.7f, bowOffset + .4f * bowLengthMult);
        controlPointsBowL[2, 3] = new Vector3(0.5f * hullWidthMult, -0.7f, bowOffset + .6f * bowLengthMult);

        controlPointsBowL[3, 0] = new Vector3(0.5f * hullWidthMult, -0.8f, bowOffset);
        controlPointsBowL[3, 1] = new Vector3(0.5f * hullWidthMult, -0.8f, bowOffset + .2f * bowLengthMult);
        controlPointsBowL[3, 2] = new Vector3(0.5f * hullWidthMult, -0.8f, bowOffset + .3f * bowLengthMult);
        controlPointsBowL[3, 3] = new Vector3(0.5f * hullWidthMult, -0.8f, bowOffset + .4f * bowLengthMult);

        Mesh hullBowMeshL = getBezierPatchMesh(controlPointsBowL, 10, 10);

        // port side of bow section
        Vector3[,] controlPointsBowR = new Vector3[4, 4];
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                controlPointsBowR[3 - i, j] = new Vector3(
                    -controlPointsBowL[i, j].x + hullWidthMult,
                    controlPointsBowL[i, j].y,
                    controlPointsBowL[i, j].z);
            }
        }

        Mesh hullBowMeshR = getBezierPatchMesh(controlPointsBowR, 10, 10);

        CombineInstance[] combinedBow = new CombineInstance[2];
        combinedBow[0].mesh = hullBowMeshL;
        combinedBow[0].transform = Matrix4x4.identity;
        combinedBow[1].mesh = hullBowMeshR;
        combinedBow[1].transform = Matrix4x4.identity;
        Mesh combinedBowMesh = new Mesh();
        combinedBowMesh.CombineMeshes(combinedBow);
        combinedBowMesh.RecalculateNormals();



        // stern section
        Vector3[,] controlPointsSternL = new Vector3[4, 4];
        controlPointsSternL[0, 0] = new Vector3(0.0f, 0.0f, 0.0f);
        controlPointsSternL[0, 1] = new Vector3(0.0f, 0.0f, -0.3f * sternLengthMult);
        controlPointsSternL[0, 2] = new Vector3(0.25f * hullWidthMult, 0.0f, -0.6f * sternLengthMult);
        controlPointsSternL[0, 3] = new Vector3(0.5f * hullWidthMult, 0.0f, -0.6f * sternLengthMult);

        controlPointsSternL[1, 0] = new Vector3(0.1f * hullWidthMult, -0.5f, 0.0f);
        controlPointsSternL[1, 1] = new Vector3(0.1f * hullWidthMult, -0.5f, -0.4f * sternLengthMult);
        controlPointsSternL[1, 2] = new Vector3(0.3f * hullWidthMult, -0.5f, -0.6f * sternLengthMult);
        controlPointsSternL[1, 3] = new Vector3(0.5f * hullWidthMult, -0.5f, -0.6f * sternLengthMult);

        controlPointsSternL[2, 0] = new Vector3(0.3f * hullWidthMult, -0.7f, 0.0f);
        controlPointsSternL[2, 1] = new Vector3(0.3f * hullWidthMult, -0.7f, -0.4f * sternLengthMult);
        controlPointsSternL[2, 2] = new Vector3(0.4f * hullWidthMult, -0.7f, -0.5f * sternLengthMult);
        controlPointsSternL[2, 3] = new Vector3(0.5f * hullWidthMult, -0.7f, -0.5f * sternLengthMult);

        controlPointsSternL[3, 0] = new Vector3(0.5f * hullWidthMult, -0.8f, 0.0f);
        controlPointsSternL[3, 1] = new Vector3(0.5f * hullWidthMult, -0.8f, -0.1f * sternLengthMult);
        controlPointsSternL[3, 2] = new Vector3(0.5f * hullWidthMult, -0.8f, -0.3f * sternLengthMult);
        controlPointsSternL[3, 3] = new Vector3(0.5f * hullWidthMult, -0.8f, -0.3f * sternLengthMult);
        // reverse order for normal direction bc points were added the other direction (front to back)
        Vector3[,] tempL = new Vector3[4, 4];
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                tempL[i, j] = new Vector3(
                    controlPointsSternL[3 - i, j].x,
                    controlPointsSternL[3 - i, j].y,
                    controlPointsSternL[3 - i, j].z);
            }
        }
        controlPointsSternL = tempL;
        Mesh hullSternMeshL = getBezierPatchMesh(controlPointsSternL, 10, 10);

        // port side
        Vector3[,] controlPointsSternR = new Vector3[4, 4];
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                controlPointsSternR[3 - i, j] = new Vector3(
                    -controlPointsSternL[i, j].x + hullWidthMult,
                    controlPointsSternL[i, j].y,
                    controlPointsSternL[i, j].z);
            }
        }
        Mesh hullSternMeshR = getBezierPatchMesh(controlPointsSternR, 10, 10);

        // combine stern
        CombineInstance[] combinedStern = new CombineInstance[2];
        combinedStern[0].mesh = hullSternMeshL;
        combinedStern[0].transform = Matrix4x4.identity;
        combinedStern[1].mesh = hullSternMeshR;
        combinedStern[1].transform = Matrix4x4.identity;
        Mesh combinedSternMesh = new Mesh();
        combinedSternMesh.CombineMeshes(combinedStern);
        combinedSternMesh.RecalculateNormals();



        // final combine and create object 
        CombineInstance[] finalHull = new CombineInstance[3];
        finalHull[0].mesh = combinedStraightMesh;
        finalHull[0].transform = Matrix4x4.identity;
        finalHull[1].mesh = combinedBowMesh;
        finalHull[1].transform = Matrix4x4.identity;
        finalHull[2].mesh = combinedSternMesh;
        finalHull[2].transform = Matrix4x4.identity;
        Mesh hullMesh = new Mesh();
        hullMesh.CombineMeshes(finalHull);
        hullMesh.RecalculateNormals();

        shipObject = new GameObject("Ship");
        MeshFilter mf = shipObject.AddComponent<MeshFilter>();
        mf.mesh = hullMesh;
        MeshRenderer mr = shipObject.AddComponent<MeshRenderer>();
        mr.material.color = Color.gray;



        // create deck with three sections, one for bow, stern and straight
        // straight section is a flat plane
        Vector3[] straightDeckVertices = new Vector3[4];
        straightDeckVertices[0] = controlPointsL[0, 0];
        straightDeckVertices[1] = controlPointsR[3, 0]; // bc R was reversed on the first axis
        straightDeckVertices[2] = controlPointsR[3, 3];
        straightDeckVertices[3] = controlPointsL[0, 3];

        int[] straightDeckTriangles = new int[6] { 0, 2, 1, 0, 3, 2 };

        Vector2[] uvs = new Vector2[4];
        uvs[0] = new Vector2(0, 0);
        uvs[1] = new Vector2(1, 0);
        uvs[2] = new Vector2(1, 1);
        uvs[3] = new Vector2(0, 1);

        Mesh straightDeckMesh = new Mesh();
        straightDeckMesh.vertices = straightDeckVertices;
        straightDeckMesh.triangles = straightDeckTriangles;
        straightDeckMesh.uv = uvs;
        straightDeckMesh.RecalculateNormals();

        // bow deck section
        Vector3[,] controlPointsBowDeck = new Vector3[4, 4];
        controlPointsBowDeck[0, 0] = controlPointsBowR[3, 0];
        controlPointsBowDeck[0, 1] = controlPointsBowR[3, 1];
        controlPointsBowDeck[0, 2] = controlPointsBowR[3, 2];
        controlPointsBowDeck[0, 3] = controlPointsBowR[3, 3];

        controlPointsBowDeck[1, 0] = controlPointsBowR[3, 0] * 0.7f + controlPointsBowL[0, 0] * 0.3f;
        controlPointsBowDeck[1, 1] = controlPointsBowR[3, 1] * 0.7f + controlPointsBowL[0, 1] * 0.3f;
        controlPointsBowDeck[1, 2] = controlPointsBowR[3, 2] * 0.7f + controlPointsBowL[0, 2] * 0.3f;
        controlPointsBowDeck[1, 3] = controlPointsBowR[3, 3] * 0.7f + controlPointsBowL[0, 3] * 0.3f;

        controlPointsBowDeck[2, 0] = controlPointsBowR[3, 0] * 0.3f + controlPointsBowL[0, 0] * 0.7f;
        controlPointsBowDeck[2, 1] = controlPointsBowR[3, 1] * 0.3f + controlPointsBowL[0, 1] * 0.7f;
        controlPointsBowDeck[2, 2] = controlPointsBowR[3, 2] * 0.3f + controlPointsBowL[0, 2] * 0.7f;
        controlPointsBowDeck[2, 3] = controlPointsBowR[3, 3] * 0.3f + controlPointsBowL[0, 3] * 0.7f;

        controlPointsBowDeck[3, 0] = controlPointsBowL[0, 0];
        controlPointsBowDeck[3, 1] = controlPointsBowL[0, 1];
        controlPointsBowDeck[3, 2] = controlPointsBowL[0, 2];
        controlPointsBowDeck[3, 3] = controlPointsBowL[0, 3];

        Mesh bowDeckMesh = getBezierPatchMesh(controlPointsBowDeck, 10, 10);

        // stern deck section
        Vector3[,] controlPointsSternDeck = new Vector3[4, 4];
        controlPointsSternDeck[0, 0] = controlPointsSternL[3, 0];
        controlPointsSternDeck[0, 1] = controlPointsSternL[3, 1];
        controlPointsSternDeck[0, 2] = controlPointsSternL[3, 2];
        controlPointsSternDeck[0, 3] = controlPointsSternL[3, 3];

        controlPointsSternDeck[1, 0] = controlPointsSternL[3, 0] * 0.7f + controlPointsSternR[0, 0] * 0.3f;
        controlPointsSternDeck[1, 1] = controlPointsSternL[3, 1] * 0.7f + controlPointsSternR[0, 1] * 0.3f;
        controlPointsSternDeck[1, 2] = controlPointsSternL[3, 2] * 0.7f + controlPointsSternR[0, 2] * 0.3f;
        controlPointsSternDeck[1, 3] = controlPointsSternL[3, 3] * 0.7f + controlPointsSternR[0, 3] * 0.3f;

        controlPointsSternDeck[2, 0] = controlPointsSternL[3, 0] * 0.3f + controlPointsSternR[0, 0] * 0.7f;
        controlPointsSternDeck[2, 1] = controlPointsSternL[3, 1] * 0.3f + controlPointsSternR[0, 1] * 0.7f;
        controlPointsSternDeck[2, 2] = controlPointsSternL[3, 2] * 0.3f + controlPointsSternR[0, 2] * 0.7f;
        controlPointsSternDeck[2, 3] = controlPointsSternL[3, 3] * 0.3f + controlPointsSternR[0, 3] * 0.7f;

        controlPointsSternDeck[3, 0] = controlPointsSternR[0, 0];
        controlPointsSternDeck[3, 1] = controlPointsSternR[0, 1];
        controlPointsSternDeck[3, 2] = controlPointsSternR[0, 2];
        controlPointsSternDeck[3, 3] = controlPointsSternR[0, 3];

        Mesh sternDeckMesh = getBezierPatchMesh(controlPointsSternDeck, 10, 10);

        CombineInstance[] finalDeck = new CombineInstance[3];
        finalDeck[0].mesh = bowDeckMesh;
        finalDeck[0].transform = Matrix4x4.identity;
        finalDeck[1].mesh = straightDeckMesh;
        finalDeck[1].transform = Matrix4x4.identity;
        finalDeck[2].mesh = sternDeckMesh;
        finalDeck[2].transform = Matrix4x4.identity;
        Mesh deckMesh = new Mesh();
        deckMesh.CombineMeshes(finalDeck);
        deckMesh.RecalculateNormals();

        // deck
        deck = new GameObject("Deck");
        MeshFilter deckMf = deck.AddComponent<MeshFilter>();
        deckMf.mesh = deckMesh;
        MeshRenderer deckMr = deck.AddComponent<MeshRenderer>();
        deckMr.material.color = Color.yellow;
        deck.transform.parent = shipObject.transform;        
    }

    // type 2, more curved, less pointed at the keel
    private void generateHull2()
    {
        // starboard side of straight section
        Vector3[,] controlPointsL = new Vector3[4, 4];
        controlPointsL[0, 0] = new Vector3(0.0f, 0.0f, 0.0f);
        controlPointsL[0, 1] = new Vector3(0.0f, 0.0f, 0.6f * hullLengthMult);
        controlPointsL[0, 2] = new Vector3(0.0f, 0.0f, 1.4f * hullLengthMult);
        controlPointsL[0, 3] = new Vector3(0.0f, 0.0f, 2.0f * hullLengthMult);

        controlPointsL[1, 0] = new Vector3(0.0f, -0.5f, 0.0f);
        controlPointsL[1, 1] = new Vector3(0.0f, -0.5f, 0.6f * hullLengthMult);
        controlPointsL[1, 2] = new Vector3(0.0f, -0.5f, 1.4f * hullLengthMult);
        controlPointsL[1, 3] = new Vector3(0.0f, -0.5f, 2.0f * hullLengthMult);

        controlPointsL[2, 0] = new Vector3(0.1f * hullWidthMult, -0.8f, 0.0f);
        controlPointsL[2, 1] = new Vector3(0.1f * hullWidthMult, -0.8f, 0.6f * hullLengthMult);
        controlPointsL[2, 2] = new Vector3(0.1f * hullWidthMult, -0.8f, 1.4f * hullLengthMult);
        controlPointsL[2, 3] = new Vector3(0.1f * hullWidthMult, -0.8f, 2.0f * hullLengthMult);

        controlPointsL[3, 0] = new Vector3(0.5f * hullWidthMult, -0.8f, 0.0f);
        controlPointsL[3, 1] = new Vector3(0.5f * hullWidthMult, -0.8f, 0.6f * hullLengthMult);
        controlPointsL[3, 2] = new Vector3(0.5f * hullWidthMult, -0.8f, 1.4f * hullLengthMult);
        controlPointsL[3, 3] = new Vector3(0.5f * hullWidthMult, -0.8f, 2.0f * hullLengthMult);

        Mesh hullStraightMeshR = getBezierPatchMesh(controlPointsL, 10, 10);

        // port side of straight section
        Vector3[,] controlPointsR = new Vector3[4, 4];
        // we can just copy right side and reverse the x
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                controlPointsR[3 - i, j] = new Vector3( // negative for correct normal direction
                    -controlPointsL[i, j].x + hullWidthMult,
                    controlPointsL[i, j].y,
                    controlPointsL[i, j].z);
            }
        }

        Mesh hullStraightMeshL = getBezierPatchMesh(controlPointsR, 10, 10);

        CombineInstance[] combinedStraight = new CombineInstance[2];
        combinedStraight[0].mesh = hullStraightMeshL;
        combinedStraight[0].transform = Matrix4x4.identity;
        combinedStraight[1].mesh = hullStraightMeshR;
        combinedStraight[1].transform = Matrix4x4.identity;
        Mesh combinedStraightMesh = new Mesh();
        combinedStraightMesh.CombineMeshes(combinedStraight);
        combinedStraightMesh.RecalculateNormals();



        // bow section
        Vector3[,] controlPointsBowL = new Vector3[4, 4];
        float bowOffset = 2.0f * hullLengthMult;
        // to make it a smooth join, we need to be aligned with the straight section
        controlPointsBowL[0, 0] = new Vector3(0.0f, 0.0f, bowOffset);
        controlPointsBowL[0, 1] = new Vector3(0.0f, 0.0f, bowOffset + 0.3f * bowLengthMult);
        controlPointsBowL[0, 2] = new Vector3(0.2f * hullWidthMult, 0.0f, bowOffset + 1.0f * bowLengthMult);
        controlPointsBowL[0, 3] = new Vector3(0.5f * hullWidthMult, 0.0f, bowOffset + 1.0f * bowLengthMult);

        controlPointsBowL[1, 0] = new Vector3(0.0f, -0.5f, bowOffset);
        controlPointsBowL[1, 1] = new Vector3(0.0f, -0.5f, bowOffset + .3f * bowLengthMult);
        controlPointsBowL[1, 2] = new Vector3(0.3f * hullWidthMult, -0.5f, bowOffset + .8f * bowLengthMult);
        controlPointsBowL[1, 3] = new Vector3(0.5f * hullWidthMult, -0.5f, bowOffset + .8f * bowLengthMult);

        controlPointsBowL[2, 0] = new Vector3(0.1f * hullWidthMult, -0.8f, bowOffset);
        controlPointsBowL[2, 1] = new Vector3(0.1f * hullWidthMult, -0.8f, bowOffset + .2f * bowLengthMult);
        controlPointsBowL[2, 2] = new Vector3(0.3f * hullWidthMult, -0.8f, bowOffset + .6f * bowLengthMult);
        controlPointsBowL[2, 3] = new Vector3(0.5f * hullWidthMult, -0.8f, bowOffset + .6f * bowLengthMult);

        controlPointsBowL[3, 0] = new Vector3(0.5f * hullWidthMult, -0.8f, bowOffset);
        controlPointsBowL[3, 1] = new Vector3(0.5f * hullWidthMult, -0.8f, bowOffset + .0f * bowLengthMult);
        controlPointsBowL[3, 2] = new Vector3(0.5f * hullWidthMult, -0.8f, bowOffset + .4f * bowLengthMult);
        controlPointsBowL[3, 3] = new Vector3(0.5f * hullWidthMult, -0.8f, bowOffset + .4f * bowLengthMult);

        Mesh hullBowMeshL = getBezierPatchMesh(controlPointsBowL, 10, 10);

        // port side of bow section
        Vector3[,] controlPointsBowR = new Vector3[4, 4];
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                controlPointsBowR[3 - i, j] = new Vector3(
                    -controlPointsBowL[i, j].x + hullWidthMult,
                    controlPointsBowL[i, j].y,
                    controlPointsBowL[i, j].z);
            }
        }

        Mesh hullBowMeshR = getBezierPatchMesh(controlPointsBowR, 10, 10);

        CombineInstance[] combinedBow = new CombineInstance[2];
        combinedBow[0].mesh = hullBowMeshL;
        combinedBow[0].transform = Matrix4x4.identity;
        combinedBow[1].mesh = hullBowMeshR;
        combinedBow[1].transform = Matrix4x4.identity;
        Mesh combinedBowMesh = new Mesh();
        combinedBowMesh.CombineMeshes(combinedBow);
        combinedBowMesh.RecalculateNormals();



        // stern section
        Vector3[,] controlPointsSternL = new Vector3[4, 4];
        controlPointsSternL[0, 0] = new Vector3(0.0f, 0.0f, 0.0f);
        controlPointsSternL[0, 1] = new Vector3(0.0f, 0.0f, -0.3f * sternLengthMult);
        controlPointsSternL[0, 2] = new Vector3(0.3f * hullWidthMult, 0.0f, -0.6f * sternLengthMult);
        controlPointsSternL[0, 3] = new Vector3(0.5f * hullWidthMult, 0.0f, -0.6f * sternLengthMult);

        controlPointsSternL[1, 0] = new Vector3(0.0f, -0.5f, 0.0f);
        controlPointsSternL[1, 1] = new Vector3(0.0f, -0.5f, -0.4f * sternLengthMult);
        controlPointsSternL[1, 2] = new Vector3(0.3f * hullWidthMult, -0.5f, -0.6f * sternLengthMult);
        controlPointsSternL[1, 3] = new Vector3(0.5f * hullWidthMult, -0.5f, -0.6f * sternLengthMult);

        controlPointsSternL[2, 0] = new Vector3(0.1f * hullWidthMult, -0.8f, 0.0f);
        controlPointsSternL[2, 1] = new Vector3(0.1f * hullWidthMult, -0.8f, -0.4f * sternLengthMult);
        controlPointsSternL[2, 2] = new Vector3(0.3f * hullWidthMult, -0.8f, -0.5f * sternLengthMult);
        controlPointsSternL[2, 3] = new Vector3(0.5f * hullWidthMult, -0.8f, -0.5f * sternLengthMult);

        controlPointsSternL[3, 0] = new Vector3(0.5f * hullWidthMult, -0.8f, 0.0f);
        controlPointsSternL[3, 1] = new Vector3(0.5f * hullWidthMult, -0.8f, -0.1f * sternLengthMult);
        controlPointsSternL[3, 2] = new Vector3(0.5f * hullWidthMult, -0.8f, -0.3f * sternLengthMult);
        controlPointsSternL[3, 3] = new Vector3(0.5f * hullWidthMult, -0.8f, -0.3f * sternLengthMult);
        // reverse order for normal direction bc points were added the other direction (front to back)
        Vector3[,] tempL = new Vector3[4, 4];
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                tempL[i, j] = new Vector3(
                    controlPointsSternL[3 - i, j].x,
                    controlPointsSternL[3 - i, j].y,
                    controlPointsSternL[3 - i, j].z);
            }
        }
        controlPointsSternL = tempL;
        Mesh hullSternMeshL = getBezierPatchMesh(controlPointsSternL, 10, 10);

        // port side
        Vector3[,] controlPointsSternR = new Vector3[4, 4];
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                controlPointsSternR[3 - i, j] = new Vector3(
                    -controlPointsSternL[i, j].x + hullWidthMult,
                    controlPointsSternL[i, j].y,
                    controlPointsSternL[i, j].z);
            }
        }
        Mesh hullSternMeshR = getBezierPatchMesh(controlPointsSternR, 10, 10);

        // combine stern
        CombineInstance[] combinedStern = new CombineInstance[2];
        combinedStern[0].mesh = hullSternMeshL;
        combinedStern[0].transform = Matrix4x4.identity;
        combinedStern[1].mesh = hullSternMeshR;
        combinedStern[1].transform = Matrix4x4.identity;
        Mesh combinedSternMesh = new Mesh();
        combinedSternMesh.CombineMeshes(combinedStern);
        combinedSternMesh.RecalculateNormals();



        // final combine and create object 
        CombineInstance[] finalHull = new CombineInstance[3];
        finalHull[0].mesh = combinedStraightMesh;
        finalHull[0].transform = Matrix4x4.identity;
        finalHull[1].mesh = combinedBowMesh;
        finalHull[1].transform = Matrix4x4.identity;
        finalHull[2].mesh = combinedSternMesh;
        finalHull[2].transform = Matrix4x4.identity;
        Mesh hullMesh = new Mesh();
        hullMesh.CombineMeshes(finalHull);
        hullMesh.RecalculateNormals();

        shipObject = new GameObject("Ship");
        MeshFilter mf = shipObject.AddComponent<MeshFilter>();
        mf.mesh = hullMesh;
        MeshRenderer mr = shipObject.AddComponent<MeshRenderer>();
        mr.material.color = Color.gray;



        // create deck with three sections, one for bow, stern and straight
        // straight section is a flat plane
        Vector3[] straightDeckVertices = new Vector3[4];
        straightDeckVertices[0] = controlPointsL[0, 0];
        straightDeckVertices[1] = controlPointsR[3, 0]; // bc R was reversed on the first axis
        straightDeckVertices[2] = controlPointsR[3, 3];
        straightDeckVertices[3] = controlPointsL[0, 3];

        int[] straightDeckTriangles = new int[6] { 0, 2, 1, 0, 3, 2 };

        Vector2[] uvs = new Vector2[4];
        uvs[0] = new Vector2(0, 0);
        uvs[1] = new Vector2(1, 0);
        uvs[2] = new Vector2(1, 1);
        uvs[3] = new Vector2(0, 1);

        Mesh straightDeckMesh = new Mesh();
        straightDeckMesh.vertices = straightDeckVertices;
        straightDeckMesh.triangles = straightDeckTriangles;
        straightDeckMesh.uv = uvs;
        straightDeckMesh.RecalculateNormals();

        // bow deck section
        Vector3[,] controlPointsBowDeck = new Vector3[4, 4];
        controlPointsBowDeck[0, 0] = controlPointsBowR[3, 0];
        controlPointsBowDeck[0, 1] = controlPointsBowR[3, 1];
        controlPointsBowDeck[0, 2] = controlPointsBowR[3, 2];
        controlPointsBowDeck[0, 3] = controlPointsBowR[3, 3];

        controlPointsBowDeck[1, 0] = controlPointsBowR[3, 0] * 0.7f + controlPointsBowL[0, 0] * 0.3f;
        controlPointsBowDeck[1, 1] = controlPointsBowR[3, 1] * 0.7f + controlPointsBowL[0, 1] * 0.3f;
        controlPointsBowDeck[1, 2] = controlPointsBowR[3, 2] * 0.7f + controlPointsBowL[0, 2] * 0.3f;
        controlPointsBowDeck[1, 3] = controlPointsBowR[3, 3] * 0.7f + controlPointsBowL[0, 3] * 0.3f;

        controlPointsBowDeck[2, 0] = controlPointsBowR[3, 0] * 0.3f + controlPointsBowL[0, 0] * 0.7f;
        controlPointsBowDeck[2, 1] = controlPointsBowR[3, 1] * 0.3f + controlPointsBowL[0, 1] * 0.7f;
        controlPointsBowDeck[2, 2] = controlPointsBowR[3, 2] * 0.3f + controlPointsBowL[0, 2] * 0.7f;
        controlPointsBowDeck[2, 3] = controlPointsBowR[3, 3] * 0.3f + controlPointsBowL[0, 3] * 0.7f;

        controlPointsBowDeck[3, 0] = controlPointsBowL[0, 0];
        controlPointsBowDeck[3, 1] = controlPointsBowL[0, 1];
        controlPointsBowDeck[3, 2] = controlPointsBowL[0, 2];
        controlPointsBowDeck[3, 3] = controlPointsBowL[0, 3];

        Mesh bowDeckMesh = getBezierPatchMesh(controlPointsBowDeck, 10, 10);

        // stern deck section
        Vector3[,] controlPointsSternDeck = new Vector3[4, 4];
        controlPointsSternDeck[0, 0] = controlPointsSternL[3, 0];
        controlPointsSternDeck[0, 1] = controlPointsSternL[3, 1];
        controlPointsSternDeck[0, 2] = controlPointsSternL[3, 2];
        controlPointsSternDeck[0, 3] = controlPointsSternL[3, 3];

        controlPointsSternDeck[1, 0] = controlPointsSternL[3, 0] * 0.7f + controlPointsSternR[0, 0] * 0.3f;
        controlPointsSternDeck[1, 1] = controlPointsSternL[3, 1] * 0.7f + controlPointsSternR[0, 1] * 0.3f;
        controlPointsSternDeck[1, 2] = controlPointsSternL[3, 2] * 0.7f + controlPointsSternR[0, 2] * 0.3f;
        controlPointsSternDeck[1, 3] = controlPointsSternL[3, 3] * 0.7f + controlPointsSternR[0, 3] * 0.3f;

        controlPointsSternDeck[2, 0] = controlPointsSternL[3, 0] * 0.3f + controlPointsSternR[0, 0] * 0.7f;
        controlPointsSternDeck[2, 1] = controlPointsSternL[3, 1] * 0.3f + controlPointsSternR[0, 1] * 0.7f;
        controlPointsSternDeck[2, 2] = controlPointsSternL[3, 2] * 0.3f + controlPointsSternR[0, 2] * 0.7f;
        controlPointsSternDeck[2, 3] = controlPointsSternL[3, 3] * 0.3f + controlPointsSternR[0, 3] * 0.7f;

        controlPointsSternDeck[3, 0] = controlPointsSternR[0, 0];
        controlPointsSternDeck[3, 1] = controlPointsSternR[0, 1];
        controlPointsSternDeck[3, 2] = controlPointsSternR[0, 2];
        controlPointsSternDeck[3, 3] = controlPointsSternR[0, 3];

        Mesh sternDeckMesh = getBezierPatchMesh(controlPointsSternDeck, 10, 10);

        CombineInstance[] finalDeck = new CombineInstance[3];
        finalDeck[0].mesh = bowDeckMesh;
        finalDeck[0].transform = Matrix4x4.identity;
        finalDeck[1].mesh = straightDeckMesh;
        finalDeck[1].transform = Matrix4x4.identity;
        finalDeck[2].mesh = sternDeckMesh;
        finalDeck[2].transform = Matrix4x4.identity;
        Mesh deckMesh = new Mesh();
        deckMesh.CombineMeshes(finalDeck);
        deckMesh.RecalculateNormals();

        // deck
        deck = new GameObject("Deck");
        MeshFilter deckMf = deck.AddComponent<MeshFilter>();
        deckMf.mesh = deckMesh;
        MeshRenderer deckMr = deck.AddComponent<MeshRenderer>();
        deckMr.material.color = Color.yellow;
        deck.transform.parent = shipObject.transform;
    }

    // type 3, almost box like
    private void generateHull3()
    {
        // starboard side of straight section
        Vector3[,] controlPointsL = new Vector3[4, 4];
        controlPointsL[0, 0] = new Vector3(0.0f, 0.0f, 0.0f);
        controlPointsL[0, 1] = new Vector3(0.0f, 0.0f, 0.6f * hullLengthMult);
        controlPointsL[0, 2] = new Vector3(0.0f, 0.0f, 1.4f * hullLengthMult);
        controlPointsL[0, 3] = new Vector3(0.0f, 0.0f, 2.0f * hullLengthMult);

        controlPointsL[1, 0] = new Vector3(0.0f, -0.8f, 0.0f);
        controlPointsL[1, 1] = new Vector3(0.0f, -0.8f, 0.6f * hullLengthMult);
        controlPointsL[1, 2] = new Vector3(0.0f, -0.8f, 1.4f * hullLengthMult);
        controlPointsL[1, 3] = new Vector3(0.0f, -0.8f, 2.0f * hullLengthMult);

        controlPointsL[2, 0] = new Vector3(0.0f * hullWidthMult, -0.8f, 0.0f);
        controlPointsL[2, 1] = new Vector3(0.0f * hullWidthMult, -0.8f, 0.6f * hullLengthMult);
        controlPointsL[2, 2] = new Vector3(0.0f * hullWidthMult, -0.8f, 1.4f * hullLengthMult);
        controlPointsL[2, 3] = new Vector3(0.0f * hullWidthMult, -0.8f, 2.0f * hullLengthMult);

        controlPointsL[3, 0] = new Vector3(0.5f * hullWidthMult, -0.8f, 0.0f);
        controlPointsL[3, 1] = new Vector3(0.5f * hullWidthMult, -0.8f, 0.6f * hullLengthMult);
        controlPointsL[3, 2] = new Vector3(0.5f * hullWidthMult, -0.8f, 1.4f * hullLengthMult);
        controlPointsL[3, 3] = new Vector3(0.5f * hullWidthMult, -0.8f, 2.0f * hullLengthMult);

        Mesh hullStraightMeshR = getBezierPatchMesh(controlPointsL, 10, 10);

        // port side of straight section
        Vector3[,] controlPointsR = new Vector3[4, 4];
        // we can just copy right side and reverse the x
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                controlPointsR[3 - i, j] = new Vector3( // negative for correct normal direction
                    -controlPointsL[i, j].x + hullWidthMult,
                    controlPointsL[i, j].y,
                    controlPointsL[i, j].z);
            }
        }

        Mesh hullStraightMeshL = getBezierPatchMesh(controlPointsR, 10, 10);

        CombineInstance[] combinedStraight = new CombineInstance[2];
        combinedStraight[0].mesh = hullStraightMeshL;
        combinedStraight[0].transform = Matrix4x4.identity;
        combinedStraight[1].mesh = hullStraightMeshR;
        combinedStraight[1].transform = Matrix4x4.identity;
        Mesh combinedStraightMesh = new Mesh();
        combinedStraightMesh.CombineMeshes(combinedStraight);
        combinedStraightMesh.RecalculateNormals();



        // bow section
        Vector3[,] controlPointsBowL = new Vector3[4, 4];
        float bowOffset = 2.0f * hullLengthMult;
        // to make it a smooth join, we need to be aligned with the straight section
        controlPointsBowL[0, 0] = new Vector3(0.0f, 0.0f, bowOffset);
        controlPointsBowL[0, 1] = new Vector3(0.0f, 0.0f, bowOffset + 0.3f * bowLengthMult);
        controlPointsBowL[0, 2] = new Vector3(0.2f * hullWidthMult, 0.0f, bowOffset + .6f * bowLengthMult);
        controlPointsBowL[0, 3] = new Vector3(0.5f * hullWidthMult, 0.0f, bowOffset + 1.0f * bowLengthMult);

        controlPointsBowL[1, 0] = new Vector3(0.0f, -0.8f, bowOffset);
        controlPointsBowL[1, 1] = new Vector3(0.0f, -0.8f, bowOffset + .3f * bowLengthMult);
        controlPointsBowL[1, 2] = new Vector3(0.3f * hullWidthMult, -0.5f, bowOffset + .6f * bowLengthMult);
        controlPointsBowL[1, 3] = new Vector3(0.5f * hullWidthMult, -0.5f, bowOffset + .8f * bowLengthMult);

        controlPointsBowL[2, 0] = new Vector3(0.0f * hullWidthMult, -0.8f, bowOffset);
        controlPointsBowL[2, 1] = new Vector3(0.0f * hullWidthMult, -0.8f, bowOffset + .6f * bowLengthMult);
        controlPointsBowL[2, 2] = new Vector3(0.3f * hullWidthMult, -0.8f, bowOffset + .8f * bowLengthMult);
        controlPointsBowL[2, 3] = new Vector3(0.5f * hullWidthMult, -0.8f, bowOffset + .8f * bowLengthMult);

        controlPointsBowL[3, 0] = new Vector3(0.5f * hullWidthMult, -0.8f, bowOffset);
        controlPointsBowL[3, 1] = new Vector3(0.5f * hullWidthMult, -0.8f, bowOffset + .6f * bowLengthMult);
        controlPointsBowL[3, 2] = new Vector3(0.5f * hullWidthMult, -0.8f, bowOffset + .8f * bowLengthMult);
        controlPointsBowL[3, 3] = new Vector3(0.5f * hullWidthMult, -0.8f, bowOffset + .8f * bowLengthMult);

        Mesh hullBowMeshL = getBezierPatchMesh(controlPointsBowL, 10, 10);

        // port side of bow section
        Vector3[,] controlPointsBowR = new Vector3[4, 4];
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                controlPointsBowR[3 - i, j] = new Vector3(
                    -controlPointsBowL[i, j].x + hullWidthMult,
                    controlPointsBowL[i, j].y,
                    controlPointsBowL[i, j].z);
            }
        }

        Mesh hullBowMeshR = getBezierPatchMesh(controlPointsBowR, 10, 10);

        CombineInstance[] combinedBow = new CombineInstance[2];
        combinedBow[0].mesh = hullBowMeshL;
        combinedBow[0].transform = Matrix4x4.identity;
        combinedBow[1].mesh = hullBowMeshR;
        combinedBow[1].transform = Matrix4x4.identity;
        Mesh combinedBowMesh = new Mesh();
        combinedBowMesh.CombineMeshes(combinedBow);
        combinedBowMesh.RecalculateNormals();



        // stern section
        Vector3[,] controlPointsSternL = new Vector3[4, 4];
        controlPointsSternL[0, 0] = new Vector3(0.0f, 0.0f, 0.0f);
        controlPointsSternL[0, 1] = new Vector3(0.0f, 0.0f, -0.3f * sternLengthMult);
        controlPointsSternL[0, 2] = new Vector3(0.0f * hullWidthMult, 0.0f, -0.6f * sternLengthMult);
        controlPointsSternL[0, 3] = new Vector3(0.5f * hullWidthMult, 0.0f, -0.6f * sternLengthMult);

        controlPointsSternL[1, 0] = new Vector3(0.0f, -0.8f, 0.0f);
        controlPointsSternL[1, 1] = new Vector3(0.0f, -0.8f, -0.4f * sternLengthMult);
        controlPointsSternL[1, 2] = new Vector3(0.0f * hullWidthMult, -0.5f, -0.6f * sternLengthMult);
        controlPointsSternL[1, 3] = new Vector3(0.5f * hullWidthMult, -0.5f, -0.6f * sternLengthMult);

        controlPointsSternL[2, 0] = new Vector3(0.0f * hullWidthMult, -0.8f, 0.0f);
        controlPointsSternL[2, 1] = new Vector3(0.0f * hullWidthMult, -0.8f, -0.4f * sternLengthMult);
        controlPointsSternL[2, 2] = new Vector3(0.0f * hullWidthMult, -0.8f, -0.5f * sternLengthMult);
        controlPointsSternL[2, 3] = new Vector3(0.5f * hullWidthMult, -0.8f, -0.5f * sternLengthMult);

        controlPointsSternL[3, 0] = new Vector3(0.5f * hullWidthMult, -0.8f, 0.0f);
        controlPointsSternL[3, 1] = new Vector3(0.5f * hullWidthMult, -0.8f, -0.1f * sternLengthMult);
        controlPointsSternL[3, 2] = new Vector3(0.5f * hullWidthMult, -0.8f, -0.3f * sternLengthMult);
        controlPointsSternL[3, 3] = new Vector3(0.5f * hullWidthMult, -0.8f, -0.3f * sternLengthMult);
        // reverse order for normal direction bc points were added the other direction (front to back)
        Vector3[,] tempL = new Vector3[4, 4];
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                tempL[i, j] = new Vector3(
                    controlPointsSternL[3 - i, j].x,
                    controlPointsSternL[3 - i, j].y,
                    controlPointsSternL[3 - i, j].z);
            }
        }
        controlPointsSternL = tempL;
        Mesh hullSternMeshL = getBezierPatchMesh(controlPointsSternL, 10, 10);

        // port side
        Vector3[,] controlPointsSternR = new Vector3[4, 4];
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                controlPointsSternR[3 - i, j] = new Vector3(
                    -controlPointsSternL[i, j].x + hullWidthMult,
                    controlPointsSternL[i, j].y,
                    controlPointsSternL[i, j].z);
            }
        }
        Mesh hullSternMeshR = getBezierPatchMesh(controlPointsSternR, 10, 10);

        // combine stern
        CombineInstance[] combinedStern = new CombineInstance[2];
        combinedStern[0].mesh = hullSternMeshL;
        combinedStern[0].transform = Matrix4x4.identity;
        combinedStern[1].mesh = hullSternMeshR;
        combinedStern[1].transform = Matrix4x4.identity;
        Mesh combinedSternMesh = new Mesh();
        combinedSternMesh.CombineMeshes(combinedStern);
        combinedSternMesh.RecalculateNormals();



        // final combine and create object 
        CombineInstance[] finalHull = new CombineInstance[3];
        finalHull[0].mesh = combinedStraightMesh;
        finalHull[0].transform = Matrix4x4.identity;
        finalHull[1].mesh = combinedBowMesh;
        finalHull[1].transform = Matrix4x4.identity;
        finalHull[2].mesh = combinedSternMesh;
        finalHull[2].transform = Matrix4x4.identity;
        Mesh hullMesh = new Mesh();
        hullMesh.CombineMeshes(finalHull);
        hullMesh.RecalculateNormals();

        shipObject = new GameObject("Ship");
        MeshFilter mf = shipObject.AddComponent<MeshFilter>();
        mf.mesh = hullMesh;
        MeshRenderer mr = shipObject.AddComponent<MeshRenderer>();
        mr.material.color = Color.gray;



        // create deck with three sections, one for bow, stern and straight
        // straight section is a flat plane
        Vector3[] straightDeckVertices = new Vector3[4];
        straightDeckVertices[0] = controlPointsL[0, 0];
        straightDeckVertices[1] = controlPointsR[3, 0]; // bc R was reversed on the first axis
        straightDeckVertices[2] = controlPointsR[3, 3];
        straightDeckVertices[3] = controlPointsL[0, 3];

        int[] straightDeckTriangles = new int[6] { 0, 2, 1, 0, 3, 2 };

        Vector2[] uvs = new Vector2[4];
        uvs[0] = new Vector2(0, 0);
        uvs[1] = new Vector2(1, 0);
        uvs[2] = new Vector2(1, 1);
        uvs[3] = new Vector2(0, 1);

        Mesh straightDeckMesh = new Mesh();
        straightDeckMesh.vertices = straightDeckVertices;
        straightDeckMesh.triangles = straightDeckTriangles;
        straightDeckMesh.uv = uvs;
        straightDeckMesh.RecalculateNormals();

        // bow deck section
        Vector3[,] controlPointsBowDeck = new Vector3[4, 4];
        controlPointsBowDeck[0, 0] = controlPointsBowR[3, 0];
        controlPointsBowDeck[0, 1] = controlPointsBowR[3, 1];
        controlPointsBowDeck[0, 2] = controlPointsBowR[3, 2];
        controlPointsBowDeck[0, 3] = controlPointsBowR[3, 3];

        controlPointsBowDeck[1, 0] = controlPointsBowR[3, 0] * 0.7f + controlPointsBowL[0, 0] * 0.3f;
        controlPointsBowDeck[1, 1] = controlPointsBowR[3, 1] * 0.7f + controlPointsBowL[0, 1] * 0.3f;
        controlPointsBowDeck[1, 2] = controlPointsBowR[3, 2] * 0.7f + controlPointsBowL[0, 2] * 0.3f;
        controlPointsBowDeck[1, 3] = controlPointsBowR[3, 3] * 0.7f + controlPointsBowL[0, 3] * 0.3f;

        controlPointsBowDeck[2, 0] = controlPointsBowR[3, 0] * 0.3f + controlPointsBowL[0, 0] * 0.7f;
        controlPointsBowDeck[2, 1] = controlPointsBowR[3, 1] * 0.3f + controlPointsBowL[0, 1] * 0.7f;
        controlPointsBowDeck[2, 2] = controlPointsBowR[3, 2] * 0.3f + controlPointsBowL[0, 2] * 0.7f;
        controlPointsBowDeck[2, 3] = controlPointsBowR[3, 3] * 0.3f + controlPointsBowL[0, 3] * 0.7f;

        controlPointsBowDeck[3, 0] = controlPointsBowL[0, 0];
        controlPointsBowDeck[3, 1] = controlPointsBowL[0, 1];
        controlPointsBowDeck[3, 2] = controlPointsBowL[0, 2];
        controlPointsBowDeck[3, 3] = controlPointsBowL[0, 3];

        Mesh bowDeckMesh = getBezierPatchMesh(controlPointsBowDeck, 10, 10);

        // stern deck section
        Vector3[,] controlPointsSternDeck = new Vector3[4, 4];
        controlPointsSternDeck[0, 0] = controlPointsSternL[3, 0];
        controlPointsSternDeck[0, 1] = controlPointsSternL[3, 1];
        controlPointsSternDeck[0, 2] = controlPointsSternL[3, 2];
        controlPointsSternDeck[0, 3] = controlPointsSternL[3, 3];

        controlPointsSternDeck[1, 0] = controlPointsSternL[3, 0] * 0.7f + controlPointsSternR[0, 0] * 0.3f;
        controlPointsSternDeck[1, 1] = controlPointsSternL[3, 1] * 0.7f + controlPointsSternR[0, 1] * 0.3f;
        controlPointsSternDeck[1, 2] = controlPointsSternL[3, 2] * 0.7f + controlPointsSternR[0, 2] * 0.3f;
        controlPointsSternDeck[1, 3] = controlPointsSternL[3, 3] * 0.7f + controlPointsSternR[0, 3] * 0.3f;

        controlPointsSternDeck[2, 0] = controlPointsSternL[3, 0] * 0.3f + controlPointsSternR[0, 0] * 0.7f;
        controlPointsSternDeck[2, 1] = controlPointsSternL[3, 1] * 0.3f + controlPointsSternR[0, 1] * 0.7f;
        controlPointsSternDeck[2, 2] = controlPointsSternL[3, 2] * 0.3f + controlPointsSternR[0, 2] * 0.7f;
        controlPointsSternDeck[2, 3] = controlPointsSternL[3, 3] * 0.3f + controlPointsSternR[0, 3] * 0.7f;

        controlPointsSternDeck[3, 0] = controlPointsSternR[0, 0];
        controlPointsSternDeck[3, 1] = controlPointsSternR[0, 1];
        controlPointsSternDeck[3, 2] = controlPointsSternR[0, 2];
        controlPointsSternDeck[3, 3] = controlPointsSternR[0, 3];

        Mesh sternDeckMesh = getBezierPatchMesh(controlPointsSternDeck, 10, 10);

        CombineInstance[] finalDeck = new CombineInstance[3];
        finalDeck[0].mesh = bowDeckMesh;
        finalDeck[0].transform = Matrix4x4.identity;
        finalDeck[1].mesh = straightDeckMesh;
        finalDeck[1].transform = Matrix4x4.identity;
        finalDeck[2].mesh = sternDeckMesh;
        finalDeck[2].transform = Matrix4x4.identity;
        Mesh deckMesh = new Mesh();
        deckMesh.CombineMeshes(finalDeck);
        deckMesh.RecalculateNormals();

        // deck
        deck = new GameObject("Deck");
        MeshFilter deckMf = deck.AddComponent<MeshFilter>();
        deckMf.mesh = deckMesh;
        MeshRenderer deckMr = deck.AddComponent<MeshRenderer>();
        deckMr.material.color = Color.yellow;
        deck.transform.parent = shipObject.transform;
    }

    // superstructure will be on the front part of the straight section of the deck
    private void generateSuperstructure()
    {
        // calculate where we can place the superstructure
        float length = hullLengthMult * 2.0f * 0.5f * superstructureLengthMult; // half the straight section multiplied by length
        float height = 0.5f * superstructureHeightMult;
        float width = hullWidthMult * 0.8f;
        float hullFrontZ = hullLengthMult * 2.0f;
        
        Mesh superstructureMesh = null;
        // simple box shape extending the total length
        if (superstructureType == SuperstructureType.boxy)
        {
            Vector3[] vertices = new Vector3[5 * 4]; ;
            // front - 012, 023
            vertices[0] = new Vector3((hullWidthMult - width) / 2.0f, 0.0f, hullFrontZ);
            vertices[1] = new Vector3((hullWidthMult + width) / 2.0f, 0.0f, hullFrontZ);
            vertices[2] = new Vector3((hullWidthMult + width) / 2.0f, height, hullFrontZ);
            vertices[3] = new Vector3((hullWidthMult - width) / 2.0f, height, hullFrontZ);
            // left side
            vertices[4] = new Vector3((hullWidthMult - width) / 2.0f, 0.0f, hullFrontZ - length);
            vertices[5] = new Vector3((hullWidthMult - width) / 2.0f, 0.0f, hullFrontZ);
            vertices[6] = new Vector3((hullWidthMult - width) / 2.0f, height, hullFrontZ);
            vertices[7] = new Vector3((hullWidthMult - width) / 2.0f, height, hullFrontZ - length);
            // back
            vertices[8] = new Vector3((hullWidthMult + width) / 2.0f, 0.0f, hullFrontZ - length);
            vertices[9] = new Vector3((hullWidthMult - width) / 2.0f, 0.0f, hullFrontZ - length);
            vertices[10] = new Vector3((hullWidthMult - width) / 2.0f, height, hullFrontZ - length);
            vertices[11] = new Vector3((hullWidthMult + width) / 2.0f, height, hullFrontZ - length);
            // right side
            vertices[12] = new Vector3((hullWidthMult + width) / 2.0f, 0.0f, hullFrontZ);
            vertices[13] = new Vector3((hullWidthMult + width) / 2.0f, 0.0f, hullFrontZ - length);
            vertices[14] = new Vector3((hullWidthMult + width) / 2.0f, height, hullFrontZ - length);
            vertices[15] = new Vector3((hullWidthMult + width) / 2.0f, height, hullFrontZ);
            // top
            vertices[16] = new Vector3((hullWidthMult + width) / 2.0f, height, hullFrontZ - length);
            vertices[17] = new Vector3((hullWidthMult - width) / 2.0f, height, hullFrontZ - length);
            vertices[18] = new Vector3((hullWidthMult - width) / 2.0f, height, hullFrontZ);
            vertices[19] = new Vector3((hullWidthMult + width) / 2.0f, height, hullFrontZ);

            int[] triangles = {0,1,2, 0,2,3,    // front
                               4,5,6, 4,6,7,    // left
                               8,9,10, 8,10,11, // back
                               12,13,14, 12,14,15, // right
                               16,17,18, 16,18,19}; // top
            superstructureMesh = new Mesh();
            superstructureMesh.vertices = vertices;
            superstructureMesh.triangles = triangles;
            superstructureMesh.RecalculateNormals();
        }
        else if (superstructureType == SuperstructureType.rounded)
        {
            // single bezier patch that is curved w/ 2 front and back panels
            Vector3[,] controlPoints1 = new Vector3[4, 4];
            controlPoints1[0, 0] = new Vector3((hullWidthMult - width) / 2.0f, 0.0f, hullFrontZ); // front left
            controlPoints1[0, 1] = new Vector3((hullWidthMult - width) / 2.0f, 0.0f, hullFrontZ - length / 3.0f);
            controlPoints1[0, 2] = new Vector3((hullWidthMult - width) / 2.0f, 0.0f, hullFrontZ - 2.0f * length / 3.0f);
            controlPoints1[0, 3] = new Vector3((hullWidthMult - width) / 2.0f, 0.0f, hullFrontZ - length); // back left

            controlPoints1[1, 0] = new Vector3((hullWidthMult - width) / 2.0f, height, hullFrontZ);
            controlPoints1[1, 1] = new Vector3((hullWidthMult - width) / 2.0f, height, hullFrontZ - length / 3.0f);
            controlPoints1[1, 2] = new Vector3((hullWidthMult - width) / 2.0f, height, hullFrontZ - 2.0f * length / 3.0f);
            controlPoints1[1, 3] = new Vector3((hullWidthMult - width) / 2.0f, height, hullFrontZ - length);

            controlPoints1[2, 0] = new Vector3((hullWidthMult + width) / 2.0f, height, hullFrontZ);
            controlPoints1[2, 1] = new Vector3((hullWidthMult + width) / 2.0f, height, hullFrontZ - length / 3.0f);
            controlPoints1[2, 2] = new Vector3((hullWidthMult + width) / 2.0f, height, hullFrontZ - 2.0f * length / 3.0f);
            controlPoints1[2, 3] = new Vector3((hullWidthMult + width) / 2.0f, height, hullFrontZ - length);

            controlPoints1[3, 0] = new Vector3((hullWidthMult + width) / 2.0f, 0.0f, hullFrontZ); // front right
            controlPoints1[3, 1] = new Vector3((hullWidthMult + width) / 2.0f, 0.0f, hullFrontZ - length / 3.0f);
            controlPoints1[3, 2] = new Vector3((hullWidthMult + width) / 2.0f, 0.0f, hullFrontZ - 2.0f * length / 3.0f);
            controlPoints1[3, 3] = new Vector3((hullWidthMult + width) / 2.0f, 0.0f, hullFrontZ - length); // back right

            Mesh ssm1 = getBezierPatchMesh(controlPoints1, 10, 10);

            // front flat surface
            Vector3[,] controlPoints2 = new Vector3[4, 4];
            controlPoints2[0, 0] = controlPoints1[0, 0]; // left bottom
            controlPoints2[1, 0] = new Vector3((hullWidthMult - width / 3.0f) / 2.0f, 0.0f, hullFrontZ);
            controlPoints2[2, 0] = new Vector3((hullWidthMult + width / 3.0f) / 2.0f, 0.0f, hullFrontZ);
            controlPoints2[3, 0] = controlPoints1[3, 0]; // right bottom

            controlPoints2[0, 1] = controlPoints1[0, 0]; // left bottom
            controlPoints2[1, 1] = new Vector3((hullWidthMult - width / 3.0f) / 2.0f, 0.0f, hullFrontZ);
            controlPoints2[2, 1] = new Vector3((hullWidthMult + width / 3.0f) / 2.0f, 0.0f, hullFrontZ);
            controlPoints2[3, 1] = controlPoints1[3, 0]; // right bottom

            controlPoints2[0, 2] = controlPoints1[0, 0]; // left bottom
            controlPoints2[1, 2] = new Vector3((hullWidthMult - width / 3.0f) / 2.0f, 0.0f, hullFrontZ);
            controlPoints2[2, 2] = new Vector3((hullWidthMult + width / 3.0f) / 2.0f, 0.0f, hullFrontZ);
            controlPoints2[3, 2] = controlPoints1[3, 0]; // right bottom

            controlPoints2[0, 3] = controlPoints1[0, 0]; //left bottom
            controlPoints2[1, 3] = controlPoints1[1, 0];
            controlPoints2[2, 3] = controlPoints1[2, 0];
            controlPoints2[3, 3] = controlPoints1[3, 0]; // right bottom

            Mesh ssm2 = getBezierPatchMesh(controlPoints2, 10, 10);

            Vector3[,] controlPoints3 = new Vector3[4, 4];
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    controlPoints3[i, j] = new Vector3(
                        controlPoints2[3 - i, j].x,
                        controlPoints2[3 - i, j].y,
                        controlPoints2[3 - i, j].z - length);
                }
            }

            Mesh ssm3 = getBezierPatchMesh(controlPoints3, 10, 10);

            CombineInstance[] combinedSSM = new CombineInstance[3];
            combinedSSM[0].mesh = ssm1;
            combinedSSM[0].transform = Matrix4x4.identity;
            combinedSSM[1].mesh = ssm2;
            combinedSSM[1].transform = Matrix4x4.identity;
            combinedSSM[2].mesh = ssm3;
            combinedSSM[2].transform = Matrix4x4.identity;
            superstructureMesh = new Mesh();
            superstructureMesh.CombineMeshes(combinedSSM);
            superstructureMesh.RecalculateNormals();
        }
        else if (superstructureType == SuperstructureType.slanted)
        {
            // 3 bezier patches, one for front (slanted), one for top, one for back
            Vector3[,] controlPoints1 = new Vector3[4, 4];
            // start from bottom right side, go horizontal
            controlPoints1[0, 0] = new Vector3((hullWidthMult + width) / 2.0f, 0.0f, hullFrontZ - length);
            controlPoints1[0, 1] = new Vector3((hullWidthMult + width) / 2.0f, 0.0f, hullFrontZ);
            controlPoints1[0, 2] = new Vector3((hullWidthMult - width) / 2.0f, 0.0f, hullFrontZ);
            controlPoints1[0, 3] = new Vector3((hullWidthMult - width) / 2.0f, 0.0f, hullFrontZ - length);

            controlPoints1[1, 0] = new Vector3((hullWidthMult + width) / 2.0f, height * 0.3f, hullFrontZ - length);
            controlPoints1[1, 1] = new Vector3((hullWidthMult + width) / 2.0f, height * 0.3f, hullFrontZ - 0.1f * length);
            controlPoints1[1, 2] = new Vector3((hullWidthMult - width) / 2.0f, height * 0.3f, hullFrontZ - 0.1f * length);
            controlPoints1[1, 3] = new Vector3((hullWidthMult - width) / 2.0f, height * 0.3f, hullFrontZ - length);

            controlPoints1[2, 0] = new Vector3((hullWidthMult + width) / 2.0f, height * 0.7f, hullFrontZ - length);
            controlPoints1[2, 1] = new Vector3((hullWidthMult + width) / 2.0f, height * 0.7f, hullFrontZ - 0.2f * length);
            controlPoints1[2, 2] = new Vector3((hullWidthMult - width) / 2.0f, height * 0.7f, hullFrontZ - 0.2f * length);
            controlPoints1[2, 3] = new Vector3((hullWidthMult - width) / 2.0f, height * 0.7f, hullFrontZ - length);

            controlPoints1[3, 0] = new Vector3((hullWidthMult + width) / 2.0f, height, hullFrontZ - length);
            controlPoints1[3, 1] = new Vector3((hullWidthMult + width) / 2.0f, height, hullFrontZ - 0.3f * length);
            controlPoints1[3, 2] = new Vector3((hullWidthMult - width) / 2.0f, height, hullFrontZ - 0.3f * length);
            controlPoints1[3, 3] = new Vector3((hullWidthMult - width) / 2.0f, height, hullFrontZ - length);

            Mesh ssm1 = getBezierPatchMesh(controlPoints1, 10, 10);

            //top
            Vector3[,] controlPoints2 = new Vector3[4, 4];
            controlPoints2[0, 0] = controlPoints1[3, 3];
            controlPoints2[0, 1] = controlPoints1[3, 3];
            controlPoints2[0, 2] = controlPoints1[3, 0];
            controlPoints2[0, 3] = controlPoints1[3, 0];

            controlPoints2[1, 0] = controlPoints1[3, 3];
            controlPoints2[1, 1] = controlPoints1[3, 3];
            controlPoints2[1, 2] = controlPoints1[3, 0];
            controlPoints2[1, 3] = controlPoints1[3, 0];

            controlPoints2[2, 0] = controlPoints1[3, 3];
            controlPoints2[2, 1] = controlPoints1[3, 3];
            controlPoints2[2, 2] = controlPoints1[3, 0];
            controlPoints2[2, 3] = controlPoints1[3, 0];

            controlPoints2[3, 0] = controlPoints1[3, 3];
            controlPoints2[3, 1] = controlPoints1[3, 2];
            controlPoints2[3, 2] = controlPoints1[3, 1];
            controlPoints2[3, 3] = controlPoints1[3, 0];

            Mesh ssm2 = getBezierPatchMesh(controlPoints2, 10, 10);

            // back can be constructed simply using triangles
            Vector3[] backVertices = new Vector3[4];
            backVertices[0] = controlPoints1[0, 0];
            backVertices[1] = controlPoints1[0, 3];
            backVertices[2] = controlPoints1[3, 0];
            backVertices[3] = controlPoints1[3, 3];

            int[] backTriangles = { 0, 1, 3, 0, 3, 2 };
            Mesh ssm3 = new Mesh();
            ssm3.vertices = backVertices;
            ssm3.triangles = backTriangles;
            ssm3.RecalculateNormals();

            CombineInstance[] combinedSSM = new CombineInstance[3];
            combinedSSM[0].mesh = ssm1;
            combinedSSM[0].transform = Matrix4x4.identity;
            combinedSSM[1].mesh = ssm2;
            combinedSSM[1].transform = Matrix4x4.identity;
            combinedSSM[2].mesh = ssm3;
            combinedSSM[2].transform = Matrix4x4.identity;
            superstructureMesh = new Mesh();
            superstructureMesh.CombineMeshes(combinedSSM);
            superstructureMesh.RecalculateNormals();
        }

        GameObject superstructure = new GameObject("Superstructure");
        MeshFilter mf = superstructure.AddComponent<MeshFilter>();
        mf.mesh = superstructureMesh;
        MeshRenderer mr = superstructure.AddComponent<MeshRenderer>();
        mr.material.color = ssColor;
        superstructure.transform.parent = shipObject.transform;
        

    }

    private void generateSail()
    {
        // sail will be in the middle of the back half of the straight section
        // base of the sail will have height based on the superstructure
        Vector3 sailBasePos = new Vector3(hullWidthMult / 2.0f, 0.0f, hullLengthMult * 2.0f * 0.4f);
        Vector3 sailLowerBarPos = new Vector3(hullWidthMult / 2.0f, superstructureHeightMult * 0.5f + 0.1f * sailHeightMult, hullLengthMult * 2.0f * 0.4f);
        Vector3 sailBaseTopPos = new Vector3(hullWidthMult / 2.0f, superstructureHeightMult * 0.5f + sailHeightMult, hullLengthMult * 2.0f * 0.4f);
        int resolution = 8; // 8 vertices for cylinder
        float sailRadius = 0.05f;
        Mesh sailStructureMesh = null;
        Mesh sailMesh = null;

        Vector3[] baseVertices = new Vector3[resolution * 2];
        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < resolution; j++)
            {
                float angle = 2.0f * Mathf.PI * j / resolution;
                float x = Mathf.Cos(angle) * sailRadius + sailBasePos.x;
                float z = Mathf.Sin(angle) * sailRadius + sailBasePos.z;
                float y = sailBasePos.y + i * (sailBaseTopPos.y - sailBasePos.y);
                baseVertices[i * resolution + j] = new Vector3(x, y, z);
            }
        }

        int[] baseTriangles = new int[(resolution * 2) * 3];
        for (int i = 0; i < resolution; i++)
        {
            int topI = i + resolution;
            int nextI = (i + 1) % resolution;
            int nextTopI = nextI + resolution;
            // side triangles
            baseTriangles[i * 6 + 0] = i;
            baseTriangles[i * 6 + 1] = topI;
            baseTriangles[i * 6 + 2] = nextI;

            baseTriangles[i * 6 + 3] = nextI;
            baseTriangles[i * 6 + 4] = topI;
            baseTriangles[i * 6 + 5] = nextTopI;
        }

        Mesh sailBaseMesh = new Mesh();
        sailBaseMesh.vertices = baseVertices;
        sailBaseMesh.triangles = baseTriangles;
        sailBaseMesh.RecalculateNormals();

        if (sailType == SailType.square)
        {
            float sailWidth = sailSizeMult * 2.0f;
            // create horizontal lower bar
            Vector3[] lowerBarVertices = new Vector3[resolution * 2];
            // should go widthwise at the height of the lower bar position
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < resolution; j++)
                {
                    float angle = 2.0f * Mathf.PI * j / resolution;
                    float x = i * sailWidth + sailLowerBarPos.x - sailWidth / 2.0f;
                    float z = Mathf.Cos(angle) * sailRadius + sailLowerBarPos.z;
                    float y = Mathf.Sin(angle) * sailRadius + sailLowerBarPos.y;
                    lowerBarVertices[i * resolution + j] = new Vector3(x, y, z);
                }
            }

            int[] lowerBarTriangles = new int[(resolution * 2) * 3];
            for (int i = 0; i < resolution; i++)
            {
                int topI = i + resolution;
                int nextI = (i + 1) % resolution;
                int nextTopI = nextI + resolution;
                // side triangles
                lowerBarTriangles[i * 6 + 0] = i;
                lowerBarTriangles[i * 6 + 1] = topI;
                lowerBarTriangles[i * 6 + 2] = nextI;

                lowerBarTriangles[i * 6 + 3] = nextI;
                lowerBarTriangles[i * 6 + 4] = topI;
                lowerBarTriangles[i * 6 + 5] = nextTopI;
            }

            Mesh sailLowerBarMesh = new Mesh();
            sailLowerBarMesh.vertices = lowerBarVertices;
            sailLowerBarMesh.triangles = lowerBarTriangles;
            sailLowerBarMesh.RecalculateNormals();

            // create cap ends for lower bar
            Vector3[] lowerBarCapVertices = new Vector3[resolution * 2];
            for (int i = 0; i < resolution; i++)
            {
                lowerBarCapVertices[i] = lowerBarVertices[i]; // left end
                lowerBarCapVertices[i + resolution] = lowerBarVertices[i + resolution]; // right end
            }
            int[] lowerBarCapTriangles = new int[6 * 2 * 3]; //2 caps with 6 triangles each
            // left cap
            for (int i = 0; i < 6; i++)
            {
                int index = 3 * i;
                lowerBarCapTriangles[index] = 0;
                lowerBarCapTriangles[index + 1] = i + 1;
                lowerBarCapTriangles[index + 2] = i + 2;
            }
            // right cap
            for (int i = 0; i < 6; i++)
            {
                int index = 3 * (i + 6);
                lowerBarCapTriangles[index] = resolution;
                lowerBarCapTriangles[index + 1] = resolution + i + 2;
                lowerBarCapTriangles[index + 2] = resolution + i + 1;
            }

            Mesh sailLowerBarCapMesh = new Mesh();
            sailLowerBarCapMesh.vertices = lowerBarCapVertices;
            sailLowerBarCapMesh.triangles = lowerBarCapTriangles;
            sailLowerBarCapMesh.RecalculateNormals();

            // upper bar
            Vector3[] upperBarVertices = new Vector3[resolution * 2];
            // can copy from lower bar, change y positions
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < resolution; j++)
                {
                    Vector3 lowerVertex = lowerBarVertices[i * resolution + j];
                    upperBarVertices[i * resolution + j] = new Vector3(lowerVertex.x, lowerVertex.y + (sailBaseTopPos.y - sailLowerBarPos.y), lowerVertex.z);
                }
            }
            Mesh sailUpperBarMesh = new Mesh();
            sailUpperBarMesh.vertices = upperBarVertices;
            sailUpperBarMesh.triangles = lowerBarTriangles; // order is the same
            sailUpperBarMesh.RecalculateNormals();

            // upper caps
            Vector3[] upperBarCapVertices = new Vector3[resolution * 2];
            for (int i = 0; i < resolution; i++)
            {
                upperBarCapVertices[i] = upperBarVertices[i]; // left end
                upperBarCapVertices[i + resolution] = upperBarVertices[i + resolution]; // right end
            }
            Mesh sailUpperBarCapMesh = new Mesh();
            sailUpperBarCapMesh.vertices = upperBarCapVertices;
            sailUpperBarCapMesh.triangles = lowerBarCapTriangles;
            sailUpperBarCapMesh.RecalculateNormals();


            CombineInstance[] combinedSailStructure = new CombineInstance[5];
            combinedSailStructure[0].mesh = sailBaseMesh;
            combinedSailStructure[0].transform = Matrix4x4.identity;
            combinedSailStructure[1].mesh = sailLowerBarMesh;
            combinedSailStructure[1].transform = Matrix4x4.identity;
            combinedSailStructure[2].mesh = sailUpperBarMesh;
            combinedSailStructure[2].transform = Matrix4x4.identity;
            combinedSailStructure[3].mesh = sailLowerBarCapMesh;
            combinedSailStructure[3].transform = Matrix4x4.identity;
            combinedSailStructure[4].mesh = sailUpperBarCapMesh;
            combinedSailStructure[4].transform = Matrix4x4.identity;
            sailStructureMesh = new Mesh();
            sailStructureMesh.CombineMeshes(combinedSailStructure);
            sailStructureMesh.RecalculateNormals();



            //sail itself
            Vector3 lowerLeft = lowerBarVertices[0];
            Vector3 lowerRight = lowerBarVertices[resolution];
            Vector3 upperLeft = upperBarVertices[0];
            Vector3 upperRight = upperBarVertices[resolution];

            Vector3[,] sailControlPoints = new Vector3[4, 4];
            sailControlPoints[0, 0] = lowerLeft;
            sailControlPoints[0, 1] = lowerLeft * 0.7f + lowerRight * 0.3f;
            sailControlPoints[0, 2] = lowerLeft * 0.3f + lowerRight * 0.7f;
            sailControlPoints[0, 3] = lowerRight;

            sailControlPoints[1, 0] = new Vector3(lowerLeft.x, lowerLeft.y * 0.7f + lowerLeft.y * 0.3f, lowerLeft.z + sailSizeMult * 0.1f);
            sailControlPoints[1, 1] = new Vector3(lowerLeft.x * 0.7f + lowerRight.x * 0.3f, lowerLeft.y * 0.7f + lowerLeft.y * 0.3f, lowerLeft.z + sailSizeMult * 0.3f);
            sailControlPoints[1, 2] = new Vector3(lowerLeft.x * 0.3f + lowerRight.x * 0.7f, lowerLeft.y * 0.7f + lowerLeft.y * 0.3f, lowerLeft.z + sailSizeMult * 0.3f);
            sailControlPoints[1, 3] = new Vector3(lowerRight.x, lowerRight.y * 0.7f + lowerRight.y * 0.3f, lowerRight.z + sailSizeMult * 0.1f);

            sailControlPoints[2, 0] = new Vector3(upperLeft.x, upperLeft.y * 0.3f + upperLeft.y * 0.7f, upperLeft.z + sailSizeMult * 0.1f);
            sailControlPoints[2, 1] = new Vector3(upperLeft.x * 0.7f + upperRight.x * 0.3f, upperLeft.y * 0.3f + upperLeft.y * 0.7f, upperLeft.z + sailSizeMult * 0.3f);
            sailControlPoints[2, 2] = new Vector3(upperLeft.x * 0.3f + upperRight.x * 0.7f, upperLeft.y * 0.3f + upperLeft.y * 0.7f, upperLeft.z + sailSizeMult * 0.3f);
            sailControlPoints[2, 3] = new Vector3(upperRight.x, upperRight.y * 0.3f + upperRight.y * 0.7f, upperRight.z + sailSizeMult * 0.1f);

            sailControlPoints[3, 0] = upperLeft;
            sailControlPoints[3, 1] = upperLeft * 0.7f + upperRight * 0.3f;
            sailControlPoints[3, 2] = upperLeft * 0.3f + upperRight * 0.7f;
            sailControlPoints[3, 3] = upperRight;

            Mesh sailMesh1 = getBezierPatchMesh(sailControlPoints, 10, 10);

            //reverse to get other direction
            Vector3[,] sailControlPoints2 = new Vector3[4, 4];
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    sailControlPoints2[i, j] = sailControlPoints[3 - i, j];
                }
            }
            Mesh sailMesh2 = getBezierPatchMesh(sailControlPoints2, 10, 10);


            CombineInstance[] combinedSailMesh = new CombineInstance[2];
            combinedSailMesh[0].mesh = sailMesh1;
            combinedSailMesh[0].transform = Matrix4x4.identity;
            combinedSailMesh[1].mesh = sailMesh2;
            combinedSailMesh[1].transform = Matrix4x4.identity;
            sailMesh = new Mesh();
            sailMesh.CombineMeshes(combinedSailMesh);
            sailMesh.RecalculateNormals();
            
        }
        else if (sailType == SailType.triangular)
        {
            float sailLength = sailSizeMult * 2.0f;
            // single triangle sail
            // one lower bar from base backwards
            Vector3[] lowerBarVertices = new Vector3[resolution * 2];
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < resolution; j++)
                {
                    float angle = 2.0f * Mathf.PI * j / resolution;
                    float x = Mathf.Cos(angle) * sailRadius + sailLowerBarPos.x;
                    float z = -i * sailLength + sailLowerBarPos.z;
                    float y = Mathf.Sin(angle) * sailRadius + sailLowerBarPos.y;
                    lowerBarVertices[i * resolution + j] = new Vector3(x, y, z);
                }
            }
            int[] lowerBarTriangles = new int[resolution * 2 * 3];
            for (int i = 0; i < resolution; i++)
            {
                int topI = i + resolution;
                int nextI = (i + 1) % resolution;
                int nextTopI = nextI + resolution;
                // side triangles
                lowerBarTriangles[i * 6 + 0] = i;
                lowerBarTriangles[i * 6 + 1] = topI;
                lowerBarTriangles[i * 6 + 2] = nextI;

                lowerBarTriangles[i * 6 + 3] = nextI;
                lowerBarTriangles[i * 6 + 4] = topI;
                lowerBarTriangles[i * 6 + 5] = nextTopI;
            }
            Mesh sailLowerBarMesh = new Mesh();
            sailLowerBarMesh.vertices = lowerBarVertices;
            sailLowerBarMesh.triangles = lowerBarTriangles;
            sailLowerBarMesh.RecalculateNormals();

            //cap for lower bar
            Vector3[] lowerBarCapVertices = new Vector3[resolution];
            for (int i = 0; i < resolution; i++)
            {
                lowerBarCapVertices[i] = lowerBarVertices[i + resolution];
            }
            int[] lowerBarCapTriangles = new int[6 * 3];
            for (int i = 0; i < 6; i++)
            {
                int index = 3 * i;
                lowerBarCapTriangles[index] = 0;
                lowerBarCapTriangles[index + 1] = i + 2;
                lowerBarCapTriangles[index + 2] = i + 1;
            }
            Mesh sailLowerBarCapMesh = new Mesh();
            sailLowerBarCapMesh.vertices = lowerBarCapVertices;
            sailLowerBarCapMesh.triangles = lowerBarCapTriangles;
            sailLowerBarCapMesh.RecalculateNormals();


            // cap for upper bar
            Vector3[] baseBarCapVertices = new Vector3[resolution];
            for (int i = 0; i < resolution; i++)
            {
                baseBarCapVertices[i] = baseVertices[i + resolution];
            }
            int[] baseBarCapTriangles = new int[6 * 3];
            for (int i = 0; i < 6; i++)
            {
                int index = 3 * i;
                baseBarCapTriangles[index] = 0;
                baseBarCapTriangles[index + 1] = i + 2;
                baseBarCapTriangles[index + 2] = i + 1;
            }
            Mesh sailBaseCapMesh = new Mesh();
            sailBaseCapMesh.vertices = baseBarCapVertices;
            sailBaseCapMesh.triangles = baseBarCapTriangles;
            sailBaseCapMesh.RecalculateNormals();

            //combine
            CombineInstance[] combinedSailStructure = new CombineInstance[4];
            combinedSailStructure[0].mesh = sailBaseMesh;
            combinedSailStructure[0].transform = Matrix4x4.identity;
            combinedSailStructure[1].mesh = sailLowerBarMesh;
            combinedSailStructure[1].transform = Matrix4x4.identity;
            combinedSailStructure[2].mesh = sailLowerBarCapMesh;
            combinedSailStructure[2].transform = Matrix4x4.identity;
            combinedSailStructure[3].mesh = sailBaseCapMesh;
            combinedSailStructure[3].transform = Matrix4x4.identity;
            sailStructureMesh = new Mesh();
            sailStructureMesh.CombineMeshes(combinedSailStructure);
            sailStructureMesh.RecalculateNormals();

            // sail itself, should be connected base and lower bar
            Vector3 end = new Vector3(sailLowerBarPos.x, sailLowerBarPos.y, sailLowerBarPos.z - sailLength);
            Vector3 top = sailBaseTopPos;
            Vector3 cross = sailLowerBarPos;
            Vector3 cp1 = cross * 0.3f + end * 0.7f;
            Vector3 cp2 = cross * 0.7f + end * 0.3f;
            Vector3[,] sailControlPoints = new Vector3[4, 4];
            sailControlPoints[0, 0] = cross;
            sailControlPoints[0, 1] = cp1;
            sailControlPoints[0, 2] = cp2;
            sailControlPoints[0, 3] = end;

            sailControlPoints[1, 0] = new Vector3(cross.x + sailSizeMult * 0.1f, cross.y * 0.7f + top.y * 0.3f, cross.z);
            sailControlPoints[1, 1] = new Vector3(cp1.x + sailSizeMult * 0.3f, cp1.y * 0.3f + top.y * 0.3f, cp1.z);
            sailControlPoints[1, 2] = new Vector3(cp2.x + sailSizeMult * 0.3f, cp2.y * 0.3f + top.y * 0.3f, cp2.z);
            sailControlPoints[1, 3] = new Vector3(end.x + sailSizeMult * 0.1f, end.y * 0.3f + top.y * 0.3f, end.z);

            sailControlPoints[2, 0] = new Vector3(cross.x + sailSizeMult * 0.1f, cross.y * 0.3f + top.y * 0.7f, cross.z);
            sailControlPoints[2, 1] = new Vector3(cp1.x + sailSizeMult * 0.3f, cp1.y * 0.3f + top.y * 0.3f, cp1.z);
            sailControlPoints[2, 2] = new Vector3(cp2.x + sailSizeMult * 0.3f, cp2.y * 0.3f + top.y * 0.3f, cp2.z);
            sailControlPoints[2, 3] = new Vector3(end.x + sailSizeMult * 0.1f, end.y * 0.3f + top.y * 0.3f, end.z);

            sailControlPoints[3, 0] = top;
            sailControlPoints[3, 1] = top;
            sailControlPoints[3, 2] = top;
            sailControlPoints[3, 3] = top;

            Mesh sailMesh1 = getBezierPatchMesh(sailControlPoints, 10, 10);

            //reverse to get other direction
            Vector3[,] sailControlPoints2 = new Vector3[4, 4];
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    sailControlPoints2[i, j] = sailControlPoints[3 - i, j];
                }
            }
            Mesh sailMesh2 = getBezierPatchMesh(sailControlPoints2, 10, 10);

            CombineInstance[] combinedSailMesh = new CombineInstance[2];
            combinedSailMesh[0].mesh = sailMesh1;
            combinedSailMesh[0].transform = Matrix4x4.identity;
            combinedSailMesh[1].mesh = sailMesh2;
            combinedSailMesh[1].transform = Matrix4x4.identity;
            sailMesh = new Mesh();
            sailMesh.CombineMeshes(combinedSailMesh);
            sailMesh.RecalculateNormals();
        }
        else if (sailType == SailType.dTriangle)
        {
            float sailLength = sailSizeMult * 2.0f;
            // double triangle sail
            // lower bar
            Vector3[] lowerBarVertices = new Vector3[resolution * 2];
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < resolution; j++)
                {
                    float angle = 2.0f * Mathf.PI * j / resolution;
                    float x = Mathf.Cos(angle) * sailRadius + sailLowerBarPos.x;
                    float z = -1.3f * i * sailLength + sailLowerBarPos.z + sailLength * 0.8f;
                    float y = Mathf.Sin(angle) * sailRadius + sailLowerBarPos.y;
                    lowerBarVertices[i * resolution + j] = new Vector3(x, y, z);
                }
            }
            int[] lowerBarTriangles = new int[resolution * 2 * 3];
            for (int i = 0; i < resolution; i++)
            {
                int topI = i + resolution;
                int nextI = (i + 1) % resolution;
                int nextTopI = nextI + resolution;
                // side triangles
                lowerBarTriangles[i * 6 + 0] = i;
                lowerBarTriangles[i * 6 + 1] = topI;
                lowerBarTriangles[i * 6 + 2] = nextI;

                lowerBarTriangles[i * 6 + 3] = nextI;
                lowerBarTriangles[i * 6 + 4] = topI;
                lowerBarTriangles[i * 6 + 5] = nextTopI;
            }
            Mesh sailLowerBarMesh = new Mesh();
            sailLowerBarMesh.vertices = lowerBarVertices;
            sailLowerBarMesh.triangles = lowerBarTriangles;
            sailLowerBarMesh.RecalculateNormals();
            // cap for lower bar
            Vector3[] lowerBarCapVertices = new Vector3[resolution * 2];
            for (int i = 0; i < resolution; i++)
            {
                lowerBarCapVertices[i] = lowerBarVertices[i];
                lowerBarCapVertices[i + resolution] = lowerBarVertices[i + resolution];
            }
            int[] lowerBarCapTriangles = new int[6 * 2 * 3];
            // left cap
            for (int i = 0; i < 6; i++)
            {
                int index = 3 * i;
                lowerBarCapTriangles[index] = 0;
                lowerBarCapTriangles[index + 1] = i + 1;
                lowerBarCapTriangles[index + 2] = i + 2;
            }
            // right cap
            for (int i = 0; i < 6; i++)
            {
                int index = 3 * (i + 6);
                lowerBarCapTriangles[index] = resolution;
                lowerBarCapTriangles[index + 1] = resolution + i + 2;
                lowerBarCapTriangles[index + 2] = resolution + i + 1;
            }
            Mesh sailLowerBarCapMesh = new Mesh();
            sailLowerBarCapMesh.vertices = lowerBarCapVertices;
            sailLowerBarCapMesh.triangles = lowerBarCapTriangles;
            sailLowerBarCapMesh.RecalculateNormals();

            // cap for upper bar
            Vector3[] baseBarCapVertices = new Vector3[resolution];
            for (int i = 0; i < resolution; i++)
            {
                baseBarCapVertices[i] = baseVertices[i + resolution];
            }
            int[] baseBarCapTriangles = new int[6 * 3];
            for (int i = 0; i < 6; i++)
            {
                int index = 3 * i;
                baseBarCapTriangles[index] = 0;
                baseBarCapTriangles[index + 1] = i + 2;
                baseBarCapTriangles[index + 2] = i + 1;
            }
            Mesh sailBaseCapMesh = new Mesh();
            sailBaseCapMesh.vertices = baseBarCapVertices;
            sailBaseCapMesh.triangles = baseBarCapTriangles;
            sailBaseCapMesh.RecalculateNormals();

            //combine 
            CombineInstance[] combinedSailStructure = new CombineInstance[4];
            combinedSailStructure[0].mesh = sailBaseMesh;
            combinedSailStructure[0].transform = Matrix4x4.identity;
            combinedSailStructure[1].mesh = sailLowerBarMesh;
            combinedSailStructure[1].transform = Matrix4x4.identity;
            combinedSailStructure[2].mesh = sailLowerBarCapMesh;
            combinedSailStructure[2].transform = Matrix4x4.identity;
            combinedSailStructure[3].mesh = sailBaseCapMesh;
            combinedSailStructure[3].transform = Matrix4x4.identity;
            sailStructureMesh = new Mesh();
            sailStructureMesh.CombineMeshes(combinedSailStructure);
            sailStructureMesh.RecalculateNormals();

            // there are two triangluar sails
            // front sail
            Vector3 top = sailBaseTopPos;
            Vector3 cross = sailLowerBarPos;
            Vector3 front = new Vector3(sailLowerBarPos.x, sailLowerBarPos.y, cross.z + sailLength * 0.8f);
            Vector3 cp1 = cross * 0.3f + front * 0.7f;
            Vector3 cp2 = cross * 0.7f + front * 0.3f;
            Vector3[,] sailControlPoints1 = new Vector3[4, 4];
            sailControlPoints1[0, 0] = cross;
            sailControlPoints1[0, 1] = cp1;
            sailControlPoints1[0, 2] = cp2;
            sailControlPoints1[0, 3] = front;

            sailControlPoints1[1, 0] = new Vector3(cross.x + sailSizeMult * 0.1f, cross.y * 0.3f + top.y * 0.3f, cross.z + 0.1f);
            sailControlPoints1[1, 1] = new Vector3(cp1.x + sailSizeMult * 0.3f, cp1.y * 0.3f + top.y * 0.3f, cp1.z);
            sailControlPoints1[1, 2] = new Vector3(cp2.x + sailSizeMult * 0.3f, cp2.y * 0.3f + top.y * 0.3f, cp2.z);
            sailControlPoints1[1, 3] = new Vector3(front.x + sailSizeMult * 0.1f, front.y * 0.3f + top.y * 0.3f, front.z);

            sailControlPoints1[2, 0] = new Vector3(cross.x + sailSizeMult * 0.1f, cross.y * 0.3f + top.y * 0.3f, cross.z + 0.1f);
            sailControlPoints1[2, 1] = new Vector3(cp1.x + sailSizeMult * 0.3f, cp1.y * 0.3f + top.y * 0.3f, cp1.z);
            sailControlPoints1[2, 2] = new Vector3(cp2.x + sailSizeMult * 0.3f, cp2.y * 0.3f + top.y * 0.3f, cp2.z);
            sailControlPoints1[2, 3] = new Vector3(front.x + sailSizeMult * 0.1f, front.y * 0.3f + top.y * 0.3f, front.z);

            sailControlPoints1[3, 0] = top;
            sailControlPoints1[3, 1] = top;
            sailControlPoints1[3, 2] = top;
            sailControlPoints1[3, 3] = top;

            Mesh sailMesh1 = getBezierPatchMesh(sailControlPoints1, 10, 10);

            //reverse
            Vector3[,] sailControlPoints2 = new Vector3[4, 4];
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    sailControlPoints2[i, j] = sailControlPoints1[3 - i, j];
                }
            }
            Mesh sailMesh2 = getBezierPatchMesh(sailControlPoints2, 10, 10);


            // back sail
            Vector3 back = new Vector3(sailLowerBarPos.x, sailLowerBarPos.y, cross.z - sailLength * 0.5f);
            Vector3 cp3 = cross * 0.3f + back * 0.7f;
            Vector3 cp4 = cross * 0.7f + back * 0.3f;
            Vector3[,] sailControlPoints3 = new Vector3[4, 4];
            sailControlPoints3[0, 0] = cross;
            sailControlPoints3[0, 1] = cp3;
            sailControlPoints3[0, 2] = cp4;
            sailControlPoints3[0, 3] = back;

            sailControlPoints3[1, 0] = new Vector3(cross.x + sailSizeMult * 0.1f, cross.y * 0.3f + top.y * 0.3f, cross.z - 0.1f);
            sailControlPoints3[1, 1] = new Vector3(cp3.x + sailSizeMult * 0.3f, cp3.y * 0.3f + top.y * 0.3f, cp3.z);
            sailControlPoints3[1, 2] = new Vector3(cp4.x + sailSizeMult * 0.3f, cp4.y * 0.3f + top.y * 0.3f, cp4.z);
            sailControlPoints3[1, 3] = new Vector3(back.x + sailSizeMult * 0.1f, back.y * 0.3f + top.y * 0.3f, back.z);

            sailControlPoints3[2, 0] = new Vector3(cross.x + sailSizeMult * 0.1f, cross.y * 0.3f + top.y * 0.3f, cross.z - 0.1f);
            sailControlPoints3[2, 1] = new Vector3(cp3.x + sailSizeMult * 0.3f, cp3.y * 0.3f + top.y * 0.3f, cp3.z);
            sailControlPoints3[2, 2] = new Vector3(cp4.x + sailSizeMult * 0.3f, cp4.y * 0.3f + top.y * 0.3f, cp4.z);
            sailControlPoints3[2, 3] = new Vector3(back.x + sailSizeMult * 0.1f, back.y * 0.3f + top.y * 0.3f, back.z);

            sailControlPoints3[3, 0] = top;
            sailControlPoints3[3, 1] = top;
            sailControlPoints3[3, 2] = top;
            sailControlPoints3[3, 3] = top;

            Mesh sailMesh3 = getBezierPatchMesh(sailControlPoints3, 10, 10);

            //reverse
            Vector3[,] sailControlPoints4 = new Vector3[4, 4];
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    sailControlPoints4[i, j] = sailControlPoints3[3 - i, j];
                }
            }
            Mesh sailMesh4 = getBezierPatchMesh(sailControlPoints4, 10, 10);

            //Combine sails
            CombineInstance[] combinedSailMesh = new CombineInstance[4];
            combinedSailMesh[0].mesh = sailMesh1;
            combinedSailMesh[0].transform = Matrix4x4.identity;
            combinedSailMesh[1].mesh = sailMesh2;
            combinedSailMesh[1].transform = Matrix4x4.identity;
            combinedSailMesh[2].mesh = sailMesh3;
            combinedSailMesh[2].transform = Matrix4x4.identity;
            combinedSailMesh[3].mesh = sailMesh4;
            combinedSailMesh[3].transform = Matrix4x4.identity;
            sailMesh = new Mesh();
            sailMesh.CombineMeshes(combinedSailMesh);
            sailMesh.RecalculateNormals();

        }

        // sailMesh = sailBaseMesh;

        sailPivotPoint = sailBasePos + this.transform.position;
        

        sail = new GameObject("Sail");
        MeshFilter mf2 = sail.AddComponent<MeshFilter>();
        mf2.mesh = sailMesh;
        MeshRenderer mr2 = sail.AddComponent<MeshRenderer>();
        sail.transform.position = shipObject.transform.position;
        sail.transform.parent = shipObject.transform;
        mr2.material.color = Color.white;

        GameObject sailStructure = new GameObject("Sail Structure");
        MeshFilter mf = sailStructure.AddComponent<MeshFilter>();
        mf.mesh = sailStructureMesh;
        MeshRenderer mr = sailStructure.AddComponent<MeshRenderer>();
        sailStructure.transform.position = shipObject.transform.position;
        sailStructure.transform.parent = sail.transform;
        mr.material.color = sailStructureColor;
    }

    private void generateProps()
    {
        // props only have a single model, but vary in number and size
        // position based off stern size
        float sternLength = 0.6f * sternLengthMult;

        // create base prop mesh and game object
        // will do three blades
        float propSize = 0.1f;

        Vector3[,] propControlPoints = new Vector3[4, 4];
        propControlPoints[0, 0] = new Vector3(0, 0, 0);
        propControlPoints[0, 1] = new Vector3(0, 0, 0);
        propControlPoints[0, 2] = new Vector3(0, 0, 0);
        propControlPoints[0, 3] = new Vector3(0, 0, 0);

        propControlPoints[1, 0] = new Vector3(-propSize * 0.5f, propSize * 0.5f, propSize * 0.1f);
        propControlPoints[1, 1] = new Vector3(-propSize * 0.2f, propSize * 0.5f, 0);
        propControlPoints[1, 2] = new Vector3(propSize * 0.2f, propSize * 0.5f, 0);
        propControlPoints[1, 3] = new Vector3(propSize * 0.5f, propSize * 0.5f, -propSize * 0.2f);

        propControlPoints[2, 0] = new Vector3(-propSize * 0.5f, propSize * 0.9f, propSize * 0.1f);
        propControlPoints[2, 1] = new Vector3(-propSize * 0.3f, propSize * 0.9f, 0);
        propControlPoints[2, 2] = new Vector3(propSize * 0.3f, propSize * 0.9f, 0);
        propControlPoints[2, 3] = new Vector3(propSize * 0.5f, propSize * 0.9f, -propSize * 0.2f);

        propControlPoints[3, 0] = new Vector3(-propSize * 0.4f, propSize, propSize * 0.05f);
        propControlPoints[3, 1] = new Vector3(-propSize * 0.2f, propSize * 1.1f, 0);
        propControlPoints[3, 2] = new Vector3(propSize * 0.2f, propSize * 1.1f, 0);
        propControlPoints[3, 3] = new Vector3(propSize * 0.4f, propSize, -propSize * 0.1f);

        Mesh propBladeMesh1 = getBezierPatchMesh(propControlPoints, 10, 10);
        // reverse for other side
        Vector3[,] propControlPoints2 = new Vector3[4, 4];
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                propControlPoints2[i, j] = propControlPoints[3 - i, j];
            }
        }
        Mesh propBladeMesh2 = getBezierPatchMesh(propControlPoints2, 10, 10);

        CombineInstance[] combinedPropMesh = new CombineInstance[2];
        combinedPropMesh[0].mesh = propBladeMesh1;
        combinedPropMesh[0].transform = Matrix4x4.identity;
        combinedPropMesh[1].mesh = propBladeMesh2;
        combinedPropMesh[1].transform = Matrix4x4.identity;
        Mesh propMesh = new Mesh();
        propMesh.CombineMeshes(combinedPropMesh);

        GameObject propArm = new GameObject("arm1");
        MeshFilter mf = propArm.AddComponent<MeshFilter>();
        mf.mesh = propMesh;
        MeshRenderer mr = propArm.AddComponent<MeshRenderer>();
        mr.material.color = new Color(0.8f, 0.8f, 0.2f);

        // create second arm
        GameObject propArm2 = new GameObject("arm2");
        MeshFilter mf2 = propArm2.AddComponent<MeshFilter>();
        mf2.mesh = propMesh;
        MeshRenderer mr2 = propArm2.AddComponent<MeshRenderer>();
        mr2.material.color = new Color(0.8f, 0.8f, 0.2f);
        propArm2.transform.RotateAround(propArm2.transform.position, Vector3.forward, 120.0f);

        // create third arm
        GameObject propArm3 = new GameObject("arm3");
        MeshFilter mf3 = propArm3.AddComponent<MeshFilter>();
        mf3.mesh = propMesh;
        MeshRenderer mr3 = propArm3.AddComponent<MeshRenderer>();
        mr3.material.color = new Color(0.8f, 0.8f, 0.2f);
        propArm3.transform.RotateAround(propArm3.transform.position, Vector3.forward, 240.0f);

        // create cyclinder base
        int resolution = 8;
        Vector3[] baseVertices = new Vector3[resolution * 2];
        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < resolution; j++)
            {
                float angle = 2.0f * Mathf.PI * j / resolution;
                float x = Mathf.Cos(angle) * propSize * 0.1f;
                float z = (1 - i) * propSize * 10f - propSize * 0.05f;
                float y = Mathf.Sin(angle) * propSize * 0.1f;
                baseVertices[i * resolution + j] = new Vector3(x, y, z);
            }
        }
        int[] baseTriangles = new int[resolution * 2 * 3];
        for (int i = 0; i < resolution; i++)
        {
            int topI = i + resolution;
            int nextI = (i + 1) % resolution;
            int nextTopI = nextI + resolution;
            // side triangles
            baseTriangles[i * 6 + 0] = i;
            baseTriangles[i * 6 + 1] = topI;
            baseTriangles[i * 6 + 2] = nextI;

            baseTriangles[i * 6 + 3] = nextI;
            baseTriangles[i * 6 + 4] = topI;
            baseTriangles[i * 6 + 5] = nextTopI;
        }
        Mesh propBaseCylinder = new Mesh();
        propBaseCylinder.vertices = baseVertices;
        propBaseCylinder.triangles = baseTriangles;
        propBaseCylinder.RecalculateNormals();

        // add cap to cylinder
        Vector3[] capVertices = new Vector3[resolution];
        for (int i = 0; i < resolution; i++)
        {
            capVertices[i] = baseVertices[i + resolution];
        }
        int[] capTriangles = new int[6 * 3];
        for (int i = 0; i < 6; i++)
        {
            int index = 3 * i;
            capTriangles[index] = 0;
            capTriangles[index + 1] = i + 2;
            capTriangles[index + 2] = i + 1;
        }
        Mesh propBaseCapMesh = new Mesh();
        propBaseCapMesh.vertices = capVertices;
        propBaseCapMesh.triangles = capTriangles;
        propBaseCapMesh.RecalculateNormals();

        // combine cylinder and cap
        CombineInstance[] combinedPropBaseMesh = new CombineInstance[2];
        combinedPropBaseMesh[0].mesh = propBaseCylinder;
        combinedPropBaseMesh[0].transform = Matrix4x4.identity;
        combinedPropBaseMesh[1].mesh = propBaseCapMesh;
        combinedPropBaseMesh[1].transform = Matrix4x4.identity;
        Mesh propBaseMesh = new Mesh();
        propBaseMesh.CombineMeshes(combinedPropBaseMesh);
        propBaseMesh.RecalculateNormals();

        GameObject propBase = new GameObject("base");
        MeshFilter mf4 = propBase.AddComponent<MeshFilter>();
        mf4.mesh = propBaseMesh;
        MeshRenderer mr4 = propBase.AddComponent<MeshRenderer>();
        mr4.material.color = new Color(0.6f, 0.6f, 0.6f);

        GameObject prop = new GameObject("Propeller");
        propArm.transform.parent = prop.transform;
        propArm2.transform.parent = prop.transform;
        propArm3.transform.parent = prop.transform;
        propBase.transform.parent = prop.transform;
        prop.transform.parent = shipObject.transform;

        // instantiate second
        GameObject prop2 = Instantiate(prop);
        prop2.transform.parent = shipObject.transform;
        Vector3 prop2Scale = prop2.transform.localScale;
        prop2Scale.x = -1.0f;
        prop2.transform.localScale = prop2Scale;

        // position prop based on width of hull
        prop.transform.position = shipObject.transform.position + new Vector3(hullWidthMult * 0.7f, -0.5f, -sternLength);
        prop2.transform.position = shipObject.transform.position + new Vector3(hullWidthMult * 0.3f, -0.5f, -sternLength);

        props = new GameObject[2];
        props[0] = prop;
        props[1] = prop2;

        propPivotPoints = new Vector3[2];
        propPivotPoints[0] = prop.transform.position + this.transform.position;
        propPivotPoints[1] = prop2.transform.position + this.transform.position;
    }

    private void textureShip()
    {
        MeshRenderer smr = shipObject.GetComponent<MeshRenderer>();

        // choose material from list
        Material mat = null;
        if (hullMaterial == Materials.metal)
            mat = Resources.Load<Material>("Metal");
        else if (hullMaterial == Materials.wood)
            mat = Resources.Load<Material>("Wood");
        else if (hullMaterial == Materials.mesh)
            mat = Resources.Load<Material>("Mesh");
        else if (hullMaterial == Materials.concrete)
            mat = Resources.Load<Material>("Concrete");
        smr.material = mat;

        MeshRenderer dmr = deck.GetComponent<MeshRenderer>();
        if (deckMaterial == Materials.metal)
            mat = Resources.Load<Material>("Metal");
        else if (deckMaterial == Materials.wood)
            mat = Resources.Load<Material>("Wood");
        else if (deckMaterial == Materials.mesh)
            mat = Resources.Load<Material>("Mesh");
        else if (deckMaterial == Materials.concrete)
            mat = Resources.Load<Material>("Concrete");
        dmr.material = mat;

    }
    // calculate a point on the bezier surface given parameters on the curve and control points
    Vector3 getBezierPoint(Vector3[,] controlPoints, float u, float v)
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

    Mesh getBezierPatchMesh(Vector3[,] controlPoints, int resolutionU, int resolutionV)
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();

        // loop and sample vertices based on resolution from the patch (which is calculated in the above function)
        for (int i = 0; i <= resolutionU; i++)
        {
            float u = (float)i / resolutionU;
            for (int j = 0; j <= resolutionV; j++)
            {
                float v = (float)j / resolutionV;
                vertices.Add(getBezierPoint(controlPoints, u, v));

                uvs.Add(new Vector2(u, v));
            }
        }

        // add triangles based on the vertex grid
        for (int i = 0; i < resolutionU; i++)
        {
            for (int j = 0; j < resolutionV; j++)
            {
                int index0 = i * (resolutionV + 1) + j;
                int index1 = index0 + 1;
                int index2 = index0 + (resolutionV + 1);
                int index3 = index2 + 1;

                triangles.Add(index0);
                triangles.Add(index2);
                triangles.Add(index1);

                triangles.Add(index1);
                triangles.Add(index2);
                triangles.Add(index3);
            }
        }

        Mesh mesh = new Mesh();
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.SetUVs(0, uvs);
        mesh.RecalculateNormals();

        return mesh;
    }

    // Start is called before the first frame update
    void Start() // mainly randomizes, then calls to generate the ship
    {
        Random.InitState(seed);
        hullType = (HullType)Random.Range(0, 3); // defines curvature and shape of hull
        hullLengthMult = Random.Range(0.5f, 3.0f);
        hullWidthMult = Random.Range(0.5f, 1.5f);
        bowLengthMult = Random.Range(0.5f, 1.5f);
        sternLengthMult = Random.Range(0.5f, 1.5f);

        superstructureType = (SuperstructureType)Random.Range(0, 3);
        superstructureLengthMult = Random.Range(0.5f, 1.0f);
        superstructureHeightMult = Random.Range(0.5f, 1.5f);

        sailType = (SailType)Random.Range(0, 3);
        sailHeightMult = Random.Range(0.8f, 2.0f);
        sailSizeMult = Random.Range(0.8f, 1.5f);

        deckMaterial = (Materials)Random.Range(0, 4);
        hullMaterial = (Materials)Random.Range(0, 4);

        sailFrequency = Random.Range(0.1f, 0.5f);

        ssColor = Color.HSVToRGB(Random.Range(215f/360f, 240f/360f), Random.Range(0.0f, 0.25f), Random.Range(0.0f, 1.0f));
        sailStructureColor = Color.HSVToRGB(Random.Range(20f/360f, 35f/360f), Random.Range(0.2f, .8f), Random.Range(0.2f, .4f));

        generateShip();

        shipObject.transform.position = this.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        // rotate the sail slightly
        if (sail != null)
        {
            float angle = Mathf.Sin(Time.time * sailFrequency * Mathf.PI) * 10.0f;
            float deltaAngle = angle - lastAngle;
            lastAngle = angle;
            sail.transform.RotateAround(sailPivotPoint, Vector3.up, deltaAngle);
        }
        if (props != null)
        {
            for (int i = 0; i < props.Length; i++)
            {
                float speed = 20.0f;
                float mult = (i % 2 == 0) ? speed : -speed;
                props[i].transform.RotateAround(propPivotPoints[i], Vector3.forward, mult);
            }
        }
    }

}

public enum HullType
{
    pointed, curved, boxy
}

public enum SuperstructureType
{
    boxy, rounded, slanted
}

public enum Materials
{
    metal, wood, mesh, concrete
}

public enum SailType
{
    square, triangular, dTriangle
}