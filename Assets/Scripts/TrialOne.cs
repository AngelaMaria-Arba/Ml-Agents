using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class TrialOne : Agent
{
    // Ball Variables
    [SerializeField] private Transform target;
    [SerializeField] private List<GameObject> objectSpawnedList = new List<GameObject>();
    public int count = 5;
    public GameObject food;    
    
    // Agent Variables
    [SerializeField] private float moveSpeed = 6f;
    private Rigidbody rb;

    // Env Variables
    [SerializeField] private Transform envLocation;
    Material envMaterial;
    public GameObject env;

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        envMaterial = env.GetComponent<Renderer>().material;
    }
    public override void OnEpisodeBegin()
    {
        // Spawn position Agent
        transform.localPosition = new Vector3(Random.Range(-4f, 4f), 0.3f, Random.Range(-4f, 4f));
       // create a random number of coins each round
        count = Random.Range(2, 7);
        // Spawn position Ball
        // target.localPosition = new Vector3(Random.Range(-4f, 4f), 0.3f, Random.Range(-4f, 4f));
        CreateBall();
       
    }
    private void CreateBall()
    {
        if(objectSpawnedList.Count != 0) 
        {
            RemoveBall(objectSpawnedList);
        }
        for (int i = 0; i < count;i++)
        {
            // Spawning 
            GameObject newBall = Instantiate(food);
            
            // Child of env
            newBall.transform.parent = envLocation;

            //Give random Location to spawn
            Vector3 ballLocation = new Vector3(Random.Range(-4f, 4f), 0.3f, Random.Range(-4f, 4f));

            // Spawn in new Location
            newBall.transform.localPosition = ballLocation;

            //Add to list
            objectSpawnedList.Add(newBall);
        }
    }
    private void RemoveBall(List<GameObject> toBeDeletedGameObjectList)
    {
        foreach (GameObject i in toBeDeletedGameObjectList)
        {
            Destroy(i.gameObject);
        }
        toBeDeletedGameObjectList.Clear();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        // sensor.AddObservation(target.localPosition);
    }
    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveRotate = actions.ContinuousActions[0];
        float moveForward = actions.ContinuousActions[1];     

        rb.MovePosition(transform.position + transform.forward * moveForward * moveSpeed * Time.deltaTime);
        transform.Rotate(0f, moveRotate * moveSpeed * 2, 0f, Space.Self);
 
    }

    public override void Heuristic(in ActionBuffers actionsOut) // Movement controlled by keyboard
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxisRaw("Horizontal");
        continuousActions[1] = Input.GetAxisRaw("Vertical");

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
            if(objectSpawnedList.Count == 0) 
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
        }
    }
}
