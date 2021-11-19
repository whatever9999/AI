using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamBlackboard : MonoBehaviour
{
    [SerializeField] private GameObject enemyFlag;
    public GameObject GetEnemyFlag() { return enemyFlag; }

    [SerializeField] private GameObject friendlyFlag;
    public GameObject GetFriendlyFlag() { return friendlyFlag; }

    private GameObject weakestMember;
    public GameObject GetWeakestMember() { return weakestMember; }

    private GameObject memberWithFlag;
    public GameObject GetMemberWithFlag() { return memberWithFlag; }
    public void SetMemberWithFlag(GameObject member) { memberWithFlag = member; }

    private void Update()
    {
        IdentifyWeakestMember();
    }

    private void IdentifyWeakestMember()
    {

    }
}
