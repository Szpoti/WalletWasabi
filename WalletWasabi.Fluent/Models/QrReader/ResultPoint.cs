/*
* Copyright 2007 ZXing authors
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

using ZXing.Common.Detector;

namespace ZXing
{
	/// <summary>
	/// Encapsulates a point of interest in an image containing a barcode. Typically, this
	/// would be the location of a finder pattern or the corner of the barcode, for example.
	/// </summary>
	/// <author>Sean Owen</author>
	public class ResultPoint
	{
		private readonly float _x;
		private readonly float _y;
		private readonly byte[] _bytesX;
		private readonly byte[] _bytesY;
		private string _toString;

		/// <summary>
		/// Initializes a new instance of the <see cref="ResultPoint"/> class.
		/// </summary>
		public ResultPoint()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ResultPoint"/> class.
		/// </summary>
		/// <param name="x">The x.</param>
		/// <param name="y">The y.</param>
		public ResultPoint(float x, float y)
		{
			_x = x;
			_y = y;
			// calculate only once for GetHashCode
			_bytesX = BitConverter.GetBytes(x);
			_bytesY = BitConverter.GetBytes(y);
		}

		/// <summary>
		/// Gets the X.
		/// </summary>
		virtual public float X
		{
			get
			{
				return _x;
			}
		}

		/// <summary>
		/// Gets the Y.
		/// </summary>
		virtual public float Y
		{
			get
			{
				return _y;
			}
		}

		/// <summary>
		/// Determines whether the specified <see cref="object"/> is equal to this instance.
		/// </summary>
		/// <param name="other">The <see cref="object"/> to compare with this instance.</param>
		/// <returns>
		///   <c>true</c> if the specified <see cref="object"/> is equal to this instance; otherwise, <c>false</c>.
		/// </returns>
		public override bool Equals(object other)
		{
			ResultPoint otherPoint = (ResultPoint)other;
			return otherPoint == null ? false : _x == otherPoint._x && _y == otherPoint._y;
		}

		/// <summary>
		/// Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		/// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
		/// </returns>
		public override int GetHashCode()
		{
			return 31 * ((_bytesX[0] << 24) + (_bytesX[1] << 16) + (_bytesX[2] << 8) + _bytesX[3]) +
						 (_bytesY[0] << 24) + (_bytesY[1] << 16) + (_bytesY[2] << 8) + _bytesY[3];
		}

		/// <summary>
		/// Returns a <see cref="string"/> that represents this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="string"/> that represents this instance.
		/// </returns>
		public override string ToString()
		{
			if (_toString == null)
			{
				var result = new System.Text.StringBuilder(25);
				result.AppendFormat(System.Globalization.CultureInfo.CurrentUICulture, "({0}, {1})", _x, _y);
				_toString = result.ToString();
			}
			return _toString;
		}

		/// <summary>
		/// Orders an array of three ResultPoints in an order [A,B,C] such that AB is less than AC and
		/// BC is less than AC and the angle between BC and BA is less than 180 degrees.
		/// </summary>
		/// <param name="patterns">array of three <see cref="ResultPoint" /> to order</param>
		public static void OrderBestPatterns(ResultPoint[] patterns)
		{
			// Find distances between pattern centers
			float zeroOneDistance = Distance(patterns[0], patterns[1]);
			float oneTwoDistance = Distance(patterns[1], patterns[2]);
			float zeroTwoDistance = Distance(patterns[0], patterns[2]);

			ResultPoint pointA, pointB, pointC;
			// Assume one closest to other two is B; A and C will just be guesses at first
			if (oneTwoDistance >= zeroOneDistance && oneTwoDistance >= zeroTwoDistance)
			{
				pointB = patterns[0];
				pointA = patterns[1];
				pointC = patterns[2];
			}
			else if (zeroTwoDistance >= oneTwoDistance && zeroTwoDistance >= zeroOneDistance)
			{
				pointB = patterns[1];
				pointA = patterns[0];
				pointC = patterns[2];
			}
			else
			{
				pointB = patterns[2];
				pointA = patterns[0];
				pointC = patterns[1];
			}

			// Use cross product to figure out whether A and C are correct or flipped.
			// This asks whether BC x BA has a positive z component, which is the arrangement
			// we want for A, B, C. If it's negative, then we've got it flipped around and
			// should swap A and C.
			if (CrossProductZ(pointA, pointB, pointC) < 0.0f)
			{
				ResultPoint temp = pointA;
				pointA = pointC;
				pointC = temp;
			}

			patterns[0] = pointA;
			patterns[1] = pointB;
			patterns[2] = pointC;
		}

		/// <summary>
		/// calculates the distance between two points
		/// </summary>
		/// <param name="pattern1">first pattern</param>
		/// <param name="pattern2">second pattern</param>
		/// <returns>
		/// distance between two points
		/// </returns>
		public static float Distance(ResultPoint pattern1, ResultPoint pattern2)
		{
			return MathUtils.Distance(pattern1._x, pattern1._y, pattern2._x, pattern2._y);
		}

		/// <summary>
		/// Returns the z component of the cross product between vectors BC and BA.
		/// </summary>
		private static float CrossProductZ(ResultPoint pointA, ResultPoint pointB, ResultPoint pointC)
		{
			float bX = pointB._x;
			float bY = pointB._y;
			return ((pointC._x - bX) * (pointA._y - bY)) - ((pointC._y - bY) * (pointA._x - bX));
		}
	}
}
