using LibraryApplication.Interfaces;
using LibraryApplication.Interfaces.IRepository;
using LibraryApplication.Services;
using Microsoft.Extensions.DependencyInjection;

namespace LibraryApplication;
    
public static class DependencyInjectionApplication
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IBookService, BookService>();
        services.AddScoped<IAuthorService, AuthorService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<ILoanService, LoanService>();
        services.AddScoped<IUserService, UserService>();
        return services;
    }
}