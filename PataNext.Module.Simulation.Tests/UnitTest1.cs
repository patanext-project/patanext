using System;
using System.Diagnostics;
using DefaultEcs;
using NUnit.Framework;
using PataNext.Module.RhythmEngine;
using PataNext.Module.RhythmEngine.Data;

namespace PataNext.Module.Simulation.Tests
{
	public class Tests
	{
		[SetUp]
		public void Setup()
		{
		}

		[Test]
		public void TestValidBeats()
		{
			var interval = TimeSpan.FromSeconds(0.5);
			var start = TimeSpan.FromSeconds(1);

			Assert.IsTrue(new Beat {Target = 0, Offset = -0.5f}.IsStartValid(start, TimeSpan.FromSeconds(1), interval));
			Assert.IsTrue(new Beat {Target = 0, Offset = +0.5f}.IsStartValid(start, TimeSpan.FromSeconds(1), interval));
			Assert.IsFalse(new Beat {Target = 0}.IsStartValid(start, TimeSpan.FromSeconds(1.5), interval));

			Assert.IsTrue(new Beat {Target = 0, Offset = 0, SliderLength = 1}.IsSliderValid(start, TimeSpan.FromSeconds(1.75), interval));

			Assert.IsFalse(new Beat {Target = 0, Offset = -0.5f}.IsStartValid(start, TimeSpan.FromSeconds(1.25), interval));
			Assert.IsTrue(new Beat {Target = 0, Offset = 0}.IsStartValid(start, TimeSpan.FromSeconds(1.25), interval));
			Assert.IsTrue(new Beat {Target = 0, Offset = +0.5f}.IsStartValid(start, TimeSpan.FromSeconds(1.25), interval));
		}

		[Test]
		public void TestIsSimpleCommandSameAsPressures()
		{
			var interval = TimeSpan.FromSeconds(0.5);

			var cmd = new RhythmCommandDefinition("march", new[]
			{
				new RhythmCommandAction(0, RhythmKeys.Pata),
				new RhythmCommandAction(1, RhythmKeys.Pata),
				new RhythmCommandAction(2, RhythmKeys.Pata),
				new RhythmCommandAction(3, RhythmKeys.Pon),
			});

			var pressures = new[]
			{
				new FlowPressure(RhythmKeys.Pata, TimeSpan.FromSeconds(2), interval),
				new FlowPressure(RhythmKeys.Pata, TimeSpan.FromSeconds(2.5), interval),
				new FlowPressure(RhythmKeys.Pata, TimeSpan.FromSeconds(3), interval),
				new FlowPressure(RhythmKeys.Pon, TimeSpan.FromSeconds(3.5), interval),
			};

			Assert.IsTrue(RhythmCommandUtility.SameAsSequence(cmd.Actions, pressures, interval));
		}
		
		[Test]
		public void TestIsSimpleCommandNotSameAsPressures_Cause_Of_WrongKey()
		{
			var interval = TimeSpan.FromSeconds(0.5);

			var cmd = new RhythmCommandDefinition("march", new[]
			{
				new RhythmCommandAction(0, RhythmKeys.Pata),
				new RhythmCommandAction(1, RhythmKeys.Pata),
				new RhythmCommandAction(2, RhythmKeys.Pata),
				new RhythmCommandAction(3, RhythmKeys.Pon),
			});

			var pressures = new[]
			{
				new FlowPressure(RhythmKeys.Pata, TimeSpan.FromSeconds(2), interval),
				new FlowPressure(RhythmKeys.Chaka, TimeSpan.FromSeconds(2.5), interval),
				new FlowPressure(RhythmKeys.Pata, TimeSpan.FromSeconds(3), interval),
				new FlowPressure(RhythmKeys.Pon, TimeSpan.FromSeconds(3.5), interval),
			};

			Assert.IsFalse(RhythmCommandUtility.SameAsSequence(cmd.Actions, pressures, interval));
		}

		[Test]
		public void TestIsSimpleCommandNotSameAsPressures_Cause_Of_WrongTime()
		{
			var interval = TimeSpan.FromSeconds(0.5);

			var cmd = new RhythmCommandDefinition("march", new[]
			{
				new RhythmCommandAction(0, RhythmKeys.Pata),
				new RhythmCommandAction(1, RhythmKeys.Pata),
				new RhythmCommandAction(2, RhythmKeys.Pata),
				new RhythmCommandAction(3, RhythmKeys.Pon),
			});

			var pressures = new[]
			{
				new FlowPressure(RhythmKeys.Pata, TimeSpan.FromSeconds(2), interval),
				new FlowPressure(RhythmKeys.Pata, TimeSpan.FromSeconds(2.5), interval),
				new FlowPressure(RhythmKeys.Pata, TimeSpan.FromSeconds(3), interval),
				new FlowPressure(RhythmKeys.Pon, TimeSpan.FromSeconds(4), interval),
			};

			Assert.IsFalse(RhythmCommandUtility.SameAsSequence(cmd.Actions, pressures, interval));
		}

		[Test]
		public void TestSliderIsSameAsPressures()
		{
			var interval = TimeSpan.FromSeconds(0.5);

			var cmd = new RhythmCommandDefinition("slidertest", new[]
			{
				new RhythmCommandAction(new Beat {Target = 0, SliderLength = 2}, RhythmKeys.Chaka),
			});

			var pressures = new[]
			{
				new FlowPressure(RhythmKeys.Chaka, TimeSpan.FromSeconds(0), interval),
				new FlowPressure(RhythmKeys.Chaka, TimeSpan.FromSeconds(1), interval) {IsSliderEnd = true},
			};

			Assert.IsTrue(RhythmCommandUtility.SameAsSequence(cmd.Actions, pressures, interval));
		}
		
		[Test]
		public void TestSliderIsFailedWithIncorrectLastPressureTime()
		{
			var interval = TimeSpan.FromSeconds(0.5);

			var cmd = new RhythmCommandDefinition("slidertest", new[]
			{
				new RhythmCommandAction(new Beat {Target = 0, SliderLength = 2}, RhythmKeys.Chaka),
			});

			var pressures = new[]
			{
				new FlowPressure(RhythmKeys.Chaka, TimeSpan.FromSeconds(0), interval),
				new FlowPressure(RhythmKeys.Chaka, TimeSpan.FromSeconds(1.5), interval) {IsSliderEnd = true},
			};

			Assert.IsFalse(RhythmCommandUtility.SameAsSequence(cmd.Actions, pressures, interval));
		}

		[Test]
		public void TestCanPredictSimple()
		{
			var cmd = new RhythmCommandDefinition("march", new []
			{
				new RhythmCommandAction(0, RhythmKeys.Pata),
				new RhythmCommandAction(1, RhythmKeys.Pata),
				new RhythmCommandAction(2, RhythmKeys.Pata),
				new RhythmCommandAction(3, RhythmKeys.Pon),
			});
			
			var interval = TimeSpan.FromSeconds(0.5);

			var pressures = new[]
			{
				new FlowPressure(RhythmKeys.Pata, TimeSpan.FromSeconds(0), interval),
				new FlowPressure(RhythmKeys.Pata, TimeSpan.FromSeconds(0.5), interval)
			};

			Assert.IsTrue(RhythmCommandUtility.CanBePredicted(cmd.Actions, pressures, interval));
		}
		
		[Test]
		public void TestFailPredictSimpleIfInvalidKey()
		{
			var cmd = new RhythmCommandDefinition("march", new []
			{
				new RhythmCommandAction(0, RhythmKeys.Pata),
				new RhythmCommandAction(1, RhythmKeys.Pata),
				new RhythmCommandAction(2, RhythmKeys.Pata),
				new RhythmCommandAction(3, RhythmKeys.Pon),
			});
			
			var interval = TimeSpan.FromSeconds(0.5);

			var pressures = new[]
			{
				new FlowPressure(RhythmKeys.Pata, TimeSpan.FromSeconds(0), interval),
				new FlowPressure(RhythmKeys.Pon, TimeSpan.FromSeconds(0.5), interval)
			};

			Assert.IsFalse(RhythmCommandUtility.CanBePredicted(cmd.Actions, pressures, interval));
		}
		
		[Test]
		public void TestFailPredictSimpleIfInvalidTime()
		{
			var cmd = new RhythmCommandDefinition("march", new []
			{
				new RhythmCommandAction(0, RhythmKeys.Pata),
				new RhythmCommandAction(1, RhythmKeys.Pata),
				new RhythmCommandAction(2, RhythmKeys.Pata),
				new RhythmCommandAction(3, RhythmKeys.Pon),
			});
			
			var interval = TimeSpan.FromSeconds(0.5);

			var pressures = new[]
			{
				new FlowPressure(RhythmKeys.Pata, TimeSpan.FromSeconds(0), interval),
				new FlowPressure(RhythmKeys.Pata, TimeSpan.FromSeconds(1), interval)
			};

			Assert.IsFalse(RhythmCommandUtility.CanBePredicted(cmd.Actions, pressures, interval));
		}
	}
}