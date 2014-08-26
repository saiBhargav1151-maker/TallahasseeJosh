using System;
using System.Collections.Generic;
using System.Linq;
using Dqe.Domain.Transformers;
using FDOT.Enterprise;
using IStaffService = Dqe.Domain.Services.IStaffService;

namespace Dqe.Infrastructure.Services
{
    public class StaffService : IStaffService
    {
        public IEnumerable<Staff> GetStaffByName(string name)
        {
            var proxy = ChannelProvider<FDOT.Enterprise.Staff.Client.IStaffService>.Default;
            var staff = proxy.SearchStaffByName(name);
            //var staff = proxy.SearchStaffBySearchCriteria(new StaffSearchCriteria { FirstName = name, LastName = name, RacfId = name, IncludeActive = true, IncludeInactive = false});
            return staff
                .Where(s => s.Active)
                .Select(s => new Staff
                             {
                                 FirstName = s.FirstName,
                                 Id = s.PrimaryId,
                                 LastName = s.LastName,
                                 UserId = s.RacfId,
                                 District = s.District,
                                 Email = s.EmailAddress,
                                 PhoneNumber = s.Phone,
                                 PhoneExt = s.PhoneExtension
                             })
                .ToList();
        }

        public Staff GetStaffById(int id)
        {
            var proxy = ChannelProvider<FDOT.Enterprise.Staff.Client.IStaffService>.Default;
            var result = proxy.GetStaffById(id);
            if (result != null)
            {
                return new Staff
                       {
                           Id = result.PrimaryId,
                           FirstName = result.FirstName,
                           LastName = result.LastName,
                           UserId = result.RacfId,
                           District = result.District,
                           Email = result.EmailAddress,
                           PhoneNumber = result.Phone,
                           PhoneExt = result.PhoneExtension
                       };
            }
            return null;
        }

        public Staff GetStaffByRacf(string id)
        {
            if (id.Contains(@"\"))
            {
                id = id.Split(new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries)[1];
            }
            var proxy = ChannelProvider<FDOT.Enterprise.Staff.Client.IStaffService>.Default;
            var result = proxy.GetStaffByRacfId(id);
            if (result != null)
            {
                return new Staff
                       {
                           Id = result.PrimaryId,
                           FirstName = result.FirstName,
                           LastName = result.LastName,
                           UserId = result.RacfId,
                           District = result.District,
                           Email = result.EmailAddress,
                           PhoneNumber = result.Phone,
                           PhoneExt = result.PhoneExtension
                       };
            }
            return null;
        }
    }
}