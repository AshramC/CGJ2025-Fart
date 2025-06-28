using QFramework;
using UnityEngine;

namespace FartGame
{
    public class ApplyBattleDamageCommand : AbstractCommand
    {
        private readonly float damage;
        
        public ApplyBattleDamageCommand(float damage)
        {
            this.damage = damage;
        }
        
        protected override void OnExecute()
        {
            if (damage <= 0f) return;
            
            var fartSystem = this.GetSystem<FartSystem>();
            fartSystem.DamageFart(damage);
            
            Debug.Log($"[ApplyBattleDamageCommand] 应用战斗伤害: {damage}");
        }
    }
}
