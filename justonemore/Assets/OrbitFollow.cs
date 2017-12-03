using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitFollow : MonoBehaviour
{
    [Header("Framing")]
    public Camera Camera;
    public Vector2 FollowTransformFraming = new Vector2(0f, 0f);
    public float FollowingSharpness = 30f;

    [Header("Distance")]
    public float DefaultDistance = 6f;
    public float MinDistance = 2f;
    public float MaxDistance = 10f;
    public float DistanceMovementSpeed = 10f;
    public float DistanceMovementSharpness = 10f;

    [Header("Rotation")]
    public bool InvertX = false;
    public bool InvertY = false;
    [Range(-90f, 90f)]
    public float DefaultVerticalAngle = 20f;
    [Range(-90f, 90f)]
    public float MinVerticalAngle = -80f;
    [Range(-90f, 90f)]
    public float MaxVerticalAngle = 80f;
    public float RotationSpeed = 10f;
    public float RotationSharpness = 30f;

    [Header("Obstruction")]
    public bool ObstructionEnabled = false;
    public float ObstructionCheckRadius = 0.5f;
    public LayerMask ObstructionLayers = -1;
    public float ObstructionSharpness = 10000f;

    public Vector3 PlanarDirection { get; private set; }
    public Transform FollowTransform;
    public List<Collider> IgnoredColliders;
    public float TargetDistance;

    private Vector2 rotationInput;
    private bool distanceIsObstructed;
    private float zoomInput;
    private float currentDistance;
    private float targetVerticalAngle;
    private RaycastHit obstructionHit;
    private int obstructionCount;
    private RaycastHit[] obstructions = new RaycastHit[MaxObstructions];
    private float obstructionTime;
    private Vector3 currentFollowPosition;

    private const int MaxObstructions = 32;

    void OnValidate()
    {
        DefaultDistance = Mathf.Clamp(DefaultDistance, MinDistance, MaxDistance);
        DefaultVerticalAngle = Mathf.Clamp(DefaultVerticalAngle, MinVerticalAngle, MaxVerticalAngle);
    }

    private void Start()
    {
        SetAngle(45f);
    }

    public void SetAngle(float angle)
    {
        targetVerticalAngle = angle;
    }

    public void SetFollowTarget(Transform target)
    {
        FollowTransform = target;

        currentDistance = DefaultDistance;

        TargetDistance = currentDistance;
        PlanarDirection = Vector3.forward;

        PlanarDirection = FollowTransform.forward;
        currentFollowPosition = FollowTransform.position;
    }

    // Receive input from the player
    public void SetInputs(float zoomInput, Vector2 rotationInput)
    {
        if (InvertX)
        {
            rotationInput.x *= -1f;
        }
        if (InvertY)
        {
            rotationInput.y *= -1f;
        }

        this.rotationInput = rotationInput;
        this.zoomInput = zoomInput;
    }

    private void LateUpdate()
    {
        float deltaTime = Time.deltaTime;

        if (FollowTransform)
        {
            // Process rotation input
            Quaternion rotationFromInput = Quaternion.Euler(FollowTransform.up * (rotationInput.x * RotationSpeed));
            PlanarDirection = rotationFromInput * PlanarDirection;
            PlanarDirection = Vector3.Cross(FollowTransform.up, Vector3.Cross(PlanarDirection, FollowTransform.up));
            targetVerticalAngle -= (rotationInput.y * RotationSpeed);
            targetVerticalAngle = Mathf.Clamp(targetVerticalAngle, MinVerticalAngle, MaxVerticalAngle);

            // Process distance input
            if (distanceIsObstructed && Mathf.Abs(zoomInput) > 0f)
            {
                TargetDistance = currentDistance;
            }
            TargetDistance += zoomInput * DistanceMovementSpeed;
            TargetDistance = Mathf.Clamp(TargetDistance, MinDistance, MaxDistance);

            // Find the smoothed follow position
            currentFollowPosition = Vector3.Lerp(currentFollowPosition, FollowTransform.position, 1f - Mathf.Exp(-FollowingSharpness * deltaTime));
//            currentFollowPosition = Vector3.Lerp(currentFollowPosition, FollowTransform.position, deltaTime);

            // Calculate smoothed rotation
            Quaternion planarRot = Quaternion.LookRotation(PlanarDirection, FollowTransform.up);
            Quaternion verticalRot = Quaternion.Euler(targetVerticalAngle, 0, 0);
            Quaternion targetRotation = Quaternion.Slerp(transform.rotation, planarRot * verticalRot, 1f - Mathf.Exp(-RotationSharpness * deltaTime));

            // Apply rotation
            transform.rotation = targetRotation;

            // Handle obstructions
            if (ObstructionEnabled) {
                RaycastHit closestHit = new RaycastHit();
                closestHit.distance = Mathf.Infinity;
                obstructionCount = Physics.SphereCastNonAlloc(currentFollowPosition, ObstructionCheckRadius, -transform.forward, obstructions, TargetDistance, ObstructionLayers, QueryTriggerInteraction.Ignore);
                for (int i = 0; i < obstructionCount; i++)
                {
                    bool isIgnored = false;
                    foreach (var collider in IgnoredColliders)
                    {
                        if (collider == obstructions[i].collider)
                        {
                            isIgnored = true;
                            break;
                        }
                    }

                    if (!isIgnored && obstructions[i].distance < closestHit.distance && obstructions[i].distance > 0)
                    {
                        closestHit = obstructions[i];
                    }
                }

                // If obstructions detecter
                if (closestHit.distance < Mathf.Infinity)
                {
                    distanceIsObstructed = true;
                    currentDistance = Mathf.Lerp(currentDistance, closestHit.distance, 1 - Mathf.Exp(-ObstructionSharpness * deltaTime));
                }
                // If no obstruction
                else
                {
                    distanceIsObstructed = false;
                    currentDistance = Mathf.Lerp(currentDistance, TargetDistance, 1 - Mathf.Exp(-DistanceMovementSharpness * deltaTime));
                }
            }

            // Find the smoothed camera orbit position
            Vector3 targetPosition = currentFollowPosition - ((targetRotation * Vector3.forward) * currentDistance);

            // Handle framing
            targetPosition += transform.right * FollowTransformFraming.x;
            targetPosition += transform.up * FollowTransformFraming.y;

            // Apply position
            transform.position = targetPosition;
        }
    }
}
