// ===========================================================
// Copyright (C) 2014-2015 Kendar.org
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation 
// files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, 
// modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software 
// is furnished to do so, subject to the following conditions:
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES 
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS 
// BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF 
// OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// ===========================================================


using System;
using System.Collections.Generic;

namespace GenericHelpers.Series
{
	public class MovingAvgBuilder : ISeriesBuilder
	{
		private ISeriesBuilder _seriesToCalculateOn;
		private List<Single> _newValues;
		private int _steps;

		public bool IsContinuous { get { return true; } }

		public MovingAvgBuilder(ISeriesBuilder seriesToCalculateOn, int steps)
		{
			if (!seriesToCalculateOn.IsContinuous) throw new InvalidOperationException("MovingAvg can be calculate only on continuos series!");
			_seriesToCalculateOn = seriesToCalculateOn;
			_steps = steps;
			_newValues = new List<float>();


		}

		public void ElaborateTimeSeries(List<TemporalValue> temporalValues)
		{
			var startPoint = -1;
			for (int i = 0; i < temporalValues.Count; i++)
			{
				if (temporalValues[i].IsSet)
				{
					startPoint = i;
					break;
				}
			}
			if (startPoint == -1) throw new InvalidOperationException("No data set!");
			_seriesToCalculateOn.ElaborateTimeSeries(temporalValues);
			for (int i = startPoint; i < temporalValues.Count; i++)
			{
				if (i < (startPoint + _steps))
				{
					_newValues.Add(0);
				}
				else
				{
					_newValues.Add(CalculatePrevAvg(i, temporalValues));
				}
			}
			for (int i = 0; i < temporalValues.Count; i++)
			{
				if (i < (startPoint + _steps))
				{
					temporalValues[i].Value = 0;
					temporalValues[i].IsSet = false;
				}
				else
				{
					temporalValues[i].Value = _newValues[i];
				}
			}
		}

		private float CalculatePrevAvg(int upTo, List<TemporalValue> temporalValues)
		{
			float result = 0.0f;
			for (int i = upTo - _steps; i < upTo; i++)
			{
				result += temporalValues[i].Value;
			}
			if (result == 0) return 0;
			return result / _steps;
		}
	}

}
