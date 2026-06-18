using Imas;
using UnityEngine;

[CreateAssetMenu(fileName = "facial_chara_sobj", menuName = "ScriptableObjects/facial_chara_sobj")]
public class FaceScrObj : ScriptableObject
{
    public FaceBlendParam[] charaParams;
}
