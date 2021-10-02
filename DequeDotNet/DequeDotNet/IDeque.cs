using System.Collections.Generic;

namespace DequeDotNet {
	public interface IDeque<T> : IList<T>, IReadOnlyList<T> {
		bool IsEmpty { get; }
		int Capacity { get; }

		void PushBack(T item);
		void PushFront(T item);

		T PopBack();
		T PopFront();
	}
}
