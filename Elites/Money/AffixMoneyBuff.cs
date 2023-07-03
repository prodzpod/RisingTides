using Mono.Cecil.Cil;
using MonoMod.Cil;
using MysticsRisky2Utils;
using MysticsRisky2Utils.BaseAssetTypes;
using R2API;
using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace RisingTides.Buffs
{
    public class AffixMoney : BaseBuff
	{
		public static ConfigOptions.ConfigurableValue<float> auraRadius = ConfigOptions.ConfigurableValue.CreateFloat(
			RisingTidesPlugin.PluginGUID, RisingTidesPlugin.PluginName, RisingTidesPlugin.config,
			"Elites: Magnetic", "Aura Radius",
			13f,
			description: "How far should the money drain aura reach? (in metres)",
			useDefaultValueConfigEntry: RisingTidesPlugin.ignoreBalanceChanges.bepinexConfigEntry
		);
		public static ConfigOptions.ConfigurableValue<float> goldDropReduction = ConfigOptions.ConfigurableValue.CreateFloat(
			RisingTidesPlugin.PluginGUID, RisingTidesPlugin.PluginName, RisingTidesPlugin.config,
			"Elites: Magnetic", "Gold Drop Reduction",
			50f, 0f, 100f,
			description: "How much less gold should this elite drop? (in %)",
			useDefaultValueConfigEntry: RisingTidesPlugin.ignoreBalanceChanges.bepinexConfigEntry
		);
		public static ConfigOptions.ConfigurableValue<float> downwardForce = ConfigOptions.ConfigurableValue.CreateFloat(
			RisingTidesPlugin.PluginGUID, RisingTidesPlugin.PluginName, RisingTidesPlugin.config,
			"Elites: Magnetic", "Downward Force",
			1500f, 0f, 1000000f,
			description: "How strong should the on-hit force be?",
			useDefaultValueConfigEntry: RisingTidesPlugin.ignoreBalanceChanges.bepinexConfigEntry
		);

		public static GameObject gravityVFX;
		public static GameObject moneyExplosionVFX;
		public static DamageAPI.ModdedDamageType magneticDamageType;

		public override void OnPluginAwake()
        {
            base.OnPluginAwake();
			NetworkingAPI.RegisterMessageType<RisingTidesAffixMoneyAuraComponent.SyncBuffed>();
			magneticDamageType = DamageAPI.ReserveDamageType();
			RisingTidesAffixMoneyBehaviour.auraPrefab = Utils.CreateBlankPrefab("RisingTidesAffixMoneyAura", true);
		}

		public override void OnLoad()
		{
			base.OnLoad();
			buffDef.name = "RisingTides_AffixMoney";
			buffDef.iconSprite = RisingTidesPlugin.AssetBundle.LoadAsset<Sprite>("Assets/Mods/RisingTides/Elites/Money/texAffixMoneyBuffIcon.png");

			On.RoR2.CharacterBody.OnBuffFirstStackGained += CharacterBody_OnBuffFirstStackGained;
			On.RoR2.CharacterBody.OnBuffFinalStackLost += CharacterBody_OnBuffFinalStackLost;
            IL.RoR2.DeathRewards.OnKilledServer += DeathRewards_OnKilledServer;
			
			Utils.CopyChildren(RisingTidesPlugin.AssetBundle.LoadAsset<GameObject>("Assets/Mods/RisingTides/Elites/Money/AffixMoneyAura.prefab"), RisingTidesAffixMoneyBehaviour.auraPrefab);
			var slowDownProjectiles = RisingTidesAffixMoneyBehaviour.auraPrefab.transform.Find("ProjectileStopper").gameObject.AddComponent<RoR2.Projectile.SlowDownProjectiles>();
			slowDownProjectiles.slowDownCoefficient = 0f;
			slowDownProjectiles.teamFilter = slowDownProjectiles.gameObject.AddComponent<TeamFilter>();
			slowDownProjectiles.teamFilter.defaultTeam = TeamIndex.Monster;
			var auraComponent = RisingTidesAffixMoneyBehaviour.auraPrefab.AddComponent<RisingTidesAffixMoneyAuraComponent>();
			auraComponent.innerSphereRenderer = auraComponent.transform.Find("SphereInner").GetComponent<Renderer>();
			auraComponent.outerSphereRenderer = auraComponent.transform.Find("SphereOuter").GetComponent<Renderer>();
			auraComponent.projectileStopper = auraComponent.transform.Find("ProjectileStopper").gameObject;
			auraComponent.innerMaterial = RisingTidesPlugin.AssetBundle.LoadAsset<Material>("Assets/Mods/RisingTides/Elites/Money/matAffixMoneyAuraInner.mat");
			auraComponent.outerMaterial = RisingTidesPlugin.AssetBundle.LoadAsset<Material>("Assets/Mods/RisingTides/Elites/Money/matAffixMoneyAuraOuter.mat");
			auraComponent.innerMaterialBuffed = RisingTidesPlugin.AssetBundle.LoadAsset<Material>("Assets/Mods/RisingTides/Elites/Money/matAffixMoneyAuraInnerBuffed.mat");
			auraComponent.outerMaterialBuffed = RisingTidesPlugin.AssetBundle.LoadAsset<Material>("Assets/Mods/RisingTides/Elites/Money/matAffixMoneyAuraOuterBuffed.mat");

			GenericGameEvents.OnHitEnemy += GenericGameEvents_OnHitEnemy;

			{
				moneyExplosionVFX = RisingTidesPlugin.AssetBundle.LoadAsset<GameObject>("Assets/Mods/RisingTides/Elites/Money/AffixMoneyExplosionVFX.prefab");
				moneyExplosionVFX.AddComponent<DestroyOnTimer>().duration = 0.6f;
				var effectComponent = moneyExplosionVFX.AddComponent<EffectComponent>();
				effectComponent.applyScale = true;
				effectComponent.soundName = "Play_gravekeeper_attack2_shoot_singleChain";
				var vfxAttributes = moneyExplosionVFX.AddComponent<VFXAttributes>();
				vfxAttributes.vfxIntensity = VFXAttributes.VFXIntensity.Low;
				vfxAttributes.vfxPriority = VFXAttributes.VFXPriority.Medium;
				RisingTidesContent.Resources.effectPrefabs.Add(moneyExplosionVFX);
			}
            {
				gravityVFX = RisingTidesPlugin.AssetBundle.LoadAsset<GameObject>("Assets/Mods/RisingTides/Elites/Money/AffixMoneyGravityVFX.prefab");
				gravityVFX.AddComponent<DestroyOnTimer>().duration = 0.6f;
				var effectComponent = gravityVFX.AddComponent<EffectComponent>();
				effectComponent.applyScale = true;
				effectComponent.soundName = "Play_bellBody_attackLand";
				var vfxAttributes = gravityVFX.AddComponent<VFXAttributes>();
				vfxAttributes.vfxIntensity = VFXAttributes.VFXIntensity.Low;
				vfxAttributes.vfxPriority = VFXAttributes.VFXPriority.Always;
				RisingTidesContent.Resources.effectPrefabs.Add(gravityVFX);
			}

			if (RisingTidesPlugin.mysticsItemsCompatibility)
			{
				RoR2Application.onLoad += () =>
				{
					MysticsItems.OtherModCompat.ElitePotion_AddSpreadEffect(buffDef, vfx: moneyExplosionVFX, procCoefficient: 1f, moddedDamageType: magneticDamageType);
				};
			}
		}

        private void GenericGameEvents_OnHitEnemy(DamageInfo damageInfo, MysticsRisky2UtilsPlugin.GenericCharacterInfo attackerInfo, MysticsRisky2UtilsPlugin.GenericCharacterInfo victimInfo)
        {
			if (!damageInfo.rejected && damageInfo.procCoefficient > 0f && attackerInfo.body && (attackerInfo.body.HasBuff(buffDef) || damageInfo.HasModdedDamageType(magneticDamageType)) && victimInfo.healthComponent)
			{
				if (victimInfo.body && ((!victimInfo.body.characterMotor && victimInfo.body.rigidbody) || (victimInfo.body.characterMotor && !victimInfo.body.characterMotor.isGrounded)))
				{
					var effectData = new EffectData
					{
						origin = victimInfo.body.corePosition,
						scale = victimInfo.body.radius
					};
					effectData.SetNetworkedObjectReference(victimInfo.gameObject);
					EffectManager.SpawnEffect(gravityVFX, effectData, true);

					victimInfo.healthComponent.TakeDamageForce(Vector3.down * downwardForce * damageInfo.procCoefficient);
				}
			}
        }

		public override void AfterContentPackLoaded()
		{
			base.AfterContentPackLoaded();
			buffDef.eliteDef = RisingTidesContent.Elites.RisingTides_Money;
		}

		private void CharacterBody_OnBuffFirstStackGained(On.RoR2.CharacterBody.orig_OnBuffFirstStackGained orig, CharacterBody self, BuffDef buffDef)
		{
			orig(self, buffDef);
			if (buffDef == this.buffDef)
			{
				var component = self.GetComponent<RisingTidesAffixMoneyBehaviour>();
				if (!component)
				{
					component = self.gameObject.AddComponent<RisingTidesAffixMoneyBehaviour>();
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
				var component = self.GetComponent<RisingTidesAffixMoneyBehaviour>();
				if (component && component.enabled)
				{
					component.enabled = false;
				}
			}
		}

		public class RisingTidesAffixMoneyAuraComponent : MonoBehaviour
        {
			public Material outerMaterial;
			public Material innerMaterial;
			public Material outerMaterialBuffed;
			public Material innerMaterialBuffed;
			public Renderer innerSphereRenderer;
			public Renderer outerSphereRenderer;
			public GameObject projectileStopper;

			public float buffTimer = 0f;
			private bool _buffed = false;
			public bool buffed
			{
				get { return _buffed; }
				set
				{
					if (_buffed == value) return;
					_buffed = value;

					if (innerSphereRenderer)
						innerSphereRenderer.material = value ? innerMaterialBuffed : innerMaterial;

					if (outerSphereRenderer)
						outerSphereRenderer.material = value ? outerMaterialBuffed : outerMaterial;

					projectileStopper.SetActive(value);

					if (value)
						Util.PlaySound("Play_lunar_exploder_death", gameObject);

					if (NetworkServer.active)
						new SyncBuffed(GetComponent<NetworkIdentity>().netId, false).Send(NetworkDestination.Clients);
				}
			}

			public void FixedUpdate()
            {
				if (NetworkServer.active)
				{
					if (buffTimer > 0f)
					{
						buffTimer -= Time.fixedDeltaTime;
						if (buffTimer <= 0f)
						{
							buffed = false;
						}
					}
				}
			}

			public class SyncBuffed : INetMessage
			{
				NetworkInstanceId objID;
				bool buffed;
				
				public SyncBuffed()
				{
				}

				public SyncBuffed(NetworkInstanceId objID, bool buffed)
				{
					this.objID = objID;
					this.buffed = buffed;
				}

				public void Deserialize(NetworkReader reader)
				{
					objID = reader.ReadNetworkId();
					buffed = reader.ReadBoolean();
				}

				public void OnReceived()
				{
					if (NetworkServer.active) return;
					var obj = Util.FindNetworkObject(objID);
					if (obj)
					{
						var component = obj.GetComponent<RisingTidesAffixMoneyAuraComponent>();
						if (component)
						{
							component.buffed = buffed;
						}
					}
				}

				public void Serialize(NetworkWriter writer)
				{
					writer.Write(objID);
					writer.Write(buffed);
				}
			}
		}

		public class RisingTidesAffixMoneyBehaviour : MonoBehaviour
		{
			public static GameObject auraPrefab;

			public RisingTidesAffixMoneyAuraComponent aura;
			public CharacterBody body;
			public SphereSearch sphereSearch;
			public float auraRadius = 0f;
			
			public float moneyDrainTimer = 0f;
			public float moneyDrainInterval = 0.5f;

			public uint baseStealCount = 1;
			public uint stealCount = 0;
			
			public void Awake()
			{
				body = GetComponent<CharacterBody>();

				auraRadius += AffixMoney.auraRadius + body.radius;
				
				sphereSearch = new SphereSearch();
				sphereSearch.radius = auraRadius;
				sphereSearch.queryTriggerInteraction = QueryTriggerInteraction.Ignore;
				sphereSearch.mask = LayerIndex.entityPrecise.mask;
			}

			public void Start()
            {
				var auraObject = Instantiate(auraPrefab, transform);
				auraObject.transform.localScale = Vector3.one * auraRadius;
				aura = auraObject.GetComponent<RisingTidesAffixMoneyAuraComponent>();
				aura.projectileStopper.GetComponent<TeamFilter>().teamIndex = body.teamComponent.teamIndex;
			}

			public void FixedUpdate()
			{
				if (!body.healthComponent || !body.healthComponent.alive) return;

				if (NetworkServer.active)
				{
					moneyDrainTimer -= Time.fixedDeltaTime;
					if (moneyDrainTimer <= 0f)
					{
						moneyDrainTimer += moneyDrainInterval;

						stealCount = (uint)Run.instance.GetDifficultyScaledCost((int)baseStealCount);

						var stolenThisCycle = 0u;

						sphereSearch.origin = body.corePosition;
						sphereSearch.RefreshCandidates();
						sphereSearch.FilterCandidatesByDistinctHurtBoxEntities();
						sphereSearch.FilterCandidatesByHurtBoxTeam(TeamMask.GetEnemyTeams(body.teamComponent.teamIndex));
						var hurtBoxes = sphereSearch.GetHurtBoxes();
						foreach (var hurtBox in hurtBoxes)
						{
							if (hurtBox.healthComponent && hurtBox.healthComponent.body && hurtBox.healthComponent.body != body && hurtBox.healthComponent.body.master)
							{
								var master = hurtBox.healthComponent.body.master;
								if (master && master.money > 0)
								{
									var stealCountForTarget = System.Math.Min(master.money, stealCount);
									master.money -= stealCountForTarget;
									stolenThisCycle += stealCountForTarget;
								}
							}
						}

						if (body.master) body.master.money += stolenThisCycle;
					}
				}
			}

			public void OnEnable()
            {
				if (aura)
					aura.gameObject.SetActive(true);
            }

			public void OnDisable()
            {
				if (aura)
					aura.gameObject.SetActive(false);
            }
		}

		private void DeathRewards_OnKilledServer(ILContext il)
		{
			ILCursor c = new ILCursor(il);

			if (c.TryGotoNext(
				MoveType.After,
				x => x.MatchCallOrCallvirt(typeof(DeathRewards), "get_goldReward")
			))
			{
				c.Emit(OpCodes.Ldarg, 0);
				c.EmitDelegate<System.Func<uint, DeathRewards, uint>>((goldReward, deathRewards) =>
				{
					if (deathRewards.characterBody.HasBuff(buffDef))
                    {
						goldReward = (uint)(goldReward * (1f - goldDropReduction / 100f));
                    }
					return goldReward;
				});
			}
		}
	}
}
