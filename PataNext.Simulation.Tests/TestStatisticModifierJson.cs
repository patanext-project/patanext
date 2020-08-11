using System.Collections.Generic;
using NUnit.Framework;
using PataNext.Game.Abilities;

namespace PataNext.Module.Simulation.Tests
{
	public class TestStatisticModifierJson
	{
		[Test]
		public void Test1()
		{
			var map = new Dictionary<string, StatisticModifier>();
			StatisticModifierJson.FromMap(ref map, @"
{
    ""modifiers"": 
			{
				""perfect"": {
					""movement_speed"": 1.2
				}
			}
		}
");
			Assert.IsTrue(map.ContainsKey("perfect"));
			Assert.AreEqual(map["perfect"].MovementSpeed, 1.2f, 0.01f);
		}
	}
}