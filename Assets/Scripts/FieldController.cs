using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DefaultNamespace;
using UnityEngine;
using Random = UnityEngine.Random;

public class FieldController : MonoBehaviour
{
    protected const int GRID_SIZE = 10;
    protected const int SHOTS_PER_MOVE = 1;

    [Header("Tile Properties")]
    [SerializeField] protected Vector3 _firstTilePosition;

    [SerializeField] protected float _tileOffset;

    [Header("Field Properties")]
    [SerializeField] protected Ship[] _shipPrefabs;
    [SerializeField] protected List<Ship> _ships;
    [SerializeField] protected Tile _tilePrefab;
    [SerializeField] protected FieldController _enemyField;
    [SerializeField] protected string _enemyName;
    protected ShipFactory _shipFactory;
    protected List<List<Tile>> _tiles;
    protected Bounds _tileBounds;
    protected Ship _selectedShip;
    protected Vector2 _selectedTilePosition;
    protected List<Color> _colors = new List<Color>();
    protected bool _isHit;
    [SerializeField] protected int _activeTilesCount;
    protected bool _isGameStarted;
    protected bool _isGameEnded;
    public event Action onGameStart;
    public event Action<FieldController> onGameEnded;
    protected int _previousShipCount;
    public event Action onShipDestroyed;
    public bool IsHit => _isHit;
    public int ActiveTilesCount => _activeTilesCount;
    public List<List<Tile>> Tiles => _tiles;

    public string EnemyName => _enemyName;

    protected virtual void Awake()
    {
        _shipFactory = GetComponent<ShipFactory>();
        _ships = new List<Ship>();
        _isGameEnded = false;
    }

    protected virtual void Start()
    {
        SetTilesPosition(false);
        // (Vector2.one * 3).ToLocalCoordinatesTest();
    }
    
    protected virtual void Update()
    {
        if (!_isGameStarted)
        {
            CheckStartInput();
        }
    }

    protected virtual void FixedUpdate()
    {
        if (_isGameStarted && (_activeTilesCount <= 0 || GetShipsHealth() <= 0) && !_isGameEnded)
        {
            _isGameEnded = true;
            onGameEnded?.Invoke(this);
        }
    }

    private float GetShipsHealth()
    {
        return _ships.Sum(ship => ship.Health);
    } 
    
    private void CheckStartInput()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            BuildShip();
        }
    
        if (Input.GetKeyDown(KeyCode.F))
        {
            FixShipPosition();
        }
    
        if (Input.GetKeyDown(KeyCode.R))
        {
            RotateShip();
        }
    
        if (Input.GetKeyDown(KeyCode.D))
        {
            DeleteLastShip();
        }
    }

    public void BuildShip()
    {
        if (_selectedShip == null)
        {
            if (!_shipFactory.AllShipsBuilt)
            {
                _selectedShip =
                    _shipFactory.MakeShip(0, _selectedTilePosition);
            }
        }
    }

    public void FixShipPosition()
    {
        if (_selectedShip != null && _selectedShip.IsAllowedToBePlaced)
        {
            _selectedShip.onDestroyed += DestroyShip;
            _selectedShip.PlaceShip();
            _ships.Add(_selectedShip);
            _selectedShip = null;
        }
        
        if (_shipFactory.AllShipsBuilt)
        {
            _isGameStarted = true;
            onGameStart?.Invoke();
        }
    }

    public void RotateShip()
    {
        if (_selectedShip != null)
        {
            _selectedShip.Direction++;
        }
    }

    public void DeleteLastShip()
    {
        if (_ships.Count > 0)
        {
            _shipFactory.Back();
            Ship lastShip;
            if (_selectedShip != null)
            {
                lastShip = _selectedShip;
                _selectedShip = null;
            }
            else
            {
                lastShip = _ships.Last();
                _ships.Remove(lastShip);
            }
            
            lastShip.onDestroyed -= DestroyShip;
            Destroy(lastShip.gameObject);
        }
    }
    
    private void SetTilesPosition(bool areTilesInitialized)
    {
        _activeTilesCount = GRID_SIZE * GRID_SIZE;
        var currentTilePosition = _firstTilePosition;
        
        if (!areTilesInitialized) _tiles = new List<List<Tile>>();

        for (int i = 0; i < GRID_SIZE; i++)
        {
            var row = new List<Tile>();
            
            for (int j = 0; j < GRID_SIZE; j++)
            {
                if (!areTilesInitialized)
                {
                    var tile = AddTile(currentTilePosition);
                    row.Add(tile);
                }
                else
                {
                    _tiles[i][j].transform.localPosition = currentTilePosition;
                }

                currentTilePosition += (Vector3) (Vector2.right * _tileOffset);
            }

            if (!areTilesInitialized) _tiles.Add(row);

            currentTilePosition += (Vector3) (Vector2.down * _tileOffset + Vector2.left * 10 * _tileOffset);
        }

        for (int i = 0; i < GRID_SIZE; i++)
        {
            for (int j = 0; j < GRID_SIZE; j++)
            {
                if (i > 0) _tiles[i][j].NearbyTiles.Add(_tiles[i-1][j]);
                if (j > 0) _tiles[i][j].NearbyTiles.Add(_tiles[i][j-1]);
                if (i < GRID_SIZE - 1) _tiles[i][j].NearbyTiles.Add(_tiles[i+1][j]);
                if (j < GRID_SIZE - 1) _tiles[i][j].NearbyTiles.Add(_tiles[i][j+1]);
            }
        }
    }

    private Tile AddTile(Vector3 position)
    {
        var tile = Instantiate(_tilePrefab, Vector3.zero, Quaternion.identity, transform);
        tile.transform.localPosition = position;
        tile.onTileClicked += OnTileSelected;
        tile.onTileShot += OnTileShot;
        return tile;
    }

    private void OnTileSelected(Vector2 tilePosition)
    {
        _selectedTilePosition = tilePosition;
        if (_selectedShip != null)
        {
            var shipPosition = _selectedTilePosition.ToTileCoordinates();
            _selectedShip.ClampPositionOnField(shipPosition);
        }
        Debug.Log(tilePosition.ToTileCoordinates());
    }

    protected virtual void OnTileShot(Vector2 tilePosition)
    {
        var isHit = false;
        foreach (var ship in _ships.FindAll(ship => ship != null))
        {
            if (ship.Area.Contains(tilePosition))
            {
                ship.GetShot();
                _activeTilesCount--;
                isHit = true;
            }
        }

        _isHit = isHit;
    }

    protected virtual void DestroyShip(Ship ship)
    {
        onShipDestroyed?.Invoke();
        _ships.Remove(ship);
    }
}
