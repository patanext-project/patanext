namespace PataNext.Game.Abilities
{

	public enum StatusEffect
	{
		/// <summary>
		/// Invalid status effect, if you meant to use a custom one, refer to <see cref="CustomEffect"/>
		/// </summary>
		Invalid = 0,

		/// <summary>
		///     Critical effect. The damage bonus is based on the crits applied to the victim.
		///     (50% = x1.5 dmg, 100% = x2 dmg, ...)
		/// </summary>
		Critical = 1,

		/// <summary>
		///     KnockBack effect.
		/// </summary>
		KnockBack = 2,

		/// <summary>
		///     Stagger effect.
		///     This effect can be applied on grounded or in air units.
		///     This effect don't cancel a hero mode.
		/// </summary>
		Stagger = 3,

		/// <summary>
		///     Burn effect.
		///     The damage is based on the rate.
		///     The first level (less than 100%) will only apply a damage over time.
		///     The second level (more or equal than 100%) will apply 2x faster DOT and will make the entity running around.
		///     The third level (more than 200%) will double the DOT damage.
		///     The last two levels can make units near of the victim being also burnt.
		/// </summary>
		Burn = 4,

		/// <summary>
		///     Sleep effect.
		///     The first level (less than 100%) will only apply a slowness (movement speed and attack speed -75%)
		///     The second level (more or equal than 100%) will make the target sleeping (can't do anything at all)
		///     The unit will be awaken if it got hit
		/// </summary>
		Sleep = 5,

		/// <summary>
		///     Freeze effect.
		///     The first level (less than 100%) will make the target being slowed down (movement speed and attack speed -50%) and
		///     apply a small DOT
		///     The second level will totally freeze the target.
		/// </summary>
		Freeze = 6,

		/// <summary>
		///     Poison effect.
		///     The damage effect is based on the enemy health and the effect rate.
		///     The second level (more than 100%) will make the poison goes through enemies
		/// </summary>
		Poison = 7,

		/// <summary>
		///     Tumble effect.
		///     This effect will make ground units fall for some time
		/// </summary>
		Tumble = 8,

		/// <summary>
		///     Wind effect.
		///     The target status effect attack rate is reduced by the wind power.
		/// </summary>
		Wind = 9,

		/// <summary>
		///     Bulldozer effect.
		///     Block physical attacks (except if the attacker had a higher power than the current resistence power)
		/// </summary>
		Piercing = 10,
		
		/// <summary>
		/// 	Silence effect.
		/// 	Deactivate ability switching and hero mode activation.
		/// </summary>
		Silence = 11,
		
		/// <summary>
		/// 	This is a custom effect, to get the resource ID, substract 1000 from it
		/// </summary>
		CustomEffect = 1000,
	}
}