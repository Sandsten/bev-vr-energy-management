using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

namespace UnityStandardAssets.Vehicles.Car
{
  [RequireComponent(typeof(CarController))]
  public class CarUserControl : MonoBehaviour
  {
    public enum InputType
    {
      KEYBOARD,
      STEERINGWHEEL
    }
    public InputType inputType;
    [Range(0,3)]
    public float throttleSensitivity = 2f;
    private CarController m_Car; // the car controller we want to use
    public float steeringWheelRotation { get; set; }
    public float throttlePosition { get; set; }
    public float footbrakePosition { get; set; }
    private Rigidbody m_car_rb;

    private void Awake()
    {
      // get the car controller
      m_Car = GetComponent<CarController>();
      m_car_rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
      float h = CrossPlatformInputManager.GetAxis("Horizontal");
      float v = CrossPlatformInputManager.GetAxis("Vertical");

      // pass the input to the car!
      // Updated from SteeringWheel.cs
      float steering = steeringWheelRotation;
      float throttle = throttlePosition;
      float footbrake = footbrakePosition * -1; // Remap to [0, -1]

      // Less sensitive throttle
      throttle = Mathf.Pow(throttle, throttleSensitivity);

      // Less steering at higher speed for stability
      float normalizedSpeed = 1 - (m_car_rb.velocity.magnitude * 3.6f) / 140f;
      normalizedSpeed = Mathf.Clamp(normalizedSpeed, 0.3f, 1);
      steering = steering * normalizedSpeed;

      // float handbrake = CrossPlatformInputManager.GetAxis("Jump");
      if (inputType == InputType.STEERINGWHEEL)
        m_Car.Move(steering, throttle, footbrake, 0f);
      else if (inputType == InputType.KEYBOARD)
        m_Car.Move(h, v, v, 0f);
    }
  }
}
