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
	abstract class Collider
	{
		protected PhysicsObject connected;
		protected ObjectType type;
		protected Matrix3 globalTransform;

		public ObjectType GetType()
		{
			return type;
		}

		public void SetConnectedPhysicsObject(PhysicsObject newConnected)
		{
			connected = newConnected;
		}

		public abstract void GetMass(out float mass, out float inertia);
		
		public abstract AABB GetAABB();

		public abstract void UpdateGlobalPoints();
	}

	class CircleCollider : Collider
	{
		float radius;
		Vector2 midPoint;
		Vector2 midPointTransformed;

		public CircleCollider(Vector2 midPoint, float radius)
		{
			type = ObjectType.Circle;
		}

		public override AABB GetAABB()
		{
			Vector2 position = globalTransform.GetTranslation();
			return new AABB(midPointTransformed.y + radius + position.y, midPointTransformed.x - radius + position.x, midPointTransformed.y - radius + position.y, midPointTransformed.x + radius + position.x);
		}

		public override void UpdateGlobalPoints()
		{
			globalTransform = connected.GetGlobalTransform();
			midPointTransformed = connected.GetGlobalTransform() * midPoint;
		}

		public override void GetMass(out float mass, out float inertia)
		{
			throw new NotImplementedException();
			//return connected.density * Trig.pi * radius * radius;
		}
	}

	class RectangleCollider : Collider
	{
		Vector2[] globalPoints = new Vector2[4];
		Vector2 centrePoint;
		Vector2 halfWidth;
		Vector2 halfHeight;
		Vector2 globalHalfHeight;
		Vector2 globalHalfWidth;

		public RectangleCollider(float minX, float minY, float width, float height)
		{
			type = ObjectType.Polygon;
			centrePoint = new Vector2(minX + width / 2, minY + height / 2);
			halfWidth = new Vector2(width / 2, 0);
			halfHeight = new Vector2(0, height / 2);
		}

		public static RectangleCollider FromTexture(Texture2D texture)
		{
			return new RectangleCollider(-texture.width / 2, -texture.height / 2, -texture.width, -texture.height);
		}

		public static RectangleCollider FromTextureName(TextureName textureName)
		{
			Texture2D texture = Game.GetTextureFromName(textureName);
			return new RectangleCollider(-texture.width / 2, -texture.height / 2, texture.width, texture.height);
		}

		public override AABB GetAABB()
		{
			Vector2 position = globalTransform.GetTranslation();

			float minX = float.PositiveInfinity, maxX = float.NegativeInfinity;
			float minY = float.PositiveInfinity, maxY = float.NegativeInfinity;
			for (int i = 0; i < globalPoints.Length; i++)
			{
				maxX = globalPoints[i].x > maxX ? globalPoints[i].x : maxX; 
				minX = globalPoints[i].x < minX ? globalPoints[i].x : minX; 
				
				maxY = globalPoints[i].y > maxY ? globalPoints[i].y : maxY; 
				minY = globalPoints[i].y < minY ? globalPoints[i].y : minY; 
			}
			return new AABB(maxY + position.y, minX + position.x, minY + position.y, maxX + position.x);
		}

		public override void GetMass(out float mass, out float inertia)
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

		public override void UpdateGlobalPoints()
		{
			globalTransform = connected.GetGlobalTransform();

			globalHalfWidth = globalTransform * halfWidth;
			globalHalfHeight = globalTransform * halfHeight;

			//minYminX
			globalPoints[0] = globalTransform * (centrePoint - halfWidth - halfHeight);
			//minYmaxX
			globalPoints[1] = globalTransform * (centrePoint + halfWidth - halfHeight);
			//maxYmaxX 
			globalPoints[2] = globalTransform * (centrePoint + halfWidth + halfHeight);
			//maxYminX 
			globalPoints[3] = globalTransform * (centrePoint - halfWidth + halfHeight);
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
		
		public Vector2 GetCentrePoint()
		{
			return (Matrix3.GetTranslation(centrePoint) * globalTransform).GetTranslation();
		}
	}

	struct AABB
	{
		public Vector2 topLeft;
		public Vector2 bottomRight;
		public Vector2 halfWidth;
		public Vector2 halfHeight;

		public AABB(float top, float left, float bottom, float right)
		{
			topLeft = new Vector2(left, top);
			bottomRight = new Vector2(right, bottom);
			halfWidth = new Vector2((right - left) / 2, 0);
			halfHeight = new Vector2(0, (bottom - top) / 2);
		}

		public AABB(Vector2 topLeft, Vector2 bottomRight)
		{
			this.topLeft = topLeft;
			this.bottomRight = bottomRight;
			halfWidth = new Vector2((bottomRight.x - topLeft.x) / 2, 0);
			halfHeight = new Vector2(0, (bottomRight.y - topLeft.y) / 2);
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
			return new AABB(maxY, minX, minY, maxX);
		}

	}

	enum ObjectType
	{
		Circle,
		Polygon
	}
}
