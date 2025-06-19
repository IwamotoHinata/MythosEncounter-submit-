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
        /// �������I�������ꍇ�l���Z�b�g
        /// </summary>
        /// <param name="stage">�X�e�[�W�̐��������������ꍇ</param>
        /// <param name="player">�v���C���[�̐��������������ꍇ</param>
        /// <param name="enemy">�G�̐��������������ꍇ</param>
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