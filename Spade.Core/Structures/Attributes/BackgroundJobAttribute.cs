using Hangfire;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Spade.Core.Structures.Attributes
{
	public class BackgroundJobAttribute : Attribute
	{
		public readonly string Interval = Cron.Never();

		public BackgroundJobAttribute(string cronInterval)
		{
			Interval = cronInterval;
		}
	}
}
