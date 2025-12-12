using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PromoCodeFactory.Core.Abstractions.Repositories;
using PromoCodeFactory.Core.Domain.Administration;
using PromoCodeFactory.WebHost.Models;

namespace PromoCodeFactory.WebHost.Controllers;

/// <summary>
///     Сотрудники
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class EmployeesController(IRepository<Employee> employeeRepository, IRepository<Role> rolesRepository)
    : ControllerBase
{
    /// <summary>
    ///     Получить данные всех сотрудников
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<List<EmployeeShortResponse>> GetEmployeesAsync()
    {
        var employees = await employeeRepository.GetAllAsync();

        var employeesModelList = employees.Select(x =>
            new EmployeeShortResponse
            {
                Id = x.Id,
                Email = x.Email,
                FullName = x.FullName
            }).ToList();

        return employeesModelList;
    }

    /// <summary>
    ///     Получить данные сотрудника по Id
    /// </summary>
    /// <returns></returns>
    [HttpGet("{id:guid}")]
    [ActionName("GetEmployeeByIdAsync")]
    public async Task<ActionResult<EmployeeResponse>> GetEmployeeByIdAsync(Guid id)
    {
        var employee = await employeeRepository.GetByIdAsync(id);

        if (employee == null)
            return NotFound();

        var employeeModel = new EmployeeResponse
        {
            Id = employee.Id,
            Email = employee.Email,
            Roles = employee.Roles.Select(x => new RoleItemResponse
            {
                Name = x.Name,
                Description = x.Description
            }).ToList(),
            FullName = employee.FullName,
            AppliedPromocodesCount = employee.AppliedPromocodesCount
        };

        return employeeModel;
    }

    /// <summary>
    ///     Создать нового сотрудника
    /// </summary>
    /// <remarks>
    ///     Пример запроса:
    ///     POST /api/v1/Employees
    ///     {
    ///     "firstName" : "Иван",
    ///     "lastName" : "Иванов",
    ///     "email" : "ivan.ivanov@example.com",
    ///     "roles": [ "Admin" ],
    ///     "appliedPromocodesCount": 10
    ///     }
    /// </remarks>
    /// <param name="employeeRequest">Данные нового сотрудника</param>
    /// <returns></returns>
    /// <response code="201">Новый сотрудник успешно создан</response>
    /// <response code="400">Некорректный запрос</response>
    /// <response code="409">Сотрудник с таким email уже существует</response>
    [HttpPost]
    [ProducesResponseType(typeof(EmployeeResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<EmployeeResponse>> CreateEmployeeAsync([FromBody] EmployeeRequest employeeRequest)
    {
        if (employeeRequest is null)
            return Problem(type: "BadRequest", title: "Invalid request", detail: "Некорректный запрос",
                statusCode: StatusCodes.Status400BadRequest);
        //check if email is exist
        var employeeByEmail = await employeeRepository.FindAsync(element =>
            string.Equals(employeeRequest.Email, element.Email, StringComparison.OrdinalIgnoreCase));
        if (employeeByEmail.Any())
            return Problem(type: "Conflict", title: "Conflict", detail: "Сотрудник с таким email уже существует",
                statusCode: StatusCodes.Status409Conflict);
        //check roles 
        var employeeRoles = await rolesRepository.FindAsync(element =>
            employeeRequest.Roles.Contains(element.Name, StringComparer.OrdinalIgnoreCase));
        var materializeRoles = employeeRoles.ToList();
        if (materializeRoles.Count != employeeRequest.Roles.Count)
            return Problem(type: "BadRequest", title: "Invalid request", detail: "Роли пользователя задана некорректно",
                statusCode: StatusCodes.Status400BadRequest);
        var employee = new Employee
        {
            Id = Guid.NewGuid(),
            Email = employeeRequest.Email,
            FirstName = employeeRequest.FirstName,
            LastName = employeeRequest.LastName,
            Roles = materializeRoles,
            AppliedPromocodesCount = employeeRequest.AppliedPromocodesCount
        };

        await employeeRepository.AddAsync(employee);

        return CreatedAtAction(nameof(GetEmployeeByIdAsync), new { id = employee.Id }, employee);
    }


    /// <summary>
    ///     Обновить данные сотрудника по Id
    /// </summary>
    /// <remarks>
    ///     Пример запроса:
    ///     PUT /api/v1/Employees/f766e2bf-340a-46ea-bff3-f1700b435895
    ///     {
    ///     "firstName" : "Иван",
    ///     "lastName" : "Иванов",
    ///     "email" : "ivan.ivanov@example.com",
    ///     "roles": [ "Admin" ],
    ///     "appliedPromocodesCount": 20
    ///     }
    /// </remarks>
    /// <param name="id">Идентификатор сотрудника</param>
    /// <param name="employeeRequest">Обновленные данные сотрудника</param>
    /// <returns></returns>
    /// <response code="200">Данные о сотруднике успешно обновлены</response>
    /// <response code="400">Некорректный запрос</response>
    /// <response code="404">Не найден сотрудник для обновления</response>
    /// <response code="409">Сотрудник с таким email уже существует</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(EmployeeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<EmployeeResponse>> UpdateEmployeeAsync(Guid id,
        [FromBody] EmployeeRequest employeeRequest)
    {
        if (employeeRequest is null)
            return Problem(type: "BadRequest", title: "Invalid request", detail: "Некорректный запрос",
                statusCode: StatusCodes.Status400BadRequest);
        var existingEmployee = await employeeRepository.GetByIdAsync(id);

        if (existingEmployee is null)
            return Problem(type: "NotFound", title: "Not found", detail: "Сотрудник не найден",
                statusCode: StatusCodes.Status404NotFound);
        //check roles 
        var employeeRoles = await rolesRepository.FindAsync(element =>
            employeeRequest.Roles.Contains(element.Name, StringComparer.OrdinalIgnoreCase));
        var materializeRoles = employeeRoles.ToList();
        if (materializeRoles.Count != employeeRequest.Roles.Count)
            return Problem(type: "BadRequest", title: "Invalid request", detail: "Роли пользователя задана некорректно",
                statusCode: StatusCodes.Status400BadRequest);
        var employeeWithSameEmail = await employeeRepository.FindAsync(element =>
            string.Equals(employeeRequest.Email, element.Email, StringComparison.OrdinalIgnoreCase) &&
            element.Id != id);

        if (employeeWithSameEmail.Any())
            return Problem(type: "Conflict", title: "Conflict", detail: "Сотрудник с таким email уже существует",
                statusCode: StatusCodes.Status409Conflict);

        //update employee
        existingEmployee.FirstName = employeeRequest.FirstName;
        existingEmployee.LastName = employeeRequest.LastName;
        existingEmployee.Email = employeeRequest.Email;
        existingEmployee.Roles = materializeRoles;
        existingEmployee.AppliedPromocodesCount = employeeRequest.AppliedPromocodesCount;

        await employeeRepository.UpdateAsync(existingEmployee);

        var response = new EmployeeResponse
        {
            Id = existingEmployee.Id,
            Email = existingEmployee.Email,
            Roles = existingEmployee.Roles.Select(x => new RoleItemResponse
            {
                Name = x.Name,
                Description = x.Description
            }).ToList(),
            FullName = existingEmployee.FullName,
            AppliedPromocodesCount = existingEmployee.AppliedPromocodesCount
        };

        return Ok(response);
    }


    /// <summary>
    ///     Удалить сотрудника по Id
    /// </summary>
    /// <remarks>
    ///     Пример запроса:
    ///     DELETE /api/v1/Employees/f766e2bf-340a-46ea-bff3-f1700b435895
    /// </remarks>
    /// <param name="id">Идентификатор сотрудника для удаления</param>
    /// <returns></returns>
    /// <response code="204">Сотрудник успешно удален</response>
    /// <response code="404">Не найден сотрудник для удаления</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteEmployeeAsync(Guid id)
    {
        var employee = employeeRepository.GetByIdAsync(id);

        if (employee is null)
            return Problem(type: "NotFound", title: "Not found", detail: "Сотрудник не найден",
                statusCode: StatusCodes.Status404NotFound);

        await employeeRepository.DeleteAsync(id);

        return NoContent();
    }
}