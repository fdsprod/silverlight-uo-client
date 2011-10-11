using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Graphics;
using Client.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Xen.Threading;

namespace Xen
{
    /// <summary>
    /// Interface for an application wrapper (XNA Game application, or WinForms application)
    /// </summary>
    interface IXNAAppWrapper : IDisposable
    {
        GraphicsDevice GraphicsDevice { get; }

        bool VSyncEnabled { get; }

        GameServiceContainer Services { get; }

        ContentManager Content { get; }

        GameComponentCollection Components { get; }

        bool IsActive { get; }

        event EventHandler Exiting;

        void Run();

        //void Exit();

        //IntPtr WindowHandle { get; }

        int? GetMouseWheelValue();
    }

    /// <summary>
    /// This class wraps either an XNAGameAppWrapper (which wraps the XNA Game class) or XNAWinFormsAppWrapper, which wraps a windows form. Either class is presented as the same common class
    /// </summary>
#if !DEBUG_API

    [System.Diagnostics.DebuggerStepThrough]
#endif
    internal sealed class XNALogic : IDisposable
    {
        internal readonly AppState state;
        private readonly Application parent;
        private IXNAAppWrapper xnaGame;

        internal XNALogic(Application parent, object host, DrawingSurface surface)
        {
            this.state = new AppState(parent);
            this.parent = parent;

            parent.UpdateManager.Add(state);
            parent.UpdateManager.Add(parent);

            if (host is GameComponentHost)
            {
                this.xnaGame = new XNAGameComponentHostAppWrapper(this, parent, host as GameComponentHost);
            }
            else
            {
#if !XBOX360
                if (host == null)
                {
                    this.xnaGame = new XNAGameAppWrapper(this, parent, surface);
                }
                //else
                //{
                //    WinFormsHostControl _host = host as WinFormsHostControl;
                //    if (_host != null)
                //        this.xnaGame = new XNAWinFormsHostAppWrapper(this, parent, _host);
                //    else
                //        this.xnaGame = new XNAGameAppWrapper(this, parent, out gamerServicesComponent);
                //}
#else
				this.xnaGame = new XNAGameAppWrapper(this, parent, out gamerServicesComponent);
#endif
            }

            this.Exiting += delegate
            {
                Xen.Graphics.Resource.ClearResourceTracking();
            };
        }

        #region XNA Game members

        /// <summary></summary>
        public GameServiceContainer Services
        {
            get
            {
                return xnaGame.Services;
            }
        }

#if !XBOX360

        //public bool IsWindowsFormsHost
        //{
        //    get { return xnaGame is XNAWinFormsHostAppWrapper; }
        //}

#endif

        /// <summary></summary>
        public ContentManager Content
        {
            get
            {
                return xnaGame.Content;
            }
        }

        /// <summary></summary>
        public GameComponentCollection Components
        {
            get
            {
                return xnaGame.Components;
            }
        }

        /// <summary></summary>
        public bool IsActive
        {
            get
            {
                return xnaGame.IsActive;
            }
        }

        /// <summary></summary>
        public event EventHandler Exiting
        {
            add { xnaGame.Exiting += value; }
            remove { xnaGame.Exiting -= value; }
        }

        /// <summary></summary>
        public void Run()
        {
            //if you are getting an exception 'Cannot call Present while render target is active' here,
            //then this is because the last draw target to be drawn in the Application Frame method was not a DrawTargetScreen!
            if (xnaGame != null)
                xnaGame.Run();
        }

        /// <summary></summary>
        //public void Exit()
        //{
        //    if (xnaGame != null)
        //        xnaGame.Exit();
        //}

        /// <summary></summary>
        //public IntPtr WindowHandle
        //{
        //    get { return xnaGame.WindowHandle; }
        //}

        public GraphicsDevice GraphicsDevice
        {
            get
            {
                return xnaGame.GraphicsDevice;
            }
        }

        /// <summary></summary>
        public bool VSyncEnabled
        {
            get
            {
                return xnaGame.VSyncEnabled;
            }
        }

        /// <summary>
        /// Dispose the wrapper
        /// </summary>
        public void Dispose()
        {
            if (xnaGame != null)
            {
                xnaGame.Dispose();
                xnaGame = null;
            }
        }

        internal Game GetGame()
        {
            return xnaGame as Game;
        }

        #endregion XNA Game members

        public void Initialise()
        {
            parent.SetApplicationThreadInstance();

            parent.Initialise();
            parent.InitialisePlayerInput(this.state.GetUpdateState().PlayerInput);

            parent.SetReadyToLoadContent();
        }

        public void LoadContent()
        {
            parent.SetGraphicsDevice(GraphicsDevice);
        }

        public void Draw(bool resetState)
        {
            DrawState state = this.state.GetRenderState(GraphicsDevice);
            state.IncrementFrameIndex();

#if XBOX360
			state.nonScreenRenderComplete = false;
#endif

            //if (resetState)
            //    state.DirtyInternalRenderState(Xen.Graphics.StateFlag.All);

            parent.OnDraw(this.state.GetFrameState(), state);
        }

#if !XBOX360

        static MouseState mouse;
        static bool centreMouse, mouseCentred, centreMousePrevious, windowFocused = true, windowFocusedBuffer = true;
        static Point mouseCentrePoint;
        static object inputLock = new object();
        static KeyboardState keyboard;
#endif

        internal void UpdateInputState()
        {
#if !XBOX360
            lock (inputLock)
            {
                keyboard = Keyboard.GetState();

                mouse = Mouse.GetState();

                int? mouseWheel = 0;
                if (xnaGame != null)
                    mouseWheel = xnaGame.GetMouseWheelValue(); //GetState doesn't work for WinForms mouse wheel..
                if (mouseWheel != null)
                    mouse = new MouseState(mouse.X, mouse.Y, mouseWheel.Value, mouse.LeftButton, mouse.MiddleButton, mouse.RightButton, mouse.XButton1, mouse.XButton2);

                //Point p = new Point(windowForm.Width / 2, windowForm.Height / 2);

                //if (centreMouse && windowFocused)
                //    Mouse.SetPosition(p.X, p.Y);

                //if (centreMousePrevious)
                //{
                //    mouseCentred = true;
                //    mouseCentrePoint = p;
                //}

                //centreMousePrevious = centreMouse;
            }
#endif
        }

        public void Update(GameTime gameTime, long totalGameTicks)
        {
#if !XBOX360
            lock (inputLock)
            {
                //if (windowForm == null)
                //{
                //    windowForm = (System.Windows.Forms.Form)System.Windows.Forms.Form.FromHandle(WindowHandle);
                //    windowForm.Deactivate += delegate { windowFocusedBuffer = false; };
                //    windowForm.Activated += delegate { windowFocusedBuffer = true; };
                //}

                windowFocused = windowFocusedBuffer;
            }
#endif

            state.SetGameTime(totalGameTicks);

            parent.CallUpdate(state.GetUpdateState());
        }

#if !XBOX360

        internal void GetInputState(ref KeyboardState k, ref MouseState m, ref bool windowFocused, ref bool mouseCentred, ref Point centerPoint)
        {
            lock (inputLock)
            {
                UpdateInputState();

                m = mouse;
                k = keyboard;
                mouseCentred = XNALogic.mouseCentred;
                centerPoint = XNALogic.mouseCentrePoint;
                windowFocused = XNALogic.windowFocused;
            }
        }

#endif

        internal static void SetMouseCentreState(bool centreMouse)
        {
#if !XBOX360
            lock (inputLock)
            {
                XNALogic.centreMouse = centreMouse;
            }
#endif
        }
    }

#if !DEBUG_API

    [System.Diagnostics.DebuggerStepThrough]
#endif
    internal sealed class XNAGameAppWrapper : Game, IXNAAppWrapper
    {
        //private readonly GraphicsDeviceManager graphics;
        private readonly RenderTargetUsage presentation;
        private readonly Application parent;
        private readonly XNALogic logic;
        public new event EventHandler Exiting;
        private static bool gamerServicesRegistered = false;

        internal XNAGameAppWrapper(XNALogic logic, Application parent, DrawingSurface surface)
            : base(surface)
        {
            this.parent = parent;
            this.logic = logic;
            //GraphicsDeviceManager.Current.RenderModeChanged += new EventHandler<RenderModeChangedEventArgs>(Current_RenderModeChanged);
            //graphics = new GraphicsDeviceManager(this);
            //graphics.PreparingDeviceSettings += new EventHandler<PreparingDeviceSettingsEventArgs>(PreparingDeviceSettings);
            //graphics.PreferredDepthStencilFormat = DepthFormat.Depth24Stencil8;
            //graphics.SynchronizeWithVerticalRetrace = true;
            //graphics.GraphicsProfile = GraphicsProfile.HiDef;

            //gamerServicesComponent = null;
            if (parent.ApplicationRequiresGamerServices)
            {
                if (gamerServicesRegistered)
                    throw new InvalidOperationException("This sample requires the GamerServices component.\nUnfortunately, XNA does not let you create this component more than once in a session.\nPlease restart the application");

                gamerServicesRegistered = true;
                //gamerServicesComponent = new Microsoft.Xna.Framework.GamerServices.GamerServicesComponent((Game)this);
                //this.Components.Add(gamerServicesComponent);
            }
            //#if DEBUG
            //#if !XBOX360
            //            Window.Title += " [XEN DEBUG API - XNA 4 FX]";
            //#endif
            //#endif
            presentation = RenderTargetUsage.DiscardContents;

            //parent.SetWindowSizeAndFormat(graphics.PreferredBackBufferWidth,graphics.PreferredBackBufferHeight,graphics.PreferredBackBufferFormat, graphics.PreferredDepthStencilFormat);

            parent.SetupGraphicsDeviceManager(GraphicsDeviceManager.Current, ref presentation);

            //values may have changed in SetupGraphicsDeviceManager
            //parent.SetWindowSizeAndFormat(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight, graphics.PreferredBackBufferFormat, graphics.PreferredDepthStencilFormat);
        }

        private void OnExitingCallback(object sender, EventArgs e)
        {
            if (Exiting != null)
                Exiting(sender, e);
        }

        //        void PreparingDeviceSettings(object sender, PreparingDeviceSettingsEventArgs e)
        //        {
        //#if DEBUG
        //            foreach (GraphicsAdapter curAdapter in GraphicsAdapter.Adapters)
        //            {
        //                if (curAdapter.Description.Contains("PerfHUD"))
        //                {
        //                    e.GraphicsDeviceInformation.Adapter = curAdapter;
        //                    break;
        //                }
        //            }
        //#endif
        //            e.GraphicsDeviceInformation.PresentationParameters.RenderTargetUsage = presentation;
        //        }

        public new GraphicsDevice GraphicsDevice
        {
            get
            {
                return GraphicsDeviceManager.Current.GraphicsDevice;
            }
        }

        //public IntPtr WindowHandle
        //{
        //    get { return Window.Handle; }
        //}

        public bool VSyncEnabled
        {
            get
            {
                return true;
            }
        }

        protected override void Initialize()
        {
            logic.GetGame().Exiting += new EventHandler<EventArgs>(OnExitingCallback);

            parent.SetGraphicsDevice(GraphicsDevice);

            base.Initialize();

            logic.Initialise();
        }

        protected override void LoadContent()
        {
            logic.LoadContent();
            base.LoadContent();
        }

        protected override void Draw(GameTime gameTime)
        {
            this.parent.gameTime = gameTime;
            base.Draw(gameTime);
            this.logic.Draw(false);
        }

        protected override void Update(GameTime gameTime)
        {
            this.parent.gameTime = gameTime;
            this.IsFixedTimeStep = false;
            logic.Update(gameTime, gameTime.TotalGameTime.Ticks);
        }

        int? IXNAAppWrapper.GetMouseWheelValue()
        {
            return null;
        }
    }

    /// <summary>
    /// <para>Enumeration representing the frequency an <see cref="IUpdate"/> object wishes to have it's Update() method called (With a special <see cref="Terminate"/> case to have the object removed from the update list).</para>
    /// <para>NOTE: User input is updated at 60hz, any class responding to user input should use <see cref="UpdateFrequency.FullUpdate60hz"/></para>
    /// </summary>
    public enum UpdateFrequency : byte
    {
        /// <summary>
        /// Call update once per frame
        /// </summary>
        [UpdateFrequency(Interval = 1)]
        OncePerFrame = 0,
        /// <summary>
        /// <para>Call update once per frame asynchronously with other <see cref="OncePerFrameAsync"/> instances</para>
        /// <para>Note: Update implementations need to be thread safe when using asynchronous updates. Async adds overhead which can make simple Update methods slower!</para>
        /// </summary>
        /// <remarks>
        /// <para>Asynchronous updates have an overhead for setup of the asynchronous update process.</para>
        /// <para>If there are fewer items to be updated than threads, then the update processes will not be run asynchronously.</para>
        /// </remarks>
        [UpdateFrequency(Interval = 1, Async = true)]
        OncePerFrameAsync = 1,
        /// <summary>
        /// <para>Call update 60 times per second</para>
        /// <para>NOTE: User input is updated at 60hz, any class responding to user input should use <see cref="FullUpdate60hz"/></para>
        /// </summary>
        [UpdateFrequency(Interval = 1)]
        FullUpdate60hz = 2,
        /// <summary>
        /// <para>Call update 60 times per second asynchronously with other <see cref="FullUpdate60hzAsync"/> instances</para>
        /// <para>NOTE: User input is updated at 60hz, any class responding to user input should use <see cref="FullUpdate60hz"/></para>
        /// <para>Note: Update implementations need to be thread safe when using asynchronous updates. Async adds overhead which can make simple Update methods slower!</para>
        /// </summary>
        /// <remarks>
        /// <para>Asynchronous updates have an overhead for setup of the asynchronous update process.</para>
        /// <para>If there are fewer items to be updated than threads, then the update processes will not be run asynchronously.</para>
        /// </remarks>
        [UpdateFrequency(Interval = 1, Async = true)]
        FullUpdate60hzAsync = 3,
        /// <summary>
        /// Call update at least 30 times per second
        /// </summary>
        [UpdateFrequency(Interval = 2)]
        HalfUpdate30hz = 4,
        /// <summary>
        /// <para>Call update at least 30 times per second asynchronously with other <see cref="HalfUpdate30hzAsync"/> instances</para>
        /// <para>Note: Update implementations need to be thread safe when using asynchronous updates. Async adds overhead which can make simple Update methods slower!</para>
        /// </summary>
        /// <remarks>
        /// <para>Asynchronous updates have an overhead for setup of the asynchronous update process.</para>
        /// <para>If there are fewer items to be updated than threads, then the update processes will not be run asynchronously.</para>
        /// </remarks>
        [UpdateFrequency(Interval = 2, Async = true)]
        HalfUpdate30hzAsync = 5,
        /// <summary>
        /// Call update at least 15 times per second
        /// </summary>
        [UpdateFrequency(Interval = 4)]
        PartialUpdate15hz = 6,
        /// <summary>
        /// Call update at least 10 times per second
        /// </summary>
        [UpdateFrequency(Interval = 6)]
        PartialUpdate10hz = 7,
        /// <summary>
        /// Call update at least 5 times per second
        /// </summary>
        [UpdateFrequency(Interval = 12)]
        PartialUpdate5hz = 8,
        /// <summary>
        /// Call update at least once every second
        /// </summary>
        [UpdateFrequency(Interval = 60)]
        PartialUpdate1hz = 9,
        /// <summary>
        /// This object no longer needs to be updated, and should be removed from the update list.
        /// </summary>
        /// <remarks>This is the preferred method for removing an object from an <see cref="UpdateManager"/></remarks>
        Terminate = 255
    }

    ///// <summary>
    ///// Enumeration representing the update status of an <see cref="IUpdate"/> or <see cref="IAsyncUpdate"/> object. Returned from the Update() method. Specify <see cref="Terminate"/> to have the object efficiently removed from the update list.
    ///// </summary>
    //public enum UpdateResult : byte
    //{
    //    /// <summary>
    //    /// Future updating should continue as normal
    //    /// </summary>
    //    Continue,
    //    /// <summary>
    //    /// This object no longer needs to be updated, and should be removed from the update list.
    //    /// </summary>
    //    /// <remarks>This is the preferred method for removing an object from an <see cref="UpdateManager"/></remarks>
    //    Terminate
    //}

    /// <summary>
    /// Interface for an object that wishes to be updated at a consistent interval (specified by <see cref="UpdateFrequency"/>)
    /// </summary>
    public interface IUpdate
    {
        ///// <summary>
        ///// Get the frequency the object wishes to have <see cref="Update"/> called.
        ///// </summary>
        ///// <remarks>This value may be changed dynamically. It is checked after each call to <see cref="Update"/>, changes should be applied by the following update loop.</remarks>
        //UpdateFrequency UpdateFrequency { get; }
        /// <summary>
        /// Method for updating the object and performing logic. Called at a desired frequency
        /// </summary>
        /// <param name="state">Current state of the application</param>
        /// <returns>Returns a <see cref="UpdateFrequency"/>, indicating the frequency the object desires to be updated in the following update loop (Or <see cref="UpdateFrequency.Terminate"/> if the object wishes to be removed from the update list)</returns>
        UpdateFrequency Update(UpdateState state);
    }

    /// <summary>
    /// Interface for an object that can be culled by an <see cref="ICuller"/>. An example use may be off-screen culling
    /// </summary>
    public interface ICullable
    {
        /// <summary>
        /// Perform a cull test on the current object. Returns false if the CullTest fails (for example, the object is offscreen)
        /// </summary>
        /// <param name="culler"></param>
        /// <returns>Returns false if the CullTest fails (for example, the object is offscreen)</returns>
        bool CullTest(ICuller culler);
    }
    /// <summary>
    /// Interface for an object that can cull world matrix defined instances using an <see cref="ICuller"/>. An example use may be off-screen culling
    /// </summary>
    public interface ICullableInstance
    {
        /// <summary>
        /// Perform a cull test on an instance of an object. Returns false if the CullTest fails (for example, the object is offscreen)
        /// </summary>
        /// <param name="culler"></param>
        /// <returns>Returns false if the CullTest fails (for example, the object is offscreen)</returns>
        /// <param name="instance">Local matrix of the instance to CullTest</param>
        bool CullTest(ICuller culler, ref Matrix instance);
    }
    /// <summary>
    /// Interface to an object that can be drawn. All drawable objects also are <see cref="ICullable"/>
    /// </summary>
    /// <remarks>
    /// <para>Some objects, such as <see cref="Graphics.DrawTarget">draw targets</see> need to have <see cref="Graphics.DrawTarget.Draw">Draw</see> called explicitly, while some objects may have <see cref="Draw"/> called from another object.</para>
    /// <para>A call to <see cref="Draw"/> should not assume the object knows it's full render state. Simple geometry classes, for example, may only draw themselves. In such a case, the 'owner' IDraw object should make sure the correct render states, shaders, etc are setup first.</para>
    /// </remarks>
    public interface IDraw : ICullable
    {
        /// <summary>
        /// Draw the object
        /// </summary>
        /// <param name="state">Current state of the application</param>
        void Draw(DrawState state);
    }
    /// <summary>
    /// Interface to an object that can be drawn at the frame level.
    /// </summary>
    public interface IFrameDraw
    {
        /// <summary>
        /// Draw the object
        /// </summary>
        /// <param name="state">Current state of the application</param>
        void Draw(FrameState state);
    }
    /// <summary>
    /// <para>Interface to a sepecial object that can drawn itself as a batch. All batch drawable objects also are <see cref="IDraw"/> instances</para>
    /// <para>Most DrawBatch objects will use hardware instancing when supported</para>
    /// </summary>
    /// <remarks>
    /// <para>This interface is usually used by low level geometry classes</para>
    /// <para>See the <see cref="Graphics.StreamFrequency"/> class for details on using hardware instancing to draw large numbers of complex objects efficiently.</para>
    /// </remarks>
    public interface IDrawBatch : IDraw
    {
        /// <summary>
        /// <para>Draw multiple instances of the object</para>
        /// </summary>
        /// <param name="state">Current state of the application</param>
        /// <param name="instances">Buffer containing the instances to draw (either created directly, or allocated dynamically with DrawState.GetDynamicInstanceBuffer)</param>
        void DrawBatch(DrawState state, Xen.Graphics.InstanceBuffer instances);

        /// <summary>
        /// <para>Draw multiple instances of the object</para>
        /// </summary>
        void DrawBatch(DrawState state, Matrix[] instances, int instanceCount);
    }
    /// <summary>
    /// Interface to an object that modifies draw state and must be setup/shutdown. Usually used by internal classes only
    /// </summary>
    public interface IBeginEndDraw
    {
        /// <summary>
        /// Enable/Disable the begin/end process
        /// </summary>
        bool Enabled { get; set; }

        /// <summary>
        /// Drawing is about to begin
        /// </summary>
        /// <param name="state">Current state of the application</param>
        void Begin(DrawState state);

        /// <summary>
        /// Drawing has completed
        /// </summary>
        /// <param name="state">Current state of the application</param>
        void End(DrawState state);
    }

    /// <summary>
    /// Base class for implementing a xen application. Implement this class in a simliar manner to the XNA <see cref="Game"/> class
    /// </summary>
    /// <remarks>This class can be explicitaly cast to an XNA Game instance:
    /// <example>
    /// <code>
    /// Xen.Application app = ...;
    ///
    /// //...
    /// Microsoft.Xna.Framework.Game xnaGame = (Microsoft.Xna.Framework.Game)app;
    /// </code>
    /// </example></remarks>
#if !DEBUG_API

    [System.Diagnostics.DebuggerStepThrough]
#endif
    public abstract class Application : IDisposable, IContentUnload, IUpdate
    {
        private readonly LinkedList<double> approxFps;
        internal float approximateFrameRate;
        private GraphicsDevice graphics;
        private SurfaceFormat screenFormat;
        private DepthFormat depthFormat;
        private Xen.Graphics.PreferredMultiSampleLevel screenMultisample;
        private int graphicsIndex;
        internal int graphicsId;
        private UpdateManager updateManager;
        internal readonly Xen.Graphics.VertexDeclarationBuilder declarationBuilder;
        private readonly ThreadPool threadPool;
        private bool supportsInstancing;
        private bool supportsInstancingSet;
        private bool initalised;
        private XNALogic xnaLogic;
        private ContentRegister content;
        private int windowWidth, windowHeight;
        private bool readToLoadContent;
        private int minimumFrameRate;
        //private Microsoft.Xna.Framework.GamerServices.GamerServicesComponent gamerServicesComponent;
        internal GameTime gameTime;
        internal DrawingSurface _drawingSurface;
        private int drawableComponents;

        private readonly List<IFrameDraw> preFrameDrawList;
        private readonly List<IFrameDraw> preFrameDrawListBuffer;

#if !XBOX360
        [ThreadStatic]
#endif
        private static WeakReference AppInstance;

#if !XBOX360
        //private bool winFormsDispose;
#endif

        /// <summary>
        /// XNA Service that returns the <see cref="Application"/>. Useful for warming resources when loading through the content pipeline.
        /// </summary>
        public sealed class ApplicationProviderService
        {
            Application app;

            internal ApplicationProviderService(Application application)
            {
                this.app = application;
            }

            /// <summary>
            /// Gets the <see cref="Application"/>
            /// </summary>
            public Application Application
            {
                get { return app; }
            }
        }

        /// <summary>
        /// <para>Gets/Sets the minimum desired frame rate. If the frame rate dips below this value, the application update loop will be slowed down to compensate</para>
        /// <para>Note: The reported frame rate from the application will not drop below this value</para>
        /// </summary>
        public int MinimumDesiredFrameRate
        {
            get { return minimumFrameRate; }
            set { if (value < 0 || value > 60) throw new ArgumentException(); minimumFrameRate = value; }
        }

        //called from Update or Draw, requires something to be drawn in the next frame
        /// <summary>
        /// <para>The the item passed in will be drawn at the start of the next frame, before the main application Draw method is called</para>
        /// </summary>
        internal void PreFrameDraw(IFrameDraw draw)
        {
            lock (this.preFrameDrawList)
            {
                this.preFrameDrawList.Add(draw);
            }
        }

#if DEBUG
        //rendering stats for the current and previous frame
        internal Xen.Graphics.DrawStatistics currentFrame, previousFrame;
#endif

        //used by the content state, primarily.
        //internal Xen.Graphics.ShaderSystemState ShaderSystem
        //{
        //    get
        //    {
        //        if (xnaLogic != null && xnaLogic.state != null)
        //        {
        //            var state = xnaLogic.state.GetRenderState(GraphicsDevice);
        //            if (state != null) return state.shaderSystem;
        //        }
        //        return null;
        //    }
        //}

        /// <summary>
        /// Override this property and return true if your Application requires gamer services
        /// </summary>
        protected internal virtual bool ApplicationRequiresGamerServices { get { return false; } }

        internal bool VSyncEnabled
        {
            get { return xnaLogic.VSyncEnabled; }
        }

        internal GraphicsDevice GraphicsDevice
        {
            get
            {
                if (graphics == null)
                    throw new InvalidOperationException("GraphicsDevice has not been initalised yet");
                return graphics;
            }
        }

        /// <summary>
        /// Gets the application thread pool instance
        /// </summary>
        public ThreadPool ThreadPool
        {
            get { return this.threadPool; }
        }

        internal bool IsInitailised
        {
            get { return initalised; }
        }

        internal void SetReadyToLoadContent()
        {
            readToLoadContent = true;
            this.LoadContent(this.content.GetState());
        }

        internal bool SupportsHardwareInstancing
        {
            get { if (!supportsInstancingSet) throw new ArgumentException("Graphics device not created yet"); return supportsInstancing; }
            set { supportsInstancing = value; supportsInstancingSet = true; }
        }

        /// <summary>
        /// A dictionary for placing global values, if desired
        /// </summary>
        public System.Collections.IDictionary UserValues
        {
            get { return xnaLogic.state.UserValues; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <remarks>This method sets up the input manager</remarks>
        public Application(DrawingSurface drawingSurface)
        {
            SetApplicationThreadInstance();

            //initalise default values
            _drawingSurface = drawingSurface;
            this.approxFps = new LinkedList<double>();
            this.declarationBuilder = new Xen.Graphics.VertexDeclarationBuilder();
            this.threadPool = new ThreadPool(this);
            this.minimumFrameRate = 5;
            this.preFrameDrawList = new List<IFrameDraw>();
            this.preFrameDrawListBuffer = new List<IFrameDraw>();

            this.updateManager = new UpdateManager();
        }

        internal void SetApplicationThreadInstance()
        {
            if (AppInstance == null)
                AppInstance = new WeakReference(null);
            AppInstance.Target = this;
        }

        /// <summary>
        /// Gets the running Application instance for this thread
        /// </summary>
        /// <returns></returns>
        public static Application GetApplicationInstance()
        {
            Application instance = null;
            if (AppInstance != null)
                instance = AppInstance.Target as Application;
            if (instance == null)
                throw new ArgumentException("Current thread does not have access to the Xen Application instance, or Initialise hasn't been called yet");
            return instance;
        }

        /// <summary>
        /// Get an instance to an application wide Content manager
        /// </summary>
        public ContentRegister Content
        {
            get
            {
                return content;
            }
        }

        /// <summary>
        /// Access the XNA base game <see cref="GameComponentCollection"/>. Use with caution, Using Game Components is not tested/supported (Not supported when using WinForms)
        /// </summary>
        public GameComponentCollection XnaComponents
        {
            get { return xnaLogic.Components; }
        }

        /// <summary>
        /// Access the XNA base game <see cref="GameServiceContainer"/> Services
        /// </summary>
        public GameServiceContainer Services
        {
            get { return xnaLogic.Services; }
        }

        /// <summary>
        ///  Indicates if this application is currently the active application.
        /// </summary>
        public bool IsActive
        {
            get { return xnaLogic.IsActive; }
        }

        /// <summary>
        /// Get the XNA Application instance
        /// </summary>
        /// <param name="application"></param>
        /// <returns></returns>
        public static implicit operator Game(Application application)
        {
            return application.xnaLogic.GetGame();
        }

        internal XNALogic Logic
        {
            get { return xnaLogic; }
        }

        /// <summary>
        /// Get the XNA GamerServices object for this application
        /// </summary>
        //public Microsoft.Xna.Framework.GamerServices.GamerServicesComponent GamerServices
        //{
        //    get { return gamerServicesComponent; }
        //}

        /// <summary>
        /// Start the main application loop. This method should be called from the entry point of the application
        /// </summary>
        public void Run()
        {
            if (xnaLogic != null)
                throw new InvalidOperationException("Application is already initalised");

            xnaLogic = new XNALogic(this, null, _drawingSurface);

            this.XnaComponents.ComponentAdded += delegate(object sender, GameComponentCollectionEventArgs e) { if (e.GameComponent is IDrawable) drawableComponents++; };
            this.XnaComponents.ComponentRemoved += delegate(object sender, GameComponentCollectionEventArgs e) { if (e.GameComponent is IDrawable) drawableComponents--; };

            xnaLogic.Services.AddService(typeof(ApplicationProviderService), new ApplicationProviderService(this));
            xnaLogic.Exiting += new EventHandler(ShutdownThreadPool);
            xnaLogic.Content.RootDirectory += "Content";

            content = new ContentRegister(this, xnaLogic.Content);
            content.Add(this);

            xnaLogic.Run();
        }

        internal void Run(GameComponentHost host)
        {
            if (host == null)
                throw new ArgumentNullException();
            if (xnaLogic != null)
                throw new InvalidOperationException("Application is already initalised");

            xnaLogic = new XNALogic(this, host, _drawingSurface);
            xnaLogic.Services.AddService(typeof(ApplicationProviderService), new ApplicationProviderService(this));
            xnaLogic.Exiting += new EventHandler(ShutdownThreadPool);
            xnaLogic.Content.RootDirectory += "Content";

            content = new ContentRegister(this, xnaLogic.Content);
            content.Add(this);

            xnaLogic.Run();
        }

#if !XBOX360

        /*
		/// <summary>
		/// <para>Start the main application loop, running the application within a <see cref="WinFormsHostControl"/>. This method should be called from the entry point of the application</para>
		/// <para>Note: The application must be running within WinForms application started with <see cref="System.Windows.Forms.Application.Run(System.Windows.Forms.Form)"/></para>
		/// </summary>
		public void Run(WinFormsHostControl host)
		{
			if (host == null)
				throw new ArgumentNullException();
			if (xnaLogic != null)
				throw new InvalidOperationException("Application is already initalised");

			xnaLogic = new XNALogic(this, host, out this.gamerServicesComponent);
			xnaLogic.Services.AddService(typeof(ApplicationProviderService), new ApplicationProviderService(this));
			xnaLogic.Exiting += new EventHandler(ShutdownThreadPool);
			xnaLogic.Content.RootDirectory += "Content";

			content = new ContentRegister(this, xnaLogic.Content);
			content.Add(this);

			xnaLogic.Run();
		}
*/

#endif

        private void ShutdownThreadPool(object sender, EventArgs e)
        {
            threadPool.Dispose();
        }

        /// <summary>
        /// Immediately shuts down the application (WinForms hosted applications will shutdown at the end of the frame)
        /// </summary>
        public virtual void Shutdown()
        {
            //xnaLogic.Exit();

            xnaLogic.Dispose();
        }

        internal void OnDraw(FrameState frame, DrawState state)
        {
#if DEBUG
            previousFrame = currentFrame;
            currentFrame.Reset();
            currentFrame.Begin();
#endif

            ushort stackHeight, stateHeight, cameraHeight, preCull, postCull;
            state.GetStackHeight(out stackHeight, out stateHeight, out cameraHeight, out preCull, out postCull);

            if (approxFps.Count == 20)
            {
                LinkedListNode<double> first = approxFps.First;
                approxFps.Remove(first);
                first.Value = state.DeltaTimeFrequency;
                approxFps.AddLast(first);
            }
            else
                approxFps.AddLast(state.DeltaTimeFrequency);

            double total = 0;
            foreach (double d in approxFps)
                total += d;
            if (approxFps.Count > 0)
                total /= approxFps.Count;

            approximateFrameRate = (float)total;

            if (drawableComponents > 0)
                state.renderState.InternalDirtyRenderState(Xen.Graphics.StateFlag.All);

            state.PrepareForNewFrame();

            //the predraw list is a list of items that need to be drawn before the frame starts
            //eg, an object in Update may require something is drawn this frame.
            //call PreFrameDraw in either Update() or Draw() to add to this list
            while (true)
            {
                lock (preFrameDrawList)
                {
                    if (preFrameDrawList.Count == 0)
                        break;

                    foreach (IFrameDraw item in preFrameDrawList)
                        preFrameDrawListBuffer.Add(item);
                    preFrameDrawList.Clear();
                }

                foreach (IFrameDraw item in preFrameDrawListBuffer)
                {
                    item.Draw(frame);
                }

                preFrameDrawListBuffer.Clear();
            }

            Frame(frame);

            state.ValidateStackHeight(stackHeight, stateHeight, cameraHeight, preCull, postCull);

#if DEBUG
            currentFrame.End();
#endif

#if !XBOX360
            //if (winFormsDispose)
            //{
            //	xnaLogic.Exit();
            //
            //	xnaLogic.Dispose();
            //}
#endif
        }

        /// <summary>
        /// Implement all application specific initalisation code in here
        /// </summary>
        protected internal abstract void Initialise();

        internal void CallUpdate(UpdateState state)
        {
            ((IUpdate)updateManager).Update(state);
        }

        /// <summary>
        /// Called at the start of the update loop, 60 times per frame
        /// </summary>
        /// <param name="state"></param>
        protected internal abstract void Update(UpdateState state);

        /// <summary>
        /// Run all drawing code for the frame
        /// </summary>
        /// <param name="state"></param>
        protected internal abstract void Frame(FrameState state);

        /// <summary>
        /// Override this method to modify the setup of the player input objects before the application starts
        /// </summary>
        /// <param name="playerInput"></param>
        protected internal virtual void InitialisePlayerInput(Xen.Input.PlayerInputCollection playerInput) { }

        /// <summary>
        /// Override this method to change the graphics device options before the application starts (eg, set resolution, fullscreen, etc)
        /// </summary>
        /// <param name="graphics">XNA GraphicsDeviceManager (NOTE: This value will be null when displaying with Windows Forms)</param>
        /// <param name="presentation">Allows modification of the <see cref="PresentationParameters"/> <see cref="RenderTargetUsage"/> for the primary display</param>
        protected internal virtual void SetupGraphicsDeviceManager(GraphicsDeviceManager graphics, ref RenderTargetUsage presentation)
        {
        }

        internal void SetWindowSizeAndFormat(int width, int height, SurfaceFormat format, DepthFormat depthFormat)
        {
            this.windowWidth = width;
            this.windowHeight = height;

            this.screenFormat = format;
            this.depthFormat = depthFormat;
            screenMultisample = Xen.Graphics.PreferredMultiSampleLevel.None;
        }

        #region IDisposable Members

        void IDisposable.Dispose()
        {
            xnaLogic.Dispose();
        }

        #endregion IDisposable Members

        /// <summary>
        /// Gets the Window the application is being displayed on (This value is null when using Windows Forms hosting)
        /// </summary>
        //public GameWindow Window
        //{
        //    get { return xnaLogic.Window; }
        //}

        /// <summary>
        /// Gets the graphics device Title Safe Area
        /// </summary>
        //public Rectangle TitleSafeArea
        //{
        //    get { return GraphicsDevice.DisplayMode.TitleSafeArea; }
        //}

        /// <summary>
        /// Gets the width (in pixels) of the render window
        /// </summary>
        public int WindowWidth
        {
            get
            {
                return windowWidth;
            }
        }

        /// <summary>
        /// Gets the height (in pixels) of the render window
        /// </summary>
        public int WindowHeight
        {
            get
            {
                return windowHeight;
            }
        }

        /// <summary>
        /// Gets the application <see cref="UpdateManager"/> instance. Add objects implementing <see cref="IUpdate"/> to this manager to perform update logic
        /// </summary>
        public UpdateManager UpdateManager
        {
            get { return updateManager; }
        }

        internal void SetGraphicsDevice(GraphicsDevice graphics)
        {
            //calculate a system wide unique id for the graphics device
            this.graphicsId = 999;
            //this.graphicsId = System.Diagnostics.Process.GetCurrentProcess().Id;

            this.screenFormat = graphics.PresentationParameters.BackBufferFormat;
            this.depthFormat = graphics.PresentationParameters.DepthStencilFormat;
            screenMultisample = (Xen.Graphics.PreferredMultiSampleLevel)graphics.PresentationParameters.MultiSampleCount;

            if (graphics != this.graphics)
                graphicsIndex++;

            this.graphicsId ^= graphicsIndex << 16;
            this.graphicsId ^= (int)(DateTime.Now.Ticks & 0xFFFF);

            this.initalised = true;
            this.graphics = graphics;

            this.windowWidth = graphics.PresentationParameters.BackBufferWidth;
            this.windowHeight = graphics.PresentationParameters.BackBufferHeight;

            //TODO: Replace
            //xnaLogic.state.GetRenderState(graphics).shaderSystem.DeviceUniqueIndex = this.graphicsId;
        }

        #region IContentOwner Members

        /// <summary>
        /// Override this method to perform custom content loading through an XNA ContentManager
        /// </summary>
        /// <param name="state"></param>
        protected virtual void LoadContent(ContentState state)
        {
        }

        /// <summary>
        /// Override this method to perform custom content unloading
        /// </summary>
        protected virtual void UnloadContent()
        {
        }

        void IContentOwner.LoadContent(ContentState state)
        {
            xnaLogic.state.GetRenderState(state).renderState.InternalDirtyRenderState(Xen.Graphics.StateFlag.All);
            if (readToLoadContent)
                this.LoadContent(state);
        }

        void IContentUnload.UnloadContent()
        {
            this.UnloadContent();
            xnaLogic.state.GetRenderState(this.graphics).renderState.InternalDirtyRenderState(Xen.Graphics.StateFlag.All);
        }

        #endregion IContentOwner Members

        internal SurfaceFormat GetScreenFormat()
        {
            return screenFormat;
        }

        internal DepthFormat GetScreenDepthFormat()
        {
            return depthFormat;
        }

        internal Xen.Graphics.PreferredMultiSampleLevel GetScreenMultisample()
        {
            return screenMultisample;
        }

        #region IUpdate Members

        UpdateFrequency IUpdate.Update(UpdateState state)
        {
            this.Update(state);
            return UpdateFrequency.FullUpdate60hz;
        }

        #endregion IUpdate Members

        /// <summary>
        /// Returns true if this application is running an XNA 4 HiDef device
        /// </summary>
        //public bool IsHiDefDevice
        //{
        //    get
        //    {
        //        return graphics.GraphicsProfile == GraphicsProfile.HiDef;
        //    }
        //}

        /// <summary>
        /// True if this is a Debug build of xen
        /// </summary>
        public static bool IsDebugBuild
        {
            get
            {
#if DEBUG
                return true;
#else
				return false;
#endif
            }
        }
    }
}