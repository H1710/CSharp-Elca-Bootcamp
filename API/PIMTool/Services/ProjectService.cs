using Microsoft.EntityFrameworkCore;
using PIMTool.Core.Domain.Entities;
using PIMTool.Core.Domain.Objects;
using PIMTool.Core.Exceptions;
using PIMTool.Core.Interfaces.Repositories;
using PIMTool.Core.Interfaces.Services;
using PIMTool.Database;
using PIMTool.Dtos;
using PIMTool.Repositories;

namespace PIMTool.Services
{
    public class ProjectService : IProjectService
    {
        private readonly IProjectRepository _repository;
        private readonly IRepository<Group> _groupRepository;
        private readonly IRepository<Employee> _employeeRepository;
        public ProjectService(IProjectRepository repository, IRepository<Group> groupRepository, IRepository<Employee> employeeRepository)
        {
            _repository = repository;
            _groupRepository = groupRepository;
            _employeeRepository = employeeRepository;
        }

        public async Task<Project?> GetAsync(int id, CancellationToken cancellationToken = default)
        {
            var entity = await _repository.GetAsync(id, cancellationToken);
            return entity;
        }

        public async Task AddAsync(Project entity, List<int> employeeIds, CancellationToken cancellationToken = default)
        {
            try
            {
                Group group = await _groupRepository.GetAsync(entity.GroupId);
                if (group == null)
                {
                    throw new BusinessException("Group does not exist");
                }

                Project project = await _repository.GetByProjectNumber(entity.ProjectNumber);
                if (project != null)
                {
                    throw new BusinessException($"Project number {project.ProjectNumber} already exist");
                }
                entity.Group = group;

                foreach (var employeeId in employeeIds)
                {
                    Employee employee = await _employeeRepository.GetAsync(employeeId);
                    if (employee.GroupId != entity.GroupId)
                    {
                        throw new BusinessException($"An employee does not belong to group ${entity.GroupId}");
                    }
                    entity.ProjectEmployees.Add(new ProjectEmployee { Employee = employee, Project = entity });
                }

                await _repository.AddAsync(entity, cancellationToken);
                await _repository.SaveChangesAsync(cancellationToken);
            } 
            catch (BusinessException)
            {
                throw;
            }
            catch (Exception)
            {
                throw;
            }
            
        }

        public async Task<PageList<Project>> Get(int pageNumber = 1, int pageSize = 5)
        {
            IQueryable<Project> queryableSource;
            queryableSource = _repository.Get();
            queryableSource = queryableSource.OrderBy(project => project.ProjectNumber);
            return PageList<Project>.ToPagedList(queryableSource, pageNumber, pageSize);
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _repository.SaveChangesAsync(cancellationToken);
        }

        public async Task<PageList<Project>> SearchProjectByProjectNumberOrNameOrCustomerAndStatus(string searchValue, string status, int pageNumber = 1, int pageSize = 5, CancellationToken cancellationToken = default)
        {
            IQueryable<Project> queryableSource;
            if (string.IsNullOrEmpty(searchValue) && string.IsNullOrEmpty(status))
            {
                queryableSource = _repository.Get();
            } 
            else
            {
                queryableSource = _repository.SearchProjectByProjectNumberOrNameOrCustomerAndStatus(searchValue, status);

            }
            queryableSource = queryableSource.OrderBy(project => project.ProjectNumber);
            return PageList<Project>.ToPagedList(queryableSource, pageNumber, pageSize);
        }

        public async Task<Project> GetByProjectNumber(int projectNumber, CancellationToken cancellationToken = default)
        {
            try
            {
                var project = await _repository.GetByProjectNumber(projectNumber);
                if (project == null)
                {
                    throw new BusinessException($"Project number {projectNumber} not found");
                }
                return project;
            }
            catch (BusinessException)
            {
                throw;
            }
            catch (Exception)
            {
                throw;
            }
            
        }

        public async Task DeleteProjects(int[] projectNumbers)
        {
            try
            {
                if (projectNumbers == null || projectNumbers.Length == 0)
                {
                    throw new BusinessException("No project IDs provided for deletion.");
                }

                List<Project> projects = new List<Project>();
                foreach (int projectNumber in projectNumbers)
                {
                    var project = await _repository.GetByProjectNumber(projectNumber);
                    if (project == null)
                    {
                        throw new BusinessException($"Project with number {projectNumber} not found.");
                    }

                    projects.Add(project);
                }
                _repository.Delete(projects.ToArray());
                await _repository.SaveChangesAsync();
            }
            catch (BusinessException) 
            { 
                throw; 
            }
            catch (Exception)
            {
                throw;
            }
            
        }

        public async Task UpdateProject(int projectNumber, Project newProject)
        {
            try
            {
                Project existingProject = await _repository.GetByProjectNumber(projectNumber);
                if (existingProject == null)
                {
                    throw new BusinessException($"Project {projectNumber} not found.");
                }

                if (!newProject.Version.SequenceEqual(existingProject.Version))
                {
                    throw new BusinessException("Concurrency conflict. The project has been modified by another user.");
                }
                existingProject.ProjectNumber = newProject.ProjectNumber;
                existingProject.Name = newProject.Name;
                existingProject.Customer = newProject.Customer;
                existingProject.StartDate = newProject.StartDate;
                existingProject.EndDate = newProject.EndDate;
                existingProject.Status = newProject.Status;
                existingProject.ProjectEmployees = newProject.ProjectEmployees;

                Group group = await _groupRepository.GetAsync(newProject.GroupId);
                if(group == null)
                {
                    throw new BusinessException($"Group {newProject.GroupId} does not exist");
                }
                existingProject.Group = group;

                await _repository.UpdateProject(existingProject);
                await _repository.SaveChangesAsync();
            } 
            catch (DbUpdateConcurrencyException ex)
            {
                throw new BusinessException("Concurrency conflict. The project has been modified by another user.");
            }
            catch (BusinessException)
            {
                throw;
            }
            catch (Exception)
            {
                throw;
            }
            
        }
    }
}