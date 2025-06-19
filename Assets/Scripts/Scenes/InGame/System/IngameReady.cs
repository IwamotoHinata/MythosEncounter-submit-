namespace Scenes.Ingame.InGameSystem
{
    public struct IngameReady
    {
        public bool StageReady;
        public bool PlayerReady;
        public bool EnemyReady;
        public void Initialize()
        {
            StageReady = false;
            PlayerReady = false;
            EnemyReady = false;
        }

        /// <summary>
        /// 生成が終了した場合値をセット
        /// </summary>
        /// <param name="stage">ステージの生成が完了した場合</param>
        /// <param name="player">プレイヤーの生成が完了した場合</param>
        /// <param name="enemy">敵の生成が完了した場合</param>
        public void SetReady(ReadyEnum ready)
        {
            switch (ready)
            {
                case ReadyEnum.StageReady:
                    StageReady = true;
                    break;
                case ReadyEnum.PlayerReady:
                    PlayerReady = true;
                    break;
                case ReadyEnum.EnemyReady:
                    EnemyReady = true;
                    break;
                default:
                    break;
            }
        }
        public bool Ready()
        {
            if (StageReady && PlayerReady && EnemyReady) return true;
            else return false;
        }
    }
}