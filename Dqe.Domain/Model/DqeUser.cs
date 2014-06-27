using System;
using System.ComponentModel.DataAnnotations;
using Dqe.Domain.Services;

namespace Dqe.Domain.Model
{
    public class DqeUser : Entity<Transformers.DqeUser>
    {
        private readonly IStaffService _staffService;

        public DqeUser(IStaffService staffService)
        {
            _staffService = staffService;
        }

        public virtual int SrsId { get; protected internal set; }

        [Range(1, 4)]
        public virtual DqeRole Role { get; protected internal set; }

        [Required]
        [StringLength(2)]
        public virtual string District { get; protected internal set; }

        public virtual string Name
        {
            get
            {
                var staff = _staffService.GetStaffById(SrsId);
                return staff == null ? string.Empty : staff.FullName;
            }
        }

        public virtual bool IsActive { get; protected internal set; }

        public override Transformers.DqeUser GetTransformer()
        {
            return new Transformers.DqeUser
            {
                Id = Id,
                IsActive = IsActive,
                Role = Role,
                District = District,
                SrsId = SrsId,
                FullName = Name,
                RoleAsString =
                    Role == DqeRole.Administrator
                        ? "System Administrator"
                        : Role == DqeRole.DistrictAdministrator
                            ? "District Administrator"
                            : Role == DqeRole.Estimator ? "Estimator" : "System"
            };
        }

        public override void Transform(Transformers.DqeUser transformer, DqeUser account)
        {
            if (transformer == null) throw new ArgumentNullException("transformer");
            IsActive = transformer.IsActive;
            Role = transformer.Role;
            District = transformer.District;
            SrsId = transformer.SrsId;
        }
    }
}