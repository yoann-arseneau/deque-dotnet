using System.Collections;
using System.Collections.Generic;

namespace DequeDotNet {
	public interface IDeque<T> : IList<T>, IReadOnlyList<T>, ICollection {
		new T this[int index] { get; set; }

		bool IsEmpty { get; }
		new int Count { get; }
		int Capacity { get; }

		void PushBack(T item);
		void PushFront(T item);

		T PopBack();
		T PopFront();
	}
}
