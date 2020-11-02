using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeasurementController : MonoBehaviour
{
    const float DUMMY_SHOULDER_HEIGHT = 1.41f;
    const float DUMMY_UPPER_ARM_LENGTH = 0.24f;
    const float DUMMY_FOREARM_LENGTH = 0.24f;

    [System.Serializable]
    public enum MeasureState
    {
        OFF,
        READY,
        MEASURED_SHOULDER,
        MEASURED_ELBOW,
        MEASURED_WRIST
    }

    public SteamVR_TrackedController RightController;
    public Transform MeasureReference;
    public Vector3 MeasuredShoulder;
    public Vector3 MeasuredElbow;
    public Vector3 MeasuredWrist;

    public Transform DummyRoot;
    public Transform DummyLeftShoulder;
    public Transform DummyRightShoulder;
    public Transform DummyLeftElbow;
    public Transform DummyRightElbow;
    public Transform DummyLeftWrist;
    public Transform DummyRightWrist;

    public Transform TargetDummyRoot;
    public Transform TargetDummyLeftShoulder;
    public Transform TargetDummyRightShoulder;
    public Transform TargetDummyLeftElbow;
    public Transform TargetDummyRightElbow;
    public Transform TargetDummyLeftWrist;
    public Transform TargetDummyRightWrist;

    public GameObject DebugCursor;
    public GameObject DebugWallLayer;
    public GameObject CircleController;
    public TextMesh InstructionText;
    public TextMesh ShoulderText;
    public TextMesh UpperArmText;
    public TextMesh ForearmText;

    public MeasureState State;

    // Use this for initialization
    void Start()
    {
        RightController.TriggerClicked += TriggerClicked;
        UpdateInstruction();

        // Start with configured scale.
        Scale(
            HoleConfiguration.INSTANCE.HeightScale,
            HoleConfiguration.INSTANCE.UpperArmScale,
            HoleConfiguration.INSTANCE.ForearmScale);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void BeginCalibration()
    {
        State = MeasureState.READY;
        DummyRoot.localScale = Vector3.one;
        DummyLeftShoulder.localScale = Vector3.one;
        DummyRightShoulder.localScale = Vector3.one;
        DummyLeftElbow.localScale = Vector3.one;
        DummyRightElbow.localScale = Vector3.one;

        MeasuredShoulder = Vector3.zero;
        MeasuredElbow = Vector3.zero;
        MeasuredWrist = Vector3.zero;

        DebugWallLayer.SetActive(true);
        CircleController.SetActive(true);

        UpdateText();
    }

    void TriggerClicked(object sender, ClickedEventArgs args)
    {
        DebugCursor.transform.position = MeasureReference.position;

        switch (State)
        {
            case MeasureState.OFF:
                // Do nothing.
                return;
            case MeasureState.READY:
                MeasuredShoulder = MeasureReference.position;
                State = MeasureState.MEASURED_SHOULDER;
                UpdateText();
                break;
            case MeasureState.MEASURED_SHOULDER:
                MeasuredElbow = MeasureReference.position;
                State = MeasureState.MEASURED_ELBOW;
                UpdateText();
                break;
            case MeasureState.MEASURED_ELBOW:
                MeasuredWrist = MeasureReference.position;
                State = MeasureState.MEASURED_WRIST;
                UpdateText();
                Calibrate();
                DebugWallLayer.SetActive(false);
                CircleController.SetActive(false);
                break;
            case MeasureState.MEASURED_WRIST:
                // For testing, if the button is clicked after calibrating, reset
                // so that calibration can be repeated.
                //BeginCalibration();
                break;
        }
    }

    void UpdateText()
    {
        if (MeasuredShoulder == Vector3.zero)
        {
            ShoulderText.text = "?";
        }
        else
        {
            float height = MeasuredShoulder.y;
            ShoulderText.text = string.Format("{0:f2}m ({1:d}%)",
                height, Mathf.RoundToInt(height * 100 / DUMMY_SHOULDER_HEIGHT));
        }

        if (MeasuredElbow == Vector3.zero)
        {
            UpperArmText.text = "?";
        }
        else
        {
            float upperArmLength = Vector3.Distance(MeasuredShoulder, MeasuredElbow);
            float dummyUpperArmLength = Vector3.Distance(DummyLeftShoulder.position, DummyLeftElbow.position);
            float upperArmRatio = upperArmLength / dummyUpperArmLength;
            UpperArmText.text = string.Format("{0:f2}m ({1:d}%)",
                upperArmLength, Mathf.RoundToInt(upperArmRatio * 100));
        }

        if (MeasuredWrist == Vector3.zero)
        {
            ForearmText.text = "?";
        }
        else
        {
            float forearmLength = Vector3.Distance(MeasuredElbow, MeasuredWrist);
            float dummyForearmLength = Vector3.Distance(DummyLeftElbow.position, DummyLeftWrist.position);
            float forearmRatio = forearmLength / dummyForearmLength;
            ForearmText.text = string.Format("{0:f2}m ({1:d}%)",
                forearmLength, Mathf.RoundToInt(forearmRatio * 100));
        }

        UpdateInstruction();
    }

    void UpdateInstruction()
    {
        string instruction;
        switch (State)
        {
            case MeasureState.OFF:
                instruction = "Press \"Calibrate\" button\nto begin calibration";
                break;
            case MeasureState.READY:
                instruction = "Locate shoulder\nand pull trigger";
                break;
            case MeasureState.MEASURED_SHOULDER:
                instruction = "Locate elbow\nand pull trigger";
                break;
            case MeasureState.MEASURED_ELBOW:
                instruction = "Locate wrist\nand pull trigger";
                break;
            case MeasureState.MEASURED_WRIST:
                instruction = "Calibration complete";
                break;
            default:
                instruction = "Unknown calibration state";
                break;
        }
        InstructionText.text = instruction;
    }

    void Calibrate()
    {
        float shoulderHeightAboveGround = MeasuredShoulder.y;
        float upperArmLength = Vector3.Distance(MeasuredShoulder, MeasuredElbow);
        float forearmLength = Vector3.Distance(MeasuredElbow, MeasuredWrist);

        // The Dummy's shoulder height is hard-coded because we want the shoulder
        // height when the dummy is standing with normal posture. But at runtime the dummy
        // may be slightly crouched or standing on its tiptoes in order to try to match the 
        // player's pose.
        float dummyShoulderHeightAboveGround = DUMMY_SHOULDER_HEIGHT;
        float dummyUpperArmLength = Vector3.Distance(DummyLeftShoulder.position, DummyLeftElbow.position);
        float dummyForearmLength = Vector3.Distance(DummyLeftElbow.position, DummyLeftWrist.position);

        float heightRatio = shoulderHeightAboveGround / dummyShoulderHeightAboveGround;
        float upperArmRatio = upperArmLength / dummyUpperArmLength;
        float forearmRatio = forearmLength / dummyForearmLength;

        Debug.Log(string.Format("{0} player {1:f2} dummy {2:f2} ratio {3:f2}",
            "Shoulder-height ", shoulderHeightAboveGround, dummyShoulderHeightAboveGround, heightRatio));
        Debug.Log(string.Format("{0} player {1:f2} dummy {2:f2} ratio {3:f2}",
            "Upper arm length", upperArmLength, dummyUpperArmLength, upperArmRatio));
        Debug.Log(string.Format("{0} player {1:f2} dummy {2:f2} ratio {3:f2}",
            "Forearm length  ", forearmLength, dummyForearmLength, forearmRatio));

        Scale(heightRatio, upperArmRatio, forearmRatio);
    }

    void Scale(float heightRatio, float upperArmRatio, float forearmRatio)
    {

        ShoulderText.text = string.Format("{0:f2}m ({1:d}%)",
                DUMMY_SHOULDER_HEIGHT * heightRatio, Mathf.RoundToInt(heightRatio * 100));
        UpperArmText.text = string.Format("{0:f2}m ({1:d}%)",
                DUMMY_UPPER_ARM_LENGTH * upperArmRatio, Mathf.RoundToInt(upperArmRatio * 100));
        ForearmText.text = string.Format("{0:f2}m ({1:d}%)",
                DUMMY_FOREARM_LENGTH * forearmRatio, Mathf.RoundToInt(forearmRatio * 100));

        // Scaling root notes requires descaling leaf nodes.
        upperArmRatio /= heightRatio;
        forearmRatio /= heightRatio;
        forearmRatio /= upperArmRatio;

        Scale(DummyRoot, heightRatio);
        Scale(DummyLeftShoulder, upperArmRatio);
        Scale(DummyRightShoulder, upperArmRatio);
        Scale(DummyLeftElbow, forearmRatio);
        Scale(DummyRightElbow, forearmRatio);

        Scale(TargetDummyRoot, heightRatio);
        Scale(TargetDummyLeftShoulder, upperArmRatio);
        Scale(TargetDummyRightShoulder, upperArmRatio);
        Scale(TargetDummyLeftElbow, forearmRatio);
        Scale(TargetDummyRightElbow, forearmRatio);
    }

    void Scale(Transform transform, float ratio)
    {
        transform.localScale = new Vector3(ratio, ratio, ratio);
    }
}