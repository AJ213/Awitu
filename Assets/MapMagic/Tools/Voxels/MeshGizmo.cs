using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.Profiling;

using Den.Tools.GUI;

namespace Den.Tools.Voxels
{
	public class MeshGizmo
	{
		public Mesh mesh;
		private Material mat;
		private static Texture2D whiteTex;

		public void Draw (Color color, string texName, Transform parent=null) =>
			Draw(color, Resources.Load(texName) as Texture2D, parent);

		public void Draw (Color color, Texture2D texture=null, Transform parent=null)
		{
			if (mesh == null) return;
		
			if (mat == null) mat = new Material( Shader.Find("Standard") ); 

			if (texture==null) 
			{
				if (whiteTex==null) whiteTex = TextureExtensions.ColorTexture(4,4,Color.white);
				texture = whiteTex;
			}	

			mat.SetColor("_Color", color);
			mat.SetTexture("_MainTex", texture);

			mat.SetPass(0);
			Graphics.DrawMeshNow(mesh, parent==null ? Matrix4x4.identity : parent.localToWorldMatrix);
		}


		public void SetMesh (Vector3[] verts, int[] tris)
		{
			if (mesh == null) { mesh = new Mesh(); mesh.MarkDynamic(); }
			if (verts.Length < mesh.vertices.Length) mesh.triangles = new int[0]; //otherwise "The supplied vertex array has less vertices than are referenced by the triangles array."

			mesh.vertices = verts;
			mesh.triangles = tris;

			mesh.RecalculateNormals();
			mesh.RecalculateBounds();
		}


		public void SetMesh (Vector3[] verts, Vector2[] uvs, int[] tris)
		{
			if (mesh == null) { mesh = new Mesh(); mesh.MarkDynamic(); }
			if (verts.Length < mesh.vertices.Length) mesh.triangles = new int[0]; //otherwise "The supplied vertex array has less vertices than are referenced by the triangles array."

			mesh.vertices = verts;
			mesh.triangles = tris;
			mesh.uv = uvs;

			mesh.RecalculateNormals();
			mesh.RecalculateBounds();
		}


		public void Randomize (int seed, float offset)
		{
			if (mesh == null) return;

			UnityEngine.Random.InitState(seed);

			Vector3[] verts = mesh.vertices;
			for (int v=0; v<verts.Length; v++)
				verts[v] += new Vector3(
					(UnityEngine.Random.value-0.5f)*offset, 
					(UnityEngine.Random.value-0.5f)*offset, 
					(UnityEngine.Random.value-0.5f)*offset );
			mesh.vertices = verts;
		}
	}
}
