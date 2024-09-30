using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public NoteBehaviour note;

    private Vector2 movement = Vector2.zero;
    private bool flap = false;

    private void Start()
    {
    }

    private void CalculateMovement()
    {
        movement = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        flap = Input.GetAxis("Jump") > 0.1f;
    }
    void Update()
    {
        CalculateMovement();
        note.SetMovement(movement.x, movement.y);
        if (flap)
            note.Flap();
    }
}
