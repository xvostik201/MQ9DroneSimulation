using UnityEngine;

[CreateAssetMenu(fileName = "MissionData", menuName = "Game/Mission Data")]
public class MissionData : ScriptableObject
{
    [Header("Информация о миссии")]
    [Tooltip("Название операции, отображается в брифинге.")]
    public string missionName = "Операция 0";

    [TextArea(3, 6)]
    [Tooltip("Краткое описание — цель, обстановка, приказ.")]
    public string briefingText =
        "Разведданные указывают на технику противника. " +
        "Приказ: обнаружить и уничтожить все силы в секторе.";

    [Header("Визуальное оформление")]
    [Tooltip("Превью или логотип миссии в меню.")]
    public Sprite missionImage;

    [Header("Служебная информация")]
    [Tooltip("ID миссии для сохранений и прогресса.")]
    public string missionID = "mission_001";

    [Tooltip("Сложность: 1 = легко, 2 = средне, 3 = тяжело.")]
    [Range(1, 3)]
    public int difficulty = 1;

    [Header("Префаб уровня")]
    [Tooltip("Префаб сцены миссии (враги, окружение, точки спавна и т.п.)")]
    public GameObject missionPrefab;
}