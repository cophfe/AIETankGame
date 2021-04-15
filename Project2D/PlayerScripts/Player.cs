using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mlib;
using Raylib;
using static Raylib.Raylib;

namespace Project2D
{
	class Player : PhysicsObject
	{
		//values for changing between slow, fast and normal
		float accelerationCap;
		const float accelerationCapNorm = 4000;
		const float accelerationFasterAmount = 500f;

		float velocityCap;
		const float velocityCapNorm = 300;
		const float velocityCapSlow = 200;
		const float velocityFasterAmount = 100f;

		int frameSpeed;
		const int frameSpeedNorm = 30;
		const int frameSpeedAddition = 5;
		const int frameSpeedSlowDown = -20; //(is addition because is added to slow or fast)

		//bools for changing between states
		bool standing = true;
		bool running = false;

		bool newGamePlus = false;
		float defaultIMass;

		//references to children objects
		readonly Eye eye1;
		readonly Eye eye2;
		readonly GameObject head;
		readonly GameObject fix;
		readonly GameObject chicken;
		readonly GameObject headHolder;
		readonly Sprite headSprite;

		//camera reference (camera is tied to player)
		SmoothCamera camera;

		//value for the offset the head has from the body
		Vector2 headOffset = new Vector2(-3, -58);

		//reference to the hunger bar (tied to the player)
		HungerHolder h;

		//movement information
		Vector2 inputVelocity;
		Vector2 inputDirection = Vector2.zero;

		//laser values
		RLColor eyeColor = RLColor.BLACK;
		int chargeSpeed = 500;
		float buildUp = 0;
		bool charging = false;
		bool cooling = false;

		//bite values
		bool biting = false;
		bool endingBite = false;
		int chickenEatAmount = 0;
		public int chickenTotalInScene = 0;
		public int chickensEatenTotal = 0;
		List<SuckedChickenInfo> suckedChickens = new List<SuckedChickenInfo>(); //reference to all chickens currently being sucked
		const float chickenSuckSpeed = 0.5f; //speed chickens are being sucked at
		public bool broadcastingHunger = false; //a public bool that anything can read to tell all chickens it the player is eating. should probably be a readonly constructor tbh but whatever
		public bool chickensBeingSucked = false; //is true if any chickens are being sucked at all
		static Vector2 eatingOffset = new Vector2(-21, 21); //the offset from the centre of the head that the chicken will move toward

		//offset eyes have for each body frame
		float[] eyeOffset = new float[24]
		{
			0, 0, 4, 4.5f, 6, -4, -8, -9, -9, -8, -7f, -5f, -3, 4, 4.5f, 6, -4, -8, -9, -9, -8, -5.5f, -4, -1
		};

		//offset head has for each bite frame
		float[] headEyeOffset = new float[10]
		{
			0, -5, -31, -35, -40, -44, -29, -14, 8, 3
		};

		public Player(TextureName fileName, Vector2 position, Scene scene, Collider collider) : base(fileName, position, 1, collider, 0.5f, 3f, 0f, 0f, scene, 1, true, false)
		{
			
			spriteManager = new Sprite(Sprite.GetFramesFromFolder("Player"), 30, 1, 23, this, RLColor.WHITE);
			spriteManager.Pause();
			spriteManager.SetFrame(0);

			//player has 3 children. two eyes and the head holder
			eye1 = new Eye(new Vector2(-34, -21), 7, 0.14f);
			eye2 = new Eye(new Vector2(9, -19), eye1);
			AddChild(eye1);
			AddChild(eye2);

			// the head holder holds the head gameobject, the chicken (only visible when a chicken is sucked in), 
			// and a gameobject called 'fix' (there was a weird black thing added to the head sprite when it was exported from adobe animate, this is just a strip of color that covers that black line for ez fix)
			headHolder = new GameObject(TextureName.Head, headOffset, 1, 0, this, false);
			headHolder.GetSprite().SetSort(0);

			head = new GameObject(TextureName.Head, Vector2.zero, 1, 0, headHolder);
			//might as well get a reference to the head sprite since it's animation is controlled through the player.
			headSprite = new Sprite(Sprite.GetFramesFromFolder("PlayerHead"), 30, 0, 9, head, RLColor.WHITE, true, 0);
			head.SetSprite(headSprite);
			headSprite.Pause();

			fix = new GameObject(TextureName.HeadFix, Vector2.up * -150, 1, 0, head)
			{
				LocalScale = new Vector2(1, 3) //just learnt that you can do this. pretty cool huh?
			};

			chicken = new GameObject(TextureName.CookedChicken, eatingOffset, 0.5f, 0, headHolder);
			chicken.GetSprite().SetSort(-1);
			chicken.SetDrawn(false);

			SortChildren();
			headHolder.SortChildren();

			LocalPosition = Vector2.one * 100;
			collider.SetCollisionLayer(CollisionLayer.Player);
			spriteManager.SetLayer(SpriteLayer.Midground);
			defaultIMass = iMass;

			//some parts of the hunger holder is controlled through the player
			h = new HungerHolder(TextureName.Xray, new Vector2(Game.screenWidth - 100, 130), 1, null);
			
			Game.GetCurrentScene().AddUIElement(h);
		}

		

		public override void Update()
		{
			accelerationCap = accelerationCapNorm; //accelerationcap is added to when player is running

			//new game plus is activated when the player eats every chicken in the game
			//when activated, the player can shoot infinite eggs, has a really high mass, is bigger, shakes the ground as it walks, and is a bit slower (also is bigger)
			//it also flings 'confetti' everywhere when it walks.
			//the programming for this is a bit dumb because at first if you shot out an egg newgameplus was disabled again, so everything had to revert back. 
			//this is no longer what i want of course since i like the idea of shooting infinite chickens out when you win
			//but i didn't fix everything again because im lazy AND I needed to do professional studies work
			if (newGamePlus)
			{
				velocityCap = velocityCapSlow;
				frameSpeed = frameSpeedNorm - frameSpeedSlowDown;
				scale = new Vector2(1.05f, 1.05f);
				int f = spriteManager.GetCurrentFrame();
				iMass = 0.000001f;
				Eye.laserPower = 200000000; //laser power is set to this constant
				Eye.cookStrength = 50;
				chickenEatAmount = 200;
				//shake the camera and shoot rainbow feathers out when the feet hit the ground 
				if ((f >= 1 && f <= 3) || (f >= 12 && f <= 14))
				{
					camera.SetShakeAmount(4);
					new Feather(position + new Vector2(Game.globalRand.Next(-20,20), 110), (float)Game.globalRand.NextDouble() * Num.pi * 2 - Num.pi, (float)Game.globalRand.NextDouble() * 0.2f + 0.1f, -3f * new Vector2(velocity.x, (float)Game.globalRand.NextDouble() * 50 - 25), (float)Game.globalRand.NextDouble() - 0.5f, Game.GetCurrentScene(), SpriteLayer.Foreground, true);
				}
				else
				{
					camera.SetShakeAmount(0); //this actually breaks the laser's camera shake but whatever, member I'm adding all these comments after the fact
				}
			}
			else
			{
				//set all values to their defaults (every frame).
				//should have made it that eye contained its own default laserpower and cookstrength consts 
				Eye.cookStrength = 5;
				iMass = defaultIMass;
				Eye.laserPower = 20000000;
				velocityCap = velocityCapNorm;
				frameSpeed = frameSpeedNorm;
				scale = Vector2.one;
			}

			//input direction is reset every frame. using WASD changes it.
			inputDirection = Vector2.zero;
			
			if (IsKeyDown(KeyboardKey.KEY_W))
			{
				inputDirection -= Vector2.up;
			}
			if (IsKeyDown(KeyboardKey.KEY_S))
			{
				inputDirection += Vector2.up;
			}
			if (IsKeyDown(KeyboardKey.KEY_A)) //A and S also change a bunch of variables to do with flipping the character
			{
				inputDirection -= Vector2.right;
				spriteManager.FlipX(false);
				headSprite.FlipX(false);
				fix.GetSprite().FlipX(false);
				eye1.FlipX(false);
				chicken.GetSprite().FlipX(false);
				chicken.LocalPosition = new Vector2(eatingOffset.x, eatingOffset.y);
				headHolder.LocalPosition = new Vector2(headOffset.x, LocalPosition.y);
			}
			if (IsKeyDown(KeyboardKey.KEY_D))
			{
				inputDirection += Vector2.right;
				spriteManager.FlipX(true);
				headSprite.FlipX(true);
				fix.GetSprite().FlipX(true);
				eye1.FlipX(true);
				chicken.GetSprite().FlipX(true);
				chicken.LocalPosition = new Vector2(-eatingOffset.x, eatingOffset.y);
				headHolder.LocalPosition = new Vector2(-headOffset.x, LocalPosition.y);
			}

			//shift is used for running. when running the velocitycap, acceleration cap and frame speed are made bigger.
			if (IsKeyPressed(KeyboardKey.KEY_LEFT_SHIFT))
			{
				running = true;
			}
			if (IsKeyReleased(KeyboardKey.KEY_LEFT_SHIFT))
			{
				running = false;
			}
			if (running)
			{
				frameSpeed -= frameSpeedAddition;
				velocityCap += velocityFasterAmount;
				accelerationCap += accelerationFasterAmount;
			}
			spriteManager.SetSpeed(frameSpeed);

			//now for lasering
			//lasering can only happen when not biting
			if (!biting && IsMouseButtonPressed(MouseButton.MOUSE_LEFT_BUTTON))
			{
				cooling = false;
				charging = true;
			}
			if (charging)
			{
				buildUp += Game.deltaTime * chargeSpeed;

				while (buildUp >= 1)
				{
					if (eyeColor.r >= 255)
					{
						eyeColor.r = 255;
						charging = false;
						buildUp = 0;
						eye1.SetLaser(true);
						camera.SetShakeAmount(5);
						break;
					}
					eyeColor.r++;
					buildUp--;
					eye1.SetTint(eyeColor);
					camera.SetShakeAmount(eyeColor.r * 0.005f);
				}
			}
			if (cooling)
			{
				buildUp += Game.deltaTime * chargeSpeed * 2;

				while (buildUp >= 1)
				{
					if (eyeColor.r == 0)
					{
						cooling = false;
						buildUp = 0;
						break;
					}

					eyeColor.r--;
					buildUp--;
					eye1.SetTint(eyeColor);
					camera.SetShakeAmount(eyeColor.r * 0.005f);
				}
			}
			if (IsMouseButtonReleased(MouseButton.MOUSE_LEFT_BUTTON))
			{
				cooling = true;
				charging = false;
				eye1.SetLaser(false);
				buildUp = 0;
			}

			//biting can only happen when leftclick isn't held (cannot be charging or lasering)
			if (!charging && IsMouseButtonPressed(MouseButton.MOUSE_RIGHT_BUTTON) && eyeColor.r != 255)
			{
				//when first clicked the headsprites limits values are set so that the jaw opens
				headSprite.SetLimits(0, 5);
				headSprite.SetBackwards(false);
				headSprite.PlayFrom((headSprite.GetCurrentFrame() + 1) % 5);
				biting = true;
				endingBite = false;
				broadcastingHunger = true;
				camera.SetShakeAmount(0);
			}
			if (biting) //biting is basically true at any point that the mousth is not at frame 0 (shut)
			{
				int fr = headSprite.GetCurrentFrame();
				UpdateTiedChickens(); //update tied chickens if mouth is becoming agape
				
				//if a chicken is dead, the camera shakes based on how open the mouth is
				//frame five is when the mouth is completely open
				if (chickensBeingSucked)
				{
					if (fr > 5)
						camera.SetShakeAmount( 0.5f * (5 - fr % 5));
					else
						camera.SetShakeAmount(0.3f * (fr));
				}

				//if rightclick has been released, the frames either start reversing toward 0 or continue toward frame 10
				if (endingBite)
				{
					if (fr <= 0) //if it reverses towards zero, when it reaches zero disable everything to do with biting. the bite has been canceled
					{
						headSprite.Pause();
						headSprite.SetBackwards(false);
						biting = false;
						endingBite = false;
						chickensBeingSucked = false;
						broadcastingHunger = false;
						camera.SetShakeAmount(0);
					}

					if (fr >= 10) //if it continues towards 10, the 'chomp' is being played. reset everything and if the chickens eaten during this bite is more than 0, make a not of it.
					{
						headSprite.Pause();
						headSprite.SetFrame(0);
						endingBite = false;
						biting = false;
						chickensBeingSucked = false;
						broadcastingHunger = false;
						camera.SetShakeAmount(0);
						chickensEatenTotal += chickenEatAmount;
						h.hungerPercent = (float)chickensEatenTotal / chickenTotalInScene;

						//if every chicken has been eaten, newGamePlus is enabled and infinite chickens can now be shot out
						if (newGamePlus || chickensEatenTotal >= chickenTotalInScene)
						{
							newGamePlus = true;
							chickenEatAmount = 300;
						}
						else //else the chicken eat amount is reset at the end of the bite
						{
							chickenEatAmount = 0;
						}
						
						//the chicken gameobject is visible (due to a chicken class being completely eaten), disable the chicken 
						chicken.SetDrawn(false);					
					}
					else if (fr > 6)
					{
						//at the end of the bite make a crunch-like camera shake
						camera.SetShakeAmount(4);
					}
				}
				else if (fr == 5) //if the mouth becomes fully agape stay it their until the right mouse button has been released
				{
					headSprite.Pause();

					//at this point eggs can be shot using the left mouse button as long as the player has eaten a chicken.
					if (chickensEatenTotal > 0 && IsMouseButtonPressed(MouseButton.MOUSE_LEFT_BUTTON))
					{
						//the chicken is shot towards the mouse and a bunch of feathers are shot in the general direction as well
						Vector2 direction = (camera.GetMouseWorldPosition() - chicken.GlobalPosition).Normalised() * 500;
						new PhysicsChicken(chicken.GlobalPosition, direction, 0.1f, 3);
						Vector2 rot = direction.Rotated(-Num.pi / 8);
						for (int i = 0; i < 15; i++)
						{
							new Feather(position, (float)Game.globalRand.NextDouble() * Num.pi * 2 - Num.pi, (float)Game.globalRand.NextDouble() * 0.2f + 0.1f, (float)(Game.globalRand.NextDouble() * 0.5f + 1) * rot.Rotated((float)Game.globalRand.NextDouble() * 0.1f - 0.05f), (float)Game.globalRand.NextDouble() - 0.5f, Game.GetCurrentScene(), SpriteLayer.Foreground);
							rot.Rotate(Num.pi * 0.01675f);
						}
						if (!newGamePlus) //if the game is in newGamePlus this does not effect the hunger level. 
						{
							chickensEatenTotal--;
							h.hungerPercent = (float)(chickensEatenTotal) / chickenTotalInScene;
							h.chickenCount = chickenTotalInScene - chickensEatenTotal;
						}
					}
				}
			}
			if (IsMouseButtonReleased(MouseButton.MOUSE_RIGHT_BUTTON) && !endingBite)
			{
				if (headSprite.GetCurrentFrame() >= 5)
				{
					//initiate close animation
					headSprite.SetLimits(5, 10);
					headSprite.Play();
					CancelTiedChickens(); //if their are chickens left after holding right click, cancel the vacuum effect
				}
				else
				{
					//if the mouth hadn't fully opened yet, just reverse the animation to close the mouth softly
					headSprite.SetBackwards(true);
				}
				endingBite = true;
			}

			//if input has been given but the character is still standing, animate the character and disable standing.
			if (standing && (inputDirection.x != 0 || inputDirection.y != 0))
			{
				standing = false;
				spriteManager.PlayFrom(1);
			}

			//if their is no input yet the character is moving, make it stand still
			if ((inputDirection.x == 0 && inputDirection.y == 0)&& !standing)
			{
				spriteManager.Pause();
				spriteManager.SetFrame(0);
				standing = true;
			}

			//eyes are offset by the eye offset and the head offset
			//head is only offset by the eye offset
			float offset = eyeOffset[spriteManager.GetCurrentFrame()];
			eye1.SetOffsetY(offset + headEyeOffset[headSprite.GetCurrentFrame()]);
			headHolder.LocalPosition = new Vector2(headHolder.LocalPosition.x, offset + headOffset.y);
			
			// it basically gives a velocity that points from the current velocity to the aimed velocity (which has the direction of input direction and the magnitude of velocity cap)
			//the velocity can only change by a vector with a max value of acceleration cap. If the distance between the aimed velocity and the current velocity is less than accelerationCap, the current velocity becomes the aimed velocity
			//just as in the chicken class, the implementation of this is not very clean and could be easily remade to be better
			
			Vector2 cache = inputDirection.Normalised() * velocityCap - velocity;
			
			if (cache.MagnitudeSquared() > accelerationCap * Game.deltaTime * accelerationCap * Game.deltaTime)
				inputVelocity = cache.Normalised() * (accelerationCap * Game.deltaTime);
			else
			{
				inputVelocity = cache;
			}
			inputVelocity = velocityCap > accelerationCap * Game.deltaTime ? inputVelocity : cache + velocity;
			AddVelocity(inputVelocity);
			
			base.Update();

			//lineofsight is a static class. the origin is set to the player + an offset
			LineOfSight.SetOrigin(LocalPosition + lOSOffset);

			if (!newGamePlus) //if newgameplus this value is not ever updated
				h.chickenCount = chickenTotalInScene - chickensEatenTotal;

		}

		Vector2 lOSOffset = new Vector2(0, 50);

		public void TieDeadChicken(Chicken chicken) //chicken is added to this when it is supposed to be eased towards the player's open mouth
		{
			if (chicken.IsDead)
			{
				suckedChickens.Add(new SuckedChickenInfo(chicken, Game.currentTime, chicken.LocalPosition));
				chicken.SetSortingOffset(1000);
			}
		}

		void UpdateTiedChickens()
		{
			if (suckedChickens.Count > 0)
			{
				float amountThrough;
				Vector2 delta;
				for (int i = 0; i < suckedChickens.Count; i++)
				{
					amountThrough = (Game.currentTime - suckedChickens[i].timeAtStart) * 0.005f * chickenSuckSpeed;
					delta = chicken.GlobalPosition - suckedChickens[i].positionAtStart;
					
					if (amountThrough >= 1) //when target is reached the chicken is deleted and the chicken object in the players mouth is made visible
					{
						suckedChickens[i].chicken.Delete();
						suckedChickens.RemoveAt(i);
						i--;
						chicken.SetDrawn(true);
						chickenEatAmount++;
						continue;
					}

					//easing used is ease in ease out cubic
					//https://easings.net/#easeInOutCubic
					if (amountThrough < 0.5f)
					{
						delta.x *= 4 * amountThrough * amountThrough * amountThrough;
						delta.y *= 4 * amountThrough * amountThrough * amountThrough;
					}
					else
					{
						delta.x *= 1 - (-2 * amountThrough + 2)  * (-2 * amountThrough + 2)  * (-2 * amountThrough + 2) / 2;
						delta.y *= 1 - (-2 * amountThrough + 2)  * (-2 * amountThrough + 2)  * (-2 * amountThrough + 2) / 2;
					}
					suckedChickens[i].chicken.LocalPosition = suckedChickens[i].positionAtStart + delta;
				}
			}
		}

		void CancelTiedChickens()
		{
			for (int i = 0; i < suckedChickens.Count; i++)
			{
				suckedChickens[i].chicken.CancelSuck();
			}
			suckedChickens.Clear();
		}

		public Vector2 GetChickenPos()
		{
			return chicken.GlobalPosition;
		}

		public GameObject GetChicken()
		{
			return chicken;
		}

		public void SetTiedCamera(SmoothCamera camera)
		{
			this.camera = camera;
		}

		public SmoothCamera GetTiedCamera()
		{
			return camera;
		}

		class SuckedChickenInfo //holds all info needed to move chicken towards mouth using easing
		{
			public Chicken chicken;
			public Vector2 positionAtStart;
			public float timeAtStart;

			public SuckedChickenInfo(Chicken chicken, float timeAtStart, Vector2 positionAtStart)
			{
				this.chicken = chicken;
				this.timeAtStart = timeAtStart;
				this.positionAtStart = positionAtStart;
			}
		}
	}
}
