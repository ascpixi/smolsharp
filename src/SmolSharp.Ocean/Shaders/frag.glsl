// This is a port of my ocean shader @ https://www.shadertoy.com/view/DtsBzX.

// Stored as a single vec3 in order to minimize code size in the host
uniform vec3 uniformValues;

#define TIME          uniformValues.z
#define ASPECT_RATIO  (uniformValues.x / uniformValues.y)
#define RESOLUTION    uniformValues.xy

// === Settings
#define RAYMARCH_ITERATIONS   32
#define WATER_ITERATIONS      16
#define WATER_ITERATIONS_NORM 32

#define WAVE_AMPLITUDE 1.65
#define WAVE_STEEPNESS 3.0
#define WAVE_LENGTH	   8.0
#define WAVE_SPEED     0.5
#define WAVE_DIVISOR   8.0
#define WAVE_D_STEEPNESS 1.04
#define WAVE_D_AMPLITUDE 0.82
#define WAVE_D_LAMBDA    0.85


#define FOV           1.5f
#define PI            3.141592653589793238 // replace when minimizing!
#define WATER_HEIGHT  1.0f
#define UP            vec3(0.0f, 1.0f, 0.0f)
#define ONE           vec3(1.0f, 1.0f, 1.0f)

const vec3 waterBaseColor = vec3(0.04f, 0.16f, 0.15f);
const vec3 skyBaseColor = vec3(0.4f, 0.77f, 0.95f);

// ===================== Animation =================== //
vec2 getYawPitch() {
    return vec2(TIME * 12.0, sin(TIME) * 20.0 + 45.0);
}

vec3 getOrigin() {
    return vec3(
        sin(TIME / 2.0) * 4.0,
        1.5 + (sin(TIME)),
        cos(TIME / 2.0) * 4.0
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
    ndc.x *= ASPECT_RATIO;
    vec3 forward = normalize(vec3(ndc, FOV));
    
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

	float k = (2.0 * PI) / lambda;
    float x = (sqrt(g * k)) * t - k * dot(dir, pos);

    return amplitude * pow(sin(x) * 0.5 + 0.5, steepness);
}

// Gets the height of the water at (pos).
float getWaterHeightAt(vec2 position, int iterations) {
    float height = 0.0;

    float time      = TIME * WAVE_SPEED;
    float steepness = WAVE_STEEPNESS;
    float amplitude = WAVE_AMPLITUDE;
    float lambda    = WAVE_LENGTH;

    const float angle   = (PI * 2.0) * 0.15;
	const mat2 rotation = mat2(cos(angle), -sin(angle), sin(angle), cos(angle));

    const vec2 beginDir = vec2(sin(0.9), cos(1.2));
    vec2 dir = beginDir;

    for(int i = 0; i < iterations; i++) {
        height += computeGerstnerWaves(position, time, steepness, amplitude, lambda, dir);

        steepness *= WAVE_D_STEEPNESS;
        amplitude *= WAVE_D_AMPLITUDE;
        lambda    *= WAVE_D_LAMBDA;
        dir *= rotation;
    }

    return height / WAVE_DIVISOR;
} 

vec3 getWaterNormal(vec2 pos) {
    float p0 = getWaterHeightAt(pos.xy, WATER_ITERATIONS_NORM) * WATER_HEIGHT; // center
    float p1 = getWaterHeightAt(pos.xy - vec2(0.01, 0), WATER_ITERATIONS_NORM) * WATER_HEIGHT; // left neighbor
    float p2 = getWaterHeightAt(pos.xy + vec2(0, 0.01), WATER_ITERATIONS_NORM) * WATER_HEIGHT; // bottom neighbor
    vec3 a = vec3(pos.x, p0, pos.y);

    return normalize(cross(a - vec3(pos.x - 0.01, p1, pos.y), a - vec3(pos.x, p2, pos.y + 0.01)));
}

// Returns the water intersection position on the given ray (line).
vec3 raymarch(vec3 rayStart, vec3 rayEnd) {
    vec3 pos = rayStart;
    vec3 dir = normalize(rayEnd - rayStart);
    for (int i = 0; i < RAYMARCH_ITERATIONS; i++) {
        // Sample the water height-map. This in theory could be just a perlin
        // noise texture, but we're using gerstner waves for our purposes instead
        float y = getWaterHeightAt(pos.xz, WATER_ITERATIONS) * WATER_HEIGHT - WATER_HEIGHT;
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
          sin(2.0 * PI),
          0.05 + TIME * 0.02,
          cos(2.0 * PI)
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

void mainImage( out vec4 fragColor, in vec2 fragCoord ) {
    vec2 uv = fragCoord / RESOLUTION;
    vec3 rayDir = getRayDirection(uv);
    vec3 origin = getOrigin(); 

    vec3 fColor; // final color

    if (rayDir.y >= 0.0) {
        // Ray is pointing upwards - render the sky.
        fColor = getSkyColor(rayDir) + getSun(rayDir);
    } else {
        // Ray is pointing downwards - render the water.

        // Calculate the intersection between the low and high planes.
        vec3 hp = getPlaneIntersection(origin, rayDir, vec3(0.0, 0.0, 0.0), UP);
        vec3 lp = getPlaneIntersection(origin, rayDir, vec3(0.0, -WATER_HEIGHT, 0.0), UP);

        // Raymarch through the water plane, using the water height-map function (getWaterHeightAt).
        vec3 wpos = raymarch(hp, lp);
        float dist = distance(origin, wpos); 

        vec3 norm = getWaterNormal(wpos.xz);
        norm = mix(norm, vec3(0.0, 1.0, 0.0), 0.7 * min(1.0, sqrt(dist * 0.01) * 1.1));
        
        float fresnel = (0.04 + (1.0 - 0.04) * (pow(1.0 - max(0.0, dot(-norm, rayDir)), 5.0)));
        vec3 reflectDir = normalize(reflect(rayDir, norm));
        reflectDir.y = abs(reflectDir.y);

        vec3 reflectColor = getSkyColor(reflectDir) + getSun(reflectDir);
        vec3 waterColor = waterBaseColor * (0.2 + (wpos.y + WATER_HEIGHT) / WATER_HEIGHT);

        fColor = fresnel * reflectColor + (1.0 - fresnel) * waterColor;
    }
    
    vec2 uvf = uv * (1.0 - uv.yx);
    float vig = uvf.x * uvf.y * 32.0;
    vig = pow(vig, 0.5);
    
    fColor *= vig;
    fragColor = vec4(applyTonemapping(fColor), 1.0);
}