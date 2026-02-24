import AdminLayout from "../components/layout/AdminLayout";
import { permissions, roles, users } from "../data/securityMock";
import Badge from "../components/ui/Badge";

export default function UsersAndPermissions() {
  return (
    <AdminLayout>
      <div className="flex-1 overflow-auto">
        <div className="p-8 space-y-8 max-w-6xl mx-auto">
          <div className="flex items-center justify-between gap-4">
            <div>
              <h1 className="text-3xl font-black tracking-tight text-[#111318]">Administración de Usuarios y Seguridad</h1>
              <p className="text-slate-500 text-sm mt-2">
                Vista inspirada en el prototipo. Datos mock hasta contar con backend de seguridad.
              </p>
            </div>
            <button className="px-4 py-2 text-sm font-semibold text-white bg-primary rounded-lg">Invitar Usuario</button>
          </div>

          <section className="bg-white rounded-2xl border border-slate-200 shadow-sm p-6">
            <h2 className="text-lg font-bold text-[#111318] mb-4">Usuarios</h2>
            <div className="space-y-4">
              {users.map((user) => (
                <div key={user.id} className="flex items-center justify-between border border-slate-100 rounded-xl p-4">
                  <div>
                    <p className="font-semibold text-slate-900">{user.name}</p>
                    <p className="text-xs text-slate-500">{user.email}</p>
                  </div>
                  <div className="flex items-center gap-3">
                    <Badge
                      text={user.status}
                      className={user.status === "Activo" ? "bg-emerald-100 text-emerald-700" : "bg-amber-100 text-amber-700"}
                    />
                    <span className="text-sm font-semibold text-slate-700">{user.role}</span>
                  </div>
                </div>
              ))}
            </div>
          </section>

          <section className="bg-white rounded-2xl border border-slate-200 shadow-sm p-6">
            <h2 className="text-lg font-bold text-[#111318] mb-4">Roles y Permisos</h2>
            <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
              {roles.map((role) => (
                <div key={role.id} className="border border-slate-100 rounded-xl p-4">
                  <h3 className="font-semibold text-slate-900">{role.name}</h3>
                  <p className="text-xs text-slate-500 mt-1">{role.description}</p>
                  <p className="text-xs text-slate-400 mt-3">{role.users} usuarios</p>
                </div>
              ))}
            </div>
            <div className="mt-6">
              <h3 className="text-sm font-semibold text-slate-700 mb-2">Permisos clave</h3>
              <div className="flex flex-wrap gap-2">
                {permissions.map((permission) => (
                  <span
                    key={permission}
                    className="px-2.5 py-1 text-xs font-semibold bg-slate-100 text-slate-600 rounded-full"
                  >
                    {permission}
                  </span>
                ))}
              </div>
            </div>
          </section>
        </div>
      </div>
    </AdminLayout>
  );
}
