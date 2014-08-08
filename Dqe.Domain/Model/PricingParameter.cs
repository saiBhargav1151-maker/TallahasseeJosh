using System;
using System.Collections.Generic;

namespace Dqe.Domain.Model
{
    public abstract class PricingParameter : Entity<Transformers.PricingParameter>
    {
        private ICollection<WorkType> _workTypes = new List<WorkType>();
        public virtual int Months { get; protected set; }
        public virtual ContractType ContractType { get; protected set; }
        public virtual QuantitiesType Quantities { get; protected set; }
        public virtual IEnumerable<WorkType> WorkTypes {get { return _workTypes; } }
        public virtual PricingModelType PricingModel { get; protected set; }
        public virtual BiddersType Bidders { get; protected set; }

        public virtual void AddWorkType(WorkType workType, DqeUser dqeUser)
        {
            CheckWorkTypeSecurity(dqeUser);
            _workTypes.Add(workType);
        }

        public virtual void RemoveWorkType(WorkType workType, DqeUser dqeUser)
        {
            CheckWorkTypeSecurity(dqeUser);
            _workTypes.Remove(workType);
        }

        public virtual void ClearWorkTypes(DqeUser dqeUser)
        {
            CheckWorkTypeSecurity(dqeUser);
            _workTypes.Clear();
        }

        public abstract void CheckWorkTypeSecurity(DqeUser dqeUser);
    }
}
