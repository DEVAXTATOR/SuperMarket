# GestaoSuper
### README.md

# Sistema de Gestão de Supermercado

Este projeto é uma aplicação ASP.NET Core para a gestão de um supermercado, incluindo funcionalidades como autenticação de utilizadores, visualização de produtos, processos de compra, e gestão de stock. A aplicação utiliza uma abordagem orientada a objetos e segue a arquitetura MVC.

## Estrutura do Projeto

- **CoreBusiness**: Contém as classes principais da lógica de negócios (`Category.cs`, `Product.cs`, `Transaction.cs`).
- **UseCases**: Define casos de uso específicos para a gestão de categorias, produtos e transações.
- **Plugins**: Contém repositórios de dados e implementação de stored procedures (`ProductSQLRepository.cs`, `MarketContext.cs`).
- **WebApp**: A aplicação web principal, incluindo controladores, vistas e modelos.

## Funcionalidades

### Autenticação de Utilizadores

A aplicação permite que os utilizadores se autentiquem utilizando um formulário de login com validação client-side.

**Ficheiro**: `WebApp/Areas/Identity/Pages/Account/Login.cshtml`
```html
<form asp-action="Login" method="post">
    <div asp-validation-summary="ModelOnly" class="text-danger"></div>
    <div class="form-floating mb-3">
        <input asp-for="Input.Email" class="form-control" autocomplete="username" placeholder="name@example.com" />
        <label asp-for="Input.Email">Email</label>
        <span asp-validation-for="Input.Email" class="text-danger"></span>
    </div>
    <div class="form-floating mb-3">
        <input asp-for="Input.Password" class="form-control" autocomplete="current-password" placeholder="password" />
        <label asp-for="Input.Password">Password</label>
        <span asp-validation-for="Input.Password" class="text-danger"></span>
    </div>
    <button type="submit" class="btn btn-primary">Login</button>
</form>
```

### Visualização de Produtos

Os utilizadores podem visualizar uma lista de produtos e detalhes de produtos específicos.

**Ficheiro**: `WebApp/Controllers/ProductsController.cs`
```csharp
public IActionResult Index()
{
    var products = _productRepository.GetProducts();
    return View(products);
}

public IActionResult Details(int id)
{
    var product = _productRepository.GetProductById(id);
    if (product == null)
    {
        return NotFound();
    }
    return View(product);
}
```

**Ficheiro**: `WebApp/Views/Products/Index.cshtml`
```html
@model IEnumerable<Product>

<h2>Products</h2>
<table class="table">
    <thead>
        <tr>
            <th>Name</th>
            <th>Category</th>
            <th>Price</th>
            <th>Quantity</th>
            <th></th>
        </tr>
    </thead>
    <tbody>
        @foreach (var product in Model)
        {
            <tr>
                <td>@product.Name</td>
                <td>@product.Category.Name</td>
                <td>@product.Price</td>
                <td>@product.Quantity</td>
                <td>
                    <a asp-action="Details" asp-route-id="@product.ProductId">Details</a>
                </td>
            </tr>
        }
    </tbody>
</table>
```

### Processo de Compra

Os utilizadores podem comprar produtos, com verificação de stock e atualização do inventário.

**Ficheiro**: `WebApp/Controllers/SalesController.cs`
```csharp
public IActionResult Create()
{
    ViewData["Products"] = new SelectList(_productRepository.GetProducts(), "ProductId", "Name");
    return View();
}

[HttpPost]
public IActionResult Create(Sale sale)
{
    if (ModelState.IsValid)
    {
        var product = _productRepository.GetProductById(sale.ProductId);
        if (product.Quantity >= sale.Quantity)
        {
            _saleRepository.AddSale(sale);
            product.Quantity -= sale.Quantity;
            _productRepository.UpdateProduct(product);
            return RedirectToAction(nameof(Index));
        }
        ModelState.AddModelError("", "Not enough stock available.");
    }
    ViewData["Products"] = new SelectList(_productRepository.GetProducts(), "ProductId", "Name");
    return View(sale);
}
```

**Ficheiro**: `WebApp/Views/Sales/Index.cshtml`
```html
@model Sale

<h2>Create Sale</h2>

<form asp-action="Create">
    <div class="form-group">
        <label asp-for="ProductId" class="control-label"></label>
        <select asp-for="ProductId" class="form-control" asp-items="ViewBag.Products"></select>
        <span asp-validation-for="ProductId" class="text-danger"></span>
    </div>
    <div class="form-group">
        <label asp-for="Quantity" class="control-label"></label>
        <input asp-for="Quantity" class="form-control" />
        <span asp-validation-for="Quantity" class="text-danger"></span>
    </div>
    <button type="submit" class="btn btn-primary">Create</button>
</form>

@section Scripts {
    @{await Html.RenderPartialAsync("_ValidationScriptsPartial");}
}
```

### Validação Client-Side

A aplicação utiliza scripts de validação client-side para garantir a entrada de dados correta nos formulários.

**Ficheiro**: `WebApp/Views/Shared/_ValidationScriptsPartial.cshtml`
```html
<environment include="Development">
    <script src="~/lib/jquery-validation/dist/jquery.validate.js"></script>
    <script src="~/lib/jquery-validation-unobtrusive/jquery.validate.unobtrusive.js"></script>
</environment>
<environment exclude="Development">
    <script src="https://ajax.aspnetcdn.com/ajax/jquery.validate/1.19.3/jquery.validate.min.js"></script>
    <script src="https://ajax.aspnetcdn.com/ajax/mvc/5.2.7/jquery.validate.unobtrusive.min.js"></script>
</environment>
```

### Gestão de Stock

A aplicação lida com a gestão de stock de produtos, incluindo a atualização das quantidades após vendas.

**Ficheiro**: `Plugins/Plugins.DataStore.SQL/ProductSQLRepository.cs`
```csharp
public class ProductSQLRepository : IProductRepository
{
    private readonly MarketContext _context;

    public ProductSQLRepository(MarketContext context)
    {
        _context = context;
    }

    public IEnumerable<Product> GetProducts()
    {
        return _context.Products.ToList();
    }

    public Product GetProductById(int productId)
    {
        return _context.Products.Find(productId);
    }

    public void AddProduct(Product product)
    {
        _context.Products.Add(product);
        _context.SaveChanges();
    }

    public void UpdateProduct(Product product)
    {
        _context.Products.Update(product);
        _context.SaveChanges();
    }

    public void RemoveProduct(int productId)
    {
        var product = _context.Products.Find(productId);
        if (product != null)
        {
            _context.Products.Remove(product);
            _context.SaveChanges();
        }
    }
}
```

### Configuração da Base de Dados

A configuração do Entity Framework é feita para gerir as tabelas e os dados da base de dados.

**Ficheiro**: `Plugins/Plugins.DataStore.SQL/MarketContext.cs`
```csharp
public class MarketContext : DbContext
{
    public MarketContext(DbContextOptions<MarketContext> options) : base(options)
    {
    }

    public DbSet<Category> Categories { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Transaction> Transactions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Category>().HasData(
            new Category { CategoryId = 1, Name = "Beverages" },
            new Category { CategoryId = 2, Name = "Snacks" }
        );
    }
}
```

## Instalação e Configuração

1. Clone o repositório para a sua máquina local.
2. Abra o projeto no Visual Studio.
3. Configure a string de conexão em `appsettings.json`.
4. Execute as migrações para criar a base de dados:
    ```sh
    dotnet ef database update
    ```
5. Execute a aplicação:
    ```sh
    dotnet run
    ```

## Contribuição

Se quiser contribuir para este projeto, por favor faça um fork do repositório, crie uma nova branch com as suas alterações, e submeta um pull request. Agradecemos as suas contribuições!

## Licença

Este projeto é licenciado sob os termos da licença MIT. Consulte o ficheiro `LICENSE` para mais informações.

---

Este README.md fornece uma visão geral completa do projeto de gestão de supermercado, incluindo a estrutura do código, funcionalidades principais, e instruções para instalação e contribuição.