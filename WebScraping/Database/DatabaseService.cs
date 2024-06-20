namespace WebScraping;


using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

public class DatabaseService
{
    private readonly AppDbContext _dbContext;

    public DatabaseService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task SaveLink(string url)
    {
        var link = new Link { Url = url };
        _dbContext.Links.Add(link);
        await _dbContext.SaveChangesAsync();
    }
}
