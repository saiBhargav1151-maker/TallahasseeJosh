using System.Linq;
using System.Web.Http;
using Dqe.Domain.Services;

namespace Dqe.WebApi.Controllers
{
    public class StaffController : ApiController
    {
        private readonly IStaffService _staffService;

        public StaffController
            (
            IStaffService staffService
            )
        {
            _staffService = staffService;
        }

        public object Get(string id)
        {
            return
                _staffService.GetStaffByName(id).Select(i => new {id = i.Id, fullName = i.FullName, district = i.District});
        }
    }
}