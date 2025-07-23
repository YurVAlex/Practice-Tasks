using System;

namespace DI_task
{
    internal class Program
    {
        // ====================================================================
        // Introduction to Dependency Injection (DI)
        // ====================================================================
        // Dependency Injection is a design pattern that allows you to remove hard-coded dependencies
        // among objects, making your code more flexible, testable, and maintainable.
        // Instead of an object creating its dependencies, those dependencies are "injected" into it.
        //
        // Key benefits:
        // 1. Decoupling: Components are less reliant on specific implementations.
        // 2. Testability: Easier to mock or substitute dependencies for unit testing.
        // 3. Reusability: Components can be reused in different contexts with different dependencies.
        // 4. Maintainability: Changes to one dependency don't necessarily break others.
        //
        // The most common form of DI is Constructor Injection, where dependencies are passed
        // through the constructor of a class.

        // ====================================================================
        // Task 1: Basic Dependency Injection (Manual)
        // Goal: Understand the core concept of passing dependencies.
        // ====================================================================

        // 1.1 Define an interface for a logger.
        public interface ILogger
        {
            void Log(string message);
        }

        // 1.2 Create a concrete implementation of the logger.
        public class ConsoleLogger : ILogger
        {
            public void Log(string message)
            {
                Console.WriteLine($"[ConsoleLog] {message}");
            }
        }

        // 1.3 Create a service that needs to use a logger.
        // Initially, imagine if ReportGenerator created its own ConsoleLogger directly.
        // That would tightly couple ReportGenerator to ConsoleLogger.
        public class ReportGenerator
        {
            private readonly ILogger _logger; // Dependency declared as an interface

            // 1.4 Implement Constructor Injection: The logger is passed in via the constructor.
            public ReportGenerator(ILogger logger)
            {
                _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            }

            public void GenerateReport(string reportName)
            {
                _logger.Log($"Starting report generation for: {reportName}");
                // Simulate report generation logic
                Console.WriteLine($"Generating complex report: {reportName}...");
                _logger.Log($"Finished report generation for: {reportName}");
            }
        }

        // ====================================================================
        // Task 2: Dependency Injection with an IoC Container
        // Goal: Learn how an Inversion of Control (IoC) container automates DI.
        //       We'll use Microsoft.Extensions.DependencyInjection, a common .NET container.
        // ====================================================================

        // 2.1 Define another service interface and implementation.
        public interface IDataService
        {
            List<string> GetData();
        }

        public class MockDataService : IDataService
        {
            public List<string> GetData()
            {
                return new List<string> { "Data Item 1", "Data Item 2", "Data Item 3" };
            }
        }

        // 2.2 Create a new service that depends on both ILogger and IDataService.
        public class DashboardService
        {
            private readonly ILogger _logger;
            private readonly IDataService _dataService;

            public DashboardService(ILogger logger, IDataService dataService)
            {
                _logger = logger ?? throw new ArgumentNullException(nameof(logger));
                _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
            }

            public void DisplayDashboard()
            {
                _logger.Log("Displaying dashboard...");
                var data = _dataService.GetData();
                Console.WriteLine("Dashboard Data:");
                foreach (var item in data)
                {
                    Console.WriteLine($"- {item}");
                }
                _logger.Log("Dashboard displayed.");
            }
        }

        // ====================================================================
        // Task 3: Understanding Service Lifetimes
        // Goal: Observe the difference between Transient, Scoped, and Singleton lifetimes.
        // ====================================================================

        // 3.1 Create a simple service to track its instance and a counter.
        public class LifetimeTrackerService
        {
            private static int _instanceCounter = 0;
            private int _instanceId;
            private int _callCounter = 0;

            public LifetimeTrackerService()
            {
                _instanceId = System.Threading.Interlocked.Increment(ref _instanceCounter);
                Console.WriteLine($"  LifetimeTrackerService Instance {_instanceId} Created.");
            }

            public void PerformAction(string caller)
            {
                _callCounter++;
                Console.WriteLine($"  [{caller}] LifetimeTrackerService Instance {_instanceId} - Call {_callCounter}");
            }
        }

        // 3.2 Create a service that depends on LifetimeTrackerService to demonstrate lifetimes.
        public class LifetimeDemonstrator
        {
            private readonly LifetimeTrackerService _tracker;

            public LifetimeDemonstrator(LifetimeTrackerService tracker)
            {
                _tracker = tracker;
            }

            public void ShowLifetime(string caller)
            {
                _tracker.PerformAction(caller);
            }
        }

        // ====================================================================
        // Task 4: Advanced Scenario - Multiple Implementations / Factory Pattern
        // Goal: Learn how to handle scenarios where you have multiple implementations
        //       of an interface and need to select one dynamically.
        // ====================================================================

        // 4.1 Define an interface for notifications.
        public interface INotificationService
        {
            void SendNotification(string recipient, string message);
        }

        // 4.2 Create multiple implementations.
        public class EmailNotificationService : INotificationService
        {
            public void SendNotification(string recipient, string message)
            {
                Console.WriteLine($"Sending Email to {recipient}: {message}");
            }
        }

        public class SmsNotificationService : INotificationService
        {
            public void SendNotification(string recipient, string message)
            {
                Console.WriteLine($"Sending SMS to {recipient}: {message}");
            }
        }

        // 4.3 Implement a factory to select the correct notification service.
        // This factory will be injected, and it will resolve the specific service.
        public class NotificationServiceFactory
        {
            private readonly IServiceProvider _serviceProvider;

            public NotificationServiceFactory(IServiceProvider serviceProvider)
            {
                _serviceProvider = serviceProvider;
            }

            public INotificationService GetService(string type)
            {
                return type.ToLower() switch
                {
                    "email" => _serviceProvider.GetRequiredService<EmailNotificationService>(),
                    "sms" => _serviceProvider.GetRequiredService<SmsNotificationService>(),
                    _ => throw new ArgumentException("Invalid notification type"),
                };
            }
        }

        public class NotificationSender
        {
            private readonly NotificationServiceFactory _factory;

            public NotificationSender(NotificationServiceFactory factory)
            {
                _factory = factory;
            }

            public void Send(string type, string recipient, string message)
            {
                var service = _factory.GetService(type);
                service.SendNotification(recipient, message);
            }
        }


        // ====================================================================
        // Main Program
        // ====================================================================
        class Program
        {
            static async Task Main(string[] args)
            {
                Console.WriteLine("--- Starting DI Practice ---");
                Console.WriteLine("\n--- Task 1: Basic Dependency Injection (Manual) ---");

                // Manually create and inject the dependency
                ILogger manualLogger = new ConsoleLogger();
                ReportGenerator manualReportGenerator = new ReportGenerator(manualLogger);
                manualReportGenerator.GenerateReport("Sales Report Q1");

                Console.WriteLine("\n--- Task 2: Dependency Injection with an IoC Container ---");

                // 2.3 Set up the IoC container (ServiceCollection)
                var services = new ServiceCollection();

                // 2.4 Register services with the container
                // Register ILogger to use ConsoleLogger whenever ILogger is requested.
                services.AddSingleton<ILogger, ConsoleLogger>();

                // Register IDataService to use MockDataService.
                services.AddTransient<IDataService, MockDataService>();

                // Register ReportGenerator (it will automatically resolve ILogger because it's registered)
                services.AddTransient<ReportGenerator>();

                // Register DashboardService (it will automatically resolve ILogger and IDataService)
                services.AddTransient<DashboardService>();

                // 2.5 Build the service provider. This is where the container is finalized.
                using (ServiceProvider serviceProvider = services.BuildServiceProvider())
                {
                    // 2.6 Resolve services from the container
                    // The container handles creating ReportGenerator and injecting ConsoleLogger.
                    var reportGeneratorFromContainer = serviceProvider.GetRequiredService<ReportGenerator>();
                    reportGeneratorFromContainer.GenerateReport("Financial Report Q2");

                    // Resolve DashboardService and see its dependencies injected.
                    var dashboardService = serviceProvider.GetRequiredService<DashboardService>();
                    dashboardService.DisplayDashboard();
                }

                Console.WriteLine("\n--- Task 3: Understanding Service Lifetimes ---");

                var lifetimeServices = new ServiceCollection();

                // 3.3 Register LifetimeTrackerService with different lifetimes and observe.

                Console.WriteLine("\n--- Transient Lifetime ---");
                // Transient: A new instance is created every time it's requested.
                lifetimeServices.AddTransient<LifetimeTrackerService>();
                lifetimeServices.AddTransient<LifetimeDemonstrator>(); // Depends on LifetimeTrackerService

                using (ServiceProvider transientProvider = lifetimeServices.BuildServiceProvider())
                {
                    Console.WriteLine("  Requesting Transient 1:");
                    var demo1 = transientProvider.GetRequiredService<LifetimeDemonstrator>();
                    demo1.ShowLifetime("Transient 1 - Call A");
                    demo1.ShowLifetime("Transient 1 - Call B"); // Same instance for demo1, but if resolved again, new instance

                    Console.WriteLine("  Requesting Transient 2 (new instance expected):");
                    var demo2 = transientProvider.GetRequiredService<LifetimeDemonstrator>();
                    demo2.ShowLifetime("Transient 2 - Call A");
                }

                Console.WriteLine("\n--- Scoped Lifetime ---");
                // Scoped: A single instance is created per scope (e.g., per web request).
                // For console apps, a scope is explicitly created.
                var scopedServices = new ServiceCollection();
                scopedServices.AddScoped<LifetimeTrackerService>();
                scopedServices.AddScoped<LifetimeDemonstrator>();

                using (ServiceProvider scopedProvider = scopedServices.BuildServiceProvider())
                {
                    Console.WriteLine("  Scope 1:");
                    using (var scope1 = scopedProvider.CreateScope())
                    {
                        var demoScope1_1 = scope1.ServiceProvider.GetRequiredService<LifetimeDemonstrator>();
                        demoScope1_1.ShowLifetime("Scope 1 - Instance 1 - Call A");
                        demoScope1_1.ShowLifetime("Scope 1 - Instance 1 - Call B");

                        var demoScope1_2 = scope1.ServiceProvider.GetRequiredService<LifetimeDemonstrator>();
                        demoScope1_2.ShowLifetime("Scope 1 - Instance 2 - Call A (should be same as Instance 1)");
                    } // Scope 1 ends, scoped services are disposed

                    Console.WriteLine("  Scope 2 (new instance expected):");
                    using (var scope2 = scopedProvider.CreateScope())
                    {
                        var demoScope2_1 = scope2.ServiceProvider.GetRequiredService<LifetimeDemonstrator>();
                        demoScope2_1.ShowLifetime("Scope 2 - Instance 1 - Call A");
                    }
                }

                Console.WriteLine("\n--- Singleton Lifetime ---");
                // Singleton: A single instance is created for the entire lifetime of the application.
                var singletonServices = new ServiceCollection();
                singletonServices.AddSingleton<LifetimeTrackerService>();
                singletonServices.AddTransient<LifetimeDemonstrator>(); // Demonstrator can be transient, but it uses the *same* singleton tracker

                using (ServiceProvider singletonProvider = singletonServices.BuildServiceProvider())
                {
                    Console.WriteLine("  Requesting Singleton 1:");
                    var demoSingleton1 = singletonProvider.GetRequiredService<LifetimeDemonstrator>();
                    demoSingleton1.ShowLifetime("Singleton 1 - Call A");
                    demoSingleton1.ShowLifetime("Singleton 1 - Call B");

                    Console.WriteLine("  Requesting Singleton 2 (same instance expected):");
                    var demoSingleton2 = singletonProvider.GetRequiredService<LifetimeDemonstrator>();
                    demoSingleton2.ShowLifetime("Singleton 2 - Call A");
                }


                Console.WriteLine("\n--- Task 4: Advanced Scenario - Multiple Implementations / Factory Pattern ---");

                var notificationServices = new ServiceCollection();

                // 4.4 Register all concrete implementations (even if they implement the same interface)
                // It's common to register concrete types if they are only resolved via a factory.
                notificationServices.AddTransient<EmailNotificationService>();
                notificationServices.AddTransient<SmsNotificationService>();

                // 4.5 Register the factory itself.
                notificationServices.AddTransient<NotificationServiceFactory>();

                // 4.6 Register the sender that uses the factory.
                notificationServices.AddTransient<NotificationSender>();

                using (ServiceProvider notificationProvider = notificationServices.BuildServiceProvider())
                {
                    var sender = notificationProvider.GetRequiredService<NotificationSender>();

                    sender.Send("email", "alice@example.com", "Your order has shipped!");
                    sender.Send("sms", "123-456-7890", "Your appointment is tomorrow.");

                    try
                    {
                        sender.Send("push", "bob_id", "New message!");
                    }
                    catch (ArgumentException ex)
                    {
                        Console.WriteLine($"Error: {ex.Message}");
                    }
                }

                Console.WriteLine("\n--- DI Practice Complete ---");
                Console.ReadKey(); // Keep console open
            }
        }
    }

}

