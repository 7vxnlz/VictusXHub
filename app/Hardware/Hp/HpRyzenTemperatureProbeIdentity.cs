using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;

namespace GHelper.Hardware.Hp;

// This is deliberately a read-only, RSMB-only identity reader. It does not expose a
// caller-selected firmware-table provider, table identifier, or firmware operation.
internal enum HpRyzenTemperatureProbeIdentitySource
{
    Unavailable,
    Wmi,
    NativeSmbios,
    RegistryBios
}

internal sealed record HpRyzenTemperatureProbeIdentity(
    HpRyzenTemperatureProbeIdentitySource Source,
    string Manufacturer,
    string Model,
    string Sku,
    string Bios,
    string Detail)
{
    internal string NormalizedManufacturer => NormalizeWords(Manufacturer);
    internal string NormalizedModel => NormalizeWords(Model);
    internal string NormalizedSku => NormalizeSku(Sku);
    internal string NormalizedBios => NormalizeWords(Bios);

    internal bool HasRawIdentity => !string.IsNullOrWhiteSpace(Manufacturer) || !string.IsNullOrWhiteSpace(Model) ||
                                    !string.IsNullOrWhiteSpace(Sku) || !string.IsNullOrWhiteSpace(Bios);

    internal bool HasRequiredIdentityFields => !string.IsNullOrWhiteSpace(Manufacturer) && !string.IsNullOrWhiteSpace(Sku);

    internal static HpRyzenTemperatureProbeIdentity Unavailable(string detail) =>
        new(HpRyzenTemperatureProbeIdentitySource.Unavailable, string.Empty, string.Empty, string.Empty, string.Empty, detail);

    internal static bool IsExactTarget(HpRyzenTemperatureProbeIdentity identity)
    {
        ArgumentNullException.ThrowIfNull(identity);

        bool hpManufacturer = identity.NormalizedManufacturer is "HP" or "HEWLETT PACKARD" or "HP INC";
        bool exactSku = string.Equals(identity.NormalizedSku, "7Z5Z2EA", StringComparison.Ordinal);
        bool biosMatchesOrUnavailable = string.IsNullOrEmpty(identity.NormalizedBios) ||
                                       identity.NormalizedBios.Contains("F 31", StringComparison.Ordinal);

        // HP + this exact SKU is the two-field product proof. Model naming may be
        // marketing-generic in SMBIOS, so it is reported but is not fuzzy-matched.
        return hpManufacturer && exactSku && biosMatchesOrUnavailable;
    }

    private static string NormalizeWords(string value)
    {
        var builder = new StringBuilder(value.Length);
        bool pendingSpace = false;
        foreach (char character in value.Trim())
        {
            if (char.IsLetterOrDigit(character))
            {
                if (pendingSpace && builder.Length > 0) builder.Append(' ');
                builder.Append(char.ToUpperInvariant(character));
                pendingSpace = false;
            }
            else
            {
                pendingSpace = true;
            }
        }

        return builder.ToString();
    }

    private static string NormalizeSku(string value)
    {
        string trimmed = value.Trim().ToUpperInvariant();
        int hashIndex = trimmed.IndexOf('#');
        return (hashIndex >= 0 ? trimmed[..hashIndex] : trimmed).Replace(" ", string.Empty, StringComparison.Ordinal);
    }
}

internal static class HpRyzenTemperatureProbeIdentitySelection
{
    internal static HpRyzenTemperatureProbeIdentity Select(HpRyzenTemperatureProbeIdentity wmi, HpRyzenTemperatureProbeIdentity nativeSmbios,
        HpRyzenTemperatureProbeIdentity registryBios)
    {
        ArgumentNullException.ThrowIfNull(wmi);
        ArgumentNullException.ThrowIfNull(nativeSmbios);
        ArgumentNullException.ThrowIfNull(registryBios);

        if (nativeSmbios.HasRequiredIdentityFields)
        {
            return nativeSmbios with
            {
                Detail = string.IsNullOrEmpty(wmi.Detail) ? nativeSmbios.Detail : nativeSmbios.Detail + " WMI: " + wmi.Detail
            };
        }

        if (wmi.HasRequiredIdentityFields)
        {
            return wmi;
        }

        if (registryBios.HasRequiredIdentityFields)
        {
            return registryBios with
            {
                Detail = registryBios.Detail + " WMI: " + wmi.Detail + " Native SMBIOS: " + nativeSmbios.Detail
            };
        }

        return HpRyzenTemperatureProbeIdentity.Unavailable(
            "WMI: " + wmi.Detail + " Native SMBIOS: " + nativeSmbios.Detail + " Registry BIOS: " + registryBios.Detail);
    }
}

internal static class HpRyzenTemperatureProbeRegistryIdentity
{
    private const string BiosKeyPath = @"HARDWARE\DESCRIPTION\System\BIOS";

    [SupportedOSPlatform("windows")]
    internal static HpRyzenTemperatureProbeIdentity Read()
    {
        try
        {
            using RegistryKey? key = Registry.LocalMachine.OpenSubKey(BiosKeyPath, writable: false);
            if (key is null)
            {
                return HpRyzenTemperatureProbeIdentity.Unavailable("Registry BIOS identity key was not found.");
            }

            string manufacturer = ReadString(key, "SystemManufacturer");
            string model = ReadString(key, "SystemProductName");
            string sku = ReadString(key, "SystemSKU");
            string bios = ReadString(key, "BIOSVersion");
            if (string.IsNullOrWhiteSpace(manufacturer) || string.IsNullOrWhiteSpace(sku))
            {
                return HpRyzenTemperatureProbeIdentity.Unavailable("Registry BIOS identity did not contain manufacturer and SKU.");
            }

            return new(HpRyzenTemperatureProbeIdentitySource.RegistryBios, manufacturer, model, sku, bios,
                "HKLM\\HARDWARE\\DESCRIPTION\\System\\BIOS read-only identity fallback.");
        }
        catch (UnauthorizedAccessException ex)
        {
            return HpRyzenTemperatureProbeIdentity.Unavailable("Registry BIOS identity access failed: " + ex.Message);
        }
    }

    [SupportedOSPlatform("windows")]
    private static string ReadString(RegistryKey key, string valueName)
    {
        object? value = key.GetValue(valueName);
        return value switch
        {
            string text => text.Trim(),
            string[] values => string.Join(" ", values).Trim(),
            _ => string.Empty
        };
    }
}

internal static class HpRyzenTemperatureProbeSmbios
{
    private const uint RawSmbiosProvider = 0x424D5352; // 'RSMB' in little-endian DWORD form.
    private const int RawSmbiosHeaderLength = 8;
    private const int MaximumRawSmbiosBytes = 1024 * 1024;

    internal static HpRyzenTemperatureProbeIdentity Read()
    {
        try
        {
            uint required = GetSystemFirmwareTable(RawSmbiosProvider, 0, null, 0);
            if (required < RawSmbiosHeaderLength || required > MaximumRawSmbiosBytes)
            {
                return HpRyzenTemperatureProbeIdentity.Unavailable("RSMB size query failed or returned an unsafe size (Win32 " + Marshal.GetLastPInvokeError() + ").");
            }

            byte[] raw = new byte[(int)required];
            uint written = GetSystemFirmwareTable(RawSmbiosProvider, 0, raw, (uint)raw.Length);
            if (written != raw.Length)
            {
                return HpRyzenTemperatureProbeIdentity.Unavailable("RSMB read was incomplete (Win32 " + Marshal.GetLastPInvokeError() + ").");
            }

            return Parse(raw);
        }
        catch (Exception ex) when (ex is DllNotFoundException or EntryPointNotFoundException or UnauthorizedAccessException)
        {
            return HpRyzenTemperatureProbeIdentity.Unavailable("RSMB access failed: " + ex.Message);
        }
    }

    internal static HpRyzenTemperatureProbeIdentity Parse(byte[] raw)
    {
        ArgumentNullException.ThrowIfNull(raw);
        if (raw.Length < RawSmbiosHeaderLength)
        {
            return HpRyzenTemperatureProbeIdentity.Unavailable("RSMB data is shorter than RawSMBIOSData.");
        }

        uint declaredLength = BitConverter.ToUInt32(raw, 4);
        if (declaredLength != raw.Length - RawSmbiosHeaderLength)
        {
            return HpRyzenTemperatureProbeIdentity.Unavailable("RSMB declared table length does not match the returned buffer.");
        }

        string manufacturer = string.Empty;
        string model = string.Empty;
        string sku = string.Empty;
        string bios = string.Empty;
        int offset = RawSmbiosHeaderLength;
        while (offset < raw.Length)
        {
            if (!TryReadStructure(raw, offset, out byte type, out int formattedLength, out int nextOffset))
            {
                return HpRyzenTemperatureProbeIdentity.Unavailable("RSMB structure or string table was malformed.");
            }

            if (type == 0 && formattedLength >= 6)
            {
                if (!TryReadString(raw, offset, formattedLength, nextOffset, raw[offset + 4], out string value) ||
                    !TryReadString(raw, offset, formattedLength, nextOffset, raw[offset + 5], out bios))
                {
                    return HpRyzenTemperatureProbeIdentity.Unavailable("RSMB BIOS string indices were malformed.");
                }
            }
            else if (type == 1 && formattedLength >= 6)
            {
                if (!TryReadString(raw, offset, formattedLength, nextOffset, raw[offset + 4], out manufacturer) ||
                    !TryReadString(raw, offset, formattedLength, nextOffset, raw[offset + 5], out model))
                {
                    return HpRyzenTemperatureProbeIdentity.Unavailable("RSMB system string indices were malformed.");
                }

                if (formattedLength > 0x19 && !TryReadString(raw, offset, formattedLength, nextOffset, raw[offset + 0x19], out sku))
                {
                    return HpRyzenTemperatureProbeIdentity.Unavailable("RSMB system SKU string index was malformed.");
                }
            }

            offset = nextOffset;
            if (type == 127) break;
        }

        if (string.IsNullOrWhiteSpace(manufacturer) || string.IsNullOrWhiteSpace(model) || string.IsNullOrWhiteSpace(sku))
        {
            return HpRyzenTemperatureProbeIdentity.Unavailable("RSMB Type 1 did not contain manufacturer, product, and SKU strings.");
        }

        return new(HpRyzenTemperatureProbeIdentitySource.NativeSmbios, manufacturer, model, sku, bios,
            "Raw SMBIOS RSMB Type 0/1 parsed read-only.");
    }

    private static bool TryReadStructure(byte[] raw, int offset, out byte type, out int formattedLength, out int nextOffset)
    {
        type = 0;
        formattedLength = 0;
        nextOffset = 0;
        if (offset < RawSmbiosHeaderLength || raw.Length - offset < 4) return false;

        type = raw[offset];
        formattedLength = raw[offset + 1];
        if (formattedLength < 4 || formattedLength > raw.Length - offset) return false;

        int cursor = offset + formattedLength;
        while (cursor + 1 < raw.Length && (raw[cursor] != 0 || raw[cursor + 1] != 0)) cursor++;
        if (cursor + 1 >= raw.Length) return false;

        nextOffset = cursor + 2;
        return true;
    }

    private static bool TryReadString(byte[] raw, int structureOffset, int formattedLength, int nextOffset, byte index, out string value)
    {
        value = string.Empty;
        if (index == 0) return true;

        int cursor = structureOffset + formattedLength;
        int found = 0;
        while (cursor < nextOffset - 1)
        {
            int start = cursor;
            while (cursor < nextOffset - 1 && raw[cursor] != 0) cursor++;
            found++;
            if (found == index)
            {
                value = Encoding.ASCII.GetString(raw, start, cursor - start).Trim();
                return !string.IsNullOrWhiteSpace(value);
            }

            cursor++;
        }

        return false;
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern uint GetSystemFirmwareTable(uint firmwareTableProviderSignature, uint firmwareTableId,
        [Out] byte[]? firmwareTableBuffer, uint bufferSize);
}
