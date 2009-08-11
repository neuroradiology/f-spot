//
// Fspot.Jobs.CleanHiddenVersionsJob.cs
//
// Author(s)
//	Ruben Vermeersch  <ruben@savanne.be>
//
// This is free software. See COPYING for details
//

using Banshee.Kernel;
using FSpot.Utils;
using System;

namespace FSpot.Jobs {
	public class CleanHiddenVersionsJob : Job
	{
		public CleanHiddenVersionsJob (uint id, string job_options, int run_at, JobPriority job_priority, bool persistent) : this (id, job_options, DbUtils.DateTimeFromUnixTime (run_at), job_priority, persistent)
		{
		}

		public CleanHiddenVersionsJob (uint id, string job_options, DateTime run_at, JobPriority job_priority, bool persistent) : base (id, job_options, job_priority, run_at, persistent)
		{
		}

		protected override bool Execute ()
		{
			Core.Database.Photos.CleanHiddenVersions ();
			return true;
		}
	}
}
