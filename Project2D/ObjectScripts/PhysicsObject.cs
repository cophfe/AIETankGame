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
		protected Collider collider = null;

		//protected Vector2 position;
		protected Vector2 velocity;
		protected Vector2 force;
		protected float angularVelocity;
		protected float angularDrag;
		protected float torque;

		public float restitution;
		public float density;
		public float drag = 0f;
		protected float mass = 1;
		protected float iMass = 1;
		protected float inertia = 1;
		protected float iInertia = 1;

		public float gravity = 0;

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
				collider.GetMass(out mass, out inertia);
				Game.GetCurrentScene().GetCollisionManager().AddObject(this);
			}
			
			if (isDynamic)
			{
				iMass = 1 / mass;
				iInertia = 1 / inertia;
			}
			else
			{
				isRotatable = false;
				mass = 0;
				iMass = 0;
			}
			if (!isRotatable)
			{
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
			if (this.collider != null)
				RemoveCollider(scene);
			this.collider = collider;
			collider.SetConnectedPhysicsObject(this);
			if (recalculateMass)
			{
				collider.GetMass(out mass, out inertia);
				iMass = 1 / mass;
				iInertia = 1 / inertia;
			}
			scene.GetCollisionManager().AddObject(this);
		}

		public void RemoveCollider(Scene scene)
		{
			this.collider = null;
			scene.GetCollisionManager().RemoveConnection(this);
		}

		// NOTE:
		// Xx Yx Zx
		// Xy Yy Zy
		// Xz Yz Zz
		// ^  ^ Up vector
		// Right Vector

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
		public void AddImpulseAtPosition(Vector2 impulse, Vector2 position)
		{
			velocity += iMass * impulse;
			angularVelocity += iInertia * (-1 * position).zCross(impulse);
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

				//angularVelocity += (float)(torque * iInertia * deltaTime);
				

				force = Vector2.Zero;
			}
			base.Update();
		}

		public override GameObject Clone()
		{
			PhysicsObject p = new PhysicsObject(TextureName.None, GlobalPosition, GlobalScale.x, collider.Clone(), drag, angularDrag, restitution, GlobalRotation, parent, density, mass != 0, inertia != 0, isDrawn);
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
