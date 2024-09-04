using System.Net.Http.Json;
using StoryBlazeServer.Models;

public class FragmentoService
{
    private readonly HttpClient _httpClient;

    public FragmentoService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<FragmentoVotadoDto>> GetMasVotadosAsync()
    {
        var response = await _httpClient.GetFromJsonAsync<Response<List<FragmentoVotadoDto>>>("https://localhost:7184/api/Fragmento/MasVotados");

        if (response != null && response.IsSuccess)
        {
            return response.Data;
        }

        return new List<FragmentoVotadoDto>();
    }
}




