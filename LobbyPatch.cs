using System;
using System.Diagnostics;
using System.Runtime;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harmony;
using UnityEngine;
using MelonLoader;

namespace TasMod
{
	[HarmonyPatch(typeof(Lobby), "Exit")]
	class ExitPatch
	{
		static void Prefix() {
			Process.GetCurrentProcess().Kill();
		}
	}
}
