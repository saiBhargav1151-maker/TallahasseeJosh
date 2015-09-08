using System.Collections.Generic;
using Dqe.Domain.Model;

namespace Dqe.Domain.Repositories.Custom
{
    public interface IDqeWebLinkRepository
    {
        DqeWebLink Get(long id);
        IEnumerable<DqeWebLink> GetWebLinks(string linkType, string val);
        IEnumerable<OtherReferenceWebLink> GetOtherReferences();
        IEnumerable<PpmChapterWebLink> GetPpmChapters();
        IEnumerable<PrepAndDocChapterWebLink> GetPrepAndDocChapters();
        IEnumerable<SpecificationWebLink> GetSpecifications();
        IEnumerable<StandardWebLink> GetStandards();
    }
}