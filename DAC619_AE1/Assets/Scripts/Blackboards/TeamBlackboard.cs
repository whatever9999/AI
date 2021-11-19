using System.Collections.Generic;
using UnityEngine;

public class TeamBlackboard : MonoBehaviour
{
    private List<AgentData> team = new List<AgentData>();
    public void AddTeamMember(AgentData member) { team.Add(member); }
    public void RemoveTeamMember(AgentData member) 
    {
        GameObject memberGO = member.gameObject;
        // No longer the member carrying the flag
        if (GetMemberWithFlag() == memberGO)
        {
            SetMemberWithFlag(memberGO);
        }
        // No longer pursuing flag
        if (GetMembersPursuingFlag().Contains(memberGO))
        {
            RemoveMemberPursuingFlag(memberGO);
        }
        // Need to recalculate weakest member
        if (GetWeakestMember() == memberGO)
        {
            SetWeakestMember(null);
        }
        // Remove from list
        team.Remove(member); 
    }

    [SerializeField] private GameObject enemyFlag;
    public GameObject GetEnemyFlag() { return enemyFlag; }

    [SerializeField] private GameObject friendlyFlag;
    public GameObject GetFriendlyFlag() { return friendlyFlag; }

    private GameObject weakestMember;
    public GameObject GetWeakestMember() { return weakestMember; }
    public void SetWeakestMember(GameObject weakestMember) { this.weakestMember = weakestMember; }

    private GameObject memberWithFlag;
    public GameObject GetMemberWithFlag() { return memberWithFlag; }
    public void SetMemberWithFlag(GameObject member) { memberWithFlag = member; }

    private List<GameObject> membersPursuingFlag = new List<GameObject>();
    public List<GameObject> GetMembersPursuingFlag() { return membersPursuingFlag; }
    public void AddMemberPursuingFlag(GameObject member) { membersPursuingFlag.Add(member); }
    public void RemoveMemberPursuingFlag(GameObject member) { membersPursuingFlag.Remove(member); }

    private void Update()
    {
        IdentifyWeakestMember();
    }

    // Iterate through team members and find who has the lowest HP
    private void IdentifyWeakestMember()
    {
        int lowestHP = 100;
        AgentData tempWeakestMember = null;
        for (int i = 0; i < team.Count; i++)
        {
            if (team[i].CurrentHitPoints < lowestHP)
            {
                lowestHP = team[i].CurrentHitPoints;
                tempWeakestMember = team[i];
            }
        }
        if (tempWeakestMember)
        {
            weakestMember = tempWeakestMember.gameObject;
        }
    }
}
