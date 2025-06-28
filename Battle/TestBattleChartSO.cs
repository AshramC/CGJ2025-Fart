using UnityEngine;
using System.Collections.Generic;

namespace FartGame.Battle
{
    /// <summary>
    /// 测试用战斗谱面数据生成器
    /// 提供预设的测试谱面配置
    /// </summary>
    [CreateAssetMenu(fileName = "TestBattleChart", menuName = "Battle System/Test Battle Chart")]
    public class TestBattleChartSO : ScriptableObject
    {
        [Header("测试谱面列表")]
        public List<BattleChartData> testCharts = new List<BattleChartData>();
        
        [Header("生成设置")]
        [SerializeField] private bool autoGenerateOnValidate = false;
        
        /// <summary>
        /// 生成所有测试谱面
        /// </summary>
        [ContextMenu("生成测试谱面")]
        public void GenerateTestCharts()
        {
            testCharts.Clear();
            
            testCharts.Add(CreateBasicChart());
            testCharts.Add(CreateRhythmChart());
            testCharts.Add(CreateHoldChart());
            testCharts.Add(CreateMixedChart());
            testCharts.Add(CreateDenseChart());
            
            Debug.Log($"[TestBattleChartSO] 生成了 {testCharts.Count} 个测试谱面");
        }
        
        /// <summary>
        /// 创建基础测试谱面
        /// </summary>
        private BattleChartData CreateBasicChart()
        {
            var chart = CreateInstance<BattleChartData>();
            chart.chartName = "基础测试";
            chart.description = "简单的四分音符，适合基础功能测试";
            chart.bpm = 120;
            chart.measures = 4;
            chart.beatsPerMeasure = 8;
            chart.fixedDropTime = 2.0f;
            chart.difficulty = 1;
            
            chart.events = new List<BattleBeatEvent>();
            
            // 每小节的第1、3、5、7拍放置Tap事件
            for (int measure = 0; measure < chart.measures; measure++)
            {
                chart.events.Add(new BattleBeatEvent(measure, 0)); // 第1拍
                chart.events.Add(new BattleBeatEvent(measure, 2)); // 第3拍
                chart.events.Add(new BattleBeatEvent(measure, 4)); // 第5拍
                chart.events.Add(new BattleBeatEvent(measure, 6)); // 第7拍
            }
            
            return chart;
        }
        
        /// <summary>
        /// 创建节奏测试谱面
        /// </summary>
        private BattleChartData CreateRhythmChart()
        {
            var chart = CreateInstance<BattleChartData>();
            chart.chartName = "节奏测试";
            chart.description = "更复杂的节奏模式，测试时间精度";
            chart.bpm = 140;
            chart.measures = 4;
            chart.beatsPerMeasure = 8;
            chart.fixedDropTime = 2.0f;
            chart.difficulty = 2;
            
            chart.events = new List<BattleBeatEvent>();
            
            // 创建更复杂的节奏模式
            for (int measure = 0; measure < chart.measures; measure++)
            {
                if (measure % 2 == 0)
                {
                    // 偶数小节：密集模式
                    for (int beat = 0; beat < chart.beatsPerMeasure; beat += 1)
                    {
                        chart.events.Add(new BattleBeatEvent(measure, beat));
                    }
                }
                else
                {
                    // 奇数小节：稀疏模式
                    chart.events.Add(new BattleBeatEvent(measure, 0));
                    chart.events.Add(new BattleBeatEvent(measure, 3));
                    chart.events.Add(new BattleBeatEvent(measure, 6));
                }
            }
            
            return chart;
        }
        
        /// <summary>
        /// 创建Hold测试谱面
        /// </summary>
        private BattleChartData CreateHoldChart()
        {
            var chart = CreateInstance<BattleChartData>();
            chart.chartName = "Hold测试";
            chart.description = "专门测试Hold音符功能";
            chart.bpm = 100;
            chart.measures = 4;
            chart.beatsPerMeasure = 8;
            chart.fixedDropTime = 2.5f;
            chart.difficulty = 2;
            
            chart.events = new List<BattleBeatEvent>();
            
            // 添加不同长度的Hold事件
            for (int measure = 0; measure < chart.measures; measure++)
            {
                switch (measure)
                {
                    case 0:
                        // 短Hold: 2拍
                        chart.events.Add(new BattleBeatEvent(measure, 0, 2));
                        chart.events.Add(new BattleBeatEvent(measure, 4, 6));
                        break;
                    case 1:
                        // 中Hold: 3拍
                        chart.events.Add(new BattleBeatEvent(measure, 1, 4));
                        chart.events.Add(new BattleBeatEvent(measure, 5));
                        break;
                    case 2:
                        // 长Hold: 4拍
                        chart.events.Add(new BattleBeatEvent(measure, 0, 4));
                        chart.events.Add(new BattleBeatEvent(measure, 6));
                        break;
                    case 3:
                        // 超长Hold: 6拍
                        chart.events.Add(new BattleBeatEvent(measure, 1, 7));
                        break;
                }
            }
            
            return chart;
        }
        
        /// <summary>
        /// 创建混合测试谱面
        /// </summary>
        private BattleChartData CreateMixedChart()
        {
            var chart = CreateInstance<BattleChartData>();
            chart.chartName = "混合测试";
            chart.description = "Tap和Hold混合，综合测试";
            chart.bpm = 130;
            chart.measures = 6;
            chart.beatsPerMeasure = 8;
            chart.fixedDropTime = 2.0f;
            chart.difficulty = 3;
            
            chart.events = new List<BattleBeatEvent>();
            
            for (int measure = 0; measure < chart.measures; measure++)
            {
                switch (measure % 3)
                {
                    case 0:
                        // Tap主导
                        chart.events.Add(new BattleBeatEvent(measure, 0));
                        chart.events.Add(new BattleBeatEvent(measure, 1));
                        chart.events.Add(new BattleBeatEvent(measure, 3));
                        chart.events.Add(new BattleBeatEvent(measure, 5));
                        chart.events.Add(new BattleBeatEvent(measure, 7));
                        break;
                    case 1:
                        // Hold主导
                        chart.events.Add(new BattleBeatEvent(measure, 0, 3));
                        chart.events.Add(new BattleBeatEvent(measure, 5, 7));
                        break;
                    case 2:
                        // 混合模式
                        chart.events.Add(new BattleBeatEvent(measure, 0));
                        chart.events.Add(new BattleBeatEvent(measure, 2, 5));
                        chart.events.Add(new BattleBeatEvent(measure, 6));
                        chart.events.Add(new BattleBeatEvent(measure, 7));
                        break;
                }
            }
            
            return chart;
        }
        
        /// <summary>
        /// 创建密集测试谱面
        /// </summary>
        private BattleChartData CreateDenseChart()
        {
            var chart = CreateInstance<BattleChartData>();
            chart.chartName = "密集测试";
            chart.description = "高密度音符，性能和反应测试";
            chart.bpm = 160;
            chart.measures = 4;
            chart.beatsPerMeasure = 8;
            chart.fixedDropTime = 1.5f;
            chart.difficulty = 5;
            
            chart.events = new List<BattleBeatEvent>();
            
            // 创建高密度谱面
            for (int measure = 0; measure < chart.measures; measure++)
            {
                for (int beat = 0; beat < chart.beatsPerMeasure; beat++)
                {
                    // 75%的拍子放置音符
                    if ((measure * chart.beatsPerMeasure + beat) % 4 != 3)
                    {
                        chart.events.Add(new BattleBeatEvent(measure, beat));
                    }
                }
                
                // 每小节最后添加一个Hold
                if (measure < chart.measures - 1)
                {
                    chart.events.Add(new BattleBeatEvent(measure, chart.beatsPerMeasure - 2, chart.beatsPerMeasure - 1));
                }
            }
            
            return chart;
        }
        
        /// <summary>
        /// 获取指定索引的测试谱面
        /// </summary>
        public BattleChartData GetTestChart(int index)
        {
            if (testCharts == null || index < 0 || index >= testCharts.Count)
                return null;
                
            return testCharts[index];
        }
        
        /// <summary>
        /// 获取测试谱面数量
        /// </summary>
        public int GetTestChartCount()
        {
            return testCharts?.Count ?? 0;
        }
        
        /// <summary>
        /// 验证所有测试谱面
        /// </summary>
        [ContextMenu("验证测试谱面")]
        public void ValidateAllCharts()
        {
            int validCount = 0;
            int invalidCount = 0;
            
            foreach (var chart in testCharts)
            {
                if (chart != null)
                {
                    var (isValid, errors) = chart.ValidateChart();
                    if (isValid)
                    {
                        validCount++;
                        Debug.Log($"✓ {chart.chartName} - 验证通过");
                    }
                    else
                    {
                        invalidCount++;
                        Debug.LogError($"✗ {chart.chartName} - 验证失败: {string.Join(", ", errors)}");
                    }
                }
                else
                {
                    invalidCount++;
                    Debug.LogError("✗ 发现空的谱面数据");
                }
            }
            
            Debug.Log($"[TestBattleChartSO] 验证完成 - 有效: {validCount}, 无效: {invalidCount}");
        }
        
        /// <summary>
        /// 打印所有谱面统计
        /// </summary>
        [ContextMenu("打印谱面统计")]
        public void PrintChartStatistics()
        {
            for (int i = 0; i < testCharts.Count; i++)
            {
                var chart = testCharts[i];
                if (chart != null)
                {
                    Debug.Log($"=== 谱面 {i + 1}: {chart.chartName} ===");
                    chart.PrintStatistics();
                }
            }
        }
        
        /// <summary>
        /// Unity编辑器验证
        /// </summary>
        private void OnValidate()
        {
            if (autoGenerateOnValidate && (testCharts == null || testCharts.Count == 0))
            {
                GenerateTestCharts();
            }
        }
    }
}
