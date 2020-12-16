using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class RegularDashboardUpdater : MonoBehaviour
{
    [Header("Universal visuals")]
    public Text speedText;
    public Text rangeEstimateText;
    public Text powerUsageText;
    public Text energyLeftText;
    public Image batteryIcon;

    [Header("Guess-o-meter + distance left")]
    public Text targetDistanceText;
    public Text distanceTraveledText;
    public Text distanceLeftText;
    
    [Header("Guess-o-meter as control group")]
    public Text odoMeterText;
    public Text tripMeterText;

    [Header("EVIS type specifics")]
    public GameObject includedInControlGroup;
    public GameObject excludedInControlGroup;

    [Header("Other car components")]
    public Battery battery;
    public Rigidbody car;

    [Header("Session Manager")]
    public SessionManager sessionManager;

    // [Header("Power bar")]
    // public RectTransform powerBarPos;
    // public RectTransform powerBarNeg;
    
    [HideInInspector]
    public float rangeEstimate;

    private float m_powerAverage;
    private float m_prevSpeed = 0;

    // Drove the slow part of our track two times and got the following results from our battery model
    // If these are larger we will get a more stable range estimate number
    private float m_previousDistanceTraveled = 13f; // ~13 km driven at 40km/h
    private float m_previousEnergyConsumed = 1.5f;  // ~1.5 kWh consumed in total over these 13km

    private float m_totalDistance;
    private float m_totalAmountOfEnergyConsumed;

    private float m_averageEnergyConsumptionPerKM; // kWh/km

    // Variables about the traveled distance
    float m_totalDistanceTraveled = 0f;
    Vector3 m_prevPosition;
    float m_targetDistance = 8f;
    float m_distanceLeftToTarget;

    bool m_isBatteryEmpty = false;
    float m_prevPower = 0;

    bool m_isThisControlGroup = false;

    [HideInInspector]
    public float m_odoMeter;
    [HideInInspector]
    public float m_tripMeter;

    void Awake()
    {
        // m_totalDistance = m_previousDistanceTraveled;
        // m_totalAmountOfEnergyConsumed = m_previousEnergyConsumed;
        m_averageEnergyConsumptionPerKM = m_totalAmountOfEnergyConsumed / m_totalDistance; // [kWh/km]'

        m_isThisControlGroup = sessionManager.eVIS == EVISType.ControlGroup;
        m_odoMeter = Random.Range(1000f, 2000f); // Random total distance the car has traveled in km
        m_tripMeter = Random.Range(53f, 252f); // Random trip meter (has to be lower than odo) 
    }

    // Start is called before the first frame update
    void Start()
    {
        UserInput.OnDPadDown += UpdateTargetDistance;
        Battery.OnBatteryEmpty += BatteryEmpty;

        m_prevPosition = car.transform.position;
        // Set an average energy consumption per km here!
        // What should it be? 

        // nissan leaf acenta 40kWh battery

        // SoC 1.37 kWh

        // Assume previous drive was local urban roads with an average speed of 40 km/h
        // Battery capacity 40kWh (updated whilst driving)
        // Average energy consumption per km 10kW/km (updated whilst driving with a new average)

        // kW at 40km/h is 3.68kW

        // kWh / kW -> hours of driving

        // hours of driving * average velocity = range

        // Distance 
        // 160km according to our simplified model

        // Starting distance
        // 4.998 km

        // kWh - how many kilowatt hours have been consumed
        UpdateTargetDistance(1);
        
        // If this is the control group, enable trip and odo meter. Hide left & traveled
        includedInControlGroup.SetActive(m_isThisControlGroup);
        excludedInControlGroup.SetActive(!m_isThisControlGroup);
    }

    private void OnDestroy()
    {
        UserInput.OnDPadDown -= UpdateTargetDistance;
        Battery.OnBatteryEmpty -= BatteryEmpty;
    }

    void BatteryEmpty()
    {
        m_isBatteryEmpty = true;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateSpeedIndicator();
        UpdatePowerUsage();
        
        if(m_isThisControlGroup) UpdateODOAndTripMeters();
        else UpdateDistanceTraveled();

        if (!m_isBatteryEmpty)
        {
            UpdateBatteryLeft();
            UpdateRangeEstimation();
        }

        if(m_isBatteryEmpty) 
        {
            // Change the color of the battery icon and text to red
            batteryIcon.color = Color.red;
            energyLeftText.color = Color.red;

        }

        // If I press the t button, reset the trip meter (T for trip)
        if(Input.GetKeyDown(KeyCode.T))
        {
            ResetTripMeter();
        }

        m_prevPosition = car.transform.position;
    }

    // CONTROL GROUP VISUALS //
    /*
        Nissan Leaf's trip meter can store distance of two trips and has can be resetted by holding down the trip selection button.
        Here we only have a single trip meter and below 
        https://www.youtube.com/watch?v=DEaLauuOw9o
    */

    void UpdateODOAndTripMeters() 
    {   
        // Delta distance is in meters
        float delta_distance = (car.transform.position - m_prevPosition).magnitude;
        // m to km before adding
        m_tripMeter += (delta_distance / 1000f);
        m_odoMeter += (delta_distance / 1000f);

        // Update the text components
        tripMeterText.text = m_tripMeter.ToString("F1"); // Trip meter displayed with one decimal, i.e 100m precision. 
        odoMeterText.text = m_odoMeter.ToString("F0"); // Odo meter displayed in whole km, always rounded down to the nearest km
    }

    void ResetTripMeter() 
    {
        Debug.Log("Trip meter reset!");
        m_tripMeter = 0;
    }

    void UpdatePowerUsage()
    {   
        float power = battery.power;

        // Update power usage in kW
        powerUsageText.text = power.ToString("F1");

        // if (power >= 0)
        // {
        //     powerBarPos.sizeDelta = new Vector2(Mathf.Lerp(m_prevPower, power, Time.deltaTime), 7.5f);
        //     powerBarNeg.sizeDelta = new Vector2(0f, 7.5f);
        // }
        // else
        // {
        //     powerBarPos.sizeDelta = new Vector2(0f, 7.5f);
        //     powerBarNeg.sizeDelta = new Vector2(Mathf.Lerp(Mathf.Abs(m_prevPower), Mathf.Abs(power), Time.deltaTime), 7.5f);
        // }

        m_prevPower = power;
    }

    void UpdateSpeedIndicator()
    {
        // Update speed in km/h
        float speed = car.velocity.magnitude * 3.6f;
        speedText.text = Mathf.Lerp(m_prevSpeed, speed, 5 * Time.deltaTime).ToString("F0");
        m_prevSpeed = speed;
    }

    void UpdateBatteryLeft()
    {
        // Update energy left in %
        energyLeftText.text = ((battery.stateOfCharge / battery.batterycapacity) * 100f).ToString("F1") + "%";
    }

    void UpdateRangeEstimation()
    {
        // SoC [kWh]
        float soc = battery.stateOfCharge;

        // total distance traveled for range estimator
        m_totalDistance = m_previousDistanceTraveled + (battery.distanceTraveled / 1000f);

        // Get the total amount of power used
        m_totalAmountOfEnergyConsumed = m_previousEnergyConsumed + battery.totalEnergyUsage;

        // Get the average energy consumed per km
        m_averageEnergyConsumptionPerKM = m_totalAmountOfEnergyConsumed / m_totalDistance;

        // Debug.Log("Dist: " + m_totalDistance + " Energy: " + m_totalAmountOfEnergyConsumed);

        // Add starting kWh by battery.energy [kWh] and then divide it by our new total distance to get [kWh/km]
        // Then divide our SoC [kWh] by energy consumption per km [kWh/km] -> [km]
        rangeEstimate = soc / m_averageEnergyConsumptionPerKM;

        rangeEstimateText.text = rangeEstimate.ToString("f0");
    }

    void UpdateDistanceTraveled()
    {
        m_totalDistanceTraveled += (car.transform.position - m_prevPosition).magnitude;

        float km = m_totalDistanceTraveled / 1000f;
        distanceTraveledText.text = "Traveled: " + km.ToString("F1");

        // Distance Left
        float distanceLeft = m_targetDistance - m_totalDistanceTraveled / 1000f;
        distanceLeftText.text = "Left: " + distanceLeft.ToString("F1");
    }

    void UpdateTargetDistance(uint button)
    {
        return; // Skipp this feature

        if (button == 0) m_targetDistance += 1f;
        if (button == 18000) m_targetDistance -= 1f;

        if (m_targetDistance < 0) m_targetDistance = 0f;

        targetDistanceText.text = "Target: " + m_targetDistance.ToString("F1");
    }
}
