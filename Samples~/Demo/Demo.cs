using System.Collections.Generic;
using Grid;
using UnityEngine;
using Vectors;

namespace StrategyGame {
    [ExecuteInEditMode]
    [RequireComponent(typeof(Grid.Grid), typeof(VectorRenderer))]
    public class Demo : MonoBehaviour {

        [SerializeField, HideInInspector] private Grid.Grid _grid;
        [SerializeField, HideInInspector] private VectorRenderer _vectors;
        
        void Start() {
            if (!TryGetComponent(out _grid)) {
                _grid = gameObject.AddComponent<Grid.Grid>();
            }
            
            if (!TryGetComponent(out _vectors)) {
                _vectors = gameObject.AddComponent<VectorRenderer>();
            }
        }
    
        void Update() {
            var size = _grid.Size;
            var selection = new List<Vector2Int>();
            var portals = new List<(Vector2Int From, Vector2Int To)>();
            
            for (var i = 0; i < size.x; i++) {
                for (var j = 0; j < size.y; j++) {
                    var pos = new Vector2Int(i, j);
                    if (_grid.GetTileProperty(pos, PieceProperty.Trap)) {
                        selection.Add(pos);
                    }

                    if (_grid.GetTileProperty(pos, PieceProperty.Portal)) {
                        var dest = _grid.GetTileData<Portal>(pos);
                        portals.Add((pos, new Vector2Int(dest.x, dest.y)));
                    }
                }
            }

            if (selection.Count > 0) {
                using (_vectors.Begin()) {
                    DrawSelection(selection, Color.red);
                    foreach (var portal in portals) {
                        _vectors.Draw(
                            new Vector3(portal.From.x, 0, portal.From.y),
                            new Vector3(portal.To.x, 0, portal.To.y),
                            Color.cyan);
                    }
                }
            }
        }

        private void DrawSelection(IEnumerable<Vector2Int> cells, Color color) {
            var edges = new HashSet<(Vector2Int l0, Vector2Int l1)>();
            
            foreach (var cell in cells) {
                var south = (pos: cell, cell + Vector2Int.right);
                var west = (pos: cell, cell + Vector2Int.up);
                var north = (cell + Vector2Int.up, cell + Vector2Int.one);
                var east = (cell + Vector2Int.right, cell + Vector2Int.one);
                if (!edges.Add(south)) edges.Remove(south);
                if (!edges.Add(west)) edges.Remove(west);
                if (!edges.Add(north)) edges.Remove(north);
                if (!edges.Add(east)) edges.Remove(east);
            }
            
            foreach (var edge in edges) {
                _vectors.Draw(
                    ToWorld(edge.l0),
                    ToWorld(edge.l1),
                    color,
                    0.1f
                );
            }
        }

        private Vector3 ToWorld(Vector2Int pos) {
            return new Vector3(pos.x - .5f, 0, pos.y - .5f);
        }
    }
}
