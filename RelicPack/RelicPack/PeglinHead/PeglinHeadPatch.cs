using Battle;
using HarmonyLib;
using PeglinRelicLib.Register;
using Relics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelicPack.PeglinHead
{
    [HarmonyPatch(typeof(PlayerHealthController), "Damage")]
    class PeglinHeadPatch
    {
        static bool ImmuneForTurn = false;
        static PeglinHeadPatch()
        {
            BattleController.OnTurnComplete += OnTurnComplete;
        }

        private static void OnTurnComplete()
        {
            ImmuneForTurn = false;
        }

        static void Prefix(PlayerHealthController __instance, ref float damage, RelicManager ____relicManager, FloatVariable ____playerHealth)
        {
            if (ImmuneForTurn)
            {
                damage = 0;
                return;
            }
            RelicEffect effect = RelicRegister.GetCustomRelicEffect("io.github.crazyjackel.peglinHead");
            if (____playerHealth.Value < damage && ____relicManager.RelicEffectActive(effect))
            {
                __instance.Heal(Plugin.Peglin_Head_Heal.Value);
                ____relicManager.RemoveRelic(effect);
                ImmuneForTurn = true;
            }
        }
    }
}
