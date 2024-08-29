using System.Collections.Generic;

namespace Plugin.UI.interfaces
{
    internal interface IPagedData<T>
    {
        int GetTotalPages();

        IList<T> GetPageImages(
          int startIndex,
          int count);
    }
}
