using ecommerce.Models;
using ecommerce.utils;
using Microsoft.EntityFrameworkCore;
using ecommerce.EntityFramework;
using ecommerce.EntityFramework.Table;

namespace ecommerce.service;

public class CategoryService
{
    private readonly AppDbContext _appDbContext;

    public CategoryService(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }

    public async Task<IEnumerable<Category>> GetCategories()
    {
        var categories = await _appDbContext.Categories.Include(p => p.Product).ToListAsync();
        return categories;
    }

    public async Task<Category?> GetCategoryById(Guid id)
    {
        var foundCategory = await _appDbContext
            .Categories.Include(p => p.Product)
            .FirstOrDefaultAsync(category => category.CategoryId == id);
        return foundCategory;
    }

    public async Task<CategoryModel> AddCategory(CategoryModel newCategory)
    {
        Category category = new Category
        {
            CategoryId = Guid.NewGuid(),
            Name = newCategory.Name,
            Slug = Function.GetSlug(newCategory.Name ?? "")
            
        };
        await _appDbContext.Categories.AddAsync(category);
        await _appDbContext.SaveChangesAsync();
        return newCategory;
    }

    public async Task<Category?> UpdateCategoryName(Guid id, CategoryModel updateCategory)
    {
        var foundCategory = await _appDbContext.Categories.FirstOrDefaultAsync(category => category.CategoryId == id);
        if (foundCategory != null)
        {
            foundCategory.Name = updateCategory.Name;
            foundCategory.Slug = Function.GetSlug(updateCategory.Name ?? "");
        }
        await _appDbContext.SaveChangesAsync();
        return foundCategory;
    }

    public async Task<bool> DeleteCategory(Guid id)
    {
        await Task.CompletedTask;
        var foundCategory = await _appDbContext.Categories.FirstOrDefaultAsync(category => category.CategoryId == id);
        if (foundCategory != null)
        {
            _appDbContext.Categories.Remove(foundCategory);
            await _appDbContext.SaveChangesAsync();
            return true;
        }
        return false;
    }
}
