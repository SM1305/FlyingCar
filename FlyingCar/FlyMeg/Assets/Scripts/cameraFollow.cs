using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraFollow : MonoBehaviour
{
    public Transform target;
    public float distance;
    public float height;
    public float minHeight;
    public float camAcceleration; //camera lag, compared to target  //~0.15
    public Vector3 positionVelocity;


    void FixedUpdate()
    {
        //update camera position
        Vector3 updateCamPosition = target.position + (target.forward * distance);
        updateCamPosition.y = Mathf.Max(updateCamPosition.y + height, minHeight);

        transform.position = Vector3.SmoothDamp(transform.position, updateCamPosition, ref positionVelocity, camAcceleration);

        //rotate camera to look at target
        Vector3 cameraAim = target.position + (target.forward * 5);
        transform.LookAt(cameraAim);
    }
}
