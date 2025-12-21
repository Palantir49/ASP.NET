using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PromoCodeFactory.Core.Domain.PromoCodeManagement;
using PromoCodeFactory.DataAccess.Repositories.Interfaces;
using PromoCodeFactory.WebHost.Models;

namespace PromoCodeFactory.WebHost.Controllers;

/// <summary>
///     Клиенты
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class CustomersController(IRepositoryWrapper repositoryWrapper)
    : ControllerBase
{
    /// <summary>
    ///     Получение списка клиентов
    /// </summary>
    /// <remarks>
    ///     Пример запроса:
    ///     GET /api/v1/Customers
    /// </remarks>
    /// <response code="200">Получен список клиентов</response>
    /// <returns>Список клиентов</returns>
    [HttpGet]
    [ProducesResponseType(typeof(CustomerShortResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CustomerShortResponse>>> GetCustomersAsync()
    {
        var customers = await repositoryWrapper.Customers.GetAll().ToListAsync();

        var customerResponseList = customers.Select(x =>
            new CustomerShortResponse
            {
                Id = x.Id,
                Email = x.Email,
                FirstName = x.FirstName,
                LastName = x.LastName
            }).ToList();

        return Ok(customerResponseList);
    }

    /// <summary>
    ///     Получение клиента по id
    /// </summary>
    /// <param name="id">id клиента</param>
    /// <remarks>
    ///     Пример запроса
    ///     GET /api/v1/Customers/a6c8c6b1-4349-45b0-ab31-244740aaf0f0
    /// </remarks>
    /// <response code="200">Получен клиент</response>
    /// <response code="404">Не найден клиент по указанному id</response>
    /// <returns>Данные клиента по id</returns>
    [HttpGet("{id:guid}")]
    [ActionName("GetCustomerByIdAsync")]
    [ProducesResponseType(typeof(CustomerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CustomerResponse>> GetCustomerByIdAsync(Guid id)
    {
        var customer = await repositoryWrapper.Customers.GetById(id).Include(x => x.PromoCodes)
            .Include(x => x.Preferences).FirstOrDefaultAsync();
        if (customer == null)
            return Problem(type: "NotFound", title: "Not Found", detail: "Не найден клиент по указанному id");
        var customerResponse = new CustomerResponse
        {
            Id = customer.Id,
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            Email = customer.Email,
            PromoCodes = customer.PromoCodes.Select(x => new PromoCodeShortResponse
            {
                Id = x.Id,
                Code = x.Code,
                BeginDate = x.BeginDate.ToString("yyyy-MM-dd"),
                EndDate = x.EndDate.ToString("yyyy-MM-dd"),
                PartnerName = x.PartnerName,
                ServiceInfo = x.ServiceInfo
            }).ToList(),
            Preferences = customer.Preferences.Select(x => new PreferenceResponse { Name = x.Name }).ToList()
        };
        return Ok(customerResponse);
    }

    /// <summary>
    ///     Создание нового клиента
    /// </summary>
    /// <remarks>
    ///     Пример запроса:
    ///     POST /api/v1/Customers
    ///     {
    ///     "firstName" : "Иван",
    ///     "lastName" : "Иванов",
    ///     "email" : "ivan.ivanov@example.com",
    ///     "PreferenceIds": [ "ef7f299f-92d7-459f-896e-078ed53ef99c", "c4bda62e-fc74-4256-a956-4760b3858cbd" ]
    ///     }
    /// </remarks>
    /// <response code="201">Новый клиент успешно создан</response>
    /// <response code="400">Некорректный запрос</response>
    /// <response code="409">Клиент с таким email уже существует</response>
    /// <param name="request">Данные нового клиента</param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(typeof(CustomerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CustomerResponse>> CreateCustomerAsync(CreateOrEditCustomerRequest request)
    {
        if (request is null)
            return Problem(type: "BadRequest", title: "Invalid request", detail: "Некорректный запрос",
                statusCode: StatusCodes.Status400BadRequest);

        var customer = await repositoryWrapper.Customers.FindByCondition(element =>
            element.Email.ToUpper() == request.Email.ToUpper()).ToListAsync();

        if (customer.Count != 0)
            return Problem(type: "Conflict", title: "Conflict", detail: "Клиент с таким email уже существует",
                statusCode: StatusCodes.Status409Conflict);


        var preferences =
            await repositoryWrapper.Preferences.FindByCondition(element =>
                request.PreferenceIds.Contains(element.Id)).ToListAsync();
        var newCustomer = new Customer
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName, LastName = request.LastName, Email = request.Email,
            Preferences = preferences
        };
        newCustomer = await repositoryWrapper.Customers.CreateAsync(newCustomer);
        await repositoryWrapper.SaveChangesAsync();

        var customerResponse = new CustomerResponse
        {
            Id = newCustomer.Id,
            FirstName = newCustomer.FirstName, LastName = newCustomer.LastName, Email = newCustomer.Email,
            PromoCodes = newCustomer.PromoCodes.Select(x => new PromoCodeShortResponse
            {
                Id = x.Id,
                Code = x.Code,
                BeginDate = x.BeginDate.ToString("yyyy-MM-dd"),
                EndDate = x.EndDate.ToString("yyyy-MM-dd"),
                PartnerName = x.PartnerName,
                ServiceInfo = x.ServiceInfo
            }).ToList(),
            Preferences = newCustomer.Preferences.Select(x => new PreferenceResponse { Name = x.Name }).ToList()
        };

        return CreatedAtAction(nameof(GetCustomerByIdAsync), new { id = customerResponse.Id }, customerResponse);
    }

    /// <summary>
    ///     Обновить данные клиента по Id
    /// </summary>
    /// <remarks>
    ///     Пример запроса:
    ///     PUT /api/v1/Customers/a6c8c6b1-4349-45b0-ab31-244740aaf0f0
    ///     {
    ///     "firstName" : "Vlad",
    ///     "lastName" : "Vladimirov",
    ///     "email" : "vlad.ivanov@example.com",
    ///     "PreferenceIds": [ "76324c47-68d2-472d-abb8-33cfa8cc0c84"]
    ///     }
    /// </remarks>
    /// <param name="id">Идентификатор клиента</param>
    /// <param name="request">Обновленные данные о клиенте</param>
    /// <returns></returns>
    /// <response code="200">Данные о клиенте успешно обновлены</response>
    /// <response code="400">Некорректный запрос</response>
    /// <response code="404">Не найден клиент для обновления</response>
    /// <response code="409">Клиент с таким email уже существует</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(CustomerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> EditCustomersAsync(Guid id, CreateOrEditCustomerRequest request)
    {
        if (request is null || id == Guid.Empty)
            return Problem(type: "BadRequest", title: "Invalid request", detail: "Некорректный запрос",
                statusCode: StatusCodes.Status400BadRequest);
        var customer = await repositoryWrapper.Customers.GetById(id).Include(x => x.Preferences).FirstOrDefaultAsync();
        if (customer == null)
            return Problem(type: "NotFound", title: "Not found", detail: "Клиент не найден",
                statusCode: StatusCodes.Status404NotFound);

        var customerWithSameEmail = await repositoryWrapper.Customers.FindByCondition(element =>
            request.Email.ToUpper() == element.Email.ToUpper() &&
            element.Id != id).FirstOrDefaultAsync();

        if (customerWithSameEmail != null)
            return Problem(type: "Conflict", title: "Conflict", detail: "Клиент с таким email уже существует",
                statusCode: StatusCodes.Status409Conflict);
        var preferences =
            await repositoryWrapper.Preferences.FindByCondition(element =>
                request.PreferenceIds.Contains(element.Id)).ToListAsync();
        if (preferences.Count != request.PreferenceIds.Count)
            return Problem(type: "BadRequest", title: "Invalid request",
                detail: "Одно или несколько указанных предпочтений не существуют",
                statusCode: StatusCodes.Status400BadRequest);
        customer.Preferences.Clear();
        customer.FirstName = request.FirstName;
        customer.LastName = request.LastName;
        customer.Email = request.Email;
        customer.Preferences = preferences;

        //update
        await repositoryWrapper.Customers.UpdateAsync(customer);
        await repositoryWrapper.SaveChangesAsync();
        var customerResponse = new CustomerResponse
        {
            Id = customer.Id,
            FirstName = customer.FirstName, LastName = customer.LastName, Email = customer.Email,
            PromoCodes = customer.PromoCodes.Select(x => new PromoCodeShortResponse
            {
                Id = x.Id,
                Code = x.Code,
                BeginDate = x.BeginDate.ToString("yyyy-MM-dd"),
                EndDate = x.EndDate.ToString("yyyy-MM-dd"),
                PartnerName = x.PartnerName,
                ServiceInfo = x.ServiceInfo
            }).ToList(),
            Preferences = customer.Preferences.Select(x => new PreferenceResponse { Name = x.Name }).ToList()
        };
        return Ok(customerResponse);
    }

    /// <summary>
    ///     Удаление клиента
    /// </summary>
    /// <remarks>
    ///     Пример запроса:
    ///     DELETE /api/v1/Customers/a6c8c6b1-4349-45b0-ab31-244740aaf0f0
    /// </remarks>
    /// <param name="id">Id клиента для удаления</param>
    /// <response code="204">Сотрудник успешно удален</response>
    /// <response code="404">Не найден сотрудник для удаления</response>
    /// <returns></returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(CustomerResponse), StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteCustomer(Guid id)
    {
        if (id == Guid.Empty)
            return Problem(type: "BadRequest", title: "Invalid request", detail: "Некорректный запрос");
        var customer = await repositoryWrapper.Customers.GetById(id).FirstOrDefaultAsync();
        if (customer == null)
            return Problem(type: "NotFound", title: "Not found", detail: "Клиент не найден");
        await repositoryWrapper.Customers.DeleteAsync(customer);
        await repositoryWrapper.SaveChangesAsync();
        return NoContent();
    }
}