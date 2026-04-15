using UnityEngine;

/// <summary>
/// Controls the table tilt during actual gameplay.
/// Supports VR controllers (gyroscope-based) and keyboard fallback.
/// Attach to the Table GameObject. Use this INSTEAD of TableTilter during gameplay.
/// </summary>
/// 

public class TableController : MonoBehaviour
{
    public enum InputMode { Keyboard, VR }

    [Header("Input")]
    public InputMode inputMode = InputMode.Keyboard;

    [Header("Keyboard Settings")]
    public float keyboardTiltSpeed = 30f;
    public float maxTiltAngle = 25f;

    [Header("VR Settings")]
    [Tooltip("Left hand transform (XR Controller)")]
    public Transform leftHand;
    [Tooltip("Right hand transform (XR Controller)")]
    public Transform rightHand;
    public float vrTiltMultiplier = 1.5f;

    private float currentTiltX = 0f;
    private float currentTiltZ = 0f;
    private Vector3 basePosition;

    private Quaternion leftHandBaseRot;
    private Quaternion rightHandBaseRot;
    private bool vrCalibrated = false;
    private float calibrateDelay = 0.5f;
    private float calibrateTimer = 0f;

    private void Start()
    {
        basePosition = transform.position;

        // Auto-detect VR: if hand transforms are assigned, switch to VR mode
        if (leftHand != null && rightHand != null)
        {
            inputMode = InputMode.VR;
        }
    }

    private void Update()
    {
        switch (inputMode)
        {
            case InputMode.Keyboard:
                HandleKeyboardInput();
                break;
            case InputMode.VR:
                HandleVRInput();
                break;
        }

        // Clamp tilt angles
        currentTiltX = Mathf.Clamp(currentTiltX, -maxTiltAngle, maxTiltAngle);
        currentTiltZ = Mathf.Clamp(currentTiltZ, -maxTiltAngle, maxTiltAngle);

        // Apply rotation, keep position fixed
        transform.rotation = Quaternion.Euler(currentTiltX, 0f, currentTiltZ);
        transform.position = basePosition;
    }

    private void HandleKeyboardInput()
    {
        // W/S tilts forward/back (X axis), A/D tilts left/right (Z axis)
        float inputX = -Input.GetAxis("Vertical");   // W = tilt forward
        float inputZ = -Input.GetAxis("Horizontal");  // A = tilt left

        currentTiltX += inputX * keyboardTiltSpeed * Time.deltaTime;
        currentTiltZ += inputZ * keyboardTiltSpeed * Time.deltaTime;

        // Spring back to center when no input
        if (Mathf.Abs(Input.GetAxis("Vertical")) < 0.01f)
            currentTiltX = Mathf.Lerp(currentTiltX, 0f, Time.deltaTime * 3f);
        if (Mathf.Abs(Input.GetAxis("Horizontal")) < 0.01f)
            currentTiltZ = Mathf.Lerp(currentTiltZ, 0f, Time.deltaTime * 3f);
    }

    private void HandleVRInput()
    {
        if (leftHand == null || rightHand == null) return;

        // Wait a moment then snapshot the starting hand rotation as neutral
        if (!vrCalibrated)
        {
            calibrateTimer += Time.deltaTime;
            if (calibrateTimer >= calibrateDelay)
            {
                leftHandBaseRot = leftHand.rotation;
                rightHandBaseRot = rightHand.rotation;
                vrCalibrated = true;
            }
            return; // keep table flat until calibrated
        }

        // Use delta from baseline instead of raw rotation
        Vector3 leftDelta = (leftHand.rotation * Quaternion.Inverse(leftHandBaseRot)).eulerAngles;
        Vector3 rightDelta = (rightHand.rotation * Quaternion.Inverse(rightHandBaseRot)).eulerAngles;

        float avgTiltX = (NormalizeAngle(leftDelta.x) + NormalizeAngle(rightDelta.x)) / 2f;
        float avgTiltZ = (NormalizeAngle(leftDelta.z) + NormalizeAngle(rightDelta.z)) / 2f;

        float heightDiff = rightHand.position.y - leftHand.position.y;
        float rollFromHeight = heightDiff * 30f;

        currentTiltX = avgTiltX * vrTiltMultiplier;
        currentTiltZ = (avgTiltZ + rollFromHeight) * vrTiltMultiplier;
    }

    private float NormalizeAngle(float angle)
    {
        if (angle > 180f) angle -= 360f;
        return angle;
    }
}
