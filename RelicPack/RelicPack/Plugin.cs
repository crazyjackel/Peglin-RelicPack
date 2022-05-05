using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using PeglinRelicLib.Model;
using PeglinRelicLib.Register;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace RelicPack
{
    [BepInPlugin(GUID, Name, Version)]
    [BepInDependency("io.github.crazyjackel.RelicLib", "1.0.3")]
    public class Plugin : BaseUnityPlugin
    {
        public const string GUID = "io.github.crazyjackel.RelicPack";
        public const string Name = "Relic Pack";
        public const string Version = "1.0.1";

        static Plugin m_plugin;
        public static Plugin myPlugin => m_plugin;
        private Harmony patcher = new Harmony(GUID);
        internal bool isPatched;

        public static ConfigEntry<float> Azide_Bomb_Percent;
        public static ConfigEntry<float> Azide_Bomb_Radius;
        public static ConfigEntry<float> Azide_Bomb_Rigged_Percent;
        public static ConfigEntry<int> Azide_Bomb_Damage;

        public static ConfigEntry<int> Half_Heart_Health;
        public static ConfigEntry<int> Full_Heart_Health;
        public static ConfigEntry<int> Full_Heart_Damage_Reduction;

        void Awake()
        {
            if (m_plugin != null) Destroy(this);
            m_plugin = this;

            #region Azide Config
            Azide_Bomb_Percent = Config.Bind("Relic Pack", "Azide_Bomb_Percent", 0.01f, "Percentage of Lighting a non-rigged Bomb while Peg is in the Air.");
            Azide_Bomb_Radius = Config.Bind("Relic Pack", "Azide_Bomb_Radius", 5.0f, "Radius for Detecting Nearby Bombs.");
            Azide_Bomb_Rigged_Percent = Config.Bind("Relic Pack", "Azide_Bomb_Rigged_Percent", 0.005f, "Percentage of Lighting a rigged Bomb while Peg is in the Air.");
            Azide_Bomb_Damage = Config.Bind("Relic Pack", "Azide_Bomb_Damage", 20, "Additional Bomb Damage.");
            #endregion

            #region Full Heart Config
            Half_Heart_Health = Config.Bind("Relic Pack", "Half_Heart_Heal", 15, "How Much Maximum Health is gained by Half a Heart.");
            Full_Heart_Health = Config.Bind("Relic Pack", "Full_Heart_Health", 30, "How Much Maximum Health is gained by a Full Heart (Note Half Hearts Health are Removed)");
            Full_Heart_Damage_Reduction = Config.Bind("Relic Pack", "Full_Heart_Damage_Reduction", 1, "How Much Damage is Lowered per Attack");
            #endregion
        }

        void OnEnable()
        {
            if (!isPatched)
            {
                #region Azide
                //Created Relic Azidaoazide
                RelicDataModel Azide = new RelicDataModel("io.github.crazyjackel.azide")
                {
                    Rarity = Relics.RelicRarity.BOSS,
                    LocalKey = "azide",
                    BundlePath = "relicpack",
                    SpriteName = "Azidoazide_Azide",
                };
                Azide.SetAssemblyPath(this);
                RelicRegister.RegisterRelic(Azide);
                LocalizationRegister.ImportTerm(new TermDataModel(Azide.NameTerm)
                {
                    English = "Azidoazide Azide"
                });
                string sign = Azide_Bomb_Damage.Value > 0 ? "+" : "";
                LocalizationRegister.ImportTerm(new TermDataModel(Azide.DescriptionTerm)
                {
                    English = $"<sprite name=\"BOMB\"> randomly light up and deal {sign}{Azide_Bomb_Damage.Value} damage."
                });;
                #endregion

                #region Full Heart

                #region Left Heart
                RelicDataModel LeftHeart = new RelicDataModel("io.github.crazyjackel.leftHeart")
                {
                    Rarity = Relics.RelicRarity.COMMON,
                    LocalKey = "leftheart",
                    BundlePath = "relicpack",
                    SpriteName = "LeftHeart"
                };
                LeftHeart.SetAssemblyPath(this);
                RelicRegister.RegisterRelic(LeftHeart);
                LocalizationRegister.ImportTerm(new TermDataModel(LeftHeart.NameTerm)
                {
                    English = "Half Ruby Heart"
                });
                LocalizationRegister.ImportTerm(new TermDataModel(LeftHeart.DescriptionTerm)
                {
                    English = $"Increases max health by <style=heal>{Half_Heart_Health.Value}</style>. Two will become One."
                });
                #endregion

                #region Right Heart
                RelicDataModel RightHeart = new RelicDataModel("io.github.crazyjackel.rightHeart")
                {
                    Rarity = Relics.RelicRarity.COMMON,
                    LocalKey = "rightheart",
                    BundlePath = "relicpack",
                    SpriteName = "RightHeart"
                };
                RightHeart.SetAssemblyPath(this);
                RelicRegister.RegisterRelic(RightHeart); 
                LocalizationRegister.ImportTerm(new TermDataModel(RightHeart.NameTerm)
                {
                    English = "Half Ruby Heart"
                });
                LocalizationRegister.ImportTerm(new TermDataModel(RightHeart.DescriptionTerm)
                {
                    English = $"Increases max health by <style=heal>{Half_Heart_Health.Value}</style>. Two will become One."
                });
                #endregion

                #region Full Heart
                RelicDataModel FullHeart = new RelicDataModel("io.github.crazyjackel.fullHeart")
                {
                    AddToPool = false,
                    LocalKey = "fullheart",
                    BundlePath = "relicpack",
                    SpriteName = "FullHeart"
                };
                FullHeart.SetAssemblyPath(this);
                RelicRegister.RegisterRelic(FullHeart);
                LocalizationRegister.ImportTerm(new TermDataModel(FullHeart.NameTerm)
                {
                    English = "Ruby Heart"
                });
                string DamageReduction = (Full_Heart_Damage_Reduction.Value > 0) ? 
                    $"Damage Taken Decreased by {Full_Heart_Damage_Reduction.Value}" : 
                        (Full_Heart_Damage_Reduction.Value < 0) ?
                        $"Damage Taken Increased by {Math.Abs(Full_Heart_Damage_Reduction.Value)}" : "";
                LocalizationRegister.ImportTerm(new TermDataModel(FullHeart.DescriptionTerm)
                {
                    English = $"Increases max health by <style=heal>{Full_Heart_Health.Value}</style>. {DamageReduction}"
                });
                #endregion

                #endregion

                patcher.PatchAll();
                isPatched = true;
            }
        }
    }
}
