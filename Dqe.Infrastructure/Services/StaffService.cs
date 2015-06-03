using System;
using System.Collections.Generic;
using System.Linq;
using Dqe.Domain.Services;
using Dqe.Domain.Transformers;
using FDOT.Enterprise;
using IStaffService = Dqe.Domain.Services.IStaffService;

namespace Dqe.Infrastructure.Services
{
    public class StaffService : IStaffService
    {
        private static readonly IDictionary<int, Staff> StaffByIdCache = new Dictionary<int, Staff>();
        private static readonly IDictionary<string, Staff> StaffByRacfCache = new Dictionary<string, Staff>();
        private static readonly object IdLock = new object();
        private static readonly object RacfLock = new object();

        public AuthenticationToken AuthenticateUser(string user, string password)
        {
            var proxy = ChannelProvider<FDOT.Enterprise.Racf.Client.IRacfService>.Default;
            var token = proxy.Authenticate(user, password);
            return new AuthenticationToken
            {
                IsAuthenticated = token.IsAuthenticated,
                IsPasswordExpired = token.IsPasswordExpired,
                Message = token.Message
            };
        }

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
            if (StaffByIdCache.ContainsKey(id))
            {
                lock (IdLock)
                {
                    return StaffByIdCache[id];    
                }
            }
            var proxy = ChannelProvider<FDOT.Enterprise.Staff.Client.IStaffService>.Default;
            var result = proxy.GetStaffById(id);
            if (result == null) return null;
            var staff = new Staff
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
            lock (IdLock)
            {
                if (StaffByIdCache.ContainsKey(id))
                {
                    return StaffByIdCache[id];
                }
                StaffByIdCache.Add(staff.Id, staff);
            }
            return staff;
        }

        public Staff GetStaffByRacf(string id)
        {
            if (id.Contains(@"\"))
            {
                id = id.Split(new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries)[1];
            }

            if (StaffByRacfCache.ContainsKey(id))
            {
                lock (RacfLock)
                {
                    return StaffByRacfCache[id];
                }
            }
            var proxy = ChannelProvider<FDOT.Enterprise.Staff.Client.IStaffService>.Default;
            var result = proxy.GetStaffByRacfId(id);
            if (result == null) return null;
            var staff = new Staff
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
            lock (RacfLock)
            {
                if (StaffByRacfCache.ContainsKey(id))
                {
                    return StaffByRacfCache[id];
                }
                StaffByRacfCache.Add(id, staff);
            }
            return staff;
        }
    }
}