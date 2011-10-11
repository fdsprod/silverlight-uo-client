using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Xen.Camera;
using Xen.Graphics.Modifier;
using Microsoft.Xna.Framework;

namespace Xen.Graphics
{
	/// <summary>
	/// Stores a list of animation transforms, stored in float4x3 format
	/// </summary>
	public sealed class AnimationTransformArray
	{
		private readonly Vector4[] matrixData;
		private int changeIndex;
		private bool isIdentity;

		/// <summary>
		/// Construct the hierachy from a bone count
		/// </summary>
		public AnimationTransformArray(int boneCount)
		{
			this.matrixData = new Vector4[boneCount * 3];
		}
		/// <summary>
		/// Construct the hierachy from a set of bone transforms
		/// </summary>
		public AnimationTransformArray(Transform[] transforms)
			: this(transforms.Length)
		{
			UpdateTransformArray(transforms);
		}
		/// <summary>
		/// Construct the hierachy from a set of bone transforms
		/// </summary>
		public AnimationTransformArray(Matrix[] transforms)
			: this(transforms.Length)
		{
			UpdateTransformArray(transforms);
		}

		/// <summary>
		/// Update the transform data
		/// </summary>
		public void UpdateTransformArray(Transform[] transforms)
		{
			if (transforms == null)
				throw new ArgumentNullException();
			if (transforms.Length > matrixData.Length / 3)
				throw new ArgumentException("hierarchy.Length");

			Transform.GetTransformArrayAsMatrix4x3(transforms, matrixData);
			changeIndex++;
			isIdentity = false;
		}

		/// <summary>
		/// Update the transform data, by remapping another transform array
		/// </summary>
		public void UpdateTransformArray(AnimationTransformArray source, uint[] boneRemapping)
		{
			uint write = 0;
			for (int i = 0; i < boneRemapping.Length; i++)
			{
				uint read = boneRemapping[i] * 3;
				this.matrixData[write++] = source.matrixData[read++];
				this.matrixData[write++] = source.matrixData[read++];
				this.matrixData[write++] = source.matrixData[read++];
			}
			changeIndex++;
			isIdentity = false;
		}

		/// <summary>
		/// Gets the matrix for the given bone
		/// </summary>
		public void GetBoneMatrix(int boneIndex, out Matrix matrix)
		{
#if XBOX360
			matrix = new Matrix();
#endif
			boneIndex *= 3;
			Vector4 vec = this.matrixData[boneIndex++];
			matrix.M11 = vec.X;
			matrix.M21 = vec.Y;
			matrix.M31 = vec.Z;
			matrix.M41 = vec.W;
			vec = this.matrixData[boneIndex++];
			matrix.M12 = vec.X;
			matrix.M22 = vec.Y;
			matrix.M32 = vec.Z;
			matrix.M42 = vec.W;
			vec = this.matrixData[boneIndex++];
			matrix.M13 = vec.X;
			matrix.M23 = vec.Y;
			matrix.M33 = vec.Z;
			matrix.M43 = vec.W;

			matrix.M14 = 0;
			matrix.M24 = 0;
			matrix.M34 = 0;
			matrix.M44 = 1;
		}

		/// <summary>
		/// Update the transform data
		/// </summary>
		public void UpdateTransformArray(Matrix[] transforms)
		{
			if (transforms == null)
				throw new ArgumentNullException();
			if (transforms.Length > matrixData.Length / 3)
				throw new ArgumentException("hierarchy.Length");

			for (int i = 0, j = 0; i < transforms.Length; i++)
			{
				Matrix m = transforms[i];
				this.matrixData[j++] = new Vector4(m.M11, m.M21, m.M31, m.M41);
				this.matrixData[j++] = new Vector4(m.M12, m.M22, m.M32, m.M42);
				this.matrixData[j++] = new Vector4(m.M13, m.M23, m.M33, m.M43);
			}
			changeIndex++;
			isIdentity = false;
		}

		/// <summary>
		/// Resets the transform to an identiy state
		/// </summary>
		public void ClearTransformArray()
		{
			isIdentity = true;
		}

		/// <summary>
		/// Returns true if the animation data has a different change index
		/// </summary>
		public bool HasChanged(ref int index)
		{
			bool changed = this.changeIndex != index;
			index = this.changeIndex;
			return changed;
		}

		/// <summary>
		/// Stored matrix data in 4x3 format. Returns null if in a cleared state.
		/// </summary>
		public Vector4[] MatrixData { get { if (isIdentity) return null; return matrixData; } }

		/// <summary>
		/// Returns true when the transform array is in an identity / cleared state
		/// </summary>
		public bool IsIdentiyState { get { return isIdentity; } }
	}
}