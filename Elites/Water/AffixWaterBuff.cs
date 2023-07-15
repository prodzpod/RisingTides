using MysticsRisky2Utils;
using MysticsRisky2Utils.BaseAssetTypes;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using R2API;

namespace RisingTides.Buffs
{
    public class AffixWater : BaseBuff
	{
		public static ConfigOptions.ConfigurableValue<float> forcedBreakOutDelay = ConfigOptions.ConfigurableValue.CreateFloat(
			RisingTidesPlugin.PluginGUID, RisingTidesPlugin.PluginName, RisingTidesPlugin.config,
			"Elites: Aquamarine", "Bubble Duration",
			5f,
			description: "How long should this elite's on-hit bubble trap last? (in seconds)",
			useDefaultValueConfigEntry: RisingTidesPlugin.ignoreBalanceChanges.bepinexConfigEntry
		);
		public static ConfigOptions.ConfigurableValue<float> cooldownBuffDuration = ConfigOptions.ConfigurableValue.CreateFloat(
			RisingTidesPlugin.PluginGUID, RisingTidesPlugin.PluginName, RisingTidesPlugin.config,
			"Elites: Aquamarine", "Bubble Cooldown",
			10f,
			description: "Targets cannot be trapped again until this time passes. How long should this immunity last? (in seconds)",
			useDefaultValueConfigEntry: RisingTidesPlugin.ignoreBalanceChanges.bepinexConfigEntry
		);
		public static ConfigOptions.ConfigurableValue<int> breakOutAttemptsRequired = ConfigOptions.ConfigurableValue.CreateInt(
			RisingTidesPlugin.PluginGUID, RisingTidesPlugin.PluginName, RisingTidesPlugin.config,
			"Elites: Aquamarine", "Bubble Break Outs Required",
			5,
			description: "How many times should you press Interact to leave the bubble early?",
			useDefaultValueConfigEntry: RisingTidesPlugin.ignoreBalanceChanges.bepinexConfigEntry
		);

		public static GameObject bubbleVehiclePrefab;
		public static GameObject bubbleBurstVFX;
		
		public override void OnPluginAwake()
        {
            base.OnPluginAwake();
			bubbleVehiclePrefab = Utils.CreateBlankPrefab("RisingTidesAffixWaterBubbleVehicle", true);
			bubbleVehiclePrefab.GetComponent<NetworkIdentity>().localPlayerAuthority = true;
		}

        public override void OnLoad()
		{
			base.OnLoad();
			buffDef.name = "RisingTides_AffixWater";
			buffDef.iconSprite = RisingTidesPlugin.AssetBundle.LoadAsset<Sprite>("Assets/Mods/RisingTides/Elites/Water/texAffixWaterBuffIcon.png");

			On.RoR2.CharacterBody.OnBuffFirstStackGained += CharacterBody_OnBuffFirstStackGained;
			On.RoR2.CharacterBody.OnBuffFinalStackLost += CharacterBody_OnBuffFinalStackLost;

			Utils.CopyChildren(RisingTidesPlugin.AssetBundle.LoadAsset<GameObject>("Assets/Mods/RisingTides/Elites/Water/WaterBubbleTrap.prefab"), bubbleVehiclePrefab);
			bubbleVehiclePrefab.AddComponent<NetworkTransform>();
			bubbleVehiclePrefab.AddComponent<RisingTidesWaterBubbleTrap>();
			var vehicleSeat = bubbleVehiclePrefab.AddComponent<VehicleSeat>();
			vehicleSeat.passengerState = new EntityStates.SerializableEntityStateType(typeof(EntityStates.GenericCharacterVehicleSeated));
			vehicleSeat.seatPosition = bubbleVehiclePrefab.transform.Find("Scaler/SeatPosition");
			vehicleSeat.exitPosition = bubbleVehiclePrefab.transform.Find("Scaler/SeatPosition");
			vehicleSeat.ejectOnCollision = false;
			vehicleSeat.hidePassenger = true;
			vehicleSeat.exitVelocityFraction = 2f;
			vehicleSeat.enterVehicleContextString = "";
			vehicleSeat.exitVehicleContextString = "RISINGTIDES_WATERBUBBLE_VEHICLE_EXIT_CONTEXT";
			vehicleSeat.disablePassengerMotor = true;
			vehicleSeat.isEquipmentActivationAllowed = false;
			var bubbleScale = bubbleVehiclePrefab.transform.Find("Scaler").gameObject.AddComponent<ObjectScaleCurve>();
			bubbleScale.useOverallCurveOnly = true;
			bubbleScale.overallCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
			bubbleScale.timeMax = 0.12f;

			GenericGameEvents.OnHitEnemy += GenericGameEvents_OnHitEnemy;

			bubbleBurstVFX = RisingTidesPlugin.AssetBundle.LoadAsset<GameObject>("Assets/Mods/RisingTides/Elites/Water/BubbleBurstVFX.prefab");
			var effectComponent = bubbleBurstVFX.AddComponent<EffectComponent>();
			effectComponent.applyScale = true;
			effectComponent.soundName = "Play_item_proc_squidTurret_shotExplode";
			var vfxAttributes = bubbleBurstVFX.AddComponent<VFXAttributes>();
			vfxAttributes.vfxIntensity = VFXAttributes.VFXIntensity.Medium;
			vfxAttributes.vfxPriority = VFXAttributes.VFXPriority.Medium;
			bubbleBurstVFX.AddComponent<DestroyOnTimer>().duration = 0.5f;
			RisingTidesContent.Resources.effectPrefabs.Add(bubbleBurstVFX);

			if (RisingTidesPlugin.mysticsItemsCompatibility)
			{
				RoR2Application.onLoad += () =>
				{
					MysticsItems.OtherModCompat.ElitePotion_AddSpreadEffect(buffDef, vfx: bubbleBurstVFX, debuff: RoR2Content.Buffs.Slow80, damageType: default);
				};
			}
		}

        private void GenericGameEvents_OnHitEnemy(DamageInfo damageInfo, MysticsRisky2UtilsPlugin.GenericCharacterInfo attackerInfo, MysticsRisky2UtilsPlugin.GenericCharacterInfo victimInfo)
        {
            if (!damageInfo.rejected && damageInfo.procCoefficient > 0f && attackerInfo.body && attackerInfo.body.HasBuff(buffDef) && victimInfo.body && !victimInfo.body.HasBuff(RisingTidesContent.Buffs.RisingTides_WaterBubbleTrapCooldown) && !victimInfo.body.currentVehicle)
            {
				var vehicle = Object.Instantiate(bubbleVehiclePrefab, victimInfo.body.corePosition, Quaternion.identity);
				vehicle.transform.localScale = Vector3.one * (1.5f + victimInfo.body.radius);
				var vehicleSeat = vehicle.GetComponent<VehicleSeat>();
				vehicleSeat.AssignPassenger(victimInfo.gameObject);
				var trapComponent = vehicle.GetComponent<RisingTidesWaterBubbleTrap>();
				trapComponent.forcedBreakOutDelay *= damageInfo.procCoefficient;
				trapComponent.cooldownBuffDuration *= damageInfo.procCoefficient;
				NetworkServer.Spawn(vehicle);
			}
        }

        public class RisingTidesWaterBubbleTrap : MonoBehaviour
        {
			public VehicleSeat vehicleSeat;
			public Rigidbody rigidbody;
			public int breakOutAttempts = 0;
			public float forcedBreakOutDelay;
			public float cooldownBuffDuration;
			public float initialUpwardsSpeed = 1.2f;

			public float age;
			public float fixedAge;
			public Wave shakeWaveX;
			public Wave shakeWaveZ;
			public float shakeTimer = 0f;
			public float shakeDuration = 0.16f;

			public void Awake()
			{
				forcedBreakOutDelay = AffixWater.forcedBreakOutDelay;
				cooldownBuffDuration = AffixWater.cooldownBuffDuration;

				vehicleSeat = GetComponent<VehicleSeat>();
                vehicleSeat.onPassengerEnter += VehicleSeat_onPassengerEnter;
				vehicleSeat.handleVehicleExitRequestServer.AddCallback(HandleVehicleExitRequest);
                vehicleSeat.onPassengerExit += VehicleSeat_onPassengerExit;

				rigidbody = GetComponent<Rigidbody>();
				rigidbody.velocity = Vector3.up * initialUpwardsSpeed;

				shakeWaveX = new Wave
				{
					amplitude = RoR2Application.rng.RangeFloat(0.03f, 0.1f),
					frequency = RoR2Application.rng.RangeFloat(10f, 20f)
				};
				shakeWaveZ = new Wave
				{
					amplitude = RoR2Application.rng.RangeFloat(0.03f, 0.1f),
					frequency = RoR2Application.rng.RangeFloat(10f, 20f)
				};
			}

            private void VehicleSeat_onPassengerEnter(GameObject passengerObject)
            {
				if (passengerObject)
				{
					var body = passengerObject.GetComponent<CharacterBody>();
					if (body && body.hasEffectiveAuthority && body.skillLocator)
					{
						void ManageSkillSlot(GenericSkill skillSlot)
						{
							if (!skillSlot.stateMachine.IsInMainState())
								skillSlot.stateMachine.SetNextStateToMain();

							if (body.skillLocator.primary) ManageSkillSlot(body.skillLocator.primary);
							if (body.skillLocator.secondary) ManageSkillSlot(body.skillLocator.secondary);
							if (body.skillLocator.utility) ManageSkillSlot(body.skillLocator.utility);
							if (body.skillLocator.special) ManageSkillSlot(body.skillLocator.special);
						}
					}
				}
			}

            private void VehicleSeat_onPassengerExit(GameObject passengerObject)
            {
				if (NetworkServer.active && passengerObject)
                {
					var body = passengerObject.GetComponent<CharacterBody>();
					if (body)
                    {
						body.AddTimedBuff(RisingTidesContent.Buffs.RisingTides_WaterBubbleTrapCooldown, cooldownBuffDuration);
					}
                }
				Destroy(gameObject);
			}

            public void Start()
            {
				Util.PlaySound("Play_item_use_tonic", gameObject);
            }

			public void Update()
			{
				age += Time.deltaTime;
				if (shakeTimer > 0f)
				{
					shakeTimer -= Time.deltaTime;

					var newPosition = rigidbody.position;
					newPosition += Vector3.left * shakeWaveX.Evaluate(age);
					newPosition += Vector3.forward * shakeWaveZ.Evaluate(age);
					rigidbody.MovePosition(newPosition);
				}
			}

			public void FixedUpdate()
            {
				fixedAge += Time.fixedDeltaTime;
				if (fixedAge >= forcedBreakOutDelay && NetworkServer.active)
                {
					Destroy(gameObject);
				}
            }

			public void HandleVehicleExitRequest(GameObject gameObject, ref bool? result)
			{
				breakOutAttempts++;
				shakeTimer = shakeDuration;
				if (breakOutAttempts < breakOutAttemptsRequired)
					result = true;
			}

			public void OnDestroy()
            {
				if (NetworkServer.active)
                {
					EffectManager.SpawnEffect(bubbleBurstVFX, new EffectData
					{
						origin = transform.position,
						scale = transform.localScale.x
					}, true);
                }
            }
		}

		public override void AfterContentPackLoaded()
		{
			base.AfterContentPackLoaded();
			buffDef.eliteDef = RisingTidesContent.Elites.RisingTides_Water;
		}

		private void CharacterBody_OnBuffFirstStackGained(On.RoR2.CharacterBody.orig_OnBuffFirstStackGained orig, CharacterBody self, BuffDef buffDef)
		{
			orig(self, buffDef);
			if (NetworkServer.active && buffDef == this.buffDef)
			{
				var component = self.GetComponent<RisingTidesAffixWaterBehaviour>();
				if (!component)
				{
					component = self.gameObject.AddComponent<RisingTidesAffixWaterBehaviour>();
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
			if (NetworkServer.active && buffDef == this.buffDef)
			{
				var component = self.GetComponent<RisingTidesAffixWaterBehaviour>();
				if (component && component.enabled)
				{
					component.enabled = false;
				}
			}
		}

		public class RisingTidesAffixWaterBehaviour : MonoBehaviour
		{
			public CharacterBody body;
			public SkillLocator skillLocator;
			public float vulnerableTimer = 0f;
			public float vulnerableDuration = 0.2f;
			public float failsafeVulnerabilityForceTimer = 0f;
			public float failsafeVulnerabilityForceThreshold = 30f;

			public void Awake()
			{
				body = GetComponent<CharacterBody>();
				skillLocator = GetComponent<SkillLocator>();
			}

			public void FixedUpdate()
			{
				failsafeVulnerabilityForceTimer += Time.fixedDeltaTime;
				var failsafeVulnerabilityForced = false;

				var usingAnySkill = false;
				if (skillLocator)
				{
					if (skillLocator.primary && !skillLocator.primary.stateMachine.IsInMainState()) usingAnySkill = true;
					else if (skillLocator.secondary && !skillLocator.secondary.stateMachine.IsInMainState()) usingAnySkill = true;
					else if (skillLocator.utility && !skillLocator.utility.stateMachine.IsInMainState()) usingAnySkill = true;
					else if (skillLocator.special && !skillLocator.special.stateMachine.IsInMainState()) usingAnySkill = true;
					else if (!skillLocator.primary && !skillLocator.secondary && !skillLocator.utility && !skillLocator.special)
                    {
						failsafeVulnerabilityForced = true;
					}
				}

				if (usingAnySkill) failsafeVulnerabilityForceTimer = 0f;
				if (failsafeVulnerabilityForceTimer >= failsafeVulnerabilityForceThreshold && !body.isPlayerControlled) failsafeVulnerabilityForced = true;

				if (vulnerableTimer <= 0f)
				{
					if (!usingAnySkill && !failsafeVulnerabilityForced && !body.HasBuff(RisingTidesContent.Buffs.RisingTides_WaterInvincibility))
					{
						body.AddBuff(RisingTidesContent.Buffs.RisingTides_WaterInvincibility);
					}
					else if ((usingAnySkill || failsafeVulnerabilityForced) && body.HasBuff(RisingTidesContent.Buffs.RisingTides_WaterInvincibility))
					{
						body.RemoveBuff(RisingTidesContent.Buffs.RisingTides_WaterInvincibility);
						vulnerableTimer = vulnerableDuration;
					}
                }
                else
                {
					if (!usingAnySkill && !failsafeVulnerabilityForced) vulnerableTimer -= Time.fixedDeltaTime;
				}
			}

			public void OnDisable()
			{
				if (body && body.HasBuff(RisingTidesContent.Buffs.RisingTides_WaterInvincibility))
				{
					body.RemoveBuff(RisingTidesContent.Buffs.RisingTides_WaterInvincibility);
					vulnerableTimer = 0f;
				}
			}
		}
	}
}
