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
		#region Variables

		
		
		//scene tree stuff
		protected GameObject parent = null;
		protected List<GameObject> children = new List<GameObject>();

		//matrices
		protected Matrix3 localTransform = new Matrix3();
		protected Matrix3 globalTransform = new Matrix3();

		//drawing
		protected bool isDrawn = false;
		private Texture2D texture;
		private Colour colour;
		Rectangle spriteRectangle = new Rectangle();
		Rectangle textureRectangle = new Rectangle();
		RLVector2 origin = new RLVector2();

		//local information
		protected Vector2 position;
		protected Vector2 scale;
		protected float rotation;

		//read only variables for drawing
		private Vector2 globalPosition;
		private float globalRotation;
		private Vector2 globalScale;

		//physics
		protected bool hasPhysics = false;

		//id
		static ulong idCounter = 0;
		#endregion

		#region Initiation
		public GameObject()
		{
			
			hasPhysics = false;
			

			Init(TextureName.None, Vector2.Zero, Vector2.One, 0, null, new Colour(0xFF, 0xFF, 0xFF, 0xFF), false);
		}

		public GameObject(TextureName image, Vector2 position, Vector2 scale, float rotation = 0, GameObject parent = null, bool isDrawn = true)
		{
			Init(image, position, scale, rotation, parent, new Colour(0xFF, 0xFF, 0xFF, 0xFF), isDrawn);
		}

		public GameObject(TextureName image)
		{
			Init(image, Vector2.Zero, Vector2.One, 0, null, new Colour(0xFF, 0xFF, 0xFF, 0xFF), true);
		}

		protected void Init(TextureName image, Vector2 position, Vector2 scale, float rotation, GameObject parent, Colour colour, bool isDrawn = true)
		{

			this.isDrawn = isDrawn;

			id = idCounter;
			idCounter++;
			
			texture = Game.GetTextureFromName(image);
			textureRectangle.width = texture.width;
			textureRectangle.height = texture.height;
			spriteRectangle.width = texture.width * scale.x;
			spriteRectangle.height = texture.height * scale.y;
			origin.x = spriteRectangle.width / 2;
			origin.y = spriteRectangle.height / 2;
			
			if (parent != null)
				parent.addChild(this);

			//position and scale will be zero if no values are given
			localTransform = Matrix3.GetTranslation(position) * Matrix3.GetRotateZ(rotation) * Matrix3.GetScale(scale);
			this.position = position;
			this.rotation = rotation;
			this.scale = scale;
			
			this.colour = colour;
			UpdateTransforms();

		}
		#endregion

		#region Scene Tree Methods
		protected void addChild(GameObject child)
		{
			children.Add(child);
			child.SetParent(this);
		}

		public void SetParent(GameObject parent)
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
			parent.RemoveChild(this);
			UnloadTexture(texture);
		}
		#endregion

		public virtual void Update(float deltaTime)
		{

			foreach (var child in children)
			{
				child.Update(deltaTime);
			}
		}

		public virtual void LateUpdate(float deltaTime)
		{

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
				globalTransform.GetAllTransformations(ref globalPosition, ref globalScale, ref globalRotation);
				
				spriteRectangle.width = texture.width * globalScale.x;
				spriteRectangle.height = texture.height * globalScale.y;
				origin.x = spriteRectangle.width / 2;
				origin.y = spriteRectangle.height / 2;

				spriteRectangle.x = globalPosition.x;
				spriteRectangle.y = globalPosition.y;

				DrawTexturePro(texture, textureRectangle, spriteRectangle, origin, globalRotation * Trig.rad2Deg, colour);
			}


			foreach (var child in children)
			{
				child.Draw();
			}
		}


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
			foreach(var child in children)
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
				rotation = -(localTransform * globalTransform.Inverse() * Matrix3.GetRotateZ(value)).GetZRotation();
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



	}
}
