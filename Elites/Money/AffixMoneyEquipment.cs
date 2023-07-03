using MysticsRisky2Utils;
using R2API;
using RoR2;
using RoR2.Orbs;
using System.Linq;
using UnityEngine;

namespace RisingTides.Equipment
{
    public class AffixMoneyEquipment : BaseEliteAffix
    {
		public static ConfigOptions.ConfigurableValue<float> duration = ConfigOptions.ConfigurableValue.CreateFloat(
			RisingTidesPlugin.PluginGUID, RisingTidesPlugin.PluginName, RisingTidesPlugin.config,
			"Elites: Magnetic", "On Use Shield Duration",
			7f,
			description: "How long should this elite aspect's on-use projectile-stopping aura last? (in seconds)",
			useDefaultValueConfigEntry: RisingTidesPlugin.ignoreBalanceChanges.bepinexConfigEntry
		);

		public override void OnLoad()
        {
            base.OnLoad();
            equipmentDef.name = "RisingTides_AffixMoney";
            equipmentDef.cooldown = 30f;
            equipmentDef.pickupIconSprite = RisingTidesPlugin.AssetBundle.LoadAsset<Sprite>("Assets/Mods/RisingTides/Elites/Money/texAffixMoneyEquipmentIcon.png");
            SetUpPickupModel();
            AdjustElitePickupMaterial(Color.white, 1.6f, RisingTidesPlugin.AssetBundle.LoadAsset<Texture2D>("Assets/Mods/RisingTides/Elites/Money/texMoneyWaterRecolorRamp.png"));

            itemDisplayPrefab = PrepareItemDisplayModel(PrefabAPI.InstantiateClone(RisingTidesPlugin.AssetBundle.LoadAsset<GameObject>("Assets/Mods/RisingTides/Elites/Money/AffixMoneyHeadpiece.prefab"), "RisingTidesAffixMoneyHeadpiece", false));
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
				var component = equipmentSlot.characterBody.GetComponent<Buffs.AffixMoney.RisingTidesAffixMoneyBehaviour>();
				if (component && component.aura)
				{
					component.aura.buffTimer = duration;
					component.aura.buffed = true;
					return true;
				}
            }
            return false;
        }

		public override void AfterContentPackLoaded()
        {
            base.AfterContentPackLoaded();
            equipmentDef.passiveBuffDef = RisingTidesContent.Buffs.RisingTides_AffixMoney;
        }
    }
}
