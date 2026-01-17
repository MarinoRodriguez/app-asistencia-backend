using System.Net.Http.Json;
using AssistantApp.Shared.Models;

namespace AssistantApp.Client.Services;

public class ClientPersonService
{
    private readonly HttpClient _http;

    public ClientPersonService(HttpClient http)
    {
        _http = http;
    }

    public async Task<ApiResponse<List<Person>>> GetAll()
    {
        var result = await _http.GetFromJsonAsync<ApiResponse<List<Person>>>("api/persons");
        return result ?? ApiResponse<List<Person>>.Fail("Error de conexión");
    }

    public async Task<ApiResponse<List<Person>>> Search(string term)
    {
        var result = await _http.GetFromJsonAsync<ApiResponse<List<Person>>>($"api/persons/search?term={term}");
        return result ?? ApiResponse<List<Person>>.Fail("Error de conexión");
    }

    public async Task<ApiResponse<Person>> GetById(int id)
    {
        var result = await _http.GetFromJsonAsync<ApiResponse<Person>>($"api/persons/{id}");
        return result ?? ApiResponse<Person>.Fail("Error de conexión");
    }

    public async Task<ApiResponse<Person>> Create(Person person)
    {
        var response = await _http.PostAsJsonAsync("api/persons", person);
        return await response.Content.ReadFromJsonAsync<ApiResponse<Person>>() 
               ?? ApiResponse<Person>.Fail("Error al crear");
    }

    public async Task<ApiResponse<Person>> Update(int id, Person person)
    {
        var response = await _http.PutAsJsonAsync($"api/persons/{id}", person);
        
        // PutAsJsonAsync no devuelve el objeto automáticamente si es 204 No Content, 
        // pero nuestra API devuelve Ok(ApiResponse) o NoContent?
        // Revisando PersonsController: devuelve Ok(response) que es JSON.
        
        return await response.Content.ReadFromJsonAsync<ApiResponse<Person>>() 
               ?? ApiResponse<Person>.Fail("Error al actualizar");
    }
}