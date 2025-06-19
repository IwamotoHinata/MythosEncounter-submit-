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
    private Vector2 _scrollPosition = Vector2.zero;//現在のスクロール位置
    private List<GameObject> _itemPrefabs = new List<GameObject>();
    private SearchField _searchField;
    private string _searchText = "";

    //デバッグ用のメニュ, 項目を作成
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
                //bool値に応じて無敵化の有効,無効化
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
                //_applyDebugValueModeがtrueになっているとき、値を変更
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
                //_applyDebugValueModeがtrueになっているとき、値を変更
                foreach (GameObject player in _players)
                {
                    player.GetComponent<PlayerStatus>().ChangeSanValue(x, ChangeValueMode.Debug);
                }
            });
    }

    /// <summary>
    /// アイテムのプレハブ情報をまとめたリストを作成, 返す関数
    /// </summary>
    /// <returns></returns>
    private List<GameObject> SetItemPrefabList(string search = "")
    {
        var _result = new List<GameObject>();
        var paths = Directory.GetFiles("Assets/Resources/Prefab/Item", search + "*.prefab");//.prefabで終わるファイルのパスを取得

        //取得したパスを用いてリストにプレハブの情報を格納
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
        //最初に処理で必要な物を取得しておく
        //再生中じゃないと取得できないもの
        if (Application.isPlaying)
        {
            if (_players.Count == 0)//プレイヤーの配列
            {
                GameObject[] tmp = GameObject.FindGameObjectsWithTag("Player");
                _players.AddRange(tmp);

                Init();
            }
                
        }
        //それ以外
        if (_itemPrefabs.Count == 0)//アイテムのリスト
        {
            Debug.Log("アイテムリスト作成");
            _itemPrefabs = SetItemPrefabList();
        }

        //再生終了時の処理(OnApplicationQuitが使えないため)
        if (!Application.isPlaying)
        {
            //プレイヤーリストの初期化
            if (_players.Count != 0)
                _players.Clear();

        }


        /*各処理について記述していく*/
        EditorGUILayout.LabelField("更新処理", EditorStyles.largeLabel);
        //更新処理
        if (GUILayout.Button("プレイヤー人数更新" + "  現在の人数：" + _players.Count, GUILayout.Width(250)))
        {
            //シーン再生中でなければ実行できない
            if (!Application.isPlaying)
                Debug.LogError("ゲーム中ではないので実行できません");

            //プレイヤーの配列
            GameObject[] tmp = GameObject.FindGameObjectsWithTag("Player");
            _players.Clear();
            _players.AddRange(tmp);

        }

        if (GUILayout.Button("アイテムリスト更新", GUILayout.Width(250)))
        {
            _itemPrefabs = SetItemPrefabList();
            if (_searchText != "")
                _searchText = "";
        }

        //オンライン設定
        _isOnlineMode = EditorGUILayout.Toggle("オンラインモード", _isOnlineMode);

        //脱出地点にワープ
        EditorGUILayout.LabelField("脱出地点にワープ", EditorStyles.largeLabel);
        if (GUILayout.Button("実行", GUILayout.Width(250)))
        {
            //シーン再生中でなければ実行できない
            if (!Application.isPlaying)
                Debug.LogError("ゲーム中ではないので実行できません");


            GameObject escapePoint = GameObject.FindGameObjectWithTag("EscapePoint");

            foreach (GameObject player in _players) 
            {
                player.transform.position = escapePoint.transform.position;
            }
        }

        //脱出アイテムを目の前に出現
        EditorGUILayout.LabelField("脱出アイテムを目の前に移動", EditorStyles.largeLabel);
        if (GUILayout.Button("実行(オフライン推奨)", GUILayout.Width(250)))
        {
            //シーン再生中でなければ実行できない
            if (!Application.isPlaying)
                Debug.LogError("ゲーム中ではないので実行できません");

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


        //プレイヤーの体力増減を行う
        EditorGUILayout.LabelField("プレイヤーのパラメータ変更", EditorStyles.largeLabel);
        _isInvincibleMode.Value = EditorGUILayout.Toggle("攻撃を無効化", _isInvincibleMode.Value);
        _applyDebugValueMode = EditorGUILayout.Toggle("Sliderの値を適応", _applyDebugValueMode);
        _debugHealth.Value = EditorGUILayout.IntSlider("体力", _debugHealth.Value, 0, 100);
        _debugSanValue.Value = EditorGUILayout.IntSlider("SAN値", _debugSanValue.Value, 0, 100);

        //アイテムの生成
        EditorGUILayout.LabelField("アイテムの生成", EditorStyles.largeLabel);

        _searchField ??= new SearchField();
        _searchText = _searchField.OnToolbarGUI(_searchText);
        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition,true, true);

        _itemPrefabs = SetItemPrefabList(_searchText);//検索内容を基に表示するアイテムボタンを設定
        for (int i = 0; i < _itemPrefabs.Count; i++)
        {
            if (GUILayout.Button(_itemPrefabs[i].name, GUILayout.Width(250)))
            {
                //シーン再生中でなければ実行できない
                if (!Application.isPlaying)
                    Debug.LogError("ゲーム中ではないので実行できません");

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
