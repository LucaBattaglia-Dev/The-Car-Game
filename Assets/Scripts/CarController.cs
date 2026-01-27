using System.Collections;
using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;

public class CarController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody carRB;
    [SerializeField] private Transform[] rayPoints;
    [SerializeField] private LayerMask drivable;
    [SerializeField] private Transform accelerationPoint;

    [Header("Suspension Settings")]
    [SerializeField] private float springStiffness;
    [SerializeField] private float damperStiffness;
    [SerializeField] private float restLength;
    [SerializeField] private float springTravel;
    [SerializeField] private float wheelRadius;

    private int[] wheelsIsGrounded = new int[4];
    private bool isGrounded = false;

    [Header("Input")]
    private float moveInput = 0;
    private float steerInput = 0;

    [Header("Car Settings")]
    [SerializeField] private float acceleration = 25f;
    [SerializeField] private float maxSpeed = 100f;
    [SerializeField] private float deceleration = 10f;
    [SerializeField] private float steerStrength = 15f;
    [SerializeField] private AnimationCurve turningCurve;

    private Vector3 currentCarLocalVelocity = Vector3.zero;
    private float carVelocityRatio = 0;

    //==============================================================================
    //Unity UpdateSystems Below
    //==============================================================================

    public void Start()
    {
        carRB = GetComponent<Rigidbody>();
    }

    public void FixedUpdate()
    {
        Suspension();
        GroundCheck();
        CalculateCarVelocity();
        Movement();
    }

    private void Update()
    {
        GetPlayerInput();
    }

    //==============================================================================
    //Unity Functions Below
    //==============================================================================

    private void Movement()
    {
        if (isGrounded)
        {
            Acceleration();
            Decelration(); // Note: Spelling matches image provided
        }
    }

    private void Acceleration()
    {
        // Applies forward force at the designated acceleration point
        carRB.AddForceAtPosition(acceleration * moveInput * transform.forward, accelerationPoint.position, ForceMode.Acceleration);
    }

    private void Decelration()
    {
        // Applies counter-force based on input
        carRB.AddForceAtPosition(deceleration * moveInput * -transform.forward, accelerationPoint.position, ForceMode.Acceleration);
    }

    private void Turn()
    {
        
        carRB.AddTorque(steerStrength * steerInput * turningCurve.Evaluate(carVelocityRatio) * Mathf.Sign(carVelocityRatio) * transform.up, ForceMode.Acceleration);
    }


    private void GetPlayerInput()
    {
        // Captures W/S or Up/Down arrows (-1 to 1)
        moveInput = Input.GetAxis("Vertical");

        // Captures A/D or Left/Right arrows (-1 to 1)
        steerInput = Input.GetAxis("Horizontal");
    }

    private void Suspension()
    {
        for (int i = 0; i < rayPoints.Length; i++)
        {
            RaycastHit hit;
            float maxLength = restLength + springTravel;

            // Cast a ray downwards from each suspension point
            if (Physics.Raycast(rayPoints[i].position, -rayPoints[i].up, out hit, maxLength + wheelRadius, drivable))
            {
                wheelsIsGrounded[i] = 1;

                // Calculate how much the spring is compressed
                float currentSpringLength = hit.distance - wheelRadius;
                float springCompression = (restLength - currentSpringLength) / springTravel;

                // Calculate the velocity of the suspension point to determine damping
                float springVelocity = Vector3.Dot(carRB.GetPointVelocity(rayPoints[i].position), rayPoints[i].up);
                float dampForce = damperStiffness * springVelocity;

                // Hooke's Law: Force = Stiffness * Compression
                float springForce = springStiffness * springCompression;

                // Combine forces (Spring pulls up, Damper resists movement)
                float netForce = springForce - dampForce;

                // Apply the force to the Rigidbody at the specific wheel position
                carRB.AddForceAtPosition(netForce * rayPoints[i].up, rayPoints[i].position);

                // Visual debugging: Red line when grounded
                Debug.DrawLine(rayPoints[i].position, hit.point, Color.red);
            }
            else
            {
                wheelsIsGrounded[i] = 0;

                // Visual debugging: Line showing max reach when in the air
                Debug.DrawLine(rayPoints[i].position, rayPoints[i].position + (wheelRadius + maxLength) * -rayPoints[i].up, Color.white);
            }
        }
    }

    private void GroundCheck()
    {
        int tempGroundedWheels = 0;

        // Loop through each wheel to count how many are currently touching the ground
        for (int i = 0; i < wheelsIsGrounded.Length; i++)
        {
            tempGroundedWheels += wheelsIsGrounded[i];
        }

        // If more than one wheel is grounded, the vehicle is considered grounded
        if (tempGroundedWheels > 1)
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }
    }

    private void CalculateCarVelocity()
    {
        // Converts world-space velocity into the car's local forward/right/up directions
        currentCarLocalVelocity = transform.InverseTransformDirection(carRB.linearVelocity);
        
        // Calculates a 0 to 1 (or higher) ratio of current speed vs max speed
        carVelocityRatio = currentCarLocalVelocity.z / maxSpeed;
    }

}
