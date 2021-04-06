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
		static Color sideWallColor = Color.FromArgb(0,0,255);

		public static void MakeSceneFromImage(PhysicsObject wallTemplate, PhysicsObject baleTemplate, Character player, PhysicsObject chickenTemplate, string map, Scene s)
		{
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


		}
	}
}
