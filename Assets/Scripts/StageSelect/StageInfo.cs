using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "StageInfo", menuName = "Stage Select/Stage Info")]
public class StageInfo : ScriptableObject
{
    [SerializeField, FormerlySerializedAs("<StageId>k__BackingField")] private string stageId;
    [SerializeField, FormerlySerializedAs("<StageName>k__BackingField")] private string stageName;
    [SerializeField, FormerlySerializedAs("<IsUnlocked>k__BackingField")] private bool defaultUnlocked = true;
    [SerializeField, FormerlySerializedAs("<IsCleared>k__BackingField")] private bool isCleared;
    [SerializeField, FormerlySerializedAs("<Description>k__BackingField"), TextArea(2, 5)] private string description;
    [SerializeField, FormerlySerializedAs("<Thumbnail>k__BackingField")] private Sprite thumbnail;
    [SerializeField, FormerlySerializedAs("<SceneName>k__BackingField")] private string sceneName;

    public string StageId => stageId;
    public string StageName => stageName;
    public bool DefaultUnlocked => defaultUnlocked;
    public bool IsCleared => isCleared;
    public string Description => description;
    public Sprite Thumbnail => thumbnail;
    public string SceneName => sceneName;
}
