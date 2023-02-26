using FastFood.Services.Models.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastFood.Web.Services.Interfaces
{
    public interface IItemService
    {
        Task Add(CreateItemDto dto);

        Task<ICollection<ListItemDto>> GetAll();
    }
}
