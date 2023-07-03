using MysticsRisky2Utils;
using MysticsRisky2Utils.BaseAssetTypes;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace RisingTides.Buffs
{
    public class WaterInvincibility : BaseBuff
    {
		public override void OnLoad()
		{
			base.OnLoad();
			buffDef.name = "RisingTides_WaterInvincibility";
			buffDef.iconSprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Junk/Common/texBuffBodyArmorIcon.tif").WaitForCompletion();
			buffDef.buffColor = new Color32(2, 149, 255, 255);

            GenericGameEvents.BeforeTakeDamage += GenericGameEvents_BeforeTakeDamage;

			Overlays.CreateOverlay(RisingTidesPlugin.AssetBundle.LoadAsset<Material>("Assets/Mods/RisingTides/Elites/Water/matAffixWaterInvincible.mat"), (model) =>
			{
				return model.body && model.body.HasBuff(buffDef);
			});
		}

        private void GenericGameEvents_BeforeTakeDamage(DamageInfo damageInfo, MysticsRisky2UtilsPlugin.GenericCharacterInfo attackerInfo, MysticsRisky2UtilsPlugin.GenericCharacterInfo victimInfo)
        {
            if (!damageInfo.rejected && victimInfo.body && victimInfo.body.HasBuff(buffDef))
            {
				EffectManager.SpawnEffect(HealthComponent.AssetReferences.damageRejectedPrefab, new EffectData
				{
					origin = damageInfo.position
				}, true);
				damageInfo.rejected = true;
			}
        }
	}
}
