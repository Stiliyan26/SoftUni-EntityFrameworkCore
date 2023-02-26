using AutoMapper;
using AutoMapper.QueryableExtensions;
using FastFood.Data;
using FastFood.Models;
using FastFood.Services.Models.Categories;
using FastFood.Web.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.Security.Cryptography.X509Certificates;

namespace FastFood.Web.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly FastFoodContext dbContext;

        private readonly IMapper mapper;
        public CategoryService(FastFoodContext dbContext, IMapper mapper)
        {
            this.dbContext = dbContext;
            this.mapper = mapper;
        }
        public async Task Add(CreateCategoryDto categoryDto)
        {
            Category category = this.mapper.Map<Category>(categoryDto);

            await dbContext.Categories.AddAsync(category);
            await dbContext.SaveChangesAsync();
        }
        public async Task<ICollection<ListCategoryDto>> GetAll()
        {
            ICollection<ListCategoryDto> result = await this.dbContext
                .Categories
                .ProjectTo<ListCategoryDto>(this.mapper.ConfigurationProvider)
                .ToArrayAsync();

            return result;
        }

        public async Task<bool> ExistById(int id)
        {
            return await this.dbContext
                .Categories
                .AnyAsync(c => c.Id == id);
        }
    }
}