using UnityEngine;

[CreateAssetMenu(fileName = "AssetLoadObject", menuName = "Scriptable Objects/AssetLoadObject")]
public class AssetLoadObject : ScriptableObject
{
    public MusicObject[] music;
    public SFXObject[] sfx;
    public VFXObject[] vfx;
}
