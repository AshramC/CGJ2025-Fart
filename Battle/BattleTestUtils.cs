using UnityEngine;
using System.Text;

namespace FartGame.Battle
{
    /// <summary>
    /// 战斗测试工具类
    /// 提供测试辅助功能和统计信息显示
    /// </summary>
    public static class BattleTestUtils
    {
        /// <summary>
        /// 格式化时间显示
        /// </summary>
        public static string FormatTime(double timeSeconds)
        {
            int minutes = (int)(timeSeconds / 60);
            int seconds = (int)(timeSeconds % 60);
            int milliseconds = (int)((timeSeconds % 1) * 1000);
            
            return $"{minutes:D2}:{seconds:D2}.{milliseconds:D3}";
        }
        
        /// <summary>
        /// 格式化判定结果
        /// </summary>
        public static string FormatJudgeResult(BattleJudgeResult result)
        {
            return result switch
            {
                BattleJudgeResult.Success => "<color=green>✓ SUCCESS</color>",
                BattleJudgeResult.Miss => "<color=red>✗ MISS</color>",
                BattleJudgeResult.None => "<color=gray>○ NONE</color>",
                _ => "未知"
            };
        }
        
        /// <summary>
        /// 获取详细的谱面信息
        /// </summary>
        public static string GetChartDetailInfo(BattleChartData chart)
        {
            if (chart == null) return "无谱面数据";
            
            var sb = new StringBuilder();
            sb.AppendLine($"谱面名称: {chart.chartName}");
            sb.AppendLine($"BPM: {chart.bpm} | 小节数: {chart.measures} | 每小节拍数: {chart.beatsPerMeasure}");
            sb.AppendLine($"总时长: {chart.GetTotalDuration():F2}秒 | 下落时间: {chart.fixedDropTime}秒");
            sb.AppendLine($"难度: {chart.difficulty}/5 | 事件数: {chart.events?.Count ?? 0}");
            
            if (chart.events != null)
            {
                int tapCount = 0;
                int holdCount = 0;
                
                foreach (var evt in chart.events)
                {
                    if (evt.eventType == BattleEventType.Tap)
                        tapCount++;
                    else if (evt.eventType == BattleEventType.Hold)
                        holdCount++;
                }
                
                sb.AppendLine($"Tap音符: {tapCount} | Hold音符: {holdCount}");
            }
            
            return sb.ToString();
        }
        
        /// <summary>
        /// 获取系统性能信息
        /// </summary>
        public static string GetPerformanceInfo()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"FPS: {1f / Time.unscaledDeltaTime:F1}");
            sb.AppendLine($"帧时间: {Time.unscaledDeltaTime * 1000:F2}ms");
            sb.AppendLine($"时间缩放: {Time.timeScale:F2}");
            sb.AppendLine($"音频DSP时间: {AudioSettings.dspTime:F3}");
            
            // 内存信息
            long totalMemory = System.GC.GetTotalMemory(false);
            sb.AppendLine($"托管内存: {totalMemory / (1024 * 1024):F1}MB");
            
            return sb.ToString();
        }
        
        /// <summary>
        /// 创建简单的进度条
        /// </summary>
        public static string CreateProgressBar(float progress, int width = 20)
        {
            progress = Mathf.Clamp01(progress);
            int filledWidth = Mathf.RoundToInt(progress * width);
            
            var sb = new StringBuilder();
            sb.Append("[");
            
            for (int i = 0; i < width; i++)
            {
                sb.Append(i < filledWidth ? "■" : "□");
            }
            
            sb.Append($"] {progress * 100:F1}%");
            return sb.ToString();
        }
        
        /// <summary>
        /// 验证测试环境
        /// </summary>
        public static (bool isValid, string[] issues) ValidateTestEnvironment(BattleTestController controller)
        {
            var issues = new System.Collections.Generic.List<string>();
            
            if (controller == null)
            {
                issues.Add("BattleTestController为空");
                return (false, issues.ToArray());
            }
            
            // 检查必要组件
            var musicManager = controller.GetComponent<MusicTimeManager>();
            if (musicManager == null)
            {
                issues.Add("缺少MusicTimeManager组件");
            }
            
            var visualizer = controller.GetComponent<BattleTestVisualizer>();
            if (visualizer == null)
            {
                issues.Add("缺少BattleTestVisualizer组件");
            }
            
            // 检查测试数据
            var testChartsField = controller.GetType().GetField("testCharts", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (testChartsField != null)
            {
                var testCharts = testChartsField.GetValue(controller) as BattleChartData[];
                if (testCharts == null || testCharts.Length == 0)
                {
                    issues.Add("没有配置测试谱面数据");
                }
            }
            
            // 检查音频设置
            if (AudioSettings.speakerMode == AudioSpeakerMode.Prologic)
            {
                issues.Add("音频设置可能影响时间精度");
            }
            
            bool isValid = issues.Count == 0;
            return (isValid, issues.ToArray());
        }
        
        /// <summary>
        /// 生成测试报告
        /// </summary>
        public static string GenerateTestReport(BattleJudgement judgementSystem, BattleChartManager chartManager, double testDuration)
        {
            if (judgementSystem == null || chartManager == null)
                return "测试数据不完整";
            
            var sb = new StringBuilder();
            sb.AppendLine("=== 战斗系统测试报告 ===");
            sb.AppendLine($"测试时长: {FormatTime(testDuration)}");
            sb.AppendLine();
            
            // 谱面进度
            sb.AppendLine($"谱面完成度: {chartManager.GetProgress() * 100:F1}%");
            sb.AppendLine(CreateProgressBar(chartManager.GetProgress()));
            sb.AppendLine();
            
            // 判定统计
            sb.AppendLine("判定统计:");
            int validInputs = judgementSystem.totalInputs - judgementSystem.invalidCount;
            int successCount = validInputs - judgementSystem.missCount;
            sb.AppendLine($"  Success: {successCount}");
            sb.AppendLine($"  Miss: {judgementSystem.missCount}");
            sb.AppendLine($"  总准确率: {judgementSystem.GetAccuracy() * 100:F1}%");
            sb.AppendLine();
            
            // 性能信息
            sb.AppendLine("性能信息:");
            sb.AppendLine(GetPerformanceInfo());
            
            return sb.ToString();
        }
        
        /// <summary>
        /// 调试辅助：在场景视图中绘制音符轨道
        /// </summary>
        public static void DrawTrackGizmos(Vector3 center, float height, float width)
        {
            #if UNITY_EDITOR
            Gizmos.color = Color.gray;
            
            // 绘制轨道线
            Vector3 start = center + Vector3.up * height / 2;
            Vector3 end = center - Vector3.up * height / 2;
            Gizmos.DrawLine(start, end);
            
            // 绘制判定线
            Gizmos.color = Color.yellow;
            Vector3 judgeLeft = center + Vector3.left * width;
            Vector3 judgeRight = center + Vector3.right * width;
            Gizmos.DrawLine(judgeLeft, judgeRight);
            
            // 绘制生成区域
            Gizmos.color = Color.green;
            Vector3 spawnLeft = start + Vector3.left * width / 2;
            Vector3 spawnRight = start + Vector3.right * width / 2;
            Gizmos.DrawLine(spawnLeft, spawnRight);
            #endif
        }
        
        /// <summary>
        /// 获取建议的测试步骤
        /// </summary>
        public static string[] GetTestSteps()
        {
            return new string[]
            {
                "1. 检查基础组件是否正确初始化",
                "2. 验证谱面数据加载是否成功",
                "3. 测试音符生成和下落动画",
                "4. 验证空格键输入响应",
                "5. 检查判定窗口和结果反馈",
                "6. 测试Hold音符的完整流程",
                "7. 验证音乐时间同步精度",
                "8. 检查内存使用和性能表现",
                "9. 测试不同难度谱面的表现",
                "10. 验证系统重置和清理功能"
            };
        }
    }
}
