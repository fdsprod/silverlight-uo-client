using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using System.Collections;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Xen.Graphics
{
	/// <summary>
	/// Interface to a vertex buffer. Stores raw geometry data for vertices (eg, position, colour, etc)
	/// </summary>
	public interface IVertices : IDisposable
	{
		/// <summary>
		/// Number of vertices
		/// </summary>
		/// <remarks>returns -1 if currently not known. This value will be updated when the buffer is processed</remarks>
		int Count { get;}
		/// <summary>
		/// Byte stride of the vertex data
		/// </summary>
		int Stride { get;}
		/// <summary>
		/// Gets or Sets the resource usage of the buffer. Set to <see cref="ResourceUsage"/>.Dynamic to allow use of <see cref="SetDirtyRange"/>
		/// </summary>
		ResourceUsage ResourceUsage { get; set;}
		/// <summary>
		/// Gets the type of the vertex contents
		/// </summary>
		Type VertexType { get;}

		/// <summary>
		/// Draw the buffer
		/// </summary>
		/// <param name="state"></param>
		/// <param name="indices">Indices to use when drawing (may be null)</param>
		/// <param name="primitiveType">Primitive type to use when drawing the buffer</param>
		void Draw(DrawState state, IIndices indices, PrimitiveType primitiveType);

		/// <summary>
		/// Draw the buffer with extended parametres
		/// </summary>
		/// <param name="state"></param>
		/// <param name="indices">Indices to use when drawing (may be null)</param>
		/// <param name="primitiveType">Primitive type to use when drawing the buffer</param>
		/// <param name="primitveCount">The number of primitives to draw</param>
		/// <param name="startIndex">The start index in the index buffer (defaults to the first index - 0)</param>
		/// <param name="vertexOffset">Starting offset into the vertex buffer (defaults to the first vertex - 0)</param>
		void Draw(DrawState state, IIndices indices, PrimitiveType primitiveType, int primitveCount, int startIndex, int vertexOffset);

		/// <summary>
		/// Draw multiple instances of the vertex buffer
		/// </summary>
		/// <param name="state"></param>
		/// <param name="indices">Indices to use when drawing (may be null)</param>
		/// <param name="primitiveType">Primitive type to use when drawing the buffer</param>
		/// <param name="instances">A buffer providing a list of instance world matrices to draw</param>
		void DrawInstances(DrawState state, IIndices indices, PrimitiveType primitiveType, InstanceBuffer instances);

		/// <summary>
		/// Draw multiple instances of the vertex buffer
		/// </summary>
		/// <param name="state"></param>
		/// <param name="indices">Indices to use when drawing (may be null)</param>
		/// <param name="primitiveType">Primitive type to use when drawing the buffer</param>
		/// <param name="instances">An array providing a list of instance world matrices to draw</param>
		/// <param name="count">The number of instances to draw</param>
		void DrawInstances(DrawState state, IIndices indices, PrimitiveType primitiveType, Matrix[] instances, int count);

		/// <summary>
		/// Draw the buffer using weighted vertex blending (animation)
		/// </summary>
		/// <param name="state"></param>
		/// <param name="indices">Indices to use when drawing (may be null)</param>
		/// <param name="primitiveType">Primitive type to use when drawing the buffer</param>
		/// <param name="animationTransforms">A buffer providing a list of animation transform matrices</param>
		void DrawBlending(DrawState state, IIndices indices, PrimitiveType primitiveType, AnimationTransformArray animationTransforms);

		/// <summary>
		/// Draw the buffer using weighted vertex blending (animation) and extended parametres
		/// </summary>
		/// <param name="state"></param>
		/// <param name="indices">Indices to use when drawing (may be null)</param>
		/// <param name="primitiveType">Primitive type to use when drawing the buffer</param>
		/// <param name="primitveCount">The number of primitives to draw</param>
		/// <param name="startIndex">The start index in the index buffer (defaults to the first index - 0)</param>
		/// <param name="vertexOffset">Starting offset into the vertex buffer (defaults to the first vertex - 0)</param>
		/// <param name="animationTransforms">A buffer providing a list of animation transform matrices</param>
		void DrawBlending(DrawState state, IIndices indices, PrimitiveType primitiveType, AnimationTransformArray animationTransforms, int primitveCount, int startIndex, int vertexOffset);


		/// <summary>
		/// Tells the buffer that the source data it was created with has changed (Requires that <see cref="ResourceUsage"/> is set to <see cref="Xen.Graphics.ResourceUsage.Dynamic"/>)
		/// </summary>
		void SetDirty();
		/// <summary>
		/// Tells the buffer that the source data it was created with has changed in the specified range (Requires that <see cref="ResourceUsage"/> is set to <see cref="Xen.Graphics.ResourceUsage.Dynamic"/>)
		/// </summary>
		/// <param name="count">number of elements that should be updated</param>
		/// <param name="startIndex"></param>
		void SetDirtyRange(int startIndex, int count);
		/// <summary>
		/// Preload (warm) the resource before its first use
		/// </summary>
		/// <param name="state"></param>
		void Warm(IState state);
		/// <summary>
		/// Preload (warm) the resource before its first use
		/// </summary>
		/// <param name="application"></param>
		void Warm(Application application);

		/// <summary>
		/// Attempt to extract vertex data for a specific vertex element type from the internal vertex data
		/// <para>Note: This method requires the IVertices object has been warmed, and that it has ResourceUsage marked as 'Readable'</para>
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="usage">The element to extract, eg <see cref="VertexElementUsage.Position"/></param>
		/// <param name="usageIndex">The usage index of the element to extract, typically zero (eg, TEXCOORD3 would have a usage index of 3)</param>
		/// <param name="output">An array large enough to store the output data</param>
		/// <returns>Returns true if the data has been written</returns>
		bool TryExtractVertexData<T>(VertexElementUsage usage, int usageIndex, T[] output) where T : struct;
	}

	interface IDeviceVertexBuffer
	{
		VertexDeclaration GetVertexDeclaration(Application application);
		VertexBuffer GetVertexBuffer(Application application, GraphicsDevice device);
		bool IsImplementationUserSpecifiedVertexElements(out VertexElement[] elements);
	}

	/// <summary>
	/// Generic vertex buffer, automatically copies data and sets up a <see cref="VertexDeclaration"/> object.
	/// </summary>
	/// <remarks>
	/// <para>Vertices objects are created by passing in the vertex data to the constructor.</para>
	/// <para>There are two constructors, one for IEnumerable data and a params T[] constructor, for for directly passing in values.</para>
	/// <para>The class knows about common types like the generic List class.</para>
	/// <para>Once created, the vertices object can be used. No further setup is required.</para>
	/// <para></para>
	/// <para>
	/// The vertices class will examine the array data type and construct a vertex declaration automatically. (Provided the structure members are public, are hardware supported types and are logically named). If this automatic generation fails, then the <see cref="VertexElementAttribute"/> attribute can be used to exactly specify structure layout.</para><para>See <see cref="VertexElementAttribute"/> for more details.
	/// </para>
	/// <example>
	/// <para>Using the existing XNA vertex structures to create four vertices:</para>
	/// <code>
	/// IList&lt;VertexPositionColor&gt; items = new List&lt;VertexPositionColor&gt;();
	///	items.Add(new VertexPositionColor(new Vector3(0,0,0), Color.Red));
	/// items.Add(new VertexPositionColor(new Vector3(0,1,0), Color.Green));
	/// items.Add(new VertexPositionColor(new Vector3(1,1,0), Color.Green));
	/// items.Add(new VertexPositionColor(new Vector3(1,0,0), Color.Red));
	/// 
	/// IVertices verts = new Vertices&lt;VertexPositionColor&gt;(items);
	/// </code>
	/// <para>Using the params constructor:</para>
	/// <code>
	/// IVertices verts = new Vertices&lt;VertexPositionColor&gt;(
	///		new VertexPositionColor(new Vector3(0,0,0), Color.Red),
	///		new VertexPositionColor(new Vector3(0,1,0), Color.Green),
	///		new VertexPositionColor(new Vector3(1,1,0), Color.Green),
	///		new VertexPositionColor(new Vector3(1,0,0), Color.Red));
	/// </code>
	/// </example>
	/// <para></para>
	/// <para>To create a dynamic vertex buffer, set the <see cref="ResourceUsage"/> of the buffer to <see cref="ResourceUsage"/>.Dynamic before it's used</para>
	/// <example>
	/// <code>
	/// verts = new new Vertices&lt;VertexPositionColor&gt;(items);
	/// verts.ResourceUsage = ResourceUsage.Dynamic;
	/// </code></example>
	/// <para>When a buffer is allocated dynamically, you can tell the buffer it's source data has changed by calling <see cref="IVertices.SetDirty"/> or <see cref="IVertices.SetDirtyRange"/>. (The source data is the collection passed into the constructor). Internally the vertices object only keeps a weak reference to the source data.</para>
	/// </remarks>
	/// <typeparam name="VertexType"></typeparam>
#if !DEBUG_API
	[System.Diagnostics.DebuggerStepThrough]
#endif
	public sealed class Vertices<VertexType> : Resource, IVertices, IDeviceVertexBuffer, IContentUnload
		where VertexType : struct
	{
#if !DEBUG_API
		[System.Diagnostics.DebuggerStepThrough]
#endif
		internal sealed class Implementation : Bufffer<VertexType>
		{
			internal bool sequentialWriteFlag;
			readonly VertexElement[] elements; // only stored for raw-data vertices
#if DEBUG
			private int previousCopyEnd;
#endif

			public Implementation(IEnumerable<VertexType> vertices) : base(vertices)
			{
			}

			public Implementation(VertexType[] data, VertexElement[] elements)
				: base(data)
			{
				this.elements = elements;
			}

			public Implementation(VertexBuffer data, VertexElement[] elements)
				: base(data.VertexCount)
			{
				this.elements = elements;
			}

			public bool IsRawDataVertices
			{
				get { return elements != null; }
			}

			protected override void WriteBlock(Application app, VertexType[] data, int startIndex, int start, int length, object target)
			{
				if (target is DynamicVertexBuffer)
				{
#if DEBUG
					app.currentFrame.DynamicVertexBufferByesCopied += Stride * length;
#endif
					SetDataOptions setOp = SetDataOptions.None;

					if (sequentialWriteFlag)
					{
						if (start == 0)
							setOp = SetDataOptions.Discard;
						else
						{
							setOp = SetDataOptions.NoOverwrite;

#if DEBUG
							if (start < previousCopyEnd)
								throw new InvalidOperationException("ResourceUsage.DynamicSequential data overwrite detected");
#endif
						}
#if DEBUG
						previousCopyEnd = start + length * Stride;
#endif
					}
					else
						if (start == 0 && length == ((DynamicVertexBuffer)target).VertexCount)
							setOp = SetDataOptions.Discard;
					
					if (IsRawDataVertices) // raw data buffer
						((DynamicVertexBuffer)target).SetData(start, data, startIndex * Stride, length * Stride, 1, setOp);
					else
						((DynamicVertexBuffer)target).SetData(start, data, startIndex, length, Stride, setOp);
				}
				else
				{
#if DEBUG
					app.currentFrame.VertexBufferByesCopied += Stride * length;
#endif
					if (IsRawDataVertices && typeof(VertexType) == typeof(byte)) // raw data buffer
						((VertexBuffer)target).SetData(start, data, startIndex * Stride, length * Stride, 1);
					else
						((VertexBuffer)target).SetData(start, data, startIndex, length, Stride);
				}
			}
			protected override void WriteComplete()
			{
			}

			public VertexElement[] GetVertexElements()
			{
				return elements;
			}
		}
	

		private Implementation buffer;
		private VertexBuffer vb;
		private VertexDeclaration decl;
		private ResourceUsage usage;
		private bool registeredOwner = false;

		internal Implementation Buffer { get { return buffer; } }

		/// <summary>
		/// [NOT VALIDATED] Use with caution. No validation is performed by this method. Creates a vertex buffer from a raw data array, using a user specified <see cref="VertexElement"/> array.
		/// </summary>
		/// <param name="data"></param>
		/// <param name="elements"></param>
		/// <returns></returns>
		/// <remarks>This method is only here to get around some limitations of the XNA content pipeline</remarks>
		public static Vertices<VertexType> CreateRawDataVertices(VertexType[] data, VertexElement[] elements)
		{
			if (typeof(byte) != typeof(VertexType))
				throw new ArgumentException("Only byte[] raw vertices are supported at this time");
			if (data == null || elements == null)
				throw new ArgumentNullException();
			if (elements.Length == 0)
				throw new ArgumentException();

			int stride = VertexElementAttribute.CalculateVertexStride(elements);

			return new Vertices<VertexType>(data, elements, stride);
		}

		/// <summary>
		/// [NOT VALIDATED] Use with caution. No validation is performed by this method. Creates a vertex buffer from a raw data array, using a user specified <see cref="VertexElement"/> array.
		/// </summary>
		/// <remarks>This method is only here to get around some limitations of the XNA content pipeline</remarks>
		public static Vertices<VertexType> CreateRawDataVertices(VertexBuffer buffer, VertexElement[] elements)
		{
			if (buffer == null)
				throw new ArgumentNullException();

			int stride = VertexElementAttribute.CalculateVertexStride(elements);

			return new Vertices<VertexType>(buffer, elements, stride);
		}

		/// <summary>
		/// <para>Creates a vertex buffer that will map an array of float or vector primitives to a specified Vertex Usage and index</para>
		/// </summary>
		/// <param name="data"></param>
		/// <param name="elementUsage"></param>
		/// <param name="index"></param>
		/// <returns></returns>
		public static Vertices<VertexType> CreateSingleElementVertices(VertexType[] data, VertexElementUsage elementUsage, int index)
		{
			if (typeof(float) != typeof(VertexType) &&
				typeof(Vector2) != typeof(VertexType) &&
				typeof(Vector3) != typeof(VertexType) &&
				typeof(Vector4) != typeof(VertexType) &&
				typeof(Matrix) != typeof(VertexType) &&
				typeof(Microsoft.Xna.Framework.Graphics.PackedVector.HalfVector2) != typeof(VertexType) &&
				typeof(Microsoft.Xna.Framework.Graphics.PackedVector.HalfVector4) != typeof(VertexType))
				throw new ArgumentException("Only float and vector types are supported for single element vertex buffers");

			if (data == null)
				throw new ArgumentNullException();
			if (index >= 16 || index < 0)
				throw new ArgumentException("index");
			
			VertexElementFormat format = VertexDeclarationBuilder.DetermineFormat(typeof(VertexType));
			VertexElement[] elements = new VertexElement[] { new VertexElement(0, format, elementUsage, index) };

			int stride = VertexElementAttribute.CalculateVertexStride(elements);

			return new Vertices<VertexType>(data, elements, stride);
		}

		private Vertices(VertexType[] data, VertexElement[] elements, int stride)
		{
			this.usage = ResourceUsage.None;
			buffer = new Implementation(data,elements);
			buffer.OverrideSize(stride, (data.Length * buffer.Stride) / stride);
		}

		private Vertices(VertexBuffer data, VertexElement[] elements, int stride)
		{
			this.usage = data.BufferUsage == BufferUsage.WriteOnly ? ResourceUsage.None : ResourceUsage.Readable;
			this.buffer = new Implementation(data, elements);
			this.vb = data;
			this.decl = data.VertexDeclaration;
			this.buffer.OverrideSize(stride, data.VertexCount);
		}

		/// <summary>
		/// Params Array Constructor
		/// </summary>
		/// <param name="vertices">vertex data containing vertices</param>
		public Vertices(params VertexType[] vertices)
			: this(ResourceUsage.None, (IEnumerable<VertexType>)vertices)
		{
		}


		/// <summary>
		/// Collection constructor
		/// </summary>
		/// <param name="vertices">vertex data</param>
		public Vertices(IEnumerable<VertexType> vertices)
			: this(ResourceUsage.None, vertices)
		{
		}

		private Vertices(ResourceUsage usage, IEnumerable<VertexType> vertices)
		{
#if DEBUG
			//validate the format now (this happens anyway when drawn)
			if (VertexDeclarationBuilder.Instance != null && typeof(VertexType) != typeof(byte) && VertexDeclarationBuilder.Instance != null) //format used by raw data vertices
				VertexDeclarationBuilder.Instance.GetDeclaration(typeof(VertexType));
#endif

			if (typeof(VertexType) == typeof(byte))
				throw new ArgumentException("VertexType");
			if (vertices == null)
				throw new ArgumentNullException("vertices");
			this.usage = usage;
			buffer = new Implementation(vertices);
		}

		/// <summary>Finalizer, calls dispose.</summary>
		~Vertices()
		{
			Dispose();
		}

		internal override int GetAllocatedDeviceBytes()
		{
			if (vb != null)
				return vb.VertexCount * vb.VertexDeclaration.VertexStride;
			return 0;
		}
		internal override int GetAllocatedManagedBytes()
		{
			int count = buffer != null ? buffer.GetCount() : 0;
			return Math.Max(0, count);
		}
		internal override ResourceType GraphicsResourceType
		{
			get { return ResourceType.VertexBuffer; }
		}
		/// <summary>
		/// Gets/Sets the <see cref="ResourceUsage"/> of the vertices
		/// </summary>
		/// <remarks>This value may only be set before the resource's first use</remarks>
		public ResourceUsage ResourceUsage
		{
			get { return usage; }
			set
			{
				if (vb != null)
					throw new InvalidOperationException("Cannot set ResourceUsage when resource is in use");
				usage = value;
				this.buffer.sequentialWriteFlag = (value & ResourceUsage.DynamicSequential) == ResourceUsage.DynamicSequential;
			}
		}
		internal override bool InUse 
		{
			get { return buffer != null && vb != null; }
		}
		internal override bool IsDisposed
		{
			get { return buffer == null && vb == null; }
		}

		Type IVertices.VertexType
		{
			get
			{
				ValidateDisposed();
				return buffer.Type;
			}
		}

		/// <summary>
		/// Number of vertices stored in the buffer, -1 if not yet known
		/// </summary>
		public int Count
		{
			get
			{
				ValidateDisposed();
				return buffer.Count;
			}
		}

		/// <summary>
		/// Byte stride of each vertex
		/// </summary>
		public int Stride
		{
			get
			{
				ValidateDisposed();
				return buffer.Stride;
			}
		}

		/// <summary>
		/// Call when source vertex data has changed and the vertex buffer requires updating.
		/// </summary>
		/// <remarks><see cref="ResourceUsage"/> must be set to <see cref="Graphics.ResourceUsage"/>.Dynamic</remarks>
		public void SetDirty()
		{
			ValidateDisposed();
			ValidateDirty();
			buffer.AddDirtyRange(0, this.buffer.Count, GetType(), true);
		}
		/// <summary>
		/// Call when source vertex data has changed with a given range and the vertex buffer requires updating.
		/// </summary>
		/// <remarks><see cref="ResourceUsage"/> must be set to <see cref="Graphics.ResourceUsage"/>.Dynamic</remarks>
		public void SetDirtyRange(int startIndex, int count)
		{
			ValidateDisposed();
			ValidateDirty();
			buffer.AddDirtyRange(startIndex, count, GetType(), false);
		}

		void ValidateDirty()
		{
			if ((usage & ResourceUsage.Dynamic) == 0)
				throw new InvalidOperationException("this.ResourceUsage lacks ResourceUsage.Dynamic flag");
		}

		void ValidateDisposed()
		{
			if (buffer == null)
				throw new ObjectDisposedException("this");
		}

		bool IDeviceVertexBuffer.IsImplementationUserSpecifiedVertexElements(out VertexElement[] elements)
		{
			elements = buffer.GetVertexElements();
			return buffer.IsRawDataVertices;
		}

		VertexDeclaration IDeviceVertexBuffer.GetVertexDeclaration(Application application)
		{
			if (decl == null)
			{
				if (buffer.IsRawDataVertices)
					decl = application.declarationBuilder.GetDeclaration(application.GraphicsDevice, this.buffer.GetVertexElements());
				else
					decl = application.declarationBuilder.GetDeclaration<VertexType>(application.GraphicsDevice);
			}
			return decl;
		}

		BufferUsage Usage
		{
			get
			{
				if ((usage & Graphics.ResourceUsage.Readable) == 0)
					return BufferUsage.WriteOnly;
				return BufferUsage.None;
			}
		}

		VertexBuffer IDeviceVertexBuffer.GetVertexBuffer(Application application, GraphicsDevice device)
		{
			if (vb == null)
			{
                if (decl == null)
                    ((IDeviceVertexBuffer)this).GetVertexDeclaration(application);

				int size = 32;
				if (buffer.CountKnown)
					size = buffer.Count;
				if (size == 0)
					throw new ArgumentException(string.Format("Vertices<{0}> data size is zero",typeof(VertexType).Name));
				if ((usage & ResourceUsage.Dynamic) == ResourceUsage.Dynamic)
					vb = new DynamicVertexBuffer(device, decl, size, Usage);
				else
					vb = new VertexBuffer(device, decl, size, Usage);

				if ((ResourceUsage & ResourceUsage.DynamicSequential) != ResourceUsage.DynamicSequential)
				{
					int written = buffer.WriteBuffer(application, 0, size * buffer.Stride, vb, null);
					if (written < buffer.Count)
					{
						vb.Dispose();
						if ((usage & ResourceUsage.Dynamic) == ResourceUsage.Dynamic)
							vb = new DynamicVertexBuffer(device, decl, buffer.Count, Usage);
						else
							vb = new VertexBuffer(device, decl, buffer.Count, Usage);
						buffer.WriteBuffer(application, 0, buffer.Count * buffer.Stride, vb, null);
					}
					this.buffer.ClearDirtyRange();
				}

				if ((usage & ResourceUsage.Dynamic) != ResourceUsage.Dynamic)
					buffer.ClearBuffer();
			}

			if ((usage & ResourceUsage.Dynamic) == ResourceUsage.Dynamic &&
				vb != null && ((DynamicVertexBuffer)vb).IsContentLost)
				SetDirty();

			if (this.buffer.IsDirty)
			{
#if XBOX360
				//the xbox doesn't let you update a bound buffer
				device.SetVertexBuffer(null);
#endif

				this.buffer.UpdateDirtyRegions(application,this.vb, this);
			}

			return vb;
		}

		internal override void Warm(Application application,GraphicsDevice device)
		{
			if (!registeredOwner)
			{
				registeredOwner = true;
				application.Content.Add(this);
				return;//add calls load, which calls warm again
			}
			ValidateDisposed();
			if (this.decl == null)
				((IDeviceVertexBuffer)this).GetVertexDeclaration(application);
			if (this.vb == null)
				((IDeviceVertexBuffer)this).GetVertexBuffer(application, device);
		}

		bool IVertices.TryExtractVertexData<T>(VertexElementUsage usage, int usageIndex, T[] output)
		{
			if (vb == null) throw new InvalidOperationException("Vertex data must be warmed before calling TryExtractVertexData");
			if (output == null || output.Length < this.Count)
				throw new ArgumentException("output data array is either null or too small");

			var elements = this.decl.GetVertexElements();
			var format = VertexDeclarationBuilder.DetermineFormat(typeof(T));

			foreach (var e in elements)
			{
				if (e.VertexElementUsage == usage && e.UsageIndex == usageIndex && e.VertexElementFormat == format)
				{
					if ((this.usage & Graphics.ResourceUsage.Readable) == 0)
					{
						if (this.buffer.IsRawDataVertices)
							throw new InvalidOperationException("Vertex buffer was not created with ResourceUsage.Readable set.\nIf this vertex buffer belongs to a Model, set the model importer 'Readable Vertex Data' property to true");
						else
							throw new InvalidOperationException("Vertex buffer was not created with ResourceUsage.Readable set");
					}

					vb.GetData<T>(e.Offset, output, 0, this.Count, this.Stride);
					return true;
				}
			}
			return false;
		}

		#region Draw

		#region standard
		
		/// <summary>
		/// Draw the vertices as primitives, using an optional index buffer (indices)
		/// </summary>
		/// <param name="state"></param>
		/// <param name="indices">indices to use when drawing (may be null)</param>
		/// <param name="primitiveType">Primitive type to draw, eg PrimitiveType.TriangleList</param>
		/// <remarks></remarks>
		public void Draw(DrawState state, IIndices indices, PrimitiveType primitiveType)
		{
			GraphicsDevice device = state.graphics;

			ValidateDisposed();

			if (decl == null)
				Warm(state);

			VertexBuffer vb = ((IDeviceVertexBuffer)this).GetVertexBuffer(state.Application, device);
			IDeviceIndexBuffer devib = indices as IDeviceIndexBuffer;

			state.graphics.SetVertexBuffer(vb);

			if (devib != null)
			{
				state.DrawIndexedPrimitives(buffer.Count, devib.GetIndexBuffer(state.Application, device), indices.Count, primitiveType, 0, indices.MinIndex, (indices.MaxIndex - indices.MinIndex) + 1, 0, -1, null, Xen.Graphics.ShaderSystem.ShaderExtension.None, typeof(VertexType), this.decl, -1);
			}
			else
			{
				state.DrawPrimitives(buffer.Count, primitiveType, 0, -1, null, Xen.Graphics.ShaderSystem.ShaderExtension.None, typeof(VertexType), this.decl);
			}
		}


		/// <summary>
		/// Draw the vertices as primitives with extended parametres, using an optional index buffer (indices)
		/// </summary>
		/// <param name="state"></param>
		/// <param name="indices">indices to use when drawing (may be null)</param>
		/// <param name="primitiveType">Primitive type to use when drawing the buffer</param>
		/// <param name="primitveCount">The number of primitives to draw</param>
		/// <param name="startIndex">The start index in the index buffer (defaults to the first index - 0)</param>
		/// <param name="vertexOffset">Starting offset into the vertex buffer (defaults to the first vertex - 0)</param>
		public void Draw(DrawState state, IIndices indices, PrimitiveType primitiveType, int primitveCount, int startIndex, int vertexOffset)
		{
			DrawBlending(state, indices, primitiveType, null, primitveCount, startIndex, vertexOffset);
		}
		
		#endregion

		#region blending

		/// <summary>
		/// Draw the vertices as primitives using weighted vertex blending (animation), using an optional index buffer (indices)
		/// </summary>
		/// <param name="state"></param>
		/// <param name="indices">indices to use when drawing (may be null)</param>
		/// <param name="primitiveType">Primitive type to draw, eg PrimitiveType.TriangleList</param>
		/// <param name="animationTransforms">A buffer providing a list of animation transform matrices</param>
		/// <remarks></remarks>
		public void DrawBlending(DrawState state, IIndices indices, PrimitiveType primitiveType, AnimationTransformArray animationTransforms)
		{
			DrawBlending(state, indices, primitiveType, animationTransforms, -1, 0, 0);
		}


		/// <summary>
		/// Draw the vertices as primitives using weighted vertex blending (animation) and extended parametres, using an optional index buffer (indices)
		/// </summary>
		/// <param name="state"></param>
		/// <param name="indices">indices to use when drawing (may be null)</param>
		/// <param name="primitiveType">Primitive type to use when drawing the buffer</param>
		/// <param name="primitveCount">The number of primitives to draw</param>
		/// <param name="startIndex">The start index in the index buffer (defaults to the first index - 0)</param>
		/// <param name="vertexOffset">Starting offset into the vertex buffer (defaults to the first vertex - 0)</param>
		/// <param name="animationTransforms">A buffer providing a list of animation transform matrices</param>
		public void DrawBlending(DrawState state, IIndices indices, PrimitiveType primitiveType, AnimationTransformArray animationTransforms, int primitveCount, int startIndex, int vertexOffset)
		{
			GraphicsDevice device = state.graphics;

			ValidateDisposed();

			if (decl == null)
				Warm(state);

			VertexBuffer vb = ((IDeviceVertexBuffer)this).GetVertexBuffer(state.Application, device);
			IDeviceIndexBuffer devib = indices as IDeviceIndexBuffer;

			device.SetVertexBuffer(vb);

			if (devib != null)
			{
				state.DrawIndexedPrimitives(
					buffer.Count, 
					devib.GetIndexBuffer(state.Application, device), 
					indices.Count, 
					primitiveType, 
					vertexOffset, 
					indices.MinIndex, 
					(indices.MaxIndex - indices.MinIndex) + 1, 
					startIndex, 
					primitveCount, 
					animationTransforms, 
					animationTransforms == null ? Graphics.ShaderSystem.ShaderExtension.None : Graphics.ShaderSystem.ShaderExtension.Blending, 
					typeof(VertexType), 
					this.decl, 
					-1);
			}
			else
			{
				state.DrawPrimitives(
					buffer.Count, 
					primitiveType, 
					vertexOffset, 
					primitveCount,
					animationTransforms,
					animationTransforms == null ? Graphics.ShaderSystem.ShaderExtension.None : Graphics.ShaderSystem.ShaderExtension.Blending, 
					typeof(VertexType), 
					this.decl);
			}
		}

		#endregion

		#region instancing

		/// <summary>
		/// Draw multiple instances of the vertex buffer
		/// </summary>
		/// <param name="state"></param>
		/// <param name="indices">Indices to use when drawing (may be null)</param>
		/// <param name="primitiveType">Primitive type to use when drawing the buffer</param>
		/// <param name="instances">A buffer providing a list of instance world matrices to draw</param>
		public void DrawInstances(DrawState state, IIndices indices, PrimitiveType primitiveType, InstanceBuffer instances)
		{
			if (instances == null)
				Draw(state, indices, primitiveType);
			else if (instances.index > 0)
				state.InternalDrawBatch(this, indices, primitiveType, instances);
		}

		/// <summary>
		/// Draw multiple instances of the vertex buffer
		/// </summary>
		/// <param name="state"></param>
		/// <param name="indices">Indices to use when drawing (may be null)</param>
		/// <param name="primitiveType">Primitive type to use when drawing the buffer</param>
		/// <param name="instances">An array providing a list of instance world matrices to draw</param>
		/// <param name="count">The number of instances to draw</param>
		public void DrawInstances(DrawState state, IIndices indices, PrimitiveType primitiveType, Matrix[] instances, int count)
		{
			if (instances == null)
				Draw(state, indices, primitiveType);
			else if (count > 0)
				state.InternalDrawBatch(this, indices, primitiveType, state.GetDynamicInstanceBuffer(instances, count));
		}

		#endregion

		#endregion

		#region IDisposable Members

		/// <summary>
		/// Dispose all graphics resources
		/// </summary>
		public void Dispose()
		{
			if (buffer != null)
			{
				buffer.Dispose();
				buffer = null;
			}
			if (vb != null)
			{
				vb.Dispose();
				vb = null;
			}
			decl = null;
			GC.SuppressFinalize(this);
		}

		#endregion

		#region IContentOwner Members

		void IContentOwner.LoadContent(ContentState state)
		{
			Warm(state);
		}

		void IContentUnload.UnloadContent()
		{
			decl = null;
		}

		#endregion
	}

}
