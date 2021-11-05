/*
  Box2D.NetStandard Copyright © 2020 Ben Ukhanov & Hugh Phoenix-Hulme https://github.com/benzuk/box2d-netstandard
  Box2DX Copyright (c) 2009 Ihar Kalasouski http://code.google.com/p/box2dx
  
// MIT License

// Copyright (c) 2019 Erin Catto

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
*/


//#define DEBUG

using System;
using System.Buffers;

namespace Box2D.NetStandard.Collision
{
	/// <summary>
	///  Used to warm start Distance.
	///  Set count to zero on first call.
	/// </summary>
	public class SimplexCache : IDisposable
	{
		///< length or area
		internal ushort count;

		internal byte[] indexA;

		///< vertices on shape A
		internal byte[] indexB;

		///< vertices on shape B
		internal float metric;

		public SimplexCache()
		{
			indexA = ArrayPool<byte>.Shared.Rent(3);
			indexB = ArrayPool<byte>.Shared.Rent(3);
			
			indexA.AsSpan().Clear();
			indexB.AsSpan().Clear();
		}

		public void Dispose()
		{
			ArrayPool<byte>.Shared.Return(indexA);
			ArrayPool<byte>.Shared.Return(indexB);
		}
	}
}