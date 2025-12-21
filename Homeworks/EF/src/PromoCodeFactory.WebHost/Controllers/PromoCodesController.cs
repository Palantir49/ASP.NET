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
///     Промокоды
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class PromoCodesController(IRepositoryWrapper repositoryWrapper)
    : ControllerBase
{
    /// <summary>
    ///     Получить все промокоды
    /// </summary>
    /// <remarks>
    ///     Пример запроса:
    ///     GET /api/v1/PromoCodes
    /// </remarks>
    /// <response code="200">Получен список промокодов</response>
    /// <returns>Список промокодов</returns>
    [HttpGet]
    [ProducesResponseType(typeof(CustomerShortResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<PromoCodeShortResponse>>> GetPromoCodesAsync()
    {
        //TODO: Получить все промокоды 
        var promocodes = await repositoryWrapper.PromoCodes.GetAll().ToListAsync();
        var result = promocodes.Select(x => new PromoCodeShortResponse
        {
            Id = x.Id, BeginDate = x.BeginDate.ToString(), EndDate = x.EndDate.ToString(), Code = x.Code,
            PartnerName = x.PartnerName, ServiceInfo = x.ServiceInfo
        }).ToList();
        return Ok(result);
    }

    /// <summary>
    ///     Создать промокод и выдать его клиентам с указанным предпочтением
    /// </summary>
    /// <remarks>
    ///     Пример запроса:
    ///     POST /api/v1/PromoCodes
    ///     {
    ///     "serviceInfo": "Test",
    ///     "partnerName": "Test",
    ///     "promoCode": "111111",
    ///     "preference": "Дети"
    ///     }
    /// </remarks>
    /// <param name="request">Запрос на выдачу промокода</param>
    /// <response code="201">Промокоды созданы</response>
    /// <response code="400">Некорректный запрос</response>
    /// <returns>201</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(PromoCodeShortResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<PromoCodeShortResponse>> GivePromoCodesToCustomersWithPreferenceAsync(
        GivePromoCodeRequest request)
    {
        if (request is null)
            return Problem(type: "BadRequest", title: "Invalid request", detail: "Некорректный запрос",
                statusCode: StatusCodes.Status400BadRequest);

        var customers = await repositoryWrapper.Customers.GetAll().Include(x => x.Preferences).ToListAsync();

        var customersWithPreference = customers
            .Where(element => element.Preferences.Any(p => p.Name.ToUpper() == request.Preference.ToUpper())).ToList();

        foreach (var customer in customersWithPreference)
        {
            var customerPromo = new PromoCode
            {
                Id = Guid.NewGuid(),
                Code = request.PromoCode,
                BeginDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(1),
                PartnerName = request.PartnerName,
                ServiceInfo = request.ServiceInfo,
                CustomerId = customer.Id
            };
            await repositoryWrapper.PromoCodes.CreateAsync(customerPromo);
        }

        await repositoryWrapper.SaveChangesAsync();
        return Created();
    }
}