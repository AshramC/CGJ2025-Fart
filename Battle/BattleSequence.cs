using UnityEngine;
using System.Collections.Generic;

namespace FartGame.Battle
{
    [System.Serializable]
    public class BattleSequence
    {
        public string sequenceName;
        public float bpm;
        public List<BeatInfo> beats;
        public float duration;
        
        public BattleSequence()
        {
            beats = new List<BeatInfo>();
        }
        
        public BeatInfo GetBeatAtTime(double time)
        {
            foreach (var beat in beats)
            {
                if (Mathf.Abs((float)(beat.time - time)) < 0.1f)
                {
                    return beat;
                }
            }
            return null;
        }
        
        public List<BeatInfo> GetBeatsInTimeRange(double startTime, double endTime)
        {
            var result = new List<BeatInfo>();
            foreach (var beat in beats)
            {
                if (beat.time >= startTime && beat.time <= endTime)
                {
                    result.Add(beat);
                }
            }
            return result;
        }
    }

    [System.Serializable]
    public class BeatInfo
    {
        public double time;
        public Direction direction;
        public BeatType type; // 16分音符、8分音符等
        
        public BeatInfo(double time, Direction direction, BeatType type = BeatType.Sixteenth)
        {
            this.time = time;
            this.direction = direction;
            this.type = type;
        }
    }
}
