using Hangfire;
using Spade.Core.Structures.Attributes;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Spade.Core.Services
{
	public class ServiceBase
	{
		public ServiceBase()
		{
			LinkBackgroundJobs();
		}

		private void LinkBackgroundJobs()
		{
			foreach (MethodInfo method in this.GetType().GetMethods())
			{
				BackgroundJobAttribute attribute = (BackgroundJobAttribute)method.GetCustomAttribute(typeof(BackgroundJobAttribute));
				if (attribute is null)
					continue;

				if (attribute.Interval != Cron.Never())
				{
					RecurringJob.AddOrUpdate(() => method.Invoke(this, new object[] { }), attribute.Interval);
					Console.WriteLine("Attached {0}:{1} as a recurring job.", method.DeclaringType.Name, method.Name);
				}
				else
				{
					BackgroundJob.Enqueue(() => method.Invoke(this, new object[] { }));
					Console.WriteLine("Attached {0}:{1} as a background job.", method.DeclaringType.Name, method.Name);
				}
			}
		}
	}
}
