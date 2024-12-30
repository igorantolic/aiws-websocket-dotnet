using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;

namespace AiWs.Api.Pages.Cache;

public class ShopWithCacheModel(IConfiguration configuration, IMemoryCache cache) : PageModel
{
    const string cacheKeyShopItems = "GitHubReposByDomains";
    public List<ShopItem>? items;

    public void OnGet()
    {
        items = cache.Get<List<ShopItem>>(cacheKeyShopItems);

        if (items == null)
        {
            items = GetDummyShopItems();
            cache.Set(cacheKeyShopItems, items, DateTime.Now.AddSeconds(5));
        }

    }

    List<ShopItem> GetDummyShopItems()
    {
        return new List<ShopItem> {
                new ShopItem { Name = $"Item1 {DateTime.Now}", Price = 10 },
                new ShopItem { Name = $"Item2  {DateTime.Now}", Price = 20 } };
    }
}

public class ShopItem
{
    public string Name { get; set; }
    public decimal Price { get; set; }
}