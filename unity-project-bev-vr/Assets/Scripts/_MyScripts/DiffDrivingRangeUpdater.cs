using System.Collections.Generic;
using UnityStandardAssets.Vehicles.Car;
using UnityEngine.UI;
using UnityEngine;

/*
    Updates the values and positions in the differentiated driving range visualization
*/
public class DiffDrivingRangeUpdater : MonoBehaviour
{
    public Rigidbody car;
    public Text totalDistanceTraveledText;
    public Text targetDistanceText;
    public Text speedTextIndicator;

    [Tooltip("15 or 10")]
    public int maxRangeDisplayed = 10; // km

    [Header("Speed indicator")]
    public RectTransform speedIndicator;
    public RectTransform currentSpeedRangeBar;
    public float needleSpeed = 10;

    [Header("Range bars")]
    public GameObject[] rangeBars;
    public GameObject rangeBarsContainer;

    [Header("Range indicators")]
    public GameObject rangeIndicatorsContainer;
    public RectTransform fifteenKmLine;
    public RectTransform tenKmLine;
    public RectTransform fiveKmLine;
    public RectTransform distanceLeftToTargetLine;

    [Header("Battery")]
    public Battery battery;
    public Text energyLeftText;
    public Image batteryIcon;

    [Header("Session Manager")]
    public SessionManager sessionManager;


    float m_IntermediateLinesThickness = 1f;
    float m_startPos = 0;
    float m_endPos = 0;
    DiffDrivingRangeSpeedStepsSettings m_settings;
    float m_maxSpeed;
    List<float[]> m_powerAtSpeed = new List<float[]>();
    CarController m_carController;
    bool m_haveReceivedInitialRangePred = false;
    float m_heightOfRangeBarArea;
    bool m_hasRangeIndicatorsBeenInstantiated = false;
    int m_numberOfRangeIndicatorBars;
    float m_totalDistanceTraveled;
    Vector3 m_prevPosition;
    int m_targetDistance;
    float m_distanceLeftToTarget;
    bool m_isBatteryEmpty = false;

    private void Awake()
    {
        m_settings = GetComponent<DiffDrivingRangeSpeedStepsSettings>();
        m_carController = car.GetComponent<CarController>();
    }

    void Start()
    {
        m_startPos = m_settings.speedSteps[0].GetComponent<RectTransform>().anchoredPosition.x;
        m_endPos = m_settings.speedSteps[m_settings.speedSteps.Length - 1].GetComponent<RectTransform>().anchoredPosition.x;
        m_maxSpeed = m_carController.MaxSpeed;

        m_heightOfRangeBarArea = tenKmLine.anchoredPosition.y;
        m_numberOfRangeIndicatorBars = maxRangeDisplayed - 1;
        m_totalDistanceTraveled = 0;
        m_targetDistance = 8;
        m_prevPosition = car.transform.position;

        UserInput.OnDPadDown += UpdateTargetDistance;
        Battery.OnBatteryEmpty += BatteryEmpty;

        CreateIntermediateRangeLines();
        UpdateTargetDistance(1);
    }

    void OnDestroy()
    {
        UserInput.OnDPadDown -= UpdateTargetDistance;
        Battery.OnBatteryEmpty -= BatteryEmpty;
    }

    // Update is called once per frame
    void Update()
    {
        if (!m_haveReceivedInitialRangePred && m_carController.OneWheelOnGround())
        {
            GetInitialPowerPredictions();
            m_haveReceivedInitialRangePred = true;
        }

        UpdateSpeedIndicator();
        UpdateDistanceTraveled();
        UpdateDistanceLeftLine();

        if (!m_isBatteryEmpty)
        {
            UpdateRangeBars();
            UpdateBatteryLeftIndicator();
        }

        if (m_isBatteryEmpty)
        {
            // Change the color of the battery icon and text to red
            batteryIcon.color = Color.red;
            energyLeftText.color = Color.red;
        }
    }

    void BatteryEmpty()
    {
        // Update or lock the dashboard in some way when the battery is empty
        m_isBatteryEmpty = true;
    }

    void CreateIntermediateRangeLines()
    {
        for (int i = 1; i <= m_numberOfRangeIndicatorBars; i++)
        {
            // Need the position in Y for each range indicator bar between 0 and 1
            // float normalizedRange = i; // All the intermediate steps
            // normalizedRange /= maxRangeDisplayed;   // Get the range between 0 and 1
            // float yPos = normalizedRange * m_heightOfRangeBarArea;
            float yPos = GetRangeLineYPos(i);

            // Place the larger intermediate lines at their correct position in Y
            if (i == 5)
            {
                fiveKmLine.anchoredPosition = new Vector2(fiveKmLine.anchoredPosition.x, yPos);
                continue;
            }

            if (i == 10 && maxRangeDisplayed == 15)
            {
                tenKmLine.anchoredPosition = new Vector2(fiveKmLine.anchoredPosition.x, yPos);
                continue;
            }

            GameObject rangeIndicator = new GameObject();
            rangeIndicator.AddComponent<Image>();
            Color c = new Color();
            c.r = 0; c.g = 152f / 255f; c.b = 12f / 255f; c.a = 80f / 255f;
            rangeIndicator.GetComponent<Image>().color = c;
            RectTransform r = rangeIndicator.GetComponent<RectTransform>();
            r.sizeDelta = new Vector2(GetComponent<RectTransform>().sizeDelta.x, m_IntermediateLinesThickness);

            // Set anchor position bottom center
            r.anchorMin = new Vector2(0.5f, 0);
            r.anchorMax = new Vector2(0.5f, 0);

            r.anchoredPosition = new Vector2(r.anchoredPosition.x, yPos);

            Instantiate(rangeIndicator, rangeIndicatorsContainer.transform);
        }

        if (maxRangeDisplayed != 15)
        {
            fifteenKmLine.gameObject.SetActive(false);
        }
    }

    float GetRangeLineYPos(float distance)
    {
        float normalizedRange = distance / maxRangeDisplayed;
        float yPos = normalizedRange * m_heightOfRangeBarArea;
        return yPos;
    }

    void GetInitialPowerPredictions()
    {
        // Get the power consumption at each speed
        for (int i = 0; i <= 140; i += 10)
        {
            // Get the power consumed at each speed [kW] and see how far we can get at that level of consumption [km]
            float[] powerAtSpeed = new float[2];
            powerAtSpeed[0] = i; // Speed in km/h

            // Get the power at a specific constant speed
            powerAtSpeed[1] = battery.GetPowerAtSpeed(i / 3.6f); //Convert km/h to m/s
            m_powerAtSpeed.Add(powerAtSpeed);
        }
    }

    /* 1. Check SoC [kWh]
        2. See how many hours of driving we will get at each speed.
            a. Divide SoC by power consumption [kWh/kW] -> [h]
        3. Multiply the speed for which the power was calculated with the number of hours we have and the range is the result! */
    void UpdateRangeBars()
    {
        float stateOfCharge = battery.stateOfCharge; // kWh

        // Update the height of the range bars at fixed speeds between 0 and 140 km/h
        for (int i = 0; i < m_powerAtSpeed.Count; i++)
        {
            float[] powerAtSpeed = m_powerAtSpeed[i];
            float power = powerAtSpeed[1];
            float speed = powerAtSpeed[0];
            RectTransform rangeBar = rangeBars[i].GetComponent<RectTransform>();

            float newHeightValue = CalculateRangeBarHeight(speed, power, stateOfCharge);
            rangeBar.sizeDelta = new Vector2(rangeBar.sizeDelta.x, newHeightValue);
        }

        // Update the current speed range bar
        float currentSpeed = car.velocity.magnitude; // m/s
        float currentSpeedPower = battery.GetPowerAtSpeed(currentSpeed);
        float currentSpeedBarHeight = CalculateRangeBarHeight(currentSpeed * 3.6f, currentSpeedPower, stateOfCharge);
        currentSpeedRangeBar.anchoredPosition = new Vector2(speedIndicator.anchoredPosition.x, currentSpeedRangeBar.anchoredPosition.y);
        currentSpeedRangeBar.sizeDelta = new Vector2(currentSpeedRangeBar.sizeDelta.x, currentSpeedBarHeight);
    }

    // Returns the height representing a certain distance in the visualization
    float CalculateRangeBarHeight(float speed, float power, float stateOfCharge)
    {
        float remainingHoursOfDriving = stateOfCharge / power;  // kWh / kW => h
        float remainingRange = speed * remainingHoursOfDriving; // km/h * h => km

        float normalizedRange = remainingRange / maxRangeDisplayed;
        float heightValue = normalizedRange * m_heightOfRangeBarArea;

        // Debug.Log(speed + "kmh : " + power + "kW");

        return heightValue;
    }

    // Updates the horizontal position of the red speed indicator
    void UpdateSpeedIndicator()
    {
        float speed = car.velocity.magnitude * 3.6f; // km/h

        // Interpolate between m_startPos & m_endPos
        float normalizedSpeed = speed / m_maxSpeed;

        float oldXPos = speedIndicator.anchoredPosition.x;
        float newXPos = (m_startPos * (1 - normalizedSpeed)) + (m_endPos * normalizedSpeed);
        float lerpPos = Mathf.Lerp(oldXPos, newXPos, Time.deltaTime * needleSpeed);

        speedIndicator.anchoredPosition = new Vector2(lerpPos, speedIndicator.anchoredPosition.y);

        speedTextIndicator.text = speed.ToString("F0") + "km/h";
    }

    void UpdateDistanceTraveled()
    {
        m_totalDistanceTraveled += (car.transform.position - m_prevPosition).magnitude;
        m_prevPosition = car.transform.position;
        float km = m_totalDistanceTraveled / 1000f;
        totalDistanceTraveledText.text = "Traveled: " + km.ToString("F1") + "km";
    }

    void UpdateTargetDistance(uint button)
    {
        return; // Skipp this feature

        if (button == 0) m_targetDistance += 1;
        if (button == 18000) m_targetDistance -= 1;

        if (m_targetDistance < 0) m_targetDistance = 0;
        if (m_targetDistance > maxRangeDisplayed) m_targetDistance = maxRangeDisplayed;

        targetDistanceText.text = "Target: " + m_targetDistance.ToString() + "km";
    }

    void UpdateDistanceLeftLine()
    {
        m_distanceLeftToTarget = m_targetDistance - m_totalDistanceTraveled / 1000f;
        float yPos = GetRangeLineYPos(m_distanceLeftToTarget);
        if (yPos < 0) yPos = 0;

        distanceLeftToTargetLine.anchoredPosition = new Vector2(distanceLeftToTargetLine.anchoredPosition.x, yPos);
        // string text = "To target: " + m_distanceLeftToTarget.ToString("F1") + "km";

        string text;
        if (m_distanceLeftToTarget < 0)
            text = "0 km";
        else
            text = m_distanceLeftToTarget.ToString("F1") + "km";
        distanceLeftToTargetLine.gameObject.GetComponentInChildren<Text>().text = text;
    }

    void UpdateBatteryLeftIndicator()
    {
        float percentLeft = (battery.stateOfCharge / battery.batterycapacity) * 100;
        energyLeftText.text = percentLeft.ToString("F1") + "%";
        // Debug.Log("State of charge: " +battery.stateOfCharge);
        // Debug.Log("Capacity: " + battery.batterycapacity);
    }
}
