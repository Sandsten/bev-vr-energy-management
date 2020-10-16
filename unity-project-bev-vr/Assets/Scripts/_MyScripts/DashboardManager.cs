using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

/*
    TODO: This should be used to control which visualization to use!
*/
public class DashboardManager : MonoBehaviour
{
    public Text speedIndicator;

    [Header("Car data")]
    public GameObject car;
    public Battery battery;
    public UserInput userInput;

    [Header("Differentiated Driving range")]
    public GameObject parentCanvas;
    public int maxSpeed = 130;
    public int spacing = 10;

    // Start is called before the first frame update
    void Start()
    {
        // InitializeVisualization();
    }

    // Update is called once per frame
    void Update()
    {
        // float vel = car.gameObject.GetComponent<Rigidbody>().velocity.magnitude * 3.6f;
        // speedIndicator.text = Mathf.FloorToInt(vel).ToString();

        // UpdateDifferentiatedDrivingRange();
    }

    void InitializeVisualization() 
    {
        for(int i = 0; i < maxSpeed/10; i++){
            int speed = i * 10;
            GameObject textObj = new GameObject(speed.ToString());

            textObj.transform.SetParent(parentCanvas.transform);
            Text text = textObj.AddComponent<Text>();
            text.text = speed.ToString();

            float dashboardWidth = parentCanvas.GetComponent<RectTransform>().rect.width;

            textObj.transform.localPosition = new Vector3(-(dashboardWidth/2) + i*spacing, 0, 0);
        }
    }

    void UpdateDifferentiatedDrivingRange()
    {
        
    }

}
