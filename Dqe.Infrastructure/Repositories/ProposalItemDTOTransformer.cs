using System;
using System.Collections;
using System.Reflection;
using Dqe.Domain.Model;
using NHibernate.Transform;

namespace Dqe.Infrastructure.Repositories
{
    /// <summary>
    /// Custom result transformer for ProposalItemDTO that handles NULL values correctly.
    /// This transformer explicitly maps columns by alias name, avoiding the NULL mapping issues
    /// that occur with AliasToBean when VendorRanking is NULL.
    /// </summary>
    public class ProposalItemDTOTransformer : IResultTransformer
    {
        private static readonly PropertyInfo[] Properties;
        private static readonly System.Collections.Generic.Dictionary<string, PropertyInfo> PropertyMap;

        static ProposalItemDTOTransformer()
        {
            Properties = typeof(ProposalItemDTO).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            PropertyMap = new System.Collections.Generic.Dictionary<string, PropertyInfo>(StringComparer.OrdinalIgnoreCase);
            foreach (var prop in Properties)
            {
                PropertyMap[prop.Name] = prop;
            }
        }

        public object TransformTuple(object[] tuple, string[] aliases)
        {
            var dto = new ProposalItemDTO();
            
            for (int i = 0; i < aliases.Length && i < tuple.Length; i++)
            {
                var alias = aliases[i];
                var value = tuple[i];
                
                if (PropertyMap.TryGetValue(alias, out var property))
                {
                    try
                    {
                        // Handle NULL values explicitly
                        if (value == null || (value is System.DBNull))
                        {
                            // Only set to null if the property is nullable
                            if (property.PropertyType.IsGenericType && 
                                property.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                            {
                                property.SetValue(dto, null);
                            }
                            // For non-nullable types, leave as default value (don't set)
                        }
                        else
                        {
                            // Convert the value to the property type if needed
                            var propertyType = property.PropertyType;
                            if (propertyType.IsGenericType && 
                                propertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                            {
                                propertyType = Nullable.GetUnderlyingType(propertyType);
                            }
                            
                            object convertedValue = value;
                            if (value.GetType() != propertyType)
                            {
                                // Handle type conversion
                                if (propertyType == typeof(decimal))
                                {
                                    convertedValue = Convert.ToDecimal(value);
                                }
                                else if (propertyType == typeof(int))
                                {
                                    convertedValue = Convert.ToInt32(value);
                                }
                                else if (propertyType == typeof(long))
                                {
                                    convertedValue = Convert.ToInt64(value);
                                }
                                else if (propertyType == typeof(DateTime))
                                {
                                    convertedValue = Convert.ToDateTime(value);
                                }
                                else if (propertyType == typeof(string))
                                {
                                    convertedValue = value.ToString();
                                }
                            }
                            
                            property.SetValue(dto, convertedValue);
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log the error but continue mapping other properties
                        System.Diagnostics.Debug.WriteLine($"Error mapping property {alias}: {ex.Message}");
                    }
                }
            }
            
            return dto;
        }

        public IList TransformList(IList collection)
        {
            return collection;
        }
    }
}

