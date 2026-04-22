using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Pcf.Administration.Core.Abstractions.Repositories;
using Pcf.Administration.Core.Abstractions.Services;
using Pcf.Administration.Core.Domain.Administration;

namespace Pcf.Administration.Core.Realizations.Services;

public sealed class EmployeeService(IRepository<Employee> employeeRepository) : IEmployeeService
{
    public async Task UpdateAppliedPromoCodesAsync(Guid id)
    {
        var employee = await employeeRepository.GetByIdAsync(id);

        if (employee == null)
            throw new KeyNotFoundException($"Сотрудника с таким {id} не найдено");

        employee.AppliedPromocodesCount++;

        await employeeRepository.UpdateAsync(employee);
    }
}