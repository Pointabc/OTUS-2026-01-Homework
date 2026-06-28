
using System.Collections;
using TelegramBotLib.Core.Entities;

namespace TelegramBotLib.Core.Helpers
{
    public static class EnumerableExtension
    {
        /// <summary>
        /// Получить пачку по номеру пачки.
        /// </summary>
        /// <param name="numbers">Коллекция IEnumerable.</param>
        /// <param name="batchSize">Размер пачки.</param>
        /// <param name="batchNumber">Номер возвращаемой пачки, нумерация с 0.</param>
        public static IEnumerable? GetBatchByNumber(this IEnumerable collection, int batchSize, int batchNumber)
        {
            if (collection == null)
                return null;

            var firstItemIndex = batchSize * (batchNumber - 1);
            var list = collection.Cast<object>().ToList();
            if (list.Count() < firstItemIndex)
                return null;

            if (list.Count() < firstItemIndex + batchSize)
                batchSize = list.Count() - firstItemIndex;

            return list.GetRange(firstItemIndex, batchSize);
        }
    }
}
