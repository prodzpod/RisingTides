using MysticsRisky2Utils;
using MysticsRisky2Utils.BaseAssetTypes;
using R2API;
using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Rendering.PostProcessing;

namespace RisingTides.Buffs
{
	public class AffixBlackHole : BaseBuff
	{
		public static ConfigOptions.ConfigurableValue<float> pullDistance = ConfigOptions.ConfigurableValue.CreateFloat(
			RisingTidesPlugin.PluginGUID, RisingTidesPlugin.PluginName, RisingTidesPlugin.config,
			"Elites: Onyx", "Pull Distance",
			25f,
			description: "How far should the pull radius be? (in metres)",
			useDefaultValueConfigEntry: RisingTidesPlugin.ignoreBalanceChanges.bepinexConfigEntry
		);
		public static ConfigOptions.ConfigurableValue<float> pullPower = ConfigOptions.ConfigurableValue.CreateFloat(
			RisingTidesPlugin.PluginGUID, RisingTidesPlugin.PluginName, RisingTidesPlugin.config,
			"Elites: Onyx", "Pull Power",
			6f,
			description: "How strong should the pull be? (in m/s)",
			useDefaultValueConfigEntry: RisingTidesPlugin.ignoreBalanceChanges.bepinexConfigEntry
		);
		public static ConfigOptions.ConfigurableValue<bool> pullWeakerByDistance = ConfigOptions.ConfigurableValue.CreateBool(
			RisingTidesPlugin.PluginGUID, RisingTidesPlugin.PluginName, RisingTidesPlugin.config,
			"Elites: Onyx", "Pull Weaker By Distance",
			true,
			description: "Should the pull get weaker the further away you are from the elite?",
			useDefaultValueConfigEntry: RisingTidesPlugin.ignoreBalanceChanges.bepinexConfigEntry
		);
		public static ConfigOptions.ConfigurableValue<bool> pullWeakerByHP = ConfigOptions.ConfigurableValue.CreateBool(
			RisingTidesPlugin.PluginGUID, RisingTidesPlugin.PluginName, RisingTidesPlugin.config,
			"Elites: Onyx", "Pull Weaker By HP",
			true,
			description: "Should the pull get weaker the more current health the elite has?",
			useDefaultValueConfigEntry: RisingTidesPlugin.ignoreBalanceChanges.bepinexConfigEntry
		);
		public static ConfigOptions.ConfigurableValue<float> markChance = ConfigOptions.ConfigurableValue.CreateFloat(
			RisingTidesPlugin.PluginGUID, RisingTidesPlugin.PluginName, RisingTidesPlugin.config,
			"Elites: Onyx", "Mark Chance",
			100f, 0f, 100f,
			description: "How likely should this elite be to inflict the on-hit debuff? (in %)",
			useDefaultValueConfigEntry: RisingTidesPlugin.ignoreBalanceChanges.bepinexConfigEntry
		);
		public static ConfigOptions.ConfigurableValue<float> markDuration = ConfigOptions.ConfigurableValue.CreateFloat(
			RisingTidesPlugin.PluginGUID, RisingTidesPlugin.PluginName, RisingTidesPlugin.config,
			"Elites: Onyx", "Mark Duration",
			30f,
			description: "How long should the on-hit debuff last? (in seconds)",
			useDefaultValueConfigEntry: RisingTidesPlugin.ignoreBalanceChanges.bepinexConfigEntry
		);
		public static ConfigOptions.ConfigurableValue<float> markBaseDamage = ConfigOptions.ConfigurableValue.CreateFloat(
			RisingTidesPlugin.PluginGUID, RisingTidesPlugin.PluginName, RisingTidesPlugin.config,
			"Elites: Onyx", "Mark Base Damage",
			60f,
			description: "How much base damage should the debuff do on the 7th stack? (scales with level)",
			useDefaultValueConfigEntry: RisingTidesPlugin.ignoreBalanceChanges.bepinexConfigEntry
		);

		public static float implosionRadius = 16f;
		public static float implosionDelay = 2f;

		public static Material voidFrozenMaterial;
		public static GameObject implosionBuildUpPrefab;
		public static GameObject implosionEffect;
		public static GameObject implosionEffectMild;

		public static DamageAPI.ModdedDamageType blackHoleMarkDamageType;
		public static DamageColorIndex blackDamageColor;

		public override void OnPluginAwake()
        {
            base.OnPluginAwake();
			implosionBuildUpPrefab = Utils.CreateBlankPrefab("RisingTidesAffixBlackHoleImplosion", true);
			NetworkingAPI.RegisterMessageType<SyncVoidFrozen>();

			blackHoleMarkDamageType = DamageAPI.ReserveDamageType();
			blackDamageColor = DamageColorAPI.RegisterDamageColor(Color.black);
		}

		public override void OnLoad()
		{
			base.OnLoad();
			buffDef.name = "RisingTides_AffixBlackHole";
			buffDef.iconSprite = RisingTidesPlugin.AssetBundle.LoadAsset<Sprite>("Assets/Mods/RisingTides/Elites/BlackHole/texAffixBlackHoleBuffIcon.png");

			Overlays.CreateOverlay(RisingTidesPlugin.AssetBundle.LoadAsset<Material>("Assets/Mods/RisingTides/Elites/BlackHole/matAffixBlackHoleOverlay.mat"), (model) =>
			{
				return model.body && model.body.HasBuff(buffDef);
			});

			On.RoR2.CharacterBody.OnBuffFirstStackGained += CharacterBody_OnBuffFirstStackGained;
			On.RoR2.CharacterBody.OnBuffFinalStackLost += CharacterBody_OnBuffFinalStackLost;
			On.RoR2.GlobalEventManager.OnCharacterDeath += GlobalEventManager_OnCharacterDeath;

			GenericGameEvents.OnHitEnemy += GenericGameEvents_OnHitEnemy;

			voidFrozenMaterial = RisingTidesPlugin.AssetBundle.LoadAsset<Material>("Assets/Mods/RisingTides/Elites/BlackHole/matAffixBlackHoleVoidFrozenOverlay.mat");

			Utils.CopyChildren(RisingTidesPlugin.AssetBundle.LoadAsset<GameObject>("Assets/Mods/RisingTides/Elites/BlackHole/BlackHoleImplosionBuildUp.prefab"), implosionBuildUpPrefab);
			implosionBuildUpPrefab.transform.localScale = Vector3.one * implosionRadius;
			var implosionController = implosionBuildUpPrefab.AddComponent<RisingTidesAffixBlackHoleImplosionController>();
			var teamFilter = implosionBuildUpPrefab.AddComponent<TeamFilter>();
			teamFilter.defaultTeam = TeamIndex.Monster;
			implosionController.growingSphereTransform = implosionController.transform.Find("GrowingSphere");
			implosionController.innerSphereTransform = implosionController.transform.Find("GrowingSphere/Inner");
			implosionController.particleSystems.Add(implosionController.transform.Find("GrowingSphere/Stars").GetComponent<ParticleSystem>());
			implosionController.particleSystems.Add(implosionController.transform.Find("GrowingSphere/InwardTrails").GetComponent<ParticleSystem>());
			var postProcessDuration = implosionBuildUpPrefab.AddComponent<PostProcessDuration>();
			postProcessDuration.ppVolume = implosionBuildUpPrefab.transform.Find("PostProcessing").GetComponent<PostProcessVolume>();
			postProcessDuration.ppWeightCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
			postProcessDuration.maxDuration = implosionDelay;
			var shakeEmitter = implosionBuildUpPrefab.transform.Find("GrowingSphere").gameObject.AddComponent<ShakeEmitter>();
			shakeEmitter.wave = new Wave
			{
				amplitude = 1.4f,
				frequency = 6f
			};
			shakeEmitter.scaleShakeRadiusWithLocalScale = true;
			shakeEmitter.radius = 3f;
			shakeEmitter.duration = 100f;

			implosionEffect = RisingTidesPlugin.AssetBundle.LoadAsset<GameObject>("Assets/Mods/RisingTides/Elites/BlackHole/BlackHoleImplosion.prefab");
			implosionEffect.AddComponent<DestroyOnTimer>().duration = 1.5f;
			var effectComponent = implosionEffect.AddComponent<EffectComponent>();
			effectComponent.applyScale = true;
			var vfxAttributes = implosionEffect.AddComponent<VFXAttributes>();
			vfxAttributes.vfxIntensity = VFXAttributes.VFXIntensity.Low;
			vfxAttributes.vfxPriority = VFXAttributes.VFXPriority.Always;
			postProcessDuration = implosionEffect.transform.Find("PostProcessing").gameObject.AddComponent<PostProcessDuration>();
			postProcessDuration.ppVolume = postProcessDuration.GetComponent<PostProcessVolume>();
			postProcessDuration.ppWeightCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
			postProcessDuration.maxDuration = 0.2f;
			shakeEmitter = implosionEffect.AddComponent<ShakeEmitter>();
			shakeEmitter.wave = new Wave
			{
				amplitude = 3f,
				frequency = 9f
			};
			shakeEmitter.radius = 5f;
			shakeEmitter.duration = 0.3f;
			shakeEmitter.amplitudeTimeDecay = true;
			shakeEmitter.scaleShakeRadiusWithLocalScale = true;
			RisingTidesContent.Resources.effectPrefabs.Add(implosionEffect);

			implosionEffectMild = PrefabAPI.InstantiateClone(implosionEffect, implosionEffect.name + "Mild", false);
			Object.Destroy(implosionEffectMild.transform.Find("PostProcessing").gameObject);
			Object.Destroy(implosionEffectMild.GetComponent<ShakeEmitter>());
			implosionEffectMild.GetComponent<EffectComponent>().soundName = "RisingTides_Play_onyx_mark_explosion";
			shakeEmitter = implosionEffectMild.AddComponent<ShakeEmitter>();
			shakeEmitter.wave = new Wave
			{
				amplitude = 1.7f,
				frequency = 7f
			};
			shakeEmitter.radius = 4f;
			shakeEmitter.duration = 0.3f;
			shakeEmitter.amplitudeTimeDecay = true;
			shakeEmitter.scaleShakeRadiusWithLocalScale = true;
			RisingTidesContent.Resources.effectPrefabs.Add(implosionEffectMild);

			VoidFrozenState.frozenEffectPrefab = implosionEffectMild;

			if (RisingTidesPlugin.mysticsItemsCompatibility)
			{
				RoR2Application.onLoad += () =>
				{
					MysticsItems.OtherModCompat.ElitePotion_AddSpreadEffect(buffDef, vfx: implosionEffectMild, procCoefficient: 1f, moddedDamageType: blackHoleMarkDamageType);
				};
			}
		}

		private void GenericGameEvents_OnHitEnemy(DamageInfo damageInfo, MysticsRisky2UtilsPlugin.GenericCharacterInfo attackerInfo, MysticsRisky2UtilsPlugin.GenericCharacterInfo victimInfo)
        {
            if (!damageInfo.rejected && damageInfo.procCoefficient > 0f && Util.CheckRoll(100f * damageInfo.procCoefficient, attackerInfo.master) && attackerInfo.body && (attackerInfo.body.HasBuff(buffDef) || damageInfo.HasModdedDamageType(blackHoleMarkDamageType)) && victimInfo.body)
            {
				if (victimInfo.body.GetBuffCount(RisingTidesContent.Buffs.RisingTides_BlackHoleMark) >= 6)
				{
					EffectManager.SpawnEffect(implosionEffectMild, new EffectData
					{
						origin = victimInfo.body.corePosition,
						scale = victimInfo.body.radius
					}, true);

					victimInfo.body.ClearTimedBuffs(RisingTidesContent.Buffs.RisingTides_BlackHoleMark);

					if (victimInfo.healthComponent)
					{
						var markDamageInfo = new DamageInfo
						{
							attacker = damageInfo.attacker,
							damage = markBaseDamage * (1f + 0.2f * (attackerInfo.body.level - 1f)),
							crit = attackerInfo.body.RollCrit(),
							procCoefficient = 0f,
							damageColorIndex = blackDamageColor,
							position = victimInfo.body.corePosition
						};
						victimInfo.healthComponent.TakeDamage(markDamageInfo);
						GlobalEventManager.instance.OnHitEnemy(markDamageInfo, victimInfo.gameObject);
						GlobalEventManager.instance.OnHitAll(markDamageInfo, victimInfo.gameObject);
					}
                }
                else
                {
					victimInfo.body.AddTimedBuff(RisingTidesContent.Buffs.RisingTides_BlackHoleMark, markDuration);
				}
			}
        }

		public override void AfterContentPackLoaded()
		{
			base.AfterContentPackLoaded();
			buffDef.eliteDef = RisingTidesContent.Elites.RisingTides_BlackHole;
		}

		private void CharacterBody_OnBuffFirstStackGained(On.RoR2.CharacterBody.orig_OnBuffFirstStackGained orig, CharacterBody self, BuffDef buffDef)
		{
			orig(self, buffDef);
			if (buffDef == this.buffDef)
			{
				var component = self.GetComponent<RisingTidesAffixBlackHoleBehaviour>();
				if (!component)
				{
					component = self.gameObject.AddComponent<RisingTidesAffixBlackHoleBehaviour>();
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
				var component = self.GetComponent<RisingTidesAffixBlackHoleBehaviour>();
				if (component && component.enabled)
				{
					component.enabled = false;
				}
			}
		}

		public class RisingTidesAffixBlackHoleBehaviour : MonoBehaviour
		{
			public CharacterBody body;
			public SphereSearch sphereSearch;
			public float pullDistanceSquared = 0f;
			
			public struct PullTargetInfo
			{
				public CharacterBody characterBody;
				public IDisplacementReceiver displacementReceiver;
			}
			public List<PullTargetInfo> pullTargets = new List<PullTargetInfo>();
			public float pullTargetRefreshTimer = 0f;
			public float pullTargetRefreshInterval = 1f;

			public void Awake()
			{
				pullDistanceSquared = pullDistance * pullDistance;

				body = GetComponent<CharacterBody>();

				sphereSearch = new SphereSearch();
				sphereSearch.radius = pullDistance;
				sphereSearch.queryTriggerInteraction = QueryTriggerInteraction.Ignore;
				sphereSearch.mask = LayerIndex.entityPrecise.mask;
			}

			public void FixedUpdate()
			{
				if (!body.healthComponent || !body.healthComponent.alive) return;

				pullTargetRefreshTimer -= Time.fixedDeltaTime;
				if (pullTargetRefreshTimer <= 0f)
				{
					pullTargetRefreshTimer += pullTargetRefreshInterval;
					RefreshPullTargets();
				}

				var pullPoint = body.corePosition;
				foreach (var target in pullTargets)
				{
					if (target.characterBody && target.displacementReceiver != null)
					{
						var vector = pullPoint - target.characterBody.corePosition;
						var currentPullPower = pullPower.Value;
						if (pullWeakerByDistance) currentPullPower *= (1f - vector.sqrMagnitude / pullDistanceSquared);
						if (pullWeakerByHP) currentPullPower *= Mathf.Clamp01(1f - body.healthComponent.combinedHealthFraction);
						target.displacementReceiver.AddDisplacement(vector.normalized * currentPullPower * Time.fixedDeltaTime);
					}
				}
			}

			public void RefreshPullTargets()
			{
				pullTargets.Clear();

				sphereSearch.origin = body.corePosition;
				sphereSearch.RefreshCandidates();
				sphereSearch.FilterCandidatesByDistinctHurtBoxEntities();
				var hurtBoxes = sphereSearch.GetHurtBoxes();
				foreach (var hurtBox in hurtBoxes)
				{
					if (hurtBox.healthComponent && hurtBox.healthComponent.body && hurtBox.healthComponent.body != body && hurtBox.healthComponent.body.hasEffectiveAuthority && !hurtBox.healthComponent.body.HasBuff(RisingTidesContent.Buffs.RisingTides_AffixBlackHole) && !hurtBox.healthComponent.body.HasBuff(RisingTidesContent.Buffs.RisingTides_AffectedByVoidFreeze))
					{
						var displacementReceiver = hurtBox.healthComponent.body.GetComponent<IDisplacementReceiver>();
						if (displacementReceiver != null)
						{
							pullTargets.Add(new PullTargetInfo
							{
								characterBody = hurtBox.healthComponent.body,
								displacementReceiver = displacementReceiver
							});
						}
					}
				}
			}
		}

		private void GlobalEventManager_OnCharacterDeath(On.RoR2.GlobalEventManager.orig_OnCharacterDeath orig, GlobalEventManager self, DamageReport damageReport)
		{
			orig(self, damageReport);
			if (NetworkServer.active && damageReport.victimBody && damageReport.victimBody.HasBuff(buffDef))
            {
				var implosionSpot = damageReport.victimBody.corePosition;

				var sphereSearch = new SphereSearch();
				sphereSearch.origin = implosionSpot;
				sphereSearch.radius = implosionRadius;
				sphereSearch.queryTriggerInteraction = QueryTriggerInteraction.Ignore;
				sphereSearch.mask = LayerIndex.entityPrecise.mask;
				sphereSearch.RefreshCandidates();
				sphereSearch.FilterCandidatesByDistinctHurtBoxEntities();
				var teamMask = new TeamMask();
				teamMask.AddTeam(damageReport.victimTeamIndex);
				sphereSearch.FilterCandidatesByHurtBoxTeam(teamMask);
				var hurtBoxes = sphereSearch.GetHurtBoxes();
				foreach (var hurtBox in hurtBoxes)
				{
					if (hurtBox.healthComponent && hurtBox.healthComponent.alive && hurtBox.healthComponent.body && hurtBox.healthComponent.body != damageReport.victimBody && !hurtBox.healthComponent.body.HasBuff(RisingTidesContent.Buffs.RisingTides_AffixBlackHole))
					{
						var setStateOnHurt = hurtBox.healthComponent.body.GetComponent<SetStateOnHurt>();
						if (setStateOnHurt) SetVoidFrozenServer(setStateOnHurt, implosionDelay);
					}
				}

				var implosion = Object.Instantiate(implosionBuildUpPrefab, implosionSpot, Quaternion.identity);
				implosion.GetComponent<TeamFilter>().teamIndex = damageReport.victimTeamIndex;
				NetworkServer.Spawn(implosion);
			}
		}

		public static void SetVoidFrozenServer(SetStateOnHurt setStateOnHurt, float duration)
        {
			if (!NetworkServer.active) return;
			if (!setStateOnHurt.canBeFrozen) return;
			if (setStateOnHurt.hasEffectiveAuthority)
			{
				SetVoidFrozenLocal(setStateOnHurt, duration);
			}
			else
			{
				new SyncVoidFrozen(setStateOnHurt.GetComponent<NetworkIdentity>().netId, duration).Send(NetworkDestination.Clients);
			}
		}

		public static void SetVoidFrozenLocal(SetStateOnHurt setStateOnHurt, float duration)
		{
			if (setStateOnHurt.targetStateMachine)
			{
				var frozenState = new VoidFrozenState();
				frozenState.freezeDuration = duration;
				setStateOnHurt.targetStateMachine.SetInterruptState(frozenState, EntityStates.InterruptPriority.Frozen);
			}
			var array = setStateOnHurt.idleStateMachine;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetNextState(new EntityStates.Idle());
			}
		}

		public class VoidFrozenState : EntityStates.BaseState
		{
			private float duration;
			private Animator modelAnimator;
			private TemporaryOverlay temporaryOverlay;
			public float freezeDuration = 0.35f;
			public static GameObject frozenEffectPrefab;

			public override void OnEnter()
			{
				base.OnEnter();
				if (sfxLocator && sfxLocator.barkSound != "")
				{
					Util.PlaySound(sfxLocator.barkSound, gameObject);
				}
				var modelTransform = GetModelTransform();
				if (modelTransform)
				{
					var characterModel = modelTransform.GetComponent<CharacterModel>();
					if (characterModel)
					{
						temporaryOverlay = gameObject.AddComponent<TemporaryOverlay>();
						temporaryOverlay.duration = freezeDuration;
						temporaryOverlay.originalMaterial = voidFrozenMaterial;
						temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 0f, 0.5f, 1f);
						temporaryOverlay.animateShaderAlpha = true;
						temporaryOverlay.AddToCharacerModel(characterModel);
					}
				}
				modelAnimator = GetModelAnimator();
				if (modelAnimator)
				{
					modelAnimator.enabled = false;
					duration = freezeDuration;
					if (frozenEffectPrefab)
						EffectManager.SpawnEffect(frozenEffectPrefab, new EffectData
						{
							origin = characterBody.corePosition,
							scale = characterBody ? characterBody.radius : 1f
						}, false);
				}
				if (rigidbody && !rigidbody.isKinematic)
				{
					rigidbody.velocity = Vector3.zero;
					if (rigidbodyMotor)
					{
						rigidbodyMotor.moveVector = Vector3.zero;
					}
				}
				if (characterDirection)
				{
					characterDirection.moveVector = characterDirection.forward;
				}
				if (NetworkServer.active && characterBody && !characterBody.HasBuff(RisingTidesContent.Buffs.RisingTides_AffectedByVoidFreeze))
                {
					characterBody.AddBuff(RisingTidesContent.Buffs.RisingTides_AffectedByVoidFreeze);
                }
			}

			public override void FixedUpdate()
			{
				base.FixedUpdate();
				if (isAuthority && fixedAge >= duration)
				{
					outer.SetNextStateToMain();
				}
			}

			public override void OnExit()
			{
				if (modelAnimator)
				{
					modelAnimator.enabled = true;
				}
				if (temporaryOverlay)
				{
					Destroy(temporaryOverlay);
				}
				if (frozenEffectPrefab)
					EffectManager.SpawnEffect(frozenEffectPrefab, new EffectData
					{
						origin = characterBody.corePosition,
						scale = characterBody ? characterBody.radius : 1f
					}, false);
				if (NetworkServer.active && characterBody && characterBody.HasBuff(RisingTidesContent.Buffs.RisingTides_AffectedByVoidFreeze))
				{
					characterBody.RemoveBuff(RisingTidesContent.Buffs.RisingTides_AffectedByVoidFreeze);
				}
				base.OnExit();
			}

			public override EntityStates.InterruptPriority GetMinimumInterruptPriority()
			{
				return EntityStates.InterruptPriority.Frozen;
			}
		}

		public class SyncVoidFrozen : INetMessage
		{
			NetworkInstanceId objID;
			float duration;

			public SyncVoidFrozen()
			{
			}

			public SyncVoidFrozen(NetworkInstanceId objID, float duration)
			{
				this.objID = objID;
				this.duration = duration;
			}

			public void Deserialize(NetworkReader reader)
			{
				objID = reader.ReadNetworkId();
				duration = reader.ReadSingle();
			}

			public void OnReceived()
			{
				if (NetworkServer.active) return;
				var obj = Util.FindNetworkObject(objID);
				if (obj)
				{
					var setStateOnHurt = obj.GetComponent<SetStateOnHurt>();
					if (setStateOnHurt)
					{
						SetVoidFrozenLocal(setStateOnHurt, duration);
					}
				}
			}

			public void Serialize(NetworkWriter writer)
			{
				writer.Write(objID);
				writer.Write(duration);
			}
		}

		public class RisingTidesAffixBlackHoleImplosionController : MonoBehaviour
        {
			public float age = 0f;
			public float fixedAge = 0f;
			public float delay = 0f;
			public Transform growingSphereTransform;
			public Transform innerSphereTransform;
			public List<ParticleSystem> particleSystems = new List<ParticleSystem>();
			public TeamFilter teamFilter;

			public void Awake()
            {
				delay = implosionDelay;
				teamFilter = GetComponent<TeamFilter>();
			}

			public void Start()
            {
				Util.PlaySound("RisingTides_Play_onyx_implosion_buildup", gameObject);
            }

			public void Update()
            {
				age += Time.deltaTime;
				var t = Mathf.Clamp01(age / delay);

				if (growingSphereTransform)
					growingSphereTransform.localScale = Vector3.one * t;
				if (innerSphereTransform)
					innerSphereTransform.localScale = Vector3.one * 2f * (1f - t);

				for (var i = 0; i < particleSystems.Count; i++)
                {
					var particleSystem = particleSystems[i];
					var mainModule = particleSystem.main;
					mainModule.simulationSpeed = 1f - t;
                }
            }

			public void FixedUpdate()
            {
				fixedAge += Time.fixedDeltaTime;
				if (fixedAge >= delay)
                {
					if (NetworkServer.active)
                    {
						var implosionPoint = transform.position;

						var sphereSearch = new SphereSearch();
						sphereSearch.origin = implosionPoint;
						sphereSearch.radius = implosionRadius;
						sphereSearch.queryTriggerInteraction = QueryTriggerInteraction.Ignore;
						sphereSearch.mask = LayerIndex.entityPrecise.mask;
						
						// convert all allies
						sphereSearch.RefreshCandidates();
						sphereSearch.FilterCandidatesByDistinctHurtBoxEntities();
						var teamMask = new TeamMask();
						teamMask.AddTeam(teamFilter.teamIndex);
						sphereSearch.FilterCandidatesByHurtBoxTeam(teamMask);
						var hurtBoxes = sphereSearch.GetHurtBoxes();
						foreach (var hurtBox in hurtBoxes)
						{
							if (hurtBox.healthComponent && hurtBox.healthComponent.body && hurtBox.healthComponent.body.equipmentSlot)
							{
								var inventory = hurtBox.healthComponent.body.inventory;
								if (inventory && inventory.currentEquipmentIndex != RisingTidesContent.Equipment.RisingTides_AffixBlackHole.equipmentIndex)
                                {
									inventory.SetEquipmentIndex(RisingTidesContent.Equipment.RisingTides_AffixBlackHole.equipmentIndex);
                                }
							}
						}

						// knock back all enemies
						var implosionRadiusSquared = sphereSearch.radius * sphereSearch.radius;
						sphereSearch.RefreshCandidates();
						sphereSearch.FilterCandidatesByDistinctHurtBoxEntities();
						teamMask = TeamMask.GetEnemyTeams(teamFilter.teamIndex);
						sphereSearch.FilterCandidatesByHurtBoxTeam(teamMask);
						hurtBoxes = sphereSearch.GetHurtBoxes();
						foreach (var hurtBox in hurtBoxes)
						{
							if (hurtBox.healthComponent)
							{
								var vector = hurtBox.transform.position - implosionPoint;
								hurtBox.healthComponent.TakeDamageForce((vector.normalized * 4000f + Vector3.up * 1000f) * (1f - vector.sqrMagnitude / implosionRadiusSquared));
							}
						}
					}

					EffectManager.SpawnEffect(implosionEffect, new EffectData
					{
						origin = transform.position,
						scale = transform.localScale.x
					}, false);
					Util.PlaySound("RisingTides_Play_onyx_implosion", gameObject);
					Destroy(gameObject);
                }
            }
        }
	}
}
