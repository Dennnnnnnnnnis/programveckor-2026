using UnityEngine;

[CreateAssetMenu(fileName = "MusicObject", menuName = "Scriptable Objects/MusicObject")]
public class MusicObject : ScriptableObject
{
    public AudioClip music;
    public bool loop = true;
    public float loopStart, loopEnd;
}
