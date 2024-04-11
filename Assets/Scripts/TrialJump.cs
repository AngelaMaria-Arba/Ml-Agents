using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class TrialJump :  Agent
{
    // Ball Variables
    [SerializeField] private List<GameObject> objectSpawnedList = new List<GameObject>();
    public int count = 5;
    public GameObject food; 

    // Agent Variables
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float jumpForce = 8f;
    [SerializeField] private float minDistanceToObstacle = 2f;
    private Rigidbody rb;
    private bool isJumping = false;
    

    // Env Variables
    [SerializeField] private Transform envLocation; // env
    Material envMaterial;
    public GameObject env; // plane

    private bool IsCollectingBalls()
    {
        // Check if the agent is collecting balls
        bool isCollectingBalls = objectSpawnedList.Count < count;

        return isCollectingBalls;
    }
    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        // Ensure rb is not null before proceeding
        if (rb == null)
        {
            Debug.LogError("Rigidbody component is missing on the agent.");
            return;
        }
        envMaterial = env.GetComponent<Renderer>().material;
        Debug.Log("Done Initialize");
    }
    private List<Vector3> FindColliders()
    {
        List<Vector3> obstaclePositions = new List<Vector3>();
        Collider[] obstacleColliders = Physics.OverlapSphere(envLocation.position, minDistanceToObstacle * 2f);
        foreach (Collider obstacleCollider in obstacleColliders)
        {
            if (obstacleCollider.CompareTag("Obstacle"))
            {
                obstaclePositions.Add(obstacleCollider.transform.position);
                
                Debug.DrawLine(envLocation.position, obstacleCollider.transform.position, Color.blue, 2f);
                Debug.Log("Found Collider Points");
            }
        }
        return obstaclePositions;
    }
    public override void OnEpisodeBegin()
    {
        
        // Spawn position Agent
        transform.localPosition = GenerateRandomPosition();
        while (transform.localPosition == Vector3.zero) { transform.localPosition = GenerateRandomPosition(); }

        // create a random number of coins spawn each round
        count = Random.Range(2, 7);
        isJumping = false;
        
        // Spawn position Ball       
        CreateBall();

    }
   
    private void CreateBall()
    {
        if (objectSpawnedList.Count != 0)
        {
            RemoveBall(objectSpawnedList);
        }
        Debug.Log("create ball");        

        for (int i = 0; i < count; i++)
        {
            // Generate random position for the ball, avoiding obstacle positions
            Vector3 ballPosition = GenerateRandomPosition();
            while(ballPosition == Vector3.zero) { ballPosition = GenerateRandomPosition(); }

            // Spawning 
            GameObject newBall = Instantiate(food);

            // Child of env
            newBall.transform.parent = envLocation;

            // Spawn in new Location
            newBall.transform.localPosition = ballPosition;

            // Add to list
            objectSpawnedList.Add(newBall);
        }
    }

    private Vector3 GenerateRandomPosition()
    {
        Vector3 randomPosition = Vector3.zero;
        bool positionFound = false;

        // Generate random positions until a suitable one is found
        while (!positionFound)
        {
            // Generate random position within environment bounds
            float xRand = Random.Range(-4f, 4f);
            float zRand = Random.Range(-4f, 4f);
            randomPosition = new Vector3(xRand, 0.3f, zRand);
           
            if(!(randomPosition.z < 0.7 && randomPosition.z > -0.7))
            {
                positionFound = true;
            }          
        }
        return randomPosition;
    }

    private void RemoveBall(List<GameObject> toBeDeletedGameObjectList)
    {
        foreach (GameObject i in toBeDeletedGameObjectList)
        {
            Destroy(i.gameObject);
            count--;
        }
        toBeDeletedGameObjectList.Clear();
        Debug.Log("RemovedBall");
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        foreach(GameObject obj in objectSpawnedList)
        {
            sensor.AddObservation(obj.transform.localPosition); 
        }
        var emptySize = 18 - objectSpawnedList.Count * 3;
        while(emptySize > 0) { sensor.AddObservation(0f); --emptySize; }
    }


    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveRotate = actions.ContinuousActions[0];
        float moveForward = actions.ContinuousActions[1];
        bool jump = actions.DiscreteActions[0] == 1;

        // Jumping logic
        if (jump && CheckGrounded() && !isJumping)
        {
            isJumping = true;
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            Debug.Log("Jumping");
        }

       
        // Penalize the agent for not collecting balls or moving towards the target
        if (!IsCollectingBalls())
        {
            AddReward(-0.01f); // Penalize for idle behavior
        }
        if(!JumpOverObstacle()) { AddReward(-0.01f); }


        // Apply forward movement and rotation
        rb.MovePosition(transform.position + transform.forward * moveForward * moveSpeed * Time.deltaTime);
        transform.Rotate(0f, moveRotate * moveSpeed * 2, 0f, Space.Self);
    }


    private bool CheckGrounded()
    {
        // Perform a raycast downward to check if the agent is grounded
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 2f)) // Adjust the distance according to your agent's height
        {
           // Check if the ray hits the ground layer or object
            if (hit.collider.CompareTag("Ground"))
            {
                Debug.Log("on Ground");
                return true;
            }
        }
        return false;
    }

    private bool JumpOverObstacle()
    {
        // Perform a raycast downward to check if the agent is grounded
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 2f)) // Adjust the distance according to your agent's height
        {
            // Check if the ray hits the ground layer or object
            if (hit.collider.CompareTag("Obstacle"))
            {
                Debug.Log("On Obstacle");
                return true;
            }
        }
        return false;
    }

    public override void Heuristic(in ActionBuffers actionsOut) // Movement controlled by keyboard
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxisRaw("Horizontal");
        continuousActions[1] = Input.GetAxisRaw("Vertical");

        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;
        discreteActions[0] = 0;
        if (Input.GetKeyDown(KeyCode.Space) ) { discreteActions[0] = 1; Debug.Log("Jumping with space"); }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Ball"))
        {
            // Remove from List
            objectSpawnedList.Remove(other.gameObject);
            // Remove from World
            Destroy(other.gameObject);

            AddReward(2f);
            if (objectSpawnedList.Count == 0)
            {
                envMaterial.color = Color.green;
                RemoveBall(objectSpawnedList);
                AddReward(1.5f);
                EndEpisode();
            }

        }
        if (other.gameObject.CompareTag("Wall"))
        {
            envMaterial.color = Color.red;
            RemoveBall(objectSpawnedList);
            AddReward(-1f);
            EndEpisode();
            Debug.Log("Wall");

        }
        if (other.gameObject.CompareTag("Obstacle"))
        {
            envMaterial.color = Color.red;
            RemoveBall(objectSpawnedList);
            AddReward(-1f);
            EndEpisode();
            Debug.Log("Obstacle");
        }
    }
}
