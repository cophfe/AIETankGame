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
	class Scene : GameObject
	{
		protected List<GameObject> UI = new List<GameObject>();
		protected SmoothCamera camera = new SmoothCamera();
		protected CollisionManager cM = new CollisionManager();
		public RLColor backgroundColor = new RLColor { a = 255, r = 0x5, g = 0x5, b = 0x00 };

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
		}

		public void UpdateUI()
		{
			foreach (var element in UI)
			{
				element.Update();
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
					j = j - 1;
				}
				children[j + 1] = cache;
			}

			for (int i = 0; i < (int)Layers.Count; i++)
			{
				foreach (var child in children)
				{
					if (child.GetSprite().GetLayer() == i)
						child.Draw();
				}
			}

			//end 2d camera
			camera.EndCamera();

			//draw GUI
			for (int i = 0; i < UI.Count; i++)
			{
				UI[i].Draw();
			}

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
		public void SetBackgroundColor(RLColor c)
		{
			backgroundColor = c;
		}

	}

	enum Layers
	{
		Background,
		Midground,
		Foreground,
		Count
	}

}
