using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Scenes.Ingame.Stage
{
    public class RoomLinqConfig : MonoBehaviour
    {
        public List<Vector2> GetLinqPath(string roomName)
        {
            List<Vector2> linqPosition = new List<Vector2>();
            switch (roomName)
            {
                case "SpawnRoom_HH(Clone)":
                    linqPosition.Add(SetPositionInts(0, 2));
                    break;
                case "EscapeRoom_HH(Clone)":
                    linqPosition.Add(SetPositionInts(0, 2));
                    linqPosition.Add(SetPositionInts(2, 1));
                    break;
                case "2x2Room1_HH(Clone)":
                    linqPosition.Add(SetPositionInts(0, 2));
                    break;
                case "2x2Room2_HH(Clone)":
                    linqPosition.Add(SetPositionInts(0, 2));
                    break;
                case "2x2Room_Stair1f_TEMP(Clone)":
                    linqPosition.Add(SetPositionInts(0, 2));
                    break;
                case "2x2Room_Stair2f_TEMP(Clone)":
                    linqPosition.Add(SetPositionInts(2, 0));
                    break;
                case "2x3Room1_HH(Clone)":
                    linqPosition.Add(SetPositionInts(1, 3));
                    break;
                case "2x3Room2_HH(Clone)":
                    linqPosition.Add(SetPositionInts(1, 3));
                    break;
                case "3x2Room1_HH(Clone)":
                    linqPosition.Add(SetPositionInts(0, 2));
                    break;
                case "3x2Room2_HH(Clone)":
                    linqPosition.Add(SetPositionInts(0, 2));
                    break;
                case "3x3Room_HH(Clone)":
                    linqPosition.Add(SetPositionInts(3, 0));
                    break;
                case "3x3StairBottom_HH(Clone)":
                    linqPosition.Add(SetPositionInts(3, 0));
                    break;
                case "3x3StairTop_HH(Clone)":
                    linqPosition.Add(SetPositionInts(3, 0));
                    break;
                case "3x4Room1_HH(Clone)":
                    linqPosition.Add(SetPositionInts(3, 1));
                    break;
                case "3x4Room2_HH(Clone)":
                    linqPosition.Add(SetPositionInts(3, 1));
                    break;
                case "4x3Room1_HH(Clone)":
                    linqPosition.Add(SetPositionInts(4, 0));
                    break;
                case "4x3Room2_HH(Clone)":
                    linqPosition.Add(SetPositionInts(4, 0));
                    break;
                case "4x4Room1_HH(Clone)":
                    linqPosition.Add(SetPositionInts(4, 2));
                    linqPosition.Add(SetPositionInts(1, 4));
                    break;
                case "4x4Room2_HH(Clone)":
                    linqPosition.Add(SetPositionInts(4, 2));
                    linqPosition.Add(SetPositionInts(1, 4));
                    break;
                default:
                    break;
            }
            return linqPosition;

        }
        private Vector2 SetPositionInts(int x,int y)
        {
            Vector2 position = Vector2.zero;
            position.x = x;
            position.y = y;
            return position;
        }

    }
}