#if ENABLE_TESTS
using NUnit.Framework;
using System;
using System.Threading;
using Hyena;
using FSpot;

namespace FSpot.Tasks.Tests
{
	[TestFixture]
	public class TaskTests
	{
		[SetUp]
		public void Initialize () {
			WorkerThreadTaskScheduler.Instance = null;
			Hyena.Log.Debugging = true;
		}

		[Test]
		public void TestWait () {
			var t = new SimpleTask<bool> (() => {
				Thread.Sleep (100);
				return true;
			});
			var start = DateTime.Now.Ticks;

			t.Start ();
			Assert.Less ((DateTime.Now.Ticks - start) / 10000, 101);
			Assert.AreEqual (TaskState.Scheduled, t.State);

			t.Wait ();
			Assert.Greater ((DateTime.Now.Ticks - start) / 10000, 99);
			Assert.AreEqual (TaskState.Completed, t.State);
		}

		[Test]
		public void TestContinue () {
			var t = new SimpleTask<bool> (() => {
				return true;
			});
			// Task is initially pending
			Assert.AreEqual (TaskState.Pending, t.State);

			var t2 = new SimpleTask<bool> (() => {
				// Parent task is completed before child starts
				Assert.AreEqual (TaskState.Completed, t.State);
				return true;
			});
			t.ContinueWith (t2);
			t2.Wait ();
		}

		[Test]
		public void TestContinueAfterComplete () {
			var t = new SimpleTask<bool> (() => {
				return true;
			});
			// Task is initially pending
			Assert.AreEqual (TaskState.Pending, t.State);

			// Make sure 't' is completed before adding continuation.
			t.Start ();
			t.Wait ();
			Assert.AreEqual (TaskState.Completed, t.State);

			var t2 = new SimpleTask<bool> (() => {
				// Parent task is completed before child starts
				Assert.AreEqual (TaskState.Completed, t.State);
				return true;
			});
			t.ContinueWith (t2);
			t2.Wait ();
			Assert.AreEqual (TaskState.Completed, t2.State);
		}

		[Test]
		public void TestContinueAfterCancel () {
			var t = new SimpleTask<bool> (() => {
				Thread.Sleep (100);
				return true;
			}, "t");
			// Task is initially pending
			Assert.AreEqual (TaskState.Pending, t.State);

			var t2 = new SimpleTask<bool> (() => {
				// Since t hasn't completed when t2 is cancelled, this should
				// not be called.
				throw new Exception ("Should not be ran after cancel!");
			}, "t2");
			t.ContinueWith (t2);
			t2.Cancel ();
			t2.Wait ();
			t.Wait ();
			// t completes because it was started.
			Assert.AreEqual (TaskState.Completed, t.State);
			Assert.AreEqual (TaskState.Cancelled, t2.State);
		}

		class SimpleTask<T> : WorkerThreadTask<T> {
			string label = null;

			public SimpleTask (TaskHandler h) : base (h) {
			}

			public SimpleTask (TaskHandler h, string label) : base (h) {
				this.label = label;
			}

			public override string ToString ()
			{
				return label ?? base.ToString ();
			}
		}
	}
}
#endif
