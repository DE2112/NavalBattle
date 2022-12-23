using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DefaultNamespace;
using UnityEngine;
using Random = UnityEngine.Random;
using Prolog;

public static class Utilities
{
    public const int GRID_SIZE = 10;
    public const float DIFFICULTY = 10;

    public static Vector2 LeftBottomCorner = new Vector2(-1.395f, -1.705f);
    public static Vector2 RightTopCorner = new Vector2(1.705f, 1.395f);
    public static Vector2 Zero = new Vector2(-1.395f, 1.395f);
    public static Vector2 End = new Vector2(1.705f, -1.705f);
    public static List<Area> Areas = new List<Area>();
    public static List<Color> AreaColors = new List<Color>();
    public static List<ShipEntity> ShipEntities = new List<ShipEntity>();

    public static float CellSize = (RightTopCorner.x - LeftBottomCorner.x) / GRID_SIZE;

    private static PrologEngine _prologEngine;

    public struct ShipEntity
    {
        public Vector2 position;
        public int direction;
        public int index;
    }

    public static void InitializePrologEngine()
    {
        _prologEngine = new PrologEngine(persistentCommandHistory: false);
        
        var query = 
            @"toLocalCoordinates(X, Y, Xr, Yr) :-
                Xr is -1.395 + X * 0.31 - 0.155,
                Yr is 1.395 - Y * 0.31 + 0.155.

              min(X,Y,Z) :- (X >= Y -> Z is Y ; Z is X).
              max(X,Y,Z) :- (X >= Y -> Z is X ; Z is Y). 

              clamp(Min,Max,X,Res) :- clampInternal(Min,Max,X,Y,Z),
                  Res is Z.

              clampInternal(Min,Max,X,Y,Z) :-
                  Y is max(Min,X),
                  Z is min(Max,Y).

              toTileCoordinates(X, Y, Xr, Yr) :- 
                  clamp(-1.395, 1.705, X, ClampedX),
                  clamp(-1.705, 1.395, Y, ClampedY),
                  Xr is floor(abs(ClampedX + 1.395) / 0.31) + 1,
                  Yr is floor(abs(ClampedY - 1.395) / 0.31) + 1.
";
        _prologEngine.ConsultFromString(query);
        var testQuery = $"toTileCoordinates({(-0.62f).ToStringSafe()}, {0f.ToStringSafe()}, Xr, Yr).";
        var solution = _prologEngine.GetFirstSolution(testQuery);
        
        Debug.Log(solution.ParseVector2());
    }

    private static string ToStringSafe(this float value)
    {
        return value.ToString().Replace(",", ".");
    }

    private static Vector2 ParseVector2(this PrologEngine.ISolution solutionRaw)
    {
        var result = new Vector2();
        var varValues = solutionRaw.VarValuesIterator.ToList();
        result.x = float.Parse(varValues[0].Value.ToString().Replace(".", ","));
        result.y = float.Parse(varValues[1].Value.ToString().Replace(".", ","));
        
        return result;
    }

    public static Vector2 ToTileCoordinates(this Vector2 value)
    {
        // var result = value.Clamp(LeftBottomCorner, RightTopCorner);
        //
        // result = new Vector2(Mathf.Abs(result.x - Zero.x), Mathf.Abs(result.y - Zero.y));
        // result /= CellSize;
        // result = result.Floor();
        // result += Vector2.one;
        //
        // return result;
        
        var result = new Vector2();
        try
        {
            var solution = _prologEngine.GetFirstSolution($"toTileCoordinates({value.x.ToStringSafe()}, {value.y.ToStringSafe()}, Xr, Yr).");
            result = solution.ParseVector2();
            Debug.Log(result);
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }

        return result;
    }

    public static Vector3 ToTileCoordinates(this Vector3 value)
    {
        var temp = (Vector2) value;
        Vector3 result = temp.ToTileCoordinates();
        result.z = -1;
        return result;
    }

    public static Vector2 ToLocalCoordinates(this Vector2 value)
    {
        // var result = value;
        // result *= CellSize;
        // result = new Vector2(Zero.x + result.x, Zero.y - result.y);
        // result += (Vector2.up + Vector2.left) * (CellSize / 2f);
        // return result

        var result = new Vector2();
        try
        {
            var solution = _prologEngine.GetFirstSolution($"toLocalCoordinates({value.x}, {value.y}, Xr, Yr).");
            result = solution.ParseVector2();
            Debug.Log(result);
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }

        return result;
    }

    public static Vector3 ToLocalCoordinates(this Vector3 value)
    {
        var temp = (Vector2) value;
        Vector3 result = temp.ToLocalCoordinates();
        result.z = -1;
        return result;
    }

    public static Vector2 Clamp(this Vector2 value, Vector2 min, Vector2 max)
    {
        return new Vector2(
            Mathf.Clamp(value.x, min.x, max.x),
            Mathf.Clamp(value.y, min.y, max.y)
        );
    }

    public static Vector2 ClampDefault(this Vector2 value, Area area)
    {
        return value.Clamp(area.min, area.max);
    }

    public static Vector2 Floor(this Vector2 value)
    {
        return new Vector2(Mathf.Floor(value.x), Mathf.Floor(value.y));
    }

    public static void UpAt(this Transform transform, int direction)
    {
        switch (direction)
        {
            case 0:
                transform.up = Vector3.down;
                break;
            case 1:
                transform.up = Vector2.right;
                break;
            case 2:
                transform.up = Vector2.up;
                break;
            case 3:
                transform.up = Vector2.left;
                break;
        }
    }

    public static Vector2 GetDirectionVector(int direction)
    {
        var result = new Vector2();
        switch (direction)
        {
            case 0:
                result = Vector2.up;
                break;
            case 1:
                result = Vector2.right;
                break;
            case 2:
                result = Vector2.down;
                break;
            case 3:
                result = Vector2.left;
                break;
        }

        return result;
    }

    public static Vector2 GetMousePosition()
    {
        return Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    public static void ClampPositionOnField(this Ship ship, Vector2 tiledPosition)
    {
        var resultPosition = tiledPosition;
        var directionVector = GetDirectionVector(ship.Direction);

        var endPoint = resultPosition + directionVector * ship.Size;

        if (endPoint.x < 1 || endPoint.x > 10)
        {
            resultPosition.x = endPoint.x > 1 ? (11 - ship.Size) : ship.Size;
        }

        if (endPoint.y < 1 || endPoint.y > 10)
        {
            resultPosition.y = endPoint.y > 1 ? (11 - ship.Size) : ship.Size;
        }

        ship.TiledPosition = resultPosition;
    }

    public static Vector2 GetMousePositionOnField(this FieldController field)
    {
        var mousePosition = (Vector2) field.transform.InverseTransformPoint(GetMousePosition());
        Debug.Log(mousePosition.ToTileCoordinates());
        return mousePosition.ToTileCoordinates();
    }

    public static void FillField(List<Ship> ships, Area area, ref int i)
    {
        const int UPPER_LIMIT = 10000;

        if (i == ships.Count - 1)
        {
            ;
        }

        if (i >= ships.Count) return;
        var currentShip = ships[i];

        var isAbleToPlace = area.IsAbleToPlaceTheShip(ships[i]);
        if (!isAbleToPlace) return;

        var j = 0;
        var k = 0;
        var l = 0;

        Vector2 possibleShipPosition, endPoint;
        var shipEntity = new ShipEntity();
        shipEntity.index = i;
        do
        {
            k = 0;
            do
            {
                possibleShipPosition = GetRandomPointInArea(area);
                k++;
            } while (possibleShipPosition.IsOutsideTheArea(area) && k < UPPER_LIMIT); //говнокод, поэтому в цикле

            if (possibleShipPosition.IsOutsideTheArea(area)) return;

            shipEntity.position = possibleShipPosition;
            // ships[i].TiledPosition = possibleShipPosition;

            var direction = 0;
            do
            {
                //ships[i].Direction = direction;
                shipEntity.direction = direction;
                endPoint = GetEndPoint(shipEntity.position, shipEntity.direction, ships[i].Size);
                direction++;
            } while (direction < 4 && endPoint.IsOutsideTheArea(area));


            if (!endPoint.IsOutsideTheArea(area)) break;
            j++;
        } while ((possibleShipPosition.IsOutsideTheArea(area) || endPoint.IsOutsideTheArea(area)) && j < UPPER_LIMIT);

        if (j >= UPPER_LIMIT)
        {
            return;
        }

        if (k >= 10000)
        {
            Debug.Log($"Ship #{i} outside of bounds");
        }

        if (l >= 10000)
        {
            Debug.Log($"Ship #{i}'s tip outside of bounds");
        }

        var subAreas = new List<Area>();
        try
        {
            subAreas = area.DissectToSubAreas(shipEntity.position, shipEntity.direction, ships[i].Size);

            if (subAreas.Count == 0) return;
            ShipEntities.Add(shipEntity);
            i++;

            foreach (var subArea in subAreas)
            {
                FillField(ships, subArea, ref i);
            }
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }

    public static List<Area> DissectToSubAreas(this Area area, Vector2 origin, int direction, int size)
    {

        var endPoint = GetEndPoint(origin, direction, size);
        var min = new Vector2(Mathf.Min(origin.x, endPoint.x), Mathf.Min(origin.y, endPoint.y));
        min = (min - Vector2.one * 1.5f).ClampDefault(area);

        var max = new Vector2(Mathf.Max(origin.x, endPoint.x), Mathf.Max(origin.y, endPoint.y));
        max = (max + Vector2.one * 1.5f).ClampDefault(area);

        var subAreas = new List<Area>();

        //test
        // var shipArea = new Area {Min = min, Max = max};
        // subAreas.Add(shipArea);

        var top = new Area {min = new Vector2(min.x, max.y), max = area.max};
        subAreas.Add(top);
        var left = new Area {min = new Vector2(max.x, area.min.y), max = new Vector2(area.max.x, max.y)};
        subAreas.Add(left);
        var bottom = new Area {min = area.min, max = new Vector2(max.x, min.y)};
        subAreas.Add(bottom);
        var right = new Area {min = new Vector2(area.min.x, min.y), max = new Vector2(min.x, area.max.y)};
        subAreas.Add(right);

        subAreas.RemoveAll(subArea => subArea.isBadArea);
        subAreas = subAreas.OrderByDescending(subArea => subArea.sqr).ToList();
        Areas.AddRange(subAreas);

        return subAreas;
    }

    private static Vector2 GetRandomPointInArea(Area area)
    {
        return new Vector2(Mathf.Floor(Random.Range(area.min.x, area.max.x)),
                Mathf.Floor(Random.Range(area.min.y, area.max.y)))
            .Clamp(area.min + Vector2.one * 0.5f, area.max - Vector2.one * 0.5f);
    }

    private static bool IsAbleToPlaceTheShip(this Area area, Ship ship)
    {
        var result = !(ship.Size > area.size.x && ship.Size > area.size.y) && (area.sqr >= ship.Size);
        return result;
    }

    private static bool IsOutsideTheBounds(this float value, float min, float max)
    {
        return !(value >= min && value <= max);
    }

    private static Vector2 GetEndPoint(Vector2 originPosition, int direction, int size)
    {
        return originPosition + GetDirectionVector(direction) * (size - 1);
    }
    
    public static Vector2 GetEndPoint(Ship ship)
    {
        return ship.TiledPosition + GetDirectionVector(ship.Direction) * (ship.Size - 1);
    }

    private static bool IsOutsideTheArea(this Vector2 value, Area area)
    {
        // return !(value.x >= min.x && value.x <= max.x) || !(value.y >= min.y && value.y <= max.y);
        return value.x.IsOutsideTheBounds(area.min.x, area.max.x) || value.y.IsOutsideTheBounds(area.min.y, area.max.y);
    }

    public static Tile GetTile(this List<List<Tile>> tiles, Vector2 position)
    {
        var xIndex = (int)position.x - 1;
        var yIndex = (int)position.y - 1;
        Debug.Log($"TILE POSITION {xIndex}, {yIndex}");

        try
        {
            var tile = tiles[yIndex][xIndex];
            return tile;
        }
        catch (IndexOutOfRangeException e)
        {
            Debug.Log(e);
            throw;
            return null;
        }
    }

    public static Tile SelectTileToShoot(this List<List<Tile>> playerFieldTiles, Tile target)
    {
        if (target == null)
        {
            int randomIndex;

            int total = 0;
            foreach (var row in playerFieldTiles)
            {
                total += row.Count(tile => !tile.IsShot);
            }
            
            int k = 0;
            while (k < 100000)
            {
                int i = 0;
                randomIndex = Random.Range(0, total - 1);
                foreach (var row in playerFieldTiles)
                {
                    foreach (var tile in row)
                    {
                        if (i == randomIndex && !tile.IsShot)
                        {
                            return tile;
                        }
                        i++;
                    }
                }
            }

            foreach (var row in playerFieldTiles)
            {
                if (row.Count(tile => !tile.IsShot) > 0)
                    return row.Find(tile => !tile.IsShot);
            }
        }
        
        return target;
    }

    public static Tile CheckTilesNearby(this List<List<Tile>> playerFieldTiles, Tile shotTile, bool isHit, bool isShipDestroyed)
    {
        if (!isShipDestroyed)
        {
            if (isHit)
            {
                //shotTile.NearbyTiles.RemoveAll(tile => tile.IsShot);
                return shotTile.GetRandomTileNearby();
            }
            else
            {
                var shotTilesNearby = shotTile.NearbyTiles.FindAll(tile => tile.IsShot);

                if (shotTilesNearby.Count > 0)
                {
                    return shotTilesNearby[0].GetRandomTileNearby();
                }
            }
        }

        return null;
    }

    private static Tile GetRandomTileNearby(this Tile tile)
    {
        var notShotTilesNearby = new List<Tile>();
        notShotTilesNearby = tile.NearbyTiles.FindAll(tile => !tile.IsShot);
        
        if (notShotTilesNearby.Count > 0)
        {
            var randomIndex = Random.Range(0, notShotTilesNearby.Count - 1);
            return notShotTilesNearby[randomIndex];
        }

        return null;
    }

    public static void PrologTest()
    {
        _prologEngine.ConsultFromString("human(socrates).");
        _prologEngine.ConsultFromString("mortal(X) :- human(X).");
        var solution = _prologEngine.GetFirstSolution(query: "mortal(socrates).");
        Debug.Log(solution.Solved);
    }
}