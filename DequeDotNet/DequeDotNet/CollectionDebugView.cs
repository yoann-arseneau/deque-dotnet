using System;
using System.Collections;
using System.Diagnostics;

namespace DequeDotNet {
	public class CollectionDebugView {
		private readonly ICollection collection;

		public CollectionDebugView(ICollection source) {
			if (source is null) {
				throw new ArgumentNullException(nameof(source));
			}
			collection = source;
		}

		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		public object[] Items {
			get {
				object[] array = new object[collection.Count];
				collection.CopyTo(array, 0);
				return array;
			}
		}
	}
}
