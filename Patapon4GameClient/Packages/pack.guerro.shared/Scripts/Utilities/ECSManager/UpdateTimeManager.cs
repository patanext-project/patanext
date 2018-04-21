using Unity.Entities;
using UnityEngine;
using UnityEngine.Experimental.PlayerLoop;

namespace Packages.pack.guerro.shared.Scripts.Utilities.ECSManager
{
    [UpdateAfter(typeof(UnityEngine.Experimental.PlayerLoop.Initialization.PlayerUpdateTime))]
    [AlwaysUpdateSystem]
    public class UpdateTimeManager : ComponentSystem
    {
        public static int FrameCount;
        
        protected override void OnUpdate()
        {
            FrameCount = Time.frameCount;
        }
    }
}