using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ReplayController : MonoBehaviour {
    public struct CaptureFrame {
        public float DeltaTime;
        public Vector3 Position;
        public Quaternion Rotation;
    }

    public bool isRecording;
    public bool isReplaying;
    private float replayCounter;
    private float replaySummedFrameDt;

    private int currentFrame;
    public List<CaptureFrame> recordingFrames;
    public List<CaptureFrame> replayFrames;


    [SerializeField]
    private Transform toRecord;

    [SerializeField]
    private Transform toReplayOn;

    public event Action OnReplayBegin;
    public event Action OnReplayEnd;
    public event Action OnRecordingBegin;
    public event Action OnRecordingEnd;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        recordingFrames = new List<CaptureFrame>();

        if (toRecord == null) {
            toRecord = transform;
        }

        if (toReplayOn == null) {
            toReplayOn = transform;
        }
    }

    // Update is called once per frame
    void FixedUpdate() {
        //NEW if your JUST recording
        if (isRecording && !isReplaying) {
            UpdateRecording(Time.fixedDeltaTime);
            //NEW if your JUST replaying
        } else if (isReplaying && !isRecording) {
            UpdateReplay(Time.fixedDeltaTime);
        }
        //NEW
        else if (isRecording && isReplaying)
        {
            UpdateRecording(Time.fixedDeltaTime);
            UpdateReplay(Time.fixedDeltaTime);
        }
    }

    private void UpdateRecording(float dt) {
        // always add frames as fast as you can!
        recordingFrames.Add(new CaptureFrame() {
            DeltaTime = dt,
            Position = toRecord.transform.position,
            Rotation = toRecord.transform.rotation
        });
    }

    private void UpdateReplay(float dt) {
        if (currentFrame < replayFrames.Count) {
            replayCounter += dt;

            // if counter has surpassed all frame deltaTime sum so far,
            //   update sum, update frame, and go to the next frame
            if (replayCounter >= replaySummedFrameDt) {
                replaySummedFrameDt += replayFrames[currentFrame].DeltaTime;
                CaptureFrame frame = replayFrames[currentFrame];

                toReplayOn.transform.SetPositionAndRotation(frame.Position, frame.Rotation);

                currentFrame++;
            }

        } else {
            Debug.Log("At end of animation! ending loop...");
            isReplaying = false;
            OnReplayEnd?.Invoke();
        }
    }

    public void StartRecording() {
        recordingFrames.Clear();
        isRecording = true;
        //isReplaying = false;
        OnRecordingBegin?.Invoke();
        Debug.Log("Began recording !!!");
    }

    public void StopRecording() {
        isRecording = false;
        Debug.Log("Ended recording !!!");
        OnRecordingEnd?.Invoke();
    }

    public void PlayReplay() {
        isReplaying = true;
        //isRecording = false;
        currentFrame = 0;
        replayCounter = 0;
        replaySummedFrameDt = 0;
        OnReplayBegin?.Invoke();
        Debug.Log("Began replay ...");
    }

    public void SaveReplay(List<CaptureFrame> frames)
    {
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string fileName = $"ghostReplay_{timestamp}.dat";

        string path = Path.Combine(Application.persistentDataPath, fileName);

        using var stream = new FileStream(path, FileMode.Create);
        using var writer = new BinaryWriter(stream);

        writer.Write(frames.Count);
        foreach (var frame in frames)
        {
            writer.Write(frame.DeltaTime);
            writer.Write(frame.Position.x);
            writer.Write(frame.Position.y);
            writer.Write(frame.Position.z);
            writer.Write(frame.Rotation.x);
            writer.Write(frame.Rotation.y);
            writer.Write(frame.Rotation.z);
            writer.Write(frame.Rotation.w);
        }

        Debug.Log($"Replay saved to: {path}");
    }
    public void SaveReplayIfBetter(List<CaptureFrame> frames)
    {
        string fileName = "bestGhostReplay.dat";
        string path = Path.Combine(Application.persistentDataPath, fileName);

        bool shouldSave = true;

        if (File.Exists(path))
        {
            List<CaptureFrame> existingFrames = LoadReplay(path);
            if (recordingFrames.Count >= existingFrames.Count)
            {
                // Existing replay is longer or equal, don't overwrite
                shouldSave = false;
                Debug.Log("Existing replay is better or equal. Not overwriting.");
            }
        }
        
        if (shouldSave)
        {
            if (File.Exists(path)) { File.Delete(path); }
            //clear the stream
            using var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
            stream.SetLength(0);
            using var writer = new BinaryWriter(stream);

            writer.Write(frames.Count);
            foreach (var frame in frames)
            {
                writer.Write(frame.DeltaTime);
                writer.Write(frame.Position.x);
                writer.Write(frame.Position.y);
                writer.Write(frame.Position.z);
                writer.Write(frame.Rotation.x);
                writer.Write(frame.Rotation.y);
                writer.Write(frame.Rotation.z);
                writer.Write(frame.Rotation.w);
            }

            Debug.Log($"Replay saved to: {path}");
        }
    }


    public List<CaptureFrame> LoadReplay(string path)
    {
        var frames = new List<CaptureFrame>();
        using var stream = new FileStream(path, FileMode.Open);
        using var reader = new BinaryReader(stream);
        int count = reader.ReadInt32();
        for (int i = 0; i < count; i++)
        {
            CaptureFrame frame = new CaptureFrame
            {
                DeltaTime = reader.ReadSingle(),
                Position = new Vector3(
                    reader.ReadSingle(),
                    reader.ReadSingle(),
                    reader.ReadSingle()),
                Rotation = new Quaternion(
                    reader.ReadSingle(),
                    reader.ReadSingle(),
                    reader.ReadSingle(),
                    reader.ReadSingle())
            };
            frames.Add(frame);
        }
        Debug.Log(frames.Count);
        return frames;
    }
}
