using MysticsRisky2Utils;
using R2API;
using RoR2;
using RoR2.Orbs;
using System.Linq;
using UnityEngine;

namespace RisingTides.Equipment
{
    public class AffixBlackHoleEquipment : BaseEliteAffix
    {
        public static ConfigOptions.ConfigurableValue<float> detonationDamagePerMark = ConfigOptions.ConfigurableValue.CreateFloat(
            RisingTidesPlugin.PluginGUID, RisingTidesPlugin.PluginName, RisingTidesPlugin.config,
            "Elites: Onyx", "On Use Detonation Damage Per Mark",
            100f,
            description: "How much damage should this elite aspect's on-use detonation deal to each enemy per their debuff stack? (in %)",
            useDefaultValueConfigEntry: RisingTidesPlugin.ignoreBalanceChanges.bepinexConfigEntry
        );

        public override void OnLoad()
        {
            base.OnLoad();
            equipmentDef.name = "RisingTides_AffixBlackHole";
            equipmentDef.cooldown = 10f;
            equipmentDef.pickupIconSprite = RisingTidesPlugin.AssetBundle.LoadAsset<Sprite>("Assets/Mods/RisingTides/Elites/BlackHole/texAffixBlackHoleEquipmentIcon.png");
            SetUpPickupModel();
            // AdjustElitePickupMaterial(Color.grey, 1.26f, RisingTidesPlugin.AssetBundle.LoadAsset<Texture2D>("Assets/Mods/RisingTides/Elites/BlackHole/texRampAffixBlackHoleEquipment.png"));
            AdjustElitePickupMaterial(Color.black, 1.26f);

            itemDisplayPrefab = PrepareItemDisplayModel(PrefabAPI.InstantiateClone(RisingTidesPlugin.AssetBundle.LoadAsset<GameObject>("Assets/Mods/RisingTides/Elites/BlackHole/BlackHoleSharkFin.prefab"), "RisingTidesAffixBarrierHeadpiece", false));
            var mat = itemDisplayPrefab.GetComponentInChildren<Renderer>().sharedMaterial;
            HopooShaderToMaterial.Standard.Apply(mat);
            HopooShaderToMaterial.Standard.DisableEverything(mat);
            onSetupIDRS += () =>
            {
                AddDisplayRule("CommandoBody", "Chest", new Vector3(0F, 0.32207F, -0.17157F), new Vector3(61.58626F, 180F, 0F), new Vector3(0.06391F, 0.06391F, 0.06391F));
                AddDisplayRule("HuntressBody", "Chest", new Vector3(0.00001F, 0.17123F, -0.12994F), new Vector3(84.38821F, 0.00001F, 180F), new Vector3(0.06639F, 0.08081F, 0.07337F));
                AddDisplayRule("Bandit2Body", "Chest", new Vector3(0F, 0.2054F, -0.17354F), new Vector3(85.18521F, 0.00001F, 180F), new Vector3(0.06235F, 0.06235F, 0.06235F));
                AddDisplayRule("ToolbotBody", "Chest", new Vector3(0.00002F, 2.46124F, -1.81533F), new Vector3(38.35899F, 180F, 0F), new Vector3(0.43989F, 0.43989F, 0.43989F));
                AddDisplayRule("EngiBody", "Chest", new Vector3(0F, 0.2425F, -0.28941F), new Vector3(87.55103F, 180F, 0F), new Vector3(0.07816F, 0.07816F, 0.07816F));
                AddDisplayRule("EngiTurretBody", "Head", new Vector3(-0.047F, 0.76482F, -0.91778F), new Vector3(333.2374F, 180F, 0F), new Vector3(0.2291F, 0.2291F, 0.2291F));
                AddDisplayRule("EngiWalkerTurretBody", "Head", new Vector3(-0.009F, 1.37273F, -0.54161F), new Vector3(0F, 180F, 0F), new Vector3(0.21039F, 0.21039F, 0.21039F));
                AddDisplayRule("MageBody", "Chest", new Vector3(0F, 0.0987F, -0.25746F), new Vector3(84.62386F, 180F, 0F), new Vector3(0.09645F, 0.09645F, 0.09645F));
                AddDisplayRule("MercBody", "Chest", new Vector3(0F, 0.18503F, -0.28213F), new Vector3(89.38293F, 180F, 0F), new Vector3(0.05412F, 0.05412F, 0.05412F));
                AddDisplayRule("TreebotBody", "PlatformBase", new Vector3(0F, -0.6238F, 0.0624F), new Vector3(-0.00001F, 180F, 180F), new Vector3(0.1368F, 0.1368F, 0.1368F));
                AddDisplayRule("LoaderBody", "MechBase", new Vector3(0F, 0.19236F, -0.0823F), new Vector3(85.59611F, 0.00001F, 180F), new Vector3(0.07694F, 0.07694F, 0.07694F));
                AddDisplayRule("CrocoBody", "SpineChest1", new Vector3(0F, 0.51759F, 0.59778F), new Vector3(327.5475F, 0F, 0F), new Vector3(0.58293F, 0.90288F, 0.91986F));
                AddDisplayRule("CrocoBody", "Chest", new Vector3(0.33495F, 0.32714F, -2.0456F), new Vector3(79.5185F, 0F, 169.457F), new Vector3(0.41576F, 0.64396F, 0.65607F));
                AddDisplayRule("CaptainBody", "Chest", new Vector3(0F, 0.2263F, -0.19729F), new Vector3(82.07296F, -0.00002F, 180F), new Vector3(0.12007F, 0.12007F, 0.12007F));
                
                AddDisplayRule("WispBody", "Head", new Vector3(0F, 0.17172F, 0.67549F), new Vector3(71.77856F, 0F, 0F), new Vector3(0.15099F, 0.15099F, 0.15099F));
                AddDisplayRule("JellyfishBody", "Hull2", new Vector3(-0.00275F, 0.52657F, -0.82924F), new Vector3(74.90458F, 180F, 0F), new Vector3(0.20951F, 0.20951F, 0.20951F));
                AddDisplayRule("BeetleBody", "Chest", new Vector3(0F, 0.17605F, -0.66683F), new Vector3(76.58255F, 0.00001F, 180F), new Vector3(0.18442F, 0.18442F, 0.18442F));
                AddDisplayRule("LemurianBody", "Chest", new Vector3(0F, -0.19474F, 1.38821F), new Vector3(71.18005F, 180F, 180F), new Vector3(0.97581F, 0.97581F, 0.97581F));
                AddDisplayRule("HermitCrabBody", "Base", new Vector3(0.00001F, 0.68282F, -0.36087F), new Vector3(55.65141F, 180F, 0F), new Vector3(0.177F, 0.177F, 0.177F));
                AddDisplayRule("ImpBody", "Chest", new Vector3(0F, 0.00037F, -0.06942F), new Vector3(71.90101F, 0.00001F, 180F), new Vector3(0.11643F, 0.11643F, 0.11643F));
                AddDisplayRule("VultureBody", "Chest", new Vector3(-0.13905F, 0.40364F, -1.54408F), new Vector3(63.08728F, 0.00001F, 180F), new Vector3(0.86913F, 0.86913F, 0.86913F));
                AddDisplayRule("RoboBallMiniBody", "ROOT", new Vector3(0F, 0.94821F, 0F), new Vector3(9.51164F, 180F, 0F), new Vector3(0.2675F, 0.2675F, 0.2675F));
                AddDisplayRule("MiniMushroomBody", "Head", new Vector3(-0.23232F, -0.00838F, 0F), new Vector3(271.04F, 90F, 0F), new Vector3(0.29763F, 0.29763F, 0.29763F));
                AddDisplayRule("BellBody", "Chain", new Vector3(-0.00488F, -0.01674F, -0.00297F), new Vector3(352.2896F, 56.70449F, 178.0383F), new Vector3(0.46573F, 0.46573F, 0.46573F));
                AddDisplayRule("BeetleGuardBody", "Chest", new Vector3(-0.0651F, 1.12174F, -2.29832F), new Vector3(84.15964F, 47.39052F, 227.6368F), new Vector3(0.59316F, 0.59316F, 0.59316F));
                AddDisplayRule("BisonBody", "Chest", new Vector3(0F, 0.08027F, 0.43315F), new Vector3(47.67143F, 180F, 180F), new Vector3(0.22756F, 0.22756F, 0.22756F));
                AddDisplayRule("GolemBody", "Chest", new Vector3(0F, 0.56927F, -0.31342F), new Vector3(89.66428F, 180F, 0F), new Vector3(0.28954F, 0.28954F, 0.28954F));
                AddDisplayRule("ParentBody", "Chest", new Vector3(-54.08593F, -0.00016F, 0.00007F), new Vector3(90F, 270F, 0F), new Vector3(30.77329F, 30.77329F, 30.77329F));
                AddDisplayRule("ClayBruiserBody", "Chest", new Vector3(-0.00002F, 0.59846F, -0.56425F), new Vector3(72.47513F, 180F, 0F), new Vector3(0.14388F, 0.14388F, 0.14388F));
                AddDisplayRule("ClayBruiserBody", "Muzzle", new Vector3(0.00001F, -0.35307F, -0.7658F), new Vector3(345.4555F, 180F, 180F), new Vector3(0.15894F, 0.15894F, 0.15894F));
                AddDisplayRule("GreaterWispBody", "MaskBase", new Vector3(0F, 0.96585F, 0.5596F), new Vector3(314.1227F, 180F, 0F), new Vector3(0.14965F, 0.14965F, 0.14965F));
                AddDisplayRule("LemurianBruiserBody", "Chest", new Vector3(0F, 1.29113F, 1.71805F), new Vector3(78.59312F, 0F, 0F), new Vector3(0.94043F, 0.94043F, 0.94043F));
                AddDisplayRule("NullifierBody", "Muzzle", new Vector3(0F, 0.95026F, 0.34576F), new Vector3(356.5133F, 180F, 0F), new Vector3(0.50009F, 0.50009F, 0.50009F));

                AddDisplayRule("BeetleQueen2Body", "Chest", new Vector3(0F, 1.2333F, 2.11659F), new Vector3(57.73215F, 180F, 180F), new Vector3(0.84575F, 0.84575F, 0.84575F));
                AddDisplayRule("ClayBossBody", "PotLidTop", new Vector3(0F, 0.33325F, 1.12097F), new Vector3(0F, 180F, 0F), new Vector3(0.47481F, 0.47481F, 0.47481F));
                AddDisplayRule("TitanBody", "Chest", new Vector3(0F, 3.78825F, -3.2316F), new Vector3(90F, 180F, 0F), new Vector3(1.35068F, 1.35068F, 1.35068F));
                AddDisplayRule("TitanGoldBody", "Chest", new Vector3(0F, 3.78825F, -3.2316F), new Vector3(90F, 180F, 0F), new Vector3(1.35068F, 1.35068F, 1.35068F));
                AddDisplayRule("VagrantBody", "Hull", new Vector3(0F, 0.68025F, -1.12448F), new Vector3(80.65052F, 180F, 0F), new Vector3(0.2448F, 0.2448F, 0.2448F));
                string[] worms = new string[]
                {
                    "MagmaWormBody",
                    "ElectricWormBody"
                };
                foreach (string worm in worms)
                {
                    AddDisplayRule(worm, "UpperJaw", new Vector3(0F, 0F, -0.51749F), new Vector3(90F, 180F, 0F), new Vector3(0.33248F, 0.33248F, 0.33247F));
                    for (var i = 1; i <= 16; i += 4)
                    {
                        Vector3 scale = Vector3.one * 0.33248F * Mathf.Pow(0.934782609f, i - 1);
                        AddDisplayRule(worm, "Neck" + i.ToString(), new Vector3(0.01423997F, 0.6766583F + 0.03293F * (i - 1), -1.209069F + 0.02657F * (i - 1)), new Vector3(270F, 0F, 0F), scale);
                    }
                }
                AddDisplayRule("RoboBallBossBody", "Shell", new Vector3(0F, 1.07915F, 0.01541F), new Vector3(0F, 180F, 0F), new Vector3(0.20578F, 0.20578F, 0.20578F));
                AddDisplayRule("SuperRoboBallBossBody", "Shell", new Vector3(0F, 1.07915F, 0.01541F), new Vector3(0F, 180F, 0F), new Vector3(0.20578F, 0.20578F, 0.20578F));
                AddDisplayRule("GravekeeperBody", "Neck1", new Vector3(0F, 2.76024F, 1.31344F), new Vector3(61.61956F, 0F, 0F), new Vector3(0.75697F, 0.75697F, 0.75697F));
                AddDisplayRule("ImpBossBody", "Chest", new Vector3(0F, 0.28061F, -0.3357F), new Vector3(77.69851F, 0.00001F, 180F), new Vector3(0.64156F, 0.64156F, 0.64156F));
                AddDisplayRule("GrandParentBody", "Chest", new Vector3(-0.00001F, 4.25656F, -4.74021F), new Vector3(70.63297F, 0F, 180F), new Vector3(1.26669F, 1.26669F, 1.26669F));
                AddDisplayRule("ScavBody", "Chest", new Vector3(0F, 4.70675F, 1.65828F), new Vector3(22.40885F, 0F, 0F), new Vector3(2.4305F, 2.4305F, 2.4305F));
            };
            ChildLocatorAdditions.list.Add(new ChildLocatorAdditions.Addition
            {
                modelName = "mdlBeetleQueen",
                transformLocation = "BeetleQueenArmature/ROOT/Base/Chest",
                childName = "Chest"
            });
            ChildLocatorAdditions.list.Add(new ChildLocatorAdditions.Addition
            {
                modelName = "mdlGravekeeper",
                transformLocation = "GravekeeperArmature/ROOT/base/stomach/chest/neck.1",
                childName = "Neck1"
            });
            ChildLocatorAdditions.list.Add(new ChildLocatorAdditions.Addition
            {
                modelName = "mdlGravekeeper",
                transformLocation = "GravekeeperArmature/ROOT/base/stomach/chest/neck.1/neck.2",
                childName = "Neck2"
            });
        }

		public override bool OnUse(EquipmentSlot equipmentSlot)
        {
            if (equipmentSlot.characterBody)
            {
                EffectManager.SimpleImpactEffect(EntityStates.GlobalSkills.LunarDetonator.Detonate.enterEffectPrefab, equipmentSlot.characterBody.corePosition, Vector3.up, false);
                Util.PlaySound(EntityStates.GlobalSkills.LunarDetonator.Detonate.enterSoundString, equipmentSlot.gameObject);

                var bullseyeSearch = new BullseyeSearch();
                bullseyeSearch.filterByDistinctEntity = true;
                bullseyeSearch.filterByLoS = false;
                bullseyeSearch.maxDistanceFilter = float.PositiveInfinity;
                bullseyeSearch.minDistanceFilter = 0f;
                bullseyeSearch.minAngleFilter = 0f;
                bullseyeSearch.maxAngleFilter = 180f;
                bullseyeSearch.sortMode = BullseyeSearch.SortMode.Distance;
                bullseyeSearch.teamMaskFilter = TeamMask.GetUnprotectedTeams(equipmentSlot.teamComponent.teamIndex);
                bullseyeSearch.searchOrigin = equipmentSlot.characterBody.corePosition;
                bullseyeSearch.viewer = null;
                bullseyeSearch.RefreshCandidates();
                bullseyeSearch.FilterOutGameObject(equipmentSlot.gameObject);
                var detonationTargets = bullseyeSearch.GetResults().ToArray();

                var detonationController = new DetonationController();
                detonationController.characterBody = equipmentSlot.characterBody;
                detonationController.interval = EntityStates.GlobalSkills.LunarDetonator.Detonate.detonationInterval;
                detonationController.detonationTargets = detonationTargets;
                detonationController.damageStat = equipmentSlot.characterBody.damage;
                detonationController.isCrit = equipmentSlot.characterBody.RollCrit();
                detonationController.active = true;

                return true;
            }
            return false;
        }

		public class DetonationController
		{
			public HurtBox[] detonationTargets;
			public CharacterBody characterBody;
			public float damageStat;
			public bool isCrit;
			public float interval;
			private int i;
			private float timer;
			private bool _active;

			public bool active
			{
				get
				{
					return _active;
				}
				set
				{
					if (_active == value) return;
					_active = value;
					if (_active)
					{
						RoR2Application.onFixedUpdate += FixedUpdate;
						return;
					}
					RoR2Application.onFixedUpdate -= FixedUpdate;
				}
			}

			private void FixedUpdate()
			{
				if (!characterBody || !characterBody.healthComponent || !characterBody.healthComponent.alive)
				{
					active = false;
					return;
				}
				timer -= Time.deltaTime;
				if (timer <= 0f)
				{
					timer = interval;
					while (i < detonationTargets.Length)
					{
                        try
                        {
							HurtBox targetHurtBox = null;
							Util.Swap(ref targetHurtBox, ref detonationTargets[i]);
							if (DoDetonation(targetHurtBox))
							{
								break;
							}
						}
						catch (System.Exception) { }
						i++;
					}
					if (i >= detonationTargets.Length)
					{
						active = false;
					}
				}
			}

			private bool DoDetonation(HurtBox targetHurtBox)
			{
				if (!targetHurtBox) return false;
				var healthComponent = targetHurtBox.healthComponent;
				if (!healthComponent) return false;
				var body = healthComponent.body;
				if (!body) return false;
				var buffCount = body.GetBuffCount(RisingTidesContent.Buffs.RisingTides_BlackHoleMark);
				if (buffCount <= 0) return false;

                var orb = new BlackHoleDetonatorOrb
                {
                    origin = characterBody.corePosition,
                    target = targetHurtBox,
                    attacker = characterBody.gameObject,
                    damageValue = damageStat * detonationDamagePerMark * buffCount,
                    damageColorIndex = DamageColorIndex.Default,
                    isCrit = isCrit,
                    procChainMask = default,
                    procCoefficient = 0f
                };
                OrbManager.instance.AddOrb(orb);
				return true;
			}
		}

		public class BlackHoleDetonatorOrb : GenericDamageOrb
		{
			public override void Begin()
			{
				speed = 120f;
				base.Begin();
			}

			public override GameObject GetOrbEffect()
			{
				return EntityStates.GlobalSkills.LunarDetonator.Detonate.orbEffectPrefab;
			}

            public override void OnArrival()
            {
                base.OnArrival();
				if (target)
				{
					EffectManager.SpawnEffect(EntityStates.GlobalSkills.LunarDetonator.Detonate.detonationEffectPrefab, new EffectData
					{
						origin = target.transform.position,
						rotation = Quaternion.identity,
						scale = 1f
					}, true);
				}
			}
        }

		public override void AfterContentPackLoaded()
        {
            base.AfterContentPackLoaded();
            equipmentDef.passiveBuffDef = RisingTidesContent.Buffs.RisingTides_AffixBlackHole;
        }
    }
}
