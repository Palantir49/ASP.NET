using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PromoCodeFactory.Core.Abstractions.Repositories;
using PromoCodeFactory.Core.Domain.PromoCodeManagement;
using PromoCodeFactory.WebHost.Models;
using PromoCodeFactory.WebHost.Services;

namespace PromoCodeFactory.WebHost.Controllers;

/// <summary>
///     Партнеры
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class PartnersController(IRepository<Partner> partnersRepository) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<PartnerResponse>>> GetPartnersAsync()
    {
        var partners = await partnersRepository.GetAllAsync();

        var response = partners.Select(x => new PartnerResponse
        {
            Id = x.Id,
            Name = x.Name,
            NumberIssuedPromoCodes = x.NumberIssuedPromoCodes,
            IsActive = true,
            PartnerLimits = x.PartnerLimits
                .Select(y => new PartnerPromoCodeLimitResponse
                {
                    Id = y.Id,
                    PartnerId = y.PartnerId,
                    Limit = y.Limit,
                    CreateDate = y.CreateDate.ToString("dd.MM.yyyy hh:mm:ss"),
                    EndDate = y.EndDate.ToString("dd.MM.yyyy hh:mm:ss"),
                    CancelDate = y.CancelDate?.ToString("dd.MM.yyyy hh:mm:ss")
                }).ToList()
        });

        return Ok(response);
    }

    [HttpGet("{id:guid}/limits/{limitId:guid}")]
    public async Task<ActionResult<PartnerPromoCodeLimit>> GetPartnerLimitAsync(Guid id, Guid limitId)
    {
        var partner = await partnersRepository.GetByIdAsync(id);

        if (partner == null)
            return NotFound();

        var limit = partner.PartnerLimits
            .FirstOrDefault(x => x.Id == limitId);

        var response = new PartnerPromoCodeLimitResponse
        {
            Id = limit.Id,
            PartnerId = limit.PartnerId,
            Limit = limit.Limit,
            CreateDate = limit.CreateDate.ToString("dd.MM.yyyy hh:mm:ss"),
            EndDate = limit.EndDate.ToString("dd.MM.yyyy hh:mm:ss"),
            CancelDate = limit.CancelDate?.ToString("dd.MM.yyyy hh:mm:ss")
        };

        return Ok(response);
    }

    [HttpPost("{id:guid}/limits")]
    public async Task<IActionResult> SetPartnerPromoCodeLimitAsync(Guid id, SetPartnerPromoCodeLimitRequest request)
    {
        var partner = await partnersRepository.GetByIdAsync(id);

        if (partner == null)
            return NotFound();

        //Если партнер заблокирован, то нужно выдать исключение
        if (!partner.IsActive)
            return BadRequest("Данный партнер не активен");

        // Используем сервис для основной логики
        PartnerLimitService partnerLimitService = new();

        var result = partnerLimitService.ProcessLimitAsync(
            partner,
            request,
            DateTime.Now);

        if (!result.IsSuccess)
            return BadRequest(result.ErrorMessage);


        await partnersRepository.UpdateAsync(partner);

        return CreatedAtAction(nameof(GetPartnerLimitAsync), new { id = partner.Id, limitId = result.NewLimitId },
            null);
    }

    [HttpPost("{id:guid}/canceledLimits")]
    public async Task<IActionResult> CancelPartnerPromoCodeLimitAsync(Guid id)
    {
        var partner = await partnersRepository.GetByIdAsync(id);

        if (partner == null)
            return NotFound();

        //Если партнер заблокирован, то нужно выдать исключение
        if (!partner.IsActive)
            return BadRequest("Данный партнер не активен");

        //Отключение лимита
        var activeLimit = partner.PartnerLimits.FirstOrDefault(x =>
            !x.CancelDate.HasValue);

        if (activeLimit != null) activeLimit.CancelDate = DateTime.Now;

        await partnersRepository.UpdateAsync(partner);

        return NoContent();
    }
}