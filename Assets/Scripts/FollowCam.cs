using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCam : MonoBehaviour
{
    public float moveSmoothness;
    public float rotSmoothness;

    public Vector3 moveOffset;
    public Vector3 rotOffset;

    public Transform catTarget;

    private void FixedUpdate()
    {
        FollowTarget();
    }
    void FollowTarget()
    {
        HandleMovement();
        HandleRotation();
    }
    void HandleMovement()
    {
        Vector3 targetPos = new Vector3();
        targetPos = catTarget.TransformPoint(moveOffset);

        transform.position = Vector3.Lerp(transform.position, targetPos, 1.0f);
    }

    void HandleRotation()
    {
        var direction = catTarget.position - transform.position;
        var rotation = new Quaternion();

        rotation = Quaternion.LookRotation(direction + rotOffset, Vector3.up);

        transform.rotation = Quaternion.Lerp(transform.rotation, rotation, rotSmoothness * Time.deltaTime);
    }
}
