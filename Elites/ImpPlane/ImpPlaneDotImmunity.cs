using MysticsRisky2Utils;
using MysticsRisky2Utils.BaseAssetTypes;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace RisingTides.Buffs
{
    public class ImpPlaneDotImmunity : BaseBuff
    {
		public override void OnLoad()
		{
			base.OnLoad();
			buffDef.name = "RisingTides_ImpPlaneDotImmunity";
			buffDef.iconSprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Junk/Common/texBuffBodyArmorIcon.tif").WaitForCompletion();
			buffDef.buffColor = new Color32(230, 0, 60, 255);

            On.RoR2.CharacterBody.SetBuffCount += CharacterBody_SetBuffCount;
            On.RoR2.DotController.AddDot += DotController_AddDot;

			Overlays.CreateOverlay(RisingTidesPlugin.AssetBundle.LoadAsset<Material>("Assets/Mods/RisingTides/Elites/ImpPlane/matAffixImpPlaneBuffedOutline.mat"), (model) =>
			{
				return model.body && model.body.HasBuff(buffDef);
			});
		}

        private void DotController_AddDot(On.RoR2.DotController.orig_AddDot orig, DotController self, GameObject attackerObject, float duration, DotController.DotIndex dotIndex, float damageMultiplier, uint? maxStacksFromAttacker, float? totalDamage, DotController.DotIndex? preUpgradeDotIndex)
        {
			if (self.victimBody && self.victimBody.HasBuff(buffDef)) return;
			orig(self, attackerObject, duration, dotIndex, damageMultiplier, maxStacksFromAttacker, totalDamage, preUpgradeDotIndex);
        }

        private void CharacterBody_SetBuffCount(On.RoR2.CharacterBody.orig_SetBuffCount orig, CharacterBody self, BuffIndex buffType, int newCount)
        {
			orig(self, buffType, newCount);
			if (NetworkServer.active && buffType == buffDef.buffIndex && newCount > 0)
            {
				Util.CleanseBody(self, false, false, false, true, false, false);
			}
        }
    }
}
