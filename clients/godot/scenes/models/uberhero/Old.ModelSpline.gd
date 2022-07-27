@tool

extends Node3D

const GpuSplineBezier = preload("res://scenes/components/gpu_spline/gpu_spline_bezier.gd")

@export var Point0Path : NodePath = NodePath("Point0")
@export var Point1Path : NodePath = NodePath("Point1")
@export var Point2Path : NodePath = NodePath("Point2")

@export var ModifyRotation : bool = true

@onready var Spline: GpuSplineBezier = get_node("Spline") as GpuSplineBezier

var pointA: Node3D
var pointB: Node3D
var pointC: Node3D

# Called when the node enters the scene tree for the first time.
func _ready():
	pointA = get_node(Point0Path)
	pointB = get_node(Point1Path)
	pointC = get_node(Point2Path)
	pass # Replace with function body.

func bzPos(t: float, a: Vector2, b: Vector2, c: Vector2) -> Vector2:
	var mT: float = 1.-t;
	return (a *           mT*mT +
			b * 2.* t   * mT    + 
			c *     t*t          );

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	var a2 = Vector2(pointA.position.x, pointA.position.y)
	var b2 = Vector2(pointB.position.x, pointB.position.y)
	var c2 = Vector2(pointC.position.x, pointC.position.y)
	
	b2 = (4.0 * b2 - a2 - c2) / 2.0;
	
	# var p0 = bzPos(0.9, a2, b2, c2)
	# var p1 = bzPos(1.0, a2, b2, c2)
	var p1 = c2
	var p0 = b2
	
	# c2 - b2 work too, but this is more precise
	if ModifyRotation:
		pointC.rotation = Vector3(0, 0, (p1 - p0).angle())
		
	return
	if global_transform.basis.get_scale().x < 0.:
		a2.x = -a2.x
		b2.x = -b2.x
		c2.x = -c2.x
	
	Spline.PointA = a2
	Spline.PointB = b2
	Spline.PointC = c2
