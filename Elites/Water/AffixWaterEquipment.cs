using MysticsRisky2Utils;
using R2API;
using RoR2;
using UnityEngine;

namespace RisingTides.Equipment
{
    public class AffixWaterEquipment : BaseEliteAffix
    {
        public static ConfigOptions.ConfigurableValue<float> healAmount = ConfigOptions.ConfigurableValue.CreateFloat(
            RisingTidesPlugin.PluginGUID, RisingTidesPlugin.PluginName, RisingTidesPlugin.config,
            "Elites: Aquamarine", "On Use Heal",
            10f,
            description: "How much health should this elite aspect's on-use heal regenerate? (in %)",
            useDefaultValueConfigEntry: RisingTidesPlugin.ignoreBalanceChanges.bepinexConfigEntry
        );

        public override void OnLoad()
        {
            base.OnLoad();
            equipmentDef.name = "RisingTides_AffixWater";
            equipmentDef.cooldown = 30f;
            equipmentDef.pickupIconSprite = RisingTidesPlugin.AssetBundle.LoadAsset<Sprite>("Assets/Mods/RisingTides/Elites/Water/texAffixWaterEquipmentIcon.png");
            SetUpPickupModel();
            AdjustElitePickupMaterial(new Color32(2, 149, 255, 255), 7f, true);

            itemDisplayPrefab = PrepareItemDisplayModel(PrefabAPI.InstantiateClone(RisingTidesPlugin.AssetBundle.LoadAsset<GameObject>("Assets/Mods/RisingTides/Elites/Water/AffixWaterHeadParticles.prefab"), "RisingTidesAffixWaterHeadpiece", false));
            onSetupIDRS += () =>
            {
                foreach (var body in BodyCatalog.allBodyPrefabBodyBodyComponents)
                {
                    var characterModel = body.GetComponentInChildren<CharacterModel>();
                    if (characterModel && characterModel.itemDisplayRuleSet != null)
                    {
                        var iceCrown = characterModel.itemDisplayRuleSet.GetEquipmentDisplayRuleGroup(RoR2Content.Equipment.AffixWhite.equipmentIndex);
                        if (!iceCrown.Equals(DisplayRuleGroup.empty))
                        {
                            var bodyName = BodyCatalog.GetBodyName(body.bodyIndex);
                            foreach (var displayRule in iceCrown.rules)
                            {
                                AddDisplayRule(bodyName, displayRule.childName, displayRule.localPos, displayRule.localAngles, displayRule.localScale);
                            }
                        }
                    }
                }
            };
        }

        public override bool OnUse(EquipmentSlot equipmentSlot)
        {
            if (equipmentSlot.characterBody)
            {
                var effectData = new EffectData
                {
                    origin = equipmentSlot.characterBody.corePosition
                };
                effectData.SetHurtBoxReference(equipmentSlot.characterBody.mainHurtBox);
                EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/CleanseEffect"), effectData, true);

                Util.CleanseBody(equipmentSlot.characterBody, true, false, true, true, true, false);
                if (equipmentSlot.characterBody.healthComponent)
                {
                    equipmentSlot.characterBody.healthComponent.HealFraction(healAmount / 100f, default);
                }

                return true;
            }
            return false;
        }

        public override void AfterContentPackLoaded()
        {
            base.AfterContentPackLoaded();
            equipmentDef.passiveBuffDef = RisingTidesContent.Buffs.RisingTides_AffixWater;
        }
    }
}
