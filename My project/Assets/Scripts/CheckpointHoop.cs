using System.IO;
using UnityEngine;

public class CheckpointHoop : MonoBehaviour
{
    [SerializeField]
    public bool isStartingHoop;
    [SerializeField]
    public bool isEndingHoop;

    BoxCollider circleCollider;
    GameObject playerController;
    ReplayController replayController;

    RaceManager raceManager;
    Menu menuReference;

    private void Awake()
    {
        playerController = GameObject.FindGameObjectWithTag("Player");
        replayController = playerController.GetComponent<ReplayController>();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        circleCollider = GetComponent<BoxCollider>();
        raceManager = RaceManager.Instance;
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Collision");
        if (other.gameObject == playerController && raceManager != null)
        {
            if (isStartingHoop)
            {
                //Debug.Log(replayController.frames.Count);
                raceManager.startTime = Time.time;
                replayController.StartRecording();


                if (raceManager.isRacingSelf)
                {
                    string path = Path.Combine(Application.persistentDataPath, "bestGhostReplay.dat");
                    if (File.Exists(path))
                    {
                        replayController.replayFrames = replayController.LoadReplay(path);
                        replayController.PlayReplay();
                    }
                }
                else
                {
                    string path;
                    if(menuReference.selectedScene == "WindingTrack")
                    {
                        path = "Assets/Files/WindingTimeToBeat.dat";
                    }
                    else
                    {
                        path = "Assets/Files/CircuitTimeToBeat.dat";
                    }

                    if (File.Exists(path))
                    {
                        replayController.replayFrames = replayController.LoadReplay(path);
                        replayController.PlayReplay();
                    }
                }
            }
            else if (isEndingHoop)
            {
                raceManager.endTime = Time.time - raceManager.startTime;
                replayController.StopRecording();
                replayController.SaveReplayIfBetter(replayController.recordingFrames);
            }
            else
            {
                raceManager.currentCheckpoint++;
                raceManager.checkpointTimestamps[raceManager.currentCheckpoint] = Time.time - raceManager.startTime;
            }
        }
    }
}
