using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Universe
{
    [RequireComponent(typeof(Rigidbody))]
    public class GravityObject : MonoBehaviour
    {
        [SerializeField] public float mass = 1;
        [SerializeField] public float gravityCDTime = .5f;

        private Vector3 velocity;
        private Rigidbody rb;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody>();
            }
            rb.mass = mass;
            rb.useGravity = false;
        }

        public void UpdateVelocity(CelestialBody[] allBodies, float timeStep)
        {
            if (useGravity)
            {
                foreach (var otherBody in allBodies)
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
            Vector3 desiredPosition = rb.position + velocity * timeStep;

            Vector3 direction = velocity * timeStep;
            Ray ray = new Ray(rb.position, direction);
            RaycastHit hit;
            if (!Physics.Raycast(ray, out hit, direction.magnitude * 5))
            {
                rb.MovePosition(desiredPosition);
                /*if (!useGravity)
                {
                    ToggleGravity(true);
                }*/
            }
            else
            {
                velocity = Vector3.zero;
                rb.MovePosition(hit.point);
                StartCoroutine(GravityCooldown());
            }
        }

        private void OnCollisionStay(Collision collision)
        {
            velocity = rb.velocity;
        }

        private bool useGravity = true;

        IEnumerator GravityCooldown()
        {
            yield return new WaitForSeconds(gravityCDTime);
            ToggleGravity(false);
        }

        public void ToggleGravity(bool newValue)
        {
            useGravity = newValue;
            rb.isKinematic = !newValue;
        }
    }
}