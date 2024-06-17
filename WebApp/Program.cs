using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using WebApp.Data;
using Plugins.DataStore.SQL;
using UseCases;
using UseCases.CategoriesUseCases;
using UseCases.DataStorePluginInterfaces;
using UseCases.ProductsUseCases;

var builder = WebApplication.CreateBuilder(args);

// Adiciona o contexto do Identity
builder.Services.AddDbContext<AccountContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("MarketManagement"));
});

// Adiciona o contexto do Market
builder.Services.AddDbContext<MarketContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("MarketManagement"));
});

// Configura a identidade padrão
builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<AccountContext>();

// Configura serviços para Razor Pages e Controllers com Views
builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews();

// Configura a autorização
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Inventory", p => p.RequireClaim("Position", "Inventory"));
    options.AddPolicy("Cashiers", p => p.RequireClaim("Position", "Cashier"));
    options.AddPolicy("Admin", policy =>
           policy.RequireAssertion(context =>
               context.User.HasClaim("Position", "Admin") ||
               context.User.HasClaim("Position", "Inventory") ||
               context.User.HasClaim("Position", "Cashier")));
});

// Configura injeção de dependência para repositórios
builder.Services.AddTransient<ICategoryRepository, CategorySQLRepository>();
builder.Services.AddTransient<IProductRepository, ProductSQLRepository>();
builder.Services.AddTransient<ITransactionRepository, TransactionSQLRepository>();

// Configura injeção de dependência para casos de uso
builder.Services.AddTransient<IViewCategoriesUseCase, ViewCategoriesUseCase>();
builder.Services.AddTransient<IViewSelectedCategoryUseCase, ViewSelectedCategoryUseCase>();
builder.Services.AddTransient<IEditCategoryUseCase, EditCategoryUseCase>();
builder.Services.AddTransient<IAddCategoryUseCase, AddCategoryUseCase>();
builder.Services.AddTransient<IDeleteCategoryUseCase, DeleteCategoryUseCase>();

builder.Services.AddTransient<IViewProductsUseCase, ViewProductsUseCase>();
builder.Services.AddTransient<IAddProductUseCase, AddProductUseCase>();
builder.Services.AddTransient<IEditProductUseCase, EditProductUseCase>();
builder.Services.AddTransient<IViewProductsInCategoryUseCase, ViewProductsInCategoryUseCase>();
builder.Services.AddTransient<IDeleteProductUseCase, DeleteProductUseCase>();
builder.Services.AddTransient<IViewSelectedProductUseCase, ViewSelectedProductUseCase>();
builder.Services.AddTransient<ISellProductUseCase, SellProductUseCase>();

builder.Services.AddTransient<IRecordTransactionUseCase, RecordTransactionUseCase>();
builder.Services.AddTransient<IGetTodayTransactionsUseCase, GetTodayTransactionsUseCase>();
builder.Services.AddTransient<ISearchTransactionsUseCase, SearchTransactionsUseCase>();

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
