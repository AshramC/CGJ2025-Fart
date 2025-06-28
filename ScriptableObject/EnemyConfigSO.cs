using UnityEngine;

namespace FartGame
{
    [CreateAssetMenu(fileName = "EnemyConfig", menuName = "FartGame/Enemy Config", order = 1)]
    public class EnemyConfigSO : ScriptableObject
    {
        [Header("敌人基础配置")]
        [Tooltip("敌人的初始耐力值")]
        public float initialStamina = 100f;
        
        [Tooltip("敌人的标签，用于区分不同类型的敌人")]
        public string enemyTag = "Default";
        
        [Header("显示信息")]
        [Tooltip("敌人的显示名称")]
        public string displayName = "敌人";
        
        [Multiline(3)]
        [Tooltip("敌人的描述信息")]
        public string description = "默认敌人描述";
    }
}
