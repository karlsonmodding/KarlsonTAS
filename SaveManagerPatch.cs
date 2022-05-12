using System;
using System.IO;
using Harmony;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TasMod
{
    [HarmonyPatch(typeof(SaveManager), "Load")]
    class LoadPatch
    {
        static bool Prefix(SaveManager __instance) {
            string filepath = Directory.GetCurrentDirectory() + "\\karlsondata.xml";
            if (!File.Exists(filepath))
            {
                __instance.NewSave();
            }
            else
            {
                __instance.state = __instance.Deserialize<PlayerSave>(File.ReadAllText(filepath));
            }
            return false;
        }
    }
    [HarmonyPatch(typeof(SaveManager), "Save")]
    class SavePatch
    {
        static bool Prefix(SaveManager __instance) {
            string filepath = Directory.GetCurrentDirectory() + "\\karlsondata.xml";
            File.WriteAllText(filepath, __instance.Serialize<PlayerSave>(__instance.state));
            return false;
        }
    }
}
