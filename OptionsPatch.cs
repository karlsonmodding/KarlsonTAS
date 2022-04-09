using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MelonLoader;
using Harmony;
namespace TasMod
{
    [HarmonyPatch(typeof(Options), nameof(Options.ChangeSlowmo), new Type[] { typeof(bool) })]
    class OptionsPatch
    {
        //[HarmonyPatch(typeof(Options), "SetSlowmo", new Type[] { typeof(bool) })]
        static void Prefix(bool b) {
            if (b) b = false;
            //MelonLogger.Msg("IT WORKS!!11!!1!!!1");
        }
    }
}
