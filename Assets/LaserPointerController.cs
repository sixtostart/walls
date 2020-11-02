using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class LaserPointerController : MonoBehaviour {

    public SteamVR_TrackedObject HMD;
    public SteamVR_LaserPointer LeftPointer;
    public SteamVR_TrackedController LeftController;
    public SteamVR_LaserPointer RightPointer;
    public SteamVR_TrackedController RightController;
    public Button ActiveButton;
    public Button PressedButton;
    public GameObject Pointable;
    public float PointerVisibleThreshold = 1.5f;
    public float PointerOnThreshold = 1f;
    public float PointerMaxThickness = 0.002f;
    public float HeadThreshold = 3f;

    // Use this for initialization
    void Start() {
        LeftPointer.PointerIn += PointerIn;
        RightPointer.PointerIn += PointerIn;
        LeftPointer.PointerOut += PointerOut;
        RightPointer.PointerOut += PointerOut;
        LeftController.TriggerClicked += PointerClicked;
        LeftController.TriggerUnclicked += PointerUnclicked;
        RightController.TriggerClicked += PointerClicked;
        RightController.TriggerUnclicked += PointerUnclicked;
    }

    void PseudocolliderAction(Transform target, Action<Button> action) {
        if (target.CompareTag("Pseudocollider"))
        {
            //Debug.Log("Found pseudocollider");
            Button button = target.GetComponentInParent<Button>();
            if (button)
            {
                action(button);
            }
            else
            {
                Debug.Log("Failed to find button parent to pseudocollider");
            }
        }
    }

    void PointerIn(object sender, PointerEventArgs args)
    {
        // Debug.Log("PointerIn " + sender + " " + args.target);
        PseudocolliderAction(args.target, delegate (Button b) {
            ActiveButton = b;
            ExecuteEvent(ExecuteEvents.pointerEnterHandler);
        });
    }

    void PointerOut(object sender, PointerEventArgs args)
    {
        // Debug.Log("PointerOut " + sender + " " + args.target);
        PseudocolliderAction(args.target, delegate (Button b) {
            ExecuteEvent(ExecuteEvents.pointerExitHandler);
            EventSystem.current.SetSelectedGameObject(null);
            ActiveButton = null;
        });
    }

    void PointerClicked(object sender, ClickedEventArgs args)
    {
        Debug.Log("Controller clicked");
        if (ActiveButton == null) return;
        PressedButton = ActiveButton;
        ExecuteEvent(ExecuteEvents.pointerDownHandler);
    }

    void PointerUnclicked(object sender, ClickedEventArgs args)
    {
        Debug.Log("Controller unclicked");
        if (ActiveButton == null)
        {
            PressedButton = null;
            return;
        }

        ExecuteEvent(ExecuteEvents.pointerUpHandler);

        if (ActiveButton == PressedButton)
        {
            ExecuteEvent(ExecuteEvents.pointerClickHandler);
        }

        PressedButton = null;
    }

    void ExecuteEvent<T>(ExecuteEvents.EventFunction<T> functor) where T : IEventSystemHandler
    {
        if (ActiveButton != null)
        {
            var pointer = new PointerEventData(EventSystem.current);
            ExecuteEvents.Execute(ActiveButton.gameObject, pointer, functor);
        }
    }

    // Update is called once per frame
    void Update () {
        UpdatePointerStrength(RightPointer);
        UpdatePointerStrength(LeftPointer);
	}

    void UpdatePointerStrength(SteamVR_LaserPointer pointer)
    {
        // Don't update the pointer strength if there's nothing to point at.
        if (Pointable == null)
        {
            return;
        }

        float gazeDistance = CalculateLookDistance(HMD.transform, Pointable.transform);

        // If the player isn't even looking towards the pedestal, don't show pointers.
        if (gazeDistance > HeadThreshold)
        {
            pointer.thickness = 0f;
            return;
        }

        // Show pointers if they're pointing in the right direction.
        float pointerDistance = CalculateLookDistance(pointer.transform, Pointable.transform);

        if (pointerDistance < PointerOnThreshold)
        {
            pointer.thickness = PointerMaxThickness;
        }
        else if (pointerDistance < PointerVisibleThreshold)
        {
            pointer.thickness = (
                1f - (pointerDistance - PointerOnThreshold) / (PointerVisibleThreshold - PointerOnThreshold))
                * PointerMaxThickness;
        }
        else
        {
            pointer.thickness = 0f;
        }
    }

    float CalculateLookDistance(Transform pointer, Transform pointable)
    {
        Vector3 pointerPosition = gnd(pointer.position);
        Vector3 pointablePosition = gnd(pointable.position);

        // Draw a ray in the direction the pointer is pointing.
        Ray pointerRay = new Ray(pointerPosition, gnd(pointer.forward));

        // Get a point on the pointer ray that is as distant along the ray as the pointable is from the ray.
        Vector3 aim = pointerRay.GetPoint(Vector3.Distance(pointerPosition, pointablePosition));

        // The distance between the projected point and the pointable location indicates how much the pointer
        // is pointing at the pointable.
        return Vector3.Distance(aim, pointablePosition);
    }

    /// <summary>
    /// Emit a vector at ground level.
    /// </summary>
    Vector3 gnd(Vector3 input)
    {
        return new Vector3(input.x, 0, input.z);
    }
}
