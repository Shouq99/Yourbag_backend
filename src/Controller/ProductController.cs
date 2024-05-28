using ecommerce.EntityFramework;
using ecommerce.Middleware;
using ecommerce.Models;
using ecommerce.service;
using ecommerce.utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ecommerce.Controller;

[ApiController]
[Route("/products")]
public class ProductController : ControllerBase
{
    private readonly ProductService _productService;

    public ProductController(ProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllProducts(
        [FromQuery] int page = 1,
        [FromQuery] int limit = 5,
        [FromQuery] SortBy sortBy = SortBy.Date,
        [FromQuery] OrderBy orderBy = OrderBy.ASC,
        [FromQuery] string? keyword = null, //name of product or description
        [FromQuery] double? minPrice = 1,
        [FromQuery] double? maxPrice = 20000.00
    )
    {
        // Get all products from the service
        var products = await _productService.GetAllProducts();
        if (!products.Any())
        {
            throw new NotFoundException("There are no Products");
        }

        // Apply filtering based on keyword and price range
        if (!string.IsNullOrEmpty(keyword))
        {
            keyword = keyword!.ToLower();
            products = products.Where(p =>
                p.Name!.ToLower().Contains(keyword) || p.Description!.ToLower().Contains(keyword)
            );
        }

        if (minPrice.HasValue && minPrice < maxPrice && maxPrice.HasValue)
        {
            products = products.Where(p => p.Price >= minPrice.Value && p.Price <= maxPrice.Value);
        }

        // Apply sorting based on sort by and sort order
        switch (sortBy)
        {
            case SortBy.Name:
                products =
                    orderBy == OrderBy.ASC
                        ? products.OrderBy(p => p.Name)
                        : products.OrderByDescending(p => p.Name);
                break;
            default:
                // Default sorting by date
                products =
                    orderBy == OrderBy.ASC
                        ? products.OrderBy(p => p.CreatedAt)
                        : products.OrderByDescending(p => p.CreatedAt);
                break;
        }

        if (!products.Any())
        {
            return ApiResponse.NotFound("No products found matching the sort/filter criteria.");
        }
        
        var totalProducts = products.Count();
        var totalPages = (int)Math.Ceiling(totalProducts / (double)limit);
        // Apply pagination
        var paginatedProducts = products.Skip((page - 1) * limit).Take(limit).ToList();

    var response = new {
        Products = paginatedProducts,
        TotalPages = totalPages,
        CurrentPage = page,
        PageSize = limit,
        TotalProducts = totalProducts
    };

        return ApiResponse.Success(
            paginatedProducts,
            "All products inside E-commerce system are returned");
    }

    [HttpGet("{slug}")]
    public async Task<IActionResult> GetProductById(string slug)
    {
        var foundProduct =
            await _productService.GetProductById(slug)
            ?? throw new NotFoundException("There is no product found matching");

        return ApiResponse.Success(foundProduct, "Product are returned successfully");
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchProducts(string searchKeyword)
    {
        // Retrieve products based on the search keyword
        var products = await _productService.SearchProducts(searchKeyword);

        if (products == null || !products.Any())
        {
            throw new NotFoundException("No products found matching the search criteria.");
        }
        return ApiResponse.Success(products);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [Authorize(Policy = "RequiredNotBanned")]
    public async Task<IActionResult> AddProduct(ProductModel newProduct)
    {
        var AddProduct = await _productService.AddProduct(newProduct);
        return ApiResponse.Created(AddProduct, "The product is Added");
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [Authorize(Policy = "RequiredNotBanned")]
    public async Task<IActionResult> UpdateProduct(Guid id, ProductModel updatedProduct)
    {
        var found =
            await _productService.UpdateProduct(id, updatedProduct)
            ?? throw new NotFoundException("The product not found");

        return ApiResponse.Success(found, "Product are updated successfully");
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [Authorize(Policy = "RequiredNotBanned")]
    public async Task<IActionResult> DeleteProduct(Guid id)
    {
        var deleted = await _productService.DeleteProduct(id);
        if (!deleted)
        {
            throw new NotFoundException("The product not found");
        }
        return ApiResponse.Success(id, "product are Deleted successfully");
    }
}
