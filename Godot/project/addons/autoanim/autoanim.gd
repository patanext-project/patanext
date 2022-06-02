@tool
extends EditorPlugin


func _enter_tree():
	# Initialization of the plugin goes here.
	get_undo_redo().connect("version_changed", undo_redo_vc)
	pass

func undo_redo_vc():
	var undo_redo: UndoRedo = get_undo_redo()
	print(undo_redo.get_current_action_name())
#	var auto = true
#	var node = modified_object as Node
#	print(node)
#	if not node:
#		return;
#
#	var path = (node.get_path() as String) + property
#	print(path)
#	print(property)
#	pass
	
func get_as_node_path(path: NodePath) -> NodePath:
	assert(":" in (path as String))  # Causes a hard crash
	path = path as NodePath
	var node_path = path as String
	var property_path = path.get_concatenated_subnames() as String
	node_path = node_path.substr(0, (path as String).length() - property_path.length() - 1)
	return node_path as NodePath
	
func get_node_property(from: Node, path: NodePath):
	assert(":" in (path as String))  # Causes a hard crash
	path = path as NodePath
	var node_path = get_as_node_path(path)
	var property_path = NodePath(path.get_concatenated_subnames()).get_as_property_path()
	return from.get_node(node_path).get_indexed(property_path)

func _process(dt: float):
	if Input.is_key_pressed(KEY_INSERT):
		_update_stuff("", true)
	else:
		_update_stuff()

func _update_stuff(property: String = "", auto_place: bool = false):
	var tree: Node = get_tree().get_edited_scene_root()
	var player: AnimationPlayer = tree.find_child("AnimationPlayer")
	if player == null:
		return
		
	if player.assigned_animation == "" or player.assigned_animation == "RESET":
		return
		
	var animation: Animation = player.get_animation(player.assigned_animation)
	var currPos = player.current_animation_position
	for trackIdx in animation.get_track_count():
		if property != "" and (animation.track_get_path(trackIdx) as String) != property:
			continue
		
		var value = get_node_property(tree, animation.track_get_path(trackIdx))
		var selected_key = -1
		for keyIdx in animation.track_get_key_count(trackIdx):
			var keyTime = animation.track_get_key_time(trackIdx, keyIdx)
			if abs(keyTime - player.current_animation_position) < 0.01:
				selected_key = keyIdx
				
		if selected_key == -1 and auto_place:
			if animation.track_get_type(trackIdx) == Animation.TYPE_BEZIER:
				animation.bezier_track_insert_key(trackIdx, currPos, value)
			else:
				animation.track_insert_key(trackIdx, selected_key, value)
				
		if selected_key >= 0:
			if animation.track_get_type(trackIdx) == Animation.TYPE_BEZIER:
				animation.bezier_track_set_key_value(trackIdx, selected_key, value)
			else:
				animation.track_set_key_value(trackIdx, selected_key, value)

func _exit_tree():
	# Clean-up of the plugin goes here.
	pass
