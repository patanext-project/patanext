extends GodotModuleRunner


# Called when the node enters the scene tree for the first time.
func _ready():
	var directory = ProjectSettings.globalize_path("res://dotnet/managed")
	if OS.has_feature("standalone"):
		directory = OS.get_executable_path().get_base_dir()
	
	GodotModuleRunner.load_module("PataNext.Export.Godot", "Entry", directory)
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	GodotModuleRunner.loop()
	pass
