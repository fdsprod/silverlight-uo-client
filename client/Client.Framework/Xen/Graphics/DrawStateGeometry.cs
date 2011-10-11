using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using System.Collections;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Xen
{
	sealed partial class DrawState
	{
		//when rendering a batch, store indices in a stream buffer
		//stream buffers are cleared every frame
		internal sealed class StreamBuffer
		{
			private readonly Matrix[] instanceMatricesData;
			private readonly Graphics.Vertices<Matrix> instanceMatrices;
			private int index;
			private bool bufferActive;
			private static int InstanceSize = System.Runtime.InteropServices.Marshal.SizeOf(typeof(Matrix));
			internal Graphics.InstanceBuffer FrequencyInstanceBuffer;

			public StreamBuffer(int count)
			{
				int createSize = 256;
				while (count > createSize)
					createSize *= 2;

				this.instanceMatricesData = new Matrix[createSize];
				this.instanceMatrices = new Graphics.Vertices<Matrix>(this.instanceMatricesData);
				this.instanceMatrices.ResourceUsage = Xen.Graphics.ResourceUsage.DynamicSequential;
				this.index = 0;
			}

			public int FreeIndices
			{
				get { return this.instanceMatricesData.Length - index; }
			}
			public int WrittenIndices
			{
				get { return index; }
			}

			public Matrix[] Prepare(out int startIndex)
			{
				if (bufferActive)
					throw new InvalidOperationException("A call to BeginDrawBatch has been made while already within a BeginDrawBatch/EndDrawBatch operation./nIf using DrawState.GetDynamicInstanceBuffer, make sure to use the buffer during the frame it was got");
				bufferActive = true;
				startIndex = index;
				return instanceMatricesData;
			}

			public void Fill(DrawState state, Graphics.VerticesGroup streamGroup, Graphics.IVertices vertices, Matrix[] matrices, int count)
			{
				if (bufferActive)
				{
					bufferActive = false;

					if (matrices == this.instanceMatricesData)
						this.instanceMatrices.SetDirtyRange(index, count);
					else
						this.instanceMatrices.Buffer.WriteBuffer(state.Application, 0, count * 4 * 16, (this.instanceMatrices as Xen.Graphics.IDeviceVertexBuffer).GetVertexBuffer(state.Application, state.graphics), matrices);

					this.index += count;
				}

				streamGroup.Count = vertices.Count;
				streamGroup.SetChild(0, vertices);

				streamGroup.SetChild(1, instanceMatrices);
				streamGroup.SetIndexOffset(1, index - count);
			}

			public void Clear()
			{
				if (bufferActive)
					throw new InvalidOperationException("A call to GetDynamicInstanceBuffer has been made without a matching draw call.");
				
				index = 0;
			}

			public bool InUse { get { return this.bufferActive; } }
		}

		internal void PrepareForNewFrame()
		{
			foreach (StreamBuffer sb in streamBuffers)
				sb.Clear();
		}

		private readonly List<StreamBuffer> streamBuffers;
		private readonly Graphics.StreamFrequency frequency;
		private readonly Graphics.VerticesGroup frequencyBuffer;

		private Graphics.InstanceBuffer nullInstanceBuffer = new Xen.Graphics.InstanceBuffer();

		private StreamBuffer GetBuffer(int count)
		{
#if XBOX360
			StreamBuffer closestBuffer = null;

			//on the xbox,
			//when tiling is active, a buffer can not be reused within the frame
			//technically it can, but XNA detects it and thinks the entire buffer is
			//being rewritten (which it isn't) and throws an exception.

			foreach (StreamBuffer buf in streamBuffers)
			{
				if (buf.WrittenIndices == 0 &&
					buf.FreeIndices >= count)
				{
					if (closestBuffer == null)
						closestBuffer = buf;
					else
					{
						if (buf.FreeIndices > closestBuffer.FreeIndices)
							closestBuffer = buf;
					}
				}
			}

			if (closestBuffer != null)
				return closestBuffer;

#else
			foreach (StreamBuffer buf in streamBuffers)
			{
				if (buf.FreeIndices >= count && buf.InUse == false)
					return buf;
			}
#endif

			StreamBuffer buffer = new StreamBuffer(count);
			streamBuffers.Add(buffer);
			return buffer;
		}


		/// <summary>
		/// Returns a buffer that instances can be written to. Use to draw instances in a call to DrawInstances"/>
		/// </summary>
		public Graphics.InstanceBuffer GetDynamicInstanceBuffer(int maxInstanceCount)
		{
			if (maxInstanceCount < 0)
				throw new ArgumentException();

			if (maxInstanceCount == 0)
				return nullInstanceBuffer;

			StreamBuffer buffer = GetBuffer(maxInstanceCount);
			int start;
			Matrix[] instanceMartixData = buffer.Prepare(out start);

			if (buffer.FrequencyInstanceBuffer == null)
				buffer.FrequencyInstanceBuffer = new Xen.Graphics.InstanceBuffer();
			buffer.FrequencyInstanceBuffer.Set(buffer, instanceMartixData, start, maxInstanceCount, 0);

			return buffer.FrequencyInstanceBuffer;
		}

		/// <summary>
		/// Returns a buffer that instances with instances written to. Use to draw instances in a call to DrawInstances
		/// </summary>
		public Graphics.InstanceBuffer GetDynamicInstanceBuffer(Matrix[] instances, int count)
		{
			if (instances == null)
				throw new ArgumentNullException();

			if (count <= 0)
				return nullInstanceBuffer;

			StreamBuffer buffer = GetBuffer(count);

			int startIndex = 0;
			buffer.Prepare(out startIndex);

			if (buffer.FrequencyInstanceBuffer == null)
				buffer.FrequencyInstanceBuffer = new Xen.Graphics.InstanceBuffer();
			buffer.FrequencyInstanceBuffer.Set(buffer, instances, startIndex, instances.Length, count);

			return buffer.FrequencyInstanceBuffer;
		}

		/// <summary>
		/// [Requires a HiDef graphics device to support hardware instancing]
		/// </summary>
		/// <param name="vertices">Vertex buffer of the instances to be drawn</param>
		/// <param name="indices">Index buffer of the instances to be drawn</param>
		/// <param name="primitiveType">Primitive type to be drawn</param>
		/// <param name="instances">Instance buffer that contains the instances to be drawn</param>
		internal void InternalDrawBatch(Xen.Graphics.IVertices vertices, Graphics.IIndices indices, PrimitiveType primitiveType, Graphics.InstanceBuffer instances)
		{
			ValidateRenderState();

			if (instances == null)
			{
				vertices.Draw(this, indices, primitiveType);
			}
			else
			{
				if (instances.InstanceCount == 0)
					return;

				if (!application.SupportsHardwareInstancing)
				{
					//loop through them all one by one.
					this.matrices.World.Push();

					//loop the entries
					int count = instances.index;
					int end = instances.instanceIndex;
					int start = end - count;
					for (int i = start; i < end; i++)
					{
						this.matrices.World.SetMatrix(ref instances.instances[i]);
						vertices.Draw(this, indices, primitiveType);
					}

					this.matrices.World.Pop();
				}
				else
				{
					if (instances.dynamicInstanceBuffer != null)
					{		
#if XBOX360
						//unlikely, but this can sometimes fall over on the 360 if the same buffer is currently in use
						graphics.SetVertexBuffer(null);
#endif
						//a dynamic stream buffer is in use, fill it out
						instances.dynamicInstanceBuffer.Fill(this, frequencyBuffer, vertices, instances.instances, instances.InstanceCount);
					}
					else
					{
						//the instances are not dynamically allocated.
						frequencyBuffer.Count = vertices.Count;
						frequencyBuffer.SetChild(0, vertices);

						frequencyBuffer.SetChild(1, instances.GetInstanceFixedData());
						frequencyBuffer.SetIndexOffset(1, 0);
					}

					frequency.InstanceCount = instances.InstanceCount;
					frequencyBuffer.DrawFrequency(this, indices, primitiveType, frequency);
				}
			}
		}



		//drawing dynamic vertices


		/// <summary>
		/// <para>Draw dynamic vertices as primitives with extended parametres, using an index buffer (indices)</para>
		/// <para>NOTE: When using this method, the vertex and index data will be copied every frame.</para>
		/// <para>This method is best for volatile vertex data that is changing every frame.</para>
		/// <para>Use a dynamic Vertices object for dynamic data that changes less frequently, or requires use with a VerticesGroup objcet.</para>
		/// </summary>
		/// <param name="vertices">source vertex data to use for drawing vertices</param>
		/// <param name="indices">indices to use when drawing</param>
		/// <param name="primitiveType">Primitive type to use when drawing the buffer</param>
		/// <param name="primitveCount">The number of primitives to draw</param>
		/// <param name="startIndex">The start index in the index buffer (defaults to the first index - 0)</param>
		/// <param name="vertexOffset">Starting offset into the vertex buffer (defaults to the first vertex - 0)</param>
		/// <param name="maximumIndex">The maximum index used by the index buffer. This determines how many vertices will be copied (zero will copy all vertices)</param>
		public void DrawDynamicIndexedVertices<VertexType>(VertexType[] vertices, int[] indices, PrimitiveType primitiveType, int primitveCount, int startIndex, int maximumIndex, int vertexOffset) where VertexType : struct
		{
			DrawDynamicVerticesArray(vertices, indices, primitiveType, primitveCount, startIndex, vertexOffset, maximumIndex, null);
		}
		/// <summary>
		/// <para>Draw dynamic vertices as primitives with extended parametres, using an index buffer (indices)</para>
		/// <para>NOTE: When using this method, the vertex and index data will be copied every frame.</para>
		/// <para>This method is best for volatile vertex data that is changing every frame.</para>
		/// <para>Use a dynamic Vertices object for dynamic data that changes less frequently, or requires use with a VerticesGroup objcet.</para>
		/// </summary>
		/// <param name="vertices">source vertex data to use for drawing vertices</param>
		/// <param name="indices">indices to use when drawing</param>
		/// <param name="primitiveType">Primitive type to use when drawing the buffer</param>
		/// <param name="primitveCount">The number of primitives to draw</param>
		/// <param name="startIndex">The start index in the index buffer (defaults to the first index - 0)</param>
		/// <param name="vertexOffset">Starting offset into the vertex buffer (defaults to the first vertex - 0)</param>
		/// <param name="maximumIndex">The maximum index used by the index buffer. This determines how many vertices will be copied (zero will copy all vertices)</param>
		public void DrawDynamicIndexedVertices<VertexType>(VertexType[] vertices, short[] indices, PrimitiveType primitiveType, int primitveCount, int startIndex, int maximumIndex, int vertexOffset) where VertexType : struct
		{
			DrawDynamicVerticesArray(vertices, indices, primitiveType, primitveCount, startIndex, vertexOffset, maximumIndex, null);
		}

		/// <summary>
		/// <para>Draw dynamic vertices as primitives with extended parametres</para>
		/// <para>NOTE: When using this method, the vertex data will be copied every frame.</para>
		/// <para>This method is best for volatile vertex data that is changing every frame.</para>
		/// <para>Use a dynamic Vertices object for dynamic data that changes less frequently, or requires use with a VerticesGroup objcet.</para>
		/// </summary>
		/// <param name="vertices">source vertex data to use for drawing vertices</param>
		/// <param name="primitiveType">Primitive type to use when drawing the buffer</param>
		/// <param name="primitveCount">The number of primitives to draw</param>
		/// <param name="vertexOffset">Starting offset into the vertex buffer (defaults to the first vertex - 0)</param>
		public void DrawDynamicVertices<VertexType>(VertexType[] vertices, PrimitiveType primitiveType, int primitveCount, int vertexOffset) where VertexType : struct
		{
			DrawDynamicVerticesArray(vertices, null, primitiveType, primitveCount, 0, vertexOffset, 0, null);
		}


		/// <summary>
		/// <para>Draw dynamic vertices as primitives with animation blending and extended parametres, using an index buffer (indices)</para>
		/// <para>NOTE: When using this method, the vertex and index data will be copied every frame.</para>
		/// <para>This method is best for volatile vertex data that is changing every frame.</para>
		/// <para>Use a dynamic Vertices object for dynamic data that changes less frequently, or requires use with a VerticesGroup objcet.</para>
		/// </summary>
		/// <param name="vertices">source vertex data to use for drawing vertices</param>
		/// <param name="indices">indices to use when drawing</param>
		/// <param name="primitiveType">Primitive type to use when drawing the buffer</param>
		/// <param name="primitveCount">The number of primitives to draw</param>
		/// <param name="startIndex">The start index in the index buffer (defaults to the first index - 0)</param>
		/// <param name="animationTransforms">Optional animation transform data for animation blending</param>
		/// <param name="vertexOffset">Starting offset into the vertex buffer (defaults to the first vertex - 0)</param>
		/// <param name="maximumIndex">The maximum index used by the index buffer. This determines how many vertices will be copied (zero will copy all vertices)</param>
		public void DrawDynamicIndexedVerticesBlending<VertexType>(VertexType[] vertices, int[] indices, PrimitiveType primitiveType, Graphics.AnimationTransformArray animationTransforms, int primitveCount, int startIndex, int maximumIndex, int vertexOffset) where VertexType : struct
		{
			DrawDynamicVerticesArray(vertices, indices, primitiveType, primitveCount, startIndex, vertexOffset, maximumIndex, animationTransforms);
		}
		/// <summary>
		/// <para>Draw dynamic vertices as primitives with animation blending and extended parametres, using an index buffer (indices)</para>
		/// <para>NOTE: When using this method, the vertex and index data will be copied every frame.</para>
		/// <para>This method is best for volatile vertex data that is changing every frame.</para>
		/// <para>Use a dynamic Vertices object for dynamic data that changes less frequently, or requires use with a VerticesGroup objcet.</para>
		/// </summary>
		/// <param name="vertices">source vertex data to use for drawing vertices</param>
		/// <param name="indices">indices to use when drawing</param>
		/// <param name="primitiveType">Primitive type to use when drawing the buffer</param>
		/// <param name="primitveCount">The number of primitives to draw</param>
		/// <param name="animationTransforms">Optional animation transform data for animation blending</param>
		/// <param name="startIndex">The start index in the index buffer (defaults to the first index - 0)</param>
		/// <param name="vertexOffset">Starting offset into the vertex buffer (defaults to the first vertex - 0)</param>
		/// <param name="maximumIndex">The maximum index used by the index buffer. This determines how many vertices will be copied (zero will copy all vertices)</param>
		public void DrawDynamicIndexedVerticesBlending<VertexType>(VertexType[] vertices, short[] indices, PrimitiveType primitiveType, Graphics.AnimationTransformArray animationTransforms, int primitveCount, int startIndex, int maximumIndex, int vertexOffset) where VertexType : struct
		{
			DrawDynamicVerticesArray(vertices, indices, primitiveType, primitveCount, startIndex, vertexOffset, maximumIndex, animationTransforms);
		}

		/// <summary>
		/// <para>Draw dynamic vertices as primitives with animation blending and extended parametres</para>
		/// <para>NOTE: When using this method, the vertex data will be copied every frame.</para>
		/// <para>This method is best for volatile vertex data that is changing every frame.</para>
		/// <para>Use a dynamic Vertices object for dynamic data that changes less frequently, or requires use with a VerticesGroup objcet.</para>
		/// </summary>
		/// <param name="vertices">source vertex data to use for drawing vertices</param>
		/// <param name="primitiveType">Primitive type to use when drawing the buffer</param>
		/// <param name="primitveCount">The number of primitives to draw</param>
		/// <param name="animationTransforms">Optional animation transform data for animation blending</param>
		/// <param name="vertexOffset">Starting offset into the vertex buffer (defaults to the first vertex - 0)</param>
		public void DrawDynamicVerticesBlending<VertexType>(VertexType[] vertices, PrimitiveType primitiveType, Graphics.AnimationTransformArray animationTransforms, int primitveCount, int vertexOffset) where VertexType : struct
		{
			DrawDynamicVerticesArray(vertices, null, primitiveType, primitveCount, 0, vertexOffset, 0, animationTransforms);
		}


		private void DrawDynamicVerticesArray<VertexType>(VertexType[] vertices, Array indices, PrimitiveType primitiveType, int primitveCount, int startIndex, int vertexOffset, int maximumIndex, Graphics.AnimationTransformArray animation) where VertexType : struct
		{
			if (DrawTarget == null)
				throw new InvalidOperationException("Vertices Draw calls should be done within the Draw() call of a DrawTarget object. (otherwise the draw target is undefined)");
			if (vertices == null)
				throw new ArgumentException();

			VertexDeclaration vd = application.declarationBuilder.GetDeclaration<VertexType>(graphics);

			int vertexCount = vertices.Length;
			if (indices != null)
				vertexCount = indices.Length;

			int primitives = 0;
			switch (primitiveType)
			{
				case PrimitiveType.LineList:
					primitives = vertexCount / 2;
					break;
				case PrimitiveType.LineStrip:
					primitives = vertexCount - 1;
					break;
				case PrimitiveType.TriangleList:
					primitives = vertexCount / 3;
					break;
				case PrimitiveType.TriangleStrip:
					primitives = vertexCount - 2;
					break;
			}

			renderState.ApplyRenderStateChanges(vertexCount, animation, animation == null ? Xen.Graphics.ShaderSystem.ShaderExtension.None : Xen.Graphics.ShaderSystem.ShaderExtension.Blending);

#if DEBUG
			shaderSystem.ValidateVertexDeclarationForShader(vd, typeof(VertexType));
#endif

			if (primitveCount > primitives ||
				primitveCount <= 0)
				throw new ArgumentException("primitiveCount");

			if (indices != null)
			{
				if (maximumIndex <= 0)
					maximumIndex = vertexCount - 1;

#if DEBUG
				System.Threading.Interlocked.Increment(ref application.currentFrame.DrawIndexedPrimitiveCallCount);
#endif

				if (indices is int[])
					graphics.DrawUserIndexedPrimitives<VertexType>(primitiveType, vertices, vertexOffset, maximumIndex + 1, (int[])indices, startIndex, primitveCount, vd);
				else
					graphics.DrawUserIndexedPrimitives<VertexType>(primitiveType, vertices, vertexOffset, maximumIndex + 1, (short[])indices, startIndex, primitveCount, vd);
			}
			else
			{
#if DEBUG
				System.Threading.Interlocked.Increment(ref application.currentFrame.DrawPrimitivesCallCount);
#endif

				graphics.DrawUserPrimitives<VertexType>(primitiveType, vertices, vertexOffset, primitveCount, vd);
			}

			//draw indexed primitives mucks up the stream settings.
			//Need to clear out the internal tracking data
			indexBuffer = null;

#if DEBUG
			switch (primitiveType)
			{
				case PrimitiveType.LineList:
				case PrimitiveType.LineStrip:
					application.currentFrame.LinesDrawn += primitveCount;
					break;
				case PrimitiveType.TriangleList:
				case PrimitiveType.TriangleStrip:
					application.currentFrame.TrianglesDrawn += primitveCount;
					break;
			}
#endif
		}


		/// <summary>
		/// <para>Draw dynamic vertices as primitives, using an optional index buffer (indices)</para>
		/// <para>NOTE: When using this method, the vertex and index data will be copied every frame.</para>
		/// <para>This method is best for volatile vertex data that is changing every frame.</para>
		/// <para>Use a dynamic Vertices object for dynamic data that changes less frequently, or requires use with a VerticesGroup objcet.</para>
		/// </summary>
		/// <param name="vertices">source vertex data to use for drawing vertices</param>
		/// <param name="indices">indices to use when drawing (may be null)</param>
		/// <param name="primitiveType">Primitive type to use when drawing the buffer</param>
		public void DrawDynamicIndexedVertices<VertexType>(VertexType[] vertices, int[] indices, PrimitiveType primitiveType) where VertexType : struct
		{
			DrawDynamicVerticesArray(vertices, indices, primitiveType);
		}
		/// <summary>
		/// <para>Draw dynamic vertices as primitives, using an optional index buffer (indices)</para>
		/// <para>NOTE: When using this method, the vertex and index data will be copied every frame.</para>
		/// <para>This method is best for volatile vertex data that is changing every frame.</para>
		/// <para>Use a dynamic Vertices object for dynamic data that changes less frequently, or requires use with a VerticesGroup objcet.</para>
		/// </summary>
		/// <param name="vertices">source vertex data to use for drawing vertices</param>
		/// <param name="indices">indices to use when drawing (may be null)</param>
		/// <param name="primitiveType">Primitive type to use when drawing the buffer</param>
		public void DrawDynamicIndexedVertices<VertexType>(VertexType[] vertices, short[] indices, PrimitiveType primitiveType) where VertexType : struct
		{
			DrawDynamicVerticesArray(vertices, indices, primitiveType);
		}

		/// <summary>
		/// <para>Draw dynamic vertices as primitives</para>
		/// <para>NOTE: When using this method, the vertex data will be copied every frame.</para>
		/// <para>This method is best for volatile vertex data that is changing every frame.</para>
		/// <para>Use a dynamic Vertices object for dynamic data that changes less frequently, or requires use with a VerticesGroup objcet.</para>
		/// </summary>
		/// <param name="vertices">source vertex data to use for drawing vertices</param>
		/// <param name="primitiveType">Primitive type to use when drawing the buffer</param>
		public void DrawDynamicVertices<VertexType>(VertexType[] vertices, PrimitiveType primitiveType) where VertexType : struct
		{
			DrawDynamicVerticesArray(vertices, null, primitiveType);
		}

		private void DrawDynamicVerticesArray<VertexType>(VertexType[] vertices, Array indices, PrimitiveType primitiveType) where VertexType : struct
		{
			if (DrawTarget == null)
				throw new InvalidOperationException("Vertices Draw calls should be done within the Draw() call of a DrawTarget object. (otherwise the draw target is undefined)");
			if (vertices == null)
				throw new ArgumentException();

			VertexDeclaration vd = application.declarationBuilder.GetDeclaration<VertexType>(graphics);

			int vertexCount = vertices.Length;
			if (indices != null)
				vertexCount = indices.Length;

			int primitives = 0;
			switch (primitiveType)
			{
				case PrimitiveType.LineList:
					primitives = vertexCount / 2;
					break;
				case PrimitiveType.LineStrip:
					primitives = vertexCount - 1;
					break;
				case PrimitiveType.TriangleList:
					primitives = vertexCount / 3;
					break;
				case PrimitiveType.TriangleStrip:
					primitives = vertexCount - 2;
					break;
			}

			renderState.ApplyRenderStateChanges(vertexCount, null, Xen.Graphics.ShaderSystem.ShaderExtension.None);

#if DEBUG
			shaderSystem.ValidateVertexDeclarationForShader(vd, typeof(VertexType));
#endif

			if (indices != null)
			{
#if DEBUG
				System.Threading.Interlocked.Increment(ref application.currentFrame.DrawIndexedPrimitiveCallCount);
#endif

				if (indices is int[])
					graphics.DrawUserIndexedPrimitives<VertexType>(primitiveType, vertices, 0, vertices.Length, (int[])indices, 0, primitives, vd);
				else
					graphics.DrawUserIndexedPrimitives<VertexType>(primitiveType, vertices, 0, vertices.Length, (short[])indices, 0, primitives, vd);
			}
			else
			{
#if DEBUG
				System.Threading.Interlocked.Increment(ref application.currentFrame.DrawPrimitivesCallCount);
#endif

				graphics.DrawUserPrimitives<VertexType>(primitiveType, vertices, 0, primitives, vd);
			}

			//draw indexed primitives mucks up the stream settings.
			//Need to clear out the internal tracking data
			indexBuffer = null;

#if DEBUG
			switch (primitiveType)
			{
				case PrimitiveType.LineList:
				case PrimitiveType.LineStrip:
					application.currentFrame.LinesDrawn += primitives;
					break;
				case PrimitiveType.TriangleList:
				case PrimitiveType.TriangleStrip:
					application.currentFrame.TrianglesDrawn += primitives;
					break;
			}
#endif
		}
	}
}