using System;
using System.Collections;

namespace InfotainmentSystem.Data
{
    // A source supports one request at a time. Cancellation is silent.
    public interface IWidgetDataSource<T>
    {
        IEnumerator Fetch(Action<DataResult<T>> completed);
        void Cancel();
    }
}
