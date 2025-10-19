using Silk.NET.Windowing;
using Silk.NET.OpenGL;
using Silk.NET.Input;
using Silk.NET.Maths;
using System;
using System.Numerics;

// ============================================================================
// PROJECT 1.3: INPUT HANDLING
// ============================================================================
// This project teaches comprehensive input handling in Silk.NET!
// 
// WHAT YOU'LL LEARN:
// - Keyboard input (key presses and holds)
// - Mouse movement and position tracking
// - Mouse button clicks
// - Multiple input methods and best practices
// ============================================================================

namespace InputHandling;

class Program
{
    private static IWindow? window;
    private static GL? gl;
    private static IInputContext? inputContext;
    
    // ========================================================================
    // BACKGROUND COLOR (controlled by input)
    // ========================================================================
    private static float red = 0.2f;
    private static float green = 0.3f;
    private static float blue = 0.4f;
    
    // ========================================================================
    // MOUSE TRACKING
    // ========================================================================
    private static Vector2 mousePosition = new Vector2(0, 0);
    private static bool leftMouseDown = false;
    private static bool rightMouseDown = false;
    
    // ========================================================================
    // KEYBOARD STATE
    // ========================================================================
    private static bool shiftHeld = false;
    
    static void Main(string[] args)
    {
        Console.WriteLine("Starting OpenGL Application...");
        Console.WriteLine("Phase 1.3: Input Handling");
        Console.WriteLine("=========================\n");
        Console.WriteLine("KEYBOARD CONTROLS:");
        Console.WriteLine("  R - Increase Red channel");
        Console.WriteLine("  G - Increase Green channel");
        Console.WriteLine("  B - Increase Blue channel");
        Console.WriteLine("  SHIFT + R/G/B - Decrease channel");
        Console.WriteLine("  SPACE - Reset to default color");
        Console.WriteLine("  ESC - Close window\n");
        Console.WriteLine("MOUSE CONTROLS:");
        Console.WriteLine("  Move mouse - See position in console");
        Console.WriteLine("  Left Click - Print coordinates");
        Console.WriteLine("  Right Click - Randomize color\n");
        
        var options = WindowOptions.Default;
        options.Size = new Vector2D<int>(800, 600);
        options.Title = "1.3 - Input Handling Demo";
        
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
        Console.WriteLine("[OnLoad] Initializing OpenGL and Input...");
        gl = window!.CreateOpenGL();
        inputContext = window.CreateInput();
        
        Console.WriteLine($"[OnLoad] OpenGL Version: {gl.GetStringS(StringName.Version)}");
        Console.WriteLine($"[OnLoad] Input devices found:");
        Console.WriteLine($"  - Keyboards: {inputContext.Keyboards.Count}");
        Console.WriteLine($"  - Mice: {inputContext.Mice.Count}");
        Console.WriteLine($"  - Gamepads: {inputContext.Gamepads.Count}\n");
        
        // ====================================================================
        // REGISTER MOUSE EVENT HANDLERS
        // ====================================================================
        // Silk.NET supports both polling (checking in Update) and 
        // event-driven input. Events are great for clicks and key presses!
        
        if (inputContext.Mice.Count > 0)
        {
            var mouse = inputContext.Mice[0];
            
            // Mouse movement event
            mouse.MouseMove += (mouse, position) =>
            {
                mousePosition = position;
            };
            
            // Mouse button down event
            mouse.MouseDown += (mouse, button) =>
            {
                if (button == MouseButton.Left)
                {
                    leftMouseDown = true;
                    Console.WriteLine($"[Mouse] Left click at ({mousePosition.X:F0}, {mousePosition.Y:F0})");
                }
                else if (button == MouseButton.Right)
                {
                    rightMouseDown = true;
                    RandomizeColor();
                }
            };
            
            // Mouse button up event
            mouse.MouseUp += (mouse, button) =>
            {
                if (button == MouseButton.Left)
                    leftMouseDown = false;
                else if (button == MouseButton.Right)
                    rightMouseDown = false;
            };
        }
        
        // ====================================================================
        // REGISTER KEYBOARD EVENT HANDLERS
        // ====================================================================
        if (inputContext.Keyboards.Count > 0)
        {
            var keyboard = inputContext.Keyboards[0];
            
            // Key down event - fires once when key is first pressed
            keyboard.KeyDown += (keyboard, key, scancode) =>
            {
                switch (key)
                {
                    case Key.R:
                        if (shiftHeld)
                            red = Math.Max(0.0f, red - 0.1f);
                        else
                            red = Math.Min(1.0f, red + 0.1f);
                        PrintColor("Red");
                        break;
                        
                    case Key.G:
                        if (shiftHeld)
                            green = Math.Max(0.0f, green - 0.1f);
                        else
                            green = Math.Min(1.0f, green + 0.1f);
                        PrintColor("Green");
                        break;
                        
                    case Key.B:
                        if (shiftHeld)
                            blue = Math.Max(0.0f, blue - 0.1f);
                        else
                            blue = Math.Min(1.0f, blue + 0.1f);
                        PrintColor("Blue");
                        break;
                        
                    case Key.Space:
                        ResetColor();
                        break;
                        
                    case Key.Escape:
                        window!.Close();
                        break;
                }
            };
        }
        
        Console.WriteLine("[OnLoad] Ready! Try the controls listed above.\n");
    }
    
    private static void OnUpdate(double deltaTime)
    {
        // ====================================================================
        // POLLING INPUT (checking state every frame)
        // ====================================================================
        // This is useful for continuous actions like movement
        // Events (OnKeyDown) are better for one-time actions
        
        if (inputContext?.Keyboards.Count > 0)
        {
            var keyboard = inputContext.Keyboards[0];
            
            // Check if Shift is currently held down
            shiftHeld = keyboard.IsKeyPressed(Key.ShiftLeft) || 
                       keyboard.IsKeyPressed(Key.ShiftRight);
            
            // Example: Holding arrow keys for smooth color changes
            if (keyboard.IsKeyPressed(Key.Up))
            {
                red = Math.Min(1.0f, red + 0.5f * (float)deltaTime);
                green = Math.Min(1.0f, green + 0.5f * (float)deltaTime);
                blue = Math.Min(1.0f, blue + 0.5f * (float)deltaTime);
            }
            
            if (keyboard.IsKeyPressed(Key.Down))
            {
                red = Math.Max(0.0f, red - 0.5f * (float)deltaTime);
                green = Math.Max(0.0f, green - 0.5f * (float)deltaTime);
                blue = Math.Max(0.0f, blue - 0.5f * (float)deltaTime);
            }
        }
        
        // ====================================================================
        // MOUSE-BASED COLOR CONTROL (Advanced!)
        // ====================================================================
        // You can use mouse position to control colors
        // Uncomment to enable:
        
        // if (leftMouseDown && window != null)
        // {
        //     red = mousePosition.X / window.Size.X;
        //     green = mousePosition.Y / window.Size.Y;
        // }
    }
    
    private static void OnRender(double deltaTime)
    {
        // Clear with the current color
        gl!.ClearColor(red, green, blue, 1.0f);
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
    // HELPER FUNCTIONS
    // ========================================================================
    
    private static void PrintColor(string channel)
    {
        Console.WriteLine($"[Color] {channel} changed - RGB({red:F2}, {green:F2}, {blue:F2})");
    }
    
    private static void ResetColor()
    {
        red = 0.2f;
        green = 0.3f;
        blue = 0.4f;
        Console.WriteLine("[Color] Reset to default RGB(0.20, 0.30, 0.40)");
    }
    
    private static void RandomizeColor()
    {
        Random rand = new Random();
        red = (float)rand.NextDouble();
        green = (float)rand.NextDouble();
        blue = (float)rand.NextDouble();
        Console.WriteLine($"[Color] Randomized to RGB({red:F2}, {green:F2}, {blue:F2})");
    }
}

// ============================================================================
// CONGRATULATIONS! 🎮
// ============================================================================
// You now understand input handling in Silk.NET!
// 
// KEY CONCEPTS YOU LEARNED:
// 
// 1. TWO INPUT METHODS:
//    a) EVENTS (KeyDown, MouseDown, MouseMove)
//       - Fire once when action happens
//       - Great for: clicks, single key presses, UI interactions
//       - Example: Pressing 'R' to change red channel
//    
//    b) POLLING (IsKeyPressed in Update loop)
//       - Check state every frame
//       - Great for: continuous movement, holding keys
//       - Example: Holding arrow keys to brighten/darken
// 
// 2. KEYBOARD INPUT:
//    - KeyDown event: Fires once when key pressed
//    - IsKeyPressed: Returns true every frame key is held
//    - Modifier keys: Check Shift, Ctrl, Alt for combinations
// 
// 3. MOUSE INPUT:
//    - MouseMove: Track cursor position
//    - MouseDown/MouseUp: Button press events
//    - MouseButton enum: Left, Right, Middle, etc.
//    - Position normalized to window coordinates
// 
// 4. INPUT CONTEXT:
//    - Created once in OnLoad
//    - Access keyboards, mice, gamepads
//    - Must be disposed in OnClosing
// 
// WHEN TO USE EACH METHOD:
// 
// Use EVENTS when:
// - You need one action per key press
// - Implementing UI buttons
// - Weapon firing, jumping, menu navigation
// - Any "discrete" action
// 
// Use POLLING when:
// - You need smooth continuous input
// - Character movement (WASD)
// - Camera rotation
// - Any action that should happen "while held"
// 
// EXPERIMENTS TO TRY:
// 
// 1. Uncomment the mouse-based color control
// 2. Add more keyboard shortcuts (try 1-9 for preset colors)
// 3. Implement double-click detection
// 4. Add mouse wheel support (ScrollWheel event)
// 5. Create a drawing app (track mouse while button held)
// 
// PHASE 1 COMPLETE! 🎉
// You're now ready for Phase 2: Drawing your first triangle!
// ============================================================================
