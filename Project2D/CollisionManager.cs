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
		delegate void GetCollisionVector( CollisionPair pair, out float penetration, out Vector2 collisionNormal);
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
			collisions.Clear();
			for (int i = 0; i < objList.Count; i++)
			{
				objList[i].GetCollider().UpdateGlobalPoints();
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
				ResolveCollision(collisions[i]);
			}
		}
		public static Vector2 drawVector = new Vector2(0, 0);
		public static GameObject obj;
		public static void ResolveCollision(CollisionPair pair)
		{
			Vector2 normal;
			float penetration;
			getCollisionVector[(int)pair.type](pair, out penetration, out normal);
			if (penetration == 0)
				return;
			Vector2 rV = pair.b.GetVelocity() - pair.a.GetVelocity();
			float dot = Vector2.Dot(rV, normal);

			if (dot > 0)
				return;

			float elasticity = Math.Min(pair.a.restitution, pair.b.restitution);
			float friction;

			float impulseMagnitude = penetration* (-(elasticity) * dot) * (pair.a.mass + pair.b.mass);
			Vector2 impulse = normal * impulseMagnitude;
			drawVector = normal * penetration;
			obj = pair.a;
			
			if (pair.a.iMass != -1)
			{
				pair.a.AddPosition(-penetration * normal);
				pair.a.AddForce(impulse * -pair.a.iMass);
			}

			if (pair.b.iMass != -1)
			{
				pair.b.AddPosition(penetration * normal);
				pair.b.AddForce(impulse * pair.b.iMass);
			}

			//Vector2 position = pair.a.GlobalPosition;
			//DrawLine((int)position.x + 50, (int)position.y + 50, (int)(position.x + normal.x * penetration) + 50, (int)(position.y + normal.y * penetration) + 50, RLColor.PURPLE);
		}
		
		const float percent = 0.6f;
		public static void CorrectPosition(CollisionPair pair)
		{
			Vector2 correction = pair.normal / (pair.a.iMass + pair.b.iMass) * percent * pair.normal;
			pair.a.AddPosition(-pair.a.iMass * correction);
			pair.b.AddPosition(pair.b.iMass * correction);
		}

		public static bool CheckAABB(AABB a, AABB b)
		{
			return (a.topLeft.x < b.bottomRight.x && a.bottomRight.x > b.topLeft.x && a.bottomRight.y < b.topLeft.y && a.topLeft.y > b.bottomRight.y);
		}

		public static void GetCollisionRectangles(CollisionPair pair, out float penetration, out Vector2 collisionNormal)
		{
			//This is a simplified version of SAT that only works with rectangles
			//It should be way, way faster (no loops!)
			//however it probably isn't for some reason.
			RectangleCollider aCol = (pair.a.GetCollider() as RectangleCollider);
			RectangleCollider bCol = (pair.b.GetCollider() as RectangleCollider);
			Vector2 aHalfWidth = aCol.GetHalfWidthVector();
			Vector2 aHalfHeight = aCol.GetHalfHeightVector();
			Vector2 aCentre = aCol.GetCentrePoint();
			Vector2 bHalfWidth = bCol.GetHalfWidthVector();
			Vector2 bHalfHeight = bCol.GetHalfHeightVector();
			Vector2 bCentre = bCol.GetCentrePoint();


			Matrix3 aTransform = pair.a.GetGlobalTransform();
			Matrix3 bTransform = pair.b.GetGlobalTransform();


			Vector2 normal = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
			Vector2 axis = aTransform.GetRightVector();
			float pValue = float.PositiveInfinity;
			float pV;
			float a;
			float b;
			//A X Axis
			///////////////////////////////////////////////////////////////////
			if (!doPA(aTransform.GetRightVector()))
			{
				penetration = 0;
				collisionNormal = Vector2.Zero;
				return;
			}
			//A Y Axis
			//////////////////////////////////////////////////////////////////
			if (!doPA(aTransform.GetForwardVector()))
			{
				penetration = 0;
				collisionNormal = Vector2.Zero;
				return;
			}
			//B X Axis
			//////////////////////////////////////////////////////////////////
			if (!doPB(bTransform.GetRightVector()))
			{
				penetration = 0;
				collisionNormal = Vector2.Zero;
				return;
			}
			//B Y Axis
			//////////////////////////////////////////////////////////////////
			if (!doPB(bTransform.GetForwardVector()))
			{
				penetration = 0;
				collisionNormal = Vector2.Zero;
				return;
			}
			//////////////////////////////////////////////////////////////////
			
			collisionNormal = normal;
			penetration = pValue;
			return;

			bool doPA(Vector2 ax)
			{
				float aW = ax.Dot(aHalfWidth);
				float aH = ax.Dot(aHalfHeight);

				float bH = ax.Dot(bHalfHeight * Math.Sign(bHalfHeight.Dot(ax)));
				float bW = ax.Dot(bHalfWidth * Math.Sign(bHalfWidth.Dot(ax)));

				a = ax.Dot(aCentre);
				b = ax.Dot(bCentre);

				pV = (((aW + aH) + (bH + bW)) - Math.Abs(a - b));

				if (pV > 0)
				{
					if (pV < Math.Abs(pValue))
					{
						pValue = pV; 
						normal = ax * Math.Sign(b-a);
					}
					return true;
				}
				else
				{
					//not colliding
					return false;
				}
			}

			bool doPB(Vector2 ax)
			{
				float aW = ax.Dot(aHalfWidth * Math.Sign(aHalfWidth.Dot(ax)));
				float aH = ax.Dot(aHalfHeight * Math.Sign(aHalfHeight.Dot(ax)));

				float bH = ax.Dot(bHalfHeight);
				float bW = ax.Dot(bHalfWidth);

				a = ax.Dot(bCentre);
				b = ax.Dot(aCentre);

				pV = (((aW + aH) + (bH + bW)) - Math.Abs(a - b));

				if (pV > 0)
				{
					
					if (pV < Math.Abs(pValue))
					{
						pValue = pV;
						normal = ax * Math.Sign(a - b);
					}
					return true;
				}
				else
				{
					//not colliding
					return false;
				}
			}
		}

		public static void GetCollisionPolygonCircle(CollisionPair pair, out float penetration, out Vector2 collisionNormal)
		{
			throw new NotImplementedException();
		}

		public static void GetCollisionCircles(CollisionPair pair, out float penetration, out Vector2 collisionNormal)
		{
			throw new NotImplementedException();
		}
	}

	struct CollisionPair
	{
		public PhysicsObject a;
		public PhysicsObject b;
		public Vector2 normal;
		//public float pDepth
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
