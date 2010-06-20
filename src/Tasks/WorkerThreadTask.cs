using Hyena;
using Hyena.Collections;
using System;
using System.Threading;
using System.Collections.Generic;

namespace FSpot.Tasks
{
	/// <summary>
	///    Simple task scheduler that executes all tasks on one worker thread.
	/// </summary>
	internal class WorkerThreadTaskScheduler {
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

		internal void Schedule (Task task)
		{
			lock (heap) {
				heap.Push (task, (int) task.Priority);
				wait.Set ();
			}
		}

		internal void Unschedule (Task task)
		{
			lock (heap) {
				heap.Remove (task);
			}
		}

		internal void Reschedule (Task task)
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
			// Not starting the worker means that the scheduler won't work,
			// but this can be useful for unit tests.
			if (start_worker) {
				worker = ThreadAssist.Spawn (SchedulerWorker);
			}
		}

		EventWaitHandle wait = new AutoResetEvent (false);
		Thread worker;
		volatile bool should_halt = false;

		void SchedulerWorker ()
		{
			while (!should_halt) {
				Task task = null;
				lock (heap) {
					if (heap.Count > 0)
						task = heap.Pop ();
				}

				if (task == null && !should_halt) {
					wait.WaitOne ();
					continue;
				}

				task.Execute ();
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

#region Task

	public class WorkerThreadTask<T> : Task<T>
	{
		public delegate T TaskHandler ();

		TaskHandler handler;

		public WorkerThreadTask (TaskHandler h) {
			this.handler = h;
		}

		protected override void InnerSchedule ()
		{
			WorkerThreadTaskScheduler.Instance.Schedule (this);
		}

		protected override void InnerUnschedule ()
		{
			WorkerThreadTaskScheduler.Instance.Unschedule (this);
		}

		protected override void InnerReschedule ()
		{
			WorkerThreadTaskScheduler.Instance.Reschedule (this);
		}

		protected override T InnerExecute () {
			return handler ();
		}
	}

#endregion

}
