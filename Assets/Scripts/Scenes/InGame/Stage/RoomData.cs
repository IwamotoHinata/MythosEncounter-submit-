using UnityEngine;

namespace Scenes.Ingame.Stage
{
    public struct RoomData
    {
        private RoomType _roomType;
        private int _roomId;
        private string _roomName;
        private GameObject _gameObject;

        public RoomType RoomType { get => _roomType; }
        public int RoomId { get => _roomId; }
        public string roomName { get => _roomName; }
        public GameObject gameObject { get => _gameObject; }

        public void RoomDataSet(RoomType roomType,int roomId)
        {
            _roomType = roomType;
            _roomId = roomId;
        }
        public void SetGameObject(GameObject gameObject)
        {
            _gameObject = gameObject;
        }
        public void SetRoomName(string name)
        {
            _roomName = name;
        }
    }
}