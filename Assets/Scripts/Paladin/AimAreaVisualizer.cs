// using UnityEngine;
//
// [RequireComponent(typeof(LineRenderer))]
// public class AimAreaVisualizer : MonoBehaviour
// {
//     [Header("Paladin")]
//     [SerializeField] private Paladin _paladin;
//
//     [Header("Настройки круга")]
//     [SerializeField] private int _circleSegments = 36;
//
//     private LineRenderer _lr;
//     private Vector3 _lastTarget = Vector3.zero;
//
//     private void Awake()
//     {
//         _lr = GetComponent<LineRenderer>();
//         _lr.useWorldSpace = true;
//         _lr.loop = true;
//         _lr.positionCount = 0;
//     }
//
//     private void Update()
//     {
//         if (_paladin == null)
//             return;
//         if (_paladin.Target != _lastTarget)
//         {
//             _lastTarget = _paladin.Target;
//             DrawSpreadCircle(_lastTarget);
//         }
//     }
//
//     private void DrawSpreadCircle(Vector3 center)
//     {
//         Vector2 a = new Vector2(_paladin.ShootPoint.position.x, _paladin.ShootPoint.position.z);
//         Vector2 b = new Vector2(center.x, center.z);
//         float horizDist = Vector2.Distance(a, b);
//
//         float radius = Mathf.Tan((_paladin.MaxSpreadAngle * 2) * Mathf.Deg2Rad) * horizDist;
//
//         Terrain terrain = Terrain.activeTerrain;
//         Transform tTrans = terrain != null ? terrain.transform : null;
//
//         var pts = new Vector3[_circleSegments + 1];
//         for (int i = 0; i <= _circleSegments; i++)
//         {
//             float theta = 2f * Mathf.PI * i / _circleSegments;
//             float x = Mathf.Cos(theta) * radius;
//             float z = Mathf.Sin(theta) * radius;
//             Vector3 worldPos = new Vector3(center.x + x, center.y + 50f, center.z + z);
//
//             if (terrain != null)
//             {
//                 float y = terrain.SampleHeight(worldPos) + tTrans.position.y;
//                 pts[i] = new Vector3(worldPos.x, y + 0.02f, worldPos.z);
//             }
//             else
//             {
//                 if (Physics.Raycast(worldPos, Vector3.down, out RaycastHit hit, 100f))
//                     pts[i] = hit.point + Vector3.up * 0.02f;
//                 else
//                     pts[i] = center;
//             }
//         }
//
//         _lr.positionCount = pts.Length;
//         _lr.SetPositions(pts);
//     }
//
// }
