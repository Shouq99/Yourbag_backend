using ecommerce.EntityFramework;
using ecommerce.EntityFramework.Table;
using ecommerce.Models;
using ecommerce.utils;
using Microsoft.EntityFrameworkCore;

namespace ecommerce.service;

public class ProductService
{
    private readonly AppDbContext _appDbContext;

    public ProductService(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }

    // Get all products
    public async Task<IEnumerable<Product>> GetAllProducts()
    {
        var products = await _appDbContext
            .Products
          //  .Include(c => c.Category)
         //   .Include(r => r.Reviews)
            .ToListAsync();
        return products;
    }

    // Get product by ID
    public async Task<Product?> GetProductById(string slug)
    {
        var foundProduct = await _appDbContext
            .Products.Include(c => c.Category)
            .Include(r => r.Reviews)
            .Include(i => i.OrderItems)
            .FirstOrDefaultAsync(product => product.Slug == slug);
        return foundProduct;
    }
    public async Task<IEnumerable<Product>> SearchProducts(string searchKeyword)
    {
        searchKeyword = searchKeyword.ToLower();
        var foundProducts = await _appDbContext
            .Products.Where(p =>
                EF.Functions.Like(p.Name!.ToLower(), $"%{searchKeyword}%")
                || EF.Functions.Like(p.Description!.ToLower(), $"%{searchKeyword}%")
            )
            .ToListAsync();
        return foundProducts;
    }

    // Create a new product
    public async Task<ProductModel> AddProduct(ProductModel newProduct)
    {
        Product product = new Product
        {
            ProductId = Guid.NewGuid(),
            Name = newProduct.Name,
            Price = newProduct.Price,
            Slug = Function.GetSlug(newProduct.Name ?? ""),
            Description = newProduct.Description,
            Image = newProduct.Image ?? "",
            CreatedAt = DateTime.UtcNow,
            CategoryId = newProduct.CategoryId
        };
        await _appDbContext.Products.AddAsync(product);
        await _appDbContext.SaveChangesAsync();
        return newProduct;
    }

    // Update an existing product
    public async Task<Product?> UpdateProduct(Guid id, ProductModel updatedProduct)
    {
        var foundProduct = await _appDbContext.Products.FirstOrDefaultAsync(product =>
            product.ProductId == id
        );
        if (foundProduct != null)
        {
            foundProduct.Name = updatedProduct.Name ?? foundProduct.Name;
            foundProduct.Price = updatedProduct.Price;
            foundProduct.Slug = Function.GetSlug(foundProduct.Name ?? "");
            foundProduct.Description = updatedProduct.Description ?? foundProduct.Description;
            foundProduct.Image = updatedProduct.Image ?? foundProduct.Image;
        }
        await _appDbContext.SaveChangesAsync();
        return foundProduct;
    }

    // Delete a product
    public async Task<bool> DeleteProduct(Guid id)
    {
        await Task.CompletedTask;
        var foundProduct = await _appDbContext.Products.FirstOrDefaultAsync(product =>
            product.ProductId == id
        );
        if (foundProduct != null)
        {
            _appDbContext.Products.Remove(foundProduct);
            await _appDbContext.SaveChangesAsync();
            return true;
        }
        return false;
    }
}
