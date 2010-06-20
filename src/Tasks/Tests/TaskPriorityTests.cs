#if ENABLE_TESTS
using NUnit.Framework;
using System;
using System.Threading;
using System.Collections.Generic;
using Hyena;
using Hyena.Collections;
using FSpot;

namespace FSpot.Tasks.Tests
{
	[TestFixture]
	public class TaskPriorityTests
	{
		[SetUp]
		public void Initialize () {
			Hyena.Log.Debugging = true;
		}

		[Test]
		public void TestDefaultPriority () {
			var scheduler = new StaticScheduler ();
			var task = new StaticTask (scheduler);

			// Task is initially unscheduled
			Assert.AreEqual (TaskPriority.Normal, task.Priority);
			Assert.AreEqual (TaskState.Pending, task.State);
			Assert.AreEqual (new Task [] {}, scheduler.Tasks);

			// Sent to scheduler when started
			task.Start ();
			Assert.AreEqual (TaskPriority.Normal, task.Priority);
			Assert.AreEqual (TaskState.Scheduled, task.State);
			Assert.AreEqual (new Task [] { task }, scheduler.Tasks);
		}

		[Test]
		public void TestCancel () {
			var scheduler = new StaticScheduler ();
			var task = new StaticTask (scheduler);

			// Task is initially unscheduled
			Assert.AreEqual (new Task [] {}, scheduler.Tasks);

			// Sent to scheduler when started
			task.Start ();
			Assert.AreEqual (TaskState.Scheduled, task.State);
			Assert.AreEqual (new Task [] { task }, scheduler.Tasks);

			// Removed from scheduler when cancelled
			task.Cancel ();
			Assert.AreEqual (TaskState.Cancelled, task.State);
			Assert.AreEqual (new Task [] { }, scheduler.Tasks);
		}

		[Test]
		public void TestOrdering () {
			var scheduler = new StaticScheduler ();
			var task1 = new StaticTask (scheduler);
			var task2 = new StaticTask (scheduler) {
				Priority = TaskPriority.Interactive
			};

			// Initially unscheduled
			Assert.AreEqual (TaskPriority.Normal, task1.Priority);
			Assert.AreEqual (TaskState.Pending, task1.State);
			Assert.AreEqual (TaskPriority.Interactive, task2.Priority);
			Assert.AreEqual (TaskState.Pending, task2.State);
			Assert.AreEqual (new Task [] {}, scheduler.Tasks);

			// Sent to scheduler when started
			task1.Start ();
			Assert.AreEqual (TaskPriority.Normal, task1.Priority);
			Assert.AreEqual (TaskState.Scheduled, task1.State);
			Assert.AreEqual (TaskPriority.Interactive, task2.Priority);
			Assert.AreEqual (TaskState.Pending, task2.State);
			Assert.AreEqual (new Task [] { task1 }, scheduler.Tasks);

			// High priority task gets sent to the front of the queue
			task2.Start ();
			Assert.AreEqual (TaskPriority.Normal, task1.Priority);
			Assert.AreEqual (TaskState.Scheduled, task1.State);
			Assert.AreEqual (TaskPriority.Interactive, task2.Priority);
			Assert.AreEqual (TaskState.Scheduled, task2.State);
			Assert.AreEqual (task2, scheduler.heap.Peek ());
			Assert.AreEqual (new Task [] { task2, task1 }, scheduler.Tasks);
		}

		[Test]
		public void TestFIFOOrdering () {
			var scheduler = new StaticScheduler ();
			var task1 = new StaticTask (scheduler);
			var task2 = new StaticTask (scheduler);

			// Initially unscheduled
			Assert.AreEqual (TaskPriority.Normal, task1.Priority);
			Assert.AreEqual (TaskState.Pending, task1.State);
			Assert.AreEqual (TaskPriority.Normal, task2.Priority);
			Assert.AreEqual (TaskState.Pending, task2.State);
			Assert.AreEqual (new Task [] {}, scheduler.Tasks);

			// Sent to scheduler when started
			task1.Start ();
			Assert.AreEqual (TaskPriority.Normal, task1.Priority);
			Assert.AreEqual (TaskState.Scheduled, task1.State);
			Assert.AreEqual (TaskPriority.Normal, task2.Priority);
			Assert.AreEqual (TaskState.Pending, task2.State);
			Assert.AreEqual (new Task [] { task1 }, scheduler.Tasks);

			// Equal priority tasks get scheduled FIFO
			task2.Start ();
			Assert.AreEqual (TaskPriority.Normal, task1.Priority);
			Assert.AreEqual (TaskState.Scheduled, task1.State);
			Assert.AreEqual (TaskPriority.Normal, task2.Priority);
			Assert.AreEqual (TaskState.Scheduled, task2.State);
			Assert.AreEqual (task1, scheduler.heap.Peek ());
			Assert.AreEqual (new Task [] { task1, task2 }, scheduler.Tasks);
		}

		[Test]
		public void TestPriorityInheritance () {
			var scheduler = new StaticScheduler ();
			var task1 = new StaticTask (scheduler);
			var task2 = new StaticTask (scheduler);
			var task3 = new StaticTask (scheduler) {
				Priority = TaskPriority.Interactive
			};

			// Initially unscheduled
			Assert.AreEqual (new Task [] {}, scheduler.Tasks);

			// Send task1 to the scheduler
			task1.Start ();
			Assert.AreEqual (new Task [] { task1 }, scheduler.Tasks);

			// Start a continuation. Should cause task2 to be scheduled.
			// It should inherit the priority from task3 and go to the
			// front of the queue.
			Assert.AreEqual (TaskPriority.Normal, task2.Priority);
			task2.ContinueWith (task3);
			Assert.AreEqual (TaskPriority.Normal, task1.Priority);
			Assert.AreEqual (TaskPriority.Interactive, task2.Priority);
			Assert.AreEqual (new Task [] { task2, task1 }, scheduler.Tasks);
		}

		[Test]
		public void TestPriorityRevert () {
			var scheduler = new StaticScheduler ();
			var task1 = new StaticTask (scheduler);
			var task2 = new StaticTask (scheduler);
			var task3 = new StaticTask (scheduler) {
				Priority = TaskPriority.Interactive
			};

			// Initially unscheduled
			Assert.AreEqual (new Task [] {}, scheduler.Tasks);

			// Send task1 and task2 to the scheduler
			task1.Start ();
			task2.Start ();
			Assert.AreEqual (TaskPriority.Normal, task1.Priority);
			Assert.AreEqual (TaskPriority.Normal, task2.Priority);
			Assert.AreEqual (TaskState.Scheduled, task1.State);
			Assert.AreEqual (TaskState.Scheduled, task2.State);
			Assert.AreEqual (new Task [] { task1, task2 }, scheduler.Tasks);

			// Start a continuation. Should cause task2 to be rescheduled.
			// It should inherit the priority from task3 and go to the
			// front of the queue.
			task2.ContinueWith (task3);
			Assert.AreEqual (TaskPriority.Normal, task1.Priority);
			Assert.AreEqual (TaskPriority.Interactive, task2.Priority);
			Assert.AreEqual (TaskState.Scheduled, task1.State);
			Assert.AreEqual (TaskState.Scheduled, task2.State);
			Assert.AreEqual (new Task [] { task2, task1 }, scheduler.Tasks);

			// Priority should revert after cancelling the child.
			task3.Cancel ();
			Assert.AreEqual (TaskPriority.Normal, task1.Priority);
			Assert.AreEqual (TaskPriority.Normal, task2.Priority);
			Assert.AreEqual (TaskState.Scheduled, task1.State);
			Assert.AreEqual (TaskState.Scheduled, task2.State);
			Assert.AreEqual (new Task [] { task1, task2 }, scheduler.Tasks);
		}
	}

	class StaticScheduler
	{
		internal IntervalHeap<Task> heap = new IntervalHeap<Task> ();

		public Task[] Tasks {
			get {
				List<Task> tasks = new List<Task> ();
				foreach (var item in heap) {
					tasks.Add (item);
				}
				return tasks.ToArray ();
			}
		}
	}

	class StaticTask : Task<bool>
	{
		public StaticScheduler Scheduler { get; set; }

		internal StaticTask (StaticScheduler scheduler)
		{
			Scheduler = scheduler;
		}

		protected override void InnerSchedule ()
		{
			lock (Scheduler.heap) {
				Scheduler.heap.Push (this, (int) Priority);
			}
		}

		protected override void InnerUnschedule ()
		{
			lock (Scheduler.heap) {
				Scheduler.heap.Remove (this);
			}
		}

		protected override void InnerReschedule ()
		{
			lock (Scheduler.heap) {
				Scheduler.heap.Remove (this);
				Scheduler.heap.Push (this, (int) Priority);
			}
		}


		protected override bool InnerExecute () {
			throw new Exception ("Not supported for this task");
		}

		public override string ToString () {
			return String.Format ("StaticTask (Priority: {0}, State: {1})", Priority, State);
		}
	}
}
#endif
