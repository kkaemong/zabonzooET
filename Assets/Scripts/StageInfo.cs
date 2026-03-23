using UnityEngine.Serialization;
using UnityEngine;

[CreateAssetMenu(fileName = "StageInfo", menuName = "Stage Select/Stage Info")]
public class StageInfo : ScriptableObject
{
    [field: SerializeField] public string StageId { get; private set; }
    [field: SerializeField] public string StageName { get; private set; }
    [field: FormerlySerializedAs("<IsUnlocked>k__BackingField")]
    [field: SerializeField] public bool DefaultUnlocked { get; private set; } = true;
    [field: SerializeField] [TextArea(2, 5)] public string Description { get; private set; }
    [field: SerializeField] public Sprite Thumbnail { get; private set; }
    [field: SerializeField] public string SceneName { get; private set; }
}
