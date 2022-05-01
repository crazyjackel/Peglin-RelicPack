using HarmonyLib;
using PeglinRelicLib.Register;
using Relics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace RelicPack.Azide
{
    [HarmonyPatch(typeof(BattleController), "ApplyBombRelics")]
    public class ApplyBombRelicPatch
    {
        public static void Prefix(ref float baseDamage, RelicManager ____relicManager)
        {
            Debug.Log(____relicManager.RelicEffectActive(RelicRegister.Instance["io.github.crazyjackel.azide"]));
            if (____relicManager.RelicEffectActive(RelicRegister.Instance["io.github.crazyjackel.azide"]))
            {
                baseDamage += Plugin.Azide_Bomb_Damage.Value;
            }
        }
    }
}
