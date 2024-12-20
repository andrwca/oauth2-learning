using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;
using System.Security.Claims;
using WebApiAuth.Data;
using WebApiAuth.Models;

namespace WebApiAuth.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProjectController : ControllerBase
    {
        private readonly IProjectsDatabase _database;
        private readonly IAuthorizationService _authorizationService;

        public ProjectController(IProjectsDatabase projects, IAuthorizationService authorizationService)
        {
            _database = projects;
            _authorizationService = authorizationService;
        }

        [HttpGet]
        [Authorize]
        [RequiredScope("Project.Read", "Project.Manage")]
        public IActionResult List()
        {
            var oid = User.FindFirstValue("oid");
            if (string.IsNullOrEmpty(oid))
            {
                return Unauthorized("User OID not found in token.");
            }

            var userProjects = _database.Projects.Where(p => p.Owners.Contains(oid)).ToList();
            return Ok(userProjects);
        }

        [HttpGet("{projectId}")]
        [Authorize]
        [RequiredScope("Project.Read", "Project.Manage")]
        public async Task<IActionResult> Get(string projectId)
        {
            var project = _database.Projects.FirstOrDefault(p => p.Id == projectId);
            if (project == null)
            {
                return NotFound();
            }

            var result = await _authorizationService.AuthorizeAsync(User, project, "ProjectOwner");
            if (!result.Succeeded)
            {
                return Forbid();
            }

            return Ok(project);
        }

        [HttpPost("{projectName}")]
        [Authorize]
        [RequiredScope("Project.Manage")]
        public IActionResult Post(string projectName)
        {
            var oid = User.FindFirstValue("oid");
            if (string.IsNullOrEmpty(oid))
            {
                return Unauthorized("User OID not found in token.");
            }

            var project = new Project(projectName, oid);
            _database.Projects.Add(project);

            return CreatedAtAction(nameof(Get), new { projectId = project.Id }, project);
        }

        [HttpPut("{projectId}/{projectName}")]
        [Authorize]
        [RequiredScope("Project.Manage")]
        public async Task<IActionResult> Put(string projectId, string projectName)
        {
            var project = _database.Projects.FirstOrDefault(p => p.Id == projectId);
            if (project == null)
            {
                return NotFound();
            }

            var result = await _authorizationService.AuthorizeAsync(User, project, "ProjectOwner");
            if (!result.Succeeded)
            {
                return Forbid();
            }

            project.Name = projectName;

            return Ok();
        }

        [HttpDelete("{projectId}")]
        [Authorize]
        [RequiredScope("Project.Manage")]
        public async Task<IActionResult> Delete(string projectId)
        {
            var project = _database.Projects.FirstOrDefault(p => p.Id == projectId);
            if (project == null)
            {
                return NotFound();
            }

            var result = await _authorizationService.AuthorizeAsync(User, project, "ProjectOwner");
            if (!result.Succeeded)
            {
                return Forbid();
            }

            _database.Projects.Remove(project);

            return Ok();
        }
    }
}
