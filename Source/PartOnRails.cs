using KSP.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PartOnRails
{
	public class PartOnRails
	{
		public static bool debug = false;

		private PartOnRails() {}
		public string moduleName { get; set; }
		public ProtoPartSnapshot part { get; set; }


		public PartOnRails(string moduleName, ProtoPartSnapshot part) {
			this.moduleName = moduleName;
			this.part = part;
		}


		public static void DebugLog(string text)
		{
			if (PartOnRails.debug) Debug.Log("[PartOnRails]: " + text);
		}
		public static void DebugLog(string text, params object[] strParams)
		{
			if (PartOnRails.debug) Debug.Log("[PartOnRails]: " + string.Format(text, strParams));
		}
		public static void DebugLog(UnityEngine.Object context)
		{
			if (PartOnRails.debug) Debug.Log("[PartOnRails]: ", context);
		}
		public static void DebugWarning(string text)
		{
			if (PartOnRails.debug) Debug.LogWarning("[PartOnRails]: " + text);
		}
		public static void DebugError(string text)
		{
			if (PartOnRails.debug) Debug.LogError("[PartOnRails]: " + text);
		}
	}
}
