using System;
using System.Collections.Generic;
using System.Text;

namespace Orders.Tests
{
    using System.Data;
    using Orders.Dal;
    using Orders.Domain.Exceptions;
    using Orders.Domain.Models;

    public class RollbackTests
    {
        private static readonly string Cs = Environment.GetEnvironmentVariable("ConnectionStrings__OrdersDb")
            ?? "Server=localhost;Database=TechStore_Orders;Trusted_Connection=True;TrustServerCertificate=True";

        [Fact]
        public async Task Failed_status_change_rolls_back_inserted_payment()
        {
            await using var read = new UnitOfWork(Cs);
            var order = (await read.Orders.GetAllAsync()).First(o => o.Status == "Submitted");
            var before = (await read.Payments.GetByOrderAsync(order.OrderId)).Count;

            await using var uow = new UnitOfWork(Cs);
            await uow.BeginTransactionAsync();
            try
            {
                await uow.Payments.AddAsync(new Payment
                {
                    OrderId = order.OrderId,
                    Amount = 1,
                    Method = "Card",
                    Status = "Succeeded",
                    PaidAt = DateTime.UtcNow
                });
              
                await Assert.ThrowsAsync<BusinessConflictException>(() =>
                    uow.Orders.ChangeStatusAsync(order.OrderId, "Paid", "test", null));
            }
            finally { await uow.RollbackAsync(); }

            var after = (await read.Payments.GetByOrderAsync(order.OrderId)).Count;
            Assert.Equal(before, after);
        }
    }
}
