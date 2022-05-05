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

            RelicEffect leftHeart = RelicRegister.GetCustomRelicEffect("io.github.crazyjackel.leftHeart");
            RelicEffect rightHeart = RelicRegister.GetCustomRelicEffect("io.github.crazyjackel.rightHeart");
            RelicEffect fullHeart = RelicRegister.GetCustomRelicEffect("io.github.crazyjackel.fullHeart");
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
