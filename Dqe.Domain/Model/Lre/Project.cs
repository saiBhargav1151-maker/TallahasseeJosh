namespace Dqe.Domain.Model.Lre
{
    public class Project
    {
        /// <summary>
        /// <id name="Id" column="ESTM_PROJ_SQ">
        /// </summary>
        public virtual long Id { get; set; }

        /// <summary>
        /// <property name="ProjectName" column="ESTM_PROJ_ID" not-null="true"/>
        /// </summary>
        public virtual string ProjectName { get; set; }

        /// <summary>
        /// <property name="District" column="MNG_DIST_CD" not-null="true"/>
        /// </summary>
        public virtual string District { get; set; }

        /// <summary>
        /// Dictates to user if they want DQE as the primary program instead of LRE
        /// It is in the DB as a single char byte
        ///<!--indictates to user if they want DQE as the primary estimating program instead of LRE. MB.-->
	    ///<property name = "QuantityComplete" column="QTY_CMPLT_CD" not-null="true" update="true" />
        /// </summary>
        public virtual string QuantityComplete { get; set; }

    }
}