using AspNet.NativeHttpServer.Models;
using System.Text;

namespace AspNet.NativeHttpServer.Views;

/// <summary>
/// Contains HTML view templates for product-related pages.
/// </summary>
public static class ProductViews
{
    /// <summary>
    /// Generates the HTML for the product list page.
    /// </summary>
    /// <param name="products">The collection of products to display.</param>
    /// <returns>The HTML content for the product list page.</returns>
    public static string ProductListView(IEnumerable<Product> products)
    {
        var html = new StringBuilder();
        
        html.AppendLine(GetHtmlHeader("Product List"));
        html.AppendLine("<body>");
        html.AppendLine(GetNavigation());
        html.AppendLine("<div class=\"container\">");
        html.AppendLine("<h1>Products</h1>");
        
        html.AppendLine("<div class=\"actions\">");
        html.AppendLine("<a href=\"/api/products\" class=\"btn btn-secondary\">View as JSON</a>");
        html.AppendLine("</div>");

        if (!products.Any())
        {
            html.AppendLine("<p>No products found.</p>");
        }
        else
        {
            html.AppendLine("<div class=\"product-grid\">");
            
            foreach (var product in products)
            {
                html.AppendLine("<div class=\"product-card\">");
                html.AppendLine($"<h3><a href=\"/products/{product.Id}\">{product.Name}</a></h3>");
                html.AppendLine($"<p class=\"description\">{product.Description}</p>");
                html.AppendLine($"<p class=\"price\">${product.Price:F2}</p>");
                html.AppendLine($"<p class=\"stock\">Stock: {product.Stock}</p>");
                html.AppendLine($"<p class=\"created\">Created: {product.CreatedAt:yyyy-MM-dd}</p>");
                html.AppendLine("</div>");
            }
            
            html.AppendLine("</div>");
        }
        
        html.AppendLine("</div>");
        html.AppendLine("</body>");
        html.AppendLine("</html>");
        
        return html.ToString();
    }

    /// <summary>
    /// Generates the HTML for the product detail page.
    /// </summary>
    /// <param name="product">The product to display.</param>
    /// <returns>The HTML content for the product detail page.</returns>
    public static string ProductDetailView(Product product)
    {
        var html = new StringBuilder();
        
        html.AppendLine(GetHtmlHeader($"Product: {product.Name}"));
        html.AppendLine("<body>");
        html.AppendLine(GetNavigation());
        html.AppendLine("<div class=\"container\">");
        
        html.AppendLine("<div class=\"actions\">");
        html.AppendLine("<a href=\"/products\" class=\"btn btn-secondary\">← Back to Products</a>");
        html.AppendLine($"<a href=\"/api/products/{product.Id}\" class=\"btn btn-secondary\">View as JSON</a>");
        html.AppendLine("</div>");
        
        html.AppendLine("<div class=\"product-detail\">");
        html.AppendLine($"<h1>{product.Name}</h1>");
        html.AppendLine($"<p class=\"description\">{product.Description}</p>");
        
        html.AppendLine("<div class=\"product-info\">");
        html.AppendLine($"<div class=\"info-item\"><strong>ID:</strong> {product.Id}</div>");
        html.AppendLine($"<div class=\"info-item\"><strong>Price:</strong> ${product.Price:F2}</div>");
        html.AppendLine($"<div class=\"info-item\"><strong>Stock:</strong> {product.Stock}</div>");
        html.AppendLine($"<div class=\"info-item\"><strong>Created:</strong> {product.CreatedAt:yyyy-MM-dd HH:mm:ss}</div>");
        html.AppendLine("</div>");
        html.AppendLine("</div>");
        
        html.AppendLine("</div>");
        html.AppendLine("</body>");
        html.AppendLine("</html>");
        
        return html.ToString();
    }

    /// <summary>
    /// Generates the HTML for the home page.
    /// </summary>
    /// <returns>The HTML content for the home page.</returns>
    public static string HomeView()
    {
        var html = new StringBuilder();
        
        html.AppendLine(GetHtmlHeader("Native HTTP Server Demo"));
        html.AppendLine("<body>");
        html.AppendLine(GetNavigation());
        html.AppendLine("<div class=\"container\">");
        
        html.AppendLine("<div class=\"hero\">");
        html.AppendLine("<h1>Welcome to Native HTTP Server Demo</h1>");
        html.AppendLine("<p>This is a demonstration of a simple HTTP server built with .NET's native HttpListener.</p>");
        html.AppendLine("</div>");
        
        html.AppendLine("<div class=\"features\">");
        html.AppendLine("<h2>Features</h2>");
        html.AppendLine("<ul>");
        html.AppendLine("<li>✅ Native HTTP server using HttpListener</li>");
        html.AppendLine("<li>✅ RESTful API endpoints</li>");
        html.AppendLine("<li>✅ HTML views and JSON responses</li>");
        html.AppendLine("<li>✅ Route parameter extraction</li>");
        html.AppendLine("<li>✅ In-memory data storage</li>");
        html.AppendLine("<li>✅ CRUD operations for products</li>");
        html.AppendLine("</ul>");
        html.AppendLine("</div>");
        
        html.AppendLine("<div class=\"api-endpoints\">");
        html.AppendLine("<h2>API Endpoints</h2>");
        html.AppendLine("<div class=\"endpoint-list\">");
        html.AppendLine("<div class=\"endpoint\"><span class=\"method get\">GET</span> <code>/products</code> - View all products (HTML)</div>");
        html.AppendLine("<div class=\"endpoint\"><span class=\"method get\">GET</span> <code>/api/products</code> - Get all products (JSON)</div>");
        html.AppendLine("<div class=\"endpoint\"><span class=\"method get\">GET</span> <code>/products/{id}</code> - View product (HTML)</div>");
        html.AppendLine("<div class=\"endpoint\"><span class=\"method get\">GET</span> <code>/api/products/{id}</code> - Get product (JSON)</div>");
        html.AppendLine("<div class=\"endpoint\"><span class=\"method post\">POST</span> <code>/api/products</code> - Create product (JSON)</div>");
        html.AppendLine("<div class=\"endpoint\"><span class=\"method put\">PUT</span> <code>/api/products/{id}</code> - Update product (JSON)</div>");
        html.AppendLine("<div class=\"endpoint\"><span class=\"method delete\">DELETE</span> <code>/api/products/{id}</code> - Delete product (JSON)</div>");
        html.AppendLine("</div>");
        html.AppendLine("</div>");
        
        html.AppendLine("</div>");
        html.AppendLine("</body>");
        html.AppendLine("</html>");
        
        return html.ToString();
    }

    /// <summary>
    /// Gets the HTML header with CSS styles.
    /// </summary>
    /// <param name="title">The page title.</param>
    /// <returns>The HTML header content.</returns>
    private static string GetHtmlHeader(string title)
    {
        return $@"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>{title}</title>
    <style>
        * {{ margin: 0; padding: 0; box-sizing: border-box; }}
        body {{ font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif; line-height: 1.6; color: #333; background-color: #f5f5f5; }}
        .container {{ max-width: 1200px; margin: 0 auto; padding: 20px; }}
        .nav {{ background: #2c3e50; color: white; padding: 1rem 0; }}
        .nav .container {{ display: flex; justify-content: space-between; align-items: center; }}
        .nav h1 {{ color: white; }}
        .nav a {{ color: #ecf0f1; text-decoration: none; margin-left: 20px; }}
        .nav a:hover {{ color: #3498db; }}
        .hero {{ text-align: center; margin: 40px 0; padding: 40px; background: white; border-radius: 8px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
        .hero h1 {{ color: #2c3e50; margin-bottom: 20px; }}
        .features, .api-endpoints {{ background: white; margin: 20px 0; padding: 30px; border-radius: 8px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
        .features h2, .api-endpoints h2 {{ color: #2c3e50; margin-bottom: 20px; }}
        .features ul {{ list-style: none; }}
        .features li {{ padding: 8px 0; }}
        .product-grid {{ display: grid; grid-template-columns: repeat(auto-fill, minmax(300px, 1fr)); gap: 20px; margin-top: 20px; }}
        .product-card {{ background: white; padding: 20px; border-radius: 8px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
        .product-card h3 {{ color: #2c3e50; margin-bottom: 10px; }}
        .product-card h3 a {{ text-decoration: none; color: inherit; }}
        .product-card h3 a:hover {{ color: #3498db; }}
        .product-card .price {{ font-size: 1.2em; font-weight: bold; color: #27ae60; }}
        .product-card .stock {{ color: #7f8c8d; }}
        .product-card .created {{ color: #95a5a6; font-size: 0.9em; }}
        .product-detail {{ background: white; padding: 30px; border-radius: 8px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
        .product-detail h1 {{ color: #2c3e50; margin-bottom: 20px; }}
        .product-info {{ margin-top: 20px; }}
        .info-item {{ padding: 10px 0; border-bottom: 1px solid #ecf0f1; }}
        .actions {{ margin: 20px 0; }}
        .btn {{ display: inline-block; padding: 10px 20px; background: #3498db; color: white; text-decoration: none; border-radius: 4px; margin-right: 10px; }}
        .btn:hover {{ background: #2980b9; }}
        .btn-secondary {{ background: #95a5a6; }}
        .btn-secondary:hover {{ background: #7f8c8d; }}
        .endpoint-list {{ margin-top: 15px; }}
        .endpoint {{ margin: 10px 0; padding: 10px; background: #f8f9fa; border-radius: 4px; }}
        .method {{ padding: 4px 8px; border-radius: 3px; font-weight: bold; font-size: 0.8em; }}
        .method.get {{ background: #28a745; color: white; }}
        .method.post {{ background: #007bff; color: white; }}
        .method.put {{ background: #ffc107; color: black; }}
        .method.delete {{ background: #dc3545; color: white; }}
        code {{ background: #e9ecef; padding: 2px 6px; border-radius: 3px; font-family: 'Courier New', monospace; }}
    </style>
</head>";
    }

    /// <summary>
    /// Gets the navigation HTML.
    /// </summary>
    /// <returns>The navigation HTML content.</returns>
    private static string GetNavigation()
    {
        return @"<nav class=""nav"">
    <div class=""container"">
        <h1>Native HTTP Server</h1>
        <div>
            <a href=""/"">Home</a>
            <a href=""/products"">Products</a>
            <a href=""/api/products"">API</a>
        </div>
    </div>
</nav>";
    }
}
