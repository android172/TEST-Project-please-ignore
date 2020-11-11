using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;

public static class SphereMeshGenerator {
    
    internal class Point_s {
        internal int id;
        internal Vector2 location;
        internal Vector2 left_limit_v, right_limit_v;
        // connection 0 is left limit point, connection 1 is right limit point
        internal List<Point_s> connections; 

        internal Point_s(int id, float x, float y) {
            this.id = id;
            this.location = new Vector2(x, y);
            connections = new List<Point_s>();
            connections.Add(null);
            connections.Add(null);
        }

        public static bool operator <(Point_s p1, Point_s p2) {
            if (p2 == null) return false;
            if (p2 == null) return false;
            if (p1.location.x < p2.location.x) return true;
            else if (p1.location.x > p2.location.x) return false;
            if (p1.location.y < p2.location.y) return true;
            return false;
        }
        public static bool operator >(Point_s p1, Point_s p2) {
            if (p1.location.x > p2.location.x) return true;
            else if (p1.location.x < p2.location.x) return false;
            if (p1.location.y > p2.location.y) return true;
            return false;
        }

        // override object.Equals
        public override bool Equals(object obj) {
            if (obj == null || GetType() != obj.GetType()) {
                return false;
            }
            Point_s other = (Point_s)obj;
            if (this.id == other.id) return true;
            return false;
        }

        // override object.GetHashCode
        public override int GetHashCode() {
            return this.id;
        }
    }

    public static void construct_mesh(Mesh mesh, int number_of_points) {
        Vector3[] vertices = new Vector3[number_of_points];
        List<Point_s> stereographic = new List<Point_s>(); // stereographic projection of vertices onto XY plane with z = 0
        int[] indicies;

        Vector3[] plane = new Vector3[number_of_points];

        // Generate sphere points
        float s = 3.6f / Mathf.Sqrt(number_of_points);
        float longitude = 0f;
        float dz = 2f / number_of_points; // delta z
        float z = 1f - dz / 2f;

        double t1 = EditorApplication.timeSinceStartup;
        for (int i = 0; i < number_of_points; i++) {
            float r = Mathf.Sqrt(1f - z * z);

            float x = Mathf.Cos(longitude)*r;
            float y = Mathf.Sin(longitude)*r;

            vertices[i] = new Vector3(x, y, z);

            // project to plane sorted
            stereographic.Add(new Point_s(i, x / (1f - z), y / (1f - z)));

            // iterate
            z -= dz;
            longitude += + s / r;
        }
        stereographic.Sort((p1, p2) => {
            if (p1.location.x < p2.location.x) return -1;
            if (p1.location.x > p2.location.x) return  1;
            if (p1.location.y < p2.location.y) return -1;
            return 1;
        });
        double t2 = EditorApplication.timeSinceStartup;

        Debug.Log(t2 - t1);

        // triangulate given points
        indicies = triangulate(stereographic, number_of_points);

        // construct new mesh
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = indicies;
        mesh.RecalculateNormals();
    }

    static int[] triangulate(List<Point_s> points, int number_of_points) {
        ////////////////////////////
        // Delaunay triangulation //
        ////////////////////////////
        // // initialize first triangle
        // which points to work on
        int p1 = 0;
        int p2 = 1;
        int p3 = 2;
        // if first 3 points are collinear
        if (are_colinear(points[0], points[1], points[2])) {
            int i;
            for (i = 0; i < number_of_points && are_colinear(points[0], points[1], points[i]); i++);
            if (i == number_of_points) {
                // there was a catastrophic error
                Debug.Log("There was an error with triangulation :: All points are colinear :: 1");
                return null;
            }
            // TODO:
            // implement this algorithm in case we start with the list of collinear points
            return null;
        }
        
        // establish first connections together with initial limits
        // points must be clockwise for initialization to work
        if (!clock_wise_oriented(points[p1], points[p2], points[p3])) {
            int temp = p1;
            p1 = p2;
            p2 = temp;
        }
        // left / right limit
        points[p1].left_limit_v = points[p2].right_limit_v = find_normal_on_line(points[p2], points[p1]);
        points[p2].left_limit_v = points[p3].right_limit_v = find_normal_on_line(points[p3], points[p2]);
        points[p3].left_limit_v = points[p1].right_limit_v = find_normal_on_line(points[p1], points[p3]);
        // left / right most point
        points[p1].connections[0] = points[p2]; // left  of p1 is p2
        points[p1].connections[1] = points[p3]; // right of p1 is p3
        points[p2].connections[0] = points[p3]; // left  of p2 is p3
        points[p2].connections[1] = points[p1]; // right of p2 is p1
        points[p3].connections[0] = points[p1]; // left  of p3 is p1
        points[p3].connections[1] = points[p2]; // right of p3 is p2

        // // we may be dealing with point projected at infinity
        bool infinity_point = false;

        // // initialize other triangles
        // first connection of any new point will be to the last point included
        // so we need to keep track of said point
        Point_s last_point = points[p3];

        // iterating trough the rest  of points
        double t1 = EditorApplication.timeSinceStartup;
        foreach (var c_point in points) {
            // if point is projected at infinity
            if (c_point.location.normalized.magnitude == 0) {
                infinity_point = true;
                continue;
            }

            // points to which our new point could have a connection
            Stack<Point_s> open_p = new Stack<Point_s>();
            open_p.Push(last_point);
            // points already checked
            Stack <Point_s> closed_p = new Stack<Point_s>();

            // iterating trough possible connections
            while (open_p.Count > 0) {
                Point_s old_point = open_p.Pop();
                closed_p.Push(old_point);
                Vector2 old_to_new_v = vector_a_to_b(old_point, c_point);

                float left_relation = Vector2.Dot(old_point.left_limit_v, old_to_new_v);
                float right_relation = Vector2.Dot(old_point.right_limit_v, old_to_new_v);

                // old and new point can form a triangle with the given common point
                Point_s common_point = null;

                // new point is behind left line
                if (left_relation < 0) {
                    // we should also check other point on that line if they are not already covered
                    // if they are we can form triangle with them
                    if (closed_p.Contains(old_point.connections[0])) common_point = old_point.connections[0];
                    else open_p.Push(old_point.connections[0]);

                    // new point is now also the new limit connection
                    Vector2 new_limit = find_normal_on_line(c_point, old_point);
                    old_point.left_limit_v = new_limit;
                    old_point.connections.Add(old_point.connections[0]);
                    old_point.connections[0] = c_point;

                    // new line is also opposite limit of new point
                    if (right_relation >= 0) {
                        if (c_point.connections[1] != null) c_point.connections.Add(c_point.connections[1]);
                        c_point.connections[1] = old_point;
                        c_point.right_limit_v = new_limit;
                    }
                    // if not just add connection
                    else c_point.connections.Add(old_point);
                }
                // new point is behind right line
                if (right_relation < 0) {
                    if (closed_p.Contains(old_point.connections[1])) common_point = old_point.connections[1];
                    else open_p.Push(old_point.connections[1]);

                    Vector2 new_limit = find_normal_on_line(old_point, c_point);
                    old_point.right_limit_v = new_limit;

                    if (left_relation >= 0) {
                        old_point.connections.Add(old_point.connections[1]);
                        old_point.connections[1] = c_point;

                        if (c_point.connections[0] != null) c_point.connections.Add(c_point.connections[0]);
                        c_point.connections[0] = old_point;
                        c_point.left_limit_v = new_limit;
                    }
                }

                // form a triangle if possible
                if (common_point != null) {
                    // points to check for optimization
                    Stack<Point_s[]> points_to_triangulate = new Stack<Point_s[]>();
                    // points need to be in CCW order for the algorithm to work
                    if (clock_wise_oriented(old_point, common_point, c_point))
                        points_to_triangulate.Push(new Point_s[] { common_point, old_point });
                    else points_to_triangulate.Push(new Point_s[] { old_point, common_point });

                    // we try to locally optimize while possible
                    while (points_to_triangulate.Count > 0) {
                        Point_s[] tri_p = points_to_triangulate.Pop();

                        // we need to find the point which forms a triangle facing ours
                        Point_s facing_point = null;
                        // triangle search
                        Vector2 normal_1_0 = find_normal_on_line(tri_p[1], tri_p[0]);
                        float min_rel = -1;
                        foreach (var tp in tri_p[0].connections) {
                            if (!tp.Equals(c_point) && tp.connections.Contains(tri_p[1])) {
                                float rel = Vector2.Dot(normal_1_0, vector_a_to_b(tri_p[0], tp));
                                if (min_rel == -1 || rel < min_rel) {
                                    facing_point = tp;
                                    min_rel = rel;
                                }
                            }
                        }

                        if (facing_point != null) {
                            Vector2 pA = tri_p[0].location;
                            Vector2 pB = tri_p[1].location;
                            Vector2 pC = c_point.location;
                            Vector2 pD = facing_point.location;

                            Vector4 mat_row_1 = new Vector4(pA.x, pA.y, pA.x * pA.x + pA.y * pA.y, 1);
                            Vector4 mat_row_2 = new Vector4(pB.x, pB.y, pB.x * pB.x + pB.y * pB.y, 1);
                            Vector4 mat_row_3 = new Vector4(pC.x, pC.y, pC.x * pC.x + pC.y * pC.y, 1);
                            Vector4 mat_row_4 = new Vector4(pD.x, pD.y, pD.x * pD.x + pD.y * pD.y, 1);

                            Matrix4x4 check = new Matrix4x4(mat_row_1, mat_row_2, mat_row_3, mat_row_4);

                            // if determinant of given matrix is positive triangulation can be locally improved by switching triangle order
                            if (check.determinant > 0) {
                                // reconnecting
                                tri_p[0].connections.Remove(tri_p[1]);
                                tri_p[1].connections.Remove(tri_p[0]);
                                c_point.connections.Add(facing_point);
                                facing_point.connections.Add(c_point);

                                // points that should also be checked
                                points_to_triangulate.Push(new Point_s[] { tri_p[0], facing_point });
                                points_to_triangulate.Push(new Point_s[] { facing_point, tri_p[1] });
                            }
                        }
                    }
                }
            }

            last_point = c_point;
        }
        double t2 = EditorApplication.timeSinceStartup;

        Debug.Log(t2 - t1);

        t1 = EditorApplication.timeSinceStartup;
        // // list all the triangles
        List<int []> triangles = new List<int []>(); // list of triangles

        for (int i = 0; i < number_of_points-2; i++) {
            List<(float angle, int value)> relevant_connections = new List<(float, int)>();

            int j = 0;
            foreach (var conn in points[i].connections) {
                if (points[i] < conn) relevant_connections.Add((vector_a_to_b(points[i], conn).normalized.y, j));
                j++;
            }

            relevant_connections.Sort((a, b) => {
                if (a.angle < b.angle) return -1;
                if (a.angle > b.angle) return 1;
                return 0;
            });

            for (int k = 0; k < relevant_connections.Count - 1; k++) {
                triangles.Add(make_a_triangle(points[i], points[i].connections[relevant_connections[k].value], points[i].connections[relevant_connections[k + 1].value]));
            }
        }
        t2 = EditorApplication.timeSinceStartup;

        Debug.Log(t2 - t1);

        t1 = EditorApplication.timeSinceStartup;
        // // calculating remaining open points
        List<Point_s> hole = new List<Point_s>();

        Point_s hole_point = last_point;
        do {
            hole.Add(hole_point);
            hole_point = hole_point.connections[1];
        } while (hole_point != null && hole_point != last_point);

        if (infinity_point) {
            for (int i = 1; i < hole.Count - 1; i++) {
                triangles.Add(new int[] { points[number_of_points - 1].id, hole[i].id, hole[i + 1].id });
            }
        }
        else {
            for (int i = 1; i < hole.Count - 1; i++) {
                triangles.Add(new int[] { hole[0].id, hole[i].id, hole[i + 1].id });
            }
        }

        // // all triangles are formed, we should list them
        int[] indicies = new int[3 * triangles.Count];
        for (int i = 0; i < triangles.Count; i++) {
            indicies[3 * i]     = triangles[i][0];
            indicies[3 * i + 1] = triangles[i][1];
            indicies[3 * i + 2] = triangles[i][2];
        }
        t2 = EditorApplication.timeSinceStartup;
        Debug.Log(t2 - t1);
        return indicies;
    }
    

    // Helper functions //
    // for vectors
    static Vector2 vector_a_to_b(Point_s a, Point_s b) {
        return b.location - a.location;
    }
    static Vector2 find_normal_on_line(Point_s a, Point_s b){
        Vector2 ab = b.location - a.location;
        return new Vector2(-ab.y, ab.x);
    }
    // for points
    static bool clock_wise_oriented(Point_s p1, Point_s p2, Point_s p3) {
        Vector2 p1_p2_n = find_normal_on_line(p1, p2); // normal on line P1P2
        Vector2 p1_p3   = vector_a_to_b(p1, p3);       // vector from P1 to P3
        if (Vector2.Dot(p1_p2_n, p1_p3) > 0) return false;
        return true;
    }
    static bool are_colinear(Point_s p1, Point_s p2, Point_s p3) {
        const float EM = 0.00000001f; // error margin

        Vector2 v1 = find_normal_on_line(p1, p2);
        Vector2 v2 = vector_a_to_b(p1, p3);
        if (Mathf.Abs(Vector2.Dot(v1, v2)) < EM) return true;
        return false;
    }
    // for triangles
    static int[] make_a_triangle(Point_s p1, Point_s p2, Point_s p3) {
        // determine orientation
        if (clock_wise_oriented(p1, p2, p3)) return new int[] { p1.id, p2.id, p3.id };
        else return new int[] { p1.id, p3.id, p2.id };
    }
}
