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
	/// <summary>
	/// A manager for Raylib's camera system
	/// </summary>
	class SmoothCamera : GameObject
	{
		Camera2D camera;
		Vector2 target;
		Vector2 offset;
		public float smoothMultiplier = 9;
		bool on = true;
		bool lineOfSight = true;
		private Vector2 globalPosition;
		GameObject targetObject = null;
		Random rand = new Random(); 
		Vector2 shake = Vector2.Zero;
		float shakeAmount;

		public SmoothCamera(Scene parent, Vector2 position, float rotation, float zoom, Vector2 offset, bool lineOfSight = true, params CollisionLayer[] ignoredLineOfSight) : base (TextureName.None , position, 1, rotation, parent, false)
		{
			this.offset = new Vector2(Game.screenWidth / 2 + offset.x, GetScreenHeight() / 2 + offset.y);
			camera = new Camera2D { target = position, offset = position + new Vector2(GetScreenWidth() / 2 + offset.x, GetScreenHeight() / 2 + offset.y), zoom = zoom, rotation = rotation };
			this.lineOfSight = lineOfSight;
			LineOfSight.SetIgnored(ignoredLineOfSight);
			GameObject vignette = new GameObject(TextureName.Vignette);
			AddChild(vignette);
			vignette.GetSprite().SetLayer(SpriteLayer.Foreground);
			vignette.LocalScale = new Vector2(Game.screenWidth, Game.screenHeight) / (1150);
			LineOfSight.SetMaxDist(new Vector2(Game.screenWidth/2, Game.screenHeight/ 2).Magnitude() + smoothMultiplier * 15);
			parent.SetCamera(this);
			LineOfSight.Initiate(parent);
		}

		public SmoothCamera()
		{
			on = false;
		}

		public override void Update()
		{
			if (targetObject != null)
			{
				Target(targetObject.GlobalPosition);
			}

			globalPosition = GlobalPosition;
			if (globalPosition != target)
			{
				GlobalPosition = globalPosition + (target - globalPosition) * Game.deltaTime * smoothMultiplier;
			}

			shake = new Vector2((float)(rand.NextDouble() * 2 - 1) * shakeAmount, (float)(rand.NextDouble() * 2 - 1) * shakeAmount);
			
			camera.target = GlobalPosition;
			camera.offset = offset - GlobalPosition + shake;
			camera.rotation = GlobalRotation;

			if (lineOfSight)
			{
				//(target should be inside objects collisionBox)
				target.y += 59;
				LineOfSight.Update();
			}
		}

		public void StartCamera()
		{	
			if (on)
			{
				BeginMode2D(camera);
			}
			
		}

		public void DrawLineOfSight()
		{
			if (lineOfSight)
				LineOfSight.Draw();
		}

		public void Target(GameObject obj)
		{
			targetObject = obj;
		}

		public void Target(Vector2 position)
		{
			target = position;
		}

		public Vector2 GetOffset()
		{
			return offset;
		}

		public Vector2 GetMouseWorldPosition()
		{
			if (on)
				return (globalTransform * Matrix3.GetTranslation(GetMousePosition())).GetTranslation() - GetOffset();
			return GetMousePosition();
		}

		public void SetShakeAmount(float shake)
		{
			shakeAmount = shake;
		}

		public void EndCamera()
		{
			for (int i = 0; i < children.Count; i++)
			{
				children[0].Draw();
			}
			EndMode2D();
		}

		public override void Draw()
		{

		}
	}
}
