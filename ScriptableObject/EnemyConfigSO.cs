using UnityEngine;
using FartGame.Battle;

namespace FartGame
{
    public enum EnemyType
    {
        Normal,         // 普通敌人
        ResourcePoint   // 资源点敌人
    }
    
    [CreateAssetMenu(fileName = "EnemyConfig", menuName = "FartGame/Enemy Config", order = 1)]
    public class EnemyConfigSO : ScriptableObject
    {
        [Header("敌人类型")]
        [Tooltip("敌人类型，决定击败后的行为模式")]
        public EnemyType enemyType = EnemyType.Normal;
        
        [Header("基础信息")]
        [Tooltip("敌人的显示名称")]
        public string displayName = "敌人";
        
        [Multiline(3)]
        [Tooltip("敌人的描述信息")]
        public string description = "默认敌人描述";
        
        [Header("战斗配置")]
        [Tooltip("敌人的攻击力，玩家Miss时受到的伤害")]
        public float attackPower = 10f;
        
        [Tooltip("该敌人的战斗谱面数据")]
        public BattleChartData battleChart;
        
        [Header("普通敌人奖励（仅Normal类型有效）")]
        [Tooltip("击败后获得的屁值上限提升")]
        public int valueReward = 20;
        
        [Tooltip("击败后获得的资源点数量")]
        public int resourceReward = 3;
        
        [Header("资源点配置（仅ResourcePoint类型有效）")]
        [Tooltip("每秒回复的屁值数量")]
        public float healingRate = 5f;
        
        [Tooltip("回复效果持续时间（秒）")]
        public float healingDuration = 10f;
    }
}
