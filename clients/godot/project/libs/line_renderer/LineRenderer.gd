@tool

extends MeshInstance3D

@export var points: Array[Vector3] = [Vector3(0,0,0),Vector3(0,5,0)]
@export var startThickness = 0.1
@export var endThickness = 0.1
@export var cornerSmooth = 5
@export var capSmooth = 5
@export var drawCaps = true
@export var drawCorners = true
@export var globalCoords = true
@export var scaleTexture = true

var camera
var cameraOrigin

func _init():
	if mesh == null:
		mesh = ImmediateMesh.new();
		print("create")

func _ready():
	pass

func _process(delta):
	if points.size() < 2:
		return
	
	camera = get_viewport().get_camera_3d()
	if camera == null:
		return
	cameraOrigin = to_local(camera.get_global_transform().origin)
	
	var progressStep = 1.0 / points.size();
	var progress = 0;
	var thickness = lerp(startThickness, endThickness, progress);
	var nextThickness = lerp(startThickness, endThickness, progress + progressStep);
	
	var immediate = mesh as ImmediateMesh
	immediate.clear_surfaces()
	immediate.surface_begin(Mesh.PRIMITIVE_TRIANGLES)
	
	for i in range(points.size() - 1):
		var A = points[i]
		var B = points[i+1]
	
		if globalCoords:
			A = to_local(A)
			B = to_local(B)
	
		var AB = B - A;
		var orthogonalABStart = (cameraOrigin - ((A + B) / 2)).cross(AB).normalized() * thickness;
		var orthogonalABEnd = (cameraOrigin - ((A + B) / 2)).cross(AB).normalized() * nextThickness;
		
		var AtoABStart = A + orthogonalABStart
		var AfromABStart = A - orthogonalABStart
		var BtoABEnd = B + orthogonalABEnd
		var BfromABEnd = B - orthogonalABEnd
		
		if i == 0:
			if drawCaps:
				cap(A, B, thickness, capSmooth)
		
		if scaleTexture:
			var ABLen = AB.length()
			var ABFloor = floor(ABLen)
			var ABFrac = ABLen - ABFloor
			
			immediate.surface_set_uv(Vector2(ABFloor, 0))
			immediate.surface_add_vertex(AtoABStart)
			immediate.surface_set_uv(Vector2(-ABFrac, 0))
			immediate.surface_add_vertex(BtoABEnd)
			immediate.surface_set_uv(Vector2(ABFloor, 1))
			immediate.surface_add_vertex(AfromABStart)
			immediate.surface_set_uv(Vector2(-ABFrac, 0))
			immediate.surface_add_vertex(BtoABEnd)
			immediate.surface_set_uv(Vector2(-ABFrac, 1))
			immediate.surface_add_vertex(BfromABEnd)
			immediate.surface_set_uv(Vector2(ABFloor, 1))
			immediate.surface_add_vertex(AfromABStart)
		else:
			immediate.surface_set_uv(Vector2(1, 0))
			immediate.surface_add_vertex(AtoABStart)
			immediate.surface_set_uv(Vector2(0, 0))
			immediate.surface_add_vertex(BtoABEnd)
			immediate.surface_set_uv(Vector2(1, 1))
			immediate.surface_add_vertex(AfromABStart)
			immediate.surface_set_uv(Vector2(0, 0))
			immediate.surface_add_vertex(BtoABEnd)
			immediate.surface_set_uv(Vector2(0, 1))
			immediate.surface_add_vertex(BfromABEnd)
			immediate.surface_set_uv(Vector2(1, 1))
			immediate.surface_add_vertex(AfromABStart)
		
		if i == points.size() - 2:
			if drawCaps:
				cap(B, A, nextThickness, capSmooth)
		else:
			if drawCorners:
				var C = points[i+2]
				if globalCoords:
					C = to_local(C)
				
				var BC = C - B;
				var orthogonalBCStart = (cameraOrigin - ((B + C) / 2)).cross(BC).normalized() * nextThickness;
				
				var angleDot = AB.dot(orthogonalBCStart)
				
				if angleDot > 0:
					corner(B, BtoABEnd, B + orthogonalBCStart, cornerSmooth)
				else:
					corner(B, B - orthogonalBCStart, BfromABEnd, cornerSmooth)
		
		progress += progressStep;
		thickness = lerp(startThickness, endThickness, progress);
		nextThickness = lerp(startThickness, endThickness, progress + progressStep);
	
	immediate.surface_end()

func cap(center, pivot, thickness, smoothing):
	var immediate = mesh as ImmediateMesh
	
	var orthogonal = (cameraOrigin - center).cross(center - pivot).normalized() * thickness;
	var axis = (center - cameraOrigin).normalized();
	
	var array = []
	for i in range(smoothing + 1):
		array.append(Vector3(0,0,0))
	array[0] = center + orthogonal;
	array[smoothing] = center - orthogonal;
	
	for i in range(1, smoothing):
		array[i] = center + (orthogonal.rotated(axis, lerp(0, PI, float(i) / smoothing)));
	
	for i in range(1, smoothing + 1):
		immediate.surface_set_uv(Vector2(0, (i - 1) / smoothing))
		immediate.surface_add_vertex(array[i - 1]);
		immediate.surface_set_uv(Vector2(0, (i - 1) / smoothing))
		immediate.surface_add_vertex(array[i]);
		immediate.surface_set_uv(Vector2(0.5, 0.5))
		immediate.surface_add_vertex(center);
		
func corner(center, start, end, smoothing):
	var immediate = mesh as ImmediateMesh
	
	var array = []
	for i in range(smoothing + 1):
		array.append(Vector3(0,0,0))
	array[0] = start;
	array[smoothing] = end;
	
	var axis = start.cross(end).normalized()
	var offset = start - center
	var angle = offset.angle_to(end - center)
	
	for i in range(1, smoothing):
		array[i] = center + offset.rotated(axis, lerp(0, angle, float(i) / smoothing));
	
	for i in range(1, smoothing + 1):
		immediate.surface_set_uv(Vector2(0, (i - 1) / smoothing))
		immediate.surface_add_vertex(array[i - 1]);
		immediate.surface_set_uv(Vector2(0, (i - 1) / smoothing))
		immediate.surface_add_vertex(array[i]);
		immediate.surface_set_uv(Vector2(0.5, 0.5))
		immediate.surface_add_vertex(center);
		
