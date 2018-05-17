using Unity.Entities;

namespace P4.Core.RythmEngine
{
    public struct DRythmBeatData : IComponentData
    {
        /// <summary>
        /// The current beat
        /// </summary>
        public int Beat;

        public float Interval;

        public float TimeOfPreviousBeat;
    }
}