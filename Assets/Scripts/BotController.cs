using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DefaultNamespace
{
    public class BotController : FieldController
    {
        [SerializeField] private float _period;
        private Tile _nextTarget;
        private bool _isPlayerMove;
        private bool _isEnemyShipDestroyed;
        private event Action onBotShipsArePlaced;
        private event Action onPlayerMoveEnded;
        private event Action onBotMoveEnded;

        private void OnDrawGizmos()
        {
        
            var i = 0;
            foreach (var area in Utilities.Areas)
            {
                Gizmos.color = _colors.Count > 0 ? _colors[i] : Color.red;
                Gizmos.DrawWireCube(area.center - Vector2.one * 0.5f, area.size); 
                i++;
            }
        }

        protected override void Awake()
        {
            base.Awake();
            _enemyField.onGameStart += PlaceShips;
            _enemyField.onShipDestroyed += OnEnemyShipDestroyed;
            onBotShipsArePlaced += MakeAPlayerMove;
            onPlayerMoveEnded += MakeABotMove;
            onBotMoveEnded += MakeAPlayerMove;
            _isPlayerMove = false;
        }

        protected override void Update()
        {
            if (Input.GetKeyDown(KeyCode.Q) && _isGameStarted)
            {
                PlaceShips();
            }
            
            if (!_isGameEnded)
            {
                if (Input.GetMouseButtonDown(0) && _isPlayerMove)
                {
                    var targetPosition = this.GetMousePositionOnField();
                    var targetTile = _tiles.GetTile(targetPosition);
                    var wasTileShotBefore = targetTile.IsShot;
                    targetTile.ShootTile();
                    onPlayerMoveEnded?.Invoke();
                }
            }
        }
        
        private void MakeAPlayerMove()
        {
            _isPlayerMove = true;
        }

        private void MakeABotMove()
        {
            if (_enemyField.ActiveTilesCount > 0)
            {
                StartCoroutine(BotMovesCoroutine());
            }
        }
        
        private IEnumerator BotMovesCoroutine()
        {
            yield return new WaitForSeconds(_period);
            
            _isPlayerMove = false;
            var targetTile = _enemyField.Tiles.SelectTileToShoot(_nextTarget);
            targetTile.ShootTile();
            _nextTarget = _enemyField.Tiles.CheckTilesNearby(targetTile, _enemyField.IsHit, _isEnemyShipDestroyed);
            onBotMoveEnded?.Invoke();
            _isEnemyShipDestroyed = false;
        }

        private void OnEnemyShipDestroyed()
        {
            _isEnemyShipDestroyed = true;
        }

        private void ShootPlayerTile(Vector2 targetPosition)
        {
            var tileToShoot = _enemyField.Tiles.GetTile(targetPosition);
            tileToShoot.ShootTile();
        }

        private void PlaceShips()
        {
            if (_ships.Count == 0) _ships = _shipFactory.MakeShips();
            
            StopCoroutine(ShipPlacementCoroutine());
            StopCoroutine(ColorCoroutine());
        
            var area = new Area {min = Vector2.one * 0.5f, max = Vector2.one * 10.5f};
        
            int i;
            int j = 0;
            do
            {
                Utilities.Areas = new List<Area>();

                i = 0;
                Utilities.ShipEntities = new List<Utilities.ShipEntity>();
                Utilities.FillField(_ships, area, ref i);
                j++;
            } while (i < _ships.Count && j < 100000);
            
            Debug.Log(i);
            Debug.Log(j);
        
            StartCoroutine(ShipPlacementCoroutine());
            StartCoroutine(ColorCoroutine());
        }
        
        

        private IEnumerator ShipPlacementCoroutine()
        {
            foreach (var ship in _ships)
            {
                ship.TiledPosition = Vector2.zero;
                ship.Direction = 0;
            }
        
            foreach (var shipEntity in Utilities.ShipEntities)
            {
                yield return new WaitForSeconds(_period);
                _ships[shipEntity.index].TiledPosition = shipEntity.position;
                _ships[shipEntity.index].Direction = shipEntity.direction;
                _ships[shipEntity.index].PlaceShip();
                //_ships[shipEntity.index].HideShip();
            }

            _isGameStarted = true;
            onBotShipsArePlaced?.Invoke();
        }

        private IEnumerator ColorCoroutine()
        {
            _colors = Utilities.Areas.ConvertAll(area => Color.red);
            for (int i = 0; i < _colors.Count; i++)
            {
                var temp = _colors[i];
                temp.a = 0;
                _colors[i] = temp; 
            }

            yield return new WaitForSeconds(0.5f);
        
            for (int i = 0; i < _colors.Count; i++)
            {
                var temp = _colors[i];
                temp.a = 1;
                _colors[i] = temp;
                yield return new WaitForSeconds(0.5f);
            }
        }
    }
}