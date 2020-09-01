using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
//using UnityEngine.Profiling;

using Den.Tools.GUI;

namespace Den.Tools.Voxels
{



	public struct Dir
	{
		public byte val;

		public Dir (int x, int y, int z) 
		{
			int xVal= x==0 ? 0b_00000000 : (x<0 ? 0b_00_000_100 : 0b_00_100_000);
			int yVal= y==0 ? 0b_00000000 : (y<0 ? 0b_00_000_010 : 0b_00_010_000);
			int zVal= z==0 ? 0b_00000000 : (z<0 ? 0b_00_000_001 : 0b_00_001_000);

			val = (byte)(xVal | yVal | zVal);
		}

		public Coord3D Coord
		{get{
			return new Coord3D( 
				-(val>>2 & 0b_00000001) + (val>>5 & 0b_00000001),
				-(val>>1 & 0b_00000001) + (val>>4 & 0b_00000001),
				-(val>>0 & 0b_00000001) + (val>>3 & 0b_00000001) );
		}}

		public Vector3 Vector
		{get{
			return new Vector3( 
				-(val>>2 & 0b_00000001) + (val>>5 & 0b_00000001),
				-(val>>1 & 0b_00000001) + (val>>4 & 0b_00000001),
				-(val>>0 & 0b_00000001) + (val>>3 & 0b_00000001) );
		}}

		public override string ToString () => Convert.ToString(val, 2);
	}


	/*public struct CoordDir
	{
		Coord3D coord;
		Dir dir;

		public CoordDir (Coord3D coord, Dir dir) { this.coord=coord; this.dir=dir; } 

		public override int GetHashCode() {return coord.x*1000000 + coord.y*10000 + coord.z*100 + dir.val;}
	}*/


	public struct Face
	{
		public int v0;
		public int v1;
		public int v2;
		public int v3;

		public Coord3D coord;
		public Dir dir; 

		public Face (int counter, Coord3D coord, Dir dir) { v0=counter; v1=counter+1; v2=counter+2; v3=counter+3; this.coord=coord; this.dir=dir; }
	
		public void AddTris (int[] tris, int start)
		{
			tris[start] = v0; //v1;
			tris[start+1] = v1; //v2;
			tris[start+2] = v2; //v0;
						
			tris[start+3] = v2; //v3;
			tris[start+4] = v3; //v0;
			tris[start+5] = v0; //v2;
			//vert order reason: common side is not mentioned, like in gizmo plane
		}

		public void AddQuads (int[] quads, int start)
		{
			quads[start] = v0;
			quads[start+1] = v1;
			quads[start+2] = v2;
			quads[start+3] = v3;
		}

		public void GenerateUvs (Vector2[] uvs)
		{
			for (int i=0; i<4; i++)
			{
				Vector2 uv;
				switch (i)
				{
					case 0: uv = new Vector2(0,0); break;
					case 1: uv = new Vector2(0,1); break;
					case 2: uv = new Vector2(1,1); break;
					case 3: uv = new Vector2(1,0); break;
					default: uv = new Vector2(0,0); break;
				}

				uv /= 4;

				Coord3D dirVector = dir.Coord;
				if (dirVector == Coord3D.up) uv +=  new Vector2(0.25f,0.25f);
				if (dirVector == Coord3D.down) uv += new Vector2(0.25f,0);
				if (dirVector == Coord3D.left) uv += new Vector2(0.5f,0f);
				if (dirVector == Coord3D.right) uv += new Vector2(0.5f,0.25f);
				if (dirVector == Coord3D.front) uv += new Vector2(0.0f,0.25f);
				if (dirVector == Coord3D.back) uv +=  new Vector2(0,0);

				switch (i)
				{
					case 0: uvs[v0] = uv; break;
					case 1: uvs[v1] = uv; break;
					case 2: uvs[v2] = uv; break;
					case 3: uvs[v3] = uv; break;
				}
			}
		}

		public int GetVert (int num)
		/// For test purpose only
		{
			switch (num)
			{
				case 0: return v0;
				case 1: return v1;
				case 2: return v2;
				case 3: return v3;
				default: return v0;
			}
		}

		public void SetVert (int num, int val)
		{
			switch (num)
			{
				case 0: v0=val; break;
				case 1: v1=val; break;
				case 2: v2=val; break;
				case 3: v3=val; break;
			}
		}

		public bool ContainsVert (int v) => v0==v || v1==v || v2==v || v3==v;
		public bool ContainsEdge (int va, int vb) => (v0==va && v1==vb) || (v1==va && v2==vb) || (v2==va && v3==vb) || (v3==va && v0==vb) ||
													 (v0==vb && v1==va) || (v1==vb && v2==va) || (v2==vb && v3==va) || (v3==vb && v0==va);
	}


}