﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Universe
{
    public class CelestialBody : MonoBehaviour
    {
        [SerializeField] public float surfaceGravity;
        [SerializeField] public float radius;
        [SerializeField] public Vector3 initialVelocity;
        [SerializeField] public Color trailColor;

        private Vector3 velocity;

        public float Mass
        {
            get
            {
                return surfaceGravity * radius * radius / UniverseConstants.G;
            }
        }

        private void Awake()
        {
            velocity = initialVelocity;
        }

        public void UpdateVelocity(CelestialBody[] allBodies, float timeStep)
        {
            foreach (var otherBody in allBodies)
            {
                if (otherBody != this)
                {
                    float sqrDst = (otherBody.transform.position - transform.position).sqrMagnitude;
                    Vector3 forceDir = (otherBody.transform.position - transform.position).normalized;

                    Vector3 acceleration = forceDir * UniverseConstants.G * otherBody.Mass / sqrDst;
                    velocity += acceleration * timeStep;
                }
            }
        }

        public void UpdateVelocity(Vector3 acceleration, float timeStep)
        {
            velocity += acceleration * timeStep;
        }

        public void UpdatePosition(float timeStep)
        {
            //rb.MovePosition(rb.position + velocity * timeStep);
            transform.position += velocity * timeStep;

        }
    }
}