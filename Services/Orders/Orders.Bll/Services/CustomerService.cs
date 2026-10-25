using AutoMapper;
using Orders.Bll.Dto;
using Orders.Dal.Interfaces;
using Orders.Domain.Exceptions;
using Orders.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Orders.Bll.Services
{
    public class CustomerService(IUnitOfWork uow, IMapper mapper) : ServiceBase(uow), ICustomerService
    {
        public async Task<CustomerDto> GetByIdAsync(long id, CancellationToken ct)
            => mapper.Map<CustomerDto>(await Uow.Customers.GetByIdAsync(id, ct)
                ?? throw new NotFoundException($"Customer {id} not found"));

        public async Task<IReadOnlyList<CustomerDto>> GetAllAsync(CancellationToken ct)
            => mapper.Map<List<CustomerDto>>(await Uow.Customers.GetAllAsync(ct));

        public async Task<CustomerDto> CreateAsync(CustomerInputDto input, CancellationToken ct)
        {
            var entity = mapper.Map<Customer>(input);
            entity.CreatedBy = "api";
            var id = await Uow.Customers.AddAsync(entity, ct);      // дубль Email -> 2627 -> 409
            return await GetByIdAsync(id, ct);
        }

        public async Task UpdateAsync(long id, CustomerInputDto input, CancellationToken ct)
        {
            var entity = mapper.Map<Customer>(input);
            entity.CustomerId = id;
            entity.UpdatedBy = "api";
            if (!await Uow.Customers.UpdateAsync(entity, ct))
                throw new NotFoundException($"Customer {id} not found");
        }

        public async Task DeleteAsync(long id, CancellationToken ct)
        {
            if (!await Uow.Customers.DeleteAsync(id, "api", ct))
                throw new NotFoundException($"Customer {id} not found");
        }
    }
}
