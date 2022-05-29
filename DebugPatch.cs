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
            string s = "\nGame Speed: " + Main.gameSpeed + "%\nPos" + Main.player.position.ToString() + "\nSavestate slot " + Main.stateSlot + "\nTAS Mod " + Main.VersionToString(Main.version);
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
            s += "\nVelocity"+Main.velocity.ToString();
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
                // easter eggs shhhhhh youre now contractually obligated not to tell anyone about this
                case "patman":
                    Application.Quit();
                    break;
                case "dani":
                    Application.OpenURL("https://www.youtube.com/watch?v=iik25wqIuFo");
                    break;
                case "dave":
                case "davetheepic04":
                    __instance.consoleLog.text += "\nHas no bitches";
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
                $"   loadbackup - loads the previous savestate\n   showinput 1 - shows key input or hides it \n \nMade by Mang432";
        }
    }
}
