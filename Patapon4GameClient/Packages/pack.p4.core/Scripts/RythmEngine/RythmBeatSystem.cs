using Unity.Entities;
using UnityEngine;

namespace P4.Core.RythmEngine
{
    /*
     * Should we have multiple engines running at the same time?
     */
    [AlwaysUpdateSystem]
    public class RythmBeatSystem : ComponentSystem
    {
        struct SystemGroup
        {
            public ComponentDataArray<DRythmBeatData> BeatData;
            public ComponentDataArray<DRythmTimeData> TimeData;

            public int Length;
        }

        [Inject] private SystemGroup m_Group;
        [Inject] private RythmSystem m_RythmSystem;
        
        protected override void OnUpdate()
        {
            var deltaTime = Time.unscaledDeltaTime;

            for (int i = 0; i != m_Group.Length; i++)
            {
                var beatData = m_Group.BeatData[i];
                var timeData = m_Group.TimeData[i];

                timeData.Value += deltaTime;
                if (timeData.Value - beatData.TimeOfPreviousBeat >= beatData.Interval)
                {
                    // New beat!
                    beatData.Beat += 1;
                    beatData.TimeOfPreviousBeat = timeData.Value;

                    m_RythmSystem.AddBeatEvent();
                }

                m_Group.BeatData[i] = beatData;
                m_Group.TimeData[i] = timeData;
            }
        }
    }
}