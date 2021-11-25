using System.Collections.Generic;
using UnityEngine;

public class TeamBlackboard : MonoBehaviour
{
    // Bases
    [SerializeField] private SetScore enemyBase;
    public SetScore GetEnemyBase() { return enemyBase; }
    [SerializeField] private SetScore friendlyBase;
    public SetScore GetFriendlyBase() { return friendlyBase; }

    // Team members
    private List<AgentData> team = new List<AgentData>();
    public void AddTeamMember(AgentData member) { team.Add(member); }
    public void RemoveTeamMember(AgentData member) 
    {
        GameObject memberGO = member.gameObject;
        // No longer the member carrying a flag
        if (GetMemberWithEnemyFlag() == memberGO)
        {
            SetMemberWithEnemyFlag(null);
        }
        if (GetMemberWithFriendlyFlag() == memberGO)
        {
            SetMemberWithFriendlyFlag(null);
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

    // Flags
    [SerializeField] private GameObject enemyFlag;
    public GameObject GetEnemyFlag() { return enemyFlag; }

    [SerializeField] private GameObject friendlyFlag;
    public GameObject GetFriendlyFlag() { return friendlyFlag; }

    // Weakest member
    private GameObject weakestMember;
    public GameObject GetWeakestMember() { return weakestMember; }
    public void SetWeakestMember(GameObject weakestMember) { this.weakestMember = weakestMember; }

    // Members holding flags
    private GameObject memberWithEnemyFlag;
    public GameObject GetMemberWithEnemyFlag() { return memberWithEnemyFlag; }
    public void SetMemberWithEnemyFlag(GameObject member) { memberWithEnemyFlag = member; }
    private GameObject memberWithFriendlyFlag;
    public GameObject GetMemberWithFriendlyFlag() { return memberWithFriendlyFlag; }
    public void SetMemberWithFriendlyFlag(GameObject member) { memberWithFriendlyFlag = member; }

    // Members pursuing flags
    private List<GameObject> membersPursuingFlag = new List<GameObject>();
    public List<GameObject> GetMembersPursuingFlag() { return membersPursuingFlag; }
    // Don't add a member who is already in the list
    public void AddMemberPursuingFlag(GameObject member) { if (!membersPursuingFlag.Contains(member)) membersPursuingFlag.Add(member); }
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
