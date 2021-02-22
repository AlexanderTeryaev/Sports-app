using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Omu.ValueInjecter;

namespace CmsApp.Helpers.Injections
{
    public class CloneInjection : ConventionInjection
    {
        protected override bool Match(ConventionInfo c)
        {
            return c.SourceProp.Name == c.TargetProp.Name && c.SourceProp.Value != null;
        }

        protected override object SetValue(ConventionInfo c)
        {
            //for value types and string just return the value as is
            if (c.SourceProp.Type.IsValueType || c.SourceProp.Type == typeof(string)
                || c.TargetProp.Type.IsValueType || c.TargetProp.Type == typeof(string))
                return c.SourceProp.Value;

            //handle arrays
            if (c.SourceProp.Type.IsArray)
            {
                var arr = c.SourceProp.Value as Array;

                if (arr == null) return null;

                var clone = Activator.CreateInstance(c.TargetProp.Type, arr.Length) as Array;

                for (var index = 0; index < arr.Length; index++)
                {
                    var a = arr.GetValue(index);
                    if (a.GetType().IsValueType || a is string) continue;
                    clone?.SetValue(
                        Activator.CreateInstance(c.TargetProp.Type.GetElementType())
                            .InjectFrom<CloneInjection>(a), index);
                }
                return clone;
            }


            if (c.SourceProp.Type.IsGenericType)
            {
                //handle IEnumerable<> also ICollection<> IList<> List<>
                if (c.SourceProp.Type.GetGenericTypeDefinition().GetInterfaces().Contains(typeof(IEnumerable)))
                {
                    var t = c.TargetProp.Type.GetGenericArguments()[0];
                    if (t.IsValueType || t == typeof(string)) return c.SourceProp.Value;

                    var tlist = typeof(List<>).MakeGenericType(t);
                    var list = Activator.CreateInstance(tlist);

                    var addMethod = tlist.GetMethod("Add");
                    var enumerable = c.SourceProp.Value as IEnumerable;
                    if (enumerable == null) return list;
                    foreach (var o in enumerable)
                    {
                        var e = Activator.CreateInstance(t).InjectFrom<CloneInjection>(o);
                        addMethod.Invoke(list, new[] {e});
                    }
                    return list;
                }

                //unhandled generic type, you could also return null or throw
                return c.SourceProp.Value;
            }

            //for simple object types create a new instace and apply the clone injection on it
            return Activator.CreateInstance(c.TargetProp.Type)
                .InjectFrom<CloneInjection>(c.SourceProp.Value);
        }
    }
}