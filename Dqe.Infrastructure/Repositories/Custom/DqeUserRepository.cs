using System.Collections.Generic;
using Dqe.Domain.Model;
using Dqe.Domain.Repositories.Custom;
using Dqe.Infrastructure.Providers;

namespace Dqe.Infrastructure.Repositories.Custom
{
    public class DqeUserRepository : IDqeUserRepository
    {
        public IEnumerable<DqeUser> GetAll(int currentUserId, string district)
        {
            var q = UnitOfWorkProvider
                .Marshaler
                .CurrentSession
                .QueryOver<DqeUser>()
                .Where(i => i.IsActive)
                .Where(i => i.Role != DqeRole.System)
                .Where(i => i.Id != currentUserId);
            if (!string.IsNullOrWhiteSpace(district))
            {
                q.Where(i => i.District == district);
            }
            return q.List();

        }

        public DqeUser Get(int id)
        {
            return UnitOfWorkProvider
                .Marshaler
                .CurrentSession
                .Get<DqeUser>(id);
        }

        public DqeUser GetBySrsId(int id)
        {
            return UnitOfWorkProvider
                .Marshaler
                .CurrentSession
                .QueryOver<DqeUser>()
                .Where(i => i.SrsId == id)
                .Where(i => i.IsActive)
                .Where(i => i.Role != DqeRole.System)
                .SingleOrDefault();
        }

        public DqeUser GetSystemAccount()
        {
            return UnitOfWorkProvider
                .Marshaler
                .CurrentSession
                .QueryOver<DqeUser>()
                .Where(i => i.Role == DqeRole.System)
                .SingleOrDefault();
        }

        // EXAMPLE: Dynamic Transformer
        //public IEnumerable<dynamic> GetUserEmailsProjection(int id)
        //{
        //    return UnitOfWorkProvider
        //        .Marshaler
        //        .CurrentSession
        //        .QueryOver<UserAccount>()
        //        .Select
        //        (
        //        Projections.Property<UserAccount>(i => i.FirstName).As("FirstName"),
        //        Projections.Property<UserAccount>(i => i.LastName).As("LastName"),
        //        Projections.Property<UserAccount>(i => i.Email).As("Email")
        //        )
        //        .TransformUsing(new DynamicTransformer())
        //        .List<dynamic>();
        //}
    }
}