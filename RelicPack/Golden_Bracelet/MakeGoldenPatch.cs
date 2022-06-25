using Battle.Enemies;
using BepInEx;
using BepInEx.Bootstrap;
using HarmonyLib;
using PeglinRelicLib.Register;
using Relics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;



namespace RelicPack.Golden_Bracelet
{
    //Commands to Make things Golden 
    //Disconnected from Functionality in order to prepare for in-game changes.

    [HarmonyPatch(typeof(PachinkoBall), "Init")]
    class MakePachinkoBallsGoldenPatch
    {
        public static void Postfix(SpriteRenderer ____renderer, RelicManager relicManager)
        {
            if (!GoldenRelics.GoldModeEnabled) return;

            ____renderer.material = Plugin.GoldenMaterial;
        }
    }

    [HarmonyPatch(typeof(PeglinBattleAnimationController), "OnEnable")]
    class MakePeglinGoldenPatch
    {
        public static void Postfix(PeglinBattleAnimationController __instance)
        {
            if (!GoldenRelics.GoldModeEnabled) return;

            SpriteRenderer render = __instance.gameObject.GetComponent<SpriteRenderer>();
            if (render != null)
            {
                render.material = Plugin.GoldenMaterial;
            }
        }
    }
    [HarmonyPatch(typeof(RelicIcon), "SetRelic")]
    class MakeRelicIconGoldenPatch
    {
        public static void Prefix(Relic r, Image ____image)
        {
            if (GoldenRelics.IsEffectGolden(r.effect))
            {
                ____image.material = Plugin.GoldenMaterial;
            }
        }
    }
    [HarmonyPatch(typeof(RelicIcon), "RestoreShader")]
    class MakeRelicRestoreShaderToGolden
    {
        public static void Postfix(Relic ___relic, Image ____image)
        {
            if (GoldenRelics.IsEffectGolden(___relic.effect))
            {
                ____image.material = Plugin.GoldenMaterial;
            }
        }
    }
    [HarmonyPatch(typeof(ChangeAnimSpeedByVertSpeed), "Update")]
    class MakeMapPeglinGoldenPatch
    {
        internal static bool switchGold = true;
        private static Material previousMat;

        public static void Postfix(ChangeAnimSpeedByVertSpeed __instance)
        {
            if (switchGold && GoldenRelics.GoldModeEnabled)
            {
                switchGold = false;
                SpriteRenderer render = __instance.gameObject.GetComponent<SpriteRenderer>();
                if (render != null)
                {
                    previousMat = render.material;
                    render.material = Plugin.GoldenMaterial;
                }
            }

            if (!switchGold && !GoldenRelics.GoldModeEnabled)
            {
                switchGold = true;
                SpriteRenderer render = __instance.gameObject.GetComponent<SpriteRenderer>();
                if (render != null && previousMat != null)
                {
                    render.material = previousMat;
                }
            }
        }
    }

    [HarmonyPatch(typeof(Enemy), "Damage")]
    class MakeEnemyGoldenOnDamagePatch
    {
        public static void Prefix(Enemy __instance)
        {
            if (!GoldenRelics.GoldModeEnabled) return;

            SpriteRenderer render = __instance.gameObject.GetComponent<SpriteRenderer>();
            if (render != null)
            {
                render.color = Color.yellow;
                render.material = Plugin.GoldenMaterial;
            }
        }
    }

    static class PegTypes
    {
        public static IEnumerable<Type> Pegs;
        static PegTypes()
        {
            Pegs = GetPegTypes();
        }

        public static IEnumerable<Type> GetPegTypes()
        {
            //Construct a List of Assemblies to Check for Pegs from.
            List<Assembly> checkAssemblies = new List<Assembly>() { typeof(Peg).Assembly };

            //Go through each Plugin to Check
            foreach (var pluginInfo in Chainloader.PluginInfos.Values)
            {
                Assembly assembly;
                try
                {
                    assembly = Assembly.LoadFrom(pluginInfo.Location);
                }
                catch (Exception)
                {
                    continue;
                }
                if (assembly == null) continue;
                checkAssemblies.Add(assembly);
            }

            //Go Through each Implementation and Decide to Patch that Function
            foreach (Assembly assemble in checkAssemblies)
            {
                if (assemble == null) continue;
                foreach (Type type in assemble.GetTypes()
                .Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(Peg))))
                {
                    yield return type;

                }
            }
        }
    }
    [HarmonyPatch]
    class MakePegsGoldenOnHitPatch
    {
        [HarmonyTargetMethods]
        static IEnumerable<MethodBase> TargetMethods()
        {
            foreach (Type type in PegTypes.Pegs)
            {
                var method = type.GetMethod("PegHit");
                if (method != null)
                {
                    yield return method;
                }
            }
        }
        public static void Postfix(Peg __instance)
        {
            if (!GoldenRelics.GoldModeEnabled) return;

            GoldenRelics.MakePegGold(__instance);
        }
    }

    [HarmonyPatch]
    class MaintainGoldenMaterialThroughResetPatch
    {
        [HarmonyTargetMethods]
        static IEnumerable<MethodBase> TargetMethods()
        {
            foreach (Type type in PegTypes.Pegs)
            {
                var method = type.GetMethod("Reset");
                if (method != null)
                {
                    yield return method;
                }
            }
        }
        static void Postfix(object __instance)
        {
            if(__instance is Peg peg && GoldenRelics.IsPegGold(peg))
            {
                GoldenRelics.ApplyMaterialToPeg(peg);
            }
        }
    }
}
