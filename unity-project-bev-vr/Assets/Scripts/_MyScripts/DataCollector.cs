using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CsvHelper;
using UnityEngine;
using UnityStandardAssets.Vehicles.Car;

/*
  This class keeps track of all the data we need to store and saves the data we need to a file
*/
[RequireComponent(typeof(SessionManager))]
public class DataCollector : MonoBehaviour
{
  [Header("Save settings")]
  [Tooltip("Enter the path in which you wish to store the data. S:\\My Documents\\<folderName> or just <folderName> to use relative path from the root directory of the Unity project")]
  public string saveLocation = "userStudiesData";
  [Tooltip("How often to store data (datapoints/s)")]
  public float saveFrequency = 10f;
  [Header("Car data")]
  [Tooltip("Script with driver controls, for storing all the data we need")]
  public CarController car;
  public Battery battery;
  public UserInput userInput;

  private List<Variables> m_dataToStore = new List<Variables>();
  private SessionManager m_sessionManager;
  private float m_saveCooldown = 0f;
  private float m_timeBetweenDataPoints;
  private RegularDashboardUpdater m_regularDashboardUpdater;

  void Awake()
  {
    m_sessionManager = gameObject.GetComponent<SessionManager>();
    m_timeBetweenDataPoints = 1 / saveFrequency;
    m_regularDashboardUpdater = m_sessionManager.regularDashboard.GetComponent<RegularDashboardUpdater>();
  }

  // Store all the data we need for each frame
  void Update()
  {
    // Only collect data once the session has started
    if (!m_sessionManager.isSessionRunning) return;

    //? There's probably a cleaner way of doing this with the sessionManager's timer
    m_saveCooldown += Time.deltaTime; // Always add onto the save cooldown
    if (m_saveCooldown <= m_timeBetweenDataPoints) return; // When we go over the assigned time, proceed with saving
    m_saveCooldown = 0f; // Reset the cooldown timer

    var data = new Variables
    {
      userID = m_sessionManager.userID,
      evisID = m_sessionManager.eVIS.ToString(),  // [DiffAndCOPE1, GuessOMeter]
      timeStamp = m_sessionManager.elapsedTime,   // [s]
      
      currentStateOfCharge = battery.stateOfCharge, // State of charge  [kWh]
      energyUsage = battery.power,                  // Power usage in    [kW]
      energyConsumed = battery.totalEnergyUsage,    // Total amount of kWh consumed [kWh]
      guesstimatedDistanceLeft =  m_sessionManager.eVIS.ToString() == "GuessOMeter" ? m_regularDashboardUpdater.rangeEstimate : -1, // The estimated distance for our guess-o-meter [km]
      
      throttlePosition = userInput.throttlePositionNormalized, // [0,1]
      breakPosition = userInput.footbreakPositionNormalized,   // [0,1]
      steeringWheelRot = userInput.steeringWheelRotationNormalized, // [-1,1]
      speed = car.GetComponent<Rigidbody>().velocity.magnitude * 3.6f, // [km/h]
      distanceTraveled = battery.distanceTraveled, // [m]

      xPosition = car.gameObject.transform.position.x,
      yPosition = car.gameObject.transform.position.y,
      zPosition = car.gameObject.transform.position.z,

      odoMeter = m_regularDashboardUpdater.m_odoMeter,
      tripMeter = m_regularDashboardUpdater.m_tripMeter,
    };

    // Save the data for this frame
    m_dataToStore.Add(data);
  }

  public bool StoreData()
  {
    Debug.Log("Saving Data");

    // Create the directory, creates it if the folder doesn't exist.
    Directory.CreateDirectory(saveLocation);

    //Give the file a unique name
    // DrivingData + userID + current date and time which the session ended
    string fileName = "data_" +
    m_sessionManager.userID.ToString() + "_" +
    m_sessionManager.eVIS.ToString() + "_" +
    System.DateTime.Now.ToString("yyyyMMddTHHmmss") +
    ".csv";

    // Write the data to a .csv file
    using (var writer = new StreamWriter(saveLocation + "\\" + fileName))
    using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
    {
      try
      {
        csv.WriteRecords(m_dataToStore);
        Debug.Log("Data Saved!");
        return true;
      }
      catch (DirectoryNotFoundException e)
      {
        Debug.Log("Data not saved. Failed to find the directory.");
        Debug.Log(e.ToString());
      }
    } // Flushing is performed automatically after an "using" statement
    return false;
  }
}
