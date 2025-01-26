using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.VisualBasic;
using ImGuiNET;
using Monogame.Imgui.Renderer;
using MonoGame.ImGui;
namespace CrimeGame
{
    public class DevInfo
    {
        private int frameCount;
        private double elapsedTime;
        public int fps;

        //private readonly ImGUIRenderer guiRenderer;
        
        public DevInfo()
        {
            
        }
        public void LoadContent(ContentManager content)
        {}

        public void Update(GameTime gameTime)
        {
            elapsedTime += gameTime.ElapsedGameTime.TotalMilliseconds;
            frameCount++;

            if (elapsedTime >= 1000)
            {
                fps = frameCount;
                frameCount = 0;
                elapsedTime = 0;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {}
    }
}
