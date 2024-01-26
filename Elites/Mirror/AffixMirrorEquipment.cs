using MysticsRisky2Utils;
using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace RisingTides.Equipment
{
    public class AffixMirrorEquipment : BaseEliteAffix
    {
        public static ConfigOptions.ConfigurableValue<float> buffDuration = ConfigOptions.ConfigurableValue.CreateFloat(
            RisingTidesPlugin.PluginGUID, RisingTidesPlugin.PluginName, RisingTidesPlugin.config,
            "Elites: Phenakite", "On Use Buff Duration",
            10f, 0f, 100f,
            description: "How long should this elite aspect's on-use buff last? (in seconds)",
            useDefaultValueConfigEntry: RisingTidesPlugin.ignoreBalanceChanges.bepinexConfigEntry
        );

        public override void OnLoad()
        {
            base.OnLoad();
            equipmentDef.name = "RisingTides_AffixMirror";
            equipmentDef.cooldown = 40f;
            equipmentDef.pickupIconSprite = RisingTidesPlugin.AssetBundle.LoadAsset<Sprite>("Assets/Mods/RisingTides/Elites/Mirror/texAffixMirrorEquipmentIcon.png");
            SetUpPickupModel();
            AdjustElitePickupMaterial(new Color32(50, 50, 50, 255), 1f, RisingTidesPlugin.AssetBundle.LoadAsset<Texture2D>("Assets/Mods/RisingTides/Elites/Mirror/texAffixMirrorRecolorRamp.png"));

            itemDisplayPrefab = PrepareItemDisplayModel(PrefabAPI.InstantiateClone(RisingTidesPlugin.AssetBundle.LoadAsset<GameObject>("Assets/Mods/RisingTides/Elites/Mirror/AffixMirrorHeadpiece.prefab"), "RisingTidesAffixMirrorHeadpiece", false));
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
                EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ProcStealthkit"), new EffectData
                {
                    origin = equipmentSlot.characterBody.corePosition,
                    rotation = Quaternion.identity
                }, true);

                equipmentSlot.characterBody.AddTimedBuff(RoR2Content.Buffs.Cloak, buffDuration);
                equipmentSlot.characterBody.AddTimedBuff(RoR2Content.Buffs.CloakSpeed, buffDuration);

                return true;
            }
            return false;
        }

        public override void AfterContentPackLoaded()
        {
            base.AfterContentPackLoaded();
            equipmentDef.passiveBuffDef = RisingTidesContent.Buffs.RisingTides_AffixMirror;
        }
    }
}
