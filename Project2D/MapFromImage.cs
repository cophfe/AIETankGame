using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mlib;
using Raylib;
using static Raylib.Raylib;

namespace Project2D
{
	static class MapFromImage
	{
		static Color wallColor = Color.FromArgb(0,0,0);
		static Color chickenColor = Color.FromArgb(255,255,255);
		static Color playerColor = Color.FromArgb(255,0,0);
		static Color baleColor = Color.FromArgb(255,255,0);
		static Color sideWallColor = Color.FromArgb(0,0,255); //some pos are off by one
		static Vector2[,] collisionMap = new Vector2[12,2] { { new Vector2(9f, 0.5f), new Vector2(14, 1) }, { new Vector2(4, 6.5f), new Vector2(6, 1) }, { new Vector2(12, 9.5f), new Vector2(8, 1) }, { new Vector2(4, 11.5f), new Vector2(6, 1) }, { new Vector2(12, 14.5f), new Vector2(8, 1) },
		 { new Vector2(1.5f, 3.5f), new Vector2(1, 6) }, { new Vector2(10.5f, 4), new Vector2(1, 6) }, { new Vector2(16.5f, 5), new Vector2(1, 16) }, { new Vector2(16.5f, 12.5f), new Vector2(1, 7) }, { new Vector2(0.5f, 9f ), new Vector2(1, 4) }, { new Vector2(7.5f, 7.5f), new Vector2(1, 1) }, { new Vector2(7.5f, 13), new Vector2(1, 2) },};

		public static void MakeSceneFromImage(PhysicsObject wallTemplate, PhysicsObject baleTemplate, Character player, PhysicsObject chickenTemplate, string map, Scene s, bool useCollisionMap = false)
		{
			if (useCollisionMap)
			{
				for (int i = 0; i < collisionMap.GetLength(0); i++)
				{
					new PhysicsObject(TextureName.None, collisionMap[i, 0] * 271 + new Vector2(-135.5f, -94f), 1, new Collider(Vector2.Zero, collisionMap[i, 1].x * 271, collisionMap[i, 1].y * 271), restitution: 1, parent: s, isDynamic: false, isDrawn: false);
				}
			}
			int chickenTotal = 0;
			Bitmap image = new Bitmap(map);
			float sizeX = wallTemplate.GetSprite().GetWidth() / wallTemplate.LocalScale.x;
			float sizeY = wallTemplate.GetSprite().GetWidth() / wallTemplate.LocalScale.x;

			GameObject cache;
			for (int y = 0; y < image.Height; y++)
			{
				for (int x = 0; x < image.Width; x++)
				{
					Color c = image.GetPixel(x, y);
					
					if (c == wallColor)
					{
						cache = wallTemplate.Clone();
						cache.LocalPosition = new Vector2(sizeX * x, sizeY * y);
						s.AddChild(cache);
					}
					else if(c == chickenColor)
					{
						cache = chickenTemplate.Clone();
						cache.LocalPosition = new Vector2(sizeX * x, sizeY * y);
						s.AddChild(cache);
						chickenTotal++;
					}
					else if (c == playerColor)
					{
						player.LocalPosition = new Vector2(sizeX * x, sizeY * y);
						s.AddChild(player);
					}
					else if (c == baleColor)
					{
						cache = baleTemplate.Clone();
						cache.LocalPosition = new Vector2(sizeX * x, sizeY * y);
						s.AddChild(cache);
					}
					else if (c == sideWallColor)
					{
						cache = wallTemplate.Clone();
						cache.LocalPosition = new Vector2(sizeX * x, sizeY * y);
						cache.SetSprite(new Sprite(Game.GetTextureFromName(TextureName.SideWall), cache, RLColor.WHITE));
						cache.GetSprite().SetLayer(SpriteLayer.Foreground);
						
						s.AddChild(cache);
					}
				}
			}

			baleTemplate.RemoveCollider(s);
			baleTemplate.Delete();
			chickenTemplate.RemoveCollider(s);
			chickenTemplate.Delete();
			wallTemplate.RemoveCollider(s);
			wallTemplate.Delete();

			player.chickenTotalInScene = chickenTotal;

		}
	}
}
