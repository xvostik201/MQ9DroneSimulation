using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class ArtilleryVisualizer : MonoBehaviour
{
    [Header("Ссылка на ваш Paladin")]
    [SerializeField] private Paladin _paladin;

    private LineRenderer _lr;

    private void Awake()
    {
        _lr = GetComponent<LineRenderer>();
        _lr.useWorldSpace = true;
        _lr.positionCount = 0;
    }

    private void Update()
    {
        if (_paladin == null || _paladin.LastTrajectory == null)
        {
            _lr.positionCount = 0;
            return;
        }

        Vector3[] pts = _paladin.LastTrajectory;
        _lr.positionCount = pts.Length;
        _lr.SetPositions(pts);
    }
}
