using System;
using GameHost;

namespace PataponGameHost.Inputs
{
	public unsafe struct PlayerInput
	{
		public struct RhythmAction
		{
			public byte flags;

			public bool IsActive
			{
				get => Bits.ToBoolean(flags, 0);
				set => Bits.SetAt(ref flags, 0, value);
			}

			public bool FrameUpdate
			{
				get => Bits.ToBoolean(flags, 1);
				set => Bits.SetAt(ref flags, 1, value);
			}

			public bool IsSliding
			{
				get => Bits.ToBoolean(flags, 2);
				set => Bits.SetAt(ref flags, 2, value);
			}

			public bool WasPressed  => IsActive && FrameUpdate;
			public bool WasReleased => !IsActive && FrameUpdate;
		}

		private fixed byte actions[sizeof(byte) * 4];

		public Span<RhythmAction> Actions
		{
			get
			{
				fixed (byte* fixedPtr = actions)
					return new Span<RhythmAction>(fixedPtr, sizeof(byte) * 4);
			}
		}
	}
}