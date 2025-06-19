using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using Fusion;
using UniRx;
using Scenes.Ingame.InGameSystem;
using Scenes.Ingame.Player;
using static UnityEditor.PlayerSettings;

public class DebugTool : EditorWindow
{
    private List<GameObject> _players = new List<GameObject>();
    //private bool _isInvincibleMode = false;

    private ReactiveProperty<bool> _isInvincibleMode = new ReactiveProperty<bool>(false);
    private bool _applyDebugValueMode = true;

    //private bool _applyDebugValue = false;
    private ReactiveProperty<int> _debugHealth = new ReactiveProperty<int>(100);
    private ReactiveProperty<int> _debugSanValue = new ReactiveProperty<int>(100);

    private bool _isOnlineMode = true;
    private Vector2 _scrollPosition = Vector2.zero;//���݂̃X�N���[���ʒu
    private List<GameObject> _itemPrefabs = new List<GameObject>();
    private SearchField _searchField;
    private string _searchText = "";

    //�f�o�b�O�p�̃��j��, ���ڂ��쐬
    [MenuItem("DebugTool/CreateGUI", false, 1)]
    static void CreateDebugGUI()
    {
        DebugTool window = GetWindow<DebugTool>();
        window.titleContent = new GUIContent("DebugTool");
    }

    
    private void Init()
    {
        //Subscribe
        _isInvincibleMode
            .Subscribe(x =>
            {
                //bool�l�ɉ����Ė��G���̗L��,������
                foreach (GameObject player in _players)
                {
                    player.GetComponent<PlayerStatus>().SetInvincibleModeBool(x);
                    Debug.Log("_isInvincibleMode");
                }
            });

        _debugHealth
            .Skip(1)
            .Where(_ => _applyDebugValueMode)
            .Subscribe(x =>
            {
                //_applyDebugValueMode��true�ɂȂ��Ă���Ƃ��A�l��ύX
                foreach (GameObject player in _players)
                {
                    player.GetComponent<PlayerStatus>().ChangeHealth(x, ChangeValueMode.Debug);
                }
            });

        _debugSanValue
            .Skip(1)
            .Where(_ => _applyDebugValueMode)
            .Subscribe(x =>
            {
                //_applyDebugValueMode��true�ɂȂ��Ă���Ƃ��A�l��ύX
                foreach (GameObject player in _players)
                {
                    player.GetComponent<PlayerStatus>().ChangeSanValue(x, ChangeValueMode.Debug);
                }
            });
    }

    /// <summary>
    /// �A�C�e���̃v���n�u�����܂Ƃ߂����X�g���쐬, �Ԃ��֐�
    /// </summary>
    /// <returns></returns>
    private List<GameObject> SetItemPrefabList(string search = "")
    {
        var _result = new List<GameObject>();
        var paths = Directory.GetFiles("Assets/Resources/Prefab/Item", search + "*.prefab");//.prefab�ŏI���t�@�C���̃p�X���擾

        //�擾�����p�X��p���ă��X�g�Ƀv���n�u�̏����i�[
        foreach (var path in paths)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null)
            {
                _result.Add(prefab);
            }
        }

        return _result;
    }


    private void OnGUI()
    {
        //�ŏ��ɏ����ŕK�v�ȕ����擾���Ă���
        //�Đ�������Ȃ��Ǝ擾�ł��Ȃ�����
        if (Application.isPlaying)
        {
            if (_players.Count == 0)//�v���C���[�̔z��
            {
                GameObject[] tmp = GameObject.FindGameObjectsWithTag("Player");
                _players.AddRange(tmp);

                Init();
            }
                
        }
        //����ȊO
        if (_itemPrefabs.Count == 0)//�A�C�e���̃��X�g
        {
            Debug.Log("�A�C�e�����X�g�쐬");
            _itemPrefabs = SetItemPrefabList();
        }

        //�Đ��I�����̏���(OnApplicationQuit���g���Ȃ�����)
        if (!Application.isPlaying)
        {
            //�v���C���[���X�g�̏�����
            if (_players.Count != 0)
                _players.Clear();

        }


        /*�e�����ɂ��ċL�q���Ă���*/
        EditorGUILayout.LabelField("�X�V����", EditorStyles.largeLabel);
        //�X�V����
        if (GUILayout.Button("�v���C���[�l���X�V" + "  ���݂̐l���F" + _players.Count, GUILayout.Width(250)))
        {
            //�V�[���Đ����łȂ���Ύ��s�ł��Ȃ�
            if (!Application.isPlaying)
                Debug.LogError("�Q�[�����ł͂Ȃ��̂Ŏ��s�ł��܂���");

            //�v���C���[�̔z��
            GameObject[] tmp = GameObject.FindGameObjectsWithTag("Player");
            _players.Clear();
            _players.AddRange(tmp);

        }

        if (GUILayout.Button("�A�C�e�����X�g�X�V", GUILayout.Width(250)))
        {
            _itemPrefabs = SetItemPrefabList();
            if (_searchText != "")
                _searchText = "";
        }

        //�I�����C���ݒ�
        _isOnlineMode = EditorGUILayout.Toggle("�I�����C�����[�h", _isOnlineMode);

        //�E�o�n�_�Ƀ��[�v
        EditorGUILayout.LabelField("�E�o�n�_�Ƀ��[�v", EditorStyles.largeLabel);
        if (GUILayout.Button("���s", GUILayout.Width(250)))
        {
            //�V�[���Đ����łȂ���Ύ��s�ł��Ȃ�
            if (!Application.isPlaying)
                Debug.LogError("�Q�[�����ł͂Ȃ��̂Ŏ��s�ł��܂���");


            GameObject escapePoint = GameObject.FindGameObjectWithTag("EscapePoint");

            foreach (GameObject player in _players) 
            {
                player.transform.position = escapePoint.transform.position;
            }
        }

        //�E�o�A�C�e����ڂ̑O�ɏo��
        EditorGUILayout.LabelField("�E�o�A�C�e����ڂ̑O�Ɉړ�", EditorStyles.largeLabel);
        if (GUILayout.Button("���s(�I�t���C������)", GUILayout.Width(250)))
        {
            //�V�[���Đ����łȂ���Ύ��s�ł��Ȃ�
            if (!Application.isPlaying)
                Debug.LogError("�Q�[�����ł͂Ȃ��̂Ŏ��s�ł��܂���");

            EscapeItem[] _escapeItem = FindObjectsOfType<EscapeItem>();
            GameObject myPlayer = GetMyPlayerOnline();
            for (int i = 0; i < _escapeItem.Length; i++)
            {
                if(_isOnlineMode)
                    _escapeItem[i].gameObject.transform.position = myPlayer.transform.position + myPlayer.transform.forward * 2 + Vector3.up * (i + 1);
                else
                    _escapeItem[i].gameObject.transform.position = _players[0].transform.position + _players[0].transform.forward * 2 + Vector3.up * (i + 1);
            }
        }


        //�v���C���[�̗̑͑������s��
        EditorGUILayout.LabelField("�v���C���[�̃p�����[�^�ύX", EditorStyles.largeLabel);
        _isInvincibleMode.Value = EditorGUILayout.Toggle("�U���𖳌���", _isInvincibleMode.Value);
        _applyDebugValueMode = EditorGUILayout.Toggle("Slider�̒l��K��", _applyDebugValueMode);
        _debugHealth.Value = EditorGUILayout.IntSlider("�̗�", _debugHealth.Value, 0, 100);
        _debugSanValue.Value = EditorGUILayout.IntSlider("SAN�l", _debugSanValue.Value, 0, 100);

        //�A�C�e���̐���
        EditorGUILayout.LabelField("�A�C�e���̐���", EditorStyles.largeLabel);

        _searchField ??= new SearchField();
        _searchText = _searchField.OnToolbarGUI(_searchText);
        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition,true, true);

        _itemPrefabs = SetItemPrefabList(_searchText);//�������e����ɕ\������A�C�e���{�^����ݒ�
        for (int i = 0; i < _itemPrefabs.Count; i++)
        {
            if (GUILayout.Button(_itemPrefabs[i].name, GUILayout.Width(250)))
            {
                //�V�[���Đ����łȂ���Ύ��s�ł��Ȃ�
                if (!Application.isPlaying)
                    Debug.LogError("�Q�[�����ł͂Ȃ��̂Ŏ��s�ł��܂���");

                foreach (GameObject player in _players)
                {
                    Vector3 spawnPos = player.transform.position + player.transform.forward * 3 + Vector3.up * 3;
                    if (_isOnlineMode)
                        RunnerSpawner.RunnerInstance.Spawn(_itemPrefabs[i], spawnPos, _itemPrefabs[i].transform.rotation);
                    else
                        Instantiate(_itemPrefabs[i], spawnPos, _itemPrefabs[i].transform.rotation);
                }                
            }
        }
        EditorGUILayout.EndScrollView();
    }

    private GameObject GetMyPlayerOnline()
    {
        if (!_isOnlineMode)
            return null;

        foreach (GameObject player in _players)
        {
            if (player.GetComponent<NetworkObject>().HasInputAuthority)
                return player;
            else
                continue;
        }

        return null;
    }
}
