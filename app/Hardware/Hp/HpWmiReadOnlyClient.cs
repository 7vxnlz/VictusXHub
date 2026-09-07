using System.Management;
using System.Runtime.InteropServices;

namespace GHelper.Hardware.Hp;

public sealed class HpWmiReadOnlyClient
{
    private const string HpWmiScopePath = @"\\.\root\wmi";
    private const string BiosMethodClassName = "hpqBIntM";
    private const string BiosDataClassName = "hpqBDataIn";

    public HpWmiReadOnlySnapshot Probe()
    {
        var errors = new List<string>();
        var rootWmiAvailability = CheckNamespace(errors);

        var hpqBIntMAvailability = HpVictusProbeAvailability.Unknown;
        var hpqBDataInAvailability = HpVictusProbeAvailability.Unknown;
        string[] hpqBIntMMethodNames = [];
        string[] hpqBDataInMethodNames = [];

        if (rootWmiAvailability == HpVictusProbeAvailability.Available)
        {
            (hpqBIntMAvailability, hpqBIntMMethodNames) = ReadClassMethodNames(BiosMethodClassName, errors);
            (hpqBDataInAvailability, hpqBDataInMethodNames) = ReadClassMethodNames(BiosDataClassName, errors);
        }

        return new HpWmiReadOnlySnapshot(
            rootWmiAvailability,
            hpqBIntMAvailability,
            hpqBDataInAvailability,
            hpqBIntMMethodNames,
            hpqBDataInMethodNames,
            errors.ToArray());
    }

    private static HpVictusProbeAvailability CheckNamespace(List<string> errors)
    {
        try
        {
            var scope = new ManagementScope(HpWmiScopePath);
            scope.Connect();

            return scope.IsConnected
                ? HpVictusProbeAvailability.Available
                : HpVictusProbeAvailability.Unavailable;
        }
        catch (ManagementException ex) when (ex.ErrorCode == ManagementStatus.InvalidNamespace)
        {
            return HpVictusProbeAvailability.Unavailable;
        }
        catch (Exception ex) when (ex is ManagementException or UnauthorizedAccessException or COMException)
        {
            errors.Add($"{HpWmiScopePath}: {ex.GetType().Name}: {ex.Message}");
            return HpVictusProbeAvailability.Unknown;
        }
    }

    private static (HpVictusProbeAvailability Availability, string[] MethodNames) ReadClassMethodNames(
        string className,
        List<string> errors)
    {
        try
        {
            var scope = new ManagementScope(HpWmiScopePath);
            scope.Connect();

            using var managementClass = new ManagementClass(scope, new ManagementPath(className), null);
            managementClass.Get();

            var methodNames = new List<string>();
            foreach (MethodData method in managementClass.Methods)
            {
                if (!string.IsNullOrWhiteSpace(method.Name))
                {
                    methodNames.Add(method.Name.Trim());
                }
            }

            return (HpVictusProbeAvailability.Available, methodNames
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(methodName => methodName, StringComparer.OrdinalIgnoreCase)
                .ToArray());
        }
        catch (ManagementException ex) when (ex.ErrorCode == ManagementStatus.NotFound)
        {
            return (HpVictusProbeAvailability.Unavailable, []);
        }
        catch (Exception ex) when (ex is ManagementException or UnauthorizedAccessException or COMException or InvalidOperationException)
        {
            errors.Add($"{className}: {ex.GetType().Name}: {ex.Message}");
            return (HpVictusProbeAvailability.Unknown, []);
        }
    }
}
