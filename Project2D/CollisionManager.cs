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
		delegate void GetCollisionVector();
		static GetCollisionVector[] getCollisionVector = new GetCollisionVector[]
		{
			GetCollisionCircles,
			GetCollisionPolygonCircle,
			GetCollisionPolygons
		};
		public static List<PhysicsObject> objList = new List<PhysicsObject>();
		public static List<CollisionPair> collisions = new List<CollisionPair>();

		public static void Initiate()
		{
			
		}

		public static void AddObject(PhysicsObject obj)
		{
			if (obj.GetCollider() != null)
				objList.Add(obj);
		}

		public static void CheckCollisions()
		{
			for (int i = 0; i < objList.Count; i++)
			{
				objList[i].GetCollider().TransformByGlobalTransform();
			}

			for (int i = 0; i < objList.Count - 1; i++)
			{
				for (int j = i + 1; j < objList.Count; j++)
				{
					//broad phase
					if (CheckAABB(objList[i].GetCollider().GetAABB(), objList[j].GetCollider().GetAABB()))
					{
						collisions.Add(new CollisionPair { a = objList[i], b = objList[j], type = (CollisionType)((int)objList[i].GetCollider().GetType() + (int)objList[j].GetCollider().GetType()) });
					}
				}
			}

			foreach (CollisionPair collision in collisions)
			{
				getCollisionVector[(int)collision.type]();
				ResolveCollision(collision);
			}
		}

		public static void ResolveCollision(CollisionPair pair)
		{

			Vector2 rV = pair.b.GetVelocity() - pair.a.GetVelocity();

			float elasticity = Math.Min(pair.a.restitution, pair.b.restitution);
			float friction;

			float impulseMagnitude = (-(1 + elasticity) * Vector2.Dot(rV, pair.normal)) / (pair.a.iMass + pair.b.iMass);
			Vector2 impulse = pair.normal * impulseMagnitude;

			pair.a.AddVelocity(impulse * -pair.a.iMass);
			pair.b.AddVelocity(impulse * pair.b.iMass);
		}

		public static bool CheckAABB(AABB a, AABB b)
		{
			return (a.topLeft.x < b.bottomRight.x && a.bottomRight.x > b.topLeft.x && a.bottomRight.y < b.topLeft.y && a.topLeft.y > b.bottomRight.y);
		}
		
		public static void GetCollisionPolygons()
		{
			//Collision vector is the vector with the smallest possible 
		}

		public static void GetCollisionPolygonCircle()
		{

		}

		public static void GetCollisionCircles()
		{

		}
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

	enum CollisionType //NOTE: adding two collider's types will give the collision type
	{
		CircleCircle,
		CirclePolygon,
		PolygonPolygon
	}
}
