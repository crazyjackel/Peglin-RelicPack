using BepInEx.Bootstrap;
using HarmonyLib;
using PeglinRelicLib.Register;
using PeglinRelicLib.Utility;
using Relics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ToolBox.Serialization;
using UnityEngine;

namespace RelicPack.Golden_Bracelet
{

    public static class GoldenRelics
    {
        public static bool IsEffectGolden(RelicEffect effect) { return relics.Contains(effect); }
        public static int numGoldenPegsHit = 0;
        public static int numGoldenRelics = 0;
        internal static bool GoldModeEnabled = false;
        private static List<RelicEffect> relics = new List<RelicEffect>();
        private static HashSet<Peg> goldenPegs = new HashSet<Peg>();

        #region Events Init
        internal static void Init()
        {
            Peg.OnPegHit += GoldenPegHit;
            BattleController.OnAttackStarted += AttackBegins;
        }
        private static void AttackBegins()
        {
            numGoldenPegsHit = 0;
        }
        private static void GoldenPegHit(Peg.PegType pegType, Peg peg)
        {
            if (!GoldenRelics.GoldModeEnabled) return;

            if (goldenPegs.Contains(peg))
            {
                numGoldenPegsHit++;
                return;
            }
        }
        #endregion

        #region Save and Load
        internal static void Load(RelicManager relicManager)
        {
            if (ModdedDataSerializer.HasKey("io.github.crazyjackel.golden"))
            {
                relics = ModdedDataSerializer.Load<List<RelicEffect>>("io.github.crazyjackel.golden");
            }
        }
        internal static void Save()
        {
            ModdedDataSerializer.Save("io.github.crazyjackel.golden", GoldenRelics.relics);
        }
        #endregion

        #region Golden Relic Commands
        internal static void CalculateNumberOfGoldenRelics(RelicManager relicManager)
        {
            int relicCount = 0;
            foreach (RelicEffect r in relics)
            {
                if (relicManager.RelicEffectActive(r))
                {
                    relicCount++;
                }
            }
            numGoldenRelics = relicCount;
        }
        internal static void MakeRelicEffectGolden(RelicEffect effect)
        {
            if (!relics.Contains(effect))
            {
                relics.Add(effect);
            }
        }
        public static bool IsPegGold(Peg peg)
        {
            return goldenPegs.Contains(peg);
        }
        public static void MakePegGold(Peg peg)
        {
            goldenPegs.Add(peg);
            ApplyMaterialToPeg(peg);
        }
        public static void ApplyMaterialToPeg(Peg peg)
        {
            Renderer[] renders = peg.gameObject.GetComponents<Renderer>();
            foreach (Renderer render in renders)
            {
                if (render != null)
                {
                    render.material = Plugin.GoldenMaterial;
                }
            }
        }
        #endregion
    }

    [HarmonyPatch(typeof(BattleController), "DoAttack")]
    public class ResetNumPegsHit
    {
        static void Postfix()
        {
            GoldenRelics.numGoldenPegsHit = 0;
        }
    }

    [HarmonyPatch(typeof(AttackManager), "GetCurrentDamage")]
    public class GetDamageGoldenBonus
    {
        static void Postfix(ref float __result)
        {
            __result += (int)(GoldenRelics.numGoldenPegsHit * GoldenRelics.numGoldenRelics * Plugin.Golden_Damage_Boost.Value);
        }
    }


    [HarmonyPatch(typeof(RelicManager), "SaveRelicData")]
    public class SaveGoldenSave
    {
        static void Postfix()
        {
            GoldenRelics.Save();
        }
    }
    [HarmonyPatch(typeof(RelicManager), "LoadRelicFromSaveFile")]
    public class LoadGoldenSave
    {
        static void Prefix(RelicManager __instance)
        {
            GoldenRelics.Load(__instance);
        }
        static void Postfix(Relic relic)
        {
            if(relic.effect.Is("io.github.crazyjackel.goldenBracelet")) GoldenRelics.GoldModeEnabled = true;
        }
    }


    [HarmonyPatch(typeof(RelicManager), "AddRelic")]
    public class AddGoldenRelic
    {
        static void Prefix(
            RelicManager __instance,
            Relic relic,
            List<Relic> ____availableCommonRelics,
            List<Relic> ____availableRareRelics,
            List<Relic> ____availableBossRelics)
        {
            if (RelicRegister.TryGetCustomRelicEffect("io.github.crazyjackel.goldenBracelet", out RelicEffect bracelet) && relic.effect == bracelet)
            {
                GoldenRelics.GoldModeEnabled = true;
                GoldenRelics.MakeRelicEffectGolden(bracelet);

                var availableRelics = ____availableCommonRelics.Concat(____availableRareRelics).Concat(____availableBossRelics).ToList();
                for (int i = 0; i < availableRelics.Count; i++)
                {
                    if (UnityEngine.Random.value < Plugin.Golden_Odds.Value) GoldenRelics.MakeRelicEffectGolden(availableRelics[i].effect);
                }
            }
        }

        static void Postfix(RelicManager __instance)
        {
            GoldenRelics.CalculateNumberOfGoldenRelics(__instance);
        }
    }
    [HarmonyPatch(typeof(RelicManager), "RemoveRelic")]
    public class RemoveGoldenRelic
    {
        static void Prefix(RelicEffect re, Dictionary<RelicEffect, Relic> ____ownedRelics)
        {
            if (____ownedRelics.ContainsKey(re) && re.Is("io.github.crazyjackel.goldenBracelet"))
            {
                GoldenRelics.GoldModeEnabled = false;
            }
        }

        static void Postfix(RelicManager __instance)
        {
            GoldenRelics.CalculateNumberOfGoldenRelics(__instance);
        }
    }
    [HarmonyPatch(typeof(RelicManager), "Reset")]
    public class ResetGoldenRelic
    {
        static void Prefix()
        {
            GoldenRelics.GoldModeEnabled = false;
            GoldenRelics.numGoldenRelics = 0;
        }
    }
}