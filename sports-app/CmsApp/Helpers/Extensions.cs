using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace CmsApp.Helpers
{
    public static class Extensions
    {
        public static DataTable ToDataTable<T>(this IEnumerable<T> items)
        {
            var dataTable = new DataTable(typeof(T).Name);

            //Get all the properties
            var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                //Setting column names as Property names
                dataTable.Columns.Add(prop.Name, prop.PropertyType);
            }
            foreach (var item in items)
            {
                var values = new object[props.Length];

                for (var i = 0; i<props.Length; i++)
                {
                    //inserting property values to datatable rows
                    values[i] = props[i].GetValue(item, null);
                }

                dataTable.Rows.Add(values);
            }

            //put a breakpoint here and check datatable
            return dataTable;
        }
    }
}