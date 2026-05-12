using UnityEngine;

namespace RouletteGame.Core.Particle
{
    //////////////////////////////////////////////////////////////////////////
    // Automatically plays all child particle systems and destroys the
    // GameObject after the longest particle lifetime completes.
    //////////////////////////////////////////////////////////////////////////
    public class ParticleAutoDestroy : MonoBehaviour
    {

        private ParticleSystem[] particles;

        //////////////////////////////////////////////////////////////////////////
        private void Awake()
        {
            // Cache all particle systems in children for lifecycle control.
            particles = GetComponentsInChildren<ParticleSystem>();
        }

        private void Start()
        {
            // Start all particle systems and schedule object destruction.
            foreach (ParticleSystem ps in particles)
            {
                ps.Play();
            }

            Destroy(gameObject, GetLongestLifetime());
        }

        // Calculate maximum possible particle lifetime across all systems.
        private float GetLongestLifetime()
        {
            float max = 0f;
           
            // Combine duration and lifetime to determine total particle existence time.
            foreach (ParticleSystem ps in particles)
            {
                float time =
                    ps.main.duration +
                    ps.main.startLifetime.constantMax;

                if (time > max) max = time;
            }

            return max;
        }
    }
}