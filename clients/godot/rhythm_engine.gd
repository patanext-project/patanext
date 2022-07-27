extends Node

var inputs: Array;

var title: String;
var curr_command: String;
var predicted: String;

func _ready():
	pass # Replace with function body.


func _process(_delta):	
	if Input.is_action_just_pressed("drum_left"):
		inputs.append(1)
	if Input.is_action_just_pressed("drum_right"):
		inputs.append(2)
	if Input.is_action_just_pressed("drum_down"):
		inputs.append(3)
	if Input.is_action_just_pressed("drum_up"):
		inputs.append(4)
		
	$Label.text = title
	$CurrCommand.text = curr_command
	$Predicted.text = predicted

func has_input_left():
	return inputs.size() > 0

func get_last_input() -> int:
	if !has_input_left():
		return 0
	
	var last: int = inputs[0]
	inputs.remove_at(0)
	
	return last;
