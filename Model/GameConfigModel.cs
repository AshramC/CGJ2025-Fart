using QFramework;
using UnityEngine;

namespace FartGame
{
    public class GameConfigModel : AbstractModel
    {
        // 初始值配置
        public float InitialFartValue = 100f;
        public float BaseMoveSpeed = 5f;
        public float InitialBodySize = 1f;
        
        // 屁值消耗配置
        public float FartConsumptionRate = 5f; // 每秒消耗的屁值
        public float MinFartValue = 0f;
        public float MaxFartValue = 1000f;
        
        // 体型和速度映射配置
        public AnimationCurve BodySizeCurve; // 屁值到体型的映射曲线
        public AnimationCurve MoveSpeedCurve; // 屁值到速度的映射曲线
        
        // 体型和速度的范围
        public float MinBodySize = 0.5f;
        public float MaxBodySize = 5f;
        public float MinMoveSpeed = 1f;
        public float MaxMoveSpeed = 10f;
        
        // 游戏结束条件配置
        public float GameOverFartThreshold = 0f; // 屁值低于此值时游戏结束
        public bool EnableTimeLimit = false; // 是否启用时间限制
        public float GameTimeLimit = 300f; // 游戏时间限制（秒）
        
        protected override void OnInit()
        {
            // 初始化默认曲线
            if (BodySizeCurve == null || BodySizeCurve.length == 0)
            {
                BodySizeCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
            }
            
            if (MoveSpeedCurve == null || MoveSpeedCurve.length == 0)
            {
                // 速度随屁值增加而降低
                MoveSpeedCurve = new AnimationCurve(
                    new Keyframe(0f, 1f),
                    new Keyframe(1f, 0f)
                );
            }
        }
        
        // 计算体型大小
        public float CalculateBodySize(float fartValue)
        {
            float normalized = Mathf.Clamp01(fartValue / MaxFartValue);
            float curveValue = BodySizeCurve.Evaluate(normalized);
            return Mathf.Lerp(MinBodySize, MaxBodySize, curveValue);
        }
        
        // 计算移动速度
        public float CalculateMoveSpeed(float fartValue)
        {
            float normalized = Mathf.Clamp01(fartValue / MaxFartValue);
            float curveValue = MoveSpeedCurve.Evaluate(normalized);
            return Mathf.Lerp(MinMoveSpeed, MaxMoveSpeed, curveValue);
        }
    }
}
