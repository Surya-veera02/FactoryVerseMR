
using UnityEngine;

public class FactorySourceSmoke : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private FactoryProductionController productionController;

    [SerializeField]
    private ParticleSystem smokeParticles;

    private bool wasRunning;

    private void Awake()
    {
        if (productionController == null)
            productionController =
                GetComponentInParent<FactoryProductionController>();

        if (smokeParticles == null)
            smokeParticles =
                GetComponentInChildren<ParticleSystem>(true);
    }

    private void Start()
    {
        if (smokeParticles != null)
            smokeParticles.Stop(
                true,
                ParticleSystemStopBehavior.StopEmittingAndClear
            );
    }

    private void Update()
    {
        if (productionController == null ||
            smokeParticles == null)
            return;

        bool isRunning =
            productionController.CurrentState ==
            FactoryProductionController.ProductionState.Running;

        // Only respond when the production state changes.
        if (isRunning == wasRunning)
            return;

        wasRunning = isRunning;

        if (isRunning)
        {
            smokeParticles.Play();
        }
        else
        {
            // Stop new smoke but allow existing particles to fade.
            smokeParticles.Stop(
                true,
                ParticleSystemStopBehavior.StopEmitting
            );
        }
    }
}
