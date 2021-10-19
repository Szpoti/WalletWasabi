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
using System.Collections.Generic;

using ZXing.Common;

namespace ZXing.QrCode.Internal
{
	/// <summary> <p>This class attempts to find alignment patterns in a QR Code. Alignment patterns look like finder
	/// patterns but are smaller and appear at regular intervals throughout the image.</p>
	///
	/// <p>At the moment this only looks for the bottom-right alignment pattern.</p>
	///
	/// <p>This is mostly a simplified copy of {@link FinderPatternFinder}. It is copied,
	/// pasted and stripped down here for maximum performance but does unfortunately duplicate
	/// some code.</p>
	///
	/// <p>This class is thread-safe but not reentrant. Each thread must allocate its own object.</p>
	///
	/// </summary>
	/// <author>  Sean Owen
	/// </author>
	/// <author>www.Redivivus.in (suraj.supekar@redivivus.in) - Ported from ZXING Java Source
	/// </author>
	internal sealed class AlignmentPatternFinder
	{
		private readonly BitMatrix _image;
		private readonly IList<AlignmentPattern> _possibleCenters;
		private readonly int _startX;
		private readonly int _startY;
		private readonly int _width;
		private readonly int _height;
		private readonly float _moduleSize;
		private readonly int[] _crossCheckStateCount;
		private readonly ResultPointCallback _resultPointCallback;

		/// <summary> <p>Creates a finder that will look in a portion of the whole image.</p>
		///
		/// </summary>
		/// <param name="image">image to search
		/// </param>
		/// <param name="startX">left column from which to start searching
		/// </param>
		/// <param name="startY">top row from which to start searching
		/// </param>
		/// <param name="width">width of region to search
		/// </param>
		/// <param name="height">height of region to search
		/// </param>
		/// <param name="moduleSize">estimated module size so far
		/// </param>
		/// <param name="resultPointCallback">callback function which is called, when a result point is found</param>
		internal AlignmentPatternFinder(BitMatrix image, int startX, int startY, int width, int height, float moduleSize, ResultPointCallback resultPointCallback)
		{
			_image = image;
			_possibleCenters = new List<AlignmentPattern>(5);
			_startX = startX;
			_startY = startY;
			_width = width;
			_height = height;
			_moduleSize = moduleSize;
			_crossCheckStateCount = new int[3];
			_resultPointCallback = resultPointCallback;
		}

		/// <summary> <p>This method attempts to find the bottom-right alignment pattern in the image. It is a bit messy since
		/// it's pretty performance-critical and so is written to be fast foremost.</p>
		///
		/// </summary>
		/// <returns><see cref="AlignmentPattern"/> if found</returns>
		internal AlignmentPattern Find()
		{
			int startX = _startX;
			int height = _height;
			int maxJ = startX + _width;
			int middleI = _startY + (height >> 1);
			// We are looking for black/white/black modules in 1:1:1 ratio;
			// this tracks the number of black/white/black modules seen so far
			int[] stateCount = new int[3];
			for (int iGen = 0; iGen < height; iGen++)
			{
				// Search from middle outwards
				int i = middleI + ((iGen & 0x01) == 0 ? ((iGen + 1) >> 1) : -((iGen + 1) >> 1));
				stateCount[0] = 0;
				stateCount[1] = 0;
				stateCount[2] = 0;
				int j = startX;
				// Burn off leading white pixels before anything else; if we start in the middle of
				// a white run, it doesn't make sense to count its length, since we don't know if the
				// white run continued to the left of the start point
				while (j < maxJ && !_image[j, i])
				{
					j++;
				}
				int currentState = 0;
				while (j < maxJ)
				{
					if (_image[j, i])
					{
						// Black pixel
						if (currentState == 1)
						{
							// Counting black pixels
							stateCount[1]++;
						}
						else
						{
							// Counting white pixels
							if (currentState == 2)
							{
								// A winner?
								if (FoundPatternCross(stateCount))
								{
									// Yes
									AlignmentPattern confirmed = HandlePossibleCenter(stateCount, i, j);
									if (confirmed != null)
									{
										return confirmed;
									}
								}
								stateCount[0] = stateCount[2];
								stateCount[1] = 1;
								stateCount[2] = 0;
								currentState = 1;
							}
							else
							{
								stateCount[++currentState]++;
							}
						}
					}
					else
					{
						// White pixel
						if (currentState == 1)
						{
							// Counting black pixels
							currentState++;
						}
						stateCount[currentState]++;
					}
					j++;
				}
				if (FoundPatternCross(stateCount))
				{
					AlignmentPattern confirmed = HandlePossibleCenter(stateCount, i, maxJ);
					if (confirmed != null)
					{
						return confirmed;
					}
				}
			}

			// Hmm, nothing we saw was observed and confirmed twice. If we had
			// any guess at all, return it.
			if (_possibleCenters.Count != 0)
			{
				return _possibleCenters[0];
			}

			return null;
		}

		/// <summary> Given a count of black/white/black pixels just seen and an end position,
		/// figures the location of the center of this black/white/black run.
		/// </summary>
		private static float? CenterFromEnd(int[] stateCount, int end)
		{
			var result = (end - stateCount[2]) - stateCount[1] / 2.0f;
			return float.IsNaN(result) ? null : result;
		}

		/// <param name="stateCount">count of black/white/black pixels just read
		/// </param>
		/// <returns> true iff the proportions of the counts is close enough to the 1/1/1 ratios
		/// used by alignment patterns to be considered a match
		/// </returns>
		private bool FoundPatternCross(int[] stateCount)
		{
			float maxVariance = _moduleSize / 2.0f;
			for (int i = 0; i < 3; i++)
			{
				if (Math.Abs(_moduleSize - stateCount[i]) >= maxVariance)
				{
					return false;
				}
			}
			return true;
		}

		/// <summary>
		///   <p>After a horizontal scan finds a potential alignment pattern, this method
		/// "cross-checks" by scanning down vertically through the center of the possible
		/// alignment pattern to see if the same proportion is detected.</p>
		/// </summary>
		/// <param name="startI">row where an alignment pattern was detected</param>
		/// <param name="centerJ">center of the section that appears to cross an alignment pattern</param>
		/// <param name="maxCount">maximum reasonable number of modules that should be
		/// observed in any reading state, based on the results of the horizontal scan</param>
		/// <param name="originalStateCountTotal">The original state count total.</param>
		/// <returns>
		/// vertical center of alignment pattern, or null if not found
		/// </returns>
		private float? CrossCheckVertical(int startI, int centerJ, int maxCount, int originalStateCountTotal)
		{
			int maxI = _image.Height;
			int[] stateCount = _crossCheckStateCount;
			stateCount[0] = 0;
			stateCount[1] = 0;
			stateCount[2] = 0;

			// Start counting up from center
			int i = startI;
			while (i >= 0 && _image[centerJ, i] && stateCount[1] <= maxCount)
			{
				stateCount[1]++;
				i--;
			}
			// If already too many modules in this state or ran off the edge:
			if (i < 0 || stateCount[1] > maxCount)
			{
				return null;
			}
			while (i >= 0 && !_image[centerJ, i] && stateCount[0] <= maxCount)
			{
				stateCount[0]++;
				i--;
			}
			if (stateCount[0] > maxCount)
			{
				return null;
			}

			// Now also count down from center
			i = startI + 1;
			while (i < maxI && _image[centerJ, i] && stateCount[1] <= maxCount)
			{
				stateCount[1]++;
				i++;
			}
			if (i == maxI || stateCount[1] > maxCount)
			{
				return null;
			}
			while (i < maxI && !_image[centerJ, i] && stateCount[2] <= maxCount)
			{
				stateCount[2]++;
				i++;
			}
			if (stateCount[2] > maxCount)
			{
				return null;
			}

			int stateCountTotal = stateCount[0] + stateCount[1] + stateCount[2];
			if (5 * Math.Abs(stateCountTotal - originalStateCountTotal) >= 2 * originalStateCountTotal)
			{
				return null;
			}

			return FoundPatternCross(stateCount) ? CenterFromEnd(stateCount, i) : null;
		}

		/// <summary> <p>This is called when a horizontal scan finds a possible alignment pattern. It will
		/// cross check with a vertical scan, and if successful, will see if this pattern had been
		/// found on a previous horizontal scan. If so, we consider it confirmed and conclude we have
		/// found the alignment pattern.</p>
		///
		/// </summary>
		/// <param name="stateCount">reading state module counts from horizontal scan
		/// </param>
		/// <param name="i">row where alignment pattern may be found
		/// </param>
		/// <param name="j">end of possible alignment pattern in row
		/// </param>
		/// <returns> {@link AlignmentPattern} if we have found the same pattern twice, or null if not
		/// </returns>
		private AlignmentPattern HandlePossibleCenter(int[] stateCount, int i, int j)
		{
			int stateCountTotal = stateCount[0] + stateCount[1] + stateCount[2];
			float? centerJ = CenterFromEnd(stateCount, j);
			if (centerJ == null)
			{
				return null;
			}

			float? centerI = CrossCheckVertical(i, (int)centerJ, 2 * stateCount[1], stateCountTotal);
			if (centerI != null)
			{
				float estimatedModuleSize = (stateCount[0] + stateCount[1] + stateCount[2]) / 3.0f;
				foreach (var center in _possibleCenters)
				{
					// Look for about the same center and module size:
					if (center.AboutEquals(estimatedModuleSize, centerI.Value, centerJ.Value))
					{
						return center.CombineEstimate(centerI.Value, centerJ.Value, estimatedModuleSize);
					}
				}
				// Hadn't found this before; save it
				var point = new AlignmentPattern(centerJ.Value, centerI.Value, estimatedModuleSize);
				_possibleCenters.Add(point);
				if (_resultPointCallback != null)
				{
					_resultPointCallback(point);
				}
			}
			return null;
		}
	}
}
