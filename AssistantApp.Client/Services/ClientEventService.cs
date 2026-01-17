using System.Net.Http.Json;
using AssistantApp.Shared.Models;

namespace AssistantApp.Client.Services;

public class ClientEventService
{
    private readonly HttpClient _http;

    public ClientEventService(HttpClient http)
    {
        _http = http;
    }

    public async Task<ApiResponse<List<Event>>> GetAll(EventState? state = null)
    {
        var url = "api/events";
        if (state.HasValue) url += $"?state={state}";
        
        var result = await _http.GetFromJsonAsync<ApiResponse<List<Event>>>(url);
        return result ?? ApiResponse<List<Event>>.Fail("Error de conexión");
    }

    public async Task<ApiResponse<Event>> GetById(int id)
    {
        var result = await _http.GetFromJsonAsync<ApiResponse<Event>>($"api/events/{id}");
        return result ?? ApiResponse<Event>.Fail("Error de conexión");
    }

    public async Task<ApiResponse<Event>> Create(Event evt)
    {
        var response = await _http.PostAsJsonAsync("api/events", evt);
        return await response.Content.ReadFromJsonAsync<ApiResponse<Event>>() 
               ?? ApiResponse<Event>.Fail("Error al crear");
    }

    public async Task<ApiResponse<Event>> Update(int id, Event evt)
    {
        var response = await _http.PutAsJsonAsync($"api/events/{id}", evt);
        return await response.Content.ReadFromJsonAsync<ApiResponse<Event>>() 
               ?? ApiResponse<Event>.Fail("Error al actualizar");
    }

    public async Task<ApiResponse<bool>> StartEvent(int id)
    {
        var response = await _http.PostAsync($"api/events/{id}/start", null);
        return await response.Content.ReadFromJsonAsync<ApiResponse<bool>>() 
               ?? ApiResponse<bool>.Fail("Error al iniciar");
    }

    public async Task<ApiResponse<bool>> FinishEvent(int id)
    {
        var response = await _http.PostAsync($"api/events/{id}/finish", null);
        return await response.Content.ReadFromJsonAsync<ApiResponse<bool>>() 
               ?? ApiResponse<bool>.Fail("Error al finalizar");
    }

    public async Task<ApiResponse<Invitation>> InvitePerson(int eventId, int personId)
    {
        var response = await _http.PostAsync($"api/events/{eventId}/invite/person/{personId}", null);
        return await response.Content.ReadFromJsonAsync<ApiResponse<Invitation>>() 
               ?? ApiResponse<Invitation>.Fail("Error al invitar persona");
    }

    public async Task<ApiResponse<int>> InviteGroup(int eventId, int groupId)
    {
        var response = await _http.PostAsync($"api/events/{eventId}/invite/group/{groupId}", null);
        return await response.Content.ReadFromJsonAsync<ApiResponse<int>>() 
               ?? ApiResponse<int>.Fail("Error al invitar grupo");
    }

    public async Task<ApiResponse<bool>> RemoveInvitation(int eventId, int personId)
    {
        var response = await _http.DeleteAsync($"api/events/{eventId}/invite/person/{personId}");
        return await response.Content.ReadFromJsonAsync<ApiResponse<bool>>() 
               ?? ApiResponse<bool>.Fail("Error al eliminar invitación");
    }
}