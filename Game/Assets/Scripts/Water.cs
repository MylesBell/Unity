using UnityEngine;
using System.Collections;

public class Water : MonoBehaviour
{
    public Mesh mesh;
    public float WaveWavelength;
    public float WaveMagnitude;
    public float WaveSpeed;
    
    void Start() {
         mesh=GetComponent<MeshFilter>().mesh;
 
         // unshare verts
         int subMeshCnt=mesh.subMeshCount;
         int[] tris=mesh.triangles;
         int triCnt=mesh.triangles.Length;
         int[] newTris=new int[triCnt];
         Vector3[] sourceVerts = mesh.vertices;
         Vector3[] sourceNorms = mesh.normals;
         Vector2[] sourceUVs = mesh.uv;
 
         Vector3[] newVertices = new Vector3[triCnt];
         Vector3[] newNorms = new Vector3[triCnt];
         Vector2[] newUVs = new Vector2[triCnt];
 
         int offsetVal=0;
 
         for (int k=0; k<subMeshCnt; k++)
         {
             int[] sourceIndices = mesh.GetTriangles(k);
 
             int[] newIndices = new int[sourceIndices.Length];
 
             // Create a unique vertex for every index in the original Mesh:
             for(int i = 0; i < sourceIndices.Length; i++)
             {
                 int newIndex=sourceIndices[i];
                 int iOffset=i+offsetVal;
                 newIndices[i] = iOffset;
                 newVertices[iOffset] = sourceVerts[newIndex];
                 newNorms[iOffset]=sourceNorms[newIndex];
                 newUVs[iOffset] = sourceUVs[newIndex];
             }
             offsetVal+=sourceIndices.Length;
 
             mesh.vertices = newVertices;
             mesh.normals=newNorms;
             mesh.uv = newUVs;
 
             mesh.SetTriangles(newIndices, k);
         }
 
         mesh.RecalculateNormals();
         mesh.RecalculateBounds();
         mesh.Optimize(); 
    }
    void Update() {  
        mesh.vertices = MorphVertices(mesh.vertices);
        mesh.RecalculateNormals();
    }
    
    Vector3[] MorphVertices(Vector3[] vertices) {
        for (int i = 0; i < vertices.Length; i++) {
            // Vector4 v0 = mul(_Object2World, vertex);
			// float phase0 = (_WaveMagnitude)* sin((_Time[1] * _WaveSpeed) + (v0.x * _WaveWavelength) + (v0.z * _WaveWavelength) + rand(v0.xzz));
			// float phase0_1 = (_WaveMagnitude)*sin(cos(rand(v0.xzz) * _WaveMagnitude * cos(_Time[1] * _WaveSpeed * sin(rand(v0.xxz)))));			
			// v0.y += phase0 + phase0_1;
			// v.vertex.xyz = mul(_World2Object, v0);
            
            Vector3 v0 = transform.TransformPoint(vertices[i]);
            Vector3 xzz = new Vector3(v0.x,v0.z,v0.z);
            Vector3 xxz = new Vector3(v0.x,v0.x,v0.z);
            float t = Time.time;
            float phase0 = (WaveMagnitude) * Mathf.Sin((t * WaveSpeed) + (v0.x * WaveWavelength) + (v0.z * WaveWavelength) + RandomFloat(xzz));
            float phase0_1 = (WaveMagnitude) * Mathf.Sin(Mathf.Cos(RandomFloat(xzz) * WaveMagnitude * Mathf.Cos(t * WaveSpeed * Mathf.Sin(RandomFloat(xxz)))));
            
            v0.y += phase0;
            
            vertices[i] = transform.InverseTransformPoint(v0);
        }
        return vertices;   
    }
    
    float RandomFloat(Vector3 co) {
        // return frac(sin( dot(co.xyz ,float3(12.9898,78.233,45.5432) )) * 43758.5453);
        float r = Mathf.Sin(Vector3.Dot(co,new Vector3(12.9898f,78.233f,45.5432f))) * 43758.5453f;
        return r - Mathf.Abs(Mathf.Floor(r));
    }
    
    void RecalculateNormals(Mesh mesh) {
        var triangles = mesh.GetTriangles(0);
        var vertices = mesh.vertices;
        var triNormals = new Vector3[triangles.Length / 3]; //Holds the normal of each triangle
        var normals = new Vector3[vertices.Length];
    }
}

