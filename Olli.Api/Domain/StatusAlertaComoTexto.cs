using System.Text.Json.Serialization;

namespace Olli.Api;

[JsonConverter(typeof(JsonStringEnumConverter<StatusAlertaComoTexto>))]
public enum StatusAlertaComoTexto
{
    Pendente,
    Concluido,
    Atrasado,
    Cancelado
}
