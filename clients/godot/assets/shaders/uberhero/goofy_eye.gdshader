shader_type spatial;

render_mode unshaded;

uniform vec3 eye_roundness = vec3(1., 1., 1.);
uniform float eye_size = 0.375;

uniform vec3 iris_roundness = vec3(1., 1., 1.);
uniform float iris_size = 0.25;

uniform vec3 body_color : source_color = vec3(0., 0., 0.);
uniform vec3 eye_color : source_color = vec3(1., 1., 1.);
uniform vec3 iris_color : source_color = vec3(0., 0., 0.);

uniform vec2 body_direction = vec2(0., 0.);
uniform vec2 eye_direction = vec2(0., 0.);

float sdCircle(vec2 p, float r) {
	return length(p) - r;
}

float sdBoxRounded(vec2 p, vec2 b, float r) {
	vec4 radius = vec4(r);
	radius.xy = (p.x>0.0)?radius.xy : radius.zw;
	radius.x  = (p.y>0.0)?radius.x  : radius.y;
	vec2 q = abs(p) - b + radius.x;
	return min(max(q.x, q.y), 0.0) + length(max(q, 0.0)) - radius.x;
}

float sdAaPart(vec2 p, vec3 roundness, float size) {	
	float d = sdBoxRounded(p, roundness.xy * size, roundness.z * size);
	
	float aa_size = 0.;
	float smoothness = 10.0;
	d = smoothstep(aa_size * 0.1 - smoothness * 0.1, aa_size * 0.1 + smoothness * 0.1, d);
	
	float pwidth = length(vec2(dFdx(d), dFdy(d)));
	d = clamp((0.5 - d) / pwidth, 0., 1.);
	
	return d;
}

vec2 uvspherize(vec2 uv, vec2 center, float strength, vec2 offset) {
	vec2 delta = uv - center;
	float delta2 = dot(delta.xy, delta.xy);
	float delta4 = delta2 * delta2;
	vec2 delta_offset = vec2(delta4 * strength);
	
	return uv + delta * delta_offset + offset;
}

float sdAaEye(vec2 p, float size) {
	p = uvspherize(p, vec2(0.), 3., body_direction);
	
	return sdAaPart(p, eye_roundness, eye_size);
}

float sdAaIris(vec2 p, float size) {
	p = uvspherize(p, body_direction * -0.15, 5., eye_direction + body_direction * 0.05);
	
	return sdAaPart(p, iris_roundness, iris_size);
}

void vertex() {
	VERTEX.z += 0.05;
	MODELVIEW_MATRIX = VIEW_MATRIX * mat4(INV_VIEW_MATRIX[0], INV_VIEW_MATRIX[1], INV_VIEW_MATRIX[2], MODEL_MATRIX[3]);
}

vec3 blendsub(vec3 base, vec3 blend, float o) {
	return mix(base, base - blend, o);
}

vec3 blendow(vec3 base, vec3 blend, float o) {
	return mix(base, blend, o);
}

vec3 fill(vec3 base, float d, vec3 color) {
	if (d <= 0.)
		return base;
	return mix(base, color, d);
}

void fragment() {
	// Place fragment code here.
	vec3 c = body_color;
	c = fill(c, sdAaEye(UV - 0.5, 0.), eye_color);
	c = fill(c, sdAaIris(UV - 0.5, 0.), iris_color);
	c = fill(c, 1. - sdAaPart(UV - 0.5, eye_roundness, 0.48), body_color);
	
	ALBEDO = c;
}
