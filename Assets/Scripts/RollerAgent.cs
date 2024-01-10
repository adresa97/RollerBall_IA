using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using static UnityEngine.GraphicsBuffer;

public class RollerAgent : Agent
{
    public Transform target;
    public float forceMultiplier = 10;

    private Rigidbody rBody;
    private bool isAtTarget;

    void Start()
    {
        rBody = GetComponent<Rigidbody>();
        isAtTarget = false;
    }

    public override void OnEpisodeBegin()
    {
        // If agent fell from platform
        if (this.transform.localPosition.y < 0)
        {
            this.rBody.angularVelocity = Vector3.zero;
            this.rBody.velocity = Vector3.zero;
            this.transform.localPosition = new Vector3(0, 0.5f, 0);
        }

        // Move the target to a new spot
        target.localPosition = new Vector3(Random.value * 8 - 4, 0.5f, Random.value * 8 - 4);

        // Reset isAtTarget to false
        isAtTarget = false;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Target and agent positions
        sensor.AddObservation(target.localPosition);
        sensor.AddObservation(this.transform.localPosition);

        // Agent velocity
        sensor.AddObservation(rBody.velocity.x);
        sensor.AddObservation(rBody.velocity.z);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // Actions, 2 (0 -> x force, 1 -> z force)
        Vector3 controlSignal = Vector3.zero;
        controlSignal.x = actionBuffers.ContinuousActions[0];
        controlSignal.z = actionBuffers.ContinuousActions[1];
        rBody.AddForce(controlSignal * forceMultiplier);

        // Rewards
        // Reached target
        if (isAtTarget)
        {
            SetReward(1.0f);
            EndEpisode();
        }

        // Fell off platform
        else if (this.transform.localPosition.y < 0)
        {
            EndEpisode();
        }
    }

    public void OnTriggerStay(Collider other)
    {
        // Function is stay and not enter to detect when target is moved to a position that is already colliding with agent
        // If this agent is inside target trigger set isAtTarget to true in order to detect arrival
        if (other.gameObject == target.gameObject) isAtTarget = true;
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Horizontal");
        continuousActionsOut[1] = Input.GetAxis("Vertical");
    }
}
