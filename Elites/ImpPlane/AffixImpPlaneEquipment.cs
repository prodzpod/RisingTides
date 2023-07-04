using MysticsRisky2Utils;
using R2API;
using RoR2;
using UnityEngine;

namespace RisingTides.Equipment
{
    public class AffixImpPlaneEquipment : BaseEliteAffix
    {
        public static ConfigOptions.ConfigurableValue<float> duration = ConfigOptions.ConfigurableValue.CreateFloat(
            RisingTidesPlugin.PluginGUID, RisingTidesPlugin.PluginName, RisingTidesPlugin.config,
            "Elites: Realgar", "On Use DoT Immunity Duration",
            10f,
            description: "How long should this elite aspect's on-use damage-over-time immunity last? (in seconds)",
            useDefaultValueConfigEntry: RisingTidesPlugin.ignoreBalanceChanges.bepinexConfigEntry
        );

        public override void OnLoad()
        {
            base.OnLoad();
            equipmentDef.name = "RisingTides_AffixImpPlane";
            equipmentDef.cooldown = 20f;
            equipmentDef.pickupIconSprite = RisingTidesPlugin.AssetBundle.LoadAsset<Sprite>("Assets/Mods/RisingTides/Elites/ImpPlane/texAffixImpPlaneEquipmentIcon.png");
            SetUpPickupModel();
            AdjustElitePickupMaterial(new Color32(50, 50, 50, 255), 0.5f, RisingTidesPlugin.AssetBundle.LoadAsset<Texture>("Assets/Mods/RisingTides/Elites/ImpPlane/texRampEliteImpPlane.png"));

            itemDisplayPrefab = PrepareItemDisplayModel(PrefabAPI.InstantiateClone(RisingTidesPlugin.AssetBundle.LoadAsset<GameObject>("Assets/Mods/RisingTides/Elites/ImpPlane/AffixImpPlaneHeadpiece.prefab"), "RisingTidesAffixImpPlaneHeadpiece", false));
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
                effectData.SetNetworkedObjectReference(equipmentSlot.characterBody.gameObject);
                EffectManager.SpawnEffect(Buffs.AffixImpPlane.scarVFX, effectData, true);

                equipmentSlot.characterBody.AddTimedBuff(RisingTidesContent.Buffs.RisingTides_ImpPlaneDotImmunity, duration);
                return true;
            }
            return false;
        }

		public override void AfterContentPackLoaded()
        {
            base.AfterContentPackLoaded();
            equipmentDef.passiveBuffDef = RisingTidesContent.Buffs.RisingTides_AffixImpPlane;
        }
    }
}
