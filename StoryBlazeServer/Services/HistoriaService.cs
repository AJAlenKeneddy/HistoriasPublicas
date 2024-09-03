using System.Net.Http.Json;
using System.Threading.Tasks;
using StoryBlazeServer.Models;


public class HistoriaService
{
    private readonly HttpClient _httpClient;

    public HistoriaService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<Historia>> GetHistoriasAsync()
    {
        var response = await _httpClient.GetFromJsonAsync<Response<List<Historia>>>("https://localhost:7184/api/Historias/ListarHistorias");

        if (response?.IsSuccess == true)
        {
            return response.Data;
        }
        return new List<Historia>();
    }
}
