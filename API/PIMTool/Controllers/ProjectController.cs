using System.ComponentModel.DataAnnotations;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PIMTool.Core.Domain.Entities;
using PIMTool.Core.Exceptions;
using PIMTool.Core.Interfaces.Services;
using PIMTool.Dtos;
using PIMTool.Services;

namespace PIMTool.Controllers
{
    [ApiController]
    [Route("project")]
    public class ProjectController : ControllerBase
    {
        private readonly IProjectService _projectService;
        private readonly IGroupService _groupService;
        private readonly IEmployeeService _employeeService;
        private readonly IMapper _mapper;

        public ProjectController(IProjectService projectService, IGroupService groupService, IEmployeeService employeeService,
            IMapper mapper)
        {
            _projectService = projectService;
            _groupService = groupService;
            _employeeService = employeeService;
            _mapper = mapper;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProjectDto>> Get([FromRoute][Required] int id)
        {
            var entity = await _projectService.GetAsync(id);
            return Ok(_mapper.Map<ProjectDto>(entity));
        }

        [HttpPost]
        public async Task<IActionResult> CreateProject([FromBody] CreateProjectRequestDto request)
        {
            try
            {
                Group group = await _groupService.GetAsync(request.GroupId);

                Project project = new Project
                {
                    ProjectNumber = request.ProjectNumber,
                    Name = request.Name,
                    Customer = request.Customer,
                    Group = group,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    Status = request.Status,
                };

                foreach (var employeeId in request.Members)
                {
                    Employee employee = await _employeeService.GetAsync(employeeId);
                    if(employee.GroupId != request.GroupId)
                    {
                        throw new Exception($"An employee does not belong to group ${request.GroupId}");
                    }
                    project.ProjectEmployees.Add(new ProjectEmployee { Employee = employee, Project = project });
                }

                await _projectService.AddAsync(project);
                await _projectService.SaveChangesAsync();

                return Ok(group);
            }
            catch (BusinessException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Something went wrong");
            }

        }

        [HttpPut("{projectNumber}")]
        public async Task<IActionResult> UpdateProject(int projectNumber, [FromBody] CreateProjectRequestDto request)
        {
            try
            {
                Project existingProject = await _projectService.GetByProjectNumber(projectNumber);
                if (existingProject == null)
                {
                    return NotFound($"Project {projectNumber} not found.");
                }

                existingProject.ProjectNumber = request.ProjectNumber;
                existingProject.Name = request.Name;
                existingProject.Customer = request.Customer;
                existingProject.StartDate = request.StartDate;
                existingProject.EndDate = request.EndDate;
                existingProject.Status = request.Status;

                Group group = await _groupService.GetAsync(request.GroupId);
                existingProject.Group = group;

                existingProject.ProjectEmployees.Clear();
                foreach (var employeeId in request.Members)
                {
                    Employee employee = await _employeeService.GetAsync(employeeId);
                    if (employee.GroupId != request.GroupId)
                    {
                        throw new BusinessException($"An employee does not belong to the group of the existing project.");
                    }
                    existingProject.ProjectEmployees.Add(new ProjectEmployee { Employee = employee, Project = existingProject });
                }

                await _projectService.SaveChangesAsync();

                return Ok(existingProject);
            }
            catch (BusinessException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Something went wrong");
            }
        }

        [HttpGet("get-all")]
        public async Task<ActionResult<List<ProjectDto>>> GetAllGroup()
        {
            var projects = Task.Run(() => _projectService.Get().Select(
                p => new ProjectDto
                {
                    Id = p.Id,
                    ProjectNumber = p.ProjectNumber,
                    Name = p.Name,
                    Customer = p.Customer,
                    Status = p.Status,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                }
           )).Result;

            return Ok(projects);
        }

        [HttpGet("search")]
        public ActionResult<IEnumerable<Project>> SearchProjectByNameAndStatus([FromQuery] string? searchValue, [FromQuery] string? status)
        {
            var projects = _projectService.SearchProjectByProjectNumberOrNameOrCustomerAndStatus(searchValue, status).Select(
                p => new ProjectDto
                {
                    Id = p.Id,
                    ProjectNumber = p.ProjectNumber,
                    Name = p.Name,
                    Customer = p.Customer,
                    Status = p.Status,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                }
           );

            if (projects == null || !projects.Any())
            {
                return NotFound();
            }

            return Ok(projects);
        }

        [HttpGet("get-by-project-num/{projectNumber}")]
        public async Task<ActionResult<CreateProjectRequestDto>> GetByProjectNumber(int projectNumber)
        {
            var project = await _projectService.GetByProjectNumber(projectNumber);

            if (project == null)
            {
                return NotFound();
            }

            var projectDto = new CreateProjectRequestDto
            {
                ProjectNumber = project.ProjectNumber,
                Name = project.Name,
                Customer = project.Customer,
                Status = project.Status,
                StartDate = project.StartDate,
                EndDate = project.EndDate,
                GroupId = project.GroupId,
                Members = project.ProjectEmployees.Select(pe => pe.EmployeeId).ToList(),
            };

            return Ok(projectDto);
        }
    }
}