using MysticsRisky2Utils;
using R2API;
using RoR2;
using UnityEngine;

namespace RisingTides.Equipment
{
    public class AffixBarrierEquipment : BaseEliteAffix
    {
        public static ConfigOptions.ConfigurableValue<float> barrierRecharge = ConfigOptions.ConfigurableValue.CreateFloat(
            RisingTidesPlugin.PluginGUID, RisingTidesPlugin.PluginName, RisingTidesPlugin.config,
            "Elites: Bismuth", "On Use Barrier Recharge",
            25f, 0f, 100f,
            description: "How much barrier should this elite aspect's on-use effect regen? (in %)",
            useDefaultValueConfigEntry: RisingTidesPlugin.ignoreBalanceChanges.bepinexConfigEntry
        );

        public static GameObject selfBuffUseEffect;

        public override void OnLoad()
        {
            base.OnLoad();
            equipmentDef.name = "RisingTides_AffixBarrier";
            equipmentDef.cooldown = 30f;
            equipmentDef.pickupIconSprite = RisingTidesPlugin.AssetBundle.LoadAsset<Sprite>("Assets/Mods/RisingTides/Elites/Barrier/texAffixBarrierEquipmentIcon.png");
            SetUpPickupModel();
            AdjustElitePickupMaterial(Color.white, 4f, RisingTidesPlugin.AssetBundle.LoadAsset<Texture2D>("Assets/Mods/RisingTides/Elites/Barrier/texRampAffixBarrierEquipment.png"));

            itemDisplayPrefab = PrepareItemDisplayModel(PrefabAPI.InstantiateClone(RisingTidesPlugin.AssetBundle.LoadAsset<GameObject>("Assets/Mods/RisingTides/Elites/Barrier/AffixBarrierShieldDisplay.prefab"), "RisingTidesAffixBarrierShieldDisplay", false));
            foreach (var renderer in itemDisplayPrefab.GetComponentsInChildren<Renderer>())
            {
                var material = renderer.sharedMaterial;
                material.SetTexture("_EmTex", material.GetTexture("_EmissionMap"));
                material.SetColor("_EmColor", material.GetColor("_EmissionColor"));
                material.SetFloat("_EmPower", 2f);
            }
            itemDisplayPrefabs["halo"] = PrepareItemDisplayModel(PrefabAPI.InstantiateClone(RisingTidesPlugin.AssetBundle.LoadAsset<GameObject>("Assets/Mods/RisingTides/Elites/Barrier/AffixBarrierHeadpiece.prefab"), "RisingTidesAffixBarrierHeadpiece", false));
            onSetupIDRS += () =>
            {
                AddDisplayRule("CommandoBody", "LowerArmL", new Vector3(-0.0146F, 0.10142F, -0.09926F), new Vector3(80.81804F, 73.87079F, 352.0796F), new Vector3(0.09507F, 0.09507F, 0.09507F));
                AddDisplayRule("HuntressBody", "LowerArmL", new Vector3(0.00453F, 0.18608F, -0.0794F), new Vector3(74.06459F, 247.7539F, 163.7938F), new Vector3(0.07896F, 0.07896F, 0.07896F));
                AddDisplayRule("Bandit2Body", "LowerArmL", new Vector3(0.07384F, 0.02959F, -0.0921F), new Vector3(54.54504F, 309.9253F, 201.8698F), new Vector3(0.09507F, 0.09507F, 0.09507F));
                AddDisplayRule("ToolbotBody", "LowerArmR", new Vector3(-0.05586F, 1.84315F, 0.81928F), new Vector3(282.2477F, 259.3919F, 1.76163F), new Vector3(1.04621F, 1.04621F, 1.04621F));
                AddDisplayRule("EngiBody", "LowerArmR", new Vector3(-0.02242F, 0.11966F, -0.07493F), new Vector3(343.2251F, 261.9322F, 178.5142F), new Vector3(0.109F, 0.109F, 0.109F));
                AddDisplayRule("EngiTurretBody", "Head", new Vector3(0F, 0.68505F, 0F), new Vector3(0F, 270F, 90F), new Vector3(0.29954F, 0.29954F, 0.29954F));
                AddDisplayRule("EngiWalkerTurretBody", "Head", new Vector3(0F, 1.41667F, -0.21911F), new Vector3(0F, 270F, 90F), new Vector3(0.31666F, 0.31666F, 0.31666F));
                AddDisplayRule("MageBody", "LowerArmR", new Vector3(-0.05743F, 0.16793F, 0.08284F), new Vector3(282.7379F, 277.4919F, 344.0832F), new Vector3(0.09507F, 0.09507F, 0.09507F));
                AddDisplayRule("MercBody", "LowerArmR", new Vector3(-0.02357F, 0.29074F, -0.12531F), new Vector3(283.4819F, 272.5186F, 168.9351F), new Vector3(0.09507F, 0.09507F, 0.09507F));
                AddDisplayRule("TreebotBody", "PlatformBase", new Vector3(0F, 1.92666F, -0.97747F), new Vector3(-0.00008F, 89.99995F, 338.9332F), new Vector3(0.23693F, 0.23693F, 0.23693F));
                AddDisplayRule("TreebotBody", "PlatformBase", new Vector3(-0.95428F, 1.92644F, -0.15987F), new Vector3(-0.00007F, 170.9057F, 338.9332F), new Vector3(0.23693F, 0.23693F, 0.23693F));
                AddDisplayRule("TreebotBody", "PlatformBase", new Vector3(0.12647F, 1.82997F, 0.83765F), new Vector3(-0.00015F, 277.5678F, 338.9332F), new Vector3(0.23693F, 0.23693F, 0.23693F));
                AddDisplayRule("TreebotBody", "PlatformBase", new Vector3(0.91269F, 1.92139F, -0.00786F), new Vector3(-0.00022F, 1.56418F, 338.9333F), new Vector3(0.23693F, 0.23693F, 0.23693F));
                AddDisplayRule("LoaderBody", "MechBase", new Vector3(-0.0029F, 0.09235F, 0.4434F), new Vector3(0F, 270F, 0F), new Vector3(0.11442F, 0.11442F, 0.11442F));
                AddDisplayRule("CrocoBody", "UpperArmL", new Vector3(-1.53705F, -0.49132F, 0.35113F), new Vector3(19.10361F, 27.51384F, 188.1016F), new Vector3(0.66791F, 0.66791F, 0.66791F));
                AddDisplayRule("CrocoBody", "UpperArmR", new Vector3(1.30091F, 0.7689F, 0.42121F), new Vector3(341.9067F, 153.8942F, 172.765F), new Vector3(0.66791F, 0.66791F, 0.66791F));
                AddDisplayRule("CaptainBody", "ClavicleL", new Vector3(0.07978F, 0.17095F, -0.1458F), new Vector3(344.9297F, 240.0269F, 175.6339F), new Vector3(0.08304F, 0.08304F, 0.08304F));
                AddDisplayRule("CaptainBody", "ClavicleR", new Vector3(-0.03803F, 0.18421F, -0.14963F), new Vector3(6.35989F, 291.6088F, 176.4272F), new Vector3(0.08304F, 0.08304F, 0.08304F));

                AddDisplayRule("WispBody", "Head", new Vector3(0F, 0.21494F, 0.57267F), new Vector3(0F, 90.00002F, 112.6696F), new Vector3(0.28338F, 0.28338F, 0.28338F));
                AddDisplayRule("JellyfishBody", "Hull2", new Vector3(-0.66769F, 0.5537F, -0.38737F), new Vector3(352.1283F, 330.5081F, 147.708F), new Vector3(0.27148F, 0.27148F, 0.27148F));
                AddDisplayRule("JellyfishBody", "Hull2", new Vector3(0.74791F, 0.19783F, -0.53016F), new Vector3(354.2131F, 220.0504F, 167.8046F), new Vector3(0.27148F, 0.27148F, 0.27148F));
                AddDisplayRule("JellyfishBody", "Hull2", new Vector3(-0.08774F, -0.09304F, 1.00823F), new Vector3(355.0139F, 86.40502F, 178.2649F), new Vector3(0.27148F, 0.27148F, 0.27148F));
                AddDisplayRule("BeetleBody", "Head", new Vector3(0F, 0.34403F, 0.37159F), new Vector3(0F, 90F, 112.8398F), new Vector3(0.25041F, 0.25041F, 0.25041F));
                AddDisplayRule("BeetleBody", "Chest", new Vector3(0F, 0.19652F, -0.61841F), new Vector3(0F, 90F, 2.51592F), new Vector3(0.25041F, 0.25041F, 0.25041F));
                AddDisplayRule("LemurianBody", "LowerArmL", new Vector3(-0.28231F, -1.43541F, -0.90572F), new Vector3(5.52779F, 286.69F, 183.0832F), new Vector3(1.5576F, 1.5576F, 1.5576F));
                AddDisplayRule("HermitCrabBody", "Base", new Vector3(-0.35249F, 0.58269F, 0.35845F), new Vector3(358.8646F, 225.772F, 31.04556F), new Vector3(0.17834F, 0.17834F, 0.17834F));
                AddDisplayRule("HermitCrabBody", "Base", new Vector3(0.00685F, 0.55719F, -0.51574F), new Vector3(344.942F, 83.88705F, 25.03429F), new Vector3(0.17834F, 0.17834F, 0.17834F));
                AddDisplayRule("ImpBody", "LowerArmL", new Vector3(0.06972F, 0.11975F, -0.03627F), new Vector3(23.06966F, 280.1783F, 174.6163F), new Vector3(0.12856F, 0.12856F, 0.12856F));
                AddDisplayRule("VultureBody", "LowerArmL", new Vector3(0.99875F, 1.2198F, -0.07211F), new Vector3(346.7694F, 18.99189F, 353.5514F), new Vector3(1.6618F, 1.6618F, 1.6618F));
                AddDisplayRule("RoboBallMiniBody", "ROOT", new Vector3(0.81676F, 0.00111F, -0.02604F), new Vector3(0F, 0F, 0F), new Vector3(0.26827F, 0.26827F, 0.26827F));
                AddDisplayRule("RoboBallMiniBody", "ROOT", new Vector3(-0.84845F, -0.00118F, -0.01821F), new Vector3(0F, 180F, 0F), new Vector3(0.26827F, 0.26827F, 0.26827F));
                AddDisplayRule("MiniMushroomBody", "Head", new Vector3(-0.11137F, -0.70176F, -0.00532F), new Vector3(2.31515F, 179.125F, 332.3143F), new Vector3(0.27659F, 0.27216F, 0.27659F));
                AddDisplayRule("BellBody", "Chain", new Vector3(-1.54561F, 2.68301F, -0.88636F), new Vector3(359.0105F, 327.1622F, 186.3831F), new Vector3(0.51058F, 0.51058F, 0.51058F));
                AddDisplayRule("BeetleGuardBody", "UpperArmL", new Vector3(0.70768F, 0.08173F, -0.65703F), new Vector3(296.3565F, 38.50573F, 359.3476F), new Vector3(0.53579F, 0.53579F, 0.53579F));
                AddDisplayRule("BeetleGuardBody", "UpperArmR", new Vector3(-0.46761F, -0.5243F, -0.96396F), new Vector3(66.01485F, 102.1822F, 316.7295F), new Vector3(0.53579F, 0.53579F, 0.53579F));
                AddDisplayRule("BeetleGuardBody", "Chest", new Vector3(-0.04737F, 0.46906F, -2.27501F), new Vector3(0F, 90F, 338.564F), new Vector3(0.69074F, 0.69074F, 0.69074F));
                AddDisplayRule("BisonBody", "Head", new Vector3(-0.01011F, 0.11968F, 0.81293F), new Vector3(359.9691F, 90.42268F, 96.09669F), new Vector3(0.17392F, 0.17392F, 0.17392F));
                AddDisplayRule("BisonBody", "Chest", new Vector3(-0.72052F, 0.35763F, 0.20302F), new Vector3(289.3813F, 2.77055F, 216.4511F), new Vector3(0.17392F, 0.17392F, 0.17392F));
                AddDisplayRule("BisonBody", "Chest", new Vector3(0.67709F, 0.41002F, 0.23429F), new Vector3(58.92508F, 166.8929F, 200.6828F), new Vector3(0.17392F, 0.17392F, 0.17392F));
                AddDisplayRule("GolemBody", "Chest", new Vector3(0F, 0.23156F, 0.5453F), new Vector3(0F, 270F, 2.80391F), new Vector3(0.42934F, 0.42934F, 0.42934F));
                AddDisplayRule("ParentBody", "Chest", new Vector3(110.5298F, -126.4879F, -0.57111F), new Vector3(0.00012F, 0.00004F, 0.62143F), new Vector3(35.59922F, 35.59922F, 35.59922F));
                AddDisplayRule("ClayBruiserBody", "Muzzle", new Vector3(0.01772F, -0.96556F, -0.22935F), new Vector3(0.2352F, 273.3438F, 358.8563F), new Vector3(0.17276F, 0.17276F, 0.17276F));
                AddDisplayRule("ClayBruiserBody", "UpperArmL", new Vector3(-0.13775F, 0.27827F, -0.23297F), new Vector3(77.3063F, 174.8385F, 82.23293F), new Vector3(0.18463F, 0.18463F, 0.18463F));
                AddDisplayRule("GreaterWispBody", "MaskBase", new Vector3(0.00844F, 0.86168F, 0.53122F), new Vector3(0.85633F, 266.8001F, 29.95748F), new Vector3(0.21107F, 0.21107F, 0.21107F));
                AddDisplayRule("LemurianBruiserBody", "LowerArmL", new Vector3(-0.63009F, 3.8933F, -0.49942F), new Vector3(15.7766F, 355.9879F, 166.7924F), new Vector3(1.62279F, 1.62279F, 1.62279F));
                AddDisplayRule("NullifierBody", "Muzzle", new Vector3(0F, -2.22915F, 0.72636F), new Vector3(0F, 270F, 336.7043F), new Vector3(0.60054F, 0.60054F, 0.60054F));

                AddDisplayRule("BeetleQueen2Body", "Head", new Vector3(0F, 3.39523F, 0.11428F), new Vector3(0F, 90F, 60.1568F), new Vector3(0.84575F, 0.84575F, 0.84575F));
                AddDisplayRule("BeetleQueen2Body", "Butt", new Vector3(0.00001F, -2.28304F, -4.30972F), new Vector3(0F, 90F, 277.3143F), new Vector3(0.99843F, 0.99843F, 0.99843F));
                AddDisplayRule("ClayBossBody", "PotBase", new Vector3(1.47037F, 0.14472F, 1.15155F), new Vector3(356.2741F, 328.1895F, 8.26984F), new Vector3(0.24784F, 0.24784F, 0.24784F));
                AddDisplayRule("ClayBossBody", "PotBase", new Vector3(-1.42564F, 0.14467F, 1.15156F), new Vector3(3.53829F, 214.9017F, 8.46569F), new Vector3(0.24784F, 0.24784F, 0.24784F));
                AddDisplayRule("ClayBossBody", "PotBase", new Vector3(0F, 0.15405F, -1.82541F), new Vector3(0F, 90F, 14.66357F), new Vector3(0.24784F, 0.24784F, 0.24784F));
                AddDisplayRule("TitanBody", "UpperArmL", new Vector3(1.86778F, 0.0468F, -0.43658F), new Vector3(354.5138F, 195.9479F, 182.7553F), new Vector3(2.25074F, 2.25074F, 2.25074F));
                AddDisplayRule("TitanGoldBody", "UpperArmL", new Vector3(1.86778F, 0.0468F, -0.43658F), new Vector3(354.5138F, 195.9479F, 182.7553F), new Vector3(2.25074F, 2.25074F, 2.25074F));
                AddDisplayRule("VagrantBody", "Hull", new Vector3(0F, 0.95729F, 1.16298F), new Vector3(0F, 270F, 29.51815F), new Vector3(0.24784F, 0.24784F, 0.24784F));
                AddDisplayRule("VagrantBody", "Hull", new Vector3(-0.98958F, 0.95724F, -0.58474F), new Vector3(0.00009F, 152.7161F, 29.51817F), new Vector3(0.24784F, 0.24784F, 0.24784F));
                AddDisplayRule("VagrantBody", "Hull", new Vector3(0.98958F, 0.95708F, -0.58468F), new Vector3(0.00012F, 36.37237F, 29.51816F), new Vector3(0.24784F, 0.24784F, 0.24784F));
                string[] worms = new string[]
                {
                    "MagmaWormBody",
                    "ElectricWormBody"
                };
                foreach (string worm in worms)
                {
                    AddDisplayRule(worm, "Head", new Vector3(0.00001F, 0.20321F, 0.35653F), new Vector3(0F, 270F, 338.4637F), new Vector3(0.50219F, 0.50219F, 0.50219F));
                    AddDisplayRule(worm, "Head", new Vector3(-0.00004F, 0.21744F, -1.24892F), new Vector3(0F, 90F, 350.3356F), new Vector3(0.50219F, 0.50219F, 0.50219F));
                    for (var i = 1; i <= 16; i++)
                    {
                        Vector3 scale = Vector3.one * 0.38363F * Mathf.Pow(0.934782609f, i - 1);
                        AddDisplayRule(worm, "Neck" + i.ToString(), new Vector3(0F, 0.67666F + 0.03293F * (i - 1), 0.75189F - 0.02657F * (i - 1)), new Vector3(0F, 270F, 350.3946F), scale);
                        // AddDisplayRule(worm, "Neck" + i.ToString(), new Vector3(1.05941F - 0.02657F * (i - 1), 0.66706F + 0.03293F * (i - 1), -0.30037F), new Vector3(0F, 0F, 350.3946F), scale);
                        AddDisplayRule(worm, "Neck" + i.ToString(), new Vector3(0F, 0.67666F + 0.03293F * (i - 1), -1.298979F + 0.02657F * (i - 1)), new Vector3(0F, 90F, 350.3946F), scale);
                        // AddDisplayRule(worm, "Neck" + i.ToString(), new Vector3(-1.05941F + 0.02657F * (i - 1), 0.66706F + 0.03293F * (i - 1), -0.30037F), new Vector3(0F, 180F, 350.3946F), scale);
                    }
                }
                AddDisplayRule("RoboBallBossBody", "MainEyeMuzzle", new Vector3(-0.54958F, 0.32175F, -0.28987F), new Vector3(297.4961F, 98.16615F, 145.5536F), new Vector3(0.1247F, 0.1247F, 0.1247F));
                AddDisplayRule("RoboBallBossBody", "MainEyeMuzzle", new Vector3(0.56366F, 0.31158F, -0.30237F), new Vector3(63.15763F, 91.27869F, 163.8493F), new Vector3(0.1247F, 0.1247F, 0.1247F));
                AddDisplayRule("RoboBallBossBody", "MainEyeMuzzle", new Vector3(0.0067F, -0.67212F, -0.41237F), new Vector3(0F, 270F, 321.6538F), new Vector3(0.1247F, 0.1247F, 0.1247F));
                AddDisplayRule("SuperRoboBallBossBody", "MainEyeMuzzle", new Vector3(-0.54958F, 0.32175F, -0.28987F), new Vector3(297.4961F, 98.16615F, 145.5536F), new Vector3(0.1247F, 0.1247F, 0.1247F));
                AddDisplayRule("SuperRoboBallBossBody", "MainEyeMuzzle", new Vector3(0.56366F, 0.31158F, -0.30237F), new Vector3(63.15763F, 91.27869F, 163.8493F), new Vector3(0.1247F, 0.1247F, 0.1247F));
                AddDisplayRule("SuperRoboBallBossBody", "MainEyeMuzzle", new Vector3(0.0067F, -0.67212F, -0.41237F), new Vector3(0F, 270F, 321.6538F), new Vector3(0.1247F, 0.1247F, 0.1247F));
                AddDisplayRule("GravekeeperBody", "DanglingRope4L", new Vector3(0.0001F, 1.80608F, 0.00003F), new Vector3(0F, 0F, 180F), new Vector3(0.75697F, 0.75697F, 0.75697F));
                AddDisplayRule("GravekeeperBody", "DanglingRope4R", new Vector3(0.0001F, 1.80608F, 0.00003F), new Vector3(-0.00001F, 180F, 180F), new Vector3(0.75697F, 0.75697F, 0.75697F));
                AddDisplayRule("ImpBossBody", "LowerArmL", new Vector3(0.24444F, 0.45194F, -0.22689F), new Vector3(0F, 270F, 174.7788F), new Vector3(0.66607F, 0.66607F, 0.66607F));
                AddDisplayRule("GrandParentBody", "Head", new Vector3(0F, 8.3197F, 0.00001F), new Vector3(0F, 270F, 0F), new Vector3(1.8948F, 1.8948F, 1.8948F));
                AddDisplayRule("ScavBody", "Stomach", new Vector3(0.23239F, 4.35636F, -8.43252F), new Vector3(9.37492F, 90.28594F, 11.99715F), new Vector3(1.72618F, 1.72618F, 1.72618F));

                foreach (var body in BodyCatalog.allBodyPrefabBodyBodyComponents)
                {
                    var characterModel = body.GetComponentInChildren<CharacterModel>();
                    if (characterModel && characterModel.itemDisplayRuleSet != null)
                    {
                        var iceCrown = characterModel.itemDisplayRuleSet.GetEquipmentDisplayRuleGroup(RoR2Content.Equipment.AffixWhite.equipmentIndex);
                        if (!iceCrown.Equals(DisplayRuleGroup.empty))
                        {
                            var bodyName = BodyCatalog.GetBodyName(body.bodyIndex);
                            foreach (var displayRule in iceCrown.rules)
                            {
                                AddDisplayRule(bodyName, itemDisplayPrefabs["halo"], displayRule.childName, displayRule.localPos, displayRule.localAngles, displayRule.localScale);
                            }
                        }
                    }
                }
            };
            ChildLocatorAdditions.list.Add(new ChildLocatorAdditions.Addition
            {
                modelName = "mdlLemurian",
                transformLocation = "LemurianArm/ROOT/base/stomach/chest/shoulder.l/upper_arm.l/lower_arm.l",
                childName = "LowerArmL"
            });
            ChildLocatorAdditions.list.Add(new ChildLocatorAdditions.Addition
            {
                modelName = "mdlImp",
                transformLocation = "ImpArmature/ROOT/base/stomach/chest/clavicle.l/upper_arm.l/lower_arm.l",
                childName = "LowerArmL"
            });
            ChildLocatorAdditions.list.Add(new ChildLocatorAdditions.Addition
            {
                modelName = "mdlVulture",
                transformLocation = "VultureArmature/ROOT/base/stomach/chest/clavicle.l/upper_arm.l/lower_arm.l",
                childName = "LowerArmL"
            });
            ChildLocatorAdditions.list.Add(new ChildLocatorAdditions.Addition
            {
                modelName = "mdlBeetleGuard",
                transformLocation = "BeetleGuardArmature/ROOT/base/chest",
                childName = "Chest"
            });
            ChildLocatorAdditions.list.Add(new ChildLocatorAdditions.Addition
            {
                modelName = "mdlBeetleGuard",
                transformLocation = "BeetleGuardArmature/ROOT/base/chest/upper_arm.l",
                childName = "UpperArmL"
            });
            ChildLocatorAdditions.list.Add(new ChildLocatorAdditions.Addition
            {
                modelName = "mdlBeetleGuard",
                transformLocation = "BeetleGuardArmature/ROOT/base/chest/upper_arm.r",
                childName = "UpperArmR"
            });
            ChildLocatorAdditions.list.Add(new ChildLocatorAdditions.Addition
            {
                modelName = "mdlBison",
                transformLocation = "BisonArmature/ROOT/Base/stomach/chest",
                childName = "Chest"
            });
            ChildLocatorAdditions.list.Add(new ChildLocatorAdditions.Addition
            {
                modelName = "mdlClayBruiser",
                transformLocation = "ClayBruiserArmature/ROOT/base/stomach/chest/clavicle.l/upper_arm.l",
                childName = "UpperArmL"
            });
            ChildLocatorAdditions.list.Add(new ChildLocatorAdditions.Addition
            {
                modelName = "mdlLemurianBruiser",
                transformLocation = "LemurianBruiserArmature/ROOT/base/stomach/chest/clavicle.l/upper_arm.l/lower_arm.l",
                childName = "LowerArmL"
            });
            ChildLocatorAdditions.list.Add(new ChildLocatorAdditions.Addition
            {
                modelName = "mdlClayBoss",
                transformLocation = "ClayBossArmature/ROOT/PotBase",
                childName = "PotBase"
            });
            ChildLocatorAdditions.list.Add(new ChildLocatorAdditions.Addition
            {
                modelName = "mdlTitan",
                transformLocation = "TitanArmature/ROOT/base/stomach/chest/upper_arm.l",
                childName = "UpperArmL"
            });
            ChildLocatorAdditions.list.Add(new ChildLocatorAdditions.Addition
            {
                modelName = "mdlGravekeeper",
                transformLocation = "GravekeeperArmature/ROOT/base/stomach/chest/neck.1/neck.2/head/danglingrope.1.1.l/danglingrope.1.2.l/danglingrope.1.3.l/danglingrope.1.4.l",
                childName = "DanglingRope4L"
            });
            ChildLocatorAdditions.list.Add(new ChildLocatorAdditions.Addition
            {
                modelName = "mdlGravekeeper",
                transformLocation = "GravekeeperArmature/ROOT/base/stomach/chest/neck.1/neck.2/head/danglingrope.1.1.r/danglingrope.1.2.r/danglingrope.1.3.r/danglingrope.1.4.r",
                childName = "DanglingRope4R"
            });
            ChildLocatorAdditions.list.Add(new ChildLocatorAdditions.Addition
            {
                modelName = "mdlImpBoss",
                transformLocation = "ImpBossArmature/ROOT/base/stomach/chest/clavicle.l/upper_arm.l/lower_arm.l",
                childName = "LowerArmL"
            });

            selfBuffUseEffect = RisingTidesPlugin.AssetBundle.LoadAsset<GameObject>("Assets/Mods/RisingTides/Elites/Barrier/SelfBuffInflictVFX.prefab");
            EffectComponent effectComponent = selfBuffUseEffect.AddComponent<EffectComponent>();
            effectComponent.applyScale = true;
            effectComponent.parentToReferencedTransform = true;
            effectComponent.soundName = "Play_merc_sword_impact";
            VFXAttributes vfxAttributes = selfBuffUseEffect.AddComponent<VFXAttributes>();
            vfxAttributes.vfxIntensity = VFXAttributes.VFXIntensity.Medium;
            vfxAttributes.vfxPriority = VFXAttributes.VFXPriority.Medium;
            RisingTidesContent.Resources.effectPrefabs.Add(selfBuffUseEffect);
        }

        public override bool OnUse(EquipmentSlot equipmentSlot)
        {
            if (equipmentSlot.characterBody)
            {
                var effectData = new EffectData
                {
                    origin = equipmentSlot.characterBody.corePosition,
                    scale = equipmentSlot.characterBody.radius
                };
                effectData.SetHurtBoxReference(equipmentSlot.characterBody.gameObject);
                EffectManager.SpawnEffect(selfBuffUseEffect, effectData, true);

                if (equipmentSlot.characterBody.healthComponent)
                {
                    equipmentSlot.characterBody.healthComponent.AddBarrier(equipmentSlot.characterBody.maxBarrier * barrierRecharge / 100f);
                }

                return true;
            }
            return false;
        }

        public override void AfterContentPackLoaded()
        {
            base.AfterContentPackLoaded();
            equipmentDef.passiveBuffDef = RisingTidesContent.Buffs.RisingTides_AffixBarrier;
        }
    }
}
