using System;
using System.Collections.Generic;
using System.Text;

namespace Orders.Tests
{
    using AutoMapper;
    using Microsoft.Extensions.Logging.Abstractions;
    using Moq;
    using Orders.Bll.Mapping;
    using Orders.Bll.Services;
    using Orders.Dal.Interfaces;
    using Orders.Domain.Exceptions;
    using Orders.Domain.Models;

    public class BllTests
    {
        private static IMapper CreateMapper() =>
            new MapperConfiguration(c => c.AddProfile<OrdersProfile>(), NullLoggerFactory.Instance).CreateMapper();

        [Fact]
        public void Mapping_configuration_is_valid() =>
            new MapperConfiguration(c => c.AddProfile<OrdersProfile>(), NullLoggerFactory.Instance)
                .AssertConfigurationIsValid();

        [Fact]
        public async Task ConfirmOrder_missing_order_throws_NotFound()
        {
            var orders = new Mock<IOrderRepository>();
            orders.Setup(o => o.GetWithItemsAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((Order?)null);
            var uow = new Mock<IUnitOfWork>();
            uow.SetupGet(u => u.Orders).Returns(orders.Object);

            var svc = new OrderService(uow.Object, CreateMapper());
            await Assert.ThrowsAsync<NotFoundException>(() => svc.ConfirmOrderAsync(999, default));
        }
    }
}
