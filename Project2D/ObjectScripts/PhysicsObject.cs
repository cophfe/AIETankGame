using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mlib;
using Raylib;
using static Raylib.Raylib;

namespace Project2D
{
	class PhysicsObject : GameObject
	{
		protected Collider collider = null; //the collider connected to the gameobject

		//movement values
		protected Vector2 velocity;
		protected float angularVelocity;
		protected Vector2 force;
		protected float torque;

		//other movement constants (constant for each object)
		public float restitution; //bounciness of object. if it is more than one it will cause bugs
		public float density; //how much mass an object has in a unit of area
		public float drag = 0f; //the wind resistance on an object's velocity
		protected float angularDrag; //the wind resistance on an object's angular velocity
		protected float mass = 1; //how easy an object is to move
		protected float inertia = 1; //how easy an object is to rotate
		protected float iMass = 1; //inverse values are held because they are used a bunch for collisions
		protected float iInertia = 1;

		public PhysicsObject(TextureName image, Vector2 position, float scale, Collider collider = null, float drag = 0, float angularDrag = 0, float restitution = 0,  float rotation = 0, GameObject parent = null, float density = 1, bool isDynamic = true, bool isRotatable = true, bool isDrawn = true) : base(image, position, scale, rotation, parent, isDrawn)
		{
			hasPhysics = true;
			this.collider = collider;
			this.restitution = restitution;
			this.drag = drag;
			this.angularDrag = angularDrag;
			this.density = density;
			
			if (collider != null)
			{
				collider.SetConnectedPhysicsObject(this);
				//mass and inertia are set by the collider, using the physics object's density
				collider.GetMass(out mass, out inertia);
				//everything with a collider is added to the scene's collision manager
				//should probably have made it so that it was the scene that this object is a decendant of but that would have been more difficult
				Game.GetCurrentScene().GetCollisionManager().AddObject(this);
			}
			
			if (isDynamic)
			{
				iMass = 1 / mass;
				iInertia = 1 / inertia;
			}
			else
			{
				//if it isn't dynamic it also isn't rotatable
				isRotatable = false;
				//inverse mass being set to zero makes the physics calculations all work out
				mass = 0;
				iMass = 0;
			}

			if (!isRotatable)
			{
				//if it isn't rotatable inertia and inverse of inertia are set to zero.
				//inverse inertia being set to zero makes the physics calculations all work out
				inertia = 0;
				iInertia = 0;
			}
		}

		public PhysicsObject(TextureName image) : base(image)
		{
			collider = null;
			drag = 0;
		}

		public Collider GetCollider()
		{
			return collider;
		}

		public void SetCollider(Collider collider, Scene scene, bool recalculateMass = false)
		{
			this.collider = collider;
			collider.SetConnectedPhysicsObject(this);

			//you have the option of recalculating mass here
			if (recalculateMass)
			{
				collider.GetMass(out mass, out inertia);
				iMass = 1 / mass;
				iInertia = 1 / inertia;
			}
		}

		public void RemoveCollider(Scene scene)
		{
			collider = null;
			scene.GetCollisionManager().RemoveConnection(this);
		}

		public Vector2 GetForce()
		{
			return force;
		}
		public void AddForce(Vector2 a)
		{
			force += a;
		}
		public float GetMass()
		{
			return mass;
		}
		public float GetInverseMass()
		{
			return iMass;
		}
		public float GetInverseInertia()
		{
			return iInertia;
		}
		public float GetInertia()
		{
			return inertia;
		}
		public void AddTorque(float a)
		{
			torque += a;
		}
		public Vector2 GetVelocity()
		{
			return velocity;
		}
		public void AddVelocity(Vector2 v)
		{
			velocity += v;
		}
		public float GetAngularVelocity()
		{
			return angularVelocity;
		}
		public void AddAngularVelocity(float v)
		{
			angularVelocity += v;
		}

		//https://www.codeproject.com/Articles/1215961/Making-a-D-Physics-Engine-Mass-Inertia-and-Forces
		//god damn there are no resources for calculation of inertia in this very specific situation
		//although it shouldn't matter that much really
		public void AddImpulseAtPosition(Vector2 impulse, Vector2 position)
		{
			velocity += iMass * impulse;
			angularVelocity += iInertia * (-1 * position).ZCross(impulse);
		}

		public override void Update()
		{
			if (hasPhysics)
			{
				velocity += force * (Game.deltaTime * iMass);
				velocity -= velocity * Math.Min(1, drag * Game.deltaTime);

				position += velocity * Game.deltaTime;
				
				angularVelocity += torque * iInertia * Game.deltaTime;
				angularVelocity -= angularVelocity * angularDrag * Game.deltaTime;

				rotation += angularVelocity * Game.deltaTime;

				//force is reset every frame
				force = Vector2.zero;
			}
			base.Update();
		}

		public override GameObject Clone()
		{
			Collider c = collider == null ? null : collider.Clone();
			PhysicsObject p = new PhysicsObject(TextureName.None, GlobalPosition, GlobalScale.x, c, drag, angularDrag, restitution, GlobalRotation, parent, density, mass != 0, inertia != 0, isDrawn);
			p.SetSprite(spriteManager.Clone());
			p.SetSortingOffset(sortingOffset);
			p.GetSprite().SetAttachedGameObject(p);
			p.velocity = velocity;
			p.angularVelocity = angularVelocity;
			p.force = force;
			return p;
		}
	}
}
