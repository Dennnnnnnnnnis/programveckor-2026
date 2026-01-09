using UnityEngine;
using static Ease;

public class Shaker
{
    public Vector2 ShakeOffset { get; private set; }

    private float time = 0f;
    private float duration = 0f;
    private float magnitude = 0f;

    private float dropoff = 0f;
    private EasingStyle dropoffStyle = EasingStyle.Linear;
    private EasingDirection dropoffDirection = EasingDirection.Out;

    public void Shake(float duration, float magnitude, float dropoff = 0f, EasingStyle easingStyle = EasingStyle.Linear, EasingDirection easingDirection = EasingDirection.Out)
    {
        if (magnitude < this.magnitude - (this.dropoff * EvaluateFromStyle(1f - (time / this.duration), dropoffStyle, dropoffDirection)))
            return;

        time = duration;
        this.duration = duration;
        this.magnitude = magnitude;

        this.dropoff = dropoff;
        dropoffStyle = easingStyle;
        dropoffDirection = easingDirection;
    }

    public void Update(float deltaTime)
    {
        if (time > 0f)
        {
            time -= deltaTime;
            float curMag = magnitude - (dropoff * EvaluateFromStyle(1f - (time / duration), dropoffStyle, dropoffDirection));
            ShakeOffset = new Vector2(Random.Range(-curMag, curMag), Random.Range(-curMag, curMag));
        }
        else
            ShakeOffset = Vector2.zero;
    }
}
