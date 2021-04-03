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
	static class LineOfSight
	{
		static CollisionLayer[] ignoredLayers = new CollisionLayer[0];
		static List<PhysicsObject> list = CollisionManager.GetObjectList();
		static Vector2 origin;
		static RLVector2[] points;
		static Matrix3 offset = Matrix3.GetRotateZ(0.0001f);
		static Matrix3 offsetBack = Matrix3.GetRotateZ(-0.0001f);
		static float rayLimit = 5000;
		public static void SetIgnored(params CollisionLayer[] ignored)
		{
			ignoredLayers = ignored;
		}

		public static void Update()
		{
			int count = list.Count;
			points = new RLVector2[count * 12 + 12];
			Hit hit;
			Ray ray;
			for ( int i = 0; i < count; i++)
			{
				Collider c = list[i].GetCollider();
				if (ignoredLayers.Contains(c.GetLayer()) || (c.GetCentrePoint() - origin).MagnitudeSquared() > rayLimit * rayLimit + 600)
				{
					continue;
				}

				Vector2[] objectPoints = c.GetGlobalPoints();

				for (int j = 0; j < objectPoints.Length; j++)
				{
					ray = new Ray(origin, (objectPoints[j] - origin).Normalised());
					points[i * 12 + 3 * j] = CollisionManager.RayCast(ray, out hit, ignoredLayers) ? ray.direction * hit.distanceAlongRay + ray.position : ray.direction * rayLimit + ray.position;
					ray = new Ray(origin, offset * (objectPoints[j] - origin).Normalised());
					points[i * 12 + 3 * j + 1] = CollisionManager.RayCast(ray, out hit, ignoredLayers) ? ray.direction * hit.distanceAlongRay + ray.position : ray.direction * rayLimit + ray.position;
					ray = new Ray(origin, offsetBack * (objectPoints[j] - origin).Normalised());
					points[i * 12 + 3 * j + 2] = CollisionManager.RayCast(ray, out hit, ignoredLayers) ? ray.direction * hit.distanceAlongRay + ray.position : ray.direction * rayLimit + ray.position;
				}
			}

			//Screen Corners
			count *= 12;
			Vector2 screenVector = new Vector2(Game.screenHeight, Game.screenWidth).Normalised();
			ray = new Ray(origin, screenVector);
			points[count] = CollisionManager.RayCast(ray, out hit, ignoredLayers) ? ray.direction * hit.distanceAlongRay + ray.position : ray.direction * rayLimit + ray.position;
			ray = new Ray(origin, offset * screenVector);
			points[count + 1] = CollisionManager.RayCast(ray, out hit, ignoredLayers) ? ray.direction * hit.distanceAlongRay + ray.position : ray.direction * rayLimit + ray.position;
			ray = new Ray(origin, offsetBack * screenVector);
			points[count + 2] = CollisionManager.RayCast(ray, out hit, ignoredLayers) ? ray.direction * hit.distanceAlongRay + ray.position : ray.direction * rayLimit + ray.position;
			
			screenVector.x = -1 * screenVector.x;
			ray = new Ray(origin, screenVector);
			points[count + 3] = CollisionManager.RayCast(ray, out hit, ignoredLayers) ? ray.direction * hit.distanceAlongRay + ray.position : ray.direction * rayLimit + ray.position;
			ray = new Ray(origin, offset * screenVector);
			points[count + 4] = CollisionManager.RayCast(ray, out hit, ignoredLayers) ? ray.direction * hit.distanceAlongRay + ray.position : ray.direction * rayLimit + ray.position;
			ray = new Ray(origin, offsetBack * screenVector);
			points[count + 5] = CollisionManager.RayCast(ray, out hit, ignoredLayers) ? ray.direction * hit.distanceAlongRay + ray.position : ray.direction * rayLimit + ray.position;

			screenVector.y = -1 * screenVector.y;
			ray = new Ray(origin, screenVector);
			points[count + 6] = CollisionManager.RayCast(ray, out hit, ignoredLayers) ? ray.direction * hit.distanceAlongRay + ray.position : ray.direction * rayLimit + ray.position;
			ray = new Ray(origin, offset * screenVector);
			points[count + 7] = CollisionManager.RayCast(ray, out hit, ignoredLayers) ? ray.direction * hit.distanceAlongRay + ray.position : ray.direction * rayLimit + ray.position;
			ray = new Ray(origin, offsetBack * screenVector);
			points[count + 8] = CollisionManager.RayCast(ray, out hit, ignoredLayers) ? ray.direction * hit.distanceAlongRay + ray.position : ray.direction * rayLimit + ray.position;

			screenVector.x = -1 * screenVector.x;
			ray = new Ray(origin, screenVector);
			points[count + 9] = CollisionManager.RayCast(ray, out hit, ignoredLayers) ? ray.direction * hit.distanceAlongRay + ray.position : ray.direction * rayLimit + ray.position;
			ray = new Ray(origin, offset * screenVector);
			points[count + 10] = CollisionManager.RayCast(ray, out hit, ignoredLayers) ? ray.direction * hit.distanceAlongRay + ray.position : ray.direction * rayLimit + ray.position;
			ray = new Ray(origin, offsetBack * screenVector);
			points[count + 11] = CollisionManager.RayCast(ray, out hit, ignoredLayers) ? ray.direction * hit.distanceAlongRay + ray.position : ray.direction * rayLimit + ray.position;

		}

		public static void SetMaxDist(float val)
		{
			rayLimit = val;
		}

		public static void Draw()
		{
			points = points.Where(ordered => ordered.x != 0 || ordered.y != 0).OrderBy(p => -Math.Atan2(p.y - origin.y, p.x - origin.x)).ToArray<RLVector2>();
			
			RLVector2[] plusOrigin = new RLVector2[points.Length + 2];
			for (int i = 0; i < points.Length; i++)
			{
				plusOrigin[i + 1] = points[i];
			}
			plusOrigin[0] = origin;
			plusOrigin[plusOrigin.Length - 1] = plusOrigin[1];

			DrawTriangleFan(plusOrigin, plusOrigin.Length, new RLColor { a = 100, r = 255, b = 255, g = 255});
			
			//Some useful visualization if needed:
			//foreach (var p in points)
			//{
			//	if (p != Vector2.Zero)
			//	{
			//		DrawCircle((int)p.x, (int)p.y, 4, RLColor.RED);
			//		DrawLineEx(origin, p, 2, RLColor.RED);
			//	}
			//}
			//for (int i = 0; i < points.Length - 1; i++)
			//{
			//	DrawLineEx(points[i], points[i + 1], 4, RLColor.ORANGE);
			//}
			//DrawLineEx(points[points.Length - 1], points[0], 4, RLColor.ORANGE);
		}

		public static void SetOrigin(Vector2 point)
		{
			origin = point;
		}
	}
}
