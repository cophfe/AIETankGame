using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Raylib;
using static Raylib.Raylib;
using Mlib;

namespace Project2D
{
    class Game
    {
        Stopwatch stopwatch = new Stopwatch();

        public static long currentTime = 0;
        private long lastTime = 0;
        private float timer = 0;
        private int fps = 1;
        private int frames;

        static public float deltaTime = 0.005f;

        public static float screenWidth;
        public static float screenHeight;
        //Scenes are gameobjects that hold every other gameobject
        static List<Scene> scenes = new List<Scene>();
        List<GameObject> initialObjs = new List<GameObject>();

        public static Texture2D[] textures;

        static int currentScene = 0;

        public static SmoothCamera camera;

        public Game()
        {
            
        }

        Character player;
        PhysicsObject crate;
        PhysicsObject crate2;

        public void Init()
        {
            screenWidth = GetScreenWidth();
            screenHeight = GetScreenHeight();
            stopwatch.Start();
            lastTime = stopwatch.ElapsedMilliseconds;

            if (Stopwatch.IsHighResolution)
            {
                Console.WriteLine("Stopwatch high-resolution frequency: {0} ticks per second", Stopwatch.Frequency);
            }

            //Initialize objects here

            textures = new Texture2D[]
            {
                LoadTexture("../Images/missing.png"),
                LoadTexture("../Images/crate.png"),
                LoadTexture("../Images/arm.png"),
                LoadTexture("../Images/Walking/standing.png"),
                LoadTexture("../Images/grid.jpg"),
                LoadTexture("../Images/pupil.png")
            };

            Collider playerCollider = Collider.FromTextureName(TextureName.Crate);
            crate = new PhysicsObject(TextureName.Crate, Vector2.One * 0, 1f, playerCollider, 1f, 1, 0f, 0, null, 1);
            playerCollider = Collider.FromTextureName(TextureName.Crate);
            crate2 = new PhysicsObject(TextureName.Crate, Vector2.One * 1500, 0.5f, playerCollider, 1, 0, 0f, 0, null, 1, true);
            crate2.AddAngularVelocity(0.5f);
            crate.AddVelocity(new Vector2(100,0));
            //initialObjs.Add(new GameObject(TextureName.Grid));
            initialObjs.Add(crate);
            initialObjs.Add(crate2);
            initialObjs.Add(new PhysicsObject(TextureName.Crate, Vector2.One * 300, 1f, Collider.FromTextureName(TextureName.Crate), 1f, 1, 0f, 0, null, 1));
            player = new Character(TextureName.Player, new Vector2(250, 250), 0.3f, 0, null, new Collider(-40, 0, 80, 100));
            initialObjs.Add(player);
            initialObjs.Add(new PhysicsObject(TextureName.Crate, Vector2.One * 1000, 1f, Collider.FromTextureName(TextureName.Crate), 1f, 1, 0f, 0, null, 1));
            initialObjs.Add(new PhysicsObject(TextureName.Crate, Vector2.One * 600, 1f, Collider.FromTextureName(TextureName.Crate), 1f, 1, 0f, 0, null, 1));
            scenes.Add(new Scene(initialObjs));

            

            camera = new SmoothCamera(scenes[0], player.GlobalPosition, 0, 1f, new Vector2(0, 0), true, CollisionLayer.Player);
            camera.Target(player);
            //CollisionManager.Initiate();
        }
        public void Shutdown()
        {
        }

        public void Update()
        {
            lastTime = currentTime;
            currentTime = stopwatch.ElapsedMilliseconds;
            deltaTime = (currentTime - lastTime) / 1000.0f;
            timer += deltaTime;
            if (timer >= 1)
            {
                fps = frames;
                frames = 0;
                timer -= 1;
            }
            frames++;

            //Update game objects here       

            CollisionManager.CheckCollisions(CollisionLayer.ScreenBounds);
            scenes[currentScene].Update(deltaTime);
            scenes[currentScene].UpdateTransforms();
        }

        public static Vector2 v;
        public static Vector2 v2;
        public static Ray r;

        public void Draw()
        {
            BeginDrawing();

            ClearBackground(new RLColor { a = 255, r = 20, g = 20, b = 20 });

            camera.StartCamera();

            //draw all objects
			scenes[currentScene].Draw();

            DrawCircle((int)v.x, (int)v.y, 5, RLColor.RED);
            DrawCircle((int)v2.x, (int)v2.y, 5, RLColor.RED);

            //end 2d camera
            camera.EndCamera();

            //draw GUI

            DrawText(fps.ToString(), 10, 10, 14, RLColor.RED);

            EndDrawing();
        }

        public static Texture2D GetTextureFromName(TextureName name)
        {
            return textures[(int)name];
        }

        public static Scene getCurrentScene()
		{
            return scenes[currentScene];
        }
    }
    
    
}
