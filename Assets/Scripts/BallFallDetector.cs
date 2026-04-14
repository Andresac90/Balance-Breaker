using UnityEngine;

/// <summary>
/// Detects when the ball enters a FallZone trigger during gameplay.
/// The GameManager attaches this at runtime and sets the onFell callback.
/// </summary>
public class BallFallDetector : MonoBehaviour
{
    public System.Action onFell;
    private bool hasFallen = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasFallen) return;

        if (other.CompareTag("FallZone"))
        {
            hasFallen = true;
            onFell?.Invoke();
        }
    }
}
