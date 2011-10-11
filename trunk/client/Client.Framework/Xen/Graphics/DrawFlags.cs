using System;
using System.Collections.Generic;
using Client.Framework;

namespace Xen.Graphics.Stack
{
#if XBOX360
	//for some odd reason, this interface (in System) is private on the 360
	internal interface ICloneable
	{
		object Clone();
	}
#endif

    /// <summary>
    /// Stores the flag value at the top of a flag stack
    /// </summary>
    public sealed class DrawFlagValue<T> : ICloneable
    {
        /// <summary>
        /// Value of the flag
        /// </summary>
        public T Value = default(T);

        //for internal use
        object ICloneable.Clone()
        {
            return this.MemberwiseClone();
        }
    }

    /// <summary>
    /// <para>This stack adds support for stacks of 'draw flags' in the DrawState</para>
    /// <para>draw flags are custom elements that can be added to the draw state</para>
    /// <para>these store totally custom data (generic), usually enums or flags that change render behaviour.</para>
    /// </summary>
#if !DEBUG_API

    [System.Diagnostics.DebuggerStepThrough]
#endif
    public sealed class DrawFlagStack
    {
        private class DrawFlag
        {
            public DrawFlag(Array stack, object top)
            {
                this.stack = stack;
                this.index = 0;
                this.top = top;
            }

            public Array stack;
            public readonly object top;
            public uint index;
        }

        private DrawFlag[] drawFlags = new DrawFlag[8];

        private static int flagCount = 0;
        private static object flagSync = new object();
        private static readonly List<Array> defaultFlagArrays = new List<Array>();
        private static readonly List<ICloneable> defaultFlagValues = new List<ICloneable>();

        private static readonly List<WeakReference> drawFlagInstances = new List<WeakReference>();
        private static readonly List<WeakReference> drawFlagInstancesBuffer = new List<WeakReference>();

        internal DrawFlagStack()
        {
            lock (flagSync)
            {
                drawFlagInstances.Add(new WeakReference(this));

                //for all the flag types,
                for (int i = 0; i < flagCount; i++)
                {
                    //initialise with a copy of the default array type
                    drawFlags[i] = new DrawFlag((Array)defaultFlagArrays[i].Clone(), defaultFlagValues[i].Clone());
                }
            }
        }

        /// <summary>
        /// Structure used for a using block with a Push method
        /// </summary>
        [System.Diagnostics.DebuggerStepThrough]
        public struct UsingPop<T> : IDisposable where T : struct
        {
            internal DrawFlagStack stack;

            /// <summary>Invokes the Pop metohd</summary>
            public void Dispose()
            {
                stack.Pop<T>();
            }
        }

        /// <summary>
        /// <para>Push the custom Draw Flag (enum or struct) currently stoed by the DrawState</para>
        /// <para>Draw flags can be any value desired, usually an enum or small struct. They can be used to control draw logic</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public UsingPop<T> Push<T>() where T : struct
        {
            int index = DrawFlagID<T>.index;
            DrawFlag flag = drawFlags[index];
            T[] array = flag.stack as T[];
            DrawFlagValue<T> top = flag.top as DrawFlagValue<T>;
            array[flag.index] = top.Value;
            if (++flag.index == array.Length)
            {
                Array.Resize(ref array, array.Length * 2);
                flag.stack = array;
#if DEBUG
                if (array.Length >= 4096)
                    throw new StackOverflowException("Potential memory leak detected: A call to PushDrawFlag() may not have a matching call to PopDrawFlag()");
#endif
            }

            UsingPop<T> pop = new UsingPop<T>();
            pop.stack = this;
            return pop;
        }

        /// <summary>
        /// <para>Push a custom Draw Flag (enum or struct) onto the DrawState</para>
        /// <para>Draw flags can be any value desired, usually an enum or small struct. They can be used to control draw logic</para>
        /// </summary>
        public UsingPop<T> Push<T>(T value) where T : struct
        {
            int index = DrawFlagID<T>.index;
            DrawFlag flag = drawFlags[index];
            T[] array = flag.stack as T[];
            DrawFlagValue<T> top = flag.top as DrawFlagValue<T>;
            array[flag.index] = top.Value;
            top.Value = value;
            if (++flag.index == array.Length)
            {
                Array.Resize(ref array, array.Length * 2);
                flag.stack = array;
#if DEBUG
                if (array.Length >= 4096)
                    throw new StackOverflowException("Potential memory leak detected: A call to PushDrawFlag() may not have a matching call to PopDrawFlag()");
#endif
            }

            UsingPop<T> pop = new UsingPop<T>();
            pop.stack = this;
            return pop;
        }

        /// <summary>
        /// <para>Push a custom Draw Flag (enum or struct) onto the DrawState</para>
        /// <para>Draw flags can be any value desired, usually an enum or small struct.
        /// They can be used to control draw logic</para>
        /// </summary>
        public UsingPop<T> Push<T>(ref T value) where T : struct
        {
            int index = DrawFlagID<T>.index;
            DrawFlag flag = drawFlags[index];
            T[] array = flag.stack as T[];
            DrawFlagValue<T> top = flag.top as DrawFlagValue<T>;
            array[flag.index] = top.Value;
            top.Value = value;
            if (++flag.index == array.Length)
            {
                Array.Resize(ref array, array.Length * 2);
                flag.stack = array;
#if DEBUG
                if (array.Length >= 4096)
                    throw new StackOverflowException("Potential memory leak detected: A call to PushDrawFlag() may not have a matching call to PopDrawFlag()");
#endif
            }

            UsingPop<T> pop = new UsingPop<T>();
            pop.stack = this;
            return pop;
        }

        /// <summary>
        /// <para>Pop a custom Draw Flag (enum or struct), stored by the DrawState</para>
        /// <para>Draw flags can be any value desired, usually an enum or small struct.
        /// They can be used to control draw logic</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void Pop<T>() where T : struct
        {
            int index = DrawFlagID<T>.index;
            DrawFlag flag = drawFlags[index];
            DrawFlagValue<T> top = flag.top as DrawFlagValue<T>;
            T[] array = flag.stack as T[];
            top.Value = array[checked(--flag.index)];
        }

        /// <summary>
        /// <para>Set a custom Draw Flag (enum or struct), stored by the DrawState</para>
        /// <para>Draw flags can be any value desired, usually an enum or small struct.
        /// They can be used to control draw logic</para>
        /// <para>Note: if you are calling this method repeatadly, consider storing a reference to the flag with <see cref="GetFlagReference"/></para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void SetFlag<T>(T value) where T : struct
        {
            DrawFlag flag = drawFlags[DrawFlagID<T>.index];
            DrawFlagValue<T> top = flag.top as DrawFlagValue<T>;
            top.Value = value;
        }

        /// <summary>
        /// <para>Set a custom Draw Flag (enum or struct), stored by the DrawState</para>
        /// <para>Draw flags can be any value desired, usually an enum or small struct.
        /// They can be used to control draw logic</para>
        /// <para>Note: if you are calling this method repeatadly, consider storing a reference to the flag with <see cref="GetFlagReference"/></para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void SetFlag<T>(ref T value) where T : struct
        {
            DrawFlagValue<T> top = drawFlags[DrawFlagID<T>.index].top as DrawFlagValue<T>;
            top.Value = value;
        }

        /// <summary>
        /// <para>Get a custom Draw Flag (enum or struct), stored by the DrawState</para>
        /// <para>Draw flags can be any value desired, usually an enum or small struct.
        /// They can be used to control draw logic</para>
        /// <para>Note: if you are calling this method repeatadly, consider storing a reference to the flag with <see cref="GetFlagReference"/></para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public T GetFlag<T>() where T : struct
        {
            DrawFlagValue<T> top = drawFlags[DrawFlagID<T>.index].top as DrawFlagValue<T>;
            return top.Value;
        }

        /// <summary>
        /// <para>Get a custom Draw Flag (enum or struct), stored by the DrawState</para>
        /// <para>Draw flags can be any value desired, usually an enum or small struct.
        /// They can be used to control draw logic</para>
        /// <para>Note: if you are calling this method repeatadly, consider storing a reference to the flag with <see cref="GetFlagReference"/></para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void GetFlag<T>(out T value) where T : struct
        {
            DrawFlagValue<T> top = drawFlags[DrawFlagID<T>.index].top as DrawFlagValue<T>;
            value = top.Value;
        }

        /// <summary>
        /// Gets a reference to the flag, this is the most efficient way to get and set the draw flag at the top of the stack at runtime, provided the reference to the DrawFlagValue object can be stored once.
        /// </summary>
        public DrawFlagValue<T> GetFlagReference<T>() where T : struct
        {
            return drawFlags[DrawFlagID<T>.index].top as DrawFlagValue<T>;
        }

        //unlikely to be called. If two apps are running at the same time, then the static indexing might get mixed up.
        //so this will be called by the alt app to keep the lists in sync
        private void DrawFlagAdded<T>() where T : struct
        {
            if (drawFlags.Length < flagCount)
                Array.Resize(ref drawFlags, drawFlags.Length * 2);
            drawFlags[flagCount - 1] = new DrawFlag(CreateDrawFlagArray<T>(), new DrawFlagValue<T>());
        }

        private static T[] CreateDrawFlagArray<T>() where T : struct
        {
            return new T[] { default(T), default(T) };
        }

        //struct for storing an index per flag type
        struct DrawFlagID<T> where T : struct
        {
            public static readonly int index;

            //static constructor for a flag type.
            static DrawFlagID()
            {
                //make sure any active draw state objects have an up to date list of flag stacks
                lock (flagSync)
                {
                    //increment the flag count and store it as an index
                    index = flagCount++;

                    defaultFlagArrays.Add(CreateDrawFlagArray<T>());
                    defaultFlagValues.Add(new DrawFlagValue<T>());

                    drawFlagInstancesBuffer.AddRange(drawFlagInstances);
                    foreach (WeakReference wr in drawFlagInstancesBuffer)
                    {
                        //for all the draw states
                        DrawFlagStack instance = wr.Target as DrawFlagStack;
                        if (instance != null)
                            instance.DrawFlagAdded<T>(); //tell it about the new flag type
                        else
                            drawFlagInstances.Remove(wr);
                    }
                    drawFlagInstancesBuffer.Clear();
                }
            }
        }
    }
}