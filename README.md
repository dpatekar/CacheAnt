# CacheAnt
.NET auto-refreshing cache

# Example
Define a caching definition:
```csharp
public class CashedCurrencies : AutoCached<IEnumerable<Currency>>
{
  private readonly IDataContext _dataContext;

  public CashedCurrencies(IDataContext dataContext)
  {
    _dataContext = dataContext;
  }

  public override TimeSpan AutoRefreshInterval => TimeSpan.FromSeconds(15);

  public override async Task<IEnumerable<Currency>> Compute()
  {
    return await _dataContext.Currency.AsNoTracking().ToListAsync();
  }
}
```
Add CacheAnt:
```csharp
services.AddCacheAnt(Assembly.GetExecutingAssembly());
```
Use it:
```csharp
public class SomeService
{
  private readonly CashedCurrencies _cashedCurrencies;

  public SomeService(CashedCurrencies cashedCurrencies)
  {
    _cashedCurrencies = cashedCurrencies;
  }

  public IEnumerable<Currency> GetCurrencies() => _cashedCurrencies.GetCached() ?? Enumerable.Empty<Currency>();
}
```

Background IHostedService CacheAntService is created and manages cache refresh in the background. Initial cache loading is done on application start.