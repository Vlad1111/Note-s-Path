using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteBehaviour : MonoBehaviour
{
    public Rigidbody rb;
    public ParticleSystem flapParticleSystem;
    public float flappingTime;
    public float flappingPower;
    public float rotationSpeed;
    public float sizeMovementSpeed;
    public Vector3 airResistanceAxis;
    public float maxSpeed = 1;

    private Vector2 movement;
    private float isFlappingTime = -1;

    // Start is called before the first frame update
    void Start()
    {
        Flap();
    }

    public void SetMovement(float x, float y)
    {
        movement.x = x;
        movement.y = y;
    }

    internal void Flap()
    {
        isFlappingTime = flappingTime;
        rb.velocity += transform.forward * flappingPower * 5f;
        flapParticleSystem.Play();
    }


    // Update is called once per frame
    void FixedUpdate()
    {
        var velocity = rb.velocity;
        
        //transform.forward * flappingPower;

        velocity = transform.worldToLocalMatrix * velocity;

        if(isFlappingTime >= 0)
        {
            velocity.z += flappingPower;
            isFlappingTime -= Time.fixedDeltaTime;
        }
        else if(flapParticleSystem.isPlaying)
            flapParticleSystem.Stop();

        velocity.x -= velocity.x * airResistanceAxis.x;
        velocity.y -= velocity.y * airResistanceAxis.y;
        velocity.z -= velocity.z * airResistanceAxis.z;

        if (velocity.z > maxSpeed)
            velocity.z = maxSpeed;

        velocity = transform.localToWorldMatrix * velocity;

        rb.velocity = velocity;
    }

    private void Update()
    {
        transform.localEulerAngles += new Vector3(movement.y * rotationSpeed * Time.deltaTime, 0, 0);
        transform.position += transform.right * movement.x * sizeMovementSpeed * Time.deltaTime;

        var teraingHeight = WorldGenerator.Instance.GetTerrainMaxHeight();
        teraingHeight = 1 - transform.localPosition.y * 1.3f / teraingHeight;
        if (teraingHeight < 0)
        {
            teraingHeight *= -5f;
            if (teraingHeight > 1)
                teraingHeight = teraingHeight * teraingHeight * 10;
            float angle = transform.localEulerAngles.x;
            if (angle > 180)
                angle -= 360;
            if (movement.y > 0)
                teraingHeight -= movement.y;
            if (angle < 0)
            {
                transform.localEulerAngles = new Vector3(angle + teraingHeight * Time.deltaTime * rotationSpeed, transform.localEulerAngles.y, transform.localEulerAngles.z);
            }
        }
    }
}
