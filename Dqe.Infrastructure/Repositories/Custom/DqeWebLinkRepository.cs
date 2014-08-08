using System;
using System.Collections.Generic;
using Dqe.Domain.Model;
using Dqe.Domain.Repositories.Custom;
using Dqe.Infrastructure.Providers;
using NHibernate.Criterion;

namespace Dqe.Infrastructure.Repositories.Custom
{
    public class DqeWebLinkRepository : IDqeWebLinkRepository
    {
        public DqeWebLink Get(int id)
        {
            return UnitOfWorkProvider
                .Marshaler
                .CurrentSession
                .Get<DqeWebLink>(id);
        }

        public IEnumerable<DqeWebLink> GetWebLinks(string linkType, string val)
        {
            switch (linkType)
            {
                case "OR":
                    return UnitOfWorkProvider
                        .Marshaler
                        .CurrentSession
                        .QueryOver<OtherReferenceWebLink>()
                        .WhereRestrictionOn(i => i.Name).IsInsensitiveLike(val, MatchMode.Anywhere)
                        .OrderBy(i => i.Name).Asc
                        .List();
                case "PC":
                    return UnitOfWorkProvider
                        .Marshaler
                        .CurrentSession
                        .QueryOver<PpmChapterWebLink>()
                        .WhereRestrictionOn(i => i.Name).IsInsensitiveLike(val, MatchMode.Anywhere)
                        .OrderBy(i => i.Name).Asc
                        .List();
                case "PD":
                    return UnitOfWorkProvider
                        .Marshaler
                        .CurrentSession
                        .QueryOver<PrepAndDocChapterWebLink>()
                        .WhereRestrictionOn(i => i.Name).IsInsensitiveLike(val, MatchMode.Anywhere)
                        .OrderBy(i => i.Name).Asc
                        .List();
                case "SP":
                    return UnitOfWorkProvider
                        .Marshaler
                        .CurrentSession
                        .QueryOver<SpecificationWebLink>()
                        .WhereRestrictionOn(i => i.Name).IsInsensitiveLike(val, MatchMode.Anywhere)
                        .OrderBy(i => i.Name).Asc
                        .List();
                case "SD":
                    return UnitOfWorkProvider
                        .Marshaler
                        .CurrentSession
                        .QueryOver<StandardWebLink>()
                        .WhereRestrictionOn(i => i.Name).IsInsensitiveLike(val, MatchMode.Anywhere)
                        .OrderBy(i => i.Name).Asc
                        .List();
                default:
                    throw new ArgumentOutOfRangeException("linkType");
            }
        }

        public IEnumerable<OtherReferenceWebLink> GetOtherReferences()
        {
            return UnitOfWorkProvider
                .Marshaler
                .CurrentSession
                .QueryOver<OtherReferenceWebLink>()
                .OrderBy(i => i.Name).Asc
                .List();
        }

        public IEnumerable<PpmChapterWebLink> GetPpmChapters()
        {
            return UnitOfWorkProvider
                .Marshaler
                .CurrentSession
                .QueryOver<PpmChapterWebLink>()
                .OrderBy(i => i.Name).Asc
                .List();
        }

        public IEnumerable<PrepAndDocChapterWebLink> GetPrepAndDocChapters()
        {
            return UnitOfWorkProvider
                .Marshaler
                .CurrentSession
                .QueryOver<PrepAndDocChapterWebLink>()
                .OrderBy(i => i.Name).Asc
                .List();
        }

        public IEnumerable<SpecificationWebLink> GetSpecifications()
        {
            return UnitOfWorkProvider
                .Marshaler
                .CurrentSession
                .QueryOver<SpecificationWebLink>()
                .OrderBy(i => i.Name).Asc
                .List();
        }

        public IEnumerable<StandardWebLink> GetStandards()
        {
            return UnitOfWorkProvider
                .Marshaler
                .CurrentSession
                .QueryOver<StandardWebLink>()
                .OrderBy(i => i.Name).Asc
                .List();
        }
    }
}