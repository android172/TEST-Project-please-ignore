using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FloraGen
{
    public static class TreeGenerator
    {
        public struct Submesh
        {
            public Vector3[] vertices;
            public int[] triangles;
        }

        public static Submesh GenerateCylinder(float minRadius, float radius, float height, int numSides, int numSegments, AnimationCurve radiusFalloff)
        {
            const float FULL_ROT = 360;//6.28f;
            Submesh mesh = new Submesh();

            int vertCount = numSides * numSegments;
            int triCount = (numSides * 2 * (numSegments - 1)) * 3;

            mesh.vertices = new Vector3[vertCount];
            mesh.triangles = new int[triCount];

            Quaternion offsetRot = Quaternion.AngleAxis(FULL_ROT / numSides, Vector3.up);
            Vector3 heightOffset = Vector3.up * height / (numSegments-1);

            for (int j = 0; j < numSegments; j++)
            {
                Vector3 lastOffset = Vector3.forward * (radius * radiusFalloff.Evaluate((float)j/(numSegments-1)) + minRadius);
                for (int i = 0; i < numSides; i++)
                {
                    mesh.vertices[j*numSides+i] = lastOffset + heightOffset * j;
                    lastOffset = offsetRot * lastOffset;
                }
            }

            int tris = 0;
            for (int j = 0; j < numSegments - 1; j++)
            {
                for (int i = 0; i < numSides - 1; i++)
                {
                    mesh.triangles[tris++] = j * numSides + i;
                    mesh.triangles[tris++] = j * numSides + i + 1;
                    mesh.triangles[tris++] = (j + 1) * numSides + i;

                    mesh.triangles[tris++] = (j + 1) * numSides + i + 1;
                    mesh.triangles[tris++] = (j + 1) * numSides + i;
                    mesh.triangles[tris++] = j * numSides + i + 1;
                }

                //Debug.Log(mesh.triangles.Length + ":::" + tris);
                mesh.triangles[tris++] = j * numSides;
                mesh.triangles[tris++] = (j + 1) * numSides;
                mesh.triangles[tris++] = (j + 1) * numSides - 1;
                
                mesh.triangles[tris++] = (j + 1) * numSides;
                mesh.triangles[tris++] = (j + 2) * numSides - 1;
                mesh.triangles[tris++] = (j + 1) * numSides - 1;
            }

            //Debug.Log(tris / 3);

            return mesh;
        }
    }
}
