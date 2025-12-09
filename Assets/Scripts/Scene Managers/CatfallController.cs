using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CatfallController : MonoBehaviour
{

    [SerializeField] private Rigidbody catRb;
    [SerializeField] private Collider catCollider;
    public LayerMask groundLayer;
    public float groundCheckRadius = 0.5f;
    public float stabilizationProportionalStrength = 1.0f;
    public float stabilizationIntegralStrength = 0.1f;
    public float stabilizationDerivativeStrength = 0.75f;

    private Vector3 groundSurfaceNormal, forwardDirection, integralError = Vector3.zero, previousError = Vector3.zero;
    private float timeInAir = 0f;
    public bool isGrounded = false, wasGrounded = true;
    private RaycastHit hit;

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            float randomX = Random.Range(catCollider.bounds.min.x, catCollider.bounds.max.x);
            float randomZ = Random.Range(catCollider.bounds.min.z, catCollider.bounds.max.z);
            Vector3 randomPos = new Vector3(randomX, catCollider.bounds.min.y, randomZ);

            Vector3 randomTorque = new Vector3(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f)).normalized;

            catRb.AddForceAtPosition(Vector3.up * 750.0f, randomPos);
            catRb.AddTorque(randomTorque * Random.Range(10.0f, 50.0f));
            Debug.DrawLine(transform.position, randomPos, Color.blue);
        }
    }

    public void FixedUpdate()
    {
        wasGrounded = isGrounded;
        isGrounded = Physics.Raycast(transform.position, Vector3.down, out hit, groundCheckRadius, groundLayer);

        // Detect takeoff
        if (!isGrounded && wasGrounded)
        {
            AirbornTrigger();
        }
        // Detect landing
        else if (isGrounded && !wasGrounded)
        {
            LandingTrigger();
        }

        // Apply stabilization if airborn
        if (!isGrounded)
        {
            timeInAir += Time.fixedDeltaTime;
            UpdateStabilization();
        }
    }

    private void AirbornTrigger()
    {
        groundSurfaceNormal = hit.normal;
        forwardDirection = transform.forward;

        timeInAir = 0.0f;

        integralError = Vector3.zero;
        previousError = Vector3.zero;
    }

    private void LandingTrigger()
    {
        timeInAir = 0.0f;
        integralError = Vector3.zero;
        previousError = Vector3.zero;
    }

    private void UpdateStabilization() //https://discussions.unity.com/t/use-torque-to-match-rotation-to-a-second-object/649161
    {
        if (timeInAir < 0.5f)
            return;

        float factor = Mathf.Clamp01((timeInAir - 0.5f) / 0.3f);

        Vector3 targetUp = groundSurfaceNormal;
        Vector3 targetForward = forwardDirection;
        targetForward = Vector3.ProjectOnPlane(targetForward, targetUp); // makes sure foward is orthogonal to up

        Quaternion targetRotation = Quaternion.LookRotation(targetForward, targetUp);
        ApplyStabilization(targetRotation, factor);
    }

    private void ApplyStabilization(Quaternion _targetRotation, float _factor) //https://www.youtube.com/watch?v=Y3MgFS-9l3s
    {

        Quaternion rotationDifference = _targetRotation * Quaternion.Inverse(catRb.rotation); // get difference from target to current rotation
        rotationDifference.ToAngleAxis(out float angle, out Vector3 axisOfRotation);

        if (angle > 180.0f)
        {
            angle -= 360;
        }

        Vector3 error = axisOfRotation * angle;

        // Proportianal
        Vector3 proportionalTerm = error * stabilizationProportionalStrength;

        // Integral
        integralError += error * Time.fixedDeltaTime;
        integralError = Vector3.ClampMagnitude(integralError, 10.0f); // prevent integral windup
        Vector3 integralTerm = integralError * stabilizationIntegralStrength;

        // Derivative
        Vector3 derivativeTerm = (error - previousError) / Time.fixedDeltaTime;
        derivativeTerm = derivativeTerm * stabilizationDerivativeStrength;

        // PID
        Vector3 pidTotal = proportionalTerm + integralTerm + derivativeTerm;
        previousError = error;

        catRb.AddTorque(pidTotal * _factor, ForceMode.Acceleration);
    }
}
