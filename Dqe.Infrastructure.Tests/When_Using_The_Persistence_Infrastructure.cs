using System;
using System.Linq;
using Dqe.Domain.Model;
using Dqe.Domain.Model.Reports;
using Dqe.Infrastructure.Fdot;
using Dqe.Infrastructure.Providers;
using Dqe.Infrastructure.Repositories.Custom;
using Dqe.Infrastructure.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dqe.Infrastructure.EntityIoC;

namespace Dqe.Infrastructure.Tests
{
    [TestClass]
// ReSharper disable InconsistentNaming
    public class When_Using_The_Persistence_Infrastructure
// ReSharper restore InconsistentNaming
    {
        [TestInitialize]
        public void SetUp()
        {
            Initializer.Initialize();
            EntityDependencyResolver.OnResolveConstructorArguments += EntityDependencyResolverOnResolveConstructorArguments;
        }

        private static object[] EntityDependencyResolverOnResolveConstructorArguments(object sender, ResolveConstructorArgumentsArgs args)
        {
            if (args.EntityType.IsAssignableFrom(typeof(DqeUser))) return new object[] { new StaffService(), new DqeUserRepository(), new ProposalRepository(), new ProjectRepository() };
            if (args.EntityType.IsAssignableFrom(typeof(PayItemStructure))) return new object[] { new PayItemStructureRepository() };
            if (args.EntityType.IsAssignableFrom(typeof(MasterFile))) return new object[] { new MasterFileRepository() };
            //if (args.EntityType.IsAssignableFrom(typeof(PayItem))) return new object[] { new PayItemRepository() };
            if (args.EntityType.IsAssignableFrom(typeof(Project))) return new object[] { new ProjectRepository(), UnitOfWorkProvider.CommandRepository, new WebTransportService() };
            if (args.EntityType.IsAssignableFrom(typeof(MarketArea))) return new object[] { new MarketAreaRepository() };
            if (args.EntityType.IsAssignableFrom(typeof(Proposal))) return new object[] { new ProposalRepository() };
            if (args.EntityType.IsAssignableFrom(typeof(ProjectEstimate))) return new object[] { new WebTransportService() };
            return null;
        }

        //[TestMethod]
        //public void CanDeleteProposalAssociatedToProject()
        //{
        //    var r = new ProposalRepository();
        //    var prop = r.GetByNumber("E5Y17").ToList()[0];
        //    var proj = prop.Projects.ToList()[0];
        //    prop.RemoveProject(proj);
        //    UnitOfWorkProvider.CommandRepository.Remove(prop);
        //    UnitOfWorkProvider.CommandRepository.Flush();
        //    UnitOfWorkProvider.TransactionManager.Commit();
        //}

        //[TestMethod]
        //public void Can_build_report_structure_from_proposal()
        //{
        //    var masterPayItemRepo = new PayItemMasterRepository();
        //    var proposalRepo = new ProposalRepository();
        //    proposalRepo.GetReportProposalAndItems("T1299", ReportProposalLevel.Authorization, masterPayItemRepo);
        //}

        //[TestMethod]
        //public void Many_To_Many_Operations_Persist_As_Expected()
        //{
        //    Initializer.CreateSystemAccount();
        //    var sys = new DqeUserRepository().GetSystemAccount();
        //    var or = new OtherReferenceWebLink();
        //    var ort = or.GetTransformer();
        //    ort.Name = "Test";
        //    ort.WebLink = "http://www.test.com";
        //    or.Transform(ort, sys);
        //    UnitOfWorkProvider.CommandRepository.Add(or);
        //    var pis = new PayItemStructure(new PayItemStructureRepository());
        //    var pist = pis.GetTransformer();
        //    pist.StructureId = "0001-001-001";
        //    pist.Title = "Test";
        //    pist.EffectiveDate = DateTime.Now;
        //    pist.Accuracy = 1;
        //    pist.PrimaryUnit = PrimaryUnitType.AC;
        //    pis.Transform(pist, sys);
        //    UnitOfWorkProvider.CommandRepository.Add(pis);
        //    UnitOfWorkProvider.TransactionManager.Commit();
        //    pis = new PayItemStructureRepository().Get(pis.Id);
        //    or = (OtherReferenceWebLink)new DqeWebLinkRepository().Get(or.Id);
        //    Assert.IsNotNull(pis);
        //    Assert.IsNotNull(or);
        //    //add many-to-many - one other reference is linked to the pay item structure
        //    pis.AddOtherReference(or, sys);
        //    UnitOfWorkProvider.TransactionManager.Commit();
        //    pis = new PayItemStructureRepository().Get(pis.Id);
        //    Assert.IsNotNull(pis);
        //    //verify the pay item structure collection has one other reference
        //    Assert.IsTrue(pis.OtherReferences.Count() == 1);
        //    //remove the other reference form the pay item structure
        //    pis.RemoveOtherReference(pis.OtherReferences.ToList()[0], sys);
        //    UnitOfWorkProvider.TransactionManager.Commit();
        //    pis = new PayItemStructureRepository().Get(pis.Id);
        //    or = (OtherReferenceWebLink)new DqeWebLinkRepository().Get(or.Id);
        //    //verify the pay item structure exists
        //    Assert.IsNotNull(pis);
        //    //verify the other reference exists
        //    Assert.IsNotNull(or);
        //    //verify the pay item structure does not contain the other reference in its collection
        //    Assert.IsTrue(!pis.OtherReferences.Any());
        //    //add many-to-many - one other reference is linked to the pay item structure
        //    pis.AddOtherReference(or, sys);
        //    UnitOfWorkProvider.TransactionManager.Commit();
        //    pis = new PayItemStructureRepository().Get(pis.Id);
        //    Assert.IsNotNull(pis);
        //    //verify the pay item structure collection has one other reference
        //    Assert.IsTrue(pis.OtherReferences.Count() == 1);
        //    UnitOfWorkProvider.TransactionManager.Commit();
        //    or = (OtherReferenceWebLink)new DqeWebLinkRepository().Get(or.Id);
        //    //verify the other reference exists
        //    Assert.IsNotNull(or);
        //    //delete the other reference
        //    UnitOfWorkProvider.CommandRepository.Remove(or);
        //    UnitOfWorkProvider.TransactionManager.Commit();
        //    pis = new PayItemStructureRepository().Get(pis.Id);
        //    //verify the pay item structure exists
        //    Assert.IsNotNull(pis);
        //    or = (OtherReferenceWebLink)new DqeWebLinkRepository().Get(or.Id);
        //    //verify the other reference was deleted
        //    Assert.IsNull(or);
        //    //verify the pay item structure does not contain the other reference in its collection
        //    Assert.IsTrue(!pis.OtherReferences.Any());
        //}
    }
}
