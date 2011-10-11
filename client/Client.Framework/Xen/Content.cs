using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Xen.Graphics;

namespace Xen
{
    /// <summary>
    /// Interface to an object that wishes to load content through an XNA <see cref="ContentManager"/>, or handle implementation specific content load/unload logic, for use with the <see cref="ContentRegister"/> class (such as the <see cref="Application.Content"/> instance)
    /// </summary>
    /// <remarks>
    /// <para>This interface is intended as a lightweight replacement for the content loading methods found in <see cref="DrawableGameComponent"/></para>
    /// <para>All XNA content should be loaded within these methods, with the exception of Textures returned by <see cref="DrawTargetTexture2D.GetTexture()"/> and <see cref="DrawTargetTextureCube.GetTexture()"/>.
    /// <br/>These textures become invalid after a call to <see cref="IContentUnload.UnloadContent"/>, they are guarenteed to be valid again during the subsequent <see cref="LoadContent"/> call.</para>
    /// <para>When registered with a <see cref="ContentRegister"/> object, a weak reference of the instance will be stored. This will prevent the object from being kept alive in an unexpected way</para>
    /// </remarks>
    public interface IContentOwner
    {
        /// <summary>
        /// Load all XNA <see cref="ContentManager"/> content, or get textures from <see cref="DrawTargetTexture2D"/> or <see cref="DrawTargetTextureCube"/> objects.
        /// </summary>
        void LoadContent(ContentState state);
    }

    /// <summary>
    /// Interface to an object that wishes to load and unload content through an XNA <see cref="ContentManager"/>, or handle implementation specific content load/unload logic, for use with the <see cref="ContentRegister"/> class (such as the <see cref="Application.Content"/> instance)
    /// </summary>
    public interface IContentUnload : IContentOwner
    {
        /// <summary>
        /// Unload all XNA <see cref="ContentManager"/> content, or null textures from <see cref="DrawTargetTexture2D"/> or <see cref="DrawTargetTextureCube"/> objects.
        /// </summary>
        void UnloadContent();
    }

    /// <summary>
    /// Interface to a <see cref="ContentRegister"/>
    /// </summary>
    public interface IContentRegister : IDisposable
    {
        /// <summary>
        /// Register an <see cref="IContentOwner"/> instance with this content manager
        /// </summary>
        /// <param name="owner"></param>
        void Add(IContentOwner owner);

        /// <summary>
        /// Unregister an <see cref="IContentOwner"/> instance with this content manager. NOTE: Instances are stored by weak reference and do not need to be manually removed (see remarks)
        /// </summary>
        /// <remarks><para>Instances are stored by weak reference, so this method should only be called when removing the object early is desired.</para>
        /// <para>Instances will not be kept alive when added, and do not need to be removed to make sure they are garbage collected</para></remarks>
        /// <param name="owner"></param>
        void Remove(IContentOwner owner);
    }

    /// <summary>
    /// Use this class to load content in a LoadContent method, or to initalise rendering of load-time resources with <see cref="PreFrameDraw"/>
    /// </summary>
#if !DEBUG_API

    [System.Diagnostics.DebuggerStepThrough]
#endif
    public sealed class ContentState : IState
    {
        private readonly ContentRegister register;
        private readonly Application application;

        internal ContentState(ContentRegister reg, Application app)
        {
            if (reg == null || app == null)
                throw new ArgumentNullException();
            this.register = reg;
            this.application = app;
        }

        /// <summary>
        /// Implicit conversion to an XNA content manager
        /// </summary>
        /// <returns></returns>
        public static implicit operator ContentManager(ContentState state)
        {
            return (ContentManager)state.register;
        }

        /// <summary>
        /// Implicit conversion to the GraphicsDevice, typically this method should only be used for object creation
        /// </summary>
        /// <returns></returns>
        public static implicit operator GraphicsDevice(ContentState state)
        {
            return GraphicsDeviceManager.Current.GraphicsDevice;
        }

        /// <summary>
        /// Gets the unique ID index for a non-global shader attribute. For use in a call to IShader.SetAttribute, IShader.SetTexture or IShader.SetSamplerState"/>
        /// </summary>
        /// <param name="name">case sensitive name of the shader attribute</param>
        /// <returns>globally unique index of the attribute name</returns>
        //public int GetShaderAttributeNameUniqueID(string name)
        //{
        //    return application.ShaderSystem.GetNameUniqueID(name);
        //}

        /// <summary>
        /// Gets the shader system global interface
        /// </summary>
        //public Graphics.IShaderGlobals ShaderGlobals
        //{
        //    get { return application.ShaderSystem; }
        //}

        /// <summary>
        /// <para>Gets the GraphicsDevice associated with this content state object</para>
        /// <para>Note; This method is provided for convienience. The ContentState class implicitly casts to GraphicsDevice</para>
        /// </summary>
        public GraphicsDevice GraphicsDevice
        {
            get
            {
                return GraphicsDeviceManager.Current.GraphicsDevice;
            }
        }

        /// <summary>
        /// Get the Content Register associated with this state
        /// </summary>
        /// <returns></returns>
        public ContentRegister ContentRegister
        {
            get { return register; }
        }

        //content manager wrapper functionality:

        /// <summary>
        /// Loads an asset that has been processed by the Content Pipeline.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public T Load<T>(string assetName)
        {
            return ((ContentManager)register).Load<T>(assetName);
        }

        /// <summary>
        /// <para>The the item passed in will be drawn at the start of the next frame, before the main application Draw method is called</para>
        /// <para>Use this method to dynamically create rendererd resources at load-time</para>
        /// </summary>
        public void PreFrameDraw(IFrameDraw item)
        {
            application.PreFrameDraw(item);
        }

        /// <summary>
        /// Gets the application associated with this content state object
        /// </summary>
        public Application Application
        {
            get { return application; }
        }

        //internal IState implementation:

        bool IState.IsAsynchronousState { get { return false; } }

        float IState.DeltaTimeFrequency { get { throw new NotSupportedException(); } }

        float IState.DeltaTimeSeconds { get { throw new NotSupportedException(); } }

        long IState.DeltaTimeTicks { get { throw new NotSupportedException(); } }

        long IState.TotalTimeTicks { get { throw new NotSupportedException(); } }

        float IState.TotalTimeSeconds { get { throw new NotSupportedException(); } }
    }

    /// <summary>
    /// Wrapper on an XNA <see cref="ContentManager"/>. Keeps track of <see cref="IContentOwner"/> instatnces by <see cref="WeakReference">weak reference</see>, calling Load/Unload content.
    /// </summary>
    /// <remarks>The <see cref="Application"/> class has its own instance of <see cref="Application.Content"/></remarks>
#if !DEBUG_API

    [System.Diagnostics.DebuggerStepThrough]
#endif
    public sealed class ContentRegister : IContentRegister
    {
        /// <summary>
        /// Implicit conversion to an XNA content manager
        /// </summary>
        /// <param name="reg"></param>
        /// <returns></returns>
        public static implicit operator ContentManager(ContentRegister reg)
        {
            return reg.manager;
        }

        internal ContentState GetState()
        {
            return state;
        }

        private readonly ContentState state;

        /// <summary>
        /// Construct a object content manager, creating an XNA content manager
        /// </summary>
        /// <param name="application">Application instance</param>
        public ContentRegister(Application application)
            : this(application, application.Content.RootDirectory)
        {
        }

        /// <summary>
        /// Construct a object content manager, creating an XNA content manager
        /// </summary>
        /// <param name="application">Application instance</param>
        /// <param name="rootDirectory">Root content directory</param>
        public ContentRegister(Application application, string rootDirectory)
            : this(application, new ContentManager(application.Services, rootDirectory))
        {
        }

        /// <summary>
        /// Construct a object content manager
        /// </summary>
        /// <param name="application">Application instance</param>
        /// <param name="manager">XNA ContentManager instatnce</param>
        public ContentRegister(Application application, ContentManager manager)
        {
            if (application == null || manager == null)
                throw new ArgumentNullException();

            this.state = new ContentState(this, application);
            this.application = application;
            //service = (IGraphicsDeviceService)manager.ServiceProvider.GetService(typeof(IGraphicsDeviceService));

            //if (service == null)
            //    throw new ArgumentException("manager.Services.IGraphicsDeviceService not found");

            created = GraphicsDeviceManager.Current.GraphicsDevice != null;

            //service.DeviceDisposing += new EventHandler<EventArgs>(DeviceResetting);
            //service.DeviceResetting += new EventHandler<EventArgs>(DeviceResetting);
            //service.DeviceCreated += new EventHandler<EventArgs>(DeviceCreated);
            //service.DeviceReset += new EventHandler<EventArgs>(DeviceReset);

            this.manager = manager;
        }

        private void DeviceResetting(object sender, EventArgs e)
        {
            CallUnload();
        }

        private void DeviceReset(object sender, EventArgs e)
        {
            CallLoad();
        }

        private void DeviceCreated(object sender, EventArgs e)
        {
            created = true;
            CallLoad();
        }

        private List<WeakReference> items = new List<WeakReference>();
        private List<WeakReference> highPriorityItems = new List<WeakReference>();
        private Stack<WeakReference> nullReferences = new Stack<WeakReference>();
        private List<WeakReference> buffer;
        private ContentManager manager;
        //private IGraphicsDeviceService service;
        private Application application;
        private bool created;
        private object sync = new object();
        private List<IContentOwner> delayedRemoveList = new List<IContentOwner>();
        private List<IContentOwner> delayedAddList = new List<IContentOwner>();

        //internal IGraphicsDeviceService GraphicsDeviceService { get { return service; } }

        /// <summary>
        /// Register an <see cref="IContentOwner"/> instance with this content manager
        /// </summary>
        /// <param name="owner"></param>
        public void Add(IContentOwner owner)
        {
            if (Monitor.TryEnter(sync))
            {
                try
                {
                    if (manager == null)
                        throw new ObjectDisposedException("this");
                    if (owner == null)
                        throw new ArgumentNullException();
                    if (nullReferences.Count > 0)
                    {
                        WeakReference wr = nullReferences.Pop();
                        wr.Target = owner;
                        items.Add(wr);
                    }
                    else
                        items.Add(new WeakReference(owner));

                    if (created)
                        owner.LoadContent(state);
                }
                finally
                {
                    Monitor.Exit(sync);
                }
            }
            else
            {
                lock (delayedAddList)
                    delayedAddList.Add(owner);
            }
        }

        private void ProcessDelayed()
        {
            lock (delayedAddList)
            {
                foreach (IContentOwner owner in delayedAddList)
                    Add(owner);
                delayedAddList.Clear();
            }
            lock (delayedRemoveList)
            {
                foreach (IContentOwner owner in delayedRemoveList)
                    Remove(owner);
                delayedRemoveList.Clear();
            }
        }

        internal void AddHighPriority(IContentOwner owner)
        {
            lock (sync)
            {
                if (manager == null)
                    throw new ObjectDisposedException("this");
                if (owner == null)
                    throw new ArgumentNullException();
                if (nullReferences.Count > 0)
                {
                    WeakReference wr = nullReferences.Pop();
                    wr.Target = owner;
                    highPriorityItems.Add(wr);
                }
                else
                    highPriorityItems.Add(new WeakReference(owner));

                if (created)
                    owner.LoadContent(state);

                ProcessDelayed();
            }
        }

        /// <summary>
        /// Unregister an <see cref="IContentOwner"/> instance with this content manager. NOTE: Instances are stored by weak reference and do not need to be manually removed (see remarks)
        /// </summary>
        /// <remarks><para>Instances are stored by weak reference, so this method should only be called when removing the object early is desired.</para>
        /// <para>Instances will not be kept alive when added, and do not need to be removed to make sure they are garbage collected</para></remarks>
        /// <param name="owner"></param>
        public void Remove(IContentOwner owner)
        {
            if (Monitor.TryEnter(sync))
            {
                try
                {
                    foreach (WeakReference wr in this.items)
                    {
                        if (wr.Target == owner)
                        {
                            if (this.items.Count > 1)
                            {
                                wr.Target = this.items[this.items.Count - 1].Target;
                                this.items[this.items.Count - 1].Target = null;
                            }
                            else
                                wr.Target = null;
                            break;
                        }
                    }
                    nullReferences.Push(this.items[this.items.Count - 1]);
                    this.items.RemoveAt(this.items.Count - 1);
                }
                finally
                {
                    Monitor.Exit(sync);
                }
            }
            else
            {
                lock (delayedRemoveList)
                    delayedRemoveList.Add(owner);
            }
        }

        private void CallLoad()
        {
            lock (sync)
            {
                if (buffer == null)
                    buffer = new List<WeakReference>();

                for (int i = 0; i < highPriorityItems.Count; i++)
                {
                    IContentOwner loader = highPriorityItems[i].Target as IContentOwner;
                    if (loader != null)
                        loader.LoadContent(this.state);
                    else
                    {
                        nullReferences.Push(highPriorityItems[i]);
                        highPriorityItems.RemoveAt(i--);
                    }
                }

                foreach (WeakReference wr in items)
                {
                    IContentOwner loader = wr.Target as IContentOwner;
                    if (loader != null)
                        buffer.Add(wr);
                    else
                        nullReferences.Push(wr);
                }
                foreach (WeakReference wr in buffer)
                {
                    IContentOwner loader = wr.Target as IContentOwner;
                    if (loader != null)
                    {
                        loader.LoadContent(this.state);
                    }
                }

                List<WeakReference> list = items;
                items = buffer;
                buffer = list;

                buffer.Clear();

                ProcessDelayed();
            }
        }

        private void CallUnload()
        {
            lock (sync)
            {
                if (buffer == null)
                    buffer = new List<WeakReference>();

                for (int i = 0; i < highPriorityItems.Count; i++)
                {
                    IContentOwner loader = highPriorityItems[i].Target as IContentOwner;
                    if (loader != null)
                    {
                        if (loader is IContentUnload)
                            (loader as IContentUnload).UnloadContent();
                    }
                    else
                    {
                        nullReferences.Push(highPriorityItems[i]);
                        highPriorityItems.RemoveAt(i--);
                    }
                }

                foreach (WeakReference wr in items)
                {
                    IContentOwner loader = wr.Target as IContentOwner;
                    if (loader != null)
                        buffer.Add(wr);
                    else
                        nullReferences.Push(wr);
                }
                foreach (WeakReference wr in buffer)
                {
                    IContentUnload loader = wr.Target as IContentUnload;
                    if (loader != null)
                        loader.UnloadContent();
                }

                List<WeakReference> list = items;
                items = buffer;
                buffer = list;

                buffer.Clear();

                ProcessDelayed();
            }
        }

        /// <summary>
        /// Dispose the Content manager and unload all instances
        /// </summary>
        public void Dispose()
        {
            if (items != null)
            {
                CallUnload();
                items.Clear();
            }
            //if (service != null)
            //{
            //    service.DeviceDisposing -= new EventHandler<EventArgs>(DeviceResetting);
            //    service.DeviceResetting -= new EventHandler<EventArgs>(DeviceResetting);
            //    service.DeviceCreated -= new EventHandler<EventArgs>(DeviceCreated);
            //    service.DeviceReset -= new EventHandler<EventArgs>(DeviceReset);
            //    service = null;
            //}
            if (manager != null)
            {
                manager.Dispose();
                manager = null;
            }
            buffer = null;
            items = null;
        }

        /// <summary>
        /// Gets or sets the ContentManager root directory.
        /// </summary>
        public string RootDirectory
        {
            get { return this.manager.RootDirectory; }
            set { this.manager.RootDirectory = value; }
        }
    }
}