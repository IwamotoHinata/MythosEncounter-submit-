using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using System.Linq;

namespace Scenes.Ingame.Stage.TEST
{
    public class candidateAislePositionTEST : MonoBehaviour
    {
        [SerializeField, Tooltip("intでステージの縦横のサイズ")]
        private Vector2 _stageSize;
        private List<Vector2> candidatePosition = new List<Vector2>();
        private RoomData[,] _stageGenerateData;
        RoomData _0Data;
        RoomData _1Data;
        // Start is called before the first frame update
        void Start()
        {
            _0Data.RoomDataSet(RoomType.none, 0);
            _1Data.RoomDataSet(RoomType.room2x2, 1);
            TEST_3x1_success();
        }
        /// <summary>
        /// 3x1の通路を読み取る成功テスト
        /// </summary>
        private void TEST_3x1_success()
        {
            _stageGenerateData = new RoomData[9, 9]
            {
                { _1Data, _0Data, _1Data, _1Data, _1Data, _0Data, _1Data, _1Data, _1Data },
                { _1Data, _0Data, _1Data, _1Data, _1Data, _0Data, _1Data, _1Data, _0Data },
                { _1Data, _0Data, _1Data, _1Data, _1Data, _0Data, _1Data, _0Data, _0Data },
                { _1Data, _1Data, _1Data, _1Data, _1Data, _0Data, _1Data, _1Data, _0Data },
                { _1Data, _1Data, _1Data, _1Data, _1Data, _1Data, _1Data, _1Data, _0Data },
                { _1Data, _1Data, _1Data, _1Data, _1Data, _1Data, _1Data, _1Data, _1Data },
                { _0Data, _1Data, _0Data, _0Data, _1Data, _1Data, _0Data, _1Data, _1Data },
                { _0Data, _1Data, _0Data, _1Data, _1Data, _1Data, _0Data, _1Data, _1Data },
                { _0Data, _1Data, _0Data, _0Data, _1Data, _1Data, _0Data, _1Data, _1Data }
            };
            var TESTdata = candidateAislePosition(offsetX: 2);
            var TESTdata2 = candidateAislePosition(offsetX: 3);
            TESTdata = TESTdata.Except(TESTdata2).ToList();
            Debug.Log($"3x1 aisle count expect == 3 , get count {TESTdata.Count}");
            foreach (var item in TESTdata)
            {
                _stageGenerateData[(int)item.x, (int)item.y].RoomDataSet(RoomType.room2x2, 8);
                Debug.Log($"positon {item.x},{item.y}");
            }
            DebugStageData();
            Assert.IsTrue(TESTdata.Count == 3);
        }

        private List<Vector2> candidateAislePosition(int offsetX = 0, int offsetY = 0)
        {
            List<Vector2> candidatePositions = new List<Vector2>();
            Vector2 setPosition = Vector2.zero;
            for (int y = 0; y < _stageSize.y - offsetY; y++)
            {
                for (int x = 0; x < _stageSize.x - offsetX; x++)
                {
                    if (_stageGenerateData[x, y].RoomId == 0 &&
                        _stageGenerateData[x, y + offsetY].RoomId == 0 &&
                        _stageGenerateData[x + offsetX, y].RoomId == 0)
                    {
                        if (x >= 1 && offsetX != 0)
                        {
                            if (_stageGenerateData[x - 1, y].RoomId == 0) continue;
                            try
                            {
                                if (_stageGenerateData[x, y - 1].RoomId == 0) continue;
                                if (_stageGenerateData[x, y + 1].RoomId == 0) continue;
                            }
                            catch (System.Exception)
                            {

                            }
                        }
                        if (y >= 1 && offsetY != 0)
                        {
                            if (_stageGenerateData[x, y - 1].RoomId == 0) continue;
                            try
                            {
                                if (_stageGenerateData[x + 1, y].RoomId == 0) continue;
                                if (_stageGenerateData[x - 1, y].RoomId == 0) continue;
                            }
                            catch (System.Exception)
                            {
                            }
                        }
                        setPosition.x = x;
                        setPosition.y = y;
                        candidatePositions.Add(setPosition);
                    }
                }
            }
            return candidatePositions;
        }
        private void DebugStageData()
        {
            string printData = "\n";
            for (int y = 0; y < _stageGenerateData.GetLength(1); y++)
            {
                for (int x = 0; x < _stageGenerateData.GetLength(0); x++)
                {
                    printData += $"[{_stageGenerateData[x, y].RoomId}]";
                }
                printData += "\n";
            }
            Debug.Log(printData);
        }
    }
}
