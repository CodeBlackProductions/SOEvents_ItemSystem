using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ItemSystem.Editor
{
    public static class AttributeFilterHelper
    {
        public static List<T> FilterEntries<T>(PropertyInfo _Property, List<T> _ListToFilter)
        {
            var filters = _Property.GetCustomAttributes(typeof(ItemToolkitFilter), false).Cast<ItemToolkitFilter>().ToArray();

            if (filters == null || filters.Length == 0)
            {
                return _ListToFilter;
            }

            List<T> filteredItems = new List<T>();

            foreach (var filter in filters)
            {
                if (filter.Additive)
                {
                    if (filter.InstanceNames != null && filter.InstanceNames.Length > 0)
                        filteredItems = _ListToFilter.Where(item => item != null && filter.InstanceNames.Contains((string)item.GetType().GetProperty("ModuleName")?.GetValue(item))).ToList();
                    if (filter.Types != null && filter.Types.Length > 0)
                        filteredItems = _ListToFilter.Where(item => item != null && filter.Types.Contains(item.GetType())).ToList();
                }
                else
                {
                    var orItems = new List<T>();
                    if (filter.InstanceNames != null && filter.InstanceNames.Length > 0)
                        orItems.AddRange(_ListToFilter.Where(item => item != null && filter.InstanceNames.Contains((string)item.GetType().GetProperty("ModuleName")?.GetValue(item))));
                    if (filter.Types != null && filter.Types.Length > 0)
                        orItems.AddRange(_ListToFilter.Where(item => item != null && filter.Types.Contains(item.GetType())));

                    filteredItems = filteredItems.Concat(orItems).Where(item => item != null).Distinct().ToList();
                }
            }

            return filteredItems;
        }

        public static bool EntryFitsFilters<T>(PropertyInfo _Property, T _Entry)
        {
            if (_Entry == null)
                return false;

            var filters = _Property.GetCustomAttributes(typeof(ItemToolkitFilter), false).Cast<ItemToolkitFilter>().ToArray();

            if (filters.Length == 0)
                return true;

            foreach (var filter in filters)
            {
                bool match = filter.Additive;

                if (filter.InstanceNames != null && filter.InstanceNames.Length > 0)
                {
                    var moduleName = (string)_Entry.GetType().GetProperty("ModuleName")?.GetValue(_Entry);
                    bool nameMatch = filter.InstanceNames.Contains(moduleName);

                    if (filter.Additive)
                        match &= nameMatch;
                    else
                        match |= nameMatch;
                }

                if (filter.Types != null && filter.Types.Length > 0)
                {
                    bool typeMatch = filter.Types.Contains(_Entry.GetType());

                    if (filter.Additive)
                        match &= typeMatch;
                    else
                        match |= typeMatch;
                }

                if (filter.Additive && !match)
                    return false;
                if (!filter.Additive && match)
                    return true;
            }

            return filters.All(f => f.Additive);
        }
    }
}