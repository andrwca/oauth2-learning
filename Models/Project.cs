namespace WebApiAuth.Models
{
    public class Project
    {
        public Project(string projectName, string ownerId)
        {
            Name = projectName;
            Owners.Add(ownerId);
        }

        public string Id { get; } = Guid.NewGuid().ToString();
        public string Name { get; set; }
        public List<string> Owners { get; set; } = [];
    }
}