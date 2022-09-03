using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Tools
{
	public static class MathTools
	{
		public static float SqwDistance(Vector3 a,Vector3 b)
		{
			return (a.x - b.x) * (a.x - b.x) 
				+ (a.y - b.y) * (a.y - b.y) 
				+ (a.z - b.z) * (a.z - b.z);
		}

		public static bool IsDistanceSmaller(Vector3 a, Vector3 b,float distance)
        {
			return SqwDistance(a, b) < distance * distance;
        }

		public static bool IsDistanceGreater(Vector3 a, Vector3 b, float distance)
		{
			return SqwDistance(a, b) > distance * distance;
		}

		public static Vector3 GetPointOnDistance(Vector3 a, Vector3 b, float distance)
        {
			return a + (b - a).normalized * distance;
        }
	}
}
