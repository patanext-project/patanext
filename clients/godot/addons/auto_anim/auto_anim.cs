#if TOOLS
using Godot;
using System;
using System.Diagnostics;

[Tool]
public partial class auto_anim : EditorPlugin
{
	public override void _EnterTree()
	{
		GD.Print("hi yo!");
		// Initialization of the plugin goes here.
	}

	public override void _ExitTree()
	{
		// Clean-up of the plugin goes here.
	}

	public override void _Process(double delta)
	{
		base._Process(delta);

		if (Input.IsKeyPressed(Key.Pagedown))
			UpdateStuff(snap: true);
		if (Input.IsKeyPressed(Key.Insert))
			UpdateStuff("", true);
		else
			UpdateStuff();
	}

	private NodePath GetAsNodePath(NodePath nodePath)
	{
		var path = nodePath.ToString();
		
		Debug.Assert(path.Contains(':'), "path.Contains(':')");
		var propertyPath = nodePath.GetConcatenatedSubNames();
		path = path[..(path.Length - propertyPath.Length - 1)];
		return path;
	}

	private Variant GetNodeProperty(Node from, NodePath nodePath)
	{
		var path = GetAsNodePath(nodePath);
		var propertyPath = new NodePath(nodePath.GetConcatenatedSubNames()).GetAsPropertyPath();
		return from.GetNode(path).GetIndexed(propertyPath);
	}
	
	private void UpdateStuff(string property = "", bool autoPlace = false, bool snap = false)
	{
		var tree = GetTree().EditedSceneRoot;
		var player = tree.FindChild("AnimationPlayer") as AnimationPlayer;
		if (player == null)
			return;

		tree = player.GetParent();
		
		if (string.IsNullOrEmpty(player.AssignedAnimation) || player.AssignedAnimation == "RESET")
			return;

		var animation = player.GetAnimation(player.AssignedAnimation);
		var currPos = player.CurrentAnimationPosition;
		for (var trackIdx = 0; trackIdx < animation.GetTrackCount(); trackIdx++)
		{
			if (!string.IsNullOrEmpty(property) && animation.TrackGetPath(trackIdx) != property)
			{
				continue;
			}

			//GD.Print($"{animation.TrackGetPath(trackIdx)} {tree.Name}");

			var value = GetNodeProperty(tree, animation.TrackGetPath(trackIdx));

			var selectedKey = -1;
			var nearestKeyTime = double.MaxValue;
			for (var keyIdx = 0; keyIdx < animation.TrackGetKeyCount(trackIdx); keyIdx++)
			{
				var keyTime = animation.TrackGetKeyTime(trackIdx, keyIdx);
				var d = Math.Abs(keyTime - currPos);
				if (nearestKeyTime < d)
					nearestKeyTime = d;
				
				if (d < 0.0001) // it can be very sensitive, so make sure we're really near of the key time
				{
					selectedKey = keyIdx;
					break;
				}
			}

			if (snap && nearestKeyTime < double.MaxValue)
				player.Seek(nearestKeyTime);

			if (selectedKey == -1 && autoPlace)
			{
				if (animation.TrackGetType(trackIdx) == Animation.TrackType.Bezier)
					animation.BezierTrackInsertKey(trackIdx, currPos, (float) value.AsDouble());
				else
					animation.TrackInsertKey(trackIdx, selectedKey, value);
			}

			if (selectedKey >= 0)
			{
				if (animation.TrackGetType(trackIdx) == Animation.TrackType.Bezier)
					animation.BezierTrackSetKeyValue(trackIdx, selectedKey, (float) value.AsDouble());
				else
					animation.TrackSetKeyValue(trackIdx, selectedKey, value);
			}
		}
	}
}
#endif
