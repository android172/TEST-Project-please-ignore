using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Universe
{
    public class SolarSystem : MonoBehaviour
    {
        // [SerializeField] private int debugSteps = 10;

        public List<CelestialBody> celestialBodies;

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
        }

        private void Update()
        {
            foreach (CelestialBody body in celestialBodies)
            {
                body.UpdatePosition(Time.deltaTime);
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
