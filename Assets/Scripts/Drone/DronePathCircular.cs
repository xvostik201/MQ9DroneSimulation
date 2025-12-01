using UnityEngine;

public class DronePathCircular : DronePathBase
{
    [Header("Circle")]
    [SerializeField] private Vector3 center = Vector3.zero;
    [SerializeField] private float radius = 300f;
    [SerializeField] private float height = 100f;
    [SerializeField] private bool clockwise = true;

    [Header("Timing & View")]
    [SerializeField] private float duration = 20f;           
    [SerializeField] private int gizmoSegments = 120;
    [SerializeField] private Color gizmoColor = Color.cyan;

    public override float Duration => duration;
    private float Dir => clockwise ? -1f : 1f;

    public override void Evaluate(float t, out Vector3 pos, out Vector3 forward)
    {
        float ang = (t / duration) * Mathf.PI * 2f * Dir;
        Vector3 circle = new Vector3(Mathf.Cos(ang), 0f, Mathf.Sin(ang)) * radius;
        pos = center + circle; pos.y = height;

        Vector3 tangent = new Vector3(-Mathf.Sin(ang), 0f, Mathf.Cos(ang)) * Dir;
        forward = tangent.normalized;
    }

    private void OnDrawGizmos()
    {
        DrawPathGizmos(
            (tt) => { Evaluate(tt, out var p, out _); return p; },
            duration, gizmoSegments, gizmoColor
        );
        Gizmos.color = Color.red;
        var c = center; c.y = height;
        Gizmos.DrawSphere(c, 5f);
    }
}