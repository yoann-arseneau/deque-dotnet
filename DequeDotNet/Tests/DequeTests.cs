using DequeDotNet;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Tests {
	static class Rand {
		private static readonly Random rand = new();

		public static int RandomIndex(this ICollection col, bool inclusive = false) {
			return rand.Next(inclusive ? col.Count + 1 : col.Count);
		}

		public static bool Chance(int desired = 1, int max = 2) {
			Debug.Assert(desired >= 0 && desired <= max);
			if (desired == 0) {
				return false;
			}
			if (desired == max) {
				return true;
			}
			else {
				return rand.Next(max) <= desired;
			}
		}
		public static int Next(int max) => rand.Next(max);
		public static double NextDouble() {
			return rand.NextDouble();
		}
	}

	[TestClass]
	public class DequeTests {
		[TestMethod]
		public void ListDequeTests() {
			BaselineTest(new ListDeque<double>());
		}

		public static void BaselineTest<T>(T expected) where T : IDeque<double>, new() {
			const int AddNew = 0;
			const int RemoveIdx = 1;
			const int RemoveValue = 2;

			var actual = new T();
			AssertEquals(expected, actual);
			for (var i = 0; i < 1000; ++i) {
				int action;
				if (expected.IsEmpty) {
					action = AddNew;
				}
				else {
					action = Rand.Next(5) switch {
						0 or 1 => AddNew,
						2 or 3 => RemoveIdx,
						4 => RemoveValue,
						_ => throw new InvalidOperationException("impossible?!"),
					};
				}
				switch (action) {
					case AddNew: {
						var val = Rand.NextDouble();
						var idx = expected.RandomIndex(true);
						if (idx == 0 && Rand.Chance()) {
							expected.PushFront(val);
							actual.PushFront(val);
						}
						else if (idx == expected.Count && Rand.Chance()) {
							expected.PushBack(val);
							actual.PushBack(val);
						}
						else {
							expected.Insert(idx, val);
							actual.Insert(idx, val);
						}
						break;
					}
					case RemoveIdx: {
						var idx = expected.RandomIndex();
						if (idx == 0 && Rand.Chance()) {
							expected.PopFront();
							actual.PopFront();
						}
						else if (idx == expected.Count && Rand.Chance()) {
							expected.PopBack();
							actual.PopBack();
						}
						else {
							expected.RemoveAt(idx);
							actual.RemoveAt(idx);
						}
						break;
					}
					case RemoveValue: {
						var val = expected[expected.RandomIndex()];
						Assert.AreEqual(expected.Remove(val), actual.Remove(val));
						break;
					}
				}
				AssertEquals(expected, actual);
			}
		}

		private static void AssertEquals(IDeque<double> expected, IDeque<double> actual) {
			Assert.AreEqual(expected.IsEmpty, actual.IsEmpty);
			Assert.AreEqual(expected.Count, actual.Count);
			for (var i = 0; i < expected.Count; ++i) {
				Assert.AreEqual(expected[i], actual[i]);
			}
			var expEn = expected.GetEnumerator();
			var actEn = expected.GetEnumerator();
			while (expEn.MoveNext()) {
				if (actEn.MoveNext()) {
					Assert.AreEqual(expEn.Current, actEn.Current);
				}
				else {
					Assert.Fail($"not enough items in {nameof(actual)}'s enumerator");
				}
			}
			if (actEn.MoveNext()) {
				Assert.Fail($"too many items in {nameof(actual)}'s enumerator");
			}
		}
	}

	class BaselineDeque<T> : List<T>, IDeque<T> {
		public bool IsEmpty => Count == 0;

		public T PopBack() {
			var result = this[Count - 1];
			RemoveAt(Count - 1);
			return result;
		}
		public T PopFront() {
			var result = this[0];
			RemoveAt(0);
			return result;
		}

		public void PushBack(T item) => Add(item);
		public void PushFront(T item) => Insert(0, item);
	}
}
