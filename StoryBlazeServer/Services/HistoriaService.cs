using Microsoft.JSInterop;
using System.Net.Http.Json;
using StoryBlazeServer.Models;
using static Microsoft.AspNetCore.Razor.Language.TagHelperMetadata;
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

    public async Task<bool> AgregarHistoriaAsync(Historia nuevaHistoria)
    {
        // Obtener el token desde localStorage
        var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");

        if (!string.IsNullOrEmpty(token))
        {
            // Obtener el ID del usuario desde el token
            var userId = _jwtService.GetUserIdFromToken(token);

            if (int.TryParse(userId, out int userIdInt))
            {
                nuevaHistoria.UsuarioCreadorId = userIdInt; // Asignar el ID del usuario a la historia
                nuevaHistoria.FechaCreacion = DateTime.Now; // Establecer la fecha de creación
            }
        }

        // Crear el mensaje de solicitud
        var requestMessage = new HttpRequestMessage(HttpMethod.Post, "https://localhost:7184/api/Historias/AgregarHistoria")
        {
            Content = JsonContent.Create(nuevaHistoria) // Enviar el contenido JSON
        };

        // Agregar el encabezado de autorización
        if (!string.IsNullOrEmpty(token))
        {
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        // Enviar la solicitud
        var response = await _httpClient.SendAsync(requestMessage);
        var result = await response.Content.ReadFromJsonAsync<Response<object>>();
        return result?.IsSuccess == true;
    }



    // Actualizar una historia existente
    public async Task<bool> ActualizarHistoriaAsync(int id, Historia historiaActualizada)
    {
        // Recuperar el token desde localStorage usando JS Interop
        var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");

        // Crear un HttpRequestMessage para agregar el token en el encabezado de autorización
        var request = new HttpRequestMessage(HttpMethod.Put, $"https://localhost:7184/api/Historias/ActualizarHistoria/{id}")
        {
            Content = JsonContent.Create(historiaActualizada)
        };

        // Agregar el token de autorización en el encabezado
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Enviar la solicitud
        var response = await _httpClient.SendAsync(request);

        // Leer la respuesta y verificar si fue exitosa
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
    public async Task<Historia?> ObtenerHistoriaCompletaAsync(int historiaId)
    {
        // Obtener el token desde localStorage
        var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");

        // Crear la solicitud HTTP
        var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"https://localhost:7184/api/Historias/ObtenerHistoriaCompleta/{historiaId}");

        // Agregar el encabezado de autorización
        if (!string.IsNullOrEmpty(token))
        {
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        // Enviar la solicitud
        var response = await _httpClient.SendAsync(requestMessage);
        if (response.IsSuccessStatusCode)
        {
            // Deserializar la respuesta
            var historiaCompleta = await response.Content.ReadFromJsonAsync<Historia>();
            return historiaCompleta;
        }

        return null;
    }

}
