using AutoMapper;
using ElasticsearchExample.Dtos;
using ElasticsearchExample.Models;

namespace ElasticsearchExample.Profiles
{
    public class AppProfile : Profile
    {
        public AppProfile()
        {
            CreateMap<Customer, CustomerReadDto>();
            CreateMap<Product, ProductReadDto>();
            CreateMap<OrderItem, OrderItemReadDto>()
                .ForMember(d => d.OrderDate, opt => opt.MapFrom(s => s.CreatedAt));
        }
    }
}
