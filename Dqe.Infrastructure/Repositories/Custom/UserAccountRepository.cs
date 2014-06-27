using System.Collections.Generic;
using NHibernate.Criterion;
using Dqe.Domain.Model;
using Dqe.Domain.Repositories.Custom;
using Dqe.Infrastructure.Providers;

namespace Dqe.Infrastructure.Repositories.Custom
{
    public class UserAccountRepository : IUserAccountRepository
    {
        public int Count()
        {
            return UnitOfWorkProvider
                .Marshaler
                .CurrentSession
                .QueryOver<UserAccount>()
                .RowCount();
        }

        public UserAccount Get(int id)
        {
            return UnitOfWorkProvider
                .Marshaler
                .CurrentSession
                .Get<UserAccount>(id);
        }

        public IEnumerable<dynamic> GetUserEmailsProjection(int id)
        {
            return UnitOfWorkProvider
                .Marshaler
                .CurrentSession
                .QueryOver<UserAccount>()
                .Select
                (
                Projections.Property<UserAccount>(i => i.FirstName).As("FirstName"), 
                Projections.Property<UserAccount>(i => i.LastName).As("LastName"),
                Projections.Property<UserAccount>(i => i.Email).As("Email")
                )
                .TransformUsing(new DynamicTransformer())
                .List<dynamic>();
        }

        public UserAccount Get(string email)
        {
            return UnitOfWorkProvider
                .Marshaler
                .CurrentSession
                .QueryOver<UserAccount>()
                .Where(i => i.Email == email)
                .SingleOrDefault();
        }

        public bool Authenticate(string email, string password)
        {
            return UnitOfWorkProvider
                .Marshaler
                .CurrentSession
                .QueryOver<UserAccount>()
                .Where(i => i.Email == email)
                .Where(i => i.AccountPassword == password)
                .Where(i => i.UnverifiedAccountToken == null)
                .RowCount() > 0;
        }

        public bool ValidateAccount(string token)
        {
            var account = UnitOfWorkProvider
                .Marshaler
                .CurrentSession
                .QueryOver<UserAccount>()
                .Where(i => i.UnverifiedAccountToken == token)
                .SingleOrDefault();
            if (account == null) return false;
            account.UnverifiedAccountToken = null;
            return true;
        }
    }
}