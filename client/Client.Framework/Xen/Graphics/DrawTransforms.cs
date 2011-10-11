using System;

//using Xen.Graphics.ShaderSystem;
using Microsoft.Xna.Framework;
using Xen.Camera;

namespace Xen.Graphics.Stack
{
    //storage for world matrix, projection, etc.

    /*
     * This file implements DrawState shader support for camera and world matrix related matrices, cullers and other camera ops
     *
     */

    /// <summary>
    /// Internal source tracking class for matrix storage
    /// </summary>
#if !DEBUG_API

    [System.Diagnostics.DebuggerStepThrough]
#endif
    public abstract class MatrixSource
    {
        internal MatrixSource(DrawState state)
        {
            this.state = state;
        }

        internal Matrix value = Matrix.Identity;
        internal Matrix transpose = Matrix.Identity;
        internal Matrix inverse = Matrix.Identity;

        internal int index = 1, transposeIndex = 1, inverseIndex = 1;
        internal readonly DrawState state;

        //make sure to compare index != transposeIndex first.
        internal void UpdateTranspose()
        {
            this.transposeIndex = index;

            this.transpose.M11 = this.value.M11;
            this.transpose.M12 = this.value.M21;
            this.transpose.M13 = this.value.M31;
            this.transpose.M14 = this.value.M41;
            this.transpose.M21 = this.value.M12;
            this.transpose.M22 = this.value.M22;
            this.transpose.M23 = this.value.M32;
            this.transpose.M24 = this.value.M42;
            this.transpose.M31 = this.value.M13;
            this.transpose.M32 = this.value.M23;
            this.transpose.M33 = this.value.M33;
            this.transpose.M34 = this.value.M43;
            this.transpose.M41 = this.value.M14;
            this.transpose.M42 = this.value.M24;
            this.transpose.M43 = this.value.M34;
            this.transpose.M44 = this.value.M44;
        }

        //make sure to compare index != inverseIndex first.
        internal void UpdateInverse()
        {
            this.inverseIndex = index;

            Matrix.Invert(ref this.value, out this.inverse);
        }

        /// <summary>
        /// Empty in the base class, only used by the Effect mapper
        /// </summary>
        internal virtual void UpdateMultiply()
        {
        }

        /// <summary>
        /// Get the Matrix
        /// </summary>
        public abstract void GetMatrix(out Matrix matrix);

        /// <summary>
        /// Get the transpose of the Matrix
        /// </summary>
        public abstract void GetTransposeMatrix(out Matrix matrix);

        /// <summary>
        /// Get the inverse of the Matrix
        /// </summary>
        public abstract void GetInverseMatrix(out Matrix matrix);
    }

#if !DEBUG_API

    [System.Diagnostics.DebuggerStepThrough]
#endif
    internal sealed class ViewProvider : MatrixSource
    {
        internal ICamera camera;
        private int camIndex;

        internal MatrixMultiply WorldView, WorldViewProjection, ViewProjection;

        public ViewProvider(DrawState state)
            : base(state)
        {
        }

        #region get / set

        public void SetViewCamera(ICamera value)
        {
            if (value != camera)
            {
                camera = value;
                camIndex = 0;
            }
            if (camera != null)
            {
                if (camera.GetViewMatrix(ref this.value, ref this.camIndex))
                {
                    index++;
                    WorldView.index++;
                    WorldViewProjection.index++;
                    ViewProjection.index++;
                }
            }
        }

        #endregion get / set

        public override void GetMatrix(out Matrix matrix)
        {
            matrix = this.value;
        }

        public override void GetTransposeMatrix(out Matrix matrix)
        {
            if (this.transposeIndex != this.index)
                UpdateTranspose();
            matrix = transpose;
        }

        public override void GetInverseMatrix(out Matrix matrix)
        {
            if (this.inverseIndex != this.index)
                UpdateInverse();
            matrix = inverse;
        }
    }

#if !DEBUG_API

    [System.Diagnostics.DebuggerStepThrough]
#endif
    internal sealed class ProjectionProvider : MatrixSource
    {
        private ICamera cam;
        private int camIndex;

        internal MatrixMultiply WorldViewProjection, WorldProjection, ViewProjection;

        public ProjectionProvider(DrawState state)
            : base(state)
        {
        }

        #region get / set

        public void SetProjectionCamera(ICamera value, ref Vector2 drawTargetSize)
        {
            if (value != cam)
            {
                cam = value;
                camIndex = 0;
            }
            if (cam != null)
            {
                if (cam.GetProjectionMatrix(ref this.value, ref drawTargetSize, ref this.camIndex))
                {
                    index++;

                    WorldViewProjection.index++;
                    WorldProjection.index++;
                    ViewProjection.index++;
                }
            }
        }

        #endregion get / set

        public override void GetMatrix(out Matrix matrix)
        {
            matrix = this.value;
        }

        public override void GetTransposeMatrix(out Matrix matrix)
        {
            if (this.transposeIndex != this.index)
                UpdateTranspose();
            matrix = transpose;
        }

        public override void GetInverseMatrix(out Matrix matrix)
        {
            if (this.inverseIndex != this.index)
                UpdateInverse();
            matrix = inverse;
        }
    }

    /// <summary>
    /// Provider class for storing a stack for the rendering World Matrix
    /// </summary>
#if !DEBUG_API

    [System.Diagnostics.DebuggerStepThrough]
#endif
    public sealed class WorldMatrixStackProvider : MatrixSource
    {
        /// <summary>
        /// Structure used for a using block with a Push method
        /// </summary>
        [System.Diagnostics.DebuggerStepThrough]
        public struct UsingPop : IDisposable
        {
            internal UsingPop(WorldMatrixStackProvider stack)
            {
                this.stack = stack;
            }

            private readonly WorldMatrixStackProvider stack;

            /// <summary>Invokes the Pop metohd</summary>
            public void Dispose()
            {
                stack.Pop();
            }
        }
        internal readonly UsingPop UsingBlock;

        //push operators:
        /// <summary>
        /// Wrapper on the Push method
        /// </summary>
        public static UsingPop operator +(WorldMatrixStackProvider state, Matrix world)
        {
            return state.Push(ref world);
        }

        /// <summary>
        /// Wrapper on the PushMultiply method
        /// </summary>
        public static UsingPop operator *(WorldMatrixStackProvider state, Matrix world)
        {
            return state.PushMultiply(ref world);
        }

        /// <summary>
        /// Wrapper on the Push method
        /// </summary>
        public static UsingPop operator +(WorldMatrixStackProvider state, Vector3 pos)
        {
            return state.PushTranslate(ref pos);
        }

        /// <summary>
        /// Wrapper on the PushMultiply method
        /// </summary>
        public static UsingPop operator *(WorldMatrixStackProvider state, Vector3 pos)
        {
            return state.PushTranslateMultiply(ref pos);
        }

        /// <summary>
        /// Cast to a <see cref="Matrix"/> implicitly
        /// </summary>
        public static implicit operator Matrix(WorldMatrixStackProvider source)
        {
            return source.value;
        }

        //source matrices that this class affects
        internal MatrixMultiply WorldView, WorldViewProjection, WorldProjection;

        //internal stack storage
        private readonly Matrix[] stack;
        private readonly int[] stackIndex;
        private readonly bool[] stackIdentity;
        private int highpoint = 1;
        internal uint top;

        internal bool isIdentity = true;

        internal WorldMatrixStackProvider(uint stackSize, DrawState state)
            : base(state)
        {
            stack = new Matrix[stackSize];
            stackIndex = new int[stackSize];
            stackIdentity = new bool[stackSize];
            this.UsingBlock = new UsingPop(this);
        }

        #region set/get/stack methods

        /// <summary>
        /// Sets the top of the current rendering world matrix stack to the matrix
        /// </summary>
        public void SetMatrix(ref Matrix matrix)
        {
            if (top == 0)
            {
                throw new InvalidOperationException("World matrix at the bottom of the stack must stay an Identity Matrix, Please use Push(ref Matrix)");
            }

            this.value = matrix;
            index = ++highpoint;
            isIdentity = false;

            WorldView.index++;
            WorldViewProjection.index++;
            WorldProjection.index++;
        }

        /// <summary>
        /// Sets the top of the current rendering world matrix stack to the position
        /// </summary>
        public void SetTranslate(ref Vector3 translate)
        {
            if (top == 0)
            {
                throw new InvalidOperationException("World matrix at the bottom of the stack must stay an Identity Matrix, Please use PushTranslate()");
            }

            this.value.M11 = 1;
            this.value.M12 = 0;
            this.value.M13 = 0;
            this.value.M14 = 0;

            this.value.M21 = 0;
            this.value.M22 = 1;
            this.value.M23 = 0;
            this.value.M24 = 0;

            this.value.M31 = 0;
            this.value.M32 = 0;
            this.value.M33 = 1;
            this.value.M34 = 0;

            this.value.M41 = translate.X;
            this.value.M42 = translate.Y;
            this.value.M43 = translate.Z;
            this.value.M44 = 1;

            index = ++highpoint;
            isIdentity = translate.X == 0 && translate.Y == 0 && translate.Z == 0;

            WorldView.index++;
            WorldViewProjection.index++;
            WorldProjection.index++;
        }

        /// <summary>
        /// Gets the top of the current rendering world matrix stack
        /// </summary>
        public override void GetMatrix(out Matrix matrix)
        {
            matrix = this.value;
        }

        /// <summary>
        /// Gets the current world matrix at the top of the world matrix stack
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="isIdentity">true if the matrix is guaranteed to be an identity matrix (this value may return a false negative)</param>
        public void GetMatrix(out Matrix matrix, out bool isIdentity)
        {
            matrix = this.value;
            isIdentity = this.isIdentity;
        }

        /// <summary>
        /// <para>Gets/Sets the position of the current rendering world matrix</para>
        /// <para>Note: Rotation is not modified when assinging this value</para>
        /// </summary>
        public Vector3 Position
        {
            get { return new Vector3(this.value.M41, this.value.M42, this.value.M43); }
            set
            {
                if (value.X != this.value.M41 ||
                    value.Y != this.value.M42 ||
                    value.Z != this.value.M43)
                {
                    this.value.M41 = value.X;
                    this.value.M42 = value.Y;
                    this.value.M43 = value.Z;

                    index = ++highpoint;
                    isIdentity = false;

                    WorldView.index++;
                    WorldViewProjection.index++;
                    WorldProjection.index++;
                }
            }
        }

        /// <summary>
        /// Gets the translation position stored in the current world matrix
        /// </summary>
        /// <param name="position"></param>
        public void GetPosition(out Vector3 position)
        {
#if XBOX360
			position = new Vector3();
#endif
            position.X = this.value.M41;
            position.Y = this.value.M42;
            position.Z = this.value.M43;
        }

        /// <summary>
        /// Pushes the matrix on to the top of the current rendering world matrix stack
        /// <para>Note: this method can be used in a 'using() {}' block to automatically call <see cref="Pop"/></para>
        /// </summary>
        public UsingPop Push(ref Matrix matrix)
        {
            if (top == 0)
            {
                this.value = matrix;
                index = ++highpoint;
                isIdentity = false;

                WorldView.index++;
                WorldViewProjection.index++;
                WorldProjection.index++;
            }
            else
            {
                if (index != stackIndex[top])
                {
                    stack[top] = matrix;
                    stackIndex[top] = index;
                    stackIdentity[top] = isIdentity;
                }

                this.value = matrix;
                index = ++highpoint;
                isIdentity = false;

                WorldView.index++;
                WorldViewProjection.index++;
                WorldProjection.index++;
            }

            top++;

            return UsingBlock;
        }

        /// <summary>
        /// Pushes an identity matrix to the top of the stack
        /// <para>Note: this method can be used in a 'using() {}' block to automatically call <see cref="Pop"/></para>
        /// </summary>
        public UsingPop PushIdentity()
        {
            if (top != 0)
            {
                this.value = Matrix.Identity;

                stack[top] = this.value;
                stackIndex[top] = index;
                stackIdentity[top] = true;

                isIdentity = true;
            }

            index = ++highpoint;
            WorldView.index++;
            WorldViewProjection.index++;
            WorldProjection.index++;

            top++;

            return UsingBlock;
        }

        /// <summary>
        /// Copies the current world matrix to the top of the stack
        /// <para>Note: this method can be used in a 'using() {}' block to automatically call <see cref="Pop"/></para>
        /// </summary>
        public UsingPop Push()
        {
            if (top != 0)
            {
                if (index != stackIndex[top])
                {
                    stack[top] = value;
                    stackIndex[top] = index;
                    stackIdentity[top] = isIdentity;
                }
            }
            top++;

            return UsingBlock;
        }

        /// <summary>
        /// Optimised equivalent of calling <see cref="Push(ref Matrix)"/>(<see cref="Microsoft.Xna.Framework.Matrix.CreateTranslation(Vector3)"/>)
        /// <para>Note: this method can be used in a 'using() {}' block to automatically call <see cref="Pop"/></para>
        /// </summary>
        public UsingPop PushTranslate(ref Vector3 translate)
        {
            if (top == 0)
            {
                if (translate.X != 0 || translate.Y != 0 || translate.Z != 0)
                {
                    this.value.M41 = translate.X;
                    this.value.M42 = translate.Y;
                    this.value.M43 = translate.Z;
                    isIdentity = false;

                    index = ++highpoint;

                    WorldView.index++;
                    WorldViewProjection.index++;
                    WorldProjection.index++;
                }
            }
            else
            {
                if (index != stackIndex[top])
                {
                    stack[top] = value;
                    stackIndex[top] = index;
                    stackIdentity[top] = isIdentity;
                }

                this.value.M11 = 1;
                this.value.M12 = 0;
                this.value.M13 = 0;
                this.value.M14 = 0;

                this.value.M21 = 0;
                this.value.M22 = 1;
                this.value.M23 = 0;
                this.value.M24 = 0;

                this.value.M31 = 0;
                this.value.M32 = 0;
                this.value.M33 = 1;
                this.value.M34 = 0;

                this.value.M41 = translate.X;
                this.value.M42 = translate.Y;
                this.value.M43 = translate.Z;
                this.value.M44 = 1;

                index = ++highpoint;
                isIdentity = false;

                WorldView.index++;
                WorldViewProjection.index++;
                WorldProjection.index++;
            }

            top++;

            return UsingBlock;
        }

        /// <summary>
        /// Optimised equivalent of calling <see cref="Push(ref Matrix)"/>(<see cref="Microsoft.Xna.Framework.Matrix.CreateTranslation(Vector3)"/>)
        /// <para>Note: this method can be used in a 'using() {}' block to automatically call <see cref="Pop"/></para>
        /// </summary>
        public UsingPop PushTranslate(Vector3 translate)
        {
            return PushTranslate(ref translate);
        }

        /// <summary>
        /// Optimised equivalent of calling <see cref="PushMultiply(ref Matrix)"/>(<see cref="Microsoft.Xna.Framework.Matrix.CreateTranslation(Vector3)"/>)
        /// <para>Note: this method can be used in a 'using() {}' block to automatically call <see cref="Pop"/></para>
        /// </summary>
        public UsingPop PushTranslateMultiply(ref Vector3 translate)
        {
            if (top == 0)
            {
                if (
                    translate.X != 0 || translate.Y != 0 || translate.Z != 0
                    )//prevents recalcuating shader constants later if not changed now
                {
                    this.value.M41 = translate.X;
                    this.value.M42 = translate.Y;
                    this.value.M43 = translate.Z;

                    isIdentity = false;
                    index = ++highpoint;

                    WorldView.index++;
                    WorldViewProjection.index++;
                    WorldProjection.index++;
                }
            }
            else
            {
                if (index != stackIndex[top])
                {
                    stack[top] = this.value;
                    stackIndex[top] = index;
                    stackIdentity[top] = isIdentity;
                }

                if (translate.X != 0 || translate.Y != 0 || translate.Z != 0)
                {
                    if (isIdentity)
                    {
                        this.value.M41 = translate.X;
                        this.value.M42 = translate.Y;
                        this.value.M43 = translate.Z;
                    }
                    else
                    {
                        this.value.M41 += translate.X * this.value.M11 +
                                            translate.Y * this.value.M21 +
                                            translate.Z * this.value.M31;

                        this.value.M42 += translate.X * this.value.M12 +
                                            translate.Y * this.value.M22 +
                                            translate.Z * this.value.M32;

                        this.value.M43 += translate.X * this.value.M13 +
                                            translate.Y * this.value.M23 +
                                            translate.Z * this.value.M33;
                    }
                    isIdentity = false;
                    index = ++highpoint;

                    WorldView.index++;
                    WorldViewProjection.index++;
                    WorldProjection.index++;
                }
            }
            top++;

            return UsingBlock;
        }

        /// <summary>
        /// Optimised equivalent of calling <see cref="PushMultiply(ref Matrix)"/>(<see cref="Microsoft.Xna.Framework.Matrix.CreateTranslation(Vector3)"/>)
        /// <para>Note: this method can be used in a 'using() {}' block to automatically call <see cref="Pop"/></para>
        /// </summary>
        public UsingPop PushTranslateMultiply(Vector3 translate)
        {
            return PushTranslateMultiply(ref translate);
        }

        /// <summary>
        /// Returns true if the world matrix has changed compared to the passed in change index
        /// </summary>
        public bool HasChanged(ref int changeIndex)
        {
            bool result = changeIndex != index;
            changeIndex = index;
            return result;
        }

        /// <summary>
        /// Pushes the transform on to the top of the current rendering world matrix stack
        /// <para>Note: this method can be used in a 'using() {}' block to automatically call <see cref="Pop"/></para>
        /// </summary>
        public UsingPop Push(ref Transform transform)
        {
            Matrix mat;
            Matrix.CreateFromQuaternion(ref transform.Rotation, out mat);

            mat.M41 = transform.Translation.X;
            mat.M42 = transform.Translation.Y;
            mat.M43 = transform.Translation.Z;

            float Scale = transform.Scale;
            if (Scale != 1)
            {
                mat.M11 *= Scale;
                mat.M12 *= Scale;
                mat.M13 *= Scale;

                mat.M21 *= Scale;
                mat.M22 *= Scale;
                mat.M23 *= Scale;

                mat.M31 *= Scale;
                mat.M32 *= Scale;
                mat.M33 *= Scale;
            }

            return Push(ref mat);
        }

        /// <summary>
        /// Pushes the transform on to the top of the current rendering world matrix stack, multiplying with the existing world matrix.
        /// <para>Note: this method can be used in a 'using() {}' block to automatically call <see cref="Pop"/></para>
        /// </summary>
        public UsingPop PushMultiply(ref Transform transform)
        {
            Matrix mat;
            Matrix.CreateFromQuaternion(ref transform.Rotation, out mat);

            mat.M41 = transform.Translation.X;
            mat.M42 = transform.Translation.Y;
            mat.M43 = transform.Translation.Z;

            float Scale = transform.Scale;
            if (Scale != 1)
            {
                mat.M11 *= Scale;
                mat.M12 *= Scale;
                mat.M13 *= Scale;

                mat.M21 *= Scale;
                mat.M22 *= Scale;
                mat.M23 *= Scale;

                mat.M31 *= Scale;
                mat.M32 *= Scale;
                mat.M33 *= Scale;
            }

            return PushMultiply(ref mat);
        }

        /// <summary>
        /// Multiplies the matrix at the top of the rendering world matrix stack with the input transform
        /// </summary>
        public void Multiply(ref Transform transform)
        {
            Matrix mat;
            Matrix.CreateFromQuaternion(ref transform.Rotation, out mat);

            mat.M41 = transform.Translation.X;
            mat.M42 = transform.Translation.Y;
            mat.M43 = transform.Translation.Z;

            float Scale = transform.Scale;
            if (Scale != 1)
            {
                mat.M11 *= Scale;
                mat.M12 *= Scale;
                mat.M13 *= Scale;

                mat.M21 *= Scale;
                mat.M22 *= Scale;
                mat.M23 *= Scale;

                mat.M31 *= Scale;
                mat.M32 *= Scale;
                mat.M33 *= Scale;
            }

            Multiply(ref mat);
        }

        /// <summary>
        /// Pushes the matrix on to the top of the current rendering world matrix stack, multiplying with the  existing world matrix.
        /// <para>Note: this method can be used in a 'using() {}' block to automatically call <see cref="Pop"/></para>
        /// </summary>
        public UsingPop PushMultiply(ref Matrix matrix)
        {
            if (top == 0)
            {
                this.value = matrix;
                index = ++highpoint;
                isIdentity = false;

                WorldView.index++;
                WorldViewProjection.index++;
                WorldProjection.index++;
            }
            else
            {
                if (index != stackIndex[top])
                {
                    stack[top] = this.value;
                    stackIndex[top] = index;
                    stackIdentity[top] = isIdentity;
                }

                if (isIdentity)
                    this.value = matrix;
                else
                {
#if NO_INLINE
					Matrix.Multiply(ref matrix, ref this.value, out this.value);
#else
                    float num16 = (((matrix.M11 * this.value.M11) + (matrix.M12 * this.value.M21)) + (matrix.M13 * this.value.M31)) + (matrix.M14 * this.value.M41);
                    float num15 = (((matrix.M11 * this.value.M12) + (matrix.M12 * this.value.M22)) + (matrix.M13 * this.value.M32)) + (matrix.M14 * this.value.M42);
                    float num14 = (((matrix.M11 * this.value.M13) + (matrix.M12 * this.value.M23)) + (matrix.M13 * this.value.M33)) + (matrix.M14 * this.value.M43);
                    float num13 = (((matrix.M11 * this.value.M14) + (matrix.M12 * this.value.M24)) + (matrix.M13 * this.value.M34)) + (matrix.M14 * this.value.M44);
                    float num12 = (((matrix.M21 * this.value.M11) + (matrix.M22 * this.value.M21)) + (matrix.M23 * this.value.M31)) + (matrix.M24 * this.value.M41);
                    float num11 = (((matrix.M21 * this.value.M12) + (matrix.M22 * this.value.M22)) + (matrix.M23 * this.value.M32)) + (matrix.M24 * this.value.M42);
                    float num10 = (((matrix.M21 * this.value.M13) + (matrix.M22 * this.value.M23)) + (matrix.M23 * this.value.M33)) + (matrix.M24 * this.value.M43);
                    float num9 = (((matrix.M21 * this.value.M14) + (matrix.M22 * this.value.M24)) + (matrix.M23 * this.value.M34)) + (matrix.M24 * this.value.M44);
                    float num8 = (((matrix.M31 * this.value.M11) + (matrix.M32 * this.value.M21)) + (matrix.M33 * this.value.M31)) + (matrix.M34 * this.value.M41);
                    float num7 = (((matrix.M31 * this.value.M12) + (matrix.M32 * this.value.M22)) + (matrix.M33 * this.value.M32)) + (matrix.M34 * this.value.M42);
                    float num6 = (((matrix.M31 * this.value.M13) + (matrix.M32 * this.value.M23)) + (matrix.M33 * this.value.M33)) + (matrix.M34 * this.value.M43);
                    float num5 = (((matrix.M31 * this.value.M14) + (matrix.M32 * this.value.M24)) + (matrix.M33 * this.value.M34)) + (matrix.M34 * this.value.M44);
                    float num4 = (((matrix.M41 * this.value.M11) + (matrix.M42 * this.value.M21)) + (matrix.M43 * this.value.M31)) + (matrix.M44 * this.value.M41);
                    float num3 = (((matrix.M41 * this.value.M12) + (matrix.M42 * this.value.M22)) + (matrix.M43 * this.value.M32)) + (matrix.M44 * this.value.M42);
                    float num2 = (((matrix.M41 * this.value.M13) + (matrix.M42 * this.value.M23)) + (matrix.M43 * this.value.M33)) + (matrix.M44 * this.value.M43);
                    float num = (((matrix.M41 * this.value.M14) + (matrix.M42 * this.value.M24)) + (matrix.M43 * this.value.M34)) + (matrix.M44 * this.value.M44);
                    this.value.M11 = num16;
                    this.value.M12 = num15;
                    this.value.M13 = num14;
                    this.value.M14 = num13;
                    this.value.M21 = num12;
                    this.value.M22 = num11;
                    this.value.M23 = num10;
                    this.value.M24 = num9;
                    this.value.M31 = num8;
                    this.value.M32 = num7;
                    this.value.M33 = num6;
                    this.value.M34 = num5;
                    this.value.M41 = num4;
                    this.value.M42 = num3;
                    this.value.M43 = num2;
                    this.value.M44 = num;
#endif
                }
                index = ++highpoint;
                isIdentity = false;

                WorldView.index++;
                WorldViewProjection.index++;
                WorldProjection.index++;
            }
            top++;

            return UsingBlock;
        }

        /// <summary>
        /// Multiplies the matrix at the top of the rendering world matrix stack with the input matrix
        /// </summary>
        public void Multiply(ref Matrix matrix)
        {
            if (top == 0)
            {
                throw new InvalidOperationException("World matrix at the bottom of the stack must stay an Identity Matrix, Please use Push(ref Matrix)");
            }
            index = ++highpoint;
            isIdentity = false;

            WorldView.index++;
            WorldViewProjection.index++;
            WorldProjection.index++;

#if NO_INLINE
			Matrix.Multiply(ref matrix, ref this.value, out this.value);
#else
            float num16 = (((matrix.M11 * this.value.M11) + (matrix.M12 * this.value.M21)) + (matrix.M13 * this.value.M31)) + (matrix.M14 * this.value.M41);
            float num15 = (((matrix.M11 * this.value.M12) + (matrix.M12 * this.value.M22)) + (matrix.M13 * this.value.M32)) + (matrix.M14 * this.value.M42);
            float num14 = (((matrix.M11 * this.value.M13) + (matrix.M12 * this.value.M23)) + (matrix.M13 * this.value.M33)) + (matrix.M14 * this.value.M43);
            float num13 = (((matrix.M11 * this.value.M14) + (matrix.M12 * this.value.M24)) + (matrix.M13 * this.value.M34)) + (matrix.M14 * this.value.M44);
            float num12 = (((matrix.M21 * this.value.M11) + (matrix.M22 * this.value.M21)) + (matrix.M23 * this.value.M31)) + (matrix.M24 * this.value.M41);
            float num11 = (((matrix.M21 * this.value.M12) + (matrix.M22 * this.value.M22)) + (matrix.M23 * this.value.M32)) + (matrix.M24 * this.value.M42);
            float num10 = (((matrix.M21 * this.value.M13) + (matrix.M22 * this.value.M23)) + (matrix.M23 * this.value.M33)) + (matrix.M24 * this.value.M43);
            float num9 = (((matrix.M21 * this.value.M14) + (matrix.M22 * this.value.M24)) + (matrix.M23 * this.value.M34)) + (matrix.M24 * this.value.M44);
            float num8 = (((matrix.M31 * this.value.M11) + (matrix.M32 * this.value.M21)) + (matrix.M33 * this.value.M31)) + (matrix.M34 * this.value.M41);
            float num7 = (((matrix.M31 * this.value.M12) + (matrix.M32 * this.value.M22)) + (matrix.M33 * this.value.M32)) + (matrix.M34 * this.value.M42);
            float num6 = (((matrix.M31 * this.value.M13) + (matrix.M32 * this.value.M23)) + (matrix.M33 * this.value.M33)) + (matrix.M34 * this.value.M43);
            float num5 = (((matrix.M31 * this.value.M14) + (matrix.M32 * this.value.M24)) + (matrix.M33 * this.value.M34)) + (matrix.M34 * this.value.M44);
            float num4 = (((matrix.M41 * this.value.M11) + (matrix.M42 * this.value.M21)) + (matrix.M43 * this.value.M31)) + (matrix.M44 * this.value.M41);
            float num3 = (((matrix.M41 * this.value.M12) + (matrix.M42 * this.value.M22)) + (matrix.M43 * this.value.M32)) + (matrix.M44 * this.value.M42);
            float num2 = (((matrix.M41 * this.value.M13) + (matrix.M42 * this.value.M23)) + (matrix.M43 * this.value.M33)) + (matrix.M44 * this.value.M43);
            float num = (((matrix.M41 * this.value.M14) + (matrix.M42 * this.value.M24)) + (matrix.M43 * this.value.M34)) + (matrix.M44 * this.value.M44);
            this.value.M11 = num16;
            this.value.M12 = num15;
            this.value.M13 = num14;
            this.value.M14 = num13;
            this.value.M21 = num12;
            this.value.M22 = num11;
            this.value.M23 = num10;
            this.value.M24 = num9;
            this.value.M31 = num8;
            this.value.M32 = num7;
            this.value.M33 = num6;
            this.value.M34 = num5;
            this.value.M41 = num4;
            this.value.M42 = num3;
            this.value.M43 = num2;
            this.value.M44 = num;
#endif
        }

        /// <summary>
        /// Pops the top of the rendering world matrix stack, Restoring the matrix saved with <see cref="Push(ref Matrix)"/>
        /// </summary>
        public void Pop()
        {
            if (checked(--top) != 0)
            {
                if (index != stackIndex[top])
                {
                    value = stack[top];
                    index = stackIndex[top];
                    isIdentity = stackIdentity[top];

                    WorldView.index++;
                    WorldViewProjection.index++;
                    WorldProjection.index++;
                }
            }
            else
            {
                index = 1;
                isIdentity = true;

                this.value.M11 = 1;
                this.value.M12 = 0;
                this.value.M13 = 0;
                this.value.M14 = 0;
                this.value.M21 = 0;
                this.value.M22 = 1;
                this.value.M23 = 0;
                this.value.M24 = 0;
                this.value.M31 = 0;
                this.value.M32 = 0;
                this.value.M33 = 1;
                this.value.M34 = 0;
                this.value.M41 = 0;
                this.value.M42 = 0;
                this.value.M43 = 0;
                this.value.M44 = 1;

                WorldView.index++;
                WorldViewProjection.index++;
                WorldProjection.index++;
            }
        }

        /// <summary>
        /// Gets the transpose of the matrix
        /// </summary>
        public override void GetTransposeMatrix(out Matrix matrix)
        {
            if (this.transposeIndex != this.index)
                UpdateTranspose();
            matrix = transpose;
        }

        /// <summary>
        /// Gets the inverse of the matrix
        /// </summary>
        public override void GetInverseMatrix(out Matrix matrix)
        {
            if (this.inverseIndex != this.index)
                UpdateInverse();
            matrix = inverse;
        }

        #endregion set/get/stack methods
    }

#if !DEBUG_API

    [System.Diagnostics.DebuggerStepThrough]
#endif
    internal sealed class MatrixMultiply : MatrixSource
    {
        private readonly MatrixSource provider, source;
        internal int indexMultiply;

        public MatrixMultiply(MatrixSource provider, MatrixSource source, DrawState state)
            : base(state)
        {
            this.provider = provider;
            this.source = source;
            this.indexMultiply = 1;
        }

        //only call this after comparing the index
        public void UpdateValue()
        {
            if (this.index != this.indexMultiply)
            {
                this.indexMultiply = this.index;

                value.M11 = (((provider.value.M11 * source.value.M11) + (provider.value.M12 * source.value.M21)) + (provider.value.M13 * source.value.M31)) + (provider.value.M14 * source.value.M41);
                value.M12 = (((provider.value.M11 * source.value.M12) + (provider.value.M12 * source.value.M22)) + (provider.value.M13 * source.value.M32)) + (provider.value.M14 * source.value.M42);
                value.M13 = (((provider.value.M11 * source.value.M13) + (provider.value.M12 * source.value.M23)) + (provider.value.M13 * source.value.M33)) + (provider.value.M14 * source.value.M43);
                value.M14 = (((provider.value.M11 * source.value.M14) + (provider.value.M12 * source.value.M24)) + (provider.value.M13 * source.value.M34)) + (provider.value.M14 * source.value.M44);
                value.M21 = (((provider.value.M21 * source.value.M11) + (provider.value.M22 * source.value.M21)) + (provider.value.M23 * source.value.M31)) + (provider.value.M24 * source.value.M41);
                value.M22 = (((provider.value.M21 * source.value.M12) + (provider.value.M22 * source.value.M22)) + (provider.value.M23 * source.value.M32)) + (provider.value.M24 * source.value.M42);
                value.M23 = (((provider.value.M21 * source.value.M13) + (provider.value.M22 * source.value.M23)) + (provider.value.M23 * source.value.M33)) + (provider.value.M24 * source.value.M43);
                value.M24 = (((provider.value.M21 * source.value.M14) + (provider.value.M22 * source.value.M24)) + (provider.value.M23 * source.value.M34)) + (provider.value.M24 * source.value.M44);
                value.M31 = (((provider.value.M31 * source.value.M11) + (provider.value.M32 * source.value.M21)) + (provider.value.M33 * source.value.M31)) + (provider.value.M34 * source.value.M41);
                value.M32 = (((provider.value.M31 * source.value.M12) + (provider.value.M32 * source.value.M22)) + (provider.value.M33 * source.value.M32)) + (provider.value.M34 * source.value.M42);
                value.M33 = (((provider.value.M31 * source.value.M13) + (provider.value.M32 * source.value.M23)) + (provider.value.M33 * source.value.M33)) + (provider.value.M34 * source.value.M43);
                value.M34 = (((provider.value.M31 * source.value.M14) + (provider.value.M32 * source.value.M24)) + (provider.value.M33 * source.value.M34)) + (provider.value.M34 * source.value.M44);
                value.M41 = (((provider.value.M41 * source.value.M11) + (provider.value.M42 * source.value.M21)) + (provider.value.M43 * source.value.M31)) + (provider.value.M44 * source.value.M41);
                value.M42 = (((provider.value.M41 * source.value.M12) + (provider.value.M42 * source.value.M22)) + (provider.value.M43 * source.value.M32)) + (provider.value.M44 * source.value.M42);
                value.M43 = (((provider.value.M41 * source.value.M13) + (provider.value.M42 * source.value.M23)) + (provider.value.M43 * source.value.M33)) + (provider.value.M44 * source.value.M43);
                value.M44 = (((provider.value.M41 * source.value.M14) + (provider.value.M42 * source.value.M24)) + (provider.value.M43 * source.value.M34)) + (provider.value.M44 * source.value.M44);
            }
        }

        internal override void UpdateMultiply()
        {
            this.UpdateValue();
        }

        public override void GetMatrix(out Matrix matrix)
        {
            if (this.indexMultiply != this.index)
                this.UpdateValue();
            matrix = this.value;
        }

        public override void GetTransposeMatrix(out Matrix matrix)
        {
            if (this.indexMultiply != this.index)
                this.UpdateValue();
            if (this.transposeIndex != this.index)
                UpdateTranspose();
            matrix = transpose;
        }

        public override void GetInverseMatrix(out Matrix matrix)
        {
            if (this.indexMultiply != this.index)
                this.UpdateValue();
            if (this.inverseIndex != this.index)
                UpdateInverse();
            matrix = inverse;
        }

        /// <summary>
        /// Cast to a <see cref="Matrix"/> implicitly
        /// </summary>
        public static implicit operator Matrix(MatrixMultiply source)
        {
            if (source.indexMultiply != source.index)
                source.UpdateValue();
            return source.value;
        }
    }

    //collection of matrix providers
    [System.Diagnostics.DebuggerStepThrough]
    internal struct MatrixProviderCollection
    {
        public MatrixProviderCollection(DrawState state)
        {
            this.World = new WorldMatrixStackProvider(DeviceRenderStateStack.StackSize, state);
            this.View = new ViewProvider(state);
            this.Projection = new ProjectionProvider(state);

            this.WorldView = new MatrixMultiply(this.World, this.View, state);
            this.WorldProjection = new MatrixMultiply(this.World, this.Projection, state);
            this.ViewProjection = new MatrixMultiply(this.View, this.Projection, state);
            this.WorldViewProjection = new MatrixMultiply(this.World, this.ViewProjection, state);

            this.World.WorldView = this.WorldView;
            this.World.WorldProjection = this.WorldProjection;
            this.World.WorldViewProjection = this.WorldViewProjection;

            this.View.WorldView = this.WorldView;
            this.View.ViewProjection = this.ViewProjection;
            this.View.WorldViewProjection = this.WorldViewProjection;

            this.Projection.ViewProjection = this.ViewProjection;
            this.Projection.WorldProjection = this.WorldProjection;
            this.Projection.WorldViewProjection = this.WorldViewProjection;
        }

        public readonly WorldMatrixStackProvider World;
        public readonly ViewProvider View;
        public readonly ProjectionProvider Projection;
        public readonly MatrixMultiply WorldView;
        public readonly MatrixMultiply WorldViewProjection;
        public readonly MatrixMultiply WorldProjection;
        public readonly MatrixMultiply ViewProjection;
    }
}