using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Harmony;
using UnityEngine;
using MelonLoader;
namespace TasMod
{
	[HarmonyPatch(typeof(PlayerMovement), "Pause")]
	class PausePatchPatch
	{
		static void Postfix(PlayerMovement __instance) {
			if (!__instance.paused) Time.timeScale = Main.gameSpeed / 100f;
		}
	}

	[HarmonyPatch(typeof(PlayerMovement), "UpdateTimescale")]
	class UpdateTimescaleSkip
	{
		static bool Prefix() {
			return false;
		}
	}

	[HarmonyPatch(typeof(PlayerMovement), "Update")]
	class UpdatePrefix
	{
		static void Prefix(PlayerMovement __instance)
		{
			Main.velocity = __instance.GetVelocity();
			Main.cameraRotation = __instance.playerCam.rotation.eulerAngles;
		}

	}

}
