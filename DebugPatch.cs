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

namespace TasMod
{
    [HarmonyPatch(typeof(Debug), "Fps")]
    class FpsPatch
    {
        static void Postfix(Debug __instance) {
            string s = "\nGame Speed: " + Main.gameSpeed + "%\nPos" + Main.player.position.ToString() + "\nSavestate slot " + Main.stateSlot + "\nTAS Mod " + Main.VersionToString(Main.version);
            __instance.fps.text += s;
        }

    }
    [HarmonyPatch(typeof(Debug), "RunCommand")]
    class RunCommandPatch { 
        static bool Prefix(Debug __instance) {
            if (__instance.console.text.Contains("setspeed "))
            {
                string s = __instance.console.text.Substring(__instance.console.text.IndexOf(' ') + 1);
                byte i = 0;
                if (byte.TryParse(s, out i))
                {
                    Main.gameSpeed = i;
                    Time.timeScale = i / 100f;
                }
            }
            else if (__instance.console.text.Contains("savestate"))
            {
                Main.SetSaveState();
            }
            else if (__instance.console.text.Contains("loadstate"))
            {
                MelonCoroutines.Start(Main.GetSaveState());
            }
            else if (__instance.console.text == "statefile")
            {
                Process.Start(Directory.GetCurrentDirectory() + $"\\savestates\\savestate{Main.stateSlot}.ini");
            }
            else if (__instance.console.text.Contains("slot "))
            {
                string s = __instance.console.text.Substring(__instance.console.text.IndexOf(' ') + 1);
                byte i = 0;
                if (byte.TryParse(s, out i))
                {
                    Main.stateSlot = i;
                }
            }

            // easter eggs shhhhhh youre now contractually obligated not to tell anyone about this

            else if (__instance.console.text.ToLower() == "patman") Application.Quit();
            else if (__instance.console.text.ToLower() == "dani")
            {
                Application.OpenURL("https://www.youtube.com/watch?v=iik25wqIuFo");
            }
            else if (__instance.console.text.ToLower() == "dave" || __instance.console.text.ToLower() == "davetheepic04")
            {
                __instance.consoleLog.text += "\nHas no bitches";
            }
            else if (__instance.console.text.ToLower() == "mee6")
            {
                __instance.consoleLog.text += "\nFuck off Mee6";
            }
            else if (__instance.console.text.ToLower() == "stab")
            {
                __instance.consoleLog.text += "\nIT HIT RIGHT IN THE HEART!";
            }
            else return true;
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
                $"\n   slot i - Changes the savestate slot to i\n\nMade by Mang432";
        }
    }
}
