#if UNITY_EDITOR

using Grid;
using StrategyGame;
using UnityEditor;
using UnityEngine;

namespace Strategy {
    [CustomEditor(typeof(Piece)), CanEditMultipleObjects]
    public class PieceEditor : Editor {
        private Vector3 _portalTarget;
        private bool _editPortal;
        
        private void OnEnable() {
            _editPortal = false;
            foreach (var t in targets) {
                if (t is Piece piece) {
                    if (piece.TryGetComponent<GridTile>(out var gridTile) &&
                        gridTile.GetProperty(PieceProperty.Portal)) {
                        _editPortal = true;
                        _portalTarget = ToWorld(gridTile.GetData<Portal>());
                    }
                }
            }
        }

        private void OnSceneGUI() {
            if (_editPortal) {
                EditorGUI.BeginChangeCheck();
                _portalTarget = Handles.PositionHandle(_portalTarget, Quaternion.identity);
                Handles.color = Color.cyan;
                Handles.DrawWireDisc(_portalTarget, Vector3.up, 0.4f, 10f);
                if (EditorGUI.EndChangeCheck()) {
                    Undo.RecordObject(target, "Move portal destination");
                    
                    if (target is Piece piece) {
                        if (piece.TryGetComponent<GridTile>(out var gridTile) &&
                            gridTile.GetProperty(PieceProperty.Portal)) {
                            gridTile.SetData(FromWorld(_portalTarget));
                            EditorUtility.SetDirty(target);
                            if (gridTile.IsAttached) {
                                var parent = gridTile.transform.parent.GetComponent<Grid.Grid>();
                                EditorUtility.SetDirty(parent);
                            }
                        }
                    }
                }
            }
        }

        private Portal FromWorld(Vector3 pos) {
            var rounded = Vector2Int.RoundToInt(new Vector2(pos.x, pos.z));
            return new Portal {x = rounded.x, y = rounded.y};
        }

        private Vector3 ToWorld(Portal pos) {
            return new Vector3(pos.x, 0, pos.y);
        }
    }
}
#endif