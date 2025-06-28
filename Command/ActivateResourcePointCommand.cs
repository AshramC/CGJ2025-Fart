using QFramework;
using UnityEngine;

namespace FartGame
{
    public class ActivateResourcePointCommand : AbstractCommand
    {
        private readonly EnemyController resourceController;
        private readonly float healingRate;
        private readonly float healingDuration;
        
        public ActivateResourcePointCommand(EnemyController controller, float rate, float duration)
        {
            resourceController = controller;
            healingRate = rate;
            healingDuration = duration;
        }
        
        protected override void OnExecute()
        {
            if (resourceController == null)
            {
                Debug.LogError("[ActivateResourcePointCommand] 资源点控制器为空");
                return;
            }
            
            // 验证是否为资源点类型
            if (resourceController.EnemyType != EnemyType.ResourcePoint)
            {
                Debug.LogError($"[ActivateResourcePointCommand] {resourceController.name} 不是资源点类型");
                return;
            }
            
            // 验证是否已被击败
            if (!resourceController.IsDefeated)
            {
                Debug.LogError($"[ActivateResourcePointCommand] 资源点 {resourceController.name} 尚未被击败");
                return;
            }
            
            // 验证是否已激活
            if (resourceController.IsResourceActive)
            {
                Debug.LogWarning($"[ActivateResourcePointCommand] 资源点 {resourceController.name} 已处于激活状态");
                return;
            }
            
            // 启动FartSystem的回复效果
            var fartSystem = this.GetSystem<FartSystem>();
            fartSystem.StartResourcePointHealing(healingRate, healingDuration);
            
            // 激活资源点状态
            resourceController.ActivateResourcePoint();
            
            // 发送资源点激活事件
            this.SendEvent(new ResourcePointActivatedEvent
            {
                resourceController = resourceController,
                healingRate = healingRate,
                duration = healingDuration
            });
            
            Debug.Log($"[ActivateResourcePointCommand] 资源点激活成功 - 每秒回复{healingRate}，持续{healingDuration}秒");
        }
    }
}
