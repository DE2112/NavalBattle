using System;
using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;

[Serializable]
public class Ship : MonoBehaviour
{
    [SerializeField] private int _size;
    [SerializeField] private int _direction;
    [SerializeField] private Vector2 _tiledPosition;
    [SerializeField] private int _health;

    [Header("Colors")] 
    [SerializeField] private Color _green;
    [SerializeField] private Color _red;
    private bool _isAllowedToBePlaced;
    private bool _isPlaced;
    private Color _defaultColor;
    private SpriteRenderer _spriteRenderer;
    private bool _isManual;
    private Collider2D _collider;
    private Area _area;
    public event Action<Ship> onDestroyed;

    public int Size => _size;
    public bool IsAllowedToBePlaced => _isAllowedToBePlaced;
    public float Health => _health;
    public Area Area => _area;

    public int Direction
    {
        get => _direction;
        set
        {
            _direction = value % 4;
            transform.UpAt(_direction);
            UpdateShipArea();
        }
    }

    public Vector2 TiledPosition
    {
        get => _tiledPosition;
        set
        {
            _tiledPosition = value;
            transform.localPosition = _tiledPosition.ToLocalCoordinates();
            transform.localPosition += Vector3.back;
            UpdateShipArea();
        }
    }

    void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _collider = GetComponent<Collider2D>();
        _defaultColor = _spriteRenderer.color;
        _spriteRenderer.color = _green;
        _area = new Area();
        UpdateShipArea();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!_isPlaced && _isManual)
        {
            _isAllowedToBePlaced = false;
            _spriteRenderer.color = _red;
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (!_isPlaced && _isManual)
        {
            _isAllowedToBePlaced = true;
            _spriteRenderer.color = _green;
        }
    }

    public void PlaceShip()
    {
        _isPlaced = true;
        _spriteRenderer.color = _defaultColor;
    }

    public void HideShip()
    {
        _spriteRenderer.color = Color.clear;
    }

    public void GetShot()
    {
        _health = _health > 0 ? _health - 1 : 0;

        if (_health == 0)
        {
            _collider.enabled = false;
            _spriteRenderer.color = Color.white;
            onDestroyed?.Invoke(this);
        }
    }

    public void Initialize(int direction, Vector2 position, bool isManual = true)
    {
        _direction = direction;
        _tiledPosition = position;
        _isAllowedToBePlaced = true;
        transform.UpAt(direction);
        _isManual = isManual;
        _health = _size;
        _collider.enabled = true;
    }

    private void UpdateShipArea()
    {
        var tip = Utilities.GetEndPoint(this);
        
        _area.min = new Vector2(Mathf.Min(_tiledPosition.x, tip.x), Mathf.Min(_tiledPosition.y, tip.y));
        _area.max = new Vector2(Mathf.Max(_tiledPosition.x, tip.x), Mathf.Max(_tiledPosition.y, tip.y));
        
        _area.min -= Vector2.one * 0.5f;
        _area.max += Vector2.one * 0.5f;
    }
}
