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
	enum CameraMode2D
	{
		Smooth,
		Direct,
		ScreenEdge
		
	}
	class SmoothCamera : GameObject
	{
		Camera2D camera;
		public CameraMode2D mode = CameraMode2D.Smooth;
		Vector2 target;
		Vector2 offset;
		public float SmoothMultiplier = 5;
		bool on = true;
		private Vector2 globalPosition;
		GameObject targetObject = null;
		public SmoothCamera(GameObject parent, Vector2 position, float rotation, float zoom, Vector2 offset) : base (TextureName.None , position, Vector2.One, rotation, parent, false)
		{
			this.offset = new Vector2(GetScreenWidth() / 2 + offset.x, GetScreenHeight() / 2 + offset.y);
			camera = new Camera2D { target = position, offset = position + new Vector2(GetScreenWidth() / 2 + offset.x, GetScreenHeight() / 2 + offset.y), zoom = zoom, rotation = rotation };
			
		}

		public void SetMode(CameraMode2D mode)
		{
			this.mode = mode;
		}

		public override void LateUpdate(float deltaTime)
		{
			if (targetObject != null)
			{
				Target(targetObject.GlobalPosition);
			}

			globalPosition = GlobalPosition;
			if (globalPosition != target)
			{
				GlobalPosition = globalPosition + (target - globalPosition) * deltaTime * SmoothMultiplier;
			}

			camera.target = GlobalPosition;
			camera.offset = offset - GlobalPosition;
			camera.rotation = GlobalRotation;

		}

		public void StartCamera()
		{	
			if (on)
				BeginMode2D(camera);
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
			return (globalTransform * Matrix3.GetTranslation(GetMousePosition())).GetTranslation() - GetOffset();
		}

		public void EndCamera()
		{
			EndMode2D();
		}
	}
}
