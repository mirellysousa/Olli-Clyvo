using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Olli.Api;

namespace Olli.Api.Tests;

public class OlliApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public OlliApiTests(WebApplicationFactory<Program> factory)
    {
        _client = factory
            .WithWebHostBuilder(builder => builder.UseEnvironment("Testing"))
            .CreateClient();
    }

    [Fact]
    public async Task ListarPets_DeveRetornar200OkComLista()
    {
        var response = await _client.GetAsync("/pets");

        response.EnsureSuccessStatusCode();
        var pets = await response.Content.ReadFromJsonAsync<List<PetDTO>>();

        Assert.IsType<List<PetDTO>>(pets);
    }

    [Fact]
    public async Task CriarPet_SemIdempotencyKey_DeveRetornar400BadRequest()
    {
        var pet = new PetDTO()
        {
            IdTutor = 1,
            Nome = "Nina",
            Especie = EspeciePetComoTexto.Gato,
            Raca = "SRD",
            DataNascimento = new DateTime(2021, 3, 12),
            PesoKg = 4.2m,
            ScoreSaude = 88
        };

        var response = await _client.PostAsJsonAsync("/pets", pet);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("IdempotencyKey", content);
    }

    [Fact]
    public async Task CriarPet_ComIdempotencyKey_DeveRetornar201Created()
    {
        var pet = new PetDTO()
        {
            IdTutor = 1,
            Nome = "Nina",
            Especie = EspeciePetComoTexto.Gato,
            Raca = "SRD",
            DataNascimento = new DateTime(2021, 3, 12),
            PesoKg = 4.2m,
            ScoreSaude = 88
        };

        _client.DefaultRequestHeaders.Add("IdempotencyKey", Guid.NewGuid().ToString());

        var response = await _client.PostAsJsonAsync("/pets", pet);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Theory]
    [InlineData(1, HttpStatusCode.OK)]
    [InlineData(-9999, HttpStatusCode.NotFound)]
    public async Task BuscarPetPorId_DeveRetornarStatusCodeCorreto(int id, HttpStatusCode statusCode)
    {
        var response = await _client.GetAsync($"/pets/{id}");

        Assert.Equal(statusCode, response.StatusCode);
    }

    [Fact]
    public async Task GerarRecomendacaoIa_DeveRetornar200Ok()
    {
        var response = await _client.GetAsync("/pets/1/recomendacao-ia");

        response.EnsureSuccessStatusCode();
        var recomendacao = await response.Content.ReadFromJsonAsync<RecomendacaoIaDTO>();

        Assert.NotNull(recomendacao);
        Assert.Equal(1, recomendacao.IdPet);
    }

    [Fact]
    public async Task HistoricoSaude_DevePermitirCrudCompleto()
    {
        await Task.Delay(TimeSpan.FromSeconds(2));

        var evento = new EventoSaudeDTO
        {
            Tipo = TipoEventoSaudeComoTexto.Consulta,
            Titulo = "Consulta preventiva",
            Descricao = "Consulta de rotina para acompanhamento.",
            DataEvento = new DateTime(2026, 5, 18),
            NomeVeterinario = "Dra. Clyvo",
            PrecisaRetorno = true,
            DataRetornoSugerida = new DateTime(2026, 6, 18)
        };

        using var createRequest = new HttpRequestMessage(HttpMethod.Post, "/pets/1/historico")
        {
            Content = JsonContent.Create(evento)
        };
        createRequest.Headers.Add("IdempotencyKey", Guid.NewGuid().ToString());

        var createResponse = await _client.SendAsync(createRequest);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var eventoCriado = await createResponse.Content.ReadFromJsonAsync<EventoSaudeDTO>();
        Assert.NotNull(eventoCriado);

        var getResponse = await _client.GetAsync($"/pets/1/historico/{eventoCriado.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        eventoCriado.Titulo = "Consulta preventiva atualizada";
        eventoCriado.PrecisaRetorno = false;

        var updateResponse = await _client.PutAsJsonAsync($"/pets/1/historico/{eventoCriado.Id}", eventoCriado);
        Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);

        var deleteResponse = await _client.DeleteAsync($"/pets/1/historico/{eventoCriado.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getDeletedResponse = await _client.GetAsync($"/pets/1/historico/{eventoCriado.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getDeletedResponse.StatusCode);
    }

    [Fact]
    public async Task AlertasPreventivos_DevePermitirCrudCompleto()
    {
        await Task.Delay(TimeSpan.FromSeconds(2));

        var alerta = new AlertaPreventivoDTO
        {
            Tipo = TipoAlertaComoTexto.Vacina,
            Titulo = "Reforco de vacina",
            Descricao = "Verificar carteira de vacinas.",
            DataPrevista = new DateTime(2026, 6, 30),
            Status = StatusAlertaComoTexto.Pendente,
            RecomendacaoIa = "Enviar lembrete ao tutor."
        };

        using var createRequest = new HttpRequestMessage(HttpMethod.Post, "/pets/1/alertas")
        {
            Content = JsonContent.Create(alerta)
        };
        createRequest.Headers.Add("IdempotencyKey", Guid.NewGuid().ToString());

        var createResponse = await _client.SendAsync(createRequest);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var alertaCriado = await createResponse.Content.ReadFromJsonAsync<AlertaPreventivoDTO>();
        Assert.NotNull(alertaCriado);

        var getResponse = await _client.GetAsync($"/pets/1/alertas/{alertaCriado.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        alertaCriado.Titulo = "Reforco de vacina atualizado";
        alertaCriado.Status = StatusAlertaComoTexto.Concluido;

        var updateResponse = await _client.PutAsJsonAsync($"/pets/1/alertas/{alertaCriado.Id}", alertaCriado);
        Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);

        var deleteResponse = await _client.DeleteAsync($"/pets/1/alertas/{alertaCriado.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getDeletedResponse = await _client.GetAsync($"/pets/1/alertas/{alertaCriado.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getDeletedResponse.StatusCode);
    }

    [Fact]
    public async Task ListarPets_MuitasVezes_DeveAcionarRateLimitCom429()
    {
        await Task.Delay(TimeSpan.FromSeconds(2));

        var response = await _client.GetAsync("/pets");

        foreach (var _ in Enumerable.Range(0, 100))
        {
            response = await _client.GetAsync("/pets");
            if (response.StatusCode == HttpStatusCode.TooManyRequests)
                break;
        }

        Assert.Equal(HttpStatusCode.TooManyRequests, response.StatusCode);

        await Task.Delay(TimeSpan.FromSeconds(2));

        response = await _client.GetAsync("/pets");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
