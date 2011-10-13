using System.Windows.Controls;

namespace Client
{
    public class ClientGame : Game
    {
        Scene scene;

        public ClientGame(DrawingSurface surface)
            : base(surface)
        {

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