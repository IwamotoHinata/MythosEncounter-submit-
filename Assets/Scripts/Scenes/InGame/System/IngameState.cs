namespace Scenes.Ingame.InGameSystem
{
    /// <summary>
    /// Ingameでの状態
    /// </summary>
    public enum IngameState
    {
        Outgame,//インゲーム前
        Initial,//ステージ生成、キャラクタースポーン、敵スポーン
        Ingame,//プレイヤーの操作
        Result,//リザルト画面
    }
}