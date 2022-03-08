using System.Collections.Generic;

namespace StrategyGame.Geometry {
    public sealed class UniqueList<T> {
        private readonly Dictionary<T, int> indices;
        private readonly List<T> values;

        public UniqueList() {
            indices = new Dictionary<T, int>();
            values = new List<T>();
        }

        public UniqueList(UniqueList<T> prototype) {
            indices = new Dictionary<T, int>(prototype.indices);
            values = new List<T>(prototype.values);
        }

        public int Count => values.Count;

        public T this[int idx] => values[idx];

        public int AddOrFind(T element) {
            if (!indices.TryGetValue(element, out var index)) {
                index = values.Count;
                values.Add(element);
            }

            return index;
        }
    }
}