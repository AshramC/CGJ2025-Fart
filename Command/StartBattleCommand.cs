using QFramework;
using UnityEngine;
using FartGame.Battle;

namespace FartGame
{
    public class StartBattleCommand : AbstractCommand
    {
        private readonly EnemyData enemyData;
        
        public StartBattleCommand(EnemyData enemyData)
        {
            this.enemyData = enemyData;
        }
        
        protected override void OnExecute()
        {
            var gameModel = this.GetModel<GameModel>();
            gameModel.CurrentGameState.Value = GameState.Battle;
            
            Debug.Log($"[StartBattle] 开始战斗: {enemyData.enemyName}");
        }
    }
}
