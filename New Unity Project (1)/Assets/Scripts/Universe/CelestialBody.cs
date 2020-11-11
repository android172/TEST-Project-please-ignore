using System.Collections;
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

        public float Mass
        {
            get
            {
                return surfaceGravity * radius * radius / UniverseConstants.G;
            }
        }
    }
}
