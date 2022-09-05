using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Game_Turn_Based
{
    public class GridManager : MonoBehaviour
    {
        public static GridManager Instance;
        [SerializeField] private int _width, _height;

        [SerializeField] private Tile _tilePrefab;

        //[SerializeField] private Transform _cam;
        [SerializeField] private Transform parentTile;

        private Dictionary<Vector2, Tile> _tiles;
        int i = -1, j = -1;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }

        void Start()
        {
            //GenerateGrid();
        }

        public void GenerateGrid()
        {
            _tiles = new Dictionary<Vector2, Tile>();
            for (int x = -_width / 2; x < _width / 2; x++)
            {
                i++;
                j = 0;
                for (int y = -_height / 2; y < _height / 2; y++)
                {
                    j++;
                    var spawnedTile = Instantiate(_tilePrefab, new Vector3(x, y), Quaternion.identity, parentTile);
                    spawnedTile.name = $"Tile {i} {j}";

                    var isOffset = (x % 2 == 0 && y % 2 != 0) || (x % 2 != 0 && y % 2 == 0);
                    spawnedTile.Init(isOffset);
                    _tiles[new Vector2(x, y)] = spawnedTile;
                }
            }

            //_cam.transform.position = new Vector3((float)_width/2 -0.5f, (float)_height / 2 - 0.5f,-10);
            GameManager.Instance.ChangeState(GameManager.GameState.SpawnAttacker);
            GameManager.Instance.ChangeState(GameManager.GameState.SpawnDefender);
        }

        public Tile GetAttackerSpawnTile()
        {
            //return _tiles.Where(t => t.Key.x < _width / 2).OrderBy(t => UnityEngine.Random.value).FirstOrDefault().Value;
            //Debug.Log(_tiles);
            return _tiles.OrderBy(t => UnityEngine.Random.value).FirstOrDefault().Value;
        }

        public Tile GetDefenderSpawnTile()
        {
            //return _tiles.Where(t => t.Key.x > _width / 2).OrderBy(t => UnityEngine.Random.value).FirstOrDefault().Value;
            return _tiles.OrderBy(t => UnityEngine.Random.value).FirstOrDefault().Value;
            //return _tiles
        }

        public Tile GetTileAtPosition(Vector2 pos)
        {
            if (_tiles.TryGetValue(pos, out var tile)) return tile;
            return null;
        }

        public int getWidth()
        {
            return _width;
        }

        public int getHeight()
        {
            return _height;
        }
    }
}