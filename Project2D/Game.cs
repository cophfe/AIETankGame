using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Raylib;
using static Raylib.Raylib;
using Mlib;

namespace Project2D
{
    static class Game //I made game static so that it would be easier to access some public functions I added
    {
        //NOTE:
        // a lot of code throughout this entire thing is really gross
        // I kept adding things on top of each other and when i needed to change old things to make new things work I just quickly patched it over in a kinda disgusting way
        // in real game development environment I would make a gross version and then make a clean version after the basics are set in stone
        // also in the future I will try to comment whilst making the code, not after. 

        //Game fps things
        static Stopwatch stopwatch = new Stopwatch();
        public static long currentTime = 0;
        static private long lastTime = 0;
        public static float timer = 0;
        public static int fps = 1;
        static private int frames;

        //enable or disable line of sight
        public static bool lOS = false;
        //pause update
        public static bool pause = false;
        
        //the change in time
        static public float deltaTime = 0.005f;

        //screen bounds
        public static float screenWidth;
        public static float screenHeight;

        //A list of all scenes
        static readonly List<Scene> scenes = new List<Scene>();

        //A list of all textures with a TextureName equivelent
        public static Texture2D[] textures;

        //The current scene
        static int currentScene = 0;

        //A globally accessible random class just because randoms are used a lot
        static public readonly Random globalRand = new Random();

        //Color that corrosponds with gameobjects (for map creation from an image)
        static Color wallColor = Color.FromArgb(0, 0, 0);
        static Color chickenColor = Color.FromArgb(255, 255, 255);
        static Color playerColor = Color.FromArgb(255, 0, 0);
        static Color baleColor = Color.FromArgb(255, 255, 0);
        static Color sideWallColor = Color.FromArgb(0, 0, 255);
        
        //the player
        static Player player;

        static public void Init()
        {
            screenWidth = GetScreenWidth();
            screenHeight = GetScreenHeight();
            stopwatch.Start();
            lastTime = stopwatch.ElapsedMilliseconds;

            if (Stopwatch.IsHighResolution)
            {
                Console.WriteLine("Stopwatch high-resolution frequency: {0} ticks per second", Stopwatch.Frequency);
            }

            //all textures are loaded in
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
                LoadTexture("../Images/eggBird.png"),
            };

            //create the scenes here (manually through code)
            Scene startMenu = new Scene();
            scenes.Add(startMenu);
            startMenu.backgroundColor = new Colour(83, 83, 116, 255);
            
            //all objects inside of the menu scene:
            startMenu.AddChild(new GameObject(TextureName.Eye, new Vector2(screenWidth/2 - 300, 300), 0.3f, 0, null, true, SpriteLayer.Background));
            startMenu.AddChild(new GameObject(TextureName.Eye, new Vector2(screenWidth/2 + 300, 300), 0.3f, 0, null, true, SpriteLayer.Background));
            Eye eye = new Eye(new Vector2(screenWidth / 2 - 300, 300), 70, 2f);
            Eye eye2 = new Eye(new Vector2(screenWidth / 2 + 300, 300), eye);
            startMenu.AddChild(eye);
            startMenu.AddChild(eye2);
            Button playButton = new Button(TextureName.Button, 430, 250, new Vector2(screenWidth / 2, screenHeight - 300), 1, null);

            //button events added here
            playButton.AddHoverEvent(() =>
            {
                if (IsMouseButtonPressed(MouseButton.MOUSE_LEFT_BUTTON))
                {
                    playButton.GetSprite().SetFrame(1);
                    playButton.GetSprite().SetTint(RLColor.WHITE);
                    playButton.LocalScale = Vector2.one * 0.94f;
                    playButton.UpdateTransforms();
                }
                if (IsMouseButtonReleased(MouseButton.MOUSE_LEFT_BUTTON))
                {
                    if (playButton.GetSprite().GetCurrentFrame() == 1)
					{
                        currentScene++;
                        pause = true;
                        lOS = true;
                        UnloadTexture(GetTextureFromName(TextureName.Button));
                        UnloadTexture(GetTextureFromName(TextureName.Eye));
                    }
                }
            });
            playButton.AddLeaveEvent(() => 
            {
                playButton.GetSprite().SetFrame(0);
                playButton.LocalScale = Vector2.one;
                playButton.UpdateTransforms();
                playButton.GetSprite().SetTint(RLColor.WHITE);
            });
            playButton.AddEnterEvent(() =>
            {
                playButton.GetSprite().SetTint(new RLColor(220,200,225,255));
                
            });

            //play button given animated sprite
            playButton.SetSprite(new Sprite(Sprite.GetFramesFromFolder("PlayButton"), 100, 0, 1, playButton, RLColor.WHITE, false));

            //play button added to UI list
            startMenu.AddUIElement(playButton);

            //now create the game scene
            Scene gameScene = new Scene();
            scenes.Add(gameScene);

            //current scene is temporarily set to this one
            currentScene++;
            //player is set
            player = new Player(TextureName.Player, new Vector2(250, 250), null, new Collider(-40, 45, 80, 60));
            //scene is given children based on the map image passed inside.

            //MANUAL COLLISION MAP USED FOR MAP3
            Vector2[,] collisionMap = new Vector2[12, 2] { { new Vector2(9f, 0.5f), new Vector2(14, 1) }, { new Vector2(4.5f, 6.5f), new Vector2(7, 1) }, { new Vector2(11.5f, 9.5f), new Vector2(9, 1) }, { new Vector2(4.5f, 11.5f), new Vector2(7, 1) }, { new Vector2(12, 14.5f), new Vector2(8, 1) },
            { new Vector2(1.5f, 3.5f), new Vector2(1, 6) }, { new Vector2(10.5f, 4), new Vector2(1, 6) }, { new Vector2(16.5f, 5), new Vector2(1, 16) }, { new Vector2(16.5f, 12.5f), new Vector2(1, 7) }, { new Vector2(0.5f, 9f ), new Vector2(1, 4) }, { new Vector2(7.5f, 7.5f), new Vector2(1, 1) }, { new Vector2(7.5f, 13), new Vector2(1, 2) },};

            //make scene from image using object templates that have no parents. to cut down on colliders, I set it so that a manual collision map is added on top and the wall template has no collision
            MakeSceneFromImage(new PhysicsObject(TextureName.Wall, Vector2.one * 300, 1f, null, 1f, 1, 3f, 0, null, 1, false),//new Collider(-135.5f, -92f, 271f, 271f)
                new PhysicsObject(TextureName.Bale, Vector2.zero, 0.7f, Collider.FromTextureName(TextureName.Bale), 2f, 3, 0.7f, 0, null, 3f, true),
                player,
                new Chicken(TextureName.Chicken, Vector2.zero, player),
                "../Images/map3.png", gameScene, collisionMap);

            //sets the camera to a new camera in scene 1
            player.SetTiedCamera(new SmoothCamera(scenes[1], player.GlobalPosition, 0, 1f, new Vector2(0, 0), CollisionLayer.Player, CollisionLayer.Enemy));
            new StartText(1f, gameScene); //adds a starttext object to the game scene
            
            currentScene = 0;
        }

        static public void Shutdown()
        {
        }

        static public void Update()
        {
            //updates time stuff
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

            //updates UI before pause
            scenes[currentScene].UpdateUI();

            if (pause)
                return;

            //Update gameobjects
            scenes[currentScene].Update();
            scenes[currentScene].UpdateTransforms();
            scenes[currentScene].UpdateCollisions();
        }

        static public void Draw()
        {
            //draw all objects and UI
            scenes[currentScene].Draw();
        }

        //Returns the specified texture
        public static Texture2D GetTextureFromName(TextureName name)
        {
            return textures[(int)name];
        }

        //returns the current scene
        public static Scene GetCurrentScene()
		{
            return scenes[currentScene];
        }

        //Gets the current player object
        public static Player GetPlayer()
		{
            return player;
		}

        //creates a scene from an image (using System.Drawing)
        public static void MakeSceneFromImage(PhysicsObject wallTemplate, PhysicsObject baleTemplate, Player player, PhysicsObject chickenTemplate, string map, Scene s, Vector2[,] collisionMap = null)
        {
            //uses a manual collision map if it is given
            if (collisionMap != null)
            {
                for (int i = 0; i < collisionMap.GetLength(0); i++)
                {
                    new PhysicsObject(TextureName.None, collisionMap[i, 0] * 271 + new Vector2(-135.5f, -94f), 1, new Collider(Vector2.zero, collisionMap[i, 1].x * 271, collisionMap[i, 1].y * 271), restitution: 1, parent: s, isDynamic: false, isDrawn: false);
                }
            }

            int chickenTotal = 0;
            Bitmap image = new Bitmap(map);
            float sizeX = wallTemplate.GetSprite().GetWidth() / wallTemplate.LocalScale.x;
            float sizeY = wallTemplate.GetSprite().GetWidth() / wallTemplate.LocalScale.x;

            GameObject cache;
            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    Color c = image.GetPixel(x, y);

                    if (c == wallColor)
                    {
                        cache = wallTemplate.Clone();
                        cache.LocalPosition = new Vector2(sizeX * x, sizeY * y);
                        s.AddChild(cache);
                    }
                    else if (c == chickenColor)
                    {
                        cache = chickenTemplate.Clone();
                        cache.LocalPosition = new Vector2(sizeX * x, sizeY * y);
                        s.AddChild(cache);
                        chickenTotal++;
                    }
                    else if (c == playerColor)
                    {
                        player.LocalPosition = new Vector2(sizeX * x, sizeY * y);
                        s.AddChild(player);
                    }
                    else if (c == baleColor)
                    {
                        cache = baleTemplate.Clone();
                        cache.LocalPosition = new Vector2(sizeX * x, sizeY * y);
                        s.AddChild(cache);
                    }
                    else if (c == sideWallColor)
                    {
                        cache = wallTemplate.Clone();
                        cache.LocalPosition = new Vector2(sizeX * x, sizeY * y);
                        cache.SetSprite(new Sprite(Game.GetTextureFromName(TextureName.SideWall), cache, RLColor.WHITE));
                        cache.GetSprite().SetLayer(SpriteLayer.Foreground);

                        s.AddChild(cache);
                    }
                }
            }

            //deletes the templates just incase they are being used in any way that they shouldn't be
            baleTemplate.RemoveCollider(s);
            baleTemplate.Delete();
            chickenTemplate.RemoveCollider(s);
            chickenTemplate.Delete();
            wallTemplate.RemoveCollider(s);
            wallTemplate.Delete();

            //tells the player how many chickens there are in a map (as the player controls the hunger bar)
            player.chickenTotalInScene = chickenTotal;

        }
    }
    
    
}
