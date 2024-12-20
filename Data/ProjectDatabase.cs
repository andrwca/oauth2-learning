using WebApiAuth.Models;

namespace WebApiAuth.Data
{
    public class ProjectDatabase : IProjectsDatabase
    {
        public IList<Project> Projects { get; } = [];
    }
}
