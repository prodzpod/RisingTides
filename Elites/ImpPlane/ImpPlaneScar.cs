using MysticsRisky2Utils;
using MysticsRisky2Utils.BaseAssetTypes;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using R2API;

namespace RisingTides.Buffs
{
    public class ImpPlaneScar : BaseBuff
    {
		public static DotController.DotDef dotDef;
		public static DotController.DotIndex dotIndex;

		public override void OnPluginAwake()
        {
            base.OnPluginAwake();
			dotDef = new DotController.DotDef
			{
				resetTimerOnAdd = false,
				damageCoefficient = 0.05f,
				damageColorIndex = DamageColorIndex.SuperBleed,
				interval = 0.2f
			};
			dotIndex = DotAPI.RegisterDotDef(dotDef);
        }

        public override void OnLoad()
		{
			base.OnLoad();
			buffDef.name = "RisingTides_ImpPlaneScar";
			buffDef.iconSprite = RisingTidesPlugin.AssetBundle.LoadAsset<Sprite>("Assets/Mods/RisingTides/Elites/ImpPlane/texAffixImpPlaneScarIcon.png");
			buffDef.buffColor = new Color32(188, 15, 52, 255);
			buffDef.canStack = false;
			
			ConfigOptions.ConfigurableValue.CreateFloat(
				RisingTidesPlugin.PluginGUID, RisingTidesPlugin.PluginName, RisingTidesPlugin.config,
				"Elites: Realgar", "Scar Damage Per Tick",
				5f,
				description: "How much damage should this elite's on-hit damage-over-time debuff deal? (in %)",
				onChanged: (newValue) => dotDef.damageCoefficient = newValue / 100f,
				useDefaultValueConfigEntry: RisingTidesPlugin.ignoreBalanceChanges.bepinexConfigEntry
			);
			ConfigOptions.ConfigurableValue.CreateFloat(
				RisingTidesPlugin.PluginGUID, RisingTidesPlugin.PluginName, RisingTidesPlugin.config,
				"Elites: Realgar", "Scar Damage Interval",
				0.2f,
				description: "How often should this elite's on-hit damage-over-time debuff tick? (in seconds)",
				onChanged: (newValue) => dotDef.interval = newValue,
				useDefaultValueConfigEntry: RisingTidesPlugin.ignoreBalanceChanges.bepinexConfigEntry
			);

			dotDef.associatedBuff = buffDef;
		}
	}
}
