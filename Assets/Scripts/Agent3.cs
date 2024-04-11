using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class Agent3 : Agent
{
    [SerializeField] private Transform target;
    [SerializeField] private float moveSpeed = 6f;
    //[SerializeField] private float fallMultiplier = 2.5f;
    //[SerializeField] private float lowJumpMultiplier = 2f;

    private Rigidbody rb;

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
    }
    public override void OnEpisodeBegin()
    {
        // Spawn position Agent
        transform.localPosition = new Vector3(Random.Range(-4f, 4f), 0.3f, Random.Range(-4f, 4f));

        // Spawn position Ball
        target.localPosition = new Vector3(Random.Range(-4f, 4f), 0.3f, Random.Range(-4f, 4f));
     }
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(target.localPosition);
    }
    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveRotate = actions.ContinuousActions[0];
        float moveForward = actions.ContinuousActions[1];
        //float moveDown = actions.ContinuousActions[2];

        rb.MovePosition(transform.position + transform.forward * moveForward * moveSpeed * Time.deltaTime);
        transform.Rotate(0f, moveRotate * moveSpeed * 2, 0f, Space.Self);

       

        /*
            Vector3 velocity = new(moveX, 0f, moveZ);
            velocity = moveSpeed * Time.deltaTime * velocity.normalized;
            transform.localPosition += velocity;
        */
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
            AddReward(2f);
            EndEpisode();
        }
        if (other.gameObject.CompareTag("Wall"))
        {
            AddReward(-1f);
            EndEpisode();
        }
    }
}
