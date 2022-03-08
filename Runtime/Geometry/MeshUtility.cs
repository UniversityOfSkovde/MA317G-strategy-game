using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace StrategyGame.Geometry {
    public static class MeshUtility {

        public static IEnumerable<(Transform transform, Mesh mesh, Material[] materials)> FindMeshes(GameObject asset) {
            foreach (var renderer in asset.GetComponentsInChildren<MeshRenderer>()) {
                if (!renderer.TryGetComponent<MeshFilter>(out var filter)) {
                    continue;
                }
                
                var mesh = filter.sharedMesh;
                if (mesh == null) {
                    Debug.LogError("Shared mesh is null");
                    continue;
                }
                
                if (!mesh.isReadable) {
                    Debug.LogError($"Could not read geometry data from mesh '{mesh.name}'");
                    continue;
                }
                
                var t = renderer.transform;
                var materials = renderer.sharedMaterials;
                
                var subMeshes = mesh.subMeshCount;
                if (subMeshes != materials.Length) {
                    Debug.LogError($"Mesh '{mesh.name}' has {subMeshes} " +
                                   "subMeshes, yet the number of materials " +
                                   "in the associated renderer is " +
                                   $"{materials.Length}.");
                    continue;
                }

                yield return (t, mesh, materials);
            }
        }

        public static void CopyMesh(Mesh src, Mesh dest) {
            dest.Clear();
            dest.vertices  = src.vertices;
            dest.normals   = src.normals;
            dest.uv        = src.uv;
            dest.triangles = src.triangles;
            dest.subMeshCount = src.subMeshCount;

            var subMeshes = new List<SubMeshDescriptor>();
            for (int i = 0; i < src.subMeshCount; i++) {
                subMeshes.Add(src.GetSubMesh(i));
            }
            
            dest.SetSubMeshes(subMeshes);
            dest.bounds = src.bounds;
        }
    }
}