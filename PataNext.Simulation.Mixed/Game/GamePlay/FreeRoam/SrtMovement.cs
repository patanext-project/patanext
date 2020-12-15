using System;
using System.Numerics;
using StormiumTeam.GameBase;

namespace PataNext.Module.Simulation.Game.GamePlay.FreeRoam
{
	public static class SrtMovement
	{
		/// <summary>
        ///     Move the character with the Aerial Srt algorithm.
        /// </summary>
        /// <param name="initialVelocity">The initial velocity to use</param>
        /// <param name="direction">The movement direction</param>
        /// <param name="settings">The movement settings</param>
        /// <param name="dt">Delta time</param>
        /// <returns>Return the new position</returns>
        public static Vector3 AerialMove(Vector3 initialVelocity, Vector3 direction, SrtAerialSettings settings, float dt)
		{
			// Fix NaN errors
			direction = SrtFixNaN(direction);

			var prevY = initialVelocity.Y;

			var wishSpeed    = direction.Length() * settings.BaseSpeed;
			var gridVelocity = new Vector3(initialVelocity.X, 0, initialVelocity.Z);
			var velocity     = initialVelocity;

			velocity = AerialAccelerate(velocity, direction, settings.Acceleration, settings.Control, dt);
			var finalVelocity      = ClampSpeed(new Vector3(velocity.X, 0, velocity.Z), gridVelocity, settings.BaseSpeed);
			var addSpeedFromHeight = Math.Clamp(-initialVelocity.Y * settings.AccelerationByHighsForce, 0, 1);

			var result = MathUtils.NormalizeSafe(finalVelocity) * (finalVelocity.Length() + addSpeedFromHeight);
			result.Y = prevY;
			return result;
		}

        /// <summary>
        ///     Move the character with the Srt (CPMA based) algorithm.
        /// </summary>
        /// <param name="initialVelocity">The initial velocity to use</param>
        /// <param name="direction">The movement direction</param>
        /// <param name="settings">The movement settings</param>
        /// <param name="dt">Delta time</param>
        /// <returns>Return the new position</returns>
        public static Vector3 GroundMove(Vector3 initialVelocity, Vector2 input, Vector3 direction, SrtGroundSettings settings, float dt, Vector3 pos = default)
		{
			// Fix NaN errors
			direction       = SrtFixNaN(direction);
			initialVelocity = SrtFixNaN(initialVelocity);
			
			// Set Y axe to zero
			var prevY = initialVelocity.Y;
			initialVelocity.Y = 0;

			var previousSpeed = initialVelocity.Length();
			var friction = GetFrictionPower
			(
				previousSpeed,
				settings.FrictionSpeedMin, settings.FrictionSpeedMax,
				settings.FrictionMin, settings.FrictionMax
			);

			var velocity = ApplyFriction(initialVelocity, direction, friction, settings.SurfaceFriction, settings.FrictionSpeed, settings.Acceleration,
				settings.Deacceleration, dt, pos);

			var wishSpeed                         = direction.Length() * settings.BaseSpeed;
			if (float.IsNaN(wishSpeed)) wishSpeed = 0;

			var strafeAngleNormalized = GetStrafeAngleNormalized(direction, MathUtils.NormalizeSafe(initialVelocity));

			if (wishSpeed > settings.BaseSpeed && wishSpeed < previousSpeed) wishSpeed = MathUtils.LerpNormalized(previousSpeed, wishSpeed, settings.DecayBaseSpeedFriction * dt);

			if (input.Y > 0.5f)
				if (previousSpeed >= settings.BaseSpeed - 0.5f)
				{
					wishSpeed = settings.SprintSpeed;

					settings.Acceleration = 2f;
				}

			velocity = GroundAccelerate
				(velocity, direction, wishSpeed, settings.Acceleration, Math.Min(strafeAngleNormalized, 0.25f), settings.DecayBaseSpeedFriction, dt);

			velocity.Y = prevY;

			return velocity;
		}

        public static Vector3 SrtFixNaN(Vector3 original)
        {
	        for (var i = 0; i != 3; i++)
		        if (float.IsNaN(original.Ref(i)))
			        original.Ref(i) = 0f;

	        return original;
        }

        /// <summary>
        ///     Get the power of the friction from the player speed
        /// </summary>
        /// <param name="speed">The player speed</param>
        /// <param name="frictionSpeedMin">The minimal speed for friction to be start</param>
        /// <param name="frictionSpeedMax">The maximal speed for friction to be stop</param>
        /// <param name="frictionMin">The minimal friction (between 0 and 1)</param>
        /// <param name="frictionMax">The maximal friction (between 0 and 1)</param>
        /// <returns>Return the new friction power</returns>
        public static float GetFrictionPower(float speed, float frictionSpeedMin, float frictionSpeedMax, float frictionMin, float frictionMax)
		{
			return Math.Clamp
			(
				frictionSpeedMin / Math.Clamp(speed, frictionSpeedMin, frictionSpeedMax),
				frictionMin, frictionMax
			);
		}

		public static float GetStrafeAngleNormalized(Vector3 direction, Vector3 velocityDirection)
		{
			return Math.Max(Math.Clamp(MathUtils.Angle(direction, velocityDirection), 1, 90) / 90f, 0f);
		}

		/// <summary>
		///     Apply the friction to a given velocity
		/// </summary>
		/// <param name="velocity">The player velocity</param>
		/// <param name="direction">The direction of the player</param>
		/// <param name="friction">The friction power to use</param>
		/// <param name="groundFriction">The friction power of the surface</param>
		/// <param name="accel">The acceleration of the player</param>
		/// <param name="deaccel">The deaceleration of the player</param>
		/// <param name="dt">The delta time</param>
		/// <returns>Return a new velocity from the friction</returns>
		public static Vector3 ApplyFriction(Vector3 velocity, Vector3 direction, float friction, float groundFriction, float maxSpeed, float accel, float deaccel, float dt, Vector3 pos)
		{
			direction = MathUtils.NormalizeSafe(direction);

			var initialSpeed = velocity.Length();

			velocity = MathUtils.MoveTowards(velocity, direction * initialSpeed, groundFriction * friction * dt);

			var remain = velocity.Length() - maxSpeed;
			if (remain > 0)
				velocity = MathUtils.MoveTowards(velocity, Vector3.Zero, dt * deaccel * Math.Min(remain, 1));

			velocity.Y = 0;
			return MathUtils.NormalizeSafe(velocity) * Math.Min(velocity.Length(), initialSpeed);
		}

		private const float FLT_MIN_NORMAL = 1.175494351e-38F;
		
		/// <summary>
        ///     Accelerate the player from ground from a given velocity
        /// </summary>
        /// <param name="velocity">The player velocity</param>
        /// <param name="wishDirection">The wished direction</param>
        /// <param name="wishSpeed">The wished speed</param>
        /// <param name="accelPower">The acceleration power</param>
        /// <param name="strafePower">The strafe power (think of CPMA movement)</param>
        /// <param name="dt">The delta time</param>
        /// <returns>The new velocity from the acceleration</returns>
        public static Vector3 GroundAccelerate(Vector3 velocity, Vector3 wishDirection, float wishSpeed, float accelPower, float strafePower, float decay, float dt)
		{
			var speed = MathUtils.LerpNormalized(velocity.Length(), Vector3.Dot(velocity, wishDirection), strafePower);
			//speed = math.length(velocity);

			var power     = 1 + Math.Abs(Vector3.Dot(MathUtils.NormalizeSafe(velocity), wishDirection));
			var nextSpeed = speed + accelPower * power * dt;
			if (nextSpeed >= wishSpeed && speed <= wishSpeed + FLT_MIN_NORMAL)
				nextSpeed = wishSpeed;

			if (wishDirection.Length() < 0.5f)
				return velocity;

			velocity += wishDirection * (nextSpeed + accelPower) * dt * power;
			velocity =  MathUtils.NormalizeSafe(velocity) * Math.Clamp(velocity.Length(), 0, Math.Min(Math.Max(speed, wishSpeed), nextSpeed));

			return velocity;
		}

        /// <summary>
        ///     Accelerate the player from air from a given velocity
        /// </summary>
        /// <param name="velocity">The player velocity</param>
        /// <param name="wishDirection">The wished direction</param>
        /// <param name="control">The control factor</param>
        /// <param name="dt">The delta time</param>
        /// <returns>The new velocity from the acceleration</returns>
        private static Vector3 AerialAccelerate(Vector3 velocity, Vector3 wishDirection, float acceleration, float control, float dt)
		{
			return velocity + wishDirection * control * dt * acceleration;
		}

		private static Vector3 ClampSpeed(Vector3 velocity, Vector3 initialVelocity, float speed)
		{	
			return MathUtils.ClampMagnitude(velocity, Math.Max(initialVelocity.Length(), speed));
		}
	}
	
		public struct SrtGroundSettings
	{
        /// <summary>
        ///     The minimal speed for friction to begin lowering (Recommanded Value: 10f)
        /// </summary>
        public float FrictionSpeedMin;

        /// <summary>
        ///     The maximal speed for friction to stop lowering (Recommanded Value: 20f)
        /// </summary>
        public float FrictionSpeedMax;

        /// <summary>
        ///     The minimal friction time (Recommanded Value: 0.25f)
        /// </summary>
        public float FrictionMin;

        /// <summary>
        ///     The maximal friction time (Recommanded Value: 1f)
        /// </summary>
        public float FrictionMax;

        /// <summary>
        ///     The default friction on a surface (Recommanded Value: 6f)
        /// </summary>
        public float SurfaceFriction;

		public float DecayBaseSpeedFriction;

        /// <summary>
        ///     The acceleration speed (Recommanded value: 0.2f)
        /// </summary>
        public float Acceleration;

        /// <summary>
        ///     The deacceleration speed (Recommanded value: 0.2f)
        /// </summary>
        public float Deacceleration;

        /// <summary>
        ///     The base speed (Recommanded value: 9f)
        /// </summary>
        public float BaseSpeed;

		public float FrictionSpeed;

        /// <summary>
        ///     The sprint speed (Recommanded value: 12f)
        /// </summary>
        public float SprintSpeed;

		public static SrtGroundSettings NewBase()
		{
			return new SrtGroundSettings
			{
				FrictionSpeedMin = 10f,
				FrictionSpeedMax = 25f,

				FrictionMin = 0.25f,
				FrictionMax = 1f,

				SurfaceFriction        = 50f,
				DecayBaseSpeedFriction = 2.5f,

				Acceleration   = 100f,
				Deacceleration = 15f,
				BaseSpeed      = 12f,
				SprintSpeed    = 14f
			};
		}
	}

	public struct SrtAerialSettings
	{
        /// <summary>
        ///     The acceleration speed (Recommanded value: 14f)
        /// </summary>
        public float Acceleration;

        /// <summary>
        ///     The force power [0-1] of the acceleration provoked by the Y axis (Recommanded value: 0.25f)
        /// </summary>
        public float AccelerationByHighsForce;

        /// <summary>
        ///     The base speed (Recommanded value: 9f)
        /// </summary>
        public float BaseSpeed;

        /// <summary>
        ///     The force of the air control (Recommanded value: 12.5f)
        /// </summary>
        public float Control;

		public static SrtAerialSettings NewBase()
		{
			return new SrtAerialSettings
			{
				Acceleration             = 1f,
				AccelerationByHighsForce = 0.004f,
				Control                  = 60f,
				BaseSpeed                = 11f
			};
		}
	}
}