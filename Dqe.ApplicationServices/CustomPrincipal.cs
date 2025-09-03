using System;
using System.Security.Principal;
using Dqe.Domain.Model;
using Dqe.Domain.Repositories.Custom;

namespace Dqe.ApplicationServices
{
    /// <summary>
    /// COMPONENT TEMPLATE
    /// </summary>
    public class CustomPrincipal : IPrincipal
    {
        private readonly IIdentity _identity;
        private readonly IDqeUserRepository _dqeUserRepository;
        
        //it is safe to inject any dependencies you need here (i.e. a user repository to lookup roles)
        public CustomPrincipal
            (
            IIdentity identity,
            IDqeUserRepository dqeUserRepository
            )
        {
            _identity = identity;
            _dqeUserRepository = dqeUserRepository;
        }

        /// <summary>
        /// REQUIRES CUSTOM IMPLEMENTATION
        /// </summary>
        /// <param name="role">Role for role check.</param>
        /// <returns>true if in role, otherwise false</returns>
        public bool IsInRole(string role)
        {
            if (!_identity.IsAuthenticated) return false;
            var user = _dqeUserRepository.Get(((DqeIdentity) _identity).Id);
            if (user == null) return false;
            var decodedRole = Enum.GetName(typeof (DqeRole), user.Role);
            return decodedRole != null && String.Equals(decodedRole, role, StringComparison.CurrentCultureIgnoreCase);
        }

        public IIdentity Identity
        {
            get { return _identity; }
        }
    }
}