using WebApiAuth.Models;

namespace WebApiAuth.Data
{
    public interface IProjectsDatabase
    {
        IList<Project> Projects { get; }
    }
}