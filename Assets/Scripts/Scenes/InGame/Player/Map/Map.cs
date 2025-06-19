using UnityEngine;
using DG.Tweening;
using UniRx;
using UniRx.Triggers;
using System;
using Scenes.Ingame.Manager;
using TMPro;

namespace Scenes.Ingame.Player
{

    /// <summary>
    /// マップに関する処理を行うスクリプト
    /// </summary>
    public class Map : MonoBehaviour
    {
        const float WALLSIZE = 5.85f;
        const float FLOOR_1F = 13f;//一階の様子を撮影するのに最適なy座標
        const float FLOOR_2F = 21f;//二階の様子を撮影するのに最適なy座標
        const float PROJECTIONSIZE_MAX = 38.5f;//カメラの射角の最大値(マップ半径19のときは55)
        const float PROJECTIONSIZE_FOLLOW = 15;//プレイヤー追従時のカメラの射角
        const float STAGERADIUS = 13;//ステージの半径（タイルの枚数）
        private Vector3 _defaultPos;

        [SerializeField] private GameObject _mapPanel;
        [SerializeField] private TMP_Text _mapText;
        [SerializeField] private GameObject _mapCamera;
        [SerializeField] private Camera _mapCamera_camera;

        private bool _isOpenMap = false;
        private int _nowViewFloor;

        private KeyCode _mapKey = KeyCode.M;
        private GameObject _player;
        private PlayerStatus _playerStatus;
        [SerializeField]private MeshRenderer[] _fogMeshRenderers = new MeshRenderer[2];//AOSFogで生成されたFogのMeshRenderer
        // Start is called before the first frame update
        void Start()
        {
            IngameManager.Instance.OnPlayerSpawnEvent
                .FirstOrDefault()
                .Subscribe(_ =>
                {
                    _player = GameObject.FindWithTag("Player");
                    _player.TryGetComponent<PlayerStatus>(out _playerStatus);
                    //オンライン対応の際はInputAuthorityで見分ける
                }).AddTo(this);

            _defaultPos = _mapCamera.transform.position;

            //Mapキーの指定を行う
            //今後実装

            //_mapKeyを押した際の処理
            //生きているときじゃないとダメ
            this.UpdateAsObservable()
                .Where(_ => Input.GetKeyDown(_mapKey))
                .Where(_ => _playerStatus.nowPlayerSurvive)
                .ThrottleFirst(TimeSpan.FromMilliseconds(100))
                .Subscribe(_ =>
                {
                    if (_isOpenMap)//マップが開いている状態の時
                    {
                        PlayerGUIPresenter.Instance.CursorSetting(true);
                        _mapPanel.SetActive(false);

                        _isOpenMap = false;
                        _mapCamera_camera.orthographicSize = PROJECTIONSIZE_FOLLOW;
                    }
                    else //マップが閉じている状態のとき
                    {
                        PlayerGUIPresenter.Instance.CursorSetting(false);
                        _mapCamera_camera.orthographicSize = PROJECTIONSIZE_MAX;



                        //Playerの場所に応じてマップを切り替える
                        if (_player.transform.position.y < WALLSIZE)//1階にいる場合
                        {
                            _mapCamera.transform.position = new Vector3(_defaultPos.x, FLOOR_1F, _defaultPos.z);
                            _fogMeshRenderers[0].enabled = true;
                            _fogMeshRenderers[1].enabled = false;
                            _nowViewFloor = 1;
                            
                        }
                        else if (WALLSIZE <= _player.transform.position.y)//2階にいる場合
                        {
                            _mapCamera.transform.position = new Vector3(_defaultPos.x, FLOOR_2F, _defaultPos.z);
                            _fogMeshRenderers[0].enabled = false;
                            _fogMeshRenderers[1].enabled = true;
                            _nowViewFloor = 2;
                        }

                        _mapPanel.SetActive(true);
                        _mapText.text = "Floor" + _nowViewFloor.ToString();

                        _isOpenMap = true;
                    }
                });
        }

        /// <summary>
        /// ボタンによるマップ切り替え。例：1階を表示しているときは2階のマップを表示させる
        /// </summary>
        public void ChangeMapFloorOnButton()
        {
            if (_nowViewFloor == 1)
            {
                _mapCamera.transform.position = new Vector3(_defaultPos.x, FLOOR_2F, _defaultPos.z);
                _fogMeshRenderers[0].enabled = false;
                _fogMeshRenderers[1].enabled = true;
                _nowViewFloor = 2;
                _mapText.text = "Floor2";
            }
            else if (_nowViewFloor == 2)
            {
                _mapCamera.transform.position = new Vector3(_defaultPos.x, FLOOR_1F, _defaultPos.z);
                _fogMeshRenderers[0].enabled = true;
                _fogMeshRenderers[1].enabled = false;
                _nowViewFloor = 1;
                _mapText.text = "Floor1";
            }
        }

        public void Update()
        {
            if (!_isOpenMap && _player != null)
            {                        

                if (_player.transform.position.y < WALLSIZE)//1階にいる場合
                {
                    _mapCamera.transform.position = new Vector3(_player.transform.position.x, FLOOR_1F, _player.transform.position.z);
                    _fogMeshRenderers[0].enabled = true;
                    _fogMeshRenderers[1].enabled = false;
                }
                else if (WALLSIZE <= _player.transform.position.y)//2階にいる場合
                {
                    _mapCamera.transform.position = new Vector3(_player.transform.position.x, FLOOR_2F, _player.transform.position.z);
                    _fogMeshRenderers[0].enabled = false;
                    _fogMeshRenderers[1].enabled = true;
                }
            }
        }

        public void SetFogs(int floor, MeshRenderer obj)
        {
            _fogMeshRenderers[floor - 1] = obj;
        }
    }
}
