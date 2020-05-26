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
		public TimeSpan Interval;

		[SetUp]
		public void Setup()
		{
			Interval = TimeSpan.FromSeconds(0.5);
		}

		[Test]
		public void TestValidBeats()
		{
			var start = TimeSpan.FromSeconds(1);

			Assert.IsTrue(new Beat {Target = 0, Offset = -0.5f}.IsStartValid(start, TimeSpan.FromSeconds(1), Interval));
			Assert.IsTrue(new Beat {Target = 0, Offset = +0.5f}.IsStartValid(start, TimeSpan.FromSeconds(1), Interval));
			Assert.IsFalse(new Beat {Target = 0}.IsStartValid(start, TimeSpan.FromSeconds(1.5), Interval));

			Assert.IsTrue(new Beat {Target = 0, Offset = 0, SliderLength = 1}.IsSliderValid(start, TimeSpan.FromSeconds(1.75), Interval));

			Assert.IsFalse(new Beat {Target = 0, Offset = -0.5f}.IsStartValid(start, TimeSpan.FromSeconds(1.25), Interval));
			Assert.IsTrue(new Beat {Target = 0, Offset = 0}.IsStartValid(start, TimeSpan.FromSeconds(1.25), Interval));
			Assert.IsTrue(new Beat {Target = 0, Offset = +0.5f}.IsStartValid(start, TimeSpan.FromSeconds(1.25), Interval));
		}

		[Test]
		public void TestIsSimpleCommandSameAsPressures()
		{
			var cmd = new RhythmCommandDefinition("march", new[]
			{
				new RhythmCommandAction(0, RhythmKeys.Pata),
				new RhythmCommandAction(1, RhythmKeys.Pata),
				new RhythmCommandAction(2, RhythmKeys.Pata),
				new RhythmCommandAction(3, RhythmKeys.Pon),
			});

			var pressures = new[]
			{
				new FlowPressure(RhythmKeys.Pata, TimeSpan.FromSeconds(2), Interval),
				new FlowPressure(RhythmKeys.Pata, TimeSpan.FromSeconds(2.5), Interval),
				new FlowPressure(RhythmKeys.Pata, TimeSpan.FromSeconds(3), Interval),
				new FlowPressure(RhythmKeys.Pon, TimeSpan.FromSeconds(3.5), Interval),
			};

			Assert.IsTrue(RhythmCommandUtility.SameAsSequence(cmd.Actions, pressures, Interval));
		}

		[Test]
		public void TestIsSimpleCommandNotSameAsPressures_Cause_Of_WrongKey()
		{
			var cmd = new RhythmCommandDefinition("march", new[]
			{
				new RhythmCommandAction(0, RhythmKeys.Pata),
				new RhythmCommandAction(1, RhythmKeys.Pata),
				new RhythmCommandAction(2, RhythmKeys.Pata),
				new RhythmCommandAction(3, RhythmKeys.Pon),
			});

			var pressures = new[]
			{
				new FlowPressure(RhythmKeys.Pata, TimeSpan.FromSeconds(2), Interval),
				new FlowPressure(RhythmKeys.Chaka, TimeSpan.FromSeconds(2.5), Interval),
				new FlowPressure(RhythmKeys.Pata, TimeSpan.FromSeconds(3), Interval),
				new FlowPressure(RhythmKeys.Pon, TimeSpan.FromSeconds(3.5), Interval),
			};

			Assert.IsFalse(RhythmCommandUtility.SameAsSequence(cmd.Actions, pressures, Interval));
		}

		[Test]
		public void TestIsSimpleCommandNotSameAsPressures_Cause_Of_WrongTime()
		{
			var cmd = new RhythmCommandDefinition("march", new[]
			{
				new RhythmCommandAction(0, RhythmKeys.Pata),
				new RhythmCommandAction(1, RhythmKeys.Pata),
				new RhythmCommandAction(2, RhythmKeys.Pata),
				new RhythmCommandAction(3, RhythmKeys.Pon),
			});

			var pressures = new[]
			{
				new FlowPressure(RhythmKeys.Pata, TimeSpan.FromSeconds(2), Interval),
				new FlowPressure(RhythmKeys.Pata, TimeSpan.FromSeconds(2.5), Interval),
				new FlowPressure(RhythmKeys.Pata, TimeSpan.FromSeconds(3), Interval),
				new FlowPressure(RhythmKeys.Pon, TimeSpan.FromSeconds(4), Interval),
			};

			Assert.IsFalse(RhythmCommandUtility.SameAsSequence(cmd.Actions, pressures, Interval));
		}

		[Test]
		public void TestSliderIsSameAsPressures()
		{
			var cmd = new RhythmCommandDefinition("slidertest", new[]
			{
				new RhythmCommandAction(new Beat {Target = 0, SliderLength = 2}, RhythmKeys.Chaka),
			});

			var pressures = new[]
			{
				new FlowPressure(RhythmKeys.Chaka, TimeSpan.FromSeconds(0), Interval),
				new FlowPressure(RhythmKeys.Chaka, TimeSpan.FromSeconds(1), Interval) {IsSliderEnd = true},
			};

			Assert.IsTrue(RhythmCommandUtility.SameAsSequence(cmd.Actions, pressures, Interval));
		}

		[Test]
		public void TestSliderIsFailedWithIncorrectLastPressureTime()
		{
			var cmd = new RhythmCommandDefinition("slidertest", new[]
			{
				new RhythmCommandAction(new Beat {Target = 0, SliderLength = 2}, RhythmKeys.Chaka),
			});

			var pressures = new[]
			{
				new FlowPressure(RhythmKeys.Chaka, TimeSpan.FromSeconds(0), Interval),
				new FlowPressure(RhythmKeys.Chaka, TimeSpan.FromSeconds(1.5), Interval) {IsSliderEnd = true},
			};

			Assert.IsFalse(RhythmCommandUtility.SameAsSequence(cmd.Actions, pressures, Interval));
		}
		
		[Test]
		public void TestSliderIsFailedWithNoSliderEnd()
		{
			var cmd = new RhythmCommandDefinition("slidertest", new[]
			{
				RhythmCommandAction.With(0, RhythmKeys.Chaka),
				RhythmCommandAction.WithSlider(1, 1, RhythmKeys.Pon), 
			});

			var pressures = new[]
			{
				new FlowPressure(RhythmKeys.Chaka, TimeSpan.FromSeconds(0), Interval),
				new FlowPressure(RhythmKeys.Pon, TimeSpan.FromSeconds(0.5), Interval),
			};

			Assert.IsFalse(RhythmCommandUtility.SameAsSequence(cmd.Actions, pressures, Interval));
		}

		[Test]
		public void TestCanPredictSimple()
		{
			var cmd = new RhythmCommandDefinition("march", new[]
			{
				new RhythmCommandAction(0, RhythmKeys.Pata),
				new RhythmCommandAction(1, RhythmKeys.Pata),
				new RhythmCommandAction(2, RhythmKeys.Pata),
				new RhythmCommandAction(3, RhythmKeys.Pon),
			});

			var Interval = TimeSpan.FromSeconds(0.5);

			var pressures = new[]
			{
				new FlowPressure(RhythmKeys.Pata, TimeSpan.FromSeconds(0), Interval),
				new FlowPressure(RhythmKeys.Pata, TimeSpan.FromSeconds(0.5), Interval)
			};

			Assert.IsTrue(RhythmCommandUtility.CanBePredicted(cmd.Actions, pressures, Interval));
		}

		[Test]
		public void TestFailPredictSimpleIfInvalidKey()
		{
			var cmd = new RhythmCommandDefinition("march", new[]
			{
				new RhythmCommandAction(0, RhythmKeys.Pata),
				new RhythmCommandAction(1, RhythmKeys.Pata),
				new RhythmCommandAction(2, RhythmKeys.Pata),
				new RhythmCommandAction(3, RhythmKeys.Pon),
			});

			var pressures = new[]
			{
				new FlowPressure(RhythmKeys.Pata, TimeSpan.FromSeconds(0), Interval),
				new FlowPressure(RhythmKeys.Pon, TimeSpan.FromSeconds(0.5), Interval)
			};

			Assert.IsFalse(RhythmCommandUtility.CanBePredicted(cmd.Actions, pressures, Interval));
		}

		[Test]
		public void TestFailPredictSimpleIfInvalidTime()
		{
			var cmd = new RhythmCommandDefinition("march", new[]
			{
				new RhythmCommandAction(0, RhythmKeys.Pata),
				new RhythmCommandAction(1, RhythmKeys.Pata),
				new RhythmCommandAction(2, RhythmKeys.Pata),
				new RhythmCommandAction(3, RhythmKeys.Pon),
			});

			var pressures = new[]
			{
				new FlowPressure(RhythmKeys.Pata, TimeSpan.FromSeconds(0), Interval),
				new FlowPressure(RhythmKeys.Pata, TimeSpan.FromSeconds(1), Interval)
			};

			Assert.IsFalse(RhythmCommandUtility.CanBePredicted(cmd.Actions, pressures, Interval));
		}
	}
}