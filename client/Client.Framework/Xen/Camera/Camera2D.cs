#region Using Statements

using System;
using Microsoft.Xna.Framework;

#endregion Using Statements

namespace Xen.Camera
{
    /// <summary>
    /// Simple Camera with no projection. May be normalised with <see cref="Camera2D.UseNormalisedCoordinates"/> for a range of [0,1]
    /// </summary>
#if !DEBUG_API

    [System.Diagnostics.DebuggerStepThrough]
#endif
    public class Camera2D : ICamera
    {
        private BoundingFrustum frustum = new BoundingFrustum(Matrix.Identity);
        private readonly Plane[] frustumPlanes = new Plane[6];
        private Matrix cameraMatrix = Matrix.Identity, viewMatrix = Matrix.Identity;
        private bool normalised = true;
        private bool dirty = true;
        private int rtWidth, rtHeight;
        private float rtWidthf, rtHeightf;
        private Vector2 bottomLeft, topRight;
        private bool frustumDirty = true;
        private bool viewMatrixDirty = true;
        private int cameraMatrixIndex = 1;
        internal static int cameraMatrixBaseIndex = 2;
        private bool reverseBFC;
        private bool inUse;

        /// <summary></summary>
        public Camera2D()
        {
        }

        /// <summary>
        /// </summary>
        /// <param name="useNormalisedCoordinates">see <see cref="UseNormalisedCoordinates"/></param>
        public Camera2D(bool useNormalisedCoordinates)
        {
            this.normalised = useNormalisedCoordinates;
        }

        //used by DrawState
        internal bool InUse { set { this.inUse = value; } }

        /// <summary>Asserts the camera is not being used by the device</summary>
        protected void AssertInUse()
        {
            if (inUse) throw new InvalidOperationException("Cannot modify a camera that is in use by DrawState");
        }

        void ICamera.GetCameraHorizontalVerticalFov(out Vector2 v)
        {
            v = new Vector2(1, 1);
        }

        bool ICamera.GetCameraHorizontalVerticalFov(ref Vector4 v, ref int changeIndex)
        {
            v.X = 1;
            v.Y = 1;
            bool result = changeIndex != -2;
            changeIndex = -2;
            return result;
        }

        void ICamera.GetCameraHorizontalVerticalFovTangent(out Vector2 v)
        {
            v = new Vector2(1, 1);
        }

        bool ICamera.GetCameraHorizontalVerticalFovTangent(ref Vector4 v, ref int changeIndex)
        {
            v.X = 1;
            v.Y = 1;
            bool result = changeIndex != -2;
            changeIndex = -2;
            return result;
        }

        void ICamera.GetCameraNearFarClip(out Vector2 v)
        {
            v = new Vector2(0, 1);
        }

        bool ICamera.GetCameraNearFarClip(ref Vector4 v, ref int changeIndex)
        {
            v.X = 0;
            v.Y = 1;
            bool result = changeIndex != -2;
            changeIndex = -2;
            return result;
        }

        /// <summary>
        /// If true, the renderer will set the <see cref="Xen.Graphics.RasterState.CullMode"/> to the opposite value (unless set to None).
        /// </summary>
        public bool ReverseBackfaceCulling
        {
            get { return reverseBFC; }
            set { if (reverseBFC != value) { AssertInUse(); reverseBFC = value; } }
        }

        bool ICamera.GetProjectionMatrix(ref Matrix matrix, ref Vector2 drawTargetSize, ref int changeIndex)
        {
            if (changeIndex != 1)
            {
                changeIndex = 1;
                matrix = Matrix.Identity;
                return true;
            }
            return false;
        }

        bool ICamera.GetViewMatrix(ref Matrix matrix, ref int changeIndex)
        {
            if (changeIndex != cameraMatrixIndex)
            {
                changeIndex = cameraMatrixIndex;
                ((ICamera)this).GetViewMatrix(out matrix);
                return true;
            }
            return false;
        }

        bool ICamera.GetCameraMatrix(ref Matrix matrix, ref int changeIndex)
        {
            if (changeIndex != cameraMatrixIndex)
            {
                changeIndex = cameraMatrixIndex;
                ((ICamera)this).GetCameraMatrix(out matrix);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Get the matrix for this camera (<see cref="GetCameraMatrix(out Matrix)"/> is the preferred method to use)
        /// </summary>
        public Matrix CameraMatrix
        {
            get
            {
                if (dirty)
                    BuildView();

                return cameraMatrix;
            }
        }

        /// <summary>
        /// Get the culling planes for this camera
        /// </summary>
        /// <returns></returns>
        public Plane[] GetCullingPlanes()
        {
            if (dirty)
                BuildView();

            if (frustumDirty)
            {
                UpdateFrustum();
            }

            return frustumPlanes;
        }

        /// <summary>
        /// Get the culling planes for this camera
        /// </summary>
        public Plane[] GetCullingPlanes(out int farPlaneIndex)
        {
            farPlaneIndex = -1;
            return GetCullingPlanes();
        }

        private void UpdateFrustum()
        {
            if (viewMatrixDirty)
            {
                Matrix.Invert(ref cameraMatrix, out viewMatrix);
                viewMatrixDirty = false;
            }
            frustum.Matrix = viewMatrix;

            frustumPlanes[0] = frustum.Near;
            frustumPlanes[1] = frustum.Far;
            frustumPlanes[2] = frustum.Left;
            frustumPlanes[3] = frustum.Right;
            frustumPlanes[4] = frustum.Bottom;
            frustumPlanes[5] = frustum.Top;

            frustumDirty = false;
        }

        bool ICullPrimitive.TestWorldBox(Vector3 min, Vector3 max, ref Matrix world)
        {
            return FrustumCull.BoxInFrustum(GetCullingPlanes(), ref min, ref max, ref world);
        }

        bool ICullPrimitive.TestWorldBox(ref Vector3 min, ref Vector3 max, ref Matrix world)
        {
            return FrustumCull.BoxInFrustum(GetCullingPlanes(), ref min, ref max, ref world);
        }

        bool ICullPrimitive.TestWorldSphere(float radius, Vector3 position)
        {
            return FrustumCull.SphereInFrustum(GetCullingPlanes(), radius, ref position);
        }

        bool ICullPrimitive.TestWorldSphere(float radius, ref Vector3 position)
        {
            return FrustumCull.SphereInFrustum(GetCullingPlanes(), radius, ref position);
        }

        ContainmentType ICullPrimitive.IntersectWorldBox(Vector3 min, Vector3 max, ref Matrix world)
        {
            return FrustumCull.BoxIntersectsFrustum(GetCullingPlanes(), ref min, ref max, ref world);
        }

        ContainmentType ICullPrimitive.IntersectWorldBox(ref Vector3 min, ref Vector3 max, ref Matrix world)
        {
            return FrustumCull.BoxIntersectsFrustum(GetCullingPlanes(), ref min, ref max, ref world);
        }

        ContainmentType ICullPrimitive.IntersectWorldSphere(float radius, Vector3 position)
        {
            return FrustumCull.SphereIntersectsFrustum(GetCullingPlanes(), radius, ref position);
        }

        ContainmentType ICullPrimitive.IntersectWorldSphere(float radius, ref Vector3 position)
        {
            return FrustumCull.SphereIntersectsFrustum(GetCullingPlanes(), radius, ref position);
        }

        bool ICullPrimitive.TestWorldBox(Vector3 min, Vector3 max)
        {
            return FrustumCull.AABBInFrustum(GetCullingPlanes(), ref min, ref max);
        }

        bool ICullPrimitive.TestWorldBox(ref Vector3 min, ref Vector3 max)
        {
            return FrustumCull.AABBInFrustum(GetCullingPlanes(), ref min, ref max);
        }

        ContainmentType ICullPrimitive.IntersectWorldBox(Vector3 min, Vector3 max)
        {
            return FrustumCull.AABBIntersectsFrustum(GetCullingPlanes(), ref min, ref max);
        }

        ContainmentType ICullPrimitive.IntersectWorldBox(ref Vector3 min, ref Vector3 max)
        {
            return FrustumCull.AABBIntersectsFrustum(GetCullingPlanes(), ref min, ref max);
        }

        /// <summary>
        /// <para>When true, the bottom left corner is 0,0, the top right is 1,1.</para>
        /// <para>When false, the bottom left corner is 0,0 and the top right is the Width/Height of the render target</para>
        /// </summary>
        public bool UseNormalisedCoordinates
        {
            get { return normalised; }
            set { if (normalised != value) { SetDirty(); normalised = value; } }
        }

        /// <summary>
        /// Overridable, get the size of the view window. Call <see cref="SetDirty"/> to have this method recalled
        /// </summary>
        /// <param name="bottomLeft"></param>
        /// <param name="topRight"></param>
        protected virtual void GetView(out Vector2 bottomLeft, out Vector2 topRight)
        {
            bottomLeft = new Vector2();

            if (normalised)
            {
                topRight = new Vector2(1, 1);
            }
            else
            {
                topRight = new Vector2(rtWidthf, rtHeightf);
            }
        }

        /// <summary>
        /// Call this method to dirty the internal state of the camera/view matrices
        /// </summary>
        protected void SetDirty()
        {
            AssertInUse();
            dirty = true;
            cameraMatrixIndex = System.Threading.Interlocked.Increment(ref cameraMatrixBaseIndex);
        }

        void ICamera.GetProjectionMatrix(out Matrix matrix, ref Vector2 drawTargetSize)
        {
            matrix = Matrix.Identity;
        }

        void ICamera.GetProjectionMatrix(out Matrix matrix, Vector2 drawTargetSize)
        {
            matrix = Matrix.Identity;
        }

        /// <summary>
        /// Gets the current camera matrix
        /// </summary>
        /// <param name="matrix"></param>
        public void GetCameraMatrix(out Matrix matrix)
        {
            if (dirty)
                BuildView();

            matrix = cameraMatrix;
        }

        void ICamera.GetViewMatrix(out Matrix matrix)
        {
            if (dirty)
                BuildView();
            if (viewMatrixDirty)
            {
                Matrix.Invert(ref cameraMatrix, out viewMatrix);
                viewMatrixDirty = false;
            }
            matrix = viewMatrix;
        }

        /// <summary>
        /// Get the position of the camera
        /// </summary>
        /// <param name="viewPoint"></param>
        public void GetCameraPosition(out Vector3 viewPoint)
        {
#if XBOX360
			viewPoint = new Vector3();
#endif
            if (dirty)
                BuildView();
            viewPoint.X = cameraMatrix.M41;
            viewPoint.Y = cameraMatrix.M42;
            viewPoint.Z = cameraMatrix.M43;
        }

        /// <summary>
        /// Get the normalised view direction of the camera
        /// </summary>
        /// <param name="viewDirection"></param>
        public void GetCameraViewDirection(out Vector3 viewDirection)
        {
#if XBOX360
			viewDirection = new Vector3();
#endif
            if (dirty)
                BuildView();
            viewDirection.X = -cameraMatrix.M31;
            viewDirection.Y = -cameraMatrix.M32;
            viewDirection.Z = -cameraMatrix.M33;
        }

        bool ICamera.GetCameraPosition(ref Vector4 viewPoint, ref int changeIndex)
        {
            if (dirty)
                BuildView();

            viewPoint.X = cameraMatrix.M41;
            viewPoint.Y = cameraMatrix.M42;
            viewPoint.Z = cameraMatrix.M43;

            bool result = changeIndex != this.cameraMatrixIndex;
            changeIndex = cameraMatrixIndex;
            return result;
        }

        bool ICamera.GetCameraViewDirection(ref Vector4 viewDirection, ref int changeIndex)
        {
            if (dirty)
                BuildView();

            viewDirection.X = -cameraMatrix.M31;
            viewDirection.Y = -cameraMatrix.M32;
            viewDirection.Z = -cameraMatrix.M33;
            bool result = changeIndex != this.cameraMatrixIndex;
            changeIndex = cameraMatrixIndex;
            return result;
        }

        private void BuildView()
        {
            Vector2 bl, tr;
            GetView(out bl, out tr);

            if (bottomLeft != bl || topRight != tr)
            {
                Matrix mat = Matrix.Identity;
                Matrix.CreateScale(0.5f, 0.5f, 1, out mat);
                mat.M41 = 0.5f;
                mat.M42 = 0.5f;

                if (tr.X != 1 || tr.Y != 1 ||
                    bl.X != 0 || bl.Y != 0)
                {
                    Matrix mat2;
                    Matrix.CreateScale((tr.X - bl.X), (tr.Y - bl.Y), 1, out mat2);
                    mat2.M41 = bl.X;
                    mat2.M42 = bl.Y;

                    Matrix.Multiply(ref mat, ref mat2, out cameraMatrix);
                }
                else
                {
                    cameraMatrix = mat;
                }

                dirty = false;
                viewMatrixDirty = true;
                frustumDirty = true;

                topRight = tr;
                bottomLeft = bl;
            }
        }

        internal void Begin(DrawState state)
        {
            int w = state.DrawTarget.Width;
            int h = state.DrawTarget.Height;

            dirty |= w != rtWidth;
            dirty |= h != rtHeight;

            rtWidth = w;
            rtHeight = h;
            rtHeightf = (float)h;
            rtWidthf = (float)w;
        }

        #region Project / UnProject

        /// <summary>
        /// <para>Projects a position in 3D space into draw target pixel coordinates</para>
        /// <para>Returns false if the projected point is behind the camera</para>
        /// </summary>
        /// <param name="position">3D world space position to project into draw target coordinates</param>
        /// <param name="coordinate">draw target coordinates of the projected position</param>
        /// <returns>True if the projected position is in front of the camera</returns>
        /// <param name="target">Draw Target space to project the point onto</param>
        public bool ProjectToTarget(ref Vector3 position, out Vector2 coordinate, Graphics.DrawTarget target)
        {
            if (target == null)
                throw new ArgumentNullException();

            Vector2 size = target.Size;

            bool result = Camera3D.ProjectToCoordinate(this, ref position, out coordinate, ref size);

            coordinate.X = size.X * (coordinate.X * 0.5f + 0.5f);
            coordinate.Y = size.Y * (coordinate.Y * 0.5f + 0.5f);

            return result;
        }

        /// <summary>
        /// Projects a position in draw target pixel coordinates into a 3D position in world space, based on a projection depth
        /// </summary>
        /// <param name="coordinate">Coordinate in draw target pixels to project into world space</param>
        /// <param name="projectDepth">World space depth to project from the camera position</param>
        /// <param name="position">projected position</param>
        /// <param name="target">Draw Target space to unproject the point from</param>
        public void ProjectFromTarget(ref Vector2 coordinate, float projectDepth, out Vector3 position, Graphics.DrawTarget target)
        {
            Vector2 size = target.Size;
            Camera3D.ProjectFromCoordinate(this, true, ref coordinate, projectDepth, out position, ref size);
        }

        /// <summary>
        /// <para>Projects a position in 3D space into [-1,+1] projected coordinates</para>
        /// <para>[-1,+1] Coordinates are equivalent of the size of a DrawTarget, where (-1,-1) is bottom left, (1,1) is top right and (0,0) is centered</para>
        /// <para>Returns false if the projected point is behind the camera</para>
        /// </summary>
        /// <param name="position">3D world space position to project into [-1,+1] projected coordinates</param>
        /// <param name="coordinate">[-1,+1] coordinates of the projected position</param>
        /// <returns>True if the projected position is in front of the camera</returns>
        public bool ProjectToCoordinate(ref Vector3 position, out Vector2 coordinate)
        {
            Vector2 size = Vector2.One;
            return Camera3D.ProjectToCoordinate(this, ref position, out coordinate, ref size);
        }

        /// <summary>
        /// <para>Projects a position in [-1,+1] coordinates into a 3D position in world space, based on a projection depth</para>
        /// <para>[-1,+1] Coordinates are equivalent of the size of a DrawTarget, where (-1,-1) is bottom left, (1,1) is top right and (0,0) is centered</para>
        /// </summary>
        /// <param name="coordinate">Coordinate in [-1,+1] to project into world space</param>
        /// <param name="projectDepth">World space depth to project from the camera position</param>
        /// <param name="position">unprojected position</param>
        public void ProjectFromCoordinate(ref Vector2 coordinate, float projectDepth, out Vector3 position)
        {
            Vector2 size = Vector2.One;
            Camera3D.ProjectFromCoordinate(this, true, ref coordinate, projectDepth, out position, ref size);
        }

        #endregion Project / UnProject
    }
}