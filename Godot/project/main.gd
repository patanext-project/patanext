extends Control

# load the SIMPLE library
const SIMPLE = preload("res://gdnative/simple.gdns")
onready var data = SIMPLE.new()

func _on_Button_pressed():
	$Label.text = "Data = " + data.get_data()

func _process(delta):
	$Label.text = "FPS = " + str(1 / delta)
