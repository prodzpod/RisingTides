using BepInEx;
using MysticsRisky2Utils;
using R2API;
using R2API.Utils;
using RoR2;
using RoR2.ContentManagement;
using System.Collections;
using System.Collections.Generic;
using System.Security;
using System.Security.Permissions;
using UnityEngine;

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
[assembly: HG.Reflection.SearchableAttribute.OptIn]
namespace RisingTides
{
    [BepInDependency(R2API.R2API.PluginGUID)]
    [BepInDependency(EliteAPI.PluginGUID)]
    [BepInDependency(DamageAPI.PluginGUID)]
    [BepInDependency(DotAPI.PluginGUID)]
    [BepInDependency(LanguageAPI.PluginGUID)]
    [BepInDependency(R2API.Networking.NetworkingAPI.PluginGUID)]
    [BepInDependency(PrefabAPI.PluginGUID)]
    [BepInDependency(RecalculateStatsAPI.PluginGUID)]
    [BepInDependency(DeployableAPI.PluginGUID)]
    [BepInDependency(MysticsRisky2UtilsPlugin.PluginGUID)]
    [BepInDependency(MysticsItems.MysticsItemsPlugin.PluginGUID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    public class RisingTidesPlugin : BaseUnityPlugin
    {
        public const string PluginGUID = "com.themysticsword.risingtides";
        public const string PluginName = "Rising Tides";
        public const string PluginVersion = "1.2.3";

        public static System.Reflection.Assembly executingAssembly;
        internal static System.Type declaringType;
        internal static PluginInfo pluginInfo;
        internal static BepInEx.Logging.ManualLogSource logger;
        internal static BepInEx.Configuration.ConfigFile config;
        public static ConfigOptions.ConfigurableValue<bool> ignoreBalanceChanges;
        public static ConfigOptions.ConfigurableValue<bool> eliteDamageMultipliersEnabled;

        private static AssetBundle _assetBundle;
        public static AssetBundle AssetBundle
        {
            get
            {
                if (_assetBundle == null)
                    _assetBundle = AssetBundle.LoadFromFile(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(pluginInfo.Location), "risingtidesassetbundle"));
                return _assetBundle;
            }
        }

        internal static bool mysticsItemsCompatibility = false;

        public void Awake()
        {
            pluginInfo = Info;
            logger = Logger;
            config = Config;
            executingAssembly = System.Reflection.Assembly.GetExecutingAssembly();
            declaringType = System.Reflection.MethodBase.GetCurrentMethod().DeclaringType;

            // SoundAPI.SoundBanks.Add(System.IO.File.ReadAllBytes(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(pluginInfo.Location), "RisingTidesSoundbank.bnk")));

            if (MysticsRisky2Utils.SoftDependencies.SoftDependencyManager.RiskOfOptionsDependency.enabled)
            {
                Sprite iconSprite = null;
                var iconPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Info.Location), "icon.png");
                if (System.IO.File.Exists(iconPath))
                {
                    var iconTexture = new Texture2D(2, 2);
                    iconTexture.LoadImage(System.IO.File.ReadAllBytes(iconPath));
                    iconSprite = Sprite.Create(iconTexture, new Rect(0, 0, iconTexture.width, iconTexture.height), new Vector2(0, 0), 100);
                }
                MysticsRisky2Utils.SoftDependencies.SoftDependencyManager.RiskOfOptionsDependency.RegisterModInfo(PluginGUID, PluginName, "New elites and a new enemy!", iconSprite);
            }

            ignoreBalanceChanges = ConfigOptions.ConfigurableValue.CreateBool(PluginGUID, PluginName, config, "General", "Ignore Balance Changes", true, "If true, most of the numerical balance-related config options will be ignored and will use recommended default values. These are usually the values that might get tweaked often during mod updates.");
            
            MysticsRisky2Utils.ContentManagement.ContentLoadHelper.PluginAwakeLoad<MysticsRisky2Utils.BaseAssetTypes.BaseItem>(executingAssembly);
            MysticsRisky2Utils.ContentManagement.ContentLoadHelper.PluginAwakeLoad<MysticsRisky2Utils.BaseAssetTypes.BaseEquipment>(executingAssembly);
            MysticsRisky2Utils.ContentManagement.ContentLoadHelper.PluginAwakeLoad<MysticsRisky2Utils.BaseAssetTypes.BaseBuff>(executingAssembly);
            MysticsRisky2Utils.ContentManagement.ContentLoadHelper.PluginAwakeLoad<MysticsRisky2Utils.BaseAssetTypes.BaseElite>(executingAssembly);
            MysticsRisky2Utils.ContentManagement.ContentLoadHelper.PluginAwakeLoad<MysticsRisky2Utils.BaseAssetTypes.BaseInteractable>(executingAssembly);
            MysticsRisky2Utils.ContentManagement.ContentLoadHelper.PluginAwakeLoad<MysticsRisky2Utils.BaseAssetTypes.BaseCharacterBody>(executingAssembly);
            MysticsRisky2Utils.ContentManagement.ContentLoadHelper.PluginAwakeLoad<MysticsRisky2Utils.BaseAssetTypes.BaseCharacterMaster>(executingAssembly);
            
            ContentManager.collectContentPackProviders += (addContentPackProvider) =>
            {
                addContentPackProvider(new RisingTidesContent());
            };

            mysticsItemsCompatibility = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(MysticsItems.MysticsItemsPlugin.PluginGUID);
        }
    }

    public class RisingTidesContent : IContentPackProvider
    {
        public string identifier
        {
            get
            {
                return RisingTidesPlugin.PluginName;
            }
        }

        public IEnumerator LoadStaticContentAsync(LoadStaticContentAsyncArgs args)
        {
            contentPack.identifier = identifier;
            MysticsRisky2Utils.ContentManagement.ContentLoadHelper contentLoadHelper = new MysticsRisky2Utils.ContentManagement.ContentLoadHelper();

            var executingAssembly = RisingTidesPlugin.executingAssembly;

            // Add content loading dispatchers to the content load helper
            System.Action[] loadDispatchers = new System.Action[]
            {
                () => contentLoadHelper.DispatchLoad<ItemDef>(executingAssembly, typeof(MysticsRisky2Utils.BaseAssetTypes.BaseItem), x => contentPack.itemDefs.Add(x)),
                () => contentLoadHelper.DispatchLoad<EquipmentDef>(executingAssembly, typeof(MysticsRisky2Utils.BaseAssetTypes.BaseEquipment), x => contentPack.equipmentDefs.Add(x)),
                () => contentLoadHelper.DispatchLoad<BuffDef>(executingAssembly, typeof(MysticsRisky2Utils.BaseAssetTypes.BaseBuff), x => contentPack.buffDefs.Add(x)),
                () => contentLoadHelper.DispatchLoad<EliteDef>(executingAssembly, typeof(MysticsRisky2Utils.BaseAssetTypes.BaseElite), x => contentPack.eliteDefs.Add(x)),
                () => contentLoadHelper.DispatchLoad<GameObject>(executingAssembly, typeof(MysticsRisky2Utils.BaseAssetTypes.BaseInteractable), null),
                () => contentLoadHelper.DispatchLoad<GameObject>(executingAssembly, typeof(MysticsRisky2Utils.BaseAssetTypes.BaseCharacterBody), x => contentPack.bodyPrefabs.Add(x)),
                () => contentLoadHelper.DispatchLoad<GameObject>(executingAssembly, typeof(MysticsRisky2Utils.BaseAssetTypes.BaseCharacterMaster), x => contentPack.masterPrefabs.Add(x))
            };
            int num = 0;
            for (int i = 0; i < loadDispatchers.Length; i = num)
            {
                loadDispatchers[i]();
                args.ReportProgress(Util.Remap((float)(i + 1), 0f, (float)loadDispatchers.Length, 0f, 0.05f));
                yield return null;
                num = i + 1;
            }

            // Start loading content. Longest part of the loading process, so we will dedicate most of the progress bar to it
            while (contentLoadHelper.coroutine.MoveNext())
            {
                args.ReportProgress(Util.Remap(contentLoadHelper.progress.value, 0f, 1f, 0.05f, 0.9f));
                yield return contentLoadHelper.coroutine.Current;
            }

            // Populate static content pack fields and add various prefabs and scriptable objects generated during the content loading part to the content pack
            loadDispatchers = new System.Action[]
            {
                () => ContentLoadHelper.PopulateTypeFields(typeof(Items), contentPack.itemDefs),
                () => ContentLoadHelper.PopulateTypeFields(typeof(Equipment), contentPack.equipmentDefs),
                () => ContentLoadHelper.PopulateTypeFields(typeof(Buffs), contentPack.buffDefs),
                () => ContentLoadHelper.PopulateTypeFields(typeof(Elites), contentPack.eliteDefs),
                () => contentPack.bodyPrefabs.Add(Resources.bodyPrefabs.ToArray()),
                () => contentPack.masterPrefabs.Add(Resources.masterPrefabs.ToArray()),
                () => contentPack.projectilePrefabs.Add(Resources.projectilePrefabs.ToArray()),
                () => contentPack.gameModePrefabs.Add(Resources.gameModePrefabs.ToArray()),
                () => contentPack.networkedObjectPrefabs.Add(Resources.networkedObjectPrefabs.ToArray()),
                () => contentPack.effectDefs.Add(Resources.effectPrefabs.ConvertAll(x => new EffectDef(x)).ToArray()),
                () => contentPack.networkSoundEventDefs.Add(Resources.networkSoundEventDefs.ToArray()),
                () => contentPack.unlockableDefs.Add(Resources.unlockableDefs.ToArray()),
                () => contentPack.entityStateTypes.Add(Resources.entityStateTypes.ToArray()),
                () => contentPack.skillDefs.Add(Resources.skillDefs.ToArray()),
                () => contentPack.skillFamilies.Add(Resources.skillFamilies.ToArray()),
                () => contentPack.sceneDefs.Add(Resources.sceneDefs.ToArray()),
                () => contentPack.gameEndingDefs.Add(Resources.gameEndingDefs.ToArray())
            };
            for (int i = 0; i < loadDispatchers.Length; i = num)
            {
                loadDispatchers[i]();
                args.ReportProgress(Util.Remap((float)(i + 1), 0f, (float)loadDispatchers.Length, 0.9f, 0.95f));
                yield return null;
                num = i + 1;
            }

            // Call "AfterContentPackLoaded" methods
            loadDispatchers = new System.Action[]
            {
                () => MysticsRisky2Utils.ContentManagement.ContentLoadHelper.InvokeAfterContentPackLoaded<MysticsRisky2Utils.BaseAssetTypes.BaseItem>(executingAssembly),
                () => MysticsRisky2Utils.ContentManagement.ContentLoadHelper.InvokeAfterContentPackLoaded<MysticsRisky2Utils.BaseAssetTypes.BaseEquipment>(executingAssembly),
                () => MysticsRisky2Utils.ContentManagement.ContentLoadHelper.InvokeAfterContentPackLoaded<MysticsRisky2Utils.BaseAssetTypes.BaseBuff>(executingAssembly),
                () => MysticsRisky2Utils.ContentManagement.ContentLoadHelper.InvokeAfterContentPackLoaded<MysticsRisky2Utils.BaseAssetTypes.BaseElite>(executingAssembly),
                () => MysticsRisky2Utils.ContentManagement.ContentLoadHelper.InvokeAfterContentPackLoaded<MysticsRisky2Utils.BaseAssetTypes.BaseInteractable>(executingAssembly),
                () => MysticsRisky2Utils.ContentManagement.ContentLoadHelper.InvokeAfterContentPackLoaded<MysticsRisky2Utils.BaseAssetTypes.BaseCharacterMaster>(executingAssembly),
                () => MysticsRisky2Utils.ContentManagement.ContentLoadHelper.InvokeAfterContentPackLoaded<MysticsRisky2Utils.BaseAssetTypes.BaseCharacterBody>(executingAssembly)
            };
            for (int i = 0; i < loadDispatchers.Length; i = num)
            {
                loadDispatchers[i]();
                args.ReportProgress(Util.Remap((float)(i + 1), 0f, (float)loadDispatchers.Length, 0.95f, 0.99f));
                yield return null;
                num = i + 1;
            }

            loadDispatchers = null;
            yield break;
        }

        public IEnumerator GenerateContentPackAsync(GetContentPackAsyncArgs args)
        {
            ContentPack.Copy(contentPack, args.output);
            args.ReportProgress(1f);
            yield break;
        }

        public IEnumerator FinalizeAsync(FinalizeAsyncArgs args)
        {
            args.ReportProgress(1f);
            yield break;
        }

        private ContentPack contentPack = new ContentPack();

        public static class Resources
        {
            public static List<GameObject> bodyPrefabs = new List<GameObject>();
            public static List<GameObject> masterPrefabs = new List<GameObject>();
            public static List<GameObject> projectilePrefabs = new List<GameObject>();
            public static List<GameObject> effectPrefabs = new List<GameObject>();
            public static List<GameObject> gameModePrefabs = new List<GameObject>();
            public static List<GameObject> networkedObjectPrefabs = new List<GameObject>();
            public static List<NetworkSoundEventDef> networkSoundEventDefs = new List<NetworkSoundEventDef>();
            public static List<UnlockableDef> unlockableDefs = new List<UnlockableDef>();
            public static List<System.Type> entityStateTypes = new List<System.Type>();
            public static List<RoR2.Skills.SkillDef> skillDefs = new List<RoR2.Skills.SkillDef>();
            public static List<RoR2.Skills.SkillFamily> skillFamilies = new List<RoR2.Skills.SkillFamily>();
            public static List<SceneDef> sceneDefs = new List<SceneDef>();
            public static List<GameEndingDef> gameEndingDefs = new List<GameEndingDef>();
        }

        public static class Items
        {
            public static ItemDef RisingTides_MirrorClone;
        }

        public static class Buffs
        {
            public static BuffDef RisingTides_AffixWater;
            public static BuffDef RisingTides_WaterInvincibility;
            public static BuffDef RisingTides_WaterBubbleTrapCooldown;
            public static BuffDef RisingTides_AffixBarrier;
            public static BuffDef RisingTides_MaxBarrierGained;
            public static BuffDef RisingTides_AffixBlackHole;
            public static BuffDef RisingTides_AffectedByVoidFreeze;
            public static BuffDef RisingTides_BlackHoleMark;
            public static BuffDef RisingTides_AffixMoney;
            public static BuffDef RisingTides_AffixNight;
            public static BuffDef RisingTides_NightSpeedBoost;
            public static BuffDef RisingTides_NightReducedVision;
            public static BuffDef RisingTides_AffixImpPlane;
            public static BuffDef RisingTides_ImpPlaneScar;
            public static BuffDef RisingTides_ImpPlaneDotImmunity;
            public static BuffDef RisingTides_AffixMirror;
        }

        public static class Elites
        {
            public static EliteDef RisingTides_Water;
            public static EliteDef RisingTides_Barrier;
            public static EliteDef RisingTides_BlackHole;
            public static EliteDef RisingTides_Money;
            public static EliteDef RisingTides_Night;
            public static EliteDef RisingTides_ImpPlane;
            public static EliteDef RisingTides_Mirror;
        }

        public static class Equipment
        {
            public static EquipmentDef RisingTides_AffixWater;
            public static EquipmentDef RisingTides_AffixBarrier;
            public static EquipmentDef RisingTides_AffixBlackHole;
            public static EquipmentDef RisingTides_AffixMoney;
            public static EquipmentDef RisingTides_AffixNight;
            public static EquipmentDef RisingTides_AffixImpPlane;
            public static EquipmentDef RisingTides_AffixMirror;
        }
    }
}
