
#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Xen.Input.State;
using Xen.Input;
#endregion

namespace Xen
{
	/// <summary>
	/// Interface to an object that can cull test and intersect test primitive shapes
	/// </summary>
	public interface ICullPrimitive
	{
		/// <summary>
		/// FrustumCull test a world space box.
		/// </summary>
		/// <param name="min">box minimum point (in local space)</param>
		/// <param name="max">box maximum point (in local space)</param>
		/// <param name="world">Absolute world-space world matrix of the box (current world matrix is ignored)</param>
		/// <returns>True if the test passes (eg, box is on screen, box intersects shape, etc)</returns>
		bool TestWorldBox(ref Vector3 min, ref Vector3 max, ref Matrix world);
		/// <summary>
		/// FrustumCull test a world space box.
		/// </summary>
		/// <param name="min">box minimum point (in local space)</param>
		/// <param name="max">box maximum point (in local space)</param>
		/// <param name="world">Absolute world-space world matrix of the box (current world matrix is ignored)</param>
		/// <returns>True if the test passes (eg, box is on screen, box intersects shape, etc)</returns>
		bool TestWorldBox(Vector3 min, Vector3 max, ref Matrix world);

		/// <summary>
		/// FrustumCull test an axis-aligned bounding box.
		/// </summary>
		/// <param name="min">box minimum point</param>
		/// <param name="max">box maximum point</param>
		/// <returns>True if the test passes (eg, box is on screen, box intersects shape, etc)</returns>
		bool TestWorldBox(ref Vector3 min, ref Vector3 max);
		/// <summary>
		/// FrustumCull test an axis-aligned bounding box.
		/// </summary>
		/// <param name="min">box minimum point</param>
		/// <param name="max">box maximum point</param>
		/// <returns>True if the test passes (eg, box is on screen, box intersects shape, etc)</returns>
		bool TestWorldBox(Vector3 min, Vector3 max);

		/// <summary>
		/// FrustumCull test a sphere.
		/// </summary>
		/// <param name="position">Absolute world-space position of the sphere (current world matrix is ignored)</param>
		/// <param name="radius">Radius of the sphere</param>
		/// <returns>True if the test passes (eg, sphere is on screen, sphere intersects shape, etc)</returns>
		bool TestWorldSphere(float radius, ref Vector3 position);
		/// <summary>
		/// FrustumCull test a sphere.
		/// </summary>
		/// <param name="position">Absolute world-space position of the sphere (current world matrix is ignored)</param>
		/// <param name="radius">Radius of the sphere</param>
		/// <returns>True if the test passes (eg, sphere is on screen, sphere intersects shape, etc)</returns>
		bool TestWorldSphere(float radius, Vector3 position);

		
		/// <summary>
		/// <para>Intersect test a world space box.</para>
		/// <para>Note: Intersection tests may be less efficient than boolean TestWorldBox</para>
		/// </summary>
		/// <param name="min">box minimum point (in local space)</param>
		/// <param name="max">box maximum point (in local space)</param>
		/// <param name="world">Absolute world-space world matrix of the box (current world matrix is ignored)</param>
		/// <returns>Intersetction test result</returns>
		ContainmentType IntersectWorldBox(ref Vector3 min, ref Vector3 max, ref Matrix world);
		/// <summary>
		/// <para>Intersect test a world space box.</para>
		/// <para>Note: Intersection tests may be less efficient than boolean TestWorldBox</para>
		/// </summary>
		/// <param name="min">box minimum point (in local space)</param>
		/// <param name="max">box maximum point (in local space)</param>
		/// <param name="world">Absolute world-space world matrix of the box (current world matrix is ignored)</param>
		/// <returns>Intersetction test result</returns>
		ContainmentType IntersectWorldBox(Vector3 min, Vector3 max, ref Matrix world);

		/// <summary>
		/// <para>Intersect test an axis aligned bounding box.</para>
		/// <para>Note: Intersection tests may be less efficient than boolean TestWorldBox</para>
		/// </summary>
		/// <param name="min">box minimum point</param>
		/// <param name="max">box maximum point</param>
		/// <returns>Intersetction test result</returns>
		ContainmentType IntersectWorldBox(ref Vector3 min, ref Vector3 max);
		/// <summary>
		/// <para>Intersect test an axis aligned bounding box.</para>
		/// <para>Note: Intersection tests may be less efficient than boolean TestWorldBox</para>
		/// </summary>
		/// <param name="min">box minimum point</param>
		/// <param name="max">box maximum point</param>
		/// <returns>Intersetction test result</returns>
		ContainmentType IntersectWorldBox(Vector3 min, Vector3 max);

		/// <summary>
		/// Intersect test a sphere.
		/// </summary>
		/// <param name="position">Absolute world-space position of the sphere (current world matrix is ignored)</param>
		/// <param name="radius">Radius of the sphere</param>
		/// <returns>Intersetction test result</returns>
		ContainmentType IntersectWorldSphere(float radius, ref Vector3 position);
		/// <summary>
		/// Intersect test a sphere.
		/// </summary>
		/// <param name="position">Absolute world-space position of the sphere (current world matrix is ignored)</param>
		/// <param name="radius">Radius of the sphere</param>
		/// <returns>Intersetction test result</returns>
		ContainmentType IntersectWorldSphere(float radius, Vector3 position);
		
	}

	/// <summary>
	/// Extension of the <see cref="ICullPrimitive"/> interface that can cull primitive shapes, but is also aware of the current context, having access to the position of the object in advance (eg the drawstate world matrix)
	/// </summary>
	public interface ICuller : ICullPrimitive
	{
		/// <summary>
		/// FrustumCull test a box. World matrix will be inferred (eg, current <see cref="DrawState"/> <see cref="Xen.Graphics.Stack.WorldMatrixStackProvider.GetMatrix(out Matrix)">world matrix</see>)
		/// </summary>
		/// <param name="min">box minimum point (in local space)</param>
		/// <param name="max">box maximum point (in local space)</param>
		/// <returns>True if the test passes (eg, box is on screen, box intersects shape, etc)</returns>
		bool TestBox(ref Vector3 min, ref Vector3 max);
		/// <summary>
		/// FrustumCull test a box. World matrix will be inferred (eg, current <see cref="DrawState"/> <see cref="Xen.Graphics.Stack.WorldMatrixStackProvider.GetMatrix(out Matrix)">world matrix</see>)
		/// </summary>
		/// <param name="min">box minimum point (in local space)</param>
		/// <param name="max">box maximum point (in local space)</param>
		/// <returns>True if the test passes (eg, box is on screen, box intersects shape, etc)</returns>
		bool TestBox(Vector3 min, Vector3 max);

		/// <summary>
		/// FrustumCull test a box with a local matrix. World matrix will be inferred (eg, current <see cref="DrawState"/> <see cref="Xen.Graphics.Stack.WorldMatrixStackProvider.GetMatrix(out Matrix)">world matrix</see>)
		/// </summary>
		/// <param name="min">box minimum point (in local space)</param>
		/// <param name="max">box maximum point (in local space)</param>
		/// <param name="boxMatrix">Local matrix of the box</param>
		/// <returns>True if the test passes (eg, box is on screen, box intersects shape, etc)</returns>
		bool TestBox(ref Vector3 min, ref Vector3 max, ref Matrix boxMatrix);
		/// <summary>
		/// FrustumCull test a box with a local matrix. World matrix will be inferred (eg, current <see cref="DrawState"/> <see cref="Xen.Graphics.Stack.WorldMatrixStackProvider.GetMatrix(out Matrix)">world matrix</see>)
		/// </summary>
		/// <param name="min">box minimum point (in local space)</param>
		/// <param name="max">box maximum point (in local space)</param>
		/// <param name="boxMatrix">Local matrix of the box</param>
		/// <returns>True if the test passes (eg, box is on screen, box intersects shape, etc)</returns>
		bool TestBox(Vector3 min, Vector3 max, ref Matrix boxMatrix);

		/// <summary>
		/// FrustumCull test a sphere. World position will be inferred (eg, current <see cref="DrawState"/> <see cref="Xen.Graphics.Stack.WorldMatrixStackProvider.GetMatrix(out Matrix)">world matrix</see>)
		/// </summary>
		/// <param name="radius">Radius of the sphere</param>
		/// <returns>True if the test passes (eg, sphere is on screen, sphere intersects shape, etc)</returns>
		bool TestSphere(float radius);
		/// <summary>
		/// FrustumCull test a sphere. World position will be inferred (eg, current <see cref="DrawState"/> <see cref="Xen.Graphics.Stack.WorldMatrixStackProvider.GetMatrix(out Matrix)">world matrix</see>)
		/// </summary>
		/// <param name="radius">Radius of the sphere</param>
		/// <param name="position">Position of the sphere in relation to the world matrix</param>
		/// <returns>True if the test passes (eg, sphere is on screen, sphere intersects shape, etc)</returns>
		bool TestSphere(float radius, ref Vector3 position);
		/// <summary>
		/// FrustumCull test a sphere. World position will be inferred (eg, current <see cref="DrawState"/> <see cref="Xen.Graphics.Stack.WorldMatrixStackProvider.GetMatrix(out Matrix)">world matrix</see>)
		/// </summary>
		/// <param name="radius">Radius of the sphere</param>
		/// <param name="position">Position of the sphere in relation to the world matrix</param>
		/// <returns>True if the test passes (eg, sphere is on screen, sphere intersects shape, etc)</returns>
		bool TestSphere(float radius, Vector3 position);


		/// <summary>
		/// <para>Intersect a box. World matrix will be inferred (eg, current <see cref="DrawState"/> <see cref="Xen.Graphics.Stack.WorldMatrixStackProvider.GetMatrix(out Matrix)">world matrix</see>)</para>
		/// <para>Note: Intersection tests may be less efficient than boolean TestBox</para>
		/// </summary>
		/// <param name="min">box minimum point (in local space)</param>
		/// <param name="max">box maximum point (in local space)</param>
		/// <returns>Intersetction test result</returns>
		ContainmentType IntersectBox(ref Vector3 min, ref Vector3 max);
		/// <summary>
		/// <para>Intersect a box. World matrix will be inferred (eg, current <see cref="DrawState"/> <see cref="Xen.Graphics.Stack.WorldMatrixStackProvider.GetMatrix(out Matrix)">world matrix</see>)</para>
		/// <para>Note: Intersection tests may be less efficient than boolean TestBox</para>
		/// </summary>
		/// <param name="min">box minimum point (in local space)</param>
		/// <param name="max">box maximum point (in local space)</param>
		/// <returns>Intersetction test result</returns>
		ContainmentType IntersectBox(Vector3 min, Vector3 max);

		/// <summary>
		/// <para>Intersect a box with a local matrix. World matrix will be inferred (eg, current <see cref="DrawState"/> <see cref="Xen.Graphics.Stack.WorldMatrixStackProvider.GetMatrix(out Matrix)">world matrix</see>)</para>
		/// <para>Note: Intersection tests may be less efficient than boolean TestBox</para>
		/// </summary>
		/// <param name="min">box minimum point (in local space)</param>
		/// <param name="max">box maximum point (in local space)</param>
		/// <param name="boxMatrix">Local matrix of the box</param>
		/// <returns>Intersetction test result</returns>
		ContainmentType IntersectBox(ref Vector3 min, ref Vector3 max, ref Matrix boxMatrix);
		/// <summary>
		/// <para>Intersect a box with a local matrix. World matrix will be inferred (eg, current <see cref="DrawState"/> <see cref="Xen.Graphics.Stack.WorldMatrixStackProvider.GetMatrix(out Matrix)">world matrix</see>)</para>
		/// <para>Note: Intersection tests may be less efficient than boolean TestBox</para>
		/// </summary>
		/// <param name="min">box minimum point (in local space)</param>
		/// <param name="max">box maximum point (in local space)</param>
		/// <param name="boxMatrix">Local matrix of the box</param>
		/// <returns>Intersetction test result</returns>
		ContainmentType IntersectBox(Vector3 min, Vector3 max, ref Matrix boxMatrix);

		/// <summary>
		/// <para>Intersect a sphere. World position will be inferred (eg, current <see cref="DrawState"/> <see cref="Xen.Graphics.Stack.WorldMatrixStackProvider.GetMatrix(out Matrix)">world matrix</see>)</para>
		/// <para>Note: Intersection tests may be less efficient than boolean TestSphere</para>
		/// </summary>
		/// <param name="radius">Radius of the sphere</param>
		/// <returns>Intersetction test result</returns>
		ContainmentType IntersectSphere(float radius);
		/// <summary>
		/// <para>Intersect a sphere. World position will be inferred (eg, current <see cref="DrawState"/> <see cref="Xen.Graphics.Stack.WorldMatrixStackProvider.GetMatrix(out Matrix)">world matrix</see>)</para>
		/// <para>Note: Intersection tests may be less efficient than boolean TestSphere</para>
		/// </summary>
		/// <param name="radius">Radius of the sphere</param>
		/// <param name="position">Position of the sphere in relation to the world matrix</param>
		/// <returns>Intersetction test result</returns>
		ContainmentType IntersectSphere(float radius, ref Vector3 position);
		/// <summary>
		/// <para>Intersect a sphere. World position will be inferred (eg, current <see cref="DrawState"/> <see cref="Xen.Graphics.Stack.WorldMatrixStackProvider.GetMatrix(out Matrix)">world matrix</see>)</para>
		/// <para>Note: Intersection tests may be less efficient than boolean TestSphere</para>
		/// </summary>
		/// <param name="radius">Radius of the sphere</param>
		/// <param name="position">Position of the sphere in relation to the world matrix</param>
		/// <returns>Intersetction test result</returns>
		ContainmentType IntersectSphere(float radius, Vector3 position);


		/// <summary>
		/// Gets the world matrix for the current context, eg the top of the rendering world matrix stack (as stored in the <see cref="DrawState"/> object)
		/// </summary>
		/// <param name="world"></param>
		void GetWorldMatrix(out Matrix world);
		/// <summary>
		/// Gets the world position for the current context, eg the top of the rendering world matrix stack (as stored in the <see cref="DrawState"/> object)
		/// </summary>
		/// <param name="position"></param>
		void GetWorldPosition(out Vector3 position);
		/// <summary>
		/// Gets the current rendering frame index
		/// </summary>
		/// <remarks><para>This value is useful if an object needs to calculate culling data once per frame, but doesn't want to recalculate it if culled more than once during the frame</para>
		/// <para>Implementations should return the same value that DrawState.Properties.FrameIndex would return for the current frame</para></remarks>
		int FrameIndex { get; }
		/// <summary>
		/// Gets the state of the application
		/// </summary>
		/// <returns></returns>
		/// <remarks><para>This is commonly the <see cref="DrawState"/>, however it is highly recommended that no drawing ever be done in a <see cref="ICullable.CullTest"/> method body</para></remarks>
		IState GetState();
	}

	//runs culling logic (so the logic isn't duplicated amongst implementations)
#if !DEBUG_API
	[System.Diagnostics.DebuggerStepThrough]
#endif
	internal static class FrustumCull
	{
		internal static bool SphereInFrustum(Plane[] frustum, float radius, ref Vector3 position)
		{
			foreach (Plane plane in frustum)
			{
				if (plane.Normal.X * position.X + plane.Normal.Y * position.Y + plane.Normal.Z * position.Z + plane.D > radius)
					return false;
			}
			return true;
		}

		internal static ContainmentType SphereIntersectsFrustum(Plane[] frustum, float radius, ref Vector3 position)
		{
			bool intersects = false;
			foreach (Plane plane in frustum)
			{
				float distance = plane.Normal.X * position.X + plane.Normal.Y * position.Y + plane.Normal.Z * position.Z + plane.D;
				if (distance > radius)
					return ContainmentType.Disjoint;
				if (distance > -radius)
					intersects = true;
			}
			return intersects ? ContainmentType.Intersects : ContainmentType.Contains;
		}

		internal static bool BoxInFrustum(Plane[] frustum, ref Vector3 minExtents, ref Vector3 maxExtents, ref Matrix world)
		{
			float dot;
			Vector3 point;

			foreach (Plane plane in frustum)
			{
				dot = plane.Normal.X * world.M11 +
						plane.Normal.Y * world.M12 +
						plane.Normal.Z * world.M13;

				if (dot >= 0)
				{
					point.X = world.M41 + world.M11 * minExtents.X;
					point.Y = world.M42 + world.M12 * minExtents.X;
					point.Z = world.M43 + world.M13 * minExtents.X;
				}
				else
				{
					point.X = world.M41 + world.M11 * maxExtents.X;
					point.Y = world.M42 + world.M12 * maxExtents.X;
					point.Z = world.M43 + world.M13 * maxExtents.X;
				}


				dot = plane.Normal.X * world.M21 +
						plane.Normal.Y * world.M22 +
						plane.Normal.Z * world.M23;

				if (dot >= 0)
				{
					point.X += world.M21 * minExtents.Y;
					point.Y += world.M22 * minExtents.Y;
					point.Z += world.M23 * minExtents.Y;
				}
				else
				{
					point.X += world.M21 * maxExtents.Y;
					point.Y += world.M22 * maxExtents.Y;
					point.Z += world.M23 * maxExtents.Y;
				}



				dot = plane.Normal.X * world.M31 +
						plane.Normal.Y * world.M32 +
						plane.Normal.Z * world.M33;

				if (dot >= 0)
				{
					point.X += world.M31 * minExtents.Z;
					point.Y += world.M32 * minExtents.Z;
					point.Z += world.M33 * minExtents.Z;
				}
				else
				{
					point.X += world.M31 * maxExtents.Z;
					point.Y += world.M32 * maxExtents.Z;
					point.Z += world.M33 * maxExtents.Z;
				}

				if (plane.Normal.X * point.X + plane.Normal.Y * point.Y + plane.Normal.Z * point.Z + plane.D > 0)
					return false;
			}
			return true;
		}

		internal static ContainmentType BoxIntersectsFrustum(Plane[] frustum, ref Vector3 minExtents, ref Vector3 maxExtents, ref Matrix world)
		{
			float dot;
			Vector3 point, overlapPoint;
			bool overlap = false;

			foreach (Plane plane in frustum)
			{
				dot = plane.Normal.X * world.M11 +
						plane.Normal.Y * world.M12 +
						plane.Normal.Z * world.M13;

				if (dot >= 0)
				{
					point.X = world.M41 + world.M11 * minExtents.X;
					point.Y = world.M42 + world.M12 * minExtents.X;
					point.Z = world.M43 + world.M13 * minExtents.X;

					overlapPoint.X = world.M41 + world.M11 * maxExtents.X;
					overlapPoint.Y = world.M42 + world.M12 * maxExtents.X;
					overlapPoint.Z = world.M43 + world.M13 * maxExtents.X;
				}
				else
				{
					point.X = world.M41 + world.M11 * maxExtents.X;
					point.Y = world.M42 + world.M12 * maxExtents.X;
					point.Z = world.M43 + world.M13 * maxExtents.X;

					overlapPoint.X = world.M41 + world.M11 * minExtents.X;
					overlapPoint.Y = world.M42 + world.M12 * minExtents.X;
					overlapPoint.Z = world.M43 + world.M13 * minExtents.X;
				}


				dot = plane.Normal.X * world.M21 +
						plane.Normal.Y * world.M22 +
						plane.Normal.Z * world.M23;

				if (dot >= 0)
				{
					point.X += world.M21 * minExtents.Y;
					point.Y += world.M22 * minExtents.Y;
					point.Z += world.M23 * minExtents.Y;

					overlapPoint.X += world.M21 * maxExtents.Y;
					overlapPoint.Y += world.M22 * maxExtents.Y;
					overlapPoint.Z += world.M23 * maxExtents.Y;
				}
				else
				{
					point.X += world.M21 * maxExtents.Y;
					point.Y += world.M22 * maxExtents.Y;
					point.Z += world.M23 * maxExtents.Y;

					overlapPoint.X += world.M21 * minExtents.Y;
					overlapPoint.Y += world.M22 * minExtents.Y;
					overlapPoint.Z += world.M23 * minExtents.Y;
				}



				dot = plane.Normal.X * world.M31 +
						plane.Normal.Y * world.M32 +
						plane.Normal.Z * world.M33;

				if (dot >= 0)
				{
					point.X += world.M31 * minExtents.Z;
					point.Y += world.M32 * minExtents.Z;
					point.Z += world.M33 * minExtents.Z;

					overlapPoint.X += world.M31 * maxExtents.Z;
					overlapPoint.Y += world.M32 * maxExtents.Z;
					overlapPoint.Z += world.M33 * maxExtents.Z;
				}
				else
				{
					point.X += world.M31 * maxExtents.Z;
					point.Y += world.M32 * maxExtents.Z;
					point.Z += world.M33 * maxExtents.Z;

					overlapPoint.X += world.M31 * minExtents.Z;
					overlapPoint.Y += world.M32 * minExtents.Z;
					overlapPoint.Z += world.M33 * minExtents.Z;
				}

				if (plane.Normal.X * point.X + plane.Normal.Y * point.Y + plane.Normal.Z * point.Z + plane.D > 0)
					return ContainmentType.Disjoint;

				if (plane.Normal.X * overlapPoint.X + plane.Normal.Y * overlapPoint.Y + plane.Normal.Z * overlapPoint.Z + plane.D > 0)
					overlap = true;
			}
			if (overlap)
				return ContainmentType.Intersects;
			return ContainmentType.Contains;
		}

		internal static bool AABBInFrustum(Plane[] frustum, ref Vector3 minExtents, ref Vector3 maxExtents)
		{
			Vector3 point;

			foreach (Plane plane in frustum)
			{
				if (plane.Normal.X >= 0)
					point.X = minExtents.X;
				else
					point.X = maxExtents.X;

				if (plane.Normal.Y >= 0)
					point.Y = minExtents.Y;
				else
					point.Y = maxExtents.Y;

				if (plane.Normal.Z >= 0)
					point.Z = minExtents.Z;
				else
					point.Z = maxExtents.Z;

				if (plane.Normal.X * point.X + plane.Normal.Y * point.Y + plane.Normal.Z * point.Z + plane.D > 0)
					return false;
			}
			return true;
		}

		internal static ContainmentType AABBIntersectsFrustum(Plane[] frustum, ref Vector3 minExtents, ref Vector3 maxExtents)
		{
			bool overlap = false;

			Vector3 point, overlapPoint;

			foreach (Plane plane in frustum)
			{
				if (plane.Normal.X >= 0)
				{
					point.X = minExtents.X;
					overlapPoint.X = maxExtents.X;
				}
				else
				{
					point.X = maxExtents.X;
					overlapPoint.X = minExtents.X;
				}

				if (plane.Normal.Y >= 0)
				{
					point.Y = minExtents.Y;
					overlapPoint.Y = maxExtents.Y;
				}
				else
				{
					point.Y = maxExtents.Y;
					overlapPoint.Y = minExtents.Y;
				}

				if (plane.Normal.Z >= 0)
				{
					point.Z = minExtents.Z;
					overlapPoint.Z = maxExtents.Z;
				}
				else
				{
					point.Z = maxExtents.Z;
					overlapPoint.Z = minExtents.Z;
				}

				if (plane.Normal.X * point.X + plane.Normal.Y * point.Y + plane.Normal.Z * point.Z + plane.D > 0)
					return ContainmentType.Disjoint;

				if (plane.Normal.X * overlapPoint.X + plane.Normal.Y * overlapPoint.Y + plane.Normal.Z * overlapPoint.Z + plane.D > 0)
					overlap = true;
			}
			if (overlap)
				return ContainmentType.Intersects;
			return ContainmentType.Contains;
		}
	}
}
