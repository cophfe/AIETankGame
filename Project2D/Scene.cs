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
			foreach (var child in children)
			{
				child.Draw();
			}
		}

		
		


	}

}
