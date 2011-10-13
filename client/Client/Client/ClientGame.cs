using System.Windows.Controls;
using Client.Configuration;

namespace Client
{
    public class ClientGame : Engine
    {
        Scene scene;

        public ClientGame(DrawingSurface surface)
            : base(surface)
        {

        }

        protected override void Initialize()
        {
            base.Initialize();

            IConfigurationService configurationService = new ConfigurationService(this);

            scene = new Scene(DrawingSurface);
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            scene.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            scene.Draw(gameTime);
        }
    }
}