using Dqe.Domain.Model;
using System;
using System.Collections.Generic;

namespace Dqe.Web.Services
{
    public class DynamicHelper
    {
        public static bool HasNotNullProperty(dynamic expandoObject, string propertyName)
        {
            if (!((IDictionary<string, object>)expandoObject).ContainsKey(propertyName))
            {
                return false;
            }
            if (((IDictionary<string, object>)expandoObject)[propertyName] == null)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Removes suffix from the end of string. This is used primarily to find the parent proposal number/ project number 
        /// </summary>
        /// <param name="entireString">The entire string, including the suffix</param>
        /// <param name="suffix">The suffix to be removed, if found at the end of the string</param>
        /// <returns>Truncated string without the given suffix at the end</returns>
        public static string RemoveSuffixFromString(string entireString, string suffix)
        {
            if (string.IsNullOrEmpty(entireString))
            {
                return entireString;
            }

            entireString = entireString.Trim();

            if (!string.IsNullOrEmpty(entireString) && 
                entireString.EndsWith(suffix, StringComparison.InvariantCultureIgnoreCase))
            {
                return entireString.Remove(entireString.Length - 2);
            }
            return string.Empty;
        }

        /// <summary>
        /// This gets the given string contains "LS", "DB", "SV", returns empty string if none is found
        /// </summary>
        /// <param name="entireString"></param>
        /// <returns>string, empty string if not found</returns>
        public static string GetSpecialSuffix(string entireString)
        {
            if (string.IsNullOrEmpty(entireString))
            {
                return string.Empty;
            }

            entireString = entireString.Trim().ToUpper();

            if (entireString.EndsWith("LS"))
            {
                return "LS";
            }
            else if (entireString.EndsWith("DB"))
            {
                return "DB";
            }
            else if (entireString.EndsWith("SV"))
            {
                return "SV";
            }

            return string.Empty;
        }

        /// <summary>
        /// This determines if the given string contains "LS", "DB", "SV"
        /// </summary>
        /// <param name="entireString"></param>
        /// <returns>bool</returns>
        public static bool ContainsSpecialSuffix(string entireString)
        {
            if (string.IsNullOrEmpty(entireString))
            {
                return false;
            }
               
            entireString = entireString.Trim().ToUpper();

            if (entireString.EndsWith("LS") ||
                entireString.EndsWith("DB") ||
                entireString.EndsWith("SV") 
                )
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns a snapshot Label display verbiage. MB.
        /// </summary>
        /// <param name="label"></param>
        /// <returns></returns>
        /// <see cref="SnapshotLabel"/>
        public static string GetSnapshotLabelString(SnapshotLabel label)
        {
            return label == SnapshotLabel.Official ? "Official"
                    : label == SnapshotLabel.Authorization ? "Authorization"
                    : label == SnapshotLabel.Phase4 ? "Phase IV"
                    : label == SnapshotLabel.Phase3 ? "Phase III"
                    : label == SnapshotLabel.Phase2 ? "Phase II"
                    : label == SnapshotLabel.Phase1 ? "Phase I"
                    : label == SnapshotLabel.Scope ? "Scope"
                    : label == SnapshotLabel.Initial ? "Initial"
                    : label == SnapshotLabel.Review ? "Review"
                    : label == SnapshotLabel.Coder ? "Coder"
                    : string.Empty;
        }
     
        //public static string GetNextSnapshotLabelString( SnapshotLabel label)
        //{
        //    return label == SnapshotLabel.Official ? "Official"
        //            : label == SnapshotLabel.Authorization ? "Authorization"
        //            : label == SnapshotLabel.Phase4 ? "Phase IV"
        //            : label == SnapshotLabel.Phase3 ? "Phase III"
        //            : label == SnapshotLabel.Phase2 ? "Phase II"
        //            : label == SnapshotLabel.Phase1 ? "Phase I"
        //            : label == SnapshotLabel.Scope ? "Scope"
        //            : label == SnapshotLabel.Initial ? "Initial"
        //            : string.Empty;
        //}
    }
}