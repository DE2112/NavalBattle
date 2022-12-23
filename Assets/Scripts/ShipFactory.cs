using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipFactory : MonoBehaviour
{
    [SerializeField] private Ship[] _shipPrefabs;
    private int _shipIndex;
    private bool _allShipsBuilt;

    public bool AllShipsBuilt => _allShipsBuilt;

    void Start()
    {
        _shipIndex = 0;
        _allShipsBuilt = false;
    }

    public Ship MakeShip(int direction, Vector2 position)
    {
        var ship = Instantiate(_shipPrefabs[_shipIndex], position, Quaternion.identity, transform);
        ship.Initialize(direction, position.ToTileCoordinates());

        _shipIndex++;

        if (_shipIndex >= _shipPrefabs.Length)
        {
            _allShipsBuilt = true;
        }
        
        return ship;
    }

    public void Back()
    {
        _shipIndex--;
    }

    public List<Ship> MakeShips()
    {
        var ships = new List<Ship>();
        foreach (var shipPrefab in _shipPrefabs)
        {
            var ship = Instantiate(_shipPrefabs[_shipIndex], Vector3.zero, Quaternion.identity, transform);
            ship.Initialize(0, Vector2.zero, false);
            ships.Add(ship);
            _shipIndex++;
        }

        _allShipsBuilt = true;

        return ships;
    }
}
