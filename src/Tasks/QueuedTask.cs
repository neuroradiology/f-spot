using Hyena;
using System;
using System.Threading;
using System.Collections.Generic;

namespace FSpot.Tasks
{
	class QueueTaskScheduler {
		static object sync_root = new object ();

		EventWaitHandle wait = new AutoResetEvent (false);

		static QueueTaskScheduler instance;
		internal static QueueTaskScheduler Instance {
			get {
				lock (sync_root) {
					if (instance == null) {
						instance = new QueueTaskScheduler ();
					}
				}
				return instance;
			}
			set {
				if (value != null)
					throw new Exception ("Can only set to null to reset");
				if (instance != null)
					instance.Finish ();
				instance = null;
			}
		}

		List<Task> queue = new List<Task> ();
		Thread worker;
		volatile bool should_halt = false;

		public QueueTaskScheduler () {
			worker = ThreadAssist.Spawn (SchedulerWorker);
		}

		public void Finish ()
		{
			should_halt = true;
			wait.Set ();
			while (!worker.Join (100))
				wait.Set ();
		}

		internal void Schedule (Task task)
		{
			lock (queue) {
				queue.Add (task);
				wait.Set ();
			}
		}

		internal void Unschedule (Task task)
		{
			lock (queue) {
				queue.Remove (task);
			}
		}

		void SchedulerWorker ()
		{
			while (!should_halt) {
				Task task = null;
				lock (queue) {
					if (queue.Count > 0)
						task = queue [0];
				}

				if (task == null && !should_halt) {
					wait.WaitOne ();
					continue;
				}

				lock (queue) {
					queue.RemoveAt (0);
				}

				task.Execute ();
			}
		}
	}

	public class QueuedTask<T> : Task<T>
	{
		public delegate T TaskHandler ();

		TaskHandler handler;

		public QueuedTask (TaskHandler h) {
			this.handler = h;
		}

		protected override void InnerSchedule ()
		{
			QueueTaskScheduler.Instance.Schedule (this);
		}

		protected override void InnerUnschedule ()
		{
			QueueTaskScheduler.Instance.Unschedule (this);
		}

		protected override T InnerExecute () {
			return handler ();
		}
	}
}
