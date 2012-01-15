// 
//  TestLogging.cs
//  
//  Author:
//       Christopher James Halse Rogers <raof@ubuntu.com>
// 
//  Copyright © 2011 Christopher James Halse Rogers <raof@ubuntu.com>
// 
//  This library is free software; you can redistribute it and/or modify
//  it under the terms of the GNU Lesser General Public License as
//  published by the Free Software Foundation; either version 2.1 of the
//  License, or (at your option) any later version.
// 
//  This library is distributed in the hope that it will be useful, but
//  WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//  Lesser General Public License for more details.
// 
//  You should have received a copy of the GNU Lesser General Public
//  License along with this library; if not, write to the Free Software
//  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;

using NUnit.Framework;

using Mono.Addins;
using Do.Platform;
using Do.Platform.Common;
using Do.Platform.ServiceStack;

namespace Do
{
	public class MockLogger : AbstractLogService
	{
		List<Tuple<LogLevel, string>> log;
		public bool recursiveLog = false;

		public override void Log (LogLevel level, string msg)
		{
			if (log != null) {
				log.Add (new Tuple<LogLevel, string> (level, msg));
				if (recursiveLog) {
					Platform.Log.Debug ("Recursive log: {0}", msg);
				}
			}
		}

		public void StartLog ()
		{
			log = new List<Tuple<LogLevel, string>> ();
		}

		public IEnumerable<Tuple<LogLevel, string>> EndLog ()
		{
			return Interlocked.Exchange (ref log, null);
		}
	}

	[TestFixture()]
	public class TestLogging
	{
		MockLogger logger;
		[SetUp()]
		public void SetUp ()
		{
			Gtk.Application.Init ();
			Gdk.Threads.Init ();
			Core.PluginManager.Initialize ();
			AddinManager.Registry.Update ();
			logger = Services.Logs.OfType<MockLogger> ().First ();
		}

		[Test()]
		public void TestSimpleDebugLogIsLogged ()
		{
			logger.StartLog ();
			Log.Debug ("This is a log message");
			var logs = logger.EndLog ();
			Assert.Contains (new Tuple<LogLevel, string> (LogLevel.Debug, "This is a log message"), logs.ToArray ());
		}

		[Test, Timeout (2000)]
		public void TestMultiThreadedLogging ()
		{
			var logMessages = new List<string> ();
			for (int i = 0; i < 100; i++) {
				logMessages.Add (string.Format ("Log message {0}", i));
			}

			logger.StartLog ();

			ManualResetEvent[] waitHandles = new ManualResetEvent[logMessages.Count];
			for (int i = 0; i < waitHandles.Length; ++i) {
				waitHandles [i] = new ManualResetEvent (false);
			}

			int threadCounter = -1;
			foreach (var message in logMessages) {
				var thread = new Thread (() => {
					int i = Interlocked.Increment (ref threadCounter);
					Log.Debug (logMessages [i]);
					waitHandles [i].Set ();
				});
				thread.Start ();
			}

			foreach (var handle in waitHandles)
				handle.WaitOne ();

			var logs = logger.EndLog ().ToArray ();
			foreach (var msg in logMessages) {
				Assert.Contains (new Tuple<LogLevel, string> (LogLevel.Debug, msg), logs);
			}
		}

		[Test]
		public void TestRecursiveLoggingThrowsException ()
		{
			string msg = "Hello";
			logger.StartLog ();
			logger.recursiveLog = true;
			Assert.Throws<InvalidOperationException> (delegate {
				Log.Debug (msg); });
			logger.recursiveLog = false;
		}
	}
}

