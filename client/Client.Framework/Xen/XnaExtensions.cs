using System;
using Client.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Xen
{
    /// <summary>
    /// Wrapper for placement of an XNA GameComponent in the main list, while letting it be drawn in a DrawTarget.
    /// </summary>
    internal sealed class XnaDrawableGameComponentWrapper : GameComponent, IDraw
    {
        private readonly DrawableGameComponent component;
        private readonly EventHandler<EventArgs> UpdateOrderChangedHandler, DisposeHandler;
        private readonly Application application;
        private Xen.Graphics.DrawTarget owner;

        public DrawableGameComponent Component { get { return component; } }

        public XnaDrawableGameComponentWrapper(DrawableGameComponent component, Xen.Graphics.DrawTarget owner, Application application)
            : base(component.Game)
        {
            if (owner == null || application == null) throw new ArgumentNullException();

            this.owner = owner;
            this.component = component;
            this.application = application;

            this.component.Initialize();
            this.DisposeHandler = new EventHandler<EventArgs>(OnDisposed);
            this.component.Disposed += DisposeHandler;

            //add this object to the main XNA update list, unless it's already there
            bool addToUpdateList = true;
            GameComponentCollection components = application.XnaComponents;

            foreach (GameComponent item in components)
            {
                XnaDrawableGameComponentWrapper wrapper = item as XnaDrawableGameComponentWrapper;
                if (item == component || (wrapper != null && wrapper.component == component))
                {
                    //already in the list...
                    addToUpdateList = false;
                    break;
                }
            }

            if (addToUpdateList)
            {
                components.Add(this);

                this.UpdateOrderChangedHandler = new EventHandler<EventArgs>(OnUpdateOrderChangedHandler);
                this.component.UpdateOrderChanged += UpdateOrderChangedHandler;
            }
        }

        private void OnDisposed(object sender, EventArgs e)
        {
            this.component.Disposed -= DisposeHandler;

            if (this.owner != null)
            {
                this.owner.Remove(this);
                this.owner = null;
            }

            //remove from the game components list
            GameComponentCollection components = application.XnaComponents;

            for (int i = 0; i < components.Count; i++)
            {
                XnaDrawableGameComponentWrapper wrapper = components[i] as XnaDrawableGameComponentWrapper;
                if ((wrapper != null && wrapper.component == component))
                {
                    components.RemoveAt(i);
                    break;
                }
            }

            if (UpdateOrderChangedHandler != null)
            {
                this.component.UpdateOrderChanged -= UpdateOrderChangedHandler;
            }

            this.Dispose();
        }

        public void Draw(DrawState state)
        {
            component.Draw(state);
        }

        public bool CullTest(ICuller culler)
        {
            return component.Visible;
        }

        public override void Update(GameTime gameTime)
        {
            if (this.component.Enabled)
                this.component.Update(gameTime);
        }

        private void OnUpdateOrderChangedHandler(object sender, EventArgs e)
        {
            this.UpdateOrder = component.UpdateOrder;
        }

        public void RemovedFromOwner()
        {
            this.owner = null;
        }
    }

    /// <summary>
    /// Static extensions class providing Xen compatible methods to XNA classes
    /// </summary>
    public static class XnaExtensions
    {
        /// <summary>
        /// [XEN EXTENSION] Draw an XNA vertex buffer
        /// </summary>
        //public static void Draw(this VertexBuffer vb, DrawState state, IndexBuffer indices, PrimitiveType primitiveType,
        //    int baseVertex, int numVertices, int primitiveCount, int startIndex)
        //{
        //    if (state == null)
        //        throw new ArgumentNullException();

        //    state.graphics.SetVertexBuffer(vb);

        //    if (indices != null)
        //        state.DrawIndexedPrimitives(vb.VertexCount, indices, indices.IndexCount, primitiveType, baseVertex, 0, numVertices, startIndex, primitiveCount, null, Xen.Graphics.ShaderSystem.ShaderExtension.None, null, vb.VertexDeclaration, -1);
        //    else
        //        state.DrawPrimitives(vb.VertexCount, primitiveType, baseVertex, primitiveCount, null, Xen.Graphics.ShaderSystem.ShaderExtension.None, null, vb.VertexDeclaration);
        //}
        /// <summary>
        /// [XEN EXTENSION] Draw an XNA vertex buffer
        /// </summary>
        //public static void Draw(this VertexBuffer vb, DrawState state, IndexBuffer indices, PrimitiveType primitiveType)
        //{
        //    if (state == null)
        //        throw new ArgumentNullException();

        //    state.graphics.SetVertexBuffer(vb);

        //    if (indices != null)
        //    {
        //        state.DrawIndexedPrimitives(vb.VertexCount, indices, indices.IndexCount, primitiveType, 0, 0, vb.VertexCount, 0, -1, null, Xen.Graphics.ShaderSystem.ShaderExtension.None, null, vb.VertexDeclaration, -1);
        //    }
        //    else
        //        state.DrawPrimitives(vb.VertexCount, primitiveType, 0, -1, null, Xen.Graphics.ShaderSystem.ShaderExtension.None, null, vb.VertexDeclaration);
        //}

        /// <summary>
        /// [XEN EXTENSION] Draw an XNA vertex buffer, with blended animation
        /// </summary>
        //public static void DrawBlending(this VertexBuffer vb, DrawState state, IndexBuffer indices,
        //    PrimitiveType primitiveType, Graphics.AnimationTransformArray animationTransforms,
        //    int baseVertex, int numVertices, int primitiveCount, int startIndex)
        //{
        //    if (state == null)
        //        throw new ArgumentNullException();

        //    state.graphics.SetVertexBuffer(vb);

        //    if (indices != null)
        //    {
        //        state.DrawIndexedPrimitives(vb.VertexCount, indices, indices.IndexCount, primitiveType, baseVertex, 0, numVertices, startIndex, primitiveCount, animationTransforms, animationTransforms == null ? Xen.Graphics.ShaderSystem.ShaderExtension.None : Xen.Graphics.ShaderSystem.ShaderExtension.Blending, null, vb.VertexDeclaration, -1);
        //    }
        //    else
        //        state.DrawPrimitives(vb.VertexCount, primitiveType, baseVertex, primitiveCount, animationTransforms, animationTransforms == null ? Xen.Graphics.ShaderSystem.ShaderExtension.None : Xen.Graphics.ShaderSystem.ShaderExtension.Blending, null, vb.VertexDeclaration);
        //}
        /// <summary>
        /// [XEN EXTENSION] Draw an XNA vertex buffer, with blended animation
        /// </summary>
        //public static void DrawBlending(this VertexBuffer vb, DrawState state, IndexBuffer indices,
        //    PrimitiveType primitiveType, Graphics.AnimationTransformArray animationTransforms)
        //{
        //    if (state == null)
        //        throw new ArgumentNullException();

        //    state.graphics.SetVertexBuffer(vb);

        //    if (indices != null)
        //    {
        //        int size = indices.IndexElementSize == IndexElementSize.SixteenBits ? 2 : 4;
        //        state.DrawIndexedPrimitives(vb.VertexCount, indices, indices.IndexCount, primitiveType, 0, 0, vb.VertexCount, 0, -1, animationTransforms, animationTransforms == null ? Xen.Graphics.ShaderSystem.ShaderExtension.None : Xen.Graphics.ShaderSystem.ShaderExtension.Blending, null, vb.VertexDeclaration, -1);
        //    }
        //    else
        //        state.DrawPrimitives(vb.VertexCount, primitiveType, 0, -1, animationTransforms, animationTransforms == null ? Xen.Graphics.ShaderSystem.ShaderExtension.None : Xen.Graphics.ShaderSystem.ShaderExtension.Blending, null, vb.VertexDeclaration);
        //}

        /// <summary>
        /// [XEN EXTENSION] Draw an XNA drawable game component
        /// </summary>
        public static void Draw(this DrawableGameComponent component, DrawState state)
        {
#if DEBUG
            RenderTargetBinding rt = state.graphics.GetRenderTargets()[0];
#endif

            component.Draw(state);

#if DEBUG
            if (rt.RenderTarget != state.graphics.GetRenderTargets()[0].RenderTarget)
                throw new InvalidOperationException(string.Format("DrawableGameComponent of type '{0}' has modified the RenderTarget when called with the Xen DrawState Extension", component.GetType().Name));
#endif

            state.RenderState.InternalDirtyRenderState(Xen.Graphics.StateFlag.All);
        }

        /// <summary>
        /// [XEN EXTENSION] Draw an XNA drawable game component
        /// </summary>
        public static void Draw(this DrawableGameComponent component, FrameState farme)
        {
            component.Draw(farme);
            farme.DrawState.RenderState.InternalDirtyRenderState(Xen.Graphics.StateFlag.All);
        }

        /// <summary>
        /// [XEN EXTENSION] Update an XNA game component
        /// </summary>
        public static void Update(this GameComponent component, UpdateState state)
        {
            component.Update(state);
        }
    }
}