using System;
using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

using Random = UnityEngine.Random;

public class MoveToGoalAgent : Agent
{
    //The transform of the goal to reach 
    [SerializeField] private Transform doorTransform;
    [SerializeField] private Transform buttonTransform;
     private Transform goalTransform;
    [SerializeField] private BoxCollider doorCollider;


    //two materials to color the scene in case of success or failure to give a visual feedback
    [SerializeField] private Material succesMaterial;
    [SerializeField] private Material failureMaterial;
    [SerializeField] private Renderer floorRenderer;

    //Agent speed for configuration purpose
    [SerializeField] private float speed = 6;


    //This methods is what the agent does with the action decided
    public override void OnActionReceived(ActionBuffers actions)
    {
        //First action is X move
        float moveX = actions.ContinuousActions[0];
        //second action is Z move
        float moveY = actions.ContinuousActions[1];

        //Pour minimiser la durée..
        AddReward(-0.01f);

        //Move!!!
        transform.Translate(new UnityEngine.Vector3(moveX, 0, moveY) * Time.deltaTime * speed);
    }

    //This methods is the "eyes" of the agent
    //What it collects is what it knows about
    //Here: the position of the goal in the local space (x,y,z)
    //its position in the local space(x, y, z)
    public override void CollectObservations(VectorSensor sensor)
    {
        //the position of the agent in the local space (x,y,z)
        sensor.AddObservation(transform.localPosition);
        //the position of the goal in the local space (x,y,z)
        sensor.AddObservation(goalTransform.localPosition);
        
    }

    //This is how reward is managed
    private void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag("button"))
        {
            if (!doorCollider.enabled)
            {
                goalTransform = doorTransform;
                AddReward(2f);
                doorCollider.enabled = true;
            }
            else
            {
                goalTransform = buttonTransform;
                AddReward(-1f);
                doorCollider.enabled = false;
            }
        }


        //On collision with goal... success!!!
        if (other.CompareTag("door") && doorCollider.enabled)
        {
            AddReward(5f);
            floorRenderer.material = succesMaterial;
            EndEpisode();
        }
        //On collision with wall... defeat!!!
       
        if (other.CompareTag("wall"))
        {
            AddReward(-2f);
            floorRenderer.material = failureMaterial;
            EndEpisode();
        }

    }
    //This is what happens on episode start
    //Just put the agent on random position
    //Just put the goal on random position
    public override void OnEpisodeBegin()
    {
        doorCollider.enabled = false;

        // Generate unique random positions within an 8x8 grid (assuming 1 unit per grid cell)
        Vector3 agentPosition, buttonPosition, doorPosition;

        // Generate unique positions using a helper function
        agentPosition = GetRandomPosition();
        do
        {
            buttonPosition = GetRandomPosition();
        } while (buttonPosition == agentPosition);

        do
        {
            doorPosition = GetRandomPosition();
        } while (doorPosition == agentPosition || doorPosition == buttonPosition);

        // Apply positions
        transform.localPosition = agentPosition;
        buttonTransform.localPosition = buttonPosition;
        doorTransform.localPosition = doorPosition;

        // Set initial goal
        goalTransform = buttonTransform;
    }

    private Vector3 GetRandomPosition()
    {
        int x = Random.Range(-4,4);
        int z = Random.Range(-4,4);
        return new Vector3(x, 0, z);
    }

    //This is how to move the agent when keyboard driven...
    //Behaviour Type = Heuristic Only or Default with no brain and no training...
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> contActions = actionsOut.ContinuousActions;
        contActions[0] = Input.GetAxisRaw("Horizontal");
        contActions[1] = Input.GetAxisRaw("Vertical");
    }


}
