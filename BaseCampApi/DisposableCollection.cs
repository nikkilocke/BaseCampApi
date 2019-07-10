using System;
using System.Collections.Generic;
using System.Text;

namespace BaseCampApi {
	public class DisposableCollection : List<IDisposable>, IDisposable {

		public T Add<T>(T item) where T : IDisposable {
			base.Add(item);
			return item;
		}

		public void Dispose() {
			foreach(var v in this) {
				try {
					v.Dispose();
				} catch {
				}
			}
		}
	}
}
