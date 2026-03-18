using AutoMapper;
using ECommerce.Application.DTOs.Category;
using ECommerce.Application.DTOs.Order;
using ECommerce.Application.DTOs.Product;
using ECommerce.Application.DTOs.Notification;
using ECommerce.Domain.Entities;

namespace ECommerce.Application.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Product, ReadProductDto>()
            .ForMember(d => d.CategoryName, opt => opt.MapFrom(s => s.Category.Name))
            .ForMember(d => d.CategoryId, opt => opt.MapFrom(s => s.CategoryId))
            .ForMember(d => d.ImageUrl, opt => opt.MapFrom(s => s.ImageUrl));

        CreateMap<CreateProductDto, Product>();
        CreateMap<UpdateProductDto, Product>();

        CreateMap<Category, ReadCategoryDto>();

        CreateMap<OrderItem, ReadOrderItemDto>()
            .ForMember(d => d.ProductName, opt => opt.MapFrom(s => s.Product.Name));

        CreateMap<Order, ReadOrderDto>();

        CreateMap<Notification, NotificationDto>();
    }
}

