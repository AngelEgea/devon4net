using System.Runtime.CompilerServices;

namespace Devon4Net.Infrastructure.Common.Helpers
{
    public static class AsyncEnumerableHelper
    {
        public static async IAsyncEnumerable<T> IEnumerableToIAsyncEnumerable<T>(IEnumerable<T> items, [EnumeratorCancellation] CancellationToken cancelationToken = default)
        {
            foreach (var item in items)
            {
                if (cancelationToken.IsCancellationRequested) break;
                yield return await Task.FromResult(item);
            }
        }

        public static async Task<IEnumerable<T>> IAsyncEnumerableToIEnumerable<T>(IAsyncEnumerable<T> items, CancellationToken cancelationToken = default)
        {
            var listResult = new List<T>();
            await foreach (var item in items)
            {
                if (cancelationToken.IsCancellationRequested) break;
                listResult.Add(item);
            }
            return listResult;
        }

        public static async IAsyncEnumerable<List<T>> Split<T>(IAsyncEnumerable<T> source, int length, [EnumeratorCancellation] CancellationToken cancelationToken = default)
        {
            var elements = new List<T>(length);
            await foreach (var element in source)
            {
                if (cancelationToken.IsCancellationRequested)
                {
                    elements.Clear();
                    break;
                }
                elements.Add(element);
                if (elements.Count == length)
                {
                    yield return elements;
                    elements = new List<T>(length);
                }
            }
            if (elements.Count > 0)
            {
                yield return elements;
            }
        }
    }
}
