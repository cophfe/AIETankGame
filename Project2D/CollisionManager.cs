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
	static class CollisionManager
	{
		delegate Vector2 GetCollisionVector( CollisionPair pair);
		static GetCollisionVector[] getCollisionVector = new GetCollisionVector[]
		{
			GetCollisionCircles,
			GetCollisionPolygonCircle,
			GetCollisionRectangles
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

			for (int i = 0; i < collisions.Count(); i++)
			{
				Vector2 normal = getCollisionVector[(int)collisions[i].type](collisions[i]);
				collisions[i] = new CollisionPair(collisions[i].a, collisions[i].b, normal, collisions[i].type);
				if (collisions[i].normal != Vector2.Zero)
				{
					ResolveCollision(collisions[i]);
					Console.WriteLine($"{collisions[i].normal.x}, {collisions[i].normal.y}");
				}
			}
		}

		public static void ResolveCollision(CollisionPair pair)
		{

			Vector2 rV = pair.b.GetVelocity() - pair.a.GetVelocity();

			float elasticity = Math.Min(pair.a.restitution, pair.b.restitution);
			float friction;

			float impulseMagnitude = (-(1 + elasticity) * Vector2.Dot(rV, pair.normal)) / (pair.a.iMass + pair.b.iMass);
			Vector2 impulse = pair.normal * impulseMagnitude;

			if (pair.a.iMass != -1)
				pair.a.AddVelocity(impulse * -pair.a.iMass);

			if (pair.b.iMass != -1)
				pair.b.AddVelocity(impulse * pair.b.iMass);
		}

		public static bool CheckAABB(AABB a, AABB b)
		{
			return (a.topLeft.x < b.bottomRight.x && a.bottomRight.x > b.topLeft.x && a.bottomRight.y < b.topLeft.y && a.topLeft.y > b.bottomRight.y);
		}
		
		public static Vector2 GetCollisionRectangles(CollisionPair pair)
		{
			//This is a simplified version of SAT that only works with rectangles
			
			Vector2[] aPoints = (pair.a.GetCollider() as RectangleCollider).GetGlobalPoints();
			Vector2[] bPoints = (pair.b.GetCollider() as RectangleCollider).GetGlobalPoints();


			Matrix3 aTransform = pair.a.GetGlobalTransform();
			Matrix3 bTransform = pair.b.GetGlobalTransform();
			Vector2[] axisAligned = aPoints, notAligned = bPoints;

			Vector2 axisAlignedPos = aTransform.GetTranslation();
			Vector2 unAlignedPos = bTransform.GetTranslation();

			bool b = false;
			Vector2 axis = aTransform.GetRightVector();
			
			Vector2 pValue = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
			for (int i = 0; i < 4; i++)
			{			
				switch (i)
				{
					case 0:
						axis = aTransform.GetRightVector();

						break;

					case 1:
						axis = aTransform.GetForwardVector();
						break;

					case 2:
						axis = bTransform.GetRightVector();
						b = true;
						notAligned = aPoints;
						axisAligned = bPoints;
						unAlignedPos = axisAlignedPos;
						axisAlignedPos = bTransform.GetTranslation();
						break;

					case 3:
						axis = bTransform.GetForwardVector();

						break;
				}
				//For our purposes, the only axis we need to test are the forward and right vectors inside of the global transform. No stored normals are needed.
				

				 //one P value for each axis. The smallest one will be used for the collision vector
			
				//SAT works by projecting all points onto a polygon's face's tangent vector, and checking 1 dimentionally, if 

				//The top and bottom of A is guaranteed to be from these points because of how I guaranteed the points will be organised inside of RectangleCollider
				float fixedMin = axis.Dot(axisAligned[0] + axisAlignedPos);
				float fixedMax= axis.Dot(axisAligned[2] + axisAlignedPos);

				//find the minimum and maximum values of B projected onto
				float variableMin = float.PositiveInfinity;
				float variableMax = float.NegativeInfinity;
			
				for (int j = 0; j < 4; j++)
				{
					//Project unaligned points point onto axis
					float projected = axis.Dot(notAligned[j] + unAlignedPos);

					//find min and max value of projected
					variableMin = projected < variableMin ? projected : variableMin;
					variableMax = projected > variableMax ? projected : variableMax;
				}

				if (variableMax > fixedMin && fixedMax > variableMin)
				{
					float cached = fixedMax < variableMax ? fixedMax - variableMin : variableMax - fixedMin;

					if (cached * cached < pValue.MagnitudeSquared())
						pValue = cached * axis;
				}
				else
				{
					return Vector2.Zero; // is not colliding
				}
			}
			return pValue;



		}

		public static Vector2 GetCollisionPolygonCircle(CollisionPair pair)
		{
			throw new NotImplementedException();
		}

		public static Vector2 GetCollisionCircles(CollisionPair pair)
		{
			throw new NotImplementedException();
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
