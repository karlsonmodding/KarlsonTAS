using System;
using System.IO;
using System.Diagnostics;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MelonLoader;
using UnityEngine;
using Harmony;
using UnityEngine.SceneManagement;

namespace TasMod
{
	[HarmonyPatch(typeof(Debug), "Fps")]
	class FpsPatch
	{
		static String[] btns = {"Fire1", "Crouch", "Jump", "Pickup", "Drop"};
		static void Postfix(Debug __instance) {
			float kmph = PlayerMovement.Instance.rb.velocity.magnitude * 3.6f;
			float mph = kmph / 1.609f;
			kmph = Mathf.Round(kmph);
			mph = Mathf.Round(mph);
			string s = "|" + kmph+ "km/h|" + mph + "mph \nGame Speed: " + Main.gameSpeed + "%\nPos" + Main.player.position.ToString() + "\nVel" + Main.velocity.ToString() + 
				"\nCam" + Main.cameraRotation.ToString() + "\nSavestate slot " + Main.stateSlot + "\nTAS Mod " + Main.VersionToString(Main.version);
			if (Main.displayInput){ 
				s += "\nKeyInput: ";
				if((Input.GetAxis("Horizontal") > 0)){
					s += "Right ";
				}else if ((Input.GetAxis("Horizontal") < 0)){
					s += "Left ";
				}
				if ((Input.GetAxis("Vertical") > 0)){
					s += "Up ";
				}
				else if ((Input.GetAxis("Vertical") < 0)){
					s += "Down ";
				}
				foreach(String btn in btns){
					if (Input.GetButton(btn)){
						s += btn+" ";
					}
				}

			}
			__instance.fps.text += s;
		}

	}
	[HarmonyPatch(typeof(Debug), "RunCommand")]
	class RunCommandPatch { 
		static bool Prefix(Debug __instance) {
			string[] command = __instance.console.text.Split(' ');
			switch (command[0].ToLower()){
				case "setspeed":
					{
						if (command.Length < 2) break;
						byte i = 0;
						if (byte.TryParse(command[1], out i))
						{
							Main.gameSpeed = i;
							Time.timeScale = i / 100f;
						}
					}
					break;
				case "savestate":
					Main.SetSaveState();
					break;
				case "loadstate":
					MelonCoroutines.Start(Main.GetSaveState());
					break;
				case "statefile":
					Process.Start(Directory.GetCurrentDirectory() + $"\\savestates\\savestate{Main.stateSlot}.ini");
					break;
				case "slot":
					{
						if (command.Length < 2) break;
						byte i = 0;
						if (byte.TryParse(command[1], out i))
						{
							Main.stateSlot = i;
						}
					}
					break;
				case "loadbackup":
					File.WriteAllText(Directory.GetCurrentDirectory() + $"\\savestates\\savestate{Main.stateSlot}.ini", Main.backup);
					MelonCoroutines.Start(Main.GetSaveState());
					break;
				case "menu":
					SceneManager.LoadScene(1);
					UIManger ui = Object.FindObjectOfType<UIManger>();
					Object.Destroy(ui.gameUI);
					Object.Destroy(ui.deadUI);
					Object.Destroy(ui.winUI);
					Object.Destroy(ui.gameObject);
					Object.Destroy(__instance.gameObject.transform);
					break;
				case "showinput":
					if (command.Length < 2) break;
					Main.displayInput = (command[1] == "true"|| command[1] == "1");
					break;
				case "copyto":
					if (command.Length < 2) break;
					File.Copy(Directory.GetCurrentDirectory() + $"\\savestates\\savestate{Main.stateSlot}.ini",
						Directory.GetCurrentDirectory() + $"\\savestates\\savestate{command[1]}.ini", true);
					break;
				case "bind":
					if (command.Length < 3) break;
					else if (char.TryParse(command[1], out char c)) { 
						{
							KeyBind kb;
							kb.key = c;
							kb.command = command[2];
							for (int i = 3; i < command.Length; i++) kb.command += " " + command[i];
							KeyBindManager.binds.Add(kb);
						}
					}
					break;
				case "unbind":
					if (command.Length < 2) break;
					char ch;
					if (!char.TryParse(command[1], out ch)) break;
					foreach (KeyBind kb in KeyBindManager.binds)
                    {
						if (kb.key == ch) KeyBindManager.binds.Remove(kb);
                    }
					break;
				case "vsync":
					if (command.Length > 1 && byte.TryParse(command[1], out byte num) && num < 5) QualitySettings.vSyncCount = num;
					break;
				// easter eggs shhhhhh youre now contractually obligated not to tell anyone about this
				case "patman":
					Application.Quit();
					break;
				case "dani":
					Application.OpenURL("https://www.youtube.com/watch?v=iik25wqIuFo");
					break;
				case "dave":
				case "davetheepic04":
					if (UnityEngine.Random.value > 0.5f) __instance.consoleLog.text += "\nHas no bitches";
					else __instance.consoleLog.text += "\nMe and my team developed a game and we need comments (for new updates) and I thought you might help and wanted to write to you so need feedback could you help me ? ";
					break;
				case "mee6":
					__instance.consoleLog.text += "\nFuck off Mee6";
					break;
				case "stab":
					__instance.consoleLog.text += "\nIT HIT RIGHT IN THE HEART!";
					break;
				case "jannik":
					__instance.consoleLog.text += "\n    o\n_`O'_\n that is a frog";
					break;
				case "rockool":
					__instance.consoleLog.text += "\nHe did a man";
					break;
				case "jjen":
					__instance.consoleLog.text += "\nbooster? more like boober\nsoo when am i getting mod";
					break;
				default:
					return true;
			}
			__instance.console.text = "";
			__instance.console.Select();
			__instance.console.ActivateInputField();
			return false;
		}
	}
	[HarmonyPatch(typeof(Debug), "CloseConsole")]
	class CloseConsolePatch
	{
		static void Postfix() {
			Time.timeScale = Main.gameSpeed / 100f;
		}
	}
	[HarmonyPatch(typeof(Debug), "Help")]
	class HelpPatch 
	{
		static void Postfix(Debug __instance) {
			__instance.consoleLog.text += $"\nTasMod {Main.VersionToString(Main.version)} alpha\n   savestate - Saves the game state\n   loadstate - Loads the game state" +
				$"\n   slot i - Changes the savestate slot to in\n   setspeed i - Changes the game speed to i%\n   statefile - Opens the savestate file\n" +
				$"   loadbackup - loads the previous savestate\n   showinput 1 - shows key input or hides it\n   copyto i - Copies the current state to slot i\n   " +
				$"vsync i - Enables/disables vsync\n   bind <i>hotkey command</i> - Sets a command to a hotkey\n   unbind <i>hotkey</i> - unsets all binds to that hotkey" +
				$" \n \nMade by Mang432\nSpecial thanks to: jannik";
		}
	}
	[HarmonyPatch(typeof(Debug), "Update")]
	class KeyBindManager
	{
		public static List<KeyBind> binds = new List<KeyBind>();
		
		static void Postfix(Debug __instance) {
			foreach (KeyBind kb in binds)
            {
				if (Input.GetKeyDown(kb.key.ToString()) && !__instance.console.isActiveAndEnabled) 
                {
					__instance.console.text = kb.command;
					__instance.RunCommand();
                }
            }
        }
	}
	[HarmonyPatch(typeof(Debug), "Start")]
	class AutoExecutor
    {
		static void Postfix(Debug __instance) {
			string dir = Directory.GetCurrentDirectory() + "\\autoexec.txt";
			if (!File.Exists(dir)) File.Create(dir);
			else
            {
				string[] commands = File.ReadAllLines(dir);
				foreach (string s in commands)
                {
					__instance.console.text = s;
					__instance.RunCommand();
                }
            }
        }
    }
}
