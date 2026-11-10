using UnityEngine;

public class Cooldown
{
    const float MIN_DURATION = 0.01f;
    public float Duration { 
        get
        {
            return _duration;    
        }
        set
        {
            _duration = Mathf.Max(MIN_DURATION, value);    
        }
    }

    private float _duration;
    private float setTime = Mathf.NegativeInfinity;
    public Cooldown(float duration)
    {
        Duration = duration;
    }

    public bool IsInCooldown() => Time.time - setTime <= _duration;
    public void StartCooldown() => setTime = Time.time;
}
