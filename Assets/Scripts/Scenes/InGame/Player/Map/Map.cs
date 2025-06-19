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
    /// �}�b�v�Ɋւ��鏈�����s���X�N���v�g
    /// </summary>
    public class Map : MonoBehaviour
    {
        const float WALLSIZE = 5.85f;
        const float FLOOR_1F = 13f;//��K�̗l�q���B�e����̂ɍœK��y���W
        const float FLOOR_2F = 21f;//��K�̗l�q���B�e����̂ɍœK��y���W
        const float PROJECTIONSIZE_MAX = 38.5f;//�J�����̎ˊp�̍ő�l(�}�b�v���a19�̂Ƃ���55)
        const float PROJECTIONSIZE_FOLLOW = 15;//�v���C���[�Ǐ]���̃J�����̎ˊp
        const float STAGERADIUS = 13;//�X�e�[�W�̔��a�i�^�C���̖����j
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
        [SerializeField]private MeshRenderer[] _fogMeshRenderers = new MeshRenderer[2];//AOSFog�Ő������ꂽFog��MeshRenderer
        // Start is called before the first frame update
        void Start()
        {
            IngameManager.Instance.OnPlayerSpawnEvent
                .FirstOrDefault()
                .Subscribe(_ =>
                {
                    _player = GameObject.FindWithTag("Player");
                    _player.TryGetComponent<PlayerStatus>(out _playerStatus);
                    //�I�����C���Ή��̍ۂ�InputAuthority�Ō�������
                }).AddTo(this);

            _defaultPos = _mapCamera.transform.position;

            //Map�L�[�̎w����s��
            //�������

            //_mapKey���������ۂ̏���
            //�����Ă���Ƃ�����Ȃ��ƃ_��
            this.UpdateAsObservable()
                .Where(_ => Input.GetKeyDown(_mapKey))
                .Where(_ => _playerStatus.nowPlayerSurvive)
                .ThrottleFirst(TimeSpan.FromMilliseconds(100))
                .Subscribe(_ =>
                {
                    if (_isOpenMap)//�}�b�v���J���Ă����Ԃ̎�
                    {
                        PlayerGUIPresenter.Instance.CursorSetting(true);
                        _mapPanel.SetActive(false);

                        _isOpenMap = false;
                        _mapCamera_camera.orthographicSize = PROJECTIONSIZE_FOLLOW;
                    }
                    else //�}�b�v�����Ă����Ԃ̂Ƃ�
                    {
                        PlayerGUIPresenter.Instance.CursorSetting(false);
                        _mapCamera_camera.orthographicSize = PROJECTIONSIZE_MAX;



                        //Player�̏ꏊ�ɉ����ă}�b�v��؂�ւ���
                        if (_player.transform.position.y < WALLSIZE)//1�K�ɂ���ꍇ
                        {
                            _mapCamera.transform.position = new Vector3(_defaultPos.x, FLOOR_1F, _defaultPos.z);
                            _fogMeshRenderers[0].enabled = true;
                            _fogMeshRenderers[1].enabled = false;
                            _nowViewFloor = 1;
                            
                        }
                        else if (WALLSIZE <= _player.transform.position.y)//2�K�ɂ���ꍇ
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
        /// �{�^���ɂ��}�b�v�؂�ւ��B��F1�K��\�����Ă���Ƃ���2�K�̃}�b�v��\��������
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

                if (_player.transform.position.y < WALLSIZE)//1�K�ɂ���ꍇ
                {
                    _mapCamera.transform.position = new Vector3(_player.transform.position.x, FLOOR_1F, _player.transform.position.z);
                    _fogMeshRenderers[0].enabled = true;
                    _fogMeshRenderers[1].enabled = false;
                }
                else if (WALLSIZE <= _player.transform.position.y)//2�K�ɂ���ꍇ
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
