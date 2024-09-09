using Microsoft.JSInterop;
using System.Net.Http.Json;
using StoryBlazeServer.Models;

public class HistoriaService
{
    private readonly HttpClient _httpClient;
    private readonly IJSRuntime _jsRuntime;

    public HistoriaService(HttpClient httpClient, IJSRuntime jsRuntime)
    {
        _httpClient = httpClient;
        _jsRuntime = jsRuntime;
    }

    // Obtener todas las historias
    public async Task<List<Historia>> GetHistoriasAsync()
    {
        var response = await _httpClient.GetFromJsonAsync<Response<List<Historia>>>("https://localhost:7184/api/Historias/ListarHistorias");
        return response?.IsSuccess == true ? response.Data : new List<Historia>();
    }

    // Obtener una historia por su ID
    public async Task<Historia> GetHistoriaByIdAsync(int id)
    {
        var response = await _httpClient.GetFromJsonAsync<Response<Historia>>($"https://localhost:7184/api/Historias/ObtenerHistoria/{id}");
        return response?.IsSuccess == true ? response.Data : null;
    }

    // Agregar una nueva historia
    public async Task<bool> AgregarHistoriaAsync(Historia nuevaHistoria)
    {
        var response = await _httpClient.PostAsJsonAsync("https://localhost:7184/api/Historias/AgregarHistoria", nuevaHistoria);
        var result = await response.Content.ReadFromJsonAsync<Response<object>>();
        return result?.IsSuccess == true;
    }

    // Actualizar una historia existente
    public async Task<bool> ActualizarHistoriaAsync(int id, Historia historiaActualizada)
    {
        var response = await _httpClient.PutAsJsonAsync($"https://localhost:7184/api/Historias/ActualizarHistoria/{id}", historiaActualizada);
        var result = await response.Content.ReadFromJsonAsync<Response<object>>();
        return result?.IsSuccess == true;
    }

    // Eliminar (lógicamente) una historia
    public async Task<bool> EliminarHistoriaAsync(int id)
    {
        var response = await _httpClient.DeleteAsync($"https://localhost:7184/api/Historias/EliminarHistoria/{id}");
        var result = await response.Content.ReadFromJsonAsync<Response<object>>();
        return result?.IsSuccess == true;
    }

    // Restaurar una historia eliminada
    public async Task<bool> RestaurarHistoriaAsync(int id)
    {
        var response = await _httpClient.PutAsJsonAsync($"https://localhost:7184/api/Historias/RestaurarHistoria/{id}", id);
        var result = await response.Content.ReadFromJsonAsync<Response<object>>();
        return result?.IsSuccess == true;
    }

    // Obtener historias del usuario autenticado
    public async Task<List<Historia>> GetHistoriasByUserAsync()
    {
        // Obtener el token desde localStorage
        var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");

        if (string.IsNullOrEmpty(token))
        {
            throw new UnauthorizedAccessException("No se encontró el token de autenticación.");
        }

        // Agregar el token a los encabezados de la solicitud
        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Hacer la solicitud al endpoint protegido
        var response = await _httpClient.GetFromJsonAsync<Response<List<Historia>>>("https://localhost:7184/api/Historias/usuario/historias");

        // Verificar la respuesta
        Console.WriteLine("Response Status: " + response?.IsSuccess);
        Console.WriteLine("Response Data: " + response?.Data?.Count);

        // Si la respuesta es exitosa, devolver los datos
        return response?.IsSuccess == true ? response.Data : new List<Historia>();
    }

}
