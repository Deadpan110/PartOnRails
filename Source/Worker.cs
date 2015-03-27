using System;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace PartOnRails
{
	[KSPAddon(KSPAddon.Startup.EveryScene, false)] 
	public class Worker : MonoBehaviour
	{
		private static ConfigNode config;
		private static Dictionary<string,string> qualifiedNames = new Dictionary<string,string>();
		private static Dictionary<uint,PartOnRails> PartsOnRails = new Dictionary<uint,PartOnRails>();

		void Awake()
		{
			if (HighLogic.LoadedScene == GameScenes.LOADING)
			{
				// Read our settings.cfg
				if (config == null) {
					config = GameDatabase.Instance.GetConfigNodes ("PartOnRails") [0];
					String debug = config.GetValue("Debug");
					if (debug != null)
					{
						PartOnRails.debug = true;
						PartOnRails.LogDebug("Debug Logging active");
					}
				}
			}
			if (HighLogic.LoadedScene == GameScenes.MAINMENU) {
				PartOnRails.LogDebug ("Caching AssemblyQualifiedNames");
				// Store the module class and namespace for location lookup
				// took me a while to figure out where this was available when vessel is not in focus
				// loosely based on solution found in:
				// http://forum.kerbalspaceprogram.com/threads/98453
				foreach (AvailablePart a in PartLoader.LoadedPartsList) {
					if (a.partPrefab.Modules != null) {
						foreach (PartModule m in a.partPrefab.Modules) {
							if (m.GetType ().Namespace != null) {
								if (qualifiedNames.ContainsKey (m.ClassName)) {
									continue;
								}
								PartOnRails.LogDebug ("Part Module {0},{1} cached", m.GetType ().Namespace, m.ClassName);
								qualifiedNames.Add (m.ClassName, m.GetType().AssemblyQualifiedName);
							}
						}
					}
				}
			}
		}


		void Start()
		{
			if (!TimeElapses())
			{
				return;
			}
			PartOnRails.LogDebug ("Worker.Start");
			UpdatePartsOnRails();
			StartCoroutine(PartOnRailsLoop ("PartOnRailsStart"));
		}


		// Create a list of all PartOnRails enabled parts currently in game
		// uses ProtoPartSnapstots regardless of if the vessel is loaded or not
		// I also feel there are too many foreach loops here... is there a faster way?
		void UpdatePartsOnRails()
		{
			PartOnRails.LogDebug("Worker.UpdatePartsOnRails - Looking for parts");
			if (FlightGlobals.fetch != null)
			{
				foreach (var v in FlightGlobals.Vessels)
				{
					foreach (ProtoPartSnapshot part in v.protoVessel.protoPartSnapshots)
					{
						foreach (ProtoPartModuleSnapshot module in part.modules)
						{
							if (module.moduleValues.GetValue("PartOnRails") != null)
							{
								if (qualifiedNames.ContainsKey(module.moduleName))
								{
									PartOnRails.LogDebug("Part {0} with id {1} contains PartOnRails in {2} ", part.partName, part.flightID, module.moduleName);

									// Add or update Dictionary like an array
									PartsOnRails [part.flightID] = new PartOnRails (module.moduleName, part);
								}
							}
						}
					}
				}
			}
		}



		// Coroutine loop - needs work!!!
		IEnumerator PartOnRailsLoop(string methodName)
		{
			PartOnRails.LogDebug("Worker.PartOnRailsLoop - {0}", methodName);

			if (PartsOnRails.Count <= 0) {
				PartOnRails.LogDebug("PartsOnRails Dictionary empty");
			}
			foreach (KeyValuePair<uint, PartOnRails> pair in PartsOnRails)
			{
				string moduleName = pair.Value.moduleName;
				ProtoPartSnapshot part = pair.Value.part;

				if (part.pVesselRef.vesselRef.loaded)
				{
					PartOnRails.LogDebug("Part {0} with id {1} is loaded - skipping", part.partName, part.flightID);
					continue;
				}

				// callback
				string qualifiedName;
				if (qualifiedNames.TryGetValue(moduleName, out qualifiedName)) {

					try
					{
						Type callback = Type.GetType (qualifiedName);

						var methodInfo = callback.GetMethod(methodName, new Type[] { typeof(ProtoPartSnapshot) });
						if (methodInfo == null) {
							PartOnRails.LogDebug ("Part {0} with id {1} has no {2} - skipping", part.partName, part.flightID, methodName);
							continue;
						}

						//PartOnRails.LogDebug ("Invoking Part {0} with id {1} at {2} in {3)", part.partName, part.flightID, methodName, moduleName);
						PartOnRails.LogDebug ("Invoking Part {0} with id {1} at {2}", part.partName, part.flightID, methodName);
						object[] args = new object[] { part };
						callback.InvokeMember(
							methodName,
							BindingFlags.InvokeMethod | BindingFlags.Public | 
							BindingFlags.Static,
							null,
							null,
							args
						);
					}
					catch (Exception e)
					{
						PartOnRails.LogDebug("An exception occured. Details:\n{0}", e.ToString());
					}
				}
				yield return null;
			}
		}


		private bool TimeElapses()
		{
			switch (HighLogic.LoadedScene) {
			case GameScenes.FLIGHT:
			case GameScenes.SPACECENTER:
			case GameScenes.TRACKSTATION:
				return true;
			default:
				return false;
			}
		}
	}
}

