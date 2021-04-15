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
		//button uses delegates in case it was to be used in more than one place (each button would do a different thing and would need different functions to play)
		OnMouseEnter mE; //these are self explanitory
		OnMouseLeave mL;
		OnMouseHover mH;
		bool entered = false; //is true when the mouse has entered the button bounds
		AABB buttonBounds; //it is easy to store button bounds as an AABB

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
				//this is pretty self explanitory: the button plays mH() when the mouse is hovering above the button, mE() when it enters the button bounds, and mL() when it leaves the button bounds.
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
