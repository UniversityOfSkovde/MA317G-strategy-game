using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace StrategyGame {
    [CreateAssetMenu(menuName = "Strategy Game/New Mesh Cache...")]
    public class MeshCache : ScriptableObject, ISerializationCallbackReceiver {

        [Serializable]
        private class Batch {
            public uint Key;
            public Mesh Mesh;
            public Material[] Materials;
            public string MeshName;
            
            public Batch(uint key, Mesh mesh, Material[] materials, string meshName) {
                Key = key;
                Mesh = mesh;
                Materials = materials;
                MeshName = meshName;
            }
        }
        
        private Dictionary<uint, Batch> _cachedData;
        private Dictionary<string, Mesh> _savedMeshes;

        public MeshCache() {
            _cachedData = new Dictionary<uint, Batch>();
            _savedMeshes = new Dictionary<string, Mesh>();
        }

        private void OnEnable() {
            _savedMeshes.Clear();
            foreach (var savedMesh in AssetDatabase
                    .LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(this))
                    .OfType<Mesh>()) {
                _savedMeshes.Add(savedMesh.name, savedMesh);
            }
        }

        public void Clear() {
            _cachedData.Clear();
            _savedMeshes.Clear();
            _serializedData.Clear();
            
            foreach (var savedMesh in AssetDatabase
                .LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(this))
                .OfType<Mesh>().ToList()) {
                AssetDatabase.RemoveObjectFromAsset(savedMesh);
            }
            
            AssetDatabase.SaveAssets();
        }

        public void Get(uint key, out Mesh mesh, out Material[] materials, Func<(Mesh, Material[])> builder) {
            if (_cachedData.TryGetValue(key, out var existing)) {
                if (existing.Mesh == null) {
                    if (_savedMeshes.TryGetValue(existing.MeshName, out var savedMesh)) {
                        existing.Mesh = savedMesh;
                    }
                }
                mesh = existing.Mesh;
                materials = existing.Materials;
                return;
            }

            var created = builder();
            mesh = created.Item1;
            materials = created.Item2;
            
            var batch = new Batch(key, mesh, materials, mesh.name);
            _cachedData.Add(key, batch);
            
            AssetDatabase.AddObjectToAsset(mesh, this);
            AssetDatabase.SaveAssets();
        }

        #region Serialization
        [SerializeField, HideInInspector] private List<Batch> _serializedData = new ();
        
        public void OnBeforeSerialize() {
            _serializedData.Clear();
            
            foreach (var pair in _cachedData) {
                _serializedData.Add(pair.Value);
            }
        }

        public void OnAfterDeserialize() {
            _cachedData.Clear();
            
            foreach (var batch in _serializedData) {
                _cachedData.Add(batch.Key, batch);
            }
        }
        #endregion
    }
    
    #if UNITY_EDITOR
    [CustomEditor(typeof(MeshCache))]
    public class MeshCacheEditor : Editor {
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            var cache = target as MeshCache;
            if (cache == null) return;
            
            if (GUILayout.Button("Clear Cache")) {
                Undo.RecordObject(cache, "Clear cache");
                cache.Clear();
                EditorUtility.SetDirty(cache);
            }
        }
    }
    #endif
}