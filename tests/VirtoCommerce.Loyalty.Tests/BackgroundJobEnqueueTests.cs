using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using VirtoCommerce.CustomerModule.Core.Services;
using VirtoCommerce.Loyalty.Core.Services;
using VirtoCommerce.Loyalty.Data.BackgroundJobs;
using VirtoCommerce.Loyalty.Data.Handlers;
using VirtoCommerce.Loyalty.Web;
using VirtoCommerce.OrdersModule.Core.Events;
using VirtoCommerce.OrdersModule.Core.Model;
using VirtoCommerce.OrdersModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.DistributedLock;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.Jobs;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Core.Security.Events;
using VirtoCommerce.StoreModule.Core.Model;
using VirtoCommerce.StoreModule.Core.Services;
using Xunit;

namespace VirtoCommerce.Loyalty.Tests;

// Any other test class that enqueues through the static BackgroundJob facade must join this collection:
// the facade has no reset API (Initialize rejects null), so Dispose leaves a DISPOSED provider behind in
// the static, and a class racing this one would see ObjectDisposedException from it.
[Collection(nameof(BackgroundJobEnqueueTests))]
[Trait("Category", "Unit")]
public class BackgroundJobEnqueueTests
{
    [Fact]
    public async Task LoyaltyProgram_OrdersAdded_EnqueuesOrdersJobWithNewNonPrototypeOrders()
    {
        //Arrange
        using var capture = new EnqueueCapture();
        var handler = CreateLoyaltyProgramHandler();

        var message = new OrderChangedEvent(
        [
            CreateOrderEntry("order1", EntryState.Added),
            CreateOrderEntry("order2", EntryState.Added, isPrototype: true),
            CreateOrderEntry("order3", EntryState.Modified),
        ]);

        //Act
        await handler.Handle(message);

        //Assert
        var payload = Assert.IsType<ProcessLoyaltyOrdersJobPayload>(capture.Payload);
        Assert.Equal(1, capture.EnqueueCount);
        Assert.Equal(["order1"], payload.OrderIds);
    }

    [Fact]
    public async Task LoyaltyProgram_NoOrdersAdded_EnqueuesNothing()
    {
        //Arrange
        using var capture = new EnqueueCapture();
        var handler = CreateLoyaltyProgramHandler();

        //Act
        await handler.Handle(new OrderChangedEvent([CreateOrderEntry("order1", EntryState.Modified)]));

        //Assert
        Assert.Equal(0, capture.EnqueueCount);
    }

    [Fact]
    public async Task LoyaltyProgram_UsersAdded_EnqueuesApplicationUsersJobWithNewUsers()
    {
        //Arrange
        using var capture = new EnqueueCapture();
        var handler = CreateLoyaltyProgramHandler();

        var message = new UserChangedEvent(
        [
            new GenericChangedEntry<ApplicationUser>(new ApplicationUser { Id = "user1" }, EntryState.Added),
            new GenericChangedEntry<ApplicationUser>(new ApplicationUser { Id = "user2" }, new ApplicationUser { Id = "user2" }, EntryState.Modified),
        ]);

        //Act
        await handler.Handle(message);

        //Assert
        var payload = Assert.IsType<ProcessLoyaltyApplicationUsersJobPayload>(capture.Payload);
        Assert.Equal(1, capture.EnqueueCount);
        Assert.Equal(["user1"], payload.ApplicationUserIds);
    }

    [Fact]
    public async Task LoyaltyMission_OrdersAdded_EnqueuesMissionsJobWithNewNonPrototypeOrders()
    {
        //Arrange
        using var capture = new EnqueueCapture();
        var handler = new LoyaltyMissionHandler(
            Mock.Of<ILoyaltyMissionLogicService>(),
            Mock.Of<IStoreService>(),
            Mock.Of<ICustomerOrderService>());

        var message = new OrderChangedEvent(
        [
            CreateOrderEntry("order1", EntryState.Added),
            CreateOrderEntry("order2", EntryState.Added, isPrototype: true),
        ]);

        //Act
        await handler.Handle(message);

        //Assert
        var payload = Assert.IsType<ProcessLoyaltyMissionsJobPayload>(capture.Payload);
        Assert.Equal(1, capture.EnqueueCount);
        Assert.Equal(["order1"], payload.OrderIds);
    }

    [Fact]
    public async Task ProcessLoyaltyMissionsJobHandler_RunsTheEventHandlerUnderTheLock()
    {
        //Arrange
        var order = new CustomerOrder { Id = "order1", StoreId = "store1" };
        var store = new Store { Id = "store1" };

        var orderServiceMock = new Mock<ICustomerOrderService>();
        orderServiceMock
            .Setup(x => x.GetAsync(It.IsAny<IList<string>>(), It.IsAny<string>(), false))
            .ReturnsAsync([order]);

        var storeServiceMock = new Mock<IStoreService>();
        storeServiceMock
            .Setup(x => x.GetAsync(It.IsAny<IList<string>>(), It.IsAny<string>(), false))
            .ReturnsAsync([store]);

        var missionLogicServiceMock = new Mock<ILoyaltyMissionLogicService>();
        var distributedLockMock = CreateDistributedLockMock();

        var eventHandler = new LoyaltyMissionHandler(missionLogicServiceMock.Object, storeServiceMock.Object, orderServiceMock.Object);
        var jobHandler = new ProcessLoyaltyMissionsJobHandler(eventHandler, distributedLockMock.Object);

        //Act
        await jobHandler.Execute(new ProcessLoyaltyMissionsJobPayload { OrderIds = ["order1"] }, context: null,
            TestContext.Current.CancellationToken);

        //Assert
        distributedLockMock.Verify(x => x.AcquireAsync("loyalty:job:process-missions", TimeSpan.FromSeconds(10), It.IsAny<CancellationToken>()), Times.Once);
        orderServiceMock.Verify(x => x.GetAsync(It.Is<IList<string>>(ids => ids.SequenceEqual(new[] { "order1" })), It.IsAny<string>(), false), Times.Once);
        missionLogicServiceMock.Verify(x => x.ProcessOrderAsync(order, store), Times.Once);
    }

    [Fact]
    public async Task ProcessLoyaltyOrdersJobHandler_LockNotAcquired_FailsSoTheEngineRetries()
    {
        //Arrange
        // Mirrors Hangfire's [DisableConcurrentExecution(10)]: a job that cannot get the lock in time must fail (and be
        // retried by the engine) rather than be skipped, otherwise the loyalty points of the orders would be lost.
        var orderServiceMock = new Mock<ICustomerOrderService>();
        var distributedLockMock = new Mock<IDistributedLock>();
        distributedLockMock
            .Setup(x => x.AcquireAsync(It.IsAny<string>(), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DistributedLockTimeoutException("loyalty:job:process-orders", TimeSpan.FromSeconds(10)));

        var eventHandler = CreateLoyaltyProgramHandler(orderServiceMock.Object);
        var jobHandler = new ProcessLoyaltyOrdersJobHandler(eventHandler, distributedLockMock.Object);

        //Act
        var act = () => jobHandler.Execute(new ProcessLoyaltyOrdersJobPayload { OrderIds = ["order1"] }, context: null,
            TestContext.Current.CancellationToken);

        //Assert
        await Assert.ThrowsAsync<DistributedLockTimeoutException>(act);
        orderServiceMock.Verify(x => x.GetAsync(It.IsAny<IList<string>>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public async Task ExpireLoyaltyMissionsJobHandler_ExpiresMissions()
    {
        //Arrange
        var missionLogicServiceMock = new Mock<ILoyaltyMissionLogicService>();
        var jobHandler = new ExpireLoyaltyMissionsJobHandler(missionLogicServiceMock.Object);

        //Act
        await jobHandler.Execute(new ExpireLoyaltyMissionsJobPayload(), context: null, TestContext.Current.CancellationToken);

        //Assert
        missionLogicServiceMock.Verify(x => x.ExpireMissionsAsync(), Times.Once);
    }

    [Fact]
    public void Module_RegistersBackgroundJobsAndExpireMissionsSchedule()
    {
        //Arrange
        var services = new ServiceCollection();
        var module = new Module { Configuration = new ConfigurationBuilder().Build() };

        //Act
        module.Initialize(services);

        //Assert
        var descriptors = services
            .Where(x => x.ServiceType == typeof(BackgroundJobDescriptor))
            .Select(x => (BackgroundJobDescriptor)x.ImplementationInstance)
            .ToDictionary(x => x.Name);

        Assert.False(descriptors[nameof(ProcessLoyaltyOrdersJobHandler)].Triggerable);
        Assert.False(descriptors[nameof(ProcessLoyaltyApplicationUsersJobHandler)].Triggerable);
        Assert.False(descriptors[nameof(ProcessLoyaltyMissionsJobHandler)].Triggerable);
        Assert.Contains(nameof(ExpireLoyaltyMissionsJobHandler), descriptors.Keys);

        // The id and the daily UTC cron of the former Hangfire registration are kept, so the existing schedule is replaced.
        var registration = Assert.Single(services
            .Where(x => x.ServiceType == typeof(RecurringJobRegistration))
            .Select(x => (RecurringJobRegistration)x.ImplementationInstance));
        Assert.Equal("Loyalty.ExpireMissions", registration.Id);
        Assert.Equal("0 0 * * *", registration.CronExpression);
        Assert.Equal(TimeZoneInfo.Utc, registration.TimeZone);
        Assert.Equal(typeof(ExpireLoyaltyMissionsJobHandler).FullName, registration.HandlerTypeName);
    }

    private static LoyaltyProgramHandler CreateLoyaltyProgramHandler(ICustomerOrderService customerOrderService = null)
    {
        return new LoyaltyProgramHandler(
            Mock.Of<ILoyaltyLogicService>(),
            Mock.Of<IStoreService>(),
            customerOrderService ?? Mock.Of<ICustomerOrderService>(),
            Mock.Of<ILoyaltyPointsCalculator>(),
            Mock.Of<IMemberService>(),
            () => throw new InvalidOperationException("UserManager is not expected to be used."));
    }

    private static GenericChangedEntry<CustomerOrder> CreateOrderEntry(string id, EntryState entryState, bool isPrototype = false)
    {
        var order = new CustomerOrder { Id = id, IsPrototype = isPrototype };

        return new GenericChangedEntry<CustomerOrder>(order, order, entryState);
    }

    private static Mock<IDistributedLock> CreateDistributedLockMock()
    {
        var distributedLockMock = new Mock<IDistributedLock>();
        distributedLockMock
            .Setup(x => x.AcquireAsync(It.IsAny<string>(), It.IsAny<TimeSpan?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Mock.Of<IDistributedLockHandle>());

        return distributedLockMock;
    }

    // Captures what a handler enqueued through the static BackgroundJob facade. IBackgroundJob is registered
    // Scoped here exactly as the engine module registers it, so this also proves the facade's per-call scope
    // resolves it - the handlers themselves are root-resolved and must never hold it.
    private sealed class EnqueueCapture : IDisposable
    {
        private readonly ServiceProvider _provider;

        public EnqueueCapture()
        {
            Setup<ProcessLoyaltyOrdersJobHandler>();
            Setup<ProcessLoyaltyApplicationUsersJobHandler>();
            Setup<ProcessLoyaltyMissionsJobHandler>();

            var services = new ServiceCollection();
            services.AddScoped(_ => BackgroundJobMock.Object);
            _provider = services.BuildServiceProvider(validateScopes: true);

            BackgroundJob.Initialize(_provider);
        }

        public Mock<IBackgroundJob> BackgroundJobMock { get; } = new();

        public object Payload { get; private set; }

        public int EnqueueCount { get; private set; }

        public void Dispose()
        {
            _provider.Dispose();
        }

        private void Setup<THandler>()
            where THandler : class
        {
            BackgroundJobMock
                .Setup(x => x.Enqueue<THandler>(It.IsAny<object>(), It.IsAny<EnqueueOptions>(), It.IsAny<CancellationToken>()))
                .Callback<object, EnqueueOptions, CancellationToken>((payload, _, _) =>
                {
                    Payload = payload;
                    EnqueueCount++;
                })
                .ReturnsAsync("job-id");
        }
    }
}
