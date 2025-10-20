# Project 3.1: Textured Quad - Load and Render an Image

This project teaches you how to load an image (PNG/JPG) and render it on a 2D quad using OpenGL textures with Silk.NET and StbImageSharp.

## What You’ll Learn

- Loading images via StbImageSharp
- Creating an OpenGL texture (glTexImage2D, mipmaps, filters, wraps)
- Adding UVs (texture coordinates) to vertices
- Writing shaders to sample `sampler2D`
- Orthographic projection for 2D rendering

## How It Works (High-Level)

1. Define a rectangle (two triangles) with positions and UVs
2. Load a PNG/JPG into CPU memory
3. Upload pixel data to GPU as an OpenGL texture
4. Configure texture sampling (filters/wraps)
5. Draw the quad and sample the texture in fragment shader

## Files

- `Program.cs`: Loads texture, sets up VBO/VAO/EBO, renders quad
- `Shaders/shader.vert`: Forwards position → clip space, passes UVs
- `Shaders/shader.frag`: Samples the texture using `sampler2D`

## Place a Sample Image

Put a test image here (create folder if needed):

```
src/Phase3_2D/3.1_TexturedQuad/textures/sample.png
```

(PNG/JPG supported)

If no file is present, the program still runs but the quad will appear black.

## Vertex Layout

Interleaved: position (x,y,z) + texcoord (u,v)

```
-0.5,  0.5, 0,   0, 1   // top-left
-0.5, -0.5, 0,   0, 0   // bottom-left
 0.5, -0.5, 0,   1, 0   // bottom-right
 0.5,  0.5, 0,   1, 1   // top-right
```

Indices (EBO):

```
0,1,2, 0,2,3
```

## Shader Overview

### Vertex (`shader.vert`)

- Inputs: `aPosition (vec3)`, `aTexCoord (vec2)`
- Uniform: `uProjection (mat4)` – orthographic projection
- Output: `vTexCoord (vec2)` to fragment shader

```
position → (projection × position) → gl_Position
       UV → vTexCoord
```

### Fragment (`shader.frag`)

- Input: `vTexCoord (vec2)`
- Uniform: `uTexture0 (sampler2D)`
- Output: `FragColor = texture(uTexture0, vTexCoord)`

## Texture Parameters Used

- Minification: `LinearMipmapLinear`
- Magnification: `Linear`
- Wrap S/T: `Repeat`
- Mipmaps: `glGenerateMipmap`

These give smooth scaling and repeating behavior.

## Projection (2D Ortho)

We create an orthographic projection matching the aspect ratio:

- x ∈ [−aspect, +aspect]
- y ∈ [−1, +1]

So the quad spans consistent size regardless of window resolution.

## Run

```bash
cd src/Phase3_2D/3.1_TexturedQuad
# place textures/sample.png
dotnet run
```

## Experiments

1. Change min/mag filters to `Nearest` for pixelated look.
2. Try `ClampToEdge` wrapping and UVs > 1.0 to see the difference.
3. Load different images (PNG/JPG), try transparent PNGs.
4. Animate UVs (add offset uniform and add to `vTexCoord`).
5. Scale/translate the quad by changing the vertex positions.

## Common Issues

- Black quad: texture not found (ensure `textures/sample.png` exists), or wrong sampler/active texture setup.
- Inverted image: Some loaders store origin top-left; if needed, flip V (`stbi_set_flip_vertically_on_load` equivalent not used here; flip UVs in CPU instead).

## Next

- 3.2: Sprite Animation – Use sprite sheets and animate frames by changing UV rects over time.
