using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarPositionHelper : MonoBehaviour
{
    public GameObject car;

    [Header("Spwanpoints")]
    public Transform startPosition;
    public Transform higwayPosition;
    public Transform debug;

    [Header("Battery script")]
    public Battery battery;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void MoveCarToStartPosition()
    {
        car.transform.SetPositionAndRotation(startPosition.position, startPosition.rotation);
        // Reset the car's previous position too
        battery.prevPosition = car.transform.position;
    }

    public void MoveCarToHighwayPosition()
    {
        car.transform.SetPositionAndRotation(higwayPosition.position, higwayPosition.rotation);
    }

    public void MoveCarToDebugPosition()
    {
        car.transform.SetPositionAndRotation(debug.position, debug.rotation);
    }

}
