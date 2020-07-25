using System;
using System.Runtime.CompilerServices;
using GameHost.Core.IO;
using RevolutionSnapshot.Core.Buffers;

namespace PataNext.Export.Desktop
{
	// TODO: Should be added to Transport library once more stable and with more features
	/// <summary>
	/// A header driver only encapsulate data around a header.
	/// </summary>
	public class HeaderTransportDriver : TransportDriver
	{
		public readonly TransportDriver  Source;
		
		public DataBufferWriter Header { get; set; }

		private DataBufferWriter tempWriter; 

		public HeaderTransportDriver(TransportDriver source)
		{
			tempWriter = new DataBufferWriter(0);
			Source = source;
		}

		public override TransportAddress TransportAddress => Source.TransportAddress;

		public override TransportConnection Accept()
		{
			return Source.Accept();
		}

		public override void Update()
		{
			Source.Update();
		}

		public override TransportEvent PopEvent()
		{
			return Source.PopEvent();
		}

		public override TransportConnection.State GetConnectionState(TransportConnection con)
		{
			return Source.GetConnectionState(con);
		}

		public unsafe Span<byte> WithHeader(Span<byte> original)
		{
			var def = default(DataBufferWriter);
			var curr = Header;
			if (Unsafe.AreSame(ref curr, ref def) || !curr.IsCreated)
				throw new InvalidOperationException("No header has been assigned!");
			
			tempWriter.Length = 0;
			tempWriter.WriteBuffer(Header);
			tempWriter.WriteSpan(original);
			return new Span<byte>((void*) tempWriter.GetSafePtr(), tempWriter.Length);
		}

		public override int Send(TransportChannel chan, TransportConnection con, Span<byte> data)
		{
			return Source.Send(chan, con, WithHeader(data));
		}

		public override int Broadcast(TransportChannel chan, Span<byte> data)
		{
			return Source.Broadcast(chan, WithHeader(data));
		}

		public override void Dispose()
		{
			Source.Dispose();
			tempWriter.Dispose();
		}
	}
}