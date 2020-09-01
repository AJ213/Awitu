using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using UnityEngine;

using Den.Tools.Matrices;

namespace Den.Tools.Segs
{ 
	public static class SplineMatrixOps
	{
		public static void Floor (MatrixWorld heights, Spline spline, bool interpolated=true)
		/// Drops spline to height matrix
		{
			for (int n=0; n<spline.nodes.Length; n++)
			{
				Vector3 node = spline.nodes[n];

				if (node.x <= heights.worldPos.x) node.x = heights.worldPos.x + 0.001f;
				if (node.x >= heights.worldPos.x+heights.worldSize.x) node.x = heights.worldPos.x + heights.worldSize.x - 0.001f;
				if (node.z <= heights.worldPos.z) node.z = heights.worldPos.z + 0.001f;
				if (node.z >= heights.worldPos.z+heights.worldSize.z) node.z = heights.worldPos.z + heights.worldSize.z - 0.001f;

				float height = interpolated ? 
					heights.GetWorldInterpolatedValue(node.x, node.z) :
					heights.GetWorldValue(node.x, node.z);

				spline.nodes[n].y = height * heights.worldSize.y;
			}
		}


		public static void Stroke (Spline spline, MatrixWorld matrix, 
			bool white=false, float intensity=1,
			bool antialiased=false, bool padOnePixel=false)
		/// Draws a line on matrix
		/// White will fill the line with 1, when disabled it will use spline height
		/// PaddedOnePixel works similarly to AA, but fills border pixels with full value (to create main tex for the mask)
		{
			Vector3 startPos = spline.nodes[0];
			Vector3 prevCoord = matrix.WorldToPixelInterpolated(startPos.x, startPos.z);
			float prevHeight = white ? intensity : (startPos.y / matrix.worldSize.y);

			for (int n=0; n<spline.nodes.Length; n++)
			{
				Vector3 pos = spline.nodes[n];
				float posHeight = white ? intensity : (pos.y / matrix.worldSize.y);
				pos = matrix.WorldToPixelInterpolated(pos.x, pos.z);

				matrix.Line(
					new Vector2(prevCoord.x, prevCoord.z), 
					new Vector2(pos.x, pos.z), 
						prevHeight,
						posHeight,
						antialised:antialiased,
						paddedOnePixel:padOnePixel,
						endInclusive:n==spline.nodes.Length-1);

				prevCoord = pos;
				prevHeight = posHeight;
			}
		}

		#region Silhouette

			public static void Silhouette (Spline[] splines, MatrixWorld matrix)
			/// Fills all pixels within closed spline with 1, and all outer pixels with 0
			/// Pixels directly in the spline are filled with 0.5
			/// Internally strokes matrix first
			{
				for (int s=0; s<splines.Length; s++)
					Stroke (splines[s], matrix, white:true, intensity:0.5f, antialiased:false, padOnePixel:false);

				Silhouette(splines, matrix, matrix);
			}

			public static void Silhouette (Spline[] splines, MatrixWorld strokeMatrix, MatrixWorld dstMatrix)
			/// Fills all pixels within closed spline with 1, and all outer pixels with 0
			/// Pixels directly in the spline are filled with 0.5
			/// Requires the matrix with line stroked. StrokeMatrix and DstMatrix could be the same
			{
				DebugGizmos.Clear("Slhuette");

				if (strokeMatrix != dstMatrix)
					dstMatrix.Fill(strokeMatrix);
					//and then using dst matrix only

				CoordRect rect = dstMatrix.rect;
				Coord min = rect.Min; Coord max = rect.Max;

				for (int x=min.x; x<max.x; x++)
					for (int z=min.z; z<max.z; z++)
					{
						int pos = (z-rect.offset.z)*rect.size.x + x - rect.offset.x;
				
						if (dstMatrix.arr[pos] < 0.01f) //free from stroke and fill
						{
							Vector2D pixelPos = (Vector2D)dstMatrix.PixelToWorld(x, z);

							bool handness = Spline.Handness(splines, pixelPos) >= 0;

							DebugGizmos.DrawDot("Slhuette", (Vector3)pixelPos, 6, color:handness ? Color.red : Color.green, additive:true);

							dstMatrix.PaintBucket(new Coord(x,z), handness ? 0.75f : 0.25f);
						}
					}
			}

			public static void PaintBucket (this MatrixWorld matrix, Coord coord, float val, float threshold=0.0001f, int maxIterations=10)
			/// Like a paintBucket tool in photoshop
			/// Fills all zero (lower than threshold) values with val, until meets borders
			/// Doesnt guarantee filling (areas after the corner could be missed)
			/// Use threshold to change between mask -1 or 0
			/// TODO: to matrix ops
			{
				CoordRect rect = matrix.rect;
				Coord min = rect.Min; Coord max = rect.Max;

				MatrixOps.Stripe stripe = new MatrixOps.Stripe( Mathf.Max(rect.size.x, rect.size.z) );

				stripe.length = rect.size.x;

				matrix[coord] = -256; //starting mask

				//first vertical spread is one row-only
				MatrixOps.ReadRow(stripe, matrix, coord.x, matrix.rect.offset.z);
				PaintBucketMaskStripe(stripe, threshold);
				MatrixOps.WriteRow(stripe, matrix, coord.x, matrix.rect.offset.z);

				for (int i=0; i<maxIterations; i++) //ten tries, but hope it will end before that
				{
					bool change = false;

					//horizontally
					for (int z=min.z; z<max.z; z++)
					{
						MatrixOps.ReadLine(stripe, matrix, rect.offset.x, z);
						change = PaintBucketMaskStripe(stripe, threshold)  ||  change;
						MatrixOps.WriteLine(stripe, matrix, rect.offset.x, z);
					}

					//vertically
					for (int x=min.x; x<max.x; x++)
					{
						MatrixOps.ReadRow(stripe, matrix, x, matrix.rect.offset.z);
						change = PaintBucketMaskStripe(stripe, threshold)  ||  change;
						MatrixOps.WriteRow(stripe, matrix, x, matrix.rect.offset.z);
					}

					if (!change)
						break;

					//if (i==maxIterations-1 && !change)
					//	Debug.Log("Reached max iterations");
				}

				//filling masked values with val
				for (int i=0; i<matrix.arr.Length; i++)
					if (matrix.arr[i] < -255) matrix.arr[i] = val;
			}

			private static bool PaintBucketMaskStripe (MatrixOps.Stripe stripe, float threshold=0.0001f)
			/// Fills stripe until first unmasked value with -256
			/// Returns true if anything masked
			{
				bool changed = false;

				//to right
				bool masking = false;
				for (int i=0; i<stripe.length; i++)
				{
					if (stripe.arr[i] < -255)
					{
						masking = true;
						continue;
					}

					if (stripe.arr[i] < threshold)
					{
						if (masking) 
						{
							stripe.arr[i] = -256;
							changed = true;
						}
					}
					else
						masking = false;
				}

				//to left
				masking = false;
				for (int i=stripe.length-1; i>=0; i--)
				{
					if (stripe.arr[i] < -255)
					{
						masking = true;
						continue;
					}

					if (stripe.arr[i] < threshold)
					{
						if (masking) 
						{
							stripe.arr[i] = -256;
							changed = true;
						}
					}
					else
						masking = false;
				}

				return changed;
			}


			public static void CombineSilhouetteSpread (Matrix silhouetteMatrix, Matrix spreadMatrix, Matrix dstMatrix)
			/// Blends silhouette and spread the way it's done in Silhouette node, so that silhouette have the spreaded edges
			/// All matrices could be the same
			{
				for (int i=0; i<dstMatrix.count; i++)
				{
					float spread = spreadMatrix.arr[i];
					float silhouette = silhouetteMatrix.arr[i];
					dstMatrix.arr[i] = silhouette > 0.5f ? spread : 1-spread;
				}
			}

		#endregion


		#region Isoline

			[StructLayout(LayoutKind.Sequential)]
			public struct MetaLine 
			{ 
				public Coord c0;  
				public Coord c1;  
				public Coord c2;  
				public Coord c3;
				public byte count;
				public bool closed;  

				public const int length = 256;
			
				public Coord this[int n]
				{
					get 
					{ 
						switch (n)
						{
							case 0: return c0;
							case 1: return c1;
							case 2: return c2;
							case 3: return c3;
							default: throw new Exception($"PixelLine array index ({n}) out of range");
						}
					}
					set 
					{ 
						switch (n)
						{
							case 0: c0 = value; break;
							case 1: c1 = value; break;
							case 2: c2 = value; break;
							case 3: c3 = value; break;
							default: throw new Exception($"PixelLine array index ({n}) out of range");
						}
					}
				}

				public static void AddToList (List<Coord> list, MetaLine line)
				{
					if (line.count==0) return;
					if (line.count>=1) list.Add(line.c0);
					if (line.count>=2) list.Add(line.c1);
					if (line.count>=3) list.Add(line.c2);
					if (line.count==4) list.Add(line.c3);
					if (line.closed) list.Add(line.c0);
				}

				public Coord Last
				{get{
					switch (count)
					{
						case 1: return c0;
						case 2: return c1;
						case 3: return c2;
						case 4: return c3;
						default: throw new Exception($"PixelLine array index ({count-1}) out of range");
					}
				}}

				public MetaLine (Coord c0, Coord c1, Coord c2, Coord c3) { this.c0=c0; this.c1=c1; this.c2=c2; this.c3=c3; count=4; closed=false; }
				public MetaLine (int c0x, int c0z, int c1x, int c1z, int c2x, int c2z, int c3x, int c3z) { c0.x=c0x; c0.z=c0z; c1.x=c1x; c1.z=c1z; c2.x=c2x; c2.z=c2z; c3.x=c3x; c3.z=c3z; count=4; closed=false; }
				public MetaLine (int c0x, int c0z, int c1x, int c1z, int c2x, int c2z) { c0.x=c0x; c0.z=c0z; c1.x=c1x; c1.z=c1z; c2.x=c2x; c2.z=c2z; c3.x=0; c3.z=0; count=3; closed=false; }
				public MetaLine (int c0x, int c0z, int c1x, int c1z) { c0.x=c0x; c0.z=c0z; c1.x=c1x; c1.z=c1z; c2.x=0; c2.z=0; c3.x=0; c3.z=0; count=2; closed=false; }
			}


			public static List<MetaLine> MatrixToMetaLines (Matrix matrix, float threshold)
			/// Create isolines in form of short unwelded meta-lines around each pixel
			{
				Coord size = matrix.rect.size; Coord offset = matrix.rect.offset;
				Coord min = matrix.rect.Min; Coord max = matrix.rect.Max;
				List<MetaLine> lines = new List<MetaLine>();

				for (int x=min.x; x<max.x; x++)
					for (int z=min.z; z<max.z; z++)
					{
						int pos = (z-offset.z)*size.x + x - offset.x;
						if (matrix.arr[pos] < threshold) continue;

						int c = 0;
						if (z==max.z-1  ||  matrix.arr[pos+size.z]>=threshold)  c = c | 0b_0000_1000;  //z==size.z-1: out of border pixels have the same fill as this one (so they considered always filled)
						if (z==min.z  ||  matrix.arr[pos-size.z]>=threshold)  c = c | 0b_0000_0100;
						if (x==min.x  ||  matrix.arr[pos-1]>=threshold)  c = c | 0b_0000_0010;
						if (x==max.x-1  ||  matrix.arr[pos+1]>=threshold)  c = c | 0b_0000_0001;

						switch (c)
						{
							case 0b_0000_0000:  //if (!top && !bottom && !left && !right)
								lines.Add( new MetaLine(x,z,  x,z+1,  x+1,z+1,  x+1,z) { closed=true } );
								break;

							case 0b_0000_0001:  //else if (!top && !bottom && !left &&  right)
								lines.Add( new MetaLine(x+1,z,  x,z,  x,z+1,  x+1,z+1) );
								break;

							case 0b_0000_0100:  //else if (!top &&  bottom && !left && !right)
								lines.Add( new MetaLine(x,z,  x,z+1,  x+1,z+1,  x+1,z) );
								break;

							case  0b_0000_0010:  //else if (!top && !bottom &&  left && !right)
								lines.Add( new MetaLine(x,z+1,  x+1,z+1,  x+1,z,  x,z) );
								break;

							case  0b_0000_1000:  //else if ( top && !bottom && !left && !right)
								lines.Add( new MetaLine(x+1,z+1,  x+1,z,  x,z,  x,z+1) );
								break;

							case  0b_0000_0011:  //else if (!top && !bottom &&  left &&  right)
								lines.Add( new MetaLine(x,z+1,  x+1,z+1) );
								lines.Add( new MetaLine(x+1,z,  x,z) );
								break;

							case 0b_0000_1100:  //else if ( top &&  bottom && !left && !right)
								lines.Add( new MetaLine(x,z,  x,z+1) );
								lines.Add( new MetaLine(x+1,z+1,  x+1,z) );
								break;

							case 0b_0000_0101:  //else if (!top &&  bottom && !left &&  right)
								lines.Add( new MetaLine(x,z,  x,z+1,  x+1,z+1) );
								break;

							case 0b_0000_0110:  //else if (!top &&  bottom &&  left && !right)
								lines.Add( new MetaLine(x,z+1,  x+1,z+1,  x+1,z) );
								break;

							case 0b_0000_1010:  //else if ( top && !bottom &&  left && !right)
								lines.Add( new MetaLine(x+1,z+1,  x+1,z,  x,z) );
								break;

							case 0b_0000_1001:  //else if ( top && !bottom && !left &&  right)
								lines.Add( new MetaLine(x+1,z,  x,z,  x,z+1) );
								break;
					
							case 0b_0000_0111:  //else if (!top &&  bottom &&  left &&  right)
								lines.Add( new MetaLine(x,z+1,  x+1,z+1) );
								break;

							case 0b_0000_1110:  //else if ( top &&  bottom &&  left && !right)
								lines.Add( new MetaLine(x+1,z+1,  x+1,z) );
								break;

							case 0b_0000_1011:  //else if ( top && !bottom &&  left &&  right)
								lines.Add( new MetaLine(x+1,z,  x,z) );
								break;

							case 0b_0000_1101:  //else if ( top &&  bottom && !left &&  right)
								lines.Add( new MetaLine(x,z,  x,z+1) );
								break;

							case 0b_0000_1111:  //else if (top && bottom && left && right)
								break;

							default:
								throw new Exception("Impossible pixels combination found");
						}
					}

				return lines;
			}

			public static List< List<Coord> > WeldMetaLines (List<MetaLine> metaLines)
			{
				//metaLines start coord lut
				Dictionary<Coord,int> startLut = new Dictionary<Coord,int>(capacity:metaLines.Count);
				int metaLinesCount = metaLines.Count;
				for (int i=0; i<metaLinesCount; i++)
					startLut.Add(metaLines[i].c0, i);

				//meta lines with the start coordinate that is not covered with other metaline end coordinate
				//they will begin a new line
				Stack<Coord> initialCoords = new Stack<Coord>();

				HashSet<Coord> endCoords = new HashSet<Coord>();
				for (int i=0; i<metaLinesCount; i++)
					endCoords.Add(metaLines[i].Last);
			
				for (int i=0; i<metaLinesCount; i++)
					if (!endCoords.Contains(metaLines[i].c0)) initialCoords.Push(metaLines[i].c0);

				//welded lines
				List< List<Coord> > weldedLines = new List< List<Coord> >();
				List<Coord> currentLine = new List<Coord>();

				//welding
				while (startLut.Count != 0)
				{
					//finding first point to weld
					Coord start;
					if (initialCoords.Count != 0)
						start = initialCoords.Pop(); 
					else
						start = startLut.AnyKey(); //if no initial coords left - then only looped lines remain - and then starting anywhere

					for (int i=0; i<metaLinesCount+2; i++) //should not reach maximum
					{
						if (startLut.TryGetValue(start, out int num)) //if has continuation
						{
							MetaLine.AddToList(currentLine, metaLines[num]);
							startLut.Remove(start);
							start = metaLines[num].Last;
							continue;
						}

						else //finishing line
						{
							weldedLines.Add(currentLine);
							currentLine = new List<Coord>();
							break;
						} 

						throw new Exception("Welding reached maximum");
					}
				}

				return weldedLines;
			}

			public static Spline[] Isoline (MatrixWorld matrix, float threshold)
			{
				List<MetaLine> metaLines = MatrixToMetaLines(matrix, threshold);
				List<List<Coord>> lineCoords = WeldMetaLines(metaLines);

				Spline[] splines = new Spline[lineCoords.Count];
				for (int s=0; s<splines.Length; s++)
				{
					Spline spline = new Spline();
					List<Coord> coords = lineCoords[s];

					splines[s] = spline;
					spline.nodes = new Vector3[coords.Count];
					for (int n=0; n<spline.nodes.Length; n++)
						spline.nodes[n] = matrix.PixelToWorld(coords[n].x-0.5f, coords[n].z-0.5f); //-0.5 since it should be between pixels, not in center 
				}

				return splines;
			}

			/*public static List<Vector2D> RelaxLine (List<Vector2D> line, float relax)
			{
				int lineCount = line.Count;

				Vector2D next = 

				for (int n=0; n<lineCount; n++)
				{

				}
			}*/

		#endregion


		#region Serpentine

	static int tmpIteration = 0;
	static int tmpNode = 0;
	static int tmpHighlightIteration = 0;
	public static int tmpHighlightNode = 0;
	static int tmpVariant = 0;

			public struct SerpentineFactors
			{
				public float incline;
				public float length;
				public float height;

				public void Normalize ()
				{
					float sum = length + incline + (height>0 ? height : -height);
					if (sum>0.0001f)
					{
						length = length/sum;
						incline = incline/sum;
						height = height/sum;
					}
				}
			}

			public static void Serpentine (MatrixWorld heights, Spline spline, float segLength, int iterations, SerpentineFactors factors)
			{
	DebugGizmos.Clear("Serp");

				factors.Normalize();
			
				float DistFn (Vector3 n1, Vector3 n2) => Mathf.Sqrt((n1.x-n2.x)*(n1.x-n2.x) + (n1.z-n2.z)*(n1.z-n2.z));

				for (int i=0; i<iterations; i++) 
				{
	tmpIteration = i;
	tmpHighlightIteration = iterations-1;

					spline.SubdivideDist(segLength, DistFn);
					Floor(heights, spline);
				

					CoordRect rect = heights.rect;
					Coord rectMin = heights.rect.offset; Coord rectMax = heights.rect.offset + heights.rect.size;
					float pixelSize = heights.PixelSize.x;

					Vector3[] newNodes = new Vector3[spline.nodes.Length];

					for (int n=1; n<spline.nodes.Length-1; n++)
					{
						//if (n>2) { newNodes[n]=spline.nodes[n]; continue; }

		tmpNode = n;

						float weight = GetNodeWeight(spline.nodes[n-1], spline.nodes[n], spline.nodes[n+1], factors, heights.worldSize.y);
						Vector3 moveVector = GetMoveVector(heights, 
							spline.nodes[n-1], spline.nodes[n], spline.nodes[n+1], 
							weight, 1, heights.PixelSize.x, factors);

						newNodes[n] = spline.nodes[n] + moveVector*3f;//*heights.PixelSize*0.2f;// * (2f/iterations);

						if (tmpHighlightIteration == i)
							DebugGizmos.DrawRay("Serp", newNodes[n], moveVector, Color.yellow, additive:true);
					}

					newNodes[0] = spline.nodes[0];
					newNodes[newNodes.Length-1] = spline.nodes[spline.nodes.Length-1];
					spline.nodes = newNodes;

					spline.Weld(segLength/2, DistFn);
				}
			}


			public static void SerpentineUnordered (MatrixWorld heights, Spline spline, float segLength, int iterations, SerpentineFactors factors)
			{
	DebugGizmos.Clear("Serp");

				factors.Normalize();
			
				float DistFn (Vector3 n1, Vector3 n2) => Mathf.Sqrt((n1.x-n2.x)*(n1.x-n2.x) + (n1.z-n2.z)*(n1.z-n2.z));

				CoordRect rect = heights.rect;
				Coord rectMin = heights.rect.offset; Coord rectMax = heights.rect.offset + heights.rect.size;
				float pixelSize = heights.PixelSize.x;

				for (int i=0; i<iterations; i++) 
				{
					spline.SubdivideDist(segLength, DistFn);
					Floor(heights, spline);

	tmpIteration = i;
	tmpHighlightIteration = iterations-1;

					//finding node with lowest rating
					float lowestRating = float.MaxValue;
					int lowestNum = 0;

					for (int n=1; n<spline.nodes.Length-1; n++)
					{
						float rating = GetNodeWeight(spline.nodes[n-1], spline.nodes[n], spline.nodes[n+1], factors, heights.worldSize.y);

						if (rating < lowestRating)
						{
							lowestRating = rating;
							lowestNum = n;
						}
					}

	tmpNode = lowestNum;

					float weight = GetNodeWeight(spline.nodes[lowestNum-1], spline.nodes[lowestNum], spline.nodes[lowestNum+1], factors, heights.worldSize.y);
					Vector3 moveVector = GetMoveVector(heights, 
						spline.nodes[lowestNum-1], spline.nodes[lowestNum], spline.nodes[lowestNum+1], 
						weight, 1, heights.PixelSize.x, factors);

					spline.nodes[lowestNum] += moveVector;//*heights.PixelSize*0.2f;// * (2f/iterations);

					spline.Weld(segLength/2, DistFn);
				}
			}


			private static Vector3 GetMoveVector (MatrixWorld heights, 
				Vector3 prev, Vector3 node, Vector3 next, 
				float thisWeight, int evalCount, float evalStep, SerpentineFactors factors)
			{
					Vector3 tan = (next - prev).normalized;
					Vector3 perp = Vector3.Cross(tan,Vector3.up);

					Vector3 minNode = node;
					float minWeight = thisWeight;

					float lWeights = 0;
					float rWeights = 0;

					float topLeftRating = 0;
					float topRightRating = 0;

					for (int p=1; p<=evalCount; p++)
					{
						Vector3 lNode = node + perp*(evalStep*p);
						lNode.y = heights.GetWorldInterpolatedValue(lNode.x, lNode.z) * heights.worldSize.y;
						float lRating = GetNodeWeight(prev, lNode, next, factors, heights.worldSize.y);
						//lWeight = lWeight*(1-(float)p/evalCount) + thisWeight*((float)p/evalCount);
						lWeights += lRating;
						if (lRating > topLeftRating) topLeftRating=lRating;

						Vector3 rNode = node - perp*(evalStep*p);
						rNode.y = heights.GetWorldInterpolatedValue(rNode.x, rNode.z) * heights.worldSize.y;
						float rRating = GetNodeWeight(prev, rNode, next, factors, heights.worldSize.y);
						//rWeight = rWeight*(1-(float)p/evalCount) + thisWeight*((float)p/evalCount);
						rWeights += rRating;
						if (rRating > topRightRating) topRightRating=rRating;
					}

					//return perp*(lWeights-rWeights);
					//return perp*(topLeftWeight-topRightWeight);
					if (topLeftRating > topRightRating  &&  topLeftRating > thisWeight)
						return perp * (topLeftRating-thisWeight);
					else if (topRightRating > topLeftRating  &&  topRightRating > thisWeight)
						return -perp * (topRightRating-thisWeight);
					else return Vector3.zero;
			}


			private static float GetNodeWeight (Vector3 prev, Vector3 node, Vector3 next, SerpentineFactors factors, float maxHeight)
			/// returns node candidate rating 0-infinity, lower is better
			{
				//incline
				Vector3 prevDelta = prev - node;
				float prevHorDist = ((Vector2D)prevDelta).Magnitude;
				float prevElevation = prevDelta.y>0 ? prevDelta.y : -prevDelta.y;
				float prevIncline = prevElevation / prevHorDist;

				Vector3 nextDelta = node - next;
				float nextHorDist = ((Vector2D)nextDelta).Magnitude;
				float nextElevation = nextDelta.y>0 ? nextDelta.y : -nextDelta.y;
				float nextIncline = nextElevation / nextHorDist;

				float inclineRating = prevIncline>nextIncline ? prevIncline : nextIncline;  //0 (planar), 1 (45 degree), to infinity (90 degree)
				inclineRating = 1 - Mathf.Atan(inclineRating) / (Mathf.PI/2f);  //1 (planar), 0.5 (45 degree), 0 (90 degree)
				//inclineRating *= inclineRating;

				//length
				Vector2D vec = ((Vector2D)(prev-next)).Normalized;
				float prevDot = Vector2D.Dot(vec, ((Vector2D)(node-prev)).Normalized);
				float nextDot = Vector2D.Dot(vec, ((Vector2D)(node-next)).Normalized);

				float length = ((Vector2D)(prev-next)).Magnitude;
				float nLength = ((Vector2D)(prev-node)).Magnitude + ((Vector2D)(node-next)).Magnitude;

				float lengthRating = length / nLength;  //1 (straight), 0.7071 (90 degree), 0 (sharp corner)
				//lengthRating *= lengthRating;

				//height
				float heightRating = node.y/maxHeight;
				if (factors.height < 0) heightRating = 1-heightRating;

			//	if (lengthFactor < 1) lengthFactor = 1; //now 1 to 2 only
			//	lengthFactor -= 1; //now 0 to 1;
			//	if (lengthFactor == 1) lengthFactor = float.MaxValue;
			//	else lengthFactor = 1 / (1-lengthFactor); //now 0 to infinity

				float rating = inclineRating*factors.incline + lengthRating*factors.length + heightRating*Mathf.Abs(factors.height);
					//Mathf.Pow(inclineRating, (1-factors.incline)) * Mathf.Pow(lengthRating, (1-factors.length));

				if (tmpIteration==tmpHighlightIteration && tmpNode==tmpHighlightNode)
				{
					DebugGizmos.DrawLabel("Serp", node,
						"Node:" + tmpNode + " var:" + tmpVariant + 
						"\nInc:" + inclineRating.ToString() + "(" + prevIncline.ToString() + ", " + nextIncline.ToString() + ")" +
						"\nLen:" + lengthRating.ToString() + //"(" + length + ", " + nLength + ")" +
						"\nSum:" + rating.ToString(),
					additive:true);

					DebugGizmos.DrawLine("Serp", node, prev, Color.red, additive:true);

					DebugGizmos.DrawDot("Serp", node, 6, Color.red, additive:true);
				}

				return rating;
			}
	
		#endregion
	}
}