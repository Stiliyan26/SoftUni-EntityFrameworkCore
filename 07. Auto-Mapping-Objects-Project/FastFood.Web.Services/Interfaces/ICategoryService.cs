using FastFood.Services.Models.Categories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastFood.Web.Services.Interfaces
{
    public interface ICategoryService
    {
        Task Add(CreateCategoryDto categoryDto);

        Task<ICollection<ListCategoryDto>> GetAll();

        Task<bool> ExistById(int id);
    }
}
