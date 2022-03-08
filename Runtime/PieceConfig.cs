using UnityEngine;

namespace StrategyGame {
    [CreateAssetMenu(menuName = "Strategy Game/New Piece Config...")]
    public class PieceConfig : ScriptableObject {
        [Header("Prefabs")]
        public GameObject _emptyPrefab;
        public GameObject _obstaclePrefab;
        public GameObject _trapPrefab;
        public GameObject _terrainPrefab;
        public GameObject _portalPrefab;
        public GameObject _checkPointPrefab;
        public GameObject _agentPrefab;
    }
}