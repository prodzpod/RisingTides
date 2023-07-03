using MysticsRisky2Utils;
using R2API;
using RoR2;
using RoR2.Orbs;
using System.Linq;
using UnityEngine;

namespace RisingTides.Equipment
{
    public class AffixNightEquipment : BaseEliteAffix
    {
		public static ConfigOptions.ConfigurableValue<float> duration = ConfigOptions.ConfigurableValue.CreateFloat(
			RisingTidesPlugin.PluginGUID, RisingTidesPlugin.PluginName, RisingTidesPlugin.config,
			"Elites: Nocturnal", "On Use Invisibility Duration",
			10f,
			description: "How long should this elite aspect's on-use invisibility last? (in seconds)",
			useDefaultValueConfigEntry: RisingTidesPlugin.ignoreBalanceChanges.bepinexConfigEntry
		);

		public override void OnLoad()
        {
            base.OnLoad();
            equipmentDef.name = "RisingTides_AffixNight";
            equipmentDef.cooldown = 40f;
            equipmentDef.pickupIconSprite = RisingTidesPlugin.AssetBundle.LoadAsset<Sprite>("Assets/Mods/RisingTides/Elites/Night/texAffixNightEquipmentIcon.png");
            SetUpPickupModel();
            AdjustElitePickupMaterial(new Color32(100, 100, 100, 255), 1f, RisingTidesPlugin.AssetBundle.LoadAsset<Texture2D>("Assets/Mods/RisingTides/Elites/Night/texAffixNightRecolorRamp.png"));

            itemDisplayPrefab = PrepareItemDisplayModel(PrefabAPI.InstantiateClone(RisingTidesPlugin.AssetBundle.LoadAsset<GameObject>("Assets/Mods/RisingTides/Elites/Night/AffixNightHeadpiece.prefab"), "RisingTidesAffixNightHeadpiece", false));
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
				equipmentSlot.characterBody.AddTimedBuff(RoR2Content.Buffs.Cloak, duration);
				EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ProcStealthkit"), new EffectData
				{
					origin = equipmentSlot.characterBody.corePosition,
					rotation = Quaternion.identity
				}, true);
				return true;
            }
            return false;
        }

		public override void AfterContentPackLoaded()
        {
            base.AfterContentPackLoaded();
            equipmentDef.passiveBuffDef = RisingTidesContent.Buffs.RisingTides_AffixNight;
        }
    }
}
