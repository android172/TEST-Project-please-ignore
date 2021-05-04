using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FloraGen
{
    public class FloraGenDebug : MonoBehaviour
    {
        public bool drawPoints, drawIndex;
        public MeshFilter mFilter;
        public float debugCubeSize = 0.1f;

        [Header("Cylinder settings:")]
        [Range(0,5)]
        public float radius;
        [Range(0, 5)]
        public float minRadius;
        [Range(0, 20)]
        public float height;
        [Range(3, 64)]
        public int numSides;
        [Range(2, 64)]
        public int numSegments;
        public AnimationCurve radiusFalloff;

        private TreeGenerator.Submesh submesh;

        [ContextMenu("Generate")]
        public void Generate()
        {
            submesh = TreeGenerator.GenerateCylinder(minRadius, radius, height, numSides, numSegments, radiusFalloff);
            Mesh mesh = new Mesh();
            mesh.Clear();
            mesh.name = "DebugMesh";
            mesh.SetVertices(submesh.vertices);
            mesh.triangles = submesh.triangles;
            //Debug.Log("Verts: " + mesh.vertices.Length + ", Tris: " + mesh.triangles.Length/3);
            mesh.RecalculateNormals();
            mFilter.sharedMesh = mesh;
            //Debug.Log(submesh.vertices[0] + " : " + mesh.vertices[0]);
            //mesh.triangles = submesh.triangles;
        }

        [ContextMenu("Randomize Curve")]
        public void RandomizeCurve()
        {
            radiusFalloff = GenerateRandomCurve(Random.Range(10, 30));
            Generate();
        }

        private void OnValidate()
        {
            Generate();
        }

        private void OnDrawGizmos()
        {
            if (!drawPoints && !drawIndex) return; 
            Gizmos.color = Color.red;
            int i = 0;
            foreach (Vector3 vert in mFilter.sharedMesh.vertices)
            {
                Gizmos.color = Color.Lerp(Color.blue, Color.red, (float)i / mFilter.sharedMesh.vertices.Length);
                if(drawPoints)
                    Gizmos.DrawWireCube(transform.position + vert, Vector3.one * debugCubeSize);
                if(drawIndex)
                    Handles.Label(transform.position + vert, ""+i);
                i++;
            }
        }

        public static AnimationCurve GenerateRandomCurve(int randomPoints)
        {
            AnimationCurve curve = new AnimationCurve();
            for (int i = 0; i < randomPoints; i++)
            {
                curve.AddKey((float)i / (randomPoints - 1), Random.Range(0f, 1f));
            }
            return curve;
        }
    }
}
