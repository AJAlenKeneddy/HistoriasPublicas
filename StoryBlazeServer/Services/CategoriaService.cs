using StoryBlazeServer.Models;

namespace StoryBlazeServer.Services
{
    public class CategoriaService
    {

        private readonly HttpClient _httpClient;

        public CategoriaService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<Categoria>> GetCategorias()
        {
            var response = await _httpClient.GetFromJsonAsync<Response<List<Categoria>>>("https://localhost:7184/api/Categoria/ListadoCategoria");

            if (response != null && response.IsSuccess)
            {
                return response.Data;
            }

            return new List<Categoria>();
        }
    }
}
