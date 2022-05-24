using HarmonyLib;
using PeglinRelicLib.Register;
using PeglinRelicLib.Utility;
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
            __state = RelicEffect.NONE;

            if (!RelicRegister.TryGetCustomRelicEffect("io.github.crazyjackel.leftHeart", out RelicEffect leftHeart)) return;

            if (!RelicRegister.TryGetCustomRelicEffect("io.github.crazyjackel.rightHeart", out RelicEffect rightHeart)) return;

            if (!RelicRegister.TryGetCustomRelic("io.github.crazyjackel.fullHeart", out Relic fullHeart)) return;

            if (effect == leftHeart && __instance.RelicEffectActive(rightHeart))
            {
                __state = rightHeart;
                relic = fullHeart;
                return;
            }

            if (effect == rightHeart && __instance.RelicEffectActive(leftHeart))
            {
                __state = leftHeart;
                relic = fullHeart;
                return;
            }
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

            if ((RelicRegister.TryGetCustomRelicEffect("io.github.crazyjackel.leftHeart", out RelicEffect leftHeart) && effect == leftHeart) ||
                (RelicRegister.TryGetCustomRelicEffect("io.github.crazyjackel.rightHeart", out RelicEffect rightHeart) && effect == rightHeart))
            {
                maxPlayerHealth.Add(Plugin.Half_Heart_Health.Value);
                playerHealth.Add(Plugin.Half_Heart_Health.Value);
                return;
            }

            if ((RelicRegister.TryGetCustomRelicEffect("io.github.crazyjackel.fullHeart", out RelicEffect fullHeart) && effect == fullHeart))
            {
                maxPlayerHealth.Add(Plugin.Full_Heart_Health.Value);
                playerHealth.Add(Plugin.Full_Heart_Health.Value);
                return;
            }
        }
    }
}
