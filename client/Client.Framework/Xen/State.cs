#region Using Statements

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Xen.Graphics;
using Xen.Input.State;

#endregion Using Statements

using Xen.Input;
using System.Collections;
using Xen.Graphics.Stack;
using Client.Framework;

namespace Xen
{
    /// <summary>
    /// Interface to state storage objects (<see cref="DrawState"/> and <see cref="UpdateState"/>)
    /// </summary>
    public interface IState
    {
        /// <summary>
        /// True if the current state is potentially running on multiple threads
        /// </summary>
        bool IsAsynchronousState { get; }

        /// <summary>
        /// Get the Application instance
        /// </summary>
        Application Application { get; }

        /// <summary>
        /// Time delta (change) for the last frame/update as a frequency. Eg, 60 for 60fps
        /// </summary>
        float DeltaTimeFrequency { get; }

        /// <summary>
        /// Time delta (change) for the last frame/update as a number of seconds. Eg, 0.0166 for 60fps
        /// </summary>
        float DeltaTimeSeconds { get; }

        /// <summary>
        /// Accurate DeltaTime timespan
        /// </summary>
        long DeltaTimeTicks { get; }

        /// <summary>
        /// Accurate performance time (application time may be different to real time if the application has performance problems)
        /// </summary>
        long TotalTimeTicks { get; }

        /// <summary>
        /// Total time in seconds
        /// </summary>
        float TotalTimeSeconds { get; }
    }
    /// <summary>
    /// State object storing state accessable during an Update() call
    /// </summary>
#if !DEBUG_API

    [System.Diagnostics.DebuggerStepThrough]
#endif
    public sealed partial class UpdateState : IState
    {
        private float dt;
        private float hz;
        private Application application;
        private float seconds;
        private long deltaTime, totalTime;
        private PlayerInputCollection input;
        private UpdateManager manager;
        private bool async;
#if !XBOX360
        private readonly KeyboardInputState keyboard = new KeyboardInputState();
        private readonly MouseInputState mouse = new MouseInputState();
#endif

        /// <summary>
        /// Cast the UpdateState to an XNA gametime
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public static implicit operator GameTime(UpdateState state)
        {
            return state.application.gameTime;
        }

        /// <summary>
        /// The number of ticks that are in one second (10000000)
        /// </summary>
        public const long TicksInOneSecond = 10000000L;

        internal UpdateState(Application application, PlayerInputCollection input)
        {
            this.application = application;
            this.input = input;
        }

        internal void UpdateDelta(float dt, float hz, long deltaTime)
        {
            this.dt = dt;
            this.hz = hz;
            this.deltaTime = deltaTime;
        }

        internal void Update(float dt, float hz, float seconds, long deltaTime, long totalTime)
        {
            this.dt = dt;
            this.hz = hz;
            this.seconds = seconds;

            this.deltaTime = deltaTime;
            this.totalTime = totalTime;
        }

#if !XBOX

        internal void UpdateWindowsInput(ref KeyboardState kb, ref MouseState ms)
        {
            this.keyboard.Update(this.totalTime, ref kb);
            this.mouse.Update(this.totalTime, ref ms);
        }

#else
		internal void UpdateXboxInput(KeyboardState[] pads)
		{
			for (int i = 0; i < 4; i++)
			{
				if (this.PlayerInput[i].ControlInput != ControlInput.KeyboardMouse)
					this.PlayerInput[i].UpdatePadState(this.totalTime, ref pads[(int)this.PlayerInput[i].ControlInput]);
			}
		}
#endif

        /// <summary>
        /// <para>The the item passed in will be drawn at the start of the next frame, before the main application Draw method is called</para>
        /// </summary>
        public void PreFrameDraw(IFrameDraw item)
        {
            if (item == null)
                throw new ArgumentNullException();

            application.PreFrameDraw(item);
        }

        /// <summary>
        /// True if the current state is potentially running on multiple threads
        /// </summary>
        public bool IsAsynchronousState
        {
            get { return async; }
            internal set { async = value; }
        }

        /// <summary>
        /// Application instance
        /// </summary>
        public Application Application
        {
            get { return application; }
        }

        /// <summary>
        /// Gets the update manager managing this object
        /// </summary>
        public UpdateManager UpdateManager
        {
            get { return manager; }
            internal set { manager = value; }
        }

        /// <summary>
        /// Time delta (change) for the last frame/update as a frequency. Eg, 60 for 60fps
        /// </summary>
        public float DeltaTimeFrequency
        {
            get { return hz; }
        }

        /// <summary>
        /// Time delta (change) for the last frame/update as a number of seconds. Eg, 0.0166 for 60fps
        /// </summary>
        public float DeltaTimeSeconds
        {
            get { return dt; }
        }

        /// <summary>
        /// Accurate delta time
        /// </summary>
        public long DeltaTimeTicks
        {
            get { return deltaTime; }
        }

        /// <summary>
        /// Accurate total time
        /// </summary>
        public long TotalTimeTicks
        {
            get { return totalTime; }
        }

        /// <summary>
        /// Total time in seconds
        /// </summary>
        public float TotalTimeSeconds
        {
            get { return seconds; }
        }

#if XBOX360
		/// <summary>
		/// <para>Stores <see cref="PlayerInput"/> instances for each player.</para>
		/// <para>PlayerInput objects represent player GamePads. To access the Keyboard/Mouse in windows, use KeyboardState and MouseState</para>
		/// </summary>
#else
        /// <summary>
        /// <para>Stores <see cref="PlayerInput"/> instances for each player.</para>
        /// <para>PlayerInput objects represent player GamePads. To access the Keyboard/Mouse in windows, use <see cref="KeyboardState"/> and <see cref="MouseState"/></para>
        /// </summary>
#endif

        public PlayerInputCollection PlayerInput
        {
            get { return input; }
        }

        /// <summary>
        /// [Windows Only]
        /// For gamepad function, using <see cref="PlayerInput"/> is recommended over direct state access for simulating a gamepad
        /// </summary>
#if XBOX360
		[Obsolete("Use PlayerInput.ChatPadState")]
#endif

        public KeyboardInputState KeyboardState
        {
#if XBOX360
			get { throw new InvalidOperationException("Use PlayerInput.ChatPadState"); }
#else
            get { return keyboard; }
#endif
        }

#if !XBOX360

        /// <summary>
        /// [Windows Only]
        /// For gamepad function, using <see cref="PlayerInput"/> is recommended over direct state access for simulating a gamepad
        /// </summary>
        public MouseInputState MouseState
        {
            get { return mouse; }
        }

#endif
    }

    //provides a unique id for a shader type (and efficiently too :)
    struct ShaderUID
    {
        private static readonly object sync = new object();
        private static uint id = 0;

        private static uint GetId()
        {
            lock (sync)
            {
                return id++;
            }
        }

        //public struct Type<T> where T : IShader, new()
        //{
        //    public static readonly uint ID = GetId();
        //}
    }

    /// <summary>
    /// Provides access to application state for rendering the current frame.
    /// </summary>
#if !DEBUG_API

    [System.Diagnostics.DebuggerStepThrough]
#endif
    public sealed class FrameState : IState
    {
        private float dt, hz, seconds;
        private long deltaTime, totalTime;
        private readonly Application application;
        private readonly DrawState state;

        internal FrameState(Application app, DrawState state)
        {
            this.application = app;
            this.state = state;
        }

        /// <summary>
        /// Cast the FrameState to a GraphicsDevice. Make sure to dirty the render state if it is changed
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public static explicit operator Microsoft.Xna.Framework.Graphics.GraphicsDevice(FrameState state)
        {
            return state.state.graphics;
        }

        /// <summary>
        /// Cast the FrameState to an XNA gametime
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public static implicit operator GameTime(FrameState state)
        {
            return state.application.gameTime;
        }

        /// <summary>
        /// Gets the approximate frame rate for the last 20 frames
        /// </summary>
        public float ApproximateFrameRate { get { return state.ApproximateFrameRate; } }

        /// <summary>
        /// Returns true if the device supports hardware instancing
        /// </summary>
        public bool SupportsHardwareInstancing { get { return state.Properties.SupportsHardwareInstancing; } }

#if DEBUG

        /// <summary>
        /// Gets the frame statistics for the previous rendered frame
        /// </summary>
        /// <param name="statistics"></param>
        public void GetPreviousFrameStatistics(out DrawStatistics statistics) { state.GetPreviousFrameStatistics(out statistics); }

#endif

        /// <summary>
        /// Gets an integer index, incremented every frame
        /// </summary>
        public int FrameIndex { get { return (state as ICuller).FrameIndex; } }

        internal DrawState DrawState { get { return state; } }

        /// <summary>
        /// <para>Gets an application wide global instance of a Shader by type</para>
        /// <para>Use this method to share instances and reduce the number of live objects (most useful for simpler shaders)</para>
        /// </summary>
        //public T GetShader<T>() where T : class, IShader, new()
        //{
        //    return state.GetShader<T>();
        //}

        /// <summary>
        /// Gets the draw flag stack
        /// </summary>
        public Graphics.Stack.DrawFlagStack DrawFlags
        {
            get { return state.DrawFlags; }
        }

        /// <summary>
        /// <para>The the item passed in will have it's Draw method called at the start of the next frame, before the main application Draw method is next called</para>
        /// </summary>
        public void PreFrameDraw(IFrameDraw draw)
        {
            this.application.PreFrameDraw(draw);
        }

        /// <summary>
        /// Gets the unique ID index for a non-global shader attribute. For use in a call to IShader.SetAttribute, IShader.SetTexture or IShader.SetSamplerState"/>
        /// </summary>
        /// <param name="name">case sensitive name of the shader attribute</param>
        /// <returns>globally unique index of the attribute name</returns>
        //public int GetShaderAttributeNameUniqueID(string name)
        //{
        //    return state.GetShaderAttributeNameUniqueID(name);
        //}

        /// <summary>
        /// Returns a buffer that instances can be written to. Use to draw instances in a call to DrawInstances"/>
        /// </summary>
        public InstanceBuffer GetDynamicInstanceBuffer(int count)
        {
            return state.GetDynamicInstanceBuffer(count);
        }

        /// <summary>
        /// Gets the cullers stack
        /// </summary>
        public Graphics.Stack.CullersStack Cullers { get { return state.Cullers; } }

        /// <summary>
        /// Gets the shader system global interface
        /// </summary>
        //public IShaderGlobals ShaderGlobals
        //{
        //    get { return state.ShaderGlobals; }
        //}

        internal void Update(float dt, float hz, long deltaTime, long totalTime, float seconds)
        {
            this.deltaTime = deltaTime;
            this.totalTime = totalTime;

            this.dt = dt;
            this.hz = hz;
            this.seconds = seconds;
        }

        bool IState.IsAsynchronousState
        {
            get { return false; }
        }

        /// <summary>
        /// Gets the Application instance
        /// </summary>
        public Application Application
        {
            get { return application; }
        }

        /// <summary>
        /// Time delta (change) for the last frame as a frequency. Eg, 60 for 60fps
        /// </summary>
        public float DeltaTimeFrequency
        {
            get { return hz; }
        }

        /// <summary>
        /// Time delta (change) for the last frame as a number of seconds. Eg, 0.0166 for 60fps
        /// </summary>
        public float DeltaTimeSeconds
        {
            get { return dt; }
        }

        /// <summary>
        /// Accurate delta time
        /// </summary>
        public long DeltaTimeTicks
        {
            get { return deltaTime; }
        }

        /// <summary>
        /// Accurate total time
        /// </summary>
        public long TotalTimeTicks
        {
            get { return totalTime; }
        }

        /// <summary>
        /// Accurate total time in seconds
        /// </summary>
        public float TotalTimeSeconds
        {
            get { return seconds; }
        }
    }

    /// <summary>
    /// Provides access to the Render State, Camera, Culler and many other methods and objects used during drawing. This class should provide almost everything an application requires when drawing (as opposed to directly accessing the GraphicsDevice)
    /// </summary>
#if !DEBUG_API

    [System.Diagnostics.DebuggerStepThrough]
#endif
    public sealed partial class DrawState : IState
    {
        private float dt;
        private float hz;
        private long deltaTime, totalTime;
        private readonly Application application;
        private DrawTarget target;
        private float seconds;
        internal Microsoft.Xna.Framework.Graphics.GraphicsDevice graphics;
        private int frameIndex;
        //private IShader[] staticShaderCache = new IShader[1024];

        /// <summary>
        /// Get the GraphicsDevice from the DrawState
        /// </summary>
        public static implicit operator Microsoft.Xna.Framework.Graphics.GraphicsDevice(DrawState state)
        {
            return state.graphics;
        }

        /// <summary>
        /// Cast the DrawState to an XNA gametime
        /// </summary>
        public static implicit operator GameTime(DrawState state)
        {
            return state.application.gameTime;
        }

        bool IState.IsAsynchronousState { get { return false; } }

        internal void IncrementFrameIndex()
        {
            frameIndex++;
        }

        int ICuller.FrameIndex { get { return frameIndex; } }

        /// <summary>
        /// <para>Gets an application wide global instance of a Shader by type</para>
        /// <para>Use this method to share instances and reduce the number of live objects (most useful for simpler shaders)</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        //        public T GetShader<T>() where T : class, IShader, new()
        //        {
        //            uint id = ShaderUID.Type<T>.ID;
        //            if (id == staticShaderCache.Length)
        //                Array.Resize(ref staticShaderCache, staticShaderCache.Length * 2);

        //            T shader = staticShaderCache[id] as T;
        //            if (shader == null)
        //            {
        //#if DEBUG
        //                if (!typeof(Xen.Graphics.ShaderSystem.BaseShader).IsAssignableFrom(typeof(T)))
        //                    throw new ArgumentException(string.Format("Shader {0} does not derive from BaseShader", typeof(T)));
        //#endif
        //                shader = new T();
        //                staticShaderCache[id] = shader;
        //            }
        //#if DEBUG
        //            if (shader.GetType() != typeof(T)) //sanity check
        //                throw new InvalidOperationException();
        //#endif
        //            return shader;
        //        }

        /// <summary>The number of ticks that are in one second (10000000)</summary>
        public const long TicksInOneSecond = 10000000L;

        /// <summary>
        /// <para>The the item passed in will have it's Draw method called at the start of the next frame, before the main application Draw method is next called</para>
        /// </summary>
        public void PreFrameDraw(IFrameDraw item)
        {
            if (item == null)
                throw new ArgumentNullException();

            application.PreFrameDraw(item);
        }

#if DEBUG

        /// <summary>Gets the <see cref="DrawStatistics"/> for the previous drawn frame (DEBUG ONLY)</summary>
        /// <param name="statistics"></param>
        public void GetPreviousFrameStatistics(out DrawStatistics statistics)
        {
            statistics = application.previousFrame;
        }

        /// <summary>Gets the <see cref="DrawStatistics"/> for the current inprogress frame, the data may be incomplete. Use <see cref="GetPreviousFrameStatistics"/> for full statistics on the previous frame. (DEBUG ONLY)</summary>
        /// <param name="statistics"></param>
        public void GetCurrentFrameStatistics(out DrawStatistics statistics)
        {
            statistics = application.currentFrame;
        }

#endif

        private void ValidateRenderState()
        {
            if (this.target == null)
                throw new InvalidOperationException("Render state can only be modified within a Draw Target internal Draw call");
        }

        internal DrawState(Application application)
        {
            if (application == null)
                throw new ArgumentNullException();

            this.application = application;

            this.matrices = new MatrixProviderCollection(this);

            this.renderState = new DeviceRenderStateStack(this);
            //this.shaderStack = new ShaderStack(this);
            this.flagStack = new DrawFlagStack();

            this.cameraStack = new CameraStack(this);
            this.cullerStack = new CullersStack(this);

            this.streamBuffers = new List<StreamBuffer>();
            this.frequencyBuffer = new Xen.Graphics.VerticesGroup(2);
            this.frequency = new Xen.Graphics.StreamFrequency(frequencyBuffer, Xen.Graphics.StreamFrequency.DataLayout.Stream0Geometry_Stream1InstanceData);
        }

        internal void Update(float dt, float hz, long deltaTime, long totalTime, float seconds, Microsoft.Xna.Framework.Graphics.GraphicsDevice device)
        {
            this.deltaTime = deltaTime;
            this.totalTime = totalTime;

            this.dt = dt;
            this.hz = hz;
            this.target = null;
            this.seconds = seconds;
            this.graphics = device;

            //if (this.shaderSystem == null)
            //    this.shaderSystem = new ShaderSystemState(this, application, this.graphics, this.matrices);

            if (this.properties == null)
                this.properties = new DrawStateProperties(this);
        }

        internal void Update(Microsoft.Xna.Framework.Graphics.GraphicsDevice device)
        {
            this.graphics = device;
        }

        /// <summary>
        /// Application instance
        /// </summary>
        public Application Application
        {
            get { return application; }
        }

        /// <summary>
        /// Current DrawTarget that is being drawn to
        /// </summary>
        public DrawTarget DrawTarget
        {
            get { return target; }
            internal set { target = value; }
        }

        //no validation...
        internal DrawTarget GetDrawTarget()
        {
            return target;
        }

        /// <summary>
        /// Approximate frame rate average for the last 20 drawn frames
        /// </summary>
        public float ApproximateFrameRate
        {
            get { return application.approximateFrameRate; }
        }

        /// <summary>
        /// Time delta (change) for the last frame as a frequency. Eg, 60 for 60fps
        /// </summary>
        public float DeltaTimeFrequency
        {
            get { return hz; }
        }

        /// <summary>
        /// Time delta (change) for the last frame as a number of seconds. Eg, 0.0166 for 60fps
        /// </summary>
        public float DeltaTimeSeconds
        {
            get { return dt; }
        }

        /// <summary>
        /// Accurate delta time
        /// </summary>
        public long DeltaTimeTicks
        {
            get { return deltaTime; }
        }

        /// <summary>
        /// Accurate total time
        /// </summary>
        public long TotalTimeTicks
        {
            get { return totalTime; }
        }

        /// <summary>
        /// Total application time in seconds
        /// </summary>
        public float TotalTimeSeconds
        {
            get { return seconds; }
        }
    }

#if !DEBUG_API

    [System.Diagnostics.DebuggerStepThrough]
#endif
    sealed partial class AppState : IState, IUpdate
    {
        internal AppState(Application game)
        {
            this.application = game;

            this.playerInputCollection = new PlayerInputCollection(input);

            for (int i = 0; i < input.Length; i++)
                input[i] = new PlayerInput(i, this.playerInputCollection);

            input[0].ControlInput = ControlInput.GamePad1;
            input[1].ControlInput = ControlInput.GamePad2;
            input[2].ControlInput = ControlInput.GamePad3;
            input[3].ControlInput = ControlInput.GamePad4;

            this.renderState = new DrawState(this.application);
            this.frameState = new FrameState(this.application, this.renderState);

            this.updateState = new UpdateState(this.application, this.playerInputCollection);
        }

        private float dt;
        private float hz;
        private Application application;
        private long deltaTime, totalTime, slowDownBias;
        private float seconds;
        private IDictionary userValues = new Dictionary<object, object>();
        private readonly PlayerInput[] input = new PlayerInput[4];
        private readonly PlayerInputCollection playerInputCollection;

        private DrawState renderState;
        private FrameState frameState;
        private UpdateState updateState;

#if !XBOX360

        private KeyboardState keyboard;
        private MouseState mouse;
        private Point mousePrev;
        private bool mouseCen = false, mousePosSet = false, windowFocused = false, desireMouseCen;
        private int focusSkip = 0;
        private Point mouseCenTo;

#else

		private readonly KeyboardState[] chatPads = new KeyboardState[4];

#endif

        bool IState.IsAsynchronousState { get { return false; } }

        internal DrawState GetRenderState(Microsoft.Xna.Framework.Graphics.GraphicsDevice graphics)
        {
            this.renderState.Update(this.dt, this.hz, this.deltaTime, this.totalTime, this.seconds, graphics);
            return renderState;
        }

        internal FrameState GetFrameState()
        {
            this.frameState.Update(this.dt, this.hz, this.deltaTime, this.totalTime, this.seconds);
            return frameState;
        }

        internal UpdateState GetUpdateState()
        {
            this.updateState.Update(this.dt, this.hz, this.seconds, this.deltaTime, this.totalTime);

            return updateState;
        }

        public Application Application
        {
            get { return application; }
        }

        public float DeltaTimeFrequency
        {
            get { return hz; }
            internal set { hz = value; }
        }

        public float DeltaTimeSeconds
        {
            get { return dt; }
            internal set { dt = value; }
        }

        /// <summary>
        /// Always an array of length 4 (PlayerInput[4])
        /// </summary>
        public PlayerInput[] PlayerInput
        {
            get { return input; }
        }

#if !XBOX360

        public KeyboardState KeyboardState
        {
            get { return keyboard; }
        }

        internal Point MouseCentredPosition
        {
            get { return mouseCenTo; }
        }

        internal bool MouseCentred
        {
            get { return mouseCen && windowFocused; }
        }

        internal bool WindowFocused
        {
            get { return windowFocused; }
        }

        internal bool DesireMouseCentred
        {
            get { return desireMouseCen; }
            set { desireMouseCen = value; }
        }

        internal Point MousePreviousPosition
        {
            get { return mousePrev; }
        }

        /// <summary>
        /// [Windows Only]
        /// Using <see cref="PlayerInput"/> is recommended over direct state access
        /// </summary>
        public MouseState MouseState
        {
            get { return mouse; }
        }

#endif

        public long DeltaTimeTicks
        {
            get { return deltaTime; }
        }

        public long TotalTimeTicks
        {
            get { return totalTime; }
        }

        public float TotalTimeSeconds
        {
            get { return seconds; }
        }

        public const long TicksInOneSecond = 10000000L;

        internal void SetGameTime(long totalGameTicks)
        {
            this.totalTime += deltaTime;
            long delta = (totalGameTicks - slowDownBias) - totalTime;

            int minFps = application.MinimumDesiredFrameRate;
            if (minFps != 0)
            {
                if (delta > TicksInOneSecond / minFps)
                {
                    //getting very slow here, so keep the app running, just slow logic down
                    slowDownBias += delta - (TicksInOneSecond / minFps);
                    delta = TicksInOneSecond / minFps;
                }
            }

            if (application.VSyncEnabled)
            {
                //when vsync is on, bias the delta time towards it..
                //this will have no impact on game speed, but is intended to keep the delta time
                //as consistent intervals, reducing jitter that results from sampling actual time.

                //this should not have an impact on game timing on a longer term scale
                //	long refresh = application.GraphicsDevice.GraphicsDeviceStatus..RefreshRate;
                //	if (refresh == 0 || refresh == 59 || refresh == 61)
                //		refresh = 60;

                long refresh = 60;

                long ticks = TicksInOneSecond / refresh;

                if (delta < 0)
                {
                    //getting a little far ahead
                    //	System.Threading.Thread.Sleep(new TimeSpan(-delta));
                }

                if (delta < ticks)
                {
                    //somehow over 60fps...?
                    delta = ticks;
                }
                else
                {
                    long dif = ticks;
                    for (int i = 0; i < 5; i++)
                    {
                        if (i == 4)
                            break;
                        if (Math.Abs(delta - ticks) < dif)
                        {
                            delta = ticks;
                            break;
                        }
                        ticks *= 2;
                        dif *= 2;
                    }
                }
            }
            this.deltaTime = delta;

            dt = (float)(delta / (double)TicksInOneSecond);
            hz = delta > 0 ? (float)(TicksInOneSecond / (double)delta) : 0;

            if (Math.Abs(Math.Round(hz) - hz) < 0.001)
            {
                hz = (float)Math.Round(hz);
            }

            seconds = (float)(totalTime / (double)TicksInOneSecond);

            if (seconds == 0)
                UpdateInput(null);
        }

        UpdateFrequency IUpdate.Update(UpdateState state)
        {
            UpdateInput(state);

#if !XBOX360
            this.updateState.UpdateWindowsInput(ref this.keyboard, ref this.mouse);
#else
			this.updateState.UpdateXboxInput(chatPads);
#endif

            return UpdateFrequency.FullUpdate60hz;
        }

        private void UpdateInput(UpdateState state)
        {
            for (int i = 0; i < 4; i++)
            {
#if XBOX360
				chatPads[i] = Keyboard.GetState((PlayerIndex)i);
#endif
            }

#if !XBOX360

            mousePrev = new Point(mouse.X, mouse.Y);
            application.Logic.GetInputState(ref keyboard, ref mouse, ref windowFocused, ref mouseCen, ref mouseCenTo);

            if (!windowFocused)
                focusSkip = 5;
            if (!mousePosSet || focusSkip > 0)
            {
                focusSkip--;
                mousePrev = new Point(mouse.X, mouse.Y);
                mouseCenTo = mousePrev;
                mousePosSet = true;
            }

            XNALogic.SetMouseCentreState(desireMouseCen && windowFocused);
            desireMouseCen = false;

#endif
            UpdateState updateState = state ?? GetUpdateState();

            for (int i = 0; i < 4; i++)
            {
                input[i].mapper.SetKMS(this, input[i].ControlInput);
#if XBOX360
				bool windowFocused = true;
#endif
                input[i].mapper.UpdateState(ref input[i].istate, updateState, input[i].KeyboardMouseControlMapping, input[i].ControlInput, windowFocused);
            }
        }

        static internal bool MatrixNotEqual(ref Matrix m1, ref Matrix m2)
        {
            //as ripped out of xna via reflector
            if (((((m1.M11 == m2.M11) && (m1.M12 == m2.M12)) && ((m1.M13 == m2.M13) && (m1.M14 == m2.M14))) && (((m1.M21 == m2.M21) && (m1.M22 == m2.M22)) && ((m1.M23 == m2.M23) && (m1.M24 == m2.M24)))) && ((((m1.M31 == m2.M31) && (m1.M32 == m2.M32)) && ((m1.M33 == m2.M33) && (m1.M34 == m2.M34))) && (((m1.M41 == m2.M41) && (m1.M42 == m2.M42)) && (m1.M43 == m2.M43))))
            {
                return (m1.M44 != m2.M44);
            }
            return true;
        }

        static internal void ApproxMatrixScale(ref Matrix m, out float length)
        {
            length = m.M11 * m.M11 + m.M12 * m.M12 + m.M13 * m.M13;
            float y = m.M21 * m.M21 + m.M22 * m.M22 + m.M23 * m.M23;
            float z = m.M31 * m.M31 + m.M32 * m.M32 + m.M33 * m.M33;
            length = Math.Max(length, Math.Max(y, z));
            if (length < 0.99995f || length > 1.00005f)
                length = (float)Math.Sqrt(length);
        }

        static internal bool MatrixIsNotIdentiy(ref Matrix matrix)
        {
            return ((matrix.M11 != 1 || matrix.M22 != 1 || matrix.M33 != 1 || matrix.M44 != 1) ||
                (matrix.M12 != 0 || matrix.M13 != 0 || matrix.M14 != 0) ||
                (matrix.M21 != 0 || matrix.M23 != 0 || matrix.M24 != 0) ||
                (matrix.M31 != 0 || matrix.M32 != 0 || matrix.M34 != 0) ||
                (matrix.M41 != 0 || matrix.M42 != 0 || matrix.M43 != 0));
        }

        #region IState Members

        Application IState.Application
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        float IState.DeltaTimeFrequency
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        float IState.DeltaTimeSeconds
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        long IState.DeltaTimeTicks
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        long IState.TotalTimeTicks
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        float IState.TotalTimeSeconds
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        #endregion IState Members

        /// <summary></summary>
        public IDictionary UserValues
        {
            get { return userValues; }
        }
    }

    namespace Graphics
    {
#if DEBUG
        /// <summary>
        /// Draw statistics are only avaliable in debug builds
        /// </summary>
        public struct DrawStatistics
        {
            Stopwatch timer;

            internal void Begin()
            {
                if (timer == null)
                    timer = new Stopwatch();

                timer.Reset();
                timer.Start();
            }

            internal void End()
            {
                if (timer != null)
                {
                    timer.Stop();
                    ApproximateDrawTimeTicks = timer.ElapsedTicks;
                }
            }

            internal void Reset()
            {
                Stopwatch timer = this.timer;
                this = new DrawStatistics();
                this.timer = timer;
            }

            /// <summary></summary>
            /// <param name="a"></param>
            /// <param name="b"></param>
            /// <returns></returns>
            public static DrawStatistics operator +(DrawStatistics a, DrawStatistics b)
            {
                return Madd(ref a, ref b, 1);
            }

            /// <summary></summary>
            /// <param name="a"></param>
            /// <param name="b"></param>
            /// <returns></returns>
            public static DrawStatistics operator -(DrawStatistics a, DrawStatistics b)
            {
                return Madd(ref a, ref b, -1);
            }

            private static DrawStatistics Madd(ref DrawStatistics a, ref DrawStatistics b, int mul)
            {
                DrawStatistics s;

                s.ApproximateDrawTimeTicks = a.ApproximateDrawTimeTicks;

                //somtimes, regular expressions are just so very useful...
                s.DrawIndexedPrimitiveCallCount = a.DrawIndexedPrimitiveCallCount + b.DrawIndexedPrimitiveCallCount * mul;
                s.DrawPrimitivesCallCount = a.DrawPrimitivesCallCount + b.DrawPrimitivesCallCount * mul;
                s.TrianglesDrawn = a.TrianglesDrawn + b.TrianglesDrawn * mul;
                s.LinesDrawn = a.LinesDrawn + b.LinesDrawn * mul;
                s.PointsDrawn = a.PointsDrawn + b.PointsDrawn * mul;
                s.InstancesDrawn = a.InstancesDrawn + b.InstancesDrawn * mul;
                s.DrawIndexedPrimitiveCallCount = a.DrawIndexedPrimitiveCallCount + b.DrawIndexedPrimitiveCallCount * mul;
                s.DrawTargetsDrawCount = a.DrawTargetsDrawCount + b.DrawTargetsDrawCount * mul;
                s.DrawTargetsPassCount = a.DrawTargetsPassCount + b.DrawTargetsPassCount * mul;
                s.BinaryShadersCreated = a.BinaryShadersCreated + b.BinaryShadersCreated * mul;
                s.BufferClearTargetCount = a.BufferClearTargetCount + b.BufferClearTargetCount * mul;
                s.BufferClearDepthCount = a.BufferClearDepthCount + b.BufferClearDepthCount * mul;
                s.BufferClearStencilCount = a.BufferClearStencilCount + b.BufferClearStencilCount * mul;
                s.VertexBufferByesCopied = a.VertexBufferByesCopied + b.VertexBufferByesCopied * mul;
                s.IndexBufferByesCopied = a.IndexBufferByesCopied + b.IndexBufferByesCopied * mul;
                s.DynamicVertexBufferByesCopied = a.DynamicVertexBufferByesCopied + b.DynamicVertexBufferByesCopied * mul;
                s.DynamicIndexBufferByesCopied = a.DynamicIndexBufferByesCopied + b.DynamicIndexBufferByesCopied * mul;
#if XBOX360
				s.XboxPixelFillBias = a.XboxPixelFillBias + b.XboxPixelFillBias * mul;
#endif

                s.timer = null;
                return s;
            }

            /// <summary></summary>
            public long ApproximateDrawTimeTicks;
            /// <summary></summary>
            public int DrawIndexedPrimitiveCallCount;
            /// <summary></summary>
            public int DrawPrimitivesCallCount;
            /// <summary></summary>
            public int TrianglesDrawn;
            /// <summary></summary>
            public int LinesDrawn;
            /// <summary></summary>
            public int PointsDrawn;
            /// <summary></summary>
            public int InstancesDrawn;
            /// <summary></summary>
            public int DrawTargetsDrawCount;
            /// <summary></summary>
            public int DrawTargetsPassCount;
            /// <summary></summary>
            public int BinaryShadersCreated;
            /// <summary></summary>
            public int BufferClearTargetCount;
            /// <summary></summary>
            public int BufferClearDepthCount;
            /// <summary></summary>
            public int BufferClearStencilCount;

            /// <summary></summary>
            public int VertexBufferByesCopied;
            /// <summary></summary>
            public int IndexBufferByesCopied;
            /// <summary></summary>
            public int DynamicVertexBufferByesCopied;
            /// <summary></summary>
            public int DynamicIndexBufferByesCopied;

#if XBOX360
			/// <summary>Number of pixels that have been filled by XNA internal render target code</summary>
			public int XboxPixelFillBias;
#endif
        }
#endif
    }
}