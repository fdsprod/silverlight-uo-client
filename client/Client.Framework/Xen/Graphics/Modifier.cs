using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Xen.Graphics.Modifier
{
    /// <summary>
    /// A simple <see cref="IBeginEndDraw"/> drawable that modifies the <see cref="Xen.Graphics.RasterState.ColourWriteMask"/>, the previous mask is reset when rendering is complete.
    /// </summary>
#if !DEBUG_API

    [System.Diagnostics.DebuggerStepThrough]
#endif
    public sealed class ColourMaskModifier : IBeginEndDraw
    {
        private bool enabled;
        private bool currentlyEnabled;
        private bool red;
        private bool green;
        private bool blue;
        private bool alpha;
        private ColorWriteChannels previous;

        /// <summary>
        /// Gets/Sets if the alpha colour channel is written to the render target
        /// </summary>
        public bool Alpha
        {
            get { return alpha; }
            set { alpha = value; }
        }

        /// <summary>
        /// Gets/Sets if the blue colour channel is written to the render target
        /// </summary>
        public bool Blue
        {
            get { return blue; }
            set { blue = value; }
        }

        /// <summary>
        /// Gets/Sets if the green colour channel is written to the render target
        /// </summary>
        public bool Green
        {
            get { return green; }
            set { green = value; }
        }

        /// <summary>
        /// Gets/Sets if the red colour channel is written to the render target
        /// </summary>
        public bool Red
        {
            get { return red; }
            set { red = value; }
        }

        //public ColourMaskModifier(bool enabled, bool red, bool green, bool blue, bool alpha)
        //{
        //    this.enabled = enabled;
        //    this.red = red;
        //    this.green = green;
        //    this.blue = blue;
        //    this.alpha = alpha;
        //}

        /// <summary>
        /// Construct the mask
        /// </summary>
        /// <param name="red"></param>
        /// <param name="green"></param>
        /// <param name="blue"></param>
        /// <param name="alpha"></param>
        public ColourMaskModifier(bool red, bool green, bool blue, bool alpha)
        {
            this.enabled = true;
            this.red = red;
            this.green = green;
            this.blue = blue;
            this.alpha = alpha;
        }

        /// <summary>
        /// Construct the mask (full mask, all channels drawn)
        /// </summary>
        public ColourMaskModifier()
            : this(true, true, true, true)
        {
        }

        /// <summary>
        /// Gets/Sets if this modified is enabled
        /// </summary>
        public bool Enabled
        {
            get
            { return enabled; }
            set
            { enabled = value; }
        }

        void IBeginEndDraw.Begin(DrawState state)
        {
            if (enabled)
            {
                previous = state.RenderState.CurrentRasterState.ColourWriteMask;

                ColorWriteChannels c = 0;
                if (red)
                    c |= ColorWriteChannels.Red;
                if (green)
                    c |= ColorWriteChannels.Green;
                if (blue)
                    c |= ColorWriteChannels.Blue;
                if (alpha)
                    c |= ColorWriteChannels.Alpha;

                state.RenderState.CurrentRasterState.ColourWriteMask = c;
                currentlyEnabled = true;
            }
        }

        void IBeginEndDraw.End(DrawState state)
        {
            if (currentlyEnabled)
                state.RenderState.CurrentRasterState.ColourWriteMask = previous;
            currentlyEnabled = false;
        }
    }

    /// <summary>
    /// A simple <see cref="IBeginEndDraw"/> drawable that modifies the <see cref="GraphicsDevice.Viewport"/>, the previous viewport is reset when rendering is complete.
    /// </summary>
#if !DEBUG_API

    [System.Diagnostics.DebuggerStepThrough]
#endif
    public sealed class ViewportModifier : IBeginEndDraw
    {
        private float left, right, top, bottom;
        private float maxDepth = 1;
        private float minDepth = 0;
        private Viewport previous;
        private bool enabled = true, currentlyEnabled, currentlyActive;

        /// <summary>
        /// Construct the viewport, sized by an existing draw target
        /// </summary>
        /// <param name="left"></param>
        /// <param name="top"></param>
        /// <param name="right"></param>
        /// <param name="bottom"></param>
        /// <param name="targetSize"></param>
        public ViewportModifier(int left, int top, int right, int bottom, DrawTarget targetSize)
            : this((left) / (float)targetSize.Width, (top) / (float)targetSize.Height, (right) / (float)targetSize.Width, (bottom) / (float)targetSize.Height)
        {
        }

        /// <summary>
        /// Construct the viewport, all sizes are in [0,1] range
        /// </summary>
        /// <param name="left"></param>
        /// <param name="top"></param>
        /// <param name="right"></param>
        /// <param name="bottom"></param>
        public ViewportModifier(float left, float top, float right, float bottom)
        {
            SetViewport(left, top, right, bottom);
        }

        /// <summary>
        /// Construct the viewport, all sizes are in [0,1] range
        /// </summary>
        /// <param name="topLeft"></param>
        /// <param name="bottomRight"></param>
        public ViewportModifier(Microsoft.Xna.Framework.Vector2 topLeft, Microsoft.Xna.Framework.Vector2 bottomRight)
            :
            this(topLeft.X, topLeft.Y, bottomRight.X, bottomRight.Y)
        {
        }

        /// <summary>
        /// Gets/Sets if this modified is enabled
        /// </summary>
        public bool Enabled
        {
            get { return enabled; }
            set { enabled = value; }
        }

        /// <summary>
        /// Gets/Sets the viewport minimum depth
        /// </summary>
        public float MinDepth
        {
            get { return minDepth; }
            set { minDepth = value; }
        }

        /// <summary>
        /// Gets/Sets the viewport maximum depth
        /// </summary>
        public float MaxDepth
        {
            get { return maxDepth; }
            set { maxDepth = value; }
        }

        /// <summary>
        /// Gets/Sets the left edge of the viewport
        /// </summary>
        public float Left { get { return left; } set { Validate(value, right, top, bottom); left = value; } }

        /// <summary>
        /// Gets/Sets the right edge of the viewport
        /// </summary>
        public float Right { get { return right; } set { Validate(left, value, top, bottom); right = value; } }

        /// <summary>
        /// Gets/Sets the top edge of the viewport
        /// </summary>
        public float Top { get { return top; } set { Validate(left, right, value, bottom); top = value; } }

        /// <summary>
        /// Gets/Sets the bottom edge of the viewport
        /// </summary>
        public float Bottom { get { return bottom; } set { Validate(left, right, top, value); bottom = value; } }

        /// <summary>
        /// Gets/Sets the width of the viewport
        /// </summary>
        public float Width { get { return right - left; } set { Validate(left, left + value, top, bottom); right = left + value; } }

        /// <summary>
        /// Gets/Sets the height of the viewport
        /// </summary>
        public float Height { get { return bottom - top; } set { Validate(left, right, top, top + value); bottom = top + value; } }

        /// <summary>
        /// Gets/Sets the top left corner of the viewport
        /// </summary>
        public Microsoft.Xna.Framework.Vector2 TopLeft
        {
            get { return new Microsoft.Xna.Framework.Vector2(left, top); }
            set { Validate(value.X, right, value.Y, bottom); left = value.X; top = value.Y; }
        }

        /// <summary>
        /// Gets/Sets the bottom right corner of the viewport
        /// </summary>
        public Microsoft.Xna.Framework.Vector2 BottomRight
        {
            get { return new Microsoft.Xna.Framework.Vector2(right, bottom); }
            set { Validate(left, value.X, top, value.Y); right = value.X; bottom = value.Y; }
        }

        /// <summary>
        /// Set the size of the viewport rectangle
        /// </summary>
        public void SetViewport(float left, float top, float right, float bottom)
        {
            Validate(left, right, top, bottom);
            this.left = left;
            this.right = right;
            this.top = top;
            this.bottom = bottom;
        }

        /// <summary>Set the viewport</summary>
        public void Begin(DrawState state)
        {
            if (currentlyActive)
                throw new InvalidOperationException("This viewport modified is already in use");
            currentlyActive = true;
            if (enabled)
            {
                GraphicsDevice device = state.graphics;
                previous = device.Viewport;

                Viewport vp = new Viewport();
                int x = state.DrawTarget.Width, y = state.DrawTarget.Height;
                vp.MaxDepth = maxDepth;
                vp.MinDepth = minDepth;
                vp.X = (int)(left * x + 0.5f);
                vp.Y = (int)(top * y + 0.5f);
                vp.Width = (int)(Width * x + 0.5f);
                vp.Height = (int)(Height * y + 0.5f);
                device.Viewport = vp;
                currentlyEnabled = true;
            }
        }

        /// <summary>
        /// Reset the viewport
        /// </summary>
        void IBeginEndDraw.End(DrawState state)
        {
            if (currentlyEnabled)
            {
                GraphicsDevice device = state.graphics;
                device.Viewport = previous;
            }
            currentlyEnabled = false;
            currentlyActive = false;
        }

        private void Validate(float l, float r, float t, float b)
        {
            const float zero = -1.0f / 32768.0f;
            const float one = 1 + 1.0f / 32768.0f;

            if (l >= r || t >= b)
                throw new ArgumentException("Size is zero or negative");
            if ((l > one || l < zero) || (r > one || r < zero) || (t > one || t < zero) || (b > one || b < zero))
                throw new ArgumentException("Arguement out of zero-one range");
        }
    }

    /// <summary>
    /// A simple <see cref="IBeginEndDraw"/> drawable that modifies the <see cref="GraphicsDevice.ScissorRectangle"/>, the previous scissor rect is reset when rendering is complete.
    /// </summary>
#if !DEBUG_API

    [System.Diagnostics.DebuggerStepThrough]
#endif
    public sealed class ScissorModifier : IBeginEndDraw
    {
        private float left, right, top, bottom;
        private bool enabled = true, currentlyActive, currentlyEnabled;
        private Microsoft.Xna.Framework.Rectangle previousRect;

        /// <summary>
        /// Construct the scissor rectangle, based on the size of an existing draw target
        /// </summary>
        /// <param name="left"></param>
        /// <param name="top"></param>
        /// <param name="right"></param>
        /// <param name="bottom"></param>
        /// <param name="targetSize"></param>
        public ScissorModifier(int left, int top, int right, int bottom, DrawTarget targetSize)
            : this((left) / (float)targetSize.Width, (top) / (float)targetSize.Height, (right) / (float)targetSize.Width, (bottom) / (float)targetSize.Height)
        {
        }

        /// <summary>
        /// Construct the scissor rectangle. Sizes are in the range [0,1]
        /// </summary>
        /// <param name="left"></param>
        /// <param name="top"></param>
        /// <param name="right"></param>
        /// <param name="bottom"></param>
        public ScissorModifier(float left, float top, float right, float bottom)
        {
            SetScissorRectangle(left, top, right, bottom);
        }

        /// <summary>
        /// Construct the scissor rectangle. Sizes are in the range [0,1]
        /// </summary>
        /// <param name="bottomRight"></param>
        /// <param name="topLeft"></param>
        public ScissorModifier(Microsoft.Xna.Framework.Vector2 topLeft, Microsoft.Xna.Framework.Vector2 bottomRight)
            :
            this(topLeft.X, topLeft.Y, bottomRight.X, bottomRight.Y)
        {
        }

        /// <summary>
        /// Gets/Sets if the modified is enabled
        /// </summary>
        public bool Enabled
        {
            get { return enabled; }
            set { enabled = value; }
        }

        /// <summary>
        /// Gets/Sets the left edge of the scissor rectangle
        /// </summary>
        public float Left { get { return left; } set { Validate(value, right, top, bottom); left = value; } }

        /// <summary>
        /// Gets/Sets the right edge of the scissor rectangle
        /// </summary>
        public float Right { get { return right; } set { Validate(left, value, top, bottom); right = value; } }

        /// <summary>
        /// Gets/Sets the top edge of the scissor rectangle
        /// </summary>
        public float Top { get { return top; } set { Validate(left, right, value, bottom); top = value; } }

        /// <summary>
        /// Gets/Sets the bottom edge of the scissor rectangle
        /// </summary>
        public float Bottom { get { return bottom; } set { Validate(left, right, top, value); bottom = value; } }

        /// <summary>
        /// Gets/Sets the width of the scissor rectangle
        /// </summary>
        public float Width { get { return right - left; } set { Validate(left, left + value, top, bottom); right = left + value; } }

        /// <summary>
        /// Gets/Sets the height of the scissor rectangle
        /// </summary>
        public float Height { get { return bottom - top; } set { Validate(left, right, top, top + value); bottom = top + value; } }

        /// <summary>
        /// Gets/Sets the top left corner of the scissor rectangle
        /// </summary>
        public Microsoft.Xna.Framework.Vector2 TopLeft
        {
            get { return new Microsoft.Xna.Framework.Vector2(left, top); }
            set { Validate(value.X, right, value.Y, bottom); left = value.X; top = value.Y; }
        }

        /// <summary>
        /// Gets/Sets the bottom right corner of the scissor rectangle
        /// </summary>
        public Microsoft.Xna.Framework.Vector2 BottomRight
        {
            get { return new Microsoft.Xna.Framework.Vector2(right, bottom); }
            set { Validate(left, value.X, top, value.Y); right = value.X; bottom = value.Y; }
        }

        /// <summary>
        /// Set the scissor rectangle
        /// </summary>
        public void SetScissorRectangle(float left, float top, float right, float bottom)
        {
            Validate(left, right, top, bottom);
            this.left = left;
            this.right = right;
            this.top = top;
            this.bottom = bottom;
        }

        /// <summary>
        /// Set the scissor rectangle
        /// </summary>
        public void Begin(DrawState state)
        {
            if (currentlyActive)
                throw new InvalidOperationException("This scissor rectangle modified is already in use");
            currentlyActive = true;
            if (enabled)
            {
                GraphicsDevice device = state.graphics;
                previousRect = device.ScissorRectangle;
                state.renderState.Push();
                if (left != 0 || right != 1 || top != 0 || bottom != 1)
                {
                    int x = state.DrawTarget.Width, y = state.DrawTarget.Height;

                    Microsoft.Xna.Framework.Rectangle rect
                        = new Rectangle(
                            (int)(left * x + 0.5f),
                            (int)(top * y + 0.5f),
                            (int)(Width * x + 0.5f),
                            (int)(Height * y + 0.5f));

                    device.ScissorRectangle = rect;
                    state.renderState.CurrentRasterState.ScissorTestEnable = true;
                }
                else
                    state.renderState.CurrentRasterState.ScissorTestEnable = false;
                currentlyEnabled = true;
            }
        }

        /// <summary>
        /// Reset the scissor rectangle
        /// </summary>
        public void End(DrawState state)
        {
            if (currentlyEnabled)
            {
                state.renderState.Pop();
                GraphicsDevice device = state.graphics;
                device.ScissorRectangle = previousRect;
            }
            currentlyActive = false;
            currentlyEnabled = false;
        }

        private void Validate(float l, float r, float t, float b)
        {
            const float zero = -1.0f / 32768.0f;
            const float one = 1 + 1.0f / 32768.0f;

            if (l >= r || t >= b)
                throw new ArgumentException("Size is zero or negative");
            if ((l > one || l < zero) || (r > one || r < zero) || (t > one || t < zero) || (b > one || b < zero))
                throw new ArgumentException("Arguement out of zero-one range");
        }
    }

    /// <summary>
    /// <para>A simple <see cref="IBeginEndDraw"/> and <see cref="IDraw"/> drawable that clears the colour,depth and stencil buffers.</para>
    /// <para>Note: All <see cref="DrawTarget"/> classes include an instance of ClearBufferModifer, see <see cref="DrawTarget.ClearBuffer"/></para>
    /// </summary>
#if !DEBUG_API

    [System.Diagnostics.DebuggerStepThrough]
#endif
    public sealed class ClearBufferModifier : IBeginEndDraw, IDraw
    {
        private Color colour;
        private byte stencil = 0;
        private float depth = 1;
        private bool clearColour = true;
        private bool clearDepth = true;
        private bool clearStencil = true;
        private bool enabled = true;
        private bool firstPassClear = false;

        /// <summary>
        /// Construct the clear buffer modifier, clearing all buffers
        /// </summary>
        public ClearBufferModifier()
        {
        }

        internal ClearBufferModifier(bool firstPassClear)
        {
            this.firstPassClear = firstPassClear;
            colour = new Color(0, 0, 0, 0);
        }

        /// <summary>
        /// Construct the clear buffer modifier, clearing all buffers
        /// </summary>
        /// <param name="colourValue"></param>
        public ClearBufferModifier(Color colourValue)
            : this(ClearOptions.Target | ClearOptions.DepthBuffer | ClearOptions.Stencil, colourValue, 1, 0)
        {
        }

        /// <summary>
        /// Construct the clear buffer modifier
        /// </summary>
        /// <param name="clearOptions"></param>
        public ClearBufferModifier(ClearOptions clearOptions)
            : this(clearOptions, Color.CornflowerBlue, 1, 0)
        {
        }

        /// <summary>
        /// Construct the clear buffer modifier
        /// </summary>
        /// <param name="clearOptions"></param>
        /// <param name="colourValue">Colour to clear the render target</param>
        public ClearBufferModifier(ClearOptions clearOptions, Color colourValue)
            : this(clearOptions, colourValue, 1, 0)
        {
        }

        /// <summary>
        /// Construct the clear buffer modifier, clearing all buffers
        /// </summary>
        /// <param name="clearOptions"></param>
        /// <param name="colourValue">colour to clear the render target</param>
        /// <param name="depthValue">Depth to clear the render target (usuall 1.0f)</param>
        /// <param name="stencilValue">Stencil value to clear the stencil buffer to</param>
        public ClearBufferModifier(ClearOptions clearOptions, Color colourValue, float depthValue, byte stencilValue)
        {
            this.ClearColourEnabled = (clearOptions & ClearOptions.Target) == ClearOptions.Target;
            this.ClearColour = colourValue;

            this.ClearStencilEnabled = (clearOptions & ClearOptions.Stencil) == ClearOptions.Stencil;
            this.ClearStencilValue = stencilValue;

            this.ClearDepthEnabled = (clearOptions & ClearOptions.DepthBuffer) == ClearOptions.DepthBuffer;
            this.ClearDepth = depthValue;
        }

        /// <summary>
        /// <para>/Sets if clearing the stencil buffer is enabled</para>
        /// <para>(NOTE: Stencil is always cleared on XBOX360 when changing draw target)</para>
        /// </summary>
        public bool ClearStencilEnabled
        {
            get { return clearStencil; }
            set { clearStencil = value; }
        }

        /// <summary>
        /// <para>Gets/Sets if clearing the depth buffer is enabled</para>
        /// <para>(NOTE: Depth is cleared on XBOX360 when changing draw target, unless the draw target <see cref="RenderTargetUsage"/> is set to <see cref="RenderTargetUsage.PreserveContents"/>)</para>
        /// </summary>
        /// <seealso cref="DrawTargetTexture2D"/>
        /// <seealso cref="DrawTargetTextureCube"/>
        /// <seealso cref="Application.SetupGraphicsDeviceManager"/>
        public bool ClearDepthEnabled
        {
            get { return clearDepth; }
            set { clearDepth = value; }
        }

        /// <summary>
        /// <para>Gets/Sets if clearing the colour buffer is enabled</para>
        /// <para>(NOTE: Colour is cleared on XBOX360 when changing draw target, unless the draw target <see cref="RenderTargetUsage"/> is set to <see cref="RenderTargetUsage.PreserveContents"/>)</para>
        /// </summary>
        /// <seealso cref="DrawTargetTexture2D"/>
        /// <seealso cref="DrawTargetTextureCube"/>
        /// <seealso cref="Application.SetupGraphicsDeviceManager"/>
        public bool ClearColourEnabled
        {
            get { return clearColour; }
            set { clearColour = value; }
        }

        /// <summary>
        /// Gets/Sets if value used to clear the depth buffer if <see cref="ClearDepthEnabled"/> is true (The depth buffer is usually best cleared to 1.0f)
        /// </summary>
        public float ClearDepth
        {
            get { return depth; }
            set { if (value > 1 || value < 0) throw new ArgumentException("Valid depth range is 0-1 (depth is usually cleared to 1)"); depth = value; }
        }

        /// <summary>
        /// Gets/Sets if colour used to clear the colour buffer if <see cref="ClearColourEnabled"/> is true
        /// </summary>
        public Color ClearColour
        {
            get { return colour; }
            set { colour = value; }
        }

        /// <summary>
        /// Gets/Sets if value used to clear the stencil buffer if <see cref="ClearStencilEnabled"/> is true
        /// </summary>
        public byte ClearStencilValue
        {
            get { return stencil; }
            set { stencil = value; }
        }

        void IBeginEndDraw.Begin(DrawState state)
        {
            if (state.DrawTarget == null)
                throw new ArgumentNullException("DrawTarget");

            float depth = this.depth;
            byte stencil = this.stencil;
            Color colour = this.colour;

            GraphicsDevice device = state.graphics;

            if (firstPassClear && clearStencil && clearDepth)
            {
                clearDepth &= state.DrawTarget.HasDepthBuffer;
                clearStencil &= state.DrawTarget.HasStencilBuffer;
            }

            if (clearDepth && state.DrawTarget != null && !state.DrawTarget.HasDepthBuffer)
                throw new ArgumentException("Cannot clear depth, current render target does not have a depth buffer");
            if (clearStencil && state.DrawTarget != null && !state.DrawTarget.HasStencilBuffer)
                throw new ArgumentException("Cannot clear stencil, current render target does not have a stencil buffer");

            ClearOptions op = 0;
            if (clearColour)
            {
                op |= ClearOptions.Target;
#if DEBUG
                System.Threading.Interlocked.Increment(ref state.Application.currentFrame.BufferClearTargetCount);
#endif
            }
            if (clearDepth)
            {
                op |= ClearOptions.DepthBuffer;
#if DEBUG
                System.Threading.Interlocked.Increment(ref state.Application.currentFrame.BufferClearDepthCount);
#endif
            }
            if (clearStencil)
            {
                op |= ClearOptions.Stencil;
#if DEBUG
                System.Threading.Interlocked.Increment(ref state.Application.currentFrame.BufferClearStencilCount);
#endif
            }

            //if true, this clear buffer is tied directly to a drawtarget
            if (firstPassClear)
            {
                if (state.DrawTarget == null)
                    return;

                bool isScreen = state.DrawTarget is DrawTargetScreen;

                RenderTargetUsage usage = RenderTargetUsage.PlatformContents;
                if (isScreen)
                    usage = device.PresentationParameters.RenderTargetUsage;
                if (state.DrawTarget is DrawTargetTexture2D)
                    usage = ((DrawTargetTexture2D)state.DrawTarget).GetRenderTarget2D().RenderTargetUsage;
                if (state.DrawTarget is DrawTargetTextureCube)
                    usage = ((DrawTargetTextureCube)state.DrawTarget).GetRenderTargetCube().RenderTargetUsage;
                if (state.DrawTarget is DrawTargetTexture2DGroup && ((DrawTargetTexture2DGroup)state.DrawTarget).Count > 0)
                    usage = ((DrawTargetTexture2DGroup)state.DrawTarget).GetTarget(0).GetRenderTarget2D().RenderTargetUsage;

#if XBOX360
				if (usage == RenderTargetUsage.PlatformContents)
					usage = RenderTargetUsage.DiscardContents;

#else
                if (usage == RenderTargetUsage.PlatformContents)
                    usage = RenderTargetUsage.PreserveContents;
#endif

                bool skipDepthClearOptimize = false;

#if !XBOX360
                //for some reason, even with DiscardContents, on windows the screen depth/stencil is not always cleared
                skipDepthClearOptimize = isScreen;
#else
				skipDepthClearOptimize = !state.nonScreenRenderComplete;
#endif

                if (!skipDepthClearOptimize && usage == RenderTargetUsage.DiscardContents && depth == 1 && stencil == 0)
                {
                    //no need to clear depth buffer, xna already did it
                    op &= ClearOptions.Target;
                }

#if !XBOX360

                //force a depth clear on the screen when using DiscardContents on windows
                if (skipDepthClearOptimize && usage == RenderTargetUsage.DiscardContents)
                {
                    if (state.DrawTarget != null && state.DrawTarget.HasDepthBuffer)
                        op |= ClearOptions.DepthBuffer;
                    if (state.DrawTarget != null && state.DrawTarget.HasStencilBuffer)
                        op |= ClearOptions.Stencil;

                    if (!this.ClearDepthEnabled)
                        depth = 1;
                    if (!this.ClearStencilEnabled)
                        stencil = 0;
                }

#else

				//if for some insane reason the application actually wants butt ugly purple..
				//then save a clear call :-)
				if (colour == new Color(128,0,255,0) && usage == RenderTargetUsage.DiscardContents)
				{
					//no need to clear target either, xbox already did it
					op &= ~ClearOptions.Target;
				}
#endif

                if (op == 0)
                    return;
            }

            //device.DepthStencilBuffer leaks
            //#if DEBUG
            //            if ((clearDepth | clearStencil) && (device.DepthStencilBuffer == null))
            //                throw new ArgumentException("XNA DepthBuffer mismatch error (XNA internal bug...) :-(");
            //#endif

            device.Clear(op, colour, depth, stencil);
        }

        void IBeginEndDraw.End(DrawState state)
        {
        }

        /// <summary>
        /// Gets/Sets if clearing the buffer is enabled
        /// </summary>
        public bool Enabled
        {
            get { return enabled; }
            set { enabled = value; }
        }

        bool ICullable.CullTest(ICuller culler)
        {
            return enabled;
        }

        /// <summary>
        /// Clear the buffers
        /// </summary>
        /// <param name="state"></param>
        public void Draw(DrawState state)
        {
            ((IBeginEndDraw)this).Begin(state);
        }
    }
}