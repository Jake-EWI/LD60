shader_type canvas_item;

uniform float heat : hint_range(0.0, 1.0) = 0.0;
uniform vec2 hot_spot = vec2(0.5, 0.5);
uniform sampler2D palette : repeat_disable;

void fragment() {
    vec4 tex = texture(TEXTURE, UV);
    float lum = dot(tex.rgb, vec3(0.299, 0.587, 0.114));
    float spot = heat * (1.0 - smoothstep(0.0, 0.5, distance(UV, hot_spot)));
    float shimmer = sin(TIME * 3.0 + UV.y * 20.0) * 0.02;
    float t = clamp(lum * 0.3 + spot * 0.7 + shimmer, 0.0, 1.0);
    COLOR = vec4(texture(palette, vec2(t, 0.5)).rgb, tex.a);
}
