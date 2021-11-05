using System.Numerics;

namespace PataNext.CoreAbilities.Mixed.Descriptions
{
    public interface IThrowProjectileAbility : ISimpleAttackAbility
    {
        public Vector2 ThrowVelocity { get; }
        public Vector2 Gravity       { get; }
    }

    public interface IThrowProjectileAbilitySettings : SimpleAttackAbility.ISettings
    {
        public Vector2 ThrowVelocity { get; }
        public Vector2 Gravity       { get; }
    }
}