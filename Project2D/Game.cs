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
        int currentScene = 0;

        Camera2D camera;

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

            scenes.Add(new Scene());
            player = new Player("../Images/player.png", new Vector2(250, 250), new Vector2(0.2f, 0.2f), 0, scenes[0]);
            
            //scenes[0].Initialise();
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

            ClearBackground(RLColor.WHITE);

            //Draw game objects here
            
            
            scenes[currentScene].Draw();
            Vector2 pos = player.GetGlobalTransform().GetTranslation();
            Vector2 m = pos + player.GetVelocity();
            //DrawLine((int)m.x, (int)m.y, (int)pos.x, (int)pos.y, RLColor.BLACK);

            DrawText(fps.ToString(), 10, 10, 14, RLColor.RED);
            EndDrawing();
        }

    }
}
