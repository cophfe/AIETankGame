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
	/// <summary>
	/// The most basic object type
	/// </summary>
	class GameObject
	{
		#region Variables
		//scene tree stuff
		protected GameObject parent = null;
		protected List<GameObject> children = new List<GameObject>();

		//matrices
		protected Matrix3 localTransform = new Matrix3();
		protected Matrix3 globalTransform = new Matrix3();

		//drawing
		protected bool isDrawn = false;
		protected Sprite spriteManager;

		//local information
		protected Vector2 position;
		protected Vector2 scale;
		protected float rotation;
		protected float sortingOffset;
		//physics
		protected bool hasPhysics = false;

		//id
		static ulong idCounter = 0;
		#endregion

		#region Initiation
		public GameObject()
		{
			hasPhysics = false;
			Init(TextureName.None, Vector2.Zero, 1, 0, null, false);
		}

		public GameObject(TextureName image, Vector2 position, float scale, float rotation = 0, GameObject parent = null, bool isDrawn = true, SpriteLayer layer = SpriteLayer.Midground)
		{
			Init(image, position, scale, rotation, parent, isDrawn, layer);
		}

		public GameObject(TextureName image)
		{
			Init(image, Vector2.Zero, 1, 0, null, true);
		}

		protected void Init(TextureName image, Vector2 position, float scale, float rotation, GameObject parent, bool isDrawn = true, SpriteLayer layer = SpriteLayer.Midground)
		{
			this.isDrawn = isDrawn;

			id = idCounter;
			idCounter++;
			spriteManager = new Sprite(Game.GetTextureFromName(image), this, RLColor.WHITE, 1, layer);
			
			if (parent != null)
				parent.AddChild(this);

			//position and scale will be zero if no values are given
			localTransform = Matrix3.GetTranslation(position) * Matrix3.GetRotateZ(rotation) * Matrix3.GetScale(Vector2.One * scale);
			this.position = position;
			this.rotation = rotation;
			this.scale = Vector2.One * scale;
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

		protected float id;

		public float Id
		{
			get
			{
				return id;
			}
		}

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

		public void SortChildren()
		{
			//using insertion sort because it is fast for when an array is almost sorted
			//also it is stable which is important
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
			rotation %= Trig.pi * 2;
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
				position = (localTransform * globalTransform.Inverse() * Matrix3.GetTranslation(value)).GetTranslation();
				
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
				//It is worrying that this requires a negative to be correct
				rotation %= Trig.pi * 2;
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
				rotation %= Trig.pi * 2;
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
				scale = (localTransform * globalTransform.Inverse() * Matrix3.GetScale(value)).GetScale();
			}
		}
		#endregion

	}
}
