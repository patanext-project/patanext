shader_type spatial;

render_mode unshaded, cull_disabled;

uniform vec4 Color : source_color = vec4(0., 0., 0., 1.);
uniform vec3[3] Points;
uniform float Width : hint_range(0.0, 1.0, 0.001) = 0.16;

#include "assets/shaders/incls/spline_utils.gdshaderinc"

void vertex() {
	int vid = VERTEX_ID / 2;
	
	//VERTEX = vec3(VERTEX.x, VERTEX.y + float(vid) * 0.05, VERTEX.z);
	
	vec3 points[4] = vec3[4](
		vec3(-3., 0.0, 1.5),
		vec3(0.5, 0.2, 0.),
		vec3(3., -0.8, -0.1),
		vec3(4., 0.5, 0.1)
	);

	float t = UV.x;
	
	vec3 pos = bezier3Curve(Points[0], Points[1], Points[2], t);
	vec3 norm = normalize(pos - bezier3Curve(Points[0], Points[1], Points[2], t - 0.01));
	norm = flatNormal(norm);
	//norm = bezier3Normal(Points[0], Points[1], Points[2], t);
	
	pos += norm * Width * (VERTEX_ID % 2 == 1 ? -1. : +1.);
	VERTEX = pos;
	
	MODELVIEW_MATRIX = VIEW_MATRIX * mat4(INV_VIEW_MATRIX[0], INV_VIEW_MATRIX[1], INV_VIEW_MATRIX[2], MODEL_MATRIX[3]);
}

void fragment() {
	// Place fragment code here.
	ALBEDO = Color.xyz;
}
