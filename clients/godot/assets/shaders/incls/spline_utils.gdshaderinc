vec3 bezier3Curve(vec3 p0, vec3 p1, vec3 p2, float t) {
	float squareT = (1. - t) * (1. - t);
	return squareT * p0 + 2. * (1. - t) * t * p1 + t * t * p2;
}

vec3 bezier3Derivative(vec3 p0, vec3 p1, vec3 p2, float t) {
	vec3 d1 = 2. * (p1 - p0);
	vec3 d2 = 2. * (p2 - p1);
	
	return (1. - t) * d1 + t * d2;
}

vec3 bezier3Normal(vec3 p0, vec3 p1, vec3 p2, float t) {
	vec3 d = bezier3Derivative(p0, p1, p2, t);
	float q = sqrt(d.x * d.x + d.y * d.y);

	return normalize(vec3(-d.y / q, d.x / q, 0.));
}

vec3 flatNormal(vec3 d) {
	float q = sqrt(d.x * d.x + d.y * d.y);
	
	return normalize(vec3(-d.y / q, d.x / q, 0.));
}

vec3 bezier4Curve(vec3 p0, vec3 p1, vec3 p2, vec3 p3, float t) {
	float squareT3 = (1. - t) * (1. - t) * (1. - t);
	float squareT2 = (1. - t) * (1. - t);
	float squareT1 = (1. - t);
	
	float powerT2 = t * t;
	float powerT3 = t * t * t;
	return squareT3 * p0 + (3. * squareT2 * t * p1) + (3. * squareT1 * powerT2 * p2) + powerT3 * p3;
}