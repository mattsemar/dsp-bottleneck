using System;

namespace Bottleneck
{
    public class FilterButtonItemAge : IComparable<FilterButtonItemAge>, IEquatable<FilterButtonItemAge>
    {
        public readonly UIButton uiButton;
        public readonly int itemId;
        public DateTime lastUpdated;

        public FilterButtonItemAge(UIButton uiButton, int itemId)
        {
            this.uiButton = uiButton;
            this.itemId = itemId;
        }


        public int CompareTo(FilterButtonItemAge other)
        {
            if (!ReferenceEquals(uiButton, other.uiButton))
            {
                return uiButton.GetInstanceID().CompareTo(other.uiButton.GetInstanceID());
            }

            return itemId.CompareTo(other.itemId);
        }

        public bool Equals(FilterButtonItemAge other)
        {
            if (other == null)
                return false;
            return ReferenceEquals(uiButton, other.uiButton) && itemId == other.itemId;
        }

        public override int GetHashCode()
        {
            return uiButton.GetInstanceID() + itemId;
        }
    }
}