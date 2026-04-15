using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class BallAgent : Agent
{
    [Header("References")]
    [Tooltip("Assign the Table transform in the Inspector")]
    public Transform table;

    [Header("Agent Settings")]
    public float moveForce = 1.5f;
    public float maxSpeed = 3f;

    private Rigidbody rb;
    private Vector3 startLocalPos;
    private Vector3 tableStartPos;
    private Quaternion tableStartRot;
    [HideInInspector] public bool isGameplay = false;

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();

        // Store initial positions relative to table
        if (table != null)
        {
            startLocalPos = table.InverseTransformPoint(transform.position);
            tableStartPos = table.position;
            tableStartRot = table.rotation;
        }
    }

    public override void OnEpisodeBegin()
    {
        if (isGameplay) return; // don't reset table or reposition during gameplay

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        float randX = Random.Range(-0.7f, 0.7f);
        float randZ = Random.Range(-0.5f, 0.5f);
        Vector3 localSpawn = new Vector3(randX, startLocalPos.y, randZ);
        if (table != null)
        {
            table.position = tableStartPos;
            table.rotation = tableStartRot;
            transform.position = table.TransformPoint(localSpawn);
            transform.rotation = Quaternion.identity;
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        if (table == null) return;

        // 1. Table tilt (X and Z euler angles, normalized to ~[-1, 1])
        Vector3 tableEuler = table.eulerAngles;
        float tiltX = NormalizeAngle(tableEuler.x) / 45f;
        float tiltZ = NormalizeAngle(tableEuler.z) / 45f;
        sensor.AddObservation(tiltX); // obs 1
        sensor.AddObservation(tiltZ); // obs 2

        // 2. Ball's local position on the table (normalized by table half-size)
        Vector3 localPos = table.InverseTransformPoint(transform.position);
        sensor.AddObservation(localPos.x / 1f);   // obs 3
        sensor.AddObservation(localPos.z / 0.75f); // obs 4

        // 3. Ball's local velocity relative to table
        Vector3 localVel = table.InverseTransformDirection(rb.linearVelocity);
        sensor.AddObservation(localVel.x / maxSpeed); // obs 5
        sensor.AddObservation(localVel.z / maxSpeed); // obs 6

        // 4. Distance to nearest edge (normalized, 0 = at edge, 1 = center)
        float distToEdgeX = 1f - Mathf.Abs(localPos.x) / 1f;
        float distToEdgeZ = 0.75f - Mathf.Abs(localPos.z) / 0.75f;
        float nearestEdgeDist = Mathf.Min(distToEdgeX, distToEdgeZ);
        sensor.AddObservation(nearestEdgeDist); // obs 7
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // Continuous actions: X force and Z force on the table surface
        float forceX = Mathf.Clamp(actions.ContinuousActions[0], -1f, 1f);
        float forceZ = Mathf.Clamp(actions.ContinuousActions[1], -1f, 1f);

        // Apply force in table's local space so the ball pushes relative to the surface
        Vector3 worldForce = table.TransformDirection(new Vector3(forceX, 0f, forceZ)) * moveForce;
        rb.AddForce(worldForce, ForceMode.Force);

        // Clamp speed
        if (rb.linearVelocity.magnitude > maxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }

        // --- Shaping Rewards ---
        Vector3 localPos = table.InverseTransformPoint(transform.position);
        float distToCenter = new Vector2(localPos.x, localPos.z).magnitude;
        float maxDist = new Vector2(1f, 0.75f).magnitude;
        float normalizedDist = distToCenter / maxDist;

        // Small reward for being near edges (encourages escaping)
        AddReward(normalizedDist * 0.005f);

        // Small penalty for sitting still near center (discourages passivity)
        if (normalizedDist < 0.2f && rb.linearVelocity.magnitude < 0.1f)
        {
            AddReward(-0.002f);
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // Keyboard fallback for testing: arrow keys / WASD move the ball
        var continuous = actionsOut.ContinuousActions;
        continuous[0] = Input.GetAxis("Horizontal"); // A/D or Left/Right
        continuous[1] = Input.GetAxis("Vertical");   // W/S or Up/Down
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("FallZone"))
        {
            if (isGameplay) return; 
            AddReward(1.0f);
            EndEpisode();
        }
    }

    /// <summary>
    /// Converts 0-360 euler angles to -180 to 180 range.
    /// </summary>
    private float NormalizeAngle(float angle)
    {
        if (angle > 180f) angle -= 360f;
        return angle;
    }
}
