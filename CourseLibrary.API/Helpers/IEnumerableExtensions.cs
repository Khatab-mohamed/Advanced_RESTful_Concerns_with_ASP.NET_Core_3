using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace CourseLibrary.API.Helpers
{
    public class IEnumerableExtensions
    {
        public static IEnumerable<ExpandoObject> ShapeData<TSource>
        (IEnumerable<TSource> source,
            string fields)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            // create a list to hold our ExpandoObject
            var expandObjectList = new List<ExpandoObject>();

            // create a list with a PropertyInfo objects on TSource. Reflection is
            // expensive, so rather than doing it for each object in the list, we do
            // it once and reuse the results. After all, part of the reflection is on the 
            // type of the object (TSource), not on the instance

            var propertyInfoList = new List<PropertyInfo>();

            if (string.IsNullOrWhiteSpace(fields))
            {
                // all public fields should be in ExpandoObject
                var propertyInfos = typeof(TSource)
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance);
                propertyInfoList.AddRange(propertyInfos);
            }
            else
            {
                // the fields are separated by "," so we split it.
                var fieldsAfterSplit = fields.Split(',');
                foreach (var field in fieldsAfterSplit)
                {
                    // trim each field, as it might contain leading
                    // or trailing spaces. Can't trim the var in foreach,
                    // so we use another var.

                    var propertyName = field.Trim();
                    // use reflection to get the property on the source object
                    // we need to include public and instance, b/a specifying a binding
                    // flag overwrites the already-existing binding flags.
                    var propertyInfo = typeof(TSource)
                        .GetProperty(propertyName,
                            BindingFlags.IgnoreCase |
                            BindingFlags.Public |
                            BindingFlags.Instance);
                    if (propertyInfo == null)
                    {
                        throw new Exception($"Property {propertyName} wasn't found on" +
                                            $"{typeof(TSource)}");
                    }

                    // add propertyInfo to list
                    propertyInfoList.Add(propertyInfo);
                }
            }

            // run through the source objects
            foreach (TSource sourceObject in source)
            {
                // create an ExpandoObject that will hold the
                // selected properties & values
                var dataShapedObject = new ExpandoObject();
                foreach (var propertyInfo in propertyInfoList)
                {
                    // GetValue returns the value of the property on the source object
                    var propertyValue = propertyInfo.GetValue(sourceObject);

                    // add the field to the ExpandObject
                    ((IDictionary<string, object>) dataShapedObject)
                        .Add(propertyInfo.Name, propertyValue);
                }

                // add ExpandObject to the list
                expandObjectList.Add(dataShapedObject);
            }

            // return the list
            return expandObjectList;
        }
    }
}
