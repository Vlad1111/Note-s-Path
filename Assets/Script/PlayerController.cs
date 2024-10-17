using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private class GroundDirectionCheck
    {
        public Vector3 direction;
        public float lenght;
        public float weight;
        public Vector3? lastPointHit;
        public float distanceRelative;

        public GroundDirectionCheck(Vector3 direction, float lenght, float weight)
        {
            this.direction = direction;
            this.lenght = lenght;
            this.weight = weight;
            lastPointHit = null;
        }
    }

    public NoteBehaviour note;
    public Transform cameraPivot;
    public float distanceToObjectsCheck;
    public float curentPower;
    public float maxPower;

    private Vector2 movement = Vector2.zero;
    private bool flap = false;

    private List<GroundDirectionCheck> directionsToCheck = new List<GroundDirectionCheck>();
    private float totalMaxSpeed;

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

        curentPower = maxPower;
        totalMaxSpeed = note.maxSpeed * 10;

        for (int i = 0; i < 360; i += 30)
        {
            float angle = Mathf.Deg2Rad * i;
            float lenght = distanceToObjectsCheck;
            float weight = 1;

            var direction = new Vector3(Mathf.Sin(angle), Mathf.Cos(angle), 0);
            if (i >= 180 - 30 && i <= 180 + 30)
                lenght *= 5;
            else weight *= 20;

            directionsToCheck.Add(new GroundDirectionCheck(direction, lenght, weight));
        }

        note.Flap();
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

    private void OnCollisionEnter(Collision collision)
    {
        if (curentPower < 1)
            curentPower = -1;
        else
            curentPower = 0.1f;
    }

    private bool didFlap = false;
    void Update()
    {
        if(note.maxSpeed < totalMaxSpeed)
            note.maxSpeed += Time.deltaTime / 60f;
        CalculateMovement();

        note.SetMovement(movement.x, movement.y);
        if (flap)
        {
            if (!didFlap)
            {
                if(curentPower >= 1)
                {
                    note.Flap();
                    curentPower -= 1;
                }
                didFlap = true;
            }
        }
        else didFlap = false;

        WorldGenerator.Instance.CheckNewPlayerPosition(transform.position);

        var rot = Quaternion.Lerp(Quaternion.identity, Quaternion.Inverse(transform.localRotation), 0.5f);
        cameraPivot.localEulerAngles = new Vector3(rot.eulerAngles.x, 0, 0);
    }

    private void FixedUpdate()
    {
        int directionClose = 0;
        float powerCollected = 0;
        foreach (var d in directionsToCheck)
        {
            var ray = new Ray(transform.position, d.direction);
            if (Physics.Raycast(ray, out RaycastHit hit, d.lenght))
            {
                directionClose++;
                var distance = 1 - (transform.position - hit.point).magnitude / d.lenght;
                powerCollected += distance * d.weight;

                d.lastPointHit = hit.point;
                d.distanceRelative = distance;
            }
            else d.lastPointHit = null;
        }
        if (directionClose == directionsToCheck.Count)
            maxPower++;
        curentPower += (powerCollected * 2 / directionsToCheck.Count + 0.02f) * Time.fixedDeltaTime;
        if (curentPower > maxPower)
            curentPower = maxPower;

        GameUiBehaviour.Instance.UpdatePowerBar(curentPower, maxPower);
    }

    private void OnDrawGizmos()
    {
        foreach(var d in directionsToCheck)
        {
            Gizmos.DrawLine(transform.position, transform.position + d.direction * d.lenght);
            if (d.lastPointHit != null)
                Gizmos.DrawSphere(d.lastPointHit.Value, 0.2f * d.distanceRelative);
        }
    }
}
