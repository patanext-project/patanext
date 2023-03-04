shader_type spatial;

render_mode unshaded;

void vertex() {
	VERTEX.z *= 0.1;
	VERTEX.z -= 0.2;
	
	MODELVIEW_MATRIX = VIEW_MATRIX * mat4(INV_VIEW_MATRIX[0], INV_VIEW_MATRIX[1], INV_VIEW_MATRIX[2], MODEL_MATRIX[3]);
}

void fragment() {
	// Place fragment code here.
	ALBEDO = vec3(0.);
}
