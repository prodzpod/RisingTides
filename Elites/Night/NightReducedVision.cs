using MysticsRisky2Utils;
using MysticsRisky2Utils.BaseAssetTypes;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using R2API;

namespace RisingTides.Buffs
{
    public class NightReducedVision : BaseBuff
    {
		public static ConfigOptions.ConfigurableValue<float> visionDistance = ConfigOptions.ConfigurableValue.CreateFloat(
			RisingTidesPlugin.PluginGUID, RisingTidesPlugin.PluginName, RisingTidesPlugin.config,
			"Elites: Nocturnal", "Debuff Vision Distance",
			20f,
			description: "How far should you be able to see when debuffed by this elite? (in metres)",
			useDefaultValueConfigEntry: RisingTidesPlugin.ignoreBalanceChanges.bepinexConfigEntry
		);

		public override void OnLoad()
		{
			base.OnLoad();
			buffDef.name = "RisingTides_NightReducedVision";
			buffDef.iconSprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/texBuffCloakIcon.tif").WaitForCompletion();
			buffDef.buffColor = new Color32(64, 0, 155, 255);

            On.RoR2.CharacterBody.RecalculateStats += CharacterBody_RecalculateStats;
		}

        private void CharacterBody_RecalculateStats(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
        {
			orig(self);
			if (self.HasBuff(buffDef))
				self.visionDistance = Mathf.Min(self.visionDistance, visionDistance);
		}
	}
}
