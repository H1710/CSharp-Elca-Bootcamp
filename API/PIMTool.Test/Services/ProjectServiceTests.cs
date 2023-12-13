using Microsoft.Extensions.DependencyInjection;
using Moq;
using PIMTool.Core.Domain.Entities;
using PIMTool.Core.Domain.Objects;
using PIMTool.Core.Exceptions;
using PIMTool.Core.Interfaces.Repositories;
using PIMTool.Core.Interfaces.Services;
using PIMTool.Dtos;
using PIMTool.Repositories;
using PIMTool.Services;

namespace PIMTool.Test.Services
{
    public class ProjectServiceTests : BaseTest
    {
        private Mock<IProjectRepository> mockProjectRepository;
        private Mock<IRepository<Group>> mockGroupRepository;
        private Mock<IRepository<Employee>> mockEmployeeRepository;
        private IProjectService _projectService;

        [SetUp]
        public void SetUp()
        {
            _projectService = ServiceProvider.GetRequiredService<IProjectService>();
            mockProjectRepository = new Mock<IProjectRepository>();
            mockGroupRepository = new Mock<IRepository<Group>>();
            mockEmployeeRepository = new Mock<IRepository<Employee>>();

            _projectService = new ProjectService(
                mockProjectRepository.Object,
                mockGroupRepository.Object,
                mockEmployeeRepository.Object
            );
        }

        private List<Project> CreateMockProjects()
        {
            List<Project> projects = new List<Project>();
            string[] status = { "NEW", "INP", "PLA", "FIN" };

            for (int i = 1; i <= 100; i++)
            {
                Project request = new Project
                {
                    ProjectNumber = i,
                    Name = "Project " + i.ToString(),
                    Customer = "Customer " + i.ToString(),
                    Status = status[i % 4],
                    StartDate = DateTime.Parse("2023-12-05"),
                    EndDate = DateTime.Parse("2023-12-05")
                };
                projects.Add(request);
            }

            return projects;
        }

        [Test]
        public async Task GetByProjectNumber_ShouldReturnProject_WhenProjectExist()
        {
            // Arrange
            int projectNumber = 1;
            Project project = new Project
            {
                Id = projectNumber,
                ProjectNumber = projectNumber,
                Name = "Test",
                Customer = "QuachHoangHuy",
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(30),
                Group = new Group()
                {
                    Id = 1,
                    GroupLeaderId = 1,
                },
                ProjectEmployees = new List<ProjectEmployee>(),
            };

            mockProjectRepository.Setup(p => p.GetByProjectNumber(projectNumber))
                .ReturnsAsync(project);

            // Act
            var projectFinded = await _projectService.GetByProjectNumber(projectNumber);

            // Assert
            Assert.AreEqual(projectNumber, projectFinded.ProjectNumber);
        }

        [Test]
        public async Task GetByProjectNumber_ShouldReturnProject_WhenProjectNotExist()
        {
            // Arrange
            int projectNumber = 2;
            Project project = new Project
            {
                Id = projectNumber,
                ProjectNumber = projectNumber,
                Name = "Test",
                Customer = "QuachHoangHuy",
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(30),
                Group = new Group()
                {
                    Id = 1,
                    GroupLeaderId = 1,
                },
                ProjectEmployees = new List<ProjectEmployee>(),
            };

            mockProjectRepository.Setup(p => p.GetByProjectNumber(projectNumber))
                .ReturnsAsync((Project)null);

            // Act & Assert
            Assert.ThrowsAsync<BusinessException>(async () =>
            {
                await _projectService.GetByProjectNumber(projectNumber);
            });
        }

        [Test]
        public async Task SearchProjectByProjectNumberOrNameOrCustomerAndStatus_WithValidParameters_ReturnsPageList()
        {
            // Arrange
            string searchValue = "P";
            string status = "INP";
            int pageNumber = 1;
            int pageSize = 5;
            List<Project> projects = CreateMockProjects();
            IQueryable<Project> queryableProjects = projects.AsQueryable();

            mockProjectRepository.Setup(repo => repo.Get()).Returns(queryableProjects);

            // Act
            var result = await _projectService.SearchProjectByProjectNumberOrNameOrCustomerAndStatus(searchValue, status, pageNumber, pageSize);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<PageList<Project>>(result);
            Assert.IsTrue(result.Items.All(p => p.Name.Contains(searchValue, StringComparison.OrdinalIgnoreCase) || p.Customer.Contains(searchValue, StringComparison.OrdinalIgnoreCase) || p.ProjectNumber.ToString().Contains(searchValue, StringComparison.OrdinalIgnoreCase)));
            Assert.IsTrue(result.Items.All(p => p.Status == status));
        }

        [Test]
        public async Task SearchProjectByProjectNumberOrNameOrCustomerAndStatus_WithEmptyValue_ReturnsPageList()
        {
            // Arrange
            string searchValue = "";
            string status = "";
            int pageNumber = 1;
            int pageSize = 5;
            List<Project> projects = CreateMockProjects();
            IQueryable<Project> queryableProjects = projects.AsQueryable();

            mockProjectRepository.Setup(repo => repo.Get()).Returns(queryableProjects);

            // Act
            var result = await _projectService.SearchProjectByProjectNumberOrNameOrCustomerAndStatus(searchValue, status, pageNumber, pageSize);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(projects.Count(), result.TotalCount);
        }
        

        [Test]
        public async Task AddAsync_ValidData_SuccessfullyAdded()
        {
            // Arrange
            var group = new Group { Id = 1, GroupLeaderId = 1 };
            var project = new Project
            {
                ProjectNumber = 1,
                Name = "Project testing",
                Customer = "Customer testing",
                Status = "NEW",
                GroupId = 1,
                StartDate = DateTime.Parse("2023-12-05"),
                EndDate = DateTime.Parse("2023-12-05")
            };
            var employeeIds = new List<int> { 1, 2, 3 };

            mockGroupRepository.Setup(repo => repo.GetAsync(group.Id, It.IsAny<CancellationToken>())).ReturnsAsync(group);
            mockProjectRepository.Setup(repo => repo.GetByProjectNumber(project.ProjectNumber)).ReturnsAsync((Project)null);

            foreach (var employeeId in employeeIds)
            {
                var employee = new Employee { Id = employeeId, GroupId = group.Id, Visa = $"AA{employeeId}", FirstName = $"F{employeeId}", LastName = $"L{employeeId}", BirthDate = DateTime.Parse("2023-12-05") };
                mockEmployeeRepository.Setup(repo => repo.GetAsync(employee.Id, It.IsAny<CancellationToken>())).ReturnsAsync(employee);
            }

            // Act
            await _projectService.AddAsync(project, employeeIds);

            // Assert
            mockProjectRepository.Verify(repo => repo.AddAsync(It.IsAny<Project>(), It.IsAny<CancellationToken>()), Times.Once);
            mockProjectRepository.Verify(repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        }
        [Test]
        public async Task AddAsync_ValidData_DuplicatedProjectNumber()
        {
            // Arrange
            var group = new Group { Id = 1, GroupLeaderId= 1 };
            var project = new Project {
                ProjectNumber = 1,
                Name = "Project testing",
                Customer = "Customer testing",
                Status = "NEW",
                GroupId = 1,
                StartDate = DateTime.Parse("2023-12-05"),
                EndDate = DateTime.Parse("2023-12-05")
            };
            var employeeIds = new List<int> { 1, 2, 3 };

            mockGroupRepository.Setup(repo => repo.GetAsync(group.Id, It.IsAny<CancellationToken>())).ReturnsAsync(group);
            mockProjectRepository.Setup(repo => repo.GetByProjectNumber(project.ProjectNumber)).ReturnsAsync((Project)project);

            foreach (var employeeId in employeeIds)
            {
                var employee = new Employee { Id = employeeId, GroupId = group.Id, Visa = $"AA{employeeId}", FirstName = $"F{employeeId}", LastName = $"L{employeeId}", BirthDate = DateTime.Parse("2023-12-05") };
                mockEmployeeRepository.Setup(repo => repo.GetAsync(employee.Id, It.IsAny<CancellationToken>())).ReturnsAsync(employee);
            }

            // Act & Assert
            Assert.ThrowsAsync<BusinessException>(async () =>
            {
                await _projectService.AddAsync(project, employeeIds);
            });

        }

        [Test]
        public async Task UpdateProject_SuccessfulUpdate()
        {
            // Arrange
            var existingProject = new Project
            {
                ProjectNumber = 1,
                Name = "Project testing",
                Customer = "Customer testing",
                Status = "NEW",
                GroupId = 1,
                ProjectEmployees = new List<ProjectEmployee>(),
                StartDate = DateTime.Parse("2023-12-05"),
                EndDate = DateTime.Parse("2023-12-05"),
                Version = BitConverter.GetBytes(new DateTime(12).Ticks),
            };
            var newProject = new Project 
            {
                ProjectNumber = 1,
                Name = "Project testing update",
                Customer = "Customer testing update",
                Status = "PLA",
                GroupId = 2,
                ProjectEmployees = new List<ProjectEmployee>(),
                StartDate = DateTime.Parse("2023-11-05"),
                EndDate = DateTime.Parse("2023-10-05"),
                Version = BitConverter.GetBytes(new DateTime(12).Ticks),
            };

            mockProjectRepository.Setup(repo => repo.GetByProjectNumber(existingProject.ProjectNumber)).ReturnsAsync(existingProject);
            mockGroupRepository.Setup(repo => repo.GetAsync(newProject.GroupId, It.IsAny<CancellationToken>())).ReturnsAsync(new Group());

            // Act
            await _projectService.UpdateProject(existingProject.ProjectNumber, newProject);

            // Assert
            mockProjectRepository.Verify(repo => repo.UpdateProject(It.IsAny<Project>()), Times.Once);
            mockProjectRepository.Verify(repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task UpdateProject_SuccessfulUpdate_DbUpdateConcurrency()
        {
            // Arrange
            var existingProject = new Project
            {
                ProjectNumber = 1,
                Name = "Project testing",
                Customer = "Customer testing",
                Status = "NEW",
                GroupId = 1,
                ProjectEmployees = new List<ProjectEmployee>(),
                StartDate = DateTime.Parse("2023-12-05"),
                EndDate = DateTime.Parse("2023-12-05"),
                Version = BitConverter.GetBytes(new DateTime(12).Ticks),
            };
            var newProject = new Project
            {
                ProjectNumber = 1,
                Name = "Project testing update",
                Customer = "Customer testing update",
                Status = "PLA",
                GroupId = 2,
                ProjectEmployees = new List<ProjectEmployee>(),
                StartDate = DateTime.Parse("2023-11-05"),
                EndDate = DateTime.Parse("2023-10-05"),
                Version = BitConverter.GetBytes(new DateTime(11).Ticks),
            };

            mockProjectRepository.Setup(repo => repo.GetByProjectNumber(existingProject.ProjectNumber)).ReturnsAsync(existingProject);
            mockGroupRepository.Setup(repo => repo.GetAsync(newProject.GroupId, It.IsAny<CancellationToken>())).ReturnsAsync(new Group());

            // Act & Assert
            Assert.ThrowsAsync<BusinessException>(async () =>
            {
                await _projectService.UpdateProject(existingProject.ProjectNumber, newProject);
            });
            
        }

        [Test]
        public async Task DeleteProjects_ValidProjectNumbers_SuccessfullyDeleted()
        {
            // Arrange
            var projectNumbers = new int[] { 1, 2, 3 };
            List<Project> projects = CreateMockProjects();

            mockProjectRepository.Setup(repo => repo.GetByProjectNumber(It.IsAny<int>()))
                .Returns<int>(projectNumber => Task.FromResult(projects.SingleOrDefault(p => p.ProjectNumber == projectNumber)));

            // Act
            await _projectService.DeleteProjects(projectNumbers);

            // Assert
            mockProjectRepository.Verify(repo => repo.Delete(It.IsAny<Project[]>()), Times.Once);
            mockProjectRepository.Verify(repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

    }
}