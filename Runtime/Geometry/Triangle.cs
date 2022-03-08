using UnityEngine;

namespace StrategyGame.Geometry {
    public readonly struct Triangle {
        public readonly Vertex V0, V1, V2;
        
        public Triangle(Vertex v0, Vertex v1, Vertex v2) {
            V0 = v0;
            V1 = v1;
            V2 = v2;
        }

        /// <summary>
        /// Returns a vector that is orthogonal to the plane of this triangle
        /// and that faces the same direction as the triangle is visible from.
        /// It is not based on the normal vectors of the vertices.
        /// </summary>
        public Vector3 Normal => Vector3.Cross(
            V1.Position - V0.Position,
            V2.Position - V0.Position
        );

        /// <summary>
        /// If this triangle faces away from the specified normal vector, return
        /// a copy of this with the winding order reversed.
        /// </summary>
        /// <param name="normal">the normal</param>
        /// <returns>a copy of this triangle</returns>
        public Triangle EnsureWindingOrderMatches(in Vector3 normal) {
            return Vector3.Dot(Normal, normal) >= 0.0f 
                ? this : new Triangle(V0, V2, V1);
        }
    }
}