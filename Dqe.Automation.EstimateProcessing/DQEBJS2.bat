Rem **********************************************************************        
Rem *                   [R]  BJS Batch File MODIFICATION LOG              *        
Rem *---------------------------------------------------------------------*        
Rem *   DATE        NAME         DESCRIPTION                              *        
Rem * 05/13/2015  KNATCRL        JOB CREATION                             *           
Rem *---------------------------------------------------------------------*        
Rem ***********************************************************************        
Rem *                      JOB INFORMATION                                *        
Rem ***********************************************************************        
Rem *  APPLICATION NAME [R]- DESIGN QUANTITIES AND ESTIMATES (DQE)        *     
Rem *                                                                     *        
Rem *  ABEND ASSISTANCE [R]- CONTACT PE3TEAM IN BSSO AT 410-5567.         *        
Rem *                                                                     *        
Rem *  SUCCESSFUL JOB   [R]- COND CODE = 0      (HIGHEST ACCEPTABLE CC)   *        
Rem *                                                                     *        
Rem *  SEVERITY         [R]- CRITICAL, NOTIFY CONTACT IMMEDIATELY         *        
Rem *                                                                     *        
Rem *  Batch LOCATION   [R]- C:\BJSFOLDER\BatchFiles                      *        
Rem *                                                                     *
Rem *  JOB DESCRIPTION  [R]- Loads official estimates to wT for jobs that *
Rem *                        are awarded or beyond                        *
Rem *                                                                     *        
Rem *  EXTERNAL EVENT   [R]- NO                                           *         
Rem *                                                                     *        
Rem *  ON REQUEST       [R]- NO                                           *         
Rem *                                                                     *        
Rem *  FREQUENCY        [R]- DAILY  									  *        
Rem *                                                                     *        
Rem ***********************************************************************        
Rem *                       ENTERPRISE DATA                               *        
Rem ***********************************************************************        
Rem *                                                                     *        
Rem *  WT DATA REQ.     [R]- BID                                          *        
Rem *                        CODETABLE                                    *
Rem *                        ALTERNATESET                                 *
Rem *                        CATEGORY                                     *
Rem *                        CATEGORYALTERNATESET                         *
Rem *                        CODEVALUE                                    *
Rem *                        COUNTY                                       *
Rem *                        DISTRICT                                     *
Rem *                        FUNDPACKAGE                                  *
Rem *                        LETTING                                      *
Rem *                        MILESTONE                                    *
Rem *                        PROJECT                                      *
Rem *                        PROJECTITEM                                  *
Rem *                        PROPOSAL                                     *
Rem *                        PROPOSALITEM                                 *
Rem *                        PROPOSALVENDOR                               *
Rem *                        REFCOUNTY                                    *
Rem *                        REFDISTRICT                                  *
Rem *                        REFITEM                                      *
Rem *                        REFVENDOR                                    *
Rem *                        PROPOSALSECTION                              *
Rem *                                                                     *        
Rem *                                                                     *        
Rem ***********************************************************************        
Rem *             BJS JOB SCHEDULING REQUIREMENTS                         *        
Rem ***********************************************************************        
Rem *BJS SYSTEM Requires [R]-                                             *        
Rem *                       - FDOT ENTERPRISE LIBRARY                     *
Rem *                       - Dqe.ApplicationServices.dll                 *
Rem *                       - Dqe.Domain.dll                              *
Rem *                       - Dqe.Infrastructure.dll                      *
Rem *                       - NHibernate.dll                              *
Rem *                       - NHibernate.Session.dll                      *
Rem *                       - Glimpse.Core.dll                            *
Rem *                       - .NET runtime                                *
Rem *                                                                     * 
Rem *  EXECUTION TIME     [R]- 3:00 AM					                  *        
Rem *                                                                     *        
Rem *  JOB NAME           [R]- DQEBJS2                                    *        
Rem *                                                                     *
Rem *  JOB PREDECESSOR    [R]- NONE                                       *        
Rem *                                                                     *        
Rem *  CANNOT RUN WITH    [R]- NONE                                       *        
Rem *                                                                     *         
Rem *  BJS Arguments      [R]- NONE					                      *         
Rem *                                                                     *         
Rem *  HOLIDAY INDICATOR  [R]- Y 					                      *        
Rem *                                                                     *         
Rem ***********************************************************************        
Rem * FTP TRANSMITTAL   [O]- NOT REQUIRED                                 *      
Rem *                                                                     *        
Rem * RPT DISTRIBUTION  [O]- NOT REQUIRED                                 *        
Rem *                                                                     *        
Rem *SPECIAL INSTRUCTION[R]-                                              *        
Rem ***********************************************************************        
Rem *                   [R]   STEP DESCRIPTION                            *        
Rem ***********************************************************************        
Rem * STEP DESCRIPTION  [R]- DQE bid history and average price data Load  *        
Rem *                                                                     *
Rem * SUCCESSFUL STEP      - COND CODE = 0 (HIGHEST ACCEPTABLE CC)        *        
Rem *                                                                     *
Rem *********************************************************************** 

@Rem Begin Mandatory

Rem  BJS/JOB_CLASS =%1
Rem  BJS/JOB_GROUP =%2
call %procs%\batchproc.bat %*

@Rem End Mandatory

Cd %Executables%\DQE
Dqe.Automation.EstimateProcessing.exe

IF ERRORLEVEL 1 GOTO SETERRORCODE

GOTO JOB_END

:SETERRORCODE
rem *** Set job status code to a negative value. This indicates that the
rem *** job has abnormally terminated.

call %PROCS%\SetStatusProc.bat -1

:JOB_END