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

                patcher.PatchAll();
                isPatched = true;
            }
        }
    }
}
