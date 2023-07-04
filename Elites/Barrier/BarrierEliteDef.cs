using MysticsRisky2Utils.BaseAssetTypes;
using UnityEngine;
using R2API;
using RoR2;
using MysticsRisky2Utils;

namespace RisingTides.Elites
{
    public class Barrier : BaseElite
    {
        public override void OnLoad()
        {
            base.OnLoad();
            eliteDef.name = "RisingTides_Barrier";
            vanillaTier = 2;
            isHonor = false;
            ConfigOptions.ConfigurableValue.CreateFloat(
                RisingTidesPlugin.PluginGUID, RisingTidesPlugin.PluginName, RisingTidesPlugin.config,
                "Elites: Bismuth", "Health Boost Coefficient",
                18f,
                description: "How much health this elite should have? (e.g. 18 means it will have 18x health)",
                onChanged: (newValue) => eliteDef.healthBoostCoefficient = newValue
            );
            ConfigOptions.ConfigurableValue.CreateFloat(
                RisingTidesPlugin.PluginGUID, RisingTidesPlugin.PluginName, RisingTidesPlugin.config,
                "Elites: Bismuth", "Damage Boost Coefficient",
                6f,
                description: "How much damage this elite should have? (e.g. 6 means it will have 6x damage)",
                onChanged: (newValue) => eliteDef.damageBoostCoefficient = newValue
            );
            EliteRamp.AddRamp(eliteDef, RisingTidesPlugin.AssetBundle.LoadAsset<Texture2D>("Assets/Mods/RisingTides/Elites/Barrier/texAffixBarrierRecolorRamp.png"));

            modelEffect = RisingTidesPlugin.AssetBundle.LoadAsset<GameObject>("Assets/Mods/RisingTides/Elites/Barrier/AffixBarrierEffect.prefab");
            
            lightColorOverride = new Color32(255, 255, 255, 255);
            particleMaterialOverride = RisingTidesPlugin.AssetBundle.LoadAsset<Material>("Assets/Mods/RisingTides/Elites/Barrier/matAffixBarrierRainbowParticle.mat");

            onModelEffectSpawn = (model, effect) =>
            {
                if (model.body)
                {
                    effect.transform.localScale += Vector3.one * model.body.radius;
                }
                Util.PlaySound("RisingTides_Play_elite_bismuth_spawn", effect);
            };
        }

        public override void AfterContentPackLoaded()
        {
            base.AfterContentPackLoaded();
            ConfigOptions.ConfigurableValue.CreateBool(
                RisingTidesPlugin.PluginGUID, RisingTidesPlugin.PluginName, RisingTidesPlugin.config,
                "Enabled Elites", "Bismuth",
                true,
                onChanged: (newValue) => eliteDef.eliteEquipmentDef = newValue ? RisingTidesContent.Equipment.RisingTides_AffixBarrier : null
            );
        }
    }
}
