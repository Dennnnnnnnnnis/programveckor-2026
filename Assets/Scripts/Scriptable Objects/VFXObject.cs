using UnityEngine;

[CreateAssetMenu(fileName = "VFXObject", menuName = "Scriptable Objects/VFXObject")]
public class VFXObject : ScriptableObject
{
    public Sprite[] sprites;
    public AnimationClip[] anims;
    public float lifetime = 0f; // If this is 0 we should base it on the animation length (or if that's not possible just don't spawn it)
    public bool manualRemove = false; // Ignores the lifetime
    [Space]
    public Vector2 minOffset = new Vector2();
    public Vector2 maxOffset = new Vector2();
    public float minAngle, maxAngle;
    [Space]
    public bool usePhysics = false;
    public float gravity = 5f;
    public Vector2 minVelocity = new Vector2();
    public Vector2 maxVelocity = new Vector2();
    public float minAngularVelocity, maxAngularVelocity;
    [Space]
    public string sortingLayer = "Effects";
    public int orderInLayer = 0;
}
