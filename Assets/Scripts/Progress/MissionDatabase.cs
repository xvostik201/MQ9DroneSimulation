using UnityEngine;

[CreateAssetMenu(menuName = "Game/MissionDatabase")]
public class MissionDatabase : ScriptableObject
{
    public MissionData[] missions;
}