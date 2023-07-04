using MysticsRisky2Utils;
using MysticsRisky2Utils.BaseAssetTypes;
using R2API;
using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using RoR2.Projectile;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.Rendering.PostProcessing;

namespace RisingTides.Buffs
{
	public class AffixImpPlane : BaseBuff
	{
		public static ConfigOptions.ConfigurableValue<float> riftProjectileInterval = ConfigOptions.ConfigurableValue.CreateFloat(
			RisingTidesPlugin.PluginGUID, RisingTidesPlugin.PluginName, RisingTidesPlugin.config,
			"Elites: Realgar", "Rift Projectile Interval",
			0.27f,
			description: "How often should the rift shoot a projectile? (in seconds)",
			useDefaultValueConfigEntry: RisingTidesPlugin.ignoreBalanceChanges.bepinexConfigEntry
		);
		public static ConfigOptions.ConfigurableValue<float> riftProjectileDamage = ConfigOptions.ConfigurableValue.CreateFloat(
			RisingTidesPlugin.PluginGUID, RisingTidesPlugin.PluginName, RisingTidesPlugin.config,
			"Elites: Realgar", "Rift Projectile Damage",
			60f,
			description: "How much damage should the rift's projectiles deal? (in %, relative to the owner's damage)",
			useDefaultValueConfigEntry: RisingTidesPlugin.ignoreBalanceChanges.bepinexConfigEntry
		);
		public static ConfigOptions.ConfigurableValue<float> scarDuration = ConfigOptions.ConfigurableValue.CreateFloat(
			RisingTidesPlugin.PluginGUID, RisingTidesPlugin.PluginName, RisingTidesPlugin.config,
			"Elites: Realgar", "Scar Duration",
			4f,
			description: "How long should this elite's debuff DoT last? (in seconds)",
			useDefaultValueConfigEntry: RisingTidesPlugin.ignoreBalanceChanges.bepinexConfigEntry
		);

		public static GameObject riftPrefab;
		public static GameObject riftProjectilePrefab;

		public static DamageAPI.ModdedDamageType scarDamageType;
		public static DamageAPI.ModdedDamageType cannotScarDamageType;
		public static GameObject scarVFX;

		public override void OnPluginAwake()
        {
            base.OnPluginAwake();
			scarDamageType = DamageAPI.ReserveDamageType();
			cannotScarDamageType = DamageAPI.ReserveDamageType();
		}

		public override void OnLoad()
		{
			base.OnLoad();
			buffDef.name = "RisingTides_AffixImpPlane";
			buffDef.iconSprite = RisingTidesPlugin.AssetBundle.LoadAsset<Sprite>("Assets/Mods/RisingTides/Elites/ImpPlane/texAffixImpPlaneBuffIcon.png");

			Overlays.CreateOverlay(RisingTidesPlugin.AssetBundle.LoadAsset<Material>("Assets/Mods/RisingTides/Elites/ImpPlane/matAffixImpPlaneOverlay.mat"), (model) =>
			{
				return model.body && model.body.HasBuff(buffDef);
			});

			On.RoR2.CharacterBody.OnBuffFirstStackGained += CharacterBody_OnBuffFirstStackGained;
			On.RoR2.CharacterBody.OnBuffFinalStackLost += CharacterBody_OnBuffFinalStackLost;

			GenericGameEvents.OnHitEnemy += GenericGameEvents_OnHitEnemy;

			riftPrefab = RisingTidesPlugin.AssetBundle.LoadAsset<GameObject>("Assets/Mods/RisingTides/Elites/ImpPlane/ImpPlaneRift.prefab");
			riftProjectilePrefab = PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/ArtifactShell/ArtifactShellSeekingSolarFlare.prefab").WaitForCompletion(), "RisingTidesAffixImpPlaneRiftProjectile", true);
			riftProjectilePrefab.AddComponent<DamageAPI.ModdedDamageTypeHolderComponent>().Add(cannotScarDamageType);
			RisingTidesContent.Resources.projectilePrefabs.Add(riftProjectilePrefab);

			scarVFX = RisingTidesPlugin.AssetBundle.LoadAsset<GameObject>("Assets/Mods/RisingTides/Elites/ImpPlane/ImpPlaneScarVFX.prefab");
			var effectComponent = scarVFX.AddComponent<EffectComponent>();
			effectComponent.applyScale = true;
			effectComponent.soundName = "RisingTides_Play_realgar_scar";
			var vfxAttributes = scarVFX.AddComponent<VFXAttributes>();
			vfxAttributes.vfxIntensity = VFXAttributes.VFXIntensity.Low;
			vfxAttributes.vfxPriority = VFXAttributes.VFXPriority.Low;
			scarVFX.AddComponent<DestroyOnTimer>().duration = 2f;
			RisingTidesContent.Resources.effectPrefabs.Add(scarVFX);

			if (RisingTidesPlugin.mysticsItemsCompatibility)
			{
				RoR2Application.onLoad += () =>
				{
					MysticsItems.OtherModCompat.ElitePotion_AddSpreadEffect(buffDef, vfx: Addressables.LoadAssetAsync<GameObject>("RoR2/Base/BleedOnHitAndExplode/BleedOnHitAndExplode_Explosion.prefab").WaitForCompletion(), procCoefficient: 1f, moddedDamageType: scarDamageType);
				};
			}
		}

		private void GenericGameEvents_OnHitEnemy(DamageInfo damageInfo, MysticsRisky2UtilsPlugin.GenericCharacterInfo attackerInfo, MysticsRisky2UtilsPlugin.GenericCharacterInfo victimInfo)
        {
            if (!damageInfo.rejected && damageInfo.procCoefficient > 0f && attackerInfo.body && (attackerInfo.body.HasBuff(buffDef) || damageInfo.HasModdedDamageType(scarDamageType)) && !damageInfo.HasModdedDamageType(cannotScarDamageType) && victimInfo.body)
            {
				if (!victimInfo.body.HasBuff(RisingTidesContent.Buffs.RisingTides_ImpPlaneScar))
                {
					var effectData = new EffectData
					{
						origin = victimInfo.body.corePosition
					};
					effectData.SetNetworkedObjectReference(victimInfo.gameObject);
					EffectManager.SpawnEffect(scarVFX, effectData, true);
                }
				foreach (var teamMember in TeamComponent.GetTeamMembers(victimInfo.teamIndex))
                {
					if (teamMember.body && !teamMember.body.HasBuff(RisingTidesContent.Buffs.RisingTides_ImpPlaneScar))
					{
						DotController.InflictDot(
							teamMember.gameObject,
							attackerInfo.gameObject,
							ImpPlaneScar.dotIndex,
							scarDuration * damageInfo.procCoefficient,
							1f
						);
					}
				}
			}
        }

		public override void AfterContentPackLoaded()
		{
			base.AfterContentPackLoaded();
			buffDef.eliteDef = RisingTidesContent.Elites.RisingTides_ImpPlane;
		}

		private void CharacterBody_OnBuffFirstStackGained(On.RoR2.CharacterBody.orig_OnBuffFirstStackGained orig, CharacterBody self, BuffDef buffDef)
		{
			orig(self, buffDef);
			if (buffDef == this.buffDef)
			{
				var component = self.GetComponent<RisingTidesAffixImpPlaneBehaviour>();
				if (!component)
				{
					component = self.gameObject.AddComponent<RisingTidesAffixImpPlaneBehaviour>();
				}
				else if (!component.enabled)
				{
					component.enabled = true;
				}
			}
		}

		private void CharacterBody_OnBuffFinalStackLost(On.RoR2.CharacterBody.orig_OnBuffFinalStackLost orig, CharacterBody self, BuffDef buffDef)
		{
			orig(self, buffDef);
			if (buffDef == this.buffDef)
			{
				var component = self.GetComponent<RisingTidesAffixImpPlaneBehaviour>();
				if (component && component.enabled)
				{
					component.enabled = false;
				}
			}
		}

		public class RisingTidesAffixImpPlaneBehaviour : MonoBehaviour
		{
			public CharacterBody body;
			public GameObject riftObject;
			public float projectileTimer;
			
			public void Awake()
			{
				body = GetComponent<CharacterBody>();
				CreateRift();
			}

			public void CreateRift()
            {
				if (riftObject) return;
				riftObject = Instantiate(riftPrefab, transform.position + Vector3.up * 3f, Quaternion.identity);
			}

			public void FixedUpdate()
			{
				if (!body.healthComponent || !body.healthComponent.alive) return;

				projectileTimer -= Time.fixedDeltaTime;
				if (projectileTimer <= 0f)
                {
					projectileTimer += riftProjectileInterval;
					FireProjectile();
                }
			}

			public void FireProjectile()
            {
				if (riftObject && body.hasEffectiveAuthority)
                {
					FireProjectileInfo fireProjectileInfo = new FireProjectileInfo();
					fireProjectileInfo.position = riftObject.transform.position;
					fireProjectileInfo.rotation = Quaternion.Euler(
						RoR2Application.rng.RangeFloat(-60f, 60f),
						RoR2Application.rng.RangeFloat(0f, 360f),
						0f
					);
					fireProjectileInfo.crit = body.RollCrit();
					fireProjectileInfo.damage = body.damage * (riftProjectileDamage / 100f);
					fireProjectileInfo.damageColorIndex = DamageColorIndex.Default;
					fireProjectileInfo.owner = gameObject;
					fireProjectileInfo.procChainMask = default;
					fireProjectileInfo.projectilePrefab = riftProjectilePrefab;
					fireProjectileInfo.speedOverride = 12f;
					ProjectileManager.instance.FireProjectile(fireProjectileInfo);
				}
            }

			public void OnEnable()
            {
				CreateRift();
            }

			public void OnDisable()
            {
				Destroy(riftObject);
            }
		}
	}
}
