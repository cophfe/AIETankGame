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

		public abstract float GetMass();
		
		public abstract AABB GetAABB();

		public abstract void TransformByGlobalTransform();
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

		public override void TransformByGlobalTransform()
		{
			globalTransform = connected.GetGlobalTransform();
			midPointTransformed = connected.GetGlobalTransform() * midPoint;
		}

		public override float GetMass()
		{
			return connected.density * Trig.pi * radius * radius;
		}
	}

	class RectangleCollider : Collider
	{
		Vector2[] localPoints = new Vector2[4];
		Vector2[] globalPoints = new Vector2[4];

		public RectangleCollider(float left, float top, float width, float height)
		{
			type = ObjectType.Polygon;
			localPoints = new Vector2[4]
			{
				new Vector2(left, top),
				new Vector2(left + width, top),
				new Vector2(left + width, top + height),
				new Vector2(left, top + height),
			};
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

		public override float GetMass()
		{
			//float area = 0;
			//int j = 0;
			//for (int i = 0; i < localPoints.Length; i++)
			//{
			//	++j;
			//	j %= localPoints.Length;
			//	//mass += 0.5f * (points[i + 1] - points[i]).Magnitude() * new Vector2(points[i].x * (0.5f - points[i+1].x), points[i].y * (0.5f - points[i + 1].y)).Magnitude();
			//	//Equation for area of a triangle (the third point is zero zero) 
			//	area += 0.5f * (float)Math.Abs(localPoints[i].x * localPoints[j].y - localPoints[j].x * localPoints[i].y);
			//}
			float area = (float)Math.Abs((localPoints[0].x - localPoints[2].x) * (localPoints[0].y - localPoints[2].y));
			return area * connected.density;
		}

		public override void TransformByGlobalTransform()
		{
			globalTransform = connected.GetGlobalTransform();
			globalPoints = new Vector2[localPoints.Length];

			for (int i = 0; i < localPoints.Length; i++)
			{
				globalPoints[i] = globalTransform * localPoints[i];
			}
		}

		public Vector2[] GetGlobalPoints()
		{
			return globalPoints;
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
