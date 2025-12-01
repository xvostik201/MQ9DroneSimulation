using UnityEngine;

[System.Serializable]
public struct BoundsFloat
{
    [SerializeField] private float _lower;
    [SerializeField] private float _upper;

    public float Lower => _lower;
    public float Upper => _upper;
    public float Length => _upper - _lower;
    public float Center => (_lower + _upper) * 0.5f;
    public float Random => UnityEngine.Random.Range(_lower, _upper);

    public BoundsFloat(float lower, float upper)
    {
        _lower = lower;
        _upper = upper;
    }

    public bool Contains(float value)
    {
        return value >= _lower && value <= _upper;
    }

    public void EnsureOrder()
    {
        if (_lower > _upper)
            (_lower, _upper) = (_upper, _lower);
    }

    public void OnValidate()
    {
        EnsureOrder();
    }

    public override string ToString()
    {
        return $"{_lower}..{_upper}";
    }
}