using System.Collections.Generic;
using UnityEngine;

#region Enums
public enum Collectable_Type
{
    ENEMY_FLAG,
    FRIENDLY_FLAG,
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

    BASE,
    NOT_IN_BASE,
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
#if DEBUG
            Debug.Log("Pick Up Collectable");
#endif //Debug
            // What collectables can the agent see
            List<GameObject> collectablesInView = sensing.GetObjectsInView();
            for (int i = 0; i < collectablesInView.Count; i++)
            {
                // Are any of them in reach and of the type we're looking for
                if (    sensing.IsItemInReach(collectablesInView[i])                                                            &&
                        type == Collectable_Type.FRIENDLY_FLAG && collectablesInView[i].name.Equals(agentData.FriendlyFlagName) ||
                        type == Collectable_Type.ENEMY_FLAG && collectablesInView[i].name.Equals(agentData.EnemyFlagName)       ||
                        type == Collectable_Type.POWER && collectablesInView[i].name.Equals("Power Up")                         ||
                        type == Collectable_Type.HEALTH && collectablesInView[i].name.Equals("Health Kit")                          )
                {
                    // If yes then pick it up
                    agentActions.CollectItem(collectablesInView[i]);
                    return NodeState.SUCCESS;
                }
            }
            // We have failed to find a collectable
#if DEBUG
            Debug.LogError("Failed to find a collectable");
#endif //Debug
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
#if DEBUG
            Debug.Log("Drop Collectable");
#endif //DEBUG
            // Get the item of Collectable_Type from the inventory
            GameObject collectable = null;
            switch (type)
            {
                case Collectable_Type.FRIENDLY_FLAG:
                    collectable = inventoryController.GetItem(agentData.FriendlyFlagName);
                    break;
                case Collectable_Type.ENEMY_FLAG:
                    collectable = inventoryController.GetItem(agentData.EnemyFlagName);
                    break;
                case Collectable_Type.HEALTH:
                    collectable = inventoryController.GetItem("Health Pack");
                    break;
                case Collectable_Type.POWER:
                    collectable = inventoryController.GetItem("Power Up");
                    break;
            }
            // Drop the item
            if (collectable)
            {
                agentActions.DropItem(collectable);
                return NodeState.SUCCESS;
            }
#if DEBUG
            Debug.LogWarning("No collectable found to drop (probably just due to agent death");
#endif //DEBUG
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
#if DEBUG
            Debug.Log("Move To Game Object");
#endif //DEBUG
            // Get the target according to GameObject_Type
            GameObject target = null;
            bool check_if_in_reach = false;
            bool check_if_in_attack_range = false;
            switch (type)
            {
                case GameObject_Type.ENEMY_FLAG:
                    target = agentData.GetTeamBlackboard().GetEnemyFlag();
                    check_if_in_reach = true;
                    break;
                case GameObject_Type.FRIENDLY_FLAG:
                    target = agentData.GetTeamBlackboard().GetFriendlyFlag();
                    check_if_in_attack_range = true;
                    break;
                case GameObject_Type.HEALTH_PACK:
                    target = GetCollectableTarget("Health Kit");
                    check_if_in_reach = true;
                    break;
                case GameObject_Type.POWER_PACK:
                    target = GetCollectableTarget("Power Up");
                    check_if_in_reach = true;
                    break;
                case GameObject_Type.NEAREST_ENEMY:
                    target = sensing.GetNearestEnemyInView();
                    check_if_in_attack_range = true;
                    break;
                case GameObject_Type.FRIENDLY_WITH_FLAG:
                    target = agentData.GetTeamBlackboard().GetMemberWithEnemyFlag();
                    check_if_in_attack_range = true;
                    break;
                case GameObject_Type.WEAKEST_FRIENDLY:
                    target = agentData.GetTeamBlackboard().GetWeakestMember();
                    check_if_in_reach = true;
                    break;
                case GameObject_Type.BASE:
                    target = agentData.FriendlyBase;
                    check_if_in_attack_range = true;
                    break;
                case GameObject_Type.NOT_IN_BASE:
                    check_if_in_reach = true;
                    break;
            }

            // Not in base is for moving the flag to a random location outside of the base (it will succeed due to the check before it)
            if (type == GameObject_Type.NOT_IN_BASE)
            {
                agentActions.MoveToRandomLocation();
                return NodeState.RUNNING;
            }

            // Move to the target
            if (check_if_in_reach && sensing.IsItemInReach(target) ||
                check_if_in_attack_range && sensing.IsInAttackRange(target))
            {
                return NodeState.SUCCESS;
            }
            agentActions.MoveTo(target);
            return NodeState.RUNNING;
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
#if DEBUG
            Debug.Log("Flee");
#endif //DEBUG
            GameObject nearestEnemy = sensing.GetNearestEnemyInView();
            if (!sensing.IsInAttackRange(nearestEnemy))
            {
                return NodeState.SUCCESS;
            }
            agentActions.Flee(nearestEnemy);
            return NodeState.RUNNING;
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
#if DEBUG
                    Debug.Log("Use health kit");
#endif //DEBUG
                    item = inventoryController.GetItem("Health Kit");
                    break;
                case Useable_Type.POWER:
#if DEBUG
                    Debug.Log("Use power up");
#endif //DEBUG
                    item = inventoryController.GetItem("Power Up");
                    break;
            }

            // Use the item
            agentActions.UseItem(item);
            return NodeState.SUCCESS;
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
#if DEBUG
            Debug.Log("Attack");
#endif //DEBUG
            GameObject nearestEnemy = sensing.GetNearestEnemyInView();
            agentActions.AttackEnemy(nearestEnemy);
            if (!sensing.IsInAttackRange(sensing.GetNearestEnemyInView()))
            {
                return NodeState.SUCCESS;
            }
            return NodeState.RUNNING;
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
#if DEBUG
                    Debug.Log("Is weakest member health below " + value + "?");
#endif //DEBUG
                    if (teamBlackboard.GetWeakestMember())
                    {
                        result = teamBlackboard.GetWeakestMember().GetComponent<AgentData>().CurrentHitPoints < value;
                    }
                    break;
                case GameObject_Type.THIS_AGENT:
                    result = agentData.CurrentHitPoints < value;
#if DEBUG
                    Debug.Log("Is this agent's health below " + value + "?");
#endif //DEBUG
                    break;
            }
            if (result)
            {
#if DEBUG
                Debug.Log("YES");
#endif //DEBUG
                return NodeState.SUCCESS;
            }
            else
            {
#if DEBUG
                Debug.Log("NO");
#endif //DEBUG
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
            // See if we can find a GO of Useable_Type (only counts if doesn't have a parent as that means it's already held)
            bool result = false;
            switch (type)
            {
                case Useable_Type.HEALTH:
#if DEBUG
                    Debug.Log("Is there a health pack on the level?");
#endif //DEBUG
                    HealthKit[] healthPacks = GameObject.FindObjectsOfType<HealthKit>();
                    for (int i = 0; i < healthPacks.Length; i++)
                    {
                        if (!healthPacks[i].transform.parent)
                        {
                            result = true;
                            break;
                        }
                    }
                    break;
                case Useable_Type.POWER:
#if DEBUG
                    Debug.Log("Is there a power pack on the level?");
#endif //DEBUG
                    PowerUp[] powerPacks = GameObject.FindObjectsOfType<PowerUp>();
                    for (int i = 0; i < powerPacks.Length; i++)
                    {
                        if (!powerPacks[i].transform.parent)
                        {
                            result = true;
                            break;
                        }
                    }
                    break;
            }
            if (result)
            {
#if DEBUG
                Debug.Log("YES");
#endif //DEBUG
                return NodeState.SUCCESS;
            }
            else
            {
#if DEBUG
                Debug.Log("NO");
#endif //DEBUG
                return NodeState.FAILURE;
            }
        }
    }

    // Check if team of type has flag
    public class TeamHasFlag : Node
    {
        TeamBlackboard teamBlackboard; // This should be the blackboard of the team we're checking (can get from WorldBlackboard)
        Team_Type type;

        public TeamHasFlag(TeamBlackboard teamBlackboard, Team_Type type)
        {
            this.teamBlackboard = teamBlackboard;
            this.type = type;
        }
        public override NodeState Evaluate()
        {
            bool got_flag = false;

            switch (type)
            {
                case Team_Type.ENEMY:
#if DEBUG
                    Debug.Log("Does the " + teamBlackboard.name + " team have the enemy flag?");
#endif //DEBUG
                    if (teamBlackboard.GetMemberWithEnemyFlag()) got_flag = true;
                    break;
                case Team_Type.FRIENDLY:
#if DEBUG
                    Debug.Log("Does the " + teamBlackboard.name + " team have the friendly flag?");
#endif //DEBUG
                    if (teamBlackboard.GetMemberWithFriendlyFlag()) got_flag = true;
                    break;
            }

            if (got_flag)
            {
#if DEBUG
                Debug.Log("YES");
#endif //DEBUG
                return NodeState.SUCCESS;
            }
            else
            {
#if DEBUG
                Debug.Log("NO");
#endif //DEBUG
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
                case Collectable_Type.ENEMY_FLAG:
#if DEBUG
                    Debug.Log("Do we have the enemy flag?");
#endif //DEBUG
                    if (inventoryController.HasItem(agentData.EnemyFlagName))
                    {
                        result = true;
                    }
                    break;
                case Collectable_Type.FRIENDLY_FLAG:
#if DEBUG
                    Debug.Log("Do we have the friendly flag?");
#endif //DEBUG
                    if (inventoryController.HasItem(agentData.FriendlyFlagName))
                    {
                        result = true;
                    }
                    break;
                case Collectable_Type.HEALTH:
#if DEBUG
                    Debug.Log("Do we have a health pack?");
#endif //DEBUG
                    if (inventoryController.HasItem("Health Kit"))
                    {
                        result = true;
                    }
                    break;
                case Collectable_Type.POWER:
#if DEBUG
                    Debug.Log("Do we have a power pack");
#endif //DEBUG
                    if (inventoryController.HasItem("Power Up"))
                    {
                        result = true;
                    }
                    break;
            }
            if (result)
            {
#if DEBUG
                Debug.Log("YES");
#endif //DEBUG
                return NodeState.SUCCESS;
            }
            else
            {
#if DEBUG
                Debug.Log("NO");
#endif //DEBUG
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
#if DEBUG
            Debug.Log("Collectable In Pickup Range?");
#endif //DEBUG
            bool result = false;
            List<GameObject> objectsInView = sensing.GetObjectsInView();
            for (int i = 0; i < objectsInView.Count; i++)
            {
                // If the item is in reach
                if (sensing.IsItemInReach(objectsInView[i]))
                {
                    // And matches the type then it's in the pickup range
                    switch (type)
                    {
                        case Collectable_Type.ENEMY_FLAG:
#if DEBUG
                            Debug.Log("Is the enemy flag in pickup range?");
#endif //DEBUG
                            if (objectsInView[i].name.Equals(agentData.EnemyFlagName))
                            {
                                result = true;
                            }
                            break;
                        case Collectable_Type.FRIENDLY_FLAG:
#if DEBUG
                            Debug.Log("Is the friendy flag in pickup range?");
#endif //DEBUG
                            if (objectsInView[i].name.Equals(agentData.FriendlyFlagName))
                            {
                                result = true;
                            }
                            break;
                        case Collectable_Type.HEALTH:
#if DEBUG
                            Debug.Log("Is the health in pickup range?");
#endif //DEBUG
                            if (objectsInView[i].name.Equals("Health Kit"))
                            {
                                result = true;
                            }
                            break;
                        case Collectable_Type.POWER:
#if DEBUG
                            Debug.Log("Is the power in pickup range?");
#endif //DEBUG
                            if (objectsInView[i].name.Equals("Power Up"))
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
#if DEBUG
                Debug.Log("YES");
#endif //DEBUG
                return NodeState.SUCCESS;
            }
            else
            {
#if DEBUG
                Debug.Log("NO");
#endif //DEBUG
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
#if DEBUG
            Debug.Log("Is there an enemy in attack range?");
#endif //DEBUG
            if (sensing.IsInAttackRange(sensing.GetNearestEnemyInView()))
            {
#if DEBUG
                Debug.Log("YES");
#endif //DEBUG
                return NodeState.SUCCESS;
            }
            else
            {
#if DEBUG
                Debug.Log("NO");
#endif //DEBUG
                return NodeState.FAILURE;
            }
        }
    }

    // Check if a friendly team member is pursuing the flag
    public class TeamMemberPursuingFlag : Node
    {
        private AgentData thisAgent;

        public TeamMemberPursuingFlag(AgentData thisAgent)
        {
            this.thisAgent = thisAgent;
        }
        public override NodeState Evaluate()
        {
#if DEBUG
            Debug.Log("Is there a team member pursuing the flag?");
#endif //DEBUG
            // If there's only one and it's us then we don't want to stop so count it as a no
            List<GameObject> membersPursuingFlag = thisAgent.GetTeamBlackboard().GetMembersPursuingFlag();
            if ((membersPursuingFlag.Count == 1 && membersPursuingFlag[0] != thisAgent.gameObject) || membersPursuingFlag.Count > 1)
            {
#if DEBUG
                Debug.Log("YES");
#endif //DEBUG
                return NodeState.SUCCESS;
            }
            else
            {
#if DEBUG
                Debug.Log("NO");
#endif //DEBUG
                return NodeState.FAILURE;
            }
        }
    }

    public class FlagAtBase : Node
    {
        private TeamBlackboard teamBlackboard;

        public FlagAtBase(TeamBlackboard teamBlackboard)
        {
            this.teamBlackboard = teamBlackboard;
        }
        public override NodeState Evaluate()
        {
#if DEBUG
            Debug.Log("Is the team's flag at their base?");
#endif //DEBUG
            if (teamBlackboard.GetFriendlyBase().IsEnemyFlagInBase())
            {
#if DEBUG
                Debug.Log("YES");
#endif //DEBUG
                return NodeState.SUCCESS;
            }
            else
            {
#if DEBUG
                Debug.Log("NO");
#endif //DEBUG
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
#if DEBUG
            Debug.Log("Inverting...");
#endif //DEBUG
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
                default:
                    break;
            }
            return nodeState;
        }
    }
}
#endregion // Decorators