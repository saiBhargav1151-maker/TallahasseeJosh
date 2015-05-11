using System;

namespace Dqe.ApplicationServices
{
    public interface ITaskRunner
    {
        void CopyMasterFile(string taskUser, int masterFileId, DateTime effectiveDate, int currentUserSrsId, int fileNumber);
    }
}