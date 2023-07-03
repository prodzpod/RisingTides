using RoR2;
using RoR2.CharacterAI;
using MysticsRisky2Utils;
using UnityEngine;
using MysticsRisky2Utils.BaseAssetTypes;

namespace RisingTides.CharacterMasters
{
    public class MushSupporter : BaseCharacterMaster
    {
        public override void OnPluginAwake()
        {
            base.OnPluginAwake();
            prefab = Utils.CreateBlankPrefab("RisingTides_MushSupporterMaster", true);
        }

        public override void OnLoad()
        {
            base.OnLoad();

            masterName = "RisingTides_MushSupporter";
            Prepare();

            spawnCard.nodeGraphType = RoR2.Navigation.MapNodeGroup.GraphType.Ground;

            EntityStateMachine aiStateMachine = prefab.AddComponent<EntityStateMachine>();
            aiStateMachine.customName = "AI";
            aiStateMachine.initialStateType = aiStateMachine.mainStateType = new EntityStates.SerializableEntityStateType(typeof(EntityStates.AI.Walker.Wander));

            BaseAI ai = prefab.AddComponent<BaseAI>();
            ai.fullVision = true;
            ai.neverRetaliateFriendlies = true;
            ai.enemyAttentionDuration = 5f;
            ai.desiredSpawnNodeGraphType = RoR2.Navigation.MapNodeGroup.GraphType.Ground;
            ai.stateMachine = aiStateMachine;
            ai.scanState = new EntityStates.SerializableEntityStateType(typeof(EntityStates.AI.Walker.Guard));
            ai.enemyAttention = 0f;
            ai.aimVectorDampTime = 0.1f;
            ai.aimVectorMaxSpeed = 60f;

            var aiSkillDriver = prefab.AddComponent<AISkillDriver>();
            aiSkillDriver.customName = "Burrow";
            aiSkillDriver.skillSlot = SkillSlot.Utility;
            aiSkillDriver.requireSkillReady = true;
            aiSkillDriver.minDistance = 40f;
            aiSkillDriver.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            aiSkillDriver.movementType = AISkillDriver.MovementType.Stop;
            aiSkillDriver.moveInputScale = 1f;
            aiSkillDriver.aimType = AISkillDriver.AimType.AtCurrentEnemy;
            aiSkillDriver.buttonPressType = AISkillDriver.ButtonPressType.Hold;

            aiSkillDriver = prefab.AddComponent<AISkillDriver>();
            aiSkillDriver.customName = "FireSpores";
            aiSkillDriver.skillSlot = SkillSlot.Primary;
            aiSkillDriver.requireSkillReady = false;
            aiSkillDriver.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            aiSkillDriver.movementType = AISkillDriver.MovementType.Stop;
            aiSkillDriver.moveInputScale = 1f;
            aiSkillDriver.aimType = AISkillDriver.AimType.AtCurrentEnemy;
            aiSkillDriver.buttonPressType = AISkillDriver.ButtonPressType.Hold;

            spawnCard.directorCreditCost = 70;
            spawnCard.forbiddenAsBoss = true;
            spawnCard.nodeGraphType = RoR2.Navigation.MapNodeGroup.GraphType.Ground;
            spawnCard.occupyPosition = true;
            spawnCard.hullSize = HullClassification.BeetleQueen;

            AddDirectorCardTo("rootjungle", "Minibosses", new DirectorCard
            {
                selectionWeight = 1,
                spawnCard = spawnCard,
                spawnDistance = DirectorCore.MonsterSpawnDistance.Standard,
                preventOverhead = true
            });
            AddDirectorCardTo("skymeadow", "Minibosses", new DirectorCard
            {
                selectionWeight = 1,
                spawnCard = spawnCard,
                spawnDistance = DirectorCore.MonsterSpawnDistance.Standard,
                preventOverhead = true
            });
        }
    }
}
