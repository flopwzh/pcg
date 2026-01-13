using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boid : MonoBehaviour
{
    private Vector3 velocity;
    private Vector3 position;
    private Vector3 wanderDirection;
    private float maxSpeed;
    private float minSpeed;
    private float maxForce;

    public Boid(Vector3 initVelocity, float maxSpeed, float minSpeed, float maxForce)
    {
        position = transform.position;
        velocity = initVelocity;
        this.maxSpeed = maxSpeed;
    }

    public void Initialize(Vector3 initVelocity, Vector3 wanderDirection, float maxSpeed, float minSpeed, float maxForce)
    {
        position = transform.position;
        velocity = initVelocity;
        this.wanderDirection = wanderDirection;
        this.maxSpeed = maxSpeed;
        this.minSpeed = minSpeed;
        this.maxForce = maxForce;
    }

    // steer the boid given a force
    public void Steer(Vector3 force, float t)
    {
        Vector3 prevVelocity = velocity;
        if (force.magnitude > maxForce) // limit force
        {
            force = Vector3.ClampMagnitude(force, maxForce);
        }
        velocity += force * t;
        // print("Force applied: " + force + " New velocity: " + velocity);
        if (velocity.magnitude > maxSpeed) // limit speed
        {
            velocity = Vector3.ClampMagnitude(velocity, maxSpeed);
        }
        else if (velocity.magnitude <= 0f) // prevent stopping
        {
            velocity = prevVelocity;
        }
        else if (velocity.magnitude < minSpeed) // enforce min speed
        {
            velocity = velocity.normalized * minSpeed;
        }
        position += velocity * t;
        transform.position = position;


        // face velocity direction
        if (velocity != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(velocity);
        }
    }

    public Vector3 GetPosition()
    {
        return position;
    }

    public Vector3 GetVelocity()
    {
        return velocity;
    }

    public Vector3 GetWanderDirection()
    {
        return wanderDirection;
    }

    public void SetWanderDirection(Vector3 newDir)
    {
        wanderDirection = newDir;
    }
}
