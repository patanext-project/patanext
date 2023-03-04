shader_type spatial;

render_mode unshaded, cull_disabled;

uniform vec4 Color : source_color = vec4(0., 0., 0., 1.);
uniform vec3[3] Points;
uniform float Width : hint_range(0.0, 1.0, 0.001) = 0.16;

uniform vec2 body_direction;

#include "assets/shaders/incls/spline_utils.gdshaderinc"

void vertex() {
	float t = UV.x;
	
	vec3[3] pts = Points;
	//for (int i = 0; i < 3; i++) pts[i] = pts[i] * MODEL_NORMAL_MATRIX * 1.;
	
	vec3 pos = bezier3Curve(pts[0], pts[1], pts[2], t);
	vec3 norm = normalize(pos - bezier3Curve(pts[0], pts[1], pts[2], t - 0.01));
	norm = flatNormal(norm);
	//norm = bezier3Normal(Points[0], Points[1], Points[2], t);
	
	// - 0.5 to put in center
	// * 2. for.... double of the scale
	pos += norm * Width * (UV.y - 0.5) * 2.;
	//pos.z += 0.5;
	VERTEX = pos;
	
	MODELVIEW_MATRIX = VIEW_MATRIX * mat4(INV_VIEW_MATRIX[0], INV_VIEW_MATRIX[1], INV_VIEW_MATRIX[2], MODEL_MATRIX[3]);
}



float sdQuad( in vec2 p, in vec2 p0, in vec2 p1, in vec2 p2, in vec2 p3 )
{
	vec2 e0 = p1 - p0; vec2 v0 = p - p0;
	vec2 e1 = p2 - p1; vec2 v1 = p - p1;
	vec2 e2 = p3 - p2; vec2 v2 = p - p2;
	vec2 e3 = p0 - p3; vec2 v3 = p - p3;

	vec2 pq0 = v0 - e0*clamp( dot(v0,e0)/dot(e0,e0), 0.0, 1.0 );
	vec2 pq1 = v1 - e1*clamp( dot(v1,e1)/dot(e1,e1), 0.0, 1.0 );
	vec2 pq2 = v2 - e2*clamp( dot(v2,e2)/dot(e2,e2), 0.0, 1.0 );
    vec2 pq3 = v3 - e3*clamp( dot(v3,e3)/dot(e3,e3), 0.0, 1.0 );
    
    vec2 ds = min( min( vec2( dot( pq0, pq0 ), v0.x*e0.y-v0.y*e0.x ),
                        vec2( dot( pq1, pq1 ), v1.x*e1.y-v1.y*e1.x )),
                   min( vec2( dot( pq2, pq2 ), v2.x*e2.y-v2.y*e2.x ),
                        vec2( dot( pq3, pq3 ), v3.x*e3.y-v3.y*e3.x ) ));

    float d = sqrt(ds.x);

	return (ds.y>0.0) ? -d : d;
}

float aa(float d) {
	float aa_size = 0.;
	float smoothness = 10.0;
	d = smoothstep(aa_size * 0.1 - smoothness * 0.1, aa_size * 0.1 + smoothness * 0.1, d);
	
	float pwidth = length(vec2(dFdx(d), dFdy(d)));
	d = clamp((0.5 - d) / pwidth, 0., 1.);
	return d;
}

float sdCircle(vec2 p, float r) {
	return length(p) - r;
}

vec2 uvrotate(vec2 uv, vec2 center, float rotation) {
	uv -= center;
    float s = sin(rotation);
    float c = cos(rotation);
    mat2 rMatrix = mat2(vec2(c, -s), vec2(s, c));
    rMatrix *= 0.5;
    rMatrix += 0.5;
    rMatrix = rMatrix * 2. - 1.;
    uv.xy = uv.xy * rMatrix;
    uv += center;
	return uv;
}

vec2 uvspherize(vec2 uv, vec2 center, float strength, vec2 offset) {
	vec2 delta = uv - center;
	float delta2 = dot(delta.xy, delta.xy);
	float delta4 = delta2 * delta2;
	vec2 delta_offset = vec2(delta4 * strength);
	
	return uv + delta * delta_offset + offset;
}

void fragment() {
	// Place fragment code here.
	ALBEDO = Color.xyz;
	ALBEDO = vec3(UV, 1.);
	
	const float rotdegrees = PI * 0.5;
	
	vec2 dir = vec2(-0.4, 0.);
	dir = body_direction;
	
	vec2 uv = UV - 0.5;
	uv = uvrotate(uv, vec2(0.), rotdegrees);
	uv = uvspherize(uv, dir * 0.1, 3.5, dir);
	
	float d = aa(sdQuad(
		uv,
		vec2(0., 0.325),
		vec2(+0.3, 0.05),
		vec2(0., -0.45),
		vec2(-0.3, 0.05)
	));
	
	//d = aa(sdCircle(UV - 0.5, 0.5));
	
	ALBEDO = vec3(d);
}
