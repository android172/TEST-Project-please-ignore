using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO;

public class SphereMeshGenerator {

    internal class Point_s {
        internal int id;
        internal Vector2 location;
        internal Vector2 left_limit_v, right_limit_v;
        // connection 0 is left limit point, connection 1 is right limit point
        internal List<Point_s> connections; 
        // flag used in triangulation
        internal bool connected_to_last_point;

        internal Point_s(int id, float x, float y) {
            this.id = id;
            this.location = new Vector2(x, y);
            connections = new List<Point_s>();
            connections.Add(null);
            connections.Add(null);
            connected_to_last_point = true;
        }
    }

    // params
    private Mesh[] target_mesh_list;
    private const int vertices_per_mesh = 50000;

    // already generated
    private Vector3[] vertices;
    private int[] indices;
    private int number_of_points_already_generated;
    // cache genereted by other ShapeMeshGenerator
    private static List<(int number_of_points, int[] indices, Vector3[] vertices)> cached_spheres = new List<(int, int[], Vector3[])>();

    // sub meshes
    Vector3[][] sub_mesh_v;
    int[][] sub_mesh_i;
    int[] starting_duplicate;

    // methods
    // constructor
    public SphereMeshGenerator() {
        vertices = null;
        indices = null;
        number_of_points_already_generated = -1;
    }
    // set target mesh list
    public void set_target_mesh(Mesh[] mesh_list) {
        target_mesh_list = mesh_list;
    }
    // get number of groups for given number of points
    public int get_no_of_groups(int no_of_points) {
        return (no_of_points - 1) / vertices_per_mesh + 1;
    }
    
    System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
    public void construct_mesh(int number_of_points, ShapeSettings settings) {
        // construct unit sphere
        if (number_of_points != number_of_points_already_generated) {
            // returns tr=ue if sphere with a given number of points is found
                sw.Start();
                construct_unit_sphere(number_of_points);
                sw.Stop();
                Debug.Log("Mesh Construction: " + sw.Elapsed);
                subdivide_meshes(number_of_points);
            if (find_sphere_in_cache(number_of_points) == false) {
                cache_sphere();
            }
        }

        // deform sphere according to the given settings
        Vector3[] vertices_def = settings.apply_noise(this.vertices, number_of_points);

        // normals calculation
        Vector3[] normals = calculate_normals(vertices_def, indices);

        // distribute vertices and normals
        Vector3[][] sub_mesh_n = new Vector3[sub_mesh_v.Length][];
        for (int i = 0; i < sub_mesh_v.Length; i++) {
            int k = starting_duplicate[i];
            sub_mesh_n[i] = new Vector3[sub_mesh_v[i].Length];
            for (int j = 0; j < sub_mesh_v[i].Length; j++) {
                sub_mesh_v[i][j] = vertices_def[k];
                sub_mesh_n[i][j] = normals[k++];
            }
        }
        
        // construct mesh
        for (int i = 0; i < target_mesh_list.Length; i++) {
            target_mesh_list[i].Clear();
            target_mesh_list[i].vertices = sub_mesh_v[i];
            target_mesh_list[i].triangles = sub_mesh_i[i];
            target_mesh_list[i].SetNormals(sub_mesh_n[i]);
            target_mesh_list[i].SetUVs(0, sub_mesh_v[i]);
        }
    }

    // find in cache
    private bool find_sphere_in_cache(int number_of_points) {
        foreach (var sphere in cached_spheres)
            if (sphere.number_of_points == number_of_points) {
                number_of_points_already_generated = sphere.number_of_points;
                vertices = sphere.vertices;
                indices = sphere.indices;
                subdivide_meshes(number_of_points);
                return true;
            }
        return false;
    }

    // put sphere in cache
    private void cache_sphere() {
        cached_spheres.Add((number_of_points_already_generated, indices, vertices));
    }

    // construct unit sphere with equally spaced points
    private void construct_unit_sphere(int number_of_points) {
        vertices = new Vector3[number_of_points];
        indices = new int[number_of_points];
        Point_s[] stereographic = new Point_s[number_of_points]; // stereographic projection of vertices onto XY plane with z = 0

        // Generate sphere points
        double s = 3.6 / System.Math.Sqrt(number_of_points);
        double longitude = 0.0;
        double dz = 2.0 / number_of_points; // delta z
        double z = 1.0 - dz / 2.0;

        for (int i = 0; i < number_of_points; i++) {
            double r = System.Math.Sqrt(1f - z * z);

            double x = System.Math.Cos(longitude) * r;
            double y = System.Math.Sin(longitude) * r;

            vertices[i] = new Vector3((float)x, (float)y, (float)z);

            // project to plane
            stereographic[number_of_points - i - 1] = new Point_s(i, (float)(x / (1.0 - z)), (float)(y / (1.0 - z)));

            // iterate
            z -= dz;
            if (z > 1.0) z = 1.0;
            longitude += +s / r;
        }

        // triangulate given points
        string path = Application.persistentDataPath + "/" + number_of_points.ToString() + ".tri";

        if (File.Exists(path)) {
            using (var reader = new BinaryReader(new FileStream(path, FileMode.Open, FileAccess.Read))) {
                int indices_length = reader.ReadInt32();
                indices = new int[indices_length];
                for (int i = 0; i < indices_length; i++)
                    indices[i] = reader.ReadInt32();
            }
        }
        else {
            indices = triangulate(stereographic, number_of_points);
            Debug.Log("Not Found");

            using (var writer = new BinaryWriter(new FileStream(path, FileMode.Create, FileAccess.Write))) {
                writer.Write(indices.Length);
                for (int i = 0; i < indices.Length; i++)
                    writer.Write(indices[i]);
            }
        }

        // done
        number_of_points_already_generated = number_of_points;
    }

    // points triangulation
    private static int[] triangulate(Point_s[] points, int number_of_points) {
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
            Debug.Log("We have collinear starters!!!");
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
        // first connection of any new point will most likely be to the last point included
        // so we need to keep track of said point
        Point_s last_point = points[p3];

        // iterating trough the rest  of points
        for (int i = p3 + 1; i < number_of_points; i++) {
            Point_s c_point = points[i];
            // if point is projected at infinity
            if (c_point.location.normalized.magnitude == 0) {
                Debug.Log("inf point : " + i);
                infinity_point = true;
                continue;
            }

            // points to which our new point could have a connection
            Stack<Point_s> open_p = new Stack<Point_s>();
            open_p.Push(last_point);
            // points already checked
            Stack<Point_s> closed_p = new Stack<Point_s>();

            // iterating trough possible connections
            while (open_p.Count > 0) {
                Point_s old_point = open_p.Pop();
                closed_p.Push(old_point);

                Vector2 old_to_new_v = vector_a_to_b(old_point, c_point);
                float left_relation = Vector2.Dot(old_point.left_limit_v, old_to_new_v);
                float right_relation = Vector2.Dot(old_point.right_limit_v, old_to_new_v);

                if (left_relation >= 0 && right_relation >= 0) {
                    c_point.connected_to_last_point = false;
                    while (left_relation >= 0 && right_relation >= 0  && !closed_p.Contains(old_point.connections[0])) {
                        old_point = old_point.connections[0];
                        closed_p.Push(old_point);

                        left_relation = Vector2.Dot(old_point.left_limit_v, old_to_new_v);
                        right_relation = Vector2.Dot(old_point.right_limit_v, old_to_new_v);
                    }
                }

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

        // // list all the triangles
        List<int []> triangles = new List<int []>(); // list of triangles

        for (int i = 2; i < number_of_points; i++) {
            Vector2 reference_vector;
            if (points[i].connected_to_last_point)
                reference_vector = vector_a_to_b(points[i], points[i - 1]).normalized;
            else reference_vector = vector_a_to_b(points[i].connections[0], points[i]).normalized;
            Vector2 reference_vector_n = new Vector2(-reference_vector.y, reference_vector.x);
            List<(float x, float y, int value)> relevant_connections = new List<(float, float, int)>();

            int j = 0;
            foreach (var conn in points[i].connections) {
                if (conn.id > points[i].id) {
                    Vector2 ab = vector_a_to_b(points[i], conn).normalized;
                    relevant_connections.Add((Vector2.Dot(ab, reference_vector_n), Vector2.Dot(ab, reference_vector), j));
                }
                j++;
            }
            
            relevant_connections.Sort((a, b) => {
                if (a.x * b.x < 0) {
                    if (a.x > 0) return -1;
                    return 1;
                }
                if (a.x * b.x > 0) {
                    if (a.x > 0) {
                        if (a.y > b.y) return -1;
                        return 1;
                    }
                    if (a.y > b.y) return 1;
                    return -1;
                }
                if (a.x == 0) {
                    if (a.y > 0) return -1;
                    if (b.x > 0) return 1;
                    return -1;
                }
                if (b.y > 0) return 1;
                if (a.x > 0) return -1;
                return 1;
            });

            // create triangles
            for (int k = 0; k < relevant_connections.Count - 1; k++) {
                int[] triangle = new int[] {points[i].id, points[i].connections[relevant_connections[k].value].id, points[i].connections[relevant_connections[k + 1].value].id};
                // determine orientation
                if (!clock_wise_oriented(points[i], points[i].connections[relevant_connections[k].value], points[i].connections[relevant_connections[k + 1].value]))
                    triangle = new int[] { triangle[0], triangle[2], triangle[1]};
                triangles.Add(triangle);
            }
        }

        // // calculating remaining open points
        List<Point_s> hole = new List<Point_s>();

        Point_s hole_point = last_point;
        do {
            hole.Add(hole_point);
            hole_point = hole_point.connections[1];
        } while (hole_point != last_point);

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
        int[] indices = new int[3 * triangles.Count];
        for (int i = 0; i < triangles.Count; i++) {
            indices[3 * i]     = triangles[i][0];
            indices[3 * i + 1] = triangles[i][1];
            indices[3 * i + 2] = triangles[i][2];
        }
        
        return indices;
    }
    
    // mesh construction
    private void subdivide_meshes(int number_of_points) {
        // if there is no need for subdevision don't subdivide
        if (number_of_points <= vertices_per_mesh) {
            sub_mesh_i = new int[1][];
            sub_mesh_i[0] = indices;
            sub_mesh_v = new Vector3[1][];
            sub_mesh_v[0] = new Vector3[number_of_points];
            starting_duplicate = new int[] { 0 };
            
            return;
        }
        
        // number of meshes that will be generated
        int number_of_groups = (number_of_points - 1) / vertices_per_mesh + 1;
        sub_mesh_i = new int[number_of_groups][];
        sub_mesh_v = new Vector3[number_of_groups][];

        // extra vertices starting index
        starting_duplicate = new int[number_of_groups];
        // indices contained in given group
        List<int>[] group_indices = new List<int>[number_of_groups];
        
        for (int i = 0; i < number_of_groups; i++) {
            starting_duplicate[i] = i * vertices_per_mesh;
            group_indices[i] = new List<int>();
        }
        
        // // sub mesh indices calculation
        // group indices
        for (int i = 0; i < indices.Length; i += 3) {
            int group = indices[i] / vertices_per_mesh;
            // if all 3 indices are not in the same group
            if (2 * group - indices[i + 1] / vertices_per_mesh - indices[i + 2] / vertices_per_mesh != 0) {
                int group2 = indices[i + 1] / vertices_per_mesh;
                int group3 = indices[i + 2] / vertices_per_mesh;
                // group and duplicates calculation
                // for second index
                if (group2 > group) {
                    group = group2;
                    if (indices[i] < starting_duplicate[group]) starting_duplicate[group] = indices[i];
                }
                else if (group2 < group) {
                    if (indices[i + 1] < starting_duplicate[group]) starting_duplicate[group] = indices[i + 1];
                }
                // for third index
                if (group3 > group) {
                    group = group3;
                    if (indices[i] < starting_duplicate[group]) starting_duplicate[group] = indices[i];
                    if (indices[i + 1] < starting_duplicate[group]) starting_duplicate[group] = indices[i + 1];
                }
                else if (group3 < group) {
                    if (indices[i + 2] < starting_duplicate[group]) starting_duplicate[group] = indices[i + 2];
                }
            }
            group_indices[group].Add(indices[i]);
            group_indices[group].Add(indices[i + 1]);
            group_indices[group].Add(indices[i + 2]);
        }
        // distribute indices
        for (int i = 0; i < number_of_groups; i++) {
            sub_mesh_i[i] = new int[group_indices[i].Count];
            for (int j = 0; j < sub_mesh_i[i].Length; j++)
                sub_mesh_i[i][j] = group_indices[i][j] - starting_duplicate[i];
        }

        // // sub mesh vertices calculation
        // calculate number of vertices for every group
        for (int i = 0; i < number_of_groups; i++) {
            int vertices_in_group = (i + 1) * vertices_per_mesh - starting_duplicate[i];
            if ((i + 1) * vertices_per_mesh > number_of_points)
                vertices_in_group = number_of_points - starting_duplicate[i];
            sub_mesh_v[i] = new Vector3[vertices_in_group];
        }
    }

    //////////////////////
    // Helper functions //
    //////////////////////
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
    // for normals
    static Vector3[] calculate_normals(Vector3[] vertices, int[] indices) {
        Vector3[] normals = new Vector3[vertices.Length];
        for (int i = 0; i < indices.Length / 3; i++) {
            // // calculating normal of current triangle
            // indexes of relevant vertices
            int index_1 = indices[3 * i];
            int index_2 = indices[3 * i + 1];
            int index_3 = indices[3 * i + 2];
            // normal calculation using cross product
            Vector3 triangle_normal = Vector3.Cross(vertices[index_2] - vertices[index_1], vertices[index_3] - vertices[index_1]);
            // // adding triangle normal as one of the factors of individual vertex normals
            normals[index_1] += triangle_normal;
            normals[index_2] += triangle_normal;
            normals[index_3] += triangle_normal;
        }
        // normalizeing normal vectors
        for (int i = 0; i < normals.Length; i++)
            normals[i].Normalize();

        return  normals;
    }
}
