#region Using Statements

using System;
using System.Collections.Generic;
using Xen.Input.Mapping;
using Xen.Input.State;

#endregion Using Statements

namespace Xen.Input
{
    /// <summary>
    /// Enumeration for a player method of input (eg, gamepad or keyboard and mouse)
    /// </summary>
    public enum ControlInput
    {
        /// <summary></summary>
        GamePad1,
        /// <summary></summary>
        GamePad2,
        /// <summary></summary>
        GamePad3,
        /// <summary></summary>
        GamePad4,
        /// <summary>[Windows Only]</summary>
        KeyboardMouse
    }

#if !XBOX360

    /// <summary>
    /// [Windows Only] Mouse input method, such as buttons, axis and wheel
    /// </summary>
    public enum MouseInput
    {
        /// <summary></summary>
        LeftButton,
        /// <summary></summary>
        RightButton,
        /// <summary></summary>
        MiddleButton,
        /// <summary></summary>
        XButton1,
        /// <summary></summary>
        XButton2,
        /// <summary></summary>
        XAxis,
        /// <summary></summary>
        YAxis,
        /// <summary></summary>
        ScrollWheel
    }

#endif

    /// <summary>
    /// Collection containing 4 <see cref="PlayerInput"/> objects. Indexable by integer or <see cref="PlayerIndex"/> enum
    /// </summary>
#if !DEBUG_API

    [System.Diagnostics.DebuggerStepThrough]
#endif
    public sealed class PlayerInputCollection : IList<PlayerInput>
    {
        private readonly PlayerInput[] array;
        internal bool asyncAcess;

        internal PlayerInputCollection(PlayerInput[] array)
        {
            this.array = array;
        }

        int IList<PlayerInput>.IndexOf(PlayerInput item)
        {
            return item.PlayerIndex;
        }

        void IList<PlayerInput>.Insert(int index, PlayerInput item)
        {
            throw new NotSupportedException();
        }

        void IList<PlayerInput>.RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        PlayerInput IList<PlayerInput>.this[int index]
        {
            get
            {
                return array[index];
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Get a <see cref="PlayerInput"/> object by index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public PlayerInput this[int index]
        {
            get
            {
                return array[index];
            }
        }

        /// <summary>
        /// Get a <see cref="PlayerInput"/> object by <see cref="PlayerIndex"/>
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        //public PlayerInput this[PlayerIndex index]
        //{
        //    get
        //    {
        //        return array[(int)index];
        //    }
        //}

        void ICollection<PlayerInput>.Add(PlayerInput item)
        {
            throw new NotSupportedException();
        }

        void ICollection<PlayerInput>.Clear()
        {
            throw new NotSupportedException();
        }

        bool ICollection<PlayerInput>.Contains(PlayerInput item)
        {
            throw new NotSupportedException();
        }

        void ICollection<PlayerInput>.CopyTo(PlayerInput[] array, int arrayIndex)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Number of <see cref="PlayerInput"/> objects (always 4)
        /// </summary>
        public int Count
        {
            get { return array.Length; }
        }

        /// <summary>
        /// Always true
        /// </summary>
        public bool IsReadOnly
        {
            get { return true; }
        }

        bool ICollection<PlayerInput>.Remove(PlayerInput item)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public IEnumerator<PlayerInput> GetEnumerator()
        {
            return ((IEnumerable<PlayerInput>)array).GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return array.GetEnumerator();
        }
    }

    /// <summary>
    /// Class storing a players user input (through either a gamepad or through a keyboard and mouse). This class is best accessed through <see cref="UpdateState.PlayerInput"/>
    /// </summary>
#if !DEBUG_API

    [System.Diagnostics.DebuggerStepThrough]
#endif
    public sealed class PlayerInput
    {
        private readonly KeyboardInputState chatPadState = new KeyboardInputState();
        private bool ronly;
        private readonly PlayerInputCollection parent;
        private ControlInput ci;
        private KeyboardMouseControlMapping kmb = new KeyboardMouseControlMapping();
        internal InputMapper mapper = new InputMapper();
        internal InputState istate = new InputState();
        private int index;

        internal PlayerInput(int index, PlayerInputCollection parent)
        {
            if (parent == null)
                throw new ArgumentNullException();
            this.index = index;
            this.parent = parent;

            kmb.inputParent = parent;
            mapper.inputParent = parent;
        }

        internal bool ReadOnly
        {
            get { return ronly; }
            set { ronly = value || parent.asyncAcess; }
        }

        /// <summary>
        /// Gets the player index for this input
        /// </summary>
        public int PlayerIndex
        {
            get { return index; }
        }

        /// <summary>
        /// [Xbox Only] Gets the current state of the chatpad
        /// </summary>
        public KeyboardInputState ChatPadState
        {
            get { return chatPadState; }
        }

        /// <summary>
        /// Gets the state of the players input (use this object to respond to player input)
        /// </summary>
        public InputState InputState
        {
            get { return istate; }
        }

        /// <summary>
        /// (Advanced use) Gets/Sets the virtual class that performs custom control mapping logic. The default implementation does no remapping
        /// </summary>
        public InputMapper InputMapper
        {
            get { return mapper; }
            set
            {
                if (ReadOnly)
                    throw new ArgumentException("readonly");

                if (value == null)
                    throw new ArgumentNullException();

                mapper = value;
                mapper.inputParent = parent;
            }
        }

        /// <summary>
        /// Gets/Sets the class used for mapping keyboard/mouse controls to gamepad equivalent controls
        /// </summary>
        public KeyboardMouseControlMapping KeyboardMouseControlMapping
        {
            get { return kmb; }
            set
            {
                if (ReadOnly)
                    throw new ArgumentException("readonly");

                if (value == null)
                    throw new ArgumentNullException();

                kmb = value;
                kmb.inputParent = parent;
            }
        }

        /// <summary>
        /// Gets/Sets the method of control used by this player (Eg, gamepad)
        /// </summary>
        public ControlInput ControlInput
        {
            get { return ci; }
            set
            {
                if (ReadOnly) throw new ArgumentException("readonly");
#if XBOX360
				if (value == ControlInput.KeyboardMouse)
					throw new ArgumentException("KeyboardMouse is not supported on Xbox");
#endif
                ci = value;
            }
        }

        internal void CloneFrom(PlayerInput playerInput)
        {
            this.ci = playerInput.ci;
            this.istate = playerInput.istate;
            this.kmb = playerInput.kmb;
            this.mapper = playerInput.mapper;
        }

#if XBOX360
		internal void UpdatePadState(long tick, ref KeyboardState keyboardState)
		{
			this.chatPadState.Update(tick,ref keyboardState);
		}
#endif
    }
}