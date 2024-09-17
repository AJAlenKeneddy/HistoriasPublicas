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

   
    public async Task<List<Historia>> GetHistoriasAsync()
    {
        var response = await _httpClient.GetFromJsonAsync<Response<List<Historia>>>("https://localhost:7184/api/Historias/ListarHistorias");
        return response?.IsSuccess == true ? response.Data : new List<Historia>();
    }

    
    public async Task<Historia> GetHistoriaByIdAsync(int id)
    {
        var response = await _httpClient.GetFromJsonAsync<Response<Historia>>($"https://localhost:7184/api/Historias/ObtenerHistoria/{id}");
        return response?.IsSuccess == true ? response.Data : null;
    }

    public async Task<bool> AgregarHistoriaAsync(Historia nuevaHistoria)
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

        // Crear el mensaje de solicitud
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




    public async Task<bool> ActualizarHistoriaAsync(int id, Historia historiaActualizada)
    {
        try
        {
            // Obtener el token desde localStorage
            var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");

            // Crear la solicitud HTTP PUT
            var request = new HttpRequestMessage(HttpMethod.Put, $"https://localhost:7184/api/Historias/ActualizarHistoria/{id}")
            {
                Content = JsonContent.Create(historiaActualizada)
            };

            // Agregar el token en los headers
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Enviar la solicitud
            var response = await _httpClient.SendAsync(request);

            // Leer la respuesta y procesar el resultado
            var result = await response.Content.ReadFromJsonAsync<Response<object>>();

            if (response.IsSuccessStatusCode && result?.IsSuccess == true)
            {
                return true; // Historia actualizada exitosamente
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                Console.WriteLine("No tienes permisos para editar esta historia.");
                return false; // No autorizado
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                Console.WriteLine("La historia no fue encontrada.");
                return false; // Historia no encontrada o eliminada
            }
            else
            {
                Console.WriteLine($"Error: {result?.Message}");
                return false; // Otros errores
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Excepción al actualizar la historia: {ex.Message}");
            return false; // Error general
        }
    }




    public async Task<bool> EliminarHistoriaAsync(int id)
    {
        var response = await _httpClient.DeleteAsync($"https://localhost:7184/api/Historias/EliminarHistoria/{id}");
        var result = await response.Content.ReadFromJsonAsync<Response<object>>();
        return result?.IsSuccess == true;
    }

    
    public async Task<bool> RestaurarHistoriaAsync(int id)
    {
        var response = await _httpClient.PutAsJsonAsync($"https://localhost:7184/api/Historias/RestaurarHistoria/{id}", id);
        var result = await response.Content.ReadFromJsonAsync<Response<object>>();
        return result?.IsSuccess == true;
    }

    
    public async Task<List<Historia>> GetHistoriasByUserAsync()
    {
        
        var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");

        if (string.IsNullOrEmpty(token))
        {
            throw new UnauthorizedAccessException("No se encontró el token de autenticación.");
        }

        
        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        
        var response = await _httpClient.GetFromJsonAsync<Response<List<Historia>>>("https://localhost:7184/api/Historias/usuario/historias");

        
        Console.WriteLine("Response Status: " + response?.IsSuccess);
        Console.WriteLine("Response Data: " + response?.Data?.Count);

        
        return response?.IsSuccess == true ? response.Data : new List<Historia>();
    }
    public async Task<Historia?> ObtenerHistoriaCompletaAsync(int historiaId)
    {
        
        var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");

        
        var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"https://localhost:7184/api/Historias/ObtenerHistoriaCompleta/{historiaId}");

        
        if (!string.IsNullOrEmpty(token))
        {
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        
        var response = await _httpClient.SendAsync(requestMessage);
        if (response.IsSuccessStatusCode)
        {
            
            var historiaCompleta = await response.Content.ReadFromJsonAsync<Historia>();
            return historiaCompleta;
        }

        return null;
    }
    public async Task<List<Historia>> GetHistoriasPorCategoriaAsync(int categoriaId)
    {
        try
        {
            
            if (categoriaId <= 0)
            {
                throw new ArgumentException("El ID de la categoría es inválido.");
            }

            
            var response = await _httpClient.GetFromJsonAsync<Response<List<Historia>>>($"https://localhost:7184/api/Historias/historiascategoria/{categoriaId}");

            
            if (response != null && response.IsSuccess)
            {
                return response.Data;
            }

            
            return new List<Historia>();
        }
        catch (HttpRequestException httpEx)
        {
            
            Console.WriteLine($"Error en la solicitud HTTP: {httpEx.Message}");
            return new List<Historia>(); 
        }
        catch (TimeoutException timeoutEx)
        {
            
            Console.WriteLine($"La solicitud tardó demasiado: {timeoutEx.Message}");
            return new List<Historia>();
        }
        catch (ArgumentException argEx)
        {
           
            Console.WriteLine($"Error en los parámetros: {argEx.Message}");
            return new List<Historia>();
        }
        catch (Exception ex)
        {
            
            Console.WriteLine($"Ocurrió un error inesperado: {ex.Message}");
            return new List<Historia>();
        }
    }


}
