using System.Net.Http.Json;
using AssistantApp.Shared.Models;

namespace AssistantApp.Client.Services;

public class ClientGroupService
{
    private readonly HttpClient _http;

    public ClientGroupService(HttpClient http)
    {
        _http = http;
    }

    public async Task<ApiResponse<List<Group>>> GetAll(bool includeInactive = true)
    {
        var result = await _http.GetFromJsonAsync<ApiResponse<List<Group>>>($"api/groups?includeInactive={includeInactive}");
        return result ?? ApiResponse<List<Group>>.Fail("Error de conexión");
    }

    public async Task<ApiResponse<Group>> GetById(int id)
    {
        var result = await _http.GetFromJsonAsync<ApiResponse<Group>>($"api/groups/{id}");
        return result ?? ApiResponse<Group>.Fail("Error de conexión");
    }

    public async Task<ApiResponse<Group>> Create(Group group)
    {
        var response = await _http.PostAsJsonAsync("api/groups", group);
        return await response.Content.ReadFromJsonAsync<ApiResponse<Group>>() 
               ?? ApiResponse<Group>.Fail("Error al crear");
    }

    public async Task<ApiResponse<Group>> Update(int id, Group group)
    {
        var response = await _http.PutAsJsonAsync($"api/groups/{id}", group);
        return await response.Content.ReadFromJsonAsync<ApiResponse<Group>>() 
               ?? ApiResponse<Group>.Fail("Error al actualizar");
    }

    public async Task<ApiResponse<bool>> Delete(int id)
    {
        var response = await _http.DeleteAsync($"api/groups/{id}");
        return await response.Content.ReadFromJsonAsync<ApiResponse<bool>>() 
               ?? ApiResponse<bool>.Fail("Error al eliminar");
    }
}