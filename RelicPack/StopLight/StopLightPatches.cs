using Battle;
using HarmonyLib;
using PeglinRelicLib.Register;
using Relics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RelicPack.StopLight
{
    [HarmonyPatch]
    class PachinkoHitStopPatch
    {
        [HarmonyTargetMethods]
        static IEnumerable<MethodBase> TargetMethods()
        {
            yield return typeof(RegularPeg).GetMethod("DoPegCollision");
        }

        static void Prefix(PachinkoBall pachinko, Peg __instance)
        {
            if (pachinko == null ||
                pachinko.IsDummy ||
                !PegTypeRegister.TryGetCustomPegType("io.github.crazyjackel.stopPeg", out Peg.PegType pegType) ||
                !__instance.SupportsPegType(pegType) ||
                __instance.pegType != pegType) return;

            AccessTools.Method(typeof(PachinkoBall), "StartDestroy").Invoke(pachinko, null);
        }
    }

    [HarmonyPriority(Priority.LowerThanNormal)]
    [HarmonyPatch(typeof(Attack), "GetModifiedDamagePerPeg")]
    class DamageMultiplication
    {
        static void Postfix(ref float __result, RelicManager ____relicManager)
        {
            if (____relicManager == null || 
                !RelicRegister.TryGetCustomRelicEffect("io.github.crazyjackel.stopLight", out RelicEffect effect) || 
                !____relicManager.RelicEffectActive(effect)) return;

            __result *= 1 + Plugin.Stop_Light_Percent_Increase.Value;
        }
    }
}
