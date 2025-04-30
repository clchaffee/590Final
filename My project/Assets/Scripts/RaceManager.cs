using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RaceManager : MonoBehaviour
{
    public static RaceManager Instance;

    [SerializeField]
    public CheckpointHoop[] checkpoints = null;

    public int currentCheckpoint = 0;

    public float startTime = 0;
    public float endTime = 0;
    public float[] checkpointTimestamps;

    private bool isTutorialVisible = true;
    public bool isRacingSelf = false;
    GameObject controlObject;
    private void Awake()
    {
        controlObject = GameObject.Find("Control Overlay");
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

        for (int i = 0; i < checkpoints.Length; i++)
        {
            Transform arrowTransform = checkpoints[i].transform.Find("arrow pointer");
            if (arrowTransform != null)
            {
                Vector3 targetDir = (checkpoints[i + 1].transform.position - arrowTransform.position).normalized;

                // Rotate so that the arrow's local UP (Y axis) points toward the next checkpoint
                Quaternion rotation = Quaternion.FromToRotation(Vector3.up, targetDir);
                arrowTransform.rotation = rotation;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (SceneManager.GetActiveScene().name == "Menu")
        {
            return;
        }
        //Tutorial code so it blocks everything while tutorial screen is up
        if (isTutorialVisible)
        {

            if (Input.anyKeyDown)
            {
                isTutorialVisible = false;
                controlObject.SetActive(false);
            }
            return;
        }
        //Don't run while in the menu

        if (Input.GetKeyDown(KeyCode.H))
        {
            isTutorialVisible = true;
            controlObject.SetActive(true);
        }
        //Checks to see if the current checkpoint is active
        if (checkpoints[currentCheckpoint].gameObject.activeSelf)
        {
            //Checks to make sure you don't go backwards from the start
            if (currentCheckpoint == 0) checkpoints[currentCheckpoint + 1].gameObject.SetActive(true);
            //Checks to make sure you don't modify past the end 
            else if (currentCheckpoint == checkpoints.Length - 1)
            {
                checkpoints[currentCheckpoint - 1].gameObject.SetActive(false);
            }
            //If you've passed the end
            else if (currentCheckpoint == checkpoints.Length)
            {

            }
            //When you go through a checkpoint, show the next one and hide the previous
            else
            {
                //Hide the past and current checkpoint, show the next one
                checkpoints[currentCheckpoint].gameObject.SetActive(false);
                checkpoints[currentCheckpoint - 1].gameObject.SetActive(false);
                checkpoints[currentCheckpoint + 1].gameObject.SetActive(true);
            }
        }
    }
}
