using AutoMapper;
using ProductShop.DTOs.Category;
using ProductShop.DTOs.CategoryProduct;
using ProductShop.DTOs.Product;
using ProductShop.DTOs.User;
using ProductShop.Models;
using System;
using System.Linq;

namespace ProductShop
{
    public class ProductShopProfile : Profile
    {
        public ProductShopProfile()
        {
            this.CreateMap<ImportUserDto, User>();
            this.CreateMap<ImportProductDto, Product>();
            this.CreateMap<ImportCategoryDto, Category>();  
            this.CreateMap<ImportCategoryProductDto, CategoryProduct>();

            this.CreateMap<Product, ExportProductsInRangeDto>()
                .ForMember(dest => dest.SellerFullName, 
                mo => mo.MapFrom(p => $"{p.Seller.FirstName} {p.Seller.LastName}"));

            //Inner DTO
            this.CreateMap<Product, ExportUserSoldPorductsDto>()
                .ForMember(dest => dest.BuyerFirstName, 
                    src => src.MapFrom(s => s.Buyer.FirstName))
                .ForMember(dest => dest.BuyerLastName, 
                    src => src.MapFrom(s => s.Buyer.LastName));

            //Outer DTO
            this.CreateMap<User, ExportUserWithAtLeastOneBuyerDto>()
                .ForMember(dest => dest.SoldProducts,
                    mo => mo.MapFrom(
                        s => s.ProductsSold
                            .Where(p => p.BuyerId.HasValue)));

            this.CreateMap<Category, ExportCategoryDto>()
                .ForMember(dest => dest.Category,
                    mo => mo.MapFrom(src => src.Name))
                .ForMember(dest => dest.ProductsCount,
                    mo => mo.MapFrom(src => src.CategoryProducts.Count()))
                .ForMember(dest => dest.AveragePrice,
                    mo => mo.MapFrom(
                        src => $"{(double)src.CategoryProducts.Average(cp => cp.Product.Price):F2}"))
                .ForMember(dest => dest.TotalRevenue,
                    mo => mo.MapFrom(
                        src => $"{(double)src.CategoryProducts.Sum(cp => cp.Product.Price):F2}"));

            //08
            this.CreateMap<Product, ExportSoldProductInfoDto08>();

            this.CreateMap<User, ExportSoldProductDto08>()
                .ForMember(d => d.Count,
                    mo => mo.MapFrom(s => s.ProductsSold.Count))
                .ForMember(d => d.Products,
                    mo => mo.MapFrom(s => s.ProductsSold));

            this.CreateMap<User, ExportUserAtLeastOneSoldProductDto08>()
                .ForMember(d => d.SoldProducts,
                    mo => mo.MapFrom(s => s.ProductsSold));

            /*this.CreateMap<User, ExportUser08>()
               .ForMember(d => d.UsersCount,
                    mo => mo.MapFrom(s => ))*/
        }
    }
}
