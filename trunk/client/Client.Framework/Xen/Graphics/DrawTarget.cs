using System;
using System.Collections.Generic;
using Client.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Xen.Camera;
using Xen.Graphics.Modifier;

namespace Xen.Graphics
{
    /// <summary>
    /// The desired multisample antialiasing level
    /// </summary>
    public enum PreferredMultiSampleLevel
    {
        /// <summary></summary>
        None = 0,
        /// <summary></summary>
        TwoSamples = 2,
        /// <summary></summary>
        FourSamples = 4,
    }

    /// <summary>
    /// Abstract base class for a Draw object that renders a list of drawable items to the screen or a render target
    /// </summary>
#if !DEBUG_API

    [System.Diagnostics.DebuggerStepThrough]
#endif
    public abstract class DrawTarget : Resource, IFrameDraw
    {
        private ICamera camera;
        private bool enabled;
        private bool rendering;
        private List<IDraw> drawList = new List<IDraw>();
        private List<IBeginEndDraw> modifiers, activeModifiers;
        internal static int baseSizeIndex = 1;
        private ClearBufferModifier bufferClear = new ClearBufferModifier(true);

        internal void CloneTo(DrawTarget clone, bool cloneModifiers, bool cloneDrawList)
        {
            clone.activeModifiers = new List<IBeginEndDraw>();
            clone.camera = this.camera;
            clone.drawList = cloneDrawList ? new List<IDraw>(this.drawList) : new List<IDraw>();
            if (this.modifiers != null)
            {
                clone.modifiers = cloneModifiers ? new List<IBeginEndDraw>(this.modifiers) : null;
                if (clone.modifiers != null)
                    clone.activeModifiers = new List<IBeginEndDraw>(clone.modifiers.Capacity);
            }
            clone.rendering = false;
            clone.enabled = true;

            clone.bufferClear = new ClearBufferModifier(true);
            clone.bufferClear.ClearColour = this.bufferClear.ClearColour;
            clone.bufferClear.ClearColourEnabled = this.bufferClear.ClearColourEnabled;
            clone.bufferClear.ClearDepth = this.bufferClear.ClearDepth;
            clone.bufferClear.ClearDepthEnabled = this.bufferClear.ClearDepthEnabled;
            clone.bufferClear.ClearStencilEnabled = this.bufferClear.ClearStencilEnabled;
            clone.bufferClear.ClearStencilValue = this.bufferClear.ClearStencilValue;
            clone.bufferClear.Enabled = this.bufferClear.Enabled;
        }

        internal DrawTarget(ICamera camera)
        {
            if (camera == null)
                throw new ArgumentNullException("camera");
            if (camera is Stack.CameraStack)
                throw new ArgumentException("camera");
            this.camera = camera;
            this.enabled = true;
        }

        internal DrawTarget(ICamera camera, bool enabled)
        {
            if (camera == null)
                throw new ArgumentNullException("camera");
            this.camera = camera;
            this.enabled = enabled;
        }

        /// <summary>
        /// Gets/Changes the clear operations performed when the draw target is drawn
        /// </summary>
        public ClearBufferModifier ClearBuffer
        {
            get { return bufferClear; }
        }

        /// <summary>
        /// Gets the surface format of the colour buffer for this draw target
        /// </summary>
        public abstract SurfaceFormat SurfaceFormat { get; }

        /// <summary>
        /// <para>Gets the depth format of the depth buffer for this draw target</para>
        /// <para>If null, the target does not have a depth buffer</para>
        /// </summary>
        public abstract DepthFormat SurfaceDepthFormat { get; }

        /// <summary>
        /// Gets the multisample level of the draw target
        /// </summary>
        public abstract PreferredMultiSampleLevel MultiSampleType { get; }

        /// <summary>
        /// Gets/Sets the camera used by this draw target
        /// </summary>
        public ICamera Camera
        {
            get { return camera; }
            set
            {
                if (value != camera)
                {
                    if (value == null)
                        throw new ArgumentNullException();
                    if (rendering)
                        throw new InvalidOperationException("DrawTarget is in use");
                    camera = value;
                }
            }
        }

        /// <summary>
        /// Adds a drawable item to the list of items to be drawn to the draw target
        /// </summary>
        /// <param name="drawable"></param>
        public void Add(IDraw drawable)
        {
            drawList.Add(drawable);
        }

        /// <summary>
        /// <para>Adds an Xna DrawableGameComponent to the list of items to be drawn to the draw target</para>
        /// <para>The component will also be initalised and added to the Applications Xna GameComponent update list</para>
        /// <para>Note: The DrawableGameComponent must not modify the current render target!</para>
        /// </summary>
        public void Add(DrawableGameComponent xnaComponent, Application application)
        {
            drawList.Add(new XnaDrawableGameComponentWrapper(xnaComponent, this, application));
        }

        /// <summary>
        /// Adds a drawable item into the list of items to be drawn to the draw target
        /// </summary>
        /// <param name="index"></param>
        /// <param name="drawable"></param>
        public void Insert(int index, IDraw drawable)
        {
            drawList.Insert(index, drawable);
        }

        /// <summary>
        /// <para>Adds an Xna DrawableGameComponent to the list of items to be drawn to the draw target</para>
        /// <para>The component will also be initalised and added to the Applications Xna GameComponent update list</para>
        /// <para>Note: The DrawableGameComponent must not modify the current render target!</para>
        /// </summary>
        public void Insert(int index, DrawableGameComponent xnaComponent, Application application)
        {
            drawList.Insert(index, new XnaDrawableGameComponentWrapper(xnaComponent, this, application));
        }

        /// <summary>
        /// Removes a drawable item from the list of items to be drawn to the draw target
        /// </summary>
        /// <param name="drawable"></param>
        /// <returns>true if the item was removed</returns>
        public bool Remove(IDraw drawable)
        {
            return drawList.Remove(drawable);
        }

        /// <summary>
        /// <para>Removes an Xna DrawableGameComponent item from the list of items to be drawn to the draw target</para>
        /// <para>Note: The component will not be disposed or removed from the Applications Xna GameComponent update list</para>
        /// </summary>
        /// <returns>true if the item was removed</returns>
        public bool Remove(DrawableGameComponent xnaComponent)
        {
            for (int i = 0; i < this.drawList.Count; i++)
            {
                XnaDrawableGameComponentWrapper wrapper = this.drawList[i] as XnaDrawableGameComponentWrapper;
                if (wrapper != null && wrapper.Component == xnaComponent)
                {
                    wrapper.RemovedFromOwner();
                    this.drawList.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Adds a begin/end drawing modified (such as a viewport modified) to the list of modifiers to be used while the draw target is being drawn
        /// </summary>
        /// <param name="modifier"></param>
        public void AddModifier(IBeginEndDraw modifier)
        {
            if (modifiers == null)
            {
                modifiers = new List<IBeginEndDraw>();
                activeModifiers = new List<IBeginEndDraw>();
            }
            modifiers.Add(modifier);
        }

        /// <summary>
        /// Adds a begin/end drawing modified (such as a clear buffer modified) into the list of modifiers to be used while the draw target is being drawn
        /// </summary>
        /// <param name="modifier"></param>
        /// <param name="index"></param>
        public void InsertModifier(int index, IBeginEndDraw modifier)
        {
            if (modifiers == null)
            {
                modifiers = new List<IBeginEndDraw>();
                activeModifiers = new List<IBeginEndDraw>();
            }
            modifiers.Insert(index, modifier);
        }

        /// <summary>
        /// Removes a begin/end drawing modified (such as a clear buffer modified) from the list of modifiers that is used while the draw target is being drawn
        /// </summary>
        /// <param name="modifier"></param>
        public bool RemoveModifier(IBeginEndDraw modifier)
        {
            if (modifiers != null)
                return modifiers.Remove(modifier);
            return false;
        }

        /// <summary>
        /// Gets/Sets if this draw target is enabled (no drawing will occur when disabled)
        /// </summary>
        public bool Enabled
        {
            get { return enabled; }
            set { enabled = value; }
        }

        /// <summary></summary>
        /// <returns></returns>
        protected virtual bool GetEnabled()
        {
            return enabled;
        }

        /// <summary>
        /// Gets the byte size of a render target format
        /// </summary>
        /// <param name="format"></param>
        /// <returns></returns>
        public static int FormatSize(SurfaceFormat format)
        {
            switch (format)
            {
                case SurfaceFormat.Color:
                    //case SurfaceFormat.Rgba1010102:
                    //case SurfaceFormat.Rg32:
                    //case SurfaceFormat.HalfVector2:
                    //case SurfaceFormat.Single:
                    return 4;
                case SurfaceFormat.Bgr565:
                case SurfaceFormat.Bgra5551:
                case SurfaceFormat.Bgra4444:
                    //case SurfaceFormat.HalfSingle:
                    return 2;
                //case SurfaceFormat.Rgba64:
                //case SurfaceFormat.HalfVector4:
                //case SurfaceFormat.Vector2:
                //return 8;
                //case SurfaceFormat.Vector4:
                //return 16;
                default:
                    //throw new NotImplementedException(format.ToString());
                    return 0;
            }
        }

        /// <summary>
        /// Gets the number of channels used by a render target format
        /// </summary>
        /// <param name="format"></param>
        /// <returns></returns>
        public static int FormatChannels(SurfaceFormat format)
        {
            switch (format)
            {
                //case SurfaceFormat.Single:
                //case SurfaceFormat.HalfSingle:
                //    return 1;

                //case SurfaceFormat.Rg32:
                //case SurfaceFormat.HalfVector2:
                //case SurfaceFormat.Vector2:
                //    return 2;

                case SurfaceFormat.Bgr565:
                    return 3;

                case SurfaceFormat.Color:
                //case SurfaceFormat.Rgba1010102:
                case SurfaceFormat.Bgra4444:
                case SurfaceFormat.Bgra5551:
                    //case SurfaceFormat.Rgba64:
                    //case SurfaceFormat.HalfVector4:
                    //case SurfaceFormat.Vector4:
                    return 4;
                default:
                    //throw new NotImplementedException(format.ToString());
                    return 0;
            }
        }

        private static Xen.Graphics.DeviceRenderState nullState = new Xen.Graphics.DeviceRenderState();

        /// <summary>
        /// Perform all drawing to this draw target. All modifiers will be applied, and all drawable items in draw list will be drawn to the draw target
        /// </summary>
        /// <param name="state"></param>
        public void Draw(FrameState state)
        {
            if (!enabled)
                return;

            DrawState drawState = state.DrawState;
            if (drawState.DrawTarget != null)
                throw new ArgumentException("Already rendering to another draw target: " + drawState.DrawTarget.ToString());

            if (GetEnabled())
            {
                drawState.DrawTarget = this;

                int repeats = GetRepeatCount();
                drawState.RenderState.Push(ref nullState);

                rendering = true;
                ushort stackHeight, stateHeight, cameraHeight, preCull, postCull;

#if DEBUG
                System.Threading.Interlocked.Increment(ref drawState.Application.currentFrame.DrawTargetsDrawCount);
#endif
                Begin(drawState);

                for (int repeat = 0; repeat < repeats; repeat++)
                {
                    drawState.GetStackHeight(out stackHeight, out stateHeight, out cameraHeight, out preCull, out postCull);

                    ICamera cam = camera;
                    if (repeats > 1)
                    {
                        if (!BeginRepeat(drawState, repeat, ref cam))
                            continue;
                    }

#if DEBUG
                    System.Threading.Interlocked.Increment(ref drawState.Application.currentFrame.DrawTargetsPassCount);

#if XBOX360
					state.Application.currentFrame.XboxPixelFillBias += this.Width * this.Height;
#endif
#endif

                    if (bufferClear.Enabled)
                        bufferClear.Draw(drawState);

                    if (modifiers != null)
                    {
                        foreach (IBeginEndDraw mod in modifiers)
                        {
                            if (mod.Enabled)
                            {
                                activeModifiers.Add(mod);
                                mod.Begin(drawState);
                            }
                        }
                    }

                    drawState.Camera.Push(cam);

                    foreach (IDraw block in drawList)
                    {
                        if (block.CullTest(drawState))
                            block.Draw(drawState);
                    }

                    drawState.Camera.Pop();

                    //end in reverse order
                    if (modifiers != null)
                    {
                        for (int i = activeModifiers.Count - 1; i >= 0; i--)
                        {
                            activeModifiers[i].End(drawState);
                        }
                        activeModifiers.Clear();
                    }

                    if (repeats > 1)
                        EndRepeat(drawState, repeat);

                    drawState.ValidateStackHeight(stackHeight, stateHeight, cameraHeight, preCull, postCull);
                }

                End(drawState);

                drawState.RenderState.Pop();

                drawState.DrawTarget = null;
                rendering = false;

                //drawState.EndFrameCleanup();
            }
        }

        /// <summary></summary>
        /// <param name="state"></param>
        protected internal abstract void Begin(DrawState state);

        /// <summary></summary>
        /// <param name="state"></param>
        protected internal abstract void End(DrawState state);

        /// <summary></summary>
        protected internal abstract bool HasDepthBuffer { get; }

        /// <summary></summary>
        protected internal abstract bool HasStencilBuffer { get; }

        //void IBeginEndDraw.Begin(DrawState state)	{ this.Begin(state); }
        //void IBeginEndDraw.End(DrawState state)		{ this.End(state); }

        /// <summary>
        /// Gets the width of the draw target
        /// </summary>
        public abstract int Width { get; }

        /// <summary>
        /// Gets the height of the draw target
        /// </summary>
        public abstract int Height { get; }

        /// <summary>
        /// Gets the width/height of the draw target as a Vector2
        /// </summary>
        public abstract Vector2 Size
        {
            get;
        }

        /// <summary></summary>
        internal protected abstract bool GetWidthHeightAsVector(out Vector4 size, ref int changeIndex);

        internal override int GetAllocatedManagedBytes()
        {
            return 0;
        }

        internal virtual int GetRepeatCount()
        {
            return 1;
        }

        internal virtual bool BeginRepeat(DrawState state, int repeat, ref ICamera camera)
        {
            return true;
        }

        internal virtual void EndRepeat(DrawState state, int repeat)
        {
        }

        internal sealed override ResourceType GraphicsResourceType
        {
            get { return ResourceType.RenderTarget; }
        }
    }

    /// <summary>
    /// A draw target that draws directly to the screen
    /// </summary>
#if !DEBUG_API

    [System.Diagnostics.DebuggerStepThrough]
#endif
    public sealed class DrawTargetScreen : DrawTarget
    {
        private readonly Xen.Application application;
        private bool hasDepth, hasStencil;
        private Vector2 windowSize;
        private int windowSizeChangeIndex = 1;

        /// <summary>
        /// Construct the draw target
        /// </summary>
        /// <param name="camera"></param>
        public DrawTargetScreen(ICamera camera)
            : base(camera)
        {
            this.application = Application.GetApplicationInstance();

            if (application == null || !application.IsInitailised)
                throw new InvalidOperationException("Application instance has not had Initalise() called yet");

            SetDepth(application);

            Vector2 ws = new Vector2((float)Width, (float)Height);
            if (ws.X != windowSize.X || ws.Y != windowSize.Y)
            {
                windowSize = ws;
                windowSizeChangeIndex = System.Threading.Interlocked.Increment(ref DrawTarget.baseSizeIndex);
            }
        }

        private void SetDepth(Application application)
        {
            DepthFormat depth = application.GraphicsDevice.PresentationParameters.DepthStencilFormat;

            hasDepth = true;
            hasStencil = depth == DepthFormat.Depth24Stencil8;
        }

        /// <summary></summary>
        /// <param name="state"></param>
        protected internal override void Begin(DrawState state)
        {
#if DEBUG
            if (this.application != Application.GetApplicationInstance())
                throw new InvalidOperationException("DrawTargetScreen is being used with an application it wasn't created with");
#endif

            GraphicsDevice device = state.graphics;

            //state.shaderSystem.ResetTextures();

            device.SetRenderTarget(null);

            Vector2 ws = new Vector2((float)Width, (float)Height);
            if (ws.X != windowSize.X || ws.Y != windowSize.Y)
            {
                windowSize = ws;
                windowSizeChangeIndex = System.Threading.Interlocked.Increment(ref DrawTarget.baseSizeIndex);
            }
        }

        /// <summary></summary>
        /// <param name="state"></param>
        protected internal override void End(DrawState state)
        {
        }

        /// <summary>
        /// Gets the surface format of the colour buffer for the screen
        /// </summary>
        public override SurfaceFormat SurfaceFormat
        {
            get { return application.GetScreenFormat(); }
        }

        /// <summary>
        /// Gets the depth format of the depth buffer for the screen
        /// </summary>
        public override DepthFormat SurfaceDepthFormat
        {
            get { return application.GetScreenDepthFormat(); }
        }

        /// <summary>
        /// Gets the multisample level for this draw target
        /// </summary>
        public override PreferredMultiSampleLevel MultiSampleType
        {
            get { return application.GetScreenMultisample(); }
        }

        /// <summary>
        /// Gets the width of the screen
        /// </summary>
        public override int Width
        {
            get
            {
                return application.WindowWidth;
            }
        }

        /// <summary>
        /// Gets the height of the screen
        /// </summary>
        public override int Height
        {
            get
            {
                return application.WindowHeight;
            }
        }

        /// <summary></summary>
        internal protected override bool GetWidthHeightAsVector(out Vector4 size, ref int changeIndex)
        {
            size = new Vector4(windowSize, 0, 0);
            bool result = changeIndex != this.windowSizeChangeIndex;
            changeIndex = this.windowSizeChangeIndex;
            return result;
        }

        /// <summary>
        /// Gets the width/height of the draw target as a Vector2
        /// </summary>
        public override Vector2 Size
        {
            get { return windowSize; }
        }

        /// <summary></summary>
        protected internal override bool HasDepthBuffer
        {
            get { return hasDepth; }
        }

        /// <summary></summary>
        protected internal override bool HasStencilBuffer
        {
            get { return hasStencil; }
        }

        internal override void Warm(Application application, GraphicsDevice device)
        {
        }

        internal override int GetAllocatedDeviceBytes()
        {
            //approximate
            return 0;// 2 * (4 + hasDepth ? 2 : 0) * Width * Height;
        }

        internal override int GetAllocatedManagedBytes()
        {
            return 0;
        }

        internal override bool InUse
        {
            get { return true; }
        }

        internal override bool IsDisposed
        {
            get { return false; }
        }
    }

    /// <summary>
    /// Stores a list of <see cref="DrawTargetTexture2D"/> as a group, for use with Multiple Render Target (MRT) support. See <see cref="MaxSimultaneousRenderTargets"/> for the maximum group size (usually 4)
    /// </summary>
    /// <remarks>
    /// <para>When using multiple render targets, each target must be the same byte size.</para>
    /// <para>Most hardware does not support blending with MRT</para>
    /// <para>The pixel shader being used must ouput a colour value for each render target being used</para>
    /// <para>For example, drawing to three render targets at once:</para>
    /// <code>
    /// void pixelShader(out float4 colour0 : COLOR0, out float4 colour1 : COLOR1, out float4 colour2 : COLOR2)
    /// {
    ///		colour0 = float4(...);
    ///		colour1 = float4(...);
    ///		colour2 = float4(...);
    ///		...
    /// }
    /// </code>
    /// </remarks>
#if !DEBUG_API

    [System.Diagnostics.DebuggerStepThrough]
#endif
    public sealed class DrawTargetTexture2DGroup : DrawTarget, IDisposable
    {
        private DrawTargetTexture2D[] targets;
        private RenderTargetBinding[] binding;

        /// <summary>
        /// Construct the render target group
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="targets"></param>
        public DrawTargetTexture2DGroup(ICamera camera, params DrawTargetTexture2D[] targets)
            : base(camera)
        {
            this.targets = (DrawTargetTexture2D[])targets.Clone();

            if (targets.Length < 1)
                throw new ArgumentException("At least one render targets must be specified");
            if (targets.Length > MaxSimultaneousRenderTargets)
                throw new ArgumentException(string.Format("Device only supports {0} simultaneous render targets", MaxSimultaneousRenderTargets));

            DrawTargetTexture2D baseTarget = targets[0];
            if (baseTarget == null)
                throw new ArgumentNullException(string.Format("target[{0}]", 0));

            for (int i = 1; i < targets.Length; i++)
            {
                if (targets[i] == null)
                    throw new ArgumentNullException(string.Format("target[{0}]", i));
                if (targets[i].Width != baseTarget.Width ||
                    targets[i].Height != baseTarget.Height)
                    throw new ArgumentException(string.Format("target[{0}] size mismatch with target[0]", i));
                if (FormatSize(targets[i].SurfaceFormat) !=
                    FormatSize(baseTarget.SurfaceFormat))
                    throw new ArgumentException(string.Format("target[{0}] SurfaceFormat size mismatch with target[0]", i));
                if (targets[i].MultiSampleType != baseTarget.MultiSampleType)
                    throw new ArgumentException(string.Format("target[{0}] multisample mismatch with target[0]", i));
            }
        }

        /// <summary>
        /// Gets the number of render targets stored in the group
        /// </summary>
        public int Count
        {
            get
            {
                if (targets == null)
                    throw new ObjectDisposedException("this");
                return targets.Length;
            }
        }

        /// <summary>
        /// Creates a clone of this draw target that shares the same rendering resources (no new resources are allocated)
        /// </summary>
        /// <param name="copyModifiers">copy modifier list into the clone</param>
        /// <param name="copyDrawList">copy draw list into the clone</param>
        /// <returns></returns>
        public DrawTargetTexture2DGroup Clone(bool copyModifiers, bool copyDrawList)
        {
            DrawTargetTexture2DGroup clone = (DrawTargetTexture2DGroup)MemberwiseClone();
            CloneTo(clone, copyModifiers, copyDrawList);
            return clone;
        }

        /// <summary>
        /// Get a draw target by index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public DrawTargetTexture2D GetTarget(int index)
        {
            if (targets == null)
                throw new ObjectDisposedException("this");
            return targets[index];
        }

        /// <summary>
        /// Gets the surface format of the first draw target in the group
        /// </summary>
        public override SurfaceFormat SurfaceFormat
        {
            get { return targets[0].SurfaceFormat; }
        }

        /// <summary>
        /// <para>Gets the depth format of the depth buffer for this draw texture</para>
        /// </summary>
        public override DepthFormat SurfaceDepthFormat
        {
            get { return targets[0].SurfaceDepthFormat; }
        }

        /// <summary>
        /// Gets the multisample level for the first draw target in the group
        /// </summary>
        public override PreferredMultiSampleLevel MultiSampleType
        {
            get { return targets[0].MultiSampleType; }
        }

        /// <summary>
        /// Gets the maximum group size supported by the hardware (usually 4 - the maximum value)
        /// </summary>
        public static int MaxSimultaneousRenderTargets
        {
            get
            {
                return 1;// GraphicsAdapter.DefaultAdapter.IsProfileSupported(GraphicsProfile.HiDef) ? 4 : 1;
            }
        }

        /// <summary></summary>
        /// <param name="state"></param>
        protected internal override void Begin(DrawState state)
        {
            if (targets == null)
                throw new ObjectDisposedException("this");

            for (int i = 0; i < targets.Length; i++)
            {
                targets[i].Warm(state);
                if (targets[i].IsDisposed)
                    throw new ObjectDisposedException("RenderTexture");
            }

#if XBOX360
			state.nonScreenRenderComplete = true;
#endif

            GraphicsDevice device = state.graphics;

            //state.shaderSystem.ResetTextures();

            if (binding == null)
            {
                this.binding = new RenderTargetBinding[targets.Length];
                for (int i = 0; i < targets.Length; i++)
                    binding[i] = new RenderTargetBinding(targets[i].GetRenderTarget2D());
            }

            device.SetRenderTargets(binding);
        }

        /// <summary></summary>
        /// <param name="state"></param>
        protected internal override void End(DrawState state)
        {
            if (targets == null)
                throw new ObjectDisposedException("this");
        }

        /// <summary></summary>
        protected internal override bool HasDepthBuffer
        {
            get
            {
                if (targets == null)
                    throw new ObjectDisposedException("this");
                return targets[0].HasDepthBuffer;
            }
        }

        /// <summary></summary>
        protected internal override bool HasStencilBuffer
        {
            get
            {
                if (targets == null)
                    throw new ObjectDisposedException("this");
                return targets[0].HasStencilBuffer;
            }
        }

        /// <summary>
        /// Gets the width of the draw targets in the group
        /// </summary>
        public override int Width
        {
            get
            {
                if (targets == null)
                    throw new ObjectDisposedException("this");
                return targets[0].Width;
            }
        }

        /// <summary>
        /// Gets the height of the draw targets in the group
        /// </summary>
        public override int Height
        {
            get
            {
                if (targets == null)
                    throw new ObjectDisposedException("this");
                return targets[0].Height;
            }
        }

        /// <summary></summary>
        /// <param name="size"></param>
        /// <param name="changeIndex"></param>
        /// <returns></returns>
        protected internal override bool GetWidthHeightAsVector(out Vector4 size, ref int changeIndex)
        {
            if (targets == null)
                throw new ObjectDisposedException("this");
            return targets[0].GetWidthHeightAsVector(out size, ref changeIndex);
        }

        /// <summary>
        /// Gets the width/height of the draw target as a Vector2
        /// </summary>
        public override Vector2 Size
        {
            get
            {
                if (targets == null)
                    throw new ObjectDisposedException("this");
                return targets[0].Size;
            }
        }

        #region IDisposable Members

        void IDisposable.Dispose()
        {
            this.targets = null;
        }

        internal override int GetAllocatedDeviceBytes()
        {
            return 0;
        }

        internal override int GetAllocatedManagedBytes()
        {
            return 0;
        }

        internal override bool InUse
        {
            get { return true; }
        }

        internal override bool IsDisposed
        {
            get { return targets == null; }
        }

        internal override void Warm(Application application, GraphicsDevice device)
        {
            if (targets == null)
                throw new ObjectDisposedException("this");
            for (int i = 0; i < targets.Length; i++)
                targets[i].Warm(application, device);
        }

        #endregion IDisposable Members
    }

    /// <summary>
    /// A draw target that draws to a <see cref="Texture2D"/> render target.
    /// </summary>
    /// <remarks>
    /// <para>Most draw targets will create the render target resoures. Note these resoures are not created until either the first time the target is drawn, or <see cref="Resource.Warm(IState)"/> is called.</para>
    /// <para>To share the resources used by a draw target texture, use <see cref="Clone"/></para>
    /// <para>A draw target can be created from XNA render targets using <see cref="CreateFromRenderTarget2D(ICamera,RenderTarget2D)"/> (not recommended)</para>
    /// </remarks>
#if !DEBUG_API

    [System.Diagnostics.DebuggerStepThrough]
#endif
    public sealed class DrawTargetTexture2D : DrawTarget, IDisposable, IContentOwner
    {
        private RenderTarget2D texture;
        private readonly int width, height;
        private bool hasDepth, hasStencil;
        private bool mipmap, depthEnabled = true;
        private SurfaceFormat format;
        private PreferredMultiSampleLevel multisample;
        private RenderTargetUsage usage;
        private DepthFormat depthFormat;
        private readonly Vector4 sizeAsVector;
        private readonly int sizeIndex = System.Threading.Interlocked.Increment(ref DrawTarget.baseSizeIndex);
        private bool ownerRegistered, isDisposed;
        private DrawTargetTexture2D cloneOf;

        /// <summary>
        /// Gets the surface format of the colour buffer for this draw texture
        /// </summary>
        public override SurfaceFormat SurfaceFormat
        {
            get { return format; }
        }

        /// <summary>
        /// <para>Gets the depth format of the depth buffer for this draw texture</para>
        /// <para>When this value is null, the target does not have a depth buffer</para>
        /// </summary>
        public override DepthFormat SurfaceDepthFormat
        {
            get { return depthFormat; }
        }

        /// <summary>
        /// Gets the multisample level for this draw target
        /// </summary>
        public override PreferredMultiSampleLevel MultiSampleType
        {
            get { return multisample; }
        }

        /// <summary>
        /// Gets the XNA render target created or shared by this draw texture. Note: Resources are not created until the first time the target is drawn or <see cref="Resource.Warm(IState)"/> is called. Directly accessing this resource is not recommended
        /// </summary>
        public RenderTarget2D GetRenderTarget2D()
        {
            return texture;
        }

        /// <summary>
        /// Creates a clone of this draw target that shares the same rendering resources (no new resources are allocated)
        /// </summary>
        /// <param name="copyModifiers">copy modifier list into the clone</param>
        /// <param name="copyDrawList">copy draw list into the clone</param>
        /// <param name="retainDepth">cloned draw target should also retain the depth stencil buffer</param>
        /// <returns></returns>
        public DrawTargetTexture2D Clone(bool retainDepth, bool copyModifiers, bool copyDrawList)
        {
            DrawTargetTexture2D clone = (DrawTargetTexture2D)MemberwiseClone();
            clone.cloneOf = this;
            clone.depthEnabled &= retainDepth;
            CloneTo(clone, copyModifiers, copyDrawList);

            if (!retainDepth)
            {
                clone.ClearBuffer.ClearDepthEnabled = false;
                clone.ClearBuffer.ClearStencilEnabled = false;
            }

            return clone;
        }

        private DrawTargetTexture2D CloneRoot
        {
            get
            {
                if (cloneOf == null)
                    return this;
                else
                    return cloneOf.CloneRoot;
            }
        }

        /// <summary>
        /// Returns true when comparing equivalent draw targets, including cloned targets
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj is DrawTargetTexture2D)
                return CloneRoot == (obj as DrawTargetTexture2D).CloneRoot;
            return false;
        }

        /// <summary></summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            if (cloneOf == null)
                return base.GetHashCode();
            return CloneRoot.GetHashCode();
        }

        /// <summary>
        /// Gets the texture for this draw target. Returns NULL if the resource hasn't been created. NOTE: this texture will become invalid after a device reset (see <see cref="IContentOwner"/> for details)
        /// </summary>
        /// <remarks>Call <see cref="GetTexture(IState)"/> to get the texture, creating the resource beforehand if required.</remarks>
        /// <returns>Texture for this draw target</returns>
        public Texture2D GetTexture()
        {
            return texture;
        }

        /// <summary>
        /// Gets the texture for this draw target, Warming the resource if required. NOTE: this texture will become invalid after a device reset (see <see cref="IContentOwner"/> for details)
        /// </summary>
        /// <returns>Texture for this draw target</returns>
        public Texture2D GetTexture(IState state)
        {
            if (this.texture == null && state != null)
            {
                DrawState ds = state as DrawState;
                if (ds != null && ds.GetDrawTarget() != null)
                    throw new InvalidOperationException("A DrawTargetTexture2D Resource may not be created while rendering to another DrawTarget");
                this.Warm(state.Application);
            }

            return GetTexture();
        }

        private void SetHasDepth()
        {
            hasDepth = true;
            hasStencil = depthFormat == DepthFormat.Depth24Stencil8;
        }

        /*
        /// <summary>
        /// Creates the draw texture. Note: Rendering resources will not be created until the first time the target is drawn or <see cref="Resource.Warm(IState)"/> is called
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="format"></param>
        public DrawTargetTexture2D(ICamera camera, int width, int height, SurfaceFormat format)
            : this(camera, width, height, format, false, MultiSampleType.None, RenderTargetUsage.PlatformContents)
        {
        }
        */

        /// <summary>
        /// Creates the draw texture. Note: Rendering resources will not be created until the first time the target is drawn or <see cref="Resource.Warm(IState)"/> is called
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="format"></param>
        /// <param name="depthFormat"></param>
        public DrawTargetTexture2D(ICamera camera, int width, int height, SurfaceFormat format, DepthFormat depthFormat)
            : this(camera, width, height, format, depthFormat, false, PreferredMultiSampleLevel.None)
        {
        }

        //public DrawTargetTexture2D(ICamera camera, int width, int height, SurfaceFormat format, bool mipmap)
        //    : this(camera, width, height, format, mipmap, MultiSampleType.None)
        //{
        //}

        /// <summary>
        /// Creates the draw texture. Note: Rendering resources will not be created until the first time the target is drawn or <see cref="Resource.Warm(IState)"/> is called
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="format"></param>
        /// <param name="depthFormat"></param>
        /// <param name="mipmap"></param>
        public DrawTargetTexture2D(ICamera camera, int width, int height, SurfaceFormat format, DepthFormat depthFormat, bool mipmap)
            : this(camera, width, height, format, depthFormat, mipmap, PreferredMultiSampleLevel.None)
        {
        }

        /// <summary>
        /// Creates the draw texture. Note: Rendering resources will not be created until the first time the target is drawn or <see cref="Resource.Warm(IState)"/> is called
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="format"></param>
        /// <param name="depthFormat"></param>
        /// <param name="mipmap"></param>
        /// <param name="multisample"></param>
        public DrawTargetTexture2D(ICamera camera, int width, int height, SurfaceFormat format, DepthFormat depthFormat, bool mipmap, PreferredMultiSampleLevel multisample)
            : this(camera, width, height, format, depthFormat, mipmap, multisample, RenderTargetUsage.DiscardContents)
        {
        }

        private DrawTargetTexture2D(ICamera camera, int width, int height, SurfaceFormat format, bool mipmap, PreferredMultiSampleLevel multisample, RenderTargetUsage usage) :
            base(camera)
        {
            this.mipmap = mipmap;
            this.format = format;
            this.multisample = multisample;
            this.usage = usage;
            this.width = width;
            this.height = height;

            this.sizeAsVector = new Vector4((float)this.width, (float)this.height, 0, 0);
        }

        /// <summary>
        /// Creates the draw texture. Note: Rendering resources will not be created until the first time the target is drawn or <see cref="Resource.Warm(IState)"/> is called
        /// </summary>
        public DrawTargetTexture2D(ICamera camera, int width, int height, SurfaceFormat format, DepthFormat depthFormat, bool mipmap, PreferredMultiSampleLevel multisample, RenderTargetUsage usage)
            : this(camera, width, height, format, mipmap, multisample, usage)
        {
            this.depthFormat = depthFormat;

            if (this.depthFormat == DepthFormat.None)
            {
                this.ClearBuffer.ClearDepthEnabled = false;
                this.ClearBuffer.ClearStencilEnabled = false;
            }
            else
            {
                if (this.depthFormat != DepthFormat.Depth24Stencil8)
                    this.ClearBuffer.ClearStencilEnabled = false;
            }

            SetHasDepth();
        }

        /// <summary>
        /// Create a draw texture directly from an XNA render target (not recommended)
        /// </summary>
        /// <returns></returns>
        public static DrawTargetTexture2D CreateFromRenderTarget2D(ICamera camera, RenderTarget2D renderTexture)
        {
            return new DrawTargetTexture2D(camera, renderTexture);
        }

        private DrawTargetTexture2D(ICamera camera, RenderTarget2D xnaRenderTexture)
            : base(camera)
        {
            if (xnaRenderTexture == null)
                throw new ArgumentNullException();
            this.texture = xnaRenderTexture;

            this.width = xnaRenderTexture.Width;
            this.height = xnaRenderTexture.Height;

            this.sizeAsVector = new Vector4((float)this.width, (float)this.height, 0, 0);

            this.depthFormat = xnaRenderTexture.DepthStencilFormat;

            SetHasDepth();
        }

        /// <summary>
        /// Returns true if the hardware supports the given colour buffer surface format
        /// </summary>
        /// <param name="format"></param>
        /// <returns></returns>
        //public static bool SupportsFormat(ref SurfaceFormat format)
        //{
        //    GraphicsProfile profile = Application.GetApplicationInstance().GraphicsDevice.GraphicsProfile;
        //    DepthFormat depth = DepthFormat.Depth24Stencil8;
        //    int ms = 0;
        ////    return GraphicsAdapter.DefaultAdapter.QueryRenderTargetFormat(profile,format,depth,ms, out format, out depth, out ms);
        //}

        /// <summary>
        /// Returns true if a format is guarenteed to support texture filtering.
        /// <para>Note: XNA 4 no longer provides correct access, so this returns a lowest common demonimator</para>
        /// </summary>
        /// <param name="format"></param>
        /// <returns></returns>
        public static bool SupportsFormatFiltering(SurfaceFormat format)
        {
            switch (format)
            {
                //case SurfaceFormat.Rg32:
                //case SurfaceFormat.Rgba64:
                //case SurfaceFormat.Single:
                //case SurfaceFormat.Vector2:
                //case SurfaceFormat.Vector4:
                //case SurfaceFormat.HalfSingle:
                //case SurfaceFormat.HalfVector2:
                //case SurfaceFormat.HalfVector4:
                //case SurfaceFormat.HdrBlendable:
                //    return false;
                default:
                    return true;
            }
        }

        /// <summary>
        /// Dispose the draw target and all resources used. If this render target is a clone, the shared resources are NOT disposed
        /// </summary>
        public void Dispose()
        {
            isDisposed = true;

            if (cloneOf == null)
            {
                if (texture != null)
                    texture.Dispose();
            }
            this.texture = null;
        }

        /// <summary></summary>
        /// <param name="state"></param>
        protected internal override void Begin(DrawState state)
        {
            if (isDisposed)
                throw new ObjectDisposedException("this");

#if XBOX360
			state.nonScreenRenderComplete = true;
#endif

            GraphicsDevice device = state.graphics;

            if (texture == null)
            {
                Warm(state);
            }

            if (texture.IsDisposed)
                throw new ObjectDisposedException("RenderTexture");

            //state.shaderSystem.ResetTextures();

            device.SetRenderTarget(texture);

            if (!this.depthEnabled)
            {
                //in XNA 4, can't disable the depth buffer without modifying render states :-(
                state.renderState.ForceDisableDepthBit = Stack.DeviceRenderStateStack.ForceDisableDepthMask;
            }
        }

        /// <summary></summary>
        /// <param name="state"></param>
        protected internal override void End(DrawState state)
        {
            if (!this.depthEnabled)
            {
                state.renderState.ForceDisableDepthBit = 0;
            }

            if (texture.IsDisposed)
                throw new ObjectDisposedException("RenderTexture");
        }

        /// <summary>
        /// Gets the width of the draw target
        /// </summary>
        public override int Width
        {
            get { return width; }
        }

        /// <summary>
        /// Gets the height of the draw target
        /// </summary>
        public override int Height
        {
            get { return height; }
        }

        /// <summary></summary>
        internal protected override bool GetWidthHeightAsVector(out Vector4 size, ref int changeIndex)
        {
            size = sizeAsVector;
            bool result = changeIndex != this.sizeIndex;
            changeIndex = this.sizeIndex;
            return result;
        }

        /// <summary>
        /// Gets the width/height of the draw target as a Vector2
        /// </summary>
        public override Vector2 Size
        {
            get { return new Vector2(sizeAsVector.X, sizeAsVector.Y); }
        }

        /// <summary></summary>
        protected internal override bool HasDepthBuffer
        {
            get { return hasDepth && depthEnabled; }
        }

        /// <summary></summary>
        protected internal override bool HasStencilBuffer
        {
            get { return hasStencil && depthEnabled; }
        }

        internal override int GetAllocatedDeviceBytes()
        {
            if (cloneOf != null)
                return 0;
            int bytes = 0;
            if (texture != null)
                bytes += FormatSize(this.format) * Width * Height;

            int depthSize = 0;
            switch (depthFormat)
            {
                case DepthFormat.Depth16:
                    depthSize = 2;
                    break;
                case DepthFormat.Depth24:
                case DepthFormat.Depth24Stencil8:
                    depthSize = 4;
                    break;
            }
            bytes += depthSize * Width * Height;
            return bytes;
        }

        internal override int GetAllocatedManagedBytes()
        {
            return 0;
        }

        internal override bool InUse
        {
            get { return texture != null; }
        }

        internal override bool IsDisposed
        {
            get
            {
                return isDisposed;
            }
        }

        internal override void Warm(Application application, GraphicsDevice device)
        {
            if (cloneOf != null)
            {
                CloneRoot.Warm(application);
                this.texture = CloneRoot.texture;
                return;
            }

            if (!ownerRegistered)
            {
                ownerRegistered = true;
                application.Content.AddHighPriority(this);
            }

            if (texture == null)
            {
                if (this.isDisposed)
                    throw new InvalidOperationException("Base render target is null or has been disposed");

                this.texture = new RenderTarget2D(device, width, height, mipmap, format, depthFormat, (int)multisample, usage);
            }
        }

        void IContentOwner.LoadContent(ContentState state)
        {
            if (IsDisposed)
                return;

            GraphicsDevice device = state;

            if (texture != null)
            {
                if (cloneOf == null)
                {
                    texture.Dispose();
                }
                texture = null;
            }

            Warm(state);
        }
    }

    /// <summary>
    /// A draw target that draws to a <see cref="TextureCube"/> render target.
    /// </summary>
    /// <remarks>
    /// <para>Most draw targets will create the render target resoures. Note these resoures are not created until either the first time the target is drawn, or <see cref="Resource.Warm(IState)"/> is called.</para>
    /// <para>To share the resources used by a draw target texture, use <see cref="Clone"/></para>
    /// <para>A draw target can be created from XNA render targets using <see cref="CreateFromRenderTargetCube(ICamera,RenderTargetCube)"/> (not recommended)</para>
    /// </remarks>
#if !DEBUG_API

    [System.Diagnostics.DebuggerStepThrough]
#endif
    public sealed class DrawTargetTextureCube : DrawTarget, IDisposable, IContentOwner
    {
        private RenderTargetCube texture;
        private readonly int resolution;
        private bool hasDepth, hasStencil;
        private bool mipmap, depthEnabled = true;
        private SurfaceFormat format;
        private PreferredMultiSampleLevel multisample;
        private RenderTargetUsage usage;
        private DepthFormat depthFormat;
        private readonly Vector4 sizeAsVector;
        private readonly int sizeIndex = System.Threading.Interlocked.Increment(ref DrawTarget.baseSizeIndex);
        private Camera3D cubeCamera;
        private Matrix cubeCameraMatrix;
        private readonly bool[] facesEnabled = new bool[6] { true, true, true, true, true, true };
        private bool anyFaceEnabled = true;
        private int maxFaceEnabled = 5;
        private int minFaceEnabled = 0;
        private bool ownerRegistered = false;
        private bool isDisposed = false;
        private DrawTargetTextureCube cloneOf;

        /// <summary>
        /// Gets if rendering to a cube face is enabled
        /// </summary>
        /// <param name="face"></param>
        /// <returns></returns>
        public bool GetFaceRenderEnabled(CubeMapFace face)
        {
            return facesEnabled[(int)face];
        }

        /// <summary>
        /// Set enabled/disabled rendering to a cubemap face
        /// </summary>
        /// <param name="face"></param>
        /// <param name="enabled"></param>
        public void SetFaceRenderEnabled(CubeMapFace face, bool enabled)
        {
            if (facesEnabled[(int)face] != enabled)
            {
                facesEnabled[(int)face] = enabled;
                ComputeEnabledFaces();
            }
        }

        /// <summary>
        /// Creates a clone of this draw target that shares the same rendering resources (no new resources are allocated)
        /// </summary>
        /// <param name="copyModifiers">copy modifier list into the clone</param>
        /// <param name="copyDrawList">copy draw list into the clone</param>
        /// <returns></returns>
        public DrawTargetTextureCube Clone(bool copyModifiers, bool copyDrawList)
        {
            DrawTargetTextureCube clone = (DrawTargetTextureCube)MemberwiseClone();
            clone.cloneOf = this;
            CloneTo(clone, copyModifiers, copyDrawList);

            return clone;
        }

        private DrawTargetTextureCube CloneRoot
        {
            get
            {
                if (cloneOf == null)
                    return this;
                else
                    return cloneOf.CloneRoot;
            }
        }

        /// <summary>
        /// Returns true when comparing equivalent draw targets, including cloned targets
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj is DrawTargetTextureCube)
                return CloneRoot == (obj as DrawTargetTextureCube).CloneRoot;
            return false;
        }

        /// <summary></summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            if (cloneOf == null)
                return base.GetHashCode();
            return CloneRoot.GetHashCode();
        }

        private void ComputeEnabledFaces()
        {
            anyFaceEnabled = false;
            maxFaceEnabled = int.MinValue;
            minFaceEnabled = int.MaxValue;

            for (int i = 0; i < facesEnabled.Length; i++)
            {
                if (facesEnabled[i])
                {
                    anyFaceEnabled = true;
                    maxFaceEnabled = Math.Max(maxFaceEnabled, i);
                    minFaceEnabled = Math.Min(minFaceEnabled, i);
                }
            }
        }

        /// <summary></summary>
        /// <returns></returns>
        protected override bool GetEnabled()
        {
            return anyFaceEnabled && base.GetEnabled();
        }

        /*
        PositiveX = 0,
        NegativeX = 1,
        PositiveY = 2,
        NegativeY = 3,
        PositiveZ = 4,
        NegativeZ = 5,*/
        private static Matrix[] CubeMapFaceMatrices = new Matrix[]
		{
			Matrix.CreateLookAt(Vector3.Zero,new Vector3(1,0,0),new Vector3(0,1,0)),
			Matrix.CreateLookAt(Vector3.Zero,new Vector3(-1,0,0),new Vector3(0,1,0)),
			Matrix.CreateLookAt(Vector3.Zero,new Vector3(0,1,0),new Vector3(0,0,1)),
			Matrix.CreateLookAt(Vector3.Zero,new Vector3(0,-1,0),new Vector3(0,0,-1)),
			Matrix.CreateLookAt(Vector3.Zero,new Vector3(0,0,-1),new Vector3(0,1,0)),
			Matrix.CreateLookAt(Vector3.Zero,new Vector3(0,0,1),new Vector3(0,1,0)),
		};

        /// <summary>
        /// Gets the matrix that represents the rotation of a cubemap face
        /// </summary>
        /// <param name="face"></param>
        /// <param name="matrix"></param>
        public static void GetCubeMapFaceMatrix(CubeMapFace face, out Matrix matrix)
        {
            matrix = CubeMapFaceMatrices[(int)face];
        }

        /// <summary>
        /// Returns true if the hardware supports the given colour buffer surface format
        /// </summary>
        /// <param name="format"></param>
        /// <returns></returns>
        public static bool SupportsFormat(ref SurfaceFormat format)
        {
            return true;
        }

        /// <summary>
        /// Gets the surface format of the colour buffer for this draw cubemap
        /// </summary>
        public override SurfaceFormat SurfaceFormat
        {
            get { return format; }
        }

        /// <summary>
        /// <para>Gets the depth format of the depth buffer for this draw texture</para>
        /// <para>When this value is null, the target does not have a depth buffer</para>
        /// </summary>
        public override DepthFormat SurfaceDepthFormat
        {
            get { return depthFormat; }
        }

        /// <summary>
        /// Gets the multisample level for this draw target
        /// </summary>
        public override PreferredMultiSampleLevel MultiSampleType
        {
            get { return multisample; }
        }

        /// <summary>
        /// Gets the XNA render target created or shared by this draw cubemap. Note: Resources are not created until the first time the target is drawn or <see cref="Resource.Warm(IState)"/> is called. Directly accessing this resource is not recommended
        /// </summary>
        public RenderTargetCube GetRenderTargetCube()
        {
            return texture;
        }

        /// <summary>
        /// Gets the texture for this draw target. Returns NULL if the resource hasn't been created. NOTE: this texture will become invalid after a device reset (see <see cref="IContentOwner"/> for details)
        /// </summary>
        /// <remarks>Call <see cref="GetTexture(DrawState)"/> to get the texture, creating the resource beforehand if required.</remarks>
        /// <returns>Texture for this draw target</returns>
        public TextureCube GetTexture()
        {
            return texture;
        }

        /// <summary>
        /// Gets the texture for this draw target, Warming the resource if required. NOTE: this texture will become invalid after a device reset (see <see cref="IContentOwner"/> for details)
        /// </summary>
        /// <returns>Texture for this draw target</returns>
        public TextureCube GetTexture(DrawState state)
        {
            if (texture == null)
            {
                if (state.GetDrawTarget() != null)
                    throw new InvalidOperationException("A DrawTargetTextureCube Resource may not be created while rendering to another DrawTarget");
                this.Warm(state);
            }

            return GetTexture();
        }

        private void SetHasDepth()
        {
            hasDepth = true;
            hasStencil = depthFormat == DepthFormat.Depth24Stencil8;
        }

        /// <summary>
        /// Creates the draw target cubemap. Note: Rendering resources will not be created until the first time the target is drawn or <see cref="Resource.Warm(IState)"/> is called
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="resolution"></param>
        /// <param name="format"></param>
        /// <param name="depthFormat"></param>
        public DrawTargetTextureCube(ICamera camera, int resolution, SurfaceFormat format, DepthFormat depthFormat)
            : this(camera, resolution, format, depthFormat, false, PreferredMultiSampleLevel.None)
        {
        }

        /// <summary>
        /// Creates the draw target cubemap. Note: Rendering resources will not be created until the first time the target is drawn or <see cref="Resource.Warm(IState)"/> is called
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="resolution"></param>
        /// <param name="format"></param>
        /// <param name="depthFormat"></param>
        /// <param name="mipmap"></param>
        public DrawTargetTextureCube(ICamera camera, int resolution, SurfaceFormat format, DepthFormat depthFormat, bool mipmap)
            : this(camera, resolution, format, depthFormat, mipmap, PreferredMultiSampleLevel.None)
        {
        }

        /// <summary>
        /// Creates the draw target cubemap. Note: Rendering resources will not be created until the first time the target is drawn or <see cref="Resource.Warm(IState)"/> is called
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="resolution"></param>
        /// <param name="format"></param>
        /// <param name="depthFormat"></param>
        /// <param name="mipmap"></param>
        /// <param name="multisample"></param>
        public DrawTargetTextureCube(ICamera camera, int resolution, SurfaceFormat format, DepthFormat depthFormat, bool mipmap, PreferredMultiSampleLevel multisample)
            : this(camera, resolution, format, depthFormat, mipmap, multisample, RenderTargetUsage.DiscardContents)
        {
        }

        /// <summary>
        /// Creates the draw target cubemap. Note: Rendering resources will not be created until the first time the target is drawn or <see cref="Resource.Warm(IState)"/> is called
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="resolution"></param>
        /// <param name="format"></param>
        /// <param name="mipmap"></param>
        /// <param name="multisample"></param>
        /// <param name="usage"></param>
        private DrawTargetTextureCube(ICamera camera, int resolution, SurfaceFormat format, bool mipmap, PreferredMultiSampleLevel multisample, RenderTargetUsage usage)
            :
            base(camera)
        {
            this.mipmap = mipmap;
            this.format = format;
            this.multisample = multisample;
            this.usage = usage;
            this.resolution = resolution;

            this.sizeAsVector = new Vector4((float)this.resolution, (float)this.resolution, 0, 0);
        }

        /// <summary>
        /// Creates the draw target cubemap. Note: Rendering resources will not be created until the first time the target is drawn or <see cref="Resource.Warm(IState)"/> is called
        /// </summary>
        public DrawTargetTextureCube(ICamera camera, int resolution, SurfaceFormat format, DepthFormat depthFormat, bool mipmap, PreferredMultiSampleLevel multisample, RenderTargetUsage usage)
            : this(camera, resolution, format, mipmap, multisample, usage)
        {
            this.depthFormat = depthFormat;

            if (this.depthFormat == DepthFormat.None)
            {
                this.ClearBuffer.ClearDepthEnabled = false;
                this.ClearBuffer.ClearStencilEnabled = false;
            }
            else
            {
                if (this.depthFormat != DepthFormat.Depth24Stencil8)
                    this.ClearBuffer.ClearStencilEnabled = false;
            }

            SetHasDepth();
        }

        /// <summary>
        /// Share the depth buffer created by this draw texture with another draw texture
        /// </summary>
        /// <param name="shareDepthTo"></param>
        public void ShareDepthBuffer(DrawTargetTextureCube shareDepthTo)
        {
            if (shareDepthTo == null || shareDepthTo == this)
                throw new ArgumentNullException();
            if (shareDepthTo.cloneOf != null ||
                cloneOf != null)
                throw new ArgumentException("Cannot share to/from clones");
            if (shareDepthTo.depthFormat != this.depthFormat)
                throw new ArgumentException("Depth format mismatch");
            if (this.Width < shareDepthTo.Width ||
                this.Height < shareDepthTo.Height)
                throw new ArgumentException("Size mismatch");
        }

        /// <summary>
        /// Create a draw target cubemap directly from an XNA render target (not recommended)
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="renderTexture"></param>
        /// <returns></returns>
        public static DrawTargetTextureCube CreateFromRenderTargetCube(ICamera camera, RenderTargetCube renderTexture)
        {
            return new DrawTargetTextureCube(camera, renderTexture);
        }

        private DrawTargetTextureCube(ICamera camera, RenderTargetCube texture)
            : base(camera)
        {
            if (texture == null)
                throw new ArgumentNullException();
            this.texture = texture;

            this.resolution = texture.Size;

            this.sizeAsVector = new Vector4((float)this.resolution, (float)this.resolution, 0, 0);

            SetHasDepth();
        }

        /// <summary>
        /// Dispose the draw target and all resources used. If this render target is a clone, the shared resources are NOT disposed
        /// </summary>
        public void Dispose()
        {
            isDisposed = true;

            if (cloneOf == null)
            {
                if (texture != null)
                    texture.Dispose();
            }
            this.texture = null;
        }

        /// <summary></summary>
        /// <param name="state"></param>
        protected internal override void Begin(DrawState state)
        {
        }

        /// <summary></summary>
        /// <param name="state"></param>
        /// <param name="repeat"></param>
        /// <param name="camera"></param>
        /// <returns></returns>
        internal override bool BeginRepeat(DrawState state, int repeat, ref ICamera camera)
        {
            if (isDisposed)
                throw new ObjectDisposedException("this");

            GraphicsDevice device = state.graphics;

            if (!facesEnabled[repeat])
                return false;

#if XBOX360
			state.nonScreenRenderComplete = true;
#endif

            if (repeat == minFaceEnabled)
            {
                if (texture == null)
                {
                    Warm(state);
                }

                if (texture.IsDisposed)
                    throw new ObjectDisposedException("RenderTexture");

                //state.shaderSystem.ResetTextures();
            }

            device.SetRenderTarget(texture, (CubeMapFace)repeat);

            if (cubeCamera == null)
                cubeCamera = new Camera3D(new Projection(MathHelper.PiOver2, 1, 100, 1));

            if (repeat == minFaceEnabled)
            {
                camera.GetCameraMatrix(out cubeCameraMatrix);

                if (camera is Camera3D)
                {
                    this.cubeCamera.Projection.NearClip = ((Camera3D)camera).Projection.NearClip;
                    this.cubeCamera.Projection.FarClip = ((Camera3D)camera).Projection.FarClip;
                    this.cubeCamera.Projection.UseLeftHandedProjection = true;// ((Camera3D)camera).Projection.UseLeftHandedProjection;
                    this.cubeCamera.ReverseBackfaceCulling = true;
                }
                else
                {
                    this.cubeCamera.Projection.NearClip = 1;
                    this.cubeCamera.Projection.FarClip = 100;
                    this.cubeCamera.Projection.UseLeftHandedProjection = false;
                }
            }

            Matrix view;
            Matrix.Multiply(ref CubeMapFaceMatrices[repeat], ref cubeCameraMatrix, out view);
            this.cubeCamera.SetCameraMatrix(ref view);

            camera = this.cubeCamera;
            return true;
        }

        /// <summary></summary>
        /// <param name="state"></param>
        /// <param name="repeat"></param>
        internal override void EndRepeat(DrawState state, int repeat)
        {
            if (texture.IsDisposed)
                throw new ObjectDisposedException("RenderTexture");
        }

        /// <summary></summary>
        /// <param name="state"></param>
        protected internal override void End(DrawState state)
        {
        }

        /// <summary>
        /// Gets the width of the draw target cubemap
        /// </summary>
        public override int Width
        {
            get { return resolution; }
        }

        /// <summary>
        /// Gets the width of the draw target cubemap
        /// </summary>
        public override int Height
        {
            get { return resolution; }
        }

        /// <summary></summary>
        internal protected override bool GetWidthHeightAsVector(out Vector4 size, ref int changeIndex)
        {
            size = sizeAsVector;
            bool result = changeIndex != this.sizeIndex;
            changeIndex = this.sizeIndex;
            return result;
        }

        /// <summary>
        /// Gets the width/height of the draw target as a Vector2
        /// </summary>
        public override Vector2 Size
        {
            get { return new Vector2(sizeAsVector.X, sizeAsVector.Y); }
        }

        /// <summary></summary>
        protected internal override bool HasDepthBuffer
        {
            get { return hasDepth && depthEnabled; }
        }

        /// <summary></summary>
        protected internal override bool HasStencilBuffer
        {
            get { return hasStencil && depthEnabled; }
        }

        internal override int GetAllocatedDeviceBytes()
        {
            if (cloneOf != null)
                return 0;
            int bytes = 0;
            if (texture != null)
                bytes += FormatSize(this.format) * Width * Height * 6;

            int depthSize = 0;
            switch (depthFormat)
            {
                case DepthFormat.Depth16:
                    depthSize = 2;
                    break;
                case DepthFormat.Depth24:
                case DepthFormat.Depth24Stencil8:
                    depthSize = 4;
                    break;
            }
            bytes += depthSize * Width * Height;
            return bytes;
        }

        internal override int GetAllocatedManagedBytes()
        {
            return 0;
        }

        internal override bool InUse
        {
            get { return texture != null; }
        }

        internal override bool IsDisposed
        {
            get
            {
                return isDisposed;
            }
        }

        internal override int GetRepeatCount()
        {
            return 6;
        }

        internal override void Warm(Application application, GraphicsDevice device)
        {
            if (cloneOf != null)
            {
                CloneRoot.Warm(application);
                this.texture = CloneRoot.texture;
                return;
            }

            if (!ownerRegistered)
            {
                ownerRegistered = true;
                application.Content.AddHighPriority(this);
            }

            if (texture == null)
            {
                this.texture = new RenderTargetCube(device, resolution, mipmap, format, depthFormat, (int)multisample, usage);
            }
        }

        void IContentOwner.LoadContent(ContentState state)
        {
            if (IsDisposed)
                return;

            GraphicsDevice device = state;

            if (texture != null)
            {
                if (cloneOf == null)
                {
                    texture.Dispose();
                }
                texture = null;
            }

            Warm(state);
        }
    }
}