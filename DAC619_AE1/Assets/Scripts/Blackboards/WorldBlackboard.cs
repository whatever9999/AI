using UnityEngine;

public class WorldBlackboard : MonoBehaviour
{
    [SerializeField] private TeamBlackboard redTeamBlackboard;
    public TeamBlackboard GetRedTeamBlackboard() { return redTeamBlackboard; }

    [SerializeField] private TeamBlackboard blueTeamBlackboard;
    public TeamBlackboard GetBlueTeamBlackboard() { return blueTeamBlackboard; }
}
