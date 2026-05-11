using UnityEngine;

public class ParticleAutoDestroy : MonoBehaviour
{
    private ParticleSystem[] particles;

    void Awake()
    {
        particles = GetComponentsInChildren<ParticleSystem>();
    }

    void Start()
    {
        foreach (ParticleSystem ps in particles)
        {
            ps.Play();
        }

        Destroy(gameObject, GetLongestLifetime());
    }

    float GetLongestLifetime()
    {
        float max = 0f;

        foreach (ParticleSystem ps in particles)
        {
            float time =
                ps.main.duration +
                ps.main.startLifetime.constantMax;

            if (time > max)
                max = time;
        }

        return max;
    }
}