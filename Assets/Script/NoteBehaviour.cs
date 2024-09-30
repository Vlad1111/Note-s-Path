using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteBehaviour : MonoBehaviour
{
    public Rigidbody rb;
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
        transform.localEulerAngles += new Vector3(movement.y, 0, 0) * rotationSpeed * Time.deltaTime;
        transform.position += transform.right * movement.x * sizeMovementSpeed * Time.deltaTime;
        //transform.localEulerAngles += new Vector3()
    }
}
