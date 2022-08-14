@tool

extends SplineModel

const GpuSplineBezier = preload("res://scenes/components/gpu_spline/gpu_spline_bezier.gd")

@export var Point0Path : NodePath = NodePath("Point0")
@export var Point1Path : NodePath = NodePath("Point1")
@export var Point2Path : NodePath = NodePath("Point2")

@export var ModifyRotation : bool = true

@onready var Spline: GpuSplineBezier = get_node("Spline") as GpuSplineBezier

# Called when the node enters the scene tree for the first time.
func _ready():
	var pointA = get_node(Point0Path)
	var pointB = get_node(Point1Path)
	var pointC = get_node(Point2Path)
	init(pointA, pointB, pointC, Spline)

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	update(ModifyRotation, global_transform.basis.get_scale().x < 0, global_position)
