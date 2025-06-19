using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Fusion;
using Common.Network;
using Common.UI;
using TMPro;

namespace Scenes.RoomSetting
{
    public class RoomSettingManager : MonoBehaviour
    {
        private static readonly int RANDOMKEY_MAX = 1000000; //ID�̍ő�l

        [Header("Prefabs")]
        [SerializeField] private NetworkRunner _runnerPrefab;
        [SerializeField] private Dialog _dialog;
        [Header("Scene Objects")]
        [SerializeField] private GameObject _canvas;
        [SerializeField] private List<GameObject> _roomSettingPanels = new List<GameObject>();
        [Header("Parameter")]
        [SerializeField] private int _maxPlayer = 0; //�ő�l��
        [SerializeField] private int _sessionStartSceneIndex = 0; //�Z�b�V�������J�n����V�[���̃C���f�b�N�X

        private int _panelIndex = 0;

        private async void Start()
        {
            await BootRunner(); //Runner�̋N��
            SwitchPanel(1);
        }

        /// <summary>
        /// NetworkRunner���N������
        /// </summary>
        public async UniTask BootRunner()
        {
            var runner = Instantiate(_runnerPrefab); //Runner�C���X�^���X��ݒu
            runner.ProvideInput = true; //���A�������g��

            var result = await runner.JoinSessionLobby(SessionLobby.ClientServer); //�Z�b�V�������r�[�ɎQ���i���z�I�j
            if (result.Ok)
            {
                //Debug.Log("Join SessionLobby");
            }
            else
            {
                //Debug.LogError($"Error : {result.ShutdownReason}");
            }
        }

        /// <summary>
        /// NetworkRunner���~����
        /// </summary>
        public void DiscardRunner()
        {
            if (RunnerManager.Runner == null)
            {
                //Debug.LogError("Error : Not Found Runner");
            }
            else
            {
                //Debug.Log("Runner Shutdown");
                RunnerManager.Runner.Shutdown();
            }
        }

        /// <summary>
        /// �p�u���b�N�Z�b�V�����̍쐬
        /// </summary>
        public async void CreatePublicSession()
        {
            var args = new StartGameArgs()
            {
                GameMode = GameMode.Host, //�Z�b�V�������̖���
                Scene = SceneRef.FromIndex(_sessionStartSceneIndex), //�Z�b�V�����J�n���̃V�[���C���f�b�N�X
                SceneManager = RunnerManager.Runner.GetComponent<NetworkSceneManagerDefault>(), //Runner�Ŏg���V�[���Ǘ��R���|�[�l���g
                PlayerCount = _maxPlayer, //�Z�b�V�����̍ő�l��
                ConnectionToken = Guid.NewGuid().ToByteArray(), //�v���C���[�̐ڑ��g�[�N���i�z�X�g�J�ڂŎg���j
            };

            var result = await RunnerManager.Runner.StartGame(args); //�Z�b�V�����J�n
            if (result.Ok == true)
            {
                //Debug.Log("Create Public Session");
            }
            else
            {
                //Debug.LogError($"Error : {result.ShutdownReason}");
            }
        }

        /// <summary>
        /// �v���C�x�[�g�Z�b�V�����̍쐬
        /// </summary>
        public async void CreatePrivateSession()
        {
            string sessionId = GetSessionId();

            var args = new StartGameArgs()
            {
                GameMode = GameMode.Host,
                Scene = SceneRef.FromIndex(_sessionStartSceneIndex),
                SceneManager = RunnerManager.Runner.GetComponent<NetworkSceneManagerDefault>(),
                SessionName = sessionId, //�Z�b�V����ID
                PlayerCount = _maxPlayer,
                ConnectionToken = Guid.NewGuid().ToByteArray(),
            };

            var result = await RunnerManager.Runner.StartGame(args); //�Z�b�V�����J�n
            if (result.Ok == true)
            {
                //Debug.Log("Create Private Session");
                Debug.Log("Session Id : " + sessionId);
            }
            else
            {
                //Debug.LogError($"Error : {result.ShutdownReason}");
            }
        }

        /// <summary>
        /// �p�u���b�N�Z�b�V�����ɎQ��
        /// </summary>
        public async void JoinPublicSession()
        {
            //�󂫂̂���Z�b�V������������
            bool sessionExist = false;
            foreach (var session in RunnerManager.Instance.SessionInfoList)
            {
                if (session.PlayerCount != _maxPlayer) sessionExist = true;
            }

            //�Z�b�V�����ɋ󂫂��Ȃ�
            if (sessionExist == false)
            {
                var dialog = Instantiate(_dialog, _canvas.transform);
                dialog.Init("���[����������܂���ł����B");
                return;
            }

            var args = new StartGameArgs()
            {
                GameMode = GameMode.Client,
                SceneManager = RunnerManager.Runner.GetComponent<NetworkSceneManagerDefault>(),
                ConnectionToken = Guid.NewGuid().ToByteArray(),
            };

            var result = await RunnerManager.Runner.StartGame(args);
            if (result.Ok == true)
            {
                //Debug.Log("Join Public Session");
            }
            else
            {
                //Debug.LogError($"Error : {result.ShutdownReason}");
            }
        }

        /// <summary>
        /// �v���C�x�[�g�Z�b�V�����ɎQ��
        /// </summary>
        /// <param name="sessionId"></param>
        public async void JoinPrivateSession(TMP_InputField sessionId)
        {
            //�Z�b�V�������݂̊m�F
            var sessionExist = RunnerManager.Instance.SessionInfoList.FirstOrDefault(x => x.Name == sessionId.text);
            if (sessionExist == null) //�Z�b�V������������Ȃ�
            {
                var dialog = Instantiate(_dialog, _canvas.transform);
                dialog.Init("���[����������܂���ł����B");
                return;
            }

            var args = new StartGameArgs()
            {
                GameMode = GameMode.Client,
                SceneManager = RunnerManager.Runner.GetComponent<NetworkSceneManagerDefault>(),
                SessionName = sessionId.text,
                ConnectionToken = Guid.NewGuid().ToByteArray(),
            };

            var result = await RunnerManager.Runner.StartGame(args);
            if (result.Ok == true)
            {
                //Debug.Log("Join Private Session");
            }
            else
            {
                //Debug.LogError($"Error : {result.ShutdownReason}");
            }
        }

        /// <summary>
        /// �p�l��UI�̐؂�ւ�
        /// </summary>
        /// <param name="index"></param>
        public void SwitchPanel(int index)
        {
            if (index >= _roomSettingPanels.Count || index < 0) return;

            _roomSettingPanels[_panelIndex].SetActive(false);
            _roomSettingPanels[index].SetActive(true);
            _panelIndex = index;
        }

        /// <summary>
        /// �v���C�x�[�g�Z�b�V����ID�̍쐬
        /// </summary>
        /// <returns></returns>
        private string GetSessionId()
        {
            string sessionId; //�Z�b�V����ID

            while (true)
            {
                int num = UnityEngine.Random.Range(0, RANDOMKEY_MAX);
                string id = num.ToString("D6");

                var result = RunnerManager.Instance.SessionInfoList.FirstOrDefault(x => x.Name == id);
                if (result == null)
                {
                    sessionId = id;
                    break;
                }
            }

            return sessionId;
        }
    }
}
