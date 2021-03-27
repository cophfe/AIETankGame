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

		public void SetCollider(Collider collider)
		{
			this.collider = collider;
		}

		// NOTE:
		// Xx Yx Zx
		// Xy Yy Zy
		// Xz Yz Zz
		// ^  ^ Up vector
		// Right Vector

		public Vector2 GetAcceleration()
		{
			return force;
		}
		public void AddAcceleration(Vector2 a)
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
				AddVelocity(velocity * drag * -deltaTime * iMass);
				angularVelocity -= angularVelocity * angularDrag * deltaTime;
				//position.x += (velocity.x - velocity.x * drag) * deltaTime;
				//position.y += (velocity.y - velocity.y * drag) * deltaTime;
				AddPosition(velocity * deltaTime);
				AddRotation((angularVelocity) * deltaTime);
			}
			force.x = 0;
			force.y = 0;
			base.Update(deltaTime);
		}
	}
}
