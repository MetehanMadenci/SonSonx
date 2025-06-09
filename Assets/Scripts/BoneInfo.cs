using UnityEngine;

[CreateAssetMenu(fileName ="New Bone", menuName = "Bone")]

public class BoneInfo : ScriptableObject
{
    [SerializeField] public int boneID;
    [SerializeField] public string boneName;
    [SerializeField] public string boneModelName;
    [TextArea(1,20)]
    [SerializeField] public string boneDescription;
    [SerializeField] public string boneLink;
}
