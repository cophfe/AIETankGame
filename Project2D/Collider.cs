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

	class PolygonCollider : Collider
	{
		Vector2[] points;
		public Vector2[] pointsTransformed;

		public PolygonCollider(Vector2[] points)
		{
			type = ObjectType.Polygon;
			this.points = points;
		}

		public override AABB GetAABB()
		{
			Vector2 position = globalTransform.GetTranslation();

			float minX = float.PositiveInfinity, maxX = float.NegativeInfinity;
			float minY = float.PositiveInfinity, maxY = float.NegativeInfinity;
			for (int i = 0; i < pointsTransformed.Length; i++)
			{
				maxX = pointsTransformed[i].x > maxX ? pointsTransformed[i].x : maxX; 
				minX = pointsTransformed[i].x < minX ? pointsTransformed[i].x : minX; 
				
				maxY = pointsTransformed[i].y > maxY ? pointsTransformed[i].y : maxY; 
				minY = pointsTransformed[i].y < minY ? pointsTransformed[i].y : minY; 
			}
			return new AABB(maxY + position.y, minX + position.x, minY + position.y, maxX + position.x);
		}

		public static AABB GetAABB(Vector2[] points)
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

		public override float GetMass()
		{
			float area = 0;
			int j = 0;
			for (int i = 0; i < points.Length; i++)
			{
				++j;
				j %= points.Length;
				//mass += 0.5f * (points[i + 1] - points[i]).Magnitude() * new Vector2(points[i].x * (0.5f - points[i+1].x), points[i].y * (0.5f - points[i + 1].y)).Magnitude();
				//Equation for area of a triangle (the third point is zero zero) 
				area += 0.5f * (float)Math.Abs(points[i].x * points[j].y - points[j].x * points[i].y);
			}
			return area * connected.density;
		}

		public override void TransformByGlobalTransform()
		{
			globalTransform = connected.GetGlobalTransform();
			pointsTransformed = new Vector2[points.Length];

			for (int i = 0; i < points.Length; i++)
			{
				pointsTransformed[i] = globalTransform * points[i];
			}
		}
	}

	struct AABB
	{
		public Vector2 topLeft;
		public Vector2 bottomRight;

		public AABB(float top, float left, float bottom, float right)
		{
			topLeft = new Vector2(left, top);
			bottomRight = new Vector2(right, bottom);
		}

		public AABB(Vector2 topLeft, Vector2 bottomRight)
		{
			this.topLeft = topLeft;
			this.bottomRight = bottomRight;
		}


	}

	enum ObjectType
	{
		Circle,
		Polygon
	}
}
