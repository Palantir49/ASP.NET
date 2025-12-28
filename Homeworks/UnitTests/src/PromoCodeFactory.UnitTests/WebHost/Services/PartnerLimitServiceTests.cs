using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using PromoCodeFactory.Core.Domain.PromoCodeManagement;
using PromoCodeFactory.WebHost.Models;
using PromoCodeFactory.WebHost.Services;
using Xunit;

namespace PromoCodeFactory.UnitTests.WebHost.Services;

public class PartnerLimitServiceTests(PartnerLimitServiceTestFixture fixture)
    : IClassFixture<PartnerLimitServiceTestFixture>
{
    private readonly PartnerLimitService _partnerLimitService = fixture.PartnerLimitService;

    [Fact]
    public void ProcessLimitAsync_WhenLimitIsZero_ReturnsFailure()
    {
        // Arrange
        var partner = new Fixture().Build<Partner>().With(p => p.IsActive, true)
            .With(p => p.PartnerLimits, new List<PartnerPromoCodeLimit>()).Create();
        var request = new Fixture().Build<SetPartnerPromoCodeLimitRequest>().With(x => x.Limit, 0).Create();
        var testTime = DateTime.UtcNow;
        // Act
        var result = _partnerLimitService.ProcessLimitAsync(partner, request, testTime);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Be("Лимит должен быть больше 0");
    }


    /// <summary>
    ///     Если партнеру выставляется лимит, то мы должны обнулить количество промокодов, которые партнер выдал
    ///     NumberIssuedPromoCodes
    /// </summary>
    [Fact]
    public void ProcessLimitAsync_IfPartnerLimitIsNull_ShouldResetNumberIssuedPromoCodes()
    {
        // Arrange
        var existingPartner = new Fixture().Build<Partner>().With(x => x.PartnerLimits, new List<PartnerPromoCodeLimit>
            {
                new() { CancelDate = null }
            }).With(x => x.IsActive, true)
            .With(x => x.NumberIssuedPromoCodes, 10).Create();
        var partnerPromoCodeLimitRequestFixture = new Fixture().Create<SetPartnerPromoCodeLimitRequest>();
        var testTime = DateTime.UtcNow;
        // Act
        var result =
            _partnerLimitService.ProcessLimitAsync(existingPartner, partnerPromoCodeLimitRequestFixture, testTime);
        //Assert
        result.IsSuccess.Should().BeTrue();
        existingPartner.NumberIssuedPromoCodes.Should().Be(0);
    }

    /// <summary>
    ///     Если лимит закончился, то количество не обнуляется
    /// </summary>
    [Fact]
    public void SetPartnerPromoCodeLimitAsync_IfPartnerLimitIsNotNull_ShouldNotResetNumberIssuedPromoCodes()
    {
        // Arrange
        var existingPartner = new Fixture().Build<Partner>().With(x => x.PartnerLimits, new List<PartnerPromoCodeLimit>
        {
            new()
            {
                CancelDate = DateTime.UtcNow.AddDays(10)
            }
        }).With(x => x.IsActive, true).With(x => x.NumberIssuedPromoCodes, 100).Create();
        var partnerPromoCodeLimitRequestFixture = new Fixture().Create<SetPartnerPromoCodeLimitRequest>();
        var testTime = DateTime.UtcNow;
        // Act
        var result =
            _partnerLimitService.ProcessLimitAsync(existingPartner, partnerPromoCodeLimitRequestFixture, testTime);
        //Assert
        result.IsSuccess.Should().BeTrue();
        existingPartner.NumberIssuedPromoCodes.Should().NotBe(0);
    }

    /// <summary>
    ///     При установке лимита нужно отключить предыдущий лимит;
    /// </summary>
    [Fact]
    public void SetPartnerPromoCodeLimitAsync_IfPartnerLimitIsNotNull_ShouldNotResetPreviusLimit()
    {
        // Arrange
        var existingPartner = new Fixture().Build<Partner>().With(x => x.PartnerLimits, new List<PartnerPromoCodeLimit>
        {
            new() { CancelDate = null }
        }).With(x => x.IsActive, true).With(x => x.NumberIssuedPromoCodes, 10).Create();

        var partnerPromoCodeLimitRequestFixture = new Fixture().Create<SetPartnerPromoCodeLimitRequest>();
        var testTime = DateTime.UtcNow;
        // Act
        var result =
            _partnerLimitService.ProcessLimitAsync(existingPartner, partnerPromoCodeLimitRequestFixture, testTime);
        //Assert
        result.IsSuccess.Should().BeTrue();
        existingPartner.PartnerLimits.FirstOrDefault()?.CancelDate.Should().NotBeNull();
    }
}