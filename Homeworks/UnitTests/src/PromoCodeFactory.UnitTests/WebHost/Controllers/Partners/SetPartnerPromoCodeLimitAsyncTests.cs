using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PromoCodeFactory.Core.Abstractions.Repositories;
using PromoCodeFactory.Core.Domain.PromoCodeManagement;
using PromoCodeFactory.WebHost.Controllers;
using PromoCodeFactory.WebHost.Models;
using Xunit;

namespace PromoCodeFactory.UnitTests.WebHost.Controllers.Partners;

public class SetPartnerPromoCodeLimitAsyncTests
{
    private readonly PartnersController _partnersController;
    private readonly Mock<IRepository<Partner>> _repositoryMock;

    public SetPartnerPromoCodeLimitAsyncTests()
    {
        _repositoryMock = new Mock<IRepository<Partner>>();
        _partnersController = new PartnersController(_repositoryMock.Object);
    }


    /// <summary>
    ///     Если партнер не найден, то также нужно выдать ошибку 404;
    /// </summary>
    [Fact]
    public async Task SetPartnerPromoCodeLimitAsync_WhenPartnerIsNull_ShouldReturnNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        var partnerPromoCodeLimitRequestFixture = new Fixture().Create<SetPartnerPromoCodeLimitRequest>();
        _repositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Partner)null);
        // Act
        var result = await _partnersController.SetPartnerPromoCodeLimitAsync(id, partnerPromoCodeLimitRequestFixture);
        //Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    /// <summary>
    ///     Если партнер заблокирован, то есть поле IsActive=false в классе Partner, то также нужно выдать ошибку 400;
    /// </summary>
    [Fact]
    public async Task SetPartnerPromoCodeLimitAsync_WhenPartnerIsNotActive_ShouldReturnBadRequest()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existingPartner = new Fixture().Build<Partner>().With(x => x.IsActive, false).Without(x => x.PartnerLimits)
            .Create();
        _repositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(existingPartner);
        var partnerPromoCodeLimitRequestFixture = new Fixture().Create<SetPartnerPromoCodeLimitRequest>();

        // Act
        var result = await _partnersController.SetPartnerPromoCodeLimitAsync(id, partnerPromoCodeLimitRequestFixture);
        //Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }


    /// <summary>
    ///     Если партнеру выставляется лимит, то мы должны обнулить количество промокодов, которые партнер выдал
    ///     NumberIssuedPromoCodes
    /// </summary>
    [Fact]
    public async Task SetPartnerPromoCodeLimitAsync_IfPartnerLimitIsNull_ShouldResetNumberIssuedPromoCodes()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existingPartner = new Fixture().Build<Partner>().With(x => x.PartnerLimits, new List<PartnerPromoCodeLimit>
            {
                new() { CancelDate = null }
            }).With(x => x.IsActive, true)
            .With(x => x.NumberIssuedPromoCodes, 10).Create();
        _repositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(existingPartner);
        var partnerPromoCodeLimitRequestFixture = new Fixture().Create<SetPartnerPromoCodeLimitRequest>();
        // Act
        var result = await _partnersController.SetPartnerPromoCodeLimitAsync(id, partnerPromoCodeLimitRequestFixture);
        //Assert
        result.Should().BeOfType<CreatedAtActionResult>();
        existingPartner.NumberIssuedPromoCodes.Should().Be(0);
    }

    /// <summary>
    ///     Если лимит закончился, то количество не обнуляется
    /// </summary>
    [Fact]
    public async Task SetPartnerPromoCodeLimitAsync_IfPartnerLimitIsNotNull_ShouldNotResetNumberIssuedPromoCodes()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existingPartner = new Fixture().Build<Partner>().With(x => x.PartnerLimits, new List<PartnerPromoCodeLimit>
        {
            new()
            {
                CancelDate = DateTime.UtcNow.AddDays(10)
            }
        }).With(x => x.IsActive, true).With(x => x.NumberIssuedPromoCodes, 100).Create();
        _repositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(existingPartner);
        var partnerPromoCodeLimitRequestFixture = new Fixture().Create<SetPartnerPromoCodeLimitRequest>();

        // Act
        var result = await _partnersController.SetPartnerPromoCodeLimitAsync(id, partnerPromoCodeLimitRequestFixture);
        //Assert
        result.Should().BeOfType<CreatedAtActionResult>();
        existingPartner.NumberIssuedPromoCodes.Should().NotBe(0);
    }

    /// <summary>
    ///     При установке лимита нужно отключить предыдущий лимит;
    /// </summary>
    [Fact]
    public async Task SetPartnerPromoCodeLimitAsync_IfPartnerLimitIsNotNull_ShouldNotResetPreviusLimit()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existingPartner = new Fixture().Build<Partner>().With(x => x.PartnerLimits, new List<PartnerPromoCodeLimit>
        {
            new() { CancelDate = null }
        }).With(x => x.IsActive, true).With(x => x.NumberIssuedPromoCodes, 10).Create();
        _repositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(existingPartner);
        var partnerPromoCodeLimitRequestFixture = new Fixture().Create<SetPartnerPromoCodeLimitRequest>();
        // Act
        var result = await _partnersController.SetPartnerPromoCodeLimitAsync(id, partnerPromoCodeLimitRequestFixture);
        //Assert
        result.Should().BeOfType<CreatedAtActionResult>();
        existingPartner.PartnerLimits.FirstOrDefault()?.CancelDate.Should().NotBeNull();
    }

    /// <summary>
    ///     Лимит должен быть больше 0;
    /// </summary>
    [Fact]
    public async Task SetPartnerPromoCodeLimitAsync_IfLimitLessOrEqualThanZero_ShouldBadRequestWithMessage()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existingPartner = new Fixture().Build<Partner>().With(x => x.IsActive, true)
            .With(x => x.PartnerLimits, new List<PartnerPromoCodeLimit>()).Create();
        _repositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(existingPartner);
        var partnerPromoCodeLimitRequestFixture =
            new Fixture().Build<SetPartnerPromoCodeLimitRequest>().With(x => x.Limit, 0).Create();
        //Act
        var result = await _partnersController.SetPartnerPromoCodeLimitAsync(id, partnerPromoCodeLimitRequestFixture);
        // Assert
        result.Should().BeOfType<BadRequestObjectResult>().Which.Value.Should().Be("Лимит должен быть больше 0");
    }

    /// <summary>
    ///     Нужно убедиться, что сохранили новый лимит в базу данных (это нужно проверить Unit-тестом);
    /// </summary>
    [Fact]
    public async Task SetPartnerPromoCodeLimitAsync_WhenPartnerLimitSet_ShouldReturnNewLimit()
    {
        // Arrange
        var id = Guid.NewGuid();
        Partner savedPartner = null;
        var existingPartner = new Fixture().Build<Partner>().With(x => x.IsActive, true)
            .With(x => x.PartnerLimits, new List<PartnerPromoCodeLimit>()).Create();
        _repositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(existingPartner);

        //setup for update method
        _repositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Partner>())).Callback<Partner>(x => { savedPartner = x; })
            .Returns(Task.CompletedTask);

        var partnerPromoCodeLimitRequestFixture =
            new Fixture().Build<SetPartnerPromoCodeLimitRequest>().With(x => x.Limit, 10).Create();
        // Act
        var result = await _partnersController.SetPartnerPromoCodeLimitAsync(id, partnerPromoCodeLimitRequestFixture);

        //Assert
        _repositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Partner>()), Times.Once);
        savedPartner.Should().NotBeNull();
        savedPartner.PartnerLimits.Count.Should().Be(1);
        savedPartner.PartnerLimits.FirstOrDefault()?.Limit.Should().Be(partnerPromoCodeLimitRequestFixture.Limit);
        savedPartner.PartnerLimits.FirstOrDefault()?.EndDate.Should().Be(partnerPromoCodeLimitRequestFixture.EndDate);
        result.Should().BeOfType<CreatedAtActionResult>();
    }
}