using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Fusion;

public class RunnerSpawner : MonoBehaviour
{
    [SerializeField] private NetworkRunner _runnerPrefab;
    public static NetworkRunner RunnerInstance;

    async void StartGame(GameMode mode)
    {
        RunnerInstance = Instantiate(_runnerPrefab);

        //予期しないシャットダウンを処理できるようにシャットダウン用のリスナーを設定
        var events = RunnerInstance.GetComponent<NetworkEvents>();
        events.OnShutdown.AddListener(OnShutdown);

        var scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex);
        var sceneInfo = new NetworkSceneInfo();

        if (scene.IsValid)
        {
            sceneInfo.AddSceneRef(scene, LoadSceneMode.Additive);
        }

        await RunnerInstance.StartGame(new StartGameArgs()
        {
            GameMode = mode,
            SessionName = "TestRoom",
            Scene = scene,
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        });

 
    }

    public void DirectStart(GameMode mode)
    {
        StartGame(mode);
    }

    void OnGUI()
    {
        if (RunnerInstance == null)
        {
            if (GUI.Button(new Rect(0, 0, 200, 40), "Host"))
            {
                StartGame(GameMode.Host);
            }
            if (GUI.Button(new Rect(0, 40, 200, 40), "Join"))
            {
                StartGame(GameMode.Client);
            }
            if (GUI.Button(new Rect(0, 80, 200, 40), "Single"))
            {
                StartGame(GameMode.Single);
            }
        }
    }

    public async Task Disconnect()
    {
        if (RunnerInstance == null)
            return;

        // Remove shutdown listener since we are disconnecting deliberately
        var events = RunnerInstance.GetComponent<NetworkEvents>();
        events.OnShutdown.RemoveListener(OnShutdown);

        await RunnerInstance.Shutdown();
        RunnerInstance = null;

        // Reset of scene network objects is needed, reload the whole scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void OnShutdown(NetworkRunner runner, ShutdownReason reason)
    {
        // Unexpected shutdown happened (e.g. Host disconnected)

        // Reset of scene network objects is needed, reload the whole scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }


}
