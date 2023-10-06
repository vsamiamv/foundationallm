using FoundationaLLM.SemanticKernel.MemorySource;
using FoundationaLLM.SemanticKernel.Core.Interfaces;
using FoundationaLLM.SemanticKernel.Core.Models.ConfigurationOptions;
using FoundationaLLM.SemanticKernel.Core.Services;

namespace FoundationaLLM.SemanticKernel.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddApplicationInsightsTelemetry();
            builder.Services.AddAuthorization();
            builder.Services.AddControllers();
            builder.Services.AddApiVersioning();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddOptions<SemanticKernelServiceSettings>()
                .Bind(builder.Configuration.GetSection("FoundationaLLM"));
            builder.Services.AddSingleton<ISemanticKernelService, SemanticKernelService>();

            // Simple, static system prompt service
            //builder.Services.AddSingleton<ISystemPromptService, InMemorySystemPromptService>();

            // System prompt service backed by an Azure blob storage account
            builder.Services.AddOptions<DurableSystemPromptServiceSettings>()
                .Bind(builder.Configuration.GetSection("FoundationaLLM:DurableSystemPrompt"));
            builder.Services.AddSingleton<ISystemPromptService, DurableSystemPromptService>();

            builder.Services.AddOptions<AzureCognitiveSearchMemorySourceSettings>()
                .Bind(builder.Configuration.GetSection("FoundationaLLM:CognitiveSearchMemorySource"));
            builder.Services.AddTransient<IMemorySource, AzureCognitiveSearchMemorySource>();

            builder.Services.AddOptions<BlobStorageMemorySourceSettings>()
                .Bind(builder.Configuration.GetSection("FoundationaLLM:BlobStorageMemorySource"));
            builder.Services.AddTransient<IMemorySource, BlobStorageMemorySource>();

            var app = builder.Build();

            app.UseExceptionHandler(exceptionHandlerApp
                => exceptionHandlerApp.Run(async context
                    => await Results.Problem().ExecuteAsync(context)));

            // Configure the HTTP request pipeline.
            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}