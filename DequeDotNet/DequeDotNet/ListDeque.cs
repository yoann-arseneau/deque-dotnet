using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace DequeDotNet {
	public class ListDeque<T> : IDeque<T> {
		public T this[int index] {
			get {
				if (index < 0 || index >= count) {
					throw new ArgumentOutOfRangeException(nameof(index));
				}
				return buffer[index];
			}
			set {
				if (index < 0 || index > count) {
					throw new ArgumentOutOfRangeException(nameof(index));
				}
				if (index == count) {
					EnsureCapacity(count + 1);
				}
				buffer[index] = value;
			}
		}

		public bool IsEmpty => count == 0;
		public int Count => count;
		public int Capacity => throw new NotImplementedException();

		bool ICollection<T>.IsReadOnly => false;

		private T[] buffer;
		private int count;
		private int version;

		public ListDeque()
			: this(0) {
		}
		public ListDeque(int capacity) {
			buffer = capacity != 0 ? new T[capacity] : Array.Empty<T>();
		}

		public bool Contains(T item) => IndexOf(item) >= 0;
		public int IndexOf(T item) => Array.IndexOf(buffer, item, 0, count);

		public void CopyTo(T[] array, int arrayIndex) => Array.Copy(buffer, 0, array, arrayIndex, count);

		public void PushBack(T item) {
			if (!EnsureCapacity(count + 1)) {
				Interlocked.Increment(ref version);
			}
			buffer[count++] = item;
		}
		public void PushFront(T item) {
			if (!EnsureCapacity(count + 1, 0)) {
				Interlocked.Increment(ref version);
				Array.Copy(buffer, 0, buffer, 1, count);
				count += 1;
			}
			buffer[0] = item;
		}
		public void Insert(int index, T item) {
			if (index == 0) {
				PushFront(item);
			}
			else if (index == count) {
				PushBack(item);
			}
			else {
				if (index < 0 || index > count) {
					throw new ArgumentOutOfRangeException(nameof(index));
				}
				if (EnsureCapacity(count + 1, index)) {
					buffer[index] = item;
					count += 1;
				}
				else {
					Interlocked.Increment(ref version);
					Array.Copy(buffer, index, buffer, index + 1, count - index);
					buffer[index] = item;
					count += 1;
				}
			}
		}

		public void Clear() {
			if (count > 0) {
				Interlocked.Increment(ref version);
				Array.Fill(buffer, default, 0, count);
				count = 0;
			}
		}
		public T PopBack() {
			if (count <= 0) {
				throw new InvalidOperationException("underflow");
			}
			Interlocked.Increment(ref version);
			count -= 1;
			var result = buffer[count];
			buffer[count] = default;
			return result;
		}
		public T PopFront() {
			if (count <= 0) {
				throw new InvalidOperationException("underflow");
			}
			Interlocked.Increment(ref version);
			var result = buffer[0];
			count -= 1;
			Array.Copy(buffer, 1, buffer, 0, count);
			buffer[count] = default;
			return result;
		}
		public bool Remove(T item) {
			if (IndexOf(item) is int index and >= 0) {
				RemoveAt(index);
				return true;
			}
			else {
				return false;
			}
		}
		public void RemoveAt(int index) {
			if (index < 0 || index >= count) {
				throw new ArgumentOutOfRangeException(nameof(index));
			}
			Array.Copy(buffer, index, buffer, index + 1, count - index);
			buffer[--count] = default;
		}

		public IEnumerator<T> GetEnumerator() {
			var staleVersion = Volatile.Read(ref version);
			for (var i = 0; i < count; ++i) {
				if (staleVersion != Volatile.Read(ref version)) {
					throw new InvalidOperationException("collection modified while iterating");
				}
				yield return buffer[i];
			}
		}

		private bool EnsureCapacity(int required, int hole = -1) {
			const int MinSize = 8;
			Debug.Assert(hole <= count);
			if (required > Capacity) {
				var recommended = Capacity <= 0x3FFFFFFF ? Capacity * 2 : int.MaxValue;
				recommended = Math.Max(recommended, MinSize);
				var newSize = Math.Max(required, recommended);
				T[] newBuf = new T[newSize];
				Interlocked.Increment(ref version);
				if (hole >= 0) {
					if (hole == 0) {
						Array.Copy(buffer, 0, newBuf, 1, count);
					}
					else {
						Array.Copy(buffer, 0, newBuf, 0, hole);
						Array.Copy(buffer, hole, newBuf, hole + 1, count - hole);
					}
				}
				else {
					Array.Copy(buffer, newBuf, count);
				}
				buffer = newBuf;
				return true;
			}
			else {
				return false;
			}
		}

		void ICollection<T>.Add(T item) => PushBack(item);
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}
