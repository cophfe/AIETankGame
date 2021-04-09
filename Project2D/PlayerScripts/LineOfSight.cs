using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Raylib;
using static Raylib.Raylib;
using Mlib;

namespace Project2D
{
	/// <summary>
	/// The line of sight limiter (attatched to a smoothcamera)
	/// </summary>
	static class LineOfSight
	{
		static CollisionLayer[] ignoredLayers = new CollisionLayer[0];
		static List<PhysicsObject> list;
		static Vector2 origin;
		static Vector2 screenOffset;
		static RLVector2[] points = new RLVector2[0];
		static Matrix3 offset = Matrix3.GetRotateZ(0.0001f);
		static Matrix3 offsetBack = Matrix3.GetRotateZ(-0.0001f);
		static float rayLimit = 2000;
		static float centreDistLimit = rayLimit;
		static CollisionManager cM;
		static RenderTexture2D lineOfSightTexture;

		static public void Initiate(Scene scene)
		{
			cM = scene.GetCollisionManager();
			list = cM.GetObjectList();
			lineOfSightTexture = LoadRenderTexture((int)Game.screenWidth * 2, (int)Game.screenHeight * 2);
			screenOffset = new Vector2(-Game.screenWidth, -Game.screenHeight);
		}

		public static void SetIgnored(params CollisionLayer[] ignored)
		{
			ignoredLayers = ignored;
		}

		public static void Update()
		{
			PhysicsObject[] array = list.Where(obj => !ignoredLayers.Contains(obj.GetCollider().GetLayer()) && (obj.GetCollider().GetCentrePoint() - origin).MagnitudeSquared() < centreDistLimit * centreDistLimit).ToArray();
			int count = array.Length;
			points = new RLVector2[count * 12 + 12];
			Hit hit;
			Ray ray;
			for ( int i = 0; i < count; i++)
			{
				Collider c = array[i].GetCollider();
				if (ignoredLayers.Contains(c.GetLayer()))
				{
					continue;
				}
				Vector2[] objectPoints = c.GetGlobalPoints();

				for (int j = 0; j < objectPoints.Length; j++)
				{
					ray = new Ray(origin, (objectPoints[j] - origin).Normalised());
					if (cM.RayCast(ray, out hit, centreDistLimit, ignoredLayers))
					{
						points[i * 12 + 3 * j] = ray.direction * hit.distanceAlongRay + ray.position;
					}
					else
					{
						points[i * 12 + 3 * j] = ray.direction * rayLimit + ray.position;
					}
					ray = new Ray(origin, offset * (objectPoints[j] - origin).Normalised());
					if (cM.RayCast(ray, out hit, centreDistLimit, ignoredLayers))
					{
						//hit.objectHit.SetTint(RLColor.WHITE);
						points[i * 12 + 3 * j + 1] = ray.direction * hit.distanceAlongRay + ray.position;
					}
					else
					{
						points[i * 12 + 3 * j + 1] = ray.direction * rayLimit + ray.position;
					}
					ray = new Ray(origin, offsetBack * (objectPoints[j] - origin).Normalised());
					if (cM.RayCast(ray, out hit, centreDistLimit, ignoredLayers))
					{
						points[i * 12 + 3 * j + 2] = ray.direction * hit.distanceAlongRay + ray.position;
					}
					else
					{
						points[i * 12 + 3 * j + 2] = ray.direction * rayLimit + ray.position;
					}
				}
			}

			//Screen Corners
			count *= 12;
			Vector2 screenVector = new Vector2(Game.screenHeight, Game.screenWidth).Normalised();
			ray = new Ray(origin, screenVector);
			points[count] = cM.RayCast(ray, out hit, centreDistLimit, ignoredLayers) ? ray.direction * hit.distanceAlongRay + ray.position : ray.direction * rayLimit + ray.position;
			ray = new Ray(origin, offset * screenVector);
			points[count + 1] = cM.RayCast(ray, out hit, centreDistLimit, ignoredLayers) ? ray.direction * hit.distanceAlongRay + ray.position : ray.direction * rayLimit + ray.position;
			ray = new Ray(origin, offsetBack * screenVector);
			points[count + 2] = cM.RayCast(ray, out hit, centreDistLimit, ignoredLayers) ? ray.direction * hit.distanceAlongRay + ray.position : ray.direction * rayLimit + ray.position;
			
			screenVector.x = -1 * screenVector.x;
			ray = new Ray(origin, screenVector);
			points[count + 3] = cM.RayCast(ray, out hit, centreDistLimit, ignoredLayers) ? ray.direction * hit.distanceAlongRay + ray.position : ray.direction * rayLimit + ray.position;
			ray = new Ray(origin, offset * screenVector);
			points[count + 4] = cM.RayCast(ray, out hit, centreDistLimit, ignoredLayers) ? ray.direction * hit.distanceAlongRay + ray.position : ray.direction * rayLimit + ray.position;
			ray = new Ray(origin, offsetBack * screenVector);
			points[count + 5] = cM.RayCast(ray, out hit, centreDistLimit, ignoredLayers) ? ray.direction * hit.distanceAlongRay + ray.position : ray.direction * rayLimit + ray.position;

			screenVector.y = -1 * screenVector.y;
			ray = new Ray(origin, screenVector);
			points[count + 6] = cM.RayCast(ray, out hit, centreDistLimit, ignoredLayers) ? ray.direction * hit.distanceAlongRay + ray.position : ray.direction * rayLimit + ray.position;
			ray = new Ray(origin, offset * screenVector);
			points[count + 7] = cM.RayCast(ray, out hit, centreDistLimit, ignoredLayers) ? ray.direction * hit.distanceAlongRay + ray.position : ray.direction * rayLimit + ray.position;
			ray = new Ray(origin, offsetBack * screenVector);
			points[count + 8] = cM.RayCast(ray, out hit, centreDistLimit, ignoredLayers) ? ray.direction * hit.distanceAlongRay + ray.position : ray.direction * rayLimit + ray.position;

			screenVector.x = -1 * screenVector.x;
			ray = new Ray(origin, screenVector);
			points[count + 9] = cM.RayCast(ray, out hit, centreDistLimit, ignoredLayers) ? ray.direction * hit.distanceAlongRay + ray.position : ray.direction * rayLimit + ray.position;
			ray = new Ray(origin, offset * screenVector);
			points[count + 10] = cM.RayCast(ray, out hit, centreDistLimit, ignoredLayers) ? ray.direction * hit.distanceAlongRay + ray.position : ray.direction * rayLimit + ray.position;
			ray = new Ray(origin, offsetBack * screenVector);
			points[count + 11] = cM.RayCast(ray, out hit, centreDistLimit, ignoredLayers) ? ray.direction * hit.distanceAlongRay + ray.position : ray.direction * rayLimit + ray.position;

			UpdateTexture();
		}

		public static void SetMaxDist(float val)
		{
			rayLimit = val;
		}

		static void UpdateTexture()
		{
			//for math.atan2 when both variables are zero, it returns the wrong thing. so it is removed.
			points = points.Where(p => p.y != origin.y || p.x != origin.x).OrderBy(p => -Math.Atan2(p.y - origin.y, p.x - origin.x)).ToArray();

			//points = points.OrderBy(p => p.y == origin.y && p.x == origin.x ? -Trig.pi : -Math.Atan2(p.y - origin.y, p.x - origin.x)).ToArray();
			//points = points.OrderBy(p => -Math.Atan2(p.y - origin.y, p.x - origin.x)).ToArray();

			RLVector2[] plusOrigin = new RLVector2[points.Length + 2];
			for (int i = 0; i < points.Length; i++)
			{
				plusOrigin[i + 1] = points[i] - origin - screenOffset;
			}
			plusOrigin[0] = -1 * screenOffset;
			plusOrigin[plusOrigin.Length - 1] = plusOrigin[1];


			BeginTextureMode(lineOfSightTexture);
			ClearBackground(RLColor.BLACK);
			DrawTriangleFan(plusOrigin, plusOrigin.Length, RLColor.WHITE);
			EndTextureMode();

		}
		public static void Draw()
		{
			BeginBlendMode(BlendMode.BLEND_MULTIPLIED);
			DrawTextureRec(lineOfSightTexture.texture, new Rectangle { width = lineOfSightTexture.texture.width, height = -lineOfSightTexture.texture.height }, origin + screenOffset, RLColor.WHITE);
			EndBlendMode();

			//Some useful visualization if needed:
			
			//if (points.Length > 0)
			//{
			//	foreach (var p in points)
			//	{
			//		if (p != Vector2.Zero)
			//		{
			//			DrawCircle((int)p.x, (int)p.y, 4, RLColor.RED);
			//			DrawLineEx(origin, p, 2, RLColor.RED);
			//		}
			//	}
			//	for (int i = 0; i < points.Length - 1; i++)
			//	{
			//		DrawLineEx(points[i], points[i + 1], 4, RLColor.ORANGE);
			//	}
			//	DrawLineEx(points[points.Length - 1], points[0], 4, RLColor.BLUE);
			//}
		}

		public static void SetOrigin(Vector2 point)
		{
			origin = point;
		}
	}
}
