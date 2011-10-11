using System;

//using Xen.Graphics.ShaderSystem;
using Microsoft.Xna.Framework;
using Xen.Graphics.Stack;

namespace Xen
{
    namespace Graphics.Stack
    {
        /// <summary>
        /// DrawState cullers stack, storing pre and post cullers.
        /// </summary>
#if !DEBUG_API

        [System.Diagnostics.DebuggerStepThrough]
#endif
        public sealed class CullersStack
        {
            private readonly DrawState state;

            internal CullersStack(DrawState state)
            {
                this.state = state;
                this.PostCuller = new UsingPostCuller(this);
                this.PreCuller = new UsingPreCuller(this);
            }

            /// <summary>
            /// Structure used for a using block with a Push method
            /// </summary>
            [System.Diagnostics.DebuggerStepThrough]
            public struct UsingPreCuller : IDisposable
            {
                internal UsingPreCuller(CullersStack stack)
                {
                    this.stack = stack;
                }

                private readonly CullersStack stack;

                /// <summary>Invokes the Pop metohd</summary>
                public void Dispose()
                {
                    stack.PopPreCuller();
                }
            }
            internal readonly UsingPreCuller PreCuller;

            /// <summary>
            /// Structure used for a using block with a Push method
            /// </summary>
            [System.Diagnostics.DebuggerStepThrough]
            public struct UsingPostCuller : IDisposable
            {
                internal UsingPostCuller(CullersStack stack)
                {
                    this.stack = stack;
                }

                private readonly CullersStack stack;

                /// <summary>Invokes the Pop metohd</summary>
                public void Dispose()
                {
                    stack.PopPostCuller();
                }
            }
            internal readonly UsingPostCuller PostCuller;

            /// <summary>
            /// Pushes a culler onto the pre-culling stack (pre-culling cull tests occurs <i>before</i> the default onscreen Culler). Fast cull operations are usually added here
            /// </summary>
            public UsingPreCuller PushPreCuller(ICullPrimitive culler)
            {
                if (culler == null)
                    throw new ArgumentNullException();
                state.preCullers[state.preCullerCount++] = culler;
                return PreCuller;
            }

            /// <summary>
            /// Removes the culler on the top of the pre-culling stack
            /// </summary>
            public void PopPreCuller()
            {
                if (state.preCullerCount == 0)
                    throw new InvalidOperationException("Stack is empty");
                state.preCullers[--state.preCullerCount] = null;
            }

            /// <summary>
            /// Pushes a culler onto the post-culling stack (post-culling cull tests  occurs <i>after</i> the default onscreen Culler). More expensive culling operations are usually added here
            /// </summary>
            /// <param name="culler"></param>
            public UsingPostCuller PushPostCuller(ICullPrimitive culler)
            {
                if (culler == null)
                    throw new ArgumentNullException();
                state.postCullers[state.postCullerCount++] = culler;
                return PostCuller;
            }

            /// <summary>
            /// Removes the culler on the top of the post-culling stack
            /// </summary>
            public void PopPostCuller()
            {
                if (state.postCullerCount == 0)
                    throw new InvalidOperationException("Stack is empty");
                state.postCullers[--state.postCullerCount] = null;
            }
        }
    }

    sealed partial class DrawState : ICuller
    {
        internal readonly ICullPrimitive[] preCullers = new ICullPrimitive[DeviceRenderStateStack.StackSize], postCullers = new ICullPrimitive[DeviceRenderStateStack.StackSize];
        internal int preCullerCount = 0, postCullerCount = 0;

        IState ICuller.GetState()
        {
            return this;
        }

        #region ICuller tests

        bool ICuller.TestBox(Vector3 min, Vector3 max)
        {
            if (matrices.World.isIdentity)
                return ((ICuller)this).TestWorldBox(ref min, ref max);
            for (int i = preCullerCount - 1; i >= 0; i--)
            {
                if (!preCullers[i].TestWorldBox(ref min, ref max, ref matrices.World.value))
                    return false;
            }

            if (!FrustumCull.BoxInFrustum(camera.GetCullingPlanes(), ref min, ref max, ref matrices.World.value))
                return false;

            for (int i = 0; i < postCullerCount; i++)
            {
                if (!postCullers[i].TestWorldBox(ref min, ref max, ref matrices.World.value))
                    return false;
            }
            return true;
        }

        bool ICuller.TestBox(ref Vector3 min, ref Vector3 max)
        {
            if (matrices.World.isIdentity)
                return ((ICuller)this).TestWorldBox(ref min, ref max);

            for (int i = preCullerCount - 1; i >= 0; i--)
            {
                if (!preCullers[i].TestWorldBox(ref min, ref max, ref matrices.World.value))
                    return false;
            }

            if (!FrustumCull.BoxInFrustum(camera.GetCullingPlanes(), ref min, ref max, ref matrices.World.value))
                return false;

            for (int i = 0; i < postCullerCount; i++)
            {
                if (!postCullers[i].TestWorldBox(ref min, ref max, ref matrices.World.value))
                    return false;
            }

            return true;
        }

        bool ICuller.TestBox(Vector3 min, Vector3 max, ref Matrix localMatrix)
        {
            if (matrices.World.isIdentity)
                return ((ICuller)this).TestWorldBox(ref min, ref max, ref localMatrix);

            Matrix matrix;
#if NO_INLINE
			Matrix.Multiply(ref localMatrix, ref worldMatrix.value, out matrix);
#else
#if XBOX360
			matrix = new Matrix();
#endif
            matrix.M11 = (((localMatrix.M11 * matrices.World.value.M11) + (localMatrix.M12 * matrices.World.value.M21)) + (localMatrix.M13 * matrices.World.value.M31)) + (localMatrix.M14 * matrices.World.value.M41);
            matrix.M12 = (((localMatrix.M11 * matrices.World.value.M12) + (localMatrix.M12 * matrices.World.value.M22)) + (localMatrix.M13 * matrices.World.value.M32)) + (localMatrix.M14 * matrices.World.value.M42);
            matrix.M13 = (((localMatrix.M11 * matrices.World.value.M13) + (localMatrix.M12 * matrices.World.value.M23)) + (localMatrix.M13 * matrices.World.value.M33)) + (localMatrix.M14 * matrices.World.value.M43);
            matrix.M14 = (((localMatrix.M11 * matrices.World.value.M14) + (localMatrix.M12 * matrices.World.value.M24)) + (localMatrix.M13 * matrices.World.value.M34)) + (localMatrix.M14 * matrices.World.value.M44);
            matrix.M21 = (((localMatrix.M21 * matrices.World.value.M11) + (localMatrix.M22 * matrices.World.value.M21)) + (localMatrix.M23 * matrices.World.value.M31)) + (localMatrix.M24 * matrices.World.value.M41);
            matrix.M22 = (((localMatrix.M21 * matrices.World.value.M12) + (localMatrix.M22 * matrices.World.value.M22)) + (localMatrix.M23 * matrices.World.value.M32)) + (localMatrix.M24 * matrices.World.value.M42);
            matrix.M23 = (((localMatrix.M21 * matrices.World.value.M13) + (localMatrix.M22 * matrices.World.value.M23)) + (localMatrix.M23 * matrices.World.value.M33)) + (localMatrix.M24 * matrices.World.value.M43);
            matrix.M24 = (((localMatrix.M21 * matrices.World.value.M14) + (localMatrix.M22 * matrices.World.value.M24)) + (localMatrix.M23 * matrices.World.value.M34)) + (localMatrix.M24 * matrices.World.value.M44);
            matrix.M31 = (((localMatrix.M31 * matrices.World.value.M11) + (localMatrix.M32 * matrices.World.value.M21)) + (localMatrix.M33 * matrices.World.value.M31)) + (localMatrix.M34 * matrices.World.value.M41);
            matrix.M32 = (((localMatrix.M31 * matrices.World.value.M12) + (localMatrix.M32 * matrices.World.value.M22)) + (localMatrix.M33 * matrices.World.value.M32)) + (localMatrix.M34 * matrices.World.value.M42);
            matrix.M33 = (((localMatrix.M31 * matrices.World.value.M13) + (localMatrix.M32 * matrices.World.value.M23)) + (localMatrix.M33 * matrices.World.value.M33)) + (localMatrix.M34 * matrices.World.value.M43);
            matrix.M34 = (((localMatrix.M31 * matrices.World.value.M14) + (localMatrix.M32 * matrices.World.value.M24)) + (localMatrix.M33 * matrices.World.value.M34)) + (localMatrix.M34 * matrices.World.value.M44);
            matrix.M41 = (((localMatrix.M41 * matrices.World.value.M11) + (localMatrix.M42 * matrices.World.value.M21)) + (localMatrix.M43 * matrices.World.value.M31)) + (localMatrix.M44 * matrices.World.value.M41);
            matrix.M42 = (((localMatrix.M41 * matrices.World.value.M12) + (localMatrix.M42 * matrices.World.value.M22)) + (localMatrix.M43 * matrices.World.value.M32)) + (localMatrix.M44 * matrices.World.value.M42);
            matrix.M43 = (((localMatrix.M41 * matrices.World.value.M13) + (localMatrix.M42 * matrices.World.value.M23)) + (localMatrix.M43 * matrices.World.value.M33)) + (localMatrix.M44 * matrices.World.value.M43);
            matrix.M44 = (((localMatrix.M41 * matrices.World.value.M14) + (localMatrix.M42 * matrices.World.value.M24)) + (localMatrix.M43 * matrices.World.value.M34)) + (localMatrix.M44 * matrices.World.value.M44);
#endif

            for (int i = preCullerCount - 1; i >= 0; i--)
            {
                if (!preCullers[i].TestWorldBox(ref min, ref max, ref matrix))
                    return false;
            }

            if (!FrustumCull.BoxInFrustum(camera.GetCullingPlanes(), ref min, ref max, ref matrix))
                return false;

            for (int i = 0; i < postCullerCount; i++)
            {
                if (!postCullers[i].TestWorldBox(ref min, ref max, ref matrix))
                    return false;
            }
            return true;
        }

        bool ICuller.TestBox(ref Vector3 min, ref Vector3 max, ref Matrix localMatrix)
        {
            if (matrices.World.isIdentity)
                return ((ICuller)this).TestWorldBox(ref min, ref max, ref localMatrix);

            Matrix matrix;
#if NO_INLINE
			Matrix.Multiply(ref localMatrix, ref worldMatrix.value, out matrix);
#else
#if XBOX360
			matrix = new Matrix();
#endif
            matrix.M11 = (((localMatrix.M11 * matrices.World.value.M11) + (localMatrix.M12 * matrices.World.value.M21)) + (localMatrix.M13 * matrices.World.value.M31)) + (localMatrix.M14 * matrices.World.value.M41);
            matrix.M12 = (((localMatrix.M11 * matrices.World.value.M12) + (localMatrix.M12 * matrices.World.value.M22)) + (localMatrix.M13 * matrices.World.value.M32)) + (localMatrix.M14 * matrices.World.value.M42);
            matrix.M13 = (((localMatrix.M11 * matrices.World.value.M13) + (localMatrix.M12 * matrices.World.value.M23)) + (localMatrix.M13 * matrices.World.value.M33)) + (localMatrix.M14 * matrices.World.value.M43);
            matrix.M14 = (((localMatrix.M11 * matrices.World.value.M14) + (localMatrix.M12 * matrices.World.value.M24)) + (localMatrix.M13 * matrices.World.value.M34)) + (localMatrix.M14 * matrices.World.value.M44);
            matrix.M21 = (((localMatrix.M21 * matrices.World.value.M11) + (localMatrix.M22 * matrices.World.value.M21)) + (localMatrix.M23 * matrices.World.value.M31)) + (localMatrix.M24 * matrices.World.value.M41);
            matrix.M22 = (((localMatrix.M21 * matrices.World.value.M12) + (localMatrix.M22 * matrices.World.value.M22)) + (localMatrix.M23 * matrices.World.value.M32)) + (localMatrix.M24 * matrices.World.value.M42);
            matrix.M23 = (((localMatrix.M21 * matrices.World.value.M13) + (localMatrix.M22 * matrices.World.value.M23)) + (localMatrix.M23 * matrices.World.value.M33)) + (localMatrix.M24 * matrices.World.value.M43);
            matrix.M24 = (((localMatrix.M21 * matrices.World.value.M14) + (localMatrix.M22 * matrices.World.value.M24)) + (localMatrix.M23 * matrices.World.value.M34)) + (localMatrix.M24 * matrices.World.value.M44);
            matrix.M31 = (((localMatrix.M31 * matrices.World.value.M11) + (localMatrix.M32 * matrices.World.value.M21)) + (localMatrix.M33 * matrices.World.value.M31)) + (localMatrix.M34 * matrices.World.value.M41);
            matrix.M32 = (((localMatrix.M31 * matrices.World.value.M12) + (localMatrix.M32 * matrices.World.value.M22)) + (localMatrix.M33 * matrices.World.value.M32)) + (localMatrix.M34 * matrices.World.value.M42);
            matrix.M33 = (((localMatrix.M31 * matrices.World.value.M13) + (localMatrix.M32 * matrices.World.value.M23)) + (localMatrix.M33 * matrices.World.value.M33)) + (localMatrix.M34 * matrices.World.value.M43);
            matrix.M34 = (((localMatrix.M31 * matrices.World.value.M14) + (localMatrix.M32 * matrices.World.value.M24)) + (localMatrix.M33 * matrices.World.value.M34)) + (localMatrix.M34 * matrices.World.value.M44);
            matrix.M41 = (((localMatrix.M41 * matrices.World.value.M11) + (localMatrix.M42 * matrices.World.value.M21)) + (localMatrix.M43 * matrices.World.value.M31)) + (localMatrix.M44 * matrices.World.value.M41);
            matrix.M42 = (((localMatrix.M41 * matrices.World.value.M12) + (localMatrix.M42 * matrices.World.value.M22)) + (localMatrix.M43 * matrices.World.value.M32)) + (localMatrix.M44 * matrices.World.value.M42);
            matrix.M43 = (((localMatrix.M41 * matrices.World.value.M13) + (localMatrix.M42 * matrices.World.value.M23)) + (localMatrix.M43 * matrices.World.value.M33)) + (localMatrix.M44 * matrices.World.value.M43);
            matrix.M44 = (((localMatrix.M41 * matrices.World.value.M14) + (localMatrix.M42 * matrices.World.value.M24)) + (localMatrix.M43 * matrices.World.value.M34)) + (localMatrix.M44 * matrices.World.value.M44);
#endif

            for (int i = preCullerCount - 1; i >= 0; i--)
            {
                if (!preCullers[i].TestWorldBox(ref min, ref max, ref matrix))
                    return false;
            }

            if (!FrustumCull.BoxInFrustum(camera.GetCullingPlanes(), ref min, ref max, ref matrix))
                return false;

            for (int i = 0; i < postCullerCount; i++)
            {
                if (!postCullers[i].TestWorldBox(ref min, ref max, ref matrix))
                    return false;
            }

            return true;
        }

        bool ICuller.TestSphere(float radius)
        {
            Vector3 pos;
#if XBOX360
			pos = new Vector3();
#endif
            pos.X = matrices.World.value.M41;
            pos.Y = matrices.World.value.M42;
            pos.Z = matrices.World.value.M43;

            for (int i = preCullerCount - 1; i >= 0; i--)
            {
                if (!preCullers[i].TestWorldSphere(radius, ref pos))
                    return false;
            }

            if (!FrustumCull.SphereInFrustum(camera.GetCullingPlanes(), radius, ref pos))
                return false;

            for (int i = 0; i < postCullerCount; i++)
            {
                if (!postCullers[i].TestWorldSphere(radius, ref pos))
                    return false;
            }

            return true;
        }

        bool ICuller.TestSphere(float radius, Vector3 position)
        {
            return ((ICuller)this).TestSphere(radius, ref position);
        }

        bool ICuller.TestSphere(float radius, ref Vector3 position)
        {
            Vector3 pos;
            if (matrices.World.isIdentity)
                pos = position;
            else
                Vector3.Transform(ref position, ref matrices.World.value, out pos);

            for (int i = preCullerCount - 1; i >= 0; i--)
            {
                if (!preCullers[i].TestWorldSphere(radius, ref pos))
                    return false;
            }

            if (!FrustumCull.SphereInFrustum(camera.GetCullingPlanes(), radius, ref pos))
                return false;

            for (int i = 0; i < postCullerCount; i++)
            {
                if (!postCullers[i].TestWorldSphere(radius, ref pos))
                    return false;
            }

            return true;
        }

        ContainmentType ICuller.IntersectBox(Vector3 min, Vector3 max)
        {
            if (matrices.World.isIdentity)
                return ((ICuller)this).IntersectWorldBox(ref min, ref max);

            ContainmentType type; bool intersect = false;

            for (int i = preCullerCount - 1; i >= 0; i--)
            {
                type = preCullers[i].IntersectWorldBox(ref min, ref max, ref matrices.World.value);
                if (type == ContainmentType.Disjoint)
                    return type;

                if (type == ContainmentType.Intersects)
                    intersect = true;
            }

            type = FrustumCull.BoxIntersectsFrustum(camera.GetCullingPlanes(), ref min, ref max, ref matrices.World.value);
            if (type == ContainmentType.Disjoint)
                return type;

            if (type == ContainmentType.Intersects)
                intersect = true;

            for (int i = 0; i < postCullerCount; i++)
            {
                type = postCullers[i].IntersectWorldBox(ref min, ref max, ref matrices.World.value);
                if (type == ContainmentType.Disjoint)
                    return type;

                if (type == ContainmentType.Intersects)
                    intersect = true;
            }
            return intersect ? ContainmentType.Intersects : ContainmentType.Contains;
        }

        ContainmentType ICuller.IntersectBox(ref Vector3 min, ref Vector3 max)
        {
            if (matrices.World.isIdentity)
                return ((ICuller)this).IntersectWorldBox(ref min, ref max);

            ContainmentType type; bool intersect = false;

            for (int i = preCullerCount - 1; i >= 0; i--)
            {
                type = preCullers[i].IntersectWorldBox(ref min, ref max, ref matrices.World.value);
                if (type == ContainmentType.Disjoint)
                    return type;

                if (type == ContainmentType.Intersects)
                    intersect = true;
            }

            type = FrustumCull.BoxIntersectsFrustum(camera.GetCullingPlanes(), ref min, ref max, ref matrices.World.value);
            if (type == ContainmentType.Disjoint)
                return type;

            if (type == ContainmentType.Intersects)
                intersect = true;

            for (int i = 0; i < postCullerCount; i++)
            {
                type = postCullers[i].IntersectWorldBox(ref min, ref max, ref matrices.World.value);
                if (type == ContainmentType.Disjoint)
                    return type;

                if (type == ContainmentType.Intersects)
                    intersect = true;
            }

            return intersect ? ContainmentType.Intersects : ContainmentType.Contains;
        }

        ContainmentType ICuller.IntersectBox(Vector3 min, Vector3 max, ref Matrix localMatrix)
        {
            if (matrices.World.isIdentity)
                return ((ICuller)this).IntersectWorldBox(ref min, ref max, ref localMatrix);

            Matrix matrix;
#if NO_INLINE
			Matrix.Multiply(ref localMatrix, ref worldMatrix.value, out matrix);
#else
#if XBOX360
			matrix = new Matrix();
#endif
            matrix.M11 = (((localMatrix.M11 * matrices.World.value.M11) + (localMatrix.M12 * matrices.World.value.M21)) + (localMatrix.M13 * matrices.World.value.M31)) + (localMatrix.M14 * matrices.World.value.M41);
            matrix.M12 = (((localMatrix.M11 * matrices.World.value.M12) + (localMatrix.M12 * matrices.World.value.M22)) + (localMatrix.M13 * matrices.World.value.M32)) + (localMatrix.M14 * matrices.World.value.M42);
            matrix.M13 = (((localMatrix.M11 * matrices.World.value.M13) + (localMatrix.M12 * matrices.World.value.M23)) + (localMatrix.M13 * matrices.World.value.M33)) + (localMatrix.M14 * matrices.World.value.M43);
            matrix.M14 = (((localMatrix.M11 * matrices.World.value.M14) + (localMatrix.M12 * matrices.World.value.M24)) + (localMatrix.M13 * matrices.World.value.M34)) + (localMatrix.M14 * matrices.World.value.M44);
            matrix.M21 = (((localMatrix.M21 * matrices.World.value.M11) + (localMatrix.M22 * matrices.World.value.M21)) + (localMatrix.M23 * matrices.World.value.M31)) + (localMatrix.M24 * matrices.World.value.M41);
            matrix.M22 = (((localMatrix.M21 * matrices.World.value.M12) + (localMatrix.M22 * matrices.World.value.M22)) + (localMatrix.M23 * matrices.World.value.M32)) + (localMatrix.M24 * matrices.World.value.M42);
            matrix.M23 = (((localMatrix.M21 * matrices.World.value.M13) + (localMatrix.M22 * matrices.World.value.M23)) + (localMatrix.M23 * matrices.World.value.M33)) + (localMatrix.M24 * matrices.World.value.M43);
            matrix.M24 = (((localMatrix.M21 * matrices.World.value.M14) + (localMatrix.M22 * matrices.World.value.M24)) + (localMatrix.M23 * matrices.World.value.M34)) + (localMatrix.M24 * matrices.World.value.M44);
            matrix.M31 = (((localMatrix.M31 * matrices.World.value.M11) + (localMatrix.M32 * matrices.World.value.M21)) + (localMatrix.M33 * matrices.World.value.M31)) + (localMatrix.M34 * matrices.World.value.M41);
            matrix.M32 = (((localMatrix.M31 * matrices.World.value.M12) + (localMatrix.M32 * matrices.World.value.M22)) + (localMatrix.M33 * matrices.World.value.M32)) + (localMatrix.M34 * matrices.World.value.M42);
            matrix.M33 = (((localMatrix.M31 * matrices.World.value.M13) + (localMatrix.M32 * matrices.World.value.M23)) + (localMatrix.M33 * matrices.World.value.M33)) + (localMatrix.M34 * matrices.World.value.M43);
            matrix.M34 = (((localMatrix.M31 * matrices.World.value.M14) + (localMatrix.M32 * matrices.World.value.M24)) + (localMatrix.M33 * matrices.World.value.M34)) + (localMatrix.M34 * matrices.World.value.M44);
            matrix.M41 = (((localMatrix.M41 * matrices.World.value.M11) + (localMatrix.M42 * matrices.World.value.M21)) + (localMatrix.M43 * matrices.World.value.M31)) + (localMatrix.M44 * matrices.World.value.M41);
            matrix.M42 = (((localMatrix.M41 * matrices.World.value.M12) + (localMatrix.M42 * matrices.World.value.M22)) + (localMatrix.M43 * matrices.World.value.M32)) + (localMatrix.M44 * matrices.World.value.M42);
            matrix.M43 = (((localMatrix.M41 * matrices.World.value.M13) + (localMatrix.M42 * matrices.World.value.M23)) + (localMatrix.M43 * matrices.World.value.M33)) + (localMatrix.M44 * matrices.World.value.M43);
            matrix.M44 = (((localMatrix.M41 * matrices.World.value.M14) + (localMatrix.M42 * matrices.World.value.M24)) + (localMatrix.M43 * matrices.World.value.M34)) + (localMatrix.M44 * matrices.World.value.M44);
#endif

            ContainmentType type; bool intersect = false;

            for (int i = preCullerCount - 1; i >= 0; i--)
            {
                type = preCullers[i].IntersectWorldBox(ref min, ref max, ref matrix);
                if (type == ContainmentType.Disjoint)
                    return type;

                if (type == ContainmentType.Intersects)
                    intersect = true;
            }

            type = FrustumCull.BoxIntersectsFrustum(camera.GetCullingPlanes(), ref min, ref max, ref matrix);
            if (type == ContainmentType.Disjoint)
                return type;

            if (type == ContainmentType.Intersects)
                intersect = true;

            for (int i = 0; i < postCullerCount; i++)
            {
                type = postCullers[i].IntersectWorldBox(ref min, ref max, ref matrix);
                if (type == ContainmentType.Disjoint)
                    return type;

                if (type == ContainmentType.Intersects)
                    intersect = true;
            }
            return intersect ? ContainmentType.Intersects : ContainmentType.Contains;
        }

        ContainmentType ICuller.IntersectBox(ref Vector3 min, ref Vector3 max, ref Matrix localMatrix)
        {
            if (matrices.World.isIdentity)
                return ((ICuller)this).IntersectWorldBox(ref min, ref max, ref localMatrix);

            Matrix matrix;
#if NO_INLINE
			Matrix.Multiply(ref localMatrix, ref worldMatrix.value, out matrix);
#else
#if XBOX360
			matrix = new Matrix();
#endif
            matrix.M11 = (((localMatrix.M11 * matrices.World.value.M11) + (localMatrix.M12 * matrices.World.value.M21)) + (localMatrix.M13 * matrices.World.value.M31)) + (localMatrix.M14 * matrices.World.value.M41);
            matrix.M12 = (((localMatrix.M11 * matrices.World.value.M12) + (localMatrix.M12 * matrices.World.value.M22)) + (localMatrix.M13 * matrices.World.value.M32)) + (localMatrix.M14 * matrices.World.value.M42);
            matrix.M13 = (((localMatrix.M11 * matrices.World.value.M13) + (localMatrix.M12 * matrices.World.value.M23)) + (localMatrix.M13 * matrices.World.value.M33)) + (localMatrix.M14 * matrices.World.value.M43);
            matrix.M14 = (((localMatrix.M11 * matrices.World.value.M14) + (localMatrix.M12 * matrices.World.value.M24)) + (localMatrix.M13 * matrices.World.value.M34)) + (localMatrix.M14 * matrices.World.value.M44);
            matrix.M21 = (((localMatrix.M21 * matrices.World.value.M11) + (localMatrix.M22 * matrices.World.value.M21)) + (localMatrix.M23 * matrices.World.value.M31)) + (localMatrix.M24 * matrices.World.value.M41);
            matrix.M22 = (((localMatrix.M21 * matrices.World.value.M12) + (localMatrix.M22 * matrices.World.value.M22)) + (localMatrix.M23 * matrices.World.value.M32)) + (localMatrix.M24 * matrices.World.value.M42);
            matrix.M23 = (((localMatrix.M21 * matrices.World.value.M13) + (localMatrix.M22 * matrices.World.value.M23)) + (localMatrix.M23 * matrices.World.value.M33)) + (localMatrix.M24 * matrices.World.value.M43);
            matrix.M24 = (((localMatrix.M21 * matrices.World.value.M14) + (localMatrix.M22 * matrices.World.value.M24)) + (localMatrix.M23 * matrices.World.value.M34)) + (localMatrix.M24 * matrices.World.value.M44);
            matrix.M31 = (((localMatrix.M31 * matrices.World.value.M11) + (localMatrix.M32 * matrices.World.value.M21)) + (localMatrix.M33 * matrices.World.value.M31)) + (localMatrix.M34 * matrices.World.value.M41);
            matrix.M32 = (((localMatrix.M31 * matrices.World.value.M12) + (localMatrix.M32 * matrices.World.value.M22)) + (localMatrix.M33 * matrices.World.value.M32)) + (localMatrix.M34 * matrices.World.value.M42);
            matrix.M33 = (((localMatrix.M31 * matrices.World.value.M13) + (localMatrix.M32 * matrices.World.value.M23)) + (localMatrix.M33 * matrices.World.value.M33)) + (localMatrix.M34 * matrices.World.value.M43);
            matrix.M34 = (((localMatrix.M31 * matrices.World.value.M14) + (localMatrix.M32 * matrices.World.value.M24)) + (localMatrix.M33 * matrices.World.value.M34)) + (localMatrix.M34 * matrices.World.value.M44);
            matrix.M41 = (((localMatrix.M41 * matrices.World.value.M11) + (localMatrix.M42 * matrices.World.value.M21)) + (localMatrix.M43 * matrices.World.value.M31)) + (localMatrix.M44 * matrices.World.value.M41);
            matrix.M42 = (((localMatrix.M41 * matrices.World.value.M12) + (localMatrix.M42 * matrices.World.value.M22)) + (localMatrix.M43 * matrices.World.value.M32)) + (localMatrix.M44 * matrices.World.value.M42);
            matrix.M43 = (((localMatrix.M41 * matrices.World.value.M13) + (localMatrix.M42 * matrices.World.value.M23)) + (localMatrix.M43 * matrices.World.value.M33)) + (localMatrix.M44 * matrices.World.value.M43);
            matrix.M44 = (((localMatrix.M41 * matrices.World.value.M14) + (localMatrix.M42 * matrices.World.value.M24)) + (localMatrix.M43 * matrices.World.value.M34)) + (localMatrix.M44 * matrices.World.value.M44);
#endif

            ContainmentType type; bool intersect = false;

            for (int i = preCullerCount - 1; i >= 0; i--)
            {
                type = preCullers[i].IntersectWorldBox(ref min, ref max, ref matrix);
                if (type == ContainmentType.Disjoint)
                    return type;

                if (type == ContainmentType.Intersects)
                    intersect = true;
            }

            type = FrustumCull.BoxIntersectsFrustum(camera.GetCullingPlanes(), ref min, ref max, ref matrix);
            if (type == ContainmentType.Disjoint)
                return type;

            if (type == ContainmentType.Intersects)
                intersect = true;

            for (int i = 0; i < postCullerCount; i++)
            {
                type = postCullers[i].IntersectWorldBox(ref min, ref max, ref matrix);
                if (type == ContainmentType.Disjoint)
                    return type;

                if (type == ContainmentType.Intersects)
                    intersect = true;
            }

            return intersect ? ContainmentType.Intersects : ContainmentType.Contains;
        }

        ContainmentType ICuller.IntersectSphere(float radius)
        {
            ContainmentType type; bool intersect = false;

            Vector3 pos;
#if XBOX360
			pos = new Vector3();
#endif
            pos.X = matrices.World.value.M41;
            pos.Y = matrices.World.value.M42;
            pos.Z = matrices.World.value.M43;

            for (int i = preCullerCount - 1; i >= 0; i--)
            {
                type = preCullers[i].IntersectWorldSphere(radius, ref pos);
                if (type == ContainmentType.Disjoint)
                    return type;

                if (type == ContainmentType.Intersects)
                    intersect = true;
            }

            type = FrustumCull.SphereIntersectsFrustum(camera.GetCullingPlanes(), radius, ref pos);
            if (type == ContainmentType.Disjoint)
                return type;

            if (type == ContainmentType.Intersects)
                intersect = true;

            for (int i = 0; i < postCullerCount; i++)
            {
                type = postCullers[i].IntersectWorldSphere(radius, ref pos);
                if (type == ContainmentType.Disjoint)
                    return type;

                if (type == ContainmentType.Intersects)
                    intersect = true;
            }

            return intersect ? ContainmentType.Intersects : ContainmentType.Contains;
        }

        ContainmentType ICuller.IntersectSphere(float radius, Vector3 position)
        {
            return ((ICuller)this).IntersectSphere(radius, ref position);
        }

        ContainmentType ICuller.IntersectSphere(float radius, ref Vector3 position)
        {
            ContainmentType type; bool intersect = false;

            Vector3 pos;
            Vector3.Transform(ref position, ref matrices.World.value, out pos);

            for (int i = preCullerCount - 1; i >= 0; i--)
            {
                type = preCullers[i].IntersectWorldSphere(radius, ref pos);
                if (type == ContainmentType.Disjoint)
                    return type;

                if (type == ContainmentType.Intersects)
                    intersect = true;
            }

            type = FrustumCull.SphereIntersectsFrustum(camera.GetCullingPlanes(), radius, ref pos);

            if (type == ContainmentType.Disjoint)
                return type;

            if (type == ContainmentType.Intersects)
                intersect = true;

            for (int i = 0; i < postCullerCount; i++)
            {
                type = postCullers[i].IntersectWorldSphere(radius, ref pos);
                if (type == ContainmentType.Disjoint)
                    return type;

                if (type == ContainmentType.Intersects)
                    intersect = true;
            }

            return intersect ? ContainmentType.Intersects : ContainmentType.Contains;
        }

        #endregion ICuller tests

        #region cull primitives

        bool ICullPrimitive.TestWorldBox(Vector3 min, Vector3 max, ref Matrix world)
        {
            for (int i = preCullerCount - 1; i >= 0; i--)
            {
                if (!preCullers[i].TestWorldBox(ref min, ref max, ref world))
                    return false;
            }

            if (!FrustumCull.BoxInFrustum(camera.GetCullingPlanes(), ref min, ref max, ref world))
                return false;

            for (int i = 0; i < postCullerCount; i++)
            {
                if (!postCullers[i].TestWorldBox(ref min, ref max, ref world))
                    return false;
            }
            return true;
        }

        bool ICullPrimitive.TestWorldBox(ref Vector3 min, ref Vector3 max, ref Matrix world)
        {
            for (int i = preCullerCount - 1; i >= 0; i--)
            {
                if (!preCullers[i].TestWorldBox(ref min, ref max, ref world))
                    return false;
            }

            if (!FrustumCull.BoxInFrustum(camera.GetCullingPlanes(), ref min, ref max, ref world))
                return false;

            for (int i = 0; i < postCullerCount; i++)
            {
                if (!postCullers[i].TestWorldBox(ref min, ref max, ref world))
                    return false;
            }
            return true;
        }

        bool ICullPrimitive.TestWorldBox(Vector3 min, Vector3 max)
        {
            for (int i = preCullerCount - 1; i >= 0; i--)
            {
                if (!preCullers[i].TestWorldBox(ref min, ref max))
                    return false;
            }

            if (!FrustumCull.AABBInFrustum(camera.GetCullingPlanes(), ref min, ref max))
                return false;

            for (int i = 0; i < postCullerCount; i++)
            {
                if (!postCullers[i].TestWorldBox(ref min, ref max))
                    return false;
            }
            return true;
        }

        bool ICullPrimitive.TestWorldBox(ref Vector3 min, ref Vector3 max)
        {
            for (int i = preCullerCount - 1; i >= 0; i--)
            {
                if (!preCullers[i].TestWorldBox(ref min, ref max))
                    return false;
            }

            if (!FrustumCull.AABBInFrustum(camera.GetCullingPlanes(), ref min, ref max))
                return false;

            for (int i = 0; i < postCullerCount; i++)
            {
                if (!postCullers[i].TestWorldBox(ref min, ref max))
                    return false;
            }
            return true;
        }

        bool ICullPrimitive.TestWorldSphere(float radius, Vector3 position)
        {
            for (int i = preCullerCount - 1; i >= 0; i--)
            {
                if (!preCullers[i].TestWorldSphere(radius, ref position))
                    return false;
            }

            if (!FrustumCull.SphereInFrustum(camera.GetCullingPlanes(), radius, ref position))
                return false;

            for (int i = 0; i < postCullerCount; i++)
            {
                if (!postCullers[i].TestWorldSphere(radius, ref position))
                    return false;
            }
            return true;
        }

        bool ICullPrimitive.TestWorldSphere(float radius, ref Vector3 position)
        {
            for (int i = preCullerCount - 1; i >= 0; i--)
            {
                if (!preCullers[i].TestWorldSphere(radius, ref position))
                    return false;
            }

            if (!FrustumCull.SphereInFrustum(camera.GetCullingPlanes(), radius, ref position))
                return false;

            for (int i = 0; i < postCullerCount; i++)
            {
                if (!postCullers[i].TestWorldSphere(radius, ref position))
                    return false;
            }
            return true;
        }

        ContainmentType ICullPrimitive.IntersectWorldBox(Vector3 min, Vector3 max, ref Matrix world)
        {
            ContainmentType type; bool intersect = false;

            for (int i = preCullerCount - 1; i >= 0; i--)
            {
                type = preCullers[i].IntersectWorldBox(ref min, ref max, ref world);
                if (type == ContainmentType.Disjoint)
                    return type;

                if (type == ContainmentType.Intersects)
                    intersect = true;
            }

            type = FrustumCull.BoxIntersectsFrustum(camera.GetCullingPlanes(), ref min, ref max, ref world);
            if (type == ContainmentType.Disjoint)
                return type;

            if (type == ContainmentType.Intersects)
                intersect = true;

            for (int i = 0; i < postCullerCount; i++)
            {
                type = postCullers[i].IntersectWorldBox(ref min, ref max, ref world);
                if (type == ContainmentType.Disjoint)
                    return type;

                if (type == ContainmentType.Intersects)
                    intersect = true;
            }
            return intersect ? ContainmentType.Intersects : ContainmentType.Contains;
        }

        ContainmentType ICullPrimitive.IntersectWorldBox(ref Vector3 min, ref Vector3 max, ref Matrix world)
        {
            ContainmentType type; bool intersect = false;

            for (int i = preCullerCount - 1; i >= 0; i--)
            {
                type = preCullers[i].IntersectWorldBox(ref min, ref max, ref world);
                if (type == ContainmentType.Disjoint)
                    return type;

                if (type == ContainmentType.Intersects)
                    intersect = true;
            }

            type = FrustumCull.BoxIntersectsFrustum(camera.GetCullingPlanes(), ref min, ref max, ref world);
            if (type == ContainmentType.Disjoint)
                return type;

            if (type == ContainmentType.Intersects)
                intersect = true;

            for (int i = 0; i < postCullerCount; i++)
            {
                type = postCullers[i].IntersectWorldBox(ref min, ref max, ref world);
                if (type == ContainmentType.Disjoint)
                    return type;

                if (type == ContainmentType.Intersects)
                    intersect = true;
            }
            return intersect ? ContainmentType.Intersects : ContainmentType.Contains;
        }

        ContainmentType ICullPrimitive.IntersectWorldBox(Vector3 min, Vector3 max)
        {
            ContainmentType type; bool intersect = false;

            for (int i = preCullerCount - 1; i >= 0; i--)
            {
                type = preCullers[i].IntersectWorldBox(ref min, ref max);
                if (type == ContainmentType.Disjoint)
                    return type;

                if (type == ContainmentType.Intersects)
                    intersect = true;
            }

            type = FrustumCull.AABBIntersectsFrustum(camera.GetCullingPlanes(), ref min, ref max);
            if (type == ContainmentType.Disjoint)
                return type;

            if (type == ContainmentType.Intersects)
                intersect = true;

            for (int i = 0; i < postCullerCount; i++)
            {
                type = postCullers[i].IntersectWorldBox(ref min, ref max);
                if (type == ContainmentType.Disjoint)
                    return type;

                if (type == ContainmentType.Intersects)
                    intersect = true;
            }
            return intersect ? ContainmentType.Intersects : ContainmentType.Contains;
        }

        ContainmentType ICullPrimitive.IntersectWorldBox(ref Vector3 min, ref Vector3 max)
        {
            ContainmentType type; bool intersect = false;

            for (int i = preCullerCount - 1; i >= 0; i--)
            {
                type = preCullers[i].IntersectWorldBox(ref min, ref max);
                if (type == ContainmentType.Disjoint)
                    return type;

                if (type == ContainmentType.Intersects)
                    intersect = true;
            }

            type = FrustumCull.AABBIntersectsFrustum(camera.GetCullingPlanes(), ref min, ref max);
            if (type == ContainmentType.Disjoint)
                return type;

            if (type == ContainmentType.Intersects)
                intersect = true;

            for (int i = 0; i < postCullerCount; i++)
            {
                type = postCullers[i].IntersectWorldBox(ref min, ref max);
                if (type == ContainmentType.Disjoint)
                    return type;

                if (type == ContainmentType.Intersects)
                    intersect = true;
            }
            return intersect ? ContainmentType.Intersects : ContainmentType.Contains;
        }

        ContainmentType ICullPrimitive.IntersectWorldSphere(float radius, Vector3 position)
        {
            ContainmentType type; bool intersect = false;

            for (int i = preCullerCount - 1; i >= 0; i--)
            {
                type = preCullers[i].IntersectWorldSphere(radius, ref position);
                if (type == ContainmentType.Disjoint)
                    return type;

                if (type == ContainmentType.Intersects)
                    intersect = true;
            }

            type = FrustumCull.SphereIntersectsFrustum(camera.GetCullingPlanes(), radius, ref position);
            if (type == ContainmentType.Disjoint)
                return type;

            if (type == ContainmentType.Intersects)
                intersect = true;

            for (int i = 0; i < postCullerCount; i++)
            {
                type = postCullers[i].IntersectWorldSphere(radius, ref position);
                if (type == ContainmentType.Disjoint)
                    return type;

                if (type == ContainmentType.Intersects)
                    intersect = true;
            }
            return intersect ? ContainmentType.Intersects : ContainmentType.Contains;
        }

        ContainmentType ICullPrimitive.IntersectWorldSphere(float radius, ref Vector3 position)
        {
            ContainmentType type; bool intersect = false;

            for (int i = preCullerCount - 1; i >= 0; i--)
            {
                type = preCullers[i].IntersectWorldSphere(radius, ref position);
                if (type == ContainmentType.Disjoint)
                    return type;

                if (type == ContainmentType.Intersects)
                    intersect = true;
            }

            type = FrustumCull.SphereIntersectsFrustum(camera.GetCullingPlanes(), radius, ref position);
            if (type == ContainmentType.Disjoint)
                return type;

            if (type == ContainmentType.Intersects)
                intersect = true;

            for (int i = 0; i < postCullerCount; i++)
            {
                type = postCullers[i].IntersectWorldSphere(radius, ref position);
                if (type == ContainmentType.Disjoint)
                    return type;

                if (type == ContainmentType.Intersects)
                    intersect = true;
            }
            return intersect ? ContainmentType.Intersects : ContainmentType.Contains;
        }

        #endregion cull primitives

        void ICuller.GetWorldMatrix(out Matrix matrix)
        {
            matrix = this.matrices.World.value;
        }

        void ICuller.GetWorldPosition(out Vector3 position)
        {
            position = new Vector3();
            position.X = this.matrices.World.value.M41;
            position.Y = this.matrices.World.value.M42;
            position.Z = this.matrices.World.value.M43;
        }
    }
}