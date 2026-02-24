import { useEffect, useMemo, useState } from "react";
import { useParams } from "react-router-dom";
import StaffLayout from "../components/layout/StaffLayout";
import ExternalModal from "../components/ui/ExternalModal";
import { attendanceApi } from "../api/services";
import { ASSISTANCE_TYPE, ASSISTANCE_BADGE, ASSISTANCE_LABEL, EVENT_STATE } from "../lib/constants";
import { loadAttendanceForEvent, loadEventById } from "../lib/attendance";
import { formatDateTime } from "../lib/format";

export default function Attendance() {
  const { eventId } = useParams();
  const [event, setEvent] = useState(null);
  const [attendance, setAttendance] = useState([]);
  const [search, setSearch] = useState("");
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [externalOpen, setExternalOpen] = useState(false);

  const loadData = async () => {
    try {
      setLoading(true);
      const currentEvent = await loadEventById(eventId);
      setEvent(currentEvent);
      if (currentEvent) {
        const list = await loadAttendanceForEvent(currentEvent.id);
        setAttendance(list);
      }
    } catch (err) {
      setError(err.message || "Error al cargar asistencia");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadData();
  }, [eventId]);

  const filtered = useMemo(() => {
    return attendance.filter((item) => {
      const fullName = `${item.person?.name || ""} ${item.person?.lastName || ""}`.toLowerCase();
      return fullName.includes(search.toLowerCase());
    });
  }, [attendance, search]);

  const handleMark = async (personId, type) => {
    if (!event || event.state !== EVENT_STATE.InProgress) return;
    await attendanceApi.mark(event.id, personId, type);
    await loadData();
  };

  const handleExternal = async (form) => {
    if (!event || event.state !== EVENT_STATE.InProgress) return;
    await attendanceApi.registerExternal(event.id, form);
    setExternalOpen(false);
    await loadData();
  };

  const isLocked = event && event.state !== EVENT_STATE.InProgress;

  return (
    <StaffLayout>
      <div className="min-h-screen bg-background-light text-slate-900">
        <header className="bg-white border-b border-slate-200 px-6 py-5">
          <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
            <div>
              <h1 className="text-2xl md:text-3xl font-bold tracking-tight">{event?.title || "Evento"}</h1>
              <p className="text-xs md:text-sm text-slate-500">
                {event?.scheduledStartDate
                  ? `Asistencia • ${formatDateTime(event.scheduledStartDate)}`
                  : "Control de asistencia en tiempo real"}
              </p>
            </div>
            <button
              className={`px-4 py-2 text-sm font-semibold text-white rounded-lg ${
                isLocked ? "bg-slate-300 cursor-not-allowed" : "bg-primary"
              }`}
              onClick={() => setExternalOpen(true)}
              disabled={isLocked}
            >
              Nuevo externo
            </button>
          </div>
          {isLocked && (
            <div className="mt-3 rounded-lg bg-amber-50 border border-amber-200 px-4 py-2 text-xs text-amber-700">
              Este evento no está en curso. Solo puedes registrar asistencia cuando el evento esté en estado "En curso".
            </div>
          )}
          <div className="mt-4 relative">
            <span className="material-symbols-outlined absolute left-3 top-1/2 -translate-y-1/2 text-slate-400">search</span>
            <input
              className="w-full pl-10 pr-4 py-2.5 bg-white border border-slate-200 rounded-lg focus:ring-2 focus:ring-primary focus:border-transparent outline-none text-sm"
              placeholder="Buscar persona..."
              value={search}
              onChange={(event) => setSearch(event.target.value)}
            />
          </div>
        </header>

        <div className="p-6">
          {loading && <p className="text-slate-500">Cargando asistencia...</p>}
          {error && <p className="text-rose-600">{error}</p>}

          {!loading && !error && (
            <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-4">
              {filtered.map((item) => (
                <div key={item.id} className="bg-white border border-slate-200 rounded-2xl p-4 flex flex-col gap-3">
                  <div className="flex items-center gap-3">
                    <div className="w-12 h-12 rounded-full bg-slate-200 overflow-hidden">
                      {item.person?.photoUrl ? (
                        <img src={item.person.photoUrl} alt={item.person?.name} className="w-full h-full object-cover" />
                      ) : (
                        <span className="material-symbols-outlined text-slate-400">person</span>
                      )}
                    </div>
                    <div className="flex-1 min-w-0">
                      <h3 className="font-bold text-lg truncate text-slate-900">
                        {item.person?.name} {item.person?.lastName}
                      </h3>
                      <p className="text-xs text-slate-500 truncate">{item.person?.email || "Sin email"}</p>
                    </div>
                    <span className={`text-[10px] font-semibold px-2 py-0.5 rounded-full ${ASSISTANCE_BADGE[item.status]}`}>
                      {ASSISTANCE_LABEL[item.status]}
                    </span>
                  </div>
                  <div className="grid grid-cols-3 gap-2">
                    <button
                      className={`py-2 rounded-xl text-xs font-semibold ${
                        isLocked ? "bg-slate-200 text-slate-400 cursor-not-allowed" : "bg-emerald-100 text-emerald-700"
                      }`}
                      onClick={() => handleMark(item.personId, ASSISTANCE_TYPE.Present)}
                      disabled={isLocked}
                    >
                      Presente
                    </button>
                    <button
                      className={`py-2 rounded-xl text-xs font-semibold ${
                        isLocked ? "bg-slate-200 text-slate-400 cursor-not-allowed" : "bg-amber-100 text-amber-700"
                      }`}
                      onClick={() => handleMark(item.personId, ASSISTANCE_TYPE.Late)}
                      disabled={isLocked}
                    >
                      Tarde
                    </button>
                    <button
                      className={`py-2 rounded-xl text-xs font-semibold ${
                        isLocked ? "bg-slate-200 text-slate-400 cursor-not-allowed" : "bg-rose-100 text-rose-700"
                      }`}
                      onClick={() => handleMark(item.personId, ASSISTANCE_TYPE.Absent)}
                      disabled={isLocked}
                    >
                      Ausente
                    </button>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>
      </div>

      <ExternalModal open={externalOpen} onClose={() => setExternalOpen(false)} onSave={handleExternal} />
    </StaffLayout>
  );
}
