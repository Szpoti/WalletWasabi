/*
* Copyright 2009 ZXing authors
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
*      http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using System;
using ZXing.Common;

namespace ZXing
{
	/// <summary>
	/// This class is the core bitmap class used by ZXing to represent 1 bit data. Reader objects
	/// accept a BinaryBitmap and attempt to decode it.
	/// </summary>
	/// <author>dswitkin@google.com (Daniel Switkin)</author>
	public sealed class BinaryBitmap
	{
		private readonly Binarizer _binarizer;
		private BitMatrix _matrix;

		/// <summary>
		///
		/// </summary>
		/// <param name="binarizer"></param>
		public BinaryBitmap(Binarizer binarizer)
		{
			if (binarizer == null)
			{
				throw new ArgumentException("Binarizer must be non-null.");
			}
			_binarizer = binarizer;
		}

		internal BinaryBitmap(BitMatrix matrix)
		{
			if (matrix == null)
			{
				throw new ArgumentException("matrix must be non-null.");
			}
			_matrix = matrix;
		}

		/// <returns>
		/// The width of the bitmap.
		/// </returns>
		public int Width
		{
			get
			{
				return _binarizer.Width;
			}
		}

		/// <returns>
		/// The height of the bitmap.
		/// </returns>
		public int Height
		{
			get
			{
				return _binarizer.Height;
			}
		}

		/// <summary>
		/// Converts one row of luminance data to 1 bit data. May actually do the conversion, or return
		/// cached data. Callers should assume this method is expensive and call it as seldom as possible.
		/// This method is intended for decoding 1D barcodes and may choose to apply sharpening.
		/// </summary>
		/// <param name="y">The row to fetch, which must be in [0, bitmap height).</param>
		/// <param name="row">An optional preallocated array. If null or too small, it will be ignored.
		/// If used, the Binarizer will call BitArray.clear(). Always use the returned object.
		/// </param>
		/// <returns> The array of bits for this row (true means black).</returns>
		public BitArray GetBlackRow(int y, BitArray row)
		{
			return _binarizer.GetBlackRow(y, row);
		}

		/// <summary>
		/// Converts a 2D array of luminance data to 1 bit. As above, assume this method is expensive
		/// and do not call it repeatedly. This method is intended for decoding 2D barcodes and may or
		/// may not apply sharpening. Therefore, a row from this matrix may not be identical to one
		/// fetched using getBlackRow(), so don't mix and match between them.
		/// </summary>
		/// <returns> The 2D array of bits for the image (true means black).</returns>
		public BitMatrix BlackMatrix
		{
			get
			{
				// The matrix is created on demand the first time it is requested, then cached. There are two
				// reasons for this:
				// 1. This work will never be done if the caller only installs 1D Reader objects, or if a
				//    1D Reader finds a barcode before the 2D Readers run.
				// 2. This work will only be done once even if the caller installs multiple 2D Readers.
				return _matrix ??= _binarizer.BlackMatrix;
			}
		}

		/// <returns>
		/// Whether this bitmap can be cropped.
		/// </returns>
		public bool CropSupported
		{
			get
			{
				return _binarizer.LuminanceSource.CropSupported;
			}
		}

		/// <summary>
		/// Returns a new object with cropped image data. Implementations may keep a reference to the
		/// original data rather than a copy. Only callable if isCropSupported() is true.
		/// </summary>
		/// <param name="left">The left coordinate, which must be in [0, Width)</param>
		/// <param name="top">The top coordinate, which must be in [0, Height)</param>
		/// <param name="width">The width of the rectangle to crop.</param>
		/// <param name="height">The height of the rectangle to crop.</param>
		/// <returns> A cropped version of this object.</returns>
		public BinaryBitmap Crop(int left, int top, int width, int height)
		{
			var newSource = _binarizer.LuminanceSource.Crop(left, top, width, height);
			return new BinaryBitmap(_binarizer.CreateBinarizer(newSource));
		}

		/// <returns>
		/// Whether this bitmap supports counter-clockwise rotation.
		/// </returns>
		public bool RotateSupported
		{
			get
			{
				return _binarizer.LuminanceSource.RotateSupported;
			}
		}

		/// <summary>
		/// Returns a new object with rotated image data by 90 degrees counterclockwise.
		/// Only callable if <see cref="RotateSupported"/> is true.
		/// </summary>
		/// <returns>A rotated version of this object.</returns>
		public BinaryBitmap RotateCounterClockwise()
		{
			var newSource = _binarizer.LuminanceSource.RotateCounterClockwise();
			return new BinaryBitmap(_binarizer.CreateBinarizer(newSource));
		}

		/// <summary>
		/// Returns a new object with rotated image data by 45 degrees counterclockwise.
		/// Only callable if <see cref="RotateSupported"/> is true.
		/// </summary>
		/// <returns>A rotated version of this object.</returns>
		public BinaryBitmap RotateCounterClockwise45()
		{
			LuminanceSource newSource = _binarizer.LuminanceSource.RotateCounterClockwise45();
			return new BinaryBitmap(_binarizer.CreateBinarizer(newSource));
		}

		/// <summary>
		/// Returns a <see cref="string"/> that represents this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="string"/> that represents this instance.
		/// </returns>
		public override string ToString()
		{
			var blackMatrix = BlackMatrix;
			return blackMatrix != null ? blackMatrix.ToString() : string.Empty;
		}
	}
}
