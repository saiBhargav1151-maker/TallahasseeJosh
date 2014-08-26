namespace Dqe.Domain.Model
{
    public class ProjectItem : Entity<Transformers.ProjectItem>
    {
        public virtual Project MyProject { get; protected internal set; }

        public virtual PayItem MyPayItem { get; protected internal set; }

        public override Transformers.ProjectItem GetTransformer()
        {
            return new Transformers.ProjectItem();
        }

        public override void Transform(Transformers.ProjectItem transformer, DqeUser account)
        {
            
        }
    }
}