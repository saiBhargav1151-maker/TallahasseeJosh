using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Dqe.Domain.Model.Wt
{
    public class Project
    {
        private readonly ICollection<Category> _categories;
        private readonly ICollection<County> _counties;
        private readonly ICollection<District> _districts; 

        public Project()
        {
            _categories = new Collection<Category>();
            _counties = new Collection<County>();
            _districts = new Collection<District>();
        }

        public virtual long Id { get; set; }

        public virtual long LsDbId { get; set; }

        public virtual string LsDbCode { get; set; }

        public virtual string ProjectNumber { get; set; }

        public virtual string SpecBook { get; set; }

        public virtual string Description { get; set; }

        public virtual string Designer { get; set; }

        public virtual string IsValid { get; set; }

        public virtual bool IsLatestVersion { get; set; }

        public virtual bool Controlling { get; set; }

        public virtual Proposal MyProposal { get; set; }

        public virtual DateTime? LettingDate { get; set; }

        public virtual string FederalProjectNumber { get; set; }

        public virtual string ProjectWorkType { get; set; }

        /// <summary>
        /// LRE Column - Dictates to user if they want DQE as the primary program instead of LRE
        /// It is in the DB as a single char byte
        /// </summary>
        public virtual string QuantityComplete { get; set; }

        #region "pricing"

        public virtual DateTime? EstimatedDate { get; set; }

        public virtual decimal? ProjectItemTotal { get; set; }

        public virtual DateTime? LastUpdatedDate { get; set; }

        public virtual string LastUpdatedBy { get; set; }

        public virtual string PricedBy { get; set; }

        public virtual DateTime? PricedDate { get; set; }
        public virtual string Pjcde1 { get; set; }
        #endregion

        public virtual void AddCategory(Category category)
        {
            _categories.Add(category);
        }

        public virtual IEnumerable<Category> Categories
        {
            get { return _categories.ToList().AsReadOnly(); }
        }

        public virtual IEnumerable<County> Counties
        {
            get { return _counties.ToList().AsReadOnly(); }
        }

        public virtual IEnumerable<District> Districts
        {
            get { return _districts.ToList().AsReadOnly(); }
        }
    }
}