using System.Windows.Controls;
using Client.Framework;

namespace Client
{
    public class ClientGame : Game
    {
        Scene scene;

        public ClientGame(DrawingSurface surface)
            : base(surface)
        {
            IsFixedTimeStep = false;
        }

        protected override void Initialize()
        {
            base.Initialize();

            scene = new Scene(DrawingSurface);
        }

        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            scene.Draw(gameTime);
        }
    }
}