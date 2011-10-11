using System;
using System.Collections.Generic;

//using Xen.Graphics.ShaderSystem;
using Microsoft.Xna.Framework;
using Xen.Camera;
using Xen.Graphics.Stack;

namespace Xen
{
    namespace Graphics.Stack
    {
        /// <summary>
        /// DrawState camera stack, storing the current and previous cameras.
        /// </summary>
#if !DEBUG_API

        [System.Diagnostics.DebuggerStepThrough]
#endif
        public sealed class CameraStack : ICamera
        {
            internal CameraStack(DrawState state)
            {
                this.state = state;
                UsingBlock = new UsingPop(this);
            }

            /// <summary>
            /// Structure used for a using block with a Push method
            /// </summary>
            [System.Diagnostics.DebuggerStepThrough]
            public struct UsingPop : IDisposable
            {
                internal UsingPop(CameraStack stack)
                {
                    this.stack = stack;
                }

                private readonly CameraStack stack;

                /// <summary>Invokes the Pop metohd</summary>
                public void Dispose()
                {
                    stack.Pop();
                }
            }
            internal readonly UsingPop UsingBlock;

            //push operator:
            /// <summary>
            /// Wrapper on Push
            /// </summary>
            public static UsingPop operator +(CameraStack stack, ICamera camera)
            {
                return stack.Push(camera);
            }

            private readonly DrawState state;
            private ICamera camera;
            private readonly Stack<ICamera> cameraStack = new Stack<ICamera>();

            internal int Count
            {
                get { return cameraStack.Count; }
            }

            /// <summary>
            /// Gets the current camera at the top of the camera stack
            /// </summary>
            public ICamera GetCamera()
            {
                return camera;
            }

            /// <summary>
            /// Gets the matrix for this camera's position/rotation
            /// </summary>
            /// <param name="matrix"></param>
            public void GetCameraMatrix(out Matrix matrix)
            {
                camera.GetCameraMatrix(out matrix);
            }

            /// <summary>
            /// Push a new Camera on to the top of the camera stack. All rendering from this call onwards will use the new camera. Restore the previous camera with a call to <see cref="Pop"/>
            /// </summary>
            /// <param name="camera"></param>
            public UsingPop Push(ICamera camera)
            {
                if (camera == null)
                    throw new ArgumentNullException();
                var cam2D = camera as Camera2D;
                if (cam2D != null) cam2D.Begin(state);

                cameraStack.Push(this.camera);

                state.BeginCamera(camera);
                this.camera = camera;

                return UsingBlock;
            }

            /// <summary>
            /// Push a the current camera to the top of the camera stack. Restore the previous camera with a call to <see cref="Pop"/>
            /// </summary>
            public UsingPop Push()
            {
                cameraStack.Push(this.camera);

                return UsingBlock;
            }

            /// <summary>
            /// Sets the Camera on to the top of the camera stack. All rendering from this call onwards will use the new camera. Restore the previous camera with a call to <see cref="Pop"/>
            /// </summary>
            /// <param name="camera"></param>
            public void SetCamera(ICamera camera)
            {
                if (camera == null)
                    throw new ArgumentNullException();
                SetCameraInternal(camera);
            }

            private void SetCameraInternal(ICamera camera)
            {
                var cam2D = camera as Camera2D;
                if (cam2D != null) cam2D.Begin(state);

                state.BeginCamera(camera);
                this.camera = camera;
            }

            /// <summary>
            /// Restores the last Camera stored with a call to <see cref="Push(ICamera)"/>
            /// </summary>
            public void Pop()
            {
                SetCameraInternal(cameraStack.Pop());
            }

            #region ICamera Members

            /// <summary>
            /// <para>Gets the current view matrix</para>
            /// <para>The view matrix is the Inverse of the Camera Matrix (see <see cref="GetCameraMatrix(out Matrix)"/></para>
            /// </summary>
            public void GetViewMatrix(out Matrix matrix)
            {
                camera.GetViewMatrix(out matrix);
            }

            bool ICamera.ReverseBackfaceCulling
            {
                get { return camera.ReverseBackfaceCulling; }
            }

            /// <summary>
            /// Gets the current projection matrix
            /// </summary>
            public bool GetProjectionMatrix(ref Matrix matrix, ref Vector2 drawTargetSize, ref int changeIndex)
            {
                return camera.GetProjectionMatrix(ref matrix, ref drawTargetSize, ref changeIndex);
            }

            /// <summary>
            /// Gets the current projection matrix
            /// </summary>
            public void GetProjectionMatrix(out Matrix matrix, ref Vector2 drawTargetSize)
            {
                camera.GetProjectionMatrix(out matrix, ref drawTargetSize);
            }

            /// <summary>
            /// Gets the current projection matrix
            /// </summary>
            public void GetProjectionMatrix(out Matrix matrix, Vector2 drawTargetSize)
            {
                camera.GetProjectionMatrix(out matrix, drawTargetSize);
            }

            /// <summary>
            /// <para>Gets the current view matrix</para>
            /// <para>The view matrix is the Inverse of the Camera Matrix (see <see cref="GetCameraMatrix(out Matrix)"/></para>
            /// </summary>
            public bool GetViewMatrix(ref Matrix matrix, ref int changeIndex)
            {
                return camera.GetViewMatrix(ref matrix, ref changeIndex);
            }

            /// <summary>
            /// Gets the world matrix representing the current camera position and rotation
            /// </summary>
            public bool GetCameraMatrix(ref Matrix matrix, ref int changeIndex)
            {
                return camera.GetCameraMatrix(ref matrix, ref changeIndex);
            }

            /// <summary>
            /// Gets the camera position in world space
            /// </summary>
            public void GetCameraPosition(out Vector3 viewPoint)
            {
                camera.GetCameraPosition(out viewPoint);
            }

            /// <summary>
            /// Get the direction the camera is looking in
            /// </summary>
            public void GetCameraViewDirection(out Vector3 viewDirection)
            {
                camera.GetCameraViewDirection(out viewDirection);
            }

            /// <summary>
            /// Gets the camera position in world space
            /// </summary>
            public bool GetCameraPosition(ref Vector4 viewPoint, ref int changeIndex)
            {
                return camera.GetCameraPosition(ref viewPoint, ref changeIndex);
            }

            /// <summary>
            /// Gets the world matrix representing the current camera position and rotation
            /// </summary>
            public bool GetCameraViewDirection(ref Vector4 viewDirection, ref int changeIndex)
            {
                return camera.GetCameraViewDirection(ref viewDirection, ref changeIndex);
            }

            void ICamera.GetCameraNearFarClip(out Vector2 nearFarClip)
            {
                camera.GetCameraNearFarClip(out nearFarClip);
            }

            bool ICamera.GetCameraNearFarClip(ref Vector4 nearFarClip, ref int changeIndex)
            {
                return camera.GetCameraNearFarClip(ref nearFarClip, ref changeIndex);
            }

            void ICamera.GetCameraHorizontalVerticalFov(out Vector2 hvFov)
            {
                camera.GetCameraHorizontalVerticalFov(out hvFov);
            }

            bool ICamera.GetCameraHorizontalVerticalFov(ref Vector4 hvFov, ref int changeIndex)
            {
                return camera.GetCameraHorizontalVerticalFov(ref hvFov, ref changeIndex);
            }

            void ICamera.GetCameraHorizontalVerticalFovTangent(out Vector2 hvFovTan)
            {
                camera.GetCameraHorizontalVerticalFovTangent(out hvFovTan);
            }

            bool ICamera.GetCameraHorizontalVerticalFovTangent(ref Vector4 hvFovTan, ref int changeIndex)
            {
                return camera.GetCameraHorizontalVerticalFovTangent(ref hvFovTan, ref changeIndex);
            }

            Plane[] ICamera.GetCullingPlanes()
            {
                return camera.GetCullingPlanes();
            }

            Plane[] ICamera.GetCullingPlanes(out int farPlaneIndex)
            {
                return camera.GetCullingPlanes(out farPlaneIndex);
            }

            bool ICamera.ProjectToTarget(ref Vector3 position, out Vector2 coordinate, DrawTarget target)
            {
                return camera.ProjectToTarget(ref position, out coordinate, target);
            }

            void ICamera.ProjectFromTarget(ref Vector2 coordinate, float projectDepth, out Vector3 position, DrawTarget target)
            {
                camera.ProjectFromTarget(ref coordinate, projectDepth, out position, target);
            }

            bool ICamera.ProjectToCoordinate(ref Vector3 position, out Vector2 coordinate)
            {
                return camera.ProjectToCoordinate(ref position, out coordinate);
            }

            void ICamera.ProjectFromCoordinate(ref Vector2 coordinate, float projectDepth, out Vector3 position)
            {
                camera.ProjectFromCoordinate(ref coordinate, projectDepth, out position);
            }

            #endregion ICamera Members

            #region ICullPrimitive Members

            bool ICullPrimitive.TestWorldBox(ref Vector3 min, ref Vector3 max, ref Matrix world)
            {
                return camera.TestWorldBox(ref min, ref max, ref world);
            }

            bool ICullPrimitive.TestWorldBox(Vector3 min, Vector3 max, ref Matrix world)
            {
                return camera.TestWorldBox(ref min, ref max, ref world);
            }

            bool ICullPrimitive.TestWorldBox(ref Vector3 min, ref Vector3 max)
            {
                return camera.TestWorldBox(ref min, ref max);
            }

            bool ICullPrimitive.TestWorldBox(Vector3 min, Vector3 max)
            {
                return camera.TestWorldBox(ref min, ref max);
            }

            bool ICullPrimitive.TestWorldSphere(float radius, ref Vector3 position)
            {
                return camera.TestWorldSphere(radius, ref position);
            }

            bool ICullPrimitive.TestWorldSphere(float radius, Vector3 position)
            {
                return camera.TestWorldSphere(radius, ref position);
            }

            ContainmentType ICullPrimitive.IntersectWorldBox(ref Vector3 min, ref Vector3 max, ref Matrix world)
            {
                return camera.IntersectWorldBox(ref min, ref max, ref world);
            }

            ContainmentType ICullPrimitive.IntersectWorldBox(Vector3 min, Vector3 max, ref Matrix world)
            {
                return camera.IntersectWorldBox(ref min, ref max, ref world);
            }

            ContainmentType ICullPrimitive.IntersectWorldBox(ref Vector3 min, ref Vector3 max)
            {
                return camera.IntersectWorldBox(ref min, ref max);
            }

            ContainmentType ICullPrimitive.IntersectWorldBox(Vector3 min, Vector3 max)
            {
                return camera.IntersectWorldBox(ref min, ref max);
            }

            ContainmentType ICullPrimitive.IntersectWorldSphere(float radius, ref Vector3 position)
            {
                return camera.IntersectWorldSphere(radius, ref position);
            }

            ContainmentType ICullPrimitive.IntersectWorldSphere(float radius, Vector3 position)
            {
                return camera.IntersectWorldSphere(radius, ref position);
            }

            #endregion ICullPrimitive Members

            #region Project / UnProject

            //this method should really be named ProjectToTarget

            /// <summary>
            /// <para>Projects a position in 3D object space into pixel coordinates of the screen (or current DrawTarget)</para>
            /// <para>Returns false if the projected point is behind the camera</para>
            /// </summary>
            /// <param name="position">3D position in object space to project into screen coordinates</param>
            /// <param name="screenCoordinate">screen coordinates of the projected position</param>
            /// <returns>True if the projected position is in front of the camera</returns>
            public bool ProjectToTarget(ref Vector3 position, out Vector2 screenCoordinate)
            {
                Vector3 pos;
                Vector3.Transform(ref position, ref state.matrices.World.value, out pos);

                Vector4 worldPositionW = new Vector4(pos, 1.0f);

                Vector2 drawTargetSize = state.DrawTarget.Size;

                Vector4.Transform(ref worldPositionW, ref state.matrices.ViewProjection.value, out worldPositionW);

                if (worldPositionW.W != 0)
                    worldPositionW.W = 1.0f / worldPositionW.W;

                screenCoordinate = new Vector2(worldPositionW.X * worldPositionW.W, worldPositionW.Y * worldPositionW.W);

                screenCoordinate.X = drawTargetSize.X * (screenCoordinate.X * 0.5f + 0.5f);
                screenCoordinate.Y = drawTargetSize.Y * (screenCoordinate.Y * 0.5f + 0.5f);

                return worldPositionW.Z > 0;
            }

            /// <summary>
            /// <para>Projects a world position in 3D space into pixel coordinates of the screen (or current DrawTarget)</para>
            /// <para>Returns false if the projected point is behind the camera</para>
            /// </summary>
            /// <param name="worldPosition">3D position in world space to project into screen coordinates</param>
            /// <param name="screenCoordinate">screen coordinates of the projected position</param>
            /// <returns>True if the projected position is in front of the camera</returns>
            public bool ProjectWorldToTarget(ref Vector3 worldPosition, out Vector2 screenCoordinate)
            {
                Vector3 pos = worldPosition;
                Vector4 worldPositionW = new Vector4(pos, 1.0f);
                MatrixMultiply target = state.matrices.ViewProjection;

                target.UpdateValue();

                Vector2 drawTargetSize = state.DrawTarget.Size;

                Vector4.Transform(ref worldPositionW, ref target.value, out worldPositionW);

                if (worldPositionW.W != 0)
                    worldPositionW.W = 1.0f / worldPositionW.W;

                screenCoordinate = new Vector2(worldPositionW.X * worldPositionW.W, worldPositionW.Y * worldPositionW.W);

                screenCoordinate.X = drawTargetSize.X * (screenCoordinate.X * 0.5f + 0.5f);
                screenCoordinate.Y = drawTargetSize.Y * (screenCoordinate.Y * 0.5f + 0.5f);

                return worldPositionW.Z > 0;
            }

            //this method should really be named ProjectFromTarget

            /// <summary>
            /// Projects a position in coordinates of the screen (or current DrawTarget) into a 3D position in world space
            /// </summary>
            /// <param name="screenPosition">Position in screen space to project into world space</param>
            /// <param name="projectDepth">Depth to project from the camera position</param>
            /// <param name="worldPosition">projected world position</param>
            public void ProjectFromTarget(ref Vector2 screenPosition, float projectDepth, out Vector3 worldPosition)
            {
                //update view*projection matrix inverse storage
                MatrixMultiply target = state.matrices.ViewProjection;
                if (target.indexMultiply != target.index)
                    target.UpdateValue();
                if (target.inverseIndex != target.index)
                    target.UpdateInverse();

                Vector2 drawTargetSize = state.DrawTarget.Size;

                Vector4 coordinate = new Vector4(0, 0, 0.5f, 1);
                if (drawTargetSize.X != 0)
                    coordinate.X = ((screenPosition.X / drawTargetSize.X) - 0.5f) * 2;
                if (drawTargetSize.Y != 0)
                    coordinate.Y = ((screenPosition.Y / drawTargetSize.Y) - 0.5f) * 2;

                Vector4.Transform(ref coordinate, ref target.inverse, out coordinate);

                if (coordinate.W != 0)
                {
                    coordinate.W = 1.0f / coordinate.W;
                    coordinate.X *= coordinate.W;
                    coordinate.Y *= coordinate.W;
                    coordinate.Z *= coordinate.W;
                    coordinate.W = 1;
                }

                //this could probably be done better...
                Vector3 cameraPos;
                camera.GetCameraPosition(out cameraPos);

                Vector3 difference = new Vector3();
                difference.X = coordinate.X - cameraPos.X;
                difference.Y = coordinate.Y - cameraPos.Y;
                difference.Z = coordinate.Z - cameraPos.Z;

                if (difference.X != 0 || difference.Y != 0 || difference.Y != 0)
                    difference.Normalize();

                difference.X *= projectDepth;
                difference.Y *= projectDepth;
                difference.Z *= projectDepth;

                worldPosition = new Vector3();
                worldPosition.X = difference.X + cameraPos.X;
                worldPosition.Y = difference.Y + cameraPos.Y;
                worldPosition.Z = difference.Z + cameraPos.Z;
            }

            #endregion Project / UnProject
        }
    }

    sealed partial class DrawState
    {
        /// <summary>
        /// Cast from the shader system to draw state
        /// </summary>
        //public static implicit operator DrawState(ShaderSystemBase ss)
        //{
        //    return (ss as Xen.Graphics.ShaderSystemState).DrawState;
        //}
        ///// <summary>
        ///// Cast from the draw state to the shader system
        ///// </summary>
        //public static implicit operator ShaderSystemBase(DrawState state)
        //{
        //    return state.shaderSystem;
        //}

        /// <summary>
        /// Gets the unique ID index for a non-global shader attribute. For use in a call to IShader.SetAttribute, IShader.SetTexture or IShader.SetSamplerState"/>
        /// </summary>
        /// <param name="name">case sensitive name of the shader attribute</param>
        /// <returns>globally unique index of the attribute name</returns>
        //public int GetShaderAttributeNameUniqueID(string name)
        //{
        //    return shaderSystem.GetNameUniqueID(name);
        //}

        #region providers

        internal readonly MatrixProviderCollection matrices;
        internal readonly CameraStack cameraStack;
        internal readonly CullersStack cullerStack;
        private ICamera camera;

        #endregion providers

        internal void GetStackHeight(out ushort worldHeight, out ushort stateHeight, out ushort cameraHeight, out ushort preCullerCount, out ushort postCullerCount)
        {
            worldHeight = (ushort)matrices.World.top;
            stateHeight = (ushort)renderState.StackIndex;
            cameraHeight = (ushort)cameraStack.Count;
            postCullerCount = (ushort)this.postCullerCount;
            preCullerCount = (ushort)this.preCullerCount;
        }

        internal void ValidateStackHeight(ushort worldHeight, ushort stateHeight, ushort cameraHeight, ushort preCullerCount, ushort postCullerCount)
        {
            if (matrices.World.top != worldHeight ||
                renderState.StackIndex != stateHeight ||
                cameraStack.Count != cameraHeight ||
                preCullerCount != this.preCullerCount ||
                postCullerCount != this.postCullerCount)
            {
                string str = "World matrix, camera or render state stack corruption detected during method call";
#if !XBOX360
                System.Diagnostics.StackFrame[] stack = new System.Diagnostics.StackTrace(false).GetFrames();
                for (int i = 0; i < stack.Length - 1; i++)
                {
                    if (stack[i].GetMethod() == System.Reflection.MethodInfo.GetCurrentMethod())
                        throw new InvalidOperationException(str + " (" + stack[i + 1].GetMethod().DeclaringType + " :: " + stack[i + 1].GetMethod() + ")");
                }
#endif
                throw new InvalidOperationException(str);
            }
        }

        /// <summary>
        /// Gets the world matrix stack
        /// </summary>
        public WorldMatrixStackProvider WorldMatrix
        {
            get
            {
#if DEBUG
                ValidateRenderState();
#endif
                return matrices.World;
            }
        }

        /// <summary>
        /// Gets the camera stack
        /// </summary>
        public CameraStack Camera
        {
            get
            {
#if DEBUG
                ValidateRenderState();
#endif
                return cameraStack;
            }
        }

        /// <summary>
        /// Gets the cullers stack
        /// </summary>
        public CullersStack Cullers
        {
            get
            {
                return cullerStack;
            }
        }

        /// <summary>
        /// Gets the ICuller interface
        /// </summary>
        public ICuller Culler
        {
            get { return this; }
        }

        internal void BeginCamera(ICamera camera)
        {
#if DEBUG
            ValidateRenderState();
#endif

            SetCameraInUse(this.camera, false);
            SetCameraInUse(camera, true);

            this.camera = camera;

            if (camera != null)
            {
                var targetSize = target.Size;

                matrices.Projection.SetProjectionCamera(camera, ref targetSize);
                matrices.View.SetViewCamera(camera);
                //shaderSystem.Camera = camera;

                this.renderState.ReverseCullBit = camera.ReverseBackfaceCulling ? DeviceRenderStateStack.ReverseCullModeMask : (ushort)0;
            }
        }

        private void SetCameraInUse(ICamera camera, bool inUse)
        {
            var cam2D = camera as Camera2D;
            if (cam2D != null) cam2D.InUse = inUse;
            var cam3D = camera as Camera3D;
            if (cam3D != null) cam3D.InUse = inUse;
        }
    }
}