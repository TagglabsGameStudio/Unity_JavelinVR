using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class RightController : MonoBehaviour
{
    private InputDevice rightController;
    public Vector3 lastPosition;
    public float throwForceMultiplier;
    // Track trigger state and axis value
    private bool wasTriggerPressed = false;
    private float triggerValue = 0f;

    // Rigidbody component of the object to manipulate
    public Rigidbody targetRigidbody;

    // Track if the object is released
    private bool isObjectReleased = false;

    // Start is called before the first frame update
    void Start()
    {
        InitialiseInputDevice(InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.Right, ref rightController);
    }

    // Update is called once per frame
    void Update()
    {
        // Ensure targetRigidbody is set
        if (targetRigidbody == null)
        {
            Debug.LogWarning("No Rigidbody assigned to targetRigidbody. Please assign one in the inspector.");
            return;

        }
        // If the object is released, stop updating its position
        if (isObjectReleased) return;
    }
    private void InitialiseInputDevice(InputDeviceCharacteristics inputDeviceCharacteristics, ref InputDevice inputDevice)
    {
        List<InputDevice> devices = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(inputDeviceCharacteristics, devices);
        if (devices.Count > 0)
        {
            inputDevice = devices[0];
        }

    }

    protected virtual void OnEnable()
    {
        Application.onBeforeRender += OnBeforeRender;
    }

    protected virtual void OnDisable()
    {
        Application.onBeforeRender -= OnBeforeRender;
    }

    [BeforeRenderOrder(-30000)]
    protected virtual void OnBeforeRender()
    {
        UpdatePosition();
    }

    private void UpdatePosition()
    {
        if (!rightController.isValid)
        {
            InitialiseInputDevice(InputDeviceCharacteristics.Controller | InputDeviceCharacteristics.Right, ref rightController);
            return;
        }
        Vector3 targetPosition = Vector3.zero;
        rightController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.devicePosition, out targetPosition);
        

        // If the object is released, do not update its position
        if (!isObjectReleased)
        {
            transform.position = targetPosition;
        }

        // Get the current trigger axis value and button state
        rightController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.triggerButton, out bool isTriggerPressed);
        rightController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.trigger, out triggerValue);


        // Calculate and log the velocity
        Vector3 velocity = transform.position - lastPosition;
        lastPosition = transform.position;
        Debug.Log(velocity.magnitude);

        // Check if trigger was pressed and is now released
        if (wasTriggerPressed && triggerValue == 0f) // Released if the trigger value is 0
        {
            Debug.Log("Trigger button released!");

            // Ensure targetRigidbody is assigned and then disable kinematic mode
            if (targetRigidbody != null)
            {
                // Detach from the controller
                targetRigidbody.transform.SetParent(null);
                isObjectReleased = true;

                targetRigidbody.isKinematic = false;

                // Apply a forward force based on the calculated velocity
                
                targetRigidbody.velocity = transform.forward * velocity.magnitude * throwForceMultiplier;
            }
        }

        // Update the state of wasTriggerPressed based on the current state
        wasTriggerPressed = triggerValue > 0f;

        
    }
}
