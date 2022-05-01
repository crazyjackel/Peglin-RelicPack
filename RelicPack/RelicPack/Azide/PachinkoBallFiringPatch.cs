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
    [HarmonyPatch(typeof(PachinkoBall), "FixedUpdate")]
    public class PachinkoBallFiringPatch
    {
        public static void Postfix(PachinkoBall __instance, RelicManager ____relicManager, LayerMask ____circleCastLayerMask)
        {
            if (____relicManager == null || !(__instance.IsFiring() || __instance.IsDummy) || !____relicManager.RelicEffectActive(RelicRegister.GetCustomRelicEffect("io.github.crazyjackel.azide"))) return;


            Collider2D[] nearbyPegs = new Collider2D[30];
            Physics2D.defaultPhysicsScene.OverlapCircle(__instance.transform.position, Plugin.Azide_Bomb_Radius.Value, nearbyPegs, ____circleCastLayerMask);
            foreach (Collider2D collider2D in nearbyPegs)
            {
                if (collider2D != null)
                {
                    Bomb component = collider2D.GetComponent<Bomb>();
                    if (component == null) continue;
                    if (UnityEngine.Random.value < (component.isRigged ? Plugin.Azide_Bomb_Rigged_Percent.Value : Plugin.Azide_Bomb_Percent.Value))
                    {
                        component.PegHit(false);
                    }
                }
            }
        }
    }
}
