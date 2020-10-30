using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CLE.OrbHunt
{
	public static class DebugWrapper 
	{
		[System.Diagnostics.Conditional("UNITY_EDITOR")]
		public static void Debug(string logMsg) 
		{
			UnityEngine.Debug.Log(logMsg);
		}
	}
}