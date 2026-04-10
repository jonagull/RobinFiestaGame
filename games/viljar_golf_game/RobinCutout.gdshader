shader_type canvas_item;

void fragment() {
    vec4 col = texture(TEXTURE, UV);
    float brightness = dot(col.rgb, vec3(0.299, 0.587, 0.114));
    float alpha = smoothstep(0.08, 0.22, brightness);
    COLOR = vec4(col.rgb, col.a * alpha);
}
