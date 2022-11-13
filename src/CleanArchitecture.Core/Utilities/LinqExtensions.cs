namespace CleanArchitecture.Core.Utilities
{
    public static class LinqExtensions
    {
        // How use Long type for Skip in Linq
        // source: https://stackoverflow.com/questions/32309807/how-use-long-type-for-skip-in-linq
        static public IQueryable<T> Skip<T>(this IQueryable<T> source, long count)
                => Skip(source, int.MaxValue, count);

        internal static IQueryable<T> Skip<T>(this IQueryable<T> source, int segmentSize, long count)
        {
            long segmentCount = Math.DivRem(count, segmentSize, out long remainder);

            for (long i = 0; i < segmentCount; i += 1)
                source = Queryable.Skip(source, segmentSize);

            if (remainder != 0)
                source = Queryable.Skip(source, (int)remainder);

            return source;
        }
    }
}