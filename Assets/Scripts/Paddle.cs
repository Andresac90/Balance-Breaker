using UnityEngine;
using UnityEngine.XR;

/// <summary>
/// A Pong-style paddle that slides along one axis on the table surface.
/// Left paddle uses left thumbstick; right paddle uses right thumbstick.
/// Attach as a CHILD of the Table GameObject so it inherits table tilt.
/// </summary>
public class Paddle : MonoBehaviour
{
    public enum PaddleSide { Left, Right }
    public enum SlideAxis { X, Z }
    public enum InputMode { Auto, Keyboard, VR }

    [Header("Paddle Identity")]
    [Tooltip("Left = left thumbstick / Q,E keys. Right = right thumbstick / O,P keys.")]
    public PaddleSide side = PaddleSide.Left;

    [Header("Slide Settings")]
    [Tooltip("How far the paddle can slide along its axis (in table-local units)")]
    public float slideRange = 0.7f;

    [Tooltip("How fast the paddle moves")]
    public float moveSpeed = 2f;

    [Tooltip("Which local axis the paddle slides along")]
    public SlideAxis slideAxis = SlideAxis.X;

    [Header("Input")]
    public InputMode inputMode = InputMode.Auto;
    [Tooltip("Deadzone for thumbstick input")]
    public float thumbstickDeadzone = 0.15f;

    [Header("Physics")]
    [Tooltip("When paddle hits a ball, how much extra push force to apply")]
    public float knockbackBoost = 2f;

    // --- Internal ---
    private Vector3 startLocalPos;
    private float currentSlide = 0f;
    private InputDevice xrDevice;

    private void Start()
    {
        startLocalPos = transform.localPosition;
    }

    private void Update()
    {
        float input = ReadInput();

        currentSlide += input * moveSpeed * Time.deltaTime;
        currentSlide = Mathf.Clamp(currentSlide, -slideRange, slideRange);

        Vector3 newLocalPos = startLocalPos;
        if (slideAxis == SlideAxis.X)
            newLocalPos.x = startLocalPos.x + currentSlide;
        else
            newLocalPos.z = startLocalPos.z + currentSlide;

        transform.localPosition = newLocalPos;
    }

    private float ReadInput()
    {
        // Try VR first if available (unless forced keyboard)
        if (inputMode != InputMode.Keyboard)
        {
            float vrValue;
            if (TryGetVRInput(out vrValue))
                return vrValue;
        }

        // Fallback to keyboard
        return GetKeyboardInput();
    }

    private bool TryGetVRInput(out float value)
    {
        value = 0f;

        if (!xrDevice.isValid)
        {
            XRNode node = (side == PaddleSide.Left) ? XRNode.LeftHand : XRNode.RightHand;
            xrDevice = InputDevices.GetDeviceAtXRNode(node);
            if (!xrDevice.isValid) return false;
        }

        Vector2 axis;
        if (!xrDevice.TryGetFeatureValue(CommonUsages.primary2DAxis, out axis))
            return false;

        float raw = (slideAxis == SlideAxis.X) ? axis.x : axis.y;

        if (Mathf.Abs(raw) < thumbstickDeadzone) return true; // VR present but stick centered
        value = raw;
        return true;
    }

    private float GetKeyboardInput()
    {
        if (side == PaddleSide.Left)
        {
            if (Input.GetKey(KeyCode.Q)) return -1f;
            if (Input.GetKey(KeyCode.E)) return 1f;
        }
        else
        {
            if (Input.GetKey(KeyCode.O)) return -1f;
            if (Input.GetKey(KeyCode.P)) return 1f;
        }
        return 0f;
    }

    private void OnCollisionEnter(Collision collision)
    {
        Rigidbody ballRb = collision.rigidbody;
        if (ballRb == null) return;

        // Only push balls (identified by having a BallAgent component)
        if (collision.gameObject.GetComponent<BallAgent>() == null) return;

        Vector3 pushDir = -collision.contacts[0].normal;
        ballRb.AddForce(pushDir * knockbackBoost, ForceMode.Impulse);
    }
}