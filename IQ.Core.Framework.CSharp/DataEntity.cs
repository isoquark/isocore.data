using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Data;


namespace IQ.Core.Framework
{
    /// <summary>
    /// Defines utility functions for working with data entities which
    /// are distinguished from "data records" by the fact that they are mutable
    /// </summary>
    public static class DataEntity
    {
        public static IReadOnlyList<PropertyInfo> props<T>() =>
            typeof(T).GetProperties();

        public static IReadOnlyList<string> propnames<T>() =>
            props<T>().Select(p => p.Name).ToList();

        /// <summary>
        /// Projects a data entity onto an array (in property declaration order)
        /// </summary>
        /// <typeparam name="T">The entity type</typeparam>
        /// <param name="o"></param>
        /// <returns></returns>
        public static object[] ToItemArray<T>(T o) =>
            props<T>().Map(p => p.GetValue(o)).ToArray<object>();

        /// <summary>
        /// Hydrates a data entity from an item array, assuming the items in the array
        /// are arranged in property declaration order
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <returns></returns>
        public static T FromItemArray<T>(object[] items)
        {
            var props = props<T>();
            if (props.Count != items.Length)
                throw new ArgumentException(
                    $"Array length of {items.Length} does not match the {props.Count} public properties on the entity");

            var o = Activator.CreateInstance<T>();
            for (int i = 0; i < props.Count; i++)
                props[i].SetValue(o, items[i]);
            return o;
        }


        /// <summary>
        /// Projects a collection of data entities onto a <see cref="DataTable"/>
        /// </summary>
        /// <typeparam name="T">The entity type</typeparam>
        /// <param name="items">The items</param>
        /// <returns></returns>
        public static DataTable ToDataTable<T>(IEnumerable<T> items)
        {
            var table = new DataTable();
            props<T>().Map(p => table.Columns.Add(p.Name, p.PropertyType));
            items.Map(ToItemArray).Map(x => table.LoadDataRow(x, true));
            return table;
        }

        /// <summary>
        /// Creates a collection of entities from a data table
        /// </summary>
        /// <typeparam name="T">The entity type</typeparam>
        /// <param name="t"></param>
        /// <returns></returns>
        public static IReadOnlyList<T> FromDataDable<T>(DataTable t)
        {
            var allprops = props<T>();
            var colprops = from c in t.Columns.Cast<DataColumn>()
                           let p = allprops.FirstOrDefault(x => x.Name == c.ColumnName)
                           where p != null
                           select new
                           {
                               Col = c,
                               Prop = p
                           };

            var items = new List<T>();
            foreach (DataRow row in t.Rows)
            {
                var item = Activator.CreateInstance<T>();
                items.Add(item);
                foreach (var colprop in colprops)
                {
                    colprop.Prop.SetValue(item, row[colprop.Col]);
                }
            }
            return items;

        }
    }
}
