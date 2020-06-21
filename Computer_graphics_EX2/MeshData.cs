using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;


public class MeshData
{
    public List<Vector3> vertices; // The vertices of the mesh 
    public List<int> triangles; // Indices of vertices that make up the mesh faces
    public Vector3[] normals; // The normals of the mesh, one per vertex

    struct Surface
    {
        public Vector3 p1, p2, p3, normal;

        public Surface(Vector3 _p1, Vector3 _p2, Vector3 _p3)
        {
            p1 = _p1;
            p2 = _p2;
            p3 = _p3;
            normal = Vector3.Cross(p1 - p3, p2 - p3).normalized;
        }
    };

    public MeshData()
    {
        vertices = new List<Vector3>();
        triangles = new List<int>();
    }

    // Returns a Unity Mesh of this MeshData that can be rendered
    public Mesh ToUnityMesh()
    {
        Mesh mesh = new Mesh
        {
            vertices = vertices.ToArray(),
            triangles = triangles.ToArray(),
            normals = normals
        };

        return mesh;
    }

    // Calculates surface normals for each vertex, according to face orientation
    public void CalculateNormals()
    {
        normals = new Vector3[vertices.Count];
        List<Surface> surfaceList = new List<Surface>();

        for (int i = 0; i < triangles.Count; i += 3)
        {
            Surface newSurface = new Surface(vertices[triangles[i]],
                                            vertices[triangles[i + 1]],
                                            vertices[triangles[i + 2]]);
            normals[triangles[i]] += newSurface.normal;
            normals[triangles[i+1]] += newSurface.normal;
            normals[triangles[i+2]] += newSurface.normal;

            surfaceList.Add(newSurface);
        }

        for (int i =  0; i < normals.Length; i++)
        {
            normals[i] = normals[i].normalized;
        }
    }

    // Edits mesh such that each face has a unique set of 3 vertices
    public void MakeFlatShaded()
    {
        List<Vector3> newVerticesList = new List<Vector3>(); 
        List<int> newTrianglesList = new List<int>();

        for (int i = 0; i < triangles.Count; i++)
        {
            newVerticesList.Add(vertices[triangles[i]]);
            newTrianglesList.Add(i);
        }
        triangles = newTrianglesList;
        vertices = newVerticesList;


    }
}