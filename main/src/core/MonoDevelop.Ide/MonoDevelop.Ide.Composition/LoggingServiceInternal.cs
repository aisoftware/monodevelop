using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Utilities;

namespace MonoDevelop.Ide.Composition
{
	[Export (typeof (ILoggingServiceInternal))]
	class LoggingServiceInternal : ILoggingServiceInternal
	{
		public void AdjustCounter (string key, string name, int delta = 1)
		{
		}

		public void PostCounters ()
		{
		}

		public void PostEvent (string key, params object[] namesAndProperties)
		{
		}

		public void PostEvent (string key, IReadOnlyList<object> namesAndProperties)
		{
		}

		public void PostEvent (TelemetryEventType eventType, string eventName, TelemetryResult result = TelemetryResult.Success, params (string name, object property)[] namesAndProperties)
		{
		}

		public void PostEvent (TelemetryEventType eventType, string eventName, TelemetryResult result, IReadOnlyList<(string name, object property)> namesAndProperties)
		{
		}

		public void PostFault (string eventName, string description, Exception exceptionObject, string additionalErrorInfo, bool? isIncludedInWatsonSample)
		{
		}
	}
}