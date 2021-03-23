using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Universe
{
    public class SolarSystem : MonoBehaviour
    {
        // [SerializeField] private int debugSteps = 10;

        [SerializeField]
        private GravityObject[] gravityObjects;

        public List<CelestialBody> celestialBodies;

        private void Start()
        {
            gravityObjects = FindObjectsOfType<GravityObject>();
        }

        public Vector3 CalculateVelocity(Vector3 position, Vector3 velocity, float timeStep)
        {
            foreach (var otherBody in celestialBodies)
            {
                float sqrDst = (otherBody.transform.position -position).sqrMagnitude;
                Vector3 forceDir = (otherBody.transform.position - position).normalized;

                Vector3 acceleration = forceDir * UniverseConstants.G * otherBody.Mass / sqrDst;
                velocity += acceleration * timeStep;
            }
            return velocity;
        }

        private void FixedUpdate()
        {
            CelestialBody[] otherBodies = celestialBodies.ToArray();

            foreach (CelestialBody body in celestialBodies)
            {
                /*Vector3 newVelocity = body.initialVelocity;
                foreach (CelestialBody otherBody in celestialBodies)
                {
                    float sqrDst = (otherBody.transform.position - body.transform.position).sqrMagnitude;
                    if (sqrDst == 0)
                        continue;
                    Vector3 dir = (otherBody.transform.position - body.transform.position).normalized;
                    
                    Vector3 velocity = dir * UniverseConstants.G * otherBody.Mass / sqrDst;
                    newVelocity += velocity;
                }

                body.transform.position = body.transform.position + newVelocity * Time.fixedDeltaTime;*/
                body.UpdateVelocity(otherBodies, Time.fixedDeltaTime);
            }

            foreach(GravityObject go in gravityObjects)
            {
                go.UpdateVelocity(otherBodies, Time.fixedDeltaTime);
            }
        }

        private void Update()
        {
            foreach (CelestialBody body in celestialBodies)
            {
                body.UpdatePosition(Time.deltaTime);
            }

            foreach (GravityObject go in gravityObjects)
            {
                go.UpdatePosition(Time.deltaTime);
            }
        }

        private void OnDrawGizmosSelected()
        {
            foreach(CelestialBody body in celestialBodies)
            {
                Gizmos.color = body.trailColor;
                Gizmos.DrawSphere(body.transform.position, body.radius);
            }
        }
    }
}
