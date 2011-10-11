#region Using Statements

using Microsoft.Xna.Framework;

#endregion Using Statements

namespace Xen.Camera
{
    /// <summary>
    /// Interface to a Camera
    /// </summary>
    public interface ICamera : ICullPrimitive
    {
        /// <summary>
        /// Gets the view matrix for this camera
        /// </summary>
        /// <remarks>The view matrix is the <see cref="Matrix.Invert(Matrix)">inverse</see> of the <see cref="GetCameraMatrix(out Matrix)">camera matrix</see></remarks>
        /// <param name="matrix"></param>
        void GetViewMatrix(out Matrix matrix);

        /// <summary>
        /// Gets the matrix for this camera's position/rotation
        /// </summary>
        /// <remarks>The <see cref="GetViewMatrix(out Matrix)">view matrix</see> is the <see cref="Matrix.Invert(Matrix)">inverse</see> of the camera matrix</remarks>
        /// <param name="matrix"></param>
        void GetCameraMatrix(out Matrix matrix);

        /// <summary>
        /// If true, the renderer will set the <see cref="Xen.Graphics.RasterState.CullMode"/> to the opposite value (unless set to None).
        /// </summary>
        bool ReverseBackfaceCulling { get; }

        /// <summary>
        /// Gets the projection matrix for this camera, with a change index. Returns true if the matrix has changed
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="changeIndex"></param>
        /// <param name="drawTargetSize">Size of the current draw target (some projections may automatically calculate aspect ratio based on the size of the draw target)</param>
        /// <returns>true if the matrix has changed</returns>
        bool GetProjectionMatrix(ref Matrix matrix, ref Vector2 drawTargetSize, ref int changeIndex);

        /// <summary>
        /// Gets the projection matrix for this camera
        /// </summary>
        /// <param name="drawTargetSize">Size of the current draw target (some projections may automatically calculate aspect ratio based on the size of the draw target)</param>
        /// <param name="matrix"></param>
        void GetProjectionMatrix(out Matrix matrix, ref Vector2 drawTargetSize);

        /// <summary>
        /// Gets the projection matrix for this camera
        /// </summary>
        /// <param name="drawTargetSize">Size of the current draw target (some projections may automatically calculate aspect ratio based on the size of the draw target)</param>
        /// <param name="matrix"></param>
        void GetProjectionMatrix(out Matrix matrix, Vector2 drawTargetSize);

        /// <summary>
        /// Gets the view matrix for this camera, with a change index. Returns true if the matrix has changed
        /// </summary>
        /// <remarks>The view matrix is the <see cref="Matrix.Invert(Matrix)">inverse</see> of the <see cref="GetCameraMatrix(out Matrix)">camera matrix</see></remarks>
        /// <param name="matrix"></param>
        /// <param name="changeIndex"></param>
        /// <returns>true if the matrix has changed</returns>
        bool GetViewMatrix(ref Matrix matrix, ref int changeIndex);

        /// <summary>
        /// Gets the matrix for this camera's position/rotation, with a change index. Returns true if the matrix has changed
        /// </summary>
        /// <remarks>The <see cref="GetViewMatrix(out Matrix)">view matrix</see> is the <see cref="Matrix.Invert(Matrix)">inverse</see> of the camera matrix</remarks>
        /// <param name="matrix"></param>
        /// <param name="changeIndex"></param>
        /// <returns>true if the matrix has changed</returns>
        bool GetCameraMatrix(ref Matrix matrix, ref int changeIndex);

        /// <summary>
        /// Gets the current position of the camera
        /// </summary>
        /// <param name="viewPoint"></param>
        void GetCameraPosition(out Vector3 viewPoint);

        /// <summary>
        /// Gets the normalised view direction of the camera
        /// </summary>
        /// <param name="viewDirection"></param>
        void GetCameraViewDirection(out Vector3 viewDirection);

        /// <summary>
        /// Gets the position of the camera as a vector4, returns if it has changed according to the changeIndex
        /// </summary>
        bool GetCameraPosition(ref Vector4 viewPoint, ref int changeIndex);

        /// <summary>
        /// Gets the normalised view direction of the camera as a vector4, returns if it has changed according to the changeIndex
        /// </summary>
        bool GetCameraViewDirection(ref Vector4 viewDirection, ref int changeIndex);

        /// <summary>
        /// Gets the near/far clip plane distances of the camera as a vector
        /// </summary>
        /// <param name="nearFarClip"></param>
        void GetCameraNearFarClip(out Vector2 nearFarClip);

        /// <summary>
        /// Gets the near/far clip plane distances of the camera as a vector4, returns if it has changed according to the changeIndex
        /// </summary>
        bool GetCameraNearFarClip(ref Vector4 nearFarClip, ref int changeIndex);

        /// <summary>
        /// Gets the horizontal/vertical field of view of the camera as a vector
        /// </summary>
        /// <param name="hvFov"></param>
        void GetCameraHorizontalVerticalFov(out Vector2 hvFov);

        /// <summary>
        /// Gets the horizontal/vertical field of view of the camera as a vector4, returning if it has changed according to the changeIndex
        /// </summary>
        bool GetCameraHorizontalVerticalFov(ref Vector4 hvFov, ref int changeIndex);

        /// <summary>
        /// Gets the tangent of the horizontal/vertical field of view of the camera as a vector
        /// </summary>
        void GetCameraHorizontalVerticalFovTangent(out Vector2 hvFovTan);

        /// <summary>
        /// Gets the tangent of the horizontal/vertical field of view of the camera as a vector4, returning if it has changed according to the changeIndex
        /// </summary>
        /// <param name="changeIndex"></param>
        /// <param name="hvFovTan"></param>
        bool GetCameraHorizontalVerticalFovTangent(ref Vector4 hvFovTan, ref int changeIndex);

        /// <summary>
        /// Get the culling planes for this camera
        /// </summary>
        /// <returns></returns>
        Plane[] GetCullingPlanes();

        /// <summary>
        /// Get the culling planes for this camera
        /// </summary>
        Plane[] GetCullingPlanes(out int farPlaneIndex);

        #region Project / UnProject

        /// <summary>
        /// <para>Projects a position in 3D space into draw target pixel coordinates</para>
        /// <para>Returns false if the projected point is behind the camera</para>
        /// </summary>
        /// <param name="position">3D world space position to project into draw target coordinates</param>
        /// <param name="coordinate">Draw target coordinates of the projected position</param>
        /// <returns>True if the projected position is in front of the camera</returns>
        /// <param name="target">Draw Target space to project the point onto</param>
        bool ProjectToTarget(ref Vector3 position, out Vector2 coordinate, Graphics.DrawTarget target);

        /// <summary>
        /// <para>Projects a position in draw target pixel coordinates into a 3D position in world space, based on a projection depth</para>
        /// <para>NOTE: For 3D Cameras this method is expensive! Where possible use <see cref="Xen.Graphics.Stack.CameraStack.ProjectFromTarget"/> in <see cref="DrawState.Camera"/></para>
        /// </summary>
        /// <param name="coordinate">Coordinate in draw target pixels to project into world space</param>
        /// <param name="projectDepth">World space depth to project from the camera position</param>
        /// <param name="position">projected position</param>
        /// <param name="target">Draw Target space to unproject the point from</param>
        void ProjectFromTarget(ref Vector2 coordinate, float projectDepth, out Vector3 position, Graphics.DrawTarget target);

        /// <summary>
        /// <para>Projects a position in 3D space into [-1,+1] projected coordinates</para>
        /// <para>[-1,+1] Coordinates are equivalent of the size of a DrawTarget, where (-1,-1) is bottom left, (1,1) is top right and (0,0) is centered</para>
        /// <para>Returns false if the projected point is behind the camera</para>
        /// </summary>
        /// <param name="position">3D world space position to project into [-1,+1] projected coordinates</param>
        /// <param name="coordinate">[-1,+1] coordinates of the projected position</param>
        /// <returns>True if the projected position is in front of the camera</returns>
        bool ProjectToCoordinate(ref Vector3 position, out Vector2 coordinate);

        /// <summary>
        /// <para>Projects a position in [-1,+1] coordinates into a 3D position in world space, based on a projection depth</para>
        /// <para>[-1,+1] Coordinates are equivalent of the size of a DrawTarget, where (-1,-1) is bottom left, (1,1) is top right and (0,0) is centered</para>
        /// <para>NOTE: For 3D Cameras this method is expensive! Where possible use <see cref="Xen.Graphics.Stack.CameraStack.ProjectFromTarget"/> in <see cref="DrawState.Camera"/></para>
        /// </summary>
        /// <param name="coordinate">Coordinate in [-1,+1] to project into world space</param>
        /// <param name="projectDepth">World space depth to project from the camera position</param>
        /// <param name="position">unprojected position</param>
        void ProjectFromCoordinate(ref Vector2 coordinate, float projectDepth, out Vector3 position);

        #endregion Project / UnProject
    }
}