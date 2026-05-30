using Raylib_cs;
using PVZBloonTest;

Raylib.InitWindow(Constants.SCREEN_W, Constants.SCREEN_H, "PVZ: Bloon Defense");
Raylib.SetTargetFPS(60);
Fonts.Load("fonts/Brianne_s_hand.ttf");
Sprites.LoadAll();   // load all textures (or generate placeholders)

var game = new Game();

while (!Raylib.WindowShouldClose())
{
    float dt = Raylib.GetFrameTime();
    game.Update(dt);
    game.Draw();
}

Sprites.UnloadAll(); // free GPU memory
Fonts.Unload();
Raylib.CloseWindow();
