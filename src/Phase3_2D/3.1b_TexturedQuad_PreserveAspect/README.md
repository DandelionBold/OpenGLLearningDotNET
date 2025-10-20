# 3.1b: Textured Quad (Preserve Aspect)

This variant keeps the original image aspect ratio by scaling the quad in X using a `uModel` matrix where scaleX = textureWidth/textureHeight.

## Key ideas
- Same UVs as 3.1/3.1a; we change geometry via a model transform.
- `uProjection * uModel * vec4(position,1)` in the vertex shader.
- Linear filtering + mipmaps.

## UV interpolation (visual)
Interpolated UVs form smooth gradients; sampling the texture at those UVs gives correct pixels without distortion when the quad matches the image aspect.

## Run
```bash
cd src/Phase3_2D/3.1b_TexturedQuad_PreserveAspect
# ensure textures/sample.png exists
dotnet run
```

## Try
- Replace `textureAspect` with 1.0 to see stretching return.
- Change min/mag filters to observe sharp vs smooth results.


