using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace StrategyGame.Geometry {
    public sealed class MeshBuilder {
        
        private readonly Dictionary<int, UniqueList<Vertex>> vertices;
        private readonly Dictionary<int, List<(int i0, int i1, int i2)>> triangles;
        private readonly Dictionary<int, Material> materials;

        private List<int> SubMeshes => materials.Keys.OrderBy(i => i).ToList();
        private List<(int SubMesh, Material Material)> Materials => materials.OrderBy(p => p.Key).Select(p => (p.Key, p.Value)).ToList();

        private int nextSubMesh = 1;
        private int GetAndIncrementNextSubMesh() => nextSubMesh++;
        
        public MeshBuilder() {
            vertices = new Dictionary<int, UniqueList<Vertex>>();
            triangles = new Dictionary<int, List<(int, int, int)>>();
            materials = new Dictionary<int, Material>();
        }

        public void Add(MeshBuilder other) {
            Add(other, Matrix4x4.identity, Matrix4x4.identity);
        }
        
        public void Add(MeshBuilder other, Matrix4x4 vertexMatrix, Matrix4x4 textureMatrix) {
            var existingMaterials = Materials;
            var materialIdxToSubMesh = new Dictionary<int, int>();
            for (var i = 0; i < existingMaterials.Count; i++) {
                var mat = existingMaterials[i];
                materialIdxToSubMesh.Add(mat.Material.GetInstanceID(), mat.SubMesh);
            }
            
            var subMeshList = other.Materials;
            for (var s = 0; s < subMeshList.Count; s++) {
                var otherSubMesh = subMeshList[s];
                if (!materialIdxToSubMesh.TryGetValue(
                        otherSubMesh.Material.GetInstanceID(),
                        out var existingSubMesh)) {
                    var otherMaterial = other.materials[otherSubMesh.SubMesh];
                    existingSubMesh = GetAndIncrementNextSubMesh();
                    materials.Add(existingSubMesh, otherMaterial);
                }

                if (!other.vertices.TryGetValue(otherSubMesh.SubMesh, out var otherVerts) || otherVerts.Count == 0) continue;
                if (!other.triangles.TryGetValue(otherSubMesh.SubMesh, out var otherTriangles) || otherTriangles.Count == 0) continue;
                
                if (!vertices.TryGetValue(existingSubMesh, out var verts)) {
                    verts = new UniqueList<Vertex>();
                    vertices.Add(existingSubMesh, verts);
                }
                
                if (!triangles.TryGetValue(existingSubMesh, out var tris)) {
                    tris = new List<(int i0, int i1, int i2)>();
                    triangles.Add(existingSubMesh, tris);
                }

                for (var i = 0; i < otherTriangles.Count; i++) {
                    var (i0, i1, i2) = otherTriangles[i];
                    var (v0, v1, v2) = (otherVerts[i0], otherVerts[i1], otherVerts[i2]);

                    var p0 = vertexMatrix.MultiplyPoint(v0.Position);
                    var p1 = vertexMatrix.MultiplyPoint(v1.Position);
                    var p2 = vertexMatrix.MultiplyPoint(v2.Position);
                    
                    var n0 = vertexMatrix.MultiplyVector(v0.Normal);
                    var n1 = vertexMatrix.MultiplyVector(v1.Normal);
                    var n2 = vertexMatrix.MultiplyVector(v2.Normal);
                    
                    var t0 = textureMatrix.MultiplyPoint(v0.TexCoord);
                    var t1 = textureMatrix.MultiplyPoint(v1.TexCoord);
                    var t2 = textureMatrix.MultiplyPoint(v2.TexCoord);
                    
                    var j0 = verts.AddOrFind(new Vertex(p0, n0, t0));
                    var j1 = verts.AddOrFind(new Vertex(p1, n1, t1));
                    var j2 = verts.AddOrFind(new Vertex(p2, n2, t2));
                    
                    tris.Add((j0, j1, j2));
                }
            }
        }

        public void Build(Mesh mesh, List<Material> sharedMaterials) {
            sharedMaterials.Clear();

            var subMeshIndices = SubMeshes;
            var vertexList    = new List<Vector3>();
            var normalList    = new List<Vector3>();
            var texCoordList  = new List<Vector2>();
            var trianglesList = new List<int>();
            var subMeshArray = new SubMeshDescriptor[subMeshIndices.Count];

            for (var s = 0; s < subMeshIndices.Count; s++) {
                var subMesh = subMeshIndices[s];
                var verts = vertices[subMesh];
                var tris  = triangles[subMesh];
                var mat   = materials[subMesh];
                
                var vertsOffset = vertexList.Count;
                for (var i = 0; i < verts.Count; i++) {
                    vertexList.Add(verts[i].Position);
                    normalList.Add(verts[i].Normal);
                    texCoordList.Add(verts[i].TexCoord);
                }

                var indexStart = trianglesList.Count;
                for (var i = 0; i < tris.Count; i++) {
                    trianglesList.Add(tris[i].i0 + vertsOffset);
                    trianglesList.Add(tris[i].i1 + vertsOffset);
                    trianglesList.Add(tris[i].i2 + vertsOffset);
                }

                subMeshArray[s] = new SubMeshDescriptor(
                    indexStart, tris.Count * 3
                );
                
                sharedMaterials.Add(mat);
            }
            
            mesh.Clear();
            mesh.SetVertices(vertexList);
            mesh.SetNormals(normalList);
            mesh.SetUVs(0, texCoordList);
            mesh.triangles = trianglesList.ToArray();
            mesh.subMeshCount = subMeshArray.Length;
            for (var i = 0; i < subMeshArray.Length; i++) {
                var subMesh = subMeshArray[i];
                mesh.SetSubMesh(i, subMesh);
            }
            
            mesh.MarkModified();
        }

        public MeshBuilder Rotate(Quaternion q) {
            var mb = new MeshBuilder();
            var subMeshList = SubMeshes;
            //var c = Vector3.one * .5f;
            for (var s = 0; s < subMeshList.Count; s++) {
                var subMesh = subMeshList[s];
                
                if (!vertices.TryGetValue(subMesh, out var verts)) continue;
                if (verts.Count == 0) continue;
                
                if (!triangles.TryGetValue(subMesh, out var tris)) continue;
                if (tris.Count == 0) continue;

                var newVerts = new UniqueList<Vertex>();
                var newTris = new List<(int, int, int)>();

                for (var i = 0; i < tris.Count; i++) {
                    var (i0, i1, i2) = tris[i];
                    var (v0, v1, v2) = (verts[i0], verts[i1], verts[i2]);
                    var p0 = q * v0.Position;
                    //var p0 = q * (v0.Position - c) + c;
                    var n0 = q * v0.Normal;
                    var p1 = q * v1.Position;
                    //var p1 = q * (v1.Position - c) + c;
                    var n1 = q * v1.Normal;
                    var p2 = q * v2.Position;
                    //var p2 = q * (v2.Position - c) + c;
                    var n2 = q * v2.Normal;
                    var j0 = newVerts.AddOrFind(new Vertex(p0, n0, v0.TexCoord));
                    var j1 = newVerts.AddOrFind(new Vertex(p1, n1, v1.TexCoord));
                    var j2 = newVerts.AddOrFind(new Vertex(p2, n2, v2.TexCoord));
                    newTris.Add((j0, j1, j2));
                }

                mb.vertices.Add(subMesh, newVerts);
                mb.triangles.Add(subMesh, newTris);
                mb.materials.Add(subMesh, materials[subMesh]);
            }

            return mb;
        }
        
        public static MeshBuilder LoadFrom(GameObject prefab) {
            var mb = new MeshBuilder();
            var instanceIdToSubMeshIndex = new Dictionary<int, int>();
            
            foreach (var drawCall in MeshUtility.FindMeshes(prefab)) {
                var t         = drawCall.transform;
                var mesh      = drawCall.mesh;
                var materials = drawCall.materials;
                var subMeshes = drawCall.mesh.subMeshCount;
                
                if (subMeshes > materials.Length) {
                    Debug.LogError($"Mesh '{drawCall.mesh.name}' has {subMeshes} " +
                                   "subMeshes, yet the number of materials " +
                                   "in the associated renderer is " +
                                   $"{materials.Length}.");
                }
                subMeshes = Math.Min(subMeshes, materials.Length);
                
                // Same order as sub-meshes, but refers to materials in 'uniqueMaterials' list
                var subMeshIndices = new List<int>();
                
                for (var i = 0; i < subMeshes; i++) {
                    var mat = materials[i];
                    var id = mat.GetInstanceID();
                    if (instanceIdToSubMeshIndex.TryGetValue(id, out var idx)) {
                        subMeshIndices.Add(idx);
                    } else {
                        var nextSubMeshIdx = mb.GetAndIncrementNextSubMesh();
                        instanceIdToSubMeshIndex.Add(id, nextSubMeshIdx);
                        subMeshIndices.Add(nextSubMeshIdx);
                        mb.materials.Add(nextSubMeshIdx, mat);
                    }
                }
                
                var meshVertices  = mesh.vertices;
                var meshNormals   = mesh.normals;
                var meshUVs       = mesh.uv;
                var meshTriangles = mesh.triangles;

                for (var s = 0; s < subMeshIndices.Count; s++) {
                    var subMesh = subMeshIndices[s];
                    
                    if (!mb.vertices.TryGetValue(subMesh, out var verts)) {
                        verts = new UniqueList<Vertex>();
                        mb.vertices.Add(subMesh, verts);
                    }
                    
                    if (!mb.triangles.TryGetValue(subMesh, out var tris)) {
                        tris = new List<(int, int, int)>();
                        mb.triangles.Add(subMesh, tris);
                    }
                    
                    var subMeshDescr = mesh.GetSubMesh(s);
                    var first = subMeshDescr.indexStart;
                    var last = first + subMeshDescr.indexCount - 3;
                    
                    for (var i = first; i <= last; i += 3) {
                        var a0 = meshTriangles[i];
                        var b0 = meshTriangles[i + 1];
                        var c0 = meshTriangles[i + 2];
                        
                        var a1 = verts.AddOrFind(new Vertex(
                            t.TransformPoint(meshVertices[a0]),
                            t.TransformVector(meshNormals[a0]),
                            meshUVs[a0]
                        ));
                        
                        var b1 = verts.AddOrFind(new Vertex(
                            t.TransformPoint(meshVertices[b0]),
                            t.TransformVector(meshNormals[b0]),
                            meshUVs[b0]
                        ));
                        
                        var c1 = verts.AddOrFind(new Vertex(
                            t.TransformPoint(meshVertices[c0]),
                            t.TransformVector(meshNormals[c0]),
                            meshUVs[c0]
                        ));
                        
                        tris.Add((a1, b1, c1));
                    }
                }
            }

            return mb;
        }
    }
}