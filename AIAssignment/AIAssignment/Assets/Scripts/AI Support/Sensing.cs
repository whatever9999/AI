using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Handles the AI agents sensory suite, uses OverlapSphereNonAlloc to detect objects in view range,
/// provides various access functions to percieved objects with different constraints e.g. only enemies
/// and access to the list holding all detected objects. Only objects in the 'VisibleToAI' layer are found
/// by the OverlapSphereNonAlloc. A ray cast is used to determine if a wall, defined by the 'Walls' layer,
/// obstruct the view.
/// </summary>
public class Sensing : MonoBehaviour
{
    // The owner of the senses
    private AgentData _agentData;

    private const int MaxObjectsInView = 10;

    // Masks to limit visibility
    public LayerMask VisibleToAiMask;
    public LayerMask WallsLayer;

    // Keep track of game objects in our visual field
    private readonly Dictionary<String, GameObject> _objectsPercieved = new Dictionary<String, GameObject>();
    public Dictionary<String, GameObject> ObjectsPercieved
    {
        get { return _objectsPercieved; }
    }

    // Use this for initialization
    void Start()
    {
        _agentData = GetComponentInParent<AgentData>();
    }

    // _overlapResults is returned by the sphere overlap function
    private Collider[] _overlapResults = new Collider[MaxObjectsInView];
    // _objects in view is the list of objects not obstructed (and not ourself)
    private List<GameObject> _objectsInView = new List<GameObject>(MaxObjectsInView);

    /// <summary>
    /// This updates the objectsPercievecd list by calling OverlapSphereNonAlloc with the mask selecting only
    /// objects the AI should be able to see. this list is filtered further by using a raycast to remove any objects
    /// obstructed by walls, using the WallsLayer layer. This method is called whenever the AI needs information about
    /// objects it can see
    /// </summary>
    private void UpdateViewedObjectsList()
    {
        _objectsInView.Clear();

        // Get objects in view
        int numFound = Physics.OverlapSphereNonAlloc(transform.position, _agentData.ViewRange, _overlapResults, VisibleToAiMask);

        // Add all except ourselves to list of GameObjects in view range
        for (int i = 0; i < numFound; i++)
        {
            if (!_overlapResults[i].gameObject.name.Equals(gameObject.transform.parent.name))
            {
                // Do this to prevent the raycast finding a wall behind the object and therefore treating the object as obstructed
                float objectDistance = Mathf.Min(Vector3.Distance(transform.position, _overlapResults[i].gameObject.transform.position), _agentData.ViewRange);

                // Ensure we are not looking through a wall
                if (!Physics.Raycast(transform.position, _overlapResults[i].gameObject.transform.position - transform.position, objectDistance, WallsLayer))
                {
                    // We can see it
                    _objectsInView.Add(_overlapResults[i].gameObject);
                }
            }
        }
    }

    /// <summary>
    /// Return a normalised vector pointing to the target GameObject
    /// </summary>
    /// <param name="target">the GameObject to get a vector to</param>
    /// <returns>Vector3 representing a normalised vetor pointing at the target</returns>
    public Vector3 GetVectorToTarget(GameObject target)
    {
        Vector3 vectorToTarget = target.transform.position - transform.position;
        return vectorToTarget.normalized;
    }

    /// <summary>
    /// Return a normalised vector pointing to the target vector
    /// </summary>
    /// <param name="target">the Vector3 to get a vector to</param>
    /// <returns>Vector3 representing a normalised vetor pointing at the target</returns>
    public Vector3 GetVectorToTarget(Vector3 target)
    {
        Vector3 vectorToTarget = target - transform.position;
        return vectorToTarget.normalized;
    }

    /// <summary>
    /// Return a normalised vector pointing away from the target GameObject
    /// </summary>
    /// <param name="target">the GameObject to get a vector to</param>
    /// <returns>Vector3 representing a normalised vetor pointing at the target</returns>
    public Vector3 GetFleeVectorFromTarget(GameObject target)
    {
        Vector3 vectorToTarget = transform.position - target.transform.position;
        return vectorToTarget.normalized;
    }

    /// <summary>
    /// Return a normalised vector pointing away from the target vector
    /// </summary>
    /// <param name="target">the Vector3 to get a vector to</param>
    /// <returns>Vector3 representing a normalised vetor pointing at the target</returns>
    public Vector3 GetFleeVectorFromTarget(Vector3 target)
    {
        Vector3 vectorToTarget = transform.position - target;
        return vectorToTarget.normalized;
    }

    /// <summary>
    /// Return a list of all the objects the AI can see
    /// </summary>
    /// <returns>List of GameObjects</returns>
    public List<GameObject> GetObjectsInView()
    {
        UpdateViewedObjectsList();
        return _objectsInView;
    }

    /// <summary>
    /// Returns a list of all the collectable objects in view
    /// </summary>
    /// <returns>List of GameObjects</returns>
    public List<GameObject> GetCollectablesInView()
    {
        UpdateViewedObjectsList();
        return _objectsInView.Where(x => x.CompareTag(Tags.Collectable)).ToList();
    }

    /// <summary>
    /// Returns the nearest collectable in view to the agent
    /// </summary>
    /// <returns>GameObject representing nearest collectable to the agent or null if no collectables are in view</returns>
    public GameObject GetNearestCollectableInView()
    {
        UpdateViewedObjectsList();

        List<GameObject> collectables = GetCollectablesInView();

        if (collectables.Count > 0)
        {
            GameObject closestCollectable = collectables.Aggregate((minItem, nextItem) => Vector3.Distance(minItem.transform.position, transform.position) < Vector3.Distance(nextItem.transform.position, transform.position) ? minItem : nextItem);
            return closestCollectable;
        }

        return null;
    }

    /// <summary>
    /// Returns a list of friendly AI's in view
    /// </summary>
    /// <returns>List of friendly AI's in view</returns>
    public List<GameObject> GetFriendliesInView()
    {
        UpdateViewedObjectsList();
        return _objectsInView.Where(x => x.CompareTag(_agentData.FriendlyTeamTag)).ToList();
    }

    /// <summary>
    /// Returns the nearest friendly AI's in view to the agent
    /// </summary>
    /// <returns>Nearest GameObject representing nearest friendly to the agent or null if no friendlies are in view</returns>
    public GameObject GetNearestFriendlyInView()
    {
        UpdateViewedObjectsList();

        List<GameObject> friendlies = GetFriendliesInView();

        if (friendlies.Count > 0)
        {
            GameObject closestFriendly = friendlies.Aggregate((minItem, nextItem) => Vector3.Distance(minItem.transform.position, transform.position) < Vector3.Distance(nextItem.transform.position, transform.position) ? minItem : nextItem);
            return closestFriendly;
        }

        return null;
    }

    /// <summary>
    /// Returns a list of enemy AI's in view
    /// </summary>
    /// <returns>List of enemies within viewrange</returns>
    public List<GameObject> GetEnemiesInView()
    {
        UpdateViewedObjectsList();
        return _objectsInView.Where(x => x.CompareTag(_agentData.EnemyTeamTag)).ToList();
    }

    /// <summary>
    /// Returns the nearest enemy AI's in view to the agent
    /// </summary>
    /// <returns>Nearest GameObject representing an enemy to the agent or null if no enemies are in view</returns>
    public GameObject GetNearestEnemyInView()
    {
        UpdateViewedObjectsList();

        List<GameObject> enemies = GetEnemiesInView();

        if(enemies.Count > 0)
        {
            GameObject closestEnemy = enemies.Aggregate((minItem, nextItem) => Vector3.Distance(minItem.transform.position, transform.position) < Vector3.Distance(nextItem.transform.position, transform.position) ? minItem : nextItem);
            return closestEnemy;
        }

        return null;
    }

    /// <summary>
    /// Returns a list of object with a specific tag in view
    /// </summary>
    /// <param name="tagToSelect">The tag to filter the returned list by</param>
    /// <returns>List of GameObjects</returns>
    public List<GameObject> GetObjectsInViewByTag(string tagToSelect)
    {
        UpdateViewedObjectsList();
        return _objectsInView.Where(x => x.CompareTag(tagToSelect)).ToList();
    }

    /// <summary>
    /// Returns an object with a specific name
    /// </summary>
    /// <param name="nameToSelect">The name of the object to return</param>
    /// <returns>GameObject</returns>
    public GameObject GetObjectInViewByName(string nameToSelect)
    {
        UpdateViewedObjectsList();
        return _objectsInView.SingleOrDefault(x=>x.name.Equals(nameToSelect));
    }

    /// <summary>
    /// Check if a GameObject is within the AI agents reach
    /// </summary>
    /// <param name="item">the item to check the distance of</param>
    /// <returns>true if the object is in range, false otherwise</returns>
    public bool IsItemInReach(GameObject item)
    {
        if (item != null)
        {
            if (Vector3.Distance(gameObject.transform.parent.position, item.transform.position) < _agentData.PickUpRange)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Check if we're with attacking range of a specific enemy AI
    /// </summary>
    /// <param name="target">The enemy AI to check the distance of</param>
    /// <returns>true if the enemy is within range, false otherwise</returns>
    public bool IsInAttackRange(GameObject target)
    {
        if (target != null)
        {
            if (Vector3.Distance(transform.position, target.transform.position) < _agentData.AttackRange)
            {
                return true;
            }
        }
        return false;
    }
}