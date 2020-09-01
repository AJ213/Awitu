using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.Profiling;

using Den.Tools.GUI;

namespace Den.Tools.Voxels
{
	public static class VoxelMeshOps
	{
		public static (Vector3[] verts, Vector2[] uvs, int[] tris) GenerateMesh (Matrix3D<bool> voxels)
		{
			(Vector3[] verts, Face[] faces) = CreateVertsFaces(voxels);
			List<(int,int)> weldLinks = CreateWeldLinks(faces);
			JoinWeldLinks(ref verts, faces, weldLinks);

			if (FindOpenEdges(faces) != 0)
				Debug.LogError("Open edges detected: " + FindOpenEdges(faces));

			if (FindIsolatedVerts(faces) != 0)
				Debug.LogError("Isolated Verts detected: " + FindIsolatedVerts(faces));

			Vector2[] uvs = MakeUvs(verts, faces);
			int[] tris = MakeTris(faces);
			
			return (verts, uvs, tris);
		}


		public static int[] MakeTris (Face[] faces)
		{
			int[] tris = new int[ faces.Length * 6 ];
			int counter = 0;
			for (int f=0; f<faces.Length; f++)
			{ 
				faces[f].AddTris(tris, counter); 
				counter+=6; 
			}

			return tris;
		}


		public static int[] MakeQuads (Face[] faces)
		{
			int[] tris = new int[ faces.Length * 4 ];
			int counter = 0;
			for (int f=0; f<faces.Length; f++)
			{ 
				faces[f].AddQuads(tris, counter); 
				counter+=4; 
			}

			return tris;
		}


		public static Vector2[] MakeUvs (Vector3[] verts, Face[] faces)
		{
			Vector2[] uvs = new Vector2[verts.Length];
			int counter = 0;
			for (int f=0; f<faces.Length; f++)
			{ 
				faces[f].GenerateUvs(uvs);
				counter+=4; 
			}
			return uvs;
		}



		#region Creating Faces

			public static (Vector3[] verts, Face[] faces) CreateVertsFaces (Matrix3D<bool> voxels)
			{
				List<Vector3> verts = new List<Vector3>();
				List<Face> faces = new List<Face>();

				//top/bottom faces
				for (int x=voxels.cube.offset.x; x<voxels.cube.offset.x+voxels.cube.size.x; x++)
					for (int z=voxels.cube.offset.z; z<voxels.cube.offset.z+voxels.cube.size.z; z++)
				{
					bool prevVal = false;
					for (int y=0; y<voxels.cube.size.y+1; y++)
					{
						bool nextVal = y<voxels.cube.size.y ? voxels[x, y+voxels.cube.offset.y, z] : false;

						if (nextVal && !prevVal)
						{
							faces.Add( new Face(verts.Count, new Coord3D(x, y+voxels.cube.offset.y, z), new Dir(0,-1,0)) );
						
							verts.Add( new Vector3(x, y+voxels.cube.offset.y, z+1) );
							verts.Add( new Vector3(x, y+voxels.cube.offset.y, z) );
							verts.Add( new Vector3(x+1, y+voxels.cube.offset.y, z) );
							verts.Add( new Vector3(x+1, y+voxels.cube.offset.y, z+1) );
						
						}

						if (!nextVal && prevVal)
						{
							faces.Add( new Face(verts.Count, new Coord3D(x, y+voxels.cube.offset.y-1, z), new Dir(0,1,0)) );
						
							verts.Add( new Vector3(x, y+voxels.cube.offset.y, z) );
							verts.Add( new Vector3(x, y+voxels.cube.offset.y, z+1) );
							verts.Add( new Vector3(x+1, y+voxels.cube.offset.y, z+1) );
							verts.Add( new Vector3(x+1, y+voxels.cube.offset.y, z) );
						}

						prevVal = nextVal;
					}
				}


				//front/back faces
				for (int x=voxels.cube.offset.x; x<voxels.cube.offset.x+voxels.cube.size.x; x++)
					for (int y=voxels.cube.offset.y; y<voxels.cube.offset.y+voxels.cube.size.y; y++)
				{
					bool prevVal = false;
					for (int z=0; z<voxels.cube.size.z+1; z++)
					{
						bool nextVal = z<voxels.cube.size.z ? voxels[x, y, z+voxels.cube.offset.z] : false;

						if (nextVal && !prevVal)
						{
							faces.Add( new Face(verts.Count, new Coord3D(x, y, z+voxels.cube.offset.z), new Dir(0,0,-1)) );
						
							verts.Add( new Vector3(x, y, z+voxels.cube.offset.z) );
							verts.Add( new Vector3(x, y+1, z+voxels.cube.offset.z) );
							verts.Add( new Vector3(x+1, y+1, z+voxels.cube.offset.z) );
							verts.Add( new Vector3(x+1, y, z+voxels.cube.offset.z) );
						}

						if (!nextVal && prevVal)
						{
							faces.Add( new Face(verts.Count, new Coord3D(x, y, z+voxels.cube.offset.z-1), new Dir(0,0,1)) );

							verts.Add( new Vector3(x+1, y, z+voxels.cube.offset.z) );
							verts.Add( new Vector3(x+1, y+1, z+voxels.cube.offset.z) );
							verts.Add( new Vector3(x, y+1, z+voxels.cube.offset.z) );
							verts.Add( new Vector3(x, y, z+voxels.cube.offset.z) );
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
							faces.Add( new Face(verts.Count, new Coord3D(x+voxels.cube.offset.x, y, z), new Dir(-1,0,0)) );
						
							verts.Add( new Vector3(x+voxels.cube.offset.x, y, z+1) );
							verts.Add( new Vector3(x+voxels.cube.offset.x, y+1, z+1) );
							verts.Add( new Vector3(x+voxels.cube.offset.x, y+1, z) );
							verts.Add( new Vector3(x+voxels.cube.offset.x, y, z) );
						}

						if (!nextVal && prevVal)
						{
							faces.Add( new Face(verts.Count, new Coord3D(x+voxels.cube.offset.x-1, y, z), new Dir(1,0,0)) );
						
							verts.Add( new Vector3(x+voxels.cube.offset.x, y, z) );
							verts.Add( new Vector3(x+voxels.cube.offset.x, y+1, z) );
							verts.Add( new Vector3(x+voxels.cube.offset.x, y+1, z+1) );
							verts.Add( new Vector3(x+voxels.cube.offset.x, y, z+1) );
						}

						prevVal = nextVal;
					}
				}

				return (verts.ToArray(), faces.ToArray());
			}

		#endregion

		#region Welding

			public static List<(int,int)> CreateWeldLinks (Face[] faces)
			{
				List<(int,int)> weldLinks = new List<(int, int)>(capacity:faces.Length*4);

				//creating a coord-to-faceNum lut of directional faces
				Dictionary<Coord3D,int> topFaces = new Dictionary<Coord3D,int>();
				Dictionary<Coord3D,int> bottomFaces = new Dictionary<Coord3D,int>();
				Dictionary<Coord3D,int> leftFaces = new Dictionary<Coord3D,int>();
				Dictionary<Coord3D,int> rightFaces = new Dictionary<Coord3D,int>();
				Dictionary<Coord3D,int> frontFaces = new Dictionary<Coord3D,int>();
				Dictionary<Coord3D,int> backFaces = new Dictionary<Coord3D,int>();

				for (int f=0; f<faces.Length; f++)
				{
					switch (faces[f].dir.val)
					{
						case 0b_00_010_000: topFaces.Add(faces[f].coord, f); break;
						case 0b_00_000_010: bottomFaces.Add(faces[f].coord, f); break;			
						case 0b_00_000_100: leftFaces.Add(faces[f].coord, f); break;
						case 0b_00_100_000: rightFaces.Add(faces[f].coord, f); break;
						case 0b_00_001_000: frontFaces.Add(faces[f].coord, f); break;
						case 0b_00_000_001: backFaces.Add(faces[f].coord, f); break;
						default: 
							throw new Exception("Unknown Direction on welding faces");
					}
				}

				//top 
				foreach (KeyValuePair<Coord3D,int> kvp in topFaces)
				{
					Coord3D coord = kvp.Key;
					int f = kvp.Value;

					{	if (frontFaces.TryGetValue(coord, out int cf))								{ weldLinks.Add((faces[cf].v2, faces[f].v1)); weldLinks.Add((faces[cf].v1, faces[f].v2)); }
						else if (topFaces.TryGetValue(coord+Coord3D.front, out int nf))				{ weldLinks.Add((faces[nf].v0, faces[f].v1)); weldLinks.Add((faces[nf].v3, faces[f].v2)); }
						else if (backFaces.TryGetValue(coord+Coord3D.front+Coord3D.up, out int of))	{ weldLinks.Add((faces[of].v0, faces[f].v1)); weldLinks.Add((faces[of].v3, faces[f].v2)); } }

					{	if (backFaces.TryGetValue(coord, out int cf))								{ weldLinks.Add((faces[cf].v1, faces[f].v0)); weldLinks.Add((faces[cf].v2, faces[f].v3)); }
						else if (topFaces.TryGetValue(coord+Coord3D.back, out int nf))				{ weldLinks.Add((faces[nf].v1, faces[f].v0)); weldLinks.Add((faces[nf].v2, faces[f].v3)); }
						else if (frontFaces.TryGetValue(coord+Coord3D.back+Coord3D.up, out int of)) { weldLinks.Add((faces[of].v3, faces[f].v0)); weldLinks.Add((faces[of].v0, faces[f].v3)); } }

					{	if (leftFaces.TryGetValue(coord, out int cf))								{ weldLinks.Add((faces[cf].v2, faces[f].v0)); weldLinks.Add((faces[cf].v1, faces[f].v1)); }
						else if (topFaces.TryGetValue(coord+Coord3D.left, out int nf))				{ weldLinks.Add((faces[nf].v3, faces[f].v0)); weldLinks.Add((faces[nf].v2, faces[f].v1)); }
						else if (rightFaces.TryGetValue(coord+Coord3D.up+Coord3D.left, out int of)) { weldLinks.Add((faces[of].v0, faces[f].v0)); weldLinks.Add((faces[of].v3, faces[f].v1)); }
					}

					{	if (rightFaces.TryGetValue(coord, out int cf))								{ weldLinks.Add((faces[cf].v2, faces[f].v2)); weldLinks.Add((faces[cf].v1, faces[f].v3)); }
						else if (topFaces.TryGetValue(coord+Coord3D.right, out int nf))				{ weldLinks.Add((faces[nf].v1, faces[f].v2)); weldLinks.Add((faces[nf].v0, faces[f].v3)); }
						else if (leftFaces.TryGetValue(coord+Coord3D.up+Coord3D.right, out int of)) { weldLinks.Add((faces[of].v0, faces[f].v2)); weldLinks.Add((faces[of].v3, faces[f].v3)); } }
				}

				//bottom
				foreach (KeyValuePair<Coord3D,int> kvp in bottomFaces)
				{
					Coord3D coord = kvp.Key;
					int f = kvp.Value;

					{	if (frontFaces.TryGetValue(coord, out int cf))								{ weldLinks.Add((faces[cf].v3, faces[f].v0)); weldLinks.Add((faces[cf].v0, faces[f].v3)); }
						else if (bottomFaces.TryGetValue(coord+Coord3D.front, out int nf))			{ weldLinks.Add((faces[nf].v1, faces[f].v0)); weldLinks.Add((faces[nf].v2, faces[f].v3)); }
						else if (backFaces.TryGetValue(coord+Coord3D.down+Coord3D.front, out int of)){weldLinks.Add((faces[of].v1, faces[f].v0)); weldLinks.Add((faces[of].v2, faces[f].v3)); } }

					{	if (backFaces.TryGetValue(coord, out int cf))								{ weldLinks.Add((faces[cf].v0, faces[f].v1)); weldLinks.Add((faces[cf].v3, faces[f].v2)); }
						else if (bottomFaces.TryGetValue(coord+Coord3D.back, out int nf))			{ weldLinks.Add((faces[nf].v0, faces[f].v1)); weldLinks.Add((faces[nf].v3, faces[f].v2)); }
						else if (frontFaces.TryGetValue(coord+Coord3D.down+Coord3D.back, out int of)){weldLinks.Add((faces[of].v2, faces[f].v1)); weldLinks.Add((faces[of].v1, faces[f].v2)); } }

					{	if (leftFaces.TryGetValue(coord, out int cf))								{ weldLinks.Add((faces[cf].v0, faces[f].v0)); weldLinks.Add((faces[cf].v3, faces[f].v1)); }
						else if (bottomFaces.TryGetValue(coord+Coord3D.left, out int nf))			{ weldLinks.Add((faces[nf].v3, faces[f].v0)); weldLinks.Add((faces[nf].v2, faces[f].v1)); }
						else if (rightFaces.TryGetValue(coord+Coord3D.down+Coord3D.left, out int of)){weldLinks.Add((faces[of].v2, faces[f].v0)); weldLinks.Add((faces[of].v1, faces[f].v1)); } }

					{	if (rightFaces.TryGetValue(coord, out int cf))								{ weldLinks.Add((faces[cf].v0, faces[f].v2)); weldLinks.Add((faces[cf].v3, faces[f].v3)); }
						else if (bottomFaces.TryGetValue(coord+Coord3D.right, out int nf))			{ weldLinks.Add((faces[nf].v1, faces[f].v2)); weldLinks.Add((faces[nf].v0, faces[f].v3)); }
						else if (leftFaces.TryGetValue(coord+Coord3D.down+Coord3D.right, out int of)){weldLinks.Add((faces[of].v2, faces[f].v2)); weldLinks.Add((faces[of].v1, faces[f].v3)); } }
				}

				//front
				foreach (KeyValuePair<Coord3D,int> kvp in frontFaces)
				{
					Coord3D coord = kvp.Key;
					int f = kvp.Value;

					{	if (topFaces.TryGetValue(coord, out int cf))								{ weldLinks.Add((faces[cf].v2, faces[f].v1)); weldLinks.Add((faces[cf].v1, faces[f].v2)); }
						else if (frontFaces.TryGetValue(coord+Coord3D.up, out int nf))				{ weldLinks.Add((faces[nf].v0, faces[f].v1)); weldLinks.Add((faces[nf].v3, faces[f].v2)); }
						else if (bottomFaces.TryGetValue(coord+Coord3D.front+Coord3D.up, out int of)){weldLinks.Add((faces[of].v2, faces[f].v1)); weldLinks.Add((faces[of].v1, faces[f].v2)); } }

					{	if (bottomFaces.TryGetValue(coord, out int cf))								{ weldLinks.Add((faces[cf].v3, faces[f].v0)); weldLinks.Add((faces[cf].v0, faces[f].v3)); }
						else if (frontFaces.TryGetValue(coord+Coord3D.down, out int nf))			{ weldLinks.Add((faces[nf].v1, faces[f].v0)); weldLinks.Add((faces[nf].v2, faces[f].v3)); }
						else if (topFaces.TryGetValue(coord+Coord3D.front+Coord3D.down, out int of)) {weldLinks.Add((faces[of].v3, faces[f].v0)); weldLinks.Add((faces[of].v0, faces[f].v3)); } }

					{	if (leftFaces.TryGetValue(coord, out int cf))								{ weldLinks.Add((faces[cf].v1, faces[f].v2)); weldLinks.Add((faces[cf].v0, faces[f].v3)); }
						else if (frontFaces.TryGetValue(coord+Coord3D.left, out int nf))			{ weldLinks.Add((faces[nf].v1, faces[f].v2)); weldLinks.Add((faces[nf].v0, faces[f].v3)); }
						else if (rightFaces.TryGetValue(coord+Coord3D.front+Coord3D.left, out int of)){weldLinks.Add((faces[of].v1, faces[f].v2)); weldLinks.Add((faces[of].v0, faces[f].v3)); } }

					{	if (rightFaces.TryGetValue(coord, out int cf))								{ weldLinks.Add((faces[cf].v2, faces[f].v1)); weldLinks.Add((faces[cf].v3, faces[f].v0)); }
						else if (frontFaces.TryGetValue(coord+Coord3D.right, out int nf))			{ weldLinks.Add((faces[nf].v2, faces[f].v1)); weldLinks.Add((faces[nf].v3, faces[f].v0)); }
						else if (leftFaces.TryGetValue(coord+Coord3D.front+Coord3D.right, out int of)){weldLinks.Add((faces[of].v2, faces[f].v1)); weldLinks.Add((faces[of].v3, faces[f].v0)); } }
				}

				//back
				foreach (KeyValuePair<Coord3D,int> kvp in backFaces)
				{
					Coord3D coord = kvp.Key;
					int f = kvp.Value;

					{	if (topFaces.TryGetValue(coord, out int cf))								{ weldLinks.Add((faces[cf].v0, faces[f].v1)); weldLinks.Add((faces[cf].v3, faces[f].v2)); }
						else if (backFaces.TryGetValue(coord+Coord3D.up, out int nf))				{ weldLinks.Add((faces[nf].v0, faces[f].v1)); weldLinks.Add((faces[nf].v3, faces[f].v2)); }
						else if (bottomFaces.TryGetValue(coord+Coord3D.back+Coord3D.up, out int of)){ weldLinks.Add((faces[of].v0, faces[f].v1)); weldLinks.Add((faces[of].v3, faces[f].v2)); } }

					{	if (bottomFaces.TryGetValue(coord, out int cf))								{ weldLinks.Add((faces[cf].v1, faces[f].v0)); weldLinks.Add((faces[cf].v2, faces[f].v3)); }
						else if (backFaces.TryGetValue(coord+Coord3D.down, out int nf))				{ weldLinks.Add((faces[nf].v1, faces[f].v0)); weldLinks.Add((faces[nf].v2, faces[f].v3)); }
						else if (topFaces.TryGetValue(coord+Coord3D.back+Coord3D.down, out int of)) { weldLinks.Add((faces[of].v1, faces[f].v0)); weldLinks.Add((faces[of].v2, faces[f].v3)); } }

					{	if (leftFaces.TryGetValue(coord, out int cf))								{ weldLinks.Add((faces[cf].v3, faces[f].v0)); weldLinks.Add((faces[cf].v2, faces[f].v1)); }
						else if (backFaces.TryGetValue(coord+Coord3D.left, out int nf))				{ weldLinks.Add((faces[nf].v3, faces[f].v0)); weldLinks.Add((faces[nf].v2, faces[f].v1)); }
						else if (rightFaces.TryGetValue(coord+Coord3D.back+Coord3D.left, out int of)){weldLinks.Add((faces[of].v3, faces[f].v0)); weldLinks.Add((faces[of].v2, faces[f].v1)); } }

					{	if (rightFaces.TryGetValue(coord, out int cf))								{ weldLinks.Add((faces[cf].v1, faces[f].v2)); weldLinks.Add((faces[cf].v0, faces[f].v3)); }
						else if (backFaces.TryGetValue(coord+Coord3D.right, out int nf))			{ weldLinks.Add((faces[nf].v1, faces[f].v2)); weldLinks.Add((faces[nf].v0, faces[f].v3)); }
						else if (leftFaces.TryGetValue(coord+Coord3D.back+Coord3D.right, out int of)){weldLinks.Add((faces[of].v1, faces[f].v2)); weldLinks.Add((faces[of].v0, faces[f].v3)); } }
				}

				//left
				foreach (KeyValuePair<Coord3D,int> kvp in leftFaces)
				{
					Coord3D coord = kvp.Key;
					int f = kvp.Value;

					{	if (topFaces.TryGetValue(coord, out int cf))								{ weldLinks.Add((faces[cf].v1, faces[f].v1)); weldLinks.Add((faces[cf].v0, faces[f].v2)); }
						else if (leftFaces.TryGetValue(coord+Coord3D.up, out int nf))				{ weldLinks.Add((faces[nf].v0, faces[f].v1)); weldLinks.Add((faces[nf].v3, faces[f].v2)); }
						else if (bottomFaces.TryGetValue(coord+Coord3D.left+Coord3D.up, out int of)) {weldLinks.Add((faces[of].v3, faces[f].v1)); weldLinks.Add((faces[of].v2, faces[f].v2)); } }

					{	if (bottomFaces.TryGetValue(coord, out int cf))								{ weldLinks.Add((faces[cf].v0, faces[f].v0)); weldLinks.Add((faces[cf].v1, faces[f].v3)); }
						else if (leftFaces.TryGetValue(coord+Coord3D.down, out int nf))				{ weldLinks.Add((faces[nf].v1, faces[f].v0)); weldLinks.Add((faces[nf].v2, faces[f].v3)); }
						else if (topFaces.TryGetValue(coord+Coord3D.left+Coord3D.down, out int of)) { weldLinks.Add((faces[of].v2, faces[f].v0)); weldLinks.Add((faces[of].v3, faces[f].v3)); } }

					{	if (frontFaces.TryGetValue(coord, out int cf))								{ weldLinks.Add((faces[cf].v3, faces[f].v0)); weldLinks.Add((faces[cf].v2, faces[f].v1)); }
						else if (leftFaces.TryGetValue(coord+Coord3D.front, out int nf))			{ weldLinks.Add((faces[nf].v3, faces[f].v0)); weldLinks.Add((faces[nf].v2, faces[f].v1)); }
						else if (backFaces.TryGetValue(coord+Coord3D.left+Coord3D.front, out int of)){weldLinks.Add((faces[of].v3, faces[f].v0)); weldLinks.Add((faces[of].v2, faces[f].v1)); } }

					{	if (backFaces.TryGetValue(coord, out int cf))								{ weldLinks.Add((faces[cf].v1, faces[f].v2)); weldLinks.Add((faces[cf].v0, faces[f].v3)); }
						else if (leftFaces.TryGetValue(coord+Coord3D.back, out int nf))				{ weldLinks.Add((faces[nf].v1, faces[f].v2)); weldLinks.Add((faces[nf].v0, faces[f].v3)); }
						else if (frontFaces.TryGetValue(coord+Coord3D.left+Coord3D.back, out int of)){weldLinks.Add((faces[of].v1, faces[f].v2)); weldLinks.Add((faces[of].v0, faces[f].v3)); } }
				} 



				//right
				foreach (KeyValuePair<Coord3D,int> kvp in rightFaces)
				{
					Coord3D coord = kvp.Key;
					int f = kvp.Value;

					{	if (topFaces.TryGetValue(coord, out int cf))								{ weldLinks.Add((faces[cf].v3, faces[f].v1)); weldLinks.Add((faces[cf].v2, faces[f].v2)); }
						else if (rightFaces.TryGetValue(coord+Coord3D.up, out int nf))				{ weldLinks.Add((faces[nf].v0, faces[f].v1)); weldLinks.Add((faces[nf].v3, faces[f].v2)); }
						else if (bottomFaces.TryGetValue(coord+Coord3D.right+Coord3D.up, out int of)){weldLinks.Add((faces[of].v1, faces[f].v1)); weldLinks.Add((faces[of].v0, faces[f].v2)); } }

					{	if (bottomFaces.TryGetValue(coord, out int cf))								{ weldLinks.Add((faces[cf].v2, faces[f].v0)); weldLinks.Add((faces[cf].v3, faces[f].v3)); }
						else if (rightFaces.TryGetValue(coord+Coord3D.down, out int nf))			{ weldLinks.Add((faces[nf].v1, faces[f].v0)); weldLinks.Add((faces[nf].v2, faces[f].v3)); }
						else if (topFaces.TryGetValue(coord+Coord3D.right+Coord3D.down, out int of)){ weldLinks.Add((faces[of].v0, faces[f].v0)); weldLinks.Add((faces[of].v1, faces[f].v3)); } }

					{	if (frontFaces.TryGetValue(coord, out int cf))								{ weldLinks.Add((faces[cf].v1, faces[f].v2)); weldLinks.Add((faces[cf].v0, faces[f].v3)); }
						else if (rightFaces.TryGetValue(coord+Coord3D.front, out int nf))			{ weldLinks.Add((faces[nf].v1, faces[f].v2)); weldLinks.Add((faces[nf].v0, faces[f].v3)); }
						else if (backFaces.TryGetValue(coord+Coord3D.right+Coord3D.front, out int of)){weldLinks.Add((faces[of].v1, faces[f].v2)); weldLinks.Add((faces[of].v0, faces[f].v3)); } }

					{	if (backFaces.TryGetValue(coord, out int cf))								{ weldLinks.Add((faces[cf].v3, faces[f].v0)); weldLinks.Add((faces[cf].v2, faces[f].v1)); }
						else if (rightFaces.TryGetValue(coord+Coord3D.back, out int nf))			{ weldLinks.Add((faces[nf].v3, faces[f].v0)); weldLinks.Add((faces[nf].v2, faces[f].v1)); }
						else if (frontFaces.TryGetValue(coord+Coord3D.right+Coord3D.back, out int of)){weldLinks.Add((faces[of].v3, faces[f].v0)); weldLinks.Add((faces[of].v2, faces[f].v1)); } }
				}

				return weldLinks;
			}


			public static void JoinWeldLinks (ref Vector3[] verts, Face[] faces, List<(int,int)> weldLinks)
			/// weldLinks come in pairs (1-8, 1-7, 2-5, 2-6, 3-0, 3-5, etc) that simplifies task a lot
			{
				int weldLinksCount = weldLinks.Count;

				//sorting weld links by first member (and second, since links are duplicating): 1-4, 2-3, 2-5, 2-8, 3-2, 3-9, 4-1, 4-6, 5-2, etc
				weldLinks.Sort((i1,i2) => i1.Item1 - i2.Item1);

				bool[] processed = new bool[weldLinks.Count];  //processed array to ignore links instead of removing them from list
		
				int[] vToId = new int[faces.Length*4]; //a lut src point num -> unique id
				vToId.Fill(-1);
				int vToIdCounter = 0; //last used id

				HashSet<int> linkGroup = new HashSet<int>();  //reusable set of verts within a single group
				List<int> prevAddedNums = new List<int>(capacity:10);
				List<int> newAddedNums = new List<int>(capacity:10);
				
				for (int i=0; i<weldLinksCount; i+=2)
				{
					if (processed[i]) continue;
					processed[i] = true;
					processed[i+1] = true;

					//filling link group for the first unprocessed weld vert met
					linkGroup.Clear();
					linkGroup.Add(weldLinks[i].Item1);
					linkGroup.Add(weldLinks[i].Item2);
					linkGroup.Add(weldLinks[i+1].Item2);

					prevAddedNums.Clear();
					prevAddedNums.Add(weldLinks[i].Item2);
					prevAddedNums.Add(weldLinks[i+1].Item2);

					//for each of the added links - adding more verts they linked with
					for (int tmp=0; tmp<10; tmp++) //while true
					{
						newAddedNums.Clear();

						int prevAddedNumsCount = prevAddedNums.Count;
						for (int v=0; v<prevAddedNumsCount; v++)
						{
							//if (!processed[prevAddedNums[v]*2])
							int n1 = weldLinks[prevAddedNums[v]*2].Item2;
							if (!linkGroup.Contains(n1))
								{ linkGroup.Add(n1); newAddedNums.Add(n1); }
							processed[prevAddedNums[v]*2] = true;

							int n2 = weldLinks[prevAddedNums[v]*2+1].Item2;
							if (!linkGroup.Contains(n2))
								{ linkGroup.Add(n2); newAddedNums.Add(n2); }
							processed[prevAddedNums[v]*2+1] = true;
						}

						if (newAddedNums.Count == 0) break;

						//swapping lists
						List<int> tmpNums = prevAddedNums;
						prevAddedNums = newAddedNums;
						newAddedNums = tmpNums;
					}
					
					foreach (int v in linkGroup)
						vToId[v] = vToIdCounter;
					vToIdCounter++;
				}

				//replacing vnum to id in faces
				for (int f=0; f<faces.Length; f++)
				{
					faces[f].v0 = vToId[faces[f].v0];
					faces[f].v1 = vToId[faces[f].v1];
					faces[f].v2 = vToId[faces[f].v2];
					faces[f].v3 = vToId[faces[f].v3];
				}

				//creating new verts array numbers based on id
				Vector3[] newVerts = new Vector3[vToIdCounter]; //vToIdCounter contains the maximal id
				for (int v=0; v<verts.Length; v++)
				{
					int id = vToId[v];
					newVerts[id] = verts[v];
				}
				verts = newVerts;
			}


			private static int[] GetWeldLinkStarts (List<(int,int)> weldLinks)
			/// Lut for quick jump to weldLink by start index (4 jumps to where 4-1, 4-6 starts)
			/// Not used since it's easier to get wel link start by *2 in current case
			/// Tested
			{
				int weldLinksCount = weldLinks.Count;
				int[] weldLinksStarts = new int[weldLinksCount*2]; //maximal vert count is f*4
				int currNum = 0;
				for (int i=0; i<weldLinksCount; i++)
				{
					int num = weldLinks[i].Item1;
					if (num != currNum)
					{
						weldLinksStarts[num] = i;
						currNum = num;
					}
				}

				return weldLinksStarts;
			}


			private static void JoinWeldLinksAlternative (ref Vector3[] verts, Face[] faces, List<(int,int)> weldLinks)
			/// An older way merge weld links. 
			/// Drawback - has to iterate through all vToId numerous times
			{
				//creating a lut src point num -> unique id
				int[] vToId = new int[faces.Length*4];
				vToId.Fill(-1);

				bool[] processed = new bool[weldLinks.Count];

				int weldLinksCount = weldLinks.Count;
				int nvCounter = 0;
				(int,int) link = weldLinks[0]; //will try to follow links to avoid merging link areas

				for (int i=0; i<weldLinksCount; i++)
				{
					//both link verts are not in lut
					if (vToId[link.Item1] < 0  &&  vToId[link.Item2] < 0)
					{
						vToId[link.Item1] = nvCounter;
						vToId[link.Item2] = nvCounter;
						nvCounter++;
					}

					//if first is in lut
					else if (vToId[link.Item1] >= 0  &&  vToId[link.Item2] < 0)
					{
						int id = vToId[link.Item1];
						vToId[link.Item2] = id;
					}

					//if second is in lut
					else if (vToId[link.Item1] < 0  &&  vToId[link.Item2] >= 0)
					{
						int id = vToId[link.Item2];
						vToId[link.Item1] = id;
					}

					//both are in lut
					else if (vToId[link.Item1] >= 0  &&  vToId[link.Item2] >= 0)
					{
						//checking if it's just a reverse combination of already existing pair
						if (vToId[link.Item1] == vToId[link.Item2]) //already welded
							continue;

						//welding two existing weld areas
						else
						{
							int greaterNum = vToId[link.Item1] > vToId[link.Item2] ? vToId[link.Item1] : vToId[link.Item2];
							int lesserNum = vToId[link.Item1] < vToId[link.Item2] ? vToId[link.Item1] : vToId[link.Item2];

							for (int j=0; j<vToId.Length; j++)
							{
								if (vToId[j] == greaterNum) vToId[j] = lesserNum;
								else if (vToId[j] > greaterNum) vToId[j]--; //offseting all numbers since one weld exists no more
							}

							nvCounter--;
							//vToId[nvCounter] = -1;
						}
					}
				}

				//replacing vnum to id in faces
				for (int f=0; f<faces.Length; f++)
				{
					faces[f].v0 = vToId[faces[f].v0];
					faces[f].v1 = vToId[faces[f].v1];
					faces[f].v2 = vToId[faces[f].v2];
					faces[f].v3 = vToId[faces[f].v3];
				}

				//creating new verts array numbers based on id
				Vector3[] newVerts = new Vector3[nvCounter]; //nvCounter contains the maximal id
				for (int v=0; v<verts.Length; v++)
				{
					int id = vToId[v];
					newVerts[id] = verts[v];
				}
				verts = newVerts;
			}

		#endregion


		#region Relax

			public static void RelaxIteration (Vector3[] verts, Vector3[] relax, Face[] faces)
			{
				int[] relCount = new int[relax.Length];
				relax.Fill(new Vector3(0,0,0));

				for (int f=0; f<faces.Length; f++)
				{
					Vector3 vert0 = verts[faces[f].v0];
					Vector3 vert1 = verts[faces[f].v1];
					Vector3 vert2 = verts[faces[f].v2];
					Vector3 vert3 = verts[faces[f].v3];

					relax[faces[f].v0] += (vert3-vert0).normalized + (vert1-vert0).normalized;  relCount[faces[f].v0]++;
					relax[faces[f].v1] += (vert0-vert1).normalized + (vert2-vert1).normalized;  relCount[faces[f].v1]++;
					relax[faces[f].v2] += (vert1-vert2).normalized + (vert3-vert2).normalized;  relCount[faces[f].v2]++;
					relax[faces[f].v3] += (vert2-vert3).normalized + (vert0-vert3).normalized;  relCount[faces[f].v3]++;
				}

				for (int n=0; n<relax.Length; n++)
					relax[n] /= relCount[n] * 2;
			}


			public static void Relax (Vector3[] verts, Face[] faces, int iterations=2, float amount=0.5f)
			{
				Vector3[] relax = new Vector3[verts.Length];

				for (int i=0; i<iterations; i++)
				{
					RelaxIteration(verts, relax, faces);

					for (int v=0; v<verts.Length; v++)
						verts[v] += relax[v]*amount;
				}
			}

		#endregion


		#region Normals

			public static void FaceNormalsByDirs (Vector3[] faceNormals, Face[] faces)
			/// Fast way to create per-face normals using face directions
			{
				for (int f=0; f<faces.Length; f++)
					faceNormals[f] = faces[f].dir.Vector;
			}

			public static void FaceNormalsByVerts (Vector3[] verts, Vector3[] faceNormals, Face[] faces)
			/// Fast way to create per-face normals using face directions
			{
				//DebugGizmos.Clear("VoxelNormals");

				for (int f=0; f<faces.Length; f++)
				{
					Vector3 vert0 = verts[faces[f].v0];
					Vector3 vert1 = verts[faces[f].v1];
					Vector3 vert2 = verts[faces[f].v2];
					Vector3 vert3 = verts[faces[f].v3];

					Vector3 norm0 = Vector3.Cross(vert1-vert0, vert3-vert0);
					Vector3 norm1 = Vector3.Cross(vert2-vert1, vert0-vert1);
					Vector3 norm2 = Vector3.Cross(vert3-vert2, vert1-vert2);
					Vector3 norm3 = Vector3.Cross(vert0-vert3, vert2-vert3);

					faceNormals[f] = (norm0+norm1+norm2+norm3) / 4;

					//DebugGizmos.DrawRay("VoxelNormals", (vert0+vert1+vert2+vert3)/4, norm1.normalized, additive:true);
				}
			}


			public static void VertNormals (Vector3[] normals, Vector3[] faceNormals, Face[] faces)
			{
				int[] normCount = new int[normals.Length];
				normals.Fill(new Vector3(0,0,0));

				for (int f=0; f<faces.Length; f++)
				{
					Vector3 faceNormal = faceNormals[f];
					normals[faces[f].v0] += faceNormal;  normCount[faces[f].v0]++;
					normals[faces[f].v1] += faceNormal;  normCount[faces[f].v1]++;
					normals[faces[f].v2] += faceNormal;  normCount[faces[f].v2]++;
					normals[faces[f].v3] += faceNormal;  normCount[faces[f].v3]++;
				}

				for (int n=0; n<normals.Length; n++)
					normals[n] /= normCount[n];
			}

		#endregion


		#region Collisions

			private static float TriSquare (Vector3 v0, Vector3 v1, Vector3 v2)
			{
				float s0 = (v0-v1).magnitude;
				float s1 = (v1-v2).magnitude;
				float s2 = (v2-v0).magnitude;
				float p = (s0+s1+s2)/2;
				return Mathf.Sqrt( p*(p-s0)*(p-s1)*(p-s2) );
			}

			private static bool IsInsideTri (Vector3 v0, Vector3 v1, Vector3 v2, Vector3 p)
			{
				float triSq = TriSquare(v0,v1,v2);

				float sq0 = TriSquare(p,v1,v2);
				float sq1 = TriSquare(v0,p,v2);
				float sq2 = TriSquare(v0,v1,p);

				return  (sq1+sq2+sq0) < triSq+0.0001f  &&
						(sq1+sq2+sq0) > triSq-0.0001f;
			}

			private static bool IsInsideFace (Vector3[] verts, Face face, Vector3 point)
			{
				return  IsInsideTri(verts[face.v1], verts[face.v2], verts[face.v0], point) ||
						IsInsideTri(verts[face.v3], verts[face.v0], verts[face.v2], point) ;
			}

			public static int IntersectedFace (Vector3[] verts, Face[] faces, Ray ray)
			/// Returns the number and baricentric coordinates of the intersected face
			{
				Quaternion rot = Quaternion.LookRotation(ray.direction);
				Vector3 origin = ray.origin;
				Matrix4x4 mat = Matrix4x4.TRS(origin, rot, Vector3.one);
				mat = mat.inverse;

				Vector3[] trVerts = new Vector3[verts.Length];
				for (int v=0; v<verts.Length; v++)
				{
					trVerts[v] = mat.MultiplyPoint(verts[v]);
					trVerts[v].z = 0;
				}

				int closestFace = -1;
				float closestDist = float.MaxValue;

				Vector3 zero = Vector3.zero;
				for (int f=0; f<faces.Length; f++)
				{
					Vector3 trv0 = trVerts[faces[f].v0];
					Vector3 trv1 = trVerts[faces[f].v1];
					Vector3 trv2 = trVerts[faces[f].v2];
					Vector3 trv3 = trVerts[faces[f].v3];

					if (trv0.x>0 && trv1.x>0 && trv2.x>0 && trv3.x>0) continue;
					if (trv0.x<0 && trv1.x<0 && trv2.x<0 && trv3.x<0) continue;
					if (trv0.y>0 && trv1.y>0 && trv2.y>0 && trv3.y>0) continue;
					if (trv0.y<0 && trv1.y<0 && trv2.y<0 && trv3.y<0) continue;

					if (IsInsideFace(trVerts, faces[f], zero))
					{
						Vector4 baryCoords = new Vector4(trv0.magnitude, trv1.magnitude, trv2.magnitude, trv3.magnitude);
						float barySum = baryCoords.x + baryCoords.y + baryCoords.z + baryCoords.w;
						baryCoords /= barySum;

						float dist = (origin-verts[faces[f].v0]).magnitude * baryCoords.x +
									 (origin-verts[faces[f].v1]).magnitude * baryCoords.y +
									 (origin-verts[faces[f].v2]).magnitude * baryCoords.z +
									 (origin-verts[faces[f].v3]).magnitude * baryCoords.w;

						if (dist < closestDist)
						{
							closestFace = f;
							closestDist = dist;
						}
					}
				}

				return closestFace;
			}


		#endregion


		#region Faces Lut

			/*public static (Dictionary<int,CoordDir>, Dictionary<CoordDir,int>) GenerateFaceCoordLuts (Face[] faces)
			{
				Dictionary<int,CoordDir> faceToCoord = new Dictionary<int,CoordDir>(capacity:faces.Length);
				Dictionary<CoordDir,int> coordToFace = new Dictionary<CoordDir,int>(capacity:faces.Length);

				for (int f=0; f<faces.Length; f++)
				{
					faceToCoord.Add(f, new CoordDir(faces[f].coord, faces[f].dir));
					coordToFace.Add(new CoordDir(faces[f].coord, faces[f].dir), f);
				}

				return (faceToCoord, coordToFace);
			}*/


		#endregion


		#region Testing

			public static int FindOpenEdges (Face[] faces)
			/// Returns the number of open edges
			{
				int openEdgesCount = 0;

				for (int f=0; f<faces.Length; f++)
				{
					{
						int va=faces[f].v0; int vb=faces[f].v1;
						bool open=true;
						for (int f2=0; f2<faces.Length; f2++)
						{
							if (f2==f) continue;
							if (faces[f2].ContainsEdge(va,vb)) {open=false; break; }
						}
						if (open) openEdgesCount++;
					}

					{
						int va=faces[f].v1; int vb=faces[f].v2;
						bool open=true;
						for (int f2=0; f2<faces.Length; f2++)
						{
							if (f2==f) continue;
							if (faces[f2].ContainsEdge(va,vb)) {open=false; break; }
						}
						if (open) openEdgesCount++;
					}

					{
						int va=faces[f].v2; int vb=faces[f].v3;
						bool open=true;
						for (int f2=0; f2<faces.Length; f2++)
						{
							if (f2==f) continue;
							if (faces[f2].ContainsEdge(va,vb)) {open=false; break; }
						}
						if (open) openEdgesCount++;
					}

					{
						int va=faces[f].v3; int vb=faces[f].v0;
						bool open=true;
						for (int f2=0; f2<faces.Length; f2++)
						{
							if (f2==f) continue;
							if (faces[f2].ContainsEdge(va,vb)) {open=false; break; }
						}
						if (open) openEdgesCount++;
					}
				}

				return openEdgesCount;
			}


			public static int FindIsolatedVerts (Face[] faces)
			{
				int maxVert = 0;
				bool[] usedVerts = new bool[faces.Length*4];

				for (int f=0; f<faces.Length; f++)
					for (int v=0; v<4; v++)
					{
						int vert = faces[f].GetVert(v);
						usedVerts[vert] = true;
						if (vert > maxVert) maxVert=vert;
					}

				int isolated = 0;
				for (int i=0; i<maxVert; i++)
					if (!usedVerts[i]) isolated++;

				return isolated;
			}


			public static (Vector3,Vector3)[] Wireframe (Vector3[] verts, Face[] faces)
			{
				HashSet<Vector2Int> lineNums = new HashSet<Vector2Int>();
				for (int f=0; f<faces.Length; f++)
				{
					if (!lineNums.Contains(new Vector2Int(faces[f].v0, faces[f].v1))  &&  !lineNums.Contains(new Vector2Int(faces[f].v1, faces[f].v0)) )
						lineNums.Add(new Vector2Int(faces[f].v0, faces[f].v1));

					if (!lineNums.Contains(new Vector2Int(faces[f].v1, faces[f].v2))  &&  !lineNums.Contains(new Vector2Int(faces[f].v2, faces[f].v1)) )
						lineNums.Add(new Vector2Int(faces[f].v1, faces[f].v2));

					if (!lineNums.Contains(new Vector2Int(faces[f].v2, faces[f].v3))  &&  !lineNums.Contains(new Vector2Int(faces[f].v3, faces[f].v2)) )
						lineNums.Add(new Vector2Int(faces[f].v2, faces[f].v3));

					if (!lineNums.Contains(new Vector2Int(faces[f].v3, faces[f].v0))  &&  !lineNums.Contains(new Vector2Int(faces[f].v0, faces[f].v3)) )
						lineNums.Add(new Vector2Int(faces[f].v3, faces[f].v0));
				}

				(Vector3,Vector3)[] linePoses = new (Vector3,Vector3)[lineNums.Count];
				int i=0;
				foreach (Vector2Int line in lineNums)
				{
					linePoses[i] = (verts[line.x], verts[line.y]);
					i++;
				}

				return linePoses;
			}


			public static (Vector3,Vector3)[] Wireframe (Vector3[] verts, Face face)
			{
				(Vector3,Vector3)[] linePoses = new (Vector3,Vector3)[4];

				linePoses[0] = (verts[face.v0], verts[face.v1]);
				linePoses[1] = (verts[face.v1], verts[face.v2]);
				linePoses[2] = (verts[face.v2], verts[face.v3]);
				linePoses[3] = (verts[face.v3], verts[face.v0]);

				return linePoses;
			}


		#endregion
	}
}