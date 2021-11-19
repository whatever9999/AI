using System.Collections.Generic;
using UnityEngine;

#region Enums
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

    BASE
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
#endregion // Enums

#region Actions
public class Actions
{
    // Pick up the nearest Collectable_Type of collectable
    public class PickUpCollectable : Node
    {
        private AgentActions agentActions;
        private AgentData agentData;
        private Sensing sensing;
        private Collectable_Type type;

        public PickUpCollectable(AgentActions agentActions, AgentData agentData, Sensing sensing, Collectable_Type type)
        {
            this.agentActions = agentActions;
            this.agentData = agentData;
            this.sensing = sensing;
            this.type = type;
        }
        public override NodeState Evaluate()
        {
            // What collectables can the agent see
            List<GameObject> collectablesInView = sensing.GetObjectsInView();
            for (int i = 0; i < collectablesInView.Count; i++)
            {
                // Are any of them in reach and of the type we're looking for
                if (    sensing.IsItemInReach(collectablesInView[i])                                                &&
                        type == Collectable_Type.FLAG && collectablesInView[i].name.Equals(agentData.EnemyFlagName) ||
                        type == Collectable_Type.POWER && collectablesInView[i].name.Equals("Power Up")             ||
                        type == Collectable_Type.HEALTH && collectablesInView[i].name.Equals("Health Kit")              )
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
        private AgentData agentData;
        private InventoryController inventoryController;
        private Collectable_Type type;

        public DropCollectable(AgentActions agentActions, AgentData agentData, InventoryController inventoryController, Collectable_Type type)
        {
            this.agentActions = agentActions;
            this.agentData = agentData;
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
                    collectable = inventoryController.GetItem(agentData.EnemyFlagName);
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
        private AgentData agentData;
        private Sensing sensing;
        private GameObject_Type type;

        public MoveToGameObject(AgentActions agentActions, AgentData agentData, Sensing sensing, GameObject_Type type)
        {
            this.agentActions = agentActions;
            this.agentData = agentData;
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
                    target = agentData.GetTeamBlackboard().GetEnemyFlag();
                    break;
                case GameObject_Type.FRIENDLY_FLAG:
                    target = agentData.GetTeamBlackboard().GetFriendlyFlag();
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
                    target = agentData.GetTeamBlackboard().GetMemberWithFlag();
                    break;
                case GameObject_Type.WEAKEST_FRIENDLY:
                    target = agentData.GetTeamBlackboard().GetWeakestMember();
                    break;
                case GameObject_Type.BASE:
                    target = agentData.FriendlyBase;
                    break;
            }
            // If we have the target then move to it
            if (target)
            {
                agentActions.MoveTo(target);
                if (sensing.IsItemInReach(target))
                {
                    return NodeState.SUCCESS;
                }
                return NodeState.RUNNING;
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

    // Attack the nearest enemy
    public class Attack : Node
    {
        Sensing sensing;
        AgentActions agentActions;

        public Attack(Sensing sensing, AgentActions agentActions)
        {
            this.sensing = sensing;
            this.agentActions = agentActions;
        }

        public override NodeState Evaluate()
        {
            GameObject nearestEnemy = sensing.GetNearestEnemyInView();
            if (nearestEnemy && sensing.IsInAttackRange(nearestEnemy))
            {
                agentActions.AttackEnemy(nearestEnemy);
                return NodeState.SUCCESS;
            }
            else
            {
                return NodeState.FAILURE;
            }
        }
    }
}
#endregion // Actions

# region Conditions
public class Conditions
{
    // Check agent health
    public class AgentHeathLessThan : Node
    {
        private AgentData agentData;
        private TeamBlackboard teamBlackboard;
        GameObject_Type type;
        int value;

        public AgentHeathLessThan(AgentData agentData, TeamBlackboard teamBlackboard, GameObject_Type type, int value)
        {
            this.agentData = agentData;
            this.teamBlackboard = teamBlackboard;
            this.type = type;
            this.value = value;
        }
        public override NodeState Evaluate()
        {
            // Check if CurrentHitPoints are less than the given value
            bool result = false;
            switch (type)
            {
                case GameObject_Type.WEAKEST_FRIENDLY:
                    if (teamBlackboard.GetWeakestMember())
                    {
                        result = teamBlackboard.GetWeakestMember().GetComponent<AgentData>().CurrentHitPoints < value;
                    }
                    break;
                case GameObject_Type.THIS_AGENT:
                    result = agentData.CurrentHitPoints < value;
                    break;
            }
            if (result)
            {
                return NodeState.SUCCESS;
            }
            else
            {
                return NodeState.FAILURE;
            }
        }
    }

    // Check if there is a useable of that type on the level
    public class UseableOnLevel : Node
    {
        Useable_Type type;

        public UseableOnLevel(Useable_Type type)
        {
            this.type = type;
        }
        public override NodeState Evaluate()
        {
            // See if we can find a GO of Useable_Type
            bool result = false;
            switch (type)
            {
                case Useable_Type.HEALTH:
                    if (GameObject.Find("Health Pack"))
                    {
                        result = true;
                    }
                    break;
                case Useable_Type.POWER:
                    if (GameObject.Find("Power Up"))
                    {
                        result = true;
                    }
                    break;
            }
            if (result)
            {
                return NodeState.SUCCESS;
            }
            else
            {
                return NodeState.FAILURE;
            }
        }
    }

    // Check if team of type has flag
    public class TeamHasFlag : Node
    {
        TeamBlackboard teamBlackboard; // This should be the blackboard of the team we're checking (can get from WorldBlackboard)

        public TeamHasFlag(TeamBlackboard teamBlackboard)
        {
            this.teamBlackboard = teamBlackboard;
        }
        public override NodeState Evaluate()
        {
            if (teamBlackboard.GetMemberWithFlag())
            {
                return NodeState.SUCCESS;
            }
            else
            {
                return NodeState.FAILURE;
            }
        }
    }

    // Check if agent has collectable of type
    public class GotCollectable : Node
    {
        private AgentData agentData;
        private InventoryController inventoryController;
        Collectable_Type type;

        public GotCollectable(AgentData agentData, InventoryController inventoryController, Collectable_Type type)
        {
            this.agentData = agentData;
            this.inventoryController = inventoryController;
            this.type = type;
        }
        public override NodeState Evaluate()
        {
            bool result = false;
            switch (type)
            {
                case Collectable_Type.FLAG:
                    if (inventoryController.HasItem(agentData.EnemyFlagName))
                    {
                        result = true;
                    }
                    break;
                case Collectable_Type.HEALTH:
                    if (inventoryController.HasItem("Health Pack"))
                    {
                        result = true;
                    }
                    break;
                case Collectable_Type.POWER:
                    if (inventoryController.HasItem("Power Up"))
                    {
                        result = true;
                    }
                    break;
            }
            if (result)
            {
                return NodeState.SUCCESS;
            }
            else
            {
                return NodeState.FAILURE;
            }
        }
    }

    // Check if collectable of type is in pickup range
    public class CollectableInPickupRange : Node
    {
        private AgentData agentData;
        private Sensing sensing;
        Collectable_Type type;

        public CollectableInPickupRange(AgentData agentData, Sensing sensing, Collectable_Type type)
        {
            this.agentData = agentData;
            this.sensing = sensing;
            this.type = type;
        }
        public override NodeState Evaluate()
        {
            bool result = false;
            List<GameObject> collectablesInView = sensing.GetCollectablesInView();
            for (int i = 0; i < collectablesInView.Count; i++)
            {
                // If the item is in reach
                if (sensing.IsItemInReach(collectablesInView[i]))
                {
                    // And matches the type then it's in the pickup range
                    switch (type)
                    {
                        case Collectable_Type.FLAG:
                            if (collectablesInView[i].name.Equals(agentData.EnemyFlagName))
                            {
                                result = true;
                            }
                            break;
                        case Collectable_Type.HEALTH:
                            if (collectablesInView[i].name.Equals("Health Pack"))
                            {
                                result = true;
                            }
                            break;
                        case Collectable_Type.POWER:
                            if (collectablesInView[i].name.Equals("Power"))
                            {
                                result = true;
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
            if (result)
            {
                return NodeState.SUCCESS;
            }
            else
            {
                return NodeState.FAILURE;
            }
        }
    }

    // Check if there is an enemy in attack range
    public class EnemyInAttackRange : Node
    {
        private Sensing sensing;

        public EnemyInAttackRange(Sensing sensing)
        {
            this.sensing = sensing;
        }
        public override NodeState Evaluate()
        {
            if (sensing.IsInAttackRange(sensing.GetNearestEnemyInView()))
            {
                return NodeState.SUCCESS;
            }
            else
            {
                return NodeState.FAILURE;
            }
        }
    }

    // Check if a friendly team member is pursuing the flag
    public class TeamMemberPursuingFlag : Node
    {
        TeamBlackboard teamBlackboard;

        public TeamMemberPursuingFlag(TeamBlackboard teamBlackboard)
        {
            this.teamBlackboard = teamBlackboard;
        }
        public override NodeState Evaluate()
        {
            if (teamBlackboard.GetMembersPursuingFlag().Count > 0)
            {
                return NodeState.SUCCESS;
            }
            else
            {
                return NodeState.FAILURE;
            }
        }
    }
}
#endregion // Conditions

#region Decorators
public class Decorators
{
    // Reverse the result of a condition
    public class Inverter : Node
    {
        private Node node;

        public Inverter(Node node)
        {
            this.node = node;
        }
        public override NodeState Evaluate()
        {
            switch (node.Evaluate())
            {
                case NodeState.RUNNING:
                    nodeState = NodeState.RUNNING;
                    break;
                case NodeState.SUCCESS:
                    nodeState = NodeState.FAILURE;
                    break;
                case NodeState.FAILURE:
                    nodeState = NodeState.SUCCESS;
                    break;
            }
            return nodeState;
        }
    }
}
#endregion // Decorators