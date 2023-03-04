@tool

class_name TestFloatAnimTree
extends AnimationNodeBlend2

func _init():
	print("created")
	
	add_input("anim0")
	add_input("anim1")
	add_input("anim2")

func _process(time, seek, seek_root):
	print("yay")

func _get_parameter_default_value(parameter):
	return 0

func _get_caption():
	"BlendFloat"
