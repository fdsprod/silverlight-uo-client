
#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Xen.Input.State;
using Xen.Input;
#endregion

namespace Xen.Camera
{
	/// <summary>
	/// Represents a 3D camera
	/// </summary>
#if !DEBUG_API
	[System.Diagnostics.DebuggerStepThrough]
#endif
	public class Camera3D : ICamera
	{
		private readonly Projection proj;
		private Matrix camMatrix = Matrix.Identity;
		private Matrix viewMatrix = Matrix.Identity;
		private bool camMatChanged = true;
		private bool viewMatDirty = true;
		private int camMatIndex = 1;
		private bool reverseBFC;
		private bool inUse;

		/// <summary>
		/// Construct a camera with the given projection, located at the given matrix
		/// </summary>
		public Camera3D(Projection projection, Matrix cameraMatrix)
		{
			this.proj = projection;
			this.camMatrix = cameraMatrix;
		}

		/// <summary>
		/// Construct a camera with a fixed projection matrix, located at the given matrix
		/// </summary>
		public Camera3D(Matrix fixedProjection, Matrix cameraMatrix)
		{
			this.proj = new FixedProjection(fixedProjection);
			this.camMatrix = cameraMatrix;
		}

		/// <summary>
		/// Construct a camera with the given projection, using an identity matrix for it's position
		/// </summary>
		public Camera3D(Projection projection)
		{
			this.proj = projection;
			this.camMatrix = Matrix.Identity;
		}

		/// <summary>
		/// Construct a camera with a fixed projection matrix, using an identity matrix for it's position
		/// </summary>
		public Camera3D(Matrix fixedProjectionMatrix)
		{
			this.proj = new FixedProjection(fixedProjectionMatrix);
			this.camMatrix = Matrix.Identity;
		}

		/// <summary>
		/// Construct a camera with a default camera projection, using an identity matrix for it's position
		/// </summary>
		public Camera3D()
		{
			this.proj = new Projection();
			this.camMatrix = Matrix.Identity;
		}

		//used by DrawState
		internal bool InUse 
		{
			set 
			{
				if (this.inUse != value)
				{
					this.inUse = value;
					if (this.proj != null) proj.InUse = value;
				}
			}
		}

		/// <summary>Asserts the camera is not being used by the device</summary>
		protected void AssertInUse()
		{
			if (inUse) throw new InvalidOperationException("Cannot modify a camera that is in use by DrawState");
		}


		/// <summary>
		/// Sets the <see cref="CameraMatrix"/> to a matrix that will make the camera look at a target
		/// </summary>
		/// <param name="cameraPosition"></param>
		/// <param name="lookAtTarget"></param>
		/// <param name="upVector">Vector representing the up direction for the camera. This vector is required to determine how the camera is orientated around the direction it is looking in</param>
		/// <remarks>
		/// <para>Using <see cref="Matrix.CreateLookAt(Vector3,Vector3,Vector3)"/> is not recommended because it creats a View matrix, so it cannot be used for non-camera matrices. The <see cref="CameraMatrix"/> of a camera is the Inverse (<see cref="Matrix.Invert(Matrix)"/>) of the View Matrix (<see cref="ICamera.GetViewMatrix(out Matrix)"/>), so trying to set the camera matrix using Matrix.CreateLookAt will produce highly unexpected results.
		/// </para></remarks>
		public virtual void LookAt(ref Vector3 lookAtTarget, ref Vector3 cameraPosition, ref Vector3 upVector)
		{
			AssertInUse();

			Vector3 dir = cameraPosition - lookAtTarget;
			if (dir.LengthSquared() == 0)
				throw new ArgumentException("target and position are the same");
			dir.Normalize();
			Vector3 xaxis;

			Vector3.Cross(ref upVector, ref dir, out xaxis);
			xaxis.Normalize();

			Vector3.Cross(ref dir, ref xaxis, out upVector);

			this.camMatrix.M11 = xaxis.X;
			this.camMatrix.M12 = xaxis.Y;
			this.camMatrix.M13 = xaxis.Z;
			this.camMatrix.M14 = 0;

			this.camMatrix.M21 = upVector.X;
			this.camMatrix.M22 = upVector.Y;
			this.camMatrix.M23 = upVector.Z;
			this.camMatrix.M24 = 0;

			this.camMatrix.M31 = dir.X;
			this.camMatrix.M32 = dir.Y;
			this.camMatrix.M33 = dir.Z;
			this.camMatrix.M34 = 0;

			this.camMatrix.M41 = cameraPosition.X;
			this.camMatrix.M42 = cameraPosition.Y;
			this.camMatrix.M43 = cameraPosition.Z;
			this.camMatrix.M44 = 1;

			camMatChanged = true;
			viewMatDirty = true;
			camMatIndex = System.Threading.Interlocked.Increment(ref Camera2D.cameraMatrixBaseIndex);
		}

		/// <summary>
		/// Sets the <see cref="CameraMatrix"/> to a matrix that will make the camera look at a target
		/// </summary>
		/// <param name="cameraPosition"></param>
		/// <param name="lookAtTarget"></param>
		/// <param name="upVector">Vector representing the up direction for the camera. This vector is required to determine how the camera is orientated around the direction it is looking in</param>
		public void LookAt(Vector3 lookAtTarget, Vector3 cameraPosition, Vector3 upVector)
		{
			LookAt(ref lookAtTarget, ref cameraPosition, ref upVector);
		}

		void ICamera.GetCameraHorizontalVerticalFov(out Vector2 v)
		{
			v = new Vector2(proj.GetHorizontalFov(), proj.GetVerticalFov());
		}

		bool ICamera.GetCameraHorizontalVerticalFov(ref Vector4 v, ref int changeIndex)
		{
			return proj.GetCameraHorizontalVerticalFov(ref v, ref changeIndex);
		}

		void ICamera.GetCameraHorizontalVerticalFovTangent(out Vector2 v)
		{
			v = new Vector2(proj.GetHorizontalFovTangent(), proj.GetVerticalFovTangent());
		}

		bool ICamera.GetCameraHorizontalVerticalFovTangent(ref Vector4 v, ref int changeIndex)
		{
			return proj.GetCameraHorizontalVerticalFovTangent(ref v, ref changeIndex);
		}

		void ICamera.GetCameraNearFarClip(out Vector2 v)
		{
			v = new Vector2(proj.NearClip, proj.FarClip);
		}
		bool ICamera.GetCameraNearFarClip(ref Vector4 v, ref int changeIndex)
		{
			return proj.GetCameraNearFarClip(ref v, ref changeIndex);
		}


		/// <summary>
		/// If true, the renderer will set the <see cref="Xen.Graphics.RasterState.CullMode"/> to the opposite value (unless set to None).
		/// </summary>
		public bool ReverseBackfaceCulling
		{
			get { return reverseBFC; }
			set { if (reverseBFC != value) { AssertInUse(); reverseBFC = value; } }
		}

		bool ICamera.ReverseBackfaceCulling { get { return reverseBFC ^ this.proj.UseLeftHandedProjection; } }

		/// <summary>
		/// Get the cameras projection
		/// </summary>
		public Projection Projection
		{
			get { return proj; }
		}

		bool ICamera.GetProjectionMatrix(ref Matrix matrix, ref Vector2 drawTargetSize, ref int changeIndex)
		{
			return this.proj.GetProjectionMatrix(ref matrix, ref drawTargetSize, ref changeIndex);
		}

		bool ICamera.GetViewMatrix(ref Matrix matrix, ref int changeIndex)
		{
			if (changeIndex != camMatIndex)
			{
				changeIndex = camMatIndex;
				((ICamera)this).GetViewMatrix(out matrix);
				return true;
			}
			return false;
		}

		bool ICamera.GetCameraMatrix(ref Matrix matrix, ref int changeIndex)
		{
			if (changeIndex != camMatIndex)
			{
				changeIndex = camMatIndex;
				((ICamera)this).GetCameraMatrix(out matrix);
				return true;
			}
			return false;
		}

		/// <summary>
		/// Gets/Sets the camera matrix. The preferred methods to use are <see cref="GetCameraMatrix(out Matrix)"/> and <see cref="SetCameraMatrix(ref Matrix)"/>
		/// </summary>
		public Matrix CameraMatrix
		{
			get { return camMatrix; }
			set { SetCameraMatrix(ref value); }
		}

		/// <summary>
		/// Sets the camera matrix
		/// </summary>
		/// <param name="value"></param>
		public void SetCameraMatrix(ref Matrix value)
		{
			if (AppState.MatrixNotEqual(ref camMatrix, ref value))
			{
				AssertInUse();
				camMatrix = value; 
				camMatChanged = true; 
				viewMatDirty = true;
				camMatIndex = System.Threading.Interlocked.Increment(ref Camera2D.cameraMatrixBaseIndex);
			}
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
		/// Get the cull planes for this camera
		/// </summary>
		/// <returns></returns>
		public Plane[] GetCullingPlanes()
		{
			if (viewMatDirty)
			{
				Matrix.Invert(ref camMatrix, out viewMatrix);
				viewMatDirty = false;
			}
			Plane[] planes = proj.GetFrustumPlanes(ref viewMatrix, camMatChanged);
			camMatChanged = false;
			return planes;
		}

		/// <summary>
		/// Get the cull planes for this camera
		/// </summary>
		public Plane[] GetCullingPlanes(out int farPlaneIndex)
		{
			farPlaneIndex = 0;
			return GetCullingPlanes();
		}

		void ICamera.GetProjectionMatrix(out Matrix matrix, ref Vector2 drawTargetSize)
		{
			this.proj.GetProjectionMatrix(out matrix, ref drawTargetSize);
		}
		void ICamera.GetProjectionMatrix(out Matrix matrix, Vector2 drawTargetSize)
		{
			this.proj.GetProjectionMatrix(out matrix, ref drawTargetSize);
		}

		/// <summary>
		/// Get the matrix for this camera
		/// </summary>
		/// <param name="matrix"></param>
		public void GetCameraMatrix(out Matrix matrix)
		{
			matrix = camMatrix;
		}
		/// <summary>
		/// Get the position of this camera
		/// </summary>
		/// <param name="viewPoint"></param>
		public void GetCameraPosition(out Vector3 viewPoint)
		{
			viewPoint = new Vector3(camMatrix.M41, camMatrix.M42, camMatrix.M43);
		}
		/// <summary>
		/// Get the view direction of this camera
		/// </summary>
		/// <param name="viewDirection"></param>
		public void GetCameraViewDirection(out Vector3 viewDirection)
		{
			viewDirection = new Vector3(-camMatrix.M31, -camMatrix.M32, -camMatrix.M33);
		}


		bool ICamera.GetCameraPosition(ref Vector4 viewPoint, ref int changeIndex)
		{
			viewPoint.X = camMatrix.M41;
			viewPoint.Y = camMatrix.M42;
			viewPoint.Z = camMatrix.M43;
			bool result = changeIndex != camMatIndex;
			changeIndex = camMatIndex;
			return result;
		}

		bool ICamera.GetCameraViewDirection(ref Vector4 viewDirection, ref int changeIndex)
		{
			viewDirection.X = -camMatrix.M31;
			viewDirection.Y = -camMatrix.M32;
			viewDirection.Z = -camMatrix.M33;
			bool result = changeIndex != camMatIndex;
			changeIndex = camMatIndex;
			return result;
		}

		void ICamera.GetViewMatrix(out Matrix matrix)
		{
			if (viewMatDirty)
			{
				Matrix.Invert(ref camMatrix, out viewMatrix);
				viewMatDirty = false;
			}
			matrix = viewMatrix;
		}

		/// <summary>
		/// Gets/Sets the position of this camera
		/// </summary>
		public Vector3 Position
		{
			get { return new Vector3(camMatrix.M41,camMatrix.M42,camMatrix.M43); }
			set 
			{ 
				if (camMatrix.M41 != value.X || camMatrix.M42 != value.Y || camMatrix.M43 != value.Z) 
				{
					AssertInUse();

					camMatrix.M41 = value.X;
					camMatrix.M42 = value.Y;
					camMatrix.M43 = value.Z;
					
					camMatChanged = true; viewMatDirty = true;

					camMatIndex = System.Threading.Interlocked.Increment(ref Camera2D.cameraMatrixBaseIndex);
				} 
			}
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

			bool result = ProjectToCoordinate(this, ref position, out coordinate, ref size);

			coordinate.X = size.X * (coordinate.X * 0.5f + 0.5f);
			coordinate.Y = size.Y * (coordinate.Y * 0.5f + 0.5f);

			return result;
		}

		/// <summary>
		/// <para>Projects a position in draw target pixel coordinates into a 3D position in world space, based on a projection depth</para>
		/// <para>NOTE: For 3D Cameras this method is expensive! Where possible use ProjectFromTarget in <see cref="DrawState.Camera"/></para>
		/// </summary>
		/// <param name="coordinate">Coordinate in draw target pixels to project into world space</param>
		/// <param name="projectDepth">World space depth to project from the camera position</param>
		/// <param name="position">projected position</param>
		/// <param name="target">Draw Target space to unproject the point from</param>
		public void ProjectFromTarget(ref Vector2 coordinate, float projectDepth, out Vector3 position, Graphics.DrawTarget target)
		{
			Vector2 size = target.Size;
			ProjectFromCoordinate(this, false, ref coordinate, projectDepth, out position, ref size);
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
			return ProjectToCoordinate(this, ref position, out coordinate, ref size);
		}

		/// <summary>
		/// <para>Projects a position in [-1,+1] coordinates into a 3D position in world space, based on a projection depth</para>
		/// <para>[-1,+1] Coordinates are equivalent of the size of a DrawTarget, where (-1,-1) is bottom left, (1,1) is top right and (0,0) is centered</para>
		/// <para>NOTE: For 3D Cameras this method is expensive! Where possible use ProjectFromTarget in <see cref="DrawState.Camera"/></para>
		/// </summary>
		/// <param name="coordinate">Coordinate in [-1,+1] to project into world space</param>
		/// <param name="projectDepth">World space depth to project from the camera position</param>
		/// <param name="position">unprojected position</param>
		public void ProjectFromCoordinate(ref Vector2 coordinate, float projectDepth, out Vector3 position)
		{
			Vector2 size = Vector2.One;
			ProjectFromCoordinate(this, false, ref coordinate, projectDepth, out position, ref size);
		}

		internal static bool ProjectToCoordinate(ICamera camera, ref Vector3 position, out Vector2 coordinate, ref Vector2 targetSize)
		{
			Vector4 worldPositionW = new Vector4(position, 1.0f);

			Matrix mat;
			camera.GetViewMatrix(out mat);
			Vector4.Transform(ref worldPositionW, ref mat, out worldPositionW);

			camera.GetProjectionMatrix(out mat, ref targetSize);
			Vector4.Transform(ref worldPositionW, ref mat, out worldPositionW);

			if (worldPositionW.W != 0)
				worldPositionW.W = 1.0f / worldPositionW.W;

			coordinate = new Vector2(worldPositionW.X * worldPositionW.W, worldPositionW.Y * worldPositionW.W);

			return worldPositionW.Z > 0;
		}


		//also used by Camera2D
		internal static void ProjectFromCoordinate(ICamera camera, bool is2D, ref Vector2 screenPosition, float projectDepth, out Vector3 position, ref Vector2 targetSize)
		{
			Vector4 coordinate = new Vector4(0, 0, 0.5f, 1);
			if (targetSize.X != 0)
				coordinate.X = ((screenPosition.X / targetSize.X) - 0.5f) * 2;
			if (targetSize.Y != 0)
				coordinate.Y = ((screenPosition.Y / targetSize.Y) - 0.5f) * 2;

			//this is much slower for 3D Cameras than 2D, due to matrix inversion requirements.
			Matrix mat;

			if (is2D)
			{
				//projection is always identity, so only need the inverse of the view matrix...
				//which is the camera matrix
				camera.GetCameraMatrix(out mat);
			}
			else
			{
				//more complex.
				Matrix pm;
				camera.GetProjectionMatrix(out pm, ref targetSize);
				camera.GetViewMatrix(out mat);

				// OUCH
				Matrix.Multiply(ref mat, ref pm, out mat); 
				Matrix.Invert(ref mat, out mat); 
			}

			Vector4.Transform(ref coordinate, ref mat, out coordinate);

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

			position = new Vector3();
			position.X = difference.X + cameraPos.X;
			position.Y = difference.Y + cameraPos.Y;
			position.Z = difference.Z + cameraPos.Z;
		}

		#endregion
	}
}


