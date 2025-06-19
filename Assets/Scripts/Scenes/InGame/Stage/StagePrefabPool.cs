using UnityEngine;

namespace Scenes.Ingame.Stage
{
    public class StagePrefabPool : MonoBehaviour
    {
        [Header("BasePrefabs")]
        [SerializeField]
        private GameObject tilePrefab;
        [SerializeField]
        private GameObject outSideWallXPrefab;
        [SerializeField]
        private GameObject outSideWallYPrefab;
        [SerializeField]
        private GameObject inSideWallXPrefab;
        [SerializeField]
        private GameObject inSideWallYPrefab;
        [SerializeField]
        private GameObject wallXDoorPrefab;
        [SerializeField]
        private GameObject wallYDoorPrefab;
        [Header("RoomPrefabs")]
        [SerializeField]
        private GameObject playerSpawnRoomPrefab;
        [SerializeField]
        private GameObject escapeSpawnRoomPrefab;
        [SerializeField]
        private GameObject[] _2x2RoomPrefab;
        [SerializeField]
        private GameObject[] _3x2RoomPrefab;
        [SerializeField]
        private GameObject[] _2x3RoomPrefab;
        [SerializeField]
        private GameObject _3x3RoomPrefab;
        [SerializeField]
        private GameObject[] _4x3RoomPrefab;
        [SerializeField]
        private GameObject[] _3x4RoomPrefab;
        [SerializeField]
        private GameObject[] _4x4RoomPrefab;
        [SerializeField]
        private GameObject _2x2RoomStair1fPrefab;
        [SerializeField]
        private GameObject _2x2RoomStair2fPrefab;
        [SerializeField]
        private GameObject _3x3RoomStair1fPrefab;
        [SerializeField]
        private GameObject _3x3RoomStair2fPrefab;
        [Header("CorridorPrefabs")]
        [SerializeField]
        private GameObject _2x4CorridorPrefab;
        [SerializeField]
        private GameObject _4x2CorridorPrefab;
        [SerializeField]
        private GameObject _2x2CorridorPrefab;

        public GameObject getTilePrefab { get => tilePrefab; }
        public GameObject getOutSideWallXPrefab { get => outSideWallXPrefab; }
        public GameObject getOutSideWallYPrefab { get => outSideWallYPrefab; }
        public GameObject getInSideWallXPrefab { get => inSideWallXPrefab; }
        public GameObject getInSideWallYPrefab { get => inSideWallYPrefab; }
        public GameObject getWallXDoorPrefab { get => wallXDoorPrefab; }
        public GameObject getWallYDoorPrefab { get => wallYDoorPrefab; }
        public GameObject getPlayerSpawnRoomPrefab { get => playerSpawnRoomPrefab; }
        public GameObject getEscapeSpawnRoomPrefab { get => escapeSpawnRoomPrefab; }
        public GameObject get3x3RoomPrefab { get => _3x3RoomPrefab; }
        public GameObject[] get4x3RoomPrefab { get => _4x3RoomPrefab; }
        public GameObject[] get3x4RoomPrefab { get => _3x4RoomPrefab; }
        public GameObject[] get4x4RoomPrefab { get => _4x4RoomPrefab; }
        public GameObject get2x2RoomStair1fPrefab { get => _2x2RoomStair1fPrefab; }
        public GameObject get2x2RoomStair2fPrefab { get => _2x2RoomStair2fPrefab; }
        public GameObject get3x3RoomStair1fPrefab { get => _3x3RoomStair1fPrefab; }
        public GameObject get3x3RoomStair2fPrefab { get => _3x3RoomStair2fPrefab; }
        public GameObject[] get2x2RoomPrefab { get => _2x2RoomPrefab; }
        public GameObject[] get3x2RoomPrefab { get => _3x2RoomPrefab; }
        public GameObject[] get2x3RoomPrefab { get => _2x3RoomPrefab; }
        public GameObject get2x4CorridorPrefab { get => _2x4CorridorPrefab; }
        public GameObject get4x2CorridorPrefab { get => _4x2CorridorPrefab; }
        public GameObject get2x2CorridorPrefab { get => _2x2CorridorPrefab; }
    }
}