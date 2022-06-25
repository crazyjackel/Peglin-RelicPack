using HarmonyLib;
using PeglinRelicLib.Register;
using Relics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace RelicPack.RubyHeart
{
    [HarmonyPatch(typeof(RelicManager), "RemoveRelic")]
    public class RemoveRelicPatch
    {
        static void Prefix(RelicManager __instance, RelicEffect re, out bool __state)
        {
            __state = __instance.RelicEffectActive(re);
        }
        static void Postfix(
            RelicManager __instance, 
            RelicEffect re, 
            FloatVariable ____maxPlayerHealth,
            FloatVariable ____playerHealth,
            bool __state)
        {

            if (!__state) return;


            if (!RelicRegister.TryGetCustomRelicEffect("io.github.crazyjackel.leftHeart", out RelicEffect leftHeart)) return;

            if (!RelicRegister.TryGetCustomRelicEffect("io.github.crazyjackel.rightHeart", out RelicEffect rightHeart)) return;

            if (!RelicRegister.TryGetCustomRelicEffect("io.github.crazyjackel.fullHeart", out RelicEffect fullHeart)) return;

            if (re == leftHeart || re == rightHeart)
            {
                ____maxPlayerHealth.Subtract(Plugin.Half_Heart_Health.Value);
                ____playerHealth.Add(0);
                return;
            }
            if(re == fullHeart)
            {
                ____maxPlayerHealth.Subtract(Plugin.Full_Heart_Health.Value);
                ____playerHealth.Add(0);
            }

        }
    }
}
