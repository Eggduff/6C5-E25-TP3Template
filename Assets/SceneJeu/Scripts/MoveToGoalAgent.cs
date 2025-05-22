using System;
using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

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
        AddReward(-0.005f);

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

        if (other.CompareTag("goal") && goalTransform.tag == "button")
        {
            if (doorCollider.enabled == false)
            {
               
                goalTransform = doorTransform;
                AddReward(1f);
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
        if (other.CompareTag("goal") && buttonTransform.tag == "door")
        {
            AddReward(5f);
            floorRenderer.material = succesMaterial;
            EndEpisode();
        }
        //On collision with wall... defeat!!!
        else if (other.CompareTag("wall"))
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

        transform.localPosition = new Vector3(0, 0, 0);
        goalTransform = buttonTransform;
               //Vector3 move = new Vector3(Random.Range(-3f, 1f), 0, Random.Range(-2f, 2f));
        //transform.Translate(move);

        //buttonTransform.localPosition = new Vector3(0, 0, 0);
        //move = new Vector3(Random.Range(1f, 3f), 0, Random.Range(-2f, 2f));
        //doorTransform.Translate(move);
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
