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
			return new AABB(midPointTransformed.y + radius, midPointTransformed.x - radius, midPointTransformed.y - radius, midPointTransformed.x + radius);
		}

		public override void TransformByGlobalTransform()
		{
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
			float minX = float.PositiveInfinity, maxX = float.NegativeInfinity;
			float minY = float.PositiveInfinity, maxY = float.NegativeInfinity;
			for (int i = 0; i < points.Length; i++)
			{
				maxX = points[i].x < maxX ? maxX : points[i].x; 
				minX = points[i].x > minX ? minX : points[i].x; 
				
				maxY = points[i].y < maxY ? maxY : points[i].y; 
				minY = points[i].y > minY ? minY : points[i].y; 
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
			Matrix3 m = connected.GetGlobalTransform();
			pointsTransformed = new Vector2[points.Length];

			for (int i = 0; i < points.Length; i++)
			{
				pointsTransformed[i] = m * points[i];
			}
		}
	}

	struct AABB
	{
		Vector2 topLeft;
		Vector2 bottomRight;

		public AABB(float top, float left, float bottom, float right)
		{
			topLeft = new Vector2(top, left);
			bottomRight = new Vector2(bottom, right);
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
