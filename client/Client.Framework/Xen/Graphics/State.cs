using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Xen.Graphics.Stack;

namespace Xen.Graphics
{
    /// <summary>
    /// Flags to represent aspects of the graphics render state that may be tracked by the application
    /// </summary>
    /// <remarks>
    /// <para>This flag is used to specify what render states has become 'dirty'</para>
    /// <para>Internally the application keeps track of what it thinks the render state is, to make changing state more efficient.</para>
    /// <para>By specifying that a render state group is dirty, the application ignores what it thinks the state is, and assumes it is entirely different.</para>
    /// <para>This forces the application to reapply all states the next time something is drawn.</para>
    /// </remarks>
    [Flags]
    public enum StateFlag
    {
        /// <summary>
        /// No state flag
        /// </summary>
        None = 0,
        /// <summary>
        /// AlphaBlending render states (<see cref="AlphaBlendState"/>)
        /// </summary>
        AlphaBlend = 1,
        /// <summary>
        /// Stencil testing render states (<see cref="StencilState"/>)
        /// </summary>
        StencilTest = 2,

        //AlphaTest will be removed in XNA 4
        /*	/// <summary>
            /// Alpha testing render states (<see cref="AlphaTestState"/>)
            /// </summary>
            AlphaTest = 4,*/

        /// <summary>
        /// Depth testing, Colour writed and face culling render states (<see cref="RasterState"/>)
        /// </summary>
        Raster = 8,
        /// <summary>
        /// Vertex and pixel shaders currently bound (XNA <see cref="Effect"/> objects also change vertex and pixel shaders)
        /// </summary>
        Shaders = 16,
        /// <summary>
        /// Pixel and vertex shader texture and sampler states
        /// </summary>
        Textures = 32,
        /// <summary>
        /// Currently bound vertex streams, vertex declaration and indices
        /// </summary>
        VerticesAndIndices = 64,
        /// <summary>
        /// Every state tracked by the application (Prefer using a combination of the other flags if possible)
        /// </summary>
        All = AlphaBlend | StencilTest /*| AlphaTest*/ | Raster | Shaders | Textures | VerticesAndIndices
    }
}

namespace Xen.Graphics.Stack
{
    [System.Diagnostics.DebuggerStepThrough]
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Explicit, Size = 16)]
    struct InternalState
    {
        [System.Runtime.InteropServices.FieldOffset(0)]
        public ulong StencilValue;
        [System.Runtime.InteropServices.FieldOffset(8)]
        public ulong UnmaskedBlendValue;
#if XBOX360
		[System.Runtime.InteropServices.FieldOffset(14)]
		public ushort UnmaskedRasterValue;
		[System.Runtime.InteropServices.FieldOffset(12)]
		public byte DepthValue;

		public const ulong BlendMask = (0xFFFFFFFFLU << 32) | RasterState.BlendMask;
#else
        [System.Runtime.InteropServices.FieldOffset(12)]
        public ushort UnmaskedRasterValue;
        [System.Runtime.InteropServices.FieldOffset(15)]
        public byte DepthValue;

        public const ulong BlendMask = 0xFFFFFFFFL | (RasterState.BlendMask << 32);
#endif

        public const uint RasterMask = RasterState.ModeMask;

        [System.Runtime.InteropServices.FieldOffset(0)]
        public StencilState Stencil;
        [System.Runtime.InteropServices.FieldOffset(8)]
        public AlphaBlendState Blend;
#if XBOX360
		[System.Runtime.InteropServices.FieldOffset(14)]
		public RasterState Raster;
		[System.Runtime.InteropServices.FieldOffset(12)]
		public DepthState Depth;
#else
        [System.Runtime.InteropServices.FieldOffset(12)]
        public RasterState Raster;
        [System.Runtime.InteropServices.FieldOffset(15)]
        public DepthState Depth;
#endif
    }

    /// <summary>
    /// Stores the <see cref="DeviceRenderState"/> stack. Use <see cref="Set"/> or <see cref="Push(ref DeviceRenderState)"/> to set the entire render state.
    /// </summary>
#if !DEBUG_API

    [System.Diagnostics.DebuggerStepThrough]
#endif
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Explicit)]
    public sealed class DeviceRenderStateStack
    {
        /// <summary>
        /// Structure used for a using block with a Push method
        /// </summary>
        [System.Diagnostics.DebuggerStepThrough]
        public struct UsingPop : IDisposable
        {
            internal UsingPop(DeviceRenderStateStack stack)
            {
                this.stack = stack;
            }

            private readonly DeviceRenderStateStack stack;

            /// <summary>Invokes the Pop metohd</summary>
            public void Dispose()
            {
                stack.state = stack.members.renderStateStack[checked(--stack.members.renderStateStackIndex)];
            }
        }

        [System.Runtime.InteropServices.FieldOffset(32)]
        internal readonly UsingPop UsingBlock;

        //push operator:
        /// <summary>
        /// Wrapper on the Push method
        /// </summary>
        public static UsingPop operator +(DeviceRenderStateStack stack, DeviceRenderState state)
        {
            return stack.Push(ref state);
        }

        /// <summary>
        /// Wrapper on the Push method
        /// </summary>
        public static UsingPop operator +(DeviceRenderStateStack stack, AlphaBlendState state)
        {
            return stack.Push(ref state);
        }

        /// <summary>
        /// Wrapper on the Push method
        /// </summary>
        public static UsingPop operator +(DeviceRenderStateStack stack, StencilState state)
        {
            return stack.Push(ref state);
        }

        /// <summary>
        /// Wrapper on the Push method
        /// </summary>
        public static UsingPop operator +(DeviceRenderStateStack stack, RasterState state)
        {
            return stack.Push(ref state);
        }

        internal DeviceRenderStateStack(DrawState state)
        {
            this.members = new InternalMembers(state);
            this.UsingBlock = new UsingPop(this);
        }

        [System.Runtime.InteropServices.FieldOffset(0)]
        internal InternalState state;

        internal const uint ForceDisableDepthMask = 32;
        [System.Runtime.InteropServices.FieldOffset(16)]
        internal uint ForceDisableDepthBit;

        internal const uint ReverseCullModeMask = 16384;
        [System.Runtime.InteropServices.FieldOffset(24)]
        internal uint ReverseCullBit;

        /// <summary>
        /// Get/Set/Modify the current stencil test render states to be used during rendering
        /// </summary>
        [System.Runtime.InteropServices.FieldOffset(0)]
        public StencilState CurrentStencilState;

        /// <summary>
        /// Get/Set/Modify the current alpha blending render states to be used during rendering
        /// </summary>
        [System.Runtime.InteropServices.FieldOffset(8)]
        public AlphaBlendState CurrentBlendState;

        /// <summary>
        /// Get/Set/Modify the current depth test, colour write and face culling render states to be used during rendering
        /// </summary>
#if XBOX360
		[System.Runtime.InteropServices.FieldOffset(14)]
#else
        [System.Runtime.InteropServices.FieldOffset(12)]
#endif
        public RasterState CurrentRasterState;

        /// <summary>
        /// Get/Set/Modify the current depth render states to be used during rendering
        /// </summary>
#if XBOX360
		[System.Runtime.InteropServices.FieldOffset(12)]
#else
        [System.Runtime.InteropServices.FieldOffset(15)]
#endif
        public DepthState CurrentDepthState;

        //package the rest of the members into a struct, as otherwise it would require field offset attributes for all of them
        internal struct InternalMembers
        {
            public readonly InternalState[] renderStateStack;
            public uint renderStateStackIndex;
            public StateFlag internalStateDirty;
            public readonly DrawState parent;

            public readonly Dictionary<ulong, BlendState> BlendMapping;
            public readonly Dictionary<ulong, DepthStencilState> DepthStencilMapping;
            public readonly Dictionary<uint, RasterizerState> RasterMapping;

            public ulong LastBlendMapping, LastDepthStencilMapping;
            public uint LastRasterMapping;

            public BlendState LastAlphaBlendState;
            public DepthStencilState LastDepthStencilState;
            public RasterizerState LastRasterizerState;

            public InternalMembers(DrawState state)
            {
                this = new InternalMembers();
                renderStateStack = new InternalState[StackSize];
                internalStateDirty = StateFlag.All;
                this.parent = state;

                this.BlendMapping = new Dictionary<ulong, BlendState>();
                this.DepthStencilMapping = new Dictionary<ulong, DepthStencilState>();
                this.RasterMapping = new Dictionary<uint, RasterizerState>();

                //initalise to defaults
                LastAlphaBlendState = new AlphaBlendState().BuildState(new RasterState());
                LastDepthStencilState = new StencilState().BuildState(new DepthState(), 0);
                LastRasterizerState = new RasterState().BuildState(0);

                this.BlendMapping.Add(0, LastAlphaBlendState);
                this.DepthStencilMapping.Add(0, LastDepthStencilState);
                this.RasterMapping.Add(0, LastRasterizerState);
            }
        }
        [System.Runtime.InteropServices.FieldOffset(64)]
        internal InternalMembers members;

        private static uint renderStackSize = 32;
        private static bool renderStackSizeUsed = false;

        internal static uint StackSize
        {
            get
            {
                renderStackSizeUsed = true;
                return renderStackSize;
            }
        }

        internal uint StackIndex { get { return members.renderStateStackIndex; } }

        /// <summary>
        /// Sets the size of the world matrix and render state stacks (default is 128)
        /// </summary>
        public static uint RenderStackSize
        {
            get { return renderStackSize; }
            set
            {
                if (renderStackSizeUsed)
                    throw new ArgumentException("This value can only be set prior to application startup");
                renderStackSize = value;
            }
        }

        /// <summary>
        /// Cast to a <see cref="DeviceRenderState"/> implicitly
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static implicit operator DeviceRenderState(DeviceRenderStateStack source)
        {
            return new DeviceRenderState(source.CurrentBlendState, source.CurrentRasterState, source.CurrentStencilState, source.CurrentDepthState);
        }

        /// <summary>
        /// <para>Saves the current render state onto the render state stack. Reset the state back with a call to <see cref="Pop"/></para>
        /// <para>Note: this method can be used in a 'using() {}' block to automatically call <see cref="Pop"/></para>
        /// </summary>
        /// <remarks><para>If you wish to modify the render state temporarily, then it is best to call PushRenderState() before making change, then call <see cref="Pop"/> after rendering is complete.</para><para>This will be a lot more efficient than manually storing the states that are changed</para></remarks>
        public UsingPop Push()
        {
#if DEBUG
            if (members.renderStateStackIndex == DeviceRenderStateStack.StackSize)
                throw new StackOverflowException("Render State stack is too small. Set DrawState.RenderStackSize to a larger value");
#endif

            members.renderStateStack[members.renderStateStackIndex++] = this.state;

            return UsingBlock;
        }

        /// <summary>
        /// Overwrites the current render state with the provided state. To Save the previous state, see <see cref="Push(ref DeviceRenderState)"/>
        /// </summary>
        /// <param name="state"></param>
        public void Set(ref DeviceRenderState state)
        {
            this.CurrentStencilState = state.Stencil;
            this.CurrentDepthState = state.Depth;
            this.CurrentBlendState = state.Blend;
            this.CurrentRasterState = state.Raster;
        }

        /// <summary>
        /// <para>Saves the current render state onto the render state stack, then copies the provided render state in. Reset the state back with a call to <see cref="Pop"/></para>
        /// <para>Note: this method can be used in a 'using() {}' block to automatically call <see cref="Pop"/></para>
        /// </summary>
        /// <remarks><para>If you wish to modify the render state temporarily, then it is best to call this method before making change, then call <see cref="Pop"/> after rendering is complete to restore the previous state.</para><para>This will be a lot more efficient than manually storing the states that are changed</para></remarks>
        public UsingPop Push(ref DeviceRenderState newState)
        {
            Push();
            this.CurrentStencilState = newState.Stencil;
            this.CurrentDepthState = newState.Depth;
            this.CurrentBlendState = newState.Blend;
            this.CurrentRasterState = newState.Raster;
            return UsingBlock;
        }

        /// <summary>
        /// <para>Saves the current render state onto the render state stack, then copies the provided alpha blend render state in. Reset the state back with a call to <see cref="Pop"/></para>
        /// <para>Note: this method can be used in a 'using() {}' block to automatically call <see cref="Pop"/></para>
        /// </summary>
        public UsingPop Push(ref AlphaBlendState blendState)
        {
            Push();
            this.state.Blend = blendState;
            return UsingBlock;
        }

        /// <summary>
        /// <para>Saves the current render state onto the render state stack, then copies the provided stenicl render state in. Reset the state back with a call to <see cref="Pop"/></para>
        /// <para>Note: this method can be used in a 'using() {}' block to automatically call <see cref="Pop"/></para>
        /// </summary>
        public UsingPop Push(ref StencilState stencilState)
        {
            Push();
            this.state.Stencil = stencilState;
            return UsingBlock;
        }

        /// <summary>
        /// <para>Saves the current render state onto the render state stack, then copies the provided raster render state in. Reset the state back with a call to <see cref="Pop"/></para>
        /// <para>Note: this method can be used in a 'using() {}' block to automatically call <see cref="Pop"/></para>
        /// </summary>
        public UsingPop Push(ref RasterState rasterState)
        {
            Push();
            this.state.Raster = rasterState;
            return UsingBlock;
        }

        /// <summary>
        /// <para>Saves the current render state onto the render state stack, then copies the provided depth render state in. Reset the state back with a call to <see cref="Pop"/></para>
        /// <para>Note: this method can be used in a 'using() {}' block to automatically call <see cref="Pop"/></para>
        /// </summary>
        public UsingPop Push(ref DepthState depthState)
        {
            Push();
            this.state.Depth = depthState;
            return UsingBlock;
        }

        /// <summary>
        /// <para>Saves the current render state onto the render state stack, then copies the provided alpha blend render state in. Reset the state back with a call to <see cref="Pop"/></para>
        /// <para>Note: this method can be used in a 'using() {}' block to automatically call <see cref="Pop"/></para>
        /// </summary>
        public UsingPop Push(AlphaBlendState blendState)
        {
            Push();
            this.state.Blend = blendState;
            return UsingBlock;
        }

        /// <summary>
        /// <para>Saves the current render state onto the render state stack, then copies the provided stenicl render state in. Reset the state back with a call to <see cref="Pop"/></para>
        /// <para>Note: this method can be used in a 'using() {}' block to automatically call <see cref="Pop"/></para>
        /// </summary>
        public UsingPop Push(StencilState stencilState)
        {
            Push();
            this.state.Stencil = stencilState;
            return UsingBlock;
        }

        /// <summary>
        /// <para>Saves the current render state onto the render state stack, then copies the provided raster render state in. Reset the state back with a call to <see cref="Pop"/></para>
        /// <para>Note: this method can be used in a 'using() {}' block to automatically call <see cref="Pop"/></para>
        /// </summary>
        public UsingPop Push(RasterState rasterState)
        {
            Push();
            this.state.Raster = rasterState;
            return UsingBlock;
        }

        /// <summary>
        /// <para>Saves the current render state onto the render state stack, then copies the provided depth render state in. Reset the state back with a call to <see cref="Pop"/></para>
        /// <para>Note: this method can be used in a 'using() {}' block to automatically call <see cref="Pop"/></para>
        /// </summary>
        public UsingPop Push(DepthState depthState)
        {
            Push();
            this.state.Depth = depthState;
            return UsingBlock;
        }

        /// <summary>
        /// Restores the last <see cref="DeviceRenderState"/> saved by a call to <see cref="Push()"/>
        /// </summary>
        public void Pop()
        {
            this.state = members.renderStateStack[checked(--members.renderStateStackIndex)];
        }

        internal void ApplyRenderStateChanges(int vertexCount, Xen.Graphics.AnimationTransformArray blendTransforms)
        {
            //get the appropriate objects, and apply them to the device

            //depth stencil
            ulong stencilValue = this.state.StencilValue;
            uint depthValue = this.state.DepthValue | ForceDisableDepthBit;

            //the first 6 bits in the stencil state is unused. DepthValue is the first 5 bits, ForceDisable is the 6th
            stencilValue |= depthValue;

            if (members.LastDepthStencilMapping != stencilValue)
            {
                members.LastDepthStencilMapping = stencilValue;
                if (!members.DepthStencilMapping.TryGetValue(stencilValue, out members.LastDepthStencilState))
                {
                    //create the new state
                    members.LastDepthStencilState = this.CurrentStencilState.BuildState(this.CurrentDepthState, ForceDisableDepthBit);
                    members.DepthStencilMapping.Add(stencilValue, members.LastDepthStencilState);
                }
            }

            //alpha blending
            ulong maskedAlphaState = this.state.UnmaskedBlendValue & InternalState.BlendMask;
            if (members.LastBlendMapping != maskedAlphaState)
            {
                members.LastBlendMapping = maskedAlphaState;
                if (!members.BlendMapping.TryGetValue(maskedAlphaState, out members.LastAlphaBlendState))
                {
                    //create the new state
                    members.LastAlphaBlendState = this.CurrentBlendState.BuildState(this.CurrentRasterState);
                    members.BlendMapping.Add(maskedAlphaState, members.LastAlphaBlendState);
                }
            }

            //raster state
            uint maskedRasterState = (this.state.UnmaskedRasterValue & InternalState.RasterMask) | ReverseCullBit;
            if (members.LastRasterMapping != maskedRasterState)
            {
                members.LastRasterMapping = maskedRasterState;
                if (!members.RasterMapping.TryGetValue(maskedRasterState, out members.LastRasterizerState))
                {
                    //create the new state
                    members.LastRasterizerState = this.CurrentRasterState.BuildState(ReverseCullBit);
                    members.RasterMapping.Add(maskedRasterState, members.LastRasterizerState);
                }
            }

            //set the states on the device
            members.parent.graphics.RasterizerState = members.LastRasterizerState;
            members.parent.graphics.DepthStencilState = members.LastDepthStencilState;
            members.parent.graphics.BlendState = members.LastAlphaBlendState;

            //if (members.parent.shaderStack.currentShader != null)
            //    members.parent.shaderSystem.ApplyRenderStateChanges(members.parent.shaderStack.currentShader, vertexCount, blendTransforms, extension);
            //if (members.parent.shaderStack.currentEffect != null)
            //    members.parent.shaderSystem.ApplyRenderStateChanges(members.parent.shaderStack.currentEffect, blendTransforms, extension);
        }

        /// <summary>
        /// Syncs the current cached state to the graphics device.
        /// <para>This logic is called automatically for all Xen classes and Xna wrappers</para>
        /// </summary>
        public void InternalSyncToGraphicsDevice()
        {
            ApplyRenderStateChanges(0, null);
        }

        /// <summary>
        /// <para>Dirties internally tracked Xen render state caches.</para>
        /// </summary>
        public void InternalDirtyRenderState(StateFlag dirtyState)
        {
            //members.parent.DirtyInternalRenderState(dirtyState);
            members.internalStateDirty |= dirtyState;
        }
    }
}

namespace Xen.Graphics
{
    /// <summary>
    /// Stores the state of the most commonly used graphics render states.
    /// </summary>
    [System.Diagnostics.DebuggerStepThrough]
    public struct DeviceRenderState : IComparable<DeviceRenderState>
    {
        /// <summary>
        /// Construct a complete render state
        /// </summary>
        public DeviceRenderState(AlphaBlendState alphaBlendState, RasterState rasterState, StencilState stencilState, DepthState depthState)
        {
            this.Stencil = stencilState;
            this.Blend = alphaBlendState;
            this.Raster = rasterState;
            this.Depth = depthState;
        }

        /// <summary>
        /// Get/Set/Modify the stencil test render states to be used during rendering
        /// </summary>
        public StencilState Stencil;

        /// <summary>
        /// Get/Set/Modify the depth buffer states to be used during rendering
        /// </summary>
        public DepthState Depth;

        /// <summary>
        /// Get/Set/Modify the alpha blending render states to be used during rendering
        /// </summary>
        public AlphaBlendState Blend;

        /// <summary>
        /// Get/Set/Modify the colour write and face culling render states to be used during rendering
        /// </summary>
        public RasterState Raster;

        /// <summary>
        /// Fast has code of all the render states
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return unchecked((int)(Stencil.op ^ Stencil.mode ^ Blend.mode ^ ((uint)Raster.mode << 16) ^ ((uint)Depth.mode << 24)));
        }

        /// <summary></summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj is DeviceRenderState)
                return ((IComparable<DeviceRenderState>)this).CompareTo((DeviceRenderState)obj) == 0;
            if (obj is DeviceRenderStateStack)
                return ((IComparable<DeviceRenderState>)this).CompareTo(((DeviceRenderStateStack)obj)) == 0;
            return false;
        }

        int IComparable<DeviceRenderState>.CompareTo(DeviceRenderState other)
        {
            if (Depth.mode > other.Depth.mode)
                return 1;
            if (Depth.mode < other.Depth.mode)
                return -1;
            if (Blend.mode > other.Blend.mode)
                return 1;
            if (Blend.mode < other.Blend.mode)
                return -1;
            if (Raster.mode > other.Raster.mode)
                return 1;
            if (Raster.mode < other.Raster.mode)
                return -1;
            if (Stencil.mode > other.Stencil.mode)
                return 1;
            if (Stencil.mode < other.Stencil.mode)
                return -1;
            if (Stencil.op > other.Stencil.op)
                return 1;
            if (Stencil.op < other.Stencil.op)
                return -1;
            return 0;
        }
    }

#if DEBUG

    internal static class BitWiseTypeValidator
    {
        public static void Validate<T>() where T : new()
        {
            object instance = new T();
            System.Reflection.PropertyInfo[] props = instance.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

            object[] defaults = new object[props.Length];
            for (int i = 0; i < props.Length; i++)
            {
                if (props[i].CanRead && props[i].CanWrite)
                    defaults[i] = props[i].GetValue(instance, null);
            }

            for (int i = 0; i < props.Length; i++)
            {
                if (!props[i].CanRead || !props[i].CanWrite)
                    continue;

                List<object> values = new List<object>();
                if (typeof(Enum).IsAssignableFrom(props[i].PropertyType))
                {
                    System.Reflection.FieldInfo[] enums = props[i].PropertyType.GetFields(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                    foreach (System.Reflection.FieldInfo field in enums)
                    {
                        values.Add(field.GetValue(null));
                    }
                }
                else
                {
                    if (typeof(bool) == props[i].PropertyType)
                    {
                        values.AddRange(new object[] { true, false });
                    }
                    else
                    {
                        if (typeof(byte) == props[i].PropertyType)
                        {
                            for (int b = 0; b < 256; b++)
                                values.Add((byte)b);
                        }
                        else
                        {
                            if (typeof(int) == props[i].PropertyType)
                                continue;
                            throw new ArgumentException();
                        }
                    }
                }

                foreach (object value in values)
                {
                    props[i].SetValue(instance, value, null);

                    if (props[i].GetValue(instance, null).Equals(value) == false)
                        throw new ArgumentException();

                    for (int p = 0; p < props.Length; p++)
                    {
                        if (!props[p].CanRead || !props[p].CanWrite)
                            continue;
                        if (p != i)
                        {
                            if (props[p].GetValue(instance, null).Equals(defaults[p]) == false)
                                throw new ArgumentException();
                        }
                    }
                }

                props[i].SetValue(instance, defaults[i], null);
            }
        }
    }

#endif

    //exactly 3 bytes
    /// <summary>
    /// Packed representation of common Alpha Blending states. 4 bytes
    /// </summary>
    /// <remarks>
    /// <para>When a pixel is drawn, it usually overwrites the existing pixel.</para>
    /// <para>For example, if you draw a quad in the centre of the screen, drawing that quad draws over the existing pixels on screen. Usually before drawing the quad, you would clear the screen first. If the screen is cleared to black, and the quad is white, the existing black colour values are overwritten with white colour values.</para>
    /// <para>The existing pixels are known as the 'destination' pixels, and the pixels being written are known as 'source' pixels.</para>
    /// <para>In the example, the desination pixels are black colour values, while the source pixels are white colour values.</para>
    /// <code>
    /// //destination values are written
    /// destination.rgba = source.rgba;
    /// </code>
    /// <para>When alpha blending is enabled, the value that is written is the result of an equation.</para>
    /// <para>A common alpha blending operation is 'additive' blending, where the source and destination colours are added together (this is commonly used in particle effects such as light flares and fire, as it 'adds' light, and brightens the on screen image (the destination pixels)) :</para>
    /// <code>
    /// //additive blending
    /// destination.rgba = destination.rgba + source.rgba;
    /// </code>
    /// <para>In such a case, the pixels already on screen (destination) will mostly stay visible, with the pixels being drawn (source) added to them</para>
    /// <para></para>
    /// <para>To set additive blending, the following states should be set:</para>
    /// <code>
    /// 	AlphaBlendState state = new AlphaBlendState();
    ///
    ///		state.BlendOperation   = BlendFunction.Add;		//This is the default value for BlendOperation
    ///		state.DestinationBlend = Blend.One;				//This is the default value for DestinationBlend
    ///		state.SourceBlend      = Blend.One;				//This is the default value for SourceBlend
    ///
    ///		state.Enabled = true;							//This is not the default value (the default is false)
    ///
    /// </code>
    /// <para>What does this mean? Alpha blending can be more complex than simple additive blending. To understand, you need to know the complete blending equation.</para>
    /// <para>The actual blending equation is:</para>
    /// <code>
    /// destination.rgba = (destination.rgba * DestinationBlend) BlendOperation (source.rgba * SourceBlend);
    /// </code>
    /// <para>As can be seen, it's more complex. With the settings used above above, the equation becomes:</para>
    /// <code>
    /// //additive blending
    /// destination.rgba = (destination.rgba * Blend.One) BlendFunction.Add (source.rgba * Blend.One);
    ///
    /// //which is...
    /// destination.rgba = (destination.rgba * 1) + (source.rgba * 1);
    ///
    /// //simplified...
    /// destination.rgba = destination.rgba + source.rgba;
    /// </code>
    /// <para>The blend operations and modes control only the mathematical operations used in the equation.</para>
    /// <para>This simple blending mode works well in a lot of cases, but you can quickly add too much colour. Most render targets (and the screen) are low precision, and cannot store r/g/b/a values greater than 1.0, so addtive blending can quickly lead to 'blown out' effects that clamp to full white (RGB = 1,1,1)</para>
    /// <para></para>
    /// <para>An example of a more complex blending mode is Alpha Blending. Not to be confused by the name, 'Alpha Blending' as a blend state and 'Alpha Blending' as a device render state are different things. For clarity, I'll call it 'SourceAlpha blending'</para>
    /// <para>With SourceAlpha blending, the source pixels store an alpha value that represents how transparent they are. Eg, the texture for leaves on a tree will store an alpha value of 1.0 on the leaf pixels, and a value of 0.0 around them in the 'transparent' pixels.</para>
    /// <para>To achieve SourceAlpha blending, the blend equation needs to perform a linear interpolation (fade) from the destination (background) to the source (tree leaves). </para>
    /// <para>A simple linear interpolation is...</para>
    /// <code>
    /// finalColour = treeColour * treeAlpha + backgroundColour * (1-treeAlpha)
    /// </code>
    /// <para>Or in blend states:</para>
    /// <code>
    /// destination.rgba = source.rgba * source.a + destination.rgba * (1-source.a);
    /// </code>
    /// <para>Converted to a AlphaBlendState object, this becomes:</para>
    /// <code>
    /// 	AlphaBlendState state = new AlphaBlendState();
    ///
    ///		state.BlendOperation   = BlendFunction.Add;
    ///		state.DestinationBlend = Blend.InverseSourceAlpha;
    ///		state.SourceBlend      = Blend.SourceAlpha;
    ///
    ///		state.Enabled = true;
    ///
    /// </code>
    /// <para>Note that 'InverseSourceAlpha' really means 'OneMinusSourceAlpha'. This is important if you use high precision render targets that can store values above 1.0!</para>
    /// <para></para>
    /// <para>Also, For advanced use, <see cref="SeparateAlphaBlendEnabled"/> can be set to true, doing so allows defining a separate equation just for the alpha channel, like so:</para>
    /// <code>
    /// destination.rgb = (destination.rgb * DestinationBlend) BlendOperation (source.rgb * SourceBlend);
    /// destination.a = (destination.a * DestinationBlendAlpha) BlendOperationAlpha (source.a * SourceBlendAlpha);
    /// </code>
    /// <para></para>
    /// <para>Note: If the blend mode modifies the destination (<see cref="DestinationBlend"/> is anything other than <see cref="Blend.One"/>), then drawing pixels in different orders will produce different results. This is especially true if depth writing is still enabled, as drawing the pixel does not completely overwrite the previous colour. For example, if multiple trees are drawn, potentially one on top of the other, then it is best to draw the trees in order from back to front. </para>
    /// </remarks>
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Explicit, Size = 4)]
#if !DEBUG_API
    [System.Diagnostics.DebuggerStepThrough]
#endif
    public struct AlphaBlendState
    {
        [System.Runtime.InteropServices.FieldOffset(0)]
        internal uint mode;

        /// <summary></summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public static explicit operator AlphaBlendState(uint state)
        {
            return new AlphaBlendState() { mode = state };
        }

        /// <summary></summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public static implicit operator uint(AlphaBlendState state)
        {
            return state.mode;
        }

        /// <summary></summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator ==(AlphaBlendState a, AlphaBlendState b)
        {
            return a.mode == b.mode;
        }

        /// <summary></summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator !=(AlphaBlendState a, AlphaBlendState b)
        {
            return a.mode != b.mode;
        }

        /// <summary></summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj is AlphaBlendState)
                return ((AlphaBlendState)obj).mode == this.mode;
            return base.Equals(obj);
        }

        /// <summary>
        /// Gets the hash code, eqivalent to the internal bitfield
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return unchecked((int)mode);
        }

        private static readonly AlphaBlendState _None = new AlphaBlendState();
        private static readonly AlphaBlendState _Alpha = new AlphaBlendState(Blend.SourceAlpha, Blend.InverseSourceAlpha);
        private static readonly AlphaBlendState _PremodulatedAlpha = new AlphaBlendState(Blend.One, Blend.InverseSourceAlpha);
        private static readonly AlphaBlendState _AlphaAdditive = new AlphaBlendState(Blend.SourceAlpha, Blend.One);
        private static readonly AlphaBlendState _Additive = new AlphaBlendState(Blend.One, Blend.One);
        private static readonly AlphaBlendState _AdditiveSaturate = new AlphaBlendState(Blend.InverseDestinationColor, Blend.One);
        private static readonly AlphaBlendState _Modulate = new AlphaBlendState(Blend.DestinationColor, Blend.Zero);
        private static readonly AlphaBlendState _ModulateAdd = new AlphaBlendState(Blend.DestinationColor, Blend.One);
        private static readonly AlphaBlendState _ModulateX2 = new AlphaBlendState(Blend.DestinationColor, Blend.SourceColor);

        /// <summary>State that disables Alpha Blending</summary>
        public static AlphaBlendState None { get { return _None; } }

        /// <summary>State that enables standard Alpha Blending (blending based on the alpha value of the source component, desitination colour is interpolated to the source colour based on source alpha)</summary>
        public static AlphaBlendState Alpha { get { return _Alpha; } }

        /// <summary>State that enables Premodulated Alpha Blending (Assumes the source colour data has been premodulated with the source alpha value, useful for reducing colour bleeding and accuracy problems at alpha edges)</summary>
        public static AlphaBlendState PremodulatedAlpha { get { return _PremodulatedAlpha; } }

        /// <summary>State that enables Additive Alpha Blending (blending based on the alpha value of the source component, the desitination colour is added to the source colour modulated by alpha)</summary>
        public static AlphaBlendState AlphaAdditive { get { return _AlphaAdditive; } }

        /// <summary>State that enables standard Additive Blending (the alpha value is ignored, the desitination colour is added to the source colour)</summary>
        public static AlphaBlendState Additive { get { return _Additive; } }

        /// <summary>State that enables Additive Saturate Blending (the alpha value is ignored, the desitination colour is added to the source colour, however the source colour is multipled by the inverse of the destination colour, preventing the addition from blowing out to pure white (eg, 0.75 + 0.75 * (1-0.75) = 0.9375))</summary>
        public static AlphaBlendState AdditiveSaturate { get { return _AdditiveSaturate; } }

        /// <summary>State that enables Modulate (multiply) Blending (the alpha value is ignored, the desitination colour is multipled with the source colour)</summary>
        public static AlphaBlendState Modulate { get { return _Modulate; } }

        /// <summary>State that enables Modulate Add (multiply+add) Blending (the alpha value is ignored, the desitination colour multipled with the source colour is added to the desitnation colour)</summary>
        public static AlphaBlendState ModulateAdd { get { return _ModulateAdd; } }

        /// <summary>State that enables Modulate (multiply) Blending, scaled by 2 (the alpha value is ignored, the desitination colour is multipled with the source colour, scaled by two)</summary>
        public static AlphaBlendState ModulateX2 { get { return _ModulateX2; } }

        /// <summary>Set the render state to no Alpha Blending, resetting all states (This is not equivalent to setting <see cref="Enabled"/> to false, however it has the same effect)</summary>
        public void SetToNoBlending() { this.mode = 0; }

        /// <summary>Set the render state to standard Alpha Blending (blending based on the alpha value of the source component, desitination colour is interpolated to the source colour based on source alpha)</summary>
        public void SetToAlphaBlending() { this.mode = _Alpha.mode; }

        /// <summary>Set the render state to Additive Alpha Blending (blending based on the alpha value of the source component, the desitination colour is added to the source colour modulated by alpha)</summary>
        public void SetToAdditiveBlending() { this.mode = _Additive.mode; }

        /// <summary>Set the render state to Premodulated Alpha Blending (Assumes the source colour data has been premodulated with the inverse of the alpha value, useful for reducing colour bleeding and accuracy problems at alpha edges)</summary>
        public void SetToPremodulatedAlphaBlending() { this.mode = _PremodulatedAlpha.mode; }

        /// <summary>Set the render state to Additive Alpha Blending (blending based on the alpha value of the source component, the desitination colour is added to the source colour modulated by alpha)</summary>
        public void SetToAlphaAdditiveBlending() { this.mode = _AlphaAdditive.mode; }

        /// <summary>Set the render state to Additive Saturate Blending (the alpha value is ignored, the desitination colour is added to the source colour, however the source colour is multipled by the inverse of the destination colour, preventing the addition from blowing out to pure white (eg, 0.75 + 0.75 * (1-0.75) = 0.9375))</summary>
        public void SetToAdditiveSaturateBlending() { this.mode = _AdditiveSaturate.mode; }

        /// <summary>Set the render state to Modulate (multiply) Blending (the alpha value is ignored, the desitination colour is multipled with the source colour)</summary>
        public void SetToModulateBlending() { this.mode = _Modulate.mode; }

        /// <summary>Set the render state to Modulate Add (multiply+add) Blending (the alpha value is ignored, the desitination colour multipled with the source colour is added to the desitnation colour)</summary>
        public void SetToModulateAddBlending() { this.mode = _ModulateAdd.mode; }

        /// <summary>Set the render state to Modulate (multiply) Blending, scaled by 2 (the alpha value is ignored, the desitination colour is multipled with the source colour, scaled by two)</summary>
        public void SetToModulateX2Blending() { this.mode = _ModulateX2.mode; }

        /// <summary>
        /// Create a alpha blend state with the given source and destination blend modes
        /// </summary>
        /// <param name="sourceBlend"></param>
        /// <param name="destinationBlend"></param>
        public AlphaBlendState(Blend sourceBlend, Blend destinationBlend)
        {
            mode = 1;
            this.SourceBlend = sourceBlend;
            this.DestinationBlend = destinationBlend;
        }

#if DEBUG

        static AlphaBlendState()
        {
            BitWiseTypeValidator.Validate<AlphaBlendState>();
        }

#endif

        /// <summary>
        /// Gets/Sets if alpha blending is enabled
        /// </summary>
        public bool Enabled
        {
            get { return (mode & 1u) == 1u; }
            set { mode = (mode & ~1u) | (value ? 1u : 0); }
        }

        /// <summary>
        /// Gets/Sets if separate alpha blending is enabled (Separate alpha blending applies an alternative blend equation to the alpha channel than the RGB channels). See <see cref="BlendOperationAlpha"/>, <see cref="SourceBlendAlpha"/> and <see cref="DestinationBlendAlpha"/>
        /// </summary>
        public bool SeparateAlphaBlendEnabled
        {
            get { return (mode & 2u) == 2u; }
            set { mode = (mode & ~2u) | (value ? 2u : 0); }
        }

        /// <summary>
        /// Gets/Sets the blending function operation. See <see cref="AlphaBlendState"/> remarks for details
        /// </summary>
        public BlendFunction BlendOperation
        {
            get
            {
                //1-5
                return (BlendFunction)(((mode >> 2) & 7u));
            }
            set
            {
                mode = (mode & ~(7u << 2)) | (7u & ((uint)value)) << 2;
            }
        }

        /// <summary>
        /// Gets/Sets the blending function operation, this value only effects the alpha channel and only when <see cref="SeparateAlphaBlendEnabled"/> is true. See <see cref="AlphaBlendState"/> remarks for details
        /// </summary>
        public BlendFunction BlendOperationAlpha
        {
            get
            {
                return (BlendFunction)(((mode >> 5) & 7u));
            }
            set
            {
                mode = (mode & ~(7u << 5)) | (7u & ((uint)value)) << 5;
            }
        }

        /// <summary>
        /// Gets/Sets the blending function source (drawn pixel) input multiply value. See <see cref="AlphaBlendState"/> remarks for details
        /// </summary>
        public Blend SourceBlend
        {
            get
            {
                return (Blend)((((mode >> 8) & 15u)));
            }
            set
            {
                mode = (mode & ~(15u << 8)) | (15u & ((uint)value)) << 8;
            }
        }

        /// <summary>
        /// Gets/Sets the blending function destination (existing pixel) input multiply value. See <see cref="AlphaBlendState"/> remarks for details
        /// </summary>
        public Blend DestinationBlend
        {
            get
            {
                return (Blend)(((mode >> 12) & 15u) ^ 1u);
            }
            set
            {
                mode = (mode & ~(15u << 12)) | (15u & ((uint)value) ^ 1u) << 12;
            }
        }

        /// <summary>
        /// Gets/Sets the blending function source (drawn pixel) input multiply value, this value only effects the alpha channel and only when <see cref="SeparateAlphaBlendEnabled"/> is true. See <see cref="AlphaBlendState"/> remarks for details
        /// </summary>
        public Blend SourceBlendAlpha
        {
            get
            {
                return (Blend)((((mode >> 16) & 15u)));
            }
            set
            {
                mode = (mode & ~(15u << 16)) | (15u & ((uint)value)) << 16;
            }
        }

        /// <summary>
        /// Gets/Sets the blending function destination (existing pixel) input multiply value, this value only effects the alpha channel and only when <see cref="SeparateAlphaBlendEnabled"/> is true. See <see cref="AlphaBlendState"/> remarks for details
        /// </summary>
        public Blend DestinationBlendAlpha
        {
            get
            {
                return (Blend)(((mode >> 20) & 15u) ^ 1u);
            }
            set
            {
                mode = (mode & ~(15u << 20)) | (15u & ((uint)value) ^ 1u) << 20;
            }
        }

        internal BlendState BuildState(RasterState rasterState)
        {
            var state = new BlendState();

            Blend source = this.SourceBlend;
            Blend dest = this.DestinationBlend;
            Blend sourceA = this.SourceBlendAlpha;
            Blend destA = this.DestinationBlendAlpha;

            if (!this.SeparateAlphaBlendEnabled)
            {
                sourceA = source;
                destA = dest;
            }

            if (!this.Enabled)
            {
                source = Blend.One;
                sourceA = Blend.One;
                dest = Blend.Zero;
                destA = Blend.Zero;
            }

            state.ColorBlendFunction = this.BlendOperation;
            state.AlphaBlendFunction = this.BlendOperationAlpha;
            state.ColorSourceBlend = source;
            state.ColorDestinationBlend = dest;
            state.AlphaSourceBlend = sourceA;
            state.AlphaDestinationBlend = destA;

            state.ColorWriteChannels = rasterState.ColourWriteMask;
            //state.ColorWriteChannels1		= rasterState.ColourWriteMask;
            //state.ColorWriteChannels2		= rasterState.ColourWriteMask;
            //state.ColorWriteChannels3		= rasterState.ColourWriteMask;

            return state;
        }
    }

    /// <summary>
    /// Packed representation of Depth Testing states. 1 byte
    /// </summary>
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Explicit, Size = 1)]
#if !DEBUG_API
    [System.Diagnostics.DebuggerStepThrough]
#endif
    public struct DepthState
    {
        [System.Runtime.InteropServices.FieldOffset(0)]
        internal byte mode;

#if DEBUG

        static DepthState()
        {
            BitWiseTypeValidator.Validate<DepthState>();
        }

#endif

        /// <summary></summary>
        public static explicit operator DepthState(byte state)
        {
            return new DepthState() { mode = state };
        }

        /// <summary></summary>
        public static implicit operator int(DepthState state)
        {
            return state.mode;
        }

        /// <summary></summary>
        public static bool operator ==(DepthState a, DepthState b)
        {
            return a.mode == b.mode;
        }

        /// <summary></summary>
        public static bool operator !=(DepthState a, DepthState b)
        {
            return a.mode != b.mode;
        }

        /// <summary></summary>
        public override bool Equals(object obj)
        {
            if (obj is DepthState)
                return ((DepthState)obj).mode == this.mode;
            return base.Equals(obj);
        }

        /// <summary>
        /// Gets the hash code. Returned value is the internal bitfield value
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return mode;
        }

        /// <summary>
        /// Gets/Sets if depth testing is enabled
        /// </summary>
        /// <remarks><para>If depth testing is disabled, then pixels will always be drawn, even if they are behind another object on screen.</para></remarks>
        public bool DepthTestEnabled
        {
            get { return (mode & 8) != 8u; }
            set { mode = (byte)((mode & ~8u) | (value ? 0 : 8u)); }
        }

        /// <summary>
        /// Gets/Sets if depth writing is enabled
        /// </summary>
        /// <remarks><para>If depth writing is disabled, pixels that are drawn will still go through normal depth testing, however they will not write a new depth value into the depth buffer. This is most useful for transparent effects, and any effect that does not have a physical representation (eg, a light cone, particle effects, etc).</para>
        /// <para>Usually, 'solid' objects with depth writing enabled will be drawn first. Such as the world, characters, models, etc. Then non-solid and effect geometry is drawn without depth writing. If this order is reversed, the solid geometry can overwrite the effects.</para></remarks>
        public bool DepthWriteEnabled
        {
            get { return (mode & 16u) != 16u; }
            set { mode = (byte)((mode & ~16u) | (value ? 0 : 16u)); }
        }

        /// <summary>
        /// Changes the comparsion function used when depth testing. WARNING:  On some video cards, changing this value can disable hierarchical z-buffer optimizations for the rest of the frame
        /// </summary>
        /// <remarks>
        /// <para>Changing the depth test function from Less to Greater midframe is <i>not recommended</i>.</para>
        /// <para>Changing between <see cref="CompareFunction.LessEqual"/> and <see cref="CompareFunction.Equal"/> is usually OK.</para>
        /// <para>On newer video cards, keeping the depth test function consistent throughout the frame will still maintain peek effciency, however some older cards are only full speed when using <see cref="CompareFunction.LessEqual"/> or <see cref="CompareFunction.Equal"/></para>
        /// <para>Setting <see cref="DepthTestEnabled"/> to false is the preferred to using <see cref="CompareFunction.Always"/>.</para>
        /// </remarks>
        public CompareFunction DepthTestFunction
        {
            get
            {
                return (CompareFunction)(((mode & 7u) ^ 3u));
            }
            set
            {
                mode = (byte)((mode & ~(7u)) | (7u & (((uint)value)) ^ 3u));
            }
        }
    }

    /// <summary>
    /// Packed representation of Colour buffer masking and Backface Cull mode states. 2 bytes
    /// </summary>
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Explicit, Size = 2)]
#if !DEBUG_API
    [System.Diagnostics.DebuggerStepThrough]
#endif
    public struct RasterState
    {
        [System.Runtime.InteropServices.FieldOffset(0)]
        internal ushort mode;

        internal const ulong BlendMask = 0xF;
        internal const ushort ModeMask = unchecked((ushort)~(BlendMask));

#if DEBUG

        static RasterState()
        {
            BitWiseTypeValidator.Validate<RasterState>();
        }

#endif

        /// <summary></summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public static explicit operator RasterState(ushort state)
        {
            return new RasterState() { mode = state };
        }

        /// <summary></summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public static implicit operator ushort(RasterState state)
        {
            return state.mode;
        }

        /// <summary></summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator ==(RasterState a, RasterState b)
        {
            return a.mode == b.mode;
        }

        /// <summary></summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator !=(RasterState a, RasterState b)
        {
            return a.mode != b.mode;
        }

        /// <summary></summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj is RasterState)
                return ((RasterState)obj).mode == this.mode;
            return base.Equals(obj);
        }

        /// <summary>
        /// Gets the hash code. Returned value is the internal bitfield value
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return mode;
        }

        /// <summary>
        /// Gets/Sets a mask for the colour channels (RGBA) that are written to the colour buffer. Set to <see cref="ColorWriteChannels.None"/> to disable all writing to the colour buffer.
        /// </summary>
        public ColorWriteChannels ColourWriteMask
        {
            get
            {
                return (ColorWriteChannels)(((~(mode)) & 15u));
            }
            set
            {
                mode = (ushort)((mode & ~(15u)) | (15u & (~((uint)value))));
            }
        }

        /// <summary>
        /// Gets/Sets the backface culling render state. Default value of <see cref="Microsoft.Xna.Framework.Graphics.CullMode.CullCounterClockwiseFace">CullCounterClockwiseFace</see>
        /// </summary>
        public CullMode CullMode
        {
            get
            {
                return (CullMode)(((((mode >> 8) & 3u) ^ 2u)));
            }
            set
            {
                mode = (ushort)((mode & ~(3u << 8)) | (3u & ((((uint)value))) ^ 2u) << 8);
            }
        }

        /// <summary>
        /// Gets/Sets a mask for the <see cref="FillMode"/> for the device (eg, <see cref="Microsoft.Xna.Framework.Graphics.FillMode.WireFrame"/> or <see cref="Microsoft.Xna.Framework.Graphics.FillMode.Solid"/>)
        /// </summary>
        public FillMode FillMode
        {
            get
            {
                return (FillMode)(((((mode >> 10) & 3u))));
            }
            set
            {
                mode = (ushort)((mode & ~(3u << 10)) | (3u & ((((uint)value)))) << 10);
            }
        }

        /// <summary>
        /// Gets/Sets if anti aliasing is enabled
        /// </summary>
        public bool MultiSampleAntiAlias
        {
            get { return (mode & 4096u) == 0; }
            set { mode = (ushort)((mode & ~4096u) | (value ? 0 : 4096u)); }
        }

        /// <summary>
        /// Gets/Sets if scissor testing is enabled
        /// </summary>
        public bool ScissorTestEnable
        {
            get { return (mode & 8192u) == 8192u; }
            set { mode = (ushort)((mode & ~8192u) | (value ? 8192u : 0)); }
        }

        internal RasterizerState BuildState(uint ReverseCullBit)
        {
            var state = new RasterizerState();

            state.FillMode = this.FillMode;
            state.MultiSampleAntiAlias = this.MultiSampleAntiAlias;
            state.ScissorTestEnable = this.ScissorTestEnable;

            CullMode mode = this.CullMode;
            if (ReverseCullBit != 0 && mode != CullMode.None)
            {
                if (mode == CullMode.CullClockwiseFace)
                    mode = CullMode.CullCounterClockwiseFace;
                else
                    mode = CullMode.CullClockwiseFace;
            }
            state.CullMode = mode;

            return state;
        }
    }

    /// <summary>
    /// Packed representation of Stencil Testing state. 8 bytes
    /// </summary>
    /// <remarks>
    /// <para>On most systems, the depth buffer is either 32 or 16 bits in size. However, with a 32bit size depth buffer, the accuracy is usually only 24bits. (True 32bit depth buffers are not supported on any DX9 video cards)</para>
    /// <para>When the depth is 24bits, the remaining 8bits can be used as the stencil buffer (see <see cref="DepthFormat.Depth24Stencil8"/>).</para>
    /// <para>In this case the stencil buffer is an 8bit integer format, (similar to a single colour in 32bit RGBA). Values range from 0 to 255.</para>
    /// <para>The difference with a stencil buffer is that operations performed on it are based almost entirely on bitmasks, increments/decrements and swapping values. It acts in a similar way to a .net <see cref="Byte"/>.</para>
    /// <para></para>
    /// <para>The name 'stencil testing' is somewhat misleading as it involves two operations, stencil reading and stencil writing.</para>
    /// <para>When stencil testing is enabled, both of these are enabled, however the default comparisons and write maskes/options all have no effect.</para>
    /// <para>For example, to 'mask' an area on screen (eg, to draw into a circle, and only a circle) then then stencil testing can be used in two passes:</para>
    /// <para>Assuming the stencil buffer is cleared to zero, the circle can be drawn first.</para>
    /// <para>With stencil testing enabled, the circile is drawn. The stencil reference value (<see cref="ReferenceValue"/>) is set to 1. The <see cref="StencilPassOperation"/> is set to <see cref="StencilOperation.Replace"/>, which means the value in the stencil buffer (when the pixel is drawn) will be <i>replaced</i> with the reference value (which is 1). Setting <see cref="RasterState.ColourWriteMask"/> to None may be desired to make the circle not visible.</para>
    /// <para>Next, the scene inside the circle is drawn. With stencil testing enabled, the reference is set to 1 again and the <see cref="StencilFunction"/> is set to <see cref="CompareFunction.Equal"/>. This way, the stencil function will only pass when the stencil value in the stencil buffer is <i>equal</i> to the <i>reference value</i> of 1. This way nothing outside the circle is drawn.</para>
    /// <para>Note that the ciricle will still be drawn into the stencil buffer at this stage.</para>
    /// <para></para>
    /// <para>Stencil testing has more complex features. The most significant, is that the stencil value can be modified in different ways for three cases.</para>
    /// <para>These cases are: When the stencil function passes, the stencil function passes but Z-testing fails and finally when the stencil function fails.</para>
    /// <para>Finally, stencil testing can be configured independantly for backfacing triangles (by setting <see cref="TwoSidedStencilModeEnabled"/> to true). (Note that <see cref="RasterState.CullMode"/> must also be set to None for this to take effect).</para>
    /// <para></para>
    /// <para>Stencil testing can be used for some very complex effects, such as stencil shadows.</para>
    /// <para>An example is determining how many pixels are within a <i>closed convex</i> volume (such as a sphere or cube) that does not intersect the near or far clip plane. Assuming a stencil buffer cleared to zero:</para>
    /// <para>In such a case, drawing the volume (with colour writes <i>and</i> depth writes disabled) and setting the stencil pass operation to <see cref="StencilOperation.Increment"/> and the stencil <i>backface z-fail</i> stencil operation to <see cref="StencilOperation.Decrement"/> will leave the stencil buffer unchanged for all pixels that are within the volume.</para>
    /// <para>Drawing the volume again, with stencil function set to Equal to a reference value of 0, only those existing pixels within the volume will be drawn.</para>
    /// </remarks>
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Explicit, Size = 8)]
#if !DEBUG_API
    [System.Diagnostics.DebuggerStepThrough]
#endif
    public struct StencilState
    {
        [System.Runtime.InteropServices.FieldOffset(0)]
        internal ulong _long;
        [System.Runtime.InteropServices.FieldOffset(0)]
        internal uint mode;
        [System.Runtime.InteropServices.FieldOffset(4)]
        internal uint op;

#if DEBUG

        static StencilState()
        {
            BitWiseTypeValidator.Validate<StencilState>();
        }

#endif

        /// <summary></summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public static explicit operator StencilState(ulong state)
        {
            return new StencilState() { _long = state };
        }

        /// <summary></summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public static implicit operator ulong(StencilState state)
        {
            return state._long;
        }

        /// <summary></summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator ==(StencilState a, StencilState b)
        {
            return a._long == b._long;
        }

        /// <summary></summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool operator !=(StencilState a, StencilState b)
        {
            return a._long != b._long;
        }

        /// <summary></summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj is StencilState)
                return ((StencilState)obj) == this;
            return base.Equals(obj);
        }

        /// <summary>
        /// Gets the hash code. Returned value is the bitwise XOR of the two internal bitfields.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return unchecked((int)(op ^ mode));
        }

        /// <summary>
        /// Gets/Sets if stencil testing is enabled
        /// </summary>
        /// <remarks>See <see cref="StencilState"/> remarks for details</remarks>
        public bool Enabled
        {
            get { return (mode & 64u) == 64u; }
            set { mode = ((mode & ~64u) | (value ? 64u : 0)); }
        }

        /// <summary>
        /// Gets/Sets if using independant stencil testing functions/operations for back and front facing triangles is enabled
        /// </summary>
        /// <remarks>See <see cref="StencilState"/> remarks for details</remarks>
        public bool TwoSidedStencilModeEnabled
        {
            get { return (mode & 128u) == 128u; }
            set { mode = ((mode & ~128u) | (value ? 128u : 0)); }
        }

        /// <summary>
        /// Gets/Sets the operation performed on the stencil buffer is the <see cref="StencilFunction"/> passes, the but depth test fails
        /// </summary>
        /// <remarks>See <see cref="StencilState"/> remarks for details</remarks>
        public StencilOperation StencilPassZFailOperation
        {
            get { return (StencilOperation)(((op >> 0) & 7u)); }
            set { op = ((op & ~(7u << 0)) | (7u & ((uint)value)) << 0); }
        }

        /// <summary>
        /// Gets/Sets the operation performed on the stencil buffer is the <see cref="StencilFunction"/> and depth test pass
        /// </summary>
        /// <remarks>See <see cref="StencilState"/> remarks for details</remarks>
        public StencilOperation StencilPassOperation
        {
            get { return (StencilOperation)(((op >> 4) & 7u)); }
            set { op = ((op & ~(7u << 4)) | (7u & ((uint)value)) << 4); }
        }

        /// <summary>
        /// Gets/Sets the operation performed on the stencil buffer is the <see cref="StencilFunction"/> fails
        /// </summary>
        /// <remarks>See <see cref="StencilState"/> remarks for details</remarks>
        public StencilOperation StencilFailOperation
        {
            get { return (StencilOperation)(((op >> 8) & 7u)); }
            set { op = ((op & ~(7u << 8)) | (7u & ((uint)value)) << 8); }
        }

        /// <summary>
        /// Gets/Sets the operation performed on the stencil buffer is the <see cref="StencilFunction"/> passes, the but depth test fails when the pixel being drawn is from a backfacing triangle and <see cref="TwoSidedStencilModeEnabled"/> is true
        /// </summary>
        /// <remarks>See <see cref="StencilState"/> remarks for details</remarks>
        public StencilOperation BackfaceStencilPassZFailOperation
        {
            get { return (StencilOperation)(((op >> 12) & 7u)); }
            set { op = ((op & ~(7u << 12)) | (7u & ((uint)value)) << 12); }
        }

        /// <summary>
        /// Gets/Sets the operation performed on the stencil buffer is the <see cref="StencilFunction"/> and the depth test pass, when the pixel being drawn is from a backfacing triangle and <see cref="TwoSidedStencilModeEnabled"/> is true
        /// </summary>
        /// <remarks>See <see cref="StencilState"/> remarks for details</remarks>
        public StencilOperation BackfaceStencilPassOperation
        {
            get { return (StencilOperation)(((op >> 16) & 7u)); }
            set { op = ((op & ~(7u << 16)) | (7u & ((uint)value)) << 16); }
        }

        /// <summary>
        /// Gets/Sets the operation performed on the stencil buffer is the <see cref="StencilFunction"/> fails, when the pixel being drawn is from a backfacing triangle and <see cref="TwoSidedStencilModeEnabled"/> is true
        /// </summary>
        /// <remarks>See <see cref="StencilState"/> remarks for details</remarks>
        public StencilOperation BackfaceStencilFailOperation
        {
            get { return (StencilOperation)(((op >> 20) & 7u)); }
            set { op = ((op & ~(7u << 20)) | (7u & ((uint)value)) << 20); }
        }

        /// <summary>
        /// Gets/Sets the comparison function performed with the value in the stencil buffer and the <see cref="ReferenceValue"/>
        /// </summary>
        /// <remarks>See <see cref="StencilState"/> remarks for details</remarks>
        public CompareFunction StencilFunction
        {
            get { return (CompareFunction)(((op >> 24) & 7u)); }
            set { op = ((op & ~(7u << 24)) | (7u & ((uint)value)) << 24); }
        }

        /// <summary>
        /// Gets/Sets the comparison function performed with the value in the stencil buffer and the <see cref="ReferenceValue"/>, when the pixel being drawn is from a backfacing triangle and <see cref="TwoSidedStencilModeEnabled"/> is true
        /// </summary>
        /// <remarks>See <see cref="StencilState"/> remarks for details</remarks>
        public CompareFunction BackfaceStencilFunction
        {
            get { return (CompareFunction)(((op >> 28) & 7u)); }
            set { op = ((op & ~(7u << 28)) | (7u & ((uint)value)) << 28); }
        }

        /// <summary>
        /// Gets/Sets the reference value used in the <see cref="StencilFunction"/> comparison
        /// </summary>
        /// <remarks>See <see cref="StencilState"/> remarks for details</remarks>
        public byte ReferenceValue
        {
            get { return (byte)((mode >> 8) & 255u); }
            set { mode = (mode & ~(255u << 8)) | ((uint)value << 8); }
        }

        /// <summary>
        /// Gets/Sets a bitmask used during stencil buffer reads. The default value is 255 (full mask).
        /// </summary>
        /// <remarks>See <see cref="StencilState"/> remarks for details</remarks>
        public byte StencilReadMask
        {
            get { return (byte)(~((mode >> 16) & 255u)); }
            set { mode = (mode & ~(255u << 16)) | ((value ^ 255u) << 16); }
        }

        /// <summary>
        /// Gets/Sets a bitmask used during stencil buffer writes. The default value is 255 (full mask).
        /// </summary>
        /// <remarks>See <see cref="StencilState"/> remarks for details</remarks>
        public byte StencilWriteMask
        {
            get { return (byte)(~((mode >> 24) & 255)); }
            set { mode = (mode & ~(255u << 24)) | ((value ^ 255u) << 24); }
        }

        internal DepthStencilState BuildState(DepthState depth, uint ForceDisableDepthBit)
        {
            var state = new DepthStencilState();

            state.StencilEnable = Enabled;
            state.StencilDepthBufferFail = StencilPassZFailOperation;
            state.StencilPass = StencilPassOperation;
            state.StencilFail = StencilFailOperation;
            state.CounterClockwiseStencilDepthBufferFail = BackfaceStencilPassZFailOperation;
            state.CounterClockwiseStencilPass = BackfaceStencilPassOperation;
            state.CounterClockwiseStencilFail = BackfaceStencilFailOperation;
            state.StencilFunction = StencilFunction;
            state.CounterClockwiseStencilFunction = BackfaceStencilFunction;
            state.TwoSidedStencilMode = this.TwoSidedStencilModeEnabled;
            state.ReferenceStencil = ReferenceValue;
            state.StencilMask = StencilReadMask;
            state.StencilWriteMask = StencilWriteMask;

            state.DepthBufferFunction = depth.DepthTestFunction;
            state.DepthBufferEnable = depth.DepthTestEnabled & (ForceDisableDepthBit == 0);
            state.DepthBufferWriteEnable = depth.DepthWriteEnabled & (ForceDisableDepthBit == 0);

            return state;
        }
    }

    //internal static class TextureSamplerStateInternal
    //{
    //    internal static SamplerState BuildState(TextureSamplerState state)
    //    {
    //        var sampler = new SamplerState();

    //        sampler.AddressU		= state.AddressU;
    //        sampler.AddressV		= state.AddressV;
    //        sampler.AddressW		= state.AddressW;
    //        sampler.Filter			= state.Filter;
    //        sampler.MaxAnisotropy	= state.MaxAnisotropy;
    //        sampler.MaxMipLevel		= state.MaxMipmapLevel;

    //        return sampler;
    //    }
    //}
}