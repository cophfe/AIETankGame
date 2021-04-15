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
	class GameObject
	{
		//scene tree stuff
		protected GameObject parent = null;
		protected List<GameObject> children = new List<GameObject>();

		//matrices
		protected Matrix3 localTransform = new Matrix3();
		protected Matrix3 globalTransform = new Matrix3();

		//drawing
		protected bool isDrawn = false; //says if object is drawn or not
		protected Sprite spriteManager; //the sprite object that is attached to the gameobject
		protected float sortingOffset; //the value that parent objects are sorted by (everything else is sorted by it's sprite's sort value). its not a great system I admit

		//local information
		//local position, scale and rotation stored and converted to a matrix every frame
		//(I think Finn said many games do it this way)
		protected Vector2 position;
		protected Vector2 scale;
		protected float rotation;

		//physics
		protected bool hasPhysics = false;

		//id (each gameobject has its own id)
		static uint idCounter = 0;

		#region Initiation
		public GameObject()
		{
			Init(TextureName.None, Vector2.zero, 1, 0, null, false);
		}

		//scale is a float because It is best for x and y scaling to be of equal magnitude otherwise it can lead to some bugs, so i just decided to make it always start with a scale with equal magnitude
		public GameObject(TextureName image, Vector2 position, float scale, float rotation = 0, GameObject parent = null, bool isDrawn = true, SpriteLayer layer = SpriteLayer.Midground)
		{
			Init(image, position, scale, rotation, parent, isDrawn, layer);
		}

		public GameObject(TextureName image)
		{
			Init(image, Vector2.zero, 1, 0, null, true);
		}

		//all constructors are just using this (so I don't have to rewrite the same thing a billion times)
		protected void Init(TextureName image, Vector2 position, float scale, float rotation, GameObject parent, bool isDrawn = true, SpriteLayer layer = SpriteLayer.Midground)
		{
			this.isDrawn = isDrawn;

			id = idCounter;
			idCounter++;
			spriteManager = new Sprite(Game.GetTextureFromName(image), this, RLColor.WHITE, 1, layer);
			
			if (parent != null)
				parent.AddChild(this);

			//position and scale will be zero if no values are given
			localTransform = Matrix3.GetTranslation(position) * Matrix3.GetRotateZ(rotation) * Matrix3.GetScale(Vector2.one * scale);
			this.position = position;
			this.rotation = rotation;
			this.scale = Vector2.one * scale;
			sortingOffset = spriteManager.CurrentTexture().height / 2 * scale;
			UpdateTransforms();
		}
		#endregion

		#region Scene Tree Methods
		public virtual void AddChild(GameObject child)
		{
			children.Add(child);
			child.SetParent(this);
		}

		protected void SetParent(GameObject parent)
		{
			if (parent != null)
			{
				RemoveChild(this);
			}
			this.parent = parent;
		}

		void RemoveChild(GameObject child)
		{
			children.Remove(child);
		}

		public void Delete()
		{
			foreach (var child in children)
			{
				child.Delete();
			}
			if (parent != null)
				parent.RemoveChild(this);
		}
		#endregion

		public virtual void Update()
		{
			for (int i = 0; i < children.Count; i++)
			{
				children[i].Update();
			}
		}

		protected float id; //the id of the object (set at initiation)

		public float Id
		{
			get
			{
				return id;
			}
		}

		//Draws the gameobject and it's children
		public virtual void Draw()
		{
			if (isDrawn)
			{
				spriteManager.Draw();
			}

			for (int i = 0; i < children.Count; i++)
			{
				children[i].Draw();
			}
		}

		//children sorting happens whenever this is called
		//(because it is almost never necessary unless it is a top level object)
		public void SortChildren()
		{
			//using insertion sort because it is fast for when an array is almost sorted
			//also it is stable which is important
			//https://www.toptal.com/developers/sorting-algorithms/insertion-sort
			GameObject cache;
			int j;
			for (int i = 1; i < children.Count; i++)
			{
				cache = children[i];
				j = i - 1;

				while (j >= 0 && children[j].GetSprite().GetSort() > cache.GetSprite().GetSort())
				{
					children[j + 1] = children[j];
					j = j - 1;
				}
				children[j + 1] = cache;
			}
		}

		public Sprite GetSprite()
		{
			return spriteManager;
		}

		public void SetSprite(Sprite sprite) 
		{
			spriteManager = sprite;
			sprite.SetAttachedGameObject(this);
		}

		public float GetSortingOffset()
		{
			return sortingOffset;
		}

		//generally sorting offset is set to the bottom of the current texture, but it can be manually set if an object should be above everything else or under everything else
		public void SetSortingOffset(float offset)
		{
			sortingOffset = offset;
		}

		public virtual void SetTint(Colour c)
		{
			if (spriteManager != null)
				spriteManager.SetTint(c);
		}

		public void SetDrawn(bool isDrawn)
		{
			this.isDrawn = isDrawn;
		}

		public bool GetDrawn()
		{
			return isDrawn;
		}

		//since gameobject is a class, it needs a clone function if you want to copy it
		public virtual GameObject Clone()
		{
			GameObject g = new GameObject(TextureName.None, GlobalPosition, GlobalScale.x, GlobalRotation, parent, isDrawn);
			g.SetSprite(spriteManager.Clone());
			g.GetSprite().SetAttachedGameObject(g);
			g.SetSortingOffset(sortingOffset);
			return g;
		}

		#region Transformations
		public virtual void UpdateTransforms()
		{
			//idk where this is supposed to go so it is going here
			localTransform = Matrix3.GetTranslation(position) * Matrix3.GetRotateZ(rotation) * Matrix3.GetScale(scale);

			if (parent == null)
			{
				globalTransform = localTransform;
			}
			else
			{
				globalTransform = parent.GetGlobalTransform() * localTransform;
			}
			foreach (var child in children)
			{
				child.UpdateTransforms();
			}
		}

		public Matrix3 GetGlobalTransform()
		{
			return globalTransform;
		}

		public void AddRotation(float rad)
		{
			rotation += rad;
			//make sure rotation does not add for infinity, limit to max rotation
			rotation %= Num.pi * 2;
		}

		public void AddPosition(Vector2 pos)
		{
			position += pos;
		}

		public Vector2 LocalPosition
		{
			get
			{
				return position;
			}
			set
			{
				position = value;
			}
		}

		public Vector2 GlobalPosition
		{
			get
			{
				return globalTransform.GetTranslation();
			}
			set
			{
				//if there is no parent than you can just set the position
				if (parent == null)
				{
					position = value;
					return;
				}

				//tbh I used trial and error to get this, it is probably the least efficient thing ever
				position = (localTransform * globalTransform.Inverse() * Matrix3.GetTranslation(value)).GetTranslation();
			}
		}

		public float LocalRotation
		{
			get
			{
				return rotation;
			}
			set
			{
				rotation = value;
				rotation %= Num.pi * 2;
			}
		}

		public float GlobalRotation
		{
			get
			{
				return globalTransform.GetZRotation();
			}
			set
			{
				if (parent == null)
				{
					rotation = value;
					return;
				}
				rotation = (localTransform * globalTransform.Inverse() * Matrix3.GetRotateZ(value)).GetZRotation();
				rotation %= Num.pi * 2;
			}
		}
		
		public Vector2 LocalScale
		{
			get
			{
				return scale;
			}
			set
			{
				scale = value;
			}
		}

		public Vector2 GlobalScale
		{
			get
			{
				return globalTransform.GetScale();
			}
			set
			{
				if (parent == null)
				{
					scale = value;
					return;
				}
				scale = (localTransform * globalTransform.Inverse() * Matrix3.GetScale(value)).GetScale();
			}
		}
		#endregion

	}
}
