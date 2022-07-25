@tool

extends Node3D

const GpuSplineBezier = preload("res://scenes/components/gpu_spline/gpu_spline_bezier.gd")

@export var Point0Path : NodePath = NodePath("Point0")
@export var Point1Path : NodePath = NodePath("Point1")
@export var Point2Path : NodePath = NodePath("Point2")

@export var ModifyRotation : bool = true

@onready var Points: Array[Node3D] = [
	get_node(Point0Path) as Node3D,
	get_node(Point1Path) as Node3D,
	get_node(Point2Path) as Node3D
]

@onready var Spline: GpuSplineBezier = get_node("Spline") as GpuSplineBezier

# Called when the node enters the scene tree for the first time.
func _ready():
	pass # Replace with function body.

func bzPos(t: float, a: Vector2, b: Vector2, c: Vector2) -> Vector2:
	var mT: float = 1.-t;
	return (a *           mT*mT +
			b * 2.* t   * mT    + 
			c *     t*t          );

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	var a = get_node(Point0Path).position
	var b = get_node(Point1Path).position
	var c = get_node(Point2Path).position
	
	var a2 = Vector2(a.x, a.y)
	var b2 = Vector2(b.x, b.y)
	var c2 = Vector2(c.x, c.y)
	
	b2 = (4.0 * b2 - a2 - c2) / 2.0;
	
	var p0 = bzPos(0.9, a2, b2, c2)
	var p1 = bzPos(1.0, a2, b2, c2)
	
	# c2 - b2 work too, but this is more precise
	if ModifyRotation:
		get_node(Point2Path).rotation = Vector3(0, 0, (p1 - p0).angle())
		
	if global_transform.basis.get_scale().x < 0.:
		a2.x = -a2.x
		b2.x = -b2.x
		c2.x = -c2.x
	
	$Spline.PointA = a2
	$Spline.PointB = b2
	$Spline.PointC = c2
		
	pass
