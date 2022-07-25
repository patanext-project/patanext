shader_type spatial;
render_mode unshaded, cull_disabled;

const bool ENABLE_UV_MAP = false;

uniform vec4 Color : source_color = vec4(0., 0., 0., 1.);

uniform vec2 PointA = vec2(0., 0.);
uniform vec2 PointB = vec2(0.5, 0.5);
uniform vec2 PointC = vec2(1.0, 0.);

uniform float Thickness : hint_range(0.0, 1.0, 0.01) = 0.2;
uniform float Smoothness : hint_range(0.0, 1.0, 0.01) = 0.2;

// Solve cubic equation for roots
vec2 solveCubic2(vec3 a)
{
	float p  = a.y - a.x*a.x/3.,
	      p3 = p*p*p,
	      q  = a.x* ( 2.*a.x*a.x - 9.*a.y ) /27. + a.z,
	      d  = q*q + 4.*p3/27.;
    
	if(d>0.) {
		vec2 x = ( vec2(1,-1)*sqrt(d) -q ) *.5;
        x = sign(x) * pow( abs(x) , vec2(1./3.) );
  		return vec2( x.x+x.y -a.x/3. );
  	}
    
 	float v = acos( -sqrt(-27./p3)*q*.5 ) / 3.,
 	      m = cos(v),
 	      n = sin(v)*sqrt(3.);

	return vec2(m+m,-n-m) * sqrt(-p/3.) - a.x/3.;
}

vec3 solveCubic(float a, float b, float c)
{
    float p = b - a*a / 3.0, p3 = p*p*p;
    float q = a * (2.0*a*a - 9.0*b) / 27.0 + c;
    float d = q*q + 4.0*p3 / 27.0;
    float offset = -a / 3.0;
    if(d >= 0.0) { 
        float z = sqrt(d);
        vec2 x = (vec2(z, -z) - q) / 2.0;
        vec2 uv = sign(x)*pow(abs(x), vec2(1.0/3.0));
        return vec3(offset + uv.x + uv.y);
    }
    float v = acos(-sqrt(-27.0 / p3) * q / 2.0) / 3.0;
    float m = cos(v), n = sin(v)*1.732050808;
    return vec3(m + m, -n - m, n - m) * sqrt(-p / 3.0) + offset;
}

float dd(vec2 a) { return dot(a, a); }

float sdBezier(vec2 A, vec2 B, vec2 C, vec2 p)
{
    B = mix(B + vec2(1e-4), B, abs(sign(B * 2.0 - A - C)));
    vec2 a = B - A, b = A - B * 2.0 + C, c = a * 2.0, d = A - p;
    vec3 k = vec3(3.*dot(a,b),2.*dot(a,a)+dot(d,b),dot(d,a)) / dot(b,b);      
    vec3 t = clamp(solveCubic(k.x, k.y, k.z), 0.0, 1.0);
    vec2 pos = A + (c + b*t.x)*t.x;
    float dis = length(pos - p);
    pos = A + (c + b*t.y)*t.y;
    dis = min(dis, length(pos - p));
    pos = A + (c + b*t.z)*t.z;
    dis = min(dis, length(pos - p));
    return dis;
}

float cro( in vec2 a, in vec2 b ) { return a.x*b.y - a.y*b.x; }

/*vec2 sdBezierUV(vec2 v0, vec2 v1, vec2 v2, vec2 p) {
		vec2 i = v0 - v2;
    vec2 j = v2 - v1;
    vec2 k = v1 - v0;
    vec2 w = j-k;

	v0-= p; v1-= p; v2-= p;
    
	float x = cro(v0, v2);
    float y = cro(v1, v0);
    float z = cro(v2, v1);

	vec2 s = 2.0*(y*j+z*k)-x*i;

    float r =  (y*z-x*x*0.25)/dd(s);
    float t = clamp( (0.5*x+y+r*dot(s,w))/(x+y+z),0.0,1.0);
    
	return vec2(t);
}*/

vec2 sdBezierUV(vec2 A, vec2 B, vec2 C, vec2 p) {
    // This is to prevent 3 colinear points, but there should be better solution to it.
    B = mix(B + vec2(1e-4), B, abs(sign(B * 2.0 - A - C)));
    
    // Calculate roots.
    vec2 a = B - A, b = A - B * 2.0 + C, c = a * 2.0, d = A - p;
    vec3 k = vec3(3.*dot(a,b),2.*dot(a,a)+dot(d,b),dot(d,a)) / dot(b,b);      
    vec2 t = clamp(solveCubic2(k).xy, 0.0, 1.0);
    // note: 3rd root is actually unnecessary, it's never closest!
    
    // [Tom'2017] simplified calculations
    vec2 dp1 = d + (c + b*t.x)*t.x;
    float d1 = dot(dp1, dp1);
    vec2 dp2 = d + (c + b*t.y)*t.y;
    float d2 = dot(dp2, dp2);
   
    // Find closest distance and t
    vec4 r = (d1 < d2) ? vec4(d1, t.x, dp1) : vec4(d2, t.y, dp2);
  
    // Sign is just cross product with gradient
    vec2 g = 2.*b*r.y + c;
    float s =  sign(g.x*r.w - g.y*r.z);

    float dist = sqrt(r.x);
   
    return vec2(0., r.y);
}

void vertex() {
	MODELVIEW_MATRIX = VIEW_MATRIX * mat4(INV_VIEW_MATRIX[0], INV_VIEW_MATRIX[1], INV_VIEW_MATRIX[2], MODEL_MATRIX[3]);
}

void fragment() {
	float d = sdBezier(PointA, PointB, PointC, UV);
	
	ALBEDO = Color.xyz;
	if (ENABLE_UV_MAP) {
		ALBEDO = sdBezierUV(PointA, PointB, PointC, UV).yyy;
	}
	float a = smoothstep(Thickness * 0.1 - Smoothness * 0.1, Thickness * 0.1 + Smoothness * 0.1, d);
	//float pwidth = fwidth(a);
	float pwidth = length(vec2(dFdx(a), dFdy(a))); 
	
	a = clamp((0.5 - a) / pwidth, 0., 1.);
	//a = smoothstep(0.5, 0.5 - pwidth * 1.5, a);
	
	ALPHA = a * Color.w;
}
