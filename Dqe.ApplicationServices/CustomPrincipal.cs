using System.Security.Principal;
using Dqe.Domain.Repositories.Custom;

namespace Dqe.ApplicationServices
{
    /// <summary>
    /// COMPONENT TEMPLATE
    /// </summary>
    public class CustomPrincipal : IPrincipal
    {
        private readonly IIdentity _identity;
        private readonly IContextService _contextService;
        private readonly IUserAccountRepository _userAccountRepository;
        
        //it is safe to inject any dependencies you need here (i.e. a user repository to lookup roles)
        public CustomPrincipal
            (
            IIdentity identity,
            IContextService contextService,
            IUserAccountRepository userAccountRepository
            )
        {
            _identity = identity;
            _contextService = contextService;
            _userAccountRepository = userAccountRepository;
        }

        /// <summary>
        /// REQUIRES CUSTOM IMPLEMENTATION
        /// </summary>
        /// <param name="role">Role for role check.</param>
        /// <returns>true if in role, otherwise false</returns>
        public bool IsInRole(string role)
        {
            if (!_identity.IsAuthenticated) return false;
            var user = (_contextService.UserId == 0)
                ? _userAccountRepository.Get(_identity.Name)
                : _userAccountRepository.Get(_contextService.UserId);
            if (user == null) return false;
            _contextService.UserId = user.Id;
            return user.AccountRole == role;
        }

        public IIdentity Identity
        {
            get { return _identity; }
        }
    }
}