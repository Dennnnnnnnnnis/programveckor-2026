using System;
using System.Collections.Generic;
using UnityEngine;

public static class Ease
{
    public enum EasingStyle { Linear, Sine, Quad, Cubic, Quart, Quint, Expo, Circ, Back, Elastic, Bounce };
    public enum EasingDirection { In, Out, InOut };

    // Heck yeah dictionary
    private static readonly Dictionary<EasingStyle, Func<float, EasingDirection, float>> easingFunctions =
    new()
    {
        { EasingStyle.Linear, Linear },
        { EasingStyle.Sine, Sine },
        { EasingStyle.Quad, Quad },
        { EasingStyle.Cubic, Cubic },
        { EasingStyle.Quart, Quart },
        { EasingStyle.Quint, Quint },
        { EasingStyle.Expo, Expo },
        { EasingStyle.Circ, Circ },
        { EasingStyle.Back, Back },
        { EasingStyle.Elastic, Elastic },
        { EasingStyle.Bounce, Bounce }
    };

    // This is for more dynamic easing stuff
    public static float EvaluateFromStyle(float value, EasingStyle style, EasingDirection direction = EasingDirection.InOut)
    {
        // I used a dictionary instead of switch statement because it looked cleaner
        if (easingFunctions.TryGetValue(style, out Func<float, EasingDirection, float> ease))
        {
            return ease(value, direction);
        }

        return value;
    }

    // The math for all of these are from 'easings.net', very useful website
    public static float Linear(float value, EasingDirection direction = EasingDirection.InOut)
    {
        // Idk why you would ever use this, but here it is
        return value;
    }
    public static float Sine(float value, EasingDirection direction = EasingDirection.InOut)
    {
        switch (direction)
        {
            case EasingDirection.In:
                return 1f - Mathf.Cos((value * Mathf.PI) / 2f);
            case EasingDirection.Out:
                return Mathf.Sin((value * Mathf.PI) / 2f);
            case EasingDirection.InOut:
                return -(Mathf.Cos(Mathf.PI * value) - 1f) / 2f;
            default:
                return value;
        }
    }
    public static float Quad(float value, EasingDirection direction = EasingDirection.InOut)
    {
        switch (direction)
        {
            case EasingDirection.In:
                return value * value;
            case EasingDirection.Out:
                return 1f - (1f - value) * (1f - value);
            case EasingDirection.InOut:
                return (value < 0.5f ? 2f * value * value : 1f - Mathf.Pow(-2f * value + 2f, 2f) / 2f);
            default:
                return value;
        }
    }
    public static float Cubic(float value, EasingDirection direction = EasingDirection.InOut)
    {
        switch (direction)
        {
            case EasingDirection.In:
                return Mathf.Pow(value, 3f);
            case EasingDirection.Out:
                return 1f - Mathf.Pow(1f - value, 3f);
            case EasingDirection.InOut:
                return (value < 0.5f ? 4f * Mathf.Pow(value, 3f) : 1f - Mathf.Pow(-2f * value + 2f, 3f) / 2f);
            default:
                return value;
        }
    }
    public static float Quart(float value, EasingDirection direction = EasingDirection.InOut)
    {
        switch (direction)
        {
            case EasingDirection.In:
                return Mathf.Pow(value, 4f);
            case EasingDirection.Out:
                return 1f - Mathf.Pow(1f - value, 4f);
            case EasingDirection.InOut:
                return (value < 0.5f ? 8f * Mathf.Pow(value, 4f) : 1f - Mathf.Pow(-2f * value + 2f, 4f) / 2f);
            default:
                return value;
        }
    }
    public static float Quint(float value, EasingDirection direction = EasingDirection.InOut)
    {
        switch (direction)
        {
            case EasingDirection.In:
                return Mathf.Pow(value, 5f);
            case EasingDirection.Out:
                return 1f - Mathf.Pow(1f - value, 5f);
            case EasingDirection.InOut:
                return (value < 0.5f ? 16f * Mathf.Pow(value, 5f) : 1f - Mathf.Pow(-2f * value + 2f, 5f) / 2f);
            default:
                return value;
        }
    }
    public static float Expo(float value, EasingDirection direction = EasingDirection.InOut)
    {
        switch (direction)
        {
            case EasingDirection.In:
                return (value == 0f ? 0f : Mathf.Pow(2f, 10f * value - 10f));
            case EasingDirection.Out:
                return (value == 1f ? 1f : 1f - Mathf.Pow(2f, -10f * value));
            case EasingDirection.InOut:
                return ((value == 0f || value == 1f) ? value : (value < 0.5f ? Mathf.Pow(2f, 20f * value - 10f) / 2f : (2f - Mathf.Pow(2f, -20f * value + 10f)) / 2f));
            default:
                return value;
        }
    }
    public static float Circ(float value, EasingDirection direction = EasingDirection.InOut)
    {
        switch (direction)
        {
            case EasingDirection.In:
                return 1f - Mathf.Sqrt(1f - Mathf.Pow(value, 2f));
            case EasingDirection.Out:
                return Mathf.Sqrt(1f - Mathf.Pow(value - 1f, 2f));
            case EasingDirection.InOut:
                return (value < 0.5f ? (1f - Mathf.Sqrt(1f - Mathf.Pow(2f * value, 2f))) / 2f : (Mathf.Sqrt(1f - Mathf.Pow(-2f * value + 2f, 2f)) + 1f) / 2f);
            default:
                return value;
        }
    }
    public static float Back(float value, EasingDirection direction = EasingDirection.InOut)
    {
        switch (direction)
        {
            case EasingDirection.In:
                return 2.70158f * value * value * value - 1.70158f * value * value;
            case EasingDirection.Out:
                return 1f + 2.70158f * Mathf.Pow(value - 1f, 3f) + 1.70158f * Mathf.Pow(value - 1f, 2f);
            case EasingDirection.InOut:
                return (value < 0.5f ? (Mathf.Pow(2f * value, 2f) * ((1.70158f * 1.525f + 1f) * 2f * value - 1.70158f * 1.525f)) / 2f : (Mathf.Pow(2f * value - 2f, 2f) * ((1.70158f * 1.525f + 1f) * (value * 2f - 2f) + 1.70158f * 1.525f) + 2f) / 2f);
            default:
                return value;
        }
    }
    public static float Elastic(float value, EasingDirection direction = EasingDirection.InOut)
    {
        switch (direction)
        {
            case EasingDirection.In:
                return ((value == 0f || value == 1f) ? value : -Mathf.Pow(2f, 10f * value - 10f) * Mathf.Sin((value * 10f - 10.75f) * (2f * Mathf.PI) / 3f));
            case EasingDirection.Out:
                return ((value == 0f || value == 1f) ? value : Mathf.Pow(2f, -10f * value) * Mathf.Sin((value * 10f - 0.75f) * (2f * Mathf.PI) / 3f) + 1f);
            case EasingDirection.InOut:
                return ((value == 0f || value == 1f) ? value : (value < 0.5f ? -(Mathf.Pow(2f, 20f * value - 10f) * Mathf.Sin((20f * value - 11.125f) * (2f * Mathf.PI) / 4.5f)) / 2f : (Mathf.Pow(2f, -20f * value + 10f) * Mathf.Sin((20f * value - 11.125f) * (2f * Mathf.PI) / 4.5f)) / 2f + 1f));
            default:
                return value;
        }
    }
    public static float Bounce(float value, EasingDirection direction = EasingDirection.InOut)
    {
        float n1 = 7.5625f;
        float d1 = 2.75f;
        float easeOut = value;

        if (direction == EasingDirection.In)
            easeOut = 1f - value;
        else if(direction == EasingDirection.InOut)
        {
            if (value < 0.5f)
                easeOut = 1f - 2f * value;
            else
                easeOut = 2f * value - 1f;
        }

        if (easeOut < 1f / d1)
        {
            easeOut = n1 * easeOut * easeOut;
        }
        else if (easeOut < 2f / d1)
        {
            easeOut -= 1.5f / d1;
            easeOut = n1 * easeOut * easeOut + 0.75f;
        }
        else if (easeOut < 2.5f / d1)
        {
            easeOut -= 2.25f / d1;
            easeOut = n1 * easeOut * easeOut + 0.9375f;
        }
        else
        {
            easeOut -= 2.625f / d1;
            easeOut = n1 * easeOut * easeOut + 0.984375f;
        }

        switch (direction)
        {
            case EasingDirection.In:
                return 1f - easeOut;
            case EasingDirection.Out:
                return easeOut;
            case EasingDirection.InOut:
                return (value < 0.5f ? (1f - easeOut) / 2f : (1f + easeOut) / 2f);
            default:
                return value;
        }
    }
}
