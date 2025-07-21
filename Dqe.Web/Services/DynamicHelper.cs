using Dqe.Domain.Model;
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