using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mlib;
using Raylib;
using static Raylib.Raylib;

namespace Project2D
{
	/// <summary>
	/// The image drawer attached to every game object
	/// </summary>
	class Sprite
	{
		float timer = 0; //animation timer
		float timeCap = 0; //when the timer reaches this value, the next frame plays
		float lastTime; //the last time found in update()
		int startFrame = 0; //the first frame of the loop
		int lastFrame = 0; //the last frame of the loop
		Texture2D[] frames; //the list of all frames (only one if not animated) 
		int currentFrame = 0;
		bool isAnimated = false; //isAnimated is different from pause. if it isn't animated it can never be played through the sprite object. if it is paused it can be played and paused freely 
		bool pause = false;
		Rectangle textureRectangle; //the rectangle of information taken from the sprite. there is only one, so all textures must be the same size
		Rectangle spriteRectangle; //the rectangle where the texture will be rendered on screen
		Vector2 origin;
		float sort = 0; //the sort point, used to organise when children of non-scene gameobjects are drawn
		SpriteLayer layer; //the sort layer, used for organising children of scenes
		Colour tint = new Colour(255, 255, 255, 255);
		bool flipX = false;
		bool flipY = false;
		Rectangle flipped; //the texture rectangle with all flip modifiers added
		GameObject attachedGameObject; 
		int backwardsMultiplier = 1; //changed to -1 when the sprite animated backwards

		//this constructor is used for sprites with multiple frames
		public Sprite(Texture2D[] frames, float millisecondsEach, int startFrame, int lastFrame, GameObject attached, Colour tint, bool animated = true, float sortValue = 1, SpriteLayer layer = SpriteLayer.Midground)
		{
			this.isAnimated = animated;
			timeCap = millisecondsEach;
			this.frames = frames;
			textureRectangle = new Rectangle { height = frames[0].height, width = frames[0].width };
			attachedGameObject = attached;
			this.startFrame = startFrame;
			this.lastFrame = lastFrame;
			sort = sortValue;
			this.layer = layer;
			this.tint = tint;
		}

		//this constructer is used for sprites with a single frame
		public Sprite(Texture2D sprite, GameObject attached, Colour tint, float sortValue = 1, SpriteLayer layer = 0)
		{
			frames = new Texture2D[1] { sprite };
			textureRectangle = new Rectangle { height = sprite.height, width = sprite.width };
			attachedGameObject = attached;
			sort = sortValue;
			this.layer = layer;
			this.tint = tint;
		}

		public float GetSort()
		{
			return sort;
		}

		public void SetSort(float sortValue)
		{
			sort = sortValue;
		}

		public int GetLayer()
		{
			return (int)layer;
		}

		public void SetLayer(SpriteLayer val)
		{
			layer = val;
		}

		public Texture2D CurrentTexture()
		{
			return frames[currentFrame];
		}

		public void Draw()
		{
			//frame is added to at a steady pace, it has its own deltatime for some stupid reason
			if (isAnimated && !pause)
			{
				timer += Game.currentTime - lastTime;
				if (timer > timeCap)
				{
					timer = 0;
					currentFrame += 1 * backwardsMultiplier;
				}
				lastTime = Game.currentTime;
			}

			//loops
			if (currentFrame > lastFrame)
				currentFrame = startFrame;

			//texture is rendered using the connected gameobject's global transform. using the info from it
			//the sprite rectangle is given a value.
			Vector2 globalPosition;
			Vector2 globalScale;
			float globalRotation;
			attachedGameObject.GetGlobalTransform().GetAllTransformations(out globalPosition, out globalScale, out globalRotation);
			spriteRectangle = new Rectangle { width = frames[currentFrame].width * globalScale.x, height = frames[currentFrame].height * globalScale.y, x = globalPosition.x, y = globalPosition.y };
			origin.x = spriteRectangle.width / 2;
			origin.y = spriteRectangle.height / 2;
			
			//flip modifiers are added
			flipped = textureRectangle;
			if (flipX)
			{
				flipped.width *= -1;
			}
			if (flipY)
			{
				flipped.height *= -1;
			}
			DrawTexturePro(frames[currentFrame], flipped, spriteRectangle, origin, globalRotation * Num.rad2Deg, tint);
		}

		public void Pause()
		{
			pause = true;
		}

		public void Play()
		{
			pause = false;
			lastTime = Game.currentTime;
			timer = 0;
		}

		public void PlayFrom(int frame)
		{
			pause = false;
			currentFrame = frame;
			lastTime = Game.currentTime;
			timer = 0;
		}

		public void SetFrame(int frame)
		{
			currentFrame = frame;
		}

		public void SetSpeed(float milliseconds)
		{
			timeCap = milliseconds;
		}

		public void FlipX(bool isFlipped)
		{
			flipX = isFlipped;
		}

		public void FlipY(bool isFlipped)
		{
			flipY = isFlipped;
		}

		//Inverts the animation (Note: backwards looping is not handled by the sprite))
		public void SetBackwards(bool isBackwards)
		{
			backwardsMultiplier = isBackwards ? -1 : 1;
		}

		// Sets the limits of an animation (where it starts and where it loops)
		public void SetLimits(int start, int end)
		{
			startFrame = start;
			lastFrame = end;
			if (currentFrame > end)
			{
				currentFrame = end;
			}
		}

		public int GetCurrentFrame()
		{
			return currentFrame;
		}

		public int GetWidth()
		{
			return frames[0].width;
		}

		public int GetHeight()
		{
			return frames[0].height;
		}


		// Returns all the files in a folder as a texture (note: assumes all files are images)
		public static Texture2D[] GetFramesFromFolder(string folderName)
		{
			string[] files = Directory.GetFiles($"../Images/{folderName}/");
			Texture2D[] frames = new Texture2D[files.Length];
			for (int i = 0; i < frames.Length; i++)
			{
				frames[i] = LoadTexture(files[i]);
			}
			return frames;
		}

		public void SetTint(Colour c)
		{
			tint = c;
		}

		public Colour GetTint()
		{
			return tint;
		}

		public Sprite Clone()
		{
			return new Sprite(frames, timeCap, startFrame, lastFrame, null, tint, isAnimated, sort, layer);
		}

		public void SetAttachedGameObject(GameObject attached)
		{
			attachedGameObject = attached;
		}

		public void UnloadFrames()
		{
			for (int i = 0; i < frames.Length; i++)
			{
				UnloadTexture(frames[i]);
			}
		}
	}

	// The names of all textures loaded through the Game class
	enum TextureName
	{
		None,
		Bale,
		Player,
		Pupil,
		Vignette,
		Button,
		Eye,
		Chicken,
		Feather,
		Head,
		HeadFix,
		CookedChicken,
		Wall,
		SideWall,
		Xray,
		Egg
	}
}
