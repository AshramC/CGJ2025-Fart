using UnityEngine;
using System.Collections.Generic;

namespace FartGame.Battle
{
    // 数据传输对象
    [System.Serializable]
    public class PlayerBattleData
    {
        public float fartValue;
        public Vector3 position;
        public bool isInFumeMode;
        // 其他需要的玩家数据
    }

    [System.Serializable]
    public class EnemyData
    {
        public string enemyName;
        public float maxStamina;
        public BattleSequence battleSequence;
        public GameObject enemyPrefab;
    }

    [System.Serializable]
    public class BattleResult
    {
        public bool isVictory;
        public float remainingStamina;
        public int totalHits;
        public int totalMisses;
        public float accuracy;
        public int maxCombo;
    }

    // 战斗状态枚举
    public enum BattlePhase 
    { 
        Initializing, 
        Preparing, 
        Playing, 
        Paused, 
        Ending, 
        Completed 
    }

    // 方向枚举
    public enum Direction { Up, Down, Left, Right }

    // 节拍类型
    public enum BeatType { Quarter, Eighth, Sixteenth }

    // 判定结果
    [System.Serializable]
    public class JudgementResult
    {
        public bool isHit;
        public bool isMiss;
        public float accuracy; // 0-1范围
        public double timeDifference;
    }

    // 战斗状态信息
    [System.Serializable]
    public class BattleStatus
    {
        public BattlePhase phase;
        public float enemyStamina;
        public int currentCombo;
        public float buttTransparency;
        public double currentMusicTime;
    }
}

// QFramework事件定义
namespace FartGame
{
    public struct BattleCompletedEvent
    {
        public FartGame.Battle.BattleResult Result;
    }
}
