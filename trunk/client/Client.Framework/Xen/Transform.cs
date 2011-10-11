using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using System.IO;

namespace Xen
{

	/// <summary>
	/// Stores a decomposed matrix transform in 32 bytes, Storing <see cref="Quaternion"/> <see cref="Rotation"/>, <see cref="Vector3"/> <see cref="Translation"/> and float <see cref="Scale"/>
	/// </summary>
	public struct Transform
	{
		readonly static Transform identity = new Transform(Matrix.Identity);
		/// <summary>
		/// Idenity transform
		/// </summary>
		public static Transform Identity
		{
			get { return identity; }
		}

		/// <summary>
		/// Construct a transform
		/// </summary>
		/// <param name="rotation"></param>
		/// <param name="scale"></param>
		/// <param name="translation"></param>
		public Transform(Quaternion rotation, Vector3 translation, float scale)
		{
			this.Rotation = rotation;
			this.Translation = translation;
			this.Scale = scale;
		}

		/// <summary>
		/// Construct this transform from a matrix
		/// </summary>
		/// <param name="matrix"></param>
		/// <param name="performValidityCheck">When true, the matrix will be checked to make sure it will produce a valid transform</param>
		public Transform(ref Matrix matrix, bool performValidityCheck)
		{
			if (performValidityCheck)
			{
				//validate the matrix is not sheered or inverted on a single axis.
				Vector3 x = new Vector3(matrix.M11, matrix.M12, matrix.M13);
				Vector3 y = new Vector3(matrix.M21, matrix.M22, matrix.M23);
				Vector3 z = new Vector3(matrix.M31, matrix.M32, matrix.M33);

				float xl = x.Length();
				float yl = y.Length();
				float zl = z.Length();

				float maxl = Math.Max(xl, Math.Max(yl, zl));
				float minl = Math.Min(xl, Math.Min(yl, zl));

				if ((maxl - minl) > maxl / 10)
					throw new ArgumentException("The input matrix is not uniformly scaled");

				if (xl > 0.000001f) x /= xl;
				if (yl > 0.000001f) y /= yl;
				if (zl > 0.000001f) z /= zl;

				Vector3 zc = Vector3.Cross(x, y);
				Vector3 yc = Vector3.Cross(z, x);
				Vector3 xc = Vector3.Cross(y, zc);

				if (Vector3.Dot(x, xc) < 0.975f || Vector3.Dot(z, zc) * Vector3.Dot(y, yc) < 0.95f)
					throw new ArgumentException("The input matrix is skewed, sheered or non uniformly scaled");
			}

			Vector3 scale;
			matrix.Decompose(out scale, out Rotation, out Translation);

			//if one or two components are negative, then the Decompose messed up.
			if (scale.X * scale.Y * scale.Z < 0)
			{
				Matrix copy = matrix;
				copy.M11 = -copy.M11;
				copy.M12 = -copy.M12;
				copy.M13 = -copy.M13;
				copy.M21 = -copy.M21;
				copy.M22 = -copy.M22;
				copy.M23 = -copy.M23;
				copy.M31 = -copy.M31;
				copy.M32 = -copy.M32;
				copy.M33 = -copy.M33;
				copy.Decompose(out scale, out Rotation, out Translation);
				scale = -scale;
			}

			this.Scale = Math.Min(Math.Min(scale.X, scale.Y), scale.Z);

			if (Scale > 0.999f && Scale < 1.001f)
				Scale = 1;

			this.Rotation.Normalize();
		}
		/// <summary>
		/// Construct this transform from a matrix
		/// </summary>
		/// <param name="matrix"></param>
		/// <param name="performValidityCheck">When true, the matrix will be checked to make sure it will produce a valid transform</param>
		public Transform(Matrix matrix, bool performValidityCheck)
			: this(ref matrix, performValidityCheck)
		{
		}
#if DEBUG
		/// <summary>Construct this transform from a matrix. Debug builds will validate the matrix</summary>
		public Transform(Matrix matrix) : this(ref matrix, true) { }
		/// <summary>Construct this transform from a matrix. Debug builds will validate the matrix</summary>
		public Transform(ref Matrix matrix) : this(ref matrix, true) { }
#else
		/// <summary>Construct this transform from a matrix. Debug builds will validate the matrix</summary>
		public Transform(Matrix matrix) : this(ref matrix, false) { }
		/// <summary>Construct this transform from a matrix. Debug builds will validate the matrix</summary>
		public Transform(ref Matrix matrix) : this(ref matrix, false) { }
#endif

		/// <summary>
		/// Rotation <see cref="Quaternion"/>
		/// </summary>
		public Quaternion Rotation;
		/// <summary>
		/// Translation vector (position)
		/// </summary>
		public Vector3 Translation;
		/// <summary>
		/// Scale value (usually should be 1)
		/// </summary>
		public float Scale;

		/// <summary>
		/// Makes sure nothing is NaN
		/// </summary>
		public void Validate()
		{
			if (float.IsNaN(Rotation.X) ||
				float.IsNaN(Rotation.Y) ||
				float.IsNaN(Rotation.Z) ||
				float.IsNaN(Rotation.W) ||

				float.IsNaN(Translation.X) ||
				float.IsNaN(Translation.Y) ||
				float.IsNaN(Translation.Z) ||

				float.IsNaN(Scale) ||

				float.IsInfinity(Rotation.X) ||
				float.IsInfinity(Rotation.Y) ||
				float.IsInfinity(Rotation.Z) ||
				float.IsInfinity(Rotation.W) ||

				float.IsInfinity(Translation.X) ||
				float.IsInfinity(Translation.Y) ||
				float.IsInfinity(Translation.Z) ||

				float.IsInfinity(Scale))
			{
				throw new ArgumentException();
			}
		}

		/// <summary>
		/// Constructs a <see cref="Matrix"/> from this transform
		/// </summary>
		/// <param name="mat"></param>
		public void GetMatrix(out Matrix mat)
		{
			Quaternion rot = Rotation;
			rot.Normalize();
			Matrix.CreateFromQuaternion(ref rot, out mat);

			mat.M41 = Translation.X;
			mat.M42 = Translation.Y;
			mat.M43 = Translation.Z;

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
		}

		/// <summary>
		/// Interpolate between two transforms
		/// </summary>
		/// <param name="from"></param>
		/// <param name="to"></param>
		/// <param name="amount"></param>
		/// <param name="result"></param>
		public static void Interpolate(ref Transform from, ref Transform to, float amount, out Transform result)
		{
			result = new Transform();
			Quaternion.Lerp(ref from.Rotation, ref to.Rotation, amount, out result.Rotation);
			Vector3.Lerp(ref from.Translation, ref to.Translation, amount, out result.Translation);
			result.Scale = from.Scale + ((to.Scale - from.Scale) * amount);
		}
		/// <summary>
		/// Interpolate to another transform
		/// </summary>
		/// <param name="to"></param>
		/// <param name="amount"></param>
		/// <returns></returns>
		public Transform Interpolate(ref Transform to, float amount)
		{
			Transform result;
			Interpolate(ref this, ref to, amount, out result);
			return result;
		}
		/// <summary>
		/// Interpolate to another transform
		/// </summary>
		/// <param name="to"></param>
		/// <param name="amount"></param>
		/// <returns></returns>
		public Transform Interpolate(Transform to, float amount)
		{
			Transform result;
			Interpolate(ref this, ref to, amount, out result);
			return result;
		}

		/// <summary>
		/// Construct the transform from a binary source
		/// </summary>
		/// <param name="reader"></param>
		public Transform(BinaryReader reader)
		{
#if XBOX360
			this = new Transform();
#endif
			Rotation.X = reader.ReadSingle();
			Rotation.Y = reader.ReadSingle();
			Rotation.Z = reader.ReadSingle();
			Rotation.W = reader.ReadSingle();
			Translation.X = reader.ReadSingle();
			Translation.Y = reader.ReadSingle();
			Translation.Z = reader.ReadSingle();
			Scale = reader.ReadSingle();
		}

		/// <summary>
		/// Multiply operator
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static Transform operator *(Transform a, Transform b)
		{
			Transform t;
			Multiply(ref a, ref b, out t);
			return t;
		}

		/// <summary>
		/// Multiply two transforms
		/// </summary>
		/// <param name="transform1"></param>
		/// <param name="transform2"></param>
		/// <param name="result"></param>
		public static void Multiply(ref Transform transform1, ref Transform transform2, out Transform result)
		{
			Quaternion q;
			Vector3 t;
			float s = transform2.Scale * transform1.Scale;
			transform2.Rotation.Normalize();
			if (transform2.Rotation.W == 1 &&
				(transform2.Rotation.X == 0 && transform2.Rotation.Y == 0 && transform2.Rotation.Z == 0))
			{
				q.X = transform1.Rotation.X;
				q.Y = transform1.Rotation.Y;
				q.Z = transform1.Rotation.Z;
				q.W = transform1.Rotation.W;
				t.X = transform1.Translation.X;
				t.Y = transform1.Translation.Y;
				t.Z = transform1.Translation.Z;
			}
			else
			{
				float num12 = transform2.Rotation.X + transform2.Rotation.X;
				float num2 = transform2.Rotation.Y + transform2.Rotation.Y;
				float num = transform2.Rotation.Z + transform2.Rotation.Z;
				float num11 = transform2.Rotation.W * num12;
				float num10 = transform2.Rotation.W * num2;
				float num9 = transform2.Rotation.W * num;
				float num8 = transform2.Rotation.X * num12;
				float num7 = transform2.Rotation.X * num2;
				float num6 = transform2.Rotation.X * num;
				float num5 = transform2.Rotation.Y * num2;
				float num4 = transform2.Rotation.Y * num;
				float num3 = transform2.Rotation.Z * num;
				t.X = ((transform1.Translation.X * ((1f - num5) - num3)) + (transform1.Translation.Y * (num7 - num9))) + (transform1.Translation.Z * (num6 + num10));
				t.Y = ((transform1.Translation.X * (num7 + num9)) + (transform1.Translation.Y * ((1f - num8) - num3))) + (transform1.Translation.Z * (num4 - num11));
				t.Z = ((transform1.Translation.X * (num6 - num10)) + (transform1.Translation.Y * (num4 + num11))) + (transform1.Translation.Z * ((1f - num8) - num5));

				num12 = (transform2.Rotation.Y * transform1.Rotation.Z) - (transform2.Rotation.Z * transform1.Rotation.Y);
				num11 = (transform2.Rotation.Z * transform1.Rotation.X) - (transform2.Rotation.X * transform1.Rotation.Z);
				num10 = (transform2.Rotation.X * transform1.Rotation.Y) - (transform2.Rotation.Y * transform1.Rotation.X);
				num9 = ((transform2.Rotation.X * transform1.Rotation.X) + (transform2.Rotation.Y * transform1.Rotation.Y)) + (transform2.Rotation.Z * transform1.Rotation.Z);
				q.X = ((transform2.Rotation.X * transform1.Rotation.W) + (transform1.Rotation.X * transform2.Rotation.W)) + num12;
				q.Y = ((transform2.Rotation.Y * transform1.Rotation.W) + (transform1.Rotation.Y * transform2.Rotation.W)) + num11;
				q.Z = ((transform2.Rotation.Z * transform1.Rotation.W) + (transform1.Rotation.Z * transform2.Rotation.W)) + num10;
				q.W = (transform2.Rotation.W * transform1.Rotation.W) - num9;
			}

			t.X = t.X * transform2.Scale + transform2.Translation.X;
			t.Y = t.Y * transform2.Scale + transform2.Translation.Y;
			t.Z = t.Z * transform2.Scale + transform2.Translation.Z;

#if XBOX360
			result = new Transform();
#endif

			result.Rotation.X = q.X;
			result.Rotation.Y = q.Y;
			result.Rotation.Z = q.Z;
			result.Rotation.W = q.W;

			result.Translation.X = t.X;
			result.Translation.Y = t.Y;
			result.Translation.Z = t.Z;
			result.Scale = s;
		}

		/// <summary>
		/// Write the transform as bytes (32 bytes)
		/// </summary>
		/// <param name="writer"></param>
		public void Write(BinaryWriter writer)
		{
			writer.Write(this.Rotation.X);
			writer.Write(this.Rotation.Y);
			writer.Write(this.Rotation.Z);
			writer.Write(this.Rotation.W);

			writer.Write(this.Translation.X);
			writer.Write(this.Translation.Y);
			writer.Write(this.Translation.Z);
			writer.Write(this.Scale);
		}

		/// <summary>Interpolate this transform towards the identity transform</summary>
		public void InterpolateToIdentity(float weighting)
		{
			this.Translation.X *= weighting;
			this.Translation.Y *= weighting;
			this.Translation.Z *= weighting;
			this.Scale = this.Scale * weighting + (1 - weighting);

			if (Rotation.W >= 0)
			{
				Rotation.X = (weighting * Rotation.X);
				Rotation.Y = (weighting * Rotation.Y);
				Rotation.Z = (weighting * Rotation.Z);
				Rotation.W = (weighting * Rotation.W) + (1 - weighting);
			}
			else
			{
				Rotation.X = (weighting * Rotation.X);
				Rotation.Y = (weighting * Rotation.Y);
				Rotation.Z = (weighting * Rotation.Z);
				Rotation.W = (weighting * Rotation.W) - (1 - weighting);
			}
			Rotation.Normalize();
		}


		/// <summary>
		/// Convert a Transform array into matrix 4x3 format, stored as a Vector4 array
		/// </summary>
		public static Vector4[] GetTransformArrayAsMatrix4x3(Transform[] source, Vector4[] transforms)
		{
			if (source == null)
				throw new ArgumentNullException();

			for (int i = 0, j = 0; i < source.Length; i++)
			{
				float xx = source[i].Rotation.X * source[i].Rotation.X;
				float yy = source[i].Rotation.Y * source[i].Rotation.Y;
				float zz = source[i].Rotation.Z * source[i].Rotation.Z;
				float xy = source[i].Rotation.X * source[i].Rotation.Y;
				float zw = source[i].Rotation.Z * source[i].Rotation.W;
				float zx = source[i].Rotation.Z * source[i].Rotation.X;
				float yw = source[i].Rotation.Y * source[i].Rotation.W;
				float yz = source[i].Rotation.Y * source[i].Rotation.Z;
				float xw = source[i].Rotation.X * source[i].Rotation.W;

				if (source[i].Scale == 1)
				{
					transforms[j].X = 1f - (2f * (yy + zz));//x
					transforms[j].Y = 2f * (xy - zw);//y
					transforms[j].Z = 2f * (zx + yw);//z
					transforms[j].W = source[i].Translation.X;
					j++;
					transforms[j].X = 2f * (xy + zw);//x
					transforms[j].Y = 1f - (2f * (zz + xx));//y
					transforms[j].Z = 2f * (yz - xw);//z
					transforms[j].W = source[i].Translation.Y;
					j++;
					transforms[j].X = 2f * (zx - yw);//x
					transforms[j].Y = 2f * (yz + xw);//y
					transforms[j].Z = 1f - (2f * (yy + xx));//z
					transforms[j].W = source[i].Translation.Z;
					j++;
				}
				else
				{
					transforms[j].X = (1f - (2f * (yy + zz))) * source[i].Scale;//x
					transforms[j].Y = (2f * (xy - zw)) * source[i].Scale;//y
					transforms[j].Z = (2f * (zx + yw)) * source[i].Scale;//z
					transforms[j].W = source[i].Translation.X;
					j++;
					transforms[j].X = (2f * (xy + zw)) * source[i].Scale;//x
					transforms[j].Y = (1f - (2f * (zz + xx))) * source[i].Scale;//y
					transforms[j].Z = (2f * (yz - xw)) * source[i].Scale;//z
					transforms[j].W = source[i].Translation.Y;
					j++;
					transforms[j].X = (2f * (zx - yw)) * source[i].Scale;//x
					transforms[j].Y = (2f * (yz + xw)) * source[i].Scale;//y
					transforms[j].Z = (1f - (2f * (yy + xx))) * source[i].Scale;//z
					transforms[j].W = source[i].Translation.Z;
					j++;
				}
			}
			return transforms;
		}
	}
}
