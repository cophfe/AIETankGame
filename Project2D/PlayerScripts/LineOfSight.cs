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
	// The line of sight limiter 
	static class LineOfSight
	{
		static CollisionLayer[] ignoredLayers = new CollisionLayer[0]; //the colliders that the line of sight will ignore
		static List<PhysicsObject> list; //a reference to the list of physicsObjects attatched to the collision manager
		static Vector2 origin; //the point that the line of sight casts from
		static Vector2 screenOffset; //the vector from the top left of the screen to the middle
		static RLVector2[] points = new RLVector2[0];
		static Matrix3 offset = Matrix3.GetRotateZ(0.0001f); //the matrix that rotates the direction vector
		static Matrix3 offsetBack = Matrix3.GetRotateZ(-0.0001f); //same but backwards 
		static float rayLimit = 2000; //the maximum distance a ray can go
		static float centreDistLimit = rayLimit; //the distance before an object is ignored
		static CollisionManager cM; //the collisionmanager from the current scene
		static RenderTexture2D lineOfSightTexture;

		static public void Initiate(Scene scene) //initiate inside a specified scene
		{
			cM = scene.GetCollisionManager();
			list = cM.GetObjectList();
			lineOfSightTexture = LoadRenderTexture((int)Game.screenWidth * 2, (int)Game.screenHeight * 2);
			screenOffset = new Vector2(-Game.screenWidth, -Game.screenHeight);
		}

		public static void SetIgnored(params CollisionLayer[] ignored) //this is called at creation of a new camera
		{
			ignoredLayers = ignored;
		}

		public static void Update()
		{
			//NOTE: Everything here is super hard to explain without visualisation but there is a visualiser in the draw function so just uncomment it and you will understand

			//this line ignores objects on the ignored layers and objects that are too far away
			PhysicsObject[] array = list.Where(obj => !ignoredLayers.Contains(obj.GetCollider().GetLayer()) && (obj.GetCollider().GetCentrePoint() - origin).MagnitudeSquared() < centreDistLimit * centreDistLimit).ToArray();
			int count = array.Length;
			//each object has four points. since three raycasts are sent out for each point, there are 12 raycasts per gameobject
			points = new RLVector2[count * 12 + 12];
			Hit hit;
			Ray ray;
			for ( int i = 0; i < count; i++)
			{
				Collider c = array[i].GetCollider();
				if (ignoredLayers.Contains(c.GetLayer())) //huh. this should have already been done so idk why its here
				{
					continue;
				}
				Vector2[] objectPoints = c.GetGlobalPoints();

				for (int j = 0; j < objectPoints.Length; j++)
				{
					//a ray is cast 3 times for each point on an object
					//the first ray shoots directly towards the point, the second is shot slightly to the right of the point, the third is shot slightly to the left of the point
					//it is really difficult to explain why this works without visualisation, but it is basically supposed to work so that if the first ray hits the point perfectly than 
					//one of the other two raycasts should shoot past that point and hit whatever is behind it
					//this is all the necessary infomation to map out a line of sight
					//i would like to note I didn't come up with this concept, it's all over the internet, google it 

					//if the ray hits something, the point is set to be to point that was hit
					//if it doesn't hit anything than the point is set to be a point at a specified distance away in the direction of the ray
					ray = new Ray(origin, (objectPoints[j] - origin).Normalised());
					points[i * 12 + 3 * j] = cM.RayCast(ray, out hit, centreDistLimit, ignoredLayers) ? ray.direction * hit.distanceAlongRay + ray.position : ray.direction * rayLimit + ray.position;
					
					ray = new Ray(origin, offset * (objectPoints[j] - origin).Normalised());
					points[i * 12 + 3 * j + 1] = cM.RayCast(ray, out hit, centreDistLimit, ignoredLayers) ? ray.direction * hit.distanceAlongRay + ray.position : ray.direction * rayLimit + ray.position;
					
					ray = new Ray(origin, offsetBack * (objectPoints[j] - origin).Normalised());
					points[i * 12 + 3 * j + 2] = cM.RayCast(ray, out hit, centreDistLimit, ignoredLayers) ? ray.direction * hit.distanceAlongRay + ray.position : ray.direction * rayLimit + ray.position;
					
				}
			}

			//Screen Corners also need to be mapped out as though they are points on an object
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

			//update render texture with new information
			UpdateTexture();
		}

		public static void SetMaxDist(float val)
		{
			rayLimit = val;
		}

		static void UpdateTexture()
		{
			//for math.atan2 when both variables are zero, it returns the wrong thing. so it is removed.
			//then the points are ordered by their angle
			points = points.Where(p => p.y != origin.y || p.x != origin.x).OrderBy(p => -Math.Atan2(p.y - origin.y, p.x - origin.x)).ToArray();

			//a new array where everything is reletive to the origin is set.
			//[0] in the array is set to be the origin. (DrawTriangleFan requires the origin to be at that position)
			//the last value in the array is set to be the same as the first value in the array so that the final triangle is drawn to complete the loop
			//-screen offset is added to everything so that the triangle fan is drawn in the middle of the render texture instead of the top left corner
			RLVector2[] plusOrigin = new RLVector2[points.Length + 2];
			for (int i = 0; i < points.Length; i++)
			{
				plusOrigin[i + 1] = points[i] - origin - screenOffset;
			}
			plusOrigin[0] = -1 * screenOffset;
			plusOrigin[plusOrigin.Length - 1] = plusOrigin[1];

			//this basically makes a texture based on the triangle fan
			BeginTextureMode(lineOfSightTexture);
			ClearBackground(RLColor.BLACK);
			DrawTriangleFan(plusOrigin, plusOrigin.Length, RLColor.WHITE);
			EndTextureMode();
			//the triangle fan covers all the areas the player can see. those areas are drawn as white on the render texture, everything else is black
		}
		public static void Draw()
		{
			//the blendmode is temporarily set to multiplied so that the white area on the render texture appears invisible
			BeginBlendMode(BlendMode.BLEND_MULTIPLIED);
			DrawTextureRec(lineOfSightTexture.texture, new Rectangle { width = lineOfSightTexture.texture.width, height = -lineOfSightTexture.texture.height }, origin + screenOffset, RLColor.WHITE);
			EndBlendMode();

			//Some useful visualization if needed:

			//if (points.Length > 0)
			//{
			//	foreach (var p in points)
			//	{
			//		if (p != Vector2.zero)
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
