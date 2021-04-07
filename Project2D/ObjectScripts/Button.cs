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
	delegate void OnMouseHover();
	delegate void OnMouseEnter();
	delegate void OnMouseLeave();

	class Button : GameObject
	{
		OnMouseEnter mE;
		OnMouseLeave mL;
		OnMouseHover mH;
		bool entered = false;
		AABB buttonBounds;

		public Button(TextureName image, float width, float height, Vector2 position, float scale, Scene scene, OnMouseHover mouseHoverEvent = null, OnMouseEnter mouseEnterEvent = null, OnMouseLeave mouseLeaveEvent = null) : base(image, position, scale, 0, null, true)
		{
			buttonBounds = new AABB(new Vector2(width/2, height/2) , width, height);
			if (scene != null)
				scene.AddUIElement(this);

			if (mouseHoverEvent == null)
			{
				mH += DefaultHoverEvent;
			}
			else
			{
				mH += mouseHoverEvent;
			}

			if (mouseLeaveEvent == null)
			{
				mL += DefaultExitEvent;
			}
			else
			{
				mL += mouseLeaveEvent;
			}

			if (mouseEnterEvent == null)
			{
				mE += DefaultEnterEvent;
			}
			else
			{
				mE += mouseEnterEvent;
			}
		}

		public override void Update()
		{
			if (isDrawn)
			{
				Vector2 delta = GetMousePosition() - position;
				if (delta.x > -buttonBounds.halfWidth && delta.x < buttonBounds.halfWidth && delta.y > -buttonBounds.halfHeight && delta.y < buttonBounds.halfHeight)
				{
					if (entered)
					{
						mH();
					}
					else
					{
						mE();
						entered = true;
					}
				}
				else if (entered)
				{
					mL();
					entered = false;
				}

			}

			base.Update();
		}

		void DefaultHoverEvent()
		{
		}

		void DefaultExitEvent()
		{
		}

		void DefaultEnterEvent()
		{
		}

		public void AddHoverEvent(OnMouseHover h)
		{
			mH += h;
		}

		public void AddLeaveEvent(OnMouseLeave l)
		{
			mL += l;
		}

		public void AddEnterEvent(OnMouseEnter e)
		{
			mE += e;
		}
	}
}
