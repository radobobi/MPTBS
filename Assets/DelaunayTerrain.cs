using System.Collections.Generic;
using UnityEngine;
using TriangleNet.Geometry;
using TriangleNet.Topology;
using TriangleNet.Voronoi;

public class DelaunayTerrain : MonoBehaviour
{
    struct HalfEdge
    {
        public int id;
        public int twin_id;
        public int start;
        public int end;
        public int face;
    }

    struct VoronoiFace
    {
        public int id;
        public float origin_x;
        public float origin_y;
        public List<HalfEdge> half_edges;
        public List<int> neighbors;
    }

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

    // TraingleNet mesh.
    private TriangleNet.Topology.DCEL.DcelMesh mesh2 = null;

    private Vector3[] vertices;
    private HalfEdge[] halfedges;
    private VoronoiFace[] faces;

    void Start()
    {
        Generate();
    }

    public virtual void Generate()
    {
        print("GENERATING TRANGULATION.");

        UnityEngine.Random.InitState(0);

        float[] seed = new float[octaves];

        for (int i = 0; i < octaves; i++)
        {
            seed[i] = Random.Range(0.0f, 100.0f);
        }

        Polygon polygon = new Polygon();
        for (int i = 0; i < randomPoints; i++)
        {
            polygon.Add(new Vertex(Random.Range(0.0f, xsize), Random.Range(0.0f, ysize)));
        }

        TriangleNet.Meshing.ConstraintOptions options = new TriangleNet.Meshing.ConstraintOptions() { ConformingDelaunay = true };
        TriangleNet.Mesh mesh = (TriangleNet.Mesh)polygon.Triangulate(options);
        //mesh2 = (TriangleNet.Topology.DCEL.DcelMesh) (new StandardVoronoi(mesh));
        mesh2 = (TriangleNet.Topology.DCEL.DcelMesh)(new BoundedVoronoi(mesh));

        MakeMesh();
        DrawMesh();
        DrawEdges();
    }

    public void MakeMesh()
    {
        print("CONVERTING TRIANGLENET OUTPUT TO UNITY MESH.");

        TriangleNet.Topology.DCEL.DcelMesh mesh2 = this.mesh2;

        vertices = new Vector3[mesh2.Vertices.Count];

        for (int indver = 0; indver < mesh2.Vertices.Count; ++indver)
        {
            vertices[indver].x = (float) mesh2.Vertices[indver].X;
            vertices[indver].z = (float) mesh2.Vertices[indver].Y;
        }

        halfedges = new HalfEdge[mesh2.HalfEdges.Count];

        for (int indhe = 0; indhe < mesh2.HalfEdges.Count; ++indhe)
        {
            halfedges[indhe].id = mesh2.HalfEdges[indhe].ID;
            halfedges[indhe].twin_id = mesh2.HalfEdges[indhe].Twin.ID;
            halfedges[indhe].start = (int)mesh2.HalfEdges[indhe].Origin.id;
            halfedges[indhe].end = (int)mesh2.HalfEdges[indhe].Twin.Origin.id;
            halfedges[indhe].face = mesh2.HalfEdges[indhe].Face.ID;
        }

        faces = new VoronoiFace[mesh2.Faces.Count];

        for (int indf = 0; indf < mesh2.Faces.Count; ++indf)
        {
            faces[indf].origin_x = (float)mesh2.Faces[indf].generator.X;
            faces[indf].origin_y = (float)mesh2.Faces[indf].generator.Y;
            faces[indf].id = mesh2.Faces[indf].ID;

            faces[indf].half_edges = new List<HalfEdge>();
            faces[indf].neighbors = new List<int>();
            for (int indhe = 0; indhe < halfedges.Length; ++indhe)
            {
                if (halfedges[indhe].face == mesh2.Faces[indf].ID)
                {
                    faces[indf].half_edges.Add(halfedges[indhe]);
                    faces[indf].neighbors.Add(halfedges[halfedges[indhe].twin_id].face);
                }
            }
        }

        print("SUCCESSFULLY GENERATED MESH!");
    }

    private void DrawMesh()
    {
        for (int i = 0; i < faces.Length; ++i)
        {
            VoronoiFace current_face = faces[i];
            List<int> triangles = new List<int>();

            Vector3 faceCenter = new Vector3(current_face.origin_x, 0, current_face.origin_y);
            List<Vector3> faceVertices = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();

            foreach (HalfEdge he in current_face.half_edges)
            {
                Vector3 v0 = faceCenter;
                Vector3 v1 = vertices[he.start];
                Vector3 v2 = vertices[he.end];

                triangles.Add(faceVertices.Count);
                triangles.Add(faceVertices.Count + 1);
                triangles.Add(faceVertices.Count + 2);

                faceVertices.Add(v2);
                faceVertices.Add(v1);
                faceVertices.Add(v0);

                Vector3 normal = Vector3.Cross(v1 - v0, v2 - v0);
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
            chunk.GetComponent<MeshCollider>().sharedMesh = chunkMesh;
            chunk.transform.parent = transform;
            chunk.gameObject.AddComponent<MapClickDetector>();
            chunk.gameObject.GetComponent<MeshRenderer>().material.color = Color.red;
        }
    }

    
    private void DrawEdges()
    {
        for (int i = 0; i < halfedges.Length; ++i)
        {
            Vector3 start = vertices[halfedges[i].start];
            Vector3 end = vertices[halfedges[i].end];
            DrawLine(new Vector3(start.x, start.z, 0), new Vector3(end.x, end.z, 0), Color.blue, transform);
        }
    }

    
    void DrawLine(Vector3 start, Vector3 end, Color color, Transform transform)
    {
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
        //GameObject.Destroy(myLine, duration);
        
        //lr.transform.parent = transform;
    }
    
}
