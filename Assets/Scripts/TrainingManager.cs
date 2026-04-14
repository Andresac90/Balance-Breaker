using UnityEngine;

/// <summary>
/// Place this on an empty GameObject in the Training scene.
/// Handles resetting the training environment and curriculum difficulty.
/// </summary>
public class TrainingManager : MonoBehaviour
{
    [Header("References")]
    public Transform table;
    public TableTilter tableTilter;

    [Header("Difficulty Curriculum")]
    [Tooltip("Max tilt angle increases as training progresses")]
    public float easyMaxTilt = 8f;
    public float hardMaxTilt = 20f;

    /// <summary>
    /// Called externally to adjust difficulty during curriculum learning.
    /// value goes from 0 (easy) to 1 (hard).
    /// </summary>
    public void SetDifficulty(float value)
    {
        if (tableTilter != null)
        {
            tableTilter.maxTiltAngle = Mathf.Lerp(easyMaxTilt, hardMaxTilt, value);
        }
    }
}
