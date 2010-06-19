using System;
using System.Threading;
using System.Collections.Generic;

namespace FSpot.Tasks
{
	public interface Task {
		void Start ();
		void Cancel ();
		void Execute ();
	}

	interface IChildrenHandling
	{
		void RemoveChild (Task task);
	}

	interface ISchedulable
	{
		void Schedule ();
		void Unschedule ();
	}

	public enum TaskState
	{
		Pending,
		Scheduled,
		Completed,
		Cancelled,
		Exception
	}

	public abstract class Task<T> : Task, ISchedulable, IChildrenHandling
	{
		public bool CancelWithChildren { get; set; }

		private Task Parent { get; set; }
		private List<Task> Children { get; set; }

		private T result;
		public T Result {
			get {
				Wait ();
				return result;
			}
		}

		private volatile TaskState state;
		public TaskState State {
			get { return state; }
			private set {
				//Hyena.Log.DebugFormat ("State Change: {0} : {1} => {2}", this, state, value);
				state = value;
			}
		}

		private EventWaitHandle WaitEvent { get; set; }

		public Task ()
		{
			CancelWithChildren = false;
			Children = new List<Task> ();
			WaitEvent = new ManualResetEvent (false);
			State = TaskState.Pending;
		}

		public void Start ()
		{
			if (Parent != null)
				throw new Exception ("You should not start child tasks yourself");
			(this as ISchedulable).Schedule ();
		}

		public void Wait ()
		{
			if (Parent == null)
				(this as ISchedulable).Schedule ();
			WaitEvent.WaitOne ();
		}

		public void ContinueWith (Task<T> task)
		{
			ContinueWith (task, true);
		}

		public void ContinueWith (Task<T> task, bool autostart)
		{
			lock (Children) {
				if (State == TaskState.Completed) {
					task.Start ();
				} else {
					task.Parent = this;
					Children.Add (task);
					if (autostart) {
						var to_start = Parent ?? this;
						to_start.Start ();
					}
				}
			}
		}

		void IChildrenHandling.RemoveChild (Task task)
		{
			lock (Children) {
				Children.Remove (task);
				if (Children.Count == 0 && CancelWithChildren)
					Cancel ();
			}
		}

		public void Cancel ()
		{
			State = TaskState.Cancelled;
			(this as ISchedulable).Unschedule ();

			if (Parent != null)
				(Parent as IChildrenHandling).RemoveChild (this);

			lock (Children) {
				foreach (var child in Children) {
					child.Cancel ();
				}
			}

			WaitEvent.Set ();
		}

		public void Execute ()
		{
			if (State != TaskState.Scheduled && State != TaskState.Cancelled)
				throw new Exception ("Can't start task manually!");

			if (State == TaskState.Cancelled)
				return;

			try {
				result = InnerExecute ();
				State = TaskState.Completed;

				foreach (var child in Children) {
					(child as ISchedulable).Schedule ();
				}
			} catch (Exception e) {
				State = TaskState.Exception;
				throw e;
			} finally {
				WaitEvent.Set ();
			}
		}

		protected abstract T InnerExecute ();

#region Scheduling

		void ISchedulable.Schedule ()
		{
			if (State == TaskState.Completed || State == TaskState.Scheduled)
				return;
			if (State != TaskState.Pending)
				throw new Exception ("Can only schedule pending tasks!");
			State = TaskState.Scheduled;
			InnerSchedule ();
		}

		void ISchedulable.Unschedule ()
		{
			if (State == TaskState.Scheduled)
				State = TaskState.Pending;
			InnerUnschedule ();
		}

		protected abstract void InnerSchedule ();
		protected abstract void InnerUnschedule ();

#endregion

	}
}
