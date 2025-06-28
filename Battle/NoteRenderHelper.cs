using UnityEngine;

/// <summary>
/// 音符渲染计算辅助类 - 无状态的实用工具
/// 提供音符下落、位置计算等辅助方法
/// </summary>
public static class NoteRenderHelper
{
    /// <summary>
    /// 计算音符下落进度
    /// </summary>
    /// <param name="currentTime">当前时间</param>
    /// <param name="judgementTime">判定时间</param>
    /// <param name="spawnTime">生成时间</param>
    /// <returns>下落进度 (0-1)</returns>
    public static float CalculateDropProgress(double currentTime, double judgementTime, double spawnTime)
    {
        if (currentTime < spawnTime) return 0f;
        if (currentTime > judgementTime) return 1f;
        
        double dropDuration = judgementTime - spawnTime;
        if (dropDuration <= 0) return 1f;
        
        return (float)((currentTime - spawnTime) / dropDuration);
    }
    
    /// <summary>
    /// 计算音符当前位置
    /// </summary>
    /// <param name="currentTime">当前时间</param>
    /// <param name="judgementTime">判定时间</param>
    /// <param name="spawnTime">生成时间</param>
    /// <param name="startPos">起始位置</param>
    /// <param name="endPos">结束位置</param>
    /// <returns>当前位置</returns>
    public static Vector3 CalculateNotePosition(double currentTime, double judgementTime, double spawnTime, 
                                               Vector3 startPos, Vector3 endPos)
    {
        float progress = CalculateDropProgress(currentTime, judgementTime, spawnTime);
        return Vector3.Lerp(startPos, endPos, progress);
    }
    
    /// <summary>
    /// 检查音符是否应该生成
    /// </summary>
    /// <param name="currentTime">当前时间</param>
    /// <param name="spawnTime">生成时间</param>
    /// <returns>是否应该生成</returns>
    public static bool ShouldSpawn(double currentTime, double spawnTime)
    {
        return currentTime >= spawnTime;
    }
    
    /// <summary>
    /// 检查音符是否应该清理
    /// </summary>
    /// <param name="currentTime">当前时间</param>
    /// <param name="judgementTime">判定时间</param>
    /// <param name="cleanupDelay">清理延迟时间（秒）</param>
    /// <returns>是否应该清理</returns>
    public static bool ShouldCleanup(double currentTime, double judgementTime, float cleanupDelay = 1.0f)
    {
        return currentTime > judgementTime + cleanupDelay;
    }
    
    /// <summary>
    /// 检查音符是否在有效生命周期内
    /// </summary>
    /// <param name="currentTime">当前时间</param>
    /// <param name="judgementTime">判定时间</param>
    /// <param name="spawnTime">生成时间</param>
    /// <param name="cleanupDelay">清理延迟时间（秒）</param>
    /// <returns>是否在生命周期内</returns>
    public static bool IsInLifetime(double currentTime, double judgementTime, double spawnTime, float cleanupDelay = 1.0f)
    {
        double cleanupTime = judgementTime + cleanupDelay;
        return currentTime >= spawnTime && currentTime <= cleanupTime;
    }
    
    /// <summary>
    /// 计算音符透明度（基于生命周期）
    /// </summary>
    /// <param name="currentTime">当前时间</param>
    /// <param name="judgementTime">判定时间</param>
    /// <param name="spawnTime">生成时间</param>
    /// <param name="isProcessed">是否已处理</param>
    /// <returns>透明度值 (0-1)</returns>
    public static float CalculateNoteAlpha(double currentTime, double judgementTime, double spawnTime, bool isProcessed)
    {
        if (isProcessed)
        {
            return 0.3f; // 已处理的音符半透明
        }
        
        if (currentTime < spawnTime)
        {
            return 0f; // 未生成时透明
        }
        
        if (currentTime > judgementTime)
        {
            // 超过判定时间后逐渐变透明
            double timePastJudgement = currentTime - judgementTime;
            return Mathf.Lerp(1f, 0f, (float)(timePastJudgement / 1.0)); // 1秒内变透明
        }
        
        return 1f; // 正常状态不透明
    }
    
    /// <summary>
    /// 根据事件类型获取音符颜色
    /// </summary>
    /// <param name="eventType">事件类型</param>
    /// <returns>音符颜色</returns>
    public static Color GetNoteColor(FartGame.Battle.BattleEventType eventType)
    {
        return eventType switch
        {
            FartGame.Battle.BattleEventType.Tap => Color.cyan,      // 轻拍 - 青色
            FartGame.Battle.BattleEventType.Hold => Color.yellow,   // 长按 - 黄色
            _ => Color.white                                        // 默认 - 白色
        };
    }
    
    /// <summary>
    /// 根据判定结果获取反馈颜色
    /// </summary>
    /// <param name="result">判定结果</param>
    /// <returns>反馈颜色</returns>
    public static Color GetJudgementColor(FartGame.Battle.BattleJudgeResult result)
    {
        return result switch
        {
            FartGame.Battle.BattleJudgeResult.Success => Color.green,
            FartGame.Battle.BattleJudgeResult.Miss => Color.red,
            _ => Color.gray
        };
    }
}