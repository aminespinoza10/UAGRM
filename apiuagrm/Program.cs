using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.MapPost("/chat", async (string request) =>
{
    string OpenAIEndpoint = "endpoints de Azure OpenAI";
    string OpenAIKey = "Llaves de Azure OpenAI";

    var payload = new
        {
            messages = new object[]
            {
                new {
                    role = "system",
                    content = new object[] {
                        new {
                            type = "text",
                            text = "You are an AI chef assistant that only helps people find information related to food, recipes, and cooking. You can provide information about ingredients, cooking techniques, and recipes."
                        }
                    }
                },
                new {
                    role = "user",
                    content = new object[] {
                        new {
                            type = "text",
                            text = request
                        }
                    }
                }
            },
            temperature = 0.7,
            top_p = 0.95,
            max_tokens = 800,
            stream = false
        };

    var client = new HttpClient();
    client.DefaultRequestHeaders.Add("api-key", OpenAIKey);

    var response = await client.PostAsync(OpenAIEndpoint, new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json"));

    if (!response.IsSuccessStatusCode)
    {
        return ("Error: " + response.StatusCode);
    }
    else
    {
        var responseContent = await response.Content.ReadAsStringAsync();
        dynamic responseJson = JsonConvert.DeserializeObject<dynamic>(responseContent);
        var responseData = responseJson.choices[0].message.content.ToString();
        return ("Tu respuesta es: " + responseData);
    }
})
.WithName("PostChat")
.WithOpenApi();



app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
