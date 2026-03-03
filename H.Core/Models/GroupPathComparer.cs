using System.Collections;

namespace H.Core.Models
{
    public class GroupPathComparer : IComparer
    {
        private int CompareObjects(ComponentBase? x, ComponentBase? y)
        {
            if (x != null)
            {
                if (y != null)
                {
                    return String.Compare(x.GroupPath, y.GroupPath, StringComparison.Ordinal);
                }
            }

            return 0;
        }

        public int Compare(object? x, object? y)
        {
            return this.CompareObjects(x as ComponentBase, y as ComponentBase);
        }
    }
}
