using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    [SerializeField] private Color _shotColor;
    private Collider2D _collider;
    public event Action<Vector2> onTileClicked;
    public event Action<Vector2> onTileShot;
    private SpriteRenderer _spriteRenderer;
    private bool _isShot;
    private List<Tile> _nearbyTiles;

    public bool IsShot => _isShot;

    public List<Tile> NearbyTiles
    {
        get => _nearbyTiles;
        set => _nearbyTiles = value;
    }
    
    private void Awake()
    {
        _collider = GetComponent<Collider2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _isShot = false;
        _nearbyTiles = new List<Tile>();
    }

    private void OnMouseOver()
    {
        onTileClicked?.Invoke(transform.localPosition);
    }

    public void ShootTile()
    {
        if (!_isShot)
        {
            onTileShot?.Invoke(transform.localPosition.ToTileCoordinates());
            _spriteRenderer.color = _shotColor;
            _isShot = true;
        }
    }
}
