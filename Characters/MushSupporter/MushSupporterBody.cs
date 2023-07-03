using MysticsRisky2Utils;
using MysticsRisky2Utils.BaseAssetTypes;
using RoR2;
using RoR2.Projectile;
using RoR2.Skills;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace RisingTides.CharacterBodies
{
    public class MushSupporter : BaseCharacterBody
    {
        public static GameObject sporeProjectile;

        public override void OnPluginAwake()
        {
            base.OnPluginAwake();
            prefab = Utils.CreateBlankPrefab("RisingTides_MushSupporterBody", true);
            prefab.GetComponent<NetworkIdentity>().localPlayerAuthority = true;

            sporeProjectile = Utils.CreateBlankPrefab("RisingTides_MushSupporterSpore", true);
            sporeProjectile.GetComponent<NetworkIdentity>().localPlayerAuthority = true;
        }

        public override void OnLoad()
        {
            base.OnLoad();

            Utils.CopyChildren(RisingTidesPlugin.AssetBundle.LoadAsset<GameObject>("Assets/Mods/RisingTides/Characters/MushSupporter/MushSupporterBody.prefab"), prefab);
            bodyName = "RisingTides_MushSupporter";

            modelBaseTransform = prefab.transform.Find("ModelBase");
            modelTransform = prefab.transform.Find("ModelBase/mdlMushSupporter");
            meshObject = prefab.transform.Find("ModelBase/mdlMushSupporter/÷илиндр").gameObject;
            Prepare();

            var mat = meshObject.GetComponent<SkinnedMeshRenderer>().sharedMaterial;
            HopooShaderToMaterial.Standard.Emission(mat, 0.3f);
            mat.SetTexture("_EmTex", mat.GetTexture("_EmissionMap"));

            SetUpChildLocator(new ChildLocator.NameTransformPair[]
            {
                new ChildLocator.NameTransformPair
                {
                    name = "ROOT",
                    transform = modelTransform.Find("јрматура/ROOT")
                },
                new ChildLocator.NameTransformPair
                {
                    name = "Cap",
                    transform = modelTransform.Find("јрматура/ROOT/шл€па")
                },
                new ChildLocator.NameTransformPair
                {
                    name = "HealthBarOrigin",
                    transform = modelTransform.Find("јрматура/ROOT/шл€па/HealthBarOrigin")
                }
            });

            modelLocator.dontReleaseModelOnDeath = false;
            modelLocator.autoUpdateModelTransform = true;
            modelLocator.dontDetatchFromParent = false;
            modelLocator.noCorpse = false;

            // body
            var characterBody = prefab.GetComponent<CharacterBody>();
            characterBody.bodyFlags = CharacterBody.BodyFlags.HasBackstabImmunity;
            characterBody.baseMaxHealth = 400f;
            characterBody.baseRegen = 0f;
            characterBody.baseMaxShield = 0f;
            characterBody.baseMoveSpeed = 0f;
            characterBody.baseAcceleration = 0f;
            characterBody.baseJumpPower = 0f;
            characterBody.baseDamage = 12f;
            characterBody.baseAttackSpeed = 1f;
            characterBody.baseCrit = 0f;
            characterBody.baseArmor = 0f;
            characterBody.baseJumpCount = 0;
            // characterBody.aimOriginTransform = modelTransform.Find("TinkererDroneArmature/AimOrigin");
            characterBody.hullClassification = HullClassification.BeetleQueen;
            characterBody.portraitIcon = RisingTidesPlugin.AssetBundle.LoadAsset<Texture>("Assets/Mods/RisingTides/Characters/MushSupporter/texMushSupporterIcon.png");
            characterBody.bodyColor = new Color32(230, 98, 88, 255);
            characterBody.isChampion = false;
            characterBody.preferredInitialStateType = new EntityStates.SerializableEntityStateType(typeof(EntityStates.Uninitialized));
            AfterCharacterBodySetup();
            characterBody.subtitleNameToken = "";

            // death rewards
            var logUnlockableDef = ScriptableObject.CreateInstance<UnlockableDef>();
            logUnlockableDef.cachedName = "Logs.RisingTides_MushSupporterBody.0";
            logUnlockableDef.nameToken = "UNLOCKABLE_LOG_RISINGTIDES_MUSHSUPPORTER";
            logUnlockableDef.displayModelPrefab = prefab;
            logUnlockableDef.hidden = false;
            RisingTidesContent.Resources.unlockableDefs.Add(logUnlockableDef);

            var deathRewards = prefab.AddComponent<DeathRewards>();
            deathRewards.logUnlockableDef = logUnlockableDef;

            // hurtbox
            SetUpHurtBoxGroup(new HurtBoxSetUpInfo[]
            {
                new HurtBoxSetUpInfo
                {
                    transform = modelTransform.Find("јрматура/ROOT/HurtBox"),
                    isBullseye = true,
                    isMain = true
                },
                new HurtBoxSetUpInfo
                {
                    transform = modelTransform.Find("јрматура/ROOT/HurtBoxWeak1"),
                    isSniperTarget = true
                },
                new HurtBoxSetUpInfo
                {
                    transform = modelTransform.Find("јрматура/ROOT/HurtBoxWeak2"),
                    isSniperTarget = true
                },
                new HurtBoxSetUpInfo
                {
                    transform = modelTransform.Find("јрматура/ROOT/HurtBoxWeak3"),
                    isSniperTarget = true
                },
                new HurtBoxSetUpInfo
                {
                    transform = modelTransform.Find("јрматура/ROOT/HurtBoxWeak4"),
                    isSniperTarget = true
                },
                new HurtBoxSetUpInfo
                {
                    transform = modelTransform.Find("јрматура/ROOT/HurtBoxWeak5"),
                    isSniperTarget = true
                },
                new HurtBoxSetUpInfo
                {
                    transform = modelTransform.Find("јрматура/ROOT/шл€па/HurtBox")
                }
            });

            // sfx
            var sfxLocator = prefab.AddComponent<SfxLocator>();
            sfxLocator.barkSound = "Play_minimushroom_idle_VO";
            sfxLocator.deathSound = "Play_jellyfish_death";

            // state machines
            EntityStateMachine bodyStateMachine = SetUpEntityStateMachine("Body", typeof(SpawnState), typeof(EntityStates.GenericCharacterMain));
            EntityStateMachine weaponStateMachine = SetUpEntityStateMachine("Weapon", typeof(EntityStates.Idle), typeof(EntityStates.Idle));

            RisingTidesContent.Resources.entityStateTypes.Add(typeof(SpawnState));
            RisingTidesContent.Resources.entityStateTypes.Add(typeof(DeathState));
            RisingTidesContent.Resources.entityStateTypes.Add(typeof(FireSpores));
            RisingTidesContent.Resources.entityStateTypes.Add(typeof(BurrowStart));
            RisingTidesContent.Resources.entityStateTypes.Add(typeof(BurrowEnd));

            var deathEffect = RisingTidesPlugin.AssetBundle.LoadAsset<GameObject>("Assets/Mods/RisingTides/Characters/MushSupporter/MushSupporterDeathEffect.prefab");
            deathEffect.AddComponent<EffectComponent>();
            var deathEffectVFX = deathEffect.AddComponent<VFXAttributes>();
            deathEffectVFX.vfxIntensity = VFXAttributes.VFXIntensity.Medium;
            deathEffectVFX.vfxPriority = VFXAttributes.VFXPriority.Always;
            deathEffect.AddComponent<DestroyOnTimer>().duration = 1f;
            var lightIntensityCurve = deathEffect.transform.Find("Light").gameObject.AddComponent<LightIntensityCurve>();
            lightIntensityCurve.curve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
            lightIntensityCurve.timeMax = 0.2f;
            var shakeEmitter = deathEffect.AddComponent<ShakeEmitter>();
            shakeEmitter.wave = new Wave
            {
                amplitude = 1.1f,
                frequency = 7f
            };
            shakeEmitter.duration = 0.2f;
            shakeEmitter.radius = 24f;
            shakeEmitter.amplitudeTimeDecay = true;
            DeathState.enterEffectPrefab = deathEffect;
            RisingTidesContent.Resources.effectPrefabs.Add(deathEffect);

            var characterDeathBehavior = prefab.GetComponent<CharacterDeathBehavior>();
            characterDeathBehavior.deathStateMachine = bodyStateMachine;
            characterDeathBehavior.deathState = new EntityStates.SerializableEntityStateType(typeof(DeathState));
            characterDeathBehavior.idleStateMachine = new EntityStateMachine[] {
                weaponStateMachine
            };
            RisingTidesContent.Resources.entityStateTypes.Add(typeof(SpawnState));

            var setStateOnHurt = prefab.AddComponent<SetStateOnHurt>();
            setStateOnHurt.canBeFrozen = true;
            setStateOnHurt.canBeHitStunned = true;
            setStateOnHurt.canBeStunned = true;
            setStateOnHurt.hurtState = new EntityStates.SerializableEntityStateType(typeof(EntityStates.HurtState));
            setStateOnHurt.hitThreshold = 0.35f;
            setStateOnHurt.targetStateMachine = bodyStateMachine;
            setStateOnHurt.idleStateMachine = new EntityStateMachine[]
            {
                weaponStateMachine
            };

            // skills
            var skillFire = ScriptableObject.CreateInstance<SkillDef>();
            ((ScriptableObject)skillFire).name = "RisingTides_MushSupporterBodyFire";
            skillFire.skillName = "Fire";
            skillFire.activationStateMachineName = "Weapon";
            skillFire.activationState = new EntityStates.SerializableEntityStateType(typeof(FireSpores));
            skillFire.interruptPriority = EntityStates.InterruptPriority.Any;
            skillFire.baseRechargeInterval = 6f;
            skillFire.baseMaxStock = 1;
            skillFire.rechargeStock = 1;
            skillFire.requiredStock = 1;
            skillFire.stockToConsume = 1;
            skillFire.resetCooldownTimerOnUse = true;
            skillFire.fullRestockOnAssign = true;
            skillFire.dontAllowPastMaxStocks = false;
            skillFire.beginSkillCooldownOnSkillEnd = false;
            skillFire.cancelSprintingOnActivation = true;
            skillFire.forceSprintDuringState = false;
            skillFire.canceledFromSprinting = false;
            skillFire.isCombatSkill = true;
            skillFire.mustKeyPress = false;
            RisingTidesContent.Resources.skillDefs.Add(skillFire);

            var skillFamily = ScriptableObject.CreateInstance<SkillFamily>();
            ((ScriptableObject)skillFamily).name = "RisingTides_MushSupporterBodyPrimaryFamily";
            skillFamily.variants = new SkillFamily.Variant[]
            {
                new SkillFamily.Variant
                {
                    skillDef = skillFire
                }
            };
            skillFamily.defaultVariantIndex = 0;
            RisingTidesContent.Resources.skillFamilies.Add(skillFamily);

            var primarySkill = prefab.AddComponent<GenericSkill>();
            primarySkill._skillFamily = skillFamily;
            primarySkill.skillName = "Fire";

            var skillBurrow = ScriptableObject.CreateInstance<SkillDef>();
            ((ScriptableObject)skillBurrow).name = "RisingTides_MushSupporterBodyBurrow";
            skillBurrow.skillName = "Burrow";
            skillBurrow.activationStateMachineName = "Weapon";
            skillBurrow.activationState = new EntityStates.SerializableEntityStateType(typeof(BurrowStart));
            skillBurrow.interruptPriority = EntityStates.InterruptPriority.Any;
            skillBurrow.baseRechargeInterval = 20f;
            skillBurrow.baseMaxStock = 1;
            skillBurrow.rechargeStock = 1;
            skillBurrow.requiredStock = 1;
            skillBurrow.stockToConsume = 1;
            skillBurrow.resetCooldownTimerOnUse = true;
            skillBurrow.fullRestockOnAssign = true;
            skillBurrow.dontAllowPastMaxStocks = false;
            skillBurrow.beginSkillCooldownOnSkillEnd = false;
            skillBurrow.cancelSprintingOnActivation = false;
            skillBurrow.forceSprintDuringState = false;
            skillBurrow.canceledFromSprinting = false;
            skillBurrow.isCombatSkill = false;
            skillBurrow.mustKeyPress = false;
            RisingTidesContent.Resources.skillDefs.Add(skillBurrow);

            skillFamily = ScriptableObject.CreateInstance<SkillFamily>();
            ((ScriptableObject)skillFamily).name = "RisingTides_MushSupporterBodyUtilityFamily";
            skillFamily.variants = new SkillFamily.Variant[]
            {
                new SkillFamily.Variant
                {
                    skillDef = skillBurrow
                }
            };
            skillFamily.defaultVariantIndex = 0;
            RisingTidesContent.Resources.skillFamilies.Add(skillFamily);

            var utilitySkill = prefab.AddComponent<GenericSkill>();
            utilitySkill._skillFamily = skillFamily;
            utilitySkill.skillName = "Burrow";

            SkillLocator skillLocator = prefab.GetComponent<SkillLocator>();
            skillLocator.primary = primarySkill;
            skillLocator.utility = utilitySkill;

            // model
            CharacterModel characterModel = modelTransform.GetComponent<CharacterModel>();
            characterModel.baseRendererInfos = new CharacterModel.RendererInfo[]
            {
                new CharacterModel.RendererInfo
                {
                    renderer = meshObject.GetComponent<SkinnedMeshRenderer>(),
                    defaultMaterial = meshObject.GetComponent<SkinnedMeshRenderer>().material,
                    defaultShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On,
                    ignoreOverlays = false
                }
            };
            AfterCharacterModelSetUp();

            onSetupIDRS += () =>
            {
                AddDisplayRule(RoR2Content.Equipment.AffixRed, EliteDisplays.fireHorn, "Cap", new Vector3(0.3295F, 0.05274F, 0F), new Vector3(321.9229F, 90F, 180F), new Vector3(0.06626F, 0.06626F, 0.06626F));
                AddDisplayRule(RoR2Content.Equipment.AffixRed, EliteDisplays.fireHorn, "Cap", new Vector3(-0.32181F, 0.05272F, 0F), new Vector3(321.9229F, 270F, 180F), new Vector3(0.06626F, 0.06626F, 0.06626F));
                AddDisplayRule(RoR2Content.Equipment.AffixBlue, EliteDisplays.lightningHorn, "Cap", new Vector3(0.31545F, 0.01121F, -0.01092F), new Vector3(90F, 270F, 0F), new Vector3(0.21421F, 0.21421F, 0.21421F));
                AddDisplayRule(RoR2Content.Equipment.AffixBlue, EliteDisplays.lightningHorn, "Cap", new Vector3(-0.31545F, 0.01121F, -0.01092F), new Vector3(90F, 90F, 0F), new Vector3(0.21421F, 0.21421F, 0.21421F));
                AddDisplayRule(RoR2Content.Equipment.AffixWhite, EliteDisplays.iceCrown, "ROOT", new Vector3(0F, 0.58429F, 0F), new Vector3(90F, 180F, 0F), new Vector3(0.02089F, 0.02089F, 0.02089F));
                AddDisplayRule(RoR2Content.Equipment.AffixPoison, EliteDisplays.poisonCrown, "Cap", new Vector3(0F, 0.02557F, 0F), new Vector3(90F, 180F, 0F), new Vector3(0.05652F, 0.05652F, 0.05652F));
                AddDisplayRule(RoR2Content.Equipment.AffixHaunted, EliteDisplays.hauntedCrown, "ROOT", new Vector3(0F, 0.61831F, 0F), new Vector3(90F, 180F, 0F), new Vector3(0.03457F, 0.03457F, 0.03457F));
                AddDisplayRule(RoR2Content.Equipment.AffixLunar, EliteDisplays.lunarEye, "ROOT", new Vector3(0F, 0.62091F, 0F), new Vector3(90F, 180F, 0F), new Vector3(0.2733F, 0.2733F, 0.2733F));
                AddDisplayRule(RisingTidesContent.Equipment.RisingTides_AffixWater, BaseEquipment.loadedDictionary["RisingTides_AffixWater"].itemDisplayPrefab, "ROOT", new Vector3(0F, 0.69753F, 0F), new Vector3(90F, 0F, 0F), new Vector3(0.01971F, 0.01971F, 0.01971F));
                AddDisplayRule(RisingTidesContent.Equipment.RisingTides_AffixBarrier, BaseEquipment.loadedDictionary["RisingTides_AffixBarrier"].itemDisplayPrefab, "Cap", new Vector3(0F, 0.13169F, 0F), new Vector3(0F, 270F, 90F), new Vector3(0.08713F, 0.08713F, 0.08713F));
                AddDisplayRule(RisingTidesContent.Equipment.RisingTides_AffixBarrier, BaseEquipment.loadedDictionary["RisingTides_AffixBarrier"].itemDisplayPrefabs["halo"], "Cap", new Vector3(0F, 0.08209F, 0.24006F), new Vector3(0F, 0F, 0F), new Vector3(0.02933F, 0.02933F, 0.02933F));
                AddDisplayRule(RisingTidesContent.Equipment.RisingTides_AffixBlackHole, BaseEquipment.loadedDictionary["RisingTides_AffixBlackHole"].itemDisplayPrefab, "Cap", new Vector3(0F, 0.04332F, 0.26297F), new Vector3(-0.00001F, 180F, 180F), new Vector3(0.08568F, 0.08568F, 0.08568F));
                AddDisplayRule(RisingTidesContent.Equipment.RisingTides_AffixMoney, BaseEquipment.loadedDictionary["RisingTides_AffixMoney"].itemDisplayPrefab, "ROOT", new Vector3(0F, 0.65479F, 0F), new Vector3(90F, 0F, 0F), new Vector3(0.01971F, 0.01971F, 0.01971F));
                AddDisplayRule(RisingTidesContent.Equipment.RisingTides_AffixNight, BaseEquipment.loadedDictionary["RisingTides_AffixNight"].itemDisplayPrefab, "ROOT", new Vector3(0F, 0.70491F, 0F), new Vector3(90F, 0F, 0F), new Vector3(0.0278F, 0.0278F, 0.0278F));
            };


            // spore projectile
            var sporeGhost = RisingTidesPlugin.AssetBundle.LoadAsset<GameObject>("Assets/Mods/RisingTides/Characters/MushSupporter/SporeProjectileGhost.prefab");
            sporeGhost.AddComponent<ProjectileGhostController>();

            Utils.CopyChildren(RisingTidesPlugin.AssetBundle.LoadAsset<GameObject>("Assets/Mods/RisingTides/Characters/MushSupporter/SporeProjectile.prefab"), sporeProjectile);
            var projectileController = sporeProjectile.AddComponent<ProjectileController>();
            projectileController.ghostPrefab = sporeGhost;
            projectileController.allowPrediction = true;
            projectileController.procCoefficient = 0.4f;
            sporeProjectile.AddComponent<ProjectileNetworkTransform>();
            sporeProjectile.AddComponent<TeamFilter>();
            sporeProjectile.AddComponent<DestroyOnTimer>().duration = 10f;
            var projectileDamage = sporeProjectile.AddComponent<ProjectileDamage>();
            var sporeProjectileComponent = sporeProjectile.AddComponent<RisingTidesSporeProjectile>();
            sporeProjectileComponent.impactEffect = RisingTidesPlugin.AssetBundle.LoadAsset<GameObject>("Assets/Mods/RisingTides/Characters/MushSupporter/SporeImpactEffect.prefab");
            sporeProjectileComponent.impactEffect.AddComponent<EffectComponent>();
            var vfxAttributes = sporeProjectileComponent.impactEffect.AddComponent<VFXAttributes>();
            vfxAttributes.vfxIntensity = VFXAttributes.VFXIntensity.Low;
            vfxAttributes.vfxPriority = VFXAttributes.VFXPriority.Low;
            sporeProjectileComponent.impactEffect.AddComponent<DestroyOnTimer>().duration = 1f;
            var projectileInflictTimedBuff = sporeProjectile.AddComponent<ProjectileInflictTimedBuff>();
            projectileInflictTimedBuff.duration = 2f;
            RoR2Application.onLoad += () =>
            {
                projectileInflictTimedBuff.buffDef = RoR2Content.Buffs.Slow50;
                sporeProjectileComponent.bodyIndexToIgnore = characterBody.bodyIndex;
            };
            sporeProjectile.AddComponent<ProjectileSteerTowardTarget>().rotationSpeed = 30f;
            var targetFinder = sporeProjectile.AddComponent<ProjectileDirectionalTargetFinder>();
            targetFinder.lookRange = 30f;
            targetFinder.lookCone = 30f;
            targetFinder.onlySearchIfNoTarget = true;
            targetFinder.allowTargetLoss = true;

            RisingTidesContent.Resources.projectilePrefabs.Add(sporeProjectile);
            RisingTidesContent.Resources.effectPrefabs.Add(sporeProjectileComponent.impactEffect);
        }

        public class RisingTidesSporeProjectile : MonoBehaviour, IProjectileImpactBehavior
        {
            public BodyIndex bodyIndexToIgnore = BodyIndex.None;

            public float enemyDamageCoefficient = 0.8f;
            public float allyHealCoefficient = 1.8f;
            public bool destroyOnEnemy = true;
            public bool destroyOnAlly = true;
            public bool destroyOnWorld = true;
            public bool impactOnWorld = true;
            public GameObject impactEffect;
            public float desiredForwardSpeedMin = 4f;
            public float desiredForwardSpeedMax = 25f;
            public float initialImpulsePowerMin = 2f;
            public float initialImpulsePowerMax = 10f;
            public float initialImpulseDuration = 0.2f;

            public float circularRotationMin = -1.5f;
            public float circularRotationMax = 1.5f;

            public float upwardsRotationMin = 0f;
            public float upwardsRotationMax = 0.02f;

            public Rigidbody rigidbody;
            public ProjectileController projectileController;
            public ProjectileDamage projectileDamage;
            public ProjectileTargetComponent projectileTargetComponent;
            public bool alive = true;
            public float desiredForwardSpeed = 1f;
            public float initialImpulsePower = 0f;
            public float fixedAge;
            public Quaternion angularRotation = Quaternion.identity;

            public void Awake()
            {
                projectileController = GetComponent<ProjectileController>();
                projectileDamage = GetComponent<ProjectileDamage>();
                projectileTargetComponent = GetComponent<ProjectileTargetComponent>();
                rigidbody = GetComponent<Rigidbody>();

                desiredForwardSpeed = RoR2Application.rng.RangeFloat(desiredForwardSpeedMin, desiredForwardSpeedMax);
                initialImpulsePower = RoR2Application.rng.RangeFloat(initialImpulsePowerMin, initialImpulsePowerMax);

                var angularRotationEuler = Vector3.zero;
                angularRotationEuler.x = RoR2Application.rng.RangeFloat(upwardsRotationMin, upwardsRotationMax);
                angularRotationEuler.y = RoR2Application.rng.RangeFloat(circularRotationMin, circularRotationMax);
                angularRotationEuler.z = 0;
                angularRotation = Quaternion.Euler(angularRotationEuler);
            }

            public void FixedUpdate()
            {
                fixedAge += Time.fixedDeltaTime;

                rigidbody.velocity = transform.forward * (desiredForwardSpeed + initialImpulsePower * Mathf.Clamp01(1f - fixedAge / initialImpulseDuration));

                if (!projectileTargetComponent || !projectileTargetComponent.target)
                {
                    transform.forward = angularRotation * transform.forward * Time.fixedDeltaTime;
                }

                if (!alive)
                {
                    Destroy(gameObject);
                }
            }

            public void OnProjectileImpact(ProjectileImpactInfo impactInfo)
            {
                if (!alive) return;

                var collider = impactInfo.collider;
                if (collider)
                {
                    var damageInfo = new DamageInfo();
                    if (projectileDamage)
                    {
                        damageInfo.damage = projectileDamage.damage * enemyDamageCoefficient;
                        damageInfo.crit = projectileDamage.crit;
                        damageInfo.attacker = projectileController.owner ? projectileController.owner.gameObject : null;
                        damageInfo.inflictor = gameObject;
                        damageInfo.position = impactInfo.estimatedPointOfImpact;
                        damageInfo.force = projectileDamage.force * transform.forward;
                        damageInfo.procChainMask = projectileController.procChainMask;
                        damageInfo.procCoefficient = projectileController.procCoefficient;
                        damageInfo.damageColorIndex = projectileDamage.damageColorIndex;
                    }

                    var hurtBox = collider.GetComponent<HurtBox>();
                    if (hurtBox)
                    {
                        var healthComponent = hurtBox.healthComponent;
                        if (healthComponent)
                        {
                            if (healthComponent.body.bodyIndex != bodyIndexToIgnore)
                            {
                                if (projectileController.teamFilter.teamIndex == healthComponent.body.teamComponent.teamIndex)
                                {
                                    if (NetworkServer.active)
                                    {
                                        healthComponent.Heal(damageInfo.damage, default);
                                    }
                                    if (destroyOnAlly) alive = false;
                                }
                                else
                                {
                                    if (NetworkServer.active)
                                    {
                                        healthComponent.TakeDamage(damageInfo);
                                        GlobalEventManager.instance.OnHitEnemy(damageInfo, healthComponent.gameObject);
                                    }
                                    if (destroyOnEnemy) alive = false;
                                }
                            }
                            else
                            {
                                return;
                            }
                        }
                    }
                    else
                    {
                        if (destroyOnWorld) alive = false;
                    }

                    if (NetworkServer.active && (hurtBox != null || impactOnWorld))
                    {
                        GlobalEventManager.instance.OnHitAll(damageInfo, collider.gameObject);
                        EffectManager.SimpleImpactEffect(impactEffect, impactInfo.estimatedPointOfImpact, impactInfo.estimatedImpactNormal, true);
                    }
                }
            }
        }

        public override void AfterContentPackLoaded()
        {
            base.AfterContentPackLoaded();
            BaseCharacterMaster.characterSpawnCards["RisingTides_MushSupporter"].prefab.GetComponent<CharacterMaster>().bodyPrefab = prefab;
        }

        // entity states
        public class SpawnState : EntityStates.BaseState
        {
            public static GameObject burrowPrefab;
            public static float duration = 0.6f;

            public override void OnEnter()
            {
                base.OnEnter();
                Util.PlaySound("Play_minimushroom_spawn", gameObject);
                PlayAnimation("Body", "Spawn", "Spawn.playbackRate", duration);
                if (burrowPrefab)
                    EffectManager.SimpleMuzzleFlash(burrowPrefab, gameObject, "шл€па", false);
            }

            public override void FixedUpdate()
            {
                base.FixedUpdate();
                if (isAuthority && fixedAge >= duration)
                {
                    outer.SetNextStateToMain();
                }
            }

            public override EntityStates.InterruptPriority GetMinimumInterruptPriority()
            {
                return EntityStates.InterruptPriority.Death;
            }
        }

        public class DeathState : EntityStates.GenericCharacterDeath
        {
            public static GameObject enterEffectPrefab;

            public override void CreateDeathEffects()
            {
                base.CreateDeathEffects();
                if (enterEffectPrefab)
                {
                    EffectManager.SimpleEffect(enterEffectPrefab, characterBody.corePosition, transform.rotation, false);
                }
            }
        }

        public class FireSpores : EntityStates.BaseState
        {
            public static float baseDuration = 2.3333f;
            public static float baseEmissionRate = 45f;
            public static float baseEmissionDelay = 0.9f;
            public static float baseEmissionDuration = 1f;
            public static float minSpread = 60f;
            public static float maxSpread = 90f;
            public static float damageCoefficient = 0.4f;

            public static string muzzleName = "Cap";

            public float duration;
            public float emissionRate;
            public float emissionDelay;
            public float emissionDuration;
            public float emissionAccumulator = 0;
            public Transform muzzleTransform;
            public bool emissionStarted = false;

            public override void OnEnter()
            {
                base.OnEnter();
                duration = baseDuration / attackSpeedStat;
                emissionRate = baseEmissionRate * attackSpeedStat;
                emissionDelay = baseEmissionDelay / attackSpeedStat;
                emissionDuration = baseEmissionDuration / attackSpeedStat;
                PlayAnimation("Gesture", "Fire", "Fire.playbackRate", duration);

                muzzleTransform = FindModelChild(muzzleName);
                if (!muzzleTransform) muzzleTransform = transform;
            }

            public override void FixedUpdate()
            {
                base.FixedUpdate();
                if (isAuthority)
                {
                    if (fixedAge >= emissionDelay && fixedAge < emissionDelay + emissionDuration)
                    {
                        if (!emissionStarted)
                        {
                            emissionStarted = true;
                            Util.PlayAttackSpeedSound("Play_minimushroom_spore_explode", gameObject, attackSpeedStat);
                        }
                        emissionAccumulator += emissionRate * Time.fixedDeltaTime;
                    }
                    while (emissionAccumulator >= 1f)
                    {
                        emissionAccumulator -= 1f;
                        EmitSpore();
                    }
                    if (fixedAge >= duration)
                    {
                        outer.SetNextStateToMain();
                    }
                }
            }

            public void EmitSpore()
            {
                var aimDirection = Util.ApplySpread(Vector3.forward, minSpread, maxSpread, 1f, 1f, 0f, 0f);
                aimDirection = Quaternion.AngleAxis(-90f, Vector3.left) * aimDirection;

                var aimRay = new Ray(muzzleTransform.position, aimDirection);

                FireProjectileInfo fireProjectileInfo = new FireProjectileInfo();
                fireProjectileInfo.position = aimRay.origin;
                fireProjectileInfo.rotation = Quaternion.LookRotation(aimRay.direction);
                fireProjectileInfo.crit = characterBody.RollCrit();
                fireProjectileInfo.damage = characterBody.damage * damageCoefficient;
                fireProjectileInfo.damageColorIndex = DamageColorIndex.Default;
                fireProjectileInfo.owner = gameObject;
                fireProjectileInfo.procChainMask = default;
                fireProjectileInfo.force = 0f;
                fireProjectileInfo.target = null;
                fireProjectileInfo.projectilePrefab = sporeProjectile;
                ProjectileManager.instance.FireProjectile(fireProjectileInfo);
            }

            public override EntityStates.InterruptPriority GetMinimumInterruptPriority()
            {
                return EntityStates.InterruptPriority.PrioritySkill;
            }
        }

        public class BurrowStart : EntityStates.BaseState
        {
            public static float baseDuration = 0.9f;
            public float duration;

            public override void OnEnter()
            {
                base.OnEnter();
                duration = baseDuration / attackSpeedStat;
                Util.PlayAttackSpeedSound("Play_miniMushroom_burrow", gameObject, attackSpeedStat);
                PlayAnimation("Body", "BurrowStart", "BurrowStart.playbackRate", duration);
                if (NetworkServer.active && !characterBody.HasBuff(RoR2Content.Buffs.ArmorBoost))
                    characterBody.AddBuff(RoR2Content.Buffs.ArmorBoost);
            }

            public override void FixedUpdate()
            {
                base.FixedUpdate();
                if (isAuthority && fixedAge >= duration)
                {
                    outer.SetNextState(new BurrowEnd());
                }
            }

            public override EntityStates.InterruptPriority GetMinimumInterruptPriority()
            {
                return EntityStates.InterruptPriority.Frozen;
            }
        }

        public class BurrowEnd : EntityStates.BaseState
        {
            public static float baseDuration = 0.5f;
            public float duration;

            public static float enemyBlinkDistance = 30f;
            public static float allyBlinkDistance = 30f;

            public override void OnEnter()
            {
                base.OnEnter();
                duration = baseDuration / attackSpeedStat;

                if (isAuthority)
                {
                    var teleportInfo = GetTeleportInfo();
                    transform.SetPositionAndRotation(teleportInfo.newPosition, teleportInfo.newRotation);
                }

                Util.PlayAttackSpeedSound("Play_miniMushroom_unborrow", gameObject, attackSpeedStat);
                PlayAnimation("Body", "BurrowEnd", "BurrowEnd.playbackRate", duration);
                if (NetworkServer.active && characterBody.HasBuff(RoR2Content.Buffs.ArmorBoost))
                    characterBody.RemoveBuff(RoR2Content.Buffs.ArmorBoost);
            }

            public struct BurrowTeleportInfo
            {
                public Vector3 newPosition;
                public Quaternion newRotation;
            }

            public BurrowTeleportInfo GetTeleportInfo()
            {
                var targetPosition = transform.position;
                if (characterBody.master && characterBody.master.aiComponents.Length > 0)
                {
                    var ai = characterBody.master.aiComponents[0];
                    if (ai.currentEnemy != null && ai.currentEnemy.GetBullseyePosition(out targetPosition))
                    {
                        var newPosition = GetRandomizedSnappedPosition(targetPosition, enemyBlinkDistance);
                        return new BurrowTeleportInfo
                        {
                            newPosition = newPosition,
                            newRotation = GetRotationForLookingAtPoint(newPosition)
                        };
                    }
                    if (ai.leader != null && ai.leader.GetBullseyePosition(out targetPosition))
                    {
                        var newPosition = GetRandomizedSnappedPosition(targetPosition, allyBlinkDistance);
                        return new BurrowTeleportInfo
                        {
                            newPosition = newPosition,
                            newRotation = GetRotationForLookingAtPoint(newPosition)
                        };
                    }
                }
                else
                {
                    var aimRay = GetAimRay();
                    var bullseyeSearch = new BullseyeSearch();
                    bullseyeSearch.searchOrigin = aimRay.origin;
                    bullseyeSearch.searchDirection = aimRay.direction;
                    bullseyeSearch.maxDistanceFilter = 100f;
                    bullseyeSearch.filterByLoS = false;
                    bullseyeSearch.sortMode = BullseyeSearch.SortMode.Distance;

                    bullseyeSearch.teamMaskFilter = TeamMask.allButNeutral;
                    bullseyeSearch.teamMaskFilter.RemoveTeam(TeamComponent.GetObjectTeam(gameObject));
                    bullseyeSearch.RefreshCandidates();
                    var enemy = bullseyeSearch.GetResults().FirstOrDefault();
                    if (enemy)
                    {
                        var newPosition = GetRandomizedSnappedPosition(enemy.transform.position, enemyBlinkDistance);
                        return new BurrowTeleportInfo
                        {
                            newPosition = newPosition,
                            newRotation = GetRotationForLookingAtPoint(newPosition)
                        };
                    }

                    bullseyeSearch.teamMaskFilter = TeamMask.none;
                    bullseyeSearch.teamMaskFilter.AddTeam(TeamComponent.GetObjectTeam(gameObject));
                    bullseyeSearch.RefreshCandidates();
                    var ally = bullseyeSearch.GetResults().FirstOrDefault();
                    if (ally)
                    {
                        var newPosition = GetRandomizedSnappedPosition(enemy.transform.position, allyBlinkDistance);
                        return new BurrowTeleportInfo
                        {
                            newPosition = newPosition,
                            newRotation = GetRotationForLookingAtPoint(newPosition)
                        };
                    }
                }
                return new BurrowTeleportInfo
                {
                    newPosition = SnapVectorToNearestNode(transform.position),
                    newRotation = Quaternion.Euler(0f, RoR2Application.rng.RangeFloat(0f, 360f), 0f)
                };
            }

            public Vector3 GetRandomizedSnappedPosition(Vector3 currentPosition, float offsetRadius)
            {
                currentPosition = SnapVectorToNearestNode(currentPosition + Random.onUnitSphere * offsetRadius);
                return currentPosition;
            }

            public Vector3 SnapVectorToNearestNode(Vector3 vector)
            {
                var nodes = SceneInfo.instance.groundNodes;
                var nodeIndex = nodes.FindClosestNode(vector, characterBody.hullClassification);
                nodes.GetNodePosition(nodeIndex, out vector);
                vector += transform.position - characterBody.footPosition;
                return vector;
            }

            public Quaternion GetRotationForLookingAtPoint(Vector3 point)
            {
                return Quaternion.Euler(0f, Util.QuaternionSafeLookRotation(point - transform.position, Vector3.up).eulerAngles.y, 0f);
            }

            public override void FixedUpdate()
            {
                base.FixedUpdate();
                if (isAuthority && fixedAge >= duration)
                {
                    outer.SetNextStateToMain();
                }
            }

            public override EntityStates.InterruptPriority GetMinimumInterruptPriority()
            {
                return EntityStates.InterruptPriority.Frozen;
            }
        }
    }
}