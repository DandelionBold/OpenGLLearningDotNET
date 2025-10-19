using Silk.NET.Windowing;
using Silk.NET.OpenGL;
using Silk.NET.Input;
using System;

// ============================================================================
// PROJECT 1.2: CHANGING BACKGROUND COLORS
// ============================================================================
// This project builds on 1.1 by adding animated, time-based color changes!
// 
// WHAT YOU'LL LEARN:
// - How to use deltaTime for animations
// - Color interpolation and smooth transitions
// - Math functions for creating effects (sin, cos)
// - Time accumulation in the render loop
// ============================================================================

namespace ChangingColors;

class Program
{
    private static IWindow? window;
    private static GL? gl;
    private static IInputContext? inputContext;
    
    // ========================================================================
    // TIME TRACKING
    // ========================================================================
    // We accumulate time to create animations
    private static float totalTime = 0.0f;
    
    // ========================================================================
    // ANIMATION MODES
    // ========================================================================
    // We'll demonstrate different color animation techniques
    private static int animationMode = 0;
    private const int MAX_MODES = 5;
    
    // Track if space was pressed last frame (to prevent multiple triggers)
    private static bool spaceWasPressedLastFrame = false;
    
    static void Main(string[] args)
    {
        Console.WriteLine("Starting OpenGL Application...");
        Console.WriteLine("Phase 1.2: Changing Background Colors");
        Console.WriteLine("======================================\n");
        Console.WriteLine("CONTROLS:");
        Console.WriteLine("  SPACE - Change animation mode");
        Console.WriteLine("  ESC   - Close window\n");
        
        var options = WindowOptions.Default;
        options.Size = new Silk.NET.Maths.Vector2D<int>(800, 600);
        options.Title = "1.2 - Changing Background Colors";
        
        window = Window.Create(options);
        
        window.Load += OnLoad;
        window.Update += OnUpdate;
        window.Render += OnRender;
        window.Closing += OnClosing;
        
        window.Run();
        
        Console.WriteLine("\nApplication closed successfully!");
    }
    
    private static void OnLoad()
    {
        Console.WriteLine("[OnLoad] Initializing OpenGL...");
        gl = window!.CreateOpenGL();
        inputContext = window.CreateInput();
        
        Console.WriteLine($"[OnLoad] OpenGL Version: {gl.GetStringS(StringName.Version)}");
        Console.WriteLine($"[OnLoad] Graphics Card: {gl.GetStringS(StringName.Renderer)}\n");
        
        Console.WriteLine("[OnLoad] Starting with Animation Mode 0: Rainbow Cycle");
        Console.WriteLine("[OnLoad] Press SPACE to change modes!\n");
    }
    
    private static void OnUpdate(double deltaTime)
    {
        // ====================================================================
        // ACCUMULATE TIME
        // ====================================================================
        // We add deltaTime each frame to track total elapsed time
        // This is the foundation of time-based animations!
        totalTime += (float)deltaTime;
        
        // ====================================================================
        // INPUT HANDLING (Preview for Project 1.3!)
        // ====================================================================
        // Check if SPACE key is pressed to change animation modes
        if (inputContext != null && inputContext.Keyboards.Count > 0)
        {
            var keyboard = inputContext.Keyboards[0];
            
            bool spacePressed = keyboard.IsKeyPressed(Key.Space);
            
            // Only trigger once per key press (not every frame while held)
            if (spacePressed && !spaceWasPressedLastFrame)
            {
                animationMode = (animationMode + 1) % MAX_MODES;
                Console.WriteLine($"\n[Mode Changed] Animation Mode {animationMode}: {GetModeName(animationMode)}");
                totalTime = 0.0f; // Reset time for new animation
            }
            
            spaceWasPressedLastFrame = spacePressed;
            
            // ESC to close
            if (keyboard.IsKeyPressed(Key.Escape))
            {
                window!.Close();
            }
        }
    }
    
    private static void OnRender(double deltaTime)
    {
        // ====================================================================
        // GET THE ANIMATED COLOR
        // ====================================================================
        // Based on the current mode, calculate the background color
        var color = GetAnimatedColor(animationMode, totalTime);
        
        // ====================================================================
        // CLEAR THE SCREEN WITH THE ANIMATED COLOR
        // ====================================================================
        gl!.ClearColor(color.R, color.G, color.B, color.A);
        gl.Clear(ClearBufferMask.ColorBufferBit);
    }
    
    private static void OnClosing()
    {
        Console.WriteLine("\n[OnClosing] Cleaning up resources...");
        inputContext?.Dispose();
        gl?.Dispose();
        Console.WriteLine("[OnClosing] Cleanup complete!");
    }
    
    // ========================================================================
    // ANIMATION FUNCTIONS
    // ========================================================================
    // These functions demonstrate different ways to animate colors
    // ========================================================================
    
    private static (float R, float G, float B, float A) GetAnimatedColor(int mode, float time)
    {
        return mode switch
        {
            0 => RainbowCycle(time),
            1 => PulsingRed(time),
            2 => SmoothTransition(time),
            3 => FastFlicker(time),
            4 => WavePattern(time),
            _ => (1.0f, 1.0f, 1.0f, 1.0f) // White fallback
        };
    }
    
    private static string GetModeName(int mode)
    {
        return mode switch
        {
            0 => "Rainbow Cycle",
            1 => "Pulsing Red",
            2 => "Smooth Transition (Blue to Yellow)",
            3 => "Fast Flicker",
            4 => "Wave Pattern",
            _ => "Unknown"
        };
    }
    
    // ========================================================================
    // MODE 0: RAINBOW CYCLE
    // ========================================================================
    // Creates a smooth rainbow effect using sine waves
    // Each color channel (R, G, B) oscillates at different phases
    // ========================================================================
    private static (float R, float G, float B, float A) RainbowCycle(float time)
    {
        // Sin waves oscillate between -1 and 1
        // We shift and scale to get 0 to 1 for colors
        float speed = 1.0f; // How fast the rainbow cycles
        
        float r = (float)(Math.Sin(time * speed) * 0.5 + 0.5);
        float g = (float)(Math.Sin(time * speed + 2.0) * 0.5 + 0.5);
        float b = (float)(Math.Sin(time * speed + 4.0) * 0.5 + 0.5);
        
        return (r, g, b, 1.0f);
    }
    
    // ========================================================================
    // MODE 1: PULSING RED
    // ========================================================================
    // Demonstrates a single color pulsing in intensity
    // Useful for effects like damage indicators, warnings, etc.
    // ========================================================================
    private static (float R, float G, float B, float A) PulsingRed(float time)
    {
        float speed = 2.0f; // Pulse twice per second
        
        // Pulse the red channel between 0.3 and 1.0
        float intensity = (float)(Math.Sin(time * speed) * 0.35 + 0.65);
        
        return (intensity, 0.0f, 0.0f, 1.0f);
    }
    
    // ========================================================================
    // MODE 2: SMOOTH TRANSITION
    // ========================================================================
    // Smoothly transitions between two colors using linear interpolation (lerp)
    // This is the foundation of color blending in games!
    // ========================================================================
    private static (float R, float G, float B, float A) SmoothTransition(float time)
    {
        // Define two colors
        float[] colorA = { 0.1f, 0.2f, 0.8f }; // Blue
        float[] colorB = { 0.9f, 0.9f, 0.2f }; // Yellow
        
        // Create a ping-pong effect: 0 → 1 → 0 → 1 ...
        float speed = 0.5f;
        float t = (float)(Math.Sin(time * speed) * 0.5 + 0.5);
        
        // Linear interpolation (lerp) between the two colors
        // Formula: result = colorA + (colorB - colorA) * t
        float r = Lerp(colorA[0], colorB[0], t);
        float g = Lerp(colorA[1], colorB[1], t);
        float b = Lerp(colorA[2], colorB[2], t);
        
        return (r, g, b, 1.0f);
    }
    
    // ========================================================================
    // MODE 3: FAST FLICKER
    // ========================================================================
    // Rapid color changes - demonstrates high-frequency animations
    // ========================================================================
    private static (float R, float G, float B, float A) FastFlicker(float time)
    {
        float speed = 10.0f; // Very fast!
        
        float r = (float)(Math.Sin(time * speed) * 0.5 + 0.5);
        float g = (float)(Math.Sin(time * speed * 1.3) * 0.5 + 0.5);
        float b = (float)(Math.Sin(time * speed * 0.7) * 0.5 + 0.5);
        
        return (r, g, b, 1.0f);
    }
    
    // ========================================================================
    // MODE 4: WAVE PATTERN
    // ========================================================================
    // Combines multiple frequencies for complex color patterns
    // Uses both sin and cos for variety
    // ========================================================================
    private static (float R, float G, float B, float A) WavePattern(float time)
    {
        // Combine slow and fast waves for interesting patterns
        float slow = (float)(Math.Sin(time * 0.5) * 0.5 + 0.5);
        float fast = (float)(Math.Cos(time * 3.0) * 0.5 + 0.5);
        
        float r = slow;
        float g = fast;
        float b = (slow + fast) / 2.0f; // Mix of both
        
        return (r, g, b, 1.0f);
    }
    
    // ========================================================================
    // UTILITY FUNCTIONS
    // ========================================================================
    
    /// <summary>
    /// Linear interpolation between two values
    /// When t = 0, returns a
    /// When t = 1, returns b
    /// When t = 0.5, returns the midpoint
    /// </summary>
    private static float Lerp(float a, float b, float t)
    {
        return a + (b - a) * t;
    }
}

// ============================================================================
// CONGRATULATIONS! 🎨
// ============================================================================
// You've learned how to animate colors using time!
// 
// KEY CONCEPTS YOU NOW KNOW:
// 
// 1. DELTA TIME:
//    - Time between frames
//    - Used to make animations frame-rate independent
//    - Accumulate it to track total elapsed time
// 
// 2. SINE AND COSINE:
//    - Create smooth, repeating animations
//    - Range from -1 to 1
//    - Add phase offsets for variety (+ 2.0, + 4.0, etc.)
// 
// 3. LERP (Linear Interpolation):
//    - Smoothly blend between two values
//    - Formula: a + (b - a) * t
//    - t = 0 gives a, t = 1 gives b, t = 0.5 gives middle
// 
// 4. TIME-BASED ANIMATIONS:
//    - Accumulate deltaTime to track total time
//    - Use math functions (sin, cos) with time
//    - Control speed by multiplying time
// 
// EXPERIMENTS TO TRY:
// 
// 1. Change the speed values in each mode
// 2. Create your own animation mode!
// 3. Try different color combinations in SmoothTransition
// 4. Combine multiple sine waves for unique patterns
// 5. Add more animation modes (try exponential, square waves, etc.)
// 
// NEXT UP: Project 1.3 - Input Handling
// We'll learn proper keyboard and mouse input for interactive applications!
// ============================================================================
