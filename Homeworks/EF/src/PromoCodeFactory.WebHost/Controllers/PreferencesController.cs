using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PromoCodeFactory.DataAccess.Repositories.Interfaces;
using PromoCodeFactory.WebHost.Models;

namespace PromoCodeFactory.WebHost.Controllers;

/// <summary>
///     Предпочтения
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class PreferencesController(IRepositoryWrapper repositoryWrapper) : Controller
{
    /// <summary>
    ///     Получить все предпочтения
    /// </summary>
    /// <remarks>
    ///     Пример запроса:
    ///     GET /api/v1/Preferences
    /// </remarks>
    /// <response code="200">Получен список предпочтений</response>
    /// <returns>Список предпочтений</returns>
    [HttpGet]
    [ProducesResponseType(typeof(CustomerShortResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<PreferenceResponse>>> Get()
    {
        var preferences = await repositoryWrapper.Preferences.GetAll().ToListAsync();
        var result = preferences.Select(element => new PreferenceResponse
        {
            Name = element.Name
        }).ToList();
        return Ok(result);
    }
}