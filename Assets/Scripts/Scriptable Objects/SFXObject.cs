using UnityEngine;

[CreateAssetMenu(fileName = "SFXObject", menuName = "Scriptable Objects/SFXObject")]
public class SFXObject : ScriptableObject
{
    public AudioClip[] sfx;

    public float minPitch = 1f, maxPitch = 1f;
    public float minVolume = 1f, maxVolume = 1f;
    public float spatialBlend = 1f;
    public bool loop = false;
}
