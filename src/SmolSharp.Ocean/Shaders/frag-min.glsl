#version 330
out vec4 FragColor;

uniform vec3 vals; // x = time, y = width, z = height

const vec3 waterBaseColor = vec3(0.04f, 0.16f, 0.15f);
const vec3 skyBaseColor = vec3(0.4f, 0.77f, 0.95f);

// ===================== Animation =================== //
vec2 getYawPitch() {
    return vec2(vals.z * 12.0, sin(vals.z) * 20.0 + 45.0);
}

vec3 getOrigin() {
    return vec3(
        sin(vals.z / 2.0) * 4.0,
        1.5 + (sin(vals.z)),
        cos(vals.z / 2.0) * 4.0
    );
}
// =================================================== //

// =============== Ray-marching utilties ============= //
// Calculates the intersection between a ray and a plane.
// Ported from https://stackoverflow.com/a/53437900/13153269
vec3 getPlaneIntersection(vec3 rayP, vec3 rayD, vec3 planeP, vec3 planeN) {
	float d = dot(planeP, -planeN);
	float t = -(d + dot(rayP, planeN)) / dot(rayD, planeN);
	return rayP + (rayD * t);
}

mat3 rotationMatrix(vec3 axis, float angle) {
    float s = sin(angle);
    float c = cos(angle);
    float oc = 1.0 - c;
    return mat3(
        oc * axis.x * axis.x + c, oc * axis.x * axis.y - axis.z * s, oc * axis.z * axis.x + axis.y * s, 
        oc * axis.x * axis.y + axis.z * s, oc * axis.y * axis.y + c, oc * axis.y * axis.z - axis.x * s, 
        oc * axis.z * axis.x - axis.y * s, oc * axis.y * axis.z + axis.x * s, oc * axis.z * axis.z + c
    );
}

// Gets the normalized direction of a ray pointing forward, beginning
// from the camera's origin, using the normalized UV screen coordinates.
vec3 getRayDirection(vec2 uv) {
    vec2 ndc = (uv * 2.0f) - 1.0f; // [-1.0; 1.0]
    ndc.x *= (vals.x / vals.y);
    vec3 forward = normalize(vec3(ndc, 1.5));
    
    vec2 angles = getYawPitch();
    
    return rotationMatrix(vec3(0.0, -1.0, 0.0), ((angles.x / 180.0) * 2.0 - 1.0)) 
         * rotationMatrix(vec3(1.0, 0.0, 0.0), ((angles.y / 180.0) * 2.0 - 1.0))
         * forward;
}

// Computes the sky color.
vec3 getSkyColor(vec3 rd) {
    return skyBaseColor - rd.y * 0.4; // keep it simple, silly!
}

float computeGerstnerWaves(vec2 pos, float t, float steepness, float amplitude, float lambda, vec2 dir) {
    const float g = 9.81;

	float k = (2.0 * 3.14) / lambda;
    float x = (sqrt(g * k)) * t - k * dot(dir, pos);

    return amplitude * pow(sin(x) * 0.5 + 0.5, steepness);
}

// Gets the height of the water at (pos).
float getWaterHeightAt(vec2 position, int iterations) {
    float height = 0.0;

    float time      = vals.z * 0.5;
    float steepness = 3.0;
    float amplitude = 1.65;
    float lambda    = 8.0;

    const float angle   = (3.14 * 2.0) * 0.15;
	const mat2 rotation = mat2(cos(angle), -sin(angle), sin(angle), cos(angle));

    const vec2 beginDir = vec2(sin(0.9), cos(1.2));
    vec2 dir = beginDir;

    for(int i = 0; i < iterations; i++) {
        height += computeGerstnerWaves(position, time, steepness, amplitude, lambda, dir);

        steepness *= 1.04;
        amplitude *= 0.82;
        lambda    *= 0.85;
        dir *= rotation;
    }

    return height / 8.0;
} 

vec3 getWaterNormal(vec2 pos) {
    float p0 = getWaterHeightAt(pos.xy, 32) * 1.0; // center
    float p1 = getWaterHeightAt(pos.xy - vec2(0.01, 0), 32) * 1.0; // left neighbor
    float p2 = getWaterHeightAt(pos.xy + vec2(0, 0.01), 32) * 1.0; // bottom neighbor
    vec3 a = vec3(pos.x, p0, pos.y);

    return normalize(cross(a - vec3(pos.x - 0.01, p1, pos.y), a - vec3(pos.x, p2, pos.y + 0.01)));
}

// Returns the water intersection position on the given ray (line).
vec3 raymarch(vec3 rayStart, vec3 rayEnd) {
    vec3 pos = rayStart;
    vec3 dir = normalize(rayEnd - rayStart);
    for (int i = 0; i < 32; i++) {
        // Sample the water height-map. This in theory could be just a perlin
        // noise texture, but we're using gerstner waves for our purposes instead
        float y = getWaterHeightAt(pos.xz, 16) * 1.0 - 1.0;
        if (y + 0.01 > pos.y) {
            return pos;
        }

        pos += dir * (pos.y - y);
    }

    return rayStart;
}

vec3 aces(vec3 x) {
    const float a = 2.31; // brightness
    const float b = 0.07;
    const float c = 2.4; // gamma
    const float d = 0.54;
    const float e = 0.04;
    return clamp((x * (a * x + b)) / (x * (c * x + d) + e), 0.0, 1.0);
}

vec3 applyTonemapping(vec3 color) {  
    return aces(color * 2.0);
}

vec3 getSunDirection() {
  return normalize(
      vec3(
          sin(2.0 * 3.14),
          0.05 + vals.z * 0.02,
          cos(2.0 * 3.14)
      )
  );
}

vec3 getSun(vec3 dir) { 
    float baseScalar = pow(max(0.0, dot(dir, getSunDirection())), 720.0);
    vec3 base = vec3(baseScalar, baseScalar, baseScalar);

    vec3 sun = smoothstep(0.5, 1.0, base * 40.0);
    vec3 haze = base * 60.0;
    return sun + haze;
}

void main() {
    vec2 uv = gl_FragCoord.xy / vals.xy;
    vec3 rayDir = getRayDirection(uv);
    vec3 origin = getOrigin(); 

    vec3 fColor; // final color

    if (rayDir.y >= 0.0) {
        // Ray is pointing upwards - render the sky.
        fColor = getSkyColor(rayDir) + getSun(rayDir);
    } else {
        // Ray is pointing downwards - render the water.

        // Calculate the intersection between the low and high planes.
        vec3 hp = getPlaneIntersection(origin, rayDir, vec3(0.0, 0.0, 0.0), vec3(0.0, 1.0, 0.0));
        vec3 lp = getPlaneIntersection(origin, rayDir, vec3(0.0, -1.0, 0.0), vec3(0.0, 1.0, 0.0));

        // Raymarch through the water plane, using the water height-map function (getWaterHeightAt).
        vec3 wpos = raymarch(hp, lp);
        float dist = distance(origin, wpos); 

        vec3 norm = getWaterNormal(wpos.xz);
        norm = mix(norm, vec3(0.0, 1.0, 0.0), 0.7 * min(1.0, sqrt(dist * 0.01) * 1.1));
        
        float fresnel = (0.04 + (1.0 - 0.04) * (pow(1.0 - max(0.0, dot(-norm, rayDir)), 5.0)));
        vec3 reflectDir = normalize(reflect(rayDir, norm));
        reflectDir.y = abs(reflectDir.y);

        vec3 reflectColor = getSkyColor(reflectDir) + getSun(reflectDir);
        vec3 waterColor = waterBaseColor * (0.2 + (wpos.y + 1.0) / 1.0);

        fColor = fresnel * reflectColor + (1.0 - fresnel) * waterColor;
    }
    
    vec2 uvf = uv * (1.0 - uv.yx);
    float vig = uvf.x * uvf.y * 32.0;
    vig = pow(vig, 0.5);
    
    fColor *= vig;
    FragColor = vec4(applyTonemapping(fColor), 1.0);
}