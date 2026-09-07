using System.Management;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace GHelper.Hardware.Hp;

public static class HpWmiAccessDeniedDiagnostics
{
    private const string HpWmiScopePath = @"\\.\root\wmi";
    private const string CimV2ScopePath = @"\\.\root\cimv2";
    private const string BiosMethodClassName = "hpqBIntM";

    public static HpWmiAccessDeniedDiagnosticsSnapshot Probe()
    {
        var errors = new List<string>();

        bool processElevated = IsProcessElevated(errors);
        string windowsIdentitySummary = GetWindowsIdentitySummary(errors);
        bool namespaceReadable = CanOpenNamespace(errors);
        bool classReadable = CanReadHpBIntMClass(errors);
        bool methodMetadataReadable = CanReadHpBIntMMethodMetadata(errors);
        HpRelatedServiceSnapshot[] hpRelatedServices = QueryHpRelatedServices(errors);

        return new HpWmiAccessDeniedDiagnosticsSnapshot(
            processElevated,
            windowsIdentitySummary,
            namespaceReadable,
            classReadable,
            methodMetadataReadable,
            hpRelatedServices,
            errors.ToArray());
    }

    private static bool IsProcessElevated(List<string> errors)
    {
        try
        {
            using WindowsIdentity identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or InvalidOperationException)
        {
            errors.Add("Process elevation check: " + FormatException(ex));
            return false;
        }
    }

    private static string GetWindowsIdentitySummary(List<string> errors)
    {
        try
        {
            using WindowsIdentity identity = WindowsIdentity.GetCurrent();

            if (identity.IsSystem)
            {
                return "System";
            }

            if (identity.IsAnonymous)
            {
                return "Anonymous";
            }

            return identity.IsAuthenticated ? "AuthenticatedUser" : "UnauthenticatedUser";
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or InvalidOperationException)
        {
            errors.Add("Windows identity summary: " + FormatException(ex));
            return "Unknown";
        }
    }

    private static bool CanOpenNamespace(List<string> errors)
    {
        try
        {
            var scope = new ManagementScope(HpWmiScopePath);
            scope.Connect();
            return scope.IsConnected;
        }
        catch (Exception ex) when (IsReadOnlyWmiException(ex))
        {
            errors.Add("root\\wmi namespace readability: " + FormatException(ex));
            return false;
        }
    }

    private static bool CanReadHpBIntMClass(List<string> errors)
    {
        try
        {
            var scope = new ManagementScope(HpWmiScopePath);
            scope.Connect();

            using var managementClass = new ManagementClass(scope, new ManagementPath(BiosMethodClassName), null);
            managementClass.Get();
            return true;
        }
        catch (Exception ex) when (IsReadOnlyWmiException(ex))
        {
            errors.Add("hpqBIntM class readability: " + FormatException(ex));
            return false;
        }
    }

    private static bool CanReadHpBIntMMethodMetadata(List<string> errors)
    {
        try
        {
            var scope = new ManagementScope(HpWmiScopePath);
            scope.Connect();

            using var managementClass = new ManagementClass(scope, new ManagementPath(BiosMethodClassName), null);
            managementClass.Get();

            _ = managementClass.Methods
                .Cast<MethodData>()
                .Select(method => method.Name)
                .ToArray();

            return true;
        }
        catch (Exception ex) when (IsReadOnlyWmiException(ex))
        {
            errors.Add("hpqBIntM method metadata readability: " + FormatException(ex));
            return false;
        }
    }

    private static HpRelatedServiceSnapshot[] QueryHpRelatedServices(List<string> errors)
    {
        var services = new List<HpRelatedServiceSnapshot>();

        try
        {
            const string query = "SELECT Name, State, StartMode FROM Win32_Service";
            using var searcher = new ManagementObjectSearcher(new ManagementScope(CimV2ScopePath), new ObjectQuery(query));
            using ManagementObjectCollection results = searcher.Get();

            foreach (ManagementObject result in results)
            {
                using (result)
                {
                    string name = result["Name"]?.ToString()?.Trim() ?? string.Empty;
                    if (!IsClearlyHpRelatedServiceName(name))
                    {
                        continue;
                    }

                    services.Add(new HpRelatedServiceSnapshot(
                        name,
                        result["State"]?.ToString()?.Trim() ?? string.Empty,
                        result["StartMode"]?.ToString()?.Trim() ?? string.Empty));
                }
            }
        }
        catch (Exception ex) when (IsReadOnlyWmiException(ex))
        {
            errors.Add("HP-related service inventory: " + FormatException(ex));
        }

        return services
            .OrderBy(service => service.Name, StringComparer.OrdinalIgnoreCase)
            .Take(50)
            .ToArray();
    }

    private static bool IsClearlyHpRelatedServiceName(string serviceName) =>
        !string.IsNullOrWhiteSpace(serviceName) &&
        (serviceName.Contains("HP", StringComparison.OrdinalIgnoreCase) ||
         serviceName.Contains("Hewlett", StringComparison.OrdinalIgnoreCase) ||
         serviceName.Contains("Omen", StringComparison.OrdinalIgnoreCase));

    private static bool IsReadOnlyWmiException(Exception ex) =>
        ex is ManagementException or UnauthorizedAccessException or COMException or InvalidOperationException;

    private static string FormatException(Exception ex) => $"{ex.GetType().Name}: {ex.Message}";
}
