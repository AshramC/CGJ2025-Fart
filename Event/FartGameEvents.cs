namespace FartGame
{
    // 游戏状态变化事件
    public struct GameStateChangedEvent
    {
        public GameState NewState;
    }
    
    // 熏模式状态变化事件
    public struct FumeModeChangedEvent
    {
        public bool IsActive;
    }
    
    // 屁值耗尽事件
    public struct FartDepletedEvent
    {
        // 可以添加额外信息，如耗尽时的位置等
    }
    
    // 游戏结束原因枚举
    public enum GameOverReason
    {
        FartDepleted,    // 屁值耗尽
        TimeUp,          // 时间到
        PlayerDeath,     // 玩家死亡
        Manual           // 手动结束
    }
    
    // 游戏结束事件
    public struct GameOverEvent
    {
        public GameOverReason Reason;
        public float FinalFartValue;
        public float GameDuration;
        public UnityEngine.Vector3 PlayerPosition;
    }
    
    // 敌人被击败事件
    public struct EnemyDefeatedEvent
    {
        public string EnemyTag;
        public UnityEngine.Vector3 EnemyPosition;
        public float RemainingStamina;
    }
    
    // 敌人战斗开始事件
    public struct EnemyBattleStartEvent
    {
        public string EnemyTag;
        public UnityEngine.Vector3 BattlePosition;
    }
}
