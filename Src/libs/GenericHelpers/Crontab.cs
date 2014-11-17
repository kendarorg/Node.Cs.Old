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

namespace GenericHelpers
{
	public class Crontab
	{
		protected const int SECONDSPERMINUTE = 60;
		protected const int MINUTESPERHOUR = 60;
		protected const int HOURESPERDAY = 24;
		protected const int DAYSPERWEEK = 7;
		protected const int MONTHSPERYEAR = 12;
		protected const int DAYSPERMONTH = 31;
		private readonly String _configLine;
		private readonly bool _considerSeconds;
		private readonly int _mscron;
		private readonly List<DateRange> _ranges = new List<DateRange>();
		private readonly bool _realCrontab = true;
		private DateTime _baseline;

		/// <summary>
		/// Format string with spaces between the entries
		///  *     *     *    *     *     *    * command to be executed
		///  -     -     -    -     -     -    -
		///  |     |     |    |     |     |    +----- year
		///  |     |     |    |     |     +----- day of week (0 - 6) (Sunday=0)
		///  |     |     |    |     +------- month (1 - 12)
		///  |     |     |    +--------- day of month (1 - 31)
		///  |     |     +----------- hour (0 - 23)
		///  |     +------------- min (0 - 59)
		///  +------------- sec (0 - 59)
		/// </summary>
		public Crontab(String commandLine, bool considerSeconds = false)
		{
			_considerSeconds = considerSeconds;
			if (string.IsNullOrEmpty(commandLine))
			{
				throw new ArgumentNullException();
			}

			_configLine =
				commandLine.Replace("\t", " ").Replace("  ", " ").Replace("?", "*").Replace("*/", "0/").Trim().ToUpper();

			if (_configLine.StartsWith("@"))
			{
				_configLine = SpecialCommands(_configLine);
			}

			String[] pars = _configLine.Split(' ');
			if (pars.Length != 7)
			{
				throw new IndexOutOfRangeException();
			}

			for (int i = 0; i < pars.Length; i++)
			{
				pars[i] = pars[i].Trim();
				_ranges.Add(SetupElement(pars[i], i));
			}

			if (pars[1] == "*" && pars[0] == "*")
			{
				_ranges[0].Starred = false;
				_ranges[0].Periodicity = 1;
				_ranges[0].Values.Add(0, 0);
				_ranges[1].Starred = false;
				_ranges[1].Periodicity = 1;
				_ranges[1].Values.Add(0, 0);
			}
		}

		public string ConfigLine
		{
			get { return _configLine; }
		}


		public Crontab(DateTime baseLine, int milliseconds)
		{
			_baseline = baseLine;
			_mscron = milliseconds;
			_realCrontab = false;
		}

		private string ReplaceMonths(string p)
		{
			//TODON Month names
			return p;
		}

		private string ReplaceWeekdays(string p)
		{
			//TODON Weekdays names
			return p;
		}

		private DateRange CreateDateRange()
		{
			return new DateRange();
		}

		private string SpecialCommands(string configLine)
		{
			switch (configLine.ToLower())
			{
				case ("@yearly"):
				case ("@annually"):
					return "0 0 0 1 1 * *";
				case ("@monthly"):
					return "0 0 0 1 * * *";
				case ("@weekly"):
					return "0 0 0 * * 0 *";
				case ("@daily"):
					return "0 0 0 * * * *";
				case ("@hourly"):
					return "0 0 * * * * *";
				default:
					return configLine;
			}
		}

		private DateRange SetupElement(string p, int i)
		{
			var d = CreateDateRange();
			d.Position = (DateSection)i;
			if (d.Position == DateSection.Month)
			{
				p = ReplaceMonths(p);
			}
			else if (d.Position == DateSection.WeekDay)
			{
				p = ReplaceWeekdays(p);
			}

			if (p == "*")
			{
				d.Starred = true;
				return d;
			}
			string[] tmp;
			if (p.IndexOf("/", StringComparison.Ordinal) > 0)
			{
				tmp = p.Split('/');
				d.Periodicity = int.Parse(tmp[1]);
				p = tmp[0];
			}
			if (p.IndexOf("*", StringComparison.Ordinal) > 0)
			{
				d.Starred = true;
				return d;
			}
			tmp = p.Split(',');
			foreach (string s in tmp)
			{
				if (s.IndexOf("-", StringComparison.Ordinal) > 0)
				{
					string[] tmp2 = s.Split('-');
					int start = int.Parse(tmp2[0]);
					int end = int.Parse(tmp2[1]);
					if (!d.Values.ContainsKey(start))
					{
						d.Values.Add(start, end);
					}
				}
				else
				{
					int val = int.Parse(s);
					d.Values.Add(val, val);
				}
			}

			return d;
		}

		public bool MayRunAt(DateTime dt)
		{
			if (!_realCrontab)
			{
				Int64 msDelta = (dt.Ticks - _baseline.Ticks) / TimeSpan.TicksPerMillisecond;
				return (msDelta % _mscron == 0);
			}
			int allOk = 0;
			for (int i = 0; i < _ranges.Count; i++)
			{
				DateRange d = _ranges[i];
				switch (d.Position)
				{
					case (DateSection.Sec):
						if (_considerSeconds)
						{
							allOk += CheckValue(dt.Second, d);
						}
						else
						{
							allOk++;
						}
						break;
					case (DateSection.Year):
						allOk += CheckValue(dt.Year, d);
						break;
					case (DateSection.WeekDay):
						allOk += CheckValue((int)dt.DayOfWeek, d);
						break;
					case (DateSection.Month):
						allOk += CheckValue(dt.Month, d); ///////
						break;
					case (DateSection.MonthDay):
						allOk += CheckValue(dt.Day, d);
						break;
					case (DateSection.Hour):
						allOk += CheckValue(dt.Hour, d);
						break;
					case (DateSection.Min):
						allOk += CheckValue(dt.Minute, d);
						break;
				}
			}
			return allOk == 7;
		}

		private int CheckValue(int p, DateRange d)
		{
			// "*" everything is ok
			if (d.Starred && d.Values.Count == 0) return 1;
			if (d.Values.Count == 1 && d.Periodicity > 0)
			{
				if (d.Values.ContainsKey(0) && d.Values[0] == 0)
				{
					if (p % d.Periodicity == 0) return 1;
				}
			}

			// 1-2,3,4
			foreach (var e in d.Values)
			{
				if (e.Key == e.Value && e.Key == p)
				{
					// 3,4
					return 1;
				}

				if (e.Key <= p && p <= e.Value)
				{
					// 1-2
					if (d.Periodicity == -1) return 1;
					// 1-10/3
					if ((p - e.Key) % d.Periodicity == 0) return 1;
				}
			}
			return 0;
		}

		public DateTime Next(DateTime? srcTime = null)
		{
			DateTime dt = srcTime != null ? srcTime.Value : DateTime.Now;
			if (!_realCrontab)
			{
				Int64 msDelta = (dt.Ticks - _baseline.Ticks) / TimeSpan.TicksPerMillisecond;
				if (msDelta % _mscron == 0) return dt;
				dt = dt + TimeSpan.FromMilliseconds(_mscron - (msDelta % _mscron));
				dt = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, 0);
				return dt;
			}


			// ReSharper disable TooWideLocalVariableScope
			// ReSharper disable RedundantAssignment
			int[] vals;
			int prev = 0;
			int differenceFromNextNearestPeriod = 0;
			// ReSharper restore RedundantAssignment
			// ReSharper restore TooWideLocalVariableScope
			bool doRestart = true;

			while (doRestart)
			{
				vals = new[] { dt.Second, dt.Minute, dt.Hour, dt.Day, dt.Month, (int)dt.DayOfWeek, dt.Year };
				doRestart = false;
				//For each of the available ranges
				for (int i = (_ranges.Count - 1); i >= 0 && doRestart == false; i--)
				{
					DateRange d = _ranges[i];
					prev = vals[i];
					differenceFromNextNearestPeriod = 0;
					switch (d.Position)
					{
						case (DateSection.Year):
							{
								//Retrieve the nearest next year including itself
								int nextYear = GetNextNearestItem(vals[i], d, 2100);
								differenceFromNextNearestPeriod = nextYear - prev;
								if (differenceFromNextNearestPeriod > 0)
								{
									dt = dt.AddYears(differenceFromNextNearestPeriod);
									vals = new[] { 0, 0, 0, 1, 1, 0, dt.Year };
								}
							}
							break;
						case (DateSection.WeekDay):
							{
								int nextDay = GetNextNearestItem(vals[i], d, DAYSPERWEEK);

								differenceFromNextNearestPeriod = nextDay - prev;
								// 1 - 6 means that it's part of the same week so add the abs of days
								/*if (differenceFromNextNearestPeriod < 0)
									{
										dt = dt.AddDays(DAYSPERWEEK);
										vals = new[] { 0, 0, 0, dt.Day, dt.Month, (int)dt.DayOfWeek, dt.Year };
										i--;
									}
									else*/
								if (differenceFromNextNearestPeriod > 0)
								{
									dt += TimeSpan.FromDays(differenceFromNextNearestPeriod);
									vals = new[] { 0, 0, 0, dt.Day, dt.Month, (int)dt.DayOfWeek, dt.Year };
								}
							}
							break;
						case (DateSection.Month):
							{
								/*vals[i] = GetNextNearestItem(vals[i], d, MONTHSPERYEAR)+1;

									if (vals[i] > 12)
									{
										vals[6] += vals[i]/12;
										vals[i] = vals[i]%12 + 1;
										differenceFromNextNearestPeriod++;
									}
									dt = new DateTime(vals[6], vals[4] - 1, vals[3], vals[2], vals[1], vals[0]);
									if (differenceFromNextNearestPeriod > 0)
									{
										vals = new[] {0, 0, 0, 1, dt.Month + 1, (int) dt.DayOfWeek, dt.Year};
										dt = new DateTime(vals[6], vals[4] - 1, vals[3], vals[2], vals[1], vals[0]);
									}
									vals = new[] {dt.Second, dt.Minute, dt.Hour, dt.Day, dt.Month + 1, (int) dt.DayOfWeek, dt.Year};*/
								//1 based
								int nextMonth = GetNextNearestItem(vals[i], d, MONTHSPERYEAR);

								differenceFromNextNearestPeriod = nextMonth - prev;
								// 1 - 6 means that it's part of the same week so add the abs of days
								/*if (differenceFromNextNearestPeriod < 0)
									{
										dt = dt.AddYears(1);
										vals = new[] { 0, 0, 0, 1, dt.Month, (int)dt.DayOfWeek, dt.Year };
										i--;
									}
									else */
								if (differenceFromNextNearestPeriod > 0)
								{
									dt = dt.AddMonths(differenceFromNextNearestPeriod);
									vals = new[] { 0, 0, 0, 1, dt.Month, (int)dt.DayOfWeek, dt.Year };
								}
							}
							break;
						case (DateSection.MonthDay):
							{
								int nextMonthDay = GetNextNearestItem(vals[i], d, DAYSPERMONTH);
								differenceFromNextNearestPeriod = nextMonthDay - prev;
								/*if (differenceFromNextNearestPeriod > 0)
									{
										dt += TimeSpan.FromDays(differenceFromNextNearestPeriod);
										vals = new[] {0, 0, 0, dt.Day, dt.Month + 1, (int) dt.DayOfWeek, dt.Year};
										dt = new DateTime(vals[6], vals[4] - 1, vals[3], vals[2], vals[1], vals[0]);
									}*/
								//vals = new[] {dt.Second, dt.Minute, dt.Hour, dt.Day, dt.Month + 1, (int) dt.DayOfWeek, dt.Year};
								/*if (differenceFromNextNearestPeriod < 0)
									{
										dt = dt.AddMonths(1);
										//dt += TimeSpan.FromDays(Math.Abs(differenceFromNextNearestPeriod));
										vals = new[] { 0, 0, 0, dt.Day, dt.Month, (int)dt.DayOfWeek, dt.Year };
										i--;
									}
									else */
								if (differenceFromNextNearestPeriod > 0)
								{
									dt += TimeSpan.FromDays(differenceFromNextNearestPeriod);
									vals = new[] { 0, 0, 0, dt.Day, dt.Month, (int)dt.DayOfWeek, dt.Year };
								}
							}
							break;
						case (DateSection.Hour):
							{
								int nextHour = GetNextNearestItem(vals[i], d, HOURESPERDAY);
								differenceFromNextNearestPeriod = nextHour - prev;
								/*if (differenceFromNextNearestPeriod > 0)
									{
										dt += TimeSpan.FromHours(differenceFromNextNearestPeriod);
										vals = new[] {0, 0, dt.Hour, dt.Day, dt.Month + 1, (int) dt.DayOfWeek, dt.Year};
										dt = new DateTime(vals[6], vals[4] - 1, vals[3], vals[2], vals[1], vals[0]);
									}
									vals = new[] {dt.Second, dt.Minute, dt.Hour, dt.Day, dt.Month + 1, (int) dt.DayOfWeek, dt.Year};*/
								/*if (differenceFromNextNearestPeriod < 0)
									{
										dt = dt.AddDays(1);
										//dt += TimeSpan.FromDays(Math.Abs(differenceFromNextNearestPeriod));
										vals = new[] { 0, 0, dt.Hour, dt.Day, dt.Month, (int)dt.DayOfWeek, dt.Year };
										i--;
									}
									else */
								if (differenceFromNextNearestPeriod > 0)
								{
									dt = dt.AddHours(differenceFromNextNearestPeriod);
									vals = new[] { 0, 0, dt.Hour, dt.Day, dt.Month, (int)dt.DayOfWeek, dt.Year };
								}
							}
							break;
						case (DateSection.Min):
							{
								int nextMinute = GetNextNearestItem(vals[i], d, MINUTESPERHOUR);
								differenceFromNextNearestPeriod = nextMinute - prev;
								/*if (differenceFromNextNearestPeriod > 0)
									{
										dt += TimeSpan.FromMinutes(differenceFromNextNearestPeriod);
									}
									vals = new[] {0, dt.Minute, dt.Hour, dt.Day, dt.Month + 1, (int) dt.DayOfWeek, dt.Year};*/
								/*if (differenceFromNextNearestPeriod < 0)
									{
										dt = dt.AddHours(1);
										//dt += TimeSpan.FromDays(Math.Abs(differenceFromNextNearestPeriod));
										vals = new[] { 0, dt.Minute, dt.Hour, dt.Day, dt.Month, (int)dt.DayOfWeek, dt.Year };
										i--;
									}
									else*/
								if (differenceFromNextNearestPeriod > 0)
								{
									dt = dt.AddMinutes(differenceFromNextNearestPeriod);
									vals = new[] { 0, dt.Minute, dt.Hour, dt.Day, dt.Month, (int)dt.DayOfWeek, dt.Year };
								}
							}
							break;
						case (DateSection.Sec):
							if (_considerSeconds)
							{
								int nextSecond = GetNextNearestItem(vals[i], d, SECONDSPERMINUTE);
								differenceFromNextNearestPeriod = nextSecond - prev;
								if (differenceFromNextNearestPeriod > 0)
								{
									dt = dt.AddSeconds(differenceFromNextNearestPeriod);
									vals = new[] { dt.Second, dt.Minute, dt.Hour, dt.Day, dt.Month, (int)dt.DayOfWeek, dt.Year };
								}
							}
							break;
					}
					dt = new DateTime(vals[6], vals[4], vals[3], vals[2], vals[1], vals[0]);
					if (differenceFromNextNearestPeriod > 0) doRestart = true;
				}
			}

			return dt; // new DateTime(vals[6], vals[4] - 1, vals[3], vals[2], vals[1], vals[0], 0);
		}

		private int GetNextNearestItem(int p, DateRange d, int max)
		{
			while (p < max)
			{
				if (CheckValue(p, d) == 1) return p;
				p++;
			}
			p = 0;
			while (p < max)
			{
				if (CheckValue(p, d) == 1) return p + max;
				p++;
			}
			return 0;
		}

		private class DateRange
		{
			public readonly Dictionary<int, int> Values = new Dictionary<int, int>();
			public int Periodicity = -1;
			public DateSection Position;
			public bool Starred;

			public DateRange()
			{
				Starred = false;
			}
		}

		private enum DateSection
		{
			Sec = 0,
			Min,
			Hour,
			MonthDay,
			Month,
			WeekDay,
			Year
		};
	}
}
