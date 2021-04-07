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
	/// A child object of the player that follows the mouse cursor and enables laser shooting
	/// </summary>
	class Eye : GameObject
	{
		Vector2 targetPosition;
		Vector2 cameraOffset;
		float yOffset = 0;
		Vector2 flipMultiplier = Vector2.One;
		Vector2 centeredPosition;
		public Vector2 middlePoint;
		float maxDistance;
		bool isMain = true;
		Eye main = null;
		bool isLasering = false;
		Vector2 mousePos;
		public Eye(Vector2 offset, float maxDistance, float size) : base(TextureName.Pupil)
		{
			centeredPosition = offset;
			position = offset;
			LocalScale = new Vector2(size, size);
			this.maxDistance = maxDistance;
			spriteManager.SetTint(new Colour(0, 0, 0, 255));
		}

		public Eye(Vector2 offset, Eye main) : base(TextureName.Pupil)
		{
			main.InitiateSecond(out maxDistance, out scale);
			isMain = false;
			this.main = main;
			centeredPosition = offset;
			spriteManager.SetTint(new Colour(0, 0, 0, 255));
			main.InitiateMiddlePoint(offset);
		}

		public override void Update()
		{
			mousePos = Game.GetCurrentScene().GetCamera().GetMouseWorldPosition();
			if (isMain)
			{
				cameraOffset = mousePos - (middlePoint * flipMultiplier + parent.GlobalPosition);
				targetPosition = cameraOffset.MagnitudeSquared() > maxDistance * maxDistance ? cameraOffset.Normalised() * maxDistance : cameraOffset;
			}
			else
			{
				Colour c;
				main.CopyValues(out flipMultiplier, out yOffset, out targetPosition, out isLasering, out c);
				spriteManager.SetTint(c);
			}
			position = centeredPosition * flipMultiplier + targetPosition;
			position.y += yOffset;
			base.Update();

		}

		public void InitiateMiddlePoint(Vector2 offset)
		{
			middlePoint = new Vector2(Trig.Lerp(centeredPosition.x, offset.x, 0.5f), Trig.Lerp(centeredPosition.y, offset.y, 0.5f));
		}
		public void InitiateSecond(out float dist, out Vector2 scale)
		{
			dist = maxDistance;
			scale = this.scale;
		}

		public void CopyValues(out Vector2 flip, out float yOffset, out Vector2 local, out bool laser, out Colour tint)
		{
			flip = flipMultiplier;
			yOffset = this.yOffset;
			local = targetPosition;
			laser = isLasering;
			tint = spriteManager.GetTint();
		}

		public Vector2 GetTarget()
		{
			return targetPosition;
		}

		static Random rand = new Random();

		public override void Draw()
		{
			float redVal = spriteManager.GetTint().GetRed();
			if (redVal > 0)
			{
				if (isMain)
				{
					Vector2 camTarget = parent.LocalPosition;
					
					cameraOffset = cameraOffset.MagnitudeSquared() > 500 * 500 ? cameraOffset.Normalised() * 500 : cameraOffset;

					if (redVal == 255)
						redVal *= 2;
					camTarget += Vector2.Lerp(Vector2.Zero, middlePoint + cameraOffset, 0.1f * redVal * 0.00192156863f);

					(parent as Player).GetTiedCamera().Target(camTarget);
				}
			}
			else if (parent is Player)
				(parent as Player).GetTiedCamera().Target(parent.LocalPosition);
			if (isLasering)
			{
				Ray ray = new Ray(GlobalPosition, (mousePos - GlobalPosition).Normalised());
				Hit hit;
				if (Game.GetCurrentScene().GetCollisionManager().RayCast(ray, out hit, 4000, CollisionLayer.Player))
				{
					DrawLineEx(ray.position, ray.direction * hit.distanceAlongRay + ray.position, 10, RLColor.RED);
					if (hit.objectHit.GetCollider().GetLayer() == CollisionLayer.Enemy)
					{
						if (hit.objectHit is Chicken)
							(hit.objectHit as Chicken).cookedValue += Game.deltaTime * 5;
						if (rand.NextDouble() * Game.deltaTime < 0.0002)
							new Feather(hit.objectHit.LocalPosition + new Vector2((float)rand.NextDouble() * 20 - 10, (float)rand.NextDouble() * 20 - 10), (float)rand.NextDouble() * Trig.pi * 2 - Trig.pi, (float)rand.NextDouble() * 0.2f + 0.1f, -700 * ray.direction.Rotated((float)rand.NextDouble() * 1f - 0.5f), (float)rand.NextDouble() - 0.5f, Game.GetCurrentScene());
					}
					else
					{
						hit.objectHit.AddImpulseAtPosition(ray.direction * 20000000 * Game.deltaTime, (ray.direction * hit.distanceAlongRay + ray.position) - hit.objectHit.GetCollider().GetCentrePoint());

					}
				}
				else
					DrawLineEx(GlobalPosition, (GlobalPosition + ray.direction * 1000), 10, RLColor.RED);
			}
			
			base.Draw();
		}

		public void SetLaser(bool isLasering)
		{
			this.isLasering = isLasering;
		}

		public void SetOffsetY(float y)
		{
			yOffset = y;
		}

		public void FlipY(bool isFlipped)
		{
			flipMultiplier.y = isFlipped ? -1 : 1;
		}

		public void FlipX(bool isFlipped)
		{
			flipMultiplier.x = isFlipped ? -1 : 1;
		}
	}
}
