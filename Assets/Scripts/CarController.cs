using System.Collections;
using UnityEngine;
using System;
using System.Collections.Generic;

public class CarController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody carRB;
    [SerializeField] private Transform[] rayPoints;
    [SerializeField] private LayerMask drivable;

    [Header("Suspension Settings")]
    [SerializeField] private float springStiffness;
    [SerializeField] private float damperStiffness;
    [SerializeField] private float restLength;
    [SerializeField] private float springTravel;
    [SerializeField] private float wheelRadius;


    public void Start()
    {
        carRB = GetComponent<Rigidbody>();
    }

    public void FixedUpdate()
    {
        Suspension();
    }
    
    private void Suspension()
    {
        foreach (Transform rayPoint in rayPoints)
        {
            RaycastHit hit;
            float maxLength = restLength + springTravel;

            if (Physics.Raycast(rayPoint.position, -rayPoint.up, out hit, maxLength + wheelRadius, drivable))
            {
                // Calculate distance and compression
                float currentSpringLength = hit.distance - wheelRadius;
                float springCompression = (restLength - currentSpringLength) / springTravel;

                // Calculate damp force (velocity-based resistance)
                float springVelocity = Vector3.Dot(carRB.GetPointVelocity(rayPoint.position), rayPoint.up);
                float dampForce = damperStiffness * springVelocity;

                // Calculate spring force (position-based resistance)
                float springForce = springStiffness * springCompression;

                // Calculate net force and apply it
                float netForce = springForce - dampForce;
                carRB.AddForceAtPosition(netForce * rayPoint.up, rayPoint.position);

                // Debug visualization
                Debug.DrawLine(rayPoint.position, hit.point, Color.red);
            }
            else
            {
                // Visualization when the wheel is off the ground
                Debug.DrawLine(rayPoint.position, rayPoint.position + (wheelRadius + maxLength) * -rayPoint.up, Color.green);
            }
        }
    }
}
