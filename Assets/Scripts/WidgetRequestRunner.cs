using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InfotainmentSystem.Data
{
    /// <summary>
    /// This class manages a single request to a data source for a widget, including caching, rate-limiting, and error handling.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class WidgetRequestRunner<T> 
    {
        private readonly MonoBehaviour owner; // The MonoBehaviour that owns this runner, used to start coroutines and log exceptions.
        private readonly IWidgetDataSource<T> source; // The data source that provides the request to run.
        private readonly Action showLoading; // The action to show a loading indicator while the request is in progress.
        private readonly Action<DataResult<T>> showResult; // The action to show the result of the request, whether successful or failed.
        private readonly Action<bool> setBusy; // The action to set the busy state of the widget.
        private readonly double cacheSeconds;  
        private readonly double minimumRefreshSeconds;
        private readonly Stack<IEnumerator> iterators = new Stack<IEnumerator>(); // Stack of enumerators to manage nested coroutines and ensure proper disposal.
        private Coroutine coroutine; // The currently running coroutine for the request, if any.
        private DataResult<T> cachedResult; // The last successful result of the request.
        private double completedAt;
        private double lastStartedAt = double.NegativeInfinity;
        private int generation;
        public bool IsLoading { get; private set; }

        /// <summary>
        /// Creates a new WidgetRequestRunner for the specified data source and widget actions.
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="source"></param>
        /// <param name="showLoading"></param>
        /// <param name="showResult"></param>
        /// <param name="setBusy"></param>
        /// <param name="cacheSeconds"></param>
        /// <param name="minimumRefreshSeconds"></param>
        public WidgetRequestRunner(MonoBehaviour owner, IWidgetDataSource<T> source,
            Action showLoading, Action<DataResult<T>> showResult,
            Action<bool> setBusy, double cacheSeconds = 300,
            double minimumRefreshSeconds = 5)
        {
            this.owner = owner;
            this.source = source;
            this.showLoading = showLoading;
            this.showResult = showResult;
            this.setBusy = setBusy;
            this.cacheSeconds = cacheSeconds;
            this.minimumRefreshSeconds = minimumRefreshSeconds;
        }

        /// <summary>
        /// Used to activate the runner, showing cached results if available and valid, or starting a new request if not.
        /// </summary>
        public void Activate()
        {
            if (cachedResult != null)
            {
                showResult(cachedResult);
                if (Time.realtimeSinceStartupAsDouble - completedAt < cacheSeconds)
                    return;
            }
            Refresh();
        }

        /// <summary>
        /// Used to refresh the request, starting a new request if not already loading and respecting the minimum refresh interval.
        /// </summary>
        public void Refresh()
        {
            if (IsLoading || !owner.isActiveAndEnabled)
                return;
            if (Time.realtimeSinceStartupAsDouble - lastStartedAt < minimumRefreshSeconds)
                return;

            IsLoading = true;
            lastStartedAt = Time.realtimeSinceStartupAsDouble;
            setBusy(true);
            showLoading();
            int requestGeneration = ++generation;
            coroutine = owner.StartCoroutine(Run(requestGeneration));
            // A source may complete without yielding (validation or test source).
            if (!IsLoading)
                coroutine = null;
        }
        /// <summary>
        /// Used to cancel the current request, if any, and reset the runner state. This allows a new request to be started later.
        /// </summary>
        public void Cancel()
        {
            ++generation;
            source.Cancel();
            if (coroutine != null)
                owner.StopCoroutine(coroutine);
            coroutine = null;
            DisposeIterators();
            IsLoading = false;
            setBusy(false);
            // Leaving/re-entering should allow an interrupted request to restart.
            lastStartedAt = double.NegativeInfinity;
        }

        /// <summary>
        /// Used to run the request coroutine, handling nested enumerators, errors, and caching.
        /// </summary>
        /// <param name="requestGeneration"></param>
        /// <returns></returns>
        private IEnumerator Run(int requestGeneration)
        {
            DataResult<T> result = null;
            Exception failure = null;
            IEnumerator fetch = null;
            try { fetch = source.Fetch(value => result = value); }
            catch (Exception exception) { failure = exception; }
            if (fetch != null)
                iterators.Push(fetch);

            // Flatten nested enumerators to own disposal and recover from errors.
            while (failure == null && iterators.Count > 0)
            {
                IEnumerator current = iterators.Peek();
                bool moved = false;
                object yielded = null;
                try
                {
                    moved = current.MoveNext();
                    if (moved) yielded = current.Current;
                }
                catch (Exception exception) { failure = exception; }

                if (failure != null)
                    break;
                if (!moved)
                {
                    (iterators.Pop() as IDisposable)?.Dispose();
                    continue;
                }
                if (yielded is IEnumerator nested)
                    iterators.Push(nested);
                else
                    yield return yielded;
            }

            source.Cancel();
            DisposeIterators();
            if (requestGeneration != generation)
                yield break;

            if (failure != null)
            {
                Debug.LogException(failure, owner);
                result = DataResult<T>.Failure("The request could not be completed.");
            }
            result = result ?? DataResult<T>.Failure("The service returned no result.");
            if (result.Status != DataStatus.Error)
            {
                cachedResult = result;
                completedAt = Time.realtimeSinceStartupAsDouble;
            }
            else
            {
                // Do not silently replace a failed refresh with old cached data.
                cachedResult = null;
                Debug.LogWarning(result.Error, owner);
            }

            showResult(result);
            // Keep the refresh button disabled during the short rate-limit window.
            double remaining = minimumRefreshSeconds -
                (Time.realtimeSinceStartupAsDouble - lastStartedAt);
            if (remaining > 0)
                yield return new WaitForSecondsRealtime((float)remaining);
            IsLoading = false;
            coroutine = null;
            setBusy(false);
        }

        /// <summary>
        /// Used to dispose of any remaining enumerators in the stack, ensuring that resources are released properly.
        /// </summary>
        private void DisposeIterators()
        {
            while (iterators.Count > 0)
                (iterators.Pop() as IDisposable)?.Dispose();
        }
    }
}
