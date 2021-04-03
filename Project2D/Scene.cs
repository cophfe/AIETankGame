using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mlib;

namespace Project2D
{
	class Scene : GameObject
	{
		public Scene(List<GameObject> gameObjects)
		{
			globalTransform = Matrix3.Identity;
			foreach (GameObject kid in gameObjects)
			{
				addChild(kid);
			}
		}

		public Scene()
		{
			globalTransform = Matrix3.Identity;
		}

		public override void Update(float deltaTime) //currently the same as in GameObject
		{
			foreach( var child in children)
			{
				child.Update(deltaTime);
			}
			foreach (var child in children)
			{
				child.LateUpdate(deltaTime);
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
			//Top level gameobjects are sorted based on their y position + sorting point offset (the higher y pos the higher sorting priority)
			//children are sorted based on their z value
			GameObject cache;
			int j;
			int n = children.Count;
			for (int i = 1; i < n; i++)
			{
				cache = children[i];
				j = i - 1;

				while (j >= 0 && -children[j].GetSortingOffset() - children[j].LocalPosition.y > -cache.GetSortingOffset() - children[j].LocalPosition.y)
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
			
		}

		public List<GameObject> GetAllSceneChildren()
		{
			return children;
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
