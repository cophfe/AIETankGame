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
		protected Vector2 acceleration;
		protected float angularVelocity;
		protected float angularDrag;
		protected float torque;

		public float restitution;
		public float drag = 0f;
		protected float mass;
		public float iMass;
		protected float inetia;
		protected float iInetia;

		public float gravity = 0;

		public PhysicsObject(string fileName, Vector2 position, Vector2 scale, Collider collider = null, float drag = 0, float angularDrag = 0, float restitution = 0,  float rotation = 0, GameObject parent = null) : base(fileName, position, scale, rotation, parent)
		{
			hasPhysics = true;
			this.collider = collider;
			this.restitution = restitution;
			this.drag = drag;
			this.angularDrag = angularDrag;
			if (collider != null) 
			{
				mass = this.collider.GetMass();
				iMass = 1 / mass;
			}
			else
			{
				mass = 1;
				iMass = 1;
			}
				
			
			
		}

		public PhysicsObject(string fileName = null) : base(fileName)
		{
			collider = null;
			drag = 0;

		}

		public void Init(Collider collider, float drag, float restitution, float rotation)
		{

		}

		// NOTE:
		// Xx Yx Zx
		// Xy Yy Zy
		// Xz Yz Zz
		// ^  ^ Up vector
		// Right Vector

		public Vector2 GetAcceleration()
		{
			return acceleration;
		}
		public void AddAcceleration(Vector2 a)
		{
			acceleration += a;
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
				acceleration.y -= gravity * iMass;

				velocity.x += acceleration.x * deltaTime;
				velocity.y += acceleration.y * deltaTime;
				velocity -= velocity * drag * deltaTime;
				angularVelocity -= angularVelocity * angularDrag * deltaTime;
				//position.x += (velocity.x - velocity.x * drag) * deltaTime;
				//position.y += (velocity.y - velocity.y * drag) * deltaTime;
				AddPosition(velocity * deltaTime);
				AddRotation((angularVelocity) * deltaTime);
			}
			acceleration.x = 0;
			acceleration.y = 0;
			base.Update(deltaTime);
		}
	}
}
