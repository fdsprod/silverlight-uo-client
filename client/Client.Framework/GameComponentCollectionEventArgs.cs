using System;

namespace Client.Framework
{
    public class GameComponentCollectionEventArgs : EventArgs
    {
        private IGameComponent gameComponent;

        public IGameComponent GameComponent
        {
            get
            {
                return gameComponent;
            }
        }

        public GameComponentCollectionEventArgs(IGameComponent gameComponent)
        {
            gameComponent = gameComponent;
        }
    }
}