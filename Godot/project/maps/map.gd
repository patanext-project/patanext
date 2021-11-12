extends Node


# Declare member variables here. Examples:
# var a = 2
# var b = "text"

func get_all_collision_shapes(node) -> Array:
	var array: Array = [];
	for N in node.get_children():
		if N is CollisionShape:
			array.append(N)
		
		if N.get_child_count() > 0:
			array.append_array(get_all_collision_shapes(N))
			
	return array
	
func get_all_meshes(node) -> Array:
	var array: Array = [];
	for N in node.get_children():
		if N is MeshInstance:
			array.append(N)
		
		if N.get_child_count() > 0:
			array.append_array(get_all_meshes(N))
			
	return array

# Called when the node enters the scene tree for the first time.
func _ready():
#	var shapes: Array = get_all_collision_shapes(self);
#	for _i in shapes:
#		var shape = _i as CollisionShape;
#		print(shape, " : ", shape.shape)
#		if shape.shape is ConvexPolygonShape:
#			var convex = shape.shape as ConvexPolygonShape;
#			for _v in convex.points:
#				print("  ", _v)
#
#	print("oui bon ok")
	
	var meshes = get_all_meshes(self);
	var output = Output.new();
	
	for _i in meshes:
		var instance = _i as MeshInstance;
		var mesh: Mesh = instance.mesh;
		print(mesh);
		
		var o: MeshOutputBase
		if (mesh is CubeMesh):
			var cube = mesh as CubeMesh;
			revghost.exchange("simu_share_collision_mesh", "cube", cube.size)
			
			o = CubeMeshOutput.new()
			o.size = cube.size
		
		o.position = instance.translation
		o.scale = instance.scale
		o.rotation = instance.rotation
		
		output.outputs.append(o)
		
	var r = JSON.print(output.get_array(), "\t")
	print(output.outputs.size())
	print(r)
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
#func _process(delta):
#	pass

class Output:
	var outputs: Array
	
	func get_array() -> Array:
		var arr: Array;
		for _i in outputs:
			arr.append(_i.to_dictionary())
			
		return arr

class MeshOutputBase:
	var type: String
	var position: Vector3
	var scale: Vector3
	var rotation: Vector3
	
	func to_dictionary() -> Dictionary:
		var d: Dictionary
		d["type"] = type
		d["position"] = position
		d["scale"] = scale
		d["rotation"] = rotation
		
		return d
	
class CubeMeshOutput:
	extends MeshOutputBase
	
	var size: Vector3
	
	func _init():
		type = "cube"

	func to_dictionary() -> Dictionary:
		var d = .to_dictionary()
		d["size"] = size
		return d
