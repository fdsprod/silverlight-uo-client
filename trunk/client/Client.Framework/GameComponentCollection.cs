using System;
using System.Collections.ObjectModel;
using System.Threading;

namespace Client.Framework
{
    public sealed class GameComponentCollection : Collection<IGameComponent>
    {
        private EventHandler<GameComponentCollectionEventArgs> _componentAdded;
        private EventHandler<GameComponentCollectionEventArgs> _componentRemoved;

        public event EventHandler<GameComponentCollectionEventArgs> ComponentAdded
        {
            add
            {
                EventHandler<GameComponentCollectionEventArgs> eventHandler = _componentAdded;
                EventHandler<GameComponentCollectionEventArgs> comparand;
                do
                {
                    comparand = eventHandler;
                    eventHandler = Interlocked.CompareExchange<EventHandler<GameComponentCollectionEventArgs>>(ref _componentAdded, comparand + value, comparand);
                }
                while (eventHandler != comparand);
            }
            remove
            {
                EventHandler<GameComponentCollectionEventArgs> eventHandler = _componentAdded;
                EventHandler<GameComponentCollectionEventArgs> comparand;
                do
                {
                    comparand = eventHandler;
                    eventHandler = Interlocked.CompareExchange<EventHandler<GameComponentCollectionEventArgs>>(ref _componentAdded, comparand - value, comparand);
                }
                while (eventHandler != comparand);
            }
        }

        public event EventHandler<GameComponentCollectionEventArgs> ComponentRemoved
        {
            add
            {
                EventHandler<GameComponentCollectionEventArgs> eventHandler = _componentRemoved;
                EventHandler<GameComponentCollectionEventArgs> comparand;
                do
                {
                    comparand = eventHandler;
                    eventHandler = Interlocked.CompareExchange<EventHandler<GameComponentCollectionEventArgs>>(ref _componentRemoved, comparand + value, comparand);
                }
                while (eventHandler != comparand);
            }
            remove
            {
                EventHandler<GameComponentCollectionEventArgs> eventHandler = _componentRemoved;
                EventHandler<GameComponentCollectionEventArgs> comparand;
                do
                {
                    comparand = eventHandler;
                    eventHandler = Interlocked.CompareExchange<EventHandler<GameComponentCollectionEventArgs>>(ref _componentRemoved, comparand - value, comparand);
                }
                while (eventHandler != comparand);
            }
        }

        protected override void InsertItem(int index, IGameComponent item)
        {
            if (IndexOf(item) != -1)
            {
                throw new ArgumentException("Cannot add the same component multiple times.");
            }
            else
            {
                base.InsertItem(index, item);
                if (item != null)
                    OnComponentAdded(new GameComponentCollectionEventArgs(item));
            }
        }

        protected override void RemoveItem(int index)
        {
            IGameComponent gameComponent = this[index];
            base.RemoveItem(index);
            if (gameComponent != null)
                OnComponentRemoved(new GameComponentCollectionEventArgs(gameComponent));
        }

        protected override void SetItem(int index, IGameComponent item)
        {
            throw new NotSupportedException("Cannot set items into GameComponentCollection.");
        }

        protected override void ClearItems()
        {
            for (int index = 0; index < Count; ++index)
                OnComponentRemoved(new GameComponentCollectionEventArgs(this[index]));
            base.ClearItems();
        }

        private void OnComponentAdded(GameComponentCollectionEventArgs eventArgs)
        {
            if (_componentAdded != null)
                _componentAdded((object)this, eventArgs);
        }

        private void OnComponentRemoved(GameComponentCollectionEventArgs eventArgs)
        {
            if (_componentRemoved != null)
                _componentRemoved((object)this, eventArgs);
        }
    }
}