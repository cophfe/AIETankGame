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
		float timer = 0;
		float timeCap = 0;
		float lastTime;
		int startFrame = 0;
		int lastFrame = 0;
		Texture2D[] frames;
		int currentFrame = 0;
		bool isAnimated = false;
		bool pause = false;
		Rectangle spriteRectangle;
		Rectangle textureRectangle;
		Vector2 origin;
		float sort = 0;
		SpriteLayer layer;
		Colour tint = new Colour(255, 255, 255, 255);
		bool flipX = false;
		bool flipY = false;
		Rectangle flipped;
		GameObject attachedGameObject;
		int backwardsMultiplier = 1;

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
			if (currentFrame > lastFrame)
				currentFrame = startFrame;

			Vector2 globalPosition;
			Vector2 globalScale;
			float globalRotation;
			attachedGameObject.GetGlobalTransform().GetAllTransformations(out globalPosition, out globalScale, out globalRotation);

			spriteRectangle = new Rectangle { width = frames[currentFrame].width * globalScale.x, height = frames[currentFrame].height * globalScale.y, x = globalPosition.x, y = globalPosition.y };
			origin.x = spriteRectangle.width / 2;
			origin.y = spriteRectangle.height / 2;
			flipped = textureRectangle;
			if (flipX)
			{
				flipped.width *= -1;
			}
			if (flipY)
			{
				flipped.height *= -1;
			}
			DrawTexturePro(frames[currentFrame], flipped, spriteRectangle, origin, globalRotation * Trig.rad2Deg, tint);
		}

		/// <summary>
		/// Pauses the sprite's animation
		/// </summary>
		public void Pause()
		{
			pause = true;
		}

		/// <summary>
		/// Resumes the sprite's animation after being paused
		/// </summary>
		public void Play()
		{
			pause = false;
			lastTime = Game.currentTime;
			timer = 0;
		}

		/// <summary>
		/// Plays the animation from a specified frame
		/// </summary>
		public void PlayFrom(int frame)
		{
			pause = false;
			currentFrame = frame;
			lastTime = Game.currentTime;
			timer = 0;
		}

		/// <summary>
		/// Sets the current frame to a value
		/// </summary>
		public void SetFrame(int frame)
		{
			currentFrame = frame;
		}

		/// <summary>
		/// Sets the number of milliseconds per frame
		/// </summary>
		public void SetSpeed(float milliseconds)
		{
			timeCap = milliseconds;
		}

		/// <summary>
		/// Sets whether the sprite is flipped on the X axis
		/// </summary>
		public void FlipX(bool isFlipped)
		{
			flipX = isFlipped;
		}

		/// <summary>
		/// Sets whether the sprite is flipped on the Y axis
		/// </summary>
		public void FlipY(bool isFlipped)
		{
			flipY = isFlipped;
		}

		/// <summary>
		/// Inverts the animation (Note: backwards looping is not handled by the sprite))
		/// </summary>
		public void SetBackwards(bool isBackwards)
		{
			backwardsMultiplier = isBackwards ? -1 : 1;
		}

		/// <summary>
		/// Sets the limits of an animation (where it starts and where it loops)
		/// </summary>
		public void SetLimits(int start, int end)
		{
			startFrame = start;
			lastFrame = end;
			if (currentFrame > end)
			{
				currentFrame = end;
			}
		}

		/// <summary>
		/// Returns the current frame
		/// </summary>
		public int GetCurrentFrame()
		{
			return currentFrame;
		}

		/// <summary>
		/// Returns the width of the sprite
		/// </summary>
		public int GetWidth()
		{
			return frames[0].width;
		}

		/// <summary>
		/// Returns the height of the sprite
		/// </summary>
		public int GetHeight()
		{
			return frames[0].height;
		}

		/// <summary>
		/// Returns all the files in a folder as a texture (note: assumes all files are images)
		/// </summary>
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

		/// <summary>
		/// Sets the tint of the sprite
		/// </summary>
		public void SetTint(Colour c)
		{
			tint = c;
		}

		/// <summary>
		/// Returns the tint of a sprite
		/// </summary>
		public Colour GetTint()
		{
			return tint;
		}

		/// <summary>
		/// Clones the sprite's values into a new sprite
		/// </summary>
		public Sprite Clone()
		{
			return new Sprite(frames, timeCap, startFrame, lastFrame, null, tint, isAnimated, sort, layer);
		}

		/// <summary>
		/// Sets the gameobject attatched to this sprite
		/// </summary>
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

	/// <summary>
	/// The names of all available textures
	/// </summary>
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
		GooBird
	}
}
