namespace Vaultr.Client.Core.Extensions;

internal static class AsyncEnumerableExtensions
{
    public static async IAsyncEnumerable<KeyValuePair<TKey, TValue>> ZipAsync<TInput, TKey, TValue>(
        this IEnumerable<KeyValuePair<TKey, TInput>> source, Func<TInput, IAsyncEnumerable<TValue>> selector)
    {
        var enumerators = new Dictionary<IAsyncEnumerator<KeyValuePair<TKey, TValue>>, Task<bool>>();

        foreach (var item in source)
        {
            var key = item.Key;
            var values = selector.Invoke(item.Value);

            var enumerator = ListAsync(key, values).GetAsyncEnumerator();

            enumerators.Add(enumerator, enumerator.MoveNextAsync().AsTask());
        }

        while (enumerators.Count > 0)
        {
            await Task.WhenAny(enumerators.Values);

            var enumerator = enumerators.FirstOrDefault(x => x.Value.IsCompletedSuccessfully);

            if (enumerator.Value.Result)
            {
                yield return enumerator.Key.Current;

                enumerators[enumerator.Key] = enumerator.Key.MoveNextAsync().AsTask();
            }
            else
            {
                enumerators.Remove(enumerator.Key);
            }
        }
    }

    private static async IAsyncEnumerable<KeyValuePair<TKey, TValue>> ListAsync<TKey, TValue>(TKey key, IAsyncEnumerable<TValue> values)
    {
        await foreach (var value in values)
        {
            yield return new(key, value);
        }
    }
}
