using MysticsRisky2Utils;
using MysticsRisky2Utils.BaseAssetTypes;
using RoR2;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using R2API;
using MonoMod.Cil;
using Mono.Cecil.Cil;

namespace RisingTides.Buffs
{
    public class AffixBarrier : BaseBuff
	{
		public static ConfigOptions.ConfigurableValue<float> healthReduction = ConfigOptions.ConfigurableValue.CreateFloat(
			RisingTidesPlugin.PluginGUID, RisingTidesPlugin.PluginName, RisingTidesPlugin.config,
			"Elites: Bismuth", "Health Reduction",
			50f, 0f, 100f,
			description: "How much less health should this elite have? (in %)",
			useDefaultValueConfigEntry: RisingTidesPlugin.ignoreBalanceChanges.bepinexConfigEntry
		);
		public static ConfigOptions.ConfigurableValue<float> barrierDamageResistance = ConfigOptions.ConfigurableValue.CreateFloat(
			RisingTidesPlugin.PluginGUID, RisingTidesPlugin.PluginName, RisingTidesPlugin.config,
			"Elites: Bismuth", "Barrier Damage Resistance",
			50f, 0f, 100f,
			description: "How much less damage should this elite take when barrier is active? (in %)",
			useDefaultValueConfigEntry: RisingTidesPlugin.ignoreBalanceChanges.bepinexConfigEntry
		);
		public static ConfigOptions.ConfigurableValue<bool> barrierKnockbackResistance = ConfigOptions.ConfigurableValue.CreateBool(
			RisingTidesPlugin.PluginGUID, RisingTidesPlugin.PluginName, RisingTidesPlugin.config,
			"Elites: Bismuth", "Barrier Knockback Resistance",
			true,
			description: "Should this elite ignore knockback when barrier is active?",
			useDefaultValueConfigEntry: RisingTidesPlugin.ignoreBalanceChanges.bepinexConfigEntry
		);
		public static ConfigOptions.ConfigurableValue<float> barrierDecayRate = ConfigOptions.ConfigurableValue.CreateFloat(
			RisingTidesPlugin.PluginGUID, RisingTidesPlugin.PluginName, RisingTidesPlugin.config,
			"Elites: Bismuth", "Barrier Decay Rate",
			0f, 0f, 100f,
			description: "How quickly should this elite's barrier decay? (in %)",
			useDefaultValueConfigEntry: RisingTidesPlugin.ignoreBalanceChanges.bepinexConfigEntry
		);
		public static ConfigOptions.ConfigurableValue<float> startingBarrier = ConfigOptions.ConfigurableValue.CreateFloat(
			RisingTidesPlugin.PluginGUID, RisingTidesPlugin.PluginName, RisingTidesPlugin.config,
			"Elites: Bismuth", "Starting Barrier",
			100f, 0f, 100f,
			description: "How much barrier should this elite spawn with? (in %)",
			useDefaultValueConfigEntry: RisingTidesPlugin.ignoreBalanceChanges.bepinexConfigEntry
		);
		public static ConfigOptions.ConfigurableValue<float> debuffDuration = ConfigOptions.ConfigurableValue.CreateFloat(
			RisingTidesPlugin.PluginGUID, RisingTidesPlugin.PluginName, RisingTidesPlugin.config,
			"Elites: Bismuth", "Debuff Duration",
			1f,
			description: "How long should the on-hit debuff last? (in seconds)",
			useDefaultValueConfigEntry: RisingTidesPlugin.ignoreBalanceChanges.bepinexConfigEntry
		);

		public static Sprite barrierBarSprite;

		public override void OnLoad()
		{
			base.OnLoad();
			buffDef.name = "RisingTides_AffixBarrier";
			buffDef.iconSprite = RisingTidesPlugin.AssetBundle.LoadAsset<Sprite>("Assets/Mods/RisingTides/Elites/Barrier/texAffixBarrierBuffIcon.png");

			Overlays.CreateOverlay(RisingTidesPlugin.AssetBundle.LoadAsset<Material>("Assets/Mods/RisingTides/Elites/Barrier/matAffixBarrierOverlay.mat"), (model) =>
			{
				return model.body && model.body.HasBuff(buffDef);
			});

			On.RoR2.HealthComponent.TakeDamageForce_Vector3_bool_bool += HealthComponent_TakeDamageForce_Vector3_bool_bool;
            On.RoR2.HealthComponent.TakeDamageForce_DamageInfo_bool_bool += HealthComponent_TakeDamageForce_DamageInfo_bool_bool;
            GenericGameEvents.OnApplyDamageReductionModifiers += GenericGameEvents_OnApplyDamageReductionModifiers;
			On.RoR2.CharacterBody.RecalculateStats += CharacterBody_RecalculateStats;
			IL.RoR2.CharacterBody.RecalculateStats += CharacterBody_RecalculateStats1;
            On.RoR2.CharacterBody.OnBuffFinalStackLost += CharacterBody_OnBuffFinalStackLost;
			barrierBarSprite = RisingTidesPlugin.AssetBundle.LoadAsset<Sprite>("Assets/Mods/RisingTides/Elites/Barrier/texAffixBarrierBarRecolor.png");
			On.RoR2.UI.HealthBar.UpdateBarInfos += HealthBar_UpdateBarInfos;
			
			GenericGameEvents.OnHitEnemy += GenericGameEvents_OnHitEnemy;

			if (RisingTidesPlugin.mysticsItemsCompatibility)
			{
				RoR2Application.onLoad += () =>
				{
					MysticsItems.OtherModCompat.ElitePotion_AddSpreadEffect(buffDef, vfx: Equipment.AffixBarrierEquipment.selfBuffUseEffect, dot: DotController.DotIndex.Bleed, damageType: default);
					MysticsItems.OtherModCompat.ElitePotion_AddSpreadEffect(buffDef, dot: DotController.DotIndex.Burn, damageType: default);
					MysticsItems.OtherModCompat.ElitePotion_AddSpreadEffect(buffDef, debuff: RoR2Content.Buffs.BeetleJuice, damageType: default);
					MysticsItems.OtherModCompat.ElitePotion_AddSpreadEffect(buffDef, dot: DotController.DotIndex.Poison, damageType: default);
					MysticsItems.OtherModCompat.ElitePotion_AddSpreadEffect(buffDef, debuff: RoR2Content.Buffs.Slow80, damageType: default);
					MysticsItems.OtherModCompat.ElitePotion_AddSpreadEffect(buffDef, debuff: RoR2Content.Buffs.Cripple, damageType: default);
					MysticsItems.OtherModCompat.ElitePotion_AddSpreadEffect(buffDef, debuff: RoR2Content.Buffs.LunarSecondaryRoot, damageType: default);
				};
			}
		}

        private void HealthComponent_TakeDamageForce_Vector3_bool_bool(On.RoR2.HealthComponent.orig_TakeDamageForce_Vector3_bool_bool orig, HealthComponent self, Vector3 force, bool alwaysApply, bool disableAirControlUntilCollision)
        {
			if (barrierKnockbackResistance && self.body.HasBuff(buffDef) && self.barrier > 0f) return;
			orig(self, force, alwaysApply, disableAirControlUntilCollision);
        }

		private void HealthComponent_TakeDamageForce_DamageInfo_bool_bool(On.RoR2.HealthComponent.orig_TakeDamageForce_DamageInfo_bool_bool orig, HealthComponent self, DamageInfo damageInfo, bool alwaysApply, bool disableAirControlUntilCollision)
		{
			if (barrierKnockbackResistance && self.body.HasBuff(buffDef) && self.barrier > 0f) return;
			orig(self, damageInfo, alwaysApply, disableAirControlUntilCollision);
		}

		private void GenericGameEvents_OnApplyDamageReductionModifiers(DamageInfo damageInfo, MysticsRisky2UtilsPlugin.GenericCharacterInfo attackerInfo, MysticsRisky2UtilsPlugin.GenericCharacterInfo victimInfo, ref float damage)
		{
			if (victimInfo.body && victimInfo.body.HasBuff(buffDef))
			{
				damage *= 1f - barrierDamageResistance / 100f;
			}
		}

		public void CharacterBody_RecalculateStats(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
		{
			orig(self);
			if (self.HasBuff(buffDef))
			{
				self.barrierDecayRate *= barrierDecayRate / 100f;
				if (NetworkServer.active && !self.HasBuff(RisingTidesContent.Buffs.RisingTides_MaxBarrierGained))
                {
					var spawnBarrier = self.maxBarrier * (startingBarrier / 100f);
					if (self.healthComponent && self.healthComponent.barrier < spawnBarrier)
					{
						self.healthComponent.Networkbarrier = spawnBarrier;
					}
					self.AddBuff(RisingTidesContent.Buffs.RisingTides_MaxBarrierGained);
				}
			}
		}

		private void CharacterBody_RecalculateStats1(ILContext il)
		{
			ILCursor c = new ILCursor(il);

			if (c.TryGotoNext(
				MoveType.After,
				x => x.MatchCallOrCallvirt(typeof(CharacterBody), "set_maxShield")
			))
			{
				c.Emit(OpCodes.Ldarg, 0);
				c.EmitDelegate<System.Action<CharacterBody>>((body) =>
				{
					if (body.HasBuff(buffDef))
					{
						body.maxHealth *= 1f - healthReduction / 100f;
					}
				});
			}
		}

		private void CharacterBody_OnBuffFinalStackLost(On.RoR2.CharacterBody.orig_OnBuffFinalStackLost orig, CharacterBody self, BuffDef buffDef)
		{
			orig(self, buffDef);
			if (NetworkServer.active && buffDef == this.buffDef && self.HasBuff(RisingTidesContent.Buffs.RisingTides_MaxBarrierGained))
            {
				self.RemoveBuff(RisingTidesContent.Buffs.RisingTides_MaxBarrierGained);
            }
		}

		private void HealthBar_UpdateBarInfos(On.RoR2.UI.HealthBar.orig_UpdateBarInfos orig, RoR2.UI.HealthBar self)
		{
			orig(self);

			if (self.source && self.source.body && self.source.body.HasBuff(buffDef))
			{
				if (self.source.barrier > 0f)
				{
					ref RoR2.UI.HealthBar.BarInfo healthBarStyle = ref self.barInfoCollection.trailingOverHealthbarInfo;
					healthBarStyle.color = Color.Lerp(healthBarStyle.color, Color.black, 0.45f * self.source.barrier / self.source.fullHealth);
				}
				ref RoR2.UI.HealthBar.BarInfo barrierBarStyle = ref self.barInfoCollection.barrierBarInfo;
				barrierBarStyle.color = Color.white;
				barrierBarStyle.sprite = barrierBarSprite;
				barrierBarStyle.sizeDelta += 2f;
			}
		}

		private void GenericGameEvents_OnHitEnemy(DamageInfo damageInfo, MysticsRisky2UtilsPlugin.GenericCharacterInfo attackerInfo, MysticsRisky2UtilsPlugin.GenericCharacterInfo victimInfo)
        {
            if (!damageInfo.rejected && damageInfo.procCoefficient > 0f && attackerInfo.body && attackerInfo.body.HasBuff(buffDef) && victimInfo.body)
            {
				uint? dotMaxStacksFromAttacker = null;
				if ((damageInfo != null) ? damageInfo.inflictor : null)
				{
					var component = damageInfo.inflictor.GetComponent<RoR2.Projectile.ProjectileDamage>();
					if (component && component.useDotMaxStacksFromAttacker)
					{
						dotMaxStacksFromAttacker = new uint?(component.dotMaxStacksFromAttacker);
					}
				}
				var currentDebuffDuration = debuffDuration * damageInfo.procCoefficient;

				var rng = RoR2Application.rng.RangeInt(0, 7);
				switch (rng)
                {
					case 0:
						var inflictDotInfo = new InflictDotInfo
						{
							victimObject = victimInfo.gameObject,
							attackerObject = attackerInfo.gameObject,
							dotIndex = DotController.DotIndex.Bleed,
							damageMultiplier = 1f,
							duration = currentDebuffDuration,
							maxStacksFromAttacker = dotMaxStacksFromAttacker
						};
						DotController.InflictDot(ref inflictDotInfo);
						break;
					case 1:
						inflictDotInfo = new InflictDotInfo
						{
							victimObject = victimInfo.gameObject,
							attackerObject = attackerInfo.gameObject,
							totalDamage = damageInfo.damage * 0.5f,
							dotIndex = DotController.DotIndex.Burn,
							damageMultiplier = 1f,
							maxStacksFromAttacker = dotMaxStacksFromAttacker
						};
						if (attackerInfo.inventory)
							StrengthenBurnUtils.CheckDotForUpgrade(attackerInfo.inventory, ref inflictDotInfo);
						DotController.InflictDot(ref inflictDotInfo);
						break;
					case 2:
						victimInfo.body.AddTimedBuff(RoR2Content.Buffs.BeetleJuice, currentDebuffDuration);
						break;
					case 3:
						inflictDotInfo = new InflictDotInfo
						{
							victimObject = victimInfo.gameObject,
							attackerObject = attackerInfo.gameObject,
							dotIndex = DotController.DotIndex.Poison,
							damageMultiplier = 1f,
							duration = currentDebuffDuration,
							maxStacksFromAttacker = dotMaxStacksFromAttacker
						};
						DotController.InflictDot(ref inflictDotInfo);
						break;
					case 4:
						victimInfo.body.AddTimedBuff(RoR2Content.Buffs.Slow80, currentDebuffDuration);
						break;
					case 5:
						victimInfo.body.AddTimedBuff(RoR2Content.Buffs.Cripple, currentDebuffDuration);
						break;
					case 6:
						victimInfo.body.AddTimedBuff(RoR2Content.Buffs.LunarSecondaryRoot, currentDebuffDuration);
						break;
				}
			}
        }

		public override void AfterContentPackLoaded()
		{
			base.AfterContentPackLoaded();
			buffDef.eliteDef = RisingTidesContent.Elites.RisingTides_Barrier;
		}
	}
}
