using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.Profiling;

using Den.Tools.GUI;

namespace Den.Tools.Voxels
{
	public class VoxelMeshGizmo
	{
		public Mesh mesh;

		private Vector3[] vertices;
		private Vector3[] normals;
		private int[] tris;

		private static Material mat;
		private static Texture2D tex;


		public VoxelMeshGizmo ()
		{
			//SetMesh(maxPoints);
		}

		public void SetVoxels (Matrix3D<bool> voxels)
		{
			SetVoxelsThread(voxels);
			SetVoxelsApply();
		}


		public void SetVoxelsThread (Matrix3D<bool> voxels)
		/// Prepares verts and tris in thread
		{
			List<Vector3> verts = new List<Vector3>();
			List<Vector3> normals = new List<Vector3>();
			List<int> tris = new List<int>();

			//bottom/top faces
			for (int x=voxels.cube.offset.x; x<voxels.cube.offset.x+voxels.cube.size.x; x++)
				for (int z=voxels.cube.offset.z; z<voxels.cube.offset.z+voxels.cube.size.z; z++)
			{
				bool prevVal = false;
				for (int y=0; y<voxels.cube.size.y+1; y++)
				{
					bool nextVal = y<voxels.cube.size.y ? voxels[x, y+voxels.cube.offset.y, z] : false;

					if (nextVal && !prevVal)
					{
						int vertsCount = verts.Count;

						tris.Add(vertsCount+1);
						tris.Add(vertsCount+2);
						tris.Add(vertsCount);
						
						tris.Add(vertsCount+3);
						tris.Add(vertsCount);
						tris.Add(vertsCount+2);
						
						verts.Add( new Vector3(x, y+voxels.cube.offset.y, z) );
						verts.Add( new Vector3(x+1, y+voxels.cube.offset.y, z) );
						verts.Add( new Vector3(x+1, y+voxels.cube.offset.y, z+1) );
						verts.Add( new Vector3(x, y+voxels.cube.offset.y, z+1) );

						normals.Add( new Vector3(0,-1,0) );
						normals.Add( new Vector3(0,-1,0) );
						normals.Add( new Vector3(0,-1,0) );
						normals.Add( new Vector3(0,-1,0) );
					}

					if (!nextVal && prevVal)
					{
						int vertsCount = verts.Count;

						tris.Add(vertsCount+1);
						tris.Add(vertsCount+2);
						tris.Add(vertsCount);
						
						tris.Add(vertsCount+3);
						tris.Add(vertsCount);
						tris.Add(vertsCount+2);
						
						verts.Add( new Vector3(x, y+voxels.cube.offset.y, z) );
						verts.Add( new Vector3(x, y+voxels.cube.offset.y, z+1) );
						verts.Add( new Vector3(x+1, y+voxels.cube.offset.y, z+1) );
						verts.Add( new Vector3(x+1, y+voxels.cube.offset.y, z) );

						normals.Add( new Vector3(0,1,0) );
						normals.Add( new Vector3(0,1,0) );
						normals.Add( new Vector3(0,1,0) );
						normals.Add( new Vector3(0,1,0) );
					}

					prevVal = nextVal;
				}
			}

			//left/right faces
			for (int x=voxels.cube.offset.x; x<voxels.cube.offset.x+voxels.cube.size.x; x++)
				for (int y=voxels.cube.offset.y; y<voxels.cube.offset.y+voxels.cube.size.y; y++)
			{
				bool prevVal = false;
				for (int z=0; z<voxels.cube.size.z+1; z++)
				{
					bool nextVal = z<voxels.cube.size.z ? voxels[x, y, z+voxels.cube.offset.z] : false;

					if (nextVal && !prevVal)
					{
						int vertsCount = verts.Count;

						tris.Add(vertsCount+1);
						tris.Add(vertsCount+2);
						tris.Add(vertsCount);
						
						tris.Add(vertsCount+3);
						tris.Add(vertsCount);
						tris.Add(vertsCount+2);
						
						verts.Add( new Vector3(x, y, z+voxels.cube.offset.z) );
						verts.Add( new Vector3(x, y+1, z+voxels.cube.offset.z) );
						verts.Add( new Vector3(x+1, y+1, z+voxels.cube.offset.z) );
						verts.Add( new Vector3(x+1, y, z+voxels.cube.offset.z) );

						normals.Add( new Vector3(0,0,1) );
						normals.Add( new Vector3(0,0,1) );
						normals.Add( new Vector3(0,0,1) );
						normals.Add( new Vector3(0,0,1) );
					}

					if (!nextVal && prevVal)
					{
						int vertsCount = verts.Count;

						tris.Add(vertsCount+1);
						tris.Add(vertsCount+2);
						tris.Add(vertsCount);
						
						tris.Add(vertsCount+3);
						tris.Add(vertsCount);
						tris.Add(vertsCount+2);
						
						verts.Add( new Vector3(x, y, z+voxels.cube.offset.z) );
						verts.Add( new Vector3(x+1, y, z+voxels.cube.offset.z) );
						verts.Add( new Vector3(x+1, y+1, z+voxels.cube.offset.z) );
						verts.Add( new Vector3(x, y+1, z+voxels.cube.offset.z) );

						normals.Add( new Vector3(0,0,-1) );
						normals.Add( new Vector3(0,0,-1) );
						normals.Add( new Vector3(0,0,-1) );
						normals.Add( new Vector3(0,0,-1) );
					}

					prevVal = nextVal;
				}
			}

			//left/right faces
			for (int y=voxels.cube.offset.y; y<voxels.cube.offset.y+voxels.cube.size.y; y++)
				for (int z=voxels.cube.offset.z; z<voxels.cube.offset.z+voxels.cube.size.z; z++)
			{
				bool prevVal = false;
				for (int x=0; x<voxels.cube.size.x+1; x++)
				{
					bool nextVal = x<voxels.cube.size.x ? voxels[x+voxels.cube.offset.x, y, z] : false;

					if (nextVal && !prevVal)
					{
						int vertsCount = verts.Count;

						tris.Add(vertsCount+1);
						tris.Add(vertsCount+2);
						tris.Add(vertsCount);
						
						tris.Add(vertsCount+3);
						tris.Add(vertsCount);
						tris.Add(vertsCount+2);
						
						verts.Add( new Vector3(x+voxels.cube.offset.x, y, z) );
						verts.Add( new Vector3(x+voxels.cube.offset.x, y, z+1) );
						verts.Add( new Vector3(x+voxels.cube.offset.x, y+1, z+1) );
						verts.Add( new Vector3(x+voxels.cube.offset.x, y+1, z) );

						normals.Add( new Vector3(1,0,0) );
						normals.Add( new Vector3(1,0,0) );
						normals.Add( new Vector3(1,0,0) );
						normals.Add( new Vector3(1,0,0) );
					}

					if (!nextVal && prevVal)
					{
						int vertsCount = verts.Count;

						tris.Add(vertsCount+1);
						tris.Add(vertsCount+2);
						tris.Add(vertsCount);
						
						tris.Add(vertsCount+3);
						tris.Add(vertsCount);
						tris.Add(vertsCount+2);
						
						verts.Add( new Vector3(x+voxels.cube.offset.x, y, z) );
						verts.Add( new Vector3(x+voxels.cube.offset.x, y+1, z) );
						verts.Add( new Vector3(x+voxels.cube.offset.x, y+1, z+1) );
						verts.Add( new Vector3(x+voxels.cube.offset.x, y, z+1) );

						normals.Add( new Vector3(-1,0,0) );
						normals.Add( new Vector3(-1,0,0) );
						normals.Add( new Vector3(-1,0,0) );
						normals.Add( new Vector3(-1,0,0) );
					}

					prevVal = nextVal;
				}
			}

			vertices = verts.ToArray();
			this.tris = tris.ToArray();
			this.normals = normals.ToArray();
		}


		public void SetVoxelsApply ()
		/// Applies SetVoxelsThread
		{
			if (mesh == null) { mesh = new Mesh(); mesh.MarkDynamic(); }
			if (vertices.Length < mesh.vertices.Length) mesh.triangles = new int[0]; //otherwise "The supplied vertex array has less vertices than are referenced by the triangles array."

			mesh.vertices = vertices;
			mesh.normals = normals;
			mesh.triangles = tris;

			mesh.RecalculateNormals();
		}


		public void Draw (Color color, Transform parent=null)
		/// Draws a line with the points previously set
		{
			if (mat == null) mat = new Material( Shader.Find("Legacy Shaders/VertexLit") ); 
			if (tex == null) tex = Resources.Load("DPUI/PolyLineTex") as Texture2D; 

			mat.SetColor("_Color", color);

			mat.SetPass(0);
			Graphics.DrawMeshNow(mesh, parent==null ? Matrix4x4.identity : parent.localToWorldMatrix);
		}
	}
}
