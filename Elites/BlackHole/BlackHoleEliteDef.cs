using MysticsRisky2Utils.BaseAssetTypes;
using UnityEngine;
using R2API;
using RoR2;
using MysticsRisky2Utils;

namespace RisingTides.Elites
{
    public class BlackHole : BaseElite
    {
        public override void OnLoad()
        {
            base.OnLoad();
            eliteDef.name = "RisingTides_BlackHole";
            vanillaTier = 2;
            isHonor = false;
            ConfigOptions.ConfigurableValue.CreateFloat(
                RisingTidesPlugin.PluginGUID, RisingTidesPlugin.PluginName, RisingTidesPlugin.config,
                "Elites: Onyx", "Health Boost Coefficient",
                18f,
                description: "How much health this elite should have? (e.g. 18 means it will have 18x health)",
                onChanged: (newValue) => eliteDef.healthBoostCoefficient = newValue
            );
            ConfigOptions.ConfigurableValue.CreateFloat(
                RisingTidesPlugin.PluginGUID, RisingTidesPlugin.PluginName, RisingTidesPlugin.config,
                "Elites: Onyx", "Damage Boost Coefficient",
                6f,
                description: "How much damage this elite should have? (e.g. 6 means it will have 6x damage)",
                onChanged: (newValue) => eliteDef.damageBoostCoefficient = newValue
            );
            EliteRamp.AddRamp(eliteDef, RisingTidesPlugin.AssetBundle.LoadAsset<Texture2D>("Assets/Mods/RisingTides/Elites/BlackHole/texAffixBlackHoleRecolorRamp.png"));

            modelEffect = RisingTidesPlugin.AssetBundle.LoadAsset<GameObject>("Assets/Mods/RisingTides/Elites/BlackHole/AffixBlackHoleEffect.prefab");
            var jitterBones = modelEffect.AddComponent<JitterBones>();
            jitterBones.perlinNoiseFrequency = 1f;
            jitterBones.perlinNoiseStrength = 0.3f;
            jitterBones.perlinNoiseMinimumCutoff = 0f;
            jitterBones.perlinNoiseMaximumCutoff = 1f;
            jitterBones.headBonusStrength = 80f;

            lightColorOverride = new Color32(84, 0, 255, 255);
            particleMaterialOverride = RisingTidesPlugin.AssetBundle.LoadAsset<Material>("Assets/Mods/RisingTides/Elites/BlackHole/matAffixBlackHoleParticleOverride.mat");

            onModelEffectSpawn = (model, effect) =>
            {
                if (model.body)
                {
                    effect.transform.localScale += Vector3.one * model.body.radius;
                }
                Util.PlaySound("RisingTides_Play_elite_onyx_spawn", effect);
                if (model.mainSkinnedMeshRenderer)
                {
                    JitterBones[] components = effect.GetComponents<JitterBones>();
                    for (int i = 0; i < components.Length; i++)
                    {
                        components[i].skinnedMeshRenderer = model.mainSkinnedMeshRenderer;
                    }
                }
            };
        }

        public override void AfterContentPackLoaded()
        {
            base.AfterContentPackLoaded();
            ConfigOptions.ConfigurableValue.CreateBool(
                RisingTidesPlugin.PluginGUID, RisingTidesPlugin.PluginName, RisingTidesPlugin.config,
                "Enabled Elites", "Onyx",
                true,
                onChanged: (newValue) => eliteDef.eliteEquipmentDef = newValue ? RisingTidesContent.Equipment.RisingTides_AffixBlackHole : null
            );
        }
    }
}
