using System;
using GameHost.Simulation.TabEcs.Interfaces;
using GameHost.Worlds.Components;

namespace PataNext.CoreAbilities.Mixed
{
    public interface ISimpleAttackAbility : IComponentData
    {
        // Can only attack if Cooldown is passed, and if there are no delay before next attack
        public TimeSpan AttackStart { get; set; }

        // prev: HasThrown, HasSlashed, ...
        public bool DidAttack { get; set; }

        /// <summary>
        /// Cooldown before waiting for the next attack
        /// </summary>
        public TimeSpan Cooldown { get; set; }
        /// <summary>
        /// Delay before the attack (does not include <see cref="Cooldown"/>)
        /// </summary>
        public TimeSpan DelayBeforeAttack { get; }

        /// <summary>
        /// Delay after the attack (does not include <see cref="Cooldown"/>)
        /// </summary>
        public TimeSpan PauseAfterAttack { get; }
    }

    public static class SimpleAttackAbilityExtensions
    {
        public static bool TriggerAttack<T>(this ref T impl, in WorldTime worldTime, TimeSpan cooldown) where T : struct, ISimpleAttackAbility 
        {
            if (impl.AttackStart == TimeSpan.Zero && impl.Cooldown <= TimeSpan.Zero)
            {
                impl.Cooldown    = cooldown;
                impl.AttackStart = worldTime.Total;
                impl.DidAttack   = false;
                return true;
            }

            return false;
        }

        public static bool CanAttackThisFrame<T>(this ref T impl, in TimeSpan currentTime) where T : struct, ISimpleAttackAbility
        {
            if (currentTime > impl.AttackStart.Add(impl.DelayBeforeAttack) && !impl.DidAttack)
            {
                System.Console.WriteLine("Frame " + currentTime.TotalMilliseconds);
                impl.DidAttack = true;
                return true;
            }

            return false;
        }

        public static bool IsAttackingAndUpdate<T>(this ref T impl, in TimeSpan currentTime) where T : struct, ISimpleAttackAbility
        {
            if (impl.AttackStart != default)
            {
                if (impl.AttackStart.Add(impl.DelayBeforeAttack + impl.PauseAfterAttack) <= currentTime)
                {
                    impl.AttackStart = default;
                }

                return true;
            }

            return false;
        }
    }
}