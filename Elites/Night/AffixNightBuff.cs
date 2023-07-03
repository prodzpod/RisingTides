using Mono.Cecil.Cil;
using MonoMod.Cil;
using MysticsRisky2Utils;
using MysticsRisky2Utils.BaseAssetTypes;
using R2API;
using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace RisingTides.Buffs
{
    public class AffixNight : BaseBuff
	{
		public static ConfigOptions.ConfigurableValue<float> debuffDuration = ConfigOptions.ConfigurableValue.CreateFloat(
			RisingTidesPlugin.PluginGUID, RisingTidesPlugin.PluginName, RisingTidesPlugin.config,
			"Elites: Nocturnal", "Debuff Duration",
			4f,
			description: "How long should this elite's on-hit debuff last? (in seconds)",
			useDefaultValueConfigEntry: RisingTidesPlugin.ignoreBalanceChanges.bepinexConfigEntry
		);

		public override void OnLoad()
		{
			base.OnLoad();
			buffDef.name = "RisingTides_AffixNight";
			buffDef.iconSprite = RisingTidesPlugin.AssetBundle.LoadAsset<Sprite>("Assets/Mods/RisingTides/Elites/Night/texAffixNightBuffIcon.png");

            On.RoR2.CharacterBody.OnOutOfDangerChanged += CharacterBody_OnOutOfDangerChanged;
            On.RoR2.CharacterBody.OnBuffFirstStackGained += CharacterBody_OnBuffFirstStackGained;
            On.RoR2.CharacterBody.OnBuffFinalStackLost += CharacterBody_OnBuffFinalStackLost;
            
			GenericGameEvents.OnHitEnemy += GenericGameEvents_OnHitEnemy;
            
			if (RisingTidesPlugin.mysticsItemsCompatibility)
			{
				RoR2Application.onLoad += () =>
				{
					MysticsItems.OtherModCompat.ElitePotion_AddSpreadEffect(buffDef, vfx: Addressables.LoadAssetAsync<GameObject>("RoR2/Junk/ArchWisp/OmniExplosionVFXArchWispCannonImpact.prefab").WaitForCompletion(), procCoefficient: 1f, damageType: DamageType.Stun1s);
				};
			}
		}

        private void CharacterBody_OnOutOfDangerChanged(On.RoR2.CharacterBody.orig_OnOutOfDangerChanged orig, CharacterBody self)
        {
			orig(self);
			if (NetworkServer.active && self.HasBuff(buffDef))
            {
				if (self.outOfDanger && !self.HasBuff(RisingTidesContent.Buffs.RisingTides_NightSpeedBoost))
				{
					self.AddBuff(RisingTidesContent.Buffs.RisingTides_NightSpeedBoost);

					var effectData = new EffectData
					{
						origin = self.corePosition
					};
					if (self.characterDirection && self.characterDirection.moveVector != Vector3.zero)
						effectData.rotation = Util.QuaternionSafeLookRotation(self.characterDirection.moveVector);
                    else
						effectData.rotation = self.transform.rotation;
					EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/SprintActivate"), effectData, true);
				}
				else if (!self.outOfDanger && self.HasBuff(RisingTidesContent.Buffs.RisingTides_NightSpeedBoost))
					self.RemoveBuff(RisingTidesContent.Buffs.RisingTides_NightSpeedBoost);
			}
        }

		private void CharacterBody_OnBuffFirstStackGained(On.RoR2.CharacterBody.orig_OnBuffFirstStackGained orig, CharacterBody self, BuffDef buffDef)
		{
			orig(self, buffDef);
			if (buffDef == this.buffDef && self.outOfDanger && !self.HasBuff(RisingTidesContent.Buffs.RisingTides_NightSpeedBoost))
			{
				self.AddBuff(RisingTidesContent.Buffs.RisingTides_NightSpeedBoost);
			}
		}

		private void CharacterBody_OnBuffFinalStackLost(On.RoR2.CharacterBody.orig_OnBuffFinalStackLost orig, CharacterBody self, BuffDef buffDef)
		{
			orig(self, buffDef);
			if (buffDef == this.buffDef && self.HasBuff(RisingTidesContent.Buffs.RisingTides_NightSpeedBoost))
            {
				self.RemoveBuff(RisingTidesContent.Buffs.RisingTides_NightSpeedBoost);
            }
		}

		private void GenericGameEvents_OnHitEnemy(DamageInfo damageInfo, MysticsRisky2UtilsPlugin.GenericCharacterInfo attackerInfo, MysticsRisky2UtilsPlugin.GenericCharacterInfo victimInfo)
        {
			if (!damageInfo.rejected && damageInfo.procCoefficient > 0f && attackerInfo.body && attackerInfo.body.HasBuff(buffDef) && victimInfo.body)
			{
				victimInfo.body.AddTimedBuff(RisingTidesContent.Buffs.RisingTides_NightReducedVision, debuffDuration * damageInfo.procCoefficient);
			}
        }

		public override void AfterContentPackLoaded()
		{
			base.AfterContentPackLoaded();
			buffDef.eliteDef = RisingTidesContent.Elites.RisingTides_Night;
		}
	}
}
