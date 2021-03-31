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


		/// <summary>
		///DEBUG~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
		/// </summary>
		public static Vector2 drawVector = new Vector2(0, 0);
		public static GameObject obj;
		//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
		public static void ResolveCollision(CollisionPair pair)
		{
			Vector2 normal;
			float penetration;
			CollisionPoints collisionPoints;
			GetCollisionRectangles(pair, out penetration, out normal, out collisionPoints);

			if (penetration == 0)
				return;
			for (int i = 0; i < collisionPoints.points.Count; i++)
			{
				Vector2 rV = pair.b.GetVelocity() - pair.a.GetVelocity();
				float dot = Vector2.Dot(rV, normal);

				if (dot > 0)
					return;

				float elasticity = Math.Min(pair.a.restitution, pair.b.restitution);

				float aIM = pair.a.GetInverseMass();

				float bIM = pair.b.GetInverseMass();

				float impulseMagnitude = (-(1 + elasticity) * dot) / (aIM + bIM);
				Vector2 impulse = normal * impulseMagnitude;

				//mass/mass+mass
				if (aIM != 0)
				{
					pair.a.AddPosition(-penetration * normal);
					pair.a.AddVelocity(impulse * -aIM);
				}

				if (bIM != 0)
				{
					pair.b.AddPosition(penetration * normal);
					pair.b.AddVelocity(impulse * bIM);
				}
			}
			//Vector2 position = pair.a.GlobalPosition;
			//DrawLine((int)position.x + 50, (int)position.y + 50, (int)(position.x + normal.x * penetration) + 50, (int)(position.y + normal.y * penetration) + 50, RLColor.PURPLE);
		}
		

		public static bool CheckAABB(AABB a, AABB b)
		{
			return (a.topLeft.x < b.bottomRight.x && a.bottomRight.x > b.topLeft.x && a.bottomRight.y < b.topLeft.y && a.topLeft.y > b.bottomRight.y);
		}

		public static void GetCollisionRectangles(CollisionPair pair, out float penetration, out Vector2 collisionNormal, out CollisionPoints collisionPoints)
		{
			//This was supposed to be a simplified version of SAT that only works with rectangles
			//It should be faster than the normal way with polygons
			//however it probably isn't because come on now you expect me to make a *fast* collision thingy?  
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

			bool pointsToB = false;

			Vector2 normal = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
			Vector2 axis;
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
				collisionPoints = null;
				return;
			}
			//A Y Axis
			//////////////////////////////////////////////////////////////////
			if (!doPA(aTransform.GetForwardVector()))
			{
				penetration = 0;
				collisionNormal = Vector2.Zero;
				collisionPoints = null;
				return;
			}
			//B X Axis
			//////////////////////////////////////////////////////////////////
			if (!doPB(bTransform.GetRightVector()))
			{
				penetration = 0;
				collisionNormal = Vector2.Zero;
				collisionPoints = null;
				return;
			}
			//B Y Axis
			//////////////////////////////////////////////////////////////////
			if (!doPB(bTransform.GetForwardVector()))
			{
				penetration = 0;
				collisionNormal = Vector2.Zero;
				collisionPoints = null;
				return;
			}
			//////////////////////////////////////////////////////////////////
			
			collisionNormal = normal;
			penetration = pValue;

			//now for rotation
			//need to find collision points
			//reference:
			//http://www.dyn4j.org/2011/11/contact-points-using-clipping/
			// ^ the only source on the entire internet for this

			Vector2[] mainPoints;
			Vector2[] secondaryPoints;

			if (pointsToB)
			{
				mainPoints = bCol.GetGlobalPoints();
				secondaryPoints = aCol.GetGlobalPoints();
			}
			else
			{
				mainPoints = aCol.GetGlobalPoints();
				secondaryPoints = bCol.GetGlobalPoints();
			}
			Edge e1 = GetMostPerpendicular(mainPoints, normal);
			Edge e2 = GetMostPerpendicular(secondaryPoints, -1 * normal);

			Edge reference;
			Edge incident;

			bool flip = false;
			if (Math.Abs((e1.v2 - e1.v1).Dot(normal)) <= Math.Abs((e2.v2 - e2.v1).Dot(normal)))
			{
				reference = e1;
				incident = e2;
			}
			else
			{
				reference = e2;
				incident = e1;

				flip = true;
			}

			Vector2 refV = reference.v2 - reference.v1;

			float o1 = refV.Dot(reference.v1);

			CollisionPoints cP = Clip(incident.v1, incident.v2, refV, o1);

			if (cP.points.Count < 2)
			{
				collisionPoints = null;
				return;
			}

			float o2 = refV.Dot(reference.v2);
			
			cP = Clip(cP.points[0], cP.points[1], -1 * refV, -o2);

			if (cP.points.Count < 2)
			{
				collisionPoints = null;
				return;
			}

			Vector2 refNorm = Vector2.zCross(refV, -1);

			if (flip) refNorm *= -1;

			float max = refNorm.Dot(reference.max);

			cP.depths.Add(refNorm.Dot(cP.points[0]) - max);
			cP.depths.Add(refNorm.Dot(cP.points[1]) - max);
			if (cP.depths[0] < 0)
			{
				cP.depths.RemoveAt(0);
				cP.points.RemoveAt(0);
			}
			if (cP.depths.Count < 0)
			{
				cP.depths.RemoveAt(cP.depths.Count);
				cP.points.RemoveAt(cP.depths.Count);
			}
			if(pointsToB)
				drawVector = cP.points[0] + pair.b.GlobalPosition;
			else
				drawVector = cP.points[0] + pair.a.GlobalPosition;
			collisionPoints = cP;
			return;
			CollisionPoints Clip(Vector2 point1, Vector2 point2, Vector2 n, float limit)
			{
				List<Vector2> clippedPoints = new List<Vector2>(2);
				float d1 = n.Dot(point1) - limit;
				float d2 = n.Dot(point2) - limit;

				if (d1 >= 0) clippedPoints.Add(point1);
				if (d2 >= 0) clippedPoints.Add(point2);

				if (d1 * d2 < 0)
				{
					Vector2 e = point2 - point1;
					float u = d1 / (d1 - d2);

					e *= u;
					e += point1;
					clippedPoints.Add(e);
				}
				return new CollisionPoints(clippedPoints);
			}

			Edge GetMostPerpendicular(Vector2[] points, Vector2 n)
			{
				float furthestVertex = float.NegativeInfinity;
				int index = 0;
				for (int i = 0; i < 4; i++)
				{
					float projectedDist = normal.Dot(points[i]);
					if (projectedDist > furthestVertex)
					{
						furthestVertex = projectedDist;
						index = i;
					}
				}

				Vector2 v = points[index];
				Vector2 v1 = points[(index + 1) % 4]; //forward one
				Vector2 v0 = points[(index + 3) % 4]; //back one

				Vector2 l = v - v1;
				Vector2 r = v - v0;
				l.SetNormalised();
				r.SetNormalised();

				if (Math.Abs(r.Dot(normal)) <= Math.Abs(l.Dot(normal)))
				{
					return new Edge(v, v0, v);
				}
				else
				{
					return new Edge(v, v, v1);
				}
			}

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
						normal = ax * Math.Sign(b - a);
						pointsToB = true;
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
						pointsToB = false;
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
		struct Edge
		{
			public Vector2 max;
			public Vector2 v1;
			public Vector2 v2;

			public Edge(Vector2 max, Vector2 v1, Vector2 v2)
			{
				this.max = max;
				this.v1 = v1;
				this.v2 = v2;
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
		public CollisionType type;

		public CollisionPair(PhysicsObject a, PhysicsObject b, CollisionType type)
		{
			this.a = a; 
			this.b = b; 
			this.type = type;
		}
	}

	enum CollisionType //NOTE: adding two collider's types will give the collision type
	{
		CircleCircle,
		CirclePolygon,
		PolygonPolygon
	}

	class CollisionPoints
	{
		public List<Vector2> points= new List<Vector2>(2);
		public List<float> depths = new List<float>(2);

		public CollisionPoints(List<Vector2> points)
		{
			this.points = points;
			this.depths = depths;
		}
	}
}
