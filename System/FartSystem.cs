using QFramework;
using UnityEngine;

namespace FartGame
{
    public class FartSystem : AbstractSystem
    {
        private PlayerModel mPlayerModel;
        private GameModel mGameModel;
        private GameConfigModel mConfig;
        
        private float mLastUpdateTime;
        private float mGameStartTime;
        
        protected override void OnInit()
        {
            mPlayerModel = this.GetModel<PlayerModel>();
            mGameModel = this.GetModel<GameModel>();
            mConfig = this.GetModel<GameConfigModel>();
            
            // 初始化玩家数据
            mPlayerModel.FartValue.Value = mConfig.InitialFartValue;
            mPlayerModel.MoveSpeed.Value = mConfig.BaseMoveSpeed;
            mPlayerModel.BodySize.Value = mConfig.InitialBodySize;
            
            // 监听熏模式变化
            this.RegisterEvent<FumeModeChangedEvent>(OnFumeModeChanged);
            
            // 监听游戏状态变化，记录游戏开始时间
            this.RegisterEvent<GameStateChangedEvent>(OnGameStateChanged);
            
            // 监听屁值变化，自动更新体型和速度
            mPlayerModel.FartValue.Register(OnFartValueChanged);
        }
        
        // 需要在游戏主循环中调用此方法
        public void Update()
        {
            // 只在游戏进行中且未暂停时更新
            if (mGameModel.CurrentGameState.Value != GameState.Gameplay || mGameModel.IsPaused.Value)
                return;
                
            float currentTime = Time.time;
            float deltaTime = currentTime - mLastUpdateTime;
            mLastUpdateTime = currentTime;
            
            // 熏模式下持续消耗屁值
            if (mPlayerModel.IsFumeMode.Value)
            {
                ConsumeFart(mConfig.FartConsumptionRate * deltaTime);
            }
            
            // 处理资源点回复效果
            UpdateResourcePointHealing(deltaTime);
            
            // 检查时间限制（如果启用）
            if (mConfig.EnableTimeLimit)
            {
                float elapsedTime = currentTime - mGameStartTime;
                if (elapsedTime >= mConfig.GameTimeLimit)
                {
                    this.SendCommand(new GameOverCommand(GameOverReason.TimeUp));
                }
            }
        }
        
        // 更新资源点回复效果
        private void UpdateResourcePointHealing(float deltaTime)
        {
            if (!isHealingActive)
                return;
                
            // 执行回复
            float healAmount = currentHealingRate * deltaTime;
            float newValue = Mathf.Min(mConfig.MaxFartValue, mPlayerModel.FartValue.Value + healAmount);
            mPlayerModel.FartValue.Value = newValue;
            
            // 更新剩余时间
            healingTimeRemaining -= deltaTime;
            
            // 检查是否结束
            if (healingTimeRemaining <= 0f)
            {
                StopResourcePointHealing();
            }
        }
        
        private void OnFumeModeChanged(FumeModeChangedEvent e)
        {
            if (e.IsActive)
            {
                // 开启熏模式时重置计时器
                mLastUpdateTime = Time.time;
            }
        }
        
        private void OnFartValueChanged(float newValue)
        {
            // 更新体型和速度
            mPlayerModel.BodySize.Value = mConfig.CalculateBodySize(newValue);
            mPlayerModel.MoveSpeed.Value = mConfig.CalculateMoveSpeed(newValue);
            
            // 检查屁值是否耗尽
            if (newValue <= mConfig.MinFartValue && mPlayerModel.IsFumeMode.Value)
            {
                // 屁值耗尽，自动关闭熏模式
                mPlayerModel.IsFumeMode.Value = false;
                this.SendEvent<FartDepletedEvent>();
            }
            
            // 检查游戏结束条件：屁值低于阈值
            if (newValue <= mConfig.GameOverFartThreshold && mGameModel.CurrentGameState.Value == GameState.Gameplay)
            {
                this.SendCommand(new GameOverCommand(GameOverReason.FartDepleted));
            }
        }
        
        private void ConsumeFart(float amount)
        {
            float newValue = Mathf.Max(mConfig.MinFartValue, mPlayerModel.FartValue.Value - amount);
            mPlayerModel.FartValue.Value = newValue;
        }
        
        // 增加屁值的公共方法（供其他系统或命令调用）
        public void AddFart(float amount)
        {
            float newValue = Mathf.Min(mConfig.MaxFartValue, mPlayerModel.FartValue.Value + amount);
            mPlayerModel.FartValue.Value = newValue;
        }
        
        // 重置玩家数据
        public void ResetPlayerData()
        {
            mPlayerModel.FartValue.Value = mConfig.InitialFartValue;
            mPlayerModel.MoveSpeed.Value = mConfig.BaseMoveSpeed;
            mPlayerModel.BodySize.Value = mConfig.InitialBodySize;
            mPlayerModel.Position.Value = UnityEngine.Vector3.zero;
            mPlayerModel.IsFumeMode.Value = false;
        }
        
        // 获取玩家熏模式状态的公共接口
        public bool IsPlayerInFumeMode()
        {
            return mPlayerModel.IsFumeMode.Value;
        }
        
        // 获取玩家当前屁值的公共接口
        public float GetPlayerFartValue()
        {
            return mPlayerModel.FartValue.Value;
        }
        
        // === 资源点回复系统 ===
        private bool isHealingActive = false;
        private float currentHealingRate = 0f;
        private float healingTimeRemaining = 0f;
        
        // 启动资源点回复效果
        public void StartResourcePointHealing(float healingRate, float duration)
        {
            if (isHealingActive)
            {
                Debug.LogWarning("[FartSystem] 已有回复效果在进行中，将覆盖当前效果");
            }
            
            isHealingActive = true;
            currentHealingRate = healingRate;
            healingTimeRemaining = duration;
            
            Debug.Log($"[FartSystem] 开始资源点回复 - 每秒回复{healingRate}，持续{duration}秒");
            this.SendEvent<ResourcePointActivatedEvent>();
        }
        
        // 停止资源点回复效果
        public void StopResourcePointHealing()
        {
            if (isHealingActive)
            {
                isHealingActive = false;
                currentHealingRate = 0f;
                healingTimeRemaining = 0f;
                
                Debug.Log("[FartSystem] 资源点回复效果已停止");
                this.SendEvent<ResourcePointDeactivatedEvent>();
            }
        }
        
        // 提升屁值上限（普通敌人奖励）
        public void IncreaseMaxFartValue(int amount)
        {
            float oldMax = mConfig.MaxFartValue;
            mConfig.MaxFartValue += amount;
            
            // 同时恢复到新的最大值
            mPlayerModel.FartValue.Value = mConfig.MaxFartValue;
            
            Debug.Log($"[FartSystem] 屁值上限提升 {amount}，从 {oldMax} 到 {mConfig.MaxFartValue}");
        }
        
        // 强制扣除屁值（战斗中受伤）
        public void DamageFart(float amount)
        {
            float newValue = Mathf.Max(mConfig.MinFartValue, mPlayerModel.FartValue.Value - amount);
            mPlayerModel.FartValue.Value = newValue;
            
            Debug.Log($"[FartSystem] 玩家受伤，扣除屁值 {amount}，当前: {newValue}");
        }
        
        // 检查是否正在回复中
        public bool IsHealingActive()
        {
            return isHealingActive;
        }
        
        // 获取回复状态信息
        public (bool isActive, float rate, float timeRemaining) GetHealingStatus()
        {
            return (isHealingActive, currentHealingRate, healingTimeRemaining);
        }
        
        // 恢复满血（战斗失败时调用）
        public void RestoreToFullHealth()
        {
            float oldValue = mPlayerModel.FartValue.Value;
            mPlayerModel.FartValue.Value = mConfig.MaxFartValue;
            
            Debug.Log($"[FartSystem] 玩家恢复满血: {oldValue} → {mConfig.MaxFartValue}");
            
            // 发送恢复事件
            this.SendEvent<PlayerHealthRestoredEvent>();
        }
        
        private void OnGameStateChanged(GameStateChangedEvent e)
        {
            // 游戏开始时记录开始时间
            if (e.NewState == GameState.Gameplay)
            {
                mGameStartTime = Time.time;
            }
        }
    }
}
