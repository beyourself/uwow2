//
// ARC4.cs: Alleged RC4(tm) compatible symmetric stream cipher
//	RC4 is a trademark of RSA Security
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace Mono.Security.Cryptography {
	// References:
	// a.	Usenet 1994 - RC4 Algorithm revealed
	//	http://www.qrst.de/html/dsds/rc4.htm

	public class ARC4 : RC4, ICryptoTransform {
		private byte[] key;
		private bool disposed;
		private byte[] state;
		private byte x;
		private byte y;

		public ARC4() {
			state = new byte[256];
			disposed = false;
		}

		public override byte[] Key {
			get { return (byte[])key.Clone(); }
			set {
				key = (byte[])value.Clone();
				KeySetup(key);
			}
		}

		#region ICryptoTransform Members

		public bool CanReuseTransform {
			get { return false; }
		}

		public bool CanTransformMultipleBlocks {
			get { return true; }
		}

		public int InputBlockSize {
			get { return 1; }
		}

		public int OutputBlockSize {
			get { return 1; }
		}

		public int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset) {
			CheckInput(inputBuffer, inputOffset, inputCount);
			// check output parameters
			if(outputBuffer == null) {
				throw new ArgumentNullException("outputBuffer");
			}
			if(outputOffset < 0) {
				throw new ArgumentOutOfRangeException("outputOffset", "< 0");
			}
			// ordered to avoid possible integer overflow
			if(outputOffset > outputBuffer.Length - inputCount) {
				throw new ArgumentException("Overflow", "inputBuffer");
			}

			return InternalTransformBlock(inputBuffer, inputOffset, inputCount, outputBuffer, outputOffset);
		}

		public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount) {
			CheckInput(inputBuffer, inputOffset, inputCount);

			var output = new byte[inputCount];
			InternalTransformBlock(inputBuffer, inputOffset, inputCount, output, 0);
			return output;
		}

		#endregion

		~ARC4() {
			Dispose(true);
		}

		protected override void Dispose(bool disposing) {
			if(disposed) {
				return;
			}
			x = 0;
			y = 0;
			if(key != null) {
				Array.Clear(key, 0, key.Length);
				key = null;
			}
			Array.Clear(state, 0, state.Length);
			state = null;
			GC.SuppressFinalize(this);
			disposed = true;
		}

		public override ICryptoTransform CreateEncryptor(byte[] rgbKey, byte[] rgvIV) {
			Key = rgbKey;
			return this;
		}

		public override ICryptoTransform CreateDecryptor(byte[] rgbKey, byte[] rgvIV) {
			Key = rgbKey;
			return CreateEncryptor();
		}

		public override void GenerateIV() {
			// not used for a stream cipher
			IV = new byte[0];
		}

		public override void GenerateKey() {
			throw new NotImplementedException();
		}

		private void KeySetup(byte[] key) {
			byte index1 = 0;
			byte index2 = 0;

			for(int counter = 0; counter < 256; counter++) {
				state[counter] = (byte)counter;
			}
			x = 0;
			y = 0;
			for(int counter = 0; counter < 256; counter++) {
				index2 = (byte)(key[index1] + state[counter] + index2);
				// swap byte
				byte tmp = state[counter];
				state[counter] = state[index2];
				state[index2] = tmp;
				index1 = (byte)((index1 + 1) % key.Length);
			}
		}

		private static void CheckInput(ICollection<byte> inputBuffer, int inputOffset, int inputCount) {
			if(inputBuffer == null) {
				throw new ArgumentNullException("inputBuffer");
			}
			if(inputOffset < 0) {
				throw new ArgumentOutOfRangeException("inputOffset", "< 0");
			}
			if(inputCount < 0) {
				throw new ArgumentOutOfRangeException("inputCount", "< 0");
			}
			// ordered to avoid possible integer overflow
			if(inputOffset > inputBuffer.Count - inputCount) {
				throw new ArgumentException("Overflow", "inputBuffer");
			}
		}

		private int InternalTransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer,
		                                   int outputOffset) {
			for(int counter = 0; counter < inputCount; counter ++) {
				x = (byte)(x + 1);
				y = (byte)(state[x] + y);
				// swap byte
				byte tmp = state[x];
				state[x] = state[y];
				state[y] = tmp;

				var xorIndex = (byte)(state[x] + state[y]);
				outputBuffer[outputOffset + counter] = (byte)(inputBuffer[inputOffset + counter] ^ state[xorIndex]);
			}
			return inputCount;
		}
	}
}