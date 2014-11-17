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

	public class TemporalSeries
	{
		private TimeSpan _span;
		private readonly List<TemporalValue> _temporalValues;
		private DateTime _from;
		private DateTime _to;
		private ISeriesBuilder _seriesBuilder;

		public List<TemporalValue> Values
		{
			get
			{
				return _temporalValues;
			}
		}

		public TemporalSeries(TimeSpan span, DateTime from, DateTime to, ISeriesBuilder seriesBuilder = null)
		{
			_span = span;
			_from = from;
			_to = to;
			_seriesBuilder = seriesBuilder;
			_temporalValues = new List<TemporalValue>();
			SetupTimeSeries();
		}

		private void SetupTimeSeries()
		{
			var start = _from;
			while (start < _to)
			{
				_temporalValues.Add(new TemporalValue { Current = start });
				start += _span;
			}
		}

		public void AddValues<T>(IEnumerable<T> values, Func<T, int> retrieveValue, Func<T, DateTime> retrieveDate)
		{
			foreach (var item in values)
			{
				AddValue(retrieveDate(item), retrieveValue(item));
			}
		}

		public void AddValue(DateTime sampleTimestamp, int sampleValue)
		{
			if (sampleTimestamp < _from || sampleTimestamp > _to) return;
			var relativePosition = sampleTimestamp - _from;
			var step = (int)Math.Floor((double)(relativePosition.TotalMilliseconds / _span.TotalMilliseconds));
			if (step > _temporalValues.Count) return;
			var current = _temporalValues[step];
			current.Value += sampleValue;
			current.IsSet = true;
		}

		public void DoFinalize()
		{
			if (_seriesBuilder != null)
			{
				_seriesBuilder.ElaborateTimeSeries(_temporalValues);
			}
		}
	}
}
