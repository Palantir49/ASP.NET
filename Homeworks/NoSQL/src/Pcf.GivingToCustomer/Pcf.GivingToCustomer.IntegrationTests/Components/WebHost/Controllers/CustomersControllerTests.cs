using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Pcf.GivingToCustomer.Core.Domain;
using Pcf.GivingToCustomer.DataAccess.Repositories;
using Pcf.GivingToCustomer.WebHost.Controllers;
using Pcf.GivingToCustomer.WebHost.Models;
using Xunit;

namespace Pcf.GivingToCustomer.IntegrationTests.Components.WebHost.Controllers;

[Collection(MongoDatabaseCollection.DbCollection)]
public class CustomersControllerTests : IClassFixture<MongoDbFixture>
{
    private readonly MongoDbRepository<Customer> _customerRepository;
    private readonly CustomersController _customersController;

    public CustomersControllerTests(MongoDbFixture mongoDbFixtureFixture)
    {
        _customerRepository = new MongoDbRepository<Customer>(mongoDbFixtureFixture.DbContext);
        var preferenceRepository = new MongoDbRepository<Preference>(mongoDbFixtureFixture.DbContext);
        var promoCodeRepository = new MongoDbRepository<PromoCode>(mongoDbFixtureFixture.DbContext);
        _customersController = new CustomersController(
            _customerRepository,
            preferenceRepository,
            promoCodeRepository);
    }

    [Fact]
    public async Task CreateCustomerAsync_CanCreateCustomer_ShouldCreateExpectedCustomer()
    {
        //Arrange 
        var preferenceId = Guid.Parse("ef7f299f-92d7-459f-896e-078ed53ef99c");
        var request = new CreateOrEditCustomerRequest
        {
            Email = "some@mail.ru",
            FirstName = "Иван",
            LastName = "Петров",
            PreferenceIds = [preferenceId]
        };

        //Act
        var result = await _customersController.CreateCustomerAsync(request);
        var actionResult = result.Result as CreatedAtActionResult;
        var id = (Guid)actionResult.Value;

        //Assert
        var actual = await _customerRepository.GetByIdAsync(id);

        actual.Email.Should().Be(request.Email);
        actual.FirstName.Should().Be(request.FirstName);
        actual.LastName.Should().Be(request.LastName);
        actual.PreferenceIds.Should()
            .ContainSingle()
            .And
            .Contain(x => x == preferenceId);
    }
}