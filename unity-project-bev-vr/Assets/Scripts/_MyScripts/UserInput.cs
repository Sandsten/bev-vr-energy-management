using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityStandardAssets.Vehicles.Car;
using UnityEngine.XR;

[RequireComponent(typeof(CarUserControl))]
public class UserInput : MonoBehaviour
{
    public Text statusText;
    [HideInInspector] public int MAX_RANGE_CONTROLLER = 32767; // All axis are int the range [-32767, 32767]
    [HideInInspector] public float steeringWheelRotationNormalized = 0;

    [HideInInspector] public float throttlePositionNormalized; // 0 - no throttle, 1 - full throttle
    [HideInInspector] public float footbreakPositionNormalized;
    public CarUserControl carUserController;

    public GameObject steeringWheel;
    public GameObject vrCamera;
    public GameObject debugCamera;
    public bool useVRCamera;

    public delegate void DPadPressAction(uint button);
    public static event DPadPressAction OnDPadDown;

    private bool m_firstSetup = true;
    private bool m_tripSelectionPressed;

    [Header("Steering wheel ffb settings")]
    [Range(0, 100)]
    public int springStrength = 50;
    [Range(0, 100)]
    public int saturationSpeed = 50;

    bool m_hasPedalsBeenTouched = false;
    bool m_firstCheck = true;
    float m_initialThrottleValue = 0;
    float m_initialFootbreakValue = 0;

    void Awake()
    {
        m_tripSelectionPressed = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        carUserController = GetComponent<CarUserControl>();

        // Initialize the steering wheel + pedals
        LogitechGSDK.LogiSteeringInitialize(false);

        // Only allow one camera to be active at a time
        debugCamera.SetActive(!useVRCamera);
        vrCamera.SetActive(useVRCamera);
        // This has to be disabled if we want to use the regular camera
        XRSettings.enabled = useVRCamera;

        // Move camera to start position
        // vrHead.transform.position = defaultCameraPos.position;
    }

    void OnApplicationQuit()
    {
        // Debug.Log("SteeringShutdown:" + LogitechGSDK.LogiSteeringShutdown());
    }

    // Update is called once per frame
    void Update()
    {
        CheckForVRCameraAdjustments();

        if (LogitechGSDK.LogiUpdate() && LogitechGSDK.LogiIsConnected(0))
        {
            // CalibrateWheelHACK(); // Suddenly the wheel had correct values right from the get go....
            ActivateSpringFFB();

            // Get the current state of the steering wheel
            LogitechGSDK.DIJOYSTATE2ENGINES rec;
            rec = LogitechGSDK.LogiGetStateUnity(0);
            UpdatePedalsAndWheelPositions(rec);

            UpdateSteeringWheelInCar();

            switch (rec.rgdwPOV[0])
            {
                case (0):
                case (18000):
                    if (!m_tripSelectionPressed)
                        if (OnDPadDown != null) OnDPadDown(rec.rgdwPOV[0]);
                    m_tripSelectionPressed = true;
                    break;
                default:
                    m_tripSelectionPressed = false;
                    break;
            }

            // if (statusText == null) return;
            // statusText.text = "";
            // statusText.text += "Steering wheel :" + rec.lX + "\n";
            // statusText.text += "Steering wheel normalized: " + steeringWheelRotationNormalized + "\n";
            // statusText.text += "Throttle :" + throttlePositionNormalized + "\n";
            // statusText.text += "Breaks :" + footbreakPositionNormalized + "\n";

            LogitechGSDK.LogiPlaySpringForce(0, 0, springStrength, saturationSpeed);
        }
    }

    void UpdatePedalsAndWheelPositions(LogitechGSDK.DIJOYSTATE2ENGINES rec)
    {
        // Would've liked to use events for these instead. But CarUserControl is in the standard assets namespace and can't access this class

        steeringWheelRotationNormalized = NormalizeWheelRotation(rec.lX);
        carUserController.steeringWheelRotation = steeringWheelRotationNormalized;

        // Normally rec.lY goes between MAX_RANGE_CONTROLLER and -MAX_RANGE_CONTROLLER.
        // Re-mapping it linearly to 0 and 1 instead
        throttlePositionNormalized = NormalizePedalPosition(rec.lY);
        footbreakPositionNormalized = NormalizePedalPosition(rec.lRz);

        // Pedals initialize at value 0.5, not sure why. Using this to make them start at 0 as it should be
        if (!m_hasPedalsBeenTouched)
        {
            if (m_firstCheck)
            {
                m_initialThrottleValue = throttlePositionNormalized;
                m_initialFootbreakValue = footbreakPositionNormalized;
                m_firstCheck = false;
            }
            if (throttlePositionNormalized != m_initialThrottleValue || footbreakPositionNormalized != m_initialFootbreakValue)
            {
                m_hasPedalsBeenTouched = true;
            }
            else
            {
                throttlePositionNormalized = footbreakPositionNormalized = 0;
            }
        }

        carUserController.throttlePosition = throttlePositionNormalized;
        carUserController.footbrakePosition = footbreakPositionNormalized;
    }

    float NormalizePedalPosition(float position)
    {
        // Change the range to [0,1]
        return 1 - (position + (float)MAX_RANGE_CONTROLLER) / (MAX_RANGE_CONTROLLER * 2f);
    }
    float NormalizeWheelRotation(float position)
    {
        return position / (float)MAX_RANGE_CONTROLLER;
    }

    void UpdateSteeringWheelInCar()
    {
        // Rotate the wheel between [-450, 450] degrees. G920 has 900 degrees of rotation
        // Lerp between these numbers using the steeringWheelRotationNormalized variable.
        // steeringWheel.gameObject.transform.RotateAroundLocal(Vector3.forward,)
        float maxRotationDegrees = 450;
        float rotationAngle = steeringWheelRotationNormalized * maxRotationDegrees;
        steeringWheel.gameObject.transform.localRotation = Quaternion.Euler(0, 0, -rotationAngle);
    }

    void ActivateSpringFFB()
    {
        if (!LogitechGSDK.LogiIsPlaying(0, LogitechGSDK.LOGI_FORCE_SPRING))
        {
            // Make the steering wheel spring back to center position
            LogitechGSDK.LogiPlaySpringForce(0, 0, 100, 10);
            Debug.Log("Steering wheel spring force enabled");
        }
    }

    // Allow the user to adjust their default head position in the car.
    // There's probably a way to calibrate this automatically
    void CheckForVRCameraAdjustments()
    {
        float moveSpeed = 0.5f;
        float moveStep = moveSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.UpArrow))
        {
            if (Input.GetKey(KeyCode.LeftControl))
            {
                vrCamera.transform.Translate(Vector3.up * moveStep, Space.Self);
            }
            else
            {
                vrCamera.transform.Translate(Vector3.forward * moveStep, Space.Self);
            }
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            if (Input.GetKey(KeyCode.LeftControl))
            {
                vrCamera.transform.Translate(Vector3.down * moveStep, Space.Self);
            }
            else
            {
                vrCamera.transform.Translate(Vector3.back * moveStep, Space.Self);
            }
        }
        if (Input.GetKey(KeyCode.LeftArrow)) vrCamera.transform.Translate(Vector3.left * moveStep, Space.Self);
        if (Input.GetKey(KeyCode.RightArrow)) vrCamera.transform.Translate(Vector3.right * moveStep, Space.Self);
    }
}

/*
DIJOYSTATE2
https://docs.microsoft.com/en-us/previous-versions/windows/desktop/ee416628%28v%3Dvs.85%29

lX - Steering wheel rotation
lY - Gas pedal / Throttle
lZ - Z-axis, often the throttle control. If the joystick does not have this axis, the value is 0.
lRx - X-axis rotation. If the joystick does not have this axis, the value is 0.
lRy - Y-axis rotation. If the joystick does not have this axis, the value is 0.
lRz - Breaks
rglSlider - Two additional axis values (formerly called the u-axis and v-axis) whose semantics depend on the joystick. Use the IDirectInputDevice8::GetObjectInfo method to obtain semantic information about these values.
rgdwPOV - Direction controllers, such as point-of-view hats. The position is indicated in hundredths of a degree clockwise from north (away from the user). The center position is normally reported as - 1; but see Remarks. For indicators that have only five positions, the value for a controller is - 1, 0, 9,000, 18,000, or 27,000.
rgbButtons - Array of buttons. The high-order bit of the byte is set if the corresponding button is down, and clear if the button is up or does not exist.

--The inputs down below aren't used by G920--

lVX - X-axis velocity.
lVY - Y-axis velocity.
lVZ - Z-axis velocity.
lVRx - X-axis angular velocity.
lVRy - Y-axis angular velocity.
lVRz - Z-axis angular velocity.
rglVSlider - Extra axis velocities.
lAX - X-axis acceleration.
lAY - Y-axis acceleration.
lAZ - Z-axis acceleration.
lARx - X-axis angular acceleration.
lARy - Y-axis angular acceleration.
lARz - Z-axis angular acceleration.
rglASlider - Extra axis accelerations.
lFX - X-axis force.
lFY - Y-axis force.
lFZ - Z-axis force.
lFRx - X-axis torque.
lFRy - Y-axis torque.
lFRz - Z-axis torque.
rglFSlider - Extra axis forces.
*/
