using System.Text.Json.Serialization;

namespace Olli.Api;

[JsonConverter(typeof(JsonStringEnumConverter<TipoEventoSaudeComoTexto>))]
public enum TipoEventoSaudeComoTexto
{
    Vacina,
    Consulta,
    Exame,
    Medicamento,
    Sintoma,
    Checkup
}
