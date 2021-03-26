using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mlib;
using Raylib;
using static Raylib.Raylib;

namespace Raylib
{
	partial struct RLVector2
	{
		public RLVector2(float x, float y)
		{
			this.x = x;
			this.y = y;
		}

		public static implicit operator Vector2(RLVector2 v)
		{
			return new Vector2(v.x, v.y);
		}

		public static implicit operator RLVector2(Vector2 v)
		{
			return new RLVector2(v.x, v.y);
		}
		
	}

	partial struct RLColor
	{
		public static implicit operator Colour(RLColor c)
		{
			return new Colour(c.r, c.g, c.b, c.a);
		}

		public static implicit operator RLColor(Colour c)
		{
			return new RLColor(c.GetRed(), c.GetGreen(), c.GetBlue(), c.GetAlpha());
		}
	}
}

