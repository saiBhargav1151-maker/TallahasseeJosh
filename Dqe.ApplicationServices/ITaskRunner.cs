using System;

namespace Dqe.ApplicationServices
{
    public interface ITaskRunner
    {
        void CopyMasterFile(int masterFileId, DateTime effectiveDate, int currentUserSrsId, int fileNumber);
    }
}