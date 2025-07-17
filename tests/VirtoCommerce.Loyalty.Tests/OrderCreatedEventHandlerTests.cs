using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Moq;
using VirtoCommerce.Loyalty.Core.Services;
using VirtoCommerce.Loyalty.Data.Handlers;
using VirtoCommerce.Loyalty.Data.Models;
using VirtoCommerce.Loyalty.Data.Repositories;
using VirtoCommerce.Loyalty.Data.Services;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.Platform.Caching;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.GenericCrud;
using Xunit;
using Microsoft.Extensions.Logging;
using VirtoCommerce.LoyaltyProgramSearchService.Core.Services;
using VirtoCommerce.Loyalty.Core.Models;
using VirtoCommerce.Platform.Core.Common;
using System.Linq;

namespace VirtoCommerce.Loyalty.Tests;

[Trait("LoyaltyProgram", "Unit")]
public class OrderCreatedEventHandlerTests : IAsyncLifetime
{
    private SqliteConnection _connection;
    private LoyaltyDbContext _context;
    private ITransactionLogService _transactionService;
    private ITransactionLogSearchService _transactionSearchService;
    private ILoyaltyProgramSearchService _loyaltySearchService;
    private IEventPublisher _eventPublisher;
    private PlatformMemoryCache _platformMemoryCache;
    private Func<ILoyaltyProgramRepository> _repositoryFactory;
    private readonly List<LoyaltyProgramEntity> _loyaltyPrograms;
    private readonly List<TransactionLogEntity> _transactionLogs;
    private const string CustomerId = "cust-1";
    private const string ObjectType = "CustomerOrder";
    private const string OrderId1 = "order-1";
    private const string OrderId2 = "order-2";
    private const string LoyaltyProgramId1 = "lp-low";
    private const string LoyaltyProgramId2 = "lp-high";
    private const decimal DefaultPoints = 50m;
    private const string StoreId = "store-1";

    public OrderCreatedEventHandlerTests()
    {
        _loyaltyPrograms =
        [
            new()
            {
                Id = LoyaltyProgramId1,
                Name = "Low Priority Program",
                IsActive = true,
                Priority = 1,
                Conditions = "{}",
                Stores = [ new LoyaltyProgramStoreEntity { StoreId = StoreId, LoyaltyProgramId = LoyaltyProgramId1 } ]
            },
            new()
            {
                Id = LoyaltyProgramId2,
                Name = "High Priority Program",
                IsActive = true,
                Priority = 5,
                Conditions = "{}",
                Stores = [ new LoyaltyProgramStoreEntity { StoreId = StoreId, LoyaltyProgramId = LoyaltyProgramId2 } ]
            }
        ];

        _transactionLogs =
        [
            new ()
            {
                Id = Guid.NewGuid().ToString(),
                LoyaltyProgramId = LoyaltyProgramId1,
                CustomerId = CustomerId,
                OperationType = LoyaltyOperationType.Debit,
                Points = DefaultPoints,
                Balance = 50m,
                CreatedDate = DateTime.UtcNow.AddDays(-3),
                ObjectId = OrderId1,
                ObjectType = ObjectType,
                Comment = "First transaction"
            },
            new ()
            {
                Id = Guid.NewGuid().ToString(),
                LoyaltyProgramId = LoyaltyProgramId2,
                CustomerId = CustomerId,
                OperationType = LoyaltyOperationType.Debit,
                Points = DefaultPoints,
                Balance = 100m,
                CreatedDate = DateTime.UtcNow.AddDays(-2),
                ObjectId = OrderId2,
                ObjectType = ObjectType,
                Comment = "Second transaction"
            }
        ];
    }

    public async Task InitializeAsync()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        await _connection.OpenAsync();

        var dbOptions = new DbContextOptionsBuilder<LoyaltyDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new LoyaltyDbContext(dbOptions);
        await _context.Database.EnsureCreatedAsync();

        _repositoryFactory = () => new LoyaltyProgramRepository(new LoyaltyDbContext(dbOptions));

        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var cachingOptions = Options.Create(new CachingOptions
        {
            CacheEnabled = true,
            CacheAbsoluteExpiration = TimeSpan.FromMinutes(10),
            CacheSlidingExpiration = TimeSpan.FromMinutes(5)
        });
        var loggerMock = new Mock<ILogger<PlatformMemoryCache>>();
        _platformMemoryCache = new PlatformMemoryCache(memoryCache, cachingOptions, loggerMock.Object);

        _eventPublisher = new Mock<IEventPublisher>().Object;

        var crudOptions = Options.Create(new CrudOptions());

        var loyaltyService = new LoyaltyProgramService(_repositoryFactory, _platformMemoryCache, _eventPublisher);
        _loyaltySearchService = new Data.Services.LoyaltyProgramSearchService(_repositoryFactory, _platformMemoryCache, loyaltyService, crudOptions);

        var transactionService = new TransactionLogService(_repositoryFactory, _platformMemoryCache, _eventPublisher);
        _transactionSearchService = new TransactionLogSearchService(_repositoryFactory, _platformMemoryCache, transactionService, crudOptions);

        _transactionService = new TransactionLogService(_repositoryFactory, _platformMemoryCache, _eventPublisher);
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
        await _connection.CloseAsync();
    }

    [Fact]
    public async Task Handle_AddsTransactionLog_ForNewOrder()
    {
        // Arrange
        var handler = new OrderCreatedEventHandler(_loyaltySearchService, _transactionSearchService, _transactionService);

        _context.Set<LoyaltyProgramEntity>().AddRange(_loyaltyPrograms);
        await _context.SaveChangesAsync();

        var order = new CustomerOrder
        {
            Id = OrderId1,
            Number = "001",
            StoreId = StoreId,
            CustomerId = CustomerId
        };

        // Act
        await handler.HandleLoyaltyProgramUsages([order]);

        // Assert
        var transaction = await _context.Set<TransactionLogEntity>().FirstOrDefaultAsync();
        Assert.NotNull(transaction);
        Assert.Equal(LoyaltyProgramId1, transaction.LoyaltyProgramId);
        Assert.Equal(DefaultPoints, transaction.Points);
    }

    [Fact]
    public async Task CheckBalanceTransactionCorrectness()
    {
        // Arrange
        var handler = new OrderCreatedEventHandler(_loyaltySearchService, _transactionSearchService, _transactionService);
        const string LoyaltyProgramId = "lp-third";
        _loyaltyPrograms.Add(new LoyaltyProgramEntity()
        {
            Id = LoyaltyProgramId,
            Name = "Highest Priority Program",
            IsActive = true,
            Priority = 0,
            Conditions = "{}",
            Stores = [new LoyaltyProgramStoreEntity { StoreId = StoreId, LoyaltyProgramId = LoyaltyProgramId }]
        });
        _context.Set<LoyaltyProgramEntity>().AddRange(_loyaltyPrograms);
        _context.Set<TransactionLogEntity>().AddRange(_transactionLogs);
        await _context.SaveChangesAsync();

        var order = new CustomerOrder
        {
            Id = "order-3",
            Number = "001",
            StoreId = StoreId,
            CustomerId = CustomerId
        };

        // Act
        await handler.HandleLoyaltyProgramUsages([order]);

        // Assert
        var criteria = AbstractTypeFactory<TransactionLogSearchCriteria>.TryCreateInstance();
        criteria.LoyaltyProgramId = LoyaltyProgramId;
        criteria.Take = 1;
        var transaction = (await _transactionSearchService.SearchAsync(criteria)).Results.FirstOrDefault();
        Assert.NotNull(transaction);
        Assert.Equal(LoyaltyProgramId, transaction.LoyaltyProgramId);
        Assert.Equal(DefaultPoints, transaction.Points);
        Assert.Equal(150m, transaction.Balance);
    }
}
