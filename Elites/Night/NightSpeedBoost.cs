using MysticsRisky2Utils;
using MysticsRisky2Utils.BaseAssetTypes;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using R2API;

namespace RisingTides.Buffs
{
    public class NightSpeedBoost : BaseBuff
    {
		public static ConfigOptions.ConfigurableValue<float> movementSpeed = ConfigOptions.ConfigurableValue.CreateFloat(
			RisingTidesPlugin.PluginGUID, RisingTidesPlugin.PluginName, RisingTidesPlugin.config,
			"Elites: Nocturnal", "Movement Speed",
			100f,
			description: "How much faster should this elite move out of danger? (in %)",
			useDefaultValueConfigEntry: RisingTidesPlugin.ignoreBalanceChanges.bepinexConfigEntry
		);
		public static ConfigOptions.ConfigurableValue<float> attackSpeed = ConfigOptions.ConfigurableValue.CreateFloat(
			RisingTidesPlugin.PluginGUID, RisingTidesPlugin.PluginName, RisingTidesPlugin.config,
			"Elites: Nocturnal", "Attack Speed",
			30f,
			description: "How much faster should this elite attack out of danger? (in %)",
			useDefaultValueConfigEntry: RisingTidesPlugin.ignoreBalanceChanges.bepinexConfigEntry
		);

		public override void OnLoad()
		{
			base.OnLoad();
			buffDef.name = "RisingTides_NightSpeedBoost";
			buffDef.iconSprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/texMovespeedBuffIcon.tif").WaitForCompletion();
			buffDef.buffColor = new Color32(64, 0, 155, 255);

            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
		}

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
			if (sender.HasBuff(buffDef))
			{
				args.moveSpeedMultAdd += movementSpeed / 100f;
				args.attackSpeedMultAdd += attackSpeed / 100f;
			}
        }
	}
}
