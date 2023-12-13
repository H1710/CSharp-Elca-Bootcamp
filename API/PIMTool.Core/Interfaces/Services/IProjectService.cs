using PIMTool.Core.Domain.Entities;
using PIMTool.Core.Domain.Objects;

namespace PIMTool.Core.Interfaces.Services
{
    public interface IProjectService
    {
        Task<Project?> GetAsync(int id, CancellationToken cancellationToken = default);

        Task AddAsync(Project entity, List<int> employeeIds, CancellationToken cancellationToken = default);

        Task SaveChangesAsync(CancellationToken cancellationToken = default);

        Task<PageList<Project>> Get(int pageNumber = 1, int pageSize = 5);

        Task<PageList<Project>> SearchProjectByProjectNumberOrNameOrCustomerAndStatus(string searchValue, string status, int pageNumber, int pageSize, CancellationToken cancellationToken = default);

        Task<Project> GetByProjectNumber(int projectNumber, CancellationToken cancellationToken = default);
        Task DeleteProjects(int[] projectNumbers);

        Task UpdateProject(int projectNumber, Project newProject);

    }
}