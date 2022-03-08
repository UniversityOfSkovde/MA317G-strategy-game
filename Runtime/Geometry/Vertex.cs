using System;
using UnityEngine;

namespace StrategyGame.Geometry {
    public readonly struct Vertex : IEquatable<Vertex> {

        public readonly Vector3 Position;
        public readonly Vector3 Normal;
        public readonly Vector2 TexCoord;

        public Vertex(Vector3 position, Vector3 normal, Vector2 texCoord) {
            Position = position;
            Normal   = normal;
            TexCoord = texCoord;
        }

        public static Vertex Lerp(Vertex from, Vertex to, float t) {
            return new Vertex(
                Vector3.Lerp(from.Position, to.Position, t),
                Vector3.Slerp(from.Normal, to.Normal, t),
                Vector2.Lerp(from.TexCoord, to.TexCoord, t)
            );
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = Position.GetHashCode();
                hashCode = (hashCode * 397) ^ Normal.GetHashCode();
                hashCode = (hashCode * 397) ^ TexCoord.GetHashCode();
                return hashCode;
            }
        }

        public bool Equals(Vertex other) {
            return Position.Equals(other.Position) 
                && Normal.Equals(other.Normal) 
                && TexCoord.Equals(other.TexCoord);
        }

        public override bool Equals(object obj) {
            return obj is Vertex other && Equals(other);
        }
    }
}