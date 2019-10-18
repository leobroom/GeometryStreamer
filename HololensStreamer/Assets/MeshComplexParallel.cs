using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using System;

public class MeshComplexParallel : MonoBehaviour
{
    NativeArray<Vector3> nativeVertices;

    Vector3[] m_ModifiedVertices;

    MeshModJob m_MeshModJob;
    JobHandle m_JobHandle;

    NativeArray<float> setVertices;

    Mesh mesh;

    internal void Initialize(int length, int[] triangles)
    {
        var filter = gameObject.GetComponent<MeshFilter>();

            filter.mesh = new Mesh();

        mesh = filter.mesh;

        mesh.vertices = new Vector3[length];
        mesh.triangles = triangles;
        mesh = filter.mesh;
        mesh.MarkDynamic();

        nativeVertices = new NativeArray<Vector3>(mesh.vertices, Allocator.Persistent);

        m_ModifiedVertices = new Vector3[nativeVertices.Length];

        setVertices = new NativeArray<float>(nativeVertices.Length * 3, Allocator.Persistent);
    }

    struct MeshModJob : IJobParallelFor
    {
        public NativeArray<Vector3> vertices;
        [NativeDisableParallelForRestriction]
        public NativeArray<float> newVertices;

        public void Execute(int i)
        {
            var vertex = vertices[i];
            int idx = i * 3;

            vertex = new Vector3(newVertices[idx], newVertices[idx +2], newVertices[idx + 1]);
            vertices[i] = vertex;
        }
    }

    public void UpdateMesh(float[] verts)
    {
        setVertices.CopyFrom(verts);

        m_MeshModJob = new MeshModJob()
        {
            vertices = nativeVertices,
            newVertices = setVertices
        };

        m_JobHandle = m_MeshModJob.Schedule(nativeVertices.Length, 64);
    }

    public void LateUpdate()
    {
        m_JobHandle.Complete();


        // copy our results to managed arrays so we can assign them
        m_MeshModJob.vertices.CopyTo(m_ModifiedVertices);

        mesh.vertices = m_ModifiedVertices;
        mesh.RecalculateNormals();
    }

    private void OnDestroy()
    {
        // make sure to Dispose() any NativeArrays when we're done
        nativeVertices.Dispose();
        setVertices.Dispose();
    }
}