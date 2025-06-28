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
