using ecommerce.Models;
using ecommerce.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ecommerce.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IWebHostEnvironment env;
        private readonly List<string> listCategories = new List<string>()
        {
            "Phones",
            "Computers",
            "Accessories",
            "Printers",
            "Cameras",
            "Others"
        };

        public ProductsController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            this.context = context;
            this.env = env;
        }

        [HttpGet("categories")]
        public IActionResult GetCategories()
        {
            return Ok(listCategories);
        }

        [HttpGet]
        public IActionResult GetProducts(
            string? search,
            string? category,
            int? minPrice,
            int? maxPrice,
            string? sort,
            string? order,
            int? page
        )
        {
            // The guy teaching this should refactor to not use tons of if & else if statements
            IQueryable<Product> query = context.Products;

            if (search != null)
            {
                query = query.Where(p => p.Name.Contains(search) || p.Description.Contains(search));
            }

            if (category != null)
            {
                query = query.Where(p => p.Category == category);
            }

            if (minPrice != null)
            {
                query = query.Where(p => p.Price >= minPrice);
            }

            if (maxPrice != null)
            {
                query = query.Where(p => p.Price <= minPrice);
            }

            if (sort == null)
            {
                sort = "id";
            }

            if (order == null || order != "asc")
            {
                order = "desc";
            }

            if (sort.ToLower() == "name")
            {
                if (order == "asc")
                {
                    query = query.OrderBy(p => p.Name);
                }
                else
                {
                    query = query.OrderByDescending(p => p.Name);
                }
            }
            else if (sort.ToLower() == "brand")
            {
                if (order == "asc")
                {
                    query = query.OrderBy(p => p.Brand);
                }
                else
                {
                    query = query.OrderByDescending(p => p.Brand);
                }
            }
            else if (sort.ToLower() == "category")
            {
                if (order == "asc")
                {
                    query = query.OrderBy(p => p.Category);
                }
                else
                {
                    query = query.OrderByDescending(p => p.Category);
                }
            }
            else if (sort.ToLower() == "price")
            {
                if (order == "asc")
                {
                    query = query.OrderBy(p => p.Price);
                }
                else
                {
                    query = query.OrderByDescending(p => p.Price);
                }
            }
            else if (sort.ToLower() == "date")
            {
                if (order == "asc")
                {
                    query = query.OrderBy(p => p.CreatedAt);
                }
                else
                {
                    query = query.OrderByDescending(p => p.CreatedAt);
                }
            }
            else
            {
                if (order == "asc")
                {
                    query = query.OrderBy(p => p.Id);
                }
                else
                {
                    query = query.OrderByDescending(p => p.Id);
                }
            }

            // Pagination
            if (page == null)
            {
                page = 1;
            }

            int pageSize = 5;
            int totalPages = 0;

            decimal count = query.Count();
            totalPages = (int)Math.Ceiling(count / pageSize);

            query = query.Skip((int)(page - 1) * pageSize).Take(pageSize);

            var products = query.ToList();

            var response = new
            {
                Products = products,
                TotalPages = totalPages,
                PageSize = pageSize,
                Page = page
            };

            return Ok(response);
        }

        [HttpGet("{id}")]
        public IActionResult GetProduct(int id)
        {
            var product = context.Products.Find(id);

            if (product == null)
            {
                return NotFound();
            }

            return Ok(product);
        }

        [Authorize(Roles = "admin")]
        [HttpPost]
        public IActionResult CreateProduct([FromForm] ProductDto newProduct)
        {
            if (!listCategories.Contains(newProduct.Category))
            {
                ModelState.AddModelError("Category", "Please provide a valid category");
                return BadRequest(ModelState);
            }

            if (newProduct.ImageFile == null)
            {
                ModelState.AddModelError("ImageFile", "The Image File is required");
                return BadRequest(ModelState);
            }

            // Save the image on the server
            string imageFileName = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            imageFileName += Path.GetExtension(newProduct.ImageFile.FileName);

            string imagesFolder = env.WebRootPath + "/images/products/";

            using (var stream = System.IO.File.Create(imagesFolder + imageFileName))
            {
                newProduct.ImageFile.CopyTo(stream);
            }

            // Save newProduct in the database
            Product product = new Product()
            {
                Name = newProduct.Name,
                Brand = newProduct.Brand,
                Category = newProduct.Category,
                Price = newProduct.Price,
                Description = newProduct.Description ?? "",
                ImageFileName = imageFileName,
                CreatedAt = DateTime.Now
            };

            context.Products.Add(product);
            context.SaveChanges();

            return Ok(product);
        }

        [Authorize(Roles = "admin")]
        [HttpPut("{id}")]
        public IActionResult UpdateProduct(int id, [FromForm] ProductDto productDto)
        {
            if (!listCategories.Contains(productDto.Category))
            {
                ModelState.AddModelError("Category", "Please provide a valid category");
                return BadRequest(ModelState);
            }

            var product = context.Products.Find(id);

            if (product == null)
            {
                return NotFound();
            }

            string imageFileName = product.ImageFileName;
            if (productDto.ImageFile != null)
            {
                imageFileName = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                imageFileName += Path.GetExtension(productDto.ImageFile.FileName);

                string imagesFolder = env.WebRootPath + "/images/products/";

                using (var stream = System.IO.File.Create(imagesFolder + imageFileName))
                {
                    productDto.ImageFile.CopyTo(stream);
                }

                System.IO.File.Delete(imagesFolder + product.ImageFileName);
            }

            product.Name = productDto.Name;
            product.Brand = productDto.Brand;
            product.Category = productDto.Category;
            product.Price = productDto.Price;
            product.Description = productDto.Description ?? "";
            product.ImageFileName = imageFileName;

            context.SaveChanges();

            return Ok(product);
        }

        [Authorize(Roles = "admin")]
        [HttpDelete("{id}")]
        public IActionResult DeleteProduct(int id)
        {
            var product = context.Products.Find(id);

            if (product == null)
            {
                return NotFound();
            }

            string imagesFolder = env.WebRootPath + "/images/products/";
            System.IO.File.Delete(imagesFolder + product.ImageFileName);

            context.Products.Remove(product);
            context.SaveChanges();

            return Ok();
        }
    }
}
