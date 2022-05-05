using HarmonyLib;
using PeglinRelicLib.Register;
using Relics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace RelicPack.RubyHeart
{
    [HarmonyPatch(typeof(RelicManager), "AddRelic")]
    public class AddRelicPatch
    {
        public static void Prefix(ref Relic relic, RelicManager __instance, out RelicEffect __state)
        {
            RelicEffect effect = relic.effect;
            RelicEffect leftHeart = RelicRegister.GetCustomRelicEffect("io.github.crazyjackel.leftHeart");
            RelicEffect rightHeart = RelicRegister.GetCustomRelicEffect("io.github.crazyjackel.rightHeart");
            RelicEffect fullHeart = RelicRegister.GetCustomRelicEffect("io.github.crazyjackel.fullHeart");

            if(effect == leftHeart && __instance.RelicEffectActive(rightHeart))
            {
                __state = rightHeart;
                relic = RelicRegister.GetCustomRelic(fullHeart);
                return;
            }
            if (effect == rightHeart && __instance.RelicEffectActive(leftHeart))
            {
                __state = leftHeart;
                relic = RelicRegister.GetCustomRelic(fullHeart);
                return;
            }
            __state = RelicEffect.NONE;
            return;
        }
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (var instruction in instructions)
            {
                yield return instruction;
                if (instruction.LoadsField(AccessTools.Field(typeof(RelicManager), "OnRelicAdded")))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(AddRelicPatch), nameof(AddHealth)));
                }
            }
        }
        static void Postfix(RelicManager __instance, RelicEffect __state)
        {
            if (__state == RelicEffect.NONE) return;
            __instance.RemoveRelic(__state);
        }
        static void AddHealth(RelicManager manager, Relic relic)
        {
            FloatVariable playerHealth = (FloatVariable)AccessTools.Field(typeof(RelicManager), "_playerHealth").GetValue(manager);
            FloatVariable maxPlayerHealth = (FloatVariable)AccessTools.Field(typeof(RelicManager), "_maxPlayerHealth").GetValue(manager);
            RelicEffect effect = relic.effect;
            if (effect == RelicRegister.GetCustomRelicEffect("io.github.crazyjackel.leftHeart") || effect == RelicRegister.GetCustomRelicEffect("io.github.crazyjackel.rightHeart"))
            {
                maxPlayerHealth.Add(Plugin.Half_Heart_Health.Value);
                playerHealth.Add(Plugin.Half_Heart_Health.Value);
                return;
            }
            if (effect == RelicRegister.GetCustomRelicEffect("io.github.crazyjackel.fullHeart"))
            {
                maxPlayerHealth.Add(Plugin.Full_Heart_Health.Value);
                playerHealth.Add(Plugin.Full_Heart_Health.Value);
            }
        }
    }
}
