#version 330 core

// ============================================================================
// FRAGMENT SHADER - Same as Project 2.2!
// ============================================================================
//
// The fragment shader doesn't care if the triangle is rotating!
// It just receives the interpolated color and uses it.
//
// The rotation happens in the VERTEX SHADER (previous stage)
// By the time we get here, everything is already transformed
// ============================================================================

// Input from vertex shader (interpolated)
in vec3 vertexColor;

// Output to screen
out vec4 FragColor;

void main()
{
    // Simply use the interpolated color
    FragColor = vec4(vertexColor, 1.0);
    
    // IMPORTANT NOTE:
    // Even though the triangle is rotating,
    // this shader code doesn't change!
    //
    // Why? Because rotation is a GEOMETRIC transformation,
    // handled by the vertex shader.
    //
    // The fragment shader only cares about COLORING pixels,
    // not WHERE those pixels are!
    //
    // SEPARATION OF CONCERNS:
    // - Vertex Shader: Geometry (position, rotation, etc.)
    // - Fragment Shader: Appearance (color, lighting, etc.)
}

// ============================================================================
// THE BEAUTY OF THE PIPELINE
// ============================================================================
//
// You can change HOW things are positioned/transformed
// WITHOUT changing how they are colored!
//
// This is why shaders are so powerful:
// - Same vertex shader can work with different fragment shaders
// - Same fragment shader can work with different vertex shaders
// - Mix and match for different effects!
//
// Example combinations:
// - Rotating transform + solid color
// - Rotating transform + gradient color (this project!)
// - Rotating transform + texture (later!)
// - Rotating + scaling + moving + lit + textured (advanced!)
// ============================================================================

