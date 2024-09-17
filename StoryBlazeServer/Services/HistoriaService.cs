using Microsoft.JSInterop;
using System.Net.Http.Json;
using StoryBlazeServer.Models;
using System.Net.Http.Headers;
using StoryBlazeServer.Services;

public class HistoriaService
{
    private readonly HttpClient _httpClient;
    private readonly IJSRuntime _jsRuntime;
    private readonly IJwtService _jwtService;

    public HistoriaService(HttpClient httpClient, IJSRuntime jsRuntime, IJwtService jwtService)
    {
        _httpClient = httpClient;
        _jsRuntime = jsRuntime;
        _jwtService = jwtService;
    }

    public async Task<List<Historia>> GetHistoriasAsync()
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<Response<List<Historia>>>("https://localhost:7184/api/Historias/ListarHistorias");
            return response?.IsSuccess == true ? response.Data : new List<Historia>();
        }
        catch (HttpRequestException ex)
        {
            Console.Error.WriteLine($"Error al obtener historias: {ex.Message}");
            return new List<Historia>();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error inesperado al obtener historias: {ex.Message}");
            return new List<Historia>();
        }
    }

    public async Task<Historia?> GetHistoriaByIdAsync(int id)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<Response<Historia>>($"https://localhost:7184/api/Historias/ObtenerHistoria/{id}");
            return response?.IsSuccess == true ? response.Data : null;
        }
        catch (HttpRequestException ex)
        {
            Console.Error.WriteLine($"Error al obtener la historia con ID {id}: {ex.Message}");
            return null;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error inesperado al obtener la historia con ID {id}: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> AgregarHistoriaAsync(Historia nuevaHistoria)
    {
        try
        {
            var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");

            if (!string.IsNullOrEmpty(token))
            {
                var userId = _jwtService.GetUserIdFromToken(token);
                if (int.TryParse(userId, out int userIdInt))
                {
                    nuevaHistoria.UsuarioCreadorId = userIdInt;
                    nuevaHistoria.FechaCreacion = DateTime.Now;
                }
            }

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, "https://localhost:7184/api/Historias/AgregarHistoria")
            {
                Content = JsonContent.Create(nuevaHistoria)
            };

            if (!string.IsNullOrEmpty(token))
            {
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var response = await _httpClient.SendAsync(requestMessage);
            var result = await response.Content.ReadFromJsonAsync<Response<object>>();
            return result?.IsSuccess == true;
        }
        catch (HttpRequestException ex)
        {
            Console.Error.WriteLine($"Error al agregar la historia: {ex.Message}");
            return false;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error inesperado al agregar la historia: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> ActualizarHistoriaAsync(int id, Historia historiaActualizada)
    {
        try
        {
            var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");
            var request = new HttpRequestMessage(HttpMethod.Put, $"https://localhost:7184/api/Historias/ActualizarHistoria/{id}")
            {
                Content = JsonContent.Create(historiaActualizada)
            };

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient.SendAsync(request);
            var result = await response.Content.ReadFromJsonAsync<Response<object>>();
            return result?.IsSuccess == true;
        }
        catch (HttpRequestException ex)
        {
            Console.Error.WriteLine($"Error al actualizar la historia con ID {id}: {ex.Message}");
            return false;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error inesperado al actualizar la historia con ID {id}: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> EliminarHistoriaAsync(int id)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"https://localhost:7184/api/Historias/EliminarHistoria/{id}");
            var result = await response.Content.ReadFromJsonAsync<Response<object>>();
            return result?.IsSuccess == true;
        }
        catch (HttpRequestException ex)
        {
            Console.Error.WriteLine($"Error al eliminar la historia con ID {id}: {ex.Message}");
            return false;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error inesperado al eliminar la historia con ID {id}: {ex.Message}");
            return false;
        }
    }

    public async Task<List<Historia>> GetHistoriasByUserAsync()
    {
        try
        {
            var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");

            if (string.IsNullOrEmpty(token))
            {
                throw new UnauthorizedAccessException("No se encontró el token de autenticación.");
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient.GetFromJsonAsync<Response<List<Historia>>>("https://localhost:7184/api/Historias/usuario/historias");
            return response?.IsSuccess == true ? response.Data : new List<Historia>();
        }
        catch (HttpRequestException ex)
        {
            Console.Error.WriteLine($"Error al obtener historias del usuario: {ex.Message}");
            return new List<Historia>();
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.Error.WriteLine($"Error de autenticación: {ex.Message}");
            return new List<Historia>();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error inesperado al obtener historias del usuario: {ex.Message}");
            return new List<Historia>();
        }
    }

    public async Task<Historia?> ObtenerHistoriaCompletaAsync(int historiaId)
    {
        try
        {
            var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"https://localhost:7184/api/Historias/ObtenerHistoriaCompleta/{historiaId}");

            if (!string.IsNullOrEmpty(token))
            {
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var response = await _httpClient.SendAsync(requestMessage);
            return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<Historia>() : null;
        }
        catch (HttpRequestException ex)
        {
            Console.Error.WriteLine($"Error al obtener la historia completa con ID {historiaId}: {ex.Message}");
            return null;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error inesperado al obtener la historia completa con ID {historiaId}: {ex.Message}");
            return null;
        }
    }

    public async Task<List<Historia>> GetHistoriasPorCategoriaAsync(int categoriaId)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<Response<List<Historia>>>($"https://localhost:7184/api/Historias/historiascategoria/{categoriaId}");
            return response?.IsSuccess == true ? response.Data : new List<Historia>();
        }
        catch (HttpRequestException ex)
        {
            Console.Error.WriteLine($"Error al obtener historias por categoría: {ex.Message}");
            return new List<Historia>();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error inesperado al obtener historias por categoría: {ex.Message}");
            return new List<Historia>();
        }
    }
}
