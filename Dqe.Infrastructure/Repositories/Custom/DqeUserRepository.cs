using System.Collections.Generic;
using Dqe.Domain.Model;
using Dqe.Domain.Repositories.Custom;
using NHibernate;

namespace Dqe.Infrastructure.Repositories.Custom
{
    public class DqeUserRepository : BaseRepository, IDqeUserRepository
    {
        public DqeUserRepository() { }

        internal DqeUserRepository(ISession session)
        {
            Session = session;
        }

        public IEnumerable<DqeUser> GetAll(long currentUserId, string district)
        {
            InitializeSession();
            var q = Session
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

        public IEnumerable<DqeUser> GetAll()
        {
            InitializeSession();
            var q = Session
                .QueryOver<DqeUser>()
                .Where(i => i.IsActive)
                .Where(i => i.Role != DqeRole.System);
            return q.List();
        }

        public DqeUser Get(long id)
        {
            InitializeSession();
            return Session
                .Get<DqeUser>(id);
        }

        public DqeUser GetBySrsId(int id)
        {
            return GetBySrsId(id, true);
        }

        public DqeUser GetBySrsId(int id, bool useActiveCriteria)
        {
            InitializeSession();
            var q = Session
                .QueryOver<DqeUser>()
                .Where(i => i.SrsId == id)
                .Where(i => i.Role != DqeRole.System);
            if (useActiveCriteria)
            {
                q.Where(i => i.IsActive);
            }
            return q.SingleOrDefault();
        }

        public DqeUser GetSystemAccount()
        {
            InitializeSession();
            return Session
                .QueryOver<DqeUser>()
                .Where(i => i.Role == DqeRole.System)
                .SingleOrDefault();
        }

        public IEnumerable<DqeUser> GetAllSystemAdministrators()
        {
            InitializeSession();
            return Session
                .QueryOver<DqeUser>()
                .Where(i => i.Role == DqeRole.Administrator)
                .Where(i => i.IsActive)
                .List();
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