using UnityEngine;

/// <summary>
/// Randomly tilts the table during training to simulate a human player.
/// Attach to the Table GameObject. Disable this during actual gameplay.
/// </summary>
public class TableTilter : MonoBehaviour
{
    [Header("Tilt Settings")]
    [Tooltip("Maximum tilt angle in degrees")]
    public float maxTiltAngle = 15f;

    [Tooltip("How fast the table tilts (lower = smoother)")]
    public float tiltSpeed = 1.5f;

    [Tooltip("How often the target tilt changes direction")]
    public float changeInterval = 1.5f;

    private float targetTiltX;
    private float targetTiltZ;
    private float timer;
    private Vector3 basePosition;

    private void Start()
    {
        basePosition = transform.position;
        PickNewTarget();
    }

    private void FixedUpdate()
    {
        timer -= Time.fixedDeltaTime;
        if (timer <= 0f)
        {
            PickNewTarget();
        }

        // Smoothly interpolate toward target tilt
        float currentX = NormalizeAngle(transform.eulerAngles.x);
        float currentZ = NormalizeAngle(transform.eulerAngles.z);

        float newX = Mathf.Lerp(currentX, targetTiltX, Time.fixedDeltaTime * tiltSpeed);
        float newZ = Mathf.Lerp(currentZ, targetTiltZ, Time.fixedDeltaTime * tiltSpeed);

        transform.rotation = Quaternion.Euler(newX, 0f, newZ);
        transform.position = basePosition; // Keep table in place
    }

    private void PickNewTarget()
    {
        targetTiltX = Random.Range(-maxTiltAngle, maxTiltAngle);
        targetTiltZ = Random.Range(-maxTiltAngle, maxTiltAngle);
        timer = changeInterval + Random.Range(-0.3f, 0.3f);
    }

    private float NormalizeAngle(float angle)
    {
        if (angle > 180f) angle -= 360f;
        return angle;
    }
}
