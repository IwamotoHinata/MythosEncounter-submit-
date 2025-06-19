namespace Scenes.Ingame.Player
{
    /// <summary>
    /// 体力, スタミナ,SAN値を変更するときのモードを指定する列挙型 
    /// </summary>
    public enum ChangeValueMode
    {
        /// <summary>
        /// 回復する
        /// </summary>
        Heal,
        /// <summary>
        /// ダメージを受ける
        /// </summary>
        Damage,
        /// <summary>
        /// ダメージを受ける（体力専用）
        /// </summary>
        Bleeding,

        /// <summary>
        /// デバッグ用.DebugToolのSliderの影響を受ける
        /// </summary>
        Debug
    }
}