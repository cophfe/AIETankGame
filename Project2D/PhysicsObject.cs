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
		Collider collider = null;

		//protected Vector2 position;
		protected Vector2 velocity;
		protected Vector2 force;
		protected float angularVelocity;
		protected float angularDrag;
		protected float torque;

		public float restitution;
		public float density;
		public float drag = 0f;
		protected float mass;
		public float iMass;
		protected float inetia;
		protected float iInetia;

		public float gravity = 0;

		public PhysicsObject(TextureName image, Vector2 position, Vector2 scale, Collider collider = null, float drag = 0, float angularDrag = 0, float restitution = 0,  float rotation = 0, GameObject parent = null, float density = 1, float mass = float.NaN) : base(image, position, scale, rotation, parent)
		{
			hasPhysics = true;
			this.collider = collider;
			this.restitution = restitution;
			this.drag = drag;
			this.angularDrag = angularDrag;
			this.density = density;
			this.mass = mass;

			if (collider == null)
			{
				if (float.IsNaN(mass))
				{
					this.mass = 1;
				}
			}
			else
			{
				collider.SetConnectedPhysicsObject(this);
				if (float.IsNaN(mass))
					this.mass = this.collider.GetMass();
				CollisionManager.AddObject(this);
			}
			iMass = 1/this.mass;

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

		public void SetCollider(Collider collider, bool recalculateMass = false)
		{
			this.collider = collider;
			if (recalculateMass)
			{
				mass = collider.GetMass();
				iMass = 1 / mass;
			}
			CollisionManager.AddObject(this);
		}

		public void RemoveCollider()
		{
			this.collider = null;
			CollisionManager.AddObject(this);
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
		public Vector2 GetVelocity()
		{
			return velocity;
		}
		public void AddVelocity(Vector2 v)
		{
			velocity += v;
		}

		public override void Update(float deltaTime)
		{
			if (hasPhysics)
			{

				AddVelocity(force * deltaTime * iMass);
				AddVelocity(velocity * Math.Min(1,-drag * deltaTime) * iMass);
				angularVelocity -= angularVelocity * angularDrag * deltaTime;
				AddPosition(velocity * deltaTime);
				AddRotation((angularVelocity) * Math.Min(deltaTime,1));
			}
			force = Vector2.Zero;
			base.Update(deltaTime);
		}
	}
}
