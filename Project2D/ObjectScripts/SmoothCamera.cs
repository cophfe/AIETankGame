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
	
	// A manager for Raylib's camera system
	class SmoothCamera : GameObject
	{
		Camera2D camera;
		Vector2 target;
		Vector2 offset;
		public float smoothMultiplier = 9;
		bool on = true;
		private Vector2 globalPosition;
		GameObject targetObject = null;
		Random rand = new Random(); 
		Vector2 shake = Vector2.zero;
		float shakeAmount;

		public SmoothCamera(Scene parent, Vector2 position, float rotation, float zoom, Vector2 offset, params CollisionLayer[] ignoredLineOfSight) : base (TextureName.None , position, 1, rotation, parent, false)
		{
			this.offset = new Vector2(Game.screenWidth / 2 + offset.x, GetScreenHeight() / 2 + offset.y);
			camera = new Camera2D { target = position, offset = position + new Vector2(GetScreenWidth() / 2 + offset.x, GetScreenHeight() / 2 + offset.y), zoom = zoom, rotation = rotation };
			
			//lineofsight ignores some collisions
			LineOfSight.SetIgnored(ignoredLineOfSight);

			//a vignette is rendered as a child object of the camera. please note I added this before I added UI to the scene, that's why it is not a UI object
			GameObject vignette = new GameObject(TextureName.Vignette);
			AddChild(vignette);
			vignette.GetSprite().SetLayer(SpriteLayer.Foreground);
			vignette.LocalScale = new Vector2(Game.screenWidth, Game.screenHeight) / (1120);
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
			//if a target object is specified, the target is set to it's position every update
			//otherwise the target is set by other objects
			if (targetObject != null)
			{
				Target(targetObject.GlobalPosition);
			}

			//camera should slowly move toward the target position
			globalPosition = GlobalPosition;
			if (globalPosition != target)
			{
				GlobalPosition = globalPosition + (target - globalPosition) * Game.deltaTime * smoothMultiplier;
			}

			//a shake variable is added based on the shake amount int. it is entirely random.
			shake = new Vector2((float)(rand.NextDouble() * 2 - 1) * shakeAmount, (float)(rand.NextDouble() * 2 - 1) * shakeAmount);
			
			camera.target = GlobalPosition;
			camera.offset = offset - GlobalPosition + shake;
			camera.rotation = GlobalRotation;
			
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
			if (Game.lOS)
				LineOfSight.Draw();
		}

		public void UpdateLineOfSight()
		{
			if (Game.lOS)
			{
				LineOfSight.Update();
			}
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

		public Vector2 GetMouseWorldPosition() //GetMousePosition() does not factor in camera offset, this function does
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
			//does not draw anything through here, camera children are drawn just before 2D camera is ended, basically acting as UI objects
		}
	}
}
