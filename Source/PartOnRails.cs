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

		public static void LogDebug(String Message, params object[] strParams)
		{
			if (!PartOnRails.debug) {
				return;
			}
			Message = String.Format(Message, strParams);
			Debug.Log("[PartOnRails]: " + Message);
		}
	}
}
