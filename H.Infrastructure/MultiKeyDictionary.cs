namespace H.Infrastructure
{
    public class MultiKeyDictionary<T1, T2, T3> : Dictionary<T1, Dictionary<T2, T3>> where T1 : notnull where T2 : notnull
    {
        public new Dictionary<T2, T3> this[T1 key]
        {
            get
            {
                if (!ContainsKey(key))
                {
                    Add(key, new Dictionary<T2, T3>());
                }

                TryGetValue(key, out Dictionary<T2, T3>? returnObj);

                return returnObj!;
            }
        }
    }
}