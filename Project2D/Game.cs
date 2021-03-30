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

        private long currentTime = 0;
        private long lastTime = 0;
        private float timer = 0;
        private int fps = 1;
        private int frames;

        private float deltaTime = 0.005f;

        //Scenes are gameobjects that hold every other gameobject
        List<Scene> scenes = new List<Scene>();
        List<GameObject> initialObjs = new List<GameObject>();

        public static Texture2D[] textures;

        int currentScene = 0;

        public static SmoothCamera camera;

        public Game()
        {
            
        }

        Player player;
        PhysicsObject crate;

        public void Init()
        {
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
                LoadTexture("../Images/player.png"),
                LoadTexture("../Images/grid.jpg")
            };

            RectangleCollider playerCollider = RectangleCollider.FromTextureName(TextureName.Crate);
            crate = new PhysicsObject(TextureName.Crate, Vector2.One * 0, Vector2.One * 0.5f, playerCollider, 0f, 1, 1, 1, null);
            crate.AddVelocity(new Vector2(100,0));
            initialObjs.Add(new GameObject(TextureName.Grid));
            initialObjs.Add(crate);
            playerCollider = RectangleCollider.FromTextureName(TextureName.Crate);
            initialObjs.Add(new PhysicsObject(TextureName.Crate, Vector2.One * 400, Vector2.One * 0.5f, playerCollider, 0f, 1, 1, 0, null));
            scenes.Add(new Scene(initialObjs));

            playerCollider = RectangleCollider.FromTextureName(TextureName.Player);
            player = new Player(TextureName.Player, TextureName.Arm, new Vector2(0, 0), Vector2.One, new Vector2(250, 250), new Vector2(0.2f, 0.2f), 0, scenes[0], playerCollider);

            camera = new SmoothCamera(scenes[0], player.GlobalPosition, 0, 1, new Vector2(0, 0));
            camera.Target(player);
            CollisionManager.Initiate();
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

            scenes[currentScene].Update(deltaTime);
            scenes[currentScene].UpdateTransforms();
            CollisionManager.CheckCollisions();
            scenes[currentScene].UpdateTransforms();
        }

        public void Draw()
        {
            BeginDrawing();

            ClearBackground(RLColor.LIGHTGRAY);

            camera.StartCamera();

            //Draw game objects here

            //Draw mouse circle
            RLVector2 m = camera.GetMouseWorldPosition();
            DrawCircle((int)m.x, (int)m.y, 5, RLColor.RED);

            //draw all objects
			scenes[currentScene].Draw();
            if (CollisionManager.obj != null)
            {
                Vector2 pos = CollisionManager.obj.GlobalPosition;
                DrawLineEx(pos, pos + CollisionManager.drawVector * 10,10, RLColor.RED);
            }
            //end 2d camera
            camera.EndCamera();

            //draw GUI

            DrawText(fps.ToString(), 10, 10, 14, RLColor.RED);
            //DrawText($"Broad Phase: {broad}", 10, 30, 14, RLColor.DARKPURPLE);
            //DrawText($"Close Phase: {close}", 10, 50, 14, RLColor.DARKPURPLE);
            EndDrawing();
        }

        public static Texture2D GetTextureFromName(TextureName name)
        {
            return textures[(int)name];
        }

    }

    enum TextureName
    {
        None,
        Crate,
        Arm,
        Player,
        Grid
    }
}
