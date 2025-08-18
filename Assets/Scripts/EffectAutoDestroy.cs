using UnityEngine;

public class EffectAutoDestroy : MonoBehaviour
{
    public float lifetime = 1f;

    private void Awake()
    {
        if (TryGetComponent<ParticleSystem>(out var ps))
        {
            float dur = ps.main.duration;
            float maxLife = ps.main.startLifetime.mode == ParticleSystemCurveMode.TwoConstants
                ? ps.main.startLifetime.constantMax
                : ps.main.startLifetime.constant;
            Destroy(gameObject, dur + maxLife + 0.1f);
        }
        else
        {
            Destroy(gameObject, lifetime);
        }
    }
}
