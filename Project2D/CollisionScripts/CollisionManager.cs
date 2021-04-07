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
	/// <summary>
	/// The class that manages collision (attached to scene)
	/// </summary>
	class CollisionManager
	{
		public List<PhysicsObject> objList = new List<PhysicsObject>();
		public List<CollisionPair> collisions = new List<CollisionPair>();

		public void AddObject(PhysicsObject obj)
		{
			if (obj.GetCollider() != null)
				objList.Add(obj);
		}

		public void CheckCollisions(params CollisionLayer[] ignored)
		{
			collisions.Clear();
			for (int i = 0; i < objList.Count; i++)
			{
				Collider collider = objList[i].GetCollider();
				collider.UpdateGlobalPoints();
				collider.UpdateAABB();
			}

			for (int i = 0; i < objList.Count - 1; i++)
			{
				if (ignored.Contains(objList[i].GetCollider().GetLayer()))
					continue;
				for (int j = i + 1; j < objList.Count; j++)
				{
					if (ignored.Contains(objList[j].GetCollider().GetLayer()))
						continue;
					//broad phase
					if (CheckAABB(objList[i].GetCollider().GetAABB(), objList[j].GetCollider().GetAABB()))
					{
						collisions.Add(new CollisionPair { a = objList[i], b = objList[j] });
					}
				}
			}

			for (int i = 0; i < collisions.Count(); i++)
			{
				ResolveCollision(collisions[i]);
			}
		}

		public static void ResolveCollision(CollisionPair pair)
		{
			Vector2 normal;
			float penetration;
			CollisionPoints collisionPoints;
			GetCollisionInformation(pair, out penetration, out normal, out collisionPoints);
			if (collisionPoints == null)
				return;

			float aIM = pair.a.GetInverseMass();
			float bIM = pair.b.GetInverseMass();
			float aII = pair.a.GetInverseInertia();
			float bII = pair.b.GetInverseInertia();
			//both are static
			if (aIM + bIM == 0)
				return;
			
			//if (normal.Dot(pair.b.GetCollider().GetCentrePoint() - pair.a.GetCollider().GetCentrePoint()) > 0)
			//	normal *= -1;
			for (int i = 0; i < collisionPoints.points.Count; i++)
			{
				//The radius vectors from the collision
				Vector2 radiusA = (collisionPoints.points[i] - pair.a.GetCollider().GetCentrePoint());
				Vector2 radiusB = (collisionPoints.points[i] - pair.b.GetCollider().GetCentrePoint());
				
				Vector2 rV = (pair.b.GetVelocity() + Vector2.ZCross(-pair.b.GetAngularVelocity(), radiusB)) - (pair.a.GetVelocity() + Vector2.ZCross(-pair.a.GetAngularVelocity(), radiusA));

				//if (Vector2.Dot(pair.b.GetVelocity() - pair.a.GetVelocity(), normal) > 0)
				//	return;
				float projectedRV = Vector2.Dot(rV, normal);

				float rACrossN = radiusA.ZCross(normal);
				float rBCrossN = radiusB.ZCross(normal);
				
				//impulse is spread evenly between collision points
				float impulseMagnitude = (-(1 + Math.Min(pair.a.restitution, pair.b.restitution)) * projectedRV) / ((aIM + bIM + (rACrossN * rACrossN) * aII + (rBCrossN * rBCrossN) * bII) * collisionPoints.points.Count);
				Vector2 impulse = normal * impulseMagnitude;
				
				pair.a.AddImpulseAtPosition(-1 * impulse, radiusA);
				pair.b.AddImpulseAtPosition(impulse, radiusB);
			}
				if (aIM != 0)
					pair.a.AddPosition(normal * -penetration);
				if (bIM != 0)
					pair.b.AddPosition(normal * penetration);
		}

		#region Ray Casting
		static Matrix3 rotateClock = Matrix3.GetRotateZ(Trig.pi /2);
		static Matrix3 rotateAntiClock = Matrix3.GetRotateZ(Trig.pi / -2);
		public bool RayCast(Ray ray, out Hit hit, float magnitude = float.PositiveInfinity, params CollisionLayer[] ignoredLayers)
		{
			//this raycast was done using this algorithm 
			//http://www.opengl-tutorial.org/miscellaneous/clicking-on-objects/picking-with-custom-ray-obb-function/
			//this page isn't the only one to use these concepts but it is were i found out how to do this without (many) transformation shenanigans

			bool collided = false;
			float distance = float.PositiveInfinity;
			PhysicsObject colliderHit = null;
			
			for (int i = 0; i < objList.Count; i++)
			{
				Collider rect = objList[i].GetCollider();
				if (ignoredLayers.Contains(rect.GetLayer()) || (ray.position - rect.GetCentrePoint()).MagnitudeSquared() > magnitude * magnitude)
					continue;

				Vector2 relative = rect.GetCentrePoint() - ray.position;
				Matrix3 t = objList[i].GetGlobalTransform();
				float minTotalDistance = 0;
				float maxTotalDistance = float.PositiveInfinity;

				//everything in the game has uniform scaling on x and y (because otherwise there are rotation issues) so only one getscale is required
				float scale = t.GetScaleX();

				Vector2 max = rect.GetLocalHalfWidthHeightVector() * scale;
				Vector2 min = max * -1;

				//X axis
				//We don't use t.GetRightVector() to cut down on the square rooting
				Vector2 x = new Vector2(t.m11, t.m21) / scale;

				//the distance between the ray and the centre of the rectangle along the rectangle's x axis 
				float e = Vector2.Dot(x, relative);
				//The projection of the ray's direction onto the rectangle's x axis
				float f = Vector2.Dot(x, ray.direction);

				float minimumDistance;
				float maximumDistance;

				//if f is around zero that means f is almost the same as x: aka it is parallel and is not colliding (unless you have a really massive object)
				if (Math.Abs(f) > 0.0001f)
				{
					f = 1 / f;
					//these contain the distance between the ray position and the hit point
					//there are two because we are crossing two lines for each axis
					//e is translation, f is rotation
					minimumDistance = (e + min.x) * f;
					maximumDistance = (e + max.x) * f;

					if (minimumDistance > maximumDistance)
					{
						float cache = minimumDistance;
						minimumDistance = maximumDistance;
						maximumDistance = cache;
					}

					maxTotalDistance = maximumDistance;

					//min distance should be more than zero. If it isn't, it should be ignored
					if (minimumDistance > 0)
					{
						minTotalDistance = minimumDistance;
					}
				}
				else if (min.x - e > 0 || max.x - e < 0)
					continue;

				//Y axis (same as x basically)
				Vector2 y = new Vector2(t.m12, t.m22) / scale;

				e = Vector2.Dot(y, relative);
				f = Vector2.Dot(y, ray.direction);

				if (Math.Abs(f) > 0.0001f)
				{
					f = 1 / f;
					minimumDistance = (e + min.y) * f;
					maximumDistance = (e + max.y) * f;

					if (minimumDistance > maximumDistance)
					{
						float cache = minimumDistance;
						minimumDistance = maximumDistance;
						maximumDistance = cache;
					}

					if (maximumDistance < maxTotalDistance)
					{
						maxTotalDistance = maximumDistance;
					}
					if (minimumDistance > minTotalDistance)
					{
						minTotalDistance = minimumDistance;
					}

					if (maxTotalDistance < minTotalDistance)
					{
						continue;
					}
				}
				else if(min.y - e > 0 || max.y - e < 0)
					continue;

				collided = true;
				if (minTotalDistance < distance)
				{
					distance = minTotalDistance;
					colliderHit = objList[i];
				}
			}

			hit = new Hit(distance, colliderHit);
			if (collided)
			{
				return true;
			}
			return false;
		}

		public bool RayCastToCollider(Ray ray, out Hit hit, Collider collider)
		{
			hit = new Hit(0, null);
			Collider rect = collider;
			Vector2 relative = rect.GetCentrePoint() - ray.position;
			Matrix3 t = collider.GetConnectedPhysicsObject().GetGlobalTransform();
			float minTotalDistance = 0;
			float maxTotalDistance = float.PositiveInfinity;

			float scale = t.GetScaleX();

			Vector2 max = rect.GetLocalHalfWidthHeightVector() * scale;
			Vector2 min = max * -1;

			Vector2 x = new Vector2(t.m11, t.m21) / scale;

			float e = Vector2.Dot(x, relative);
			float f = Vector2.Dot(x, ray.direction);

			float minimumDistance;
			float maximumDistance;

			if (Math.Abs(f) > 0.0001f)
			{
				f = 1 / f;
				minimumDistance = (e + min.x) * f;
				maximumDistance = (e + max.x) * f;

				if (minimumDistance > maximumDistance)
				{
					float cache = minimumDistance;
					minimumDistance = maximumDistance;
					maximumDistance = cache;
				}

				maxTotalDistance = maximumDistance;

				if (minimumDistance > 0)
				{
					minTotalDistance = minimumDistance;
				}

				if (maxTotalDistance < minTotalDistance)
				{
					return false;
				}
			}
			else if (min.x - e > 0 || max.x - e < 0)
				return false;

			Vector2 y = new Vector2(t.m12, t.m22) / scale;

			e = Vector2.Dot(y, relative);
			f = Vector2.Dot(y, ray.direction);

			if (Math.Abs(f) > 0.0001f)
			{
				f = 1 / f;
				minimumDistance = (e + min.y) * f;
				maximumDistance = (e + max.y) * f;

				if (minimumDistance > maximumDistance)
				{
					float cache = minimumDistance;
					minimumDistance = maximumDistance;
					maximumDistance = cache;
				}

				if (maximumDistance < maxTotalDistance)
				{
					maxTotalDistance = maximumDistance;
				}
				if (minimumDistance > minTotalDistance)
				{
					minTotalDistance = minimumDistance;
				}

				if (maxTotalDistance < minTotalDistance)
				{
					return false;
				}
			}
			else if (min.y - e > 0 || max.y - e < 0)
				return false;

			hit = new Hit(minTotalDistance, collider.GetConnectedPhysicsObject());
			return true;
		}

		public bool RayCastSimple(Ray ray, out List<PhysicsObject> hitObjects, params CollisionLayer[] ignoredLayers)
		{
			//this raycaster simply checks if a ray is within the smaller angle between two vectors
			//those vectors are based on the points of a collider.

			hitObjects = new List<PhysicsObject>();
			for (int i = 0; i < objList.Count; i++)
			{
				if (ignoredLayers != null && ignoredLayers.Contains(objList[i].GetCollider().GetLayer()))
					continue;
				AABB aABB = objList[i].GetCollider().GetAABB();

				float w = aABB.halfWidth * Math.Sign(aABB.centre.x - ray.position.x);
				float h = aABB.halfHeight * Math.Sign(aABB.centre.y - ray.position.y);

				Vector2 biggest;
				Vector2 smallest;

				bool horizontal = ray.position.x > -aABB.halfWidth + aABB.centre.x && ray.position.x < aABB.halfWidth + aABB.centre.x;
				bool vertical = ray.position.y > -aABB.halfHeight + aABB.centre.y && ray.position.y < aABB.halfHeight + aABB.centre.y;
				if (horizontal && vertical)
				{
					hitObjects.Add(objList[i]);
					continue;
				}
				else if (horizontal)
				{
					biggest = new Vector2(aABB.centre.x + w, aABB.centre.y - h) - ray.position;
					smallest = new Vector2(aABB.centre.x - w, aABB.centre.y - h) - ray.position;
				}
				else if (vertical)
				{
					biggest = new Vector2(aABB.centre.x - w, aABB.centre.y - h) - ray.position;
					smallest = new Vector2(aABB.centre.x - w, aABB.centre.y + h) - ray.position;
				}
				else
				{
					biggest = new Vector2(aABB.centre.x + w, aABB.centre.y - h) - ray.position;
					smallest = new Vector2(aABB.centre.x - w, aABB.centre.y + h) - ray.position;
				}

				if (!(biggest.ZCross(ray.direction) * biggest.ZCross(smallest) >= 0 && smallest.ZCross(ray.direction) * smallest.ZCross(biggest) >= 0))
				{
					continue;
				}
				hitObjects.Add(objList[i]);
			}

			return hitObjects.Count > 0;
		}

		#endregion

		public static bool CheckAABB(AABB a, AABB b)
		{
			return (a.halfWidth + b.halfWidth > Math.Abs(a.centre.x - b.centre.x) && a.halfHeight + b.halfHeight > Math.Abs(a.centre.y - b.centre.y));
		}

		public static void GetCollisionInformation(CollisionPair pair, out float penetration, out Vector2 collisionNormal, out CollisionPoints collisionPoints)
		{
			//Collision Detection using SAT
			
			Collider aCol = pair.a.GetCollider();
			Collider bCol = pair.b.GetCollider();
			Vector2 aHalfWidth = aCol.GetHalfWidthVector();
			Vector2 aHalfHeight = aCol.GetHalfHeightVector();
			Vector2 aCentre = aCol.GetCentrePoint();
			Vector2 bHalfWidth = bCol.GetHalfWidthVector();
			Vector2 bHalfHeight = bCol.GetHalfHeightVector();
			Vector2 bCentre = bCol.GetCentrePoint();

			Matrix3 aTransform = pair.a.GetGlobalTransform();
			Matrix3 bTransform = pair.b.GetGlobalTransform();

			Vector2 normal = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
			float pValue = float.PositiveInfinity;
			float pV;
			float a;
			float b;
			penetration = 0;
			collisionNormal = Vector2.Zero;
			collisionPoints = null;
			//A X Axis
			///////////////////////////////////////////////////////////////////
			if (!doPA(aTransform.GetRightVector()))
				return;
			//A Y Axis
			//////////////////////////////////////////////////////////////////
			if (!doPA(aTransform.GetForwardVector()))
				return;
			//B X Axis
			//////////////////////////////////////////////////////////////////
			if (!doPB(bTransform.GetRightVector()))
				return;
			//B Y Axis
			//////////////////////////////////////////////////////////////////
			if (!doPB(bTransform.GetForwardVector()))
				return;
			//////////////////////////////////////////////////////////////////
			
			collisionNormal = normal;
			penetration = pValue;
			
			//The rest is to find collision points
			//reference:
			//http://www.dyn4j.org/2011/11/contact-points-using-clipping/
			// ^ all other sources I could find on the internet are based off this


			if (pair.a.GetInverseInertia() == 0 && pair.b.GetInverseInertia() == 0)
			{
				//no need to calculate
				collisionPoints = new CollisionPoints(new List<Vector2>() { Vector2.One });
				return;
			}
			Vector2[] mainPoints;
			Vector2[] secondaryPoints;
			
			mainPoints = bCol.GetGlobalPoints();
			secondaryPoints = aCol.GetGlobalPoints();
			
			Edge e1 = GetMostPerpendicular(mainPoints, -1 * normal);
			Edge e2 = GetMostPerpendicular(secondaryPoints,  normal);
			
			Edge reference;
			Edge incident;

			if (Math.Abs((e1.v1 - e1.v2).Dot(normal)) <= Math.Abs((e2.v1 - e2.v2).Dot(normal)))
			{
				reference = e1;
				incident = e2;
			}
			else
			{
				reference = e2;
				incident = e1;
			}

			Vector2 refV = (reference.v2 - reference.v1).Normalised();

			float o1 = refV.Dot(reference.v1);
			CollisionPoints cP = Trim(incident.v1, incident.v2, refV, o1);

			if (cP.points.Count < 2)
			{
				collisionPoints = null;
				return;
			}

			float o2 = refV.Dot(reference.v2);
			cP = Trim(cP.points[0], cP.points[1], -1 * refV, -o2);

			if (cP.points.Count < 2)
			{
				collisionPoints = null;
				return;
			}

			Vector2 refNorm = Vector2.ZCross(refV, -1);

			float max = refNorm.Dot(reference.max);

			cP.depths.Add(refNorm.Dot(cP.points[0]) - max);
			cP.depths.Add(refNorm.Dot(cP.points[1]) - max);
			if (cP.depths[0] < 0)
			{
				cP.depths.RemoveAt(0);
				cP.points.RemoveAt(0);
			}
			if (cP.depths[cP.depths.Count - 1] < 0)
			{
				cP.depths.RemoveAt(cP.depths.Count - 1);
				cP.points.RemoveAt(cP.points.Count - 1);
			}

			collisionPoints = cP;
			return;

			#region Local Functions
			CollisionPoints Trim(Vector2 point1, Vector2 point2, Vector2 norm, float limit)
			{
				List<Vector2> clippedPoints = new List<Vector2>(2);
				float d1 = norm.Dot(point1) - limit;
				float d2 = norm.Dot(point2) - limit;

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
					float projectedDist = n.Dot(points[i]);
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

				if (r.Dot(n) <= l.Dot(n))
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
					if (pV < pValue)
					{
						pValue = pV;
						normal = ax * Math.Sign(b - a);
					}
					return true;
				}
				else
				{
					return false;
				}
			}

			bool doPB(Vector2 ax)
			{
				float aW = ax.Dot(aHalfWidth * Math.Sign(aHalfWidth.Dot(ax)));
				float aH = ax.Dot(aHalfHeight * Math.Sign(aHalfHeight.Dot(ax)));

				float bH = ax.Dot(bHalfHeight);
				float bW = ax.Dot(bHalfWidth);

				a = ax.Dot(aCentre);
				b = ax.Dot(bCentre);

				pV = (((aW + aH) + (bH + bW)) - Math.Abs(a - b));

				if (pV > 0)
				{
					
					if (pV < Math.Abs(pValue))
					{
						pValue = pV;
						normal = ax * Math.Sign(b - a);
					}
					return true;
				}
				else
				{
					return false;
				}
			}
			#endregion
		}

		public List<PhysicsObject> GetObjectList()
		{
			return objList;
		}

		public void RemoveConnection(PhysicsObject obj)
		{
			objList.Remove(obj);
		}
	}

	struct CollisionPair
	{
		public PhysicsObject a;
		public PhysicsObject b;

		public CollisionPair(PhysicsObject a, PhysicsObject b)
		{
			this.a = a; 
			this.b = b; 
		}
	}

	class CollisionPoints
	{
		public List<Vector2> points= new List<Vector2>(2);
		public List<float> depths = new List<float>(2);

		public CollisionPoints(List<Vector2> points)
		{
			this.points = points;
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
}
