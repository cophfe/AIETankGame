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
    class Program
    {
        static void Main(string[] args)
        {
            Game game = new Game();
            int height = 1000, width = 1000;
            InitWindow(width, height, "gamer game for gamers");
            game.Init();

            while (!WindowShouldClose())
            {
                game.Update();
                game.Draw();
            }

            game.Shutdown();

            CloseWindow();
        }
    }
}
