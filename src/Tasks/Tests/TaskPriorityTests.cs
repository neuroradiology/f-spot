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
			WorkerThreadTaskScheduler.Instance = null;
			WorkerThreadTaskScheduler.Instance = new WorkerThreadTaskScheduler (false);
			Hyena.Log.Debugging = true;
		}

		[Test]
		public void TestDefaultPriority () {
			var task = new TestTask ();

			// Task is initially unscheduled
			Assert.AreEqual (TaskPriority.Normal, task.Priority);
			Assert.AreEqual (TaskState.Pending, task.State);
			Assert.AreEqual (new Task [] {}, WorkerThreadTaskScheduler.Instance.Tasks);

			// Sent to scheduler when started
			task.Start ();
			Assert.AreEqual (TaskPriority.Normal, task.Priority);
			Assert.AreEqual (TaskState.Scheduled, task.State);
			Assert.AreEqual (new Task [] { task }, WorkerThreadTaskScheduler.Instance.Tasks);
		}

		[Test]
		public void TestCancel () {
			var task = new TestTask ();

			// Task is initially unscheduled
			Assert.AreEqual (new Task [] {}, WorkerThreadTaskScheduler.Instance.Tasks);

			// Sent to scheduler when started
			task.Start ();
			Assert.AreEqual (TaskState.Scheduled, task.State);
			Assert.AreEqual (new Task [] { task }, WorkerThreadTaskScheduler.Instance.Tasks);

			// Removed from scheduler when cancelled
			task.Cancel ();
			Assert.AreEqual (TaskState.Cancelled, task.State);
			Assert.AreEqual (new Task [] { }, WorkerThreadTaskScheduler.Instance.Tasks);
		}

		[Test]
		public void TestOrdering () {
			var task1 = new TestTask ();
			var task2 = new TestTask () {
				Priority = TaskPriority.Interactive
			};

			// Initially unscheduled
			Assert.AreEqual (TaskPriority.Normal, task1.Priority);
			Assert.AreEqual (TaskState.Pending, task1.State);
			Assert.AreEqual (TaskPriority.Interactive, task2.Priority);
			Assert.AreEqual (TaskState.Pending, task2.State);
			Assert.AreEqual (new Task [] {}, WorkerThreadTaskScheduler.Instance.Tasks);

			// Sent to scheduler when started
			task1.Start ();
			Assert.AreEqual (TaskPriority.Normal, task1.Priority);
			Assert.AreEqual (TaskState.Scheduled, task1.State);
			Assert.AreEqual (TaskPriority.Interactive, task2.Priority);
			Assert.AreEqual (TaskState.Pending, task2.State);
			Assert.AreEqual (new Task [] { task1 }, WorkerThreadTaskScheduler.Instance.Tasks);

			// High priority task gets sent to the front of the queue
			task2.Start ();
			Assert.AreEqual (TaskPriority.Normal, task1.Priority);
			Assert.AreEqual (TaskState.Scheduled, task1.State);
			Assert.AreEqual (TaskPriority.Interactive, task2.Priority);
			Assert.AreEqual (TaskState.Scheduled, task2.State);
			Assert.AreEqual (task2, WorkerThreadTaskScheduler.Instance.heap.Peek ());
			Assert.AreEqual (new Task [] { task2, task1 }, WorkerThreadTaskScheduler.Instance.Tasks);
		}

		[Test]
		public void TestFIFOOrdering () {
			var task1 = new TestTask ();
			var task2 = new TestTask ();

			// Initially unscheduled
			Assert.AreEqual (TaskPriority.Normal, task1.Priority);
			Assert.AreEqual (TaskState.Pending, task1.State);
			Assert.AreEqual (TaskPriority.Normal, task2.Priority);
			Assert.AreEqual (TaskState.Pending, task2.State);
			Assert.AreEqual (new Task [] {}, WorkerThreadTaskScheduler.Instance.Tasks);

			// Sent to scheduler when started
			task1.Start ();
			Assert.AreEqual (TaskPriority.Normal, task1.Priority);
			Assert.AreEqual (TaskState.Scheduled, task1.State);
			Assert.AreEqual (TaskPriority.Normal, task2.Priority);
			Assert.AreEqual (TaskState.Pending, task2.State);
			Assert.AreEqual (new Task [] { task1 }, WorkerThreadTaskScheduler.Instance.Tasks);

			// Equal priority tasks get scheduled FIFO
			task2.Start ();
			Assert.AreEqual (TaskPriority.Normal, task1.Priority);
			Assert.AreEqual (TaskState.Scheduled, task1.State);
			Assert.AreEqual (TaskPriority.Normal, task2.Priority);
			Assert.AreEqual (TaskState.Scheduled, task2.State);
			Assert.AreEqual (task1, WorkerThreadTaskScheduler.Instance.heap.Peek ());
			Assert.AreEqual (new Task [] { task1, task2 }, WorkerThreadTaskScheduler.Instance.Tasks);
		}

		[Test]
		public void TestPriorityInheritance () {
			var task1 = new TestTask ();
			var task2 = new TestTask ();
			var task3 = new TestTask () {
				Priority = TaskPriority.Interactive
			};

			// Initially unscheduled
			Assert.AreEqual (new Task [] {}, WorkerThreadTaskScheduler.Instance.Tasks);

			// Send task1 to the scheduler
			task1.Start ();
			Assert.AreEqual (new Task [] { task1 }, WorkerThreadTaskScheduler.Instance.Tasks);

			// Start a continuation. Should cause task2 to be scheduled.
			// It should inherit the priority from task3 and go to the
			// front of the queue.
			Assert.AreEqual (TaskPriority.Normal, task2.Priority);
			task2.ContinueWith (task3);
			Assert.AreEqual (TaskPriority.Normal, task1.Priority);
			Assert.AreEqual (TaskPriority.Interactive, task2.Priority);
			Assert.AreEqual (new Task [] { task2, task1 }, WorkerThreadTaskScheduler.Instance.Tasks);
		}

		[Test]
		public void TestPriorityRevert () {
			var task1 = new TestTask ();
			var task2 = new TestTask ();
			var task3 = new TestTask () {
				Priority = TaskPriority.Interactive
			};

			// Initially unscheduled
			Assert.AreEqual (new Task [] {}, WorkerThreadTaskScheduler.Instance.Tasks);

			// Send task1 and task2 to the scheduler
			task1.Start ();
			task2.Start ();
			Assert.AreEqual (TaskPriority.Normal, task1.Priority);
			Assert.AreEqual (TaskPriority.Normal, task2.Priority);
			Assert.AreEqual (TaskState.Scheduled, task1.State);
			Assert.AreEqual (TaskState.Scheduled, task2.State);
			Assert.AreEqual (new Task [] { task1, task2 }, WorkerThreadTaskScheduler.Instance.Tasks);

			// Start a continuation. Should cause task2 to be rescheduled.
			// It should inherit the priority from task3 and go to the
			// front of the queue.
			task2.ContinueWith (task3);
			Assert.AreEqual (TaskPriority.Normal, task1.Priority);
			Assert.AreEqual (TaskPriority.Interactive, task2.Priority);
			Assert.AreEqual (TaskState.Scheduled, task1.State);
			Assert.AreEqual (TaskState.Scheduled, task2.State);
			Assert.AreEqual (new Task [] { task2, task1 }, WorkerThreadTaskScheduler.Instance.Tasks);

			// Priority should revert after cancelling the child.
			task3.Cancel ();
			Assert.AreEqual (TaskPriority.Normal, task1.Priority);
			Assert.AreEqual (TaskPriority.Normal, task2.Priority);
			Assert.AreEqual (TaskState.Scheduled, task1.State);
			Assert.AreEqual (TaskState.Scheduled, task2.State);
			Assert.AreEqual (new Task [] { task1, task2 }, WorkerThreadTaskScheduler.Instance.Tasks);
		}

		private class TestTask : Task<bool> {
			public TestTask () : base (() => true) {

			}
		}
	}
}
#endif
