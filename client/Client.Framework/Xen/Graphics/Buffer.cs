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
	//base class for index/vertex internal data buffers
#if !DEBUG_API
	[System.Diagnostics.DebuggerStepThrough]
#endif
	abstract class Bufffer<DataType> : IDisposable
		where DataType : struct
	{
		private IEnumerable<DataType> data;
		private bool isDisposed;
		private static int typeStride;
		private int count = -1, instanceStride = typeStride;
		private bool isList;
		private List<DirtyRange> dirtyRanges;

		private sealed class GeometryBuffer
		{
			const int BufferSize = 32;
			public static DataType[] Buffer = new DataType[BufferSize];
		}

		static Bufffer()
		{
			typeStride = System.Runtime.InteropServices.Marshal.SizeOf(typeof(DataType));
			ValidateType(typeof(DataType));
		}

		private static void ValidateType(Type t)
		{
			if (t.IsClass)
				throw new ArgumentException(string.Format("Type '{0}' contains non-valuetype member(s)", typeof(DataType)));

			foreach (FieldInfo f in t.GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public))
				if (f.FieldType != t)
					ValidateType(f.FieldType);
		}


#if !DEBUG_API
		[System.Diagnostics.DebuggerStepThrough]
#endif
		private struct DirtyRange
		{
			public DirtyRange(int start, int length) { this.length = length; this.start = start; }
			public int start, length;
			public bool Merge(DirtyRange range)
			{
				DirtyRange r = this;

				if (range.start <= start + length &&
					start <= range.start + range.length)
				{
					int end = Math.Max(this.start + this.length, range.start + range.length);
					this.start = Math.Min(this.start, range.start);
					this.length = end - this.start;
					return true;
				}
				return false;
			}
		}

		public bool IsDirty
		{
			get { return dirtyRanges != null && dirtyRanges.Count > 0; }
		}

		public void UpdateDirtyRegions(Application application, object target, object parent)
		{
			int writeRange = -1;
			foreach (DirtyRange range in dirtyRanges)
			{
				if ((range.start == 0 && range.length == Count) || !isList)
				{
					//write buffer from the start
					writeRange = Math.Max(writeRange, (range.length + range.start) * Stride);
				}
				else
					WriteBuffer(application, range.start, range.length * Stride, target, null);
			}

			if (writeRange != -1)
				WriteBuffer(application, 0, writeRange, target, null);

			dirtyRanges.Clear();
		}

		public void ClearDirtyRange()
		{
			if (dirtyRanges != null)
				dirtyRanges.Clear();
		}

		public Bufffer(IEnumerable<DataType> data)
		{
			if (data == null)
				throw new ArgumentNullException();

			this.data = data;

			count = GetCount();
		}

		public Bufffer(int count)
		{
			this.count = count;
		}

		internal int GetCount()
		{
			IEnumerable<DataType> data = this.data;

			if (data is ICollection<DataType>)
				return (data as ICollection<DataType>).Count;
			if (data is ICollection)
				return (data as ICollection).Count;
			if (data is Array)
				return (data as Array).Length;

			return -1;
		}

		public bool CountKnown
		{
			get
			{
				return count != -1;
			}
		}

		public void ClearBuffer()
		{
			data = null;
		}

		public IEnumerable<DataType> Data
		{
			get { return data; }
		}

		public bool Disposed
		{
			get
			{
				return isDisposed;
			}
		}

		public void Dispose()
		{
			data = null;
			isDisposed = true;
		}

		public void AddDirtyRange(int startIndex, int count, Type sourceType, bool fillEntireRange)
		{
			if ((startIndex < 0 || count <= 0) && !fillEntireRange)
				throw new ArgumentException("Invalid range specified");

			IEnumerable<DataType> data = this.data;

			//if (data == null)
			//    throw new InvalidOperationException("Cannot set a dirty region when source data has been garbage collected");

			isList = true;

			if (data is IList == false)
			{
				if (data is ICollection == false || !(startIndex == 0 && count == (data as ICollection).Count && !fillEntireRange))
				{
					if (!(data is ICollection == false && data is IEnumerable && fillEntireRange))
						throw new InvalidOperationException(sourceType.Name + "<" + sourceType.GetGenericArguments()[0].Name + "> source data must implement the IList<" + sourceType.GetGenericArguments()[0].Name + "> interface to set a dirty subrange");
				}
				else
				{
					if (data is ICollection && (startIndex == 0 && count == (data as ICollection).Count))
						fillEntireRange = true;
				}
				isList = false;
			}

			IList dataList = data as IList;

			if (!fillEntireRange && ((startIndex + count) > dataList.Count))
				throw new ArgumentException("Invalid range specified");

			if (this.dirtyRanges == null)
				this.dirtyRanges = new List<DirtyRange>();

			if (fillEntireRange)
			{
				dirtyRanges.Clear();
				if (data is ICollection)
					dirtyRanges.Add(new DirtyRange(0, (data as ICollection).Count));
				else
					dirtyRanges.Add(new DirtyRange(0, -1));//special case as length is not yet known
			}
			else
			{
				DirtyRange range = new DirtyRange(startIndex, count);

				for (int i = 0; i < dirtyRanges.Count; i++)
					if (dirtyRanges[i].Merge(range))
						return;

				dirtyRanges.Add(range);
			}
		}

		public int Count
		{
			get
			{
				if (count == -1)
					throw new ArgumentException("Count is not available until the data has been processed");
				return count;
			}
		}

		public Type Type
		{
			get { return typeof(DataType); }
		}

		public int Stride
		{
			get
			{
				return instanceStride;
			}
		}

		internal void OverrideSize(int stride, int count)
		{
			this.count = count;
			this.instanceStride = stride;
		}

		protected abstract void WriteBlock(Application app, DataType[] data, int sourceStartIndex, int writeOffsetBytes, int copyElements, object target);
		protected abstract void WriteComplete();

		public int WriteBuffer(Application app, int startIndex, int bytesToWrite, object target, DataType[] source)
		{
			DataType[] data = source ?? (this.data as DataType[]);

			//source data is an array, easy copy
			if (data != null)
			{

				WriteBlock(app, data, startIndex, startIndex * instanceStride, bytesToWrite / instanceStride, target);
				WriteComplete();

				return bytesToWrite / instanceStride;
			}

			DataType[] writeBuffer = null;
			//not an array, is the count known?
			if (count != -1)
			{
				//it is.. try and make a copy first
				try
				{
					writeBuffer = new DataType[bytesToWrite / instanceStride];

					IList<DataType> list = this.data as IList<DataType>;
					//easy to copy an IList
					if (startIndex == 0 && writeBuffer.Length == count && list != null)
						list.CopyTo(writeBuffer, 0);
					else
					{
						//not so easy
						int inc = 0;
						int offset = startIndex;
						int range = bytesToWrite / instanceStride;

						foreach (DataType item in this.data)
						{
							if (offset-- > 0)
								continue;
							if (--range < 0)
								break;
							writeBuffer[inc++] = item;
						}
					}
				}
				catch (OutOfMemoryException)
				{
					//couldn't allocate big enough copy array, so do it bit by bit
					writeBuffer = null;
				}

				if (writeBuffer != null)
				{
					WriteBlock(app, writeBuffer, 0, startIndex * instanceStride, bytesToWrite / instanceStride, target);
					WriteComplete();
					return bytesToWrite / instanceStride;
				}
			}

			//copy block by block
			writeBuffer = GeometryBuffer.Buffer;

			lock (writeBuffer)
			{
				int inc = 0;
				int written = 0;
				int offset = startIndex;
				int range = bytesToWrite / instanceStride;
				int loopCount = 0;

				foreach (DataType item in this.data)
				{
					loopCount++;

					if (offset-- > 0)
						continue;
					if (--range < 0)
					{
						if (count == -1)
							continue;
						break;
					}

					writeBuffer[inc++] = item;

					if (inc == writeBuffer.Length)
					{
						WriteBlock(app, writeBuffer, 0, startIndex * instanceStride, inc, target);

						startIndex += inc;
						written += writeBuffer.Length;
						inc = 0;
					}
				}
				if (inc != 0)
				{
					WriteBlock(app, writeBuffer, 0, startIndex * instanceStride, inc, target);
					written += inc;
				}
				if (count == -1)
					count = loopCount;
				WriteComplete();
				return written;
			}
		}


	}
}