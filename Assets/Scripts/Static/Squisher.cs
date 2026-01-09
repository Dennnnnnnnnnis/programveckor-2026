using UnityEngine;
using static Ease;

public class Squisher
{
    public Vector2 SizeMultiplier { get; private set; }

    private float time = 0f;
    private float duration = 0f;
    private EasingStyle easingStyle = EasingStyle.Linear;
    private EasingDirection easingDir = EasingDirection.Out;

    private float squish = 0f;
    private float squishFrom = 0f;
    private float squishTo = 0f;

    public void Squish(float duration, float squishTo, EasingStyle easingStyle = EasingStyle.Linear, EasingDirection easingDirection = EasingDirection.Out)
    {
        time = duration;
        this.duration = duration;

        this.squishTo = squishTo;
        squishFrom = squish;

        this.easingStyle = easingStyle;
        easingDir = easingDirection;
    }
    public void Squish(float duration, float squishTo, float squishFrom, EasingStyle easingStyle = EasingStyle.Linear, EasingDirection easingDirection = EasingDirection.Out)
    {
        time = duration;
        this.duration = duration;

        this.squishTo = squishTo;
        this.squishFrom = squishFrom;

        this.easingStyle = easingStyle;
        easingDir = easingDirection;
    }

    public void Update(float deltaTime)
    {
        if (time > 0f)
        {
            time -= deltaTime;
            squish = squishFrom + ((squishTo - squishFrom) * EvaluateFromStyle(1f - (time / duration), easingStyle, easingDir));
            if(squish > 0f)
            {
                SizeMultiplier = new Vector2(squish + 1f, 1f / (squish + 1f));
            }
            else if (squish < 0f)
            {
                SizeMultiplier = new Vector2(1f / (-squish + 1f), -squish + 1f);
            }
        }
        else
            SizeMultiplier = new Vector2(1f, 1f);
    }
}
