using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CSharp;
using Unity.Entities;
using UnityEngine;

public class TestCompileToSystem : MonoBehaviour {

	private string log(object toLog)
	{
		Debug.Log(toLog);
		return toLog.ToString();
	}

	// Use this for initialization
	void Start () {
		return;
		var code = @"
		using Unity.Entities;
		using UnityEngine;
		using UnityEngine.Jobs;
		using P4Main.Graphics;

		namespace Teessst
		{
			[AlwaysUpdateSystem]
			public class MyCoolSystem : ComponentSystem
			{
				struct Group
				{
					public ComponentDataArray<CatmullRomSplineWorld> components;
					public TransformAccessArray transforms;
					public int Length;	
				}

				[Inject] Group m_Group;

				protected override void OnCreateManager(int capacity)
				{
					Debug.Log(""Helo"");
				}

				protected override void OnUpdate()
				{
					for (int i = 0; i != m_Group.components.Length; i++)
					{
						var component = m_Group.components[i];
						var transform = m_Group.transforms[i];
						transform.position = new Vector3(Mathf.PingPong(Time.time, 2), Mathf.PingPong(Time.time, 2));
					}
				}
			}
		}
		";

		CSharpCodeProvider provider = new CSharpCodeProvider();
		CompilerParameters parameters = new CompilerParameters(AppDomain.CurrentDomain.GetAssemblies()
			.Where(a => !a.IsDynamic)
			.Select(a => log(a.Location))
			.ToArray());
		parameters.GenerateInMemory = true;
		parameters.GenerateExecutable = false;
		CompilerResults results = provider.CompileAssemblyFromSource(parameters, code);

		if (results.Errors.HasErrors)
		{
			StringBuilder sb = new StringBuilder();

			foreach (CompilerError error in results.Errors)
			{
				sb.AppendLine(String.Format("Error ({0}): {1}", error.ErrorNumber, error.ErrorText));
			}

			throw new InvalidOperationException(sb.ToString());
		}

		var assembly = results.CompiledAssembly;
		// Get systems
		var systemTypes = assembly.GetTypes().Where(t => 
			t.IsSubclassOf(typeof(ComponentSystemBase)) && 
			!t.IsAbstract && 
			!t.ContainsGenericParameters && 
			t.GetCustomAttributes(typeof(DisableAutoCreationAttribute), true).Length == 0);
		foreach (var system in systemTypes)
		{
			var world = World.Active;
			world.CreateManager(system);
		}

		ScriptBehaviourUpdateOrder.UpdatePlayerLoop(World.Active);
	}
}