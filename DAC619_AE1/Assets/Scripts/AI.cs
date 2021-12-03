using System.Collections.Generic;
using UnityEngine;

/*****************************************************************************************************************************
 * Write your core AI code in this file here. The private variable 'agentScript' contains all the agents actions which are listed
 * below. Ensure your code it clear and organised and commented.
 *
 * Unity Tags
 * public static class Tags
 * public const string BlueTeam = "Blue Team";	The tag assigned to blue team members.
 * public const string RedTeam = "Red Team";	The tag assigned to red team members.
 * public const string Collectable = "Collectable";	The tag assigned to collectable items (health kit and power up).
 * public const string Flag = "Flag";	The tag assigned to the flags, blue or red.
 * 
 * Unity GameObject names
 * public static class Names
 * public const string PowerUp = "Power Up";	Power up name
 * public const string HealthKit = "Health Kit";	Health kit name.
 * public const string BlueFlag = "Blue Flag";	The blue teams flag name.
 * public const string RedFlag = "Red Flag";	The red teams flag name.
 * public const string RedBase = "Red Base";    The red teams base name.
 * public const string BlueBase = "Blue Base";  The blue teams base name.
 * public const string BlueTeamMember1 = "Blue Team Member 1";	Blue team member 1 name.
 * public const string BlueTeamMember2 = "Blue Team Member 2";	Blue team member 2 name.
 * public const string BlueTeamMember3 = "Blue Team Member 3";	Blue team member 3 name.
 * public const string RedTeamMember1 = "Red Team Member 1";	Red team member 1 name.
 * public const string RedTeamMember2 = "Red Team Member 2";	Red team member 2 name.
 * public const string RedTeamMember3 = "Red Team Member 3";	Red team member 3 name.
 * 
 * _agentData properties and public variables
 * public bool IsPoweredUp;	Have we powered up, true if we’re powered up, false otherwise.
 * public int CurrentHitPoints;	Our current hit points as an integer
 * public bool HasFriendlyFlag;	True if we have collected our own flag
 * public bool HasEnemyFlag;	True if we have collected the enemy flag
 * public GameObject FriendlyBase; The friendly base GameObject
 * public GameObject EnemyBase;    The enemy base GameObject
 * public int FriendlyScore; The friendly teams score
 * public int EnemyScore;       The enemy teams score
 * 
 * _agentActions methods
 * public bool MoveTo(GameObject target)	Move towards a target object. Takes a GameObject representing the target object as a parameter. Returns true if the location is on the navmesh, false otherwise.
 * public bool MoveTo(Vector3 target)	Move towards a target location. Takes a Vector3 representing the destination as a parameter. Returns true if the location is on the navmesh, false otherwise.
 * public bool MoveToRandomLocation()	Move to a random location on the level, returns true if the location is on the navmesh, false otherwise.
 * public void CollectItem(GameObject item)	Pick up an item from the level which is within reach of the agent and put it in the inventory. Takes a GameObject representing the item as a parameter.
 * public void DropItem(GameObject item)	Drop an inventory item or the flag at the agents’ location. Takes a GameObject representing the item as a parameter.
 * public void UseItem(GameObject item)	Use an item in the inventory (currently only health kit or power up). Takes a GameObject representing the item to use as a parameter.
 * public void AttackEnemy(GameObject enemy)	Attack the enemy if they are close enough. ). Takes a GameObject representing the enemy as a parameter.
 * public void Flee(GameObject enemy)	Move in the opposite direction to the enemy. Takes a GameObject representing the enemy as a parameter.
 * 
 * _agentSenses properties and methods
 * public List<GameObject> GetObjectsInViewByTag(string tag)	Return a list of objects with the same tag. Takes a string representing the Unity tag. Returns null if no objects with the specified tag are in view.
 * public GameObject GetObjectInViewByName(string name)	Returns a specific GameObject by name in view range. Takes a string representing the objects Unity name as a parameter. Returns null if named object is not in view.
 * public List<GameObject> GetObjectsInView()	Returns a list of objects within view range. Returns null if no objects are in view.
 * public List<GameObject> GetCollectablesInView()	Returns a list of objects with the tag Collectable within view range. Returns null if no collectable objects are in view.
 * public List<GameObject> GetFriendliesInView()	Returns a list of friendly team AI agents within view range. Returns null if no friendlies are in view.
 * public List<GameObject> GetEnemiesInView()	Returns a list of enemy team AI agents within view range. Returns null if no enemies are in view.
 * public GameObject GetNearestEnemyInView()   Returns the nearest enemy AI in view to the agent. Returns null if no enemies are in view.
 * public bool IsItemInReach(GameObject item)	Checks if we are close enough to a specific collectible item to pick it up), returns true if the object is close enough, false otherwise.
 * public bool IsInAttackRange(GameObject target)	Check if we're with attacking range of the target), returns true if the target is in range, false otherwise.
 * public Vector3 GetVectorToTarget(GameObject target)  Return a normalised vector pointing to the target GameObject
 * public Vector3 GetVectorToTarget(Vector3 target)     Return a normalised vector pointing to the target vector
 * public Vector3 GetFleeVectorFromTarget(GameObject target)    Return a normalised vector pointing away from the target GameObject
 * public Vector3 GetFleeVectorFromTarget(Vector3 target)   Return a normalised vector pointing away from the target vector
 * 
 * _agentInventory properties and methods
 * public bool AddItem(GameObject item)	Adds an item to the inventory if there's enough room (max capacity is 'Constants.InventorySize'), returns true if the item has been successfully added to the inventory, false otherwise.
 * public GameObject GetItem(string itemName)	Retrieves an item from the inventory as a GameObject, returns null if the item is not in the inventory.
 * public bool HasItem(string itemName)	Checks if an item is stored in the inventory, returns true if the item is in the inventory, false otherwise.
 * 
 * You can use the game objects name to access a GameObject from the sensing system. Thereafter all methods require the GameObject as a parameter.
 * 
*****************************************************************************************************************************/

/// <summary>
/// Implement your AI script here, you can put everything in this file, or better still, break your code up into multiple files
/// and call your script here in the Update method. This class includes all the data members you need to control your AI agent
/// get information about the world, manage the AI inventory and access essential information about your AI.
///
/// You may use any AI algorithm you like, but please try to write your code properly and professionaly and don't use code obtained from
/// other sources, including the labs.
///
/// See the assessment brief for more details
/// </summary>
public class AI : MonoBehaviour
{
    // Gives access to important data about the AI agent (see above)
    private AgentData _agentData;
    // Gives access to the agent senses
    private Sensing _agentSenses;
    // gives access to the agents inventory
    private InventoryController _agentInventory;
    // This is the script containing the AI agents actions
    // e.g. agentScript.MoveTo(enemy);
    private AgentActions _agentActions;

    // Create base node
    Selector CaptureTheFlagAI;


    // Use this for initialization
    void Start()
    {
        // Initialise the accessable script components
        _agentData = GetComponent<AgentData>();
        _agentActions = GetComponent<AgentActions>();
        _agentSenses = GetComponentInChildren<Sensing>();
        _agentInventory = GetComponentInChildren<InventoryController>();

        #region Declare Actions
        // Pick Up Collectable
        Actions.PickUpCollectable PickUpEnemyFlag = new Actions.PickUpCollectable(_agentActions, _agentData, _agentSenses, Collectable_Type.ENEMY_FLAG);
        Actions.PickUpCollectable PickUpFriendlyFlag = new Actions.PickUpCollectable(_agentActions, _agentData, _agentSenses, Collectable_Type.FRIENDLY_FLAG);
        Actions.PickUpCollectable PickUpHealth = new Actions.PickUpCollectable(_agentActions, _agentData, _agentSenses, Collectable_Type.HEALTH);
        Actions.PickUpCollectable PickUpPower = new Actions.PickUpCollectable(_agentActions, _agentData, _agentSenses, Collectable_Type.POWER);

        // Drop Collectable
        Actions.DropCollectable DropHealth = new Actions.DropCollectable(_agentActions, _agentData, _agentInventory, Collectable_Type.HEALTH);
        Actions.DropCollectable DropEnemyFlag = new Actions.DropCollectable(_agentActions, _agentData, _agentInventory, Collectable_Type.ENEMY_FLAG);
        Actions.DropCollectable DropFriendlyFlag = new Actions.DropCollectable(_agentActions, _agentData, _agentInventory, Collectable_Type.FRIENDLY_FLAG);

        // Move to GO
        Actions.MoveToGameObject MoveToEnemyFlag = new Actions.MoveToGameObject(_agentActions, _agentData, _agentSenses, GameObject_Type.ENEMY_FLAG);
        Actions.MoveToGameObject MoveToFriendlyFlag = new Actions.MoveToGameObject(_agentActions, _agentData, _agentSenses, GameObject_Type.FRIENDLY_FLAG);
        Actions.MoveToGameObject MoveToHealthPack = new Actions.MoveToGameObject(_agentActions, _agentData, _agentSenses, GameObject_Type.HEALTH_PACK);
        Actions.MoveToGameObject MoveToPowerPack = new Actions.MoveToGameObject(_agentActions, _agentData, _agentSenses, GameObject_Type.POWER_PACK);
        Actions.MoveToGameObject MoveToNearestEnemy = new Actions.MoveToGameObject(_agentActions, _agentData, _agentSenses, GameObject_Type.NEAREST_ENEMY);
        Actions.MoveToGameObject MoveToFriendlyWithFlag = new Actions.MoveToGameObject(_agentActions, _agentData, _agentSenses, GameObject_Type.FRIENDLY_WITH_FLAG);
        Actions.MoveToGameObject MoveToWeakestFriendly = new Actions.MoveToGameObject(_agentActions, _agentData, _agentSenses, GameObject_Type.WEAKEST_FRIENDLY);
        Actions.MoveToGameObject MoveToBase = new Actions.MoveToGameObject(_agentActions, _agentData, _agentSenses, GameObject_Type.BASE);
        Actions.MoveToGameObject MoveToNotInBase = new Actions.MoveToGameObject(_agentActions, _agentData, _agentSenses, GameObject_Type.NOT_IN_BASE);

        // Flee
        Actions.Flee Flee = new Actions.Flee(_agentActions, _agentSenses);

        // Use Useable
        Actions.UseUseable UseHealth = new Actions.UseUseable(_agentActions, _agentData, _agentInventory, Useable_Type.HEALTH);
        Actions.UseUseable UsePower = new Actions.UseUseable(_agentActions, _agentData, _agentInventory, Useable_Type.POWER);

        // Attack
        Actions.Attack Attack = new Actions.Attack(_agentSenses, _agentActions);
        #endregion // Declare Actions

        #region Declare Conditions
        // Agent Health Less Than
        Conditions.AgentHeathLessThan ThisAgentHealthCheck = new Conditions.AgentHeathLessThan(_agentData, _agentData.GetTeamBlackboard(), GameObject_Type.THIS_AGENT, 30);
        Conditions.AgentHeathLessThan WeakestMemberHealthCheck = new Conditions.AgentHeathLessThan(_agentData, _agentData.GetTeamBlackboard(), GameObject_Type.WEAKEST_FRIENDLY, 40);

        // Useable on Level
        Conditions.UseableOnLevel HealthOnLevel = new Conditions.UseableOnLevel(Useable_Type.HEALTH);
        Conditions.UseableOnLevel PowerOnLevel = new Conditions.UseableOnLevel(Useable_Type.POWER);

        // Team Has Flag
        Conditions.TeamHasFlag FriendlyHasEnemyFlag = new Conditions.TeamHasFlag(_agentData.GetTeamBlackboard(), Team_Type.ENEMY);
        Conditions.TeamHasFlag FriendlyHasFriendlyFlag = new Conditions.TeamHasFlag(_agentData.GetTeamBlackboard(), Team_Type.FRIENDLY);
        Conditions.TeamHasFlag EnemyHasFlag;
        // The blue team need to check the red team blackboard and vice versa
        if (_agentData.FriendlyTeam == AgentData.Teams.BlueTeam)
        {
            EnemyHasFlag = new Conditions.TeamHasFlag(_agentData.GetWorldBlackboard().GetRedTeamBlackboard(), Team_Type.ENEMY);
        }
        else
        {
            EnemyHasFlag = new Conditions.TeamHasFlag(_agentData.GetWorldBlackboard().GetBlueTeamBlackboard(), Team_Type.ENEMY);
        }

        // Got Collectable
        Conditions.GotCollectable GotEnemyFlag = new Conditions.GotCollectable(_agentData, _agentInventory, Collectable_Type.ENEMY_FLAG);
        Conditions.GotCollectable GotFriendlyFlag = new Conditions.GotCollectable(_agentData, _agentInventory, Collectable_Type.FRIENDLY_FLAG);
        Conditions.GotCollectable GotHealth = new Conditions.GotCollectable(_agentData, _agentInventory, Collectable_Type.HEALTH);
        Conditions.GotCollectable GotPower = new Conditions.GotCollectable(_agentData, _agentInventory, Collectable_Type.POWER);

        // Collectable in Pickup Range
        Conditions.CollectableInPickupRange EnemyFlagInPickupRange = new Conditions.CollectableInPickupRange(_agentData, _agentSenses, Collectable_Type.ENEMY_FLAG);
        Conditions.CollectableInPickupRange FriendlyFlagInPickupRange = new Conditions.CollectableInPickupRange(_agentData, _agentSenses, Collectable_Type.FRIENDLY_FLAG);
        Conditions.CollectableInPickupRange HealthInPickupRange = new Conditions.CollectableInPickupRange(_agentData, _agentSenses, Collectable_Type.HEALTH);
        Conditions.CollectableInPickupRange PowerInPickupRange = new Conditions.CollectableInPickupRange(_agentData, _agentSenses, Collectable_Type.POWER);

        // Enemy in Attack Range
        Conditions.EnemyInAttackRange EnemyInAttackRange = new Conditions.EnemyInAttackRange(_agentSenses);

        // Team Member Pursuing Flag
        Conditions.TeamMemberPursuingFlag TeamMemberPursuingFlag = new Conditions.TeamMemberPursuingFlag(_agentData);

        // Flag At Base
        Conditions.FlagAtBase FlagAtFriendlyBase;
        Conditions.FlagAtBase FlagAtEnemyBase;
        if (_agentData.FriendlyTeam == AgentData.Teams.BlueTeam)
        {
            FlagAtEnemyBase = new Conditions.FlagAtBase(_agentData.GetWorldBlackboard().GetRedTeamBlackboard());
            FlagAtFriendlyBase = new Conditions.FlagAtBase(_agentData.GetWorldBlackboard().GetBlueTeamBlackboard());
        }
        else
        {
            FlagAtEnemyBase = new Conditions.FlagAtBase(_agentData.GetWorldBlackboard().GetBlueTeamBlackboard());
            FlagAtFriendlyBase = new Conditions.FlagAtBase(_agentData.GetWorldBlackboard().GetRedTeamBlackboard());
        }

        Conditions.HealthNextToWeakest HealthNextToWeakest = new Conditions.HealthNextToWeakest(_agentData.GetTeamBlackboard());
        #endregion // Declare Conditions

        #region Delcare Decorated Conditions
        Decorators.Inverter NotGotFriendlyFlag = new Decorators.Inverter(GotFriendlyFlag);
        Decorators.Inverter NotGotEnemyFlag = new Decorators.Inverter(GotEnemyFlag);
        Decorators.Inverter NoFriendlyHasFriendlyFlag = new Decorators.Inverter(FriendlyHasFriendlyFlag);
        Decorators.Inverter NoFriendlyHasEnemyFlag = new Decorators.Inverter(FriendlyHasEnemyFlag);
        Decorators.Inverter NoTeamMemberPursuingFlag = new Decorators.Inverter(TeamMemberPursuingFlag);
        Decorators.Inverter NoFlagAtFriendlyBase = new Decorators.Inverter(FlagAtFriendlyBase);
        Decorators.Inverter NoFlagAtEnemyBase = new Decorators.Inverter(FlagAtEnemyBase);
        Decorators.Inverter HealthNotNextToWeakest = new Decorators.Inverter(HealthNextToWeakest);
        #endregion // Declare Decorated Conditions

        #region Create Branches
        // Grab Item
        Sequence GrabHealth = new Sequence(new List<Node> { HealthInPickupRange, PickUpHealth });
        Sequence GrabPower = new Sequence(new List<Node> { PowerInPickupRange, PickUpPower });
        Sequence GrabConsideringAid = new Sequence(new List<Node> { HealthNotNextToWeakest, GrabHealth });
        Selector GrabItem = new Selector(new List<Node> { GrabConsideringAid, GrabPower });

        // Attack Enemy
        Sequence PowerAttack = new Sequence(new List<Node> { GotPower, UsePower, Attack });
        Selector DoAttack = new Selector(new List<Node> { PowerAttack, Attack });
        Sequence AttackEnemy = new Sequence(new List<Node> { EnemyInAttackRange, DoAttack });

        // Get Flag
        Sequence GetEnemyFlag = new Sequence(new List<Node> { NotGotFriendlyFlag, NoFlagAtFriendlyBase, MoveToEnemyFlag, EnemyFlagInPickupRange, PickUpEnemyFlag });

        // Stock Up
        Sequence GetHealth = new Sequence(new List<Node> { HealthOnLevel, MoveToHealthPack, GrabHealth });
        Sequence GetPower = new Sequence(new List<Node> { PowerOnLevel, MoveToPowerPack, GrabPower });
        Selector StockUp = new Selector(new List<Node> { GetHealth, GetPower });

        // Protect Flag
        Sequence AttackNearestEnemy = new Sequence(new List<Node> { MoveToNearestEnemy, AttackEnemy });
        Sequence ProtectEnemyFlag = new Sequence(new List<Node> { FriendlyHasEnemyFlag, MoveToFriendlyWithFlag, AttackNearestEnemy });

        // Aid
        Sequence GivePack = new Sequence(new List<Node> { GotHealth, MoveToWeakestFriendly, DropHealth });
        Sequence FindPack = new Sequence(new List<Node> { HealthOnLevel, MoveToHealthPack, GrabHealth, MoveToWeakestFriendly, DropHealth });
        Selector ProvideHealthPack = new Selector(new List<Node> { GivePack, FindPack });
        Sequence Aid = new Sequence(new List<Node> { WeakestMemberHealthCheck, ProvideHealthPack });

        // Save Flag
        Sequence SaveFriendlyFlag = new Sequence(new List<Node> { EnemyHasFlag, MoveToFriendlyFlag, AttackEnemy });

        // Remove Friendly Flag (from enemy base) 
        Sequence RemoveFriendlyFlag = new Sequence(new List<Node> { NotGotEnemyFlag, NoFriendlyHasFriendlyFlag, FlagAtEnemyBase, MoveToFriendlyFlag, FriendlyFlagInPickupRange, PickUpFriendlyFlag, MoveToNotInBase });
        // If we picked up the friendly flag in order to remove it from the enemy base then drop it
        Sequence PutFriendlyFlagDown = new Sequence(new List<Node> { GotFriendlyFlag, NoFlagAtEnemyBase, DropFriendlyFlag });

        // Pursue Flag
        Sequence PursueEnemyFlag = new Sequence(new List<Node> { NoFriendlyHasEnemyFlag, NoTeamMemberPursuingFlag, GetEnemyFlag });

        // Protect Self
        Sequence UseHealthPack = new Sequence(new List<Node> { GotHealth, UseHealth });
        Sequence GetHealthPack = new Sequence(new List<Node> { HealthOnLevel, MoveToHealthPack, GrabHealth, GotHealth, UseHealth });
        Sequence Escape = new Sequence(new List<Node> { EnemyInAttackRange, Flee });
        Selector ProtectHealth = new Selector(new List<Node> { UseHealthPack, GetHealthPack, Escape });
        Sequence ProtectSelf = new Sequence(new List<Node> { ThisAgentHealthCheck, ProtectHealth });

        // Return Flag
        Sequence ReturnEnemyFlag = new Sequence(new List<Node> { GotEnemyFlag, MoveToBase, DropEnemyFlag });

        // Friendly Flag Defense
        Selector FriendlyFlagDefence = new Selector(new List<Node> { PutFriendlyFlagDown, RemoveFriendlyFlag, SaveFriendlyFlag });
        #endregion // Create Branches

        // Each team has slightly different tactic
        // Red prioritises defense and will remove flags from their base before attempting to get the enemy flag
        // Blue prioritises attack and will persue the enemy flag before attempting to remove flags from their base
        CaptureTheFlagAI = new Selector(new List<Node> { GrabItem, ReturnEnemyFlag, ProtectSelf, PursueEnemyFlag, FriendlyFlagDefence, Aid, ProtectEnemyFlag, StockUp, GetEnemyFlag, AttackNearestEnemy });
    }

    // Update is called once per frame
    void Update ()
    {
        // Run your AI code in here
        CaptureTheFlagAI.Evaluate();
    }
}