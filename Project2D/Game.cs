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
                LoadTexture("../Images/player.png")
            };

            RectangleCollider playerCollider = RectangleCollider.FromTextureName(TextureName.Crate);
            crate = new PhysicsObject(TextureName.Crate, Vector2.One * 0, Vector2.One * 0.5f, playerCollider, 0.5f, 1, 1, 0, null);
            initialObjs.Add(crate);
            initialObjs.Add(new PhysicsObject(TextureName.Crate, Vector2.One * 400, Vector2.One * 0.5f,null, 0.5f, 1, 1, 0, null));
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

            scenes[currentScene].UpdateTransforms();
            scenes[currentScene].Update(deltaTime);
            CollisionManager.CheckCollisions();
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

            //draw player collision info
   //         bool broad = false;
   //         bool close = false;
   //         {
                
   //             player.GetCollider().TransformByGlobalTransform();
   //             crate.GetCollider().TransformByGlobalTransform();
   //             Vector2[] pT = (player.GetCollider() as RectangleCollider).GetGlobalPoints();
   //             int j = 0;
   //             for (int i = 0; i < pT.Length; i++)
   //             {
   //                 j++;
   //                 j %= pT.Length;
   //                 DrawCircle((int)(pT[i].x + player.LocalPosition.x), (int)(pT[i].y + player.LocalPosition.y), 5, RLColor.MAGENTA);
   //                 DrawLine((int)(pT[i].x + player.LocalPosition.x), (int)(pT[i].y + player.LocalPosition.y), (int)(pT[j].x + player.LocalPosition.x), (int)(pT[j].y + player.LocalPosition.y), RLColor.MAGENTA);
   //             }
   //             AABB aabb = (player.GetCollider().GetAABB());
   //             Vector2[] pX = new Vector2[] { aabb.topLeft, new Vector2(aabb.topLeft.x, aabb.bottomRight.y), aabb.bottomRight, new Vector2(aabb.bottomRight.x, aabb.topLeft.y) };
   //             j = 0;
   //             for (int i = 0; i < pX.Length; i++)
   //             {
   //                 j++;
   //                 j %= pT.Length;
   //                 DrawCircle((int)(pX[i].x), (int)(pX[i].y), 5, RLColor.ORANGE);
   //                 DrawLine((int)(pX[i].x), (int)(pX[i].y), (int)(pX[j].x), (int)(pX[j].y), RLColor.ORANGE);
   //             }
   //             Matrix3 gT = player.GetGlobalTransform();
   //             Vector2 axisY = gT.GetForwardVector();
   //             Vector2 axisX = gT.GetRightVector();

               
                
   //             axisY *= 1000;
   //             axisX *= 1000;

   //             DrawLine((int)axisY.x, (int)axisY.y, (int)-axisY.x, (int)-axisY.y, RLColor.BLUE);
   //             DrawLine((int)axisX.x, (int)axisX.y, (int)-axisX.x, (int)-axisX.y, RLColor.BLUE);



   //             Vector2[] pTC = (crate.GetCollider() as RectangleCollider).GetGlobalPoints();
   //             j = 0;
   //             for (int i = 0; i < pT.Length; i++)
   //             {
   //                 j++;
   //                 j %= pT.Length;
   //                 DrawCircle((int)(pTC[i].x + crate.LocalPosition.x), (int)(pTC[i].y + crate.LocalPosition.y), 5, RLColor.MAGENTA);
   //                 DrawLine((int)(pTC[i].x + crate.LocalPosition.x), (int)(pTC[i].y + crate.LocalPosition.y), (int)(pTC[j].x + crate.LocalPosition.x), (int)(pTC[j].y + crate.LocalPosition.y), RLColor.MAGENTA);
   //             }
   //             AABB aabbC = (crate.GetCollider().GetAABB());
   //             pX = new Vector2[] { aabbC.topLeft, new Vector2(aabbC.topLeft.x, aabbC.bottomRight.y), aabbC.bottomRight, new Vector2(aabbC.bottomRight.x, aabbC.topLeft.y) };
   //             j = 0;
   //             for (int i = 0; i < pX.Length; i++)
   //             {
   //                 j++;
   //                 j %= pT.Length;
   //                 DrawCircle((int)(pX[i].x), (int)(pX[i].y), 5, RLColor.ORANGE);
   //                 DrawLine((int)(pX[i].x), (int)(pX[i].y), (int)(pX[j].x), (int)(pX[j].y), RLColor.ORANGE);
   //             }

   //             gT = crate.GetGlobalTransform();
   //             Vector2 cAxisY = gT.GetForwardVector();
   //             Vector2 cAxisX = gT.GetRightVector();

   //             cAxisY *= 1000;
   //             cAxisX *= 1000;

   //             DrawLine((int)cAxisY.x, (int)cAxisY.y, (int)-cAxisY.x, (int)-cAxisY.y, RLColor.RED);
   //             DrawLine((int)cAxisX.x, (int)cAxisX.y, (int)-cAxisX.x, (int)-cAxisX.y, RLColor.RED);
   //             broad = CollisionManager.CheckAABB(aabb, aabbC);
   //             if (broad)
			//	{
                    

   //                 //Close phase

   //                 //project onto player axis
   //                 axisX.SetNormalised();
   //                 axisY.SetNormalised();
   //                 cAxisY.SetNormalised();
   //                 cAxisX.SetNormalised();

   //                 Vector2[] dotPAP = new Vector2[pT.Length];
   //                 Vector2[] dotPAC = new Vector2[pTC.Length];
			//		for (int i = 0; i < pT.Length; i++)
			//		{
   //                     dotPAP[i] = new Vector2(axisX.Dot(pT[i] + player.LocalPosition), axisY.Dot(pT[i] + player.LocalPosition));
			//		}

			//		for (int i = 0; i < pTC.Length; i++)
   //                 {
   //                     dotPAC[i] = new Vector2(axisX.Dot(pTC[i] + crate.LocalPosition), axisY.Dot(pTC[i] + crate.LocalPosition));
   //                 }


   //                 //Draw it projected

   //                 for (int i = 0; i < dotPAP.Length; i++)
   //                 {
   //                     Vector2 dotVector = axisX * dotPAP[i].x;
   //                     DrawCircle((int)dotVector.x, (int)dotVector.y, 3, RLColor.BLUE);
   //                     dotVector = axisY * dotPAP[i].y;
   //                     DrawCircle((int)dotVector.x, (int)dotVector.y, 3, RLColor.BLUE);
   //                 }

   //                 for (int i = 0; i < dotPAC.Length; i++)
   //                 {
   //                     Vector2 dotVector = axisX * dotPAC[i].x;
   //                     DrawCircle((int)dotVector.x, (int)dotVector.y, 3, RLColor.BLUE);
   //                     dotVector = axisY * dotPAC[i].y;
   //                     DrawCircle((int)dotVector.x, (int)dotVector.y, 3, RLColor.BLUE);
   //                 }

   //                 //project onto crate axis

   //                 float[] dotXCAP = new float[pT.Length];
   //                 float[] dotYCAP = new float[pT.Length];
   //                 float[] dotXCAC = new float[pTC.Length];
   //                 float[] dotYCAC = new float[pTC.Length];

   //                 Vector2[] dotCAP = new Vector2[pT.Length];
   //                 Vector2[] dotCAC = new Vector2[pTC.Length];

   //                 for (int i = 0; i < pT.Length; i++)
   //                 {
   //                     dotCAP[i] = new Vector2(cAxisX.Dot(pT[i] + player.LocalPosition), cAxisY.Dot(pT[i] + player.LocalPosition));
   //                 }

   //                 for (int i = 0; i < pTC.Length; i++)
   //                 {
   //                     dotCAC[i] = new Vector2(cAxisX.Dot(pTC[i] + crate.LocalPosition), cAxisY.Dot(pTC[i] + crate.LocalPosition));
   //                 }

   //                 //Draw projected
   //                 for (int i = 0; i < dotCAP.Length; i++)
   //                 {
   //                     Vector2 dotVector = cAxisX * dotCAP[i].x;
   //                     DrawCircle((int)dotVector.x, (int)dotVector.y, 3, RLColor.BLUE);
   //                     dotVector = cAxisY * dotCAP[i].y;
   //                     DrawCircle((int)dotVector.x, (int)dotVector.y, 3, RLColor.BLUE);
   //                 }

   //                 for (int i = 0; i < dotCAC.Length; i++)
   //                 {
   //                     Vector2 dotVector = cAxisX * dotCAC[i].x;
   //                     DrawCircle((int)dotVector.x, (int)dotVector.y, 3, RLColor.DARKPURPLE);
   //                     dotVector = cAxisY * dotCAC[i].y;
   //                     DrawCircle((int)dotVector.x, (int)dotVector.y, 3, RLColor.DARKPURPLE);
   //                 }

   //                 AABB projectedAABBCAC = AABB.GetAABBFromPoints(dotCAC);
   //                 AABB projectedAABBCAP = AABB.GetAABBFromPoints(dotCAP);
   //                 AABB projectedAABBPAP = AABB.GetAABBFromPoints(dotPAP);
   //                 AABB projectedAABBPAC = AABB.GetAABBFromPoints(dotPAC);

   //                 if (CollisionManager.CheckAABB(projectedAABBCAC, projectedAABBCAP) && CollisionManager.CheckAABB(projectedAABBPAP, projectedAABBPAC))
   //                 {
   //                     close = true;

   //                     Vector2 axis = new Vector2();

                        
                        
   //                     CollisionPair collision = new CollisionPair(player, crate, Vector2.One, CollisionType.PolygonPolygon);
   //                     CollisionManager.ResolveCollision(collision);
   //                 }
                        
   //             }

			//}

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
        Player
    }
}
