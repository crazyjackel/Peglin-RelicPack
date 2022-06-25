using Battle;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using HarmonyLib;
using PeglinRelicLib.Model;
using PeglinRelicLib.Register;
using PeglinRelicLib.Utility;
using RelicPack.Golden_Bracelet;
using Relics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace RelicPack
{
    [BepInPlugin(GUID, Name, Version)]
    [BepInDependency("io.github.crazyjackel.RelicLib", "2.0.1")]
    public class Plugin : BaseUnityPlugin
    {
        public const string GUID = "io.github.crazyjackel.RelicPack";
        public const string Name = "Relic Pack";
        public const string Version = "2.0.1";

        internal static string m_path;
        const string m_bundlepath = "relicpack";
        internal static string BundlePath => Path.Combine(m_path, m_bundlepath);

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

        public static ConfigEntry<float> Golden_Odds;
        public static ConfigEntry<float> Golden_Damage_Boost;

        public static ConfigEntry<int> Peglin_Head_Heal;

        public static ConfigEntry<float> Stop_Light_Percent_Increase;

        #region Preloaded Assets for Internal Use
        internal static Material GoldenMaterial;
        internal static Sprite StopPeg;
        #endregion
        static Plugin()
        {
            //Calculate out a BasePath
            var assembly = typeof(Plugin).Assembly;
            var uri = new UriBuilder(assembly.CodeBase);
            m_path = Path.GetDirectoryName(Uri.UnescapeDataString(uri.Path));
        }


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

            #region Golden Bracelet Config
            Golden_Odds = Config.Bind("Relic Pack", "Golden_Odds", 0.5f, "Percent of Relic Being Golden after Picking up Golden Bracelet");
            Golden_Damage_Boost = Config.Bind("Relic Pack", "Golden_Damage_Boost", 1f, "Damage per Golden Relic for Hitting a Golden Peg, Rounds Down to Int");
            #endregion

            #region Infected Peglin Head Config
            Peglin_Head_Heal = Config.Bind("Relic Pack", "Peglin_Head_Heal", 30, "Amount to Heal after Almost Dying");
            #endregion

            #region Stop Light Config
            Stop_Light_Percent_Increase = Config.Bind("Relic Pack", "Stop_Light_Percent_Increase", 0.3f, "Damage Percent Increase");
            #endregion

            AssetBundle bundle = AssetBundle.LoadFromFile(BundlePath);

            GoldenMaterial = bundle.LoadAsset<Material>("GoldenMat");
            StopPeg = bundle.LoadAsset<Sprite>("StopPeg");

            bundle.Unload(false);
        }

        void OnEnable()
        {
            if (!isPatched)
            {
                #region Items

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
                RelicRegister.RegisterRelic(Azide, out _);

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
                RelicRegister.RegisterRelic(LeftHeart, out _);
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
                RelicRegister.RegisterRelic(RightHeart, out _);
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
                RelicRegister.RegisterRelic(FullHeart, out _);

                #endregion

                #endregion

                #region Golden Bracelet
                RelicDataModel GoldenBracelet = new RelicDataModel("io.github.crazyjackel.goldenBracelet")
                {
                    Rarity = RelicRarity.BOSS,
                    LocalKey = "goldenBracelet",
                    BundlePath = "relicpack",
                    SpriteName = "GoldenBracelet"
                };
                GoldenBracelet.SetAssemblyPath(this);
                RelicRegister.RegisterRelic(GoldenBracelet, out _);
                #endregion

                #region Infected Pegling Head
                RelicDataModel PeglinHead = new RelicDataModel("io.github.crazyjackel.peglinHead")
                {
                    Rarity = RelicRarity.RARE,
                    LocalKey = "peglinHead",
                    BundlePath = "relicpack",
                    SpriteName = "PeglinHead"
                };
                PeglinHead.SetAssemblyPath(this);
                RelicRegister.RegisterRelic(PeglinHead, out _);
                #endregion

                #region Stop Light
                RelicDataModel StopLight = new RelicDataModel("io.github.crazyjackel.stopLight")
                {
                    Rarity = RelicRarity.RARE,
                    LocalKey = "stopLight",
                    BundlePath = "relicpack",
                    SpriteName = "StopLight"
                };
                StopLight.SetAssemblyPath(this);
                RelicRegister.RegisterRelic(StopLight, out RelicEffect stopLight);
                #endregion

                #endregion

                #region PegTypes

                #region StopPegType
                PegTypeDataModel StopPegType = new PegTypeDataModel("io.github.crazyjackel.stopPeg")
                    .AddSupport<RegularPeg>((peg) =>
                    {
                        SpriteRenderer renderer = peg.GetComponent<SpriteRenderer>();
                        if (renderer == null) return;
                        renderer.sprite = StopPeg;
                    })
                    .AddBoardPegCount((pegManager) => 
                    {
                        RelicManager manager = (RelicManager)AccessTools.Field(typeof(PegManager), "_relicManager").GetValue(pegManager);
                        if(manager != null && manager.RelicEffectActive(stopLight))
                        {
                            return 1;
                        }
                        return 0;
                    });
                PegTypeRegister.RegisterPegType(StopPegType, out _);
                #endregion

                #endregion

                #region Localization
                string DamageReduction = (Full_Heart_Damage_Reduction.Value > 0) ?
                    $"Damage Taken Decreased by {Full_Heart_Damage_Reduction.Value}" :
                        (Full_Heart_Damage_Reduction.Value < 0) ?
                        $"Damage Taken Increased by {Math.Abs(Full_Heart_Damage_Reduction.Value)}" : "";
                string sign = Azide_Bomb_Damage.Value > 0 ? "+" : "";

                LocalizationHelper.ImportTerm(
                    new TermDataModel(Azide.NameTerm)
                    {
                        English = "Azidoazide Azide",
                        Chinese = "危险爆炸物"
                    },
                    new TermDataModel(Azide.DescriptionTerm)
                    {
                        English = $"<sprite name=\"BOMB\"> randomly light up and deal {sign}{Azide_Bomb_Damage.Value} damage.",
                        Chinese = $"随机引爆<sprite name=\"BOMB\">，并对敌人造成{sign}{Azide_Bomb_Damage.Value}点伤害。"
                    },
                    new TermDataModel(FullHeart.NameTerm)
                    {
                        English = "Ruby Heart",
                        Chinese = "露比的整个心脏"
                    },
                    new TermDataModel(FullHeart.DescriptionTerm)
                    {
                        English = $"Increases max health by <style=heal>{Full_Heart_Health.Value}</style>. {DamageReduction}",
                        Chinese = $"增加<style=heal>{Full_Heart_Health.Value}</style>点血量上限。{DamageReduction}"
                    },
                    new TermDataModel(GoldenBracelet.NameTerm)
                    {
                        English = "Golden Bracelet",
                        Chinese = "金手镯"
                    },
                    new TermDataModel(GoldenBracelet.DescriptionTerm)
                    {
                        English = "Make Everything Golden. Golden Stuff is Stronger.",
                        Chinese = "使一切变金变强。"
                    },
                    new TermDataModel(LeftHeart.NameTerm)
                    {
                        English = "Half Ruby Heart",
                        Chinese = "露比的左半个心脏"
                    },
                    new TermDataModel(LeftHeart.DescriptionTerm)
                    {
                        English = $"Increases max health by <style=heal>{Half_Heart_Health.Value}</style>. Two will become One.",
                        Chinese = $"增加<style=heal>{Half_Heart_Health.Value}</style>点血量上限。和右半个心脏合成整个心脏。"
                    },
                    new TermDataModel(PeglinHead.NameTerm)
                    {
                        English = "Infected Peglin Head",
                        Chinese = "受感染的佩哥林的头"
                    },
                    new TermDataModel(PeglinHead.DescriptionTerm)
                    {
                        English = $"Extra Life. <style=heal>Heal {Peglin_Head_Heal.Value}</style> on Death.",
                        Chinese = $"额外一条命。当你将要死亡时，<style=heal>恢复{Peglin_Head_Heal.Value}</style>点血量。"
                    },
                    new TermDataModel(StopLight.NameTerm)
                    {
                        English = "Stop Light",
                        Chinese = "禁行指示灯"
                    },
                    new TermDataModel(StopLight.DescriptionTerm)
                    {
                        English = $"Deal {(int)(Stop_Light_Percent_Increase.Value * 100)}% increased Damage. A Stop Sign appears.",
                        Chinese = $"额外造成{(int)(Stop_Light_Percent_Increase.Value * 100)}%伤害，但会出现有停止标识的钉子。"
                    },
                    new TermDataModel(RightHeart.NameTerm)
                    {
                        English = "Half Ruby Heart",
                        Chinese = "露比的右半个心脏"
                    },
                    new TermDataModel(RightHeart.DescriptionTerm)
                    {
                        English = $"Increases max health by <style=heal>{Half_Heart_Health.Value}</style>. Two will become One.",
                        Chinese = $"增加<style=heal>{Half_Heart_Health.Value}</style>点血量上限。和左半个心脏合成整个心脏。"
                    }
                );

                #endregion

                patcher.PatchAll();
                isPatched = true;

                GoldenRelics.Init();
            }
        }
    }
}
