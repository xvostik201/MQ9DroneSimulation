using UnityEngine;

public abstract class DronePathBase : MonoBehaviour
{
    public abstract float Duration { get; }

    public virtual void Init(Transform drone) {}

    public abstract void Evaluate(float t, out Vector3 pos, out Vector3 forward);

    protected void DrawPathGizmos(System.Func<float, Vector3> sampler, float duration, int segments, Color color)
    {
        if (segments < 2) return;
        Gizmos.color = color;
        Vector3 prev = sampler(0f);
        for (int i = 1; i <= segments; i++)
        {
            float tt = duration * (i / (float)segments);
            Vector3 p = sampler(tt);
            Gizmos.DrawLine(prev, p);
            prev = p;
        }
    }
}