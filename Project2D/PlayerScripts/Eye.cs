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
	class Eye : GameObject
	{
		static Vector2 targetPosition; //aimed final position
		Vector2 cameraOffset; //the amount the camera is offset due to laser. is also used to calculate target position
		static float yOffset = 0; //the amount the eyes are offset from their target position (used when moving or biting to fit with animation)
		static Vector2 flipMultiplier = Vector2.one; //the vector the eyes centrepoint is multiplied by to make it be in the right position when turning around
		Vector2 centeredPosition; //the centrepoint of an eye
		public Vector2 middlePoint; //the point in the centre of both the eyes
		float maxDistance; //the maximum distance the eyes can move
		bool isMain = true; //whether the eye is the main eye or not (the main eye sets all static variables, the secondary does not)
		Eye main = null; //a reference to the main eye (only set if isMain is false)
		static bool isLasering = false; //a bool stating whether the eye is lasering or not (set by the player)
		Vector2 mousePos; //the mouseposition (factoring in camera movement)
		static public float laserPower = 20000000; //the amount of force the laser imparts on a collider
		static public int cookStrength = 5; //the amount of cooking a laser imparts on a chicken

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
			main.InitiateSecond(out scale);
			isMain = false;
			this.main = main;
			centeredPosition = offset;
			maxDistance = main.maxDistance;
			spriteManager.SetTint(new Colour(0, 0, 0, 255));
			main.InitiateMiddlePoint(offset);
		}

		public override void Update()
		{
			//cannot simply use Raylib.GetMousePosition() because it does not factor in camera offset, got to use the function from inside the camera 
			mousePos = Game.GetCurrentScene().GetCamera().GetMouseWorldPosition();
			//if this is the main eye, find the direction from the middlepoint to the mouse position and using that information find the target offset. clamp it by the max distance
			//if it isn't the main eye, set the tint of the eye to the main eye's tint
			if (isMain)
			{
				cameraOffset = mousePos - (middlePoint * flipMultiplier + parent.GlobalPosition);
				targetPosition = cameraOffset.MagnitudeSquared() > maxDistance * maxDistance ? cameraOffset.Normalised() * maxDistance : cameraOffset;
			}
			else
			{	
				spriteManager.SetTint(main.spriteManager.GetTint());
			}
			//update position
			position = centeredPosition * flipMultiplier + targetPosition;
			position.y += yOffset;
			base.Update();
		}

		public void InitiateMiddlePoint(Vector2 offset)
		{
			//since middlepoint isn't static, it needs to be copied from the main eye to the secondary eye. this is used at initiation
			middlePoint = new Vector2(Num.Lerp(centeredPosition.x, offset.x, 0.5f), Num.Lerp(centeredPosition.y, offset.y, 0.5f));
		}
		public void InitiateSecond(out Vector2 scale)
		{
			//same thing as middlepoint
			scale = this.scale;
		}

		public Vector2 GetTarget()
		{
			return targetPosition;
		}

		static Random rand = new Random();

		public override void Draw()
		{
			//the eyes control the camera's target position
			//recieve the red value (if it is more than one, the laser is charging or cooling. if it is 255, the laser is charged)
			float redVal = spriteManager.GetTint().GetRed();
			if (redVal > 0)
			{
				if (isMain)
				{
					Vector2 camTarget = parent.LocalPosition;
					
					//clamp offset to 500 (should be a var tbh)
					cameraOffset = cameraOffset.MagnitudeSquared() > 500 * 500 ? cameraOffset.Normalised() * 500 : cameraOffset;

					if (redVal == 255) //the camera gets offset further if the laser is fully charged
						redVal *= 2;

					//should also use vars here tbh. i dont remember what that decimal value comes from
					//linearly interpolate between zero and the camera offset + middlepoint value, and add it on to the previous camera target (player's localPos)
					camTarget += Vector2.Lerp(Vector2.zero, middlePoint + cameraOffset, redVal * 0.000192156863f);

					//the player holds the camera. target the camera to the new position
					(parent as Player).GetTiedCamera().Target(camTarget);
				}
			}
			else if (parent is Player) //technically the player doesn't have to be a player, so this checks before targeting the camera.
				(parent as Player).GetTiedCamera().Target(parent.LocalPosition);
			
			//this is where lasering is controlled, each eye does it individually
			//lasers are seperate from eye looking for better control purposes, each eye shoots its own ray at the mouse position and has its own response
			//bool is enabled by player
			if (isLasering)
			{
				//shoot a ray
				Ray ray = new Ray(GlobalPosition, (mousePos - GlobalPosition).Normalised());
				if (Game.GetCurrentScene().GetCollisionManager().RayCast(ray, out Hit hit, 4000, CollisionLayer.Player))
				{
					//draw line to hit position from start position
					DrawLineEx(ray.position, ray.direction * hit.distanceAlongRay + ray.position, 10, RLColor.RED);
					if (hit.objectHit.GetCollider().GetLayer() == CollisionLayer.Enemy) //check if collide with chicken
					{
						//cook chicken if hit
						if (hit.objectHit is Chicken) 
							(hit.objectHit as Chicken).cookedValue += Game.deltaTime * cookStrength;
						//randomly shoot feathers sometimes
						if (rand.NextDouble() * Game.deltaTime < 0.0002) 
							new Feather(hit.objectHit.LocalPosition + new Vector2((float)rand.NextDouble() * 20 - 10, (float)rand.NextDouble() * 20 - 10), (float)rand.NextDouble() * Num.pi * 2 - Num.pi, (float)rand.NextDouble() * 0.2f + 0.1f, -700 * ray.direction.Rotated((float)rand.NextDouble() * 1f - 0.5f), (float)rand.NextDouble() - 0.5f, Game.GetCurrentScene());
					}
					else
					{
						//if not chicken, add impulse at position on hit object (equivelent to adding force at position since deltatime is used here)
						hit.objectHit.AddImpulseAtPosition(ray.direction * laserPower * Game.deltaTime, (ray.direction * hit.distanceAlongRay + ray.position) - hit.objectHit.GetCollider().GetCentrePoint());
					}
				}
				else
					DrawLineEx(GlobalPosition, (GlobalPosition + ray.direction * 1000), 10, RLColor.RED); //if the raycast misses, just render a 1000 unit long line
			}
			
			base.Draw();
		}

		public void SetLaser(bool isLasering)
		{
			Eye.isLasering = isLasering;
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
