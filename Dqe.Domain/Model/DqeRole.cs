namespace Dqe.Domain.Model
{
    /// <summary>
    ///General: Users will be limited to view only projects they are working on. New roles need to be established in DQE.This will help controlling confidential data in the system. It will be a security enhancement that doesn’t currently exist in DQE and will allow more users access to the data. 
    //Estimator: Change the existing Estimator role so they can only view construction lettings. This role may be renamed or removed as other roles are developed. See below for details.
    //Maintenance/Construction/Both: Enhance the estimator role to allow users to see only contract types they are given permission to see such as maintenance only or construction only contracts. 
    //Engineer: This role will be like the current estimator role but without access to the official estimate. 
    //Reviewer: View only with ability to take snapshots (global). 
    //Admin Enhanced: Central Office read and do for all projects and reports with ability to take snapshots.
    //Admin: Read only for both Central Office and District level. 
    /// </summary>
    public enum DqeRole
    {
        System = 'S',
        Administrator = 'A',
        DistrictAdministrator = 'D',
        PayItemAdministrator = 'P',
        CostBasedTemplateAdministrator = 'T',
        Estimator = 'E',
        Reviewer = 'R',
        Engineer = 'N',
        Maintenance = 'M',
        AdminReadOnly = 'R', 
        General = 'G'
    }

}