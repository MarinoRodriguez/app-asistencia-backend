using System.Net.Http.Json;
using AssistantApp.Shared.Models;

namespace AssistantApp.Client.Services;

public class ClientAttendanceService
{
    private readonly HttpClient _http;

    public ClientAttendanceService(HttpClient http)
    {
        _http = http;
    }

    public async Task<ApiResponse<List<Assistance>>> GetByEvent(int eventId)
    {
        var result = await _http.GetFromJsonAsync<ApiResponse<List<Assistance>>>($"api/attendance/event/{eventId}");
        return result ?? ApiResponse<List<Assistance>>.Fail("Error de conexión");
    }

    public async Task<ApiResponse<Assistance>> Mark(int eventId, int personId, AssistanceType type)
    {
        var request = new { EventId = eventId, PersonId = personId, Type = type };
        var response = await _http.PostAsJsonAsync("api/attendance/mark", request);
        return await response.Content.ReadFromJsonAsync<ApiResponse<Assistance>>() 
               ?? ApiResponse<Assistance>.Fail("Error al marcar asistencia");
    }

    public async Task<ApiResponse<Assistance>> RegisterExternal(int eventId, Person person)
    {
        var response = await _http.PostAsJsonAsync($"api/attendance/external/{eventId}", person);
        return await response.Content.ReadFromJsonAsync<ApiResponse<Assistance>>() 
               ?? ApiResponse<Assistance>.Fail("Error al registrar externo");
    }

    public async Task<ApiResponse<bool>> Remove(int eventId, int personId)
    {
        var response = await _http.DeleteAsync($"api/attendance/event/{eventId}/person/{personId}");
        return await response.Content.ReadFromJsonAsync<ApiResponse<bool>>() 
               ?? ApiResponse<bool>.Fail("Error al eliminar asistencia");
    }
}