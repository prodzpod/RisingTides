using MysticsRisky2Utils.BaseAssetTypes;
using UnityEngine;
using R2API;
using RoR2;
using MysticsRisky2Utils;

namespace RisingTides.Elites
{
    public class Night : BaseElite
    {
        public override void OnLoad()
        {
            base.OnLoad();
            eliteDef.name = "RisingTides_Night";
            vanillaTier = 1;
            isHonor = true;
            ConfigOptions.ConfigurableValue.CreateFloat(
                RisingTidesPlugin.PluginGUID, RisingTidesPlugin.PluginName, RisingTidesPlugin.config,
                "Elites: Nocturnal", "Health Boost Coefficient",
                4f,
                description: "How much health this elite should have? (e.g. 18 means it will have 18x health)",
                onChanged: (newValue) => eliteDef.healthBoostCoefficient = newValue
            );
            ConfigOptions.ConfigurableValue.CreateFloat(
                RisingTidesPlugin.PluginGUID, RisingTidesPlugin.PluginName, RisingTidesPlugin.config,
                "Elites: Nocturnal", "Damage Boost Coefficient",
                2f,
                description: "How much damage this elite should have? (e.g. 6 means it will have 6x damage)",
                onChanged: (newValue) => eliteDef.damageBoostCoefficient = newValue
            );
            EliteRamp.AddRamp(eliteDef, RisingTidesPlugin.AssetBundle.LoadAsset<Texture2D>("Assets/Mods/RisingTides/Elites/Night/texAffixNightRecolorRamp.png"));
        }

        public override void AfterContentPackLoaded()
        {
            base.AfterContentPackLoaded();
            ConfigOptions.ConfigurableValue.CreateBool(
                RisingTidesPlugin.PluginGUID, RisingTidesPlugin.PluginName, RisingTidesPlugin.config,
                "Enabled Elites", "Nocturnal",
                true,
                onChanged: (newValue) => eliteDef.eliteEquipmentDef = newValue ? RisingTidesContent.Equipment.RisingTides_AffixNight : null
            );
        }
    }
}
