using UnityEngine.UI;
using UnityEngine;

/// <summary>
/// This component will keep track of the running session.
/// If we are running, stopping, user id, time, which EVIS to use and such.
/// </summary>
[RequireComponent(typeof(DataCollector))]
public class SessionManager : MonoBehaviour
{
    public string userID;
    public EVISType eVIS;
    public Battery battery;

    [Header("Status indicators")]
    public Text statusText;
    public Text timer;
    public Text info;

    [Header("Dashboards")]
    public GameObject regularDashboard;
    public GameObject diffDrivingRangeDashboard;

    [HideInInspector]
    public float elapsedTime;
    [HideInInspector]
    public bool isSessionRunning = false;

    private float m_startTime;
    private Text m_statusText;
    private DataCollector m_dataCollector;
    private bool m_batteryEmpty = false;
    private float m_startingStateOfCharge = 1.2f;
    private CarPositionHelper m_carPositionHelper;

    void Awake()
    {
        m_dataCollector = gameObject.GetComponent<DataCollector>();
        m_carPositionHelper = gameObject.GetComponent<CarPositionHelper>();
    }

    void Start()
    {
        Terrain.activeTerrain.basemapDistance = 10000;

        m_startingStateOfCharge = battery.stateOfCharge;

        // Show the selected dashboard
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
        // Show info which could be useful for the user study moderator
        info.text = "User ID: " + userID + "\n" 
                   + "Distance: " + battery.distanceTraveled.ToString("F0") + "m" + "\n"
                   + "Is battery empty:" + m_batteryEmpty.ToString() + "\n"
                   + "Battery SoC: " + battery.stateOfCharge.ToString("F3");
        
        if (!isSessionRunning) return;

        elapsedTime = Time.time - m_startTime;
        timer.text = elapsedTime.ToString("F1");
    }

    /// <summary>
    /// Triggered when Soc <= 0 in our Battery.cs script
    /// </summary>
    void BatteryEmpty()
    {
        m_batteryEmpty = true;
        EndSession();
    }

    /// <summary>
    /// Starts the session by setting the appropriate variables and resetting the cars state.
    /// </summary>
    public void StartSession()
    {
        // Only alow one running session
        if (isSessionRunning) return;

        isSessionRunning = true;
        statusText.text = "Session Started and is running";
        m_startTime = Time.time;
        
        // Reset the cars position and distance traveled to ensure everyone has the exact same starting condition
        m_carPositionHelper.MoveCarToStartPosition();
        battery.stateOfCharge = m_startingStateOfCharge;
        battery.distanceTraveled = 0f;
    }

    public void EndSession()
    {
        // Only allow ending a session if there's a session already running
        if (!isSessionRunning) return;

        m_dataCollector.StoreData();
        statusText.text = "Data saved";
        isSessionRunning = false;
    }
}
