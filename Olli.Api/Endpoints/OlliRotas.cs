using System.ComponentModel;
using Microsoft.EntityFrameworkCore;
using Olli.Api;

namespace Olli.Api.Endpoints;

public static class OlliRotas
{
    public static void RegistrarRotasOlli(this WebApplication app)
    {
        #region Rotas

        app.MapGet("/", () => "Olli API - Clyvo VET")
            .WithTags("status")
            .ExcludeFromDescription();

        var tutores = app.MapGroup("/tutores").WithTags("tutores");

        tutores.MapGet("/", ListarTutores)
            .WithName("ListarTutores")
            .WithSummary("Apresenta todos os tutores")
            .WithDescription("Apresenta todos os tutores cadastrados na Olli e a quantidade de pets vinculados.")
            .Produces<List<TutorDTO>>();

        tutores.MapGet("/{id:int}", BuscarTutor)
            .WithName("BuscarTutorPorId")
            .WithSummary("Busca um tutor por id")
            .WithDescription("Busca um tutor por id. Se o tutor nao for encontrado, retorna status code 404.")
            .Produces<TutorDTO>(200)
            .Produces(404);

        tutores.MapPost("/", CriarTutor)
            .WithName("CriarTutor")
            .WithSummary("Cria um novo tutor")
            .WithDescription("Cria um tutor responsavel por um ou mais pets na jornada preventiva da Olli.")
            .Produces<TutorDTO>(201)
            .Produces(400)
            .AddEndpointFilter<IdempotencyKeyRequiredEndpointFilter>()
            .Accepts<TutorDTO>("application/json");

        tutores.MapPut("/{id:int}", AtualizarTutor)
            .WithName("AtualizarTutor")
            .WithSummary("Atualiza um tutor")
            .Produces(204)
            .Produces(404);

        tutores.MapDelete("/{id:int}", DeletarTutor)
            .WithName("DeletarTutorPorId")
            .WithSummary("Deleta um tutor pelo seu id")
            .Produces(204)
            .Produces(404);

        var pets = app.MapGroup("/pets").WithTags("pets");

        pets.MapGet("/", ListarPets)
            .WithName("ListarPets")
            .WithSummary("Apresenta todos os pets")
            .WithDescription("Apresenta todos os pets cadastrados, incluindo tutor e score preventivo.")
            .Produces<List<PetDTO>>();

        pets.MapGet("/{id:int}", BuscarPet)
            .WithName("BuscarPetPorId")
            .WithSummary("Busca um pet por id")
            .WithDescription("Busca um pet por id. Se o pet nao for encontrado, retorna status code 404.")
            .Produces<PetDTO>(200)
            .Produces(404);

        pets.MapPost("/", CriarPet)
            .WithName("CriarPet")
            .WithSummary("Cadastra um novo pet")
            .WithDescription("Cadastra um pet e vincula ao tutor responsavel.")
            .Produces<PetDTO>(201)
            .Produces(400)
            .Produces(404)
            .AddEndpointFilter<IdempotencyKeyRequiredEndpointFilter>()
            .Accepts<PetDTO>("application/json");

        pets.MapPut("/{id:int}", AtualizarPet)
            .WithName("AtualizarPet")
            .WithSummary("Atualiza informacoes do pet")
            .Produces(204)
            .Produces(404);

        pets.MapDelete("/{id:int}", DeletarPet)
            .WithName("DeletarPetPorId")
            .WithSummary("Deleta um pet pelo seu id")
            .Produces(204)
            .Produces(404);

        pets.MapGet("/{id:int}/historico", ListarHistoricoSaude)
            .WithName("ListarHistoricoSaudeDoPet")
            .WithSummary("Apresenta o historico clinico do pet")
            .Produces<List<EventoSaudeDTO>>(200)
            .Produces(404);

        pets.MapPost("/{id:int}/historico", CriarEventoSaude)
            .WithName("CriarEventoSaudeDoPet")
            .WithSummary("Registra um evento no historico do pet")
            .Produces<EventoSaudeDTO>(201)
            .Produces(400)
            .Produces(404)
            .AddEndpointFilter<IdempotencyKeyRequiredEndpointFilter>()
            .Accepts<EventoSaudeDTO>("application/json");

        pets.MapGet("/{id:int}/historico/{idEvento:int}", BuscarEventoSaude)
            .WithName("BuscarEventoSaudeDoPet")
            .WithSummary("Busca um evento do historico do pet")
            .Produces<EventoSaudeDTO>(200)
            .Produces(404);

        pets.MapPut("/{id:int}/historico/{idEvento:int}", AtualizarEventoSaude)
            .WithName("AtualizarEventoSaudeDoPet")
            .WithSummary("Atualiza um evento do historico do pet")
            .Produces(204)
            .Produces(404)
            .Accepts<EventoSaudeDTO>("application/json");

        pets.MapDelete("/{id:int}/historico/{idEvento:int}", DeletarEventoSaude)
            .WithName("DeletarEventoSaudeDoPet")
            .WithSummary("Deleta um evento do historico do pet")
            .Produces(204)
            .Produces(404);

        pets.MapGet("/{id:int}/alertas", ListarAlertasPreventivos)
            .WithName("ListarAlertasPreventivosDoPet")
            .WithSummary("Apresenta os alertas preventivos do pet")
            .Produces<List<AlertaPreventivoDTO>>(200)
            .Produces(404);

        pets.MapPost("/{id:int}/alertas", CriarAlertaPreventivo)
            .WithName("CriarAlertaPreventivoDoPet")
            .WithSummary("Cria um alerta preventivo para o pet")
            .Produces<AlertaPreventivoDTO>(201)
            .Produces(400)
            .Produces(404)
            .AddEndpointFilter<IdempotencyKeyRequiredEndpointFilter>()
            .Accepts<AlertaPreventivoDTO>("application/json");

        pets.MapGet("/{id:int}/alertas/{idAlerta:int}", BuscarAlertaPreventivo)
            .WithName("BuscarAlertaPreventivoDoPet")
            .WithSummary("Busca um alerta preventivo do pet")
            .Produces<AlertaPreventivoDTO>(200)
            .Produces(404);

        pets.MapPut("/{id:int}/alertas/{idAlerta:int}", AtualizarAlertaPreventivo)
            .WithName("AtualizarAlertaPreventivoDoPet")
            .WithSummary("Atualiza um alerta preventivo do pet")
            .Produces(204)
            .Produces(404)
            .Accepts<AlertaPreventivoDTO>("application/json");

        pets.MapDelete("/{id:int}/alertas/{idAlerta:int}", DeletarAlertaPreventivo)
            .WithName("DeletarAlertaPreventivoDoPet")
            .WithSummary("Deleta um alerta preventivo do pet")
            .Produces(204)
            .Produces(404);

        pets.MapGet("/{id:int}/recomendacao-ia", GerarRecomendacaoIa)
            .WithName("GerarRecomendacaoIaDoPet")
            .WithSummary("Gera uma recomendacao preventiva simulada")
            .WithDescription("Simula a IA da Olli analisando score, historico e alertas pendentes do pet.")
            .Produces<RecomendacaoIaDTO>(200)
            .Produces(404);

        app.Run();

        #endregion
    }

    #region Servicos

    static async Task<IResult> ListarTutores(OlliDb db)
    {
        var tutores = await db.Tutores.Include(tutor => tutor.Pets).ToListAsync();
        return TypedResults.Ok(tutores.Select(tutor => new TutorDTO(tutor)).ToList());
    }

    static async Task<IResult> BuscarTutor([Description("id do tutor que sera buscado.")] int id, OlliDb db)
    {
        var tutor = await db.Tutores.Include(t => t.Pets).FirstOrDefaultAsync(t => t.Id == id);

        return tutor is not null
            ? TypedResults.Ok(new TutorDTO(tutor))
            : TypedResults.NotFound();
    }

    static async Task<IResult> CriarTutor([Description("Tutor a ser cadastrado")] TutorDTO tutorDTO, OlliDb db)
    {
        if (string.IsNullOrWhiteSpace(tutorDTO.Nome))
            return TypedResults.BadRequest("Nome do tutor e obrigatorio.");

        var tutor = new Tutor
        {
            Nome = tutorDTO.Nome,
            Email = tutorDTO.Email,
            Telefone = tutorDTO.Telefone
        };

        db.Tutores.Add(tutor);
        await db.SaveChangesAsync();

        tutorDTO = new TutorDTO(tutor);

        return TypedResults.Created($"/tutores/{tutor.Id}", tutorDTO);
    }

    static async Task<IResult> AtualizarTutor(
        [Description("id do tutor que sera atualizado.")] int id,
        TutorDTO tutorDTO,
        OlliDb db)
    {
        var tutor = await db.Tutores.FindAsync(id);

        if (tutor is null) return TypedResults.NotFound();

        tutor.Nome = tutorDTO.Nome;
        tutor.Email = tutorDTO.Email;
        tutor.Telefone = tutorDTO.Telefone;

        await db.SaveChangesAsync();

        return TypedResults.NoContent();
    }

    static async Task<IResult> DeletarTutor([Description("id do tutor que sera deletado.")] int id, OlliDb db)
    {
        if (await db.Tutores.FindAsync(id) is Tutor tutor)
        {
            db.Tutores.Remove(tutor);
            await db.SaveChangesAsync();
            return TypedResults.NoContent();
        }

        return TypedResults.NotFound();
    }

    static async Task<IResult> ListarPets(OlliDb db)
    {
        var pets = await db.Pets.Include(pet => pet.Tutor).ToListAsync();
        return TypedResults.Ok(pets.Select(pet => new PetDTO(pet)).ToList());
    }

    static async Task<IResult> BuscarPet([Description("id do pet que sera buscado.")] int id, OlliDb db)
    {
        var pet = await db.Pets.Include(p => p.Tutor).FirstOrDefaultAsync(p => p.Id == id);

        return pet is not null
            ? TypedResults.Ok(new PetDTO(pet))
            : TypedResults.NotFound();
    }

    static async Task<IResult> CriarPet([Description("Pet a ser cadastrado")] PetDTO petDTO, OlliDb db)
    {
        if (string.IsNullOrWhiteSpace(petDTO.Nome))
            return TypedResults.BadRequest("Nome do pet e obrigatorio.");

        var tutor = await db.Tutores.FindAsync(petDTO.IdTutor);
        if (tutor is null)
            return TypedResults.NotFound();

        var pet = new Pet
        {
            IdTutor = petDTO.IdTutor,
            Nome = petDTO.Nome,
            Especie = petDTO.Especie,
            Raca = petDTO.Raca,
            DataNascimento = petDTO.DataNascimento,
            PesoKg = petDTO.PesoKg,
            ScoreSaude = petDTO.ScoreSaude <= 0 ? 80 : Math.Clamp(petDTO.ScoreSaude, 0, 100),
            Observacoes = petDTO.Observacoes
        };

        db.Pets.Add(pet);
        await db.SaveChangesAsync();

        pet.Tutor = tutor;
        petDTO = new PetDTO(pet);

        return TypedResults.Created($"/pets/{pet.Id}", petDTO);
    }

    static async Task<IResult> AtualizarPet(
        [Description("id do pet que sera atualizado.")] int id,
        PetDTO petDTO,
        OlliDb db)
    {
        var pet = await db.Pets.FindAsync(id);

        if (pet is null) return TypedResults.NotFound();

        pet.Nome = petDTO.Nome;
        pet.Especie = petDTO.Especie;
        pet.Raca = petDTO.Raca;
        pet.DataNascimento = petDTO.DataNascimento;
        pet.PesoKg = petDTO.PesoKg;
        pet.ScoreSaude = Math.Clamp(petDTO.ScoreSaude, 0, 100);
        pet.Observacoes = petDTO.Observacoes;

        await db.SaveChangesAsync();

        return TypedResults.NoContent();
    }

    static async Task<IResult> DeletarPet([Description("id do pet que sera deletado.")] int id, OlliDb db)
    {
        if (await db.Pets.FindAsync(id) is Pet pet)
        {
            db.Pets.Remove(pet);
            await db.SaveChangesAsync();
            return TypedResults.NoContent();
        }

        return TypedResults.NotFound();
    }

    static async Task<IResult> ListarHistoricoSaude([Description("id do pet.")] int id, OlliDb db)
    {
        if (!await db.Pets.AnyAsync(pet => pet.Id == id))
            return TypedResults.NotFound();

        var historico = await db.EventosSaude
            .Where(evento => evento.IdPet == id)
            .OrderByDescending(evento => evento.DataEvento)
            .ToListAsync();

        return TypedResults.Ok(historico.Select(evento => new EventoSaudeDTO(evento)).ToList());
    }

    static async Task<IResult> CriarEventoSaude(
        [Description("id do pet.")] int id,
        EventoSaudeDTO eventoDTO,
        OlliDb db)
    {
        if (!await db.Pets.AnyAsync(pet => pet.Id == id))
            return TypedResults.NotFound();

        var evento = new EventoSaude
        {
            IdPet = id,
            Tipo = eventoDTO.Tipo,
            Titulo = eventoDTO.Titulo,
            Descricao = eventoDTO.Descricao,
            DataEvento = eventoDTO.DataEvento,
            NomeVeterinario = eventoDTO.NomeVeterinario,
            PrecisaRetorno = eventoDTO.PrecisaRetorno,
            DataRetornoSugerida = eventoDTO.DataRetornoSugerida
        };

        db.EventosSaude.Add(evento);
        await db.SaveChangesAsync();

        eventoDTO = new EventoSaudeDTO(evento);

        return TypedResults.Created($"/pets/{id}/historico/{evento.Id}", eventoDTO);
    }

    static async Task<IResult> BuscarEventoSaude(
        [Description("id do pet.")] int id,
        [Description("id do evento de saude.")] int idEvento,
        OlliDb db)
    {
        var evento = await db.EventosSaude.FirstOrDefaultAsync(evento => evento.Id == idEvento && evento.IdPet == id);

        return evento is not null
            ? TypedResults.Ok(new EventoSaudeDTO(evento))
            : TypedResults.NotFound();
    }

    static async Task<IResult> AtualizarEventoSaude(
        [Description("id do pet.")] int id,
        [Description("id do evento de saude.")] int idEvento,
        EventoSaudeDTO eventoDTO,
        OlliDb db)
    {
        var evento = await db.EventosSaude.FirstOrDefaultAsync(evento => evento.Id == idEvento && evento.IdPet == id);

        if (evento is null) return TypedResults.NotFound();

        evento.Tipo = eventoDTO.Tipo;
        evento.Titulo = eventoDTO.Titulo;
        evento.Descricao = eventoDTO.Descricao;
        evento.DataEvento = eventoDTO.DataEvento;
        evento.NomeVeterinario = eventoDTO.NomeVeterinario;
        evento.PrecisaRetorno = eventoDTO.PrecisaRetorno;
        evento.DataRetornoSugerida = eventoDTO.DataRetornoSugerida;

        await db.SaveChangesAsync();

        return TypedResults.NoContent();
    }

    static async Task<IResult> DeletarEventoSaude(
        [Description("id do pet.")] int id,
        [Description("id do evento de saude.")] int idEvento,
        OlliDb db)
    {
        var evento = await db.EventosSaude.FirstOrDefaultAsync(evento => evento.Id == idEvento && evento.IdPet == id);

        if (evento is null) return TypedResults.NotFound();

        db.EventosSaude.Remove(evento);
        await db.SaveChangesAsync();

        return TypedResults.NoContent();
    }

    static async Task<IResult> ListarAlertasPreventivos([Description("id do pet.")] int id, OlliDb db)
    {
        if (!await db.Pets.AnyAsync(pet => pet.Id == id))
            return TypedResults.NotFound();

        var alertas = await db.AlertasPreventivos
            .Where(alerta => alerta.IdPet == id)
            .OrderBy(alerta => alerta.DataPrevista)
            .ToListAsync();

        return TypedResults.Ok(alertas.Select(alerta => new AlertaPreventivoDTO(alerta)).ToList());
    }

    static async Task<IResult> CriarAlertaPreventivo(
        [Description("id do pet.")] int id,
        AlertaPreventivoDTO alertaDTO,
        OlliDb db)
    {
        if (!await db.Pets.AnyAsync(pet => pet.Id == id))
            return TypedResults.NotFound();

        var alerta = new AlertaPreventivo
        {
            IdPet = id,
            Tipo = alertaDTO.Tipo,
            Titulo = alertaDTO.Titulo,
            Descricao = alertaDTO.Descricao,
            DataPrevista = alertaDTO.DataPrevista,
            Status = alertaDTO.Status,
            RecomendacaoIa = alertaDTO.RecomendacaoIa
        };

        db.AlertasPreventivos.Add(alerta);
        await db.SaveChangesAsync();

        alertaDTO = new AlertaPreventivoDTO(alerta);

        return TypedResults.Created($"/pets/{id}/alertas/{alerta.Id}", alertaDTO);
    }

    static async Task<IResult> BuscarAlertaPreventivo(
        [Description("id do pet.")] int id,
        [Description("id do alerta preventivo.")] int idAlerta,
        OlliDb db)
    {
        var alerta = await db.AlertasPreventivos.FirstOrDefaultAsync(alerta => alerta.Id == idAlerta && alerta.IdPet == id);

        return alerta is not null
            ? TypedResults.Ok(new AlertaPreventivoDTO(alerta))
            : TypedResults.NotFound();
    }

    static async Task<IResult> AtualizarAlertaPreventivo(
        [Description("id do pet.")] int id,
        [Description("id do alerta preventivo.")] int idAlerta,
        AlertaPreventivoDTO alertaDTO,
        OlliDb db)
    {
        var alerta = await db.AlertasPreventivos.FirstOrDefaultAsync(alerta => alerta.Id == idAlerta && alerta.IdPet == id);

        if (alerta is null) return TypedResults.NotFound();

        alerta.Tipo = alertaDTO.Tipo;
        alerta.Titulo = alertaDTO.Titulo;
        alerta.Descricao = alertaDTO.Descricao;
        alerta.DataPrevista = alertaDTO.DataPrevista;
        alerta.Status = alertaDTO.Status;
        alerta.RecomendacaoIa = alertaDTO.RecomendacaoIa;

        await db.SaveChangesAsync();

        return TypedResults.NoContent();
    }

    static async Task<IResult> DeletarAlertaPreventivo(
        [Description("id do pet.")] int id,
        [Description("id do alerta preventivo.")] int idAlerta,
        OlliDb db)
    {
        var alerta = await db.AlertasPreventivos.FirstOrDefaultAsync(alerta => alerta.Id == idAlerta && alerta.IdPet == id);

        if (alerta is null) return TypedResults.NotFound();

        db.AlertasPreventivos.Remove(alerta);
        await db.SaveChangesAsync();

        return TypedResults.NoContent();
    }

    static async Task<IResult> GerarRecomendacaoIa([Description("id do pet.")] int id, OlliDb db)
    {
        var pet = await db.Pets
            .Include(p => p.Alertas)
            .Include(p => p.HistoricoSaude)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (pet is null) return TypedResults.NotFound();

        var alertasPendentes = pet.Alertas.Count(alerta => alerta.Status is StatusAlertaComoTexto.Pendente or StatusAlertaComoTexto.Atrasado);
        var ultimoEvento = pet.HistoricoSaude.OrderByDescending(evento => evento.DataEvento).FirstOrDefault();
        var prioridade = pet.ScoreSaude < 60 || alertasPendentes >= 2 ? "Alta" : pet.ScoreSaude < 80 ? "Media" : "Baixa";

        var recomendacao = new RecomendacaoIaDTO
        {
            IdPet = pet.Id,
            NomePet = pet.Nome,
            ScoreSaude = pet.ScoreSaude,
            NivelPrioridade = prioridade,
            Resumo = $"O pet {pet.Nome} possui score {pet.ScoreSaude} e {alertasPendentes} alerta(s) preventivo(s) pendente(s).",
            ProximasAcoes =
            [
                alertasPendentes > 0 ? "Revisar alertas preventivos em aberto." : "Manter rotina preventiva atual.",
                ultimoEvento?.PrecisaRetorno == true ? "Agendar retorno sugerido no ultimo atendimento." : "Registrar novos eventos clinicos quando houver consulta.",
                "A recomendacao da IA e apenas orientativa e nao substitui avaliacao veterinaria."
            ]
        };

        return TypedResults.Ok(recomendacao);
    }

    #endregion
}

public sealed class IdempotencyKeyRequiredEndpointFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        return context.HttpContext.Request.Headers.ContainsKey("IdempotencyKey")
            ? await next(context)
            : TypedResults.BadRequest("Header IdempotencyKey e obrigatorio para operacoes POST.");
    }
}
