using System;
using UnityEngine;

public class RaceManager : MonoBehaviour
{
    public static RaceManager Instance;

    [SerializeField]
    public CheckpointHoop[] checkpoints = null;

    public int currentCheckpoint = 0;

    public float startTime = 0;
    public float endTime = 0;
    public float[] checkpointTimestamps;
    private void Awake()
    {
        Instance = this;
        if (checkpoints == null)
        {
            checkpoints = new CheckpointHoop[0];
        }

    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        for (int i = 1; i < checkpoints.Length; i++)
        {
            checkpoints[i].gameObject.SetActive(false);
        }
        checkpointTimestamps = new float[checkpoints.Length];
    }

    // Update is called once per frame
    void Update()
    {
        //Checks to see if the current checkpoint is active
        if (checkpoints[currentCheckpoint].gameObject.activeSelf)
        {
            //Checks to make sure you don't go backwards from the start
            if(currentCheckpoint == 0) checkpoints[currentCheckpoint + 1].gameObject.SetActive(true);
            //Checks to make sure you don't modify past the end 
            else if (currentCheckpoint == checkpoints.Length - 1)
            {
                checkpoints[currentCheckpoint - 1].gameObject.SetActive(false);
            }
            //If you've passed the end
            else if(currentCheckpoint == checkpoints.Length)
            {

            }
            //When you go through a checkpoint, show the next one and hide the previous
            else
            {
                checkpoints[currentCheckpoint - 1].gameObject.SetActive(false);
                checkpoints[currentCheckpoint + 1].gameObject.SetActive(true);
            }
        }        
    }
}
