using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using Microsoft.Xna.Framework.Graphics;

namespace Xen.Graphics
{
	/// <summary>
	/// Flags that can specify resource usage for <see cref="IVertices">Vertex Buffers</see> and <see cref="IIndices">Index Buffers</see>.
	/// </summary>
	/// <remarks>The <see cref="ResourceUsage.Dynamic"/> flag allows for using the SetDirtyRange methods of <see cref="IVertices.SetDirtyRange">Vertex Buffers</see> and <see cref="IIndices.SetDirtyRange">Index Buffers</see></remarks>
	[Flags]
	public enum ResourceUsage : byte
	{
		/// <summary>
		/// Default usage
		/// </summary>
		None = 0,
		/// <summary>
		/// Buffer will be allocated in a way optimised for content that changes frequently
		/// </summary>
		Dynamic = 1,
		/// <summary>
		/// [Use with caution] Buffer is allocated as <see cref="Dynamic"/>. All data writes are assumed to be non-overlapping and performed in order starting from zero. (See remarks for details)
		/// </summary>
		/// <remarks>
		/// <para>This flag is the equivalent of using <see cref="SetDataOptions.Discard"/> for any data changes starting at zero, and <see cref="SetDataOptions.NoOverwrite"/> for the rest.</para>
		/// <para>Hence it is a requirement that data copies be done in-order, and always starting from zero for their use order.</para>
		/// <para>The way to use this flag is with a large buffer that gets small amounts of data written over a number of frames (with each small block of data  not overwriting previous, and writes being performed in order starting from zero).</para>
		/// <para>NOTE: Misuse of this flag can cause unexpected behaviour. What you are seeing on screen can be upto 5 frames behind what the CPU is processing. The driver will assume buffer writes not starting at zero are copying to memory that the application is no longer using. Hence misuse of this flag makes it possible to have the CPU write data to the GPU that will then get used to draw a <i>previous frame</i>. If such a case occurs without using this flag, the CPU will wait for the GPU to catch up, so the copy does not overwrite data that may be in use (this can cause a significant pipeline stall)</para>
		/// <para>IMPORTANT: Also note, due to the usage cases for DynamicSequential, when the buffer is created it's data will not be automatically filled. A call to SetDirtyRange() will be required first.</para>
		/// </remarks>
		DynamicSequential = 3,
		/// <summary>
		/// The buffer will be Read/Write. This flag is required for calling <see cref="IVertices.TryExtractVertexData"/>
		/// </summary>
		Readable = 4
	}

	/// <summary>
	/// Base class for all implemented objects that use graphics resources
	/// </summary>
#if !DEBUG_API
	[System.Diagnostics.DebuggerStepThrough]
#endif
	public abstract class Resource
	{

		/// <summary></summary>
		public enum ResourceType
		{
			/// <summary></summary>
			All,
			/// <summary></summary>
			RenderTarget,
			/// <summary></summary>
			VertexBuffer,
			/// <summary></summary>
			IndexBuffer
		}

		static List<WeakReference> resources = new List<WeakReference>();
		static List<Resource> localResourceList = new List<Resource>();
		static List<WeakReference> localRefList = new List<WeakReference>();
		static Stack<WeakReference> nullReferences = new Stack<WeakReference>();
		static bool resourceCreated = false;
#if DEBUG
		static bool monitoringEnabled = true;
#else
		static bool monitoringEnabled = false;
#endif

		/// <summary></summary>
		internal Resource()
		{
			resourceCreated = true;
			if (monitoringEnabled)
			{
				lock (resources)
				{
					WeakReference wr = null;
					if (nullReferences.Count > 0)
						wr = nullReferences.Pop();
					else
						wr = new WeakReference(null);
					wr.Target = this;
					resources.Add(wr);
				}
			}
		}

		/// <summary>
		/// Call this to enable resource tracking (Note: Resource Tracking is always on in DEBUG builds)
		/// </summary>
		/// <remarks>This method must be called before or during constructing the application instance</remarks>
		public static void EnableReleaseResourceTracking()
		{
			if (resourceCreated && !monitoringEnabled)
				throw new InvalidOperationException("EnableResourceTracking can only be called before any resources have been created");
			monitoringEnabled = true;
		}
		/// <summary>
		/// Call this to enable resource tracking (Note: Resource Tracking is always on in DEBUG builds)
		/// </summary>
		/// <remarks>This method must be called before or during constructing the application instance</remarks>
		/// <param name="throwOnError"></param>
		public static bool EnableReleaseResourceTracking(bool throwOnError)
		{
			if (resourceCreated && !monitoringEnabled)
			{
				if (throwOnError)
					EnableReleaseResourceTracking();
				return false;
			}
			monitoringEnabled = true;
			return true;
		}

		internal static void ClearResourceTracking()
		{
			resources.Clear();
			localResourceList.Clear();
			localRefList.Clear();
			nullReferences.Clear();
			monitoringEnabled = false;
			resourceCreated = false;
		}

		/// <summary>
		/// Returns the number of resource objects that are still being referenced
		/// </summary>
		/// <returns></returns>
		public static int CountDisposedResourcesStillAlive()
		{
			int count = 0;
			lock (resources)
			{
				GetResources(localResourceList);
				foreach (Resource r in localResourceList)
				{
					if (r.IsDisposed)
						count++;
				}
				localResourceList.Clear();
			}
			return count;
		}
		/// <summary>
		/// Returns the number of resource objects that are still being referenced
		/// </summary>
		/// <returns></returns>
		public static int CountDisposedResourcesStillAlive(ResourceType filter)
		{
			int count = 0;
			lock (resources)
			{
				GetResources(localResourceList);
				foreach (Resource r in localResourceList)
				{
					if (r.IsDisposed && r.GraphicsResourceType == filter)
						count++;
				}
				localResourceList.Clear();
			}
			return count;
		}

		/// <summary>
		/// Returns the number of resources that are yet to be used
		/// </summary>
		/// <returns></returns>
		public static int CountResourcesNotUsedByDevice()
		{
			int count = 0;
			lock (resources)
			{
				GetResources(localResourceList);
				foreach (Resource r in localResourceList)
				{
					if (r.IsDisposed == false && r.InUse == false)
						count++;
				}
				localResourceList.Clear();
			}
			return count;
		}
		/// <summary>
		/// Returns the number of resources that are yet to be used
		/// </summary>
		/// <returns></returns>
		public static int CountResourcesNotUsedByDevice(ResourceType filter)
		{
			int count = 0;
			lock (resources)
			{
				GetResources(localResourceList);
				foreach (Resource r in localResourceList)
				{
					if (r.IsDisposed == false && r.InUse == false && r.GraphicsResourceType == filter)
						count++;
				}
				localResourceList.Clear();
			}
			return count;
		}

		/// <summary>
		/// Gets approximate allocation size in managed memory for all resources
		/// </summary>
		/// <returns></returns>
		public static int GetAllAllocatedManagedBytes()
		{
			return GetAllAllocatedManagedBytes(ResourceType.All);
		}

		/// <summary>
		/// Gets approximate allocation size in graphics memory for all resources
		/// </summary>
		/// <returns></returns>
		public static int GetAllAllocatedDeviceBytes()
		{
			return GetAllAllocatedDeviceBytes((ResourceType)0);
		}

		/// <summary>
		/// Gets approximate allocation size in managed memory for all resources of a specific type
		/// </summary>
		/// <returns></returns>
		/// <param name="filter"></param>
		public static int GetAllAllocatedManagedBytes(ResourceType filter)
		{
			int bytes = 0;
			lock (localResourceList)
			{
				GetResources(localResourceList);
				foreach (Resource r in localResourceList)
				{
					if (r.GraphicsResourceType == filter)
						bytes += r.GetAllocatedManagedBytes();
				}
				localResourceList.Clear();
			}
			return bytes;
		}
		/// <summary>
		/// Gets approximate allocation size in graphics memory for all resources of a specific type
		/// </summary>
		/// <returns></returns>
		/// <param name="filter"></param>
		public static int GetAllAllocatedDeviceBytes(ResourceType filter)
		{
			int bytes = 0;
			lock (localResourceList)
			{
				GetResources(localResourceList);
				foreach (Resource r in localResourceList)
				{
					if (r.GraphicsResourceType == filter || filter == (ResourceType)0)
						bytes += r.GetAllocatedDeviceBytes();
				}
				localResourceList.Clear();
			}
			return bytes;
		}
		/// <summary>
		/// Fills a dictionay with the count of each resource type
		/// </summary>
		/// <returns></returns>
		public static void GetResourceCountByType(IDictionary<Type,int> resourceCounts)
		{
			if (resourceCounts == null)
				throw new ArgumentNullException();

			lock (localResourceList)
			{
				GetResources(localResourceList);
				foreach (Resource r in localResourceList)
				{
					Type type = r.GetType();
					int count;
					if (resourceCounts.TryGetValue(type,out count))
						resourceCounts[type] = count + 1;
					else
						resourceCounts.Add(type,1);
				}
				localResourceList.Clear();
			}
		}
		/// <summary>
		/// Gets the number of resources tracked
		/// </summary>
		/// <returns></returns>
		public static int GetResourceCount()
		{
			lock (localResourceList)
			{
				GetResources(localResourceList);
				int count = localResourceList.Count;
				localResourceList.Clear();
				return count;
			}
		}
		/// <summary>
		/// Gets the number of resources tracked
		/// </summary>
		/// <returns></returns>
		public static int GetResourceCount(ResourceType filter)
		{
			lock (localResourceList)
			{
				GetResources(localResourceList);
				int count = 0;
				foreach (Resource res in localResourceList)
				{
					if (res.GraphicsResourceType == filter)
						count++;
				}
				localResourceList.Clear();
				return count;
			}
		}

		private static void GetResources(List<Resource> resourceList)
		{
			if (!monitoringEnabled)
				throw new ArgumentException("Resource Tracking is not enabled");

			lock (resources)
			{
				foreach (WeakReference wr in resources)
				{
					Resource target = wr.Target as Resource;
					if (target != null)
					{
						resourceList.Add(target);
						localRefList.Add(wr);
					}
					else
					{
						wr.Target = null;
						nullReferences.Push(wr);
					}
				}
				if (localRefList.Count != resources.Count)
				{
					resources.Clear();
					resources.AddRange(localRefList);
				}
				localRefList.Clear();
			}
		}

		internal abstract int GetAllocatedManagedBytes();
		internal abstract int GetAllocatedDeviceBytes();
		internal abstract ResourceType GraphicsResourceType { get;}
		internal abstract bool InUse { get; }
		internal abstract bool IsDisposed { get; }

		/// <summary>
		/// Preload (warm) the resource before its first use
		/// </summary>
		/// <param name="state"></param>
		public void Warm(IState state)
		{
			Warm(state.Application);
		}
		/// <summary>
		/// Preload (warm) the resource before its first use
		/// </summary>
		/// <param name="application"></param>
		public void Warm(Application application)
		{
			lock (this)
				Warm(application,application.GraphicsDevice);
		}
		internal abstract void Warm(Application application, GraphicsDevice state);
	}
}
