using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CMPG223.Dtos;
using CMPG223.Models;
using CMPG223.Services;

namespace CMPG223.Controllers
{
    public interface IEmployeeController
    {
        Task<List<EmployeeDto>> GetEmployeeRoles();
        Task<List<UserLoginDto>> GetUserLoginDtos();
        Task<List<Role>> GetRoles();
        Task<bool> UpdateEmployee(EmployeeDto employeeDto);
        Task<bool> InsertEmployee(EmployeeDto employeeDto);
    }


    public class EmployeeController : IEmployeeController
    {
        private readonly IDatabaseService _databaseService;

        public EmployeeController(IDatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        public async Task<List<EmployeeDto>> GetEmployeeRoles()
        {
            var employees = await GetEmployees();
            return await ConvertEmployeeListIntoDto(employees);
        }


        public async Task<List<UserLoginDto>> GetUserLoginDtos()
        {
            var employees = await GetEmployeesWhereActive();
            var userLogins = await GetUserLogins();

            return userLogins.Select(userLogin => new UserLoginDto()
                {
                    UserLoginId = userLogin.UserLoginId,
                    UserName = userLogin.UserName,
                    IsActive = userLogin.IsActive,
                    Employee = employees.FirstOrDefault(x => x.EmployeeId == userLogin.EmployeeFk)
                })
                .ToList();
        }

        private async Task<List<UserLogin>> GetUserLogins()
        {
            return await _databaseService.GetUserLogins();
        }

        private async Task<List<EmployeeDto>> GetEmployeesWhereActive()
        {
            var employees = await _databaseService.GetEmployeesWhereActive();
            return await ConvertEmployeeListIntoDto(employees);
        }

        public async Task<List<Role>> GetRoles()
        {
            return await _databaseService.GetRoles();
        }

        public async Task<bool> UpdateEmployee(EmployeeDto employeeDto)
        {
            Employee employee = new Employee
            {
                EmployeeId = employeeDto.EmployeeId,
                IsActive = employeeDto.IsActive,
                RoleFk = employeeDto.Role.RoleId
            };
            return await _databaseService.UpdateEmployee(employee) != 0;
        }

        public async Task<bool> InsertEmployee(EmployeeDto employeeDto)
        {
            if (CheckEmployeeDto(employeeDto))
            {
                Employee employee = new Employee
                {
                    Name = employeeDto.Name,
                    Surname = employeeDto.Surname,
                    IsActive = employeeDto.IsActive,
                    RoleFk = employeeDto.Role.RoleId
                };
                return await _databaseService.InsertEmployee(employee) != 0;
            }

            return false;
        }

        private async Task<List<Employee>> GetEmployees()
        {
            return await _databaseService.GetEmployees();
        }

        private async Task<List<EmployeeDto>> ConvertEmployeeListIntoDto(List<Employee> employees)
        {
            var roles = await GetRoles();

            return employees.Select(emp => new EmployeeDto
                {
                    EmployeeId = emp.EmployeeId,
                    Name = emp.Name,
                    Surname = emp.Surname,
                    IsActive = emp.IsActive,
                    Role = roles.FirstOrDefault(x => x.RoleId == emp.RoleFk)
                })
                .ToList();
        }

        private bool CheckEmployeeDto(EmployeeDto employeeDto)
        {
            return !string.IsNullOrEmpty(employeeDto.Name) && !string.IsNullOrEmpty(employeeDto.Surname);
        }
    }
}