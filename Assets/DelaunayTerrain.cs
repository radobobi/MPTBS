﻿using System.Collections.Generic;
using UnityEngine;
using TriangleNet.Geometry;
using TriangleNet.Voronoi;

public class DelaunayTerrain : MonoBehaviour {
    private enum FaceMergingRule
    {
        SMALLEST_AREA_NEIGHBOR = 0,
        LARGEST_EDGE_NEIGHBOR = 1,
        LARGEST_AREA_NEIGHBOR = 2
    }

    private FaceMergingRule defaultFaceMergingRule = FaceMergingRule.LARGEST_AREA_NEIGHBOR;

    public struct HalfEdge {
        public int id;
        public int twin_id;
        public int start;
        public int end;
        public int face;
    }

    public struct VoronoiFace {
        public int id;
        public float origin_x;
        public float origin_y;
        public List<HalfEdge> half_edges;
        public List<int> neighbors;
        public Mesh mesh;
        public Transform chunk;
        public bool merged;
        public float area;
    }

    private static float EPSILON = 0.000000001f;
    private static float MAX_MERGING_ITERATIONS = 10;
    public float MIN_EDGE_LENGTH = 0.1f;
    public float MIN_FACE_AREA = 2f;

    // Maximum size of the terrain.
    public int xsize = 10;
    public int ysize = 10;

    // Number of random points to generate.
    public int randomPoints = 1000;

    // Triangles in each chunk.
    public int trianglesInChunk = 20000;

    // Perlin noise parameters
    public float sampleSize = 1.0f;
    public int octaves = 8;
    public float frequencyBase = 2;
    public float persistence = 1.1f;

    // Prefab which is generated for each chunk of the mesh.
    public Transform chunkPrefab = null;
    public Transform textPrefab = null;
    public Transform spherePrefab = null;
    //public Font font = null;

    // TraingleNet mesh.
    private TriangleNet.Topology.DCEL.DcelMesh mesh2 = null;

    public Vector3[] vertices;
    public HalfEdge[] halfedges;
    public VoronoiFace[] faces;
    public FaceManager[] faces_managers;

    /* Adding a combat manager */
    private CombatManager _cm;

    void Start() {
        
        UnitsStats setUpStats = UnitsStats.CreateUnitsStats();
        Army attackingArmy = Army.CreateMyArmy(); 
        attackingArmy.Start();
        attackingArmy.addUnitToArmy(Unit.CreateMyUnit().SetParams((int)UnitType.Swordsman, ""));
        attackingArmy.addUnitToArmy(Unit.CreateMyUnit().SetParams((int)UnitType.Swordsman, ""));
        Army defendingArmy = Army.CreateMyArmy();
        defendingArmy.Start();
        defendingArmy.addUnitToArmy(Unit.CreateMyUnit().SetParams((int)UnitType.Swordsman, ""));
        defendingArmy.addUnitToArmy(Unit.CreateMyUnit().SetParams((int)UnitType.Archer, ""));
        _cm = CombatManager.CreateMyCM(attackingArmy, defendingArmy);
        _cm.ConductBattle();
        

        GenerateMap();
    }

    public virtual void GenerateMap() {
        print("GENERATING TRANGULATION.");

        UnityEngine.Random.InitState(0);

        // What does this do?
        float[] seed = new float[octaves];

        for (int i = 0; i < octaves; i++) {
            seed[i] = UnityEngine.Random.Range(0.0f, 100.0f);
        }

        Polygon polygon = new Polygon();
        for (int i = 0; i < randomPoints; i++) {
            polygon.Add(new Vertex(UnityEngine.Random.Range(0.0f, xsize), UnityEngine.Random.Range(0.0f, ysize)));
        }

        // Generate Delauney triangulation from points.
        TriangleNet.Meshing.ConstraintOptions options = new TriangleNet.Meshing.ConstraintOptions() { ConformingDelaunay = true };
        TriangleNet.Mesh mesh = (TriangleNet.Mesh)polygon.Triangulate(options);
        // Generate Voronoi Tesselation from Delauney triangulation. 
        //mesh2 = (TriangleNet.Topology.DCEL.DcelMesh)(new StandardVoronoi(mesh, new TriangleNet.Geometry.Rectangle(0f, 0f, (float)xsize, (float)ysize)));
        mesh2 = (TriangleNet.Topology.DCEL.DcelMesh)(new BoundedVoronoi(mesh));

        // Post-processing and object-generation.
        ObtainVerticesEdgesAndFaces();
        //RemoveShortEdges();
        GenerateMesh();
        MergeSmallFaces();
        PopulateFacesManagers();

        // Drawing calls. (Explicit or implicit.)
        DrawEdges();
        DrawEdgeLabels();
        DrawFaceLabels();
        //GenerateVertexSpheres();
        GenerateFaceCenterSpheres();
    }

    // Extracts edge and vertex info from TriangletNet Delauney data structure.
    public void ObtainVerticesEdgesAndFaces() {
        print("CONVERTING TRIANGLENET OUTPUT TO UNITY MESH.");

        TriangleNet.Topology.DCEL.DcelMesh mesh2 = this.mesh2;

        vertices = new Vector3[mesh2.Vertices.Count];

        for (int indver = 0; indver < mesh2.Vertices.Count; ++indver) {
            vertices[indver].x = (float)mesh2.Vertices[indver].X;
            vertices[indver].z = (float)mesh2.Vertices[indver].Y;
        }

        halfedges = new HalfEdge[mesh2.HalfEdges.Count];

        for (int indhe = 0; indhe < mesh2.HalfEdges.Count; ++indhe) {
            halfedges[indhe].id = mesh2.HalfEdges[indhe].ID;
            halfedges[indhe].twin_id = mesh2.HalfEdges[indhe].Twin.ID;
            halfedges[indhe].start = (int)mesh2.HalfEdges[indhe].Origin.id;
            halfedges[indhe].end = (int)mesh2.HalfEdges[indhe].Twin.Origin.id;
            halfedges[indhe].face = mesh2.HalfEdges[indhe].Face.ID;
        }

        faces = new VoronoiFace[mesh2.Faces.Count];

        for (int indf = 0; indf < mesh2.Faces.Count; ++indf) {
            faces[indf].merged = false;
            faces[indf].origin_x = (float)mesh2.Faces[indf].generator.X;
            faces[indf].origin_y = (float)mesh2.Faces[indf].generator.Y;
            faces[indf].id = mesh2.Faces[indf].ID;

            faces[indf].half_edges = new List<HalfEdge>();
            faces[indf].neighbors = new List<int>();
            for (int indhe = 0; indhe < halfedges.Length; ++indhe) {
                if (halfedges[indhe].face == mesh2.Faces[indf].ID) {
                    faces[indf].half_edges.Add(halfedges[indhe]);
                    faces[indf].neighbors.Add(halfedges[halfedges[indhe].twin_id].face);
                }
            }
        }
    }

    // Generates meshes from vertices and edges.
    private void GenerateMesh() {
        for (int i = 0; i < faces.Length; ++i) {
            // Skip if face area is 0..
            faces[i].area = ComputeFaceArea(i);
            //float area = ComputeFaceArea(i);

            VoronoiFace current_face = faces[i];
            List<int> triangles = new List<int>();

            Vector3 faceCenter = new Vector3(current_face.origin_x, 0, current_face.origin_y);
            List<Vector3> faceVertices = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();

            foreach (HalfEdge he in current_face.half_edges) {
                Vector3 v0 = faceCenter;
                Vector3 v1 = vertices[he.start];
                Vector3 v2 = vertices[he.end];

                triangles.Add(faceVertices.Count);
                triangles.Add(faceVertices.Count + 1);
                triangles.Add(faceVertices.Count + 2);

                faceVertices.Add(v2);
                faceVertices.Add(v1);
                faceVertices.Add(v0);

                Vector3 normal = Vector3.Cross(v2 - v0, v1 - v0);
                normals.Add(normal);
                normals.Add(normal);
                normals.Add(normal);

                uvs.Add(new Vector2(0.0f, 0.0f));
                uvs.Add(new Vector2(0.0f, 0.0f));
                uvs.Add(new Vector2(0.0f, 0.0f));
            }

            Mesh chunkMesh = new Mesh();
            chunkMesh.vertices = faceVertices.ToArray();
            chunkMesh.uv = uvs.ToArray();
            chunkMesh.triangles = triangles.ToArray();
            chunkMesh.normals = normals.ToArray();

            Transform chunk = Instantiate<Transform>(chunkPrefab, transform.position, transform.rotation);
            chunk.GetComponent<MeshFilter>().mesh = chunkMesh;

            if (faces[i].area >= EPSILON) {
                chunk.GetComponent<MeshCollider>().sharedMesh = chunkMesh;
            }

            chunk.rotation = new Quaternion(0.707f, 0, 0, -0.707f);
            chunk.transform.parent = transform;
            chunk.gameObject.AddComponent<MapClickDetector>();
            chunk.gameObject.GetComponent<MeshRenderer>().material.color = Color.red;

            faces[i].mesh = chunkMesh;
            faces[i].chunk = chunk;
            AssignVoronoiFace(faces[i], chunk.GetComponent<FaceManager>());

        }

        print("SUCCESSFULLY GENERATED MESH!");
    }

    // Draws edges of Voronoi cells.
    private void DrawEdges() {
        for (int i = 0; i < halfedges.Length; ++i) {
            if (halfedges[i].face >= 0) {
                Vector3 start = vertices[halfedges[i].start];
                Vector3 end = vertices[halfedges[i].end];
                DrawLine(new Vector3(start.x, start.z, 0), new Vector3(end.x, end.z, 0), Color.blue, transform);
            }
        }
    }

    // Draws edge labels.
    private void DrawEdgeLabels() {
        for (int i = 0; i < halfedges.Length; ++i) {
            if (halfedges[i].face >= 0) {
                Vector3 start = vertices[halfedges[i].start];
                Vector3 end = vertices[halfedges[i].end];
                DrawText(new Vector3((start.x + end.x) / 2, (start.z + end.z) / 2, 0), i.ToString(), Color.black);
            }
        }
    }

    // Draws face labels.
    private void DrawFaceLabels() {
        for (int i = 0; i < faces.Length; ++i) {
            if (faces[i].id >= 0) {
                DrawText(new Vector3(faces[i].origin_x, faces[i].origin_y, 0), i.ToString(), Color.green);
            }
        }
    }

    // Function for drawing lines. (Used to draw edges.)
    void DrawLine(Vector3 start, Vector3 end, Color color, Transform transform) {
        GameObject myLine = new GameObject("Line " + start.ToString() + "--" + end.ToString());
        myLine.transform.parent = transform;
        myLine.transform.localPosition = Vector3.zero;
        myLine.AddComponent<LineRenderer>();
        LineRenderer lr = myLine.GetComponent<LineRenderer>();
        lr.material = new Material(Shader.Find("Particles/Alpha Blended Premultiply"));
        lr.startColor = color;
        lr.endColor = color;
        lr.startWidth = 0.1f;
        lr.endWidth = 0.1f;
        lr.SetPosition(0, start + transform.position);
        lr.SetPosition(1, end + transform.position);
    }

    // Function for drawing text. (Used to draw labels.)
    void DrawText(Vector3 loc, string txt, Color clr) {
        Transform text = Instantiate<Transform>(textPrefab, transform.position, transform.rotation);
        text.transform.parent = transform;
        text.transform.localPosition = loc;
        text.GetComponent<TextMesh>().text = txt;
        text.GetComponent<TextMesh>().characterSize = 1;
        text.GetComponent<TextMesh>().color = clr;
    }


    // Iterate over edges.
    // Bring ends of shorts edges together.
    // Be careful with neibhors.
    private void RemoveShortEdges() {
        Vector3 start;
        Vector3 end;
        float edge_length;

        for (int i = 0; i < this.halfedges.Length; ++i) {
            start = vertices[halfedges[i].start];
            end = vertices[halfedges[i].end];
            edge_length = Vector3.Distance(start, end);
            if (edge_length < MIN_EDGE_LENGTH && edge_length > float.Epsilon) { 
                vertices[halfedges[i].start] = vertices[halfedges[i].end];
                RemoveNeighborsForEdge(i);
            }
        }

        RecomputeFacesCenters();
    }

    // Makes the two faces neighboring a given edge not be each other's neighbors.
    // Used in the course of uniting faces.
    private void RemoveNeighborsForEdge(int halfedge_index) {


        HalfEdge he = halfedges[halfedge_index];
        HalfEdge he_twin = halfedges[he.twin_id];
   
        // Check that edge is only edge uniting the two faces.
        int he_face = he.face;
        int he_twin_face = he_twin.face;

        if(he_face < 0 | he_twin_face < 0 )
        {
            return;
        }

        foreach(HalfEdge halfEdge in faces[he_face].half_edges)
        {
            // If (1) the current edge is not the original edge
            // AND (2) the current edge's face is the same as the original edge's face
            // AND (3) the current edge is a "real" edge (i.e. longer than min allowed length),
            // THEN skip removing the neighbor.
            if(halfEdge.id != he.id && halfEdge.face == he_face 
                && Vector3.Distance(vertices[halfEdge.start],vertices[halfEdge.end]) > MIN_EDGE_LENGTH)
            {
                    return;
            }
        }
        
        if (he.face >= 0 && he_twin.face >= 0) {
            faces[he_twin.face].neighbors.Remove(faces[he.face].id);
            faces[he.face].neighbors.Remove(faces[he_twin.face].id);
        }
    }

    // Recomputes the centers of faces.
    private void RecomputeFacesCenters() {
        for (int i = 0; i < faces.Length; ++i) {
            RecomputeFaceCenter(i);
        }
    }

    // Finds the center of a face by averaging the vertices.
    private void RecomputeFaceCenter(int face_index) {
        VoronoiFace f = faces[face_index];

        // Assuming the face is convex, we can just put the center at the 
        // average of the vertices.

        Vector3 new_center = Vector3.zero;
        for (int i = 0; i < f.half_edges.Count; ++i) {
            new_center += vertices[f.half_edges[i].start];
        }
        new_center = new_center / f.half_edges.Count;

        faces[face_index].origin_x = new_center.x;
        faces[face_index].origin_y = new_center.z;
    }

    // Iterate over faces. Identify small ones. Merge with neighbors.
    private void MergeSmallFaces() {
        bool merged_faces = false;
        int iterations = 0;

        do {
            merged_faces = false;
            print("Face merge iteration " + iterations.ToString());
            ++iterations;

            for (int i = 0; i < faces.Length; ++i) {
                //print(i);

                VoronoiFace face_i = faces[i];
                if (face_i.merged) {
                    //print("FACE ALREADY MERGED.");
                    continue;
                }

                //float area = ComputeFaceArea(i);
                float area = face_i.area;

                if (area < MIN_FACE_AREA) {
                    // FIX THIS.
                    if (face_i.neighbors.Count == 0) {
                        //print("NO NEIGHBORS, PROCEEDING.");
                        continue;
                    }

                    int neighbor_to_merge_with = -1;

                    switch(defaultFaceMergingRule)
                    {
                        case FaceMergingRule.SMALLEST_AREA_NEIGHBOR:
                            neighbor_to_merge_with = FindNeighborWithSmallestArea(i);
                            break;
                        case FaceMergingRule.LARGEST_AREA_NEIGHBOR:
                            neighbor_to_merge_with = FindNeighborWithLargestArea(i);
                            break;
                        case FaceMergingRule.LARGEST_EDGE_NEIGHBOR:
                            neighbor_to_merge_with = FindNeighborOnLongestEdge(i);
                            break;
                    }
                    
                    // Merge faces with the neighbor of least area.
                    if (neighbor_to_merge_with >= 0) {
                        print("Merging " + i + ", " + neighbor_to_merge_with);
                        MergeFaces(i, neighbor_to_merge_with);
                        merged_faces = true;
                    }
                }
            }
        } while (merged_faces && iterations < MAX_MERGING_ITERATIONS);

        print("COMPLETED MERGING FACES.");
    }

    // Returns neighbor of face_i that has smallest area.
    private int FindNeighborWithSmallestArea(int face_ind)
    {
        VoronoiFace face_i = faces[face_ind];
        //float area = ComputeFaceArea(i);
        float area = face_i.area;

        if (face_i.neighbors.Count == 0)
        {
            return -1;
        }

        // Iterate over neighbors, find one with smallest area.
        int current_smallest_area_face_index = face_i.neighbors[0];
        //float current_smallest_area = ComputeFaceArea(face_i.neighbors[0]);
        float current_smallest_area = (face_i.neighbors[0] != -1) ? faces[face_i.neighbors[0]].area : -1f;
        for (int j = 1; j < face_i.neighbors.Count; ++j)
        {
            int current_face_index = face_i.neighbors[j];
            //float current_face_area = ComputeFaceArea(face_i.neighbors[j]);
            float current_face_area = (face_i.neighbors[j] != -1) ? faces[face_i.neighbors[j]].area : -1f;

            if (((current_face_area < current_smallest_area || current_smallest_area < 0) && current_face_area > 0))
            {
                current_smallest_area_face_index = current_face_index;
            }
        }

        // Merge faces with the neighbor of least area.
        if (current_smallest_area_face_index >= 0)
        {
            return current_smallest_area_face_index;
        }

        return -1;
    }

    // Returns neighbor of face_i that has largest area.
    private int FindNeighborWithLargestArea(int face_ind)
    {
        VoronoiFace face_i = faces[face_ind];
        //float area = ComputeFaceArea(i);
        float area = face_i.area;

        if (face_i.neighbors.Count == 0)
        {
            return -1;
        }

        // Iterate over neighbors, find one with largest area.
        int current_largest_area_face_index = face_i.neighbors[0];
        float current_largest_area = (face_i.neighbors[0] != -1) ? faces[face_i.neighbors[0]].area : -1f;
        for (int j = 1; j < face_i.neighbors.Count; ++j)
        {
            int current_face_index = face_i.neighbors[j];
            //float current_face_area = ComputeFaceArea(face_i.neighbors[j]);
            float current_face_area = (face_i.neighbors[j] != -1) ? faces[face_i.neighbors[j]].area : -1f;

            if (((current_face_area > current_largest_area || current_largest_area < 0) && current_face_area > 0))
            {
                current_largest_area_face_index = current_face_index;
            }
        }

        // Merge faces with the neighbor of least area.
        if (current_largest_area_face_index >= 0)
        {
            return current_largest_area_face_index;
        }

        return -1;
    }

    // Returns neighbor of face_i that has largest area.
    private int FindNeighborOnLongestEdge(int face_ind)
    {
        VoronoiFace face_i = faces[face_ind];

        if (face_i.neighbors.Count == 0)
        {
            return -1;
        }

        int current_neighbor_on_longest_edge_ind = -1;
        float current_longest_edge_length = -1f;
        foreach(HalfEdge he in face_i.half_edges)
        {
            float len = Vector3.Distance(vertices[he.start], vertices[he.end]);
            if (len > current_longest_edge_length && he.twin_id > -1 && halfedges[he.twin_id].face > -1)
            {
                current_neighbor_on_longest_edge_ind = halfedges[he.twin_id].face;
            }  
        }

        if(current_neighbor_on_longest_edge_ind > -1)
        {
            return current_neighbor_on_longest_edge_ind;
        }

        return -1;
    }

    // Computes face area by iterating over the composing triangles.
    private float ComputeFaceArea(int face_index) {
        if (face_index < 0) {
            return -1;
        }

        VoronoiFace f = faces[face_index];
        float area = 0f;

        for (int i = 0; i < f.half_edges.Count; ++i) {
            HalfEdge he = f.half_edges[i];
            area += TriangleArea(vertices[he.start], vertices[he.end], new Vector3(f.origin_x, 0, f.origin_y));
        }

        return area;
    }

    // Computes a triangle's area.
    private float TriangleArea(Vector3 a, Vector3 b, Vector3 c) {
        return Vector3.Magnitude(Vector3.Cross((b - a), (c - a))) / 2f;
    }

    // Merge mesh of face1 with face2.
    // Assign all edges of face1 to face2.
    // Assign all neighbors of face1 to face2.
    private void MergeFaces(int ind_face1, int ind_face2) {
        Mesh mesh1 = faces[ind_face1].mesh;
        Mesh mesh2 = faces[ind_face2].mesh;

        List<int> triangles = new List<int>();
        List<Vector3> faceVertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();

        // Vertices
        for (int i = 0; i < mesh1.vertices.Length; ++i) {
            faceVertices.Add(mesh1.vertices[i]);
        }
        for (int i = 0; i < mesh2.vertices.Length; ++i) {
            faceVertices.Add(mesh2.vertices[i]);
        }

        // Normals
        for (int i = 0; i < mesh1.normals.Length; ++i) {
            normals.Add(mesh1.normals[i]);
        }
        for (int i = 0; i < mesh2.normals.Length; ++i) {
            normals.Add(mesh2.normals[i]);
        }

        // uvs
        for (int i = 0; i < mesh1.uv.Length; ++i) {
            uvs.Add(mesh1.uv[i]);
        }
        for (int i = 0; i < mesh2.uv.Length; ++i) {
            uvs.Add(mesh2.uv[i]);
        }

        // triangles
        for (int i = 0; i < mesh1.triangles.Length; ++i) {
            triangles.Add(mesh1.triangles[i]);
        }
        for (int i = 0; i < mesh2.triangles.Length; ++i) {
            triangles.Add(mesh1.vertices.Length + mesh2.triangles[i]);
        }

        // Generate new mesh.
        Mesh chunkMesh = new Mesh();
        chunkMesh.vertices = faceVertices.ToArray();
        chunkMesh.uv = uvs.ToArray();
        chunkMesh.triangles = triangles.ToArray();
        chunkMesh.normals = normals.ToArray();

        Transform chunk = Instantiate<Transform>(chunkPrefab, transform.position, transform.rotation);
        chunk.GetComponent<MeshFilter>().mesh = chunkMesh;
        chunk.GetComponent<MeshCollider>().sharedMesh = chunkMesh;
        chunk.transform.parent = transform;
        // Set correct rotation.
        chunk.rotation = new Quaternion(0.707f, 0, 0, -0.707f);
        chunk.gameObject.AddComponent<MapClickDetector>();
        chunk.gameObject.GetComponent<MeshRenderer>().material.color = Color.red;

        // Reassign face1 halfedges.
        for (int i = 0; i < faces[ind_face1].half_edges.Count; ++i) {
            HalfEdge he = faces[ind_face1].half_edges[i];

            //print("Checking half-edge " + he.id + ", belonging to face " + halfedges[he.twin_id].face.ToString());

            if (halfedges[he.twin_id].face != ind_face2) {
                //he.face = ind_face2;
                halfedges[he.id].face = ind_face2;
                faces[ind_face2].half_edges.Add(halfedges[he.id]);
            }
            else {
                //print("Removing half-edge " + he.id.ToString() + "; twin of " + he.twin_id.ToString());
                faces[ind_face2].half_edges.Remove(halfedges[he.twin_id]);
                halfedges[he.id].face = -1;
                halfedges[he.twin_id].face = -1;
            }
        }

        // Reassign face1 neighbors.
        for (int i = 0; i < faces[ind_face1].neighbors.Count; ++i) {

            if (faces[ind_face1].neighbors[i] >= 0) {
                faces[faces[ind_face1].neighbors[i]].neighbors.Remove(ind_face1);
                faces[faces[ind_face1].neighbors[i]].neighbors.Remove(ind_face2);
                faces[faces[ind_face1].neighbors[i]].neighbors.Add(ind_face2);
                faces[ind_face2].neighbors.Remove(faces[ind_face1].neighbors[i]);
                faces[ind_face2].neighbors.Add(faces[ind_face1].neighbors[i]);
            }
        }

        // Remove self-reference that appears twice.
        faces[ind_face2].neighbors.Remove(ind_face2);
        faces[ind_face2].neighbors.Remove(ind_face2);

        Destroy(faces[ind_face1].chunk.gameObject);
        Destroy(faces[ind_face2].chunk.gameObject);

        faces[ind_face2].chunk = chunk;
        faces[ind_face2].mesh = chunkMesh;

        faces[ind_face1].merged = true;
        faces[ind_face2].area += faces[ind_face1].area;
        faces[ind_face1].area = 0f;

        AssignVoronoiFace(faces[ind_face2], chunk.GetComponent<FaceManager>());
    }

    // Generates a sphere object used to draw vertices.
    private void GenerateVertexSpheres() {
        for (int i = 0; i < vertices.Length; ++i) {
            Transform chunk = Instantiate<Transform>(spherePrefab, transform.position, transform.rotation);

            chunk.transform.parent = transform;
            chunk.transform.localPosition = new Vector3(vertices[i].x, vertices[i].z, 0);
        }
    }

    // Generates a sphere object used to draw face centers.
    private void GenerateFaceCenterSpheres() {
        for (int i = 0; i < faces.Length; ++i) {
            Transform chunk = Instantiate<Transform>(spherePrefab, transform.position, transform.rotation);

            chunk.transform.parent = transform;
            chunk.transform.localPosition = new Vector3(faces[i].origin_x, faces[i].origin_y, 0);
            chunk.gameObject.GetComponent<MeshRenderer>().material.color = Color.green;
        }
    }

    // Assigns the VoronoiFace struct to the FaceManager object belonging to the object containing the mesh.
    private void AssignVoronoiFace(VoronoiFace face_struct, FaceManager face_manager) {
        face_manager.id = face_struct.id;
        face_manager.origin_x = face_struct.origin_x;
        face_manager.origin_y = face_struct.origin_y;
        face_manager.half_edges = face_struct.half_edges;
        face_manager.neighbors_ids = face_struct.neighbors;
        face_manager.area = face_struct.area;
        face_manager.merged = face_struct.merged;
    }

    // Populates an array with references to the FaceManager objects.
    // This is so that there exists a central reference to the FaceManager objects.
    private void PopulateFacesManagers() {
        faces_managers = new FaceManager[faces.Length];

        for (int i = 0; i < faces.Length; ++i) {
            if (faces[i].chunk != null) {
                faces_managers[i] = faces[i].chunk.GetComponent<FaceManager>();
            }
        }
    }
}