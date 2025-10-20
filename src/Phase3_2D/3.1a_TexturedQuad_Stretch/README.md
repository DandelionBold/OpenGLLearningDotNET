# 3.1a: Textured Quad (Stretch)

This variant draws the image on a square quad, so non‑square images appear stretched. It demonstrates that textures are mapped to whatever shape you render.

## Key ideas
- Texture UVs (0..1) interpolate across the triangles.
- The quad is square; the texture fits the quad (may stretch).
- Linear filtering with mipmaps for smooth scaling.
- Ortho projection keeps consistent scale.

## Gradients and interpolation
The GPU interpolates UVs across the triangle surface. Visualizing UVs produces a gradient; sampling a texture at those UVs produces the image.

## Run
```bash
cd src/Phase3_2D/3.1a_TexturedQuad_Stretch
# ensure textures/sample.png exists
dotnet run
```

## Try
- Use a very wide image to see squashing.
- Switch magnification filter to `Nearest`.
- Flip UVs to explore orientation.


