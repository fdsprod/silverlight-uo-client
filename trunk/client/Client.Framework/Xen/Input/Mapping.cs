#region Using Statements

using System;
using System.Windows.Input;
using Client.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Xen.Input.State;

#endregion Using Statements

namespace Xen.Input.Mapping
{
    /// <summary>
    /// This class provides overridable logic for mapping to controller buttons.
    /// </summary>
    /// <remarks>
    /// <para>
    /// If desired, this class can be overridden to map a physical button on the controller to a virtual button. Eg, 'b' could be swapped with 'a'. The player will press 'b', but the application will see 'a' being pressed.
    /// </para>
    /// </remarks>
#if !DEBUG_API

    [System.Diagnostics.DebuggerStepThrough]
#endif
    public class InputMapper
    {
        internal PlayerInputCollection inputParent;
        private KeyboardMouseState kms = new KeyboardMouseState();

        internal void ValidateAsync()
        {
            if (inputParent != null && inputParent.asyncAcess)
                throw new InvalidOperationException("This value is readonly in an Asynchronous State");
        }

        /// <summary>
        /// Current state of the keyboard and mouse. Prefer use of UpdateState.KeyboardState and UpdateState.MouseState.
        /// </summary>
        internal protected KeyboardMouseState KeyboardMouseState
        {
            get { return kms; }
            internal set { kms = value; }
        }

        private bool lockMouse, mouseVisible;

        //internal void SetFullScreen()
        //{
        //    if (!lockMouseSet)
        //        lockMouse = true;
        //}

#if !XBOX360
        /// <summary>
        /// [Windows Only] When using <see cref="ControlInput.KeyboardMouse">KeyboardMouse</see> as the player <see cref="PlayerInput.ControlInput">ControlInput</see>, Set to true to centre the mouse after each frame. Defaults to true when running in fullscreen, false otherwise.
        /// </summary>
#else
		/// <summary>
		/// [Windows Only]
		/// </summary>
#endif

        public bool CentreMouseToWindow
        {
            get { return lockMouse; }
            set { ValidateAsync(); lockMouse = value; }
        }

#if !XBOX360
        /// <summary>
        /// <para>[Windows Only] When using <see cref="ControlInput.KeyboardMouse">KeyboardMouse</see> as the player <see cref="PlayerInput.ControlInput">ControlInput</see>, Sets the visibility state of the mouse.</para>
        /// <para>This property has no effect when using Windows Forms hosting</para>
        /// </summary>
#else
		/// <summary>
		/// [Windows Only]
		/// </summary>
#endif

        public bool MouseVisible
        {
            get { return mouseVisible; }
            set { ValidateAsync(); mouseVisible = value; }
        }

        internal void SetKMS(AppState gameState, ControlInput inputType)
        {
#if !XBOX360

            kms.MousePositionPrevious = gameState.MousePreviousPosition;

            if (inputType == ControlInput.KeyboardMouse)
            {
                gameState.DesireMouseCentred |= lockMouse;
                Game game = (Game)gameState.Application;
                //if (game != null)
                //    game.IsMouseVisible = mouseVisible;

                if (lockMouse && gameState.MouseCentred)
                {
                    kms.MousePositionPrevious = gameState.MouseCentredPosition;
                }
            }

            kms.MouseState = gameState.MouseState;
            kms.KeyboardState = gameState.KeyboardState;
#endif
        }

        internal void UpdateState(ref InputState inputState, UpdateState gameState, KeyboardMouseControlMapping mapping, ControlInput inputType, bool windowFocused)
        {
            //GamePadState gps;

            switch (inputType)
            {
#if !XBOX360
                case ControlInput.KeyboardMouse:
                    this.kms.WindowFocused = windowFocused;
                    UpdateState(gameState, inputState, KeyboardMouseState, mapping);
                    return;
#endif
                //case ControlInput.GamePad1:
                //    gameState.GetGamePadState(0, out gps);
                //    UpdateState(gameState, inputState, gps);
                //    return;
                //case ControlInput.GamePad2:
                //    gameState.GetGamePadState(1, out gps);
                //    UpdateState(gameState, inputState, gps);
                //    return;
                //case ControlInput.GamePad3:
                //    gameState.GetGamePadState(2, out gps);
                //    UpdateState(gameState, inputState, gps);
                //    return;
                //case ControlInput.GamePad4:
                //    gameState.GetGamePadState(3, out gps);
                //    UpdateState(gameState, inputState, gps);
                //    return;
            }
            throw new ArgumentException();
        }

        /// <summary>
        /// Method that may be used when overriding one of the 'UpdateState' methods
        /// </summary>
        protected void SetFaceButtonStates(UpdateState gameState, InputState state, bool AButtonState, bool BButtonState, bool XButtonState, bool YButtonState)
        {
            long tick = gameState.TotalTimeTicks;

            state.buttons.a.SetState(AButtonState, tick);
            state.buttons.b.SetState(BButtonState, tick);
            state.buttons.x.SetState(XButtonState, tick);
            state.buttons.y.SetState(YButtonState, tick);
        }

        /// <summary>
        /// Method that may be used when overriding one of the 'UpdateState' methods
        /// </summary>
        protected void SetDpadStates(UpdateState gameState, InputState state, bool DPadDownButtonState, bool DPadUpButtonState, bool DPadLeftButtonState, bool DPadRightButtonState)
        {
            long tick = gameState.TotalTimeTicks;

            state.buttons.dpadD.SetState(DPadDownButtonState, tick);
            state.buttons.dpadU.SetState(DPadUpButtonState, tick);
            state.buttons.dpadL.SetState(DPadLeftButtonState, tick);
            state.buttons.dpadR.SetState(DPadRightButtonState, tick);
        }

        /// <summary>
        /// Method that may be used when overriding one of the 'UpdateState' methods
        /// </summary>
        protected void SetShoulderButtonStates(UpdateState gameState, InputState state, bool leftShoulderButtonState, bool rightShoulderButtonState)
        {
            long tick = gameState.TotalTimeTicks;

            state.buttons.shoulderL.SetState(leftShoulderButtonState, tick);
            state.buttons.shoulderR.SetState(rightShoulderButtonState, tick);
        }

        /// <summary>
        /// Method that may be used when overriding one of the 'UpdateState' methods
        /// </summary>
        protected void SetStickButtonStates(UpdateState gameState, InputState state, bool leftStickClickButtonState, bool rightStickClickButtonState)
        {
            long tick = gameState.TotalTimeTicks;

            state.buttons.leftStickClick.SetState(leftStickClickButtonState, tick);
            state.buttons.rightStickClick.SetState(rightStickClickButtonState, tick);
        }

        /// <summary>
        /// Method that may be used when overriding one of the 'UpdateState' methods
        /// </summary>
        protected void SetSpecialButtonStates(UpdateState gameState, InputState state, bool backButtonState, bool startButtonState)
        {
            long tick = gameState.TotalTimeTicks;

            state.buttons.back.SetState(backButtonState, tick);
            state.buttons.start.SetState(startButtonState, tick);
        }

        /// <summary>
        /// Method that may be used when overriding one of the 'UpdateState' methods
        /// </summary>
        protected void SetTriggerStates(UpdateState gameState, InputState state, float leftTriggerState, float rightTriggerState)
        {
            state.triggers.leftTrigger = leftTriggerState;
            state.triggers.rightTrigger = rightTriggerState;
        }

        /// <summary>
        /// Method that may be used when overriding one of the 'UpdateState' methods
        /// </summary>
        protected void SetStickStates(UpdateState gameState, InputState state, Vector2 leftStickState, Vector2 rightStickState)
        {
            state.sticks.leftStick = leftStickState;
            state.sticks.rightStick = rightStickState;
        }

        /// <summary>
        /// Override this method to change how raw keyboard and mouse input values are translated to a <see cref="InputState"/> object
        /// </summary>
        /// <param name="gameState"></param>
        /// <param name="state">structure to write input state values</param>
        /// <param name="inputState">stores raw keyboard state</param>
        /// <param name="mapping">stores the current mapped input values</param>
        protected virtual void UpdateState(UpdateState gameState, InputState state, KeyboardMouseState inputState, KeyboardMouseControlMapping mapping)
        {
            long tick = gameState.TotalTimeTicks;

            state.buttons.a.SetState(mapping.A.GetValue(inputState), tick);
            state.buttons.b.SetState(mapping.B.GetValue(inputState), tick);
            state.buttons.x.SetState(mapping.X.GetValue(inputState), tick);
            state.buttons.y.SetState(mapping.Y.GetValue(inputState), tick);

            state.buttons.dpadD.SetState(mapping.DpadDown.GetValue(inputState), tick);
            state.buttons.dpadU.SetState(mapping.DpadUp.GetValue(inputState), tick);
            state.buttons.dpadL.SetState(mapping.DpadLeft.GetValue(inputState), tick);
            state.buttons.dpadR.SetState(mapping.DpadRight.GetValue(inputState), tick);

            state.buttons.shoulderL.SetState(mapping.LeftShoulder.GetValue(inputState), tick);
            state.buttons.shoulderR.SetState(mapping.RightShoulder.GetValue(inputState), tick);

            state.buttons.back.SetState(mapping.Back.GetValue(inputState), tick);
            state.buttons.start.SetState(mapping.Start.GetValue(inputState), tick);
            state.buttons.leftStickClick.SetState(mapping.LeftStickClick.GetValue(inputState), tick);
            state.buttons.rightStickClick.SetState(mapping.RightStickClick.GetValue(inputState), tick);

            state.triggers.leftTrigger = mapping.LeftTrigger.GetValue(inputState, false);
            state.triggers.rightTrigger = mapping.RightTrigger.GetValue(inputState, false);

            Vector2 v = new Vector2();

            v.Y = mapping.LeftStickForward.GetValue(inputState, false) + mapping.LeftStickBackward.GetValue(inputState, true);
            v.X = mapping.LeftStickLeft.GetValue(inputState, true) + mapping.LeftStickRight.GetValue(inputState, false);

            state.sticks.leftStick = v;

            v.Y = mapping.RightStickForward.GetValue(inputState, false) + mapping.RightStickBackward.GetValue(inputState, true);
            v.X = mapping.RightStickLeft.GetValue(inputState, true) + mapping.RightStickRight.GetValue(inputState, false);

            state.sticks.rightStick = v;
        }

        /// <summary>
        /// Override this method to change how raw <see cref="GamePadState"/> values are translated to a <see cref="InputState"/> object
        /// </summary>
        /// <param name="gameState"></param>
        /// <param name="input"></param>
        /// <param name="state"></param>
        protected virtual void UpdateState(UpdateState gameState, InputState input)
        {
            long tick = gameState.TotalTimeTicks;

            //input.buttons.a.SetState(state.Buttons.A == ButtonState.Pressed, tick);
            //input.buttons.b.SetState(state.Buttons.B == ButtonState.Pressed, tick);
            //input.buttons.x.SetState(state.Buttons.X == ButtonState.Pressed, tick);
            //input.buttons.y.SetState(state.Buttons.Y == ButtonState.Pressed, tick);

            //input.buttons.shoulderL.SetState(state.Buttons.LeftShoulder == ButtonState.Pressed, tick);
            //input.buttons.shoulderR.SetState(state.Buttons.RightShoulder == ButtonState.Pressed, tick);

            //input.buttons.dpadD.SetState(state.DPad.Down == ButtonState.Pressed, tick);
            //input.buttons.dpadL.SetState(state.DPad.Left == ButtonState.Pressed, tick);
            //input.buttons.dpadR.SetState(state.DPad.Right == ButtonState.Pressed, tick);
            //input.buttons.dpadU.SetState(state.DPad.Up == ButtonState.Pressed, tick);

            //input.buttons.back.SetState(state.Buttons.Back == ButtonState.Pressed, tick);
            //input.buttons.start.SetState(state.Buttons.Start == ButtonState.Pressed, tick);
            //input.buttons.leftStickClick.SetState(state.Buttons.LeftStick == ButtonState.Pressed, tick);
            //input.buttons.rightStickClick.SetState(state.Buttons.RightStick == ButtonState.Pressed, tick);

            //input.triggers.leftTrigger = state.Triggers.Left;
            //input.triggers.rightTrigger = state.Triggers.Right;

            //input.sticks.leftStick = state.ThumbSticks.Left;
            //input.sticks.rightStick = state.ThumbSticks.Right;
        }
    }

    /// <summary>
    /// Structure that stores a mapping for a single keyboard key / mouse input
    /// </summary>
    /// <remarks>
    /// <para>
    /// This class can be implicitly cast from a MouseInput enumeration and <see cref="Key"/> enumeration
    /// </para>
    /// <para>eg:</para>
    /// <example>
    /// <code>
    /// KeyboardMouseControlMap map = Key.W;
    /// </code>
    /// </example>
    /// </remarks>
#if !DEBUG_API
    [System.Diagnostics.DebuggerStepThrough]
#endif
    public struct KeyboardMouseControlMap : IComparable<KeyboardMouseControlMap>
    {
        /// <summary>
        /// Create a mapping from a <see cref="Key"/> enum (note this class can be implicitly assigned from <see cref="Key"/>)
        /// </summary>
        /// <param name="key"></param>
        /// <remarks>
        /// <para>
        /// This class can be implicitly cast from a <see cref="Key"/> enumeration
        /// </para>
        /// <para>eg:</para>
        /// <example>
        /// <code>
        /// KeyboardMouseControlMap map = Key.W;
        /// </code>
        /// </example>
        /// </remarks>
        public KeyboardMouseControlMap(Key key)
        {
            this.key = key;
#if !XBOX360
            this.usemouse = false;
            this.mouse = MouseInput.LeftButton;
#endif
        }

        /// <summary>Returns the name of the key or mouse selected</summary>
        /// <returns></returns>
        public override string ToString()
        {
#if !XBOX360
            if (this.usemouse)
                return this.mouse.ToString();
            else
                return this.key.ToString();
#else
			return this.key.ToString();
#endif
        }

#if !XBOX360

        /// <summary>
        /// [Windows Only] Create a mapping from a <see cref="MouseInput"/> enum (note this class can be implicitly assigned from <see cref="MouseInput"/>)
        /// </summary>
        /// <param name="mouse"></param>
        /// <remarks>
        /// <para>
        /// This class can be implicitly cast from a <see cref="MouseInput"/> enumeration
        /// </para>
        /// <para>eg:</para>
        /// <example>
        /// <code>
        /// KeyboardMouseControlMap map = MouseInput.LeftButton;
        /// </code>
        /// </example>
        /// </remarks>
        public KeyboardMouseControlMap(MouseInput mouse)
        {
            this.mouse = mouse;
            this.usemouse = true;
            this.key = (Key)0;
        }

        /// <summary>
        /// <see cref="MouseInput"/> implicit cast
        /// </summary>
        /// <param name="mouse"></param>
        /// <returns></returns>
        public static implicit operator KeyboardMouseControlMap(MouseInput mouse)
        {
            return new KeyboardMouseControlMap(mouse);
        }

#endif

        /// <summary>
        /// True if this is an analog input (variable), false if it is digital (on/off)
        /// </summary>
        public bool IsAnalog
        {
            get
            {
#if !XBOX360
                if (usemouse)
                    return mouse == MouseInput.YAxis || mouse == MouseInput.XAxis || mouse == MouseInput.ScrollWheel;
#endif
                return false;
            }
        }

        /// <summary>
        /// <see cref="Key"/> implicit cast
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static implicit operator KeyboardMouseControlMap(Key key)
        {
            return new KeyboardMouseControlMap(key);
        }

        private Key key;
#if !XBOX360
        private bool usemouse;
        private MouseInput mouse;

        /// <summary>
        /// [Windows Only] True if this mapping is using the mouse
        /// </summary>
        public bool UsingMouse
        {
            get { return usemouse; }
        }

        /// <summary>
        /// [Windows Only] Gets/Sets the input directly as a <see cref="MouseInput"/>
        /// </summary>
        public MouseInput MouseInputMapping
        {
            get { if (!usemouse) throw new ArgumentException("UsingMouse == false"); return mouse; }
            set { mouse = value; usemouse = true; }
        }

#endif

        /// <summary>
        /// Gets/Sets the input directly as a <see cref="Key"/>
        /// </summary>
        public Key KeyMapping
        {
            get
            {
#if !XBOX360
                if (usemouse) throw new ArgumentException("UsingMouse == true");
#endif
                return key;
            }
            set
            {
                key = value;
#if !XBOX360
                usemouse = false;
#endif
            }
        }

#if !XBOX360

        /// <summary>
        /// Get the value as a float
        /// </summary>
        public float GetValue(KeyboardMouseState inputState, bool invert)
        {
            if (!inputState.WindowFocused)
                return 0;

            KeyboardState ks = inputState.KeyboardState;
            MouseState ms = inputState.MouseState;

            if (usemouse)
            {
                float val = 0;
                switch (mouse)
                {
                    case MouseInput.LeftButton:
                        val = ms.LeftButton == ButtonState.Pressed ? 1 : 0;
                        break;
                    case MouseInput.MiddleButton:
                        val = ms.MiddleButton == ButtonState.Pressed ? 1 : 0;
                        break;
                    case MouseInput.RightButton:
                        val = ms.RightButton == ButtonState.Pressed ? 1 : 0;
                        break;
                    case MouseInput.ScrollWheel:
                        if (invert)
                            return 0;
                        val = ms.ScrollWheelValue / 640.0f;
                        break;
                    case MouseInput.XButton1:
                        val = ms.XButton1 == ButtonState.Pressed ? 1 : 0;
                        break;
                    case MouseInput.XButton2:
                        val = ms.XButton2 == ButtonState.Pressed ? 1 : 0;
                        break;
                    case MouseInput.XAxis:
                        if (invert)
                            return 0;
                        val = (ms.X - inputState.MousePositionPrevious.X) / 8.0f;
                        break;
                    case MouseInput.YAxis:
                        if (invert)
                            return 0;
                        val = (ms.Y - inputState.MousePositionPrevious.Y) / -8.0f;
                        break;
                }

                if (invert)
                    return -val;
                return val;
            }
            else
            {
                if (ks.IsKeyDown(key))
                {
                    if (invert)
                        return -1;
                    return 1;
                }
                return 0;
            }
        }

        /// <summary>
        /// Get the value as a boolean
        /// </summary>
        public bool GetValue(KeyboardMouseState inputState)
        {
            if (!inputState.WindowFocused)
                return false;

            if (usemouse)
            {
                switch (mouse)
                {
                    case MouseInput.LeftButton:
                        return inputState.MouseState.LeftButton == ButtonState.Pressed;
                    case MouseInput.MiddleButton:
                        return inputState.MouseState.MiddleButton == ButtonState.Pressed;
                    case MouseInput.RightButton:
                        return inputState.MouseState.RightButton == ButtonState.Pressed;
                    case MouseInput.XButton1:
                        return inputState.MouseState.XButton1 == ButtonState.Pressed;
                    case MouseInput.XButton2:
                        return inputState.MouseState.XButton2 == ButtonState.Pressed;
                }
                throw new ArgumentException();
            }
            else
                return inputState.KeyboardState.IsKeyDown(key);
        }

#else
		/// <summary>
		/// Get the value as a float
		/// </summary>
		public float GetValue(KeyboardMouseState inputState, bool invert)
		{
			KeyboardState ks = inputState.KeyboardState;

			if (ks.IsKeyDown(key))
			{
				if (invert)
					return -1;
				return 1;
			}
			return 0;
		}
		/// <summary>
		/// Get the value as a boolean
		/// </summary>
		public bool GetValue(KeyboardMouseState inputState)
		{
			return inputState.KeyboardState.IsKeyDown(key);
		}
#endif

        /// <summary>
        /// IComparable implementation
        /// </summary>
        /// <param name="map"></param>
        /// <returns></returns>
        public int CompareTo(KeyboardMouseControlMap map)
        {
#if !XBOX360
            if (map.usemouse != usemouse)
            {
                if (this.usemouse)
                    return 1;
                else
                    return -1;
            }

            if (key != map.key)
                return (int)map.key - (int)key;
            return (int)map.mouse - (int)mouse;
#else
		return (int)map.key - (int)key;
#endif
        }
    }

    /// <summary>
    /// <para>This class is designed to only be used in windows, it maps keyboard and mouse inputs to a virtual controller</para>
    /// <para>Class that stores <see cref="KeyboardMouseControlMap">mapping objects</see> for mapping keyboard/mouse inputs into an equiavlent control mapping</para>
    /// </summary>
#if !DEBUG_API

    [System.Diagnostics.DebuggerStepThrough]
#endif
    public sealed class KeyboardMouseControlMapping
    {
        internal PlayerInputCollection inputParent;
        private KeyboardMouseControlMap ls_f, rs_f, ls_b, rs_b, rs_l, ls_l, ls_r, rs_r, leftTrigger, rightTrigger, a, b, x, y, back, start, DpadF, DpadB, DpadL, DpadR, leftClick, rightClick, shoulderL, shoulderR;

        /// <summary></summary>
        public KeyboardMouseControlMapping()
        {
#if !XBOX360
            ls_f = Key.W;
            rs_f = MouseInput.YAxis;
            ls_b = Key.S;
            rs_b = MouseInput.YAxis;
            ls_l = Key.A;
            rs_l = MouseInput.XAxis;
            ls_r = Key.D;
            rs_r = MouseInput.XAxis;
            leftTrigger = MouseInput.LeftButton;
            rightTrigger = MouseInput.RightButton;
#else
				//why someone would want to use the keyboard for control on the 360 is beyond me..
			ls_f = Key.W; rs_f = Key.I;
			ls_b = Key.S; rs_b = Key.K;
			ls_l = Key.A; rs_l = Key.J;
			ls_r = Key.D; rs_r = Key.L;
			leftTrigger = Key.LeftShift;
			rightTrigger = Key.LeftControl;
#endif
            a = Key.Space;
            b = Key.E;
            x = Key.Z;
            y = Key.C;
            back = Key.Escape;
            start = Key.Enter;
            DpadF = Key.Up;
            DpadB = Key.Down;
            DpadL = Key.Left;
            DpadR = Key.Right;
            leftClick = Key.Q;
            rightClick = Key.R;
            shoulderL = Key.F;
            shoulderR = Key.V;
        }

        /// <summary>
        /// Returns true there are analog/digital mismatches
        /// </summary>
        /// <returns></returns>
        public bool TestForAnalogDigitalConflicts()
        {
            KeyboardMouseControlMap[] maps = new KeyboardMouseControlMap[] { ls_f, rs_f, ls_b, rs_b, ls_l, rs_l, ls_r, rs_r, leftTrigger, rightTrigger, shoulderL, shoulderR, a, b, x, y, back, start, DpadF, DpadB, DpadL, DpadR, leftClick, rightClick };

            for (int i = 1; i < maps.Length; i++)
            {
                if (maps[i].IsAnalog)
                    continue;
            }
            //make sure buttons are digital
            maps = new KeyboardMouseControlMap[] { shoulderL, shoulderR, a, b, x, y, back, start, DpadF, DpadB, DpadL, DpadR, leftClick, rightClick };
            foreach (KeyboardMouseControlMap map in maps)
                if (map.IsAnalog)
                    return true;

            //directions must be analog or digital, not both
            if (ls_f.IsAnalog != ls_b.IsAnalog ||
                ls_l.IsAnalog != ls_r.IsAnalog ||
                rs_f.IsAnalog != rs_b.IsAnalog ||
                rs_l.IsAnalog != rs_r.IsAnalog)
                return true;

            return false;
        }

        /// <summary>
        /// Returns true if two or more mappings share the same value
        /// </summary>
        /// <returns></returns>
        public bool TestForConflicts()
        {
            KeyboardMouseControlMap[] maps = new KeyboardMouseControlMap[] { ls_f, rs_f, ls_b, rs_b, ls_l, rs_l, ls_r, rs_r, leftTrigger, rightTrigger, shoulderL, shoulderR, a, b, x, y, back, start, DpadF, DpadB, DpadL, DpadR, leftClick, rightClick };
            Array.Sort<KeyboardMouseControlMap>(maps);
            for (int i = 1; i < maps.Length; i++)
            {
                if (maps[i - 1].CompareTo(maps[i]) == 0)
                    return true;
            }
            return false;
        }

        #region properties

        internal void ValidateAsync()
        {
            if (inputParent != null && inputParent.asyncAcess)
                throw new InvalidOperationException("This value is readonly in an Asynchronous State");
        }

#if !XBOX360
        /// <summary>
        /// Gets/Sets the <see cref="Key"/> or <see cref="Mouse"/> mapping for the right trigger (default is <see cref="MouseInput.RightButton"/>)
        /// </summary>
#else
		/// <summary>
		/// Gets/Sets the <see cref="Key"/> mapping for the right trigger
		/// </summary>
#endif

        public KeyboardMouseControlMap RightTrigger
        {
            get { return rightTrigger; }
            set { ValidateAsync(); rightTrigger = value; }
        }

#if !XBOX360
        /// <summary>
        /// Gets/Sets the <see cref="Key"/> or <see cref="Mouse"/> mapping for the left trigger (default is <see cref="MouseInput.LeftButton"/>)
        /// </summary>
#else
		/// <summary>
		/// Gets/Sets the <see cref="Key"/> mapping for the left trigger
		/// </summary>
#endif

        public KeyboardMouseControlMap LeftTrigger
        {
            get { return leftTrigger; }
            set { ValidateAsync(); leftTrigger = value; }
        }

        /// <summary>
        /// Gets/Sets the <see cref="Key"/> mapping for the right stick click button (default is <see cref="Key.R"/>)
        /// </summary>
        public KeyboardMouseControlMap RightStickClick
        {
            get { return rightClick; }
            set { ValidateAsync(); if (value.IsAnalog) throw new ArgumentException(); rightClick = value; }
        }

        /// <summary>
        /// Gets/Sets the <see cref="Key"/> mapping for the left stick click button (default is <see cref="Key.Q"/>)
        /// </summary>
        public KeyboardMouseControlMap LeftStickClick
        {
            get { return leftClick; }
            set { ValidateAsync(); if (value.IsAnalog) throw new ArgumentException(); leftClick = value; }
        }

        /// <summary>
        /// Gets/Sets the <see cref="Key"/> mapping for the right shoulder button (default is <see cref="Key.V"/>)
        /// </summary>
        public KeyboardMouseControlMap RightShoulder
        {
            get { return shoulderR; }
            set { ValidateAsync(); if (value.IsAnalog) throw new ArgumentException(); shoulderR = value; }
        }

        /// <summary>
        /// Gets/Sets the <see cref="Key"/> mapping for the left shoulder button (default is <see cref="Key.F"/>)
        /// </summary>
        public KeyboardMouseControlMap LeftShoulder
        {
            get { return shoulderL; }
            set { ValidateAsync(); if (value.IsAnalog) throw new ArgumentException(); shoulderL = value; }
        }

        /// <summary>
        /// Gets/Sets the <see cref="Key"/> mapping for the d-pad right button (default is <see cref="Key.Right"/>)
        /// </summary>
        public KeyboardMouseControlMap DpadRight
        {
            get { return DpadR; }
            set { ValidateAsync(); if (value.IsAnalog) throw new ArgumentException(); DpadR = value; }
        }

        /// <summary>
        /// Gets/Sets the <see cref="Key"/> mapping for the d-pad left button (default is <see cref="Key.Left"/>)
        /// </summary>
        public KeyboardMouseControlMap DpadLeft
        {
            get { return DpadL; }
            set { ValidateAsync(); if (value.IsAnalog) throw new ArgumentException(); DpadL = value; }
        }

        /// <summary>
        /// Gets/Sets the <see cref="Key"/> mapping for the d-pad down button (default is <see cref="Key.Down"/>)
        /// </summary>
        public KeyboardMouseControlMap DpadDown
        {
            get { return DpadB; }
            set { ValidateAsync(); if (value.IsAnalog) throw new ArgumentException(); DpadB = value; }
        }

        /// <summary>
        /// Gets/Sets the <see cref="Key"/> mapping for the d-pad up button (default is <see cref="Key.Up"/>)
        /// </summary>
        public KeyboardMouseControlMap DpadUp
        {
            get { return DpadF; }
            set { ValidateAsync(); if (value.IsAnalog) throw new ArgumentException(); DpadF = value; }
        }

        /// <summary>
        /// Gets/Sets the <see cref="Key"/> mapping for the start button (default is <see cref="Key.Enter"/>)
        /// </summary>
        public KeyboardMouseControlMap Start
        {
            get { return start; }
            set { ValidateAsync(); if (value.IsAnalog) throw new ArgumentException(); start = value; }
        }

        /// <summary>
        /// Gets/Sets the <see cref="Key"/> mapping for the back button (default is <see cref="Key.Back"/>)
        /// </summary>
        public KeyboardMouseControlMap Back
        {
            get { return back; }
            set { ValidateAsync(); if (value.IsAnalog) throw new ArgumentException(); back = value; }
        }

        /// <summary>
        /// Gets/Sets the <see cref="Key"/> mapping for the 'x' button (default is <see cref="Key.Z"/>)
        /// </summary>
        public KeyboardMouseControlMap X
        {
            get { return x; }
            set { ValidateAsync(); if (value.IsAnalog) throw new ArgumentException(); x = value; }
        }

        /// <summary>
        /// Gets/Sets the <see cref="Key"/> mapping for the 'y' button (default is <see cref="Key.C"/>)
        /// </summary>
        public KeyboardMouseControlMap Y
        {
            get { return y; }
            set { ValidateAsync(); if (value.IsAnalog) throw new ArgumentException(); y = value; }
        }

        /// <summary>
        /// Gets/Sets the <see cref="Key"/> mapping for the 'b' button (default is <see cref="Key.E"/>)
        /// </summary>
        public KeyboardMouseControlMap B
        {
            get { return b; }
            set { ValidateAsync(); if (value.IsAnalog) throw new ArgumentException(); b = value; }
        }

        /// <summary>
        /// Gets/Sets the <see cref="Key"/> mapping for the 'a' button (default is <see cref="Key.Space"/>)
        /// </summary>
        public KeyboardMouseControlMap A
        {
            get { return a; }
            set { ValidateAsync(); if (value.IsAnalog) throw new ArgumentException(); a = value; }
        }

#if !XBOX360
        /// <summary>
        /// Gets/Sets the <see cref="Key"/> or <see cref="Mouse"/> mapping for the left stick's right direction (default is <see cref="Key.D"/>)
        /// </summary>
#else
		/// <summary>
		/// Gets/Sets the <see cref="Key"/> mapping for the left stick's right direction
		/// </summary>
#endif

        public KeyboardMouseControlMap LeftStickRight
        {
            get { return ls_r; }
            set { ValidateAsync(); ls_r = value; }
        }

#if !XBOX360
        /// <summary>
        /// Gets/Sets the <see cref="Key"/> or <see cref="Mouse"/> mapping for the left stick's left direction (default is <see cref="Key.A"/>)
        /// </summary>
#else
		/// <summary>
		/// Gets/Sets the <see cref="Key"/> mapping for the left stick's left direction
		/// </summary>
#endif

        public KeyboardMouseControlMap LeftStickLeft
        {
            get { return ls_l; }
            set { ValidateAsync(); ls_l = value; }
        }

#if !XBOX360
        /// <summary>
        /// Gets/Sets the <see cref="Key"/> or <see cref="Mouse"/> mapping for the left stick's backwards direction (default is <see cref="Key.S"/>)
        /// </summary>
#else
		/// <summary>
		/// Gets/Sets the <see cref="Key"/> mapping for the left stick's backwards direction
		/// </summary>
#endif

        public KeyboardMouseControlMap LeftStickBackward
        {
            get { return ls_b; }
            set { ValidateAsync(); ls_b = value; }
        }

#if !XBOX360
        /// <summary>
        /// Gets/Sets the <see cref="Key"/> or <see cref="Mouse"/> mapping for the left stick's forwards direction (default is <see cref="Key.W"/>)
        /// </summary>
#else
		/// <summary>
		/// Gets/Sets the <see cref="Key"/> mapping for the left stick's forwards direction
		/// </summary>
#endif

        public KeyboardMouseControlMap LeftStickForward
        {
            get { return ls_f; }
            set { ValidateAsync(); ls_f = value; }
        }

#if !XBOX360
        /// <summary>
        /// Gets/Sets the <see cref="Key"/> or <see cref="Mouse"/> mapping for the right stick's right direction (default is <see cref="MouseInput.XAxis"/>)
        /// </summary>
#else
		/// <summary>
		/// Gets/Sets the <see cref="Key"/> mapping for the right stick's right direction
		/// </summary>
#endif

        public KeyboardMouseControlMap RightStickRight
        {
            get { return rs_r; }
            set { ValidateAsync(); rs_r = value; }
        }

#if !XBOX360
        /// <summary>
        /// Gets/Sets the <see cref="Key"/> or <see cref="Mouse"/> mapping for the right stick's left direction (default is <see cref="MouseInput.XAxis"/>)
        /// </summary>
#else
		/// <summary>
		/// Gets/Sets the <see cref="Key"/> mapping for the right stick's left direction
		/// </summary>
#endif

        public KeyboardMouseControlMap RightStickLeft
        {
            get { return rs_l; }
            set { ValidateAsync(); rs_l = value; }
        }

#if !XBOX360
        /// <summary>
        /// Gets/Sets the <see cref="Key"/> or <see cref="Mouse"/> mapping for the right stick's backwards direction (default is <see cref="MouseInput.YAxis"/>)
        /// </summary>
#else
		/// <summary>
		/// Gets/Sets the <see cref="Key"/> mapping for the right stick's backwards direction
		/// </summary>
#endif

        public KeyboardMouseControlMap RightStickBackward
        {
            get { return rs_b; }
            set { ValidateAsync(); rs_b = value; }
        }

#if !XBOX360
        /// <summary>
        /// Gets/Sets the <see cref="Key"/> or <see cref="Mouse"/> mapping for the right stick's forwards direction (default is <see cref="MouseInput.YAxis"/>)
        /// </summary>
#else
		/// <summary>
		/// Gets/Sets the <see cref="Key"/> mapping for the right stick's forwards direction
		/// </summary>
#endif

        public KeyboardMouseControlMap RightStickForward
        {
            get { return rs_f; }
            set { ValidateAsync(); rs_f = value; }
        }

        #endregion properties
    }
}