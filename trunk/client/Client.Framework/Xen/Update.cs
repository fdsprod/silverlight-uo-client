using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Xen
{
	/// <summary>
	/// UpdateManager stores a list of <see cref="IUpdate"/> object instances and manages calling <see cref="IUpdate.Update(UpdateState)"/>
	/// </summary>
	/// <remarks><para>An update manager may be added to another update manager (as it implements IUpdate), this can make separating application logic easier.</para></remarks>
#if !DEBUG_API
	[System.Diagnostics.DebuggerStepThrough]
#endif
	public sealed class UpdateManager : IUpdate, IDisposable
	{
		private static readonly List<UpdateFrequency> frequencyValues;
		private static readonly List<object[]> frequencyAttributes;
		private bool enabled = true;

		/// <summary>
		/// Gets/Sets if this update manager is enabled. When disabled, child items will not be updated.
		/// </summary>
		/// <remarks><para>As update managers can be nested within eachother, Disabling an entire update manager can be an effective way to disable large portions of a games logic structure.</para></remarks>
		public bool Enabled
		{
			get { return enabled; }
			set { enabled = value; }
		}


		static UpdateManager()
		{
			System.Reflection.FieldInfo[] enums = typeof(UpdateFrequency).GetFields(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);

			frequencyValues = new List<UpdateFrequency>();// new UpdateFrequency[enums.Length];
			frequencyAttributes = new List<object[]>();// new object[enums.Length][];
			for (int i = 0; i < enums.Length; i++)
			{
				UpdateFrequency freq = (UpdateFrequency)enums[i].GetValue(null);
				object[] atts = enums[i].GetCustomAttributes(false);

				if (atts.Length > 0)
				{
					frequencyValues.Add(freq);
					frequencyAttributes.Add(atts);
				}
			}
		}

		private Updater[][] updaters;
		private readonly int[] indexes;
		private readonly int[] frameUpdateIndex;
		private int interval = 0;
		private bool updating;
		private readonly List<UpdateEntry>[] moveList;
		//bool indicates if the item should be sequentially distributed
		private readonly List<KeyValuePair<IUpdate, bool>> addList = new List<KeyValuePair<IUpdate, bool>>();
		private readonly Dictionary<IUpdate, UpdateEntry> updateList = new Dictionary<IUpdate, UpdateEntry>();
		private readonly object syncMinor = new object();
		private long updateTimer, totalTime;
		private bool pauseIfAppInactive;
		private float updateSpeed = 1;

		/// <summary>
		/// <para>If true, the update manager will pause all updating when <see cref="Application.IsActive"/> is false</para>
		/// <para>(Items updating at <see cref="UpdateFrequency.OncePerFrame"/> will still update as normal)</para>
		/// </summary>
		public bool PauseUpdatesWhenApplicationIsInactive
		{
			get { return pauseIfAppInactive; }
			set { pauseIfAppInactive = value; }
		}

		/// <summary>
		/// <para>Update Speed scale factor, 1.0 represents full update speed, 0.5 represents halved update speed, etc.</para>
		/// </summary>
		public float UpdateSpeed
		{
			get { return updateSpeed; }
			set { if (value < 0) throw new ArgumentException(); updateSpeed = value; }
		}

		/// <summary>
		/// Construct the update manager
		/// </summary>
		public UpdateManager()
		{
			updaters = new Updater[frequencyValues.Count][];
			indexes = new int[updaters.Length];
			moveList = new List<UpdateEntry>[updaters.Length];
			for (int i = 0; i < updaters.Length; i++)
				moveList[i] = new List<UpdateEntry>();

			frameUpdateIndex = new int[updaters.Length];

			int frequencyIndex = 0;
			foreach (UpdateFrequency frequency in frequencyValues)
			{
				foreach (object att in frequencyAttributes[frequencyIndex++])
				{
					if (att is UpdateFrequencyAttribute)
					{
						int interval = (att as UpdateFrequencyAttribute).Interval;
						bool async = (att as UpdateFrequencyAttribute).Async;

						updaters[(int)frequency] = new Updater[interval];

						for (int i = 0; i < interval; i++)
							updaters[(int)frequency][i] = new Updater(frequency, interval, i, updateList, syncMinor, async);
					}
				}
			}
		}

		/// <summary>
		/// Add an item to the update manager in direct order (See <see cref="Add(IUpdate,bool)"/> remarks for more information)
		/// </summary>
		/// <param name="update"></param>
		/// <remarks>See <see cref="Add(IUpdate,bool)"/> remarks for more information</remarks>
		public void Add(IUpdate update)
		{
			Monitor.Enter(syncMinor);

			addList.Add(new KeyValuePair<IUpdate,bool>(update,false));

			Monitor.Exit(syncMinor);
		}

		/// <summary>
		/// Add an item to the update manager either sequential order or directly. See remarks for details
		/// </summary>
		/// <param name="update"></param>
		/// <param name="seqentialDistribution">See remarks for details</param>
		/// <remarks>
		/// <para>
		/// When an item is added to an UpdateManager, it will be put in a temporary list. When main updating completes items in this list will have their <see cref="IUpdate.Update"/> method called. This will determine their desired <see cref="UpdateFrequency"/> so they can be inserted into the main update list.
		/// </para>
		/// <para>Therefore if any items are created before or during the update manager's update loop, those items will also have <see cref="IUpdate.Update"/> called in that frame (once all other updating has completed).</para>
		/// <para>The next time they have Update called, they will be in the main list of items.</para>
		/// <para>Because many items are often inserted into the update manager at once, and because items may not require they have Update called every update frame (usually for performance reasons), it is possible that the items updating at (for example) 5 times per second will mostly be at the same timestep, and not evenly distributed.</para>
		/// <para>By specifying <paramref name="seqentialDistribution"/> as true, the items will be inserted into the manager such that they have Update called on differet frames. If 12 items are added at 5hz, then the first item will Update on the first frame, the second on the second frame, and so on. Because there are 12 items, and 60 frames, there should be no frame where two or more items have Update called. This should help to ballance CPU load.</para>
		/// <para>However, this system only places items in order by sequency, and does not do more intelligent placement (eg, inserting in the shortest list). The second time the item has Update() called the <see cref="UpdateState.DeltaTimeSeconds"/> will be incorrect.</para>
		/// <para>SeqentialDistribution only affects adding the item, it does not change the logic for when an item changes it's updat frequency</para>
		/// </remarks>
		public void Add(IUpdate update, bool seqentialDistribution)
		{
			Monitor.Enter(syncMinor);

			addList.Add(new KeyValuePair<IUpdate, bool>(update, seqentialDistribution));

			Monitor.Exit(syncMinor);
		}

		/// <summary>
		/// Remove an item from the update manager. Returning <see cref="UpdateFrequency.Terminate"/> from an item's <see cref="IUpdate.Update"/> implementation will also remove the item.
		/// </summary>
		/// <param name="update"></param>
		/// <returns>returns true if the item was removed</returns>
		public bool Remove(IUpdate update)
		{
			Monitor.Enter(syncMinor);
			UpdateEntry index;

			if (updateList.TryGetValue(update,out index))
			{
				index.updater.RemoveAt(index);
			}

			Monitor.Exit(syncMinor);
			return index != null;
		}

		UpdateFrequency IUpdate.Update(UpdateState state)
		{
			if (totalTime == 0)
				totalTime = state.TotalTimeTicks;

			if (updaters == null)
				return UpdateFrequency.Terminate;

			if (!enabled)
				return UpdateFrequency.OncePerFrame;

			UpdateManager parent = state.UpdateManager;
			state.UpdateManager = this;
			updating = true;

			//update at exactly 60hz
			if (!pauseIfAppInactive || state.Application.IsActive)
				updateTimer += state.DeltaTimeTicks;

			long delta = long.MaxValue;
			if (updateSpeed != 0)
				delta = Convert.ToInt64((double)UpdateState.TicksInOneSecond / (60.0 * updateSpeed));

			bool resetUpdate = false;

			float currentHz = state.DeltaTimeFrequency;
			float currentDts = state.DeltaTimeSeconds;
			float currentSeconds = state.TotalTimeSeconds;
			long currentDeltaTime = state.DeltaTimeTicks;
			long currentTotalTime = state.TotalTimeTicks;


			int count = 0;
			while (updateTimer >= delta)
			{
				count++;

				resetUpdate = true;
				updateTimer -= delta;
				float deltaSeconds = (float)(delta / (double)UpdateState.TicksInOneSecond);
				float totalSeconds = (float)(totalTime / (double)UpdateState.TicksInOneSecond);

				state.Update(0, 0, totalSeconds, delta, totalTime);

				for (int i = 0; i < updaters.Length; i++)
				{
					if (i == (int)UpdateFrequency.OncePerFrame ||
						i == (int)UpdateFrequency.OncePerFrameAsync)
						continue;

					Updater updater = updaters[i][frameUpdateIndex[i]];

					if (updater != null)
					{
						long localDelta = delta * (long)updater.Interval;
						state.UpdateDelta(deltaSeconds * updater.IntervalF, updater.Hz, localDelta);

						updater.Update(interval, state, this.moveList);
					}
				}

				for (int i = 0; i < moveList.Length; i++)
				{
					if (moveList[i].Count > 0)
					{
						foreach (UpdateEntry update in moveList[i])
							MoveItem(update, (UpdateFrequency)i);
						moveList[i].Clear();
					}
				}


				for (int i = 0; i < frameUpdateIndex.Length; i++)
				{
					if (i == (int)UpdateFrequency.OncePerFrame ||
						i == (int)UpdateFrequency.OncePerFrameAsync)
						continue;

					frameUpdateIndex[i]++;
					if (frameUpdateIndex[i] == updaters[i].Length)
						frameUpdateIndex[i] = 0;
				}


				if (addList.Count > 0)
				{
					state.UpdateDelta(0, 0, 0);

					for (int i = 0; i < addList.Count; i++)
					{
						UpdateFrequency frequency = addList[i].Key.Update(state);
						if (frequency != UpdateFrequency.Terminate)
							AddItem(addList[i].Key, frequency, addList[i].Value);
					}

					addList.Clear();

					resetUpdate = true;
				}

				totalTime += delta;
				interval++;
			}

			if (resetUpdate)
			{
				state.Update(currentDts, currentHz, currentSeconds, currentDeltaTime, currentTotalTime);
			}

			//update the OncePerFrame
			{
				Updater updater = updaters[(int)UpdateFrequency.OncePerFrame][0];
				if (updater != null)
					updater.Update(interval, state, this.moveList);

				updater = updaters[(int)UpdateFrequency.OncePerFrameAsync][0];
				if (updater != null)
					updater.Update(interval, state, this.moveList);

				for (int i = 0; i < moveList.Length; i++)
				{
					if (moveList[i].Count > 0)
					{
						foreach (UpdateEntry update in moveList[i])
							MoveItem(update, (UpdateFrequency)i);
						moveList[i].Clear();
					}
				}

			}

			if (addList.Count > 0)
			{
				state.UpdateDelta(0, 0, 0);

				for (int i = 0; i < addList.Count; i++)
				{
					UpdateFrequency frequency = addList[i].Key.Update(state);
					if (frequency != UpdateFrequency.Terminate)
						AddItem(addList[i].Key, frequency, addList[i].Value);
				}

				state.Update(currentDts, currentHz, currentSeconds, currentDeltaTime, currentTotalTime);
				addList.Clear();
			}

			state.UpdateManager = parent;
			updating = false;

			return UpdateFrequency.OncePerFrame;
		}

		private void AddItem(IUpdate update, UpdateFrequency frequency, bool sequential)
		{
			int index = (int)frequency;
			Updater[] list = updaters[index];
			if (sequential)
				list[(indexes[index]++) % list.Length].Add(update);
			else
				list[frameUpdateIndex[index]].Add(update);
		}
		private void MoveItem(UpdateEntry update, UpdateFrequency frequency)
		{
			int index = (int)frequency;
			Updater[] list = updaters[index];

			list[frameUpdateIndex[index]].AddMovedItem(update);
		}

		/// <summary>
		/// Remove all items from the update manager
		/// </summary>
		public void RemoveAll()
		{
			lock (syncMinor)
			{
				if (updating)
					throw new InvalidOperationException("UpdaterManager is currently updating");

				updateList.Clear();
				for (int i = 0; i < this.moveList.Length; i++)
					this.moveList[i].Clear();
				this.addList.Clear();
				for (int i = 0; i < updaters.Length; i++)
					for (int n = 0; n < updaters[i].Length; n++)
						updaters[i][n].RemoveAll();
			}
		}

		/// <summary>
		/// Dispose the update manager
		/// </summary>
		public void Dispose()
		{
			lock (syncMinor)
			{
				if (updaters != null)
					RemoveAll();
				updaters = null;
			}
		}
	}

#if !DEBUG_API
	[System.Diagnostics.DebuggerStepThrough]
#endif
	sealed class UpdateFrequencyAttribute : Attribute
	{
		private int interval;

		public int Interval
		{
			get { return interval; }
			set { interval = value; }
		}

		private bool async;

		public bool Async
		{
			get { return async; }
			set { async = value; }
		}
	}

#if !DEBUG_API
	[System.Diagnostics.DebuggerStepThrough]
#endif
	sealed class UpdateEntry
	{
		public UpdateEntry(IUpdate item, Updater updater, int index)
		{
			this.item = item;
			this.updater = updater;
			this.index = index;
		}
		public readonly IUpdate item;
		public Updater updater;
		public int index;
	}

#if !DEBUG_API
	[System.Diagnostics.DebuggerStepThrough]
#endif
	sealed class Updater
	{
		struct Action
		{
			public Action(object item, bool add)
			{
				this.item = item;
				this.add = add;
			}
			public object item;
			public bool add;
		}

		private readonly int interval, index, hz;
		private UpdateEntry[] entries = new UpdateEntry[8];
		private int entriesHighIndex = 0;
		private readonly List<Action> actionList = new List<Action>();
		private readonly Dictionary<IUpdate, UpdateEntry> indices;
		private readonly object syncMajor = new object();
		private readonly object syncMinor;
		private readonly UpdateFrequency frequncy;
		private readonly float intervalF;
		private bool iteratingList = false;
		private bool async = false;


		public Updater(UpdateFrequency frequncy, int interval, int index, Dictionary<IUpdate, UpdateEntry> indexer, object syncMinor, bool async)
		{
			this.async = async;
			this.intervalF = (float)interval;
			this.syncMinor = syncMinor;
			this.indices = indexer;
			this.hz = 60 / interval;

			this.index = index;
			this.frequncy = frequncy;
			this.interval = interval;
		}

		public void RemoveAll()
		{
			this.entries = new UpdateEntry[8];
			this.entriesHighIndex = 0;
			this.actionList.Clear();
		}

		public UpdateFrequency UpdateFrequency
		{
			get { return frequncy; }
		}
		public float IntervalF
		{
			get { return intervalF; }
		}
		public int Interval
		{
			get { return interval; }
		}
		public int Hz
		{
			get { return hz; }
		}


		public void Add(IUpdate update)
		{
			bool enter = Monitor.TryEnter(syncMajor);
			if (enter && !iteratingList)
				AddItem(update);
			else
				actionList.Add(new Action(update, true));
			if (enter)
				Monitor.Exit(syncMajor);
		}
		public void Remove(IUpdate update)
		{
			bool enter = Monitor.TryEnter(syncMajor);
			if (enter && !iteratingList)
				RemoveItem(update);
			else
				actionList.Add(new Action(update, false));
			if (enter)
				Monitor.Exit(syncMajor);
		}
		public void RemoveAt(UpdateEntry entry)
		{
			bool enter = Monitor.TryEnter(syncMajor);
			if (enter && !iteratingList)
				RemoveIndex(entry);
			else
				actionList.Add(new Action(entry, false));
			if (enter)
				Monitor.Exit(syncMajor);
		}
		//must be locked to sync
		private void AddItem(IUpdate update)
		{
			if (!indices.ContainsKey(update))
			{
				if (entriesHighIndex == entries.Length)
					Array.Resize(ref entries, entries.Length * 2);
				UpdateEntry entry = new UpdateEntry(update, this, entriesHighIndex);
				indices.Add(update, entry);
				entries[entriesHighIndex++] = entry;
			}
		}
		public void AddMovedItem(UpdateEntry entry)
		{
			if (entriesHighIndex == entries.Length)
				Array.Resize(ref entries, entries.Length * 2);
			entry.index = entriesHighIndex;
			entry.updater = this;
			entries[entriesHighIndex++] = entry;
		}

		//must be locked to syncmajor
		private bool RemoveItem(IUpdate update)
		{
			UpdateEntry index;

			if (indices.TryGetValue(update, out index))
			{
				RemoveIndex(index);
				return true;
			}

			return false;
		}

		//must be in syncminor
		private void RemoveIndex(UpdateEntry entry)
		{
			indices.Remove(entry.item);
			entries[entry.index] = null;
		}



		public void Update(int index, UpdateState state, List<UpdateEntry>[] moveList)
		{
			if (entriesHighIndex == 0 && 
				actionList.Count == 0)
				return;

			Monitor.Enter(syncMajor);
			{
				iteratingList = true;

				int count = 0;

				if (this.async)
				{
					state.IsAsynchronousState = this.async;
					state.PlayerInput.asyncAcess = this.async;

					if (this.async && state.Application.ThreadPool.ThreadCount > 0)
						count = AsyncUpdateEntries(state, moveList);
					else
						count = UpdateEntries(state, moveList);

					state.PlayerInput.asyncAcess = false;
					state.IsAsynchronousState = false;
				}
				else
					count = UpdateEntries(state, moveList);

				this.entriesHighIndex = count;

				iteratingList = false;
				if (actionList.Count > 0)
				{
					Monitor.Enter(syncMinor);

					for (int i = 0; i < actionList.Count; i++)
					{
						if (actionList[i].add)
							AddItem((IUpdate)actionList[i].item);
						else
							RemoveIndex((UpdateEntry)actionList[i].item);
					}
					actionList.Clear();
				
					Monitor.Exit(syncMinor);
				}

			}
			Monitor.Exit(syncMajor);
		}

		private int UpdateEntries(UpdateState state, List<UpdateEntry>[] moveList)
		{
			int count = 0;

			for (int i = 0; i < entriesHighIndex; i++)
			{
				if (entries[i] != null)
				{
					entries[i].index = count;

					UpdateFrequency frequency = entries[i].item.Update(state);

					if (frequency != this.frequncy)
					{
						if (frequency == UpdateFrequency.Terminate)
						{
							Monitor.Enter(syncMinor);
							RemoveIndex(entries[i]);
							Monitor.Exit(syncMinor);
						}
						else
							moveList[(int)frequency].Add(entries[i]);

						entries[i] = null;
					}
				}

				if ((entries[count] = entries[i]) != null)
					count++;
			}
			return count;
		}


		private int asyncIndex;
		private AsyncProcessor[] asyncProcessors;

		private class AsyncProcessor : Xen.Threading.IAction
		{
			private readonly Updater parent;
			public readonly List<KeyValuePair<UpdateFrequency,UpdateEntry>> moveList;
			public Xen.Threading.WaitCallback callback;
 
			public AsyncProcessor(Updater parent)
			{
				this.parent = parent;
				this.moveList = new List<KeyValuePair<UpdateFrequency, UpdateEntry>>();
			}

			public void PerformAction(object data)
			{
				parent.AsyncUpdateEntriesThread((UpdateState)data, null, moveList);
			}
		}

		private int AsyncUpdateEntries(UpdateState state, List<UpdateEntry>[] moveList)
		{
			Xen.Threading.ThreadPool pool = state.Application.ThreadPool;

			if (entriesHighIndex < 2)
				return UpdateEntries(state, moveList);

			if (asyncProcessors == null)
			{
				asyncProcessors = new AsyncProcessor[pool.ThreadCount];
				for (int i = 0; i < asyncProcessors.Length; i++)
					asyncProcessors[i] = new AsyncProcessor(this);
			}
			asyncIndex = -1;

			for (int i = 0; i < asyncProcessors.Length; i++)
				asyncProcessors[i].callback = pool.QueueTask(asyncProcessors[i], state);
			AsyncUpdateEntriesThread(state, moveList,null);

			for (int i = 0; i < asyncProcessors.Length; i++)
			{
				asyncProcessors[i].callback.WaitForCompletion();
				if (asyncProcessors[i].moveList.Count > 0)
				{
					foreach (KeyValuePair<UpdateFrequency,UpdateEntry> entry in asyncProcessors[i].moveList)
						moveList[(int)entry.Key].Add(entry.Value);
				}
				asyncProcessors[i].moveList.Clear();
			}

			int count = 0;
			for (int i = 0; i < entriesHighIndex; i++)
			{
				if (entries[i] != null)
					entries[i].index = count;

				if ((entries[count] = entries[i]) != null)
					count++;
			}
			return count;
		}

		private void AsyncUpdateEntriesThread(UpdateState state, List<UpdateEntry>[] moveList, List<KeyValuePair<UpdateFrequency, UpdateEntry>> keyedMoveList)
		{
			while (true)
			{
				int i = Interlocked.Increment(ref asyncIndex);
				if (i >= entriesHighIndex)
					break;

				if (entries[i] != null)
				{
					entries[i].index = i;

					UpdateFrequency frequency = entries[i].item.Update(state);

					if (frequency != this.frequncy)
					{
						if (frequency == UpdateFrequency.Terminate)
						{
							Monitor.Enter(syncMinor);
							RemoveIndex(entries[i]);
							Monitor.Exit(syncMinor);
						}
						else
						{
							if (moveList != null)
								moveList[(int)frequency].Add(entries[i]);
							if (keyedMoveList != null)
								keyedMoveList.Add(new KeyValuePair<UpdateFrequency, UpdateEntry>(frequency, entries[i]));
								
						}

						entries[i] = null;
					}
				}
			}
		}
	}
}
