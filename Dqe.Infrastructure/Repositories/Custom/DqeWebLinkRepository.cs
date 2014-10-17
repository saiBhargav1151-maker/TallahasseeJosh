using System;
using System.Collections.Generic;
using Dqe.Domain.Model;
using Dqe.Domain.Repositories.Custom;
using NHibernate;
using NHibernate.Criterion;

namespace Dqe.Infrastructure.Repositories.Custom
{
    public class DqeWebLinkRepository : BaseRepository, IDqeWebLinkRepository
    {
        public DqeWebLinkRepository() { }

        internal DqeWebLinkRepository(ISession session)
        {
            Session = session;
        }

        public DqeWebLink Get(int id)
        {
            InitializeSession();
            return Session.Get<DqeWebLink>(id);
        }

        public IEnumerable<DqeWebLink> GetWebLinks(string linkType, string val)
        {
            InitializeSession();
            switch (linkType)
            {
                case "OR":
                    return Session
                        .QueryOver<OtherReferenceWebLink>()
                        .WhereRestrictionOn(i => i.Name).IsInsensitiveLike(val, MatchMode.Anywhere)
                        .OrderBy(i => i.Name).Asc
                        .List();
                case "PC":
                    return Session
                        .QueryOver<PpmChapterWebLink>()
                        .WhereRestrictionOn(i => i.Name).IsInsensitiveLike(val, MatchMode.Anywhere)
                        .OrderBy(i => i.Name).Asc
                        .List();
                case "PD":
                    return Session
                        .QueryOver<PrepAndDocChapterWebLink>()
                        .WhereRestrictionOn(i => i.Name).IsInsensitiveLike(val, MatchMode.Anywhere)
                        .OrderBy(i => i.Name).Asc
                        .List();
                case "SP":
                    return Session
                        .QueryOver<SpecificationWebLink>()
                        .WhereRestrictionOn(i => i.Name).IsInsensitiveLike(val, MatchMode.Anywhere)
                        .OrderBy(i => i.Name).Asc
                        .List();
                case "SD":
                    return Session
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
            InitializeSession();
            return Session
                .QueryOver<OtherReferenceWebLink>()
                .OrderBy(i => i.Name).Asc
                .List();
        }

        public IEnumerable<PpmChapterWebLink> GetPpmChapters()
        {
            InitializeSession();
            return Session
                .QueryOver<PpmChapterWebLink>()
                .OrderBy(i => i.Name).Asc
                .List();
        }

        public IEnumerable<PrepAndDocChapterWebLink> GetPrepAndDocChapters()
        {
            InitializeSession();
            return Session
                .QueryOver<PrepAndDocChapterWebLink>()
                .OrderBy(i => i.Name).Asc
                .List();
        }

        public IEnumerable<SpecificationWebLink> GetSpecifications()
        {
            InitializeSession();
            return Session
                .QueryOver<SpecificationWebLink>()
                .OrderBy(i => i.Name).Asc
                .List();
        }

        public IEnumerable<StandardWebLink> GetStandards()
        {
            InitializeSession();
            return Session
                .QueryOver<StandardWebLink>()
                .OrderBy(i => i.Name).Asc
                .List();
        }
    }
}