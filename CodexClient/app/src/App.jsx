import { Navigate, Route, Routes } from "react-router-dom";
import AdminDashboard from "./pages/AdminDashboard";
import AdminEvents from "./pages/AdminEvents";
import EventConfig from "./pages/EventConfig";
import PeopleAndGroups from "./pages/PeopleAndGroups";
import GroupsPage from "./pages/GroupsPage";
import UsersAndPermissions from "./pages/UsersAndPermissions";
import Analytics from "./pages/Analytics";
import Attendance from "./pages/Attendance";
import AttendanceHome from "./pages/AttendanceHome";

export default function App() {
  return (
    <Routes>
      <Route path="/" element={<Navigate to="/admin/dashboard" replace />} />
      <Route path="/admin/dashboard" element={<AdminDashboard />} />
      <Route path="/admin/events" element={<AdminEvents />} />
      <Route path="/admin/events/:id/config" element={<EventConfig />} />
      <Route path="/admin/people" element={<PeopleAndGroups />} />
      <Route path="/admin/groups" element={<GroupsPage />} />
      <Route path="/admin/security" element={<UsersAndPermissions />} />
      <Route path="/admin/analytics" element={<Analytics />} />
      <Route path="/staff/attendance" element={<AttendanceHome />} />
      <Route path="/staff/attendance/event/:eventId" element={<Attendance />} />
      <Route path="/staff/attendance/desktop" element={<Navigate to="/staff/attendance" replace />} />
      <Route path="/staff/attendance/tablet" element={<Navigate to="/staff/attendance" replace />} />
      <Route path="/staff/attendance/variant-3" element={<Navigate to="/staff/attendance" replace />} />
      <Route path="*" element={<Navigate to="/admin/dashboard" replace />} />
    </Routes>
  );
}
