@tool

extends MeshInstance3D

@export var PointA : Vector2
@export var PointB : Vector2
@export var PointC : Vector2

@export_range(0., 10., 0.01) var Thickness : float = 0.3
@export_range(0., 1., 0.01) var Offset : float = 0.5

class BoundingSquare:
	var lb : Vector2 = Vector2(1.79769e308, 79769e308)
	var rt : Vector2 = Vector2(-1.79769e308, -1.79769e308)
	
	func give(point : Vector2):
		if lb.x > point.x:
			lb.x = point.x
		if lb.y > point.y:
			lb.y = point.y
			
		if rt.x < point.x:
			rt.x = point.x
		if rt.y < point.y:
			rt.y = point.y
			
	func signNz(f: float) -> float: return +1. if f >= 0. else -1.
			
	func size() -> Vector2:
		# since we force the quad to be in the center
		# we put abs on every element
		#var rectSize = Vector2(abs(abs(rt.x) - abs(lb.x)), abs(abs(rt.y) - abs(lb.y)))
		var rectSize = Vector2(max(abs(lb.x), abs(rt.x)), max(abs(lb.y), abs(rt.y)))
		rectSize.x = abs(max(rectSize.x, rectSize.y)) * signNz(rectSize.x)
		rectSize.y = abs(max(rectSize.x, rectSize.y)) * signNz(rectSize.y)
		return rectSize
	func center() -> Vector2:
		var rectSize = Vector2(abs(lb.x - rt.x), abs(lb.y - rt.y))
		var squareSize = size()
		var middle = lb.lerp(rt, 0.5)
		var op = (squareSize - rectSize) / 2
		op = op * Vector2(signNz(middle.x), signNz(middle.y))
		
		return lb.lerp(rt, 0.5) + (squareSize - rectSize) / 2
		
var bs: BoundingSquare = BoundingSquare.new()
var prevSize: Vector2

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	bs.lb = Vector2()
	bs.rt = Vector2()
	bs.give(PointA)
	bs.give(PointB)
	bs.give(PointC)
	
	var quad = mesh as QuadMesh
	var newSize = bs.size() * (2 + Offset + Thickness) + Vector2(Offset, Offset)
	if newSize != prevSize: 
		prevSize = newSize
		quad.size = newSize
	
	var center = Vector2(0.5, 0.5)
	# quad.center_offset = Vector3(center.x, center.y, 0.0)
		
	var invA = PointA  / quad.size + center
	invA.y = 1 - invA.y
	var invB = PointB / quad.size + center
	invB.y = 1 - invB.y
	var invC = PointC  / quad.size + center
	invC.y = 1 - invC.y
	
	quad.material.set_shader_param("PointA", invA)
	quad.material.set_shader_param("PointB", invB)
	quad.material.set_shader_param("PointC", invC)
	quad.material.set_shader_param("Thickness", Thickness / quad.size.x)
	
	pass

