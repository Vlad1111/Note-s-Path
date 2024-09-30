using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollowTarget : MonoBehaviour
{
    public Camera mainCamera;
    public Transform target;
    [Range(0, 60)]
    public float power = 1;

    private void LateUpdate()
    {
        if (target)
        {
            mainCamera.transform.position -= (mainCamera.transform.position - target.position) * Time.deltaTime * power;
            mainCamera.transform.rotation = Quaternion.Lerp(mainCamera.transform.rotation, target.transform.rotation, Time.deltaTime * power);
        }
    }
}
