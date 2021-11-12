extends Control

func _on_Button_pressed():
	# Exchange method
	# First argument: method name
	# Next arguments (variadic (aka params in C#)): method arguments
	#
	# In this case there are no method arguments
	# This will call Stormium.Export.Godot.Program.OnExchange(,,)
	$Button.text = $Node.exchange("party_toggle")[0]

func _process(delta):
	if (delta == 0):
		return;
	
	$Label.text = "FPS = " + str(1 / delta)
