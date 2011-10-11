using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Xen.Camera;
using Xen.Graphics;
using Xen.Graphics.Stack;

namespace Xen
{
    namespace Graphics
    {
        /// <summary>
        /// A class for exposing properties of the DrawState class that may be less useful, but still desirable.
        /// </summary>
        public sealed class DrawStateProperties : IContentUnload
        {
            private readonly DrawState state;

            internal DrawStateProperties(DrawState state)
            {
                this.state = state;
                state.Application.Content.Add(this);
            }

            /// <summary>
            /// Gets the current View Matrix
            /// </summary>
            public MatrixSource ViewMatrix { get { return state.matrices.View; } }

            /// <summary>
            /// Gets the current Projection Matrix
            /// </summary>
            public MatrixSource ProjectionMatrix { get { return state.matrices.Projection; } }

            /// <summary>
            /// Gets the current View Projection Matrix
            /// </summary>
            public MatrixSource ViewProjectionMatrix { get { return state.matrices.ViewProjection; } }

            /// <summary>
            /// Gets the current World Matrix
            /// </summary>
            public MatrixSource WorldMatrix { get { return state.matrices.World; } }

            /// <summary>
            /// Gets the current World Projection Matrix
            /// </summary>
            public MatrixSource WorldProjectionMatrix { get { return state.matrices.WorldProjection; } }

            /// <summary>
            /// Gets the current World View Matrix
            /// </summary>
            public MatrixSource WorldViewMatrix { get { return state.matrices.WorldView; } }

            /// <summary>
            /// Gets the current World View Projection Matrix
            /// </summary>
            public MatrixSource WorldViewProjectionMatrix { get { return state.matrices.WorldViewProjection; } }

            /// <summary>
            /// Hardware supports Shader Model 3 hardware instancing, or is running on the xbox360
            /// </summary>
            public bool SupportsHardwareInstancing
            {
                get { return state.Application.SupportsHardwareInstancing; }
            }

            /// <summary>Gets the frame index (incremented every time <see cref="Xen.Application.Frame(FrameState)"/> is called)</summary>
            public int FrameIndex
            {
                get { return (state as ICuller).FrameIndex; }
            }

            private Texture2D white, normal, black;

            /// <summary>
            /// A generic texture that is solid white
            /// </summary>
            public Texture2D WhiteTexture
            {
                get { return white; }
            }

            /// <summary>
            /// A generic texture that is solid RGB 0.5,0.5,1.0 - which can be used as a stand in for a normal map
            /// </summary>
            public Texture2D FlatNormalMapTexture
            {
                get { return normal; }
            }

            /// <summary>
            /// A generic texture that is solid black
            /// </summary>
            public Texture2D BlackTexture
            {
                get { return black; }
            }

            void IContentOwner.LoadContent(ContentState state)
            {
                white = MakeTexture(Color.White, state);
                normal = MakeTexture(new Color(127, 127, 255), state);
                black = MakeTexture(Color.Black, state);
            }

            private Texture2D MakeTexture(Color colour, ContentState state)
            {
                Color[] colours = new Color[]
				{
					colour,colour,colour,colour
				};

                Texture2D texture = new Texture2D(state, 2, 2, false, SurfaceFormat.Color);
                texture.SetData(colours);
                return texture;
            }

            void IContentUnload.UnloadContent()
            {
                white.Dispose(); white = null;
                normal.Dispose(); normal = null;
                black.Dispose(); black = null;
            }
        }
    }

    sealed partial class DrawState
    {
        /// <summary>Helper method used to wrap a Push method</summary>
        //public static Graphics.Stack.ShaderStack.UsingPop operator +(DrawState state, Graphics.IShader shader)
        //{
        //    return state.shaderStack.Push(shader);
        //}
        ///// <summary>Helper method used to wrap a Push method</summary>
        //public static Graphics.Stack.ShaderStack.UsingPop operator +(DrawState state, Effect effect)
        //{
        //    return state.shaderStack.Push(effect);
        //}
        /// <summary>Helper method used to wrap a Push method</summary>
        public static Graphics.Stack.DeviceRenderStateStack.UsingPop operator +(DrawState state, DeviceRenderState renderState)
        {
            return state.renderState.Push(ref renderState);
        }

        /// <summary>Helper method used to wrap a Push method</summary>
        public static Graphics.Stack.DeviceRenderStateStack.UsingPop operator +(DrawState state, AlphaBlendState renderState)
        {
            return state.renderState.Push(ref renderState);
        }

        /// <summary>Helper method used to wrap a Push method</summary>
        public static Graphics.Stack.DeviceRenderStateStack.UsingPop operator +(DrawState state, StencilState renderState)
        {
            return state.renderState.Push(ref renderState);
        }

        /// <summary>Helper method used to wrap a Push method</summary>
        public static Graphics.Stack.DeviceRenderStateStack.UsingPop operator +(DrawState state, DepthState renderState)
        {
            return state.renderState.Push(ref renderState);
        }

        /// <summary>Helper method used to wrap a Push method</summary>
        public static Graphics.Stack.DeviceRenderStateStack.UsingPop operator +(DrawState state, RasterState renderState)
        {
            return state.renderState.Push(ref renderState);
        }

        /// <summary>Helper method used to wrap a Push method</summary>
        public static Graphics.Stack.CameraStack.UsingPop operator +(DrawState state, ICamera camera)
        {
            return state.cameraStack.Push(camera);
        }

        /// <summary>Helper method used to wrap a Push method</summary>
        public static Graphics.Stack.WorldMatrixStackProvider.UsingPop operator +(DrawState state, Matrix world)
        {
            return state.matrices.World.Push(ref world);
        }

        /// <summary>Helper method used to wrap a PushMultiply method</summary>
        public static Graphics.Stack.WorldMatrixStackProvider.UsingPop operator *(DrawState state, Matrix world)
        {
            return state.matrices.World.PushMultiply(ref world);
        }

        /// <summary>Helper method used to wrap a Push method</summary>
        public static Graphics.Stack.WorldMatrixStackProvider.UsingPop operator +(DrawState state, Vector3 pos)
        {
            return state.matrices.World.PushTranslate(ref pos);
        }

        /// <summary>Helper method used to wrap a PushMultiply method</summary>
        public static Graphics.Stack.WorldMatrixStackProvider.UsingPop operator *(DrawState state, Vector3 pos)
        {
            return state.matrices.World.PushTranslateMultiply(ref pos);
        }

        /// <summary>Helper method used to wrap a IBeginEndDraw begin/end draw object</summary>
        public static UsingPopBeginEndDraw operator +(DrawState state, IBeginEndDraw draw)
        {
            return new UsingPopBeginEndDraw(draw, state);
        }

        /// <summary>
        /// Structure used for a using block with a + operator IBeginEndDraw object
        /// </summary>
        [System.Diagnostics.DebuggerStepThrough]
        public struct UsingPopBeginEndDraw : IDisposable
        {
            internal UsingPopBeginEndDraw(IBeginEndDraw draw, DrawState state)
            {
                this.draw = draw;
                this.state = state;
                draw.Begin(state);
            }

            private readonly IBeginEndDraw draw;
            private readonly DrawState state;

            /// <summary>Invokes the Pop metohd</summary>
            public void Dispose()
            {
                draw.End(state);
            }
        }

        private IndexBuffer indexBuffer;

        //Shader system members:
        //internal Xen.Graphics.ShaderSystemState shaderSystem;
        //internal readonly Graphics.Stack.ShaderStack shaderStack;
        internal readonly Graphics.Stack.DeviceRenderStateStack renderState;

        private readonly Graphics.Stack.DrawFlagStack flagStack;
        private Graphics.DrawStateProperties properties;

#if XBOX360
		/// <summary>
		///<para>there is a bug in XNA with render target clearing on the 360</para>
		///<para>the screen doesn't obey the RenderTargetUsage option when you *only* render to the screen</para>
		///<para>so keep track if a texture has been rendered to yet</para>
		/// </summary>
		internal bool nonScreenRenderComplete;
#endif

        /// <summary>
        /// A collection of useful extnension properties and state objects
        /// </summary>
        public Graphics.DrawStateProperties Properties
        {
            get
            {
                return properties;
            }
        }

        /// <summary>
        /// Gets the rendering shader stack, use this to assign shaders
        /// </summary>
        //public Graphics.Stack.ShaderStack Shader
        //{
        //    get
        //    {
        //        return shaderStack;
        //    }
        //}

        /// <summary>
        /// Gets the draw flag stack
        /// </summary>
        public Graphics.Stack.DrawFlagStack DrawFlags
        {
            get
            {
                return flagStack;
            }
        }

        /// <summary>
        /// Gets the shader system global interface
        /// </summary>
        //public Graphics.IShaderGlobals ShaderGlobals
        //{
        //    get { return shaderSystem; }
        //}

        internal IndexBuffer IndexBuffer
        {
            get { return indexBuffer; }
            set
            {
                if (indexBuffer != value)
                {
                    indexBuffer = value;
                    graphics.Indices = value;
                }
            }
        }

        /// <summary>
        /// Gets the current <see cref="DeviceRenderState"/>. Members of this instance can be directly modified. Always push/pop the render state when drawing. To set the entire render state, see RenderState.Push or RenderState.Set
        /// </summary>
        /// <remarks><para>It is highly recommended that changes to render state are doen through this instance, rather than directly through the GraphicsDevice.</para>
        /// <para>If render state is changed through the GraphicsDevice, then the internal state cache will become invalid, and must be reset with a call to <see cref="DirtyInternalRenderState"/>, with the appropriate flags set indicating what parts of the render state have changed</para></remarks>
        public Graphics.Stack.DeviceRenderStateStack RenderState
        {
            get
            {
                return renderState;
            }
        }

        //internal void EndFrameCleanup()
        //{
        //    //set a null effect.
        //    Xen.Graphics.ShaderSystem.ShaderEffect effect = new Xen.Graphics.ShaderSystem.ShaderEffect();
        //    shaderSystem.SetEffect(null, ref effect, Xen.Graphics.ShaderSystem.ShaderExtension.None);
        //}

        //internal void DirtyInternalRenderState(StateFlag dirtyState)
        //{
        //    this.shaderSystem.DirtyInternalRenderState(dirtyState);
        //}

        //        // internal copies of DrawPrimitives

        //        internal void DrawIndexedPrimitives(
        //            int rawVertexCount,
        //            IndexBuffer indices,
        //            int indexCount,
        //            PrimitiveType primitiveType,
        //            int baseVertex,
        //            int minVertexIndex,
        //            int numVertices,
        //            int startIndex,
        //            int primitiveCount,
        //            Graphics.AnimationTransformArray animationArray,
        //            Graphics.ShaderSystem.ShaderExtension extension,
        //            Type vertexType,
        //            VertexDeclaration declaration,
        //            int instanceCount)
        //        {
        //#if DEBUG
        //            ValidateRenderState();
        //#endif

        //            if (this.indexBuffer != indices)
        //            {
        //                this.indexBuffer = indices;
        //                this.graphics.Indices = indices;
        //            }

        //            if (startIndex < 0 || startIndex > indexCount)
        //                throw new ArgumentException("startIndex");

        //            if (primitiveCount < 0)
        //            {
        //                switch (primitiveType)
        //                {
        //                    case PrimitiveType.LineList:
        //                        primitiveCount *= -((indexCount - startIndex) / 2);
        //                        break;
        //                    case PrimitiveType.LineStrip:
        //                        primitiveCount *= -((indexCount - startIndex) - 1);
        //                        break;
        //                    case PrimitiveType.TriangleList:
        //                        primitiveCount *= -((indexCount - startIndex) / 3);
        //                        break;
        //                    case PrimitiveType.TriangleStrip:
        //                        primitiveCount *= -((indexCount - startIndex) - 2);
        //                        break;
        //                }
        //            }
        //            else
        //            {
        //                int vertCount = 0;
        //                switch (primitiveType)
        //                {
        //                    case PrimitiveType.LineList:
        //                        vertCount = primitiveCount * 2;
        //                        break;
        //                    case PrimitiveType.LineStrip:
        //                        vertCount = primitiveCount + 1;
        //                        break;
        //                    case PrimitiveType.TriangleList:
        //                        vertCount = primitiveCount * 3;
        //                        break;
        //                    case PrimitiveType.TriangleStrip:
        //                        vertCount = primitiveCount + 2;
        //                        break;
        //                }
        //#if XBOX360
        //                //don't do this check when instancing on the 360
        //                if (extension != Xen.Graphics.ShaderSystem.ShaderExtension.Instancing)
        //#endif
        //                if (startIndex + vertCount > indexCount)
        //                    throw new ArgumentException("primtiveCount or startIndex is out of range");
        //            }

        //            renderState.ApplyRenderStateChanges(rawVertexCount, animationArray, extension);

        //#if DEBUG
        //            shaderSystem.ValidateVertexDeclarationForShader(declaration, vertexType);

        //            System.Threading.Interlocked.Increment(ref application.currentFrame.DrawIndexedPrimitiveCallCount);

        //            switch (primitiveType)
        //            {
        //                case PrimitiveType.LineList:
        //                case PrimitiveType.LineStrip:
        //                    application.currentFrame.LinesDrawn += primitiveCount * (instanceCount == -1 ? 1 : instanceCount);
        //                    break;
        //                case PrimitiveType.TriangleList:
        //                case PrimitiveType.TriangleStrip:
        //                    application.currentFrame.TrianglesDrawn += primitiveCount * (instanceCount == -1 ? 1 : instanceCount);
        //                    break;
        //            }
        //#endif

        //            if (shaderStack.currentEffect != null &&
        //                shaderStack.currentEffect.CurrentTechnique.Passes.Count > 1)
        //            {
        //                //no pass will be bound if the effect has more than one pass
        //                foreach (EffectPass pass in shaderStack.currentEffect.CurrentTechnique.Passes)
        //                {
        //                    pass.Apply();

        //                    if (instanceCount == -1)
        //                        graphics.DrawIndexedPrimitives(primitiveType, baseVertex, minVertexIndex, numVertices, startIndex, primitiveCount);
        //                    else
        //                        graphics.DrawInstancedPrimitives(primitiveType, baseVertex, minVertexIndex, numVertices, startIndex, primitiveCount, instanceCount);
        //                }
        //            }
        //            else
        //            {
        //                if (instanceCount == -1)
        //                    graphics.DrawIndexedPrimitives(primitiveType, baseVertex, minVertexIndex, numVertices, startIndex, primitiveCount);
        //                else
        //                    graphics.DrawInstancedPrimitives(primitiveType, baseVertex, minVertexIndex, numVertices, startIndex, primitiveCount, instanceCount);
        //            }
        //        }

        ////        internal void DrawPrimitives(
        ////            int rawVertexCount,
        //            PrimitiveType primitiveType,
        //            int startVertex,
        //            int primitiveCount,
        //            Graphics.AnimationTransformArray animationArray,
        //            Graphics.ShaderSystem.ShaderExtension extension,
        //            Type vertexType,
        //            VertexDeclaration declaration)
        //        {
        //#if DEBUG
        //            ValidateRenderState();
        //#endif
        //            if (startVertex < 0 || startVertex > rawVertexCount)
        //                throw new ArgumentException("startVertex");

        //            if (primitiveCount == -1)
        //            {
        //                switch (primitiveType)
        //                {
        //                    case PrimitiveType.LineList:
        //                        primitiveCount = (rawVertexCount - startVertex) / 2;
        //                        break;
        //                    case PrimitiveType.LineStrip:
        //                        primitiveCount = (rawVertexCount - startVertex) - 1;
        //                        break;
        //                    case PrimitiveType.TriangleList:
        //                        primitiveCount = (rawVertexCount - startVertex) / 3;
        //                        break;
        //                    case PrimitiveType.TriangleStrip:
        //                        primitiveCount = (rawVertexCount - startVertex) - 2;
        //                        break;
        //                }
        //            }
        //            else
        //            {
        //                int vertCount = 0;
        //                switch (primitiveType)
        //                {
        //                    case PrimitiveType.LineList:
        //                        vertCount = primitiveCount * 2;
        //                        break;
        //                    case PrimitiveType.LineStrip:
        //                        vertCount = primitiveCount +1;
        //                        break;
        //                    case PrimitiveType.TriangleList:
        //                        vertCount = primitiveCount * 3;
        //                        break;
        //                    case PrimitiveType.TriangleStrip:
        //                        vertCount = primitiveCount + 2;
        //                        break;
        //                }

        //#if XBOX360
        //                //don't do this check when instancing on the 360
        //                if (extension != Xen.Graphics.ShaderSystem.ShaderExtension.Instancing)
        //#endif
        //                if (startVertex + vertCount > rawVertexCount)
        //                    throw new ArgumentException("primtiveCount or startVertex is out of range");
        //            }

        //            renderState.ApplyRenderStateChanges(rawVertexCount, animationArray, extension);

        //#if DEBUG
        //            shaderSystem.ValidateVertexDeclarationForShader(declaration, vertexType);

        //            System.Threading.Interlocked.Increment(ref application.currentFrame.DrawPrimitivesCallCount);

        //            switch (primitiveType)
        //            {
        //                case PrimitiveType.LineList:
        //                case PrimitiveType.LineStrip:
        //                    application.currentFrame.LinesDrawn += primitiveCount;
        //                    break;
        //                case PrimitiveType.TriangleList:
        //                case PrimitiveType.TriangleStrip:
        //                    application.currentFrame.TrianglesDrawn += primitiveCount;
        //                    break;
        //            }
        //#endif

        //            if (shaderStack.currentEffect != null &&
        //                shaderStack.currentEffect.CurrentTechnique.Passes.Count > 1)
        //            {
        //                //no pass will be bound if the effect has more than one pass
        //                foreach (EffectPass pass in shaderStack.currentEffect.CurrentTechnique.Passes)
        //                {
        //                    pass.Apply();

        //                    graphics.DrawPrimitives(primitiveType, startVertex, primitiveCount);
        //                }
        //            }
        //            else
        //                graphics.DrawPrimitives(primitiveType, startVertex, primitiveCount);
        //        }
    }
}