using System.Text.Json.Serialization;

namespace Olli.Api;

[JsonConverter(typeof(JsonStringEnumConverter<TipoAlertaComoTexto>))]
public enum TipoAlertaComoTexto
{
    Vacina,
    Retorno,
    Checkup,
    Medicamento,
    Exame,
    SinalIoT
}
