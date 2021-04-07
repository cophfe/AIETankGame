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
        public static float timer = 0;
        public static int fps = 1;
        private int frames;
        public bool pause = false;
        static public float deltaTime = 0.005f;

        public static float screenWidth;
        public static float screenHeight;

        //Scenes are gameobjects that hold every other gameobject
        static List<Scene> scenes = new List<Scene>();

        public static Texture2D[] textures;

        static int currentScene = 0;

        public static SmoothCamera camera;
        public static Random globalRand = new Random();

        public Game()
        {
            
        }

        static Character player;

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
                LoadTexture("../Images/bale.png"),
                LoadTexture("../Images/Player/astanding.png"),
                LoadTexture("../Images/pupil.png"),
                LoadTexture("../Images/vignette.png"),
                LoadTexture("../Images/PlayButton/button.png"),
                LoadTexture("../Images/eye.png"),
                LoadTexture("../Images/Chicken/astanding.png"),
                LoadTexture("../Images/feather.png"),
                LoadTexture("../Images/PlayerHead/Bite_100.png"),
                LoadTexture("../Images/quickHeadFix.png"),
                LoadTexture("../Images/Chicken/zcooked16.png"),
                LoadTexture("../Images/wall.png"),
                LoadTexture("../Images/sideWall.png"),
                LoadTexture("../Images/xray.png"),
                LoadTexture("../Images/gooBird.png"),
            };

            Scene startMenu = new Scene();
            Scene game = new Scene();
            scenes.Add(startMenu);
            //scenes.Add(game);
            startMenu.backgroundColor = new Colour();
            startMenu.AddChild(new GameObject(TextureName.Eye, new Vector2(screenWidth/2 - 300, 300), 0.3f, 0, null, true, SpriteLayer.Background));
            startMenu.AddChild(new GameObject(TextureName.Eye, new Vector2(screenWidth/2 + 300, 300), 0.3f, 0, null, true, SpriteLayer.Background));
            Eye eye = new Eye(new Vector2(screenWidth / 2 - 300, 300), 70, 2f);
            Eye eye2 = new Eye(new Vector2(screenWidth / 2 + 300, 300), eye);
            startMenu.AddChild(eye);
            startMenu.AddChild(eye2);
            Button playButton = new Button(TextureName.Button, 430, 250, new Vector2(screenWidth / 2, screenHeight - 300), 1, null);
            playButton.AddHoverEvent(() =>
            {
                if (IsMouseButtonPressed(MouseButton.MOUSE_LEFT_BUTTON))
                {
                    playButton.GetSprite().SetFrame(1);
                    playButton.GetSprite().SetTint(RLColor.WHITE);
                    playButton.LocalScale = Vector2.One * 0.96f;
                    playButton.UpdateTransforms();
                }
                if (IsMouseButtonReleased(MouseButton.MOUSE_LEFT_BUTTON))
                {
                    if (playButton.GetSprite().CurrentFrame() == 1)
                        currentScene++;
                }
            });

            playButton.AddLeaveEvent(() => 
            {
                playButton.GetSprite().SetFrame(0);
                playButton.LocalScale = Vector2.One;
                playButton.UpdateTransforms();
                playButton.GetSprite().SetTint(RLColor.WHITE);
            });

            playButton.AddEnterEvent(() =>
            {
                playButton.GetSprite().SetTint(new RLColor(220,200,225,255));
                
            });
            playButton.SetSprite(new Sprite(Sprite.GetFramesFromFolder("PlayButton"), 100, 0, 1, playButton, RLColor.WHITE, false));
            startMenu.AddUIElement(playButton);
            startMenu.backgroundColor = new Colour(83, 83, 116, 255);

            Scene s = new Scene();
            scenes.Add(s);
            currentScene++;
            player = new Character(TextureName.Player, new Vector2(250, 250), 0.3f, 0, null, new Collider(-40, 45, 80, 60));
            MapFromImage.MakeSceneFromImage(new PhysicsObject(TextureName.Wall, Vector2.One * 300, 1f, null, 1f, 1, 3f, 0, null, 1, false),//new Collider(-135.5f, -92f, 271f, 271f)
                new PhysicsObject(TextureName.Bale, Vector2.Zero, 0.7f, Collider.FromTextureName(TextureName.Bale), 2f, 3, 0.7f, 0, null, 3f, true),
                player,
                new Chicken(TextureName.Chicken, Vector2.Zero, player),
                "../Images/map3.png", s, true);
            camera = new SmoothCamera(scenes[1], player.GlobalPosition, 0, 1f, new Vector2(0, 0), true, CollisionLayer.Player, CollisionLayer.Enemy);
            player.SetTiedCamera(camera);
            camera.LocalPosition = player.LocalPosition;
            
            currentScene = 0;
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

            scenes[currentScene].UpdateUI();
            if (pause)
                return;

            //Update game objects here
            scenes[currentScene].UpdateTransforms();
            scenes[currentScene].Update();
            scenes[currentScene].UpdateCollisions();
        }

        public void Draw()
        {
            //draw all objects and UI
            
            scenes[currentScene].Draw();
        }

        public static Texture2D GetTextureFromName(TextureName name)
        {
            return textures[(int)name];
        }

        public static Scene GetCurrentScene()
		{
            return scenes[currentScene];
        }

        public static Character GetPlayer()
		{
            return player;
		}
    }
    
    
}
