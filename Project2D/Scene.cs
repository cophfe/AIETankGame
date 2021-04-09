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
	/// A root object containing all UI, cameras, the collision manager, and the drawing rules
	/// </summary>
	class Scene : GameObject
	{
		protected List<GameObject> UI = new List<GameObject>();
		protected SmoothCamera camera = new SmoothCamera();
		protected CollisionManager cM = new CollisionManager();
		public RLColor backgroundColor = new RLColor { a = 0xFF, r = 0x94, g = 0x7d, b = 0x31 };

		public Scene(List<GameObject> gameObjects, List<GameObject> UI = null)
		{
			globalTransform = Matrix3.Identity;
			foreach (GameObject kid in gameObjects)
			{
				AddChild(kid);
			}
			if (UI != null)
			{
				this.UI = UI;
			}
		}

		public Scene()
		{
			globalTransform = Matrix3.Identity;
		}

		public override void Update() //currently the same as in GameObject
		{
			for (int i = 0; i < children.Count; i++)
			{
				children[i].Update();
			}
			camera.UpdateLineOfSight();
		}

		public void UpdateUI()
		{
			for (int i = 0; i < UI.Count; i++)
			{
				UI[i].Update();
			}
		}

		public override void UpdateTransforms()
		{
			foreach (var child in children)
			{
				child.UpdateTransforms();
			}
		}

		public override void Draw()
		{
			BeginDrawing();
			ClearBackground(backgroundColor);
			camera.StartCamera();

			//Top level gameobjects are sorted based on their y position + sorting point offset (the higher y pos the higher sorting priority)
			//children are sorted based on their sprite's z value
			//sort is insertion sort
			GameObject cache;
			int j;
			int n = children.Count;
			for (int i = 1; i < n; i++)
			{
				cache = children[i];
				j = i - 1;

				while (j >= 0 && children[j].GetSortingOffset() + children[j].LocalPosition.y > cache.GetSortingOffset() + cache.LocalPosition.y)
			
				{
					children[j + 1] = children[j];
					j -= 1;
				}
				children[j + 1] = cache;
			}

			for (int i = 0; i < children.Count; i++)
			{
				if (children[i].GetSprite().GetLayer() == (int)SpriteLayer.Background)
					children[i].Draw();
			}
			camera.DrawLineOfSight();
			for (int i = 1; i < (int)SpriteLayer.Count; i++)
			{
				for (int k = 0; k < children.Count; k++)
				{
					if (children[k].GetSprite().GetLayer() == i)
						children[k].Draw();
				}
			}

			//end 2d camera
			camera.EndCamera();

			//draw GUI
			for (int i = 0; i < UI.Count; i++)
			{
				UI[i].Draw();
			}
			DrawText($"{Game.fps}", 10, 10, 10, RLColor.RED);
			EndDrawing();
		}

		public List<GameObject> GetAllSceneChildren()
		{
			return children;
		}

		public void UpdateCollisions()
		{
			cM.CheckCollisions();
		}

		public CollisionManager GetCollisionManager()
		{
			return cM;
		}

		public void SetCamera(SmoothCamera cam)
		{
			camera = cam;
		}

		public SmoothCamera GetCamera()
		{
			return camera;
		}

		public void AddUIElement(GameObject UIObject)
		{
			UI.Add(UIObject);
		}

		public List<GameObject> GetUIElements()
		{
			return UI;
		}

		public void SetBackgroundColor(RLColor c)
		{
			backgroundColor = c;
		}

	}

	enum SpriteLayer
	{
		Background,
		Midground,
		Foreground,
		Count
	}

}
