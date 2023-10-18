using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using PillCat.Facades.Strategies.Interfaces;
using PillCat.Models.Configuration;
using PillCat.Services.Interfaces;
using RestEase;
using Serilog;
using Serilog.Exceptions;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace PillCat.Facades.Extensions
{
    [ExcludeFromCodeCoverage]
    public static class ServiceCollectionExtensions
    {
        private const string APPLICATION_NAME = "PillCat";
        private const string API_VERSION = "v1";
        private const string XML_EXTENSION = ".xml";
        private const string INTERFACE_PREFIX = "I";
        private const string BASE_CLASS_PREFIX = "Base";
        private const string APPLICATION_KEY = "Application";
        private const string FACADE_ASSEMBLY = "PillCat.Facades";
        private const string FACADE_BASECLASS = "BaseFacade";
        private const string SERVICE_ASSEMBLY = "PillCat.Services";
        private const string SERVICE_BASECLASS = "BaseService";
        private const string EXCEPTION_ASSEMBLY = "PillCat.Facades";
        private const string EXCEPTION_BASECLASS = "ExceptionHandlingStrategy";
        private const string SETTINGS_NAME = "Settings";

        #region public methods
        /// <summary>
        /// Registers project's specific services
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        public static void AddSingletons(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton(configuration);
            AddServicesAndRepositories(services);
            AddExceptionHandlingStrategies(services);
            AddLogger(services, configuration);
            AddSwagger(services);
            AddSettings(services);
            AddOcrApiClient(services);
        }
        #endregion

        #region private methods
        private static void AddSwagger(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc(API_VERSION, new OpenApiInfo { Title = APPLICATION_NAME, Version = API_VERSION });
                var xmlFile = APPLICATION_NAME + XML_EXTENSION;
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });
        }

        private static void AddLogger(IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<ILogger>(new LoggerConfiguration()
               .ReadFrom.Configuration(configuration)
               .Enrich.WithProperty(APPLICATION_KEY, APPLICATION_NAME)
               .Enrich.WithExceptionDetails()
               .CreateLogger());
        }

        private static void AddServicesAndRepositories(IServiceCollection services)
        {
            var assembliesDictionary = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>(FACADE_ASSEMBLY, FACADE_BASECLASS),
                new KeyValuePair<string, string>(SERVICE_ASSEMBLY, SERVICE_BASECLASS)
            };

            foreach (var assemblie in assembliesDictionary)
            {
                AddTypesWithReflection(services, assemblie.Key, assemblie.Value);
            }
        }
        private static void AddExceptionHandlingStrategies(this IServiceCollection services)
        {
            services.AddSingleton(provider =>
            {
                var implementationTypes = GetImplementatonTypes(EXCEPTION_ASSEMBLY, EXCEPTION_BASECLASS);
                var logger = provider.GetService<ILogger>();
                var exceptionHandlingStrategies = new Dictionary<Type, IExceptionHandlingStrategy>();
                foreach (var implementationType in implementationTypes)
                {
                    var exceptionType = implementationType.BaseType.GetGenericArguments().First();
                    exceptionHandlingStrategies.Add(exceptionType,
                        (IExceptionHandlingStrategy)Activator.CreateInstance(implementationType, logger));
                }
                return exceptionHandlingStrategies;
            });
        }

        private static void AddTypesWithReflection(IServiceCollection services, string assemblyName, string baseClassName)
        {
            var implementationTypes = GetImplementatonTypes(assemblyName, baseClassName);

            foreach (var implementationType in implementationTypes)
            {
                var interfaceType = implementationType.GetInterface($"{INTERFACE_PREFIX}{implementationType.Name}")
                                        ?? implementationType.GetInterface(baseClassName.Replace(BASE_CLASS_PREFIX, INTERFACE_PREFIX));
                services.AddScoped(interfaceType, implementationType);
            }
        }
        private static void AddSettings(IServiceCollection services)
        {
            services.AddOptions<Settings>().Configure<IConfiguration>((settings, configuration) =>
            {
                configuration.Bind(SETTINGS_NAME, settings);
            });
        }

        private static void AddOcrApiClient(IServiceCollection services)
        {
            var ocrApi = RestClient.For<IOcrClient>("https://api.ocr.space");
            services.AddSingleton(ocrApi);
        }

        private static IEnumerable<Type> GetImplementatonTypes(string assemblyName, string baseClassName)
            => Assembly.Load(assemblyName).GetTypes().Where(x => x.BaseType != null && x.BaseType.Name.StartsWith(baseClassName));

        #endregion
    }
}