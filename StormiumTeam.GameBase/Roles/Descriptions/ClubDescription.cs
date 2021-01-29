using System;
using System.Drawing;
using System.Runtime.InteropServices;
using GameHost.Injection;
using GameHost.Native.Char;
using GameHost.Revolution.Snapshot.Serializers;
using GameHost.Revolution.Snapshot.Systems;
using GameHost.Revolution.Snapshot.Utilities;
using GameHost.Simulation.Features.ShareWorldState.BaseSystems;
using GameHost.Simulation.TabEcs.Interfaces;
using JetBrains.Annotations;
using StormiumTeam.GameBase.Roles.Components;
using StormiumTeam.GameBase.Roles.Interfaces;

namespace StormiumTeam.GameBase.Roles.Descriptions
{
	// A club is not a team.
	// It only contains information like colors, names of a team, or any entity that is relative with.
	// A club shouldn't be used as a parent of entities (unlike team or player descriptions)
	//
	// The club data should only be used for visual stuff, not for gameplay and logic.
	public struct ClubDescription : IEntityDescription
	{
		public class RegisterRelative : Relative<ClubDescription>.Register
		{
		}

		public class Register : RegisterGameHostComponentData<ClubDescription>
		{
		}
	}

	public struct ClubInformation : IComponentData
	{
		public CharBuffer64 Name; //< we don't use a game resource since it would only stay one entity...
		public Color32      PrimaryColor;
		public Color32      SecondaryColor;

		public class Register : RegisterGameHostComponentData<ClubInformation>
		{
		}

		public struct Snapshot : IReadWriteSnapshotData<Snapshot>, ISnapshotSyncWithComponent<ClubInformation>
		{
			public uint Tick { get; set; }

			public CharBuffer64 Name;
			public Color32      PrimaryColor;
			public Color32      SecondaryColor;

			public void Serialize(in BitBuffer buffer, in Snapshot baseline, in EmptySnapshotSetup setup)
			{
				if (Name.Equals(baseline.Name))
					buffer.AddBool(false);
				else
				{
					var span         = Name.Span;
					var baselineSpan = baseline.Name.Span;

					buffer.AddBool(true)
					      .AddIntDelta(span.Length, baselineSpan.Length);
					for (var i = 0; i < span.Length; i++)
						buffer.AddUIntDelta(span[i], baselineSpan[i]);
				}

				buffer.AddUIntD4Delta(PrimaryColor.UInt, baseline.PrimaryColor.UInt)
				      .AddUIntD4Delta(SecondaryColor.UInt, baseline.SecondaryColor.UInt);
			}

			public void Deserialize(in BitBuffer buffer, in Snapshot baseline, in EmptySnapshotSetup setup)
			{
				if (buffer.ReadBool())
				{
					var baselineSpan = baseline.Name.Span;
					Name.SetLength(buffer.ReadIntDelta(baselineSpan.Length));

					var span = Name.Span;
					for (var i = 0; i < span.Length; i++)
						span[i] = (char) buffer.ReadUIntDelta(baselineSpan[i]);
				}
				else
					Name = baseline.Name;

				PrimaryColor.UInt   = buffer.ReadUIntD4Delta(baseline.PrimaryColor.UInt);
				SecondaryColor.UInt = buffer.ReadUIntD4Delta(baseline.SecondaryColor.UInt);
			}

			public void FromComponent(in ClubInformation component, in EmptySnapshotSetup setup)
			{
				Name           = component.Name;
				PrimaryColor   = component.PrimaryColor;
				SecondaryColor = component.SecondaryColor;
			}

			public void ToComponent(ref ClubInformation component, in EmptySnapshotSetup setup)
			{
				component.Name           = Name;
				component.PrimaryColor   = PrimaryColor;
				component.SecondaryColor = SecondaryColor;
			}
		}

		public class Serializer : DeltaSnapshotSerializerBase<Snapshot, ClubInformation>
		{
			public Serializer([NotNull] ISnapshotInstigator instigator, [NotNull] Context ctx) : base(instigator, ctx)
			{
				AddToBufferSettings = false;
			}
		}
	}

	[StructLayout(LayoutKind.Explicit)]
	public struct Color32 : IEquatable<Color32>
	{
		[FieldOffset(0)]
		public byte R;

		[FieldOffset(1)]
		public byte G;

		[FieldOffset(2)]
		public byte B;

		[FieldOffset(3)]
		public byte A;

		[FieldOffset(0)]
		public uint UInt;

		public static implicit operator Color(Color32 color)
		{
			return Color.FromArgb(color.A, color.R, color.G, color.B);
		}

		public static implicit operator Color32(Color color)
		{
			Color32 export;
			export.UInt = default;
			export.A    = color.A;
			export.R    = color.R;
			export.G    = color.G;
			export.B    = color.B;
			return export;
		}

		public bool Equals(Color32 other)
		{
			return R == other.R && G == other.G && B == other.B && A == other.A && UInt == other.UInt;
		}

		public override bool Equals(object? obj)
		{
			return obj is Color32 other && Equals(other);
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(R, G, B, A, UInt);
		}

		public static bool operator ==(Color32 left, Color32 right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(Color32 left, Color32 right)
		{
			return !left.Equals(right);
		}
	}
}