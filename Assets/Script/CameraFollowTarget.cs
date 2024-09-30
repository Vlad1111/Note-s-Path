using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollowTarget : MonoBehaviour
{
    public Camera mainCamera;
    public Transform target;
    [Range(0, 1)]
    public float power = 1;

    private void FixedUpdate()
    {
        if (target)
        {
            mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, target.position, power);
            mainCamera.transform.rotation = Quaternion.Lerp(mainCamera.transform.rotation, target.transform.rotation, power);
        }
    }
}
