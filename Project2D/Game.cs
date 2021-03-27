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
                LoadTexture("../Images/player.png")
            };

            initialObjs.Add(new PhysicsObject(TextureName.Crate, Vector2.One * 40, Vector2.One * 0.5f, null, 0.5f, 1, 1, 0, null));
            initialObjs.Add(new PhysicsObject(TextureName.Crate, Vector2.One * 400, Vector2.One * 0.5f,null, 0.5f, 1, 1, 0, null));
            scenes.Add(new Scene(initialObjs));

            PolygonCollider playerCollider = new PolygonCollider(new Vector2[] { new Vector2(-169, 214), new Vector2(169, 214), new Vector2(169, -214), new Vector2(-169, -214)});
            player = new Player(TextureName.Player, TextureName.Arm, new Vector2(0, 0), Vector2.One, new Vector2(250, 250), new Vector2(0.2f, 0.2f), 0, scenes[0], playerCollider);

            camera = new SmoothCamera(scenes[0], player.GlobalPosition, 0, 1, new Vector2(0, 0));
            camera.Target(player);

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

            scenes[currentScene].UpdateTransforms();
            scenes[currentScene].Update(deltaTime);
        }

        public void Draw()
        {
            BeginDrawing();

            ClearBackground(RLColor.GRAY);

            camera.StartCamera();

            //Draw game objects here

            RLVector2 m = camera.GetMouseWorldPosition();
            DrawCircle((int)m.x, (int)m.y, 5, RLColor.RED);

			player.GetCollider().TransformByGlobalTransform();
			Vector2[] pT = (player.GetCollider() as PolygonCollider).pointsTransformed;
			scenes[currentScene].Draw();

            int j = 0;
			for (int i = 0; i < pT.Length; i++)
			{
                j++;
                j %= pT.Length;
				DrawCircle((int)(pT[i].x + player.LocalPosition.x), (int)(pT[i].y + player.LocalPosition.y), 5, RLColor.MAGENTA);
                DrawLine((int)(pT[i].x + player.LocalPosition.x), (int)(pT[i].y + player.LocalPosition.y), (int)(pT[j].x + player.LocalPosition.x), (int)(pT[j].y + player.LocalPosition.y), RLColor.MAGENTA);
            }

            camera.EndCamera();

            DrawText(fps.ToString(), 10, 10, 14, RLColor.RED);
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
        Player
    }
}
