namespace PataNext.Game.Modules.Abilities.Components;

public struct StatisticModifier
{
    public float Attack;
    public float Defense;

    public float ReceiveDamage;

    public float MovementSpeed;
    public float MovementAttackSpeed;
    public float MovementReturnSpeed;
    public float AttackSpeed;

    public float AttackSeekRange;

    public float Weight;
    public float Knockback;

    public static readonly StatisticModifier Default = new StatisticModifier
    {
        Attack  = 1,
        Defense = 1,

        ReceiveDamage = 1,

        MovementSpeed       = 1,
        MovementAttackSpeed = 1,
        MovementReturnSpeed = 1,
        AttackSpeed         = 1,

        AttackSeekRange = 1,

        Weight    = 1,
        Knockback = 1,
    };
}