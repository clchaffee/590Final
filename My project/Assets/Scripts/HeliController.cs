using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class HeliController : MonoBehaviour
{
    /// <summary>
    /// The following is an explanation of the input map:
    ///
    /// </summary>
    /// 
    InputAction pitchAction; //Pitch Negative ("W") and Positive ("S")
    InputAction yawAction; //Yaw Negative ("Q") and Positive ("E")
    InputAction turnAction; //Turn Negative ("A") and Positive ("D")
    InputAction liftAction; //Lift Negative ("Ctrl") and Positive ("Shift")
    InputAction recordAction; //Pitch Negative ("W") and Positive ("S")
    InputAction replayAction;
    float pitchInputValue;
    float yawInputValue;
    float turnInputValue;
    float liftInputValue;

    public Rigidbody Helicopter;

    [SerializeField]
    public ReplayController ReplayController;

    private bool isRecording = false;

    public float turnScale = 3f;
    public float forwardScale = 10f;
    public float pitchScale = 20f;
    public float yawScale = 30f;

    public float pitchForcePercent = 1.5f;
    public float turnForcePersent = 1.5f;

    public float engineForce;

    private Vector2 forwardMove = Vector2.zero;
    //up and down
    private Vector2 pitch = Vector2.zero;
    //side to side
    private float yaw = 0f;
    public bool isGrounded = true;


    private void Start()
    {
        pitchAction = InputSystem.actions.FindAction("Pitch");
        yawAction = InputSystem.actions.FindAction("Yaw");
        turnAction = InputSystem.actions.FindAction("Turn");
        liftAction = InputSystem.actions.FindAction("Lift");
        recordAction = InputSystem.actions.FindAction("Toggle Recording");
        replayAction = InputSystem.actions.FindAction("Replay");
    }
    private void FixedUpdate()
    {
        pitchInputValue = pitchAction.ReadValue<float>();
        yawInputValue = yawAction.ReadValue<float>();
        turnInputValue = turnAction.ReadValue<float>();
        liftInputValue = liftAction.ReadValue<float>();

        ApplyMovement();
        ApplyLift();
        ApplyPitch();


        OnTurn(turnInputValue);
        OnPitch(pitchInputValue);
        OnYaw(yawInputValue);
        OnLift(liftInputValue);

        //The IsPressed function will not work, as this calls the method during its started, performed, and canceled phases
        //This causes the start and end functions to be called three times, causing overwriting data 
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (!isRecording)
            {

                ReplayController.StartRecording();
                isRecording = true;
            }
            else
            {
                ReplayController.StopRecording();
                isRecording = false;
            }
        }
        if (replayAction.IsPressed())
        {
            //ReplayController.SaveReplay(ReplayController.frames);
        }
    }

    private void ApplyMovement()
    {
        ////calculate turn force, reducing yaw when moving forward quickly.
        //float turnTo = turnScale * Mathf.Lerp(
        //    forwardMove.x, //base forward movement
        //    forwardMove.x * (turnForcePersent - Mathf.Abs(forwardMove.y)), //adjusting yaw for forward speed
        //    Mathf.Max(0f, forwardMove.y)); //interpolation factor based on forward movment
        ////update yaw based on the turn force
        //yaw = Mathf.Lerp(yaw, turnTo, Time.fixedDeltaTime * turnScale);
        ////apply torque to yaw the helicopter
        //Helicopter.AddRelativeTorque(0f, yaw * Helicopter.mass, 0f);
        ////apply force to move the helicopter forward
        //Helicopter.AddRelativeForce(Vector3.forward * Mathf.Max(0f, forwardMove.y * forwardScale * Helicopter.mass));

        float turnTo = turnScale * Mathf.Lerp(0, forwardMove.x, 1f - Mathf.Abs(forwardMove.y));
        yaw = Mathf.Lerp(yaw, turnTo, Time.fixedDeltaTime * turnScale);
        Helicopter.AddRelativeTorque(0f, yaw * Helicopter.mass, 0f);
        Helicopter.AddRelativeForce(Vector3.forward * forwardMove.y * forwardScale * Helicopter.mass);


    }

    private void ApplyLift()
    {
        //heavier helicopters are harder to lift
        float upwardForce = 1 - Mathf.Clamp(Helicopter.transform.position.y / 100, 0, 1);
        //calculate force based on the current engine force and upward force
        upwardForce = Mathf.Lerp(0f, engineForce, upwardForce) * Helicopter.mass;
        Helicopter.AddRelativeForce(Vector3.up * upwardForce);
    }

    private void ApplyPitch()
    {
        ////calculate the x and y of pitch dependant on the pitch scale and current pitch and yaw
        //pitch.x = Mathf.Lerp(pitch.x, pitch.x * yawScale, Time.fixedDeltaTime * pitchScale);
        //pitch.x = Mathf.Lerp(pitch.y, pitch.y * pitchScale, Time.fixedDeltaTime * pitchScale);
        ////rotate the helicopter based on the calculated pitch
        //Helicopter.transform.localRotation = Quaternion.Euler(pitch.y, Helicopter.transform.localEulerAngles.y, -pitch.x);

        pitch.x = Mathf.Lerp(pitch.x, forwardMove.x * yawScale, Time.deltaTime * pitchScale);
        pitch.y = Mathf.Lerp(pitch.y, forwardMove.y * pitchScale, Time.deltaTime * pitchScale);
        Helicopter.transform.localRotation = Quaternion.Euler(pitch.y, Helicopter.transform.localEulerAngles.y, -pitch.x);
    }

    private void OnPitch(float inputValue)
    {
        float addY = 0f;
        if (inputValue > 0 && !isGrounded)
        {
            addY = -Time.deltaTime;
        }
        if (inputValue < 0 && !isGrounded)
        {
            addY = Time.deltaTime;
        }
        forwardMove.y += addY;
        forwardMove.y = Mathf.Clamp(forwardMove.y, -1, 1);
    }
    private void OnTurn(float inputValue)
    {
        float addX = 0f;
        if (!isGrounded)
        {
            if (inputValue > 0)
                addX = Time.deltaTime;
            else if (inputValue < 0)
                addX = -Time.deltaTime;
        }

        forwardMove.x += addX;
        forwardMove.x = Mathf.Lerp(forwardMove.x, 0, Time.fixedDeltaTime * 2f); // Add damping
        forwardMove.x = Mathf.Clamp(forwardMove.x, -1, 1);
    }
    private void OnYaw(float inputValue)
    {
        float torque = 0f;
        if (inputValue > 0 && !isGrounded)
        {
            torque = (turnForcePersent - Mathf.Abs(forwardMove.y)) * Helicopter.mass;
        }
        if (inputValue < 0 && !isGrounded)
        {
            torque = -(turnForcePersent - Mathf.Abs(forwardMove.y)) * Helicopter.mass;
        }
        Helicopter.AddRelativeTorque(0f, torque, 0f);
    }
    private void OnLift(float inputValue)
    {
        System.Console.WriteLine("Lift");
        if (inputValue > 0)
        {
            engineForce += 0.1f;
        }
        else if (inputValue < 0)
        {
            engineForce -= 0.12f;
            if (engineForce < 0)
            {
                engineForce = 0;
            }
        }
    }

    private void OnCollisionEnter()
    {
        isGrounded = true;
    }

    private void OnCollisionExit()
    {
        isGrounded = false;
    }
}


