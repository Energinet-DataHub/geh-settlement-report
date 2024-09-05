// Copyright 2020 Energinet DataHub A/S
//
// Licensed under the Apache License, Version 2.0 (the "License2");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Reflection;
using System.Text.Json.Serialization;
using Asp.Versioning;
using Energinet.DataHub.Core.App.WebApp.Extensions.Builder;
using Energinet.DataHub.Core.App.WebApp.Extensions.DependencyInjection;
using Energinet.DataHub.Core.Logging.LoggingMiddleware;

const string subsystemName = "mark-part";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpLoggingScope(subsystemName);
builder.Services.AddApplicationInsightsForWebApp(subsystemName);
builder.Services.AddHealthChecksForWebApp();

builder.Services
    .AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services
    .AddApiVersioningForWebApp(new ApiVersion(1, 0))
    .AddSwaggerForWebApp(Assembly.GetExecutingAssembly(), subsystemName);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseRouting();
app.UseSwaggerForWebApp();
app.UseHttpsRedirection();
// app.UseCommonExceptionHandling(exceptionBuilder =>
// {
//     exceptionBuilder.Use(new FluentValidationExceptionHandler("market_participant"));
//     exceptionBuilder.Use(new NotFoundValidationExceptionHandler("market_participant"));
//     exceptionBuilder.Use(new DataValidationExceptionHandler("market_participant"));
//     exceptionBuilder.Use(new FallbackExceptionHandler("market_participant"));
// });
app.UseLoggingScope();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers().RequireAuthorization();

app.MapLiveHealthChecks();
app.MapReadyHealthChecks();

app.Run();
