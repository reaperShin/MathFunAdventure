using UnityEngine;

public static class DeathVFXFactory
{
    // Create a simple particle system at runtime and destroy it after duration
    public static void CreateDeathVFX(Vector3 position, float duration = 2f)
    {
        GameObject go = new GameObject("RuntimeDeathVFX");
        go.transform.position = position;

        var ps = go.AddComponent<ParticleSystem>();
        // Ensure the system is stopped before changing duration or curves
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        var main = ps.main;
        // Prevent auto-play on creation
        main.playOnAwake = false;
        main.loop = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.4f, 1.0f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(1.0f, 3.0f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.2f, 0.6f);
        main.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 0.5f, 0.2f));
        main.duration = duration;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = ps.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new ParticleSystem.Burst[] {
            new ParticleSystem.Burst(0f, 12),
            new ParticleSystem.Burst(0.1f, 6),
        });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.4f;

        var col = ps.colorOverLifetime;
        col.enabled = true;
        Gradient g = new Gradient();
        g.SetKeys(
            new GradientColorKey[] { new GradientColorKey(new Color(1f,0.5f,0.2f), 0f), new GradientColorKey(new Color(1f,0.1f,0.1f), 1f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
        );
        col.color = new ParticleSystem.MinMaxGradient(g);

        var vel = ps.velocityOverLifetime;
        vel.enabled = true;
        vel.space = ParticleSystemSimulationSpace.Local;
        // Ensure all velocity curves use the same mode by assigning MinMaxCurves for x,y,z
        vel.x = new ParticleSystem.MinMaxCurve(0f, 0f);
        vel.y = new ParticleSystem.MinMaxCurve(0.5f, 1.5f);
        vel.z = new ParticleSystem.MinMaxCurve(0f, 0f);

        ps.Play();
        Object.Destroy(go, duration + 0.5f);
    }
}
