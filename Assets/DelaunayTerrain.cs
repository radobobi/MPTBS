using System.Collections.Generic;
using UnityEngine;
using TriangleNet.Geometry;
using TriangleNet.Topology;
using TriangleNet.Voronoi;

public class DelaunayTerrain : MonoBehaviour
{
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

    // Elevations at each point in the mesh
    private List<float> elevations;

    // The delaunay mesh
    private TriangleNet.Mesh mesh = null;
    private TriangleNet.Topology.DCEL.DcelMesh mesh2 = null;

    struct HalfEdge
    {
        public int start;
        public int end;
        public string face;
    }

    struct VoronoiFace
    {
        public float origin_x;
        public float origin_y;
        public List<HalfEdge> half_edges;
    }

    void Start()
    {
        Generate();
    }

    public virtual void Generate()
    {
        print("GENERATING TRANGULATION.");

        UnityEngine.Random.InitState(0);

        elevations = new List<float>();

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
        mesh = (TriangleNet.Mesh)polygon.Triangulate(options);
        mesh2 = (TriangleNet.Topology.DCEL.DcelMesh) (new StandardVoronoi(mesh));

        // Sample perlin noise to get elevations
        foreach (Vertex vert in mesh.Vertices)
        {
            float elevation = 0.0f;
            
            /*
            float amplitude = Mathf.Pow(persistence, octaves);
            float frequency = 1.0f;
            float maxVal = 0.0f;

            for (int o = 0; o < octaves; o++)
            {
                float sample = (Mathf.PerlinNoise(seed[o] + (float)vert.x * sampleSize / (float)xsize * frequency,
                                                  seed[o] + (float)vert.y * sampleSize / (float)ysize * frequency) - 0.5f) * amplitude;
                elevation += sample;
                maxVal += amplitude;
                amplitude /= persistence;
                frequency *= frequencyBase;
            }

            elevation = elevation / maxVal;*/
            elevations.Add(elevation);
        }

        //MakeMesh();
        MakeMesh2();
    }


    public void MakeMesh()
    {
        print("MAKING TRIANGLES MESH.");

        IEnumerator<Triangle> triangleEnumerator = mesh.Triangles.GetEnumerator();

        for (int chunkStart = 0; chunkStart < mesh.Triangles.Count; chunkStart += trianglesInChunk)
        {
            List<int> triangles = new List<int>();

            int chunkEnd = chunkStart + trianglesInChunk;
            for (int i = chunkStart; i < chunkEnd; i++)
            {
                if (!triangleEnumerator.MoveNext())
                {
                    break;
                }

                Triangle triangle = triangleEnumerator.Current;

                List<Vector3> vertices = new List<Vector3>();
                List<Vector3> normals = new List<Vector3>();
                List<Vector2> uvs = new List<Vector2>();

                // For the triangles to be right-side up, they need
                // to be wound in the opposite direction
                Vector3 v0 = GetPoint3D(triangle.vertices[2].id);
                Vector3 v1 = GetPoint3D(triangle.vertices[1].id);
                Vector3 v2 = GetPoint3D(triangle.vertices[0].id);

                triangles.Add(vertices.Count);
                triangles.Add(vertices.Count + 1);
                triangles.Add(vertices.Count + 2);

                vertices.Add(v0);
                vertices.Add(v1);
                vertices.Add(v2);

                Vector3 normal = Vector3.Cross(v1 - v0, v2 - v0);
                normals.Add(normal);
                normals.Add(normal);
                normals.Add(normal);

                uvs.Add(new Vector2(0.0f, 0.0f));
                uvs.Add(new Vector2(0.0f, 0.0f));
                uvs.Add(new Vector2(0.0f, 0.0f));

                Mesh chunkMesh = new Mesh();
                chunkMesh.vertices = vertices.ToArray();
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
    }

    public void MakeMesh2()
    {
        print("MAKING VORONOI MESH.");

        TriangleNet.Topology.DCEL.DcelMesh mesh2 = this.mesh2;

        Vector3[] vertices = new Vector3[mesh2.Vertices.Count];

        for (int indver = 0; indver < mesh2.Vertices.Count; ++indver)
        {
            print(indver);
            //vertices[indver] = GetPoint3D(mesh2.Vertices[indver].id);
            vertices[indver].x = (float) mesh2.Vertices[indver].X;
            vertices[indver].z = (float) mesh2.Vertices[indver].Y;
            print(vertices[indver]);
        }

        HalfEdge[] halfedges = new HalfEdge[mesh2.HalfEdges.Count];

        for (int indhe = 0; indhe < mesh2.HalfEdges.Count; ++indhe)
        {
            print(indhe);
            halfedges[indhe].start = (int)mesh2.HalfEdges[indhe].Origin.id;
            halfedges[indhe].end = (int)mesh2.HalfEdges[indhe].Twin.Origin.id;
            halfedges[indhe].face = (string)mesh2.HalfEdges[indhe].Face.ToString();
        }

        VoronoiFace[] faces = new VoronoiFace[mesh2.Faces.Count];

        for (int indf = 0; indf < mesh2.Faces.Count; ++indf)
        {
            print(indf);
            faces[indf].origin_x = (float)mesh2.Faces[indf].generator.X;
            faces[indf].origin_y = (float)mesh2.Faces[indf].generator.Y;

            faces[indf].half_edges = new List<HalfEdge>();
            for (int indhe = 0; indhe < halfedges.Length; ++indhe)
            {
                if (halfedges[indhe].face.Equals(mesh2.Faces[indf].ToString()))
                {
                    faces[indf].half_edges.Add(halfedges[indhe]);
                }
            }
        }

        for (int i = 0; i < faces.Length; ++i)
        {
            VoronoiFace current_face = faces[i];
            List<int> triangles = new List<int>();

            Vector3 faceCenter = new Vector3(current_face.origin_x, 0, current_face.origin_y);
            List<Vector3> faceVertices = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();

            //int chunkEnd = chunkStart + trianglesInChunk;
            foreach (HalfEdge he in current_face.half_edges)
            {
                Vector3 v0 = faceCenter;
                Vector3 v1 = vertices[he.start];
                Vector3 v2 = vertices[he.end];

                triangles.Add(faceVertices.Count);
                triangles.Add(faceVertices.Count+1);
                triangles.Add(faceVertices.Count+2);

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

    // Equivalent to calling new Vector3(GetPointLocation(i).x, GetElevation(i), GetPointLocation(i).y)
    public Vector3 GetPoint3D(int index)
    {
        Vertex vertex = mesh.vertices[index];
        //float elevation = elevations[index];
        return new Vector3((float)vertex.x, 0, (float)vertex.y);
    }

    /*
    public void OnDrawGizmos()
    {
        if (mesh == null)
        {
            // Probably in the editor
            return;
        }

        Gizmos.color = Color.red;
        foreach (Edge edge in mesh.Edges)
        {
            Vertex v0 = mesh.vertices[edge.P0];
            Vertex v1 = mesh.vertices[edge.P1];
            Vector3 p0 = new Vector3((float)v0.x, 0.0f, (float)v0.y);
            Vector3 p1 = new Vector3((float)v1.x, 0.0f, (float)v1.y);
            Gizmos.DrawLine(p0, p1);
        }
        
    }
    */
}