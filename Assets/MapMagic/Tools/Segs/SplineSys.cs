using System;
using System.Collections.Generic;
using UnityEngine;

namespace Den.Tools.Segs
{ 
	public class SplineSys: ICloneable
	{
		public Spline[] splines;

		public SplineSys () {}
		public SplineSys (Spline[] splines) => this.splines=splines;
		public SplineSys (SplineSys other)
		{
			splines = new Spline[other.splines.Length];
			for (int s=0; s<splines.Length; s++)
				splines[s] = new Spline(other.splines[s]);
		}

		public object Clone () { return new SplineSys(this); }
	}
}