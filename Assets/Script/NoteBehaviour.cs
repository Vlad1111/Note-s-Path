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
    }

    public void SetMovement(float x, float y)
    {
        movement.x = x;
        movement.y = y;
    }

    internal void Flap()
    {
        isFlappingTime = flappingTime;
        rb.velocity += transform.forward * flappingPower * 20f;
        flapParticleSystem.Play();
    }


    // Update is called once per frame
    void FixedUpdate()
    {
        var velocity = rb.velocity;
        
        velocity += transform.right * movement.x * sizeMovementSpeed * Time.fixedDeltaTime;
        //transform.forward * flappingPower;

        velocity = transform.worldToLocalMatrix * velocity;

        float flap = (maxSpeed - velocity.z) / maxSpeed;
        if (isFlappingTime >= 0)
        {
            flap  *= flappingPower;
            isFlappingTime -= Time.fixedDeltaTime;
        }
        else
        {
            flap *= flappingPower / 30;
            if (flapParticleSystem.isPlaying)
                flapParticleSystem.Stop();
        }
        Debug.Log(flap);
        if(flap > 0)
            velocity.z += flap;

        float angle = transform.localEulerAngles.x;
        if (angle > 180)
            angle -= 360;
        if(angle > 0)
            velocity.z += angle * Physics.gravity.y * Time.fixedDeltaTime / 180f;

        velocity.x -= velocity.x * airResistanceAxis.x;
        velocity.z -= velocity.y * airResistanceAxis.y / 30f;
        velocity.z -= velocity.z * airResistanceAxis.z;
        velocity.y -= velocity.y * airResistanceAxis.y;

        //if (velocity.z > maxSpeed)
        //    velocity.z = maxSpeed;

        velocity = transform.localToWorldMatrix * velocity;

        rb.velocity = velocity;

        //transform.position += transform.right * movement.x * sizeMovementSpeed * Time.fixedDeltaTime;
    }

    private void Update()
    {
        transform.localEulerAngles += new Vector3(movement.y * rotationSpeed * Time.deltaTime, 0, 0);

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
