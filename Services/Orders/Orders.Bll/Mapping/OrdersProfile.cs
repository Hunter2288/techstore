using AutoMapper;
using Orders.Bll.Dto;
using Orders.Domain.Models;

namespace Orders.Bll.Mapping
{
    public class OrdersProfile : Profile
    {
        public OrdersProfile()
        {
            CreateMap<Customer, CustomerDto>();
            CreateMap<OrderItem, OrderItemDto>();
            CreateMap<Order, OrderDto>();
            CreateMap<Payment, PaymentDto>();

            CreateMap<CustomerInputDto, Customer>()
                .ForMember(d => d.CustomerId, o => o.Ignore())
                .ForMember(d => d.CreatedAt, o => o.Ignore())
                .ForMember(d => d.CreatedBy, o => o.Ignore())
                .ForMember(d => d.UpdatedAt, o => o.Ignore())
                .ForMember(d => d.UpdatedBy, o => o.Ignore())
                .ForMember(d => d.IsDeleted, o => o.Ignore());
        }
    }
}