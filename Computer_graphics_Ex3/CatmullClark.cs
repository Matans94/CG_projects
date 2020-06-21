using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


public class CCMeshData
{
    public List<Vector3> points; // Original mesh points
    public List<Vector4> faces; // Original mesh quad faces
    public List<Vector4> edges; // Original mesh edges
    public List<Vector3> facePoints; // Face points, as described in the Catmull-Clark algorithm
    public List<Vector3> edgePoints; // Edge points, as described in the Catmull-Clark algorithm
    public List<Vector3> newPoints; // New locations of the original mesh points, according to Catmull-Clark
}


public static class CatmullClark
{



    // Returns a QuadMeshData representing the input mesh after one iteration of Catmull-Clark subdivision.
    public static QuadMeshData Subdivide(QuadMeshData quadMeshData)
    // Create and initialize a CCMeshData corresponding to the given QuadMeshData
    {
        CCMeshData meshData = new CCMeshData();
        meshData.points = quadMeshData.vertices;
        meshData.faces = quadMeshData.quads;
        meshData.edges = GetEdges(meshData);
        meshData.facePoints = GetFacePoints(meshData);
        meshData.edgePoints = GetEdgePoints(meshData);
        meshData.newPoints = GetNewPoints(meshData);

        return NextMesh(meshData);
    }


    // this function receives a mesh with its data about the edges, edge points and new point's position
    // it creates a new mesh object that is the resault of a single catmull clarck's subdivide proccess
    private static QuadMeshData NextMesh(CCMeshData mesh)
    {
        List<Vector3> newMeshPoints = mesh.edgePoints;
        newMeshPoints.AddRange(mesh.facePoints);
        newMeshPoints.AddRange(mesh.newPoints);

        List<Vector4> newFaces = new List<Vector4>();

        int n = mesh.edges.Count;
        int k = mesh.facePoints.Count;


        for (int i = 0; i < mesh.faces.Count; i++)
        {
            List<int> faceEdgesIndex = GetFacesEdges(mesh.edges, i);

            Vector4 nf1 = new Vector4(n + i, faceEdgesIndex[0], 0, 0);
            Vector4 nf2 = new Vector4(n + i, faceEdgesIndex[1], 0, 0);
            Vector4 nf3 = new Vector4(n + i, faceEdgesIndex[2], 0, 0);
            Vector4 nf4 = new Vector4(n + i, faceEdgesIndex[3], 0, 0);


            //----------------------------------------------------------------------------------------------------------------------
            // the first edge's points.
            int v1 = (int)mesh.edges[faceEdgesIndex[0]].x;
            int v2 = (int)mesh.edges[faceEdgesIndex[0]].y;

            // i'm looking for the second apearing point out of the two
            if ((mesh.faces[i].x == v1 && mesh.faces[i].y == v2) || (mesh.faces[i].y == v1 && mesh.faces[i].z == v2) ||
                 (mesh.faces[i].z == v1 && mesh.faces[i].w == v2) || (mesh.faces[i].w == v1 && mesh.faces[i].x == v2))
            {
                //v2 after v1 in f
                nf1.z = n + k + v2;
            }
            else // v1 after v2 in f
            {
                nf1.z = n + k + v1;
            }


            int edges1 = EdgesByPointAndFace((int)nf1.z - n - k, i, 0, faceEdgesIndex, mesh.edges);
            nf1.w = faceEdgesIndex[edges1];

            //----------------------------------------------------------------------------------------------------------------------

            // the second edge's points.
            v1 = (int)mesh.edges[faceEdgesIndex[1]].x;
            v2 = (int)mesh.edges[faceEdgesIndex[1]].y;

            // i'm looking for the second apearing point out of the two
            if ((mesh.faces[i].x == v1 && mesh.faces[i].y == v2) || (mesh.faces[i].y == v1 && mesh.faces[i].z == v2) ||
                 (mesh.faces[i].z == v1 && mesh.faces[i].w == v2) || (mesh.faces[i].w == v1 && mesh.faces[i].x == v2))
            {
                //v2 after v1 in f
                nf2.z = n + k + v2;
            }
            else // v1 after v2 in f
            {
                nf2.z = n + k + v1;
            }

            int edges2 = EdgesByPointAndFace((int)nf2.z - n - k, i, 1, faceEdgesIndex, mesh.edges);
            nf2.w = faceEdgesIndex[edges2];


            //----------------------------------------------------------------------------------------------------------------------


            // the third edge's points.
            v1 = (int)mesh.edges[faceEdgesIndex[2]].x;
            v2 = (int)mesh.edges[faceEdgesIndex[2]].y;

            // i'm looking for the second apearing point out of the two
            if ((mesh.faces[i].x == v1 && mesh.faces[i].y == v2) || (mesh.faces[i].y == v1 && mesh.faces[i].z == v2) ||
                 (mesh.faces[i].z == v1 && mesh.faces[i].w == v2) || (mesh.faces[i].w == v1 && mesh.faces[i].x == v2))
            {
                //v2 after v1 in f
                nf3.z = n + k + v2;
            }
            else // v1 after v2 in f
            {
                nf3.z = n + k + v1;
            }


            int edges3 = EdgesByPointAndFace((int)nf3.z - n - k, i, 2, faceEdgesIndex, mesh.edges);
            nf3.w = faceEdgesIndex[edges3];



            //----------------------------------------------------------------------------------------------------------------------

            // the forth edge's points.
            v1 = (int)mesh.edges[faceEdgesIndex[3]].x;
            v2 = (int)mesh.edges[faceEdgesIndex[3]].y;

            // i'm looking for the second apearing point out of the two
            if ((mesh.faces[i].x == v1 && mesh.faces[i].y == v2) || (mesh.faces[i].y == v1 && mesh.faces[i].z == v2) ||
                 (mesh.faces[i].z == v1 && mesh.faces[i].w == v2) || (mesh.faces[i].w == v1 && mesh.faces[i].x == v2))
            {
                //v2 after v1 in f
                nf4.z = n + k + v2;
            }
            else // v1 after v2 in f
            {
                nf4.z = n + k + v1;
            }



            int edges4 = EdgesByPointAndFace((int)nf4.z - n - k, i, 3, faceEdgesIndex, mesh.edges);
            nf4.w = faceEdgesIndex[edges4];

            newFaces.Add(nf1);
            newFaces.Add(nf2);
            newFaces.Add(nf3);
            newFaces.Add(nf4);



        }

        return new QuadMeshData(newMeshPoints, newFaces);

    }


    // this function receives a point index(from mesh.point), a face index (from mesh.faces), the list of the face's edges indexes
    // (from mesh.edges) and the edges list from the mesh.
    // it finds the second endge (the first one is edgeIdx) that shares point and face.
    // returns the edge index from the faceEdgesIdx list.
    private static int EdgesByPointAndFace(int point, int face, int edgeIdx, List<int> faceEdgesIdx, List<Vector4> edges)
    {

        for(int i = 0; i < faceEdgesIdx.Count; i++)
        {
            if((edges[faceEdgesIdx[i]].x == point || edges[faceEdgesIdx[i]].y == point) && (edges[faceEdgesIdx[i]].z == face || edges[faceEdgesIdx[i]].w == face) && i != edgeIdx)
            {
                return i;   
            }
        }

        return -1;
    }


    // this function receives the edges list and a face index from mesh.faces list.
    // it returns a list of all 4 edges that are in the face given
    private static List<int> GetFacesEdges(List<Vector4> edges, int faceIdx)
    {
        List<int> edgesIndex = new List<int>();
        for(int j = 0; j < edges.Count; j++)
        {
            // add all the edges in that face. we know that there must be exactly 4 edges that fit.
            if(edges[j].z == faceIdx || edges[j].w == faceIdx)
            {
                edgesIndex.Add(j);
            }
        }
        return edgesIndex;

    }





    // a comperator for lists of int.
    public class IndexesComparer : EqualityComparer<int[]>
    {
        private List<Vector3> points;

        public IndexesComparer(List<Vector3> OGpoints)
        {
            points = OGpoints;
        }

        private static readonly float EPSILON = 0.00001f;

        // this function receives two lists, each has two integers that represents indexes of points from the points list.
        // it return true if these two lists hold the same two points.
        public override bool Equals(int[] v1, int[] v2)
        {

            if ((Vector3.Distance(points[v1[0]], points[v2[0]]) < EPSILON && Vector3.Distance(points[v1[1]], points[v2[1]]) < EPSILON) ||
                (Vector3.Distance(points[v1[0]], points[v2[1]]) < EPSILON && Vector3.Distance(points[v1[1]], points[v2[0]]) < EPSILON))
            {
                return true;
            }
            return false;
        }

        public override int GetHashCode(int[] v)
        {
            return 0;
        }
    }



    // Returns a list of all edges in the mesh defined by given points and faces.
    // Each edge is represented by Vector4(p1, p2, f1, f2)
    // p1, p2 are the edge vertices
    // f1, f2 are faces incident to the edge. If the edge belongs to one face only, f2 is -1
    public static List<Vector4> GetEdges(CCMeshData mesh)
    {
        IndexesComparer comp = new IndexesComparer(mesh.points);

        Dictionary<int[], int> d = new Dictionary<int[], int>(comp);

        List<Vector4> edges = new List<Vector4>();


        for (int i = 0; i < mesh.faces.Count; i++)
        {
            
            List<int[]> edgesOptions = EdgesOptions(mesh.faces[i]);

            for ( int j = 0; j < edgesOptions.Count; j++)
            {
                // if thoes two points were allready in the dictionary, 
                // that means there is another face that share them. add a new edge to the edges list.
                // the new edge is represented by the two indexes of the shared points, 
                // by the index of the face that was found first and by the current face's index.
                if(d.ContainsKey(edgesOptions[j]))
                {
                    Vector4 newEdge = new Vector4(edgesOptions[j][0], edgesOptions[j][1], d[edgesOptions[j]], i);
                    edges.Add(newEdge);
                }
                else
                {
                    // these two points were not in the dictionary before and that means that no earlier 
                    // face shares those points. add the points with the current face index.
                    d.Add(edgesOptions[j], i);
                }
            }
            
        }

        return edges;
        
    }



    // this function returns all the posible edges there can be from the points of the face given
    private static List<int[]> EdgesOptions(Vector4 face)
    {
        List<int[]> options = new List<int[]>();

        int[] key1 = new int[2];
        key1[0] = (int)face.x;
        key1[1] = (int)face.y;
        options.Add(key1);
        int[] key2 = new int[2];
        key2[0] = (int)face.x;
        key2[1] = (int)face.z;
        options.Add(key2);
        int[] key3 = new int[2];
        key3[0] = (int)face.w;
        key3[1] = (int)face.x;
        options.Add(key3);
        int[] key4 = new int[2];
        key4[0] = (int)face.y;
        key4[1] = (int)face.z;
        options.Add(key4);
        int[] key5 = new int[2];
        key5[0] = (int)face.y;
        key5[1] = (int)face.w;
        options.Add(key5);
        int[] key6 = new int[2];
        key6[0] = (int)face.z;
        key6[1] = (int)face.w;
        options.Add(key6);


        return options;
    }


    // Returns a list of "face points" for the given CCMeshData, as described in the Catmull-Clark algorithm 
    public static List<Vector3> GetFacePoints(CCMeshData mesh)
    {
        List<Vector3> facePointList = new List<Vector3>();

        for(int i = 0; i < mesh.faces.Count; i++)
        {
            Vector3 avg = new Vector3();
            avg = mesh.points[(int)mesh.faces[i].x] + mesh.points[(int)mesh.faces[i].y] + mesh.points[(int)mesh.faces[i].z] + mesh.points[(int)mesh.faces[i].w];
            avg /= 4;
            facePointList.Add(avg);
        }

        return facePointList;
    }

    // Returns a list of "edge points" for the given CCMeshData, as described in the Catmull-Clark algorithm 
    public static List<Vector3> GetEdgePoints(CCMeshData mesh)
    {
        List<Vector3> edgePointsList = new List<Vector3>();

        for(int i = 0; i < mesh.edges.Count; i++)
        {
            Vector3 newEdgePoint = new Vector3();
            newEdgePoint = mesh.points[(int)mesh.edges[i].x] + mesh.points[(int)mesh.edges[i].y] + mesh.facePoints[(int)mesh.edges[i].z] + mesh.facePoints[(int)mesh.edges[i].w];
            newEdgePoint /= 4;
            edgePointsList.Add(newEdgePoint);
        }

        return edgePointsList;

    }

    // this function returns a list of lists.
    // the external list's size equals to the points size
    // every point has a list of all the indexes of edges that share it
    private static List<List<int>> GetPointsData(List<Vector4> edges, int pointCount)
    {
        List<List<int>> pointsData = new List<List<int>>();

        for(int i = 0; i < pointCount; i++)
        {
            List<int> n = new List<int>();
            pointsData.Add(n);
        }

        for(int i = 0; i < edges.Count; i++)
        {
            pointsData[(int)edges[i].x].Add(i);
            pointsData[(int)edges[i].y].Add(i);
        }


        return pointsData;
    }


    // Returns a list of new locations of the original points for the given CCMeshData, as described in the CC algorithm 
    public static List<Vector3> GetNewPoints(CCMeshData mesh)
    {
        List<List<int>> pointsData = GetPointsData(mesh.edges, mesh.points.Count);

        List<Vector3> newPoints = new List<Vector3>();

        // running over the points data list
        for(int i = 0; i < mesh.points.Count; i++)
        {
            
            Vector3 f = new Vector3(0, 0, 0);
            Vector3 r = new Vector3(0, 0, 0);

            // runing over all of the edges that share this point
            for(int j = 0; j < pointsData[i].Count; j++)
            {
                // calculate f of every point
                f += mesh.facePoints[(int)mesh.edges[pointsData[i][j]].z];
                f += mesh.facePoints[(int)mesh.edges[pointsData[i][j]].w];


                // calculate r of every point
                r += (mesh.points[(int)mesh.edges[pointsData[i][j]].x] + mesh.points[(int)mesh.edges[pointsData[i][j]].y]) / 2;


            }

            int n = pointsData[i].Count;

            f /= 2 * n;
            r /= n;

            // calculate p's new position
            newPoints.Add((f + 2 * r + (n - 3) * mesh.points[i]) / n);

        }

        return newPoints;
    }
}
