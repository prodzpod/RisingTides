using MysticsRisky2Utils.BaseAssetTypes;
using UnityEngine;
using R2API;
using RoR2;
using MysticsRisky2Utils;

namespace RisingTides.Elites
{
    public class Money : BaseElite
    {
        public override void OnLoad()
        {
            base.OnLoad();
            eliteDef.name = "RisingTides_Money";
            vanillaTier = 1;
            isHonor = true; ConfigOptions.ConfigurableValue.CreateFloat(
                 RisingTidesPlugin.PluginGUID, RisingTidesPlugin.PluginName, RisingTidesPlugin.config,
                 "Elites: Magnetic", "Health Boost Coefficient",
                 4f,
                 description: "How much health this elite should have? (e.g. 18 means it will have 18x health)",
                 onChanged: (newValue) => eliteDef.healthBoostCoefficient = newValue
             );
            ConfigOptions.ConfigurableValue.CreateFloat(
                RisingTidesPlugin.PluginGUID, RisingTidesPlugin.PluginName, RisingTidesPlugin.config,
                "Elites: Magnetic", "Damage Boost Coefficient",
                2f,
                description: "How much damage this elite should have? (e.g. 6 means it will have 6x damage)",
                onChanged: (newValue) => eliteDef.damageBoostCoefficient = newValue
            );
            EliteRamp.AddRamp(eliteDef, RisingTidesPlugin.AssetBundle.LoadAsset<Texture2D>("Assets/Mods/RisingTides/Elites/Money/texMoneyWaterRecolorRamp.png"));
        }

        public override void AfterContentPackLoaded()
        {
            base.AfterContentPackLoaded();
            eliteDef.eliteEquipmentDef = RisingTidesContent.Equipment.RisingTides_AffixMoney;
        }
    }
}
