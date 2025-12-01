using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MapHUD : MonoBehaviour
{
    [Header("World & Map Sizes")]
    [SerializeField] private Vector2 _worldSize = new(2000f, 2000f);
    [SerializeField] private Vector2 _mapSize   = new(900f, 900f);
    [SerializeField] private Vector2 _droneIconSizeByWorld = new(10f, 10f);
    [SerializeField] private Vector2 _paladinIconSizeByWorld = new(10f, 10f);
    [SerializeField] private float _lookAreaError = 90f;

    [Header("References")]
    [SerializeField] private RectTransform _mapRectTransform;
    [SerializeField] private RectTransform _droneIconRectTransform;
    [SerializeField] private GameObject _paladinIconPrefab;
    [SerializeField] private RectTransform _markIconRectTransform;
    [SerializeField] private RectTransform _droneToMarkLine;
    [SerializeField] private RectTransform _droneLookArea;
    [SerializeField] private RectTransform _segmentPrefab;
    [SerializeField] private RectTransform _segmentsParent;
    [SerializeField] private RectTransform _paladinsStatusText;
    [SerializeField] private RectTransform _impactZoneRect;
    [SerializeField] private Image _paladinStatusImage;

    [Header("Path Settings")]
    [SerializeField, Range(4, 256)] private int _pathSamples = 64;
    [SerializeField, Range(0.1f, 10f)] private float _pathThickness = 2f;

    private DroneController _drone;
    private DroneSensorSystem _droneSensorSystem;
    private DronePathBase _pathForMap;
    private PaladinManager _paladinManager;
    private readonly List<RectTransform> _segments = new();

    public void Init(DroneController drone, DroneSensorSystem droneSensorSystem, PaladinManager paladinManager)
    {
        _drone = drone;
        _droneSensorSystem = droneSensorSystem;
        _paladinManager = paladinManager;
        
        _drone.OnPositionChanged += UpdateDroneIconPosition;
        _drone.OnPathChanged += HandlePathChanged;
        _drone.OnMarkPlaced += UpdateMarkPosition;

        _droneSensorSystem.OnYawChanged += UpdateDroneLookArea;

        _paladinManager.OnPaladinInit += CreatePaladinIcon;
        _paladinManager.OnGroupStatusChanged += UpdateGroupStatusUI;
        
        HandlePathChanged(_drone.CurrentPath);
        UpdateDroneIconPosition(_drone.transform.position);
    }

    private void OnDisable()
    {
        if (_drone == null) return;

        _drone.OnPositionChanged -= UpdateDroneIconPosition;
        _drone.OnPathChanged -= HandlePathChanged;
        _drone.OnMarkPlaced -= UpdateMarkPosition;
        
        if(_droneSensorSystem != null)
            _droneSensorSystem.OnYawChanged -= UpdateDroneLookArea;

        if (_paladinManager != null)
        {
            _paladinManager.OnPaladinInit -= CreatePaladinIcon;
            _paladinManager.OnGroupStatusChanged -= UpdateGroupStatusUI;
            
        }
    }

    private void HandlePathChanged(DronePathBase path)
    {
        _pathForMap = path;
        DrawPathOnMap();
    }

    private void UpdateGroupStatusUI(PaladinStatus status)
    {
        Color color = status switch
        {
            PaladinStatus.Idle   => Color.gray,
            PaladinStatus.Aiming => Color.yellow,
            PaladinStatus.Aimed  => new Color(1f, 0.5f, 0f),
            PaladinStatus.Fired  => Color.green,
            _ => Color.white
        };

        _paladinStatusImage.color = color;
        _paladinsStatusText.GetComponent<TMPro.TMP_Text>().text = status.ToString().ToUpper();
    }


    private void DrawPathOnMap()
    {
        if (_pathForMap == null || _segmentPrefab == null || _pathSamples < 2)
            return;

        Vector2[] points = new Vector2[_pathSamples];
        for (int i = 0; i < _pathSamples; i++)
        {
            float t = _pathForMap.Duration * (i / (float)(_pathSamples - 1));
            _pathForMap.Evaluate(t, out var worldPos, out _);
            points[i] = WorldToMap(worldPos);
        }

        while (_segments.Count < _pathSamples - 1)
        {
            var seg = Instantiate(_segmentPrefab, _segmentsParent);
            seg.gameObject.SetActive(true);
            _segments.Add(seg);
        }

        for (int i = 0; i < points.Length - 1; i++)
        {
            Vector2 start = points[i];
            Vector2 end   = points[i + 1];
            Vector2 dir   = end - start;
            float length  = dir.magnitude;

            var seg = _segments[i];
            seg.gameObject.SetActive(true);

            seg.anchoredPosition = start + dir * 0.5f;
            seg.localRotation = Quaternion.Euler(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);

            seg.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, length);
            seg.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _pathThickness);
        }

        for (int i = points.Length - 1; i < _segments.Count; i++)
            _segments[i].gameObject.SetActive(false);
    }
    
    public void ResetMap()
    {
        _markIconRectTransform.gameObject.SetActive(false);
        HideImpactZone();
        _droneToMarkLine.gameObject.SetActive(false);

        foreach (Transform child in _mapRectTransform)
        {
            if (child.CompareTag("PaladinIcon"))
                Destroy(child.gameObject);
        }
    }

    private void UpdateDroneIconPosition(Vector3 position)
    {
        Vector2 mapPos = WorldToMap(position);
        _droneIconRectTransform.anchoredPosition = mapPos;

        float yaw = _drone.transform.eulerAngles.y;
        _droneIconRectTransform.localRotation = Quaternion.Euler(0f, 0f, -yaw);
    
        _droneIconRectTransform.sizeDelta = WorldSizeToMap(_droneIconSizeByWorld);

        UpdateDroneToMarkLine();
    }

    private void UpdateDroneToMarkLine()
    {
        if (_droneToMarkLine == null || !_markIconRectTransform.gameObject.activeSelf)
        {
            if (_droneToMarkLine != null)
                _droneToMarkLine.gameObject.SetActive(false);
            return;
        }

        _droneToMarkLine.gameObject.SetActive(true);

        Vector2 start = _droneIconRectTransform.anchoredPosition;
        Vector2 end   = _markIconRectTransform.anchoredPosition;
        Vector2 dir   = end - start;
        float length  = dir.magnitude;

        _droneToMarkLine.anchoredPosition = start + dir * 0.5f;
        _droneToMarkLine.localRotation = Quaternion.Euler(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
        _droneToMarkLine.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, length);
    }

    private Vector2 WorldSizeToMap(Vector2 worldSize)
    {
        float w = (worldSize.x / _worldSize.x) * _mapSize.x;
        float h = (worldSize.y / _worldSize.y) * _mapSize.y;
        return new Vector2(w, h);
    }

    private void CreatePaladinIcon(Vector3 worldPosition)
    {
        GameObject go = Instantiate(_paladinIconPrefab, _mapRectTransform);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = WorldToMap(worldPosition);
        rt.sizeDelta = WorldSizeToMap(_paladinIconSizeByWorld);
    }
    private void UpdateDroneLookArea(float yaw)
    {
        _droneLookArea.localRotation = Quaternion.Euler(0, 0, -yaw + _lookAreaError);
    }
    public bool TryGetMapClickPosition(out Vector3 worldPos)
    {
        worldPos = Vector3.zero;
        Vector2 mousePos = Mouse.current.position.ReadValue();

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _mapRectTransform, mousePos, null, out Vector2 localPoint))
        {
            if (IsPointInsideMap(localPoint))
            {
                worldPos = MapToWorld(localPoint);
                return true;
            }
        }

        return false;
    }

    public void UpdateMarkPosition(Vector3 position)
    {
        _markIconRectTransform.gameObject.SetActive(true);
        Vector2 mapPos = WorldToMap(position);
        _markIconRectTransform.anchoredPosition = mapPos;
        UpdateDroneToMarkLine();
    }
    
    public void ShowImpactZone(Vector3 centerWorld, float radiusWorld)
    {
        if (_impactZoneRect == null)
            return;

        _impactZoneRect.gameObject.SetActive(true);

        Vector2 mapPos = WorldToMap(centerWorld);
        _impactZoneRect.anchoredPosition = mapPos;

        float sizeX = (radiusWorld / _worldSize.x) * _mapSize.x * 2f;
        float sizeY = (radiusWorld / _worldSize.y) * _mapSize.y * 2f;

        _impactZoneRect.sizeDelta = new Vector2(sizeX, sizeY);
    }

    public void HideImpactZone()
    {
        if (_impactZoneRect != null)
            _impactZoneRect.gameObject.SetActive(false);
    }

    public void TryPlaceMark()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _mapRectTransform, mousePos, null, out Vector2 localPoint))
        {
            if (IsPointInsideMap(localPoint))
            {
                PlaceMark(localPoint);
            }
        }
    }
    private bool IsPointInsideMap(Vector2 localPoint)
    {
        Vector2 halfSize = _mapRectTransform.rect.size * 0.5f;

        bool insideX = localPoint.x >= -halfSize.x && localPoint.x <= halfSize.x;
        bool insideY = localPoint.y >= -halfSize.y && localPoint.y <= halfSize.y;

        return insideX && insideY;
    }

    private void PlaceMark(Vector2 localPoint)
    {
        _markIconRectTransform.gameObject.SetActive(true);
        _markIconRectTransform.anchoredPosition = localPoint;
    }

    private Vector3 MapToWorld(Vector2 mapPoint)
    {
        float nx = (mapPoint.x + _mapSize.x * 0.5f) / _mapSize.x;
        float ny = (mapPoint.y + _mapSize.y * 0.5f) / _mapSize.y;

        float worldX = Mathf.Lerp(-_worldSize.x * 0.5f, _worldSize.x * 0.5f, nx);
        float worldZ = Mathf.Lerp(-_worldSize.y * 0.5f, _worldSize.y * 0.5f, ny);

        return new Vector3(worldX, 0f, worldZ);
    }
    
    private Vector2 WorldToMap(Vector3 worldPos)
    {
        float nx = Mathf.InverseLerp(-_worldSize.x * 0.5f, _worldSize.x * 0.5f, worldPos.x);
        float ny = Mathf.InverseLerp(-_worldSize.y * 0.5f, _worldSize.y * 0.5f, worldPos.z);

        return new Vector2(
            nx * _mapSize.x - _mapSize.x * 0.5f,
            ny * _mapSize.y - _mapSize.y * 0.5f
        );
    }
}
