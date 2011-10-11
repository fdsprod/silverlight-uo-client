using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Xen.Camera;
using Xen.Graphics.Stack;

namespace Xen.Graphics
{
    /// <summary>
    /// Interface to shader system global values
    /// </summary>
    public interface IShaderGlobals
    {
        #region Get / Set shader globals

        /// <summary>
        /// Set the global matrix array value used by shaders
        /// </summary>
        /// <param name="name">name of the value (case sensitive)</param>
        /// <param name="value">value to assign the shader global</param>
        void SetShaderGlobal(string name, Matrix[] value);

        /// <summary>
        /// Set the global vector array value used by shaders
        /// </summary>
        /// <param name="name">name of the value (case sensitive)</param>
        /// <param name="value">value to assign the shader global</param>
        void SetShaderGlobal(string name, Vector4[] value);

        /// <summary>
        /// Set the global vector array value used by shaders
        /// </summary>
        /// <param name="name">name of the value (case sensitive)</param>
        /// <param name="value">value to assign the shader global</param>
        void SetShaderGlobal(string name, Vector3[] value);

        /// <summary>
        /// Set the global vector array value used by shaders
        /// </summary>
        /// <param name="name">name of the value (case sensitive)</param>
        /// <param name="value">value to assign the shader global</param>
        void SetShaderGlobal(string name, Vector2[] value);

        /// <summary>
        /// Set the global float array value used by shaders
        /// </summary>
        /// <param name="name">name of the value (case sensitive)</param>
        /// <param name="value">value to assign the shader global</param>
        void SetShaderGlobal(string name, float[] value);

        /// <summary>
        /// Set the global matrix value used by shaders
        /// </summary>
        /// <param name="name">name of the value (case sensitive)</param>
        /// <param name="value">value to assign the shader global</param>
        void SetShaderGlobal(string name, ref Matrix value);

        /// <summary>
        /// Get the shader global array by name. Returns true if the value exists
        /// </summary>
        /// <param name="name">name of the value (case sensitive)</param>
        /// <param name="value">output value to be assigned the shader global value (if it exists)</param>
        /// <remarks>True if the value exists, and has been ouput</remarks>
        bool GetShaderGlobal(string name, out Matrix[] value);

        /// <summary>
        /// Get the shader global array by name. Returns true if the value exists
        /// </summary>
        /// <param name="name">name of the value (case sensitive)</param>
        /// <param name="value">output value to be assigned the shader global value (if it exists)</param>
        /// <remarks>True if the value exists, and has been ouput</remarks>
        bool GetShaderGlobal(string name, out Vector4[] value);

        /// <summary>
        /// Get the shader global array by name. Returns true if the value exists
        /// </summary>
        /// <param name="name">name of the value (case sensitive)</param>
        /// <param name="value">output value to be assigned the shader global value (if it exists)</param>
        /// <remarks>True if the value exists, and has been ouput</remarks>
        bool GetShaderGlobal(string name, out Vector3[] value);

        /// <summary>
        /// Get the shader global array by name. Returns true if the value exists
        /// </summary>
        /// <param name="name">name of the value (case sensitive)</param>
        /// <param name="value">output value to be assigned the shader global value (if it exists)</param>
        /// <remarks>True if the value exists, and has been ouput</remarks>
        bool GetShaderGlobal(string name, out Vector2[] value);

        /// <summary>
        /// Get the shader global array by name. Returns true if the value exists
        /// </summary>
        /// <param name="name">name of the value (case sensitive)</param>
        /// <param name="value">output value to be assigned the shader global value (if it exists)</param>
        /// <remarks>True if the value exists, and has been ouput</remarks>
        bool GetShaderGlobal(string name, out float[] value);

        /// <summary>
        /// Get the shader global matrix by name. Returns true if the value exists
        /// </summary>
        /// <param name="name">name of the value (case sensitive)</param>
        /// <param name="value">output value to be assigned the shader global value (if it exists)</param>
        /// <remarks>True if the value exists, and has been ouput</remarks>
        bool GetShaderGlobal(string name, out Matrix value);

        /// <summary>
        /// Set the global vector4 value used by shaders
        /// </summary>
        /// <param name="name">name of the value (case sensitive)</param>
        /// <param name="value">value to assign the shader global</param>
        void SetShaderGlobal(string name, ref Vector4 value);

        /// <summary>
        /// Get the shader global vector4 by name. Returns true if the value exists
        /// </summary>
        /// <param name="name">name of the value (case sensitive)</param>
        /// <param name="value">output value to be assigned the shader global value (if it exists)</param>
        /// <remarks>True if the value exists, and has been ouput</remarks>
        bool GetShaderGlobal(string name, ref Vector4 value);

        /// <summary>
        /// Set the global vector3 value used by shaders
        /// </summary>
        /// <param name="name">name of the value (case sensitive)</param>
        /// <param name="value">value to assign the shader global</param>
        void SetShaderGlobal(string name, ref Vector3 value);

        /// <summary>
        /// Get the shader global vector3 by name. Returns true if the value exists
        /// </summary>
        /// <param name="name">name of the value (case sensitive)</param>
        /// <param name="value">output value to be assigned the shader global value (if it exists)</param>
        /// <remarks>True if the value exists, and has been ouput</remarks>
        bool GetShaderGlobal(string name, out Vector3 value);

        /// <summary>
        /// Set the global vector2 value used by shaders
        /// </summary>
        /// <param name="name">name of the value (case sensitive)</param>
        /// <param name="value">value to assign the shader global</param>
        void SetShaderGlobal(string name, ref Vector2 value);

        /// <summary>
        /// Get the shader global vector2 by name. Returns true if the value exists
        /// </summary>
        /// <param name="name">name of the value (case sensitive)</param>
        /// <param name="value">output value to be assigned the shader global value (if it exists)</param>
        /// <remarks>True if the value exists, and has been ouput</remarks>
        bool GetShaderGlobal(string name, out Vector2 value);

        /// <summary>
        /// Set the global Vector4 value used by shaders
        /// </summary>
        /// <param name="name">name of the value (case sensitive)</param>
        /// <param name="value">value to assign the shader global</param>
        void SetShaderGlobal(string name, Vector4 value);

        /// <summary>
        /// Set the global vector3 value used by shaders
        /// </summary>
        /// <param name="name">name of the value (case sensitive)</param>
        /// <param name="value">value to assign the shader global</param>
        void SetShaderGlobal(string name, Vector3 value);

        /// <summary>
        /// Set the global vector2 value used by shaders
        /// </summary>
        /// <param name="name">name of the value (case sensitive)</param>
        /// <param name="value">value to assign the shader global</param>
        void SetShaderGlobal(string name, Vector2 value);

        /// <summary>
        /// Set the global float value used by shaders
        /// </summary>
        /// <param name="name">name of the value (case sensitive)</param>
        /// <param name="value">value to assign the shader global</param>
        void SetShaderGlobal(string name, float value);

        /// <summary>
        /// Set the global boolean value used by shaders
        /// </summary>
        /// <param name="name">name of the value (case sensitive)</param>
        /// <param name="value">value to assign the shader global</param>
        void SetShaderGlobal(string name, bool value);

        /// <summary>
        /// Get the shader global float by name. Returns true if the value exists
        /// </summary>
        /// <param name="name">name of the value (case sensitive)</param>
        /// <param name="value">output value to be assigned the shader global value (if it exists)</param>
        /// <remarks>True if the value exists, and has been ouput</remarks>
        bool GetShaderGlobal(string name, out float value);

        /// <summary>
        /// Set the global texture sampler state used by shaders
        /// </summary>
        /// <param name="name">name of the value (case sensitive)</param>
        /// <param name="value">value to assign the shader global</param>
        void SetShaderGlobal(string name, TextureSamplerState value);

        /// <summary>
        /// Set the global texture used by shaders
        /// </summary>
        /// <param name="name">name of the value (case sensitive)</param>
        /// <param name="value">value to assign the shader global</param>
        void SetShaderGlobal(string name, Microsoft.Xna.Framework.Graphics.Texture value);

        /// <summary>
        /// Get the shader global texture by name. Returns true if the value exists
        /// </summary>
        /// <param name="name">name of the value (case sensitive)</param>
        /// <param name="value">output value to be assigned the shader global value (if it exists)</param>
        /// <remarks>True if the value exists, and has been ouput</remarks>
        bool GetShaderGlobal(string name, out Microsoft.Xna.Framework.Graphics.Texture value);

        /// <summary>
        /// Set the global texture (2D) used by shaders
        /// </summary>
        /// <param name="name">name of the value (case sensitive)</param>
        /// <param name="value">value to assign the shader global</param>
        void SetShaderGlobal(string name, Microsoft.Xna.Framework.Graphics.Texture2D value);

        /// <summary>
        /// Get the shader global texture (2D) by name. Returns true if the value exists
        /// </summary>
        /// <param name="name">name of the value (case sensitive)</param>
        /// <param name="value">output value to be assigned the shader global value (if it exists)</param>
        /// <remarks>True if the value exists, and has been ouput</remarks>
        bool GetShaderGlobal(string name, out Microsoft.Xna.Framework.Graphics.Texture2D value);

        /// <summary>
        /// Set the global texture (cubemap) used by shaders
        /// </summary>
        /// <param name="name">name of the value (case sensitive)</param>
        /// <param name="value">value to assign the shader global</param>
        void SetShaderGlobal(string name, Microsoft.Xna.Framework.Graphics.TextureCube value);

        /// <summary>
        /// Get the shader global texture (cubemap) by name. Returns true if the value exists
        /// </summary>
        /// <param name="name">name of the value (case sensitive)</param>
        /// <param name="value">output value to be assigned the shader global value (if it exists)</param>
        /// <remarks>True if the value exists, and has been ouput</remarks>
        bool GetShaderGlobal(string name, out Microsoft.Xna.Framework.Graphics.TextureCube value);

        #endregion Get / Set shader globals
    }

    namespace Stack
    {
        /// <summary>
        /// Shader stack storage for the DrawState
        /// </summary>
#if !DEBUG_API

        [System.Diagnostics.DebuggerStepThrough]
#endif
        public sealed class ShaderStack
        {
            /// <summary>
            /// Structure used for a using block with a Push method
            /// </summary>
            [System.Diagnostics.DebuggerStepThrough]
            public struct UsingPop : IDisposable
            {
                internal UsingPop(ShaderStack stack)
                {
                    this.stack = stack;
                }

                private readonly ShaderStack stack;

                /// <summary>Invokes the Pop metohd</summary>
                public void Dispose()
                {
                    stack.shaders[checked(stack.index--)] = null;
                    object top = stack.shaders[stack.index];
                    stack.currentShader = top as IShader;
                    stack.currentEffect = top as Effect;
                }
            }
            internal readonly UsingPop UsingBlock;

            //push operator:
            /// <summary>
            /// Wrapper on Push
            /// </summary>
            public static UsingPop operator +(ShaderStack stack, IShader shader)
            {
                return stack.Push(shader);
            }

            //push operator:
            /// <summary>
            /// Wrapper on Push
            /// </summary>
            public static UsingPop operator +(ShaderStack stack, Effect effect)
            {
                return stack.Push(effect);
            }

            private object[] shaders = new object[32];
            internal IShader currentShader;
            internal Effect currentEffect;
            private uint index;
            private readonly DrawState state;

            internal ShaderStack(DrawState state)
            {
                this.UsingBlock = new UsingPop(this);
                this.state = state;
            }

            /// <summary>
            /// Pushes a generic instance of the given shader type on to the top of the current rendering shader stack
            /// <para>Note: this method can be used in a 'using() {}' block to automatically call <see cref="Pop"/></para>
            /// </summary>
            public UsingPop Push<T>() where T : class, IShader, new()
            {
                return Push(state.GetShader<T>());
            }

            /// <summary>
            /// Pushes the shader on to the top of the current rendering shader stack
            /// <para>Note: this method can be used in a 'using() {}' block to automatically call <see cref="Pop"/></para>
            /// </summary>
            public UsingPop Push(IShader shader)
            {
                if (shader == null)
                    throw new ArgumentNullException();
                if (index == shaders.Length)
                    Array.Resize(ref shaders, shaders.Length * 2);
                shaders[++index] = shader;
                currentShader = shader;
                currentEffect = null;
                return UsingBlock;
            }

            /// <summary>
            /// Pushes the Effect on to the top of the current rendering shader stack
            /// <para>Note: this method can be used in a 'using() {}' block to automatically call <see cref="Pop"/></para>
            /// </summary>
            public UsingPop Push(Effect effect)
            {
                if (effect == null)
                    throw new ArgumentNullException();
                if (index == shaders.Length)
                    Array.Resize(ref shaders, shaders.Length * 2);
                shaders[++index] = effect;
                currentEffect = effect;
                currentShader = null;
                return UsingBlock;
            }

            /// <summary>
            /// Duplicates the shader on to the top of the current rendering shader stack
            /// <para>Note: this method can be used in a 'using() {}' block to automatically call <see cref="Pop"/></para>
            /// </summary>
            public UsingPop Push()
            {
                if (index == shaders.Length)
                    Array.Resize(ref shaders, shaders.Length * 2);
                object shader = shaders[index];
                shaders[++index] = shader;
                return UsingBlock;
            }

            /// <summary>
            /// Pops the top of the rendering shader stack, Restoring the shader saved with <see cref="Push(IShader)"/>
            /// </summary>
            public void Pop()
            {
                shaders[checked(index--)] = null;
                currentShader = shaders[index] as IShader;
                currentEffect = shaders[index] as Effect;
            }

            /// <summary>
            /// Gets the shader at the top of the shader stack
            /// <para>Returns null if an <see cref="Effect"/> is currently at the top of the stack</para>
            /// </summary>
            public IShader CurrentShader
            {
                get { return currentShader; }
                set
                {
                    if (value == null)
                        throw new ArgumentNullException();
                    currentShader = value;
                    currentEffect = null;
                    shaders[index] = value;
                }
            }

            /// <summary>
            /// Gets the Effect at the top of the shader stack
            /// <para>Returns null if an <see cref="IShader"/> is currently at the top of the stack</para>
            /// </summary>
            public Effect CurrentEffect
            {
                get { return currentEffect; }
                set
                {
                    if (value == null)
                        throw new ArgumentNullException();
                    currentEffect = value;
                    currentShader = null;
                    shaders[index] = value;
                }
            }

            /// <summary>
            /// Sets the current shader, replacing the shader at the top of the stack
            /// </summary>
            public void Set(IShader shader)
            {
                if (shader == null)
                    throw new ArgumentNullException();
                currentShader = shader;
                shaders[index] = shader;
            }

            /// <summary>
            /// Sets the current <see cref="Effect"/>, replacing the shader at the top of the stack
            /// </summary>
            public void Set(Effect effect)
            {
                if (effect == null)
                    throw new ArgumentNullException();
                currentEffect = effect;
                currentShader = null;
                shaders[index] = effect;
            }
        }
    }

#if !DEBUG_API

    [System.Diagnostics.DebuggerStepThrough]
#endif
    internal sealed class ShaderSystemState : ShaderSystemBase, IShaderGlobals
    {
        public ShaderSystemState(DrawState state, Application application, Microsoft.Xna.Framework.Graphics.GraphicsDevice graphics, MatrixProviderCollection matrices)
        {
            if (state == null)
                throw new ArgumentNullException();
            if (application == null)
                throw new ArgumentNullException();
            if (graphics == null)
                throw new ArgumentNullException();

            this.matrices = matrices;
            this.application = application;
            this.state = state;
            this.graphics = graphics;

            this.DeviceUniqueIndex = application.graphicsId;

            var defaultState = TextureSamplerStateInternal.BuildState(new TextureSamplerState());
            samplerMapping.Add(0, defaultState);

            for (int i = 0; i < psCount; i++)
                psSamplerState[i] = defaultState;

            for (int i = 0; i < vsCount; i++)
                vsSamplerState[i] = defaultState;

            this.effectMapper = new EffectParamMapper(state);
        }

        public override bool IsShaderBound(IShader shader)
        {
            return shader == gpuBoundShader;
        }

        #region members

        private readonly MatrixProviderCollection matrices;
        private readonly Application application;
        private readonly DrawState state;
        private readonly Microsoft.Xna.Framework.Graphics.GraphicsDevice graphics;

        private readonly EffectParamMapper effectMapper;

        private Xen.Graphics.IShader boundShader, gpuBoundShader;
        private Microsoft.Xna.Framework.Graphics.Effect boundEffect;
        private Microsoft.Xna.Framework.Graphics.EffectPass boundPass;
        private ShaderExtension boundExtension;
        private bool boundShaderDirty;
        private int boundShaderWVPindex;
        private int bufferVertexCount, bufferVertexCountChangeIndex = 1;
        private ShaderExtension extensionMode = ShaderExtension.None;
        private Xen.Graphics.AnimationTransformArray blendTransforms;
        private int blendChangeIdx, blendChangeLocalIdx;
        private ICamera camera;

        #region members: textures and samplers

        private const int psCount = 16, vsCount = 4;

        private readonly Microsoft.Xna.Framework.Graphics.Texture[]
            psTextures = new Microsoft.Xna.Framework.Graphics.Texture[psCount],
            vsTextures = new Microsoft.Xna.Framework.Graphics.Texture[vsCount];

        private readonly int[]
            psSamplerValue = new int[psCount],
            vsSamplerValue = new int[vsCount];
        private readonly SamplerState[]
            psSamplerState = new SamplerState[psCount],
            vsSamplerState = new SamplerState[vsCount];

        private readonly Dictionary<int, SamplerState> samplerMapping = new Dictionary<int, SamplerState>();

        #endregion members: textures and samplers

        #region members: shader content

#if DEBUG
        private readonly Dictionary<Microsoft.Xna.Framework.Graphics.Effect, int[]> effectInstructionCounts = new Dictionary<Microsoft.Xna.Framework.Graphics.Effect, int[]>();
#endif

        #endregion members: shader content

        #region members: shader globals internal storage

        private ShaderGlobal<Matrix>[] matrixGlobals = new ShaderGlobal<Matrix>[8];
        private ShaderGlobal<Vector4>[] v4Globals = new ShaderGlobal<Vector4>[8];
        private ShaderGlobal<Vector3>[] v3Globals = new ShaderGlobal<Vector3>[8];
        private ShaderGlobal<Vector2>[] v2Globals = new ShaderGlobal<Vector2>[8];
        private ShaderGlobal<float>[] singleGlobals = new ShaderGlobal<float>[8];
        private ShaderGlobal<bool>[] booleanGlobals = new ShaderGlobal<bool>[8];

        private ShaderGlobal<Matrix[]>[] matrixArrayGlobals = new ShaderGlobal<Matrix[]>[8];
        private ShaderGlobal<Vector4[]>[] v4ArrayGlobals = new ShaderGlobal<Vector4[]>[8];
        private ShaderGlobal<Vector3[]>[] v3ArrayGlobals = new ShaderGlobal<Vector3[]>[8];
        private ShaderGlobal<Vector2[]>[] v2ArrayGlobals = new ShaderGlobal<Vector2[]>[8];
        private ShaderGlobal<float[]>[] singleArrayGlobals = new ShaderGlobal<float[]>[8];

        private TextureSamplerState[] samplerGlobals = new TextureSamplerState[8];
        private Microsoft.Xna.Framework.Graphics.Texture[] textureGlobals = new Microsoft.Xna.Framework.Graphics.Texture[8];
        private Microsoft.Xna.Framework.Graphics.Texture2D[] texture2DGlobals = new Microsoft.Xna.Framework.Graphics.Texture2D[8];
        private Microsoft.Xna.Framework.Graphics.TextureCube[] textureCubeGlobals = new Microsoft.Xna.Framework.Graphics.TextureCube[8];

        private readonly static Dictionary<string, int> uniqueNameIndex = new Dictionary<string, int>();

        private readonly Dictionary<string, int>
            matrixGlobalLookup = new Dictionary<string, int>(),
            v4GlobalLookup = new Dictionary<string, int>(),
            v3GlobalLookup = new Dictionary<string, int>(),
            v2GlobalLookup = new Dictionary<string, int>(),
            singleGlobalLookup = new Dictionary<string, int>(),
            booleanGlobalLookup = new Dictionary<string, int>(),

            matrixArrayGlobalLookup = new Dictionary<string, int>(),
            v4ArrayGlobalLookup = new Dictionary<string, int>(),
            v3ArrayGlobalLookup = new Dictionary<string, int>(),
            v2ArrayGlobalLookup = new Dictionary<string, int>(),
            singleArrayGlobalLookup = new Dictionary<string, int>(),

            samplerGlobalLookup = new Dictionary<string, int>(),
            textureGlobalLookup = new Dictionary<string, int>(),
            texture2DGlobalLookup = new Dictionary<string, int>(),
            texture3DGlobalLookup = new Dictionary<string, int>(),
            textureCubeGlobalLookup = new Dictionary<string, int>();

        private sealed class ShaderGlobal<T>
        {
            public T value;
            public int id;
        }

        #endregion members: shader globals internal storage

        public DrawState DrawState { get { return state; } }

        internal ICamera Camera { set { this.camera = value; } }

        #endregion members

        #region DrawState interaction

        internal void DirtyInternalRenderState(StateFlag dirtyState)
        {
            if ((dirtyState & StateFlag.Shaders) != 0)
            {
                boundShader = null;
                gpuBoundShader = null;
                this.boundShaderDirty = true;
            }

            if ((dirtyState & StateFlag.Textures) != 0)
            {
                boundShader = null;
                gpuBoundShader = null;

                //for (int i = 0; i < psSamplerDirty.Length; i++)
                //    psSamplerDirty[i] = true;
                //for (int i = 0; i < vsSamplerDirty.Length; i++)
                //    vsSamplerDirty[i] = true;

                for (int i = 0; i < psTextures.Length; i++)
                    psTextures[i] = null;
                for (int i = 0; i < vsTextures.Length; i++)
                    vsTextures[i] = null;
            }
        }

        internal void ApplyRenderStateChanges(IShader shader, int vertexCount, Xen.Graphics.AnimationTransformArray transforms, Xen.Graphics.ShaderSystem.ShaderExtension extension)
        {
#if DEBUG
            bool blend, instanc;
            shader.GetExtensionSupport(out blend, out instanc);
            if (extension == ShaderExtension.Blending && !blend)
                throw new InvalidOperationException(string.Format("The shader technique '{0}' does not have an animation blending extension", shader.GetType().FullName));
            if (extension == ShaderExtension.Instancing && !instanc)
                throw new InvalidOperationException(string.Format("The shader technique '{0}' does not have an instancing extension", shader.GetType().FullName));
#endif

            if (this.bufferVertexCount != vertexCount)
            {
                this.bufferVertexCountChangeIndex++;
                this.bufferVertexCount = vertexCount;
                boundShaderDirty = true;
            }

            bool extensionChanged = extension != extensionMode;
            boundShaderDirty |= extensionChanged;
            extensionMode = extension;

            //work out if blend matrices have changed, and the shader needs updating...
            if (transforms != this.blendTransforms)
            {
                //local index is used to compare to the actual transform object, reset when changing
                this.blendChangeLocalIdx = -1;
                //this index is used to compare the incomming shader. it always increments
                this.blendChangeIdx++;

                this.blendTransforms = transforms;
                this.boundShaderDirty = true;
            }
            else if (transforms != null && transforms.HasChanged(ref blendChangeLocalIdx))
            {
                //matrices object hasn't changed, but the data has.
                this.blendChangeIdx++;
                this.boundShaderDirty = true;
            }

            if (boundShader != shader
                || boundShaderDirty
                || boundShaderWVPindex != matrices.WorldViewProjection.index
                || boundShader.HasChanged)
            {
                Type type = shader.GetType();

                bool instanceChanged = this.boundShader != shader;

                boundShader = shader;
                boundShaderDirty = false;
                boundShaderWVPindex = matrices.WorldViewProjection.index;

                shader.Begin(this, instanceChanged, extensionChanged, extension);
            }
        }

        internal void ApplyRenderStateChanges(Effect effect, Xen.Graphics.AnimationTransformArray transforms, Xen.Graphics.ShaderSystem.ShaderExtension extension)
        {
            if (extension == ShaderExtension.Instancing)
                throw new InvalidOperationException("The bound Effect cannot be used for instancing");

            //binding an Effect, not an IShader
            gpuBoundShader = null;
            boundShader = null;

            BasicEffect basicEffect = effect as BasicEffect;
            bool changed = false;

            if (basicEffect != null)
            {
                basicEffect.World = this.matrices.World.value;
                basicEffect.View = this.matrices.View.value;
                basicEffect.Projection = this.matrices.Projection.value;
                changed = true;
            }
            else
                changed = effectMapper.ApplyMapping(effect, extension);

            if (boundEffect != effect)
            {
                boundEffect = effect;

                if (effect.CurrentTechnique.Passes.Count == 1)
                {
                    //begin the first pass too
                    boundPass = effect.CurrentTechnique.Passes[0];
                    boundPass.Apply();
                }
            }
            else if (changed)
            {
                //commit changes
                boundPass.Apply();
            }
        }

#if DEBUG

        internal void ValidateVertexDeclarationForShader(Microsoft.Xna.Framework.Graphics.VertexDeclaration decl, Type vertType)
        {
            if (boundShader != null && decl != null)
                application.declarationBuilder.ValidateVertexDeclarationForShader(decl, boundShader, vertType);
        }

#endif

        internal void ResetTextures()
        {
            for (int i = 0; i < psTextures.Length; i++)
            {
                if (psTextures[i] != null)
                {
                    graphics.Textures[i] = null;
                    psTextures[i] = null;
                }
            }
            for (int i = 0; i < vsTextures.Length; i++)
            {
                if (psTextures[i] != null)
                {
                    graphics.VertexTextures[i] = null;
                    vsTextures[i] = null;
                }
            }
            boundShader = null;
            gpuBoundShader = null;
        }

        #endregion DrawState interaction

        #region global setup, helper and shader init

        private int GetIndex<T>(ref ShaderGlobal<T>[] array, string name, Dictionary<string, int> dict)
        {
            int index;
            if (!dict.TryGetValue(name, out index))
            {
                if (dict.Count == array.Length)
                {
                    //resize up
                    index = array.Length;
                    ShaderGlobal<T>[] newArray = new ShaderGlobal<T>[array.Length * 2];
                    for (int i = 0; i < array.Length; i++)
                        newArray[i] = array[i];
                    newArray[index] = new ShaderGlobal<T>();
                    array = newArray;
                }
                else
                {
                    index = dict.Count;
                    array[index] = new ShaderGlobal<T>();
                }

                dict.Add(name, index);
            }
            return index;
        }

        public int GetIndexTexture<T>(ref T[] array, string name, Dictionary<string, int> dict)
        {
            int index;
            if (!dict.TryGetValue(name, out index))
            {
                if (dict.Count == array.Length)
                {
                    //resize up
                    index = array.Length;
                    T[] newArray = new T[array.Length * 2];
                    for (int i = 0; i < array.Length; i++)
                        newArray[i] = array[i];
                    array = newArray;
                }
                else
                    index = dict.Count;

                dict.Add(name, index);
            }
            return index;
        }

        public override sealed int GetGlobalUniqueID<T>(string name)
        {
            Type type = typeof(T);

            if (type == typeof(Matrix))
                return GetIndex(ref matrixGlobals, name, matrixGlobalLookup);

            if (type == typeof(Vector4))
                return GetIndex(ref v4Globals, name, v4GlobalLookup);

            if (type == typeof(Vector3))
                return GetIndex(ref v3Globals, name, v3GlobalLookup);

            if (type == typeof(Vector2))
                return GetIndex(ref v2Globals, name, v2GlobalLookup);

            if (type == typeof(float))
                return GetIndex(ref singleGlobals, name, singleGlobalLookup);

            if (type == typeof(bool))
                return GetIndex(ref booleanGlobals, name, booleanGlobalLookup);

            //array types

            if (type == typeof(Matrix[]))
                return GetIndex(ref matrixArrayGlobals, name, matrixArrayGlobalLookup);

            if (type == typeof(Vector4[]))
                return GetIndex(ref v4ArrayGlobals, name, v4ArrayGlobalLookup);

            if (type == typeof(Vector3[]))
                return GetIndex(ref v3ArrayGlobals, name, v3ArrayGlobalLookup);

            if (type == typeof(Vector2[]))
                return GetIndex(ref v2ArrayGlobals, name, v2ArrayGlobalLookup);

            if (type == typeof(float[]))
                return GetIndex(ref singleArrayGlobals, name, singleArrayGlobalLookup);

            //texture and sampler types

            if (type == typeof(TextureSamplerState))
                return GetIndexTexture(ref samplerGlobals, name, samplerGlobalLookup);

            if (type == typeof(Microsoft.Xna.Framework.Graphics.Texture))
                return GetIndexTexture(ref textureGlobals, name, textureGlobalLookup);

            if (type == typeof(Microsoft.Xna.Framework.Graphics.Texture2D))
                return GetIndexTexture(ref texture2DGlobals, name, texture2DGlobalLookup);

            if (type == typeof(Microsoft.Xna.Framework.Graphics.TextureCube))
                return GetIndexTexture(ref textureCubeGlobals, name, textureCubeGlobalLookup);

            throw new NotImplementedException();
        }

        public override sealed int GetNameUniqueID(string name)
        {
            int index;
            if (!uniqueNameIndex.TryGetValue(name, out index))
            {
                index = uniqueNameIndex.Count + 1;
                uniqueNameIndex.Add(name, index);
            }
            return index;
        }

        #endregion global setup, helper and shader init

        #region set global, non-array types

        public override sealed bool SetGlobalMatrix4(ref Vector4 x, ref Vector4 y, ref Vector4 z, ref Vector4 w, int uid, ref int changeId)
        {
            ShaderGlobal<Matrix> global = matrixGlobals[uid];
            if (global.id != changeId)
            {
#if DEBUG
                if (global.id == 0)
                    ValidateGlobalAccess<Matrix>(uid, matrixGlobalLookup);
#endif
                x.X = global.value.M11; x.Y = global.value.M21; x.Z = global.value.M31; x.W = global.value.M41;
                y.X = global.value.M12; y.Y = global.value.M22; y.Z = global.value.M32; y.W = global.value.M42;
                z.X = global.value.M13; z.Y = global.value.M23; z.Z = global.value.M33; z.W = global.value.M43;
                w.X = global.value.M14; w.Y = global.value.M24; w.Z = global.value.M34; w.W = global.value.M44;
            }
            bool result = global.id != changeId;
            changeId = global.id; return result;
        }

        public override sealed bool SetGlobalMatrix3(ref Vector4 x, ref Vector4 y, ref Vector4 z, int uid, ref int changeId)
        {
            ShaderGlobal<Matrix> global = matrixGlobals[uid];

            if (global.id != changeId)
            {
#if DEBUG
                if (global.id == 0)
                    ValidateGlobalAccess<Matrix>(uid, matrixGlobalLookup);
#endif
                x.X = global.value.M11; x.Y = global.value.M21; x.Z = global.value.M31; x.W = global.value.M41;
                y.X = global.value.M12; y.Y = global.value.M22; y.Z = global.value.M32; y.W = global.value.M42;
                z.X = global.value.M13; z.Y = global.value.M23; z.Z = global.value.M33; z.W = global.value.M43;
            }
            bool result = global.id != changeId;
            changeId = global.id; return result;
        }

        public override sealed bool SetGlobalMatrix2(ref Vector4 x, ref Vector4 y, int uid, ref int changeId)
        {
            ShaderGlobal<Matrix> global = matrixGlobals[uid];

            if (global.id != changeId)
            {
#if DEBUG
                if (global.id == 0)
                    ValidateGlobalAccess<Matrix>(uid, matrixGlobalLookup);
#endif
                x.X = global.value.M11; x.Y = global.value.M21; x.Z = global.value.M31; x.W = global.value.M41;
                y.X = global.value.M12; y.Y = global.value.M22; y.Z = global.value.M32; y.W = global.value.M42;
            }
            bool result = global.id != changeId;
            changeId = global.id; return result;
        }

        public override sealed bool SetGlobalMatrix1(ref Vector4 x, int uid, ref int changeId)
        {
            ShaderGlobal<Matrix> global = matrixGlobals[uid];

            if (global.id != changeId)
            {
#if DEBUG
                if (global.id == 0)
                    ValidateGlobalAccess<Matrix>(uid, matrixGlobalLookup);
#endif
                x.X = global.value.M11; x.Y = global.value.M21; x.Z = global.value.M31; x.W = global.value.M41;
            }
            bool result = global.id != changeId;
            changeId = global.id; return result;
        }

        public override sealed bool SetGlobalVector4(ref Vector4 value, int uid, ref int changeId)
        {
            ShaderGlobal<Vector4> global = v4Globals[uid];
#if DEBUG
            if (global.id == 0)
                ValidateGlobalAccess<Vector4>(uid, v4GlobalLookup);
#endif
            value = global.value;

            bool result = global.id != changeId;
            changeId = global.id; return result;
        }

        public override sealed bool SetGlobalVector3(ref Vector4 value, int uid, ref int changeId)
        {
            ShaderGlobal<Vector3> global = v3Globals[uid];
#if DEBUG
            if (global.id == 0)
                ValidateGlobalAccess<Vector3>(uid, v3GlobalLookup);
#endif
            value.X = global.value.X;
            value.Y = global.value.Y;
            value.Z = global.value.Z;

            bool result = global.id != changeId;
            changeId = global.id; return result;
        }

        public override sealed bool SetGlobalVector2(ref Vector4 value, int uid, ref int changeId)
        {
            ShaderGlobal<Vector2> global = v2Globals[uid];
#if DEBUG
            if (global.id == 0)
                ValidateGlobalAccess<Vector2>(uid, v2GlobalLookup);
#endif
            value.X = global.value.X;
            value.Y = global.value.Y;

            bool result = global.id != changeId;
            changeId = global.id; return result;
        }

        public override sealed bool SetGlobalSingle(ref Vector4 value, int uid, ref int changeId)
        {
            ShaderGlobal<Single> global = singleGlobals[uid];
#if DEBUG
            if (global.id == 0)
                ValidateGlobalAccess<Single>(uid, singleGlobalLookup);
#endif
            value.X = global.value;

            bool result = global.id != changeId;
            changeId = global.id; return result;
        }

        public override sealed bool SetGlobalBool(bool[] array, int index, int uid)
        {
            ShaderGlobal<bool> global = booleanGlobals[uid];
            bool current = array[index];
#if DEBUG
            if (global.id == 0)
                ValidateGlobalAccess<bool>(uid, booleanGlobalLookup);
#endif
            array[index] = global.value;
            return current != global.value;
        }

        #endregion set global, non-array types

        #region validate global

        private void ValidateGlobalAccess<T>(int uid, Dictionary<string, int> lookup)
        {
            string name = "value";
            foreach (KeyValuePair<string, int> kvp in lookup)
            {
                if (kvp.Value == uid)
                    name = kvp.Key;
            }
            throw new InvalidOperationException(string.Format("Shader is accessing uninitalised global value {0} \'{1}\'", typeof(T).Name, name));
        }

        private void ValidateGlobalTextureAccess<T>(int uid, Dictionary<string, int> lookup)
        {
            string name = "value";
            foreach (KeyValuePair<string, int> kvp in lookup)
            {
                if (kvp.Value == uid)
                    name = kvp.Key;
            }
            throw new InvalidOperationException(string.Format("Shader is accessing uninitalised global texture {0} \'{1}\'", typeof(T).Name, name));
        }

        #endregion validate global

        #region set global, array types

        public override sealed bool SetGlobalMatrix4(Vector4[] array, int start, int end, int uid, ref int changeId)
        {
            ShaderGlobal<Matrix[]> global = matrixArrayGlobals[uid];
            if (global.id != changeId)
            {
#if DEBUG
                if (global.id == 0)
                    ValidateGlobalAccess<Matrix[]>(uid, matrixArrayGlobalLookup);
#endif
                Matrix mat;
                Vector4 v = new Vector4();
                for (int i = start, j = 0; i < end && j < global.value.Length; )
                {
                    mat = global.value[j++];
                    v.X = mat.M11; v.X = mat.M21; v.X = mat.M31; v.X = mat.M41;
                    array[i++] = v;
                    v.X = mat.M12; v.X = mat.M22; v.X = mat.M32; v.X = mat.M42;
                    array[i++] = v;
                    v.X = mat.M13; v.X = mat.M23; v.X = mat.M33; v.X = mat.M43;
                    array[i++] = v;
                    v.X = mat.M14; v.X = mat.M24; v.X = mat.M34; v.X = mat.M44;
                    array[i++] = v;
                }
            }
            bool result = global.id != changeId;
            return changeId != global.id;
        }

        public override sealed bool SetGlobalMatrix3(Vector4[] array, int start, int end, int uid, ref int changeId)
        {
            ShaderGlobal<Matrix[]> global = matrixArrayGlobals[uid];
            if (global.id != changeId)
            {
#if DEBUG
                if (global.id == 0)
                    ValidateGlobalAccess<Matrix[]>(uid, matrixArrayGlobalLookup);
#endif
                Matrix mat;
                Vector4 v = new Vector4();
                for (int i = start, j = 0; i < end && j < global.value.Length; )
                {
                    mat = global.value[j++];
                    v.X = mat.M11; v.X = mat.M21; v.X = mat.M31; v.X = mat.M41;
                    array[i++] = v;
                    v.X = mat.M12; v.X = mat.M22; v.X = mat.M32; v.X = mat.M42;
                    array[i++] = v;
                    v.X = mat.M13; v.X = mat.M23; v.X = mat.M33; v.X = mat.M43;
                    array[i++] = v;
                }
            }
            bool result = global.id != changeId;
            return changeId != global.id;
        }

        public override sealed bool SetGlobalMatrix2(Vector4[] array, int start, int end, int uid, ref int changeId)
        {
            ShaderGlobal<Matrix[]> global = matrixArrayGlobals[uid];
            if (global.id != changeId)
            {
#if DEBUG
                if (global.id == 0)
                    ValidateGlobalAccess<Matrix[]>(uid, matrixArrayGlobalLookup);
#endif
                Matrix mat;
                Vector4 v = new Vector4();
                for (int i = start, j = 0; i < end && j < global.value.Length; )
                {
                    mat = global.value[j++];
                    v.X = mat.M11; v.X = mat.M21; v.X = mat.M31; v.X = mat.M41;
                    array[i++] = v;
                    v.X = mat.M12; v.X = mat.M22; v.X = mat.M32; v.X = mat.M42;
                    array[i++] = v;
                }
            }
            bool result = global.id != changeId;
            return changeId != global.id;
        }

        public override sealed bool SetGlobalMatrix1(Vector4[] array, int start, int end, int uid, ref int changeId)
        {
            ShaderGlobal<Matrix[]> global = matrixArrayGlobals[uid];
            if (global.id != changeId)
            {
#if DEBUG
                if (global.id == 0)
                    ValidateGlobalAccess<Matrix[]>(uid, matrixArrayGlobalLookup);
#endif
                Matrix mat;
                Vector4 v = new Vector4();
                for (int i = start, j = 0; i < end && j < global.value.Length; )
                {
                    mat = global.value[j++];
                    v.X = mat.M11; v.X = mat.M21; v.X = mat.M31; v.X = mat.M41;
                    array[i++] = v;
                }
            }
            bool result = global.id != changeId;
            return changeId != global.id;
        }

        public override sealed bool SetGlobalVector4(Vector4[] array, int start, int end, int uid, ref int changeId)
        {
            ShaderGlobal<Vector4[]> global = v4ArrayGlobals[uid];
            if (global.id != changeId)
            {
#if DEBUG
                if (global.id == 0)
                    ValidateGlobalAccess<Vector4[]>(uid, v4ArrayGlobalLookup);
#endif
                for (int i = start, j = 0; i < end && j < global.value.Length; )
                    array[i++] = global.value[j++];
                changeId = global.id;
            }
            bool result = global.id != changeId;
            return changeId != global.id;
        }

        public override sealed bool SetGlobalVector3(Vector4[] array, int start, int end, int uid, ref int changeId)
        {
            ShaderGlobal<Vector3[]> global = v3ArrayGlobals[uid];
            if (global.id != changeId)
            {
#if DEBUG
                if (global.id == 0)
                    ValidateGlobalAccess<Vector3[]>(uid, v3ArrayGlobalLookup);
#endif
                for (int i = start, j = 0; i < end && j < global.value.Length; )
                    array[i++] = new Vector4(global.value[j++], 0);
                changeId = global.id;
            }
            bool result = global.id != changeId;
            return changeId != global.id;
        }

        public override sealed bool SetGlobalVector2(Vector4[] array, int start, int end, int uid, ref int changeId)
        {
            ShaderGlobal<Vector2[]> global = v2ArrayGlobals[uid];
            if (global.id != changeId)
            {
#if DEBUG
                if (global.id == 0)
                    ValidateGlobalAccess<Vector2[]>(uid, v2ArrayGlobalLookup);
#endif
                for (int i = start, j = 0; i < end && j < global.value.Length; )
                    array[i++] = new Vector4(global.value[j++], 0, 0);
                changeId = global.id;
            }
            bool result = global.id != changeId;
            return changeId != global.id;
        }

        public override sealed bool SetGlobalSingle(Vector4[] array, int start, int end, int uid, ref int changeId)
        {
            ShaderGlobal<float[]> global = singleArrayGlobals[uid];
            if (global.id != changeId)
            {
#if DEBUG
                if (global.id == 0)
                    ValidateGlobalAccess<float[]>(uid, singleArrayGlobalLookup);
#endif
                for (int i = start, j = 0; i < end && j < global.value.Length; )
                    array[i++].X = global.value[j++];
                changeId = global.id;
            }
            bool result = global.id != changeId;
            return changeId != global.id;
        }

        #endregion set global, array types

        #region set global, texture

        public override sealed TextureSamplerState GetGlobalTextureSamplerState(int uid)
        {
#if DEBUG
            if (samplerGlobals[uid] == null)
                ValidateGlobalTextureAccess<TextureSamplerState>(uid, samplerGlobalLookup);
#endif
            return samplerGlobals[uid];
        }

        public override sealed Microsoft.Xna.Framework.Graphics.Texture GetGlobalTexture(int uid)
        {
#if DEBUG
            if (textureGlobals[uid] == null)
                ValidateGlobalTextureAccess<Microsoft.Xna.Framework.Graphics.Texture>(uid, textureGlobalLookup);
#endif
            return textureGlobals[uid];
        }

        public override sealed Microsoft.Xna.Framework.Graphics.Texture2D GetGlobalTexture2D(int uid)
        {
#if DEBUG
            if (texture2DGlobals[uid] == null)
                ValidateGlobalTextureAccess<Microsoft.Xna.Framework.Graphics.Texture2D>(uid, texture2DGlobalLookup);
#endif
            return texture2DGlobals[uid];
        }

        public override sealed Microsoft.Xna.Framework.Graphics.TextureCube GetGlobalTextureCube(int uid)
        {
#if DEBUG
            if (textureCubeGlobals[uid] == null)
                ValidateGlobalTextureAccess<Microsoft.Xna.Framework.Graphics.TextureCube>(uid, textureCubeGlobalLookup);
#endif
            return textureCubeGlobals[uid];
        }

        #endregion set global, texture

        #region User code: Get / Set shader globals from IShaderGlobals

        public void SetShaderGlobal(string name, Matrix[] value)
        {
            if (value == null)
                throw new ArgumentNullException();
            ShaderGlobal<Matrix[]> global = matrixArrayGlobals[GetIndex(ref matrixArrayGlobals, name, matrixArrayGlobalLookup)];

            global.value = value;
            global.id++;
            boundShaderDirty = true;
        }

        public void SetShaderGlobal(string name, Vector4[] value)
        {
            if (value == null)
                throw new ArgumentNullException();
            ShaderGlobal<Vector4[]> global = v4ArrayGlobals[GetIndex(ref v4ArrayGlobals, name, v4ArrayGlobalLookup)];

            global.value = value;
            global.id++;
            boundShaderDirty = true;
        }

        public void SetShaderGlobal(string name, Vector3[] value)
        {
            if (value == null)
                throw new ArgumentNullException();
            ShaderGlobal<Vector3[]> global = v3ArrayGlobals[GetIndex(ref v3ArrayGlobals, name, v3ArrayGlobalLookup)];

            global.value = value;
            global.id++;
            boundShaderDirty = true;
        }

        public void SetShaderGlobal(string name, Vector2[] value)
        {
            if (value == null)
                throw new ArgumentNullException();
            ShaderGlobal<Vector2[]> global = v2ArrayGlobals[GetIndex(ref v2ArrayGlobals, name, v2ArrayGlobalLookup)];

            global.value = value;
            global.id++;
            boundShaderDirty = true;
        }

        public void SetShaderGlobal(string name, float[] value)
        {
            if (value == null)
                throw new ArgumentNullException();
            ShaderGlobal<float[]> global = singleArrayGlobals[GetIndex(ref singleArrayGlobals, name, singleArrayGlobalLookup)];

            global.value = value;
            global.id++;
            boundShaderDirty = true;
        }

        public void SetShaderGlobal(string name, ref Matrix value)
        {
            ShaderGlobal<Matrix> global = matrixGlobals[GetIndex(ref matrixGlobals, name, matrixGlobalLookup)];
            if (global.id == 0 ||
                AppState.MatrixNotEqual(ref value, ref global.value))
            {
                global.value = value;
                global.id++;
                boundShaderDirty = true;
            }
        }

        public bool GetShaderGlobal(string name, out Matrix[] value)
        {
            value = null;
            int index;
            if (matrixArrayGlobalLookup.TryGetValue(name, out index))
            {
                value = matrixArrayGlobals[index].value;
                return true;
            }
            return false;
        }

        public bool GetShaderGlobal(string name, out Vector4[] value)
        {
            value = null;
            int index;
            if (v4ArrayGlobalLookup.TryGetValue(name, out index))
            {
                value = v4ArrayGlobals[index].value;
                return true;
            }
            return false;
        }

        public bool GetShaderGlobal(string name, out Vector3[] value)
        {
            value = null;
            int index;
            if (v3ArrayGlobalLookup.TryGetValue(name, out index))
            {
                value = v3ArrayGlobals[index].value;
                return true;
            }
            return false;
        }

        public bool GetShaderGlobal(string name, out Vector2[] value)
        {
            value = null;
            int index;
            if (v2ArrayGlobalLookup.TryGetValue(name, out index))
            {
                value = v2ArrayGlobals[index].value;
                return true;
            }
            return false;
        }

        public bool GetShaderGlobal(string name, out float[] value)
        {
            value = null;
            int index;
            if (singleArrayGlobalLookup.TryGetValue(name, out index))
            {
                value = singleArrayGlobals[index].value;
                return true;
            }
            return false;
        }

        public bool GetShaderGlobal(string name, out Matrix value)
        {
            int index;
            if (matrixGlobalLookup.TryGetValue(name, out index))
            {
                value = matrixGlobals[index].value;
                return true;
            }
            value = default(Matrix);
            return false;
        }

        public void SetShaderGlobal(string name, ref Vector4 value)
        {
            ShaderGlobal<Vector4> global = v4Globals[GetIndex(ref v4Globals, name, v4GlobalLookup)];
            if (global.id == 0 ||
                (value.X != global.value.X ||
                 value.Y != global.value.Y ||
                 value.Z != global.value.Z ||
                 value.W != global.value.W))
            {
                global.value = value;
                global.id++;
                boundShaderDirty = true;
            }
        }

        public bool GetShaderGlobal(string name, ref Vector4 value)
        {
            int index;
            if (v4GlobalLookup.TryGetValue(name, out index))
            {
                value = v4Globals[index].value;
                return true;
            }
            value = default(Vector4);
            return false;
        }

        public void SetShaderGlobal(string name, ref Vector3 value)
        {
            ShaderGlobal<Vector3> global = v3Globals[GetIndex(ref v3Globals, name, v3GlobalLookup)];
            if (global.id == 0 ||
                (value.X != global.value.X ||
                 value.Y != global.value.Y ||
                 value.Z != global.value.Z))
            {
                global.value = value;
                global.id++;
                boundShaderDirty = true;
            }
        }

        public bool GetShaderGlobal(string name, out Vector3 value)
        {
            int index;
            if (v3GlobalLookup.TryGetValue(name, out index))
            {
                value = v3Globals[index].value;
                return true;
            }
            value = default(Vector3);
            return false;
        }

        public void SetShaderGlobal(string name, ref Vector2 value)
        {
            ShaderGlobal<Vector2> global = v2Globals[GetIndex(ref v2Globals, name, v2GlobalLookup)];
            if (global.id == 0 ||
                (value.X != global.value.X ||
                 value.Y != global.value.Y))
            {
                global.value = value;
                global.id++;
                boundShaderDirty = true;
            }
        }

        public bool GetShaderGlobal(string name, out Vector2 value)
        {
            int index;
            if (v2GlobalLookup.TryGetValue(name, out index))
            {
                value = v2Globals[index].value;
                return true;
            }
            value = default(Vector2);
            return false;
        }

        public void SetShaderGlobal(string name, Vector4 value)
        {
            SetShaderGlobal(name, ref value);
        }

        public void SetShaderGlobal(string name, Vector3 value)
        {
            SetShaderGlobal(name, ref value);
        }

        public void SetShaderGlobal(string name, Vector2 value)
        {
            SetShaderGlobal(name, ref value);
        }

        public void SetShaderGlobal(string name, float value)
        {
            ShaderGlobal<float> global = singleGlobals[GetIndex(ref singleGlobals, name, singleGlobalLookup)];
            if (global.id == 0 ||
                value != global.value)
            {
                global.value = value;
                global.id++;
                boundShaderDirty = true;
            }
        }

        public void SetShaderGlobal(string name, bool value)
        {
            ShaderGlobal<bool> global = booleanGlobals[GetIndex(ref booleanGlobals, name, booleanGlobalLookup)];
            if (global.id == 0 ||
                value != global.value)
            {
                global.value = value;
                global.id++;
                boundShaderDirty = true;
            }
        }

        public bool GetShaderGlobal(string name, out float value)
        {
            int index;
            if (singleGlobalLookup.TryGetValue(name, out index))
            {
                value = singleGlobals[index].value;
                return true;
            }
            value = default(float);
            return false;
        }

        public void SetShaderGlobal(string name, TextureSamplerState value)
        {
            int i = GetIndexTexture(ref samplerGlobals, name, samplerGlobalLookup);
            samplerGlobals[i] = value;
            boundShaderDirty = true;
        }

        public void SetShaderGlobal(string name, Microsoft.Xna.Framework.Graphics.Texture value)
        {
            int i = GetIndexTexture(ref textureGlobals, name, textureGlobalLookup);
            textureGlobals[i] = value;
            boundShaderDirty = true;
        }

        public bool GetShaderGlobal(string name, out Microsoft.Xna.Framework.Graphics.Texture value)
        {
            int index;
            if (textureGlobalLookup.TryGetValue(name, out index))
            {
                value = textureGlobals[index];
                return true;
            }
            value = null;
            return false;
        }

        public void SetShaderGlobal(string name, Microsoft.Xna.Framework.Graphics.Texture2D value)
        {
            int i = GetIndexTexture(ref texture2DGlobals, name, texture2DGlobalLookup);
            texture2DGlobals[i] = value;
            boundShaderDirty = true;
        }

        public bool GetShaderGlobal(string name, out Microsoft.Xna.Framework.Graphics.Texture2D value)
        {
            int index;
            if (texture2DGlobalLookup.TryGetValue(name, out index))
            {
                value = texture2DGlobals[index];
                return true;
            }
            value = null;
            return false;
        }

        public void SetShaderGlobal(string name, Microsoft.Xna.Framework.Graphics.TextureCube value)
        {
            int i = GetIndexTexture(ref textureCubeGlobals, name, textureCubeGlobalLookup);
            textureCubeGlobals[i] = value;
            boundShaderDirty = true;
        }

        public bool GetShaderGlobal(string name, out Microsoft.Xna.Framework.Graphics.TextureCube value)
        {
            int index;
            if (textureCubeGlobalLookup.TryGetValue(name, out index))
            {
                value = textureCubeGlobals[index];
                return true;
            }
            value = null;
            return false;
        }

        #endregion User code: Get / Set shader globals from IShaderGlobals

        #region shader interaction setters/creation/init

        public override sealed void SetPixelShaderSamplers(Microsoft.Xna.Framework.Graphics.Texture[] textures, Xen.Graphics.TextureSamplerState[] samplers)
        {
            for (int index = 0; index < textures.Length; index++)
            {
                Microsoft.Xna.Framework.Graphics.Texture texture = textures[index];

                if (texture != null && texture != psTextures[index])
                {
                    graphics.Textures[index] = texture;
                    psTextures[index] = texture;
                }
            }

            for (int index = 0; index < samplers.Length; index++)
            {
                int value = samplers[index];

                if (value != psSamplerValue[index])
                {
                    psSamplerValue[index] = value;

                    if (!samplerMapping.TryGetValue(value, out psSamplerState[index]))
                    {
                        //create the sampler state
                        psSamplerState[index] = TextureSamplerStateInternal.BuildState(samplers[index]);
                        samplerMapping.Add(value, psSamplerState[index]);
                    }
                }

                graphics.SamplerStates[index] = psSamplerState[index];
            }
        }

        public override sealed void SetVertexShaderSamplers(Microsoft.Xna.Framework.Graphics.Texture[] textures, Xen.Graphics.TextureSamplerState[] samplers)
        {
            for (int index = 0; index < textures.Length; index++)
            {
                Microsoft.Xna.Framework.Graphics.Texture texture = textures[index];

                if (texture != null && texture != vsTextures[index])
                {
                    graphics.VertexTextures[index] = texture;
                    vsTextures[index] = texture;
                }
            }

            for (int index = 0; index < samplers.Length; index++)
            {
                int value = samplers[index];

                if (value != vsSamplerValue[index])
                {
                    vsSamplerValue[index] = value;

                    if (!samplerMapping.TryGetValue(value, out vsSamplerState[index]))
                    {
                        //create the sampler state
                        vsSamplerState[index] = TextureSamplerStateInternal.BuildState(samplers[index]);
                        samplerMapping.Add(value, vsSamplerState[index]);
                    }
                }

                graphics.VertexSamplerStates[index] = vsSamplerState[index];
            }
        }

        public override sealed void SetPixelShaderSampler(int index, Microsoft.Xna.Framework.Graphics.Texture texture, Xen.Graphics.TextureSamplerState sampler)
        {
            if (texture != null && texture != psTextures[index])
            {
                graphics.Textures[index] = texture;
                psTextures[index] = texture;
            }

            Microsoft.Xna.Framework.Graphics.SamplerState samplerState = graphics.SamplerStates[index];

            int value = sampler;
            if (value != psSamplerValue[index])
            {
                psSamplerValue[index] = value;

                if (!samplerMapping.TryGetValue(value, out psSamplerState[index]))
                {
                    //create the sampler state
                    psSamplerState[index] = TextureSamplerStateInternal.BuildState(sampler);
                    samplerMapping.Add(value, psSamplerState[index]);
                }
            }
            graphics.SamplerStates[index] = psSamplerState[index];
        }

        public override sealed void SetVertexShaderSampler(int index, Microsoft.Xna.Framework.Graphics.Texture texture, Xen.Graphics.TextureSamplerState sampler)
        {
            if (texture != null && texture != vsTextures[index])
            {
                graphics.VertexTextures[index] = texture;
                vsTextures[index] = texture;
            }

            Microsoft.Xna.Framework.Graphics.SamplerState samplerState = graphics.VertexSamplerStates[index];

            int value = sampler;
            if (value != vsSamplerValue[index])
            {
                vsSamplerValue[index] = value;

                if (!samplerMapping.TryGetValue(value, out vsSamplerState[index]))
                {
                    //create the sampler state
                    vsSamplerState[index] = TextureSamplerStateInternal.BuildState(sampler);
                    samplerMapping.Add(value, vsSamplerState[index]);
                }
            }
            graphics.VertexSamplerStates[index] = vsSamplerState[index];
        }

        public override void CreateEffect(
            out ShaderEffect effect,
            byte[] compressedEffectBytes,
            int vsInstructionCount,
            int psInstructionCount)
        {
            effect = new ShaderEffect();

            byte[] decompressed;

            //decompress shader bytes
            //#if XBOX360
            //simple run length encoding for the xbox, compression streams aren't supported. Usually gets about 2-3x smaller
            decompressed = Xen.Graphics.ShaderSystem.ShaderSystemBase.SimpleDecompress(compressedEffectBytes);
            //#else
            ////slightly more complex, decompress using a deflate stream. Usually gets about 8x smaller
            //using (System.IO.MemoryStream stream = new System.IO.MemoryStream(compressedEffectBytes))
            //using (System.IO.Compression.DeflateStream decompressor = new System.IO.Compression.DeflateStream(stream, System.IO.Compression.CompressionMode.Decompress))
            //using (System.IO.BinaryReader reader = new System.IO.BinaryReader(stream))
            //{
            //    int length = reader.ReadInt32();

            //    decompressed = new byte[length];
            //    decompressor.Read(decompressed, 0, decompressed.Length);
            //}
            //#endif
            //effect.Effect = SilverlightEffect.(this.graphics, decompressed);

            Microsoft.Xna.Framework.Graphics.EffectTechnique technique = effect.Effect.Techniques[0];
            effect.Effect.CurrentTechnique = technique;

            effect.Pass = technique.Passes[0];

            if (technique.Passes.Count > 1)
            {
                if (technique.Passes[1].Name == "Blending")
                    effect.BlendPass = technique.Passes[1];
                if (technique.Passes[1].Name == "Instancing")
                    effect.InstancePass = technique.Passes[1];
            }
            if (technique.Passes.Count > 2 && technique.Passes[2].Name == "Instancing")
                effect.InstancePass = technique.Passes[2];

            effect.vs_c = effect.Effect.Parameters["_vs_c"];
            effect.vs_b = effect.Effect.Parameters["_vs_b"];
            effect.ps_c = effect.Effect.Parameters["_ps_c"];
            effect.ps_b = effect.Effect.Parameters["_ps_b"];
            effect.vsb_c = effect.Effect.Parameters["_vsb_c"];
            effect.vsi_c = effect.Effect.Parameters["_vsi_c"];

#if DEBUG
            System.Threading.Interlocked.Increment(ref application.currentFrame.BinaryShadersCreated);
            effectInstructionCounts.Add(effect.Effect, new int[] { vsInstructionCount, psInstructionCount });
#endif
        }

        //REPLACE
        [Obsolete]
        public void ResetExtension()
        {
            extensionMode = ShaderExtension.None;
        }

        public override void SetEffect(IShader owner, ref ShaderEffect effect, ShaderExtension extension)
        {
            gpuBoundShader = owner;

            //boundEffect
            if (boundEffect != effect.Effect)
            {
                boundEffect = effect.Effect;

                //choose the pass to use
                if (extension == ShaderExtension.None)
                    boundPass = effect.Pass;
                else if (extension == ShaderExtension.Instancing)
                {
                    boundPass = effect.InstancePass;
                    if (boundPass == null) throw new InvalidOperationException(string.Format("The shader of type '{0}' does not support instancing", boundShader.GetType()));
                }
                else
                {
                    boundPass = effect.BlendPass;
                    if (boundPass == null) throw new InvalidOperationException(string.Format("The shader of type '{0}' does not support blending", boundShader.GetType()));
                }

                boundExtension = extension;

                if (effect.Effect != null)
                {
                    boundPass.Apply();
                }
            }
            else
            {
                //shader has changed extension mode
                if (boundExtension != extension)
                {
                    //choose the pass to use
                    if (extension == ShaderExtension.None)
                        boundPass = effect.Pass;
                    else if (extension == ShaderExtension.Instancing)
                        boundPass = effect.InstancePass;
                    else
                        boundPass = effect.BlendPass;

                    boundPass.Apply();
                    boundExtension = extension;
                }
                else if (boundEffect != null)
                    boundPass.Apply();	//commit changes
            }
        }

        #endregion shader interaction setters/creation/init

        #region IShaderSystem IValue Setters

        #region BLENDMATRICES

        public override bool SetBlendMatricesDirect(SilverlightEffectParameter param, ref int changeIndex)
        {
            if (blendChangeIdx != changeIndex)
            {
                param.SetValue(blendTransforms.MatrixData);
            }
            bool result = blendChangeIdx != changeIndex;
            changeIndex = blendChangeIdx; return result;
        }

        public override bool SetBlendMatricesVector4Array(Vector4[] array, int index, int count, ref int changeIndex)
        {
            if (blendChangeIdx != changeIndex)
            {
                Vector4[] source = blendTransforms.MatrixData;

                for (int i = 0; i < count && i < source.Length; i++)
                    array[index++] = source[i];
            }
            bool result = blendChangeIdx != changeIndex;
            changeIndex = blendChangeIdx; return result;
        }

        #endregion BLENDMATRICES

        #region CAMERA

        public override sealed bool SetViewDirectionVector3(ref Vector4 value, ref int changeIndex)
        {
            return camera.GetCameraViewDirection(ref value, ref changeIndex);
        }

        public override sealed bool SetViewDirectionVector4(ref Vector4 value, ref int changeIndex)
        {
            return camera.GetCameraViewDirection(ref value, ref changeIndex);
        }

        public override sealed bool SetCameraFovVector2(ref Vector4 value, ref int changeIndex)
        {
            return camera.GetCameraHorizontalVerticalFov(ref value, ref changeIndex);
        }

        public override sealed bool SetCameraFovTangentVector2(ref Vector4 value, ref int changeIndex)
        {
            return camera.GetCameraHorizontalVerticalFovTangent(ref value, ref changeIndex);
        }

        public override sealed bool SetCameraNearFarVector2(ref Vector4 value, ref int changeIndex)
        {
            return camera.GetCameraNearFarClip(ref value, ref changeIndex);
        }

        public override sealed bool SetViewPointVector3(ref Vector4 value, ref int changeIndex)
        {
            return camera.GetCameraPosition(ref value, ref changeIndex);
        }

        public override sealed bool SetViewPointVector4(ref Vector4 value, ref int changeIndex)
        {
            return camera.GetCameraPosition(ref value, ref changeIndex);
        }

        #endregion CAMERA

        #region WORLD matrix

        public override sealed bool SetWorldMatrix(ref Vector4 x, ref Vector4 y, ref Vector4 z, ref Vector4 w, ref int changeIndex)
        {
            WorldMatrixStackProvider target = matrices.World;

            if (target.index != changeIndex)
            {
                x.X = target.value.M11; x.Y = target.value.M21; x.Z = target.value.M31; x.W = target.value.M41;
                y.X = target.value.M12; y.Y = target.value.M22; y.Z = target.value.M32; y.W = target.value.M42;
                z.X = target.value.M13; z.Y = target.value.M23; z.Z = target.value.M33; z.W = target.value.M43;
                w.X = target.value.M14; w.Y = target.value.M24; w.Z = target.value.M34; w.W = target.value.M44;
            }
            bool result = changeIndex != target.index;
            changeIndex = target.index; return result;
        }

        public override sealed bool SetWorldInverseMatrix(ref Vector4 x, ref Vector4 y, ref Vector4 z, ref Vector4 w, ref int changeIndex)
        {
            WorldMatrixStackProvider target = matrices.World;

            if (target.index != changeIndex)
            {
                if (target.inverseIndex != target.index)
                    target.UpdateInverse();

                x.X = target.value.M11; x.Y = target.value.M21; x.Z = target.value.M31; x.W = target.value.M41;
                y.X = target.value.M12; y.Y = target.value.M22; y.Z = target.value.M32; y.W = target.value.M42;
                z.X = target.value.M13; z.Y = target.value.M23; z.Z = target.value.M33; z.W = target.value.M43;
                w.X = target.value.M14; w.Y = target.value.M24; w.Z = target.value.M34; w.W = target.value.M44;
            }
            bool result = changeIndex != target.index;
            changeIndex = target.index; return result;
        }

        public override sealed bool SetWorldTransposeMatrix(ref Vector4 x, ref Vector4 y, ref Vector4 z, ref Vector4 w, ref int changeIndex)
        {
            WorldMatrixStackProvider target = matrices.World;

            if (target.index != changeIndex)
            {
                if (target.transposeIndex != target.index)
                    target.UpdateTranspose();

                x.X = target.value.M11; x.Y = target.value.M21; x.Z = target.value.M31; x.W = target.value.M41;
                y.X = target.value.M12; y.Y = target.value.M22; y.Z = target.value.M32; y.W = target.value.M42;
                z.X = target.value.M13; z.Y = target.value.M23; z.Z = target.value.M33; z.W = target.value.M43;
                w.X = target.value.M14; w.Y = target.value.M24; w.Z = target.value.M34; w.W = target.value.M44;
            }
            bool result = changeIndex != target.index;
            changeIndex = target.index; return result;
        }

        #endregion WORLD matrix

        #region PROJECTION matrix

        public override sealed bool SetProjectionMatrix(ref Vector4 x, ref Vector4 y, ref Vector4 z, ref Vector4 w, ref int changeIndex)
        {
            ProjectionProvider target = matrices.Projection;

            if (target.index != changeIndex)
            {
                x.X = target.value.M11; x.Y = target.value.M21; x.Z = target.value.M31; x.W = target.value.M41;
                y.X = target.value.M12; y.Y = target.value.M22; y.Z = target.value.M32; y.W = target.value.M42;
                z.X = target.value.M13; z.Y = target.value.M23; z.Z = target.value.M33; z.W = target.value.M43;
                w.X = target.value.M14; w.Y = target.value.M24; w.Z = target.value.M34; w.W = target.value.M44;
            }
            bool result = changeIndex != target.index;
            changeIndex = target.index; return result;
        }

        public override sealed bool SetProjectionInverseMatrix(ref Vector4 x, ref Vector4 y, ref Vector4 z, ref Vector4 w, ref int changeIndex)
        {
            ProjectionProvider target = matrices.Projection;

            if (target.index != changeIndex)
            {
                if (target.inverseIndex != target.index)
                    target.UpdateInverse();

                x.X = target.value.M11; x.Y = target.value.M21; x.Z = target.value.M31; x.W = target.value.M41;
                y.X = target.value.M12; y.Y = target.value.M22; y.Z = target.value.M32; y.W = target.value.M42;
                z.X = target.value.M13; z.Y = target.value.M23; z.Z = target.value.M33; z.W = target.value.M43;
                w.X = target.value.M14; w.Y = target.value.M24; w.Z = target.value.M34; w.W = target.value.M44;
            }
            bool result = changeIndex != target.index;
            changeIndex = target.index; return result;
        }

        public override sealed bool SetProjectionTransposeMatrix(ref Vector4 x, ref Vector4 y, ref Vector4 z, ref Vector4 w, ref int changeIndex)
        {
            ProjectionProvider target = matrices.Projection;

            if (target.index != changeIndex)
            {
                if (target.transposeIndex != target.index)
                    target.UpdateTranspose();

                x.X = target.value.M11; x.Y = target.value.M21; x.Z = target.value.M31; x.W = target.value.M41;
                y.X = target.value.M12; y.Y = target.value.M22; y.Z = target.value.M32; y.W = target.value.M42;
                z.X = target.value.M13; z.Y = target.value.M23; z.Z = target.value.M33; z.W = target.value.M43;
                w.X = target.value.M14; w.Y = target.value.M24; w.Z = target.value.M34; w.W = target.value.M44;
            }
            bool result = changeIndex != target.index;
            changeIndex = target.index; return result;
        }

        #endregion PROJECTION matrix

        #region VIEW matrix

        public override sealed bool SetViewMatrix(ref Vector4 x, ref Vector4 y, ref Vector4 z, ref Vector4 w, ref int changeIndex)
        {
            ViewProvider target = matrices.View;

            if (target.index != changeIndex)
            {
                x.X = target.value.M11; x.Y = target.value.M21; x.Z = target.value.M31; x.W = target.value.M41;
                y.X = target.value.M12; y.Y = target.value.M22; y.Z = target.value.M32; y.W = target.value.M42;
                z.X = target.value.M13; z.Y = target.value.M23; z.Z = target.value.M33; z.W = target.value.M43;
                w.X = target.value.M14; w.Y = target.value.M24; w.Z = target.value.M34; w.W = target.value.M44;
            }
            bool result = changeIndex != target.index;
            changeIndex = target.index; return result;
        }

        public override sealed bool SetViewInverseMatrix(ref Vector4 x, ref Vector4 y, ref Vector4 z, ref Vector4 w, ref int changeIndex)
        {
            ViewProvider target = matrices.View;

            if (target.index != changeIndex)
            {
                if (target.inverseIndex != target.index)
                    target.UpdateInverse();

                x.X = target.value.M11; x.Y = target.value.M21; x.Z = target.value.M31; x.W = target.value.M41;
                y.X = target.value.M12; y.Y = target.value.M22; y.Z = target.value.M32; y.W = target.value.M42;
                z.X = target.value.M13; z.Y = target.value.M23; z.Z = target.value.M33; z.W = target.value.M43;
                w.X = target.value.M14; w.Y = target.value.M24; w.Z = target.value.M34; w.W = target.value.M44;
            }
            bool result = changeIndex != target.index;
            changeIndex = target.index; return result;
        }

        public override sealed bool SetViewTransposeMatrix(ref Vector4 x, ref Vector4 y, ref Vector4 z, ref Vector4 w, ref int changeIndex)
        {
            ViewProvider target = matrices.View;

            if (target.index != changeIndex)
            {
                if (target.transposeIndex != target.index)
                    target.UpdateTranspose();

                x.X = target.value.M11; x.Y = target.value.M21; x.Z = target.value.M31; x.W = target.value.M41;
                y.X = target.value.M12; y.Y = target.value.M22; y.Z = target.value.M32; y.W = target.value.M42;
                z.X = target.value.M13; z.Y = target.value.M23; z.Z = target.value.M33; z.W = target.value.M43;
                w.X = target.value.M14; w.Y = target.value.M24; w.Z = target.value.M34; w.W = target.value.M44;
            }
            bool result = changeIndex != target.index;
            changeIndex = target.index; return result;
        }

        #endregion VIEW matrix

        #region VIEW * PROJECTION matrix

        public override sealed bool SetViewProjectionMatrix(ref Vector4 x, ref Vector4 y, ref Vector4 z, ref Vector4 w, ref int changeIndex)
        {
            MatrixMultiply target = matrices.ViewProjection;

            if (target.index != changeIndex)
            {
                if (target.indexMultiply != target.index)
                    target.UpdateValue();

                x.X = target.value.M11; x.Y = target.value.M21; x.Z = target.value.M31; x.W = target.value.M41;
                y.X = target.value.M12; y.Y = target.value.M22; y.Z = target.value.M32; y.W = target.value.M42;
                z.X = target.value.M13; z.Y = target.value.M23; z.Z = target.value.M33; z.W = target.value.M43;
                w.X = target.value.M14; w.Y = target.value.M24; w.Z = target.value.M34; w.W = target.value.M44;
            }
            bool result = changeIndex != target.index;
            changeIndex = target.index; return result;
        }

        public override sealed bool SetViewProjectionInverseMatrix(ref Vector4 x, ref Vector4 y, ref Vector4 z, ref Vector4 w, ref int changeIndex)
        {
            MatrixMultiply target = matrices.ViewProjection;

            if (target.index != changeIndex)
            {
                if (target.indexMultiply != target.index)
                    target.UpdateValue();
                if (target.inverseIndex != target.index)
                    target.UpdateInverse();

                x.X = target.value.M11; x.Y = target.value.M21; x.Z = target.value.M31; x.W = target.value.M41;
                y.X = target.value.M12; y.Y = target.value.M22; y.Z = target.value.M32; y.W = target.value.M42;
                z.X = target.value.M13; z.Y = target.value.M23; z.Z = target.value.M33; z.W = target.value.M43;
                w.X = target.value.M14; w.Y = target.value.M24; w.Z = target.value.M34; w.W = target.value.M44;
            }
            bool result = changeIndex != target.index;
            changeIndex = target.index; return result;
        }

        public override sealed bool SetViewProjectionTransposeMatrix(ref Vector4 x, ref Vector4 y, ref Vector4 z, ref Vector4 w, ref int changeIndex)
        {
            MatrixMultiply target = matrices.ViewProjection;

            if (target.index != changeIndex)
            {
                if (target.indexMultiply != target.index)
                    target.UpdateValue();
                if (target.transposeIndex != target.index)
                    target.UpdateTranspose();

                x.X = target.value.M11; x.Y = target.value.M21; x.Z = target.value.M31; x.W = target.value.M41;
                y.X = target.value.M12; y.Y = target.value.M22; y.Z = target.value.M32; y.W = target.value.M42;
                z.X = target.value.M13; z.Y = target.value.M23; z.Z = target.value.M33; z.W = target.value.M43;
                w.X = target.value.M14; w.Y = target.value.M24; w.Z = target.value.M34; w.W = target.value.M44;
            }
            bool result = changeIndex != target.index;
            changeIndex = target.index; return result;
        }

        #endregion VIEW * PROJECTION matrix

        #region WORLD * PROJECTION matrix

        public override sealed bool SetWorldProjectionMatrix(ref Vector4 x, ref Vector4 y, ref Vector4 z, ref Vector4 w, ref int changeIndex)
        {
            MatrixMultiply target = matrices.WorldProjection;

            if (target.index != changeIndex)
            {
                if (target.indexMultiply != target.index)
                    target.UpdateValue();

                x.X = target.value.M11; x.Y = target.value.M21; x.Z = target.value.M31; x.W = target.value.M41;
                y.X = target.value.M12; y.Y = target.value.M22; y.Z = target.value.M32; y.W = target.value.M42;
                z.X = target.value.M13; z.Y = target.value.M23; z.Z = target.value.M33; z.W = target.value.M43;
                w.X = target.value.M14; w.Y = target.value.M24; w.Z = target.value.M34; w.W = target.value.M44;
            }
            bool result = changeIndex != target.index;
            changeIndex = target.index; return result;
        }

        public override sealed bool SetWorldProjectionInverseMatrix(ref Vector4 x, ref Vector4 y, ref Vector4 z, ref Vector4 w, ref int changeIndex)
        {
            MatrixMultiply target = matrices.WorldProjection;

            if (target.index != changeIndex)
            {
                if (target.indexMultiply != target.index)
                    target.UpdateValue();
                if (target.inverseIndex != target.index)
                    target.UpdateInverse();

                x.X = target.value.M11; x.Y = target.value.M21; x.Z = target.value.M31; x.W = target.value.M41;
                y.X = target.value.M12; y.Y = target.value.M22; y.Z = target.value.M32; y.W = target.value.M42;
                z.X = target.value.M13; z.Y = target.value.M23; z.Z = target.value.M33; z.W = target.value.M43;
                w.X = target.value.M14; w.Y = target.value.M24; w.Z = target.value.M34; w.W = target.value.M44;
            }
            bool result = changeIndex != target.index;
            changeIndex = target.index; return result;
        }

        public override sealed bool SetWorldProjectionTransposeMatrix(ref Vector4 x, ref Vector4 y, ref Vector4 z, ref Vector4 w, ref int changeIndex)
        {
            MatrixMultiply target = matrices.WorldProjection;

            if (target.index != changeIndex)
            {
                if (target.indexMultiply != target.index)
                    target.UpdateValue();
                if (target.transposeIndex != target.index)
                    target.UpdateTranspose();

                x.X = target.value.M11; x.Y = target.value.M21; x.Z = target.value.M31; x.W = target.value.M41;
                y.X = target.value.M12; y.Y = target.value.M22; y.Z = target.value.M32; y.W = target.value.M42;
                z.X = target.value.M13; z.Y = target.value.M23; z.Z = target.value.M33; z.W = target.value.M43;
                w.X = target.value.M14; w.Y = target.value.M24; w.Z = target.value.M34; w.W = target.value.M44;
            }
            bool result = changeIndex != target.index;
            changeIndex = target.index; return result;
        }

        #endregion WORLD * PROJECTION matrix

        #region WORLD * VIEW matrix

        public override sealed bool SetWorldViewMatrix(ref Vector4 x, ref Vector4 y, ref Vector4 z, ref Vector4 w, ref int changeIndex)
        {
            MatrixMultiply target = matrices.WorldView;

            if (target.index != changeIndex)
            {
                if (target.indexMultiply != target.index)
                    target.UpdateValue();

                x.X = target.value.M11; x.Y = target.value.M21; x.Z = target.value.M31; x.W = target.value.M41;
                y.X = target.value.M12; y.Y = target.value.M22; y.Z = target.value.M32; y.W = target.value.M42;
                z.X = target.value.M13; z.Y = target.value.M23; z.Z = target.value.M33; z.W = target.value.M43;
                w.X = target.value.M14; w.Y = target.value.M24; w.Z = target.value.M34; w.W = target.value.M44;
            }
            bool result = changeIndex != target.index;
            changeIndex = target.index; return result;
        }

        public override sealed bool SetWorldViewInverseMatrix(ref Vector4 x, ref Vector4 y, ref Vector4 z, ref Vector4 w, ref int changeIndex)
        {
            MatrixMultiply target = matrices.WorldView;

            if (target.index != changeIndex)
            {
                if (target.indexMultiply != target.index)
                    target.UpdateValue();
                if (target.inverseIndex != target.index)
                    target.UpdateInverse();

                x.X = target.value.M11; x.Y = target.value.M21; x.Z = target.value.M31; x.W = target.value.M41;
                y.X = target.value.M12; y.Y = target.value.M22; y.Z = target.value.M32; y.W = target.value.M42;
                z.X = target.value.M13; z.Y = target.value.M23; z.Z = target.value.M33; z.W = target.value.M43;
                w.X = target.value.M14; w.Y = target.value.M24; w.Z = target.value.M34; w.W = target.value.M44;
            }
            bool result = changeIndex != target.index;
            changeIndex = target.index; return result;
        }

        public override sealed bool SetWorldViewTransposeMatrix(ref Vector4 x, ref Vector4 y, ref Vector4 z, ref Vector4 w, ref int changeIndex)
        {
            MatrixMultiply target = matrices.WorldView;

            if (target.index != changeIndex)
            {
                if (target.indexMultiply != target.index)
                    target.UpdateValue();
                if (target.transposeIndex != target.index)
                    target.UpdateTranspose();

                x.X = target.value.M11; x.Y = target.value.M21; x.Z = target.value.M31; x.W = target.value.M41;
                y.X = target.value.M12; y.Y = target.value.M22; y.Z = target.value.M32; y.W = target.value.M42;
                z.X = target.value.M13; z.Y = target.value.M23; z.Z = target.value.M33; z.W = target.value.M43;
                w.X = target.value.M14; w.Y = target.value.M24; w.Z = target.value.M34; w.W = target.value.M44;
            }
            bool result = changeIndex != target.index;
            changeIndex = target.index; return result;
        }

        #endregion WORLD * VIEW matrix

        #region WORLD * VIEW * PROJECTION matrix

        public override sealed bool SetWorldViewProjectionMatrix(ref Vector4 x, ref Vector4 y, ref Vector4 z, ref Vector4 w, ref int changeIndex)
        {
            MatrixMultiply target = matrices.WorldViewProjection;
            MatrixMultiply viewprojection = matrices.ViewProjection;

            if (target.index != changeIndex)
            {
                if (viewprojection.indexMultiply != viewprojection.index)
                    viewprojection.UpdateValue();
                if (target.indexMultiply != target.index)
                    target.UpdateValue();

                x.X = target.value.M11; x.Y = target.value.M21; x.Z = target.value.M31; x.W = target.value.M41;
                y.X = target.value.M12; y.Y = target.value.M22; y.Z = target.value.M32; y.W = target.value.M42;
                z.X = target.value.M13; z.Y = target.value.M23; z.Z = target.value.M33; z.W = target.value.M43;
                w.X = target.value.M14; w.Y = target.value.M24; w.Z = target.value.M34; w.W = target.value.M44;
            }
            bool result = changeIndex != target.index;
            changeIndex = target.index; return result;
        }

        public override sealed bool SetWorldViewProjectionInverseMatrix(ref Vector4 x, ref Vector4 y, ref Vector4 z, ref Vector4 w, ref int changeIndex)
        {
            MatrixMultiply target = matrices.WorldViewProjection;
            MatrixMultiply viewprojection = matrices.ViewProjection;

            if (target.index != changeIndex)
            {
                if (viewprojection.indexMultiply != viewprojection.index)
                    viewprojection.UpdateValue();
                if (target.indexMultiply != target.index)
                    target.UpdateValue();
                if (target.inverseIndex != target.index)
                    target.UpdateInverse();

                x.X = target.value.M11; x.Y = target.value.M21; x.Z = target.value.M31; x.W = target.value.M41;
                y.X = target.value.M12; y.Y = target.value.M22; y.Z = target.value.M32; y.W = target.value.M42;
                z.X = target.value.M13; z.Y = target.value.M23; z.Z = target.value.M33; z.W = target.value.M43;
                w.X = target.value.M14; w.Y = target.value.M24; w.Z = target.value.M34; w.W = target.value.M44;
            }
            bool result = changeIndex != target.index;
            changeIndex = target.index; return result;
        }

        public override sealed bool SetWorldViewProjectionTransposeMatrix(ref Vector4 x, ref Vector4 y, ref Vector4 z, ref Vector4 w, ref int changeIndex)
        {
            MatrixMultiply target = matrices.WorldViewProjection;
            MatrixMultiply viewprojection = matrices.ViewProjection;

            if (target.index != changeIndex)
            {
                if (viewprojection.indexMultiply != viewprojection.index)
                    viewprojection.UpdateValue();
                if (target.indexMultiply != target.index)
                    target.UpdateValue();
                if (target.transposeIndex != target.index)
                    target.UpdateTranspose();

                x.X = target.value.M11; x.Y = target.value.M21; x.Z = target.value.M31; x.W = target.value.M41;
                y.X = target.value.M12; y.Y = target.value.M22; y.Z = target.value.M32; y.W = target.value.M42;
                z.X = target.value.M13; z.Y = target.value.M23; z.Z = target.value.M33; z.W = target.value.M43;
                w.X = target.value.M14; w.Y = target.value.M24; w.Z = target.value.M34; w.W = target.value.M44;
            }
            bool result = changeIndex != target.index;
            changeIndex = target.index; return result;
        }

        #endregion WORLD * VIEW * PROJECTION matrix

        #region MISC

        public override sealed bool SetWindowSizeVector2(ref Vector4 value, ref int changeIndex)
        {
            return this.state.DrawTarget.GetWidthHeightAsVector(out value, ref changeIndex);
        }

        public override sealed bool SetVertexCountSingle(ref Vector4 value, ref int changeIndex)
        {
            value.X = (float)(bufferVertexCount);

            bool result = changeIndex != this.bufferVertexCountChangeIndex;
            changeIndex = this.bufferVertexCountChangeIndex;
            return result;
        }

        #endregion MISC

        #endregion IShaderSystem IValue Setters

        #region Effect param mapping

        private sealed class EffectParamMapper
        {
            #region internal param mapping instances

            struct MatrixMapping
            {
                public MatrixMapping(MatrixSource target, MatrixSource source)
                {
                    this.target = target;
                    this.source = source;
                    this.multiply = target is MatrixMultiply;
                    this.sourceMultiply = source is MatrixMultiply;
                    this.changeIndex = -1;
                    this.inverse = false;
                    this.transpose = false;
                    this.param = null;
                }

                public readonly MatrixSource target, source;
                public readonly bool multiply, sourceMultiply;
                public bool transpose, inverse;
                public SilverlightEffectParameter param;
                public int changeIndex;
            }

            private sealed class ParamMapping
            {
                private readonly MatrixMapping[] map;
                private readonly SilverlightEffectParameter blendMatrices;
                private int blendChangedIndex;

                public ParamMapping(MatrixMapping[] map, SilverlightEffectParameter blendMatrices)
                {
                    if (map == null) throw new ArgumentNullException();
                    this.map = map;
                    this.blendChangedIndex = -1;
                    this.blendMatrices = blendMatrices;
                }

                public bool Apply(ShaderSystemState parent, ShaderExtension extension)
                {
                    bool changed = false;

                    for (int i = 0; i < map.Length; i++)
                    {
                        MatrixMapping entry = map[i];
                        if (entry.changeIndex != entry.target.index)
                        {
                            if (entry.sourceMultiply)
                                entry.source.UpdateMultiply();
                            if (entry.multiply)
                                entry.target.UpdateMultiply();
                            if (entry.inverse)
                            {
                                if (entry.target.inverseIndex != entry.target.index)
                                    entry.target.UpdateInverse();
                                entry.param.SetValue(entry.target.inverse);
                            }
                            else if (entry.transpose)
                            {
                                if (entry.target.transposeIndex != entry.target.index)
                                    entry.target.UpdateTranspose();
                                entry.param.SetValue(entry.target.transpose);
                            }
                            else
                                entry.param.SetValue(entry.target.value);

                            map[i].changeIndex = entry.target.index;
                            changed = true;
                        }
                    }

                    if (extension == ShaderExtension.Blending && this.blendMatrices != null && parent.blendChangeIdx != this.blendChangedIndex)
                    {
                        this.blendMatrices.SetValue(parent.blendTransforms.MatrixData);
                        this.blendChangedIndex = parent.blendChangeIdx;
                        changed = true;
                    }

                    return changed;
                }
            }

            #endregion internal param mapping instances

            private enum SpecialMapping
            {
                BlendMatrices,
            }

            private readonly Dictionary<string, MatrixMapping> matrixMap;
            private readonly Dictionary<string, SpecialMapping> specialMap;
            private readonly List<MatrixMapping> matrixMapList;
            private readonly Dictionary<Effect, ParamMapping> effectMap;
            private readonly EventHandler<EventArgs> disposeEvent;
            private readonly ShaderSystemState shaderSystem;

            public EffectParamMapper(DrawState state)
            {
                matrixMap = new Dictionary<string, MatrixMapping>(StringComparer.InvariantCultureIgnoreCase);
                specialMap = new Dictionary<string, SpecialMapping>(StringComparer.InvariantCultureIgnoreCase);
                effectMap = new Dictionary<Effect, ParamMapping>();
                disposeEvent = new EventHandler<EventArgs>(OnEffectDisposed);
                matrixMapList = new List<MatrixMapping>();
                shaderSystem = state.shaderSystem;

                MatrixProviderCollection list = state.matrices;

                Add(list.World, null, "world");
                Add(list.View, null, "view");
                Add(list.Projection, null, "projection");
                Add(list.ViewProjection, null, "viewprojection");
                Add(list.WorldView, null, "worldview");
                Add(list.WorldProjection, null, "worldprojection");
                Add(list.WorldViewProjection, list.ViewProjection, "worldviewprojection");

                specialMap.Add("blendmatrices", SpecialMapping.BlendMatrices);
            }

            private void Add(MatrixSource target, MatrixSource source, string name)
            {
                MatrixMapping map = new MatrixMapping(target, source);
                matrixMap.Add(name, map);
                map.inverse = true;
                matrixMap.Add(name + "inverse", map);
                map.inverse = false;
                map.transpose = true;
                matrixMap.Add(name + "transpose", map);
            }

            //returns true if the shader has changed
            public bool ApplyMapping(SilverlightEffect effect, ShaderExtension extension)
            {
                ParamMapping mapping;
                if (!effectMap.TryGetValue(effect, out mapping))
                {
                    SilverlightEffectParameter blendParam = null;
                    foreach (SilverlightEffectParameter param in effect.Parameters)
                    {
                        string semantic = param.Semantic;
                        if (semantic != null)
                        {
                            MatrixMapping map;
                            SpecialMapping special;
                            if (matrixMap.TryGetValue(semantic, out map))
                            {
                                map.param = param;
                                matrixMapList.Add(map);
                            }
                            else if (specialMap.TryGetValue(semantic, out special))
                            {
                                blendParam = param;
                            }
                        }
                    }
                    mapping = new ParamMapping(matrixMapList.ToArray(), blendParam);
                    matrixMapList.Clear();
                    effectMap.Add(effect, mapping);

                    //effect.Disposing += disposeEvent;
                }
                return mapping.Apply(shaderSystem, extension);
            }

            private void OnEffectDisposed(object sender, EventArgs e)
            {
                if (sender is Effect)
                {
                    //(sender as Effect).Disposing -= disposeEvent;
                    effectMap.Remove(sender as Effect);
                }
            }
        }

        #endregion Effect param mapping
    }
}