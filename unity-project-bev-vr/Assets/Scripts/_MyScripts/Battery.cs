using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.Vehicles.Car;

public class Battery : MonoBehaviour
{
    public float batterycapacity = 20f; // kWh
                                        //float speed = 0f;
                                        //float drag = 0.4f;
                                        //float rollingresistance = 0.4f;

    public float stateOfCharge = 5f;
    public bool hasBattery = true;
    Vector3 prevPosition;
    float lastVelocity = 0;
    float lastAltitude = 0;
    public float distanceTraveled = 0f;
    public float power;
    public float totalEnergyUsage = 0;

    public Text m_BatteryLeft;
    public Text m_Velocity;
    public Text m_Power;
    public Text allForces;
    public Text m_groundsurface;
    public Text m_outdoortemp;
    public Text m_distancetraveled;
    public Text m_kwhkm;

    public RectTransform powerBarPos;
    public RectTransform powerBarNeg;

    public float outdoortemp = 22f;

    [HideInInspector]
    public float powerConsumptionAverage;

    float onamp = 1f;
    float carvoltage = 360f;

    [SerializeField]
    Transform car;

    float interval = 0.1f;
    float nextTime = 0;

    float fa, fr, fs, fd, faux = 0f;

    // evenergy
    //var evenergy,
    //float speed = undefined, 
    //time = undefined, 
    //float distance = undefined,
    //soc = undefined,
    //float acceleration = 0; // acceleration
    float slope = 0; // slope
                     //mass = 1521, // vehicle mass
    float cr = 0.012f; // vehicle roll resistance coefficient
    float cd = 0.29f; // vehicle drag coefficient
    float area = 2.7435f; // vehicle frontal area
    float r = 1.225f; // air density
    float g = 9.82f; // gravity
    [Range(0, 1)]
    public float efficiencyEnergyOut = 0.87f; // efficiency
    [Range(0, 1)]
    public float efficiencyEnergyIn = 0.87f; // efficiency

    Rigidbody car_rb;
    CarController m_carController;
    private float m_averagePower;
    private float m_prevPower = 0;

    public delegate void BatteryEmptyAction();
    public static event BatteryEmptyAction OnBatteryEmpty;

    [Header("Low-pass filter")]
    public float m_filterResponsiveness = 1;

    private float m_prevAcceleration = 0;

    // Use this for initialization
    void Awake()
    {
        prevPosition = car.transform.position;
        car_rb = car.GetComponent<Rigidbody>();
        m_carController = car.GetComponent<CarController>();
        lastAltitude = car_rb.position.y;
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time >= nextTime)
        {
            //do something here every interval seconds
            Burn();

            nextTime += interval;

            allForces.text = "Acceleration resistance: " + (int)fa + "kg m/s^2" + "\n" +
                             "Rolling resistance: " + (int)fr + "kg m/s^2" + "\n" +
                             "Drag resistance: " + (int)fd + "kg m/s^2" + "\n" +
                             "Slope resistance: " + (int)fs + "kg m/s^2" + "\n" +
                             "Aux power: " + (int)(faux * 1000) + "Watts" + "\n";

            m_outdoortemp.text = "Outdoor temp: " + outdoortemp.ToString() + "C";
        }
    }

    void Burn()
    {
        if (hasBattery)
        {
            float enrg = energy(); // kWh. When negative it means we are regenerating
            stateOfCharge -= enrg;
            totalEnergyUsage += enrg;
        }

        // Don't overfill the battery!
        if (stateOfCharge > batterycapacity)
            stateOfCharge = batterycapacity;

        // Allow the user to continue driving even if the battery is empty
        // Let the state of charge get into the negative range since it's interesting to see how far off the user was
        if (stateOfCharge <= 0)
        {
            // Notify all listeners that the battery is empty
            if (OnBatteryEmpty != null) OnBatteryEmpty();
            hasBattery = false;
        }

        // Add to total distance traveled
        float dist = Vector3.Distance(prevPosition, car.transform.position);
        distanceTraveled += dist;

        //print("Total distance: " + distanceTraveled);
        //print("kWh/km: " + (20-batterycapacity)/(distanceTraveled/1000));

        float vel = car_rb.velocity.magnitude * 3.6f;
        if (m_BatteryLeft != null) m_BatteryLeft.text = stateOfCharge.ToString("F3") + " kWh";
        if (m_Velocity != null) m_Velocity.text = (string)((int)vel).ToString() + " km/h";
        if (m_distancetraveled != null) m_distancetraveled.text = (distanceTraveled / 1000f).ToString("F2") + "km";

        // Battery used divided by distance traveled in km
        if (m_kwhkm != null) m_kwhkm.text = ((batterycapacity - stateOfCharge) / (distanceTraveled / 1000)).ToString("F3") + "kWh/km";

        prevPosition = car.transform.position;
    }

    float totalForce(float speed, bool ignoreAccAndSlope)
    {
        // nothing is pulling power from battery since the vehicle is in the air
        if (!m_carController.OneWheelOnGround())
        {
            fa = fr = fs = fd = 0f;
            return 0f;
        }

        float mass = car_rb.mass;

        // In the initial prediction at each speed, we do not account for acceleration or slope. It's constant speed on a flat surface
        float acceleration = 0;
        if (!ignoreAccAndSlope)
        {
            acceleration = (speed - lastVelocity) / interval;
            acceleration = Mathf.Lerp(m_prevAcceleration, acceleration, Time.deltaTime * 10);

            // acceleration = Mathf.Lerp(acceleration, (speed - lastVelocity) / interval, interval * 100f);
            lastVelocity = speed;

            m_prevAcceleration = acceleration;

            //* Acceleration resistance
            // fa = Mathf.Lerp(fa, acceleration * mass, Time.deltaTime * 10);
            fa = acceleration * mass; // kg m/s^2
            if (fa < 0)
                fa = fa * efficiencyEnergyIn * efficiencyEnergyIn; // If negative acceleration, the acceleration resistance is less

            //* Slope resistance
            slope = car_rb.position.y - lastAltitude; // Slope is the difference in height
            fs = slope * g * mass; // kg m/s^2

            lastAltitude = car_rb.position.y;
        }
        else
        {
            fa = fs = 0f;
        }

        // add 
        //0.0062 to 0.015[26]		Car tire measurements
        //0.010 to 0.015[27]		Ordinary car tires on concrete
        //0.0385 to 0.073[28]		Stage coach (19th century) on dirt road. Soft snow on road for worst case.
        //0.3[27]		Ordinary car tires on sand
        //* Roll resistance
        if (speed > 0.001f)
            fr = cr * g * mass; // kg m/s^2
        else
            fr = 0f;

        //* Wind resistance a.k.a. drag
        // 1/2 * air density * vehicle drag coefficient * frontal area * speed^2
        fd = 0.5f * r * cd * area * speed * speed; // kg m/s^2

        float total = fa + fs + fr + fd;

        return total;  // kg m/s^2
    }

    // Returns energy used in kWh
    float energy()
    {
        power = Power(); // Need access to this from DataCollector
        float powerPerS = power * interval; // P * dt
        float powerPerHour = powerPerS / 3600; // 3600s in one h
        return powerPerHour;
    }

    // Returns the power [kW] the car is using
    public float Power()
    {
        float speed = this.GetComponent<Rigidbody>().velocity.magnitude;
        float powerTot = powerAvgAux();

        // P = F * V [W] https://en.wikipedia.org/wiki/Power_(physics)
        // P/1000 [kW]

        // If we have negative force (P), it represent the force which is required to slow down to this speed
        float force = totalForce(speed, false);

        if (force > 0)
        {
            // Efficiency < 1 means more energy is required. So we divide
            // powerTot += (force * speed / 1000f); // kW
            powerTot += (force * speed / 1000f / efficiencyEnergyOut); // kW
        }
        else
        {
            // Efficiency < 1 means less energy is regenerated. So we multiply
            powerTot += (force * speed / 1000f * efficiencyEnergyIn); // kW
            // powerTot += (force * speed / 1000f); // kW
        }

        // Electrical circuits aren't instantaneous. Filtering by imitating a low-pass filter using lerp
        powerTot = Mathf.Lerp(m_prevPower, powerTot, m_filterResponsiveness * Time.deltaTime);
        m_prevPower = powerTot;

        // RectTransform objectRectTransformPos = powerBarPos.GetComponent<RectTransform>();
        // RectTransform objectRectTransformNeg = powerBarNeg.GetComponent<RectTransform>();


        if (m_Power != null) m_Power.text = (string)powerTot.ToString("F1") + " kW";

        return powerTot;
    }

    // This is used by the differentiated drivingrange visualization
    public float GetPowerAtSpeed(float speed)
    {
        float power = powerAvgAux();
        // P = F * V [W] https://en.wikipedia.org/wiki/Power_(physics)
        // P/1000 [kW]
        float force = totalForce(speed, true);
        // If the efficiency is low, we have to use more power.
        power += (force * speed / 1000f / efficiencyEnergyOut); // kW

        return power;
    }

    // estimates on average from test data and do not consider drivers own choice of air conditioning
    // The car's base energy usage
    float powerAvgAux()
    {
        // cooling 0.5-3kW
        // heating up to 6 kW

        //-10 grader car 2.7kW average, 0.9-4.7
        //21 grader 0.9kW Avg
        //35 grader 1.2 kW avg

        // Approximation of Nissan Leaf average auxillary consumption from testdata found online
        // https://www.fleetcarma.com/electric-vehicle-heating-chevrolet-volt-nissan-leaf/
        if (outdoortemp <= 21f)
        {
            faux = (-58.06f * outdoortemp + 2119.4f) / 1000f; //kW
        }
        else
        {
            faux = (21.4f * outdoortemp + 450f) / 1000f; //kW
        }
        return faux; //W
    }

    public bool HasBattery()
    {
        return hasBattery;
    }

    // float powerSystemOn()
    // {
    //   // Test case, not accurate, 
    //   return ((onamp * carvoltage) / 1000) / efficiency; //kW
    // }
}
