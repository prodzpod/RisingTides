using MysticsRisky2Utils;
using MysticsRisky2Utils.BaseAssetTypes;
using R2API;
using RoR2;
using UnityEngine;

namespace RisingTides.Items
{
    public class MirrorClone : BaseItem
    {
        public override void OnLoad()
        {
            base.OnLoad();
            itemDef.name = "RisingTides_MirrorClone";
            SetItemTierWhenAvailable(ItemTier.NoTier);
            itemDef.tags = new ItemTag[]
            {
                ItemTag.WorldUnique,
                ItemTag.AIBlacklist,
                ItemTag.BrotherBlacklist
            };
            itemDef.hidden = true;
            itemDef.canRemove = false;

            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
            GenericGameEvents.BeforeTakeDamage += GenericGameEvents_BeforeTakeDamage;
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender.inventory && sender.inventory.GetItemCount(itemDef) > 0 && sender.healthComponent)
            {
                sender.healthComponent.globalDeathEventChanceCoefficient = 0f;
            }
        }

        private void GenericGameEvents_BeforeTakeDamage(DamageInfo damageInfo, MysticsRisky2UtilsPlugin.GenericCharacterInfo attackerInfo, MysticsRisky2UtilsPlugin.GenericCharacterInfo victimInfo)
        {
            if (attackerInfo.inventory && attackerInfo.inventory.GetItemCount(itemDef) > 0)
            {
                damageInfo.damage = Mathf.Min(0.01f, damageInfo.procCoefficient);
                damageInfo.procCoefficient = Mathf.Min(0.01f, damageInfo.procCoefficient);
            }
        }
    }
}
