using System.Collections.Generic;
using Grid;
using StrategyGame.Geometry;
using UnityEngine;

namespace StrategyGame {
    [ExecuteInEditMode]
    [RequireComponent(typeof(GridTile), typeof(MeshFilter), typeof(MeshRenderer))]
    public class Piece : MonoBehaviour {

        [Header("Configuration")] 
        [SerializeField] private PieceConfig _config;
        [SerializeField] private MeshCache _cache;
        
        //private bool _requireUpdate;

        [SerializeField, HideInInspector] private MeshFilter _filter;
        [SerializeField, HideInInspector] private MeshRenderer _renderer;
        [SerializeField, HideInInspector] private GridTile _gridTile;

        // private void Start() {
        //     _requireUpdate = true;
        // }
        
        private void OnEnable() {
            if (!TryGetComponent(out _filter)) {
                _filter = gameObject.AddComponent<MeshFilter>();
            }
            
            if (!TryGetComponent(out _renderer)) {
                _renderer = gameObject.AddComponent<MeshRenderer>();
            }
            
            if (!TryGetComponent(out _gridTile)) {
                _gridTile = gameObject.AddComponent<GridTile>();
            }

            //_requireUpdate = true;
        }

        // private void OnValidate() {
        //     _requireUpdate = true;
        // }

        private void Update() {
            if (_cache == null) return;
            if (_config == null) return;
            if (_gridTile == null) return;
            
            // if (!_requireUpdate) return;
            // _requireUpdate = false;

            var key = _gridTile.GetBitset();
            _cache.Get(key, out var mesh, out var materials, () => {
                var mb = MeshBuilder.LoadFrom(_config._emptyPrefab);
                if (_gridTile.GetProperty(PieceProperty.Obstacle)) {
                    mb.Add(MeshBuilder.LoadFrom(_config._obstaclePrefab));
                }
                if (_gridTile.GetProperty(PieceProperty.Trap)) {
                    mb.Add(MeshBuilder.LoadFrom(_config._trapPrefab));
                }
                if (_gridTile.GetProperty(PieceProperty.Terrain)) {
                    mb.Add(MeshBuilder.LoadFrom(_config._terrainPrefab));
                }
                if (_gridTile.GetProperty(PieceProperty.Portal)) {
                    mb.Add(MeshBuilder.LoadFrom(_config._portalPrefab));
                }
                if (_gridTile.GetProperty(PieceProperty.CheckPoint)) {
                    mb.Add(MeshBuilder.LoadFrom(_config._checkPointPrefab));
                }
                if (_gridTile.GetProperty(PieceProperty.Agent)) {
                    mb.Add(MeshBuilder.LoadFrom(_config._agentPrefab));
                }

                var mesh = new Mesh{name = $"Piece {key}"};
                mesh.MarkDynamic();
                
                var mats = new List<Material>();
                mb.Build(mesh, mats);
                return (mesh, mats.ToArray());
            });
            
            _filter.sharedMesh = mesh;
            _renderer.sharedMaterials = materials;
        }
    }
}
