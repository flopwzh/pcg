using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class BoidController : MonoBehaviour
{
    private List<Boid> boids;
    public int numBoids = 50;
    public float worldSize = 5f;

    public int seed = 42;

    public bool toggleCentering = true;
    public float centeringWeight = 1f;
    public float neighborRadius = 3f;
    public int centeringMaxNeighbors = 5;


    public bool toggleAvoidance = true;
    public bool toggleObstacles = true;
    public bool toggleTransparency = false;
    public float avoidanceWeight = 1f;
    public float collisionRadius = 0.25f;
    public float obstacleDistance = 2f;


    public bool toggleMatching = true;
    public float matchingWeight = 1f;
    public int matchingMaxNeighbors = 5;
    
    public bool toggleWandering = true;
    public float wanderWeight = 1f;
    public float wanderInterval = 2f;

    public float boundaryWeight = 1f;


    public bool toggelTrails = true;
    public float maxSpeed = 5f;
    public float minSpeed = 1f;
    public float maxForce = 5f;


    public float scatterDuration = 2f;
    public float scatterStrength = 10f;

    private float scatterTimer = 0f;
    private float wanderTimer = 0f;

    private Vector3[][] obstacles;
    private GameObject[] obstacleObjects;
    private int numObstacles = 6;

    private int createdBoidCount = 0;
    private int activeBoidCount = 0;


    // Start is called before the first frame update
    void Start()
    {
        // instantiate boids
        // we want to start all boids at random positions within the world box
        Random.InitState(seed);
        boids = new List<Boid>();
        for (int i = 0; i < numBoids; i++)
        {
            createBoid(i);
        }

        // obstacles
        obstacles = new Vector3[numObstacles][];
        obstacleObjects = new GameObject[numObstacles];
        obstacles[0] = new Vector3[]
        {
            new Vector3(1f, 1f, -1f), 
            new Vector3(5f, 1.3f, 1f)
        };
        obstacles[1] = new Vector3[]
        {
            new Vector3(-4f, -2.5f, -2f), 
            new Vector3(-2f, -2f, 3f)
        };
        obstacles[2] = new Vector3[]
        {
            new Vector3(-4f, 3.6f, -2f), 
            new Vector3(4f, 4f, -1f)
        };
        obstacles[3] = new Vector3[]
        {
            new Vector3(-0.5f, -3f, 0f), 
            new Vector3(0f, 2f, 5f)
        };
        obstacles[4] = new Vector3[]
        {
            new Vector3(1f, -1.5f, -5f), 
            new Vector3(4f, -1f, -3f)
        };
        obstacles[5] = new Vector3[]
        {
            new Vector3(-1f, 4.5f, -2f), 
            new Vector3(1f, 5f, -1f)
        };

        for (int i = 0; i < numObstacles; i++)
        {
            GameObject obstacle = new GameObject("Obstacle_" + i);
            obstacle.AddComponent<MeshFilter>().mesh = createObstacle(obstacles[i][0], obstacles[i][1]);
            obstacle.AddComponent<MeshRenderer>().material = Resources.Load<Material>("obstacle");
            obstacle.GetComponent<MeshRenderer>().material.color = new Color(0.5f, 0.5f, 0.5f, 1f);
            obstacleObjects[i] = obstacle;
            activeBoidCount++;
        }
        
        wanderTimer = wanderInterval;
    }

    // Update is called once per frame
    void Update()
    {
        // dynamically create or destroy/disable boids based on numBoids
        if (activeBoidCount < numBoids)
        {
            // first reactivate any disabled boids
            for (int i = activeBoidCount; i < Mathf.Min(numBoids, createdBoidCount); i++)
            {
                boids[i].gameObject.SetActive(true);
                activeBoidCount++;
            }
            // then create new boids if needed
            for (int i = createdBoidCount; i < numBoids; i++)
            {
                createBoid(i);
                activeBoidCount++;
            }
        }
        else if (activeBoidCount > numBoids)
        {
            // disable extra boids
            int temp = activeBoidCount;
            for (int i = numBoids; i < activeBoidCount; i++)
            {
                boids[i].gameObject.SetActive(false);
                temp--;
            }
            activeBoidCount = temp;
        }

        List<Boid> activeBoids = boids.GetRange(0, activeBoidCount);

        print(scatterTimer);
        // scatter boids by increasing the wander strength temporarily
        if (Input.GetKeyDown(KeyCode.Space))
        {
            scatterTimer = scatterDuration;
        }
        if (scatterTimer > 0f)
        {
            scatterTimer -= Time.deltaTime;
        }

        // change wander direction at intervals
        wanderTimer -= Time.deltaTime;
        if (wanderTimer <= 0f)
        {
            wanderTimer = wanderInterval;
            // update wander direction for each boid
            foreach (Boid boid in activeBoids)
            {
                Vector3 newWanderDir = Random.onUnitSphere;
                boid.SetWanderDirection(newWanderDir);
            }
        }

        foreach (Boid boid in activeBoids)
        {
            Vector3 force = Vector3.zero;
            // get nearest boids within radius
            List<Boid> neighbors = GetNeighbors(boid, activeBoids);
            // compute forces here
            if (toggleCentering) // center to nearby boids
            {
                float totalWeight = 0f;
                Vector3 totalWeightedDistance = Vector3.zero;
                for (int i = 0; i < Mathf.Min(neighbors.Count, centeringMaxNeighbors); i++)
                {
                    Boid neighbor = neighbors[i];
                    float distance = Vector3.Distance(boid.transform.position, neighbor.transform.position);
                    float weight = Mathf.Clamp01(1f - (distance / neighborRadius));
                    totalWeight += weight;
                    totalWeightedDistance += weight * (neighbor.transform.position - boid.transform.position);
                }
                if (totalWeight > 0f)
                {
                    Vector3 centeringForce = totalWeightedDistance / totalWeight;
                    force += centeringForce * centeringWeight;
                }
                    
            }
            if (toggleAvoidance) // avoid very close boids and obstacles
            {
                // boids
                Vector3 avoidanceForce = Vector3.zero;
                foreach (Boid neighbor in neighbors)
                {
                    float distance = Vector3.Distance(boid.transform.position, neighbor.transform.position);
                    if (distance < collisionRadius)
                    {
                        float weight = Mathf.Clamp01(1f - (distance / collisionRadius));
                        avoidanceForce += weight * (boid.transform.position - neighbor.transform.position);
                    }
                }
                force += avoidanceForce * avoidanceWeight;

                // also avoid obstacles
                if (toggleObstacles)
                {
                    Vector3 hitPoint, collisionNormal;
                    Vector3 obstacleAvoidanceForce = Vector3.zero;
                    foreach (Vector3[] obstacle in obstacles)
                    {
                        if (ObstacleCollision(boid, obstacleDistance, obstacle[0], obstacle[1], out hitPoint, out collisionNormal))
                        {
                            obstacleAvoidanceForce += collisionNormal;
                        }
                    }
                    force += obstacleAvoidanceForce * avoidanceWeight;
                }
            }
            if (toggleMatching) // match velocity with nearby boids
            {
                Vector3 matchingForce = Vector3.zero;
                for (int i = 0; i < Mathf.Min(neighbors.Count, matchingMaxNeighbors); i++)
                {
                    Boid neighbor = neighbors[i];
                    float distance = Vector3.Distance(boid.transform.position, neighbor.transform.position);
                    float weight = Mathf.Clamp01(1f - (distance / neighborRadius));
                    matchingForce += weight * (neighbor.GetVelocity() - boid.GetVelocity());
                }
                force += matchingForce * matchingWeight;
            }
            if (toggleWandering) // random wander
            {
                Vector3 wanderForce = boid.GetWanderDirection();
                if (scatterTimer > 0f)
                {
                    wanderForce *= scatterStrength;
                }               
                force += wanderForce * wanderWeight;
            }
            // boundary force
            float distToCenter = boid.transform.position.magnitude;
            float distToEdge = distToCenter - worldSize;
            if (distToEdge < 0f) distToEdge = 0f;
            Vector3 boundaryForce = -boid.transform.position.normalized * distToEdge;
            force += boundaryForce * boundaryWeight;

            boid.Steer(force, Time.deltaTime);

            // trail check
            TrailRenderer trail = boid.GetComponent<TrailRenderer>();
            if (toggelTrails)
            {
                trail.enabled = true;
            }
            else
            {
                trail.enabled = false;
            }
        }

        // transparency toggle
        for (int i = 0; i < obstacleObjects.Length; i++)
        {
            Color color = obstacleObjects[i].GetComponent<MeshRenderer>().material.color;
            if (toggleTransparency)
            {
                color.a = 0.4f;
            }
            else
            {
                color.a = 1f;
            }
            obstacleObjects[i].GetComponent<MeshRenderer>().material.color = color;
        }
    }

    // gets neighbors ordered by distance
    private List<Boid> GetNeighbors(Boid boid, List<Boid> boids)
    {
        List<Boid> neighbors = new List<Boid>();

        List<(Boid b, float d)> neighborDistances = new List<(Boid, float)>();
        foreach (Boid other in boids)
        {
            if (other == boid) continue;
            float distance = Vector3.Distance(boid.transform.position, other.transform.position);
            if (distance <= neighborRadius)
            {
                neighborDistances.Add((other, distance));
            }
        }

        neighborDistances.Sort((a, b) => a.d.CompareTo(b.d));
        foreach(var bd in neighborDistances)
        {
            neighbors.Add(bd.b);
        }
        return neighbors;
    }

    private void createBoid(int i)
    {
        // generate initial position and velocity
        Vector3 initPosition = new Vector3(
            Random.Range(-worldSize, worldSize),
            Random.Range(-worldSize, worldSize),
            Random.Range(-worldSize, worldSize)
        );
        initPosition = Vector3.ClampMagnitude(initPosition, worldSize);
        Vector3 initVelocity = Random.onUnitSphere * Random.Range(minSpeed, maxSpeed);

        // create game object
        GameObject boidObject = new GameObject("Boid_" + i);
        boidObject.AddComponent<MeshFilter>().mesh = createBoidMesh();
        boidObject.AddComponent<MeshRenderer>().material = new Material(Shader.Find("Standard"));
        boidObject.transform.position = initPosition;
        boidObject.GetComponent<MeshRenderer>().material.color = new Color(Random.Range(0.5f,1f), Random.Range(0.5f,1f), Random.Range(0.5f,1f), 1f);

        // attach script and initialize
        Boid boid = boidObject.AddComponent<Boid>();
        boid.Initialize(initVelocity, Random.onUnitSphere, maxSpeed, minSpeed, maxForce);
        boids.Add(boid);

        // attach trail
        TrailRenderer trail = boidObject.AddComponent<TrailRenderer>();
        trail.time = 1f;
        trail.startWidth = 0.05f;
        trail.endWidth = 0.05f;
        trail.enabled = true;
        trail.material = new Material(Shader.Find("Standard"));
        trail.material.color = new Color(Random.Range(0.5f,1f), Random.Range(0.5f,1f), Random.Range(0.5f,1f), 0.5f);

        createdBoidCount++;
    }

    private bool ObstacleCollision(Boid boid, float distance, Vector3 c1, Vector3 c2, out Vector3 hitPoint, out Vector3 collisionNormal)
    {
        hitPoint = Vector3.zero;
        collisionNormal = Vector3.zero;

        // calculate ray
        Vector3 direction = boid.GetVelocity().normalized;
        Ray ray = new Ray(boid.GetPosition(), direction);

        // calcaulte the entry and exit t value for the ray on the obstacle
        float tMin = (c1.x - ray.origin.x) / ray.direction.x;
        float tMax = (c2.x - ray.origin.x) / ray.direction.x;
        if (tMin > tMax)
        {
            (tMin, tMax) = (tMax, tMin);
        }

        float tyMin = (c1.y - ray.origin.y) / ray.direction.y;
        float tyMax = (c2.y - ray.origin.y) / ray.direction.y;
        if (tyMin > tyMax)
        {
            (tyMin, tyMax) = (tyMax, tyMin);
        }
        if ((tMin > tyMax) || (tyMin > tMax)) // no collision
        {
            return false;
        }

        if (tyMin > tMin)
        {
            tMin = tyMin;
        }
        if (tyMax < tMax)
        {
            tMax = tyMax;
        }

        float tzMin = (c1.z - ray.origin.z) / ray.direction.z;
        float tzMax = (c2.z - ray.origin.z) / ray.direction.z;
        if (tzMin > tzMax)
        {
            (tzMin, tzMax) = (tzMax, tzMin);
        }
        if ((tMin > tzMax) || (tzMin > tMax)) // no collision
        {
            return false;
        }
        if (tzMin > tMin)
        {
            tMin = tzMin;
        }
        if (tzMax < tMax)
        {
            tMax = tzMax;
        }

        // no collision
        if (tMin < 0f || tMin > distance)
        {
            return false;
        }
        
        // collision
        hitPoint = ray.origin + ray.direction * tMin;
        float e = 0.001f;
        if (Mathf.Abs(hitPoint.x - c1.x) < e)
        {
            collisionNormal = new Vector3(-1f, 0f, 0f);
        }
        else if (Mathf.Abs(hitPoint.x - c2.x) < e)
        {
            collisionNormal = new Vector3(1f, 0f, 0f);
        }
        else if (Mathf.Abs(hitPoint.y - c1.y) < e)
        {
            collisionNormal = new Vector3(0f, -1f, 0f);
        }
        else if (Mathf.Abs(hitPoint.y - c2.y) < e)
        {
            collisionNormal = new Vector3(0f, 1f, 0f);
        }
        else if (Mathf.Abs(hitPoint.z - c1.z) < e)
        {
            collisionNormal = new Vector3(0f, 0f, -1f);
        }
        else if (Mathf.Abs(hitPoint.z - c2.z) < e)
        {
            collisionNormal = new Vector3(0f, 0f, 1f);
        }
        return true;
    }

    private Mesh createBoidMesh()
    {
        Mesh mesh = new Mesh();

        Vector3[] vertices = new Vector3[]
        {
            // front face
            new Vector3(0.025f, 0.025f, 0.2f),
            new Vector3(-0.025f, 0.025f, 0.2f),
            new Vector3(-0.025f, -0.025f, 0.2f),
            new Vector3(0.025f, -0.025f, 0.2f),
            // front top
            new Vector3(0.025f, 0.025f, 0.2f),
            new Vector3(0.05f, 0.05f, 0.1f),
            new Vector3(-0.05f, 0.05f, 0.1f),
            new Vector3(-0.025f, 0.025f, 0.2f),
            // front left
            new Vector3(-0.025f, 0.025f, 0.2f),
            new Vector3(-0.05f, 0.05f, 0.1f),
            new Vector3(-0.05f, -0.05f, 0.1f),
            new Vector3(-0.025f, -0.025f, 0.2f),
            // front bottom
            new Vector3(-0.025f, -0.025f, 0.2f),
            new Vector3(-0.05f, -0.05f, 0.1f),
            new Vector3(0.05f, -0.05f, 0.1f),
            new Vector3(0.025f, -0.025f, 0.2f),
            // front right
            new Vector3(0.025f, -0.025f, 0.2f),
            new Vector3(0.05f, -0.05f, 0.1f),
            new Vector3(0.05f, 0.05f, 0.1f),
            new Vector3(0.025f, 0.025f, 0.2f),
            
            // middle top
            new Vector3(0.05f, 0.05f, 0.1f),
            new Vector3(0.05f, 0.05f, 0.0f),
            new Vector3(-0.05f, 0.05f, 0.0f),
            new Vector3(-0.05f, 0.05f, 0.1f),
            // middle left
            new Vector3(-0.05f, 0.05f, 0.1f),
            new Vector3(-0.05f, 0.05f, 0.0f),
            new Vector3(-0.05f, -0.05f, 0.0f),
            new Vector3(-0.05f, -0.05f, 0.1f),
            // middle bottom
            new Vector3(-0.05f, -0.05f, 0.1f),
            new Vector3(-0.05f, -0.05f, 0.0f),
            new Vector3(0.05f, -0.05f, 0.0f),
            new Vector3(0.05f, -0.05f, 0.1f),
            // middle right
            new Vector3(0.05f, -0.05f, 0.1f),
            new Vector3(0.05f, -0.05f, 0.0f),
            new Vector3(0.05f, 0.05f, 0.0f),
            new Vector3(0.05f, 0.05f, 0.1f),
            
            // back top
            new Vector3(0.05f, 0.05f, 0.0f),
            new Vector3(0.0f, 0.05f, -0.1f),
            new Vector3(-0.05f, 0.05f, 0.0f),
            // back left
            new Vector3(-0.05f, 0.05f, 0.0f),
            new Vector3(0.0f, 0.05f, -0.1f),
            new Vector3(0.0f, -0.05f, -0.1f),
            new Vector3(-0.05f, -0.05f, 0.0f),
            // back bottom
            new Vector3(-0.05f, -0.05f, 0.0f),
            new Vector3(0.0f, -0.05f, -0.1f),
            new Vector3(0.05f, -0.05f, 0.0f),
            // back right
            new Vector3(0.05f, -0.05f, 0.0f),
            new Vector3(0.0f, -0.05f, -0.1f),
            new Vector3(0.0f, 0.05f, -0.1f),
            new Vector3(0.05f, 0.05f, 0.0f),

            // tail fin
            new Vector3(0.0f, 0.05f, -0.1f),
            new Vector3(0.0f, 0.1f, -0.2f),
            new Vector3(0.0f, -0.1f, -0.2f),
            new Vector3(0.0f, -0.05f, -0.1f),
            // right side
            new Vector3(0.0f, 0.05f, -0.1f),
            new Vector3(0.0f, -0.05f, -0.1f),
            new Vector3(0.0f, -0.1f, -0.2f),
            new Vector3(0.0f, 0.1f, -0.2f),

            // right fin

            // bottom side


            // left fin

            // bottom side

            
            //dorsal fin

            // right side
        };

        int[] triangles = new int[]
        {
            // front
            0,1,2,
            2,3,0,
            // front top
            4,5,6,
            6,7,4,
            // front left
            8,9,10,
            10,11,8,
            // front bottom
            12,13,14,
            14,15,12,
            // front right
            16,17,18,
            18,19,16,

            // middle top
            20,21,22,
            22,23,20,
            // middle left
            24,25,26,
            26,27,24,
            // middle bottom
            28,29,30,
            30,31,28,
            // middle right
            32,33,34,
            34,35,32,

            // back top
            36,37,38,
            // back left
            39,40,41,
            41,42,39,
            // back bottom
            43,44,45,
            // back right
            46,47,48,
            48,49,46,

            // tail fin
            50,51,52,
            52,53,50,
            // right side
            54,55,56,
            56,57,54
        };

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        return mesh;
    }

    // creates box obstacle given two corner points
    // corner1 should always have smaller values than corner2
    private Mesh createObstacle(Vector3 corner1, Vector3 corner2)
    {
        Mesh mesh = new Mesh();

        Vector3[] vertices = new Vector3[]
        {
            // on x axis close
            new Vector3(corner1.x, corner1.y, corner1.z),
            new Vector3(corner1.x, corner2.y, corner1.z),
            new Vector3(corner2.x, corner2.y, corner1.z),
            new Vector3(corner2.x, corner1.y, corner1.z),
            // on z axis close
            new Vector3(corner1.x, corner1.y, corner1.z),
            new Vector3(corner1.x, corner1.y, corner2.z),
            new Vector3(corner1.x, corner2.y, corner2.z),
            new Vector3(corner1.x, corner2.y, corner1.z),
            // on x axis far
            new Vector3(corner2.x, corner2.y, corner2.z),
            new Vector3(corner1.x, corner2.y, corner2.z),
            new Vector3(corner1.x, corner1.y, corner2.z),
            new Vector3(corner2.x, corner1.y, corner2.z),
            // on z axis far
            new Vector3(corner2.x, corner2.y, corner2.z),
            new Vector3(corner2.x, corner1.y, corner2.z),
            new Vector3(corner2.x, corner1.y, corner1.z),
            new Vector3(corner2.x, corner2.y, corner1.z),
            // top
            new Vector3(corner1.x, corner2.y, corner1.z),
            new Vector3(corner1.x, corner2.y, corner2.z),
            new Vector3(corner2.x, corner2.y, corner2.z),
            new Vector3(corner2.x, corner2.y, corner1.z),
            // bottom
            new Vector3(corner1.x, corner1.y, corner1.z),
            new Vector3(corner2.x, corner1.y, corner1.z),
            new Vector3(corner2.x, corner1.y, corner2.z),
            new Vector3(corner1.x, corner1.y, corner2.z)
        };

        int[] triangles = new int[]
        {
            // on x axis close
            0,1,2,
            2,3,0,
            // on z axis close
            4,5,6,
            6,7,4,
            // on x axis far
            8,9,10,
            10,11,8,
            // on z axis far
            12,13,14,
            14,15,12,
            // top
            16,17,18,
            18,19,16,
            // bottom
            20,21,22,
            22,23,20
        };

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        return mesh;
    }
}
