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
	/// A buffer providing fleixble setup for instancing matrices (for use with IVertices.DrawInstances)
	/// </summary>
#if !DEBUG_API
	[System.Diagnostics.DebuggerStepThrough]
#endif
	public sealed class InstanceBuffer
	{
		internal InstanceBuffer()
		{
		}

		/// <summary>
		/// Construct a new instance buffer, with the specified maximum number of indices
		/// </summary>
		/// <param name="maxCount"></param>
		public InstanceBuffer(int maxCount)
		{
			this.instances = new Matrix[maxCount];
			this.instanceLength = maxCount;

			this.instainceData = new Vertices<Matrix>(this.instances);
			this.instainceData.ResourceUsage = ResourceUsage.DynamicSequential;
		}

		/// <summary>
		/// Construct a new instance buffer, with the specified list of static instances. The instances will be readonly
		/// </summary>
		public InstanceBuffer(Matrix[] instances)
		{
			this.instances = instances;
			this.instanceLength = instances.Length;
			this.index = instances.Length;
			this.instanceIndex = instances.Length;

			this.instainceData = new Vertices<Matrix>(this.instances);
			this.lastWriteIndex = -1;	//special read only flag
		}

		//buffer is being used internally as a dynamic buffer
		internal void Set(DrawState.StreamBuffer buffer, Matrix[] instances, int start, int length, int count)
		{
			this.dynamicInstanceBuffer = buffer;
			this.instances = instances;
			this.instanceIndex = start + count;
			this.instanceLength = length;
			this.index = count;
		}

		//there are two ways that instance data is stored internally
		internal DrawState.StreamBuffer dynamicInstanceBuffer;
		private readonly Vertices<Matrix> instainceData;

		internal Matrix[] instances;
		internal int index;
		internal int instanceIndex, instanceLength;
		private int lastWriteIndex;	//-1 means readonly

		internal IVertices GetInstanceFixedData()
		{
			if (instainceData != null && index > lastWriteIndex && lastWriteIndex != -1)
			{
				this.instainceData.SetDirtyRange(lastWriteIndex, index - lastWriteIndex);
				lastWriteIndex = index;
			}
			return instainceData;
		}

		/// <summary>
		/// Add an instance to the buffer, using the DrawState to get the current World Matrix
		/// </summary>
		/// <param name="state"></param>
		public void AddInstance(DrawState state)
		{
			if (lastWriteIndex == -1)
				throw new InvalidOperationException("InstanceBuffer is Readonly");
#if DEBUG
			if (index == instanceLength)
				throw new IndexOutOfRangeException();
#endif

			instances[instanceIndex] = state.matrices.World.value;

			instanceIndex++;
			index++;
		}

		/// <summary>
		/// Add an instance to the buffer
		/// </summary>
		/// <param name="position"></param>
		public void AddInstance(ref Vector3 position)
		{
			if (lastWriteIndex == -1)
				throw new InvalidOperationException("InstanceBuffer is Readonly");
#if DEBUG
			if (index == instanceLength)
				throw new IndexOutOfRangeException();
#endif
			Matrix mat = new Matrix();

#if XBOX360
			mat = new Matrix();
#endif

			mat.M11 = 1;
			mat.M22 = 1;
			mat.M33 = 1;
			mat.M41 = position.X;
			mat.M42 = position.Y;
			mat.M43 = position.Z;
			mat.M44 = 1;

			instances[instanceIndex] = mat;

			instanceIndex++;
			index++;
		}

		/// <summary>
		/// Add an instance directly to the buffer
		/// </summary>
		public void AddInstance(ref Matrix matrix)
		{
			if (lastWriteIndex == -1)
				throw new InvalidOperationException("InstanceBuffer is Readonly");
#if DEBUG
			if (index == instanceLength)
				throw new IndexOutOfRangeException();
#endif
			index++;
			instances[instanceIndex++] = matrix;
		}

		/// <summary>
		/// Clear the instance list
		/// </summary>
		public void Clear()
		{
			if (lastWriteIndex == -1)
				throw new InvalidOperationException("InstanceBuffer is Readonly");
			this.instanceIndex -= this.index;
			this.index = 0;
			this.lastWriteIndex = 0;
		}

		/// <summary>
		/// Gets the number of instances added to the buffer
		/// </summary>
		public int InstanceCount { get { return index; } }
		/// <summary>
		/// Gets the maximum number of instances that can be added to this buffer
		/// </summary>
		public int MaxInstanceCount { get { return instanceLength; } }

	}

	/// <summary>
	/// Sets parametres for hardware instancing when drawing with a VerticesGroup object (Supported on windows with vertex shader 3 or greater, emulated on Xbox360 when using <see cref="DataLayout.Stream0Geometry_Stream1InstanceData"/>)
	/// </summary>
	/// <remarks>
	/// <para>Hardware instancing is not supported on the xbox 360, only PCs with vertex shader model 3.0 are supported.</para>
	/// <para>However, when using <see cref="DataLayout.Stream0Geometry_Stream1InstanceData"/>, support is emulated on the Xbox by repeating index data.</para>
	/// <para>Hardware instancing uses two or more vertex buffers, with each buffer being repeated.</para>
	/// <para>Common usage, would be to have the first buffer store the model geometry, repeated X number of times.
	/// With the second buffer storing per-instance data, such as the world matrix.
	/// Instance data is repeated for each vertex. A 5 vertex mesh drawn 3 times will repeat the instance (world matrix) data AAAAABBBBBCCCCC, where the vertices are repeated ABCDEABCDEABCDE.</para>
	/// <para>The DrawBatch methods in <see cref="DrawState"/> are an easy way to automatically setup the instance data buffer in an optimal way.</para>
	/// <para></para>
	/// <para>Supporting instancing in a shader is fairly simple:</para>
	/// <para>Instead of using the WORLDVIEWPROJECTION matrix to transform your vertices,
	/// use the VIEWPROJECTION matrix multipled by the world matrix from instance data:</para>
	/// <para></para>
	/// <para>Non instancing example:</para>
	/// <code>
	/// float4x4 worldViewProjection : WORLDVIEWPROJECTION;
	/// 
	/// void SimpleVS(in float4 position : POSITION, out float4 out_position : POSITION)
	/// {
	///		out_position = mul(pos,worldViewProjection);
	/// }
	/// </code>
	/// <para>Instancing example:</para>
	/// <code>
	/// float4x4 viewProjection : VIEWPROJECTION;
	/// 
	/// void SimpleInstancedVS(	in  float4 position		: POSITION, 
	///							out float4 out_position : POSITION,
	///							in  float4 worldX		: POSITION12,
	/// 						in  float4 worldY		: POSITION13,
	/// 						in  float4 worldZ		: POSITION14,
	/// 						in  float4 worldW		: POSITION15)
	/// {
	///		float4x4 world = float4x4(worldX,worldY,worldZ,worldW);
	/// 
	///		out_position = mul(mul(pos,world),viewProjection);
	/// }
	/// </code>
	/// <para>This can also be done with a simple structure:</para>
	/// <code>
	/// struct __InstanceWorldMatirx
	/// {
	/// 	float4 position12 : POSITION12;
	/// 	float4 position13 : POSITION13;
	/// 	float4 position14 : POSITION14;
	/// 	float4 position15 : POSITION15;
	/// };
	/// 
	/// float4x4 viewProjection : VIEWPROJECTION;
	/// 
	/// void SimpleInstancedVS(	in  float4 position		: POSITION, 
	///							out float4 out_position : POSITION,
	///							in  __InstanceWorldMatirx world)
	/// {
	///		out_position = mul(mul(pos,(float4x4)world),viewProjection);
	/// }
	/// </code>
	/// <para></para>
	/// <para><b>Shader Instancing:</b></para>
	/// <para>It is highly recommended to implement shader-instancing for rendring smaller batches of small objects, or for supporting instancing on older hardware and the xbox</para>
	/// <para>Shader instancing is a technique where the instance matrices are stored as shader constants, and the vertex/index data is duplicated to draw multiple instances.</para>
	/// <para>The disadvantage is the extra memory overhead is considerable for large meshes. Each vertex must also store an index to the instance matrix to use.</para>
	/// <para>For vertex-shader 2.0, there is uaully room for an array of 50-60 matrices</para>
	/// <code>
	/// float4x4 viewProjection : VIEWPROJECTION;
	/// float4x4 world[60];
	/// 
	/// void ShaderInstancedVS(	in  float4 position		: POSITION, 
	///							out float4 out_position : POSITION, 
	///							in  float  index		: POSITION1)
	/// {
	///		out_position = mul(mul(pos,world[index]),viewProjection);
	/// }
	/// </code>
	/// </remarks>
	/// <seealso cref="DrawState"/>
#if !DEBUG_API
	[System.Diagnostics.DebuggerStepThrough]
#endif
	public sealed class StreamFrequency
	{
		/// <summary>
		/// Data layout for the frequency data. Most implementations will want to use <see cref="DataLayout.Stream0Geometry_Stream1InstanceData"/>.
		/// </summary>
		public enum DataLayout
		{
			/// <summary>
			/// First vertex buffer contains geometry data, second buffer contains instance data (eg world matrix)
			/// </summary>
			Stream0Geometry_Stream1InstanceData,
			/// <summary>
			/// Use this if you know what your doing...
			/// </summary>
			Custom
		}

		internal readonly int[] frequency;
		internal DataLayout layout = DataLayout.Custom;

		/// <summary>
		/// Specifies how many times the geometry should be drawn
		/// </summary>
		public int InstanceCount { get; set; }

		/// <summary>
		/// Setup the frequency data and source vertex buffer
		/// </summary>
		/// <param name="vertices"></param>
		/// <param name="repeatCount">Number of times the vertex data should be repeated</param>
		public StreamFrequency(VerticesGroup vertices, int repeatCount) : this(vertices, DataLayout.Stream0Geometry_Stream1InstanceData)
		{
			this.InstanceCount = repeatCount;
		}

		/// <summary>
		/// Automatic setup of frequency data from a vertices group (eg, geometry and instance data in two buffers)
		/// </summary>
		/// <param name="vertices"></param>
		/// <param name="layout"></param>
		public StreamFrequency(VerticesGroup vertices, DataLayout layout)
		{
			this.layout = layout;
			this.frequency = new int[vertices.ChildCount];
			
			if (layout == DataLayout.Stream0Geometry_Stream1InstanceData)
			{
				if (vertices.ChildCount < 2)
					throw new ArgumentException("vertices.ChildCount");
				if (vertices.GetChild(1) != null)
					InstanceCount = vertices.GetChild(1).Count;
				frequency[0] = 0;
				frequency[1] = 1;
			}
		}


		/// <summary>
		/// Set the raw frequency/indexFrequency/dataFrequency values. Only exposed as their use is so badly documented by microsoft, and there may be hidden unexpected uses.
		/// </summary>
		/// <param name="index">the stream index to set</param>
		/// <param name="instanceCount">The number of instances stored in the stream</param>
		public void SetData(int index, int instanceCount)
		{
			layout = DataLayout.Custom;
			this.frequency[index] = instanceCount;
		}
	}

	/// <summary>
	/// Stores a group of <see cref="IVertices"/> instances, sharing their unique elements in a single <see cref="VertexDeclaration"/>
	/// </summary>
	/// <remarks>
	/// <para>This class can be used to draw vertices from multiple <see cref="IVertices"/> vertex buffers, sharing the first instance of each element type.</para>
	/// <para>For example, the first vertex buffer may specify positions, while the second may specify normals. Using a <see cref="VerticesGroup"/> is a way to share this data, without creating a new resource.</para>
	/// </remarks>
#if !DEBUG_API
	[System.Diagnostics.DebuggerStepThrough]
#endif
	public sealed class VerticesGroup : Resource, IVertices, IDeviceVertexBuffer, IContentUnload
	{
		/// <summary>
		/// The maximum group size supported by the hardware
		/// </summary>
		public static int MaxGroupSize
		{
			get
			{
				return 16;
			}
		}
		/// <summary>
		/// Construct the group of vertices
		/// </summary>
		/// <param name="children"></param>
		public VerticesGroup(params IVertices[] children)
		{
			if (children.Length <= 1)
				throw new ArgumentException("Specify at least two children");

			if (children.Length > MaxGroupSize)
				throw new ArgumentException("Hardware does not support this many streams, see VerticesGroup.MaxGroupSize");

			int index = 0;
			foreach (IVertices vb in children)
			{
				if (vb == null)
					throw new ArgumentNullException();
				if (vb is VerticesGroup)
					throw new ArgumentException("VerticesGroup as Child");
				if (vb is IDeviceVertexBuffer == false)
					throw new ArgumentException(vb.GetType().Name + " as Child");
				if ((vb.ResourceUsage & ResourceUsage.Dynamic) == ResourceUsage.Dynamic)
					this.containsDynamicBuffers = true;
				if (Array.IndexOf(children, vb) != index++)
					throw new ArgumentException("Duplicate entry detected");
			}

			bufferTypes = new Type[children.Length];
			for (int i = 0; i < children.Length; i++)
				bufferTypes[i] = children[i].VertexType;

			this.buffers = children;
			this.offsets = new int[children.Length];
		}


		/// <summary>
		/// Construct the group of vertices
		/// </summary>
		internal VerticesGroup(int count)
		{
			if (count <= 1)
				throw new ArgumentException("Specify at least two children");

			if (count > MaxGroupSize)
				throw new ArgumentException("Hardware does not support this many streams, see VerticesGroup.MaxGroupSize");

			bufferTypes = new Type[count];

			this.buffers = new IVertices[count];
			this.offsets = new int[count];
		}


		private readonly IVertices[] buffers;
		private readonly int[] offsets;
		private readonly Type[] bufferTypes;
		private int count = -1;
		private readonly bool containsDynamicBuffers;
        private VertexBufferBinding[] binding, instancBinding;
		private bool ownerRegistered = false;
		private bool bindingDirty;
 
		/// <summary>
		/// Get a group member by index
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public IVertices GetChild(int index)
		{
			return buffers[index];
		}

		internal void SetChild(int index, IVertices child)
		{
			if (buffers[index] != child)
			{
				buffers[index] = child;
				bufferTypes[index] = child.VertexType;
				bindingDirty = true;
			}
		}

		ResourceUsage IVertices.ResourceUsage { get { throw new NotImplementedException(); } set { throw new NotImplementedException(); } }

		/// <summary>
		/// Number of group members
		/// </summary>
		public int ChildCount
		{
			get { return buffers.Length; }
		}

		/// <summary>
		/// Approximate number of vertices (minimum over the group), -1 is not yet known
		/// </summary>
		public int Count
		{
			get 
			{
				if (count != -1)
					return count;
				if (containsDynamicBuffers)
					throw new NotSupportedException("Count is not supported with Dynamic children");
				int c = int.MaxValue;
				foreach (IVertices vb in buffers)
				{
					c = Math.Min(vb.Count, c);
				}
				this.count = c;
				return count;
			}
			set
			{
				count = value;
			}
		}

		/// <summary>
		/// Set the read offset to a group member
		/// </summary>
		/// <param name="index"></param>
		/// <param name="offset">Element offset to start reading into the buffer</param>
		public void SetIndexOffset(int index, int offset)
		{
			offsets[index] = offset * buffers[index].Stride;
		}

		/// <summary>
		/// Get the read offset of a group member
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public int GetIndexOffset(int index)
		{
			return offsets[index] / buffers[index].Stride;
		}

		int IVertices.Stride
		{
			get { throw new NotSupportedException(); }
		}

		Type IVertices.VertexType
		{
			get { throw new NotSupportedException(); }
		}

		#region IDeviceVertexBuffer Members

		VertexBuffer IDeviceVertexBuffer.GetVertexBuffer(Application app, GraphicsDevice device)
		{
			return null;
		}

		bool IDeviceVertexBuffer.IsImplementationUserSpecifiedVertexElements(out VertexElement[] elements)
		{
			elements = null;
			return false;
		}

		VertexDeclaration IDeviceVertexBuffer.GetVertexDeclaration(Application game)
		{
			return null;
		}
        
		void UpdateBinding(DrawState state)
		{
            state.Application.declarationBuilder.ValidateArrayDeclaration(state.graphics, bufferTypes, this.buffers);

			if (binding == null)
			{
				this.binding = new VertexBufferBinding[this.buffers.Length];
				this.instancBinding = new VertexBufferBinding[this.buffers.Length];
			}
            for (int i = 0; i < buffers.Length; i++)
            {
				VertexBuffer vb = ((IDeviceVertexBuffer)this.buffers[i]).GetVertexBuffer(state.Application, state.graphics);
                this.binding[i] = new VertexBufferBinding(vb);
				this.instancBinding[i] = new VertexBufferBinding(vb);
            }

			bindingDirty = false;
		}

		#endregion

		#region core draw function
		
		private void  DrawCore(DrawState state, IIndices indices, PrimitiveType primitiveType, AnimationTransformArray animationTransforms, StreamFrequency frequency, int primitiveCount, int startIndex, int vertexOffset)
		{
			if (frequency != null && !state.Application.SupportsHardwareInstancing)
				throw new InvalidOperationException("Only HiDef devices can use hardware instancing. Check DrawState.Properties.SupportsHardwareInstancing");

			GraphicsDevice device = state.graphics;

			IDeviceIndexBuffer devib = indices as IDeviceIndexBuffer;

            if (binding == null || bindingDirty)
				UpdateBinding(state);

			int instanceCountDebug = -1;

			//call getVertexBuffer() on any referenced buffers
			for (int i = 0; i < buffers.Length; i++)
			{
				//this will make sure they are updated
				(buffers[i] as IDeviceVertexBuffer).GetVertexBuffer(state.Application, state.graphics);
			}

			if (frequency != null)
			{
				if (devib == null)
					throw new InvalidOperationException("An index buffer must be specified when using instancing");
				instanceCountDebug = frequency.InstanceCount;
				for (int i = 0; i < this.buffers.Length; i++)
					this.instancBinding[i] = new VertexBufferBinding(this.instancBinding[i].VertexBuffer, 0, frequency.frequency[i]);
				device.SetVertexBuffers(instancBinding);
			}
			else
				device.SetVertexBuffers(binding);

			//set the shader extenion...
			Xen.Graphics.ShaderSystem.ShaderExtension extension = Xen.Graphics.ShaderSystem.ShaderExtension.None;

			//blending or instancing?
			if (animationTransforms != null)
				extension = Xen.Graphics.ShaderSystem.ShaderExtension.Blending;
			else if (frequency != null && frequency.layout == StreamFrequency.DataLayout.Stream0Geometry_Stream1InstanceData)
				extension = Xen.Graphics.ShaderSystem.ShaderExtension.Instancing;
			

			if (devib != null)
			{
				state.DrawIndexedPrimitives(this.Count, devib.GetIndexBuffer(state.Application, device), indices.Count, primitiveType, vertexOffset, indices.MinIndex, (indices.MaxIndex - indices.MinIndex) + 1, startIndex, primitiveCount, animationTransforms, extension, null, null, instanceCountDebug);
			}
			else
			{
				state.DrawPrimitives(this.Count, primitiveType, vertexOffset, primitiveCount, animationTransforms, extension, null, null);
			}
		}

		#endregion

		#region standard Draw

		/// <summary>
		/// Draw the vertices group as primitives, using an optional index buffer (indices)
		/// </summary>
		/// <param name="state"></param>
		/// <param name="indices">(optional) indices to use during drawing</param>
		/// <param name="primitiveType">Primitive type to draw, eg PrimitiveType.TriangleList</param>
		/// <remarks></remarks>
		public void Draw(DrawState state, IIndices indices, PrimitiveType primitiveType)
		{
			this.DrawCore(state, indices, primitiveType, null, null, -1, 0, 0);
		}


		/// <summary>
		/// Draw the vertices as primitives with extended parametres, using an optional index buffer (indices)
		/// </summary>
		/// <param name="state"></param>
		/// <param name="indices">indices to use when drawing (may be null)</param>
		/// <param name="primitiveType">Primitive type to use when drawing the buffer</param>
		/// <param name="primitiveCount">The number of primitives to draw</param>
		/// <param name="startIndex">The start index in the index buffer (defaults to the first index - 0)</param>
		/// <param name="vertexOffset">Starting offset into the vertex buffer (defaults to the first vertex - 0)</param>
		public void Draw(DrawState state, IIndices indices, PrimitiveType primitiveType, int primitiveCount, int startIndex, int vertexOffset)
		{
			this.DrawCore(state, indices, primitiveType, null, null, primitiveCount, startIndex, vertexOffset);
		}

		#endregion

		#region blending

		/// <summary>
		/// Draw the buffer using weighted vertex blending (animation)
		/// </summary>
		/// <param name="state"></param>
		/// <param name="indices">Indices to use when drawing (may be null)</param>
		/// <param name="primitiveType">Primitive type to use when drawing the buffer</param>
		/// <param name="animationTransforms">A buffer providing a list of animation transform matrices</param>
		public void DrawBlending(DrawState state, IIndices indices, PrimitiveType primitiveType, AnimationTransformArray animationTransforms)
		{
			DrawCore(state, indices, primitiveType, animationTransforms, null, -1,0,0);
		}

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
		public void DrawBlending(DrawState state, IIndices indices, PrimitiveType primitiveType, AnimationTransformArray animationTransforms, int primitveCount, int startIndex, int vertexOffset)
		{
			DrawCore(state, indices, primitiveType, animationTransforms, null,primitveCount,startIndex,vertexOffset);
		}


		#endregion

		#region draw frequency

		/// <summary>
		/// <para>Draw the vertices group as primitives, using an optional index buffer (indices) and a <see cref="StreamFrequency"/> object to directly specify Hardware Instancing information (Shader Model 3 and Windows Only)</para>
		/// <para>Note, using DrawInstances is recommended over directly calling this method</para>
		/// </summary>
		/// <param name="state"></param>
		/// <param name="indices">(optional) indices to use during drawing</param>
		/// <param name="primitiveType">Primitive type to draw, eg PrimitiveType.TriangleList</param>
		/// <param name="frequency">(optional) <see cref="StreamFrequency"/> setting the shader model 3 instance frequency data (used for hardware instancing)</param>
		/// <remarks></remarks>
		public void DrawFrequency(DrawState state, IIndices indices, PrimitiveType primitiveType, StreamFrequency frequency)
		{
			this.DrawCore(state, indices, primitiveType, null, frequency, -1, 0, 0);
		}

		/// <summary>
		/// <para>Draw the vertices group as primitives, using an optional index buffer (indices) and a <see cref="StreamFrequency"/> object to directly specify Hardware Instancing information (Shader Model 3 and Windows Only)</para>
		/// <para>Note, using DrawInstances is recommended over directly calling this method</para>
		/// </summary>
		/// <param name="state"></param>
		/// <param name="indices">(optional) indices to use during drawing</param>
		/// <param name="primitiveType">Primitive type to draw, eg PrimitiveType.TriangleList</param>
		/// <param name="frequency">(optional) <see cref="StreamFrequency"/> setting the shader model 3 instance frequency data (used for hardware instancing)</param>
		/// <param name="primitiveCount">The number of primitives to draw</param>
		/// <param name="startIndex">The start index in the index buffer (defaults to the first index - 0)</param>
		/// <param name="vertexOffset">Starting offset into the vertex buffer (defaults to the first vertex - 0)</param>
		/// <remarks></remarks>
		public void DrawFrequency(DrawState state, IIndices indices, PrimitiveType primitiveType, StreamFrequency frequency, int primitiveCount, int startIndex, int vertexOffset)
		{
			DrawCore(state, indices, primitiveType, null, frequency, primitiveCount, startIndex, vertexOffset);
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

		void IDisposable.Dispose()
		{
			//has no effect
		}

		/// <summary>
		/// Not supported for vertices groups
		/// </summary>
		void IVertices.SetDirty()
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Not supported for vertices groups
		/// </summary>
		/// <param name="count"></param>
		/// <param name="startIndex"></param>
		void IVertices.SetDirtyRange(int startIndex, int count)
		{
			throw new NotSupportedException();
		}

		internal override int GetAllocatedManagedBytes()
		{
			return 0;
		}

		internal override int GetAllocatedDeviceBytes()
		{
			return 0;
		}

		internal override ResourceType GraphicsResourceType
		{
			get { return ResourceType.VertexBuffer; }
		}

		internal override bool InUse
		{
			get { return binding != null; }
		}

		internal override bool IsDisposed
		{
			get { return this.buffers == null; }
		}

		internal override void Warm(Application application, GraphicsDevice device)
		{
			if (!ownerRegistered)
			{
				ownerRegistered = true;
				application.Content.Add(this);
				return;
			}
			if (buffers != null)
			{
				foreach (IVertices buffer in buffers)
				{
					if (buffer is Resource)
						(buffer as Resource).Warm(application);
				}
			}
			((IDeviceVertexBuffer)this).GetVertexDeclaration(application);
		}

		bool IVertices.TryExtractVertexData<T>(VertexElementUsage usage, int usageIndex, T[] output)
		{
			foreach (var child in buffers)
			{
				if (child.TryExtractVertexData(usage, usageIndex, output))
					return true;
			}
			return false;
		}

		#region IContentOwner Members

		void IContentOwner.LoadContent(ContentState state)
		{
			Warm(state);
		}

		void IContentUnload.UnloadContent()
		{
			binding = null;
		}

		#endregion
	}
}