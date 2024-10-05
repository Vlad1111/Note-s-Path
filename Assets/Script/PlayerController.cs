using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public NoteBehaviour note;
    public Transform cameraPivot;

    private Vector2 movement = Vector2.zero;
    private bool flap = false;

    private float screenUnit = -1;
    private float screenScensitibity = 0.3f;
    private float clickTime = 0.2f;

    private Touch? lastTouchPoint = null;
    private float firstTouchTime = -1;

    private void Start()
    {
        if (Screen.width > Screen.height)
            screenUnit = Screen.height;
        else screenUnit = Screen.width;
    }

    private void CalculateMovement()
    {
        if(Input.touchCount > 0)
        {
            var touch = Input.GetTouch(0);
            if(lastTouchPoint != null)
            {
                var delta =  ((touch.position - lastTouchPoint.Value.position) / screenUnit) / screenScensitibity;
                movement = delta;
                if (movement.x > 1)
                    movement.x = 1;
                else if(movement.x < -1)
                    movement.x = -1;

                if(movement.y > 1)
                    movement.y = 1;
                else if(movement.y < -1)
                    movement.y = -1;
            }
            else
            {
                firstTouchTime = Time.time;
                lastTouchPoint = touch;
            }
        }
        else
        {
            if(lastTouchPoint != null)
            {
                if (Time.time - firstTouchTime < clickTime)
                    flap = true;
                else flap = false;
                lastTouchPoint = null;
                firstTouchTime = -1;
            }
            else
            {
                movement = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
                flap = Input.GetAxis("Jump") > 0.1f;
            }
        }
    }
    void Update()
    {
        CalculateMovement();
        note.SetMovement(movement.x, movement.y);
        if (flap)
            note.Flap();
        WorldGenerator.Instance.CheckNewPlayerPosition(transform.position);

        var rot = Quaternion.Lerp(Quaternion.identity, Quaternion.Inverse(transform.localRotation), 0.5f);
        cameraPivot.localEulerAngles = new Vector3(rot.eulerAngles.x, 0, 0);
    }
}
