using Godot;
using System;
using PataNext.models.scripts;

public partial class test_anim_tree : Node3D
{
	private AnimationFloatNode _floatNode;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_floatNode = new AnimationFloatNode(true);
		
		var animTree = GetNode<AnimationTree>("AnimationTree");
		var blendTree = (AnimationNodeBlendTree) animTree.TreeRoot;
		
		blendTree.AddNode("root", _floatNode);
		blendTree.ConnectNode("output", 0, "root");
		
		GD.Print($"output? {blendTree.GetNode("output")}");

		var wLeft = _floatNode.AddWeightedInput();
		blendTree.AddNode("w_left", new AnimationNodeAnimation {Animation = "anim_lib/left"});
		blendTree.ConnectNode("root", wLeft, "w_left");
		
		var wRight = _floatNode.AddWeightedInput();
		blendTree.AddNode("w_right", new AnimationNodeAnimation {Animation = "anim_lib/right"});
		blendTree.ConnectNode("root", wRight, "w_right");
		
		_floatNode.SetWeight(wLeft, 0.1f);
		_floatNode.SetWeight(wRight, 0.2f);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		_floatNode.SetWeight(0, (float) GetNode<VSlider>("Control/left").Ratio);
		_floatNode.SetWeight(1, (float) GetNode<VSlider>("Control/right").Ratio);
		_floatNode.IsNormalized = GetNode<CheckButton>("Control/CheckButton").ButtonPressed;
	}
}
