using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project2D
{
	class Collider
	{
		protected GameObject tiedGameObject;
		protected ObjectType type;

		public Collider(GameObject tiedObj)
		{
			tiedGameObject = tiedObj;
		}

		public virtual float GetMass()
		{
			return 1;
		}
	}

	class CircleCollider : Collider
	{
		float radius;

		public CircleCollider(GameObject tiedObj) : base(tiedObj)
		{
			type = ObjectType.Circle;
		}

	}

	class BoxCollider : Collider
	{
		float width;
		float height;

		public BoxCollider(GameObject tiedObj) : base(tiedObj)
		{
		}
	}

	class PolygonCollider : Collider
	{
		float[] points;
		int[,] edges;

		public PolygonCollider(GameObject tiedObj) : base(tiedObj)
		{
		}
	}

	enum ObjectType
	{
		Circle,
		Polygon
	}
}
