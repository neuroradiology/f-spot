using Hyena;
using Hyena.Collections;
using System;
using System.Threading;
using System.Collections.Generic;

namespace FSpot.Tasks
{
	/// <summary>
	///    Simple task scheduler that pumps the threadpool.
	/// </summary>
	internal class WorkerThreadTaskScheduler : IScheduler {
		static object sync_root = new object ();

#region Singleton Pattern

		static WorkerThreadTaskScheduler instance;
		internal static WorkerThreadTaskScheduler Instance {
			get {
				lock (sync_root) {
					if (instance == null) {
						instance = new WorkerThreadTaskScheduler ();
					}
				}
				return instance;
			}
			set {
				if (value == null && instance != null) {
					instance.Finish ();
					instance = null;
				}

				if (instance != null)
					throw new Exception ("Need to finish the current instance first!");
				instance = value;
			}
		}

#endregion

#region Queue Management

		internal IntervalHeap<Task> heap = new IntervalHeap<Task> ();

		public void Schedule (Task task)
		{
			lock (heap) {
				heap.Push (task, (int) task.Priority);
				wait.Set ();
			}
		}

		public void Unschedule (Task task)
		{
			lock (heap) {
				heap.Remove (task);
			}
		}

		public void Reschedule (Task task)
		{
			lock (heap) {
				heap.Remove (task);
				heap.Push (task, (int) task.Priority);
				wait.Set ();
			}
		}

		internal Task[] Tasks {
			get {
				List<Task> tasks = new List<Task> ();
				foreach (var item in heap) {
					tasks.Add (item);
				}
				return tasks.ToArray ();
			}
		}

#endregion

#region Worker Thread

		internal WorkerThreadTaskScheduler () : this (true) {
		}

		internal WorkerThreadTaskScheduler (bool start_worker)
		{
			max_tasks = Environment.ProcessorCount * 2;
			Log.DebugFormat ("Doing at most {0} tasks", max_tasks);

			// Not starting the worker means that the scheduler won't work,
			// but this can be useful for unit tests.
			if (start_worker) {
				worker = ThreadAssist.Spawn (SchedulerWorker);
			}
		}

		EventWaitHandle wait = new AutoResetEvent (false);
		Thread worker;

		volatile bool should_halt = false;
		int max_tasks = 0;
		volatile int tasks_queued = 0;

		void SchedulerWorker ()
		{
			while (!should_halt) {
				Task task = null;
				lock (heap) {
					if (heap.Count > 0)
						task = heap.Pop ();
				}

				if ((tasks_queued >= max_tasks || task == null) && !should_halt) {
					wait.WaitOne ();
					if (task == null)
						continue;
				}

				Interlocked.Increment (ref tasks_queued);
				ThreadPool.QueueUserWorkItem ((o) => {
					task.Execute ();
					Log.DebugFormat ("Finished task {0}", task);
					Interlocked.Decrement (ref tasks_queued);
					wait.Set ();
				});
			}
		}

		public void Finish ()
		{
			should_halt = true;
			wait.Set ();
			while (worker != null && !worker.Join (100))
				wait.Set ();
		}

#endregion

	}
}
