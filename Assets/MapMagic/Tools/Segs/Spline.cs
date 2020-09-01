using System;
using System.Collections.Generic;
using UnityEngine;

namespace Den.Tools.Segs
{ 
	[Serializable] 
	public class Spline
	{
		public Vector3[] nodes;
		public int count;

		public Spline () { nodes = new Vector3[0]; count = 0; }
		public Spline (int nodeCount) { nodes = new Vector3[nodeCount]; count = nodeCount; }
		public Spline (Spline src) { nodes = ArrayTools.Copy(src.nodes); count = src.count; }
		public Spline (Vector3 start, Vector3 end) { nodes = new Vector3[2] {start,end}; count = 2; }
		public Spline (Vector3[] nodes) { this.nodes = nodes; count = nodes.Length; }


		public Vector3 Min
		{get{
			Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
			for (int n=0; n<nodes.Length; n++)
			{
				if (nodes[n].x < min.x) min.x = nodes[n].x;
				if (nodes[n].y < min.y) min.y = nodes[n].y;
				if (nodes[n].z < min.z) min.z = nodes[n].z;
			}
			return min;
		}}

		public Vector3 Max
		{get{
			Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
			for (int n=0; n<nodes.Length; n++)
			{
				if (nodes[n].x > max.x) max.x = nodes[n].x;
				if (nodes[n].y > max.y) max.y = nodes[n].y;
				if (nodes[n].z > max.z) max.z = nodes[n].z;
			}
			return max;
		}}


		public Vector3 GetPoint (int n, float p)
		{
			return nodes[n]*(1-p) + nodes[n+1]*p;
		}

		public void Split (int n, float p)
		{
			Vector3 point = GetPoint(n,p);
			ArrayTools.Insert(ref nodes, n+1, point); //new node position will be n+1
		}

		public void RemoveNode (int n) => ArrayTools.RemoveAt(ref nodes, n);


		public void SubdivideDist (float maxDist, Func<Vector3,Vector3,float> distFn=null)
		/// Adds nodes so that the no distances between nodes is larger than maxDist
		{
			List<Vector3> newNodes = new List<Vector3>();
			for (int n=0; n<nodes.Length-1; n++)
			{
				newNodes.Add(nodes[n]);

				float dist = distFn==null ?
					(nodes[n] - nodes[n+1]).magnitude :
					distFn(nodes[n],nodes[n+1]);
				
				if (dist > maxDist)
				{
					int subdivs = (int)(dist / maxDist);
					for (int i=1; i<=subdivs; i++)
					{
						float p = (float)i / (subdivs+1);
						newNodes.Add( GetPoint(n,p) );
					}
				}
			}

			newNodes.Add(nodes[nodes.Length-1]);

			//if (newNodes.Count != nodes.Length) //if changed
				nodes = newNodes.ToArray();
		}


		public void Weld (float threshold, Func<Vector3,Vector3,float> distFn=null, bool keepStartEnd=true)
		/// Combines all closely placed nodes in one
		{
			List<Vector3> newNodes = new List<Vector3>();

			Vector3 weldSum = nodes[0];
			int weldCount = 1;

			for (int n=1; n<nodes.Length; n++)
			{
				float dist = distFn==null ?
					(weldSum/weldCount - nodes[n]).magnitude :
					distFn(weldSum/weldCount, nodes[n]);

				if (dist < threshold)
				{
					weldSum += nodes[n];
					weldCount ++;
				}

				else
				{
					newNodes.Add(weldSum / weldCount);

					weldSum = nodes[n];
					weldCount = 1;
				}
			}

			newNodes.Add(weldSum / weldCount); //last one
			
			if (keepStartEnd)
			{
				newNodes[0] = nodes[0];
				newNodes[newNodes.Count-1] = nodes[nodes.Length-1];
			}

			//if (newNodes.Count != nodes.Length) //if changed
				nodes = newNodes.ToArray();
		}


		public void Relax (float blur, int iterations) 
		/// Moves nodes to make the spline smooth
		/// Works with auto-tangents only
		{
			for (int i=0; i<iterations; i++)
				Relax(blur);
		}

		public void Relax (float blur)
		/// Moves nodes to make the spline smooth
		/// Works with auto-tangents only
		{
			Vector3 prev = nodes[0];
			for (int n=1; n<nodes.Length-1; n++)
			{
				Vector3 relPos = (prev + nodes[n+1])/2;
				prev = nodes[n];
				nodes[n] = relPos*blur + nodes[n]*(1-blur);
			}
		}


		public void Optimize (float deviation)
		/// Removes those nodes that should not change the shape a lot
		{
			int iterations = nodes.Length-2; //in worst case should remove all but start/end
			if (iterations <= 0) return;

			List<Vector3> newNodes = new List<Vector3>(nodes.Length);
			newNodes.AddRange(nodes);

			for (int i=0; i<iterations; i++) //using recorded itterations since nodes count will change
			{
				float minDeviation = float.MaxValue;
				int minN = -1;

				for (int n=1; n<newNodes.Count-1; n++)
				{
					//checking how far point placed from node-node line
					float currDistToLine = newNodes[n].DistanceToLine(newNodes[n-1], newNodes[n+1]);
					//float currLine = (nodes[n-1] - nodes[n+1]).magnitude;
					//float currDeviation = (currDistToLine*currDistToLine) / currLine;
					float currDeviation = currDistToLine;

					if (currDeviation < minDeviation)
					{
						minN = n;
						minDeviation = currDeviation;
					}
				}

				if (minDeviation > deviation) break;

				newNodes.RemoveAt(minN);
			}

			nodes = newNodes.ToArray();
		}


		public float Handness (Vector2D point)
		/// Determines wheter the point is on the left or on the right of a spline (from top view)
		/// returns either positiove (left) or negative (right) value;
		{
			(int n, float p, float dist) = ClosestToPoint(point);
			return (point.x-nodes[n].x)*(nodes[n+1].z-nodes[n].z) - (point.z-nodes[n].z)*(nodes[n+1].x-nodes[n].x);
		}


		public static float Handness (Spline[] splines, Vector2D point)
		/// Same for multiple lines
		{
			(int s, int n, float p, float dist) = ClosestSplineToPoint(splines, (Vector3)point, horizontalOnly:true);
			Spline spline = splines[s];
			return (point.x-spline.nodes[n].x)*(spline.nodes[n+1].z-spline.nodes[n].z) - (point.z-spline.nodes[n].z)*(spline.nodes[n+1].x-spline.nodes[n].x);
		}


		public void SplitNearPoints (Vector3[] points, float[] ranges, bool horizontalOnly=false)
		{
			Vector3 min = Min;
			Vector3 max = Max;

			for (int p=0; p<points.Length; p++)
			{
				Vector3 point = points[p];
				float range = ranges[p];

				if (point.x+range < min.x ||  (!horizontalOnly && point.y+range < min.y)  ||  point.z+range < min.z  ||
					point.x-range > max.x  ||  (!horizontalOnly && point.y-range > max.y)  ||  point.z-range > max.z)
						continue;

				//if within range from a node - skipping (already splitted here)
				bool splitted = false;
				for (int n=0; n<nodes.Length; n++)
				{
					float distSq = horizontalOnly ?
						(nodes[n].x-point.x)*(nodes[n].x-point.x) + (nodes[n].z-point.z)*(nodes[n].z-point.z) :
						(nodes[n].x-point.x)*(nodes[n].x-point.x) + (nodes[n].y-point.y)*(nodes[n].y-point.y) + (nodes[n].z-point.z)*(nodes[n].z-point.z);
					if (distSq < range*range)
						{ splitted=true; break; }
				}
				if (splitted)
					continue;

				(int node, float percent, float dist) = ClosestToPoint(point, horizontalOnly);
				if (dist < range)
					Split(node,percent);
			}
		}


		public void Push (Vector3[] points, float[] ranges, float intensity=1, float nodesPointsRatio=0, bool horizontalOnly=true)
		/// Moves points away from the segment so that they lay no closer than range
		/// If nodesPointsRatio = 0 pushing nodes only, if 1 - pushing points only
		/// Changes the points array (if nodesPointsRatio!=0)
		/// DistFactor multiplies the range to move point (for iteration push it should be less than 1)
		{
			for (int n=0; n<nodes.Length-1; n++)
			{
				Vector3 min = Vector3.Min(nodes[n], nodes[n+1]);
				Vector3 max = Vector3.Max(nodes[n], nodes[n+1]);
				float length = (nodes[n] - nodes[n+1]).magnitude;

				Vector3 start = nodes[n]; //will change the nodes position, thus keeping original values
				Vector3 end = nodes[n+1];

				for (int p=0; p<points.Length; p++)
				{
					Vector3 point = points[p];
					float range = ranges[p];

					if (min.x > point.x+range || max.x < point.x-range ||
						min.z > point.z+range || max.z < point.z-range ||
						(!horizontalOnly && (min.y > point.y+range || max.y < point.y-range)))
							continue;

					Vector3 closestPoint = PointNearestToPos(point, start, end);
					closestPoint = ClampPointToSegment(closestPoint, start, end);
					float percent = (closestPoint-start).magnitude / length;
					percent = 2*percent*percent*percent - 3*percent*percent + 2*percent;

					DebugGizmos.DrawLine("Push1", start, end);
					DebugGizmos.DrawDot("Push", closestPoint, 6);

					Vector3 pushVector = PushVector(point, closestPoint, range, horizontalOnly) * intensity;

					points[p] += pushVector * nodesPointsRatio;

					float pStart = (1-percent)*2;  if (pStart>1) pStart = 1;
					nodes[n] -= pushVector*(1-nodesPointsRatio) * pStart;

					float pEnd = percent*2;  if (pEnd>1) pEnd = 1;
					nodes[n+1] -= pushVector*(1-nodesPointsRatio) * pEnd;
				}
			}
		}


		private Vector3 PushVector (Vector3 point, Vector3 otherPoint, float otherRange, bool horizontalOnly=true)
		{
			Vector3 awayDelta = otherPoint-point; 
			if (horizontalOnly) awayDelta.y = 0;

			Vector3 awayDir = awayDelta.normalized; //awayDelta/lineDist
			float dist = awayDelta.magnitude;
			float distLeft = otherRange-dist;
				
			if (dist > otherRange) return new Vector3(0,0,0);
			else return -awayDir*distLeft;
		}


		#region Distance To Line

			public (int n, float p, float dist) ClosestToPoint (Vector3 pos, bool horizontalOnly=false)
			/// Returns the coordinate of a point on the spline that is closest to pos
			{
				int cn = -1;
				float cp = 0;
				float cDist = float.MaxValue;
				if (horizontalOnly) pos.y = 0;

				for (int n=0; n<nodes.Length-1; n++)
				{
					Vector3 start = nodes[n];
					Vector3 end = nodes[n+1];
					if (horizontalOnly) { start.y=0; end.y=0; }

					//skipping if pos is out of bounds+dist
					Vector3 segMin = Vector3.Min(start, end);
					Vector3 segMax = Vector3.Max(start, end);

					if (segMin.x > pos.x+cDist  ||  (!horizontalOnly && segMin.y > pos.y+cDist)  ||  segMin.z > pos.z+cDist  ||
						segMax.x < pos.x-cDist  ||  (!horizontalOnly && segMax.y < pos.y-cDist)  ||  segMax.z < pos.z-cDist)
							continue;

					Vector3 closestPoint = PointNearestToPos(pos, start, end);
					closestPoint = ClampPointToSegment(closestPoint, start, end);

					float dist = (closestPoint-pos).magnitude;
					if (dist<cDist)
					{
						cn = n;
						cp = (closestPoint-start).magnitude / (end-start).magnitude;
						cDist = dist;
					}
				}

				return (cn, cp, cDist);
			}


			public (int n, float p, float dist) ClosestToPoint (Vector2D pos) => ClosestToPoint((Vector3)pos, horizontalOnly:true);


			public (int n, float p, float dist) ClosestToRay (Ray ray)
			/// Returns the coordinate of a point on the spline that is closest to infinite ray
			/// Ray should be normalized
			{
				int cn = 0;
				float cp = 0;
				float cDist = float.MaxValue;

				for (int n=0; n<nodes.Length-1; n++)
				{
					Vector3 closestPoint = PointNearestToRay(ray, nodes[n], nodes[n+1]);
					closestPoint = ClampPointToSegment(closestPoint, nodes[n], nodes[n+1]);

					//and finding distance from closest point back to line
					Vector3 backPoint = PointNearestToPos(closestPoint, ray.origin, ray.origin+ray.direction);
					if (((backPoint-ray.origin).normalized - ray.direction).sqrMagnitude > 1) 
						backPoint = ray.origin;

					//DebugGizmos.DrawLine("Yellow " + n, backPoint, closestPoint, Color.yellow);

					float dist = (closestPoint-backPoint).magnitude;
					if (dist<cDist)
					{
						cn = n;
						cp = (closestPoint-nodes[n]).magnitude / (nodes[n+1]-nodes[n]).magnitude;
						cDist = dist;
					}
				}

				return (cn, cp, cDist);
			}


			private Vector3 PointNearestToPos (Vector3 pos, Vector3 segStart, Vector3 segEnd)
			/// Finds a point on a segment that is nearest to the given pos
			{
				Vector3 segVec = segStart - segEnd;
				Vector3 posVec = pos - segStart;
				float segVecMagnitude = segVec.magnitude;

				float percent = Vector3.Dot(posVec, segVec) / (segVecMagnitude*segVecMagnitude);
			
				return segVec*percent + segStart;
			}

			private Vector3 PointNearestToRay (Ray ray, Vector3 segStart, Vector3 segEnd)
			/// Finds a point on a segment that is nearest to the given infinite ray
			/// Ray should be normalized
			/// Borrowed from stackoverflow, I'll promise to return it back when it's not needed. Thanks 16807
			{
				Vector3 lineVec = ray.direction; //(lineEnd-lineStart).normalized;
				Vector3 segVec = (segEnd-segStart).normalized;
				Vector3 deltaVec = segStart-ray.origin; //-lineStart;

				Vector3 crossVec = Vector3.Cross(segVec,lineVec).normalized;
				Vector3 proj = Vector3.Dot(deltaVec, lineVec) * lineVec;
				Vector3 rej = deltaVec 
					- Vector3.Dot(deltaVec, lineVec) * lineVec 
					- Vector3.Dot(deltaVec, crossVec) * crossVec;
				float rejMagnitude = rej.magnitude;
				return segStart - segVec*rejMagnitude / Vector3.Dot(segVec,rej/rejMagnitude);
			}
		
			private Vector3 ClampPointToSegment (Vector3 point, Vector3 segStart, Vector3 segEnd)
			/// If pointNearest is located before the segment start or after segment end resturing it to start or to end
			{
				Vector3 segVec = segEnd - segStart;
				float segVecMagnitude = segVec.magnitude;
				Vector3 segVecNormalized = segVec / segVecMagnitude;

				float percentFromStart = (segStart-point).magnitude / segVecMagnitude;
				if (percentFromStart >= 1) return segEnd - segVec*0.0001f; //-segVec to avoid pointing to vert itself

				float percentFromEnd = (segEnd-point).magnitude / segVecMagnitude;
				if (percentFromEnd >= 1) return segStart + segVec*0.0001f;

				return point;
			}

			public static (int s, int n, float p, float dist) ClosestSplineToPoint (Spline[] splines, Vector3 pos, bool horizontalOnly=false)
			{
				int cs = 0;
				int cn = -1;
				float cp = 0;
				float cDist = float.MaxValue;
				if (horizontalOnly) pos.y = 0;

				for (int s=0; s<splines.Length; s++)
				{
					(int n, float p, float dist) = splines[s].ClosestToPoint(pos, horizontalOnly);
					if (dist < cDist)
						{ cs=s;  cn=n; cp=p; cDist=dist; }
				}

				return (cs, cn, cp, cDist);
			}

		#endregion


		#region Cut

			public enum CutAxis { AxisX, AxisZ }
			public enum CutSide { Negative=-1, Positive=1 }

			/*public void CutByRect (Vector3 pos, Vector3 size)
			/// Splits all segments so that each intersection with AABB rect has a node
			{			
				for (int i=0; i<13; i++) //spline could be divided in 12 parts maximum
				{
					List<Vector3> newNodes = new List<Vector3>();

					for (int n=0; n<nodes.Length-1; n++)
					{
						//early check - if inside/outside rect
						Vector3 min = Vector3.Min(nodes[n], nodes[n+1]);
						Vector3 max = Vector3.Max(nodes[n], nodes[n+1]);

						if (max.x < pos.x  ||  min.x > pos.x+size.x ||
							max.z < pos.z  ||  min.z > pos.z+size.z) 
								{ newNodes.Add(nodes[n+1]); continue; } //fully outside
						if (min.x > pos.x  &&  max.x < pos.x+size.x &&
							min.z > pos.z  &&  max.z < pos.z+size.z) 
								{ newNodes.Add(nodes[n+1]); continue; } //fully inside

						//splitting
						float sp = segments[s].IntersectRect(pos, size);
						if (sp < 0.0001f  ||  sp > 0.999f) 
							{ newSegments.Add(segments[s]); continue; }  //no intersection

						(Segment s1, Segment s2) = segments[s].GetSplitted(sp);
						newSegments.Add(s1);
						newSegments.Add(s2);
					}

					bool segemntsCountChanged = segments.Length != newSegments.Count;
					segments = newSegments.ToArray();
					if (!segemntsCountChanged) break; //if no nodes added - exiting 12 iterations
				}
			}*/


			public void CutAA (float val, CutAxis axis)
			/// Cuts the line creating points on horizontal line with X coordinate
			{
				List<Vector3> newNodes = new List<Vector3>(capacity: nodes.Length) {
					nodes[0] };

				for (int n=0; n<nodes.Length-1; n++)
				{
					float pos0 = axis==CutAxis.AxisX ? nodes[n].x : nodes[n].z;
					float pos1 = axis==CutAxis.AxisX ? nodes[n+1].x : nodes[n+1].z;

					// early check - on one side only
					if (pos0 < val  && pos1 < val) { newNodes.Add(nodes[n+1]); continue; }
					if (pos0 > val  &&  pos1 > val) { newNodes.Add(nodes[n+1]); continue; }

					// cutting
					float percent = (pos0 - val) / (pos0 - pos1);
					newNodes.Add( new Vector3(
						nodes[n].x*(1-percent) + nodes[n+1].x*percent,
						nodes[n].y*(1-percent) + nodes[n+1].y*percent,
						nodes[n].z*(1-percent) + nodes[n+1].z*percent ) );

					newNodes.Add(nodes[n+1]);
				}

				nodes = newNodes.ToArray();
			}


			public Spline[] RemoveOuterAA (float val, CutAxis axis, CutSide side)
			/// Removes segment if any of the segment nodes is less than x
			/// Will split spline in several, returns new splitted splines
			/// Add a bit (or subtract if side is negative) from x when using together wit Cut:  +0.0001f*(int)side
			{
				List<Spline> newSplines = new List<Spline>();

				List<Vector3> currSpline = null;

				for (int n=0; n<nodes.Length; n++)
				{
					bool isOuter = axis==CutAxis.AxisX ? nodes[n].x > val : nodes[n].z > val;
					if (side == CutSide.Negative) isOuter = !isOuter;

					// starting new spline
					if (currSpline == null)
					{
						if (!isOuter)
						{
							currSpline = new List<Vector3>();
							currSpline.Add(nodes[n]);
						}

						//ignoring if under x
					}

					else
					{
						//ending spline
						if (isOuter)
						{
							newSplines.Add( new Spline(currSpline.ToArray()) );
							currSpline = null;
						}

						//adding node
						else
							currSpline.Add(nodes[n]);
					}		
				}

				if (currSpline!=null)
					newSplines.Add( new Spline(currSpline.ToArray()) );

				return newSplines.ToArray();
			}


			public void CutAABB (Vector2D pos, Vector2D size)
			{
				CutAA(pos.x, CutAxis.AxisX);
				CutAA(pos.x+size.x, CutAxis.AxisX);
				CutAA(pos.z, CutAxis.AxisZ);
				CutAA(pos.z+size.z, CutAxis.AxisZ);
			}


			public void RemoveOuterAABB (Vector2D pos, Vector2D size)
			{
				RemoveOuterAA(pos.x - 0.0001f, CutAxis.AxisX, CutSide.Negative);
				RemoveOuterAA(pos.x+size.x + 0.0001f, CutAxis.AxisX, CutSide.Positive);
				RemoveOuterAA(pos.z - 0.0001f, CutAxis.AxisZ, CutSide.Negative);
				RemoveOuterAA(pos.z+size.z + 0.0001f, CutAxis.AxisZ, CutSide.Positive);
			}

		#endregion


		#region Interlink

			private struct LinkIds { public int id1; public int id2; public LinkIds(int i1, int i2) {id1=i1; id2=i2;} }

			private struct Link
			{
				public int p1;
				public int p2;

				public Link (int p1, int p2)  { this.p1=p1; this.p2=p2; }
				public override int GetHashCode () => p1<<1 + p2;
			}

			public static Spline[] GabrielGraph (PosTab objs, int maxLinks=4, int triesPerObj=8)
			{
				List<Spline> splines = new List<Spline>();

				triesPerObj = Mathf.Min(objs.totalCount-1, triesPerObj);

				Dictionary<int,int[]> closestMap = new Dictionary<int, int[]>();

				//bool CheckIfWithin (Transition trs) 
				//{
				//	return  trs.pos.x >= worldPos.x  &&  trs.pos.x <= worldPos.x+worldSize.x  &&
				//			trs.pos.z >= worldPos.z  &&  trs.pos.z <= worldPos.x+worldSize.z;
				//}

				//filling closest map
				foreach (Transition trs in objs.All())
				{
					int[] closestIds = new int[triesPerObj];
					closestMap.Add(trs.id, closestIds);

					float minDist = 0.001f;
					for (int i=0; i<triesPerObj; i++)
					{
						Transition closest = objs.Closest(trs.pos.x, trs.pos.z, minDist); //, filterFn:CheckIfWithin);

						float curDistSq = (trs.pos.x-closest.pos.x)*(trs.pos.x-closest.pos.x) + (trs.pos.z-closest.pos.z)*(trs.pos.z-closest.pos.z);
						minDist = Mathf.Sqrt(curDistSq)  + 0.001f;

						closestIds[i] = closest.id;
					}

					//maybe could speed up by creating a list of points nearby and then sorting theese points by distance
				}

				//connecting
				HashSet<LinkIds> connections = new HashSet<LinkIds>();
				Dictionary<int,int> idToLinksCount = new Dictionary<int,int>();
				foreach (var kvp in closestMap)
				{
					int id1 = kvp.Key;
					int[] closestIds1 = kvp.Value;

					for (int num1=0; num1<closestIds1.Length; num1++)
					{
						int id2 = closestIds1[num1];
						if (id2 == 0) continue; // no more objects left during closest map

						int[] closestIds2 = closestMap[id2];
						int num2 = closestIds2.Find(id1);

						//if id1 not contains in closestIds2
						if (num2 < 0) continue; 

						//if this link was not created earlier
						if (connections.Contains( new LinkIds(id1, id2) ) || 
							connections.Contains( new LinkIds(id2, id1) ) )
								continue;

						//if there is no common ids before num1 and num2
						bool hasCommonNodeCloser = false; //SNIPPET: the ideal case of using GOTO
						for (int i=0; i<num1; i++)
						{
							for (int j=0; j<num2; j++)
								if (closestIds1[i] == closestIds2[j]) { hasCommonNodeCloser = true; break; }
							if (hasCommonNodeCloser) break;
						}
						if (hasCommonNodeCloser) continue;

						//if the maximum number of connections reached
						idToLinksCount.TryGetValue(id1, out int linksCount1);
						idToLinksCount.TryGetValue(id2, out int linksCount2);
						if (linksCount1 >= maxLinks || linksCount2 >= maxLinks)
							continue;

						connections.Add( new LinkIds(id1, id2) );
						idToLinksCount.ForceAdd(id1, linksCount1 + 1);
						idToLinksCount.ForceAdd(id2, linksCount2 + 1);
					}
				}

				//converting connection links to positions
				Dictionary<int,Vector3> idToPos = new Dictionary<int, Vector3>();
				foreach (Transition trs in objs.All())
					idToPos.Add(trs.id, trs.pos);

				foreach (LinkIds ids in connections)
				{
					Vector3 pos1 = idToPos[ids.id1];
					Vector3 pos2 = idToPos[ids.id2];

					Spline line = new Spline(pos1, pos2);
					splines.Add(line);
				}

				return splines.ToArray();
			}


			public static Spline[] GabrielGraph (Vector3[] poses, int maxLinks=4, int triesPerObj=8)
			{
				triesPerObj = Mathf.Min(poses.Length-1, triesPerObj);

				int[][] closestMap = new int[poses.Length][];

				//bool CheckIfWithin (Transition trs) 
				//{
				//	return  trs.pos.x >= worldPos.x  &&  trs.pos.x <= worldPos.x+worldSize.x  &&
				//			trs.pos.z >= worldPos.z  &&  trs.pos.z <= worldPos.x+worldSize.z;
				//}

				//filling closest map
				for (int p=0; p<poses.Length; p++)
				{
					int[] closestIds = new int[triesPerObj];
					closestMap[p] = closestIds;

					float[] closestDists = new float[triesPerObj];
					for (int i=0; i<closestDists.Length; i++)
						closestDists[i] = float.MaxValue;

					for (int p2=0; p2<poses.Length; p2++)
					{
						if (p==p2) continue;

						float dist = (poses[p]-poses[p2]).sqrMagnitude;
						for (int i=0; i<closestDists.Length; i++)
						{
							if (dist < closestDists[i])
							{
								closestDists.InsertRemoveLast(i, dist);
								closestIds.InsertRemoveLast(i, p2);
								break;
							}
						}
					}
				}

				//TODO: split in filling closest and gabriel

				//connecting
				HashSet<Link> links = new HashSet<Link>();
				int[] linksPerPos = new int[poses.Length];

				for (int p1=0; p1<closestMap.Length; p1++)
				{
					int[] closestP1 = closestMap[p1];

					foreach (int p2 in closestP1)
					{
						//if this link was not created earlier
						if (links.Contains( new Link(p1, p2) ) || 
							links.Contains( new Link(p2, p1) ) )
								continue;

						int[] closestP2 = closestMap[p2];

						//if there is no common ids before p1 and p2
						bool hasCommonNodeCloser = false; //SNIPPET: the ideal case of using GOTO
						for (int i=0; i<closestP1.Length; i++)
						{
							if (closestP1[i] == p2)
								break; //standard exit

							for (int j=0; j<closestP2.Length; j++)
							{
								if (closestP2[j] == p1)
									break; //standard exit

								if (closestP1[i] == closestP2[j]) 
									{ hasCommonNodeCloser = true; break; }
							}
							if (hasCommonNodeCloser) break;
						}
						if (hasCommonNodeCloser) continue;

						//if the maximum number of connections reached
						if (linksPerPos[p1] >= maxLinks || linksPerPos[p2] >= maxLinks)
							continue;

						links.Add( new Link(p1, p2) );
						linksPerPos[p1]++;
						linksPerPos[p2]++;
					}
				}

				//converting connection links to positions
				Spline[] splines = new Spline[links.Count];
				int counter = 0;
				foreach (Link link in links)
				{
					splines[counter] = new Spline(poses[link.p1], poses[link.p2]);
					counter++;
				}

				return splines;
			}

		#endregion


		#region Weld Links

			public static Spline[] ReWeld (Spline[] splines, float threshold)
			/// Splits splines to segments and then welds them properly
			/// Useful for welding splines, removing duplicating segments, etc
			{
				((int,int)[] links, Vector3[] nodes) = SplitToLinks(splines);
				StructArrayExtended<int>[] mergeGroups = CompileMergeLists(nodes, threshold);
				MergePoints(nodes, mergeGroups);
				MergeLinks(links, nodes, mergeGroups);
				RemoveDuplicatingLinks(ref links);
				return WeldLinks(links, nodes);
			}


			public static Spline[] WeldClose (Spline[] splines, float threshold)
			/// Will weld two splines if they are going within threshold
			/// TODO: will not create points on intersections
			{
				//lines AABB for skipping
				(Vector3 min,Vector3 max)[] splinesMinMax = new (Vector3,Vector3)[splines.Length];
				for (int s=0; s<splinesMinMax.Length; s++)
					splinesMinMax[s] = (splines[s].Min, splines[s].Max);

				//universal ranges array
				int maxCount = 0;
				foreach (Spline spline in splines)
					if (spline.nodes.Length > maxCount)
						maxCount = spline.nodes.Length;

				float[] ranges = new float[maxCount];
				for (int i=0; i<maxCount; i++)
					ranges[i] = threshold;
				
				//splitting
				Spline[] splitted = new Spline[splines.Length]; //using splines clone to avoid modify originals
				for (int s=0; s<splines.Length; s++)
					splitted[s] =  new Spline(splines[s]);
				
				for (int s=0; s<splines.Length; s++)
				{
					Spline spline = splitted[s];

					Vector3 min = splinesMinMax[s].min;
					Vector3 max = splinesMinMax[s].max;

					for (int s2=0; s2<splines.Length; s2++)
					{
						if (s == s2)
							continue;

						if (max.x+threshold < splinesMinMax[s2].min.x  ||  min.x-threshold > splinesMinMax[s2].max.x  ||
							max.z+threshold < splinesMinMax[s2].min.z  ||  min.z-threshold > splinesMinMax[s2].max.z )
								continue;

						spline.SplitNearPoints(splines[s2].nodes, ranges, horizontalOnly:true);
					}
				}

				return ReWeld(splitted, threshold);
			}


			public static ((int,int)[] links, Vector3[] nodes) SplitToLinks (Spline[] splines)
			{
				int linksCount = 0;
				int nodesCount = 0;
				foreach (Spline spline in splines)
				{
					linksCount += spline.nodes.Length-1;
					nodesCount += spline.nodes.Length;
				}

				(int,int)[] links = new (int,int)[linksCount];
				Vector3[] nodes = new Vector3[nodesCount];

				int nodeCounter=0;
				int linkCounter = 0;
				foreach (Spline spline in splines)
				{
					for (int n=0; n<spline.nodes.Length-1; n++) //except last one
					{
						nodes[nodeCounter] = spline.nodes[n];
						nodeCounter++;

						links[linkCounter] = (nodeCounter-1, nodeCounter);
						linkCounter++;
					}

					nodes[nodeCounter] = spline.nodes[spline.nodes.Length-1]; //last one
					nodeCounter++;
				}

				return (links, nodes);
			}


			private static StructArrayExtended<int>[] CompileMergeLists (Vector3[] nodes, float threshold, bool horizontalOnly=true)
			/// Returns arrays of merge group indexes
			/// TODO: to object ops
			{
				bool[] merged = new bool[nodes.Length];
				StructArrayExtended<int>[] mergeGroups = new StructArrayExtended<int>[nodes.Length];

				//TODO: use spatial hash
				for (int n=0; n<nodes.Length; n++)
				{
					if (merged[n]) continue;
					merged[n] = true;
				
					mergeGroups[n].Add(n);

					Vector3 mergePos = nodes[n];
					int mergeSum = 1;
					Vector3 mergeCenter = nodes[n];

					for (int n2=0; n2<nodes.Length; n2++)
					{
						if (merged[n2]) continue;

						float dist = horizontalOnly ?
							(nodes[n2].x-mergeCenter.x)*(nodes[n2].x-mergeCenter.x) + (nodes[n2].z-mergeCenter.z)*(nodes[n2].z-mergeCenter.z) :
							(nodes[n2]-mergeCenter).sqrMagnitude;

						if (dist < threshold*threshold)
						{
							mergeGroups[n].Add(n2);
							mergePos += nodes[n2];
							mergeSum ++;
							mergeCenter = mergePos/mergeSum;

							merged[n2] = true;
						}
					}
				}

				//checks whether there is a duplicate index in merge groups
				merged.Fill(false);
				for (int n=0; n<nodes.Length; n++)
					for (int i=0; i<mergeGroups[n].count; i++)
					{
						if (merged[ mergeGroups[n][i] ])
							throw new Exception("Duplicate index in merge groups");
						merged[ mergeGroups[n][i] ] = true;
					}

				return mergeGroups;
			}


			private static void MergePoints (Vector3[] nodes, StructArrayExtended<int>[] mergeGroups)
			/// Places nodes at the merge group's average pos
			{
				for (int n=0; n<nodes.Length; n++)
				{
					if (mergeGroups[n].count < 2) continue;

					Vector3 center = Vector3.zero;
					for (int i=0; i<mergeGroups[n].count; i++)
						center += nodes[ mergeGroups[n][i] ];

					center /= mergeGroups[n].count;

					for (int i=0; i<mergeGroups[n].count; i++)
						nodes[ mergeGroups[n][i] ] = center;
				}
			}


			private static void MergeLinks ((int,int)[] links, Vector3[] nodes, StructArrayExtended<int>[] mergeGroups)
			{
				//lut from old index to first merged one
				int[] replaceLut = new int[nodes.Length];
				for (int n=0; n<nodes.Length; n++)
				{
					if (mergeGroups[n].count == 0)
						continue; //do not overwrite merged nodes

					if (mergeGroups[n].count == 1)
						{ replaceLut[n] = n; continue; }

					int first = mergeGroups[n][0];
					for (int i=0; i<mergeGroups[n].count; i++)
						replaceLut[ mergeGroups[n][i] ] = first;
				}

				//replacing numbers in links
				for (int l=0; l<links.Length; l++)
				{
					links[l].Item1 = replaceLut[ links[l].Item1 ];
					links[l].Item2 = replaceLut[ links[l].Item2 ];
				}
			}


			private static void RemoveDuplicatingLinks (ref (int,int)[] links)
			{
				//removing duplicating links
				bool[] duplicating = new bool[links.Length];
				int duplicatingNum = 0;
				HashSet<long> linksHashes = new HashSet<long>();
				for (int l=0; l<links.Length; l++)
				{
					if (links[l].Item1 == links[l].Item2)
						{ duplicating[l] = true; duplicatingNum++; continue; }

					int min = links[l].Item1<links[l].Item2 ? links[l].Item1 : links[l].Item2;
					int max = links[l].Item1>links[l].Item2 ? links[l].Item1 : links[l].Item2;

					long hash = ((long)min)<<31 | (long)max;  //31 since 32 is sign
					if (linksHashes.Contains(hash))
						{ duplicating[l] = true; duplicatingNum++; continue; }
					linksHashes.Add(hash);
				}
				
				(int,int)[] newLinks = new (int,int)[links.Length-duplicatingNum];
				int c = 0;
				for (int l=0; l<links.Length; l++)
					if (!duplicating[l])
					{
						newLinks[c] = links[l];
						c++;
					}
				links = newLinks;
			}


			public static Spline[] WeldLinks ((int,int)[] links, Vector3[] nodes)
			{
				List<Spline> splines = new List<Spline>();

				bool[] linksWelded = new bool[links.Length];
				int linksWeldedCount = 0;

				int[] linksPerNode = new int[nodes.Length];
				for (int l=0; l<links.Length; l++)
				{
					linksPerNode[links[l].Item1]++;
					linksPerNode[links[l].Item2]++;
				}

				//for all standard links (the ones that have 2 connections) creating nodeNum -> linkNum lut
				//actually including all, but they won't be used anyways
				int[] numToLinkLut1 = new int[nodes.Length];
				int[] numToLinkLut2 = new int[nodes.Length]; //2 arrays: one for start, one for end

				numToLinkLut1.Fill(-1);
				for (int l=0; l<links.Length; l++)
				{
					if (numToLinkLut1[links[l].Item1] < 0)
						numToLinkLut1[links[l].Item1] = l;
					else
						numToLinkLut2[links[l].Item1] = l;

					if (numToLinkLut1[links[l].Item2] < 0)
						numToLinkLut1[links[l].Item2] = l;
					else
						numToLinkLut2[links[l].Item2] = l;
				}

				//finding first link id (the one who got no connections or more than 2 connections)
				for (int l=0; l<links.Length; l++)
				{
					if (linksWelded[l]) continue;

					(int,int) link = links[l];  //will be changed in tmp loop
					int linkNum = l;  //assigned with link
					
					//standard middle link
					if (linksPerNode[link.Item1] == 2  &&  linksPerNode[link.Item2] == 2)  
						continue; 

					//stand-alone link
					if (linksPerNode[link.Item1] != 2  &&  linksPerNode[link.Item2] != 2)  
					{
						splines.Add( new Spline(nodes[link.Item1], nodes[link.Item2]) );
						linksWelded[l] = true;
						linksWeldedCount ++;
						continue;
					}

					//standard case
					List<int> splineNodes = new List<int>();

					if (linksPerNode[link.Item1] != 2) splineNodes.Add(link.Item1); //then this is the first node
					else splineNodes.Add(link.Item2);

					for (int tmp=0; tmp<links.Length; tmp++)
					{
						int last = splineNodes[splineNodes.Count-1];
						int next = link.Item1!=last ? link.Item1 : link.Item2;

						splineNodes.Add(next);
						linksWelded[linkNum] = true;
						linksWeldedCount ++;

						if (linksPerNode[next] != 2) //end of the line
							break;

						//finding next link
						/*for (int i=0; i<links.Length; i++)
						{
							if (linksWelded[i]) continue;
							//if the same link - continue since it's already marked welded

							if (links[i].Item1==next || links[i].Item2==next)
							{ 
								link = links[i]; 
								linkNum = i;
								break; 
							}
						}*/

						if (numToLinkLut1[next] != linkNum)
							linkNum = numToLinkLut1[next];
						else
							linkNum = numToLinkLut2[next];

						link = links[linkNum];
					}

					//converting node nums to nodes
					Spline spline = new Spline(splineNodes.Count);
					for (int n=0; n<spline.nodes.Length; n++)
						spline.nodes[n] = nodes[splineNodes[n]];

					splines.Add(spline);
				}

				return splines.ToArray();
			}

		#endregion
	}
}