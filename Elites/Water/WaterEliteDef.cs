using MysticsRisky2Utils.BaseAssetTypes;
using UnityEngine;
using R2API;
using RoR2;
using MysticsRisky2Utils;

namespace RisingTides.Elites
{
    public class Water : BaseElite
    {
        public override void OnLoad()
        {
            base.OnLoad();
            eliteDef.name = "RisingTides_Water";
            vanillaTier = 2;
            isHonor = false;
            ConfigOptions.ConfigurableValue.CreateFloat(
                RisingTidesPlugin.PluginGUID, RisingTidesPlugin.PluginName, RisingTidesPlugin.config,
                "Elites: Aquamarine", "Health Boost Coefficient",
                18f,
                description: "How much health this elite should have? (e.g. 18 means it will have 18x health)",
                onChanged: (newValue) => eliteDef.healthBoostCoefficient = newValue
            );
            ConfigOptions.ConfigurableValue.CreateFloat(
                RisingTidesPlugin.PluginGUID, RisingTidesPlugin.PluginName, RisingTidesPlugin.config,
                "Elites: Aquamarine", "Damage Boost Coefficient",
                6f,
                description: "How much damage this elite should have? (e.g. 6 means it will have 6x damage)",
                onChanged: (newValue) => eliteDef.damageBoostCoefficient = newValue
            );
            EliteRamp.AddRamp(eliteDef, RisingTidesPlugin.AssetBundle.LoadAsset<Texture2D>("Assets/Mods/RisingTides/Elites/Water/texAffixWaterRecolorRamp.png"));

            modelEffect = RisingTidesPlugin.AssetBundle.LoadAsset<GameObject>("Assets/Mods/RisingTides/Elites/Water/AffixWaterEffect.prefab");
            
            lightColorOverride = new Color32(122, 255, 241, 255);
            particleMaterialOverride = RisingTidesPlugin.AssetBundle.LoadAsset<Material>("Assets/Mods/RisingTides/Elites/Water/matWaterMist.mat");

            onModelEffectSpawn = (model, effect) =>
            {
                if (model.body)
                {
                    effect.transform.localScale += Vector3.one * model.body.radius;
                }
                Util.PlaySound("RisingTides_Play_elite_aquamarine_spawn", effect);
            };
        }

        public override void AfterContentPackLoaded()
        {
            base.AfterContentPackLoaded();
            eliteDef.eliteEquipmentDef = RisingTidesContent.Equipment.RisingTides_AffixWater;
        }
    }
}
