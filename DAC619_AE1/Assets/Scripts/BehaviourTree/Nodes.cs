using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Collectable_Type
{
    FLAG,
    HEALTH,
    POWER,
}
public enum GameObject_Type
{
    ENEMY_FLAG,
    FRIENDLY_FLAG,

    HEALTH_PACK,
    POWER_PACK,

    NEAREST_ENEMY,
    FRIENDLY_WITH_FLAG,
    WEAKEST_FRIENDLY,
    THIS_AGENT,
}
public enum Useable_Type
{
    HEALTH,
    POWER,
}
public enum Team_Type
{
    ENEMY,
    FRIENDLY,
}

public class Actions
{
    // Pick up the nearest Collectable_Type of collectable
    public class PickUpCollectable : Node
    {
        private AgentActions agentActions;
        private Sensing sensing;
        private Collectable_Type type;

        public PickUpCollectable(AgentActions agentActions, Sensing sensing, Collectable_Type type)
        {
            this.agentActions = agentActions;
            this.sensing = sensing;
            this.type = type;
        }
        public override NodeState Evaluate()
        {
            // What collectables can the agent see
            List<GameObject> collectablesInView = sensing.GetCollectablesInView();
            for (int i = 0; i < collectablesInView.Count; i++)
            {
                // Are any of them in reach and of the type we're looking for
                if (    sensing.IsItemInReach(collectablesInView[i])                                        &&
                        type == Collectable_Type.FLAG && collectablesInView[i].name.Equals("Flag")          ||
                        type == Collectable_Type.POWER && collectablesInView[i].name.Equals("Power Up")     ||
                        type == Collectable_Type.HEALTH && collectablesInView[i].name.Equals("Health Kit")     )
                {
                    // If yes then pick it up
                    agentActions.CollectItem(collectablesInView[i]);
                    return NodeState.SUCCESS;
                }
            }
            // We have failed to find a collectable
            return NodeState.FAILURE;
        }
    }

    // Drop the first type of that collectable held
    public class DropCollectable : Node
    {
        private AgentActions agentActions;
        private InventoryController inventoryController;
        private Collectable_Type type;

        public DropCollectable(AgentActions agentActions, AgentData agentData, InventoryController inventoryController, Collectable_Type type)
        {
            this.agentActions = agentActions;
            this.inventoryController = inventoryController;
            this.type = type;
        }
        public override NodeState Evaluate()
        {
            // Get the item of Collectable_Type from the inventory
            GameObject collectable = null;
            switch (type)
            {
                case Collectable_Type.FLAG:
                    collectable = inventoryController.GetItem("Flag");
                    break;
                case Collectable_Type.HEALTH:
                    collectable = inventoryController.GetItem("Health Pack");
                    break;
                case Collectable_Type.POWER:
                    collectable = inventoryController.GetItem("Power Up");
                    break;
            }
            // Drop the item if we have it
            if (collectable)
            {
                agentActions.DropItem(collectable);
                return NodeState.SUCCESS;
            }
            return NodeState.FAILURE;
        }
    }

    // Move to the nearest of that type of game object
    public class MoveToGameObject : Node
    {
        private AgentActions agentActions;
        private TeamBlackboard teamBlackboard;
        private Sensing sensing;
        private GameObject_Type type;

        public MoveToGameObject(AgentActions agentActions, TeamBlackboard teamBlackboard, Sensing sensing, GameObject_Type type)
        {
            this.agentActions = agentActions;
            this.teamBlackboard = teamBlackboard;
            this.sensing = sensing;
            this.type = type;
        }
        public override NodeState Evaluate()
        {
            // Get the target according to GameObject_Type
            GameObject target = null;
            switch (type)
            {
                case GameObject_Type.ENEMY_FLAG:
                    target = teamBlackboard.GetEnemyFlag();
                    break;
                case GameObject_Type.FRIENDLY_FLAG:
                    target = teamBlackboard.GetFriendlyFlag();
                    break;
                case GameObject_Type.HEALTH_PACK:
                    target = GetCollectableTarget("Health Kit");
                    break;
                case GameObject_Type.POWER_PACK:
                    target = GetCollectableTarget("Power Up");
                    break;
                case GameObject_Type.NEAREST_ENEMY:
                    target = sensing.GetNearestEnemyInView();
                    break;
                case GameObject_Type.FRIENDLY_WITH_FLAG:
                    target = teamBlackboard.GetMemberWithFlag();
                    break;
                case GameObject_Type.WEAKEST_FRIENDLY:
                    target = teamBlackboard.GetWeakestMember();
                    break;
            }
            // If we have the target then move to it
            if (target)
            {
                agentActions.MoveTo(target);
                return NodeState.SUCCESS;
            }
            else
            {
                return NodeState.FAILURE;
            }
        }
        // Get a collectable with the name 'name' in the agent's view
        private GameObject GetCollectableTarget(string name)
        {
            List<GameObject> collectablesInView = sensing.GetCollectablesInView();
            for (int i = 0; i < collectablesInView.Count; i++)
            {
                if (collectablesInView[i].name.Equals(name))
                {
                    return collectablesInView[i];
                }
            }
            return null;
        }
    }

    // Flee the nearest enemy
    public class Flee : Node
    {
        private AgentActions agentActions;
        private Sensing sensing;

        public Flee(AgentActions agentActions, Sensing sensing)
        {
            this.agentActions = agentActions;
            this.sensing = sensing;
            
        }
        public override NodeState Evaluate()
        {
            GameObject nearestEnemy = sensing.GetNearestEnemyInView();
            if (nearestEnemy)
            {
                agentActions.Flee(nearestEnemy);
                return NodeState.SUCCESS;
            }
            else
            {
                return NodeState.FAILURE;
            }
        }
    }

    // Use the first type of that useable in inventory
    public class UseUseable : Node
    {
        private AgentActions agentActions;
        private AgentData agentData;
        private InventoryController inventoryController;
        private Useable_Type type;

        public UseUseable(AgentActions agentActions, AgentData agentData, InventoryController inventoryController, Useable_Type type)
        {
            this.agentActions = agentActions;
            this.agentData = agentData;
            this.inventoryController = inventoryController;
            this.type = type;
        }
        public override NodeState Evaluate()
        {
            // Get an item of Useable_Type from our inventory
            GameObject item = null;
            switch (type)
            {
                case Useable_Type.HEALTH:
                    inventoryController.GetItem("Health Pack");
                    break;
                case Useable_Type.POWER:
                    inventoryController.GetItem("Power Up");
                    break;
            }

            // Use the item if we have it
            if (item)
            {
                agentActions.UseItem(item);
                return NodeState.SUCCESS;
            }
            else
            {
                return NodeState.FAILURE;
            }
        }
    }
}

public class Conditions
{
    public class AgentHeathLessThan : Node
    {
        private AgentData agentData;
        private TeamBlackboard teamBlackboard;
        GameObject_Type type;

        public AgentHeathLessThan(AgentData agentData, TeamBlackboard teamBlackboard, GameObject_Type type)
        {
            this.agentData = agentData;
            this.teamBlackboard = teamBlackboard;
            this.type = type;
        }
        public override NodeState Evaluate()
        {
            // TODO: Check agent health
            throw new System.NotImplementedException();
        }
    }

    public class UseableOnLevel : Node
    {
        WorldBlackboard worldBlackboard;
        Useable_Type type;

        public UseableOnLevel(WorldBlackboard worldBlackboard, Useable_Type type)
        {
            this.worldBlackboard = worldBlackboard;
            this.type = type;
        }
        public override NodeState Evaluate()
        {
            // TODO: Check if there is a useable of that type on the level
            throw new System.NotImplementedException();
        }
    }

    public class TeamHasFlag : Node
    {
        WorldBlackboard worldBlackboard;
        Team_Type type;

        public TeamHasFlag(WorldBlackboard worldBlackboard, Team_Type type)
        {
            this.worldBlackboard = worldBlackboard;
            this.type = type;
        }
        public override NodeState Evaluate()
        {
            // TODO: Check if team of type has flag
            throw new System.NotImplementedException();
        }
    }

    public class GotCollectable : Node
    {
        private AgentData agentData;
        Collectable_Type type;

        public GotCollectable(AgentData agentData, Collectable_Type type)
        {
            this.agentData = agentData;
            this.type = type;
        }
        public override NodeState Evaluate()
        {
            // TODO: Check if agent has collectable of type
            throw new System.NotImplementedException();
        }
    }

    public class CollectableInPickupRange : Node
    {
        private AgentData agentData;
        Collectable_Type type;

        public CollectableInPickupRange(AgentData agentData, Collectable_Type type)
        {
            this.agentData = agentData;
            this.type = type;
        }
        public override NodeState Evaluate()
        {
            // TODO: Check if collectable of type is in pickup range
            throw new System.NotImplementedException();
        }
    }

    public class EnemyInAttackRange : Node
    {
        private AgentData agentData;

        public EnemyInAttackRange(AgentData agentData)
        {
            this.agentData = agentData;
        }
        public override NodeState Evaluate()
        {
            // TODO: Check if there is an enemy in attack range
            throw new System.NotImplementedException();
        }
    }

    public class TeamMemberPursuingFlag : Node
    {
        TeamBlackboard teamBlackboard;

        public TeamMemberPursuingFlag(TeamBlackboard teamBlackboard)
        {
            this.teamBlackboard = teamBlackboard;
        }
        public override NodeState Evaluate()
        {
            // TODO: Check if a friendly team member is pursuing the flag
            throw new System.NotImplementedException();
        }
    }
}