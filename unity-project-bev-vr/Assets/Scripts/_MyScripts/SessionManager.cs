using UnityEngine.UI;
using UnityEngine;

/*
	This component will keep track of the running session.
	If we are running, stopping, user id, time, which EVIS to use and such.
*/
[RequireComponent(typeof(DataCollector))]
public class SessionManager : MonoBehaviour
{
    public string userID;
    public EVISType eVIS;
    public Battery battery;
    [Tooltip("Decide which state-of-charge to start at")]
    [HideInInspector]public float startingStateOfCharge = 1.3f;

    [Header("Status indicators")]
    public Text statusText;
    public Text timer;
    public Text info;

    [Header("Dashboards")]
    public GameObject regularDashboard;
    public GameObject diffDrivingRangeDashboard;

    [HideInInspector]
    public float elapsedTime; // Time in session
    [HideInInspector]
    public bool isSessionRunning = false;

    private float m_startTime;
    private Text m_statusText;
    private DataCollector m_dataCollector;

    void Awake()
    {
        m_dataCollector = gameObject.GetComponent<DataCollector>();
    }

    void Start()
    {
        Terrain.activeTerrain.basemapDistance = 10000;

        startingStateOfCharge = battery.stateOfCharge;

        if (eVIS == EVISType.DiffAndCOPE1)
        {
            regularDashboard.SetActive(false);
            diffDrivingRangeDashboard.SetActive(true);
        }
        else if (eVIS == EVISType.GuessOMeter || eVIS == EVISType.ControlGroup)
        {
            regularDashboard.SetActive(true);
            diffDrivingRangeDashboard.SetActive(false);
        }

        Battery.OnBatteryEmpty += BatteryEmpty;
    }

    private void OnDestroy() {
        Battery.OnBatteryEmpty -= BatteryEmpty;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isSessionRunning) return;

        elapsedTime = Time.time - m_startTime;

        timer.text = elapsedTime.ToString();

        info.text = "User ID: " + userID + "\n" 
                   + "Distance: " + battery.distanceTraveled.ToString("F0") + "m";
    }

    void BatteryEmpty()
    {
        // Show some kind of text to the user that they've run out of battery but to continue driving
    }

    public void StartSession()
    {
        if (isSessionRunning) return;

        isSessionRunning = true;
        statusText.text = "Session Started and is running";
        m_startTime = Time.time;

        battery.stateOfCharge = startingStateOfCharge; // Start the session at the desired 

        //TODO: Load the propper EVIS that we've selected
        //TODO: Set the battery power level
        //TODO: Maybe set the different car parameters that we need here as well?
    }

    public void EndSession()
    {
        if (!isSessionRunning) return;

        m_dataCollector.StoreData();
        statusText.text = "Data saved";
        isSessionRunning = false;
    }


}
