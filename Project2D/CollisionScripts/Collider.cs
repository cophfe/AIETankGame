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
	class Collider
	{
		protected PhysicsObject connected;
		protected Matrix3 globalTransform;
		CollisionLayer layer;
		Vector2[] globalPoints = new Vector2[4];
		Vector2 centrePoint;
		Vector2 halfWidth;
		Vector2 halfHeight;
		Vector2 globalHalfHeight;
		Vector2 globalHalfWidth;
		protected AABB aABB;

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
			halfWidth = new Vector2(width / 2, 0);
			halfHeight = new Vector2(0, height / 2);
			this.layer = layer;
		}

		public static Collider FromTexture(Texture2D texture, CollisionLayer layer = CollisionLayer.Default)
		{
			return new Collider(-texture.width / 2, -texture.height / 2, -texture.width, -texture.height, layer);
		}

		public static Collider FromTextureName(TextureName textureName, CollisionLayer layer = CollisionLayer.Default)
		{
			Texture2D texture = Game.GetTextureFromName(textureName);
			return new Collider(-texture.width / 2, -texture.height / 2, texture.width, texture.height, layer);
		}

		public void UpdateAABB()
		{
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
			float area = (float)Math.Abs((halfWidth.x * 2) * (halfHeight.y * 2));
			mass =  area * connected.density;
			float inert;

			//OKAY I KNOW THIS IS TERRIBLE BUT THE ONLY WAY I FOUND TO GET INERTIA FOR A 2D OBJECT WAS FOR TRIANGLES
			//(because inertia is not usually computed like this and this is probably technically wrong)
			//bottom left
			Vector2 a = centrePoint - halfWidth - halfHeight;
			//bottom right
			Vector2 b = centrePoint + halfWidth - halfHeight;
			float triMass = connected.density * 0.5f * Math.Abs(a.zCross(b));
			inert = triMass * (a.MagnitudeSquared() + b.MagnitudeSquared() + a.Dot(b)) / 6;
			//bottom left
			a = centrePoint - halfWidth - halfHeight;
			//top left
			b = centrePoint - halfWidth + halfHeight;
			triMass = connected.density * 0.5f * Math.Abs(a.zCross(b));
			inert += triMass * (a.MagnitudeSquared() + b.MagnitudeSquared() + a.Dot(b)) / 6;
			inertia = 2 * inert;
		}

		public void UpdateGlobalPoints()
		{
			globalTransform = connected.GetGlobalTransform();

			globalHalfWidth = globalTransform * halfWidth;
			globalHalfHeight = globalTransform * halfHeight;
			Vector2 position = globalTransform.GetTranslation();

			//minYminX
			globalPoints[0] = globalTransform * (centrePoint - halfWidth - halfHeight) + position;
			//minYmaxX
			globalPoints[1] = globalTransform * (centrePoint + halfWidth - halfHeight) + position;
			//maxYmaxX 
			globalPoints[2] = globalTransform * (centrePoint + halfWidth + halfHeight) + position;
			//maxYminX 
			globalPoints[3] = globalTransform * (centrePoint - halfWidth + halfHeight) + position;
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
			return new Vector2(halfWidth.x, halfHeight.y);
		}
		
		public Vector2 GetCentrePoint()
		{
			return (Matrix3.GetTranslation(centrePoint) * globalTransform).GetTranslation();
		}
	}

	struct AABB
	{
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

	struct Hit
	{
		public float distanceAlongRay;
		public PhysicsObject objectHit;

		public Hit(float distanceAlongRay, PhysicsObject colliderHit)
		{
			this.distanceAlongRay = distanceAlongRay;
			this.objectHit = colliderHit;
		}
	}

	enum CollisionLayer
	{
		Default,
		ScreenBounds,
		Player,
		Objects,
		Count
	}
}
