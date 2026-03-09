namespace AssistantApp.Shared;

public static class Permissions
{
    // Users & Roles
    public const string UsersView = "users.view";
    public const string UsersCreate = "users.create";
    public const string UsersManage = "users.manage";
    public const string UsersRolesManage = "users.roles.manage";
    public const string UsersResetPassword = "users.reset_password";
    public const string UsersDelete = "users.delete";

    public const string RolesView = "roles.view";
    public const string RolesManage = "roles.manage";
    public const string RolesDelete = "roles.delete";
    public const string RolePermissionsManage = "roles.permissions.manage";

    // Persons
    public const string PersonsView = "persons.view";
    public const string PersonsCreateAdmin = "persons.create.admin";
    public const string PersonsCreateAttendance = "persons.create.attendance";
    public const string PersonsEdit = "persons.edit";
    public const string PersonsDelete = "persons.delete";
    public const string PersonsGroupsManage = "persons.groups.manage";

    // Groups
    public const string GroupsView = "groups.view";
    public const string GroupsCreate = "groups.create";
    public const string GroupsEdit = "groups.edit";
    public const string GroupsDelete = "groups.delete";

    // Events
    public const string EventsView = "events.view";
    public const string EventsCreate = "events.create";
    public const string EventsEdit = "events.edit";
    public const string EventsStart = "events.start";
    public const string EventsClose = "events.close";
    public const string EventsDelete = "events.delete";

    // Attendance
    public const string AttendanceView = "attendance.view";
    public const string AttendanceTake = "attendance.take";
    public const string AttendanceAddPerson = "attendance.add_person";
    public const string AttendanceEdit = "attendance.edit";

    // Reports & Admin
    public const string ReportsView = "reports.view";
    public const string AdminDashboardView = "admin.dashboard.view";

    public static readonly string[] All =
    {
        UsersView,
        UsersCreate,
        UsersManage,
        UsersRolesManage,
        UsersResetPassword,
        UsersDelete,
        RolesView,
        RolesManage,
        RolesDelete,
        RolePermissionsManage,
        PersonsView,
        PersonsCreateAdmin,
        PersonsCreateAttendance,
        PersonsEdit,
        PersonsDelete,
        PersonsGroupsManage,
        GroupsView,
        GroupsCreate,
        GroupsEdit,
        GroupsDelete,
        EventsView,
        EventsCreate,
        EventsEdit,
        EventsStart,
        EventsClose,
        EventsDelete,
        AttendanceView,
        AttendanceTake,
        AttendanceAddPerson,
        AttendanceEdit,
        ReportsView,
        AdminDashboardView
    };
}
