using MysticsRisky2Utils;
using MysticsRisky2Utils.BaseAssetTypes;
using R2API;
using RoR2;
using RoR2.Orbs;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace RisingTides.Buffs
{
    public class AffixMirror : BaseBuff
	{
		public static ConfigOptions.ConfigurableValue<int> cloneCount = ConfigOptions.ConfigurableValue.CreateInt(
			RisingTidesPlugin.PluginGUID, RisingTidesPlugin.PluginName, RisingTidesPlugin.config,
			"Elites: Phenakite", "Clone Count",
			3, 0, 10,
			description: "How many fake clones should this elite spawn with?",
			useDefaultValueConfigEntry: RisingTidesPlugin.ignoreBalanceChanges.bepinexConfigEntry
		);
		public static ConfigOptions.ConfigurableValue<bool> obscureCloneNames = ConfigOptions.ConfigurableValue.CreateBool(
			RisingTidesPlugin.PluginGUID, RisingTidesPlugin.PluginName, RisingTidesPlugin.config,
			"Elites: Phenakite", "Obscure Clone Names",
			false,
			description: "Should this elite's monster/player name be obscured when pinged or shown in UI?",
			useDefaultValueConfigEntry: RisingTidesPlugin.ignoreBalanceChanges.bepinexConfigEntry
		);
		public static ConfigOptions.ConfigurableValue<float> itemStealChance = ConfigOptions.ConfigurableValue.CreateFloat(
			RisingTidesPlugin.PluginGUID, RisingTidesPlugin.PluginName, RisingTidesPlugin.config,
			"Elites: Phenakite", "Item Steal Chance",
			100f, 0f, 100f,
			description: "What should be the chance of this elite stealing the target's items on hit? (in %)",
			useDefaultValueConfigEntry: RisingTidesPlugin.ignoreBalanceChanges.bepinexConfigEntry
		);
		public static ConfigOptions.ConfigurableValue<int> itemStealCount = ConfigOptions.ConfigurableValue.CreateInt(
			RisingTidesPlugin.PluginGUID, RisingTidesPlugin.PluginName, RisingTidesPlugin.config,
			"Elites: Phenakite", "Item Steal Count",
			1, 0, 100,
			description: "How many of the target's items should this elite steal on hit?",
			useDefaultValueConfigEntry: RisingTidesPlugin.ignoreBalanceChanges.bepinexConfigEntry
		);

		public DeployableSlot cloneDeployableSlot;

        public override void OnPluginAwake()
        {
            base.OnPluginAwake();
			cloneDeployableSlot = DeployableAPI.RegisterDeployableSlot(GetDeployableSameSlotLimit);
        }

		public override void OnLoad()
		{
			base.OnLoad();
			buffDef.name = "RisingTides_AffixMirror";
			buffDef.iconSprite = RisingTidesPlugin.AssetBundle.LoadAsset<Sprite>("Assets/Mods/RisingTides/Elites/Mirror/texAffixMirrorIcon.png");

			Overlays.CreateOverlay(RisingTidesPlugin.AssetBundle.LoadAsset<Material>("Assets/Mods/RisingTides/Elites/Mirror/matAffixMirrorOverlay.mat"), (model) =>
			{
				return model.body && model.body.HasBuff(buffDef);
			});

			On.RoR2.CharacterBody.OnBuffFirstStackGained += CharacterBody_OnBuffFirstStackGained;
            GlobalEventManager.onCharacterDeathGlobal += GlobalEventManager_onCharacterDeathGlobal;
            On.RoR2.CharacterBody.GetUserName += CharacterBody_GetUserName;
			GenericGameEvents.OnHitEnemy += GenericGameEvents_OnHitEnemy;

			if (RisingTidesPlugin.mysticsItemsCompatibility)
			{
				RoR2Application.onLoad += () =>
				{
					MysticsItems.OtherModCompat.ElitePotion_AddSpreadEffect(buffDef, vfx: Addressables.LoadAssetAsync<GameObject>("RoR2/Base/StunChanceOnHit/ImpactStunGrenade.prefab").WaitForCompletion(), damageType: DamageType.Stun1s);
				};
			}
		}

        public int GetDeployableSameSlotLimit(CharacterMaster self, int deployableCountMultiplier)
		{
			return cloneCount;
		}

		private void CharacterBody_OnBuffFirstStackGained(On.RoR2.CharacterBody.orig_OnBuffFirstStackGained orig, CharacterBody self, BuffDef buffDef)
		{
			orig(self, buffDef);
			if (this.buffDef == buffDef && NetworkServer.active && self.master && self.inventory && self.inventory.GetItemCount(RisingTidesContent.Items.RisingTides_MirrorClone) <= 0)
			{
				var calculatedCloneCount = self.master.GetDeployableSameSlotLimit(cloneDeployableSlot);
				for (var i = 0; i < calculatedCloneCount; i++)
				{
					if (self.master.GetDeployableCount(cloneDeployableSlot) >= calculatedCloneCount) break;

					var masterCopySpawnCard = MasterCopySpawnCard.FromMaster(self.master, true, true);
					if (!masterCopySpawnCard) return;
					masterCopySpawnCard.GiveItem(RisingTidesContent.Items.RisingTides_MirrorClone);

					var directorSpawnRequest = new DirectorSpawnRequest(masterCopySpawnCard, new DirectorPlacementRule
					{
						placementMode = DirectorPlacementRule.PlacementMode.Approximate,
						minDistance = 2f * self.bestFitRadius,
						maxDistance = 12f * self.bestFitRadius,
						position = self.corePosition
					}, RoR2Application.rng);
					directorSpawnRequest.summonerBodyObject = self.gameObject;
					var spawnedGameObject = DirectorCore.instance.TrySpawnObject(directorSpawnRequest);
					if (spawnedGameObject)
					{
						var spawnedMaster = spawnedGameObject.GetComponent<CharacterMaster>();
						var deployable = spawnedGameObject.AddComponent<Deployable>();
						deployable.onUndeploy = new UnityEngine.Events.UnityEvent();
						deployable.onUndeploy.AddListener(() => spawnedMaster.TrueKill());
						self.master.AddDeployable(deployable, cloneDeployableSlot);
					}
				}
			}
		}

		private void GlobalEventManager_onCharacterDeathGlobal(DamageReport damageReport)
		{
			if (damageReport.victimMaster && damageReport.victimMaster.deployablesList != null)
			{
				for (var i = damageReport.victimMaster.deployablesList.Count - 1; i >= 0; i--)
				{
					var deployable = damageReport.victimMaster.deployablesList[i];
					if (deployable.slot == cloneDeployableSlot)
					{
						var deployableComponent = deployable.deployable;
						damageReport.victimMaster.deployablesList.RemoveAt(i);
						deployableComponent.ownerMaster = null;
						deployableComponent.onUndeploy.Invoke();
					}
				}
			}
		}

		private string CharacterBody_GetUserName(On.RoR2.CharacterBody.orig_GetUserName orig, CharacterBody self)
		{
			if (self.HasBuff(buffDef) && obscureCloneNames) return Language.GetString("RISINGTIDES_AFFIXMIRROR_OBSCURED_NAME");
			return orig(self);
		}

		private void GenericGameEvents_OnHitEnemy(DamageInfo damageInfo, MysticsRisky2UtilsPlugin.GenericCharacterInfo attackerInfo, MysticsRisky2UtilsPlugin.GenericCharacterInfo victimInfo)
        {
            if (!damageInfo.rejected && damageInfo.procCoefficient > 0f && attackerInfo.body && attackerInfo.body.HasBuff(buffDef) && attackerInfo.inventory && attackerInfo.inventory.GetItemCount(RisingTidesContent.Items.RisingTides_MirrorClone) <= 0 && victimInfo.body && victimInfo.inventory)
            {
				for (var i = 0; i < itemStealCount; i++)
                {
					var allVictimItems = new List<ItemIndex>();
					foreach (var itemIndex in victimInfo.inventory.itemAcquisitionOrder)
                    {
						var itemDef = ItemCatalog.GetItemDef(itemIndex);
						if (itemDef && !itemDef.hidden && itemDef.canRemove && itemDef.DoesNotContainTag(ItemTag.CannotSteal))
                        {
							allVictimItems.Add(itemIndex);
                        }
                    }

					if (allVictimItems.Count <= 0)
						break;

					if (Util.CheckRoll(itemStealChance * damageInfo.procCoefficient, attackerInfo.master))
                    {
						var randomItemIndex = RoR2Application.rng.NextElementUniform(allVictimItems);
						var itemCount = victimInfo.inventory.GetItemCount(randomItemIndex);
						if (itemCount > 0)
						{
							victimInfo.inventory.RemoveItem(randomItemIndex, 1);
							ItemTransferOrb.DispatchItemTransferOrb(victimInfo.body.corePosition, attackerInfo.inventory, randomItemIndex, 1);
						}
					}
                }
			}
        }

		public override void AfterContentPackLoaded()
		{
			base.AfterContentPackLoaded();
			buffDef.eliteDef = RisingTidesContent.Elites.RisingTides_Mirror;
		}
	}
}
