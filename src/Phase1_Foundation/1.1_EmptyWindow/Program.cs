using Silk.NET.Windowing;
using Silk.NET.OpenGL;

// ============================================================================
// PROJECT 1.1: EMPTY WINDOW
// ============================================================================
// This is your first OpenGL program! It creates a window and keeps it open.
// 
// WHAT YOU'LL LEARN:
// - How to create a window with Silk.NET
// - The basic structure of an OpenGL application
// - The render loop (load → update → render → close)
// ============================================================================

namespace EmptyWindow;

class Program
{
    // Window instance - this represents our application window
    private static IWindow? window;
    
    // OpenGL context - this is how we communicate with the GPU
    private static GL? gl;
    
    static void Main(string[] args)
    {
        Console.WriteLine("Starting OpenGL Application...");
        Console.WriteLine("Phase 1.1: Empty Window");
        Console.WriteLine("========================\n");
        
        // ====================================================================
        // STEP 1: CONFIGURE THE WINDOW
        // ====================================================================
        // We set up window options like size, title, and API version
        var options = WindowOptions.Default;
        options.Size = new Silk.NET.Maths.Vector2D<int>(800, 600);  // Window size: 800x600 pixels
        options.Title = "1.1 - My First OpenGL Window!";             // Window title bar text
        
        // ====================================================================
        // STEP 2: CREATE THE WINDOW
        // ====================================================================
        window = Window.Create(options);
        
        // ====================================================================
        // STEP 3: REGISTER EVENT HANDLERS
        // ====================================================================
        // These are callback functions that get called at different stages
        
        // OnLoad: Called once when the window is first created
        window.Load += OnLoad;
        
        // OnUpdate: Called every frame before rendering (for game logic)
        window.Update += OnUpdate;
        
        // OnRender: Called every frame to draw to the screen
        window.Render += OnRender;
        
        // OnClosing: Called when the window is about to close
        window.Closing += OnClosing;
        
        // ====================================================================
        // STEP 4: START THE APPLICATION
        // ====================================================================
        // This starts the event loop and keeps the window open
        // The window will stay open until you close it
        window.Run();
        
        Console.WriteLine("\nApplication closed successfully!");
    }
    
    // ========================================================================
    // EVENT: ONLOAD
    // ========================================================================
    // This is called ONCE when the window is created
    // Perfect place to initialize OpenGL and load resources
    // ========================================================================
    private static void OnLoad()
    {
        Console.WriteLine("[OnLoad] Window is loading...");
        
        // Get the OpenGL context from the window
        // GL is the object we use to call OpenGL functions
        gl = window!.CreateOpenGL();
        
        Console.WriteLine($"[OnLoad] OpenGL Version: {gl.GetStringS(StringName.Version)}");
        Console.WriteLine($"[OnLoad] Graphics Card: {gl.GetStringS(StringName.Renderer)}");
        Console.WriteLine($"[OnLoad] OpenGL Vendor: {gl.GetStringS(StringName.Vendor)}");
        Console.WriteLine("[OnLoad] Window loaded successfully!\n");
    }
    
    // ========================================================================
    // EVENT: ONUPDATE
    // ========================================================================
    // This is called every frame BEFORE rendering
    // Delta time (dt) tells us how much time has passed since the last frame
    // Use this for game logic, physics, input handling, etc.
    // ========================================================================
    private static void OnUpdate(double deltaTime)
    {
        // For now, we don't need to do anything here
        // In future projects, we'll add input handling and game logic here
        
        // Uncomment this to see how fast your application is running:
         Console.WriteLine($"[OnUpdate] Delta Time: {deltaTime:F4} seconds ({1.0 / deltaTime:F1} FPS)");
    }
    
    // ========================================================================
    // EVENT: ONRENDER
    // ========================================================================
    // This is called every frame to draw to the screen
    // All your drawing code goes here!
    // ========================================================================
    private static void OnRender(double deltaTime)
    {
        // ====================================================================
        // CLEAR THE SCREEN
        // ====================================================================
        // Before drawing anything, we clear the screen
        // This prevents old frames from showing through
        
        // Set the clear color (RGBA: Red, Green, Blue, Alpha)
        // Values range from 0.0 to 1.0
        // This creates a dark blue background
        gl!.ClearColor(0.1f, 0.15f, 0.2f, 1.0f);
        
        // Actually clear the screen with the color we just set
        // ColorBufferBit means we're clearing the color buffer
        gl.Clear(ClearBufferMask.ColorBufferBit);
        
        // ====================================================================
        // DRAWING HAPPENS HERE
        // ====================================================================
        // For now, we're just clearing the screen
        // In the next projects, we'll draw shapes here!
    }
    
    // ========================================================================
    // EVENT: ONCLOSING
    // ========================================================================
    // This is called when the window is about to close
    // Clean up resources here (delete buffers, textures, etc.)
    // ========================================================================
    private static void OnClosing()
    {
        Console.WriteLine("\n[OnClosing] Cleaning up resources...");
        
        // Dispose of the OpenGL context
        gl?.Dispose();
        
        Console.WriteLine("[OnClosing] Cleanup complete!");
    }
}

// ============================================================================
// CONGRATULATIONS! 🎉
// ============================================================================
// You've created your first OpenGL window!
// 
// WHAT'S HAPPENING:
// 1. We create a window
// 2. We initialize OpenGL
// 3. Every frame, we:
//    - Update game logic (OnUpdate)
//    - Clear the screen with a color (OnRender)
//    - Display the result
// 4. When you close the window, we clean up
// 
// NEXT STEPS:
// - Try changing the window size
// - Try changing the clear color (make it red, green, or any color!)
// - Try printing the FPS in OnUpdate
// 
// In Project 1.2, we'll make the color change over time!
// ============================================================================
