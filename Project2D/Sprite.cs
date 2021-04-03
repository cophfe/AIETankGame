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
	class Sprite
	{
		float timer = 0;
		float timeCap = 0;
		float lastTime;
		int startFrame = 0;
		int lastFrame = 0;
		Texture2D[] frames;
		int currentFrame = 0;
		bool animated = false;
		bool pause = false;
		Rectangle spriteRectangle;
		Rectangle textureRectangle;
		Vector2 origin;
		float sort = 0;
		Layers layer = 0;
		Colour tint = new Colour(255, 255, 255, 255);
		bool flipX = false;
		bool flipY = false;
		Rectangle flipped;
		GameObject attachedGameObject;

		public Sprite(Texture2D[] frames, float millisecondsEach, int startFrame, int lastFrame, GameObject attached)
		{
			animated = true;
			timeCap = millisecondsEach;
			this.frames = frames;
			textureRectangle = new Rectangle { height = frames[0].height, width = frames[0].width };
			attachedGameObject = attached;
			this.startFrame = startFrame;
			this.lastFrame = lastFrame;
		}
		public Sprite(Texture2D sprite, GameObject attached)
		{
			frames = new Texture2D[1] { sprite };
			textureRectangle = new Rectangle { height = sprite.height, width = sprite.width };
			attachedGameObject = attached;
		}

		public float GetSort()
		{
			return sort;
		}

		public int GetLayer()
		{
			return (int)layer;
		}

		public Texture2D CurrentTexture()
		{
			return frames[currentFrame];
		}

		public void Draw()
		{
			if (animated && !pause)
			{
				timer += Game.currentTime - lastTime;
				while (timer > timeCap)
				{
					timer -= timeCap;
					++currentFrame;
					if (currentFrame == lastFrame)
						currentFrame = startFrame;
				}
				lastTime = Game.currentTime;
			}

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

		public void SetLimits(int start, int end)
		{
			startFrame = start;
			lastFrame = end;
		}

		public int CurrentFrame()
		{
			return currentFrame;
		}

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
	}

	enum TextureName
	{
		None,
		Crate,
		Arm,
		Player,
		Grid,
		Pupil
	}
}
