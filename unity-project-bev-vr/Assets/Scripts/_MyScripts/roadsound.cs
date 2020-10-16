using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Vehicles.Car;

public class roadsound : MonoBehaviour
{
    public Rigidbody rb_Car;

    AudioSource m_audioSource;
    float m_acceleration;
    float m_prevSpeed;

    void Awake() {
        m_audioSource = gameObject.GetComponent<AudioSource>();    
    }
    // Start is called before the first frame update
    void Start()
    {
        m_audioSource.volume = 0;
    }

    // Update is called once per frame
    void Update()
    {
        float speed = rb_Car.velocity.magnitude * 3.6f; // km/h
        float maxSpeed = rb_Car.gameObject.GetComponent<CarController>().MaxSpeed;
        // Interpolate between speed 0 and speed  (maxSpeed)
        // Adjust road sound volume accordingly
        m_audioSource.volume = speed/maxSpeed;

    }
}
