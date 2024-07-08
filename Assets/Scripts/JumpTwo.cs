using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class JumpTwo : Agent
{
    [SerializeField] private List<GameObject> objectSpawnedList = new List<GameObject>();
    public int count = 5;
    public GameObject food;

    public Vector3 startCorner;
    public Vector3 endCorner;
    public int rows = 5;
    public int columns = 5;
    public float rotationSpeed=4f;

    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float jumpForce = 2f;
    private Rigidbody rb;
    private bool isJumping = false;

    [SerializeField] private Transform envLocation;
    Material envMaterial;
    public GameObject env;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody component is missing on the agent.");
        }
        else
        {
            envMaterial = env.GetComponent<Renderer>().material;
            Debug.Log("Done Initialize");
        }
    }

    public override void OnEpisodeBegin()
    {
        ResetAgentPosition();
        CreateBall();
    }

    private void ResetAgentPosition()
    {
        transform.localPosition = GenerateRandomPosition();
        while (transform.localPosition == Vector3.zero)
        {
            transform.localPosition = GenerateRandomPosition();
        }
        count = Random.Range(2, 7);
        isJumping = false;
    }

    private void CreateBall()
    {
        foreach (GameObject obj in objectSpawnedList)
        {
            Destroy(obj);
        }
        objectSpawnedList.Clear();
        Debug.Log("create ball");
        SpawnBalls();
        count = objectSpawnedList.Count;
    }

    private Vector3 GenerateRandomPosition()
    {
        Vector3 randomPosition = Vector3.zero;
        bool positionFound = false;

        while (!positionFound)
        {
            float xRand = Random.Range(-4f, 4f);
            float zRand = Random.Range(-4f, 4f);
            randomPosition = new Vector3(xRand, 0.3f, zRand);

            if (!(randomPosition.z < 1.3 && randomPosition.z > -1.3))
            {
                positionFound = true;
            }
        }
        return randomPosition;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        foreach (GameObject obj in objectSpawnedList)
        {
            if (obj != null)
            {
                sensor.AddObservation(obj.transform.localPosition);
            }
            else
            {
                sensor.AddObservation(Vector3.zero);
            }
        }
    }

      public override void OnActionReceived(ActionBuffers actions)
    {
        float moveForward = actions.ContinuousActions[0];
        float rotate = actions.ContinuousActions[1];
        bool jump = actions.DiscreteActions[0] == 1;

        if (jump && CheckGrounded() && !isJumping)
        {
            isJumping = true;
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            Debug.Log("Jumping");
        }
        if (!jump) { isJumping = false; }

        // Rotate the agent
        transform.Rotate(0, rotate * rotationSpeed * Time.deltaTime, 0);

        // Move the agent forward
        Vector3 forwardMovement = transform.forward * moveForward * moveSpeed * Time.deltaTime;
        rb.MovePosition(rb.position + forwardMovement);

        if (transform.position.y < -1)
        {
            Debug.Log("Not on ground");
            envMaterial.color = Color.blue;
            AddReward(-3f);
            EndEpisode();
        }
    }

    private bool CheckGrounded()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 2f))
        {
            if (hit.collider.CompareTag("Ground"))
            {
                return true;
            }
        }
        return false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Ball"))
        {
            objectSpawnedList.Remove(other.gameObject);
            Destroy(other.gameObject);
            AddReward(3f);
            if (objectSpawnedList.Count == 0)
            {
                envMaterial.color = Color.green;
                AddReward(2.5f);
                EndEpisode();
            }
        }
        else if (other.gameObject.CompareTag("Wall"))
        {
            envMaterial.color = Color.red;
            AddReward(-2f);
            EndEpisode();
            Debug.Log("Wall");
        }
    }

    void SpawnBalls()
    {
        Vector3 widthVector = (endCorner - startCorner) / columns;
        Vector3 heightVector = Vector3.Cross(widthVector, Vector3.up).normalized * (Vector3.Distance(startCorner, endCorner) / rows);

        for (int i = 0; i < columns; i++)
        {
            for (int j = 0; j < rows; j++)
            {
                Vector3 spawnPosition = startCorner + i * widthVector + j * heightVector;
                spawnPosition.y = 0.3f;
                objectSpawnedList.Add(Instantiate(food, spawnPosition, Quaternion.identity));
            }
        }
        Debug.Log("Count =" + objectSpawnedList.Count);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxisRaw("Vertical"); // Forward movement
        continuousActions[1] = Input.GetAxisRaw("Horizontal"); // Rotation

        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;
        discreteActions[0] = Input.GetKey(KeyCode.Space) ? 1 : 0;
    }
}