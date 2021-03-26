using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mlib;

namespace Project2D
{
	static class CollisionManager
	{
		public static List<PhysicsObject> objList = new List<PhysicsObject>();
		public static List<CollisionPair> collisions = new List<CollisionPair>();

		public static void AddObject(PhysicsObject obj)
		{
			objList.Add(obj);
		}

		public static void CheckCollision()
		{
			for (int i = 0; i < objList.Count - 1; i++)
			{
				for (int j = i + 1; j < objList.Count; j++)
				{
					//check AABBs against each other
					//if they intersect then:
					//collisions.Add(new CollisionPair { a = objList[i], b = objList[j] });
				}
			}

			foreach (CollisionPair collision in collisions)
			{
				//
				ResolveCollision(collision);
			}
			//foreach(var first in objList)
			//{
			//	foreach (var second in objList)
			//	{
			//		if (first.Id == second.Id)
			//			return;

			//		//
			//	}
			//}
		}

		public static void ResolveCollision(CollisionPair pair)
		{

			Vector2 rV = pair.b.GetVelocity() - pair.a.GetVelocity();

			float elasticity = Math.Min(pair.a.restitution, pair.b.restitution);

			//
			float impulseMagnitude = (-(1 + elasticity) * Vector2.Dot(rV, pair.normal)) / (pair.a.iMass + pair.b.iMass);
			Vector2 impulse = pair.normal * impulseMagnitude;

			pair.a.AddVelocity(impulse * -pair.a.iMass);
			pair.b.AddVelocity(impulse * pair.b.iMass);
		}

		public static void CheckAABB()
		{

		}
		
	}

	struct AABB
	{
		Vector2 topLeft;
		Vector2 bottomRight;
	}

	struct CollisionPair
	{
		public PhysicsObject a;
		public PhysicsObject b;
		public Vector2 normal;
		public CollisionType type;

		public CollisionPair(PhysicsObject a, PhysicsObject b, Vector2 cNorm, CollisionType type)
		{
			this.a = a; 
			this.b = b; 
			normal = cNorm;
			this.type = type;
		}
	}
	//Vector2 between = wallStartVertex - wallEndVertex
	//between.normalize;
	//Vector2 wallNormal = new Vector(between.y, -between.x) //(rotated 90 degrees)
	enum CollisionType
	{
		CircleCircle,
		CirclePolygon,
		PolygonPolygon
	}
}
