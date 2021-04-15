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
	// A rectangle collider stored as a width vector, a height vector, and a centre point
	class Collider
	{

		//technically this class only needs to store halfHeight, halfWidth and the centrepoint to function 
		//but it also stores some extras because the extras are used for collision and inside the collider is the place I decided to store them

		protected PhysicsObject connected; //connected physics object, used to get globalTransform
		protected Matrix3 globalTransform; 
		Vector2[] globalPoints = new Vector2[4]; //since the collider is always a rectangle, there is always four points. This isn't needed for the version of SAT I'm using but it is necessary for calculating the collision point(s) and can be used for AABB
		Vector2 globalHalfHeight; //basically the y axis from the globalTransform, with a magnitude the same as the half height of the collider
		Vector2 globalHalfWidth; //perpendicular to half height, the same thing basically but for width
		Vector2 centrePoint; //the centrepoint of the collider reletive to the connected physics object
		
		CollisionLayer layer;
		
		float halfWidth;
		float halfHeight;
		protected AABB aABB; //the axis aligned bounding box of the shape (recalculated every frame)

		public void SetConnectedPhysicsObject(PhysicsObject newConnected)
		{
			connected = newConnected;
		}
		public PhysicsObject GetConnectedPhysicsObject()
		{
			return connected;
		}
		
		public Collider(float minX, float minY, float width, float height, CollisionLayer layer = CollisionLayer.Default)
		{
			centrePoint = new Vector2(minX + width / 2, minY + height / 2);
			halfWidth = width/2;
			halfHeight = height/2;
			this.layer = layer;
		}

		public Collider(Vector2 centrePoint, float width, float height, CollisionLayer layer = CollisionLayer.Default)
		{
			this.centrePoint = centrePoint;
			halfWidth = width / 2;
			halfHeight = height / 2;
			this.layer = layer;
		}

		//gets a collider the same size as a texture
		public static Collider FromTexture(Texture2D texture, CollisionLayer layer = CollisionLayer.Default)
		{
			return new Collider(-texture.width / 2, -texture.height / 2, -texture.width, -texture.height, layer);
		}

		//same thing as FromTexture but uses TextureName enum
		public static Collider FromTextureName(TextureName textureName, CollisionLayer layer = CollisionLayer.Default)
		{
			Texture2D texture = Game.GetTextureFromName(textureName);
			return new Collider(-texture.width / 2, -texture.height / 2, texture.width, texture.height, layer);
		}

		public void UpdateAABB()
		{
			//I did this before I converted the collider to the centrepoint & halfwidth vectors system so it uses the global points instead of using the halfwidth and height vectors
			float minX = float.PositiveInfinity, maxX = float.NegativeInfinity;
			float minY = float.PositiveInfinity, maxY = float.NegativeInfinity;
			for (int i = 0; i < globalPoints.Length; i++)
			{
				maxX = globalPoints[i].x > maxX ? globalPoints[i].x : maxX; 
				minX = globalPoints[i].x < minX ? globalPoints[i].x : minX; 
				
				maxY = globalPoints[i].y > maxY ? globalPoints[i].y : maxY; 
				minY = globalPoints[i].y < minY ? globalPoints[i].y : minY; 
			}
			aABB = new AABB(minY, minX, maxY, maxX);
		}

		public AABB GetAABB()
		{
			return aABB;
		}

		public CollisionLayer GetLayer()
		{
			return layer;
		}

		public void SetCollisionLayer(CollisionLayer layer)
		{
			this.layer = layer;
		}

		public void GetMass(out float mass, out float inertia)
		{
			//super easy to get mass, just area x density:
			float area = (float)Math.Abs((halfWidth * 2) * (halfHeight * 2));
			mass =  area * connected.density;

			//moment of inertia (or rotational inertia) is harder if you want it to be physically accurate, especially since the moment of inertia is a 3D concept. (I think second moment of area * density is it's equivelent in 2D?)
			//I found an equation online that works with triangles (found below)
			//other equations are available but they are all so suspiciously inconsistant that I had a heart attack and just used the first one I found
			//I have a slightly stronger grasp on the concepts now and I think I would be able to just use the equation found here: https://en.wikipedia.org/wiki/List_of_second_moments_of_area (get the magnitude of the calculated vector and multiply it by density)
			float inert;
			//also I now realize I just had to calculate a single triangles here (instead of 2 triangles), but I didn't soo thats how the cookie gonna crumble
			Vector2 a = centrePoint + new Vector2( -halfWidth, halfHeight);
			Vector2 b = centrePoint + new Vector2( halfWidth, - halfHeight);
			float triMass = connected.density * 0.5f * Math.Abs(a.ZCross(b));
			inert = triMass * (a.MagnitudeSquared() + b.MagnitudeSquared() + a.Dot(b)) / 6;
			a = centrePoint + new Vector2(-halfWidth, -halfHeight);
			b = centrePoint + new Vector2(-halfWidth, halfHeight);
			triMass = connected.density * 0.5f * Math.Abs(a.ZCross(b));
			inert += triMass * (a.MagnitudeSquared() + b.MagnitudeSquared() + a.Dot(b)) / 6;
			inertia = 2 * inert;
		}

		public void UpdateGlobalPoints()
		{
			globalTransform = connected.GetGlobalTransform();

			globalHalfWidth = globalTransform * new Vector2(halfWidth, 0);
			globalHalfHeight = globalTransform * new Vector2(0, halfHeight);
			Vector2 position = globalTransform.GetTranslation();
			
			//this basically converts the halfWidth and halfHeight values into local points, adds them to the centrepoint and transforms it using the global transform.
			//minYminX
			globalPoints[0] = globalTransform * (centrePoint + new Vector2(-halfWidth, -halfHeight)) + position;
			//minYmaxX
			globalPoints[1] = globalTransform * (centrePoint + new Vector2(halfWidth, -halfHeight)) + position;
			//maxYmaxX 
			globalPoints[2] = globalTransform * (centrePoint + new Vector2(halfWidth, halfHeight)) + position;
			//maxYminX 
			globalPoints[3] = globalTransform * (centrePoint + new Vector2(-halfWidth, halfHeight)) + position;
		}

		public Vector2[] GetGlobalPoints()
		{
			return globalPoints;
		}

		public Vector2 GetHalfWidthVector()
		{
			return globalHalfWidth;
		}

		public Vector2 GetHalfHeightVector()
		{
			return globalHalfHeight;
		}

		public Vector2 GetLocalHalfWidthHeightVector()
		{
			return new Vector2(halfWidth, halfHeight);
		}
		
		public Vector2 GetCentrePoint()
		{
			return (Matrix3.GetTranslation(centrePoint) * globalTransform).GetTranslation();
		}

		public Collider Clone()
		{
			Collider c = new Collider(centrePoint, halfWidth * 2, halfHeight * 2, layer);
			return c;
		}
	}

	struct AABB
	{
		//centre is global, the others are local
		public float halfWidth;
		public float halfHeight;
		public Vector2 centre;

		public AABB(float top, float left, float bottom, float right)
		{
			halfWidth = (right - left) / 2;
			halfHeight = (bottom - top) / 2;
			centre = new Vector2(left + halfWidth, top + halfHeight);
		}

		public AABB(Vector2 topLeft, Vector2 bottomRight)
		{
			halfWidth = (bottomRight.x - topLeft.x) / 2;
			halfHeight = (bottomRight.y - topLeft.y) / 2;
			centre = new Vector2(topLeft.x + halfWidth, topLeft.y + halfHeight);
		}

		public AABB(Vector2 centrePoint, float width, float height)
		{
			centre = centrePoint;
			halfWidth = width / 2;
			halfHeight = height / 2;
		}

		//this be pretty self explanitory
		public static AABB GetAABBFromPoints(Vector2[] points)
		{
			float minX = float.PositiveInfinity, maxX = float.NegativeInfinity;
			float minY = float.PositiveInfinity, maxY = float.NegativeInfinity;
			for (int i = 0; i < points.Length; i++)
			{
				maxX = points[i].x > maxX ? points[i].x : maxX;
				minX = points[i].x < minX ? points[i].x : minX;

				maxY = points[i].y > maxY ? points[i].y : maxY;
				minY = points[i].y < minY ? points[i].y : minY;
			}
			return new AABB(minY, minX, maxY, maxX);
		}
	}

	struct Ray
	{
		public Vector2 position;
		public Vector2 direction;

		public Ray(Vector2 position, Vector2 direction)
		{
			this.position = position;
			this.direction = direction;
		}
	}

	struct Hit //the info returned when the ray hits an object
	{
		public float distanceAlongRay;
		public PhysicsObject objectHit;

		public Hit(float distanceAlongRay, PhysicsObject colliderHit)
		{
			this.distanceAlongRay = distanceAlongRay;
			objectHit = colliderHit;
		}
	}

	enum CollisionLayer
	{
		Default,
		Enemy,
		Player,
		Count
	}
}
