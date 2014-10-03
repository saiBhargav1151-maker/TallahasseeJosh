using System;
using System.ComponentModel.DataAnnotations;

namespace Dqe.Domain.Model.Wt
{
    public class Project
    {
        [Required]
        public virtual long Id { get; set; }

        [StringLength(20)]
        [Required]
        public virtual string ProjectNumber { get; set; }

        [StringLength(20)]
        [Required]
        public virtual string SpecBook { get; set; }

        [StringLength(256)]
        [Required]
        public virtual string Description { get; set; }

        public virtual Proposal MyProposal { get; set; }

        //TODO: convert these to references

        public virtual long? PrimaryDistrict { get; set; }

        public virtual long? PrimaryCounty { get; set; }
    }
}