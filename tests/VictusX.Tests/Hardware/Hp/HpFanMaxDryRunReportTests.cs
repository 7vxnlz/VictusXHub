using System.Text.Json;
using GHelper.Hardware.Hp;
using Xunit;

namespace VictusX.Tests.Hardware.Hp;

public sealed class HpFanMaxDryRunReportTests
{
    [Fact]
    public void DefaultReport_IsEvaluatedAndBlockedByMissingDeviceValidation()
    {
        HpFanMaxDryRunReport report = HpFanMaxDryRunReport.CreateDefaultBlocked();

        Assert.False(report.SetFanMaxWriteImplemented);
        Assert.False(report.SetFanMaxWriteAllowed);
        Assert.True(report.SetFanMaxDryRunEvaluated);
        Assert.Null(report.SetFanMaxDeviceValidatedInputLength);
        Assert.Contains(
            nameof(HpFanMaxDeviceValidationStopReason.NoDeviceValidatedInputLength),
            report.SetFanMaxDryRunBlockedReasons);
        Assert.Contains("Device-validate exactly one", report.SetFanMaxNextRequiredProof, StringComparison.Ordinal);
    }

    [Fact]
    public void FullySatisfiedEvidence_StillCannotAllowOrImplementWrites()
    {
        HpFanMaxDryRunReport report = HpFanMaxDryRunReport.FromEvidence(new HpFanMaxDeviceValidationEvidence
        {
            HasObservedOneByteReferenceEvidence = true,
            IsOneByteShapeDeviceValidated = true,
            HasOneByteRestoreDisableEvidence = true,
            IsFanMaxGetReadbackAvailable = true,
            BaselineMaxFanEnabled = false,
            HasEnableVerificationReadbackPlan = true,
            HasRestoreVerificationReadbackPlan = true,
            HasHumanReviewedReferenceEvidence = true,
            HasHumanConfirmedSelectedInputLength = true,
            HasHumanApprovedRecoveryPlan = true
        });

        Assert.False(report.SetFanMaxWriteImplemented);
        Assert.False(report.SetFanMaxWriteAllowed);
        Assert.Equal(1, report.SetFanMaxDeviceValidatedInputLength);
        Assert.Empty(report.SetFanMaxDryRunBlockedReasons);
        Assert.Contains("separate human safety review", report.SetFanMaxNextRequiredProof, StringComparison.Ordinal);
    }

    [Fact]
    public void Report_SerializesTheExpectedSafeFieldNamesAndValues()
    {
        string json = JsonSerializer.Serialize(HpFanMaxDryRunReport.CreateDefaultBlocked());
        using JsonDocument document = JsonDocument.Parse(json);
        JsonElement root = document.RootElement;

        Assert.Equal(2, HpVictusCapabilityReportMetadata.SchemaVersion);
        Assert.All(
            HpVictusCapabilityReportMetadata.SetFanMaxSchemaV2ExperimentalStatusFields,
            field => Assert.True(root.TryGetProperty(field, out _), field));
        Assert.False(root.GetProperty("SetFanMaxWriteImplemented").GetBoolean());
        Assert.False(root.GetProperty("SetFanMaxWriteAllowed").GetBoolean());
        Assert.Equal("NO-GO", root.GetProperty("SetFanMaxFirstWriteGateStatus").GetString());
        Assert.False(root.GetProperty("SetFanMaxFirstWriteGateSatisfied").GetBoolean());
        Assert.Contains("Missing exact-device proof", root.GetProperty("SetFanMaxFirstWriteGateReason").GetString(), StringComparison.Ordinal);
        Assert.Equal("FourByte", root.GetProperty("SetFanMaxExperimentalPayloadCandidate").GetString());
        Assert.True(root.GetProperty("SetFanMaxPhysicalResponseObserved").GetBoolean());
        Assert.Equal(2, root.GetProperty("SetFanMaxPhysicalResponseConfirmationCount").GetInt32());
        Assert.False(root.GetProperty("SetFanMaxReadbackReliable").GetBoolean());
        Assert.True(root.GetProperty("SetFanMaxDeveloperExperimentAllowed").GetBoolean());
        Assert.Equal("FourByte", root.GetProperty("SetFanMaxDeveloperExperimentPayload").GetString());
        Assert.False(root.GetProperty("SetFanMaxNormalControlValidated").GetBoolean());
        Assert.False(root.GetProperty("SetFanMaxUserFacingControlAllowed").GetBoolean());
        Assert.True(root.GetProperty("SetFanMaxDryRunEvaluated").GetBoolean());
        Assert.Equal(JsonValueKind.Null, root.GetProperty("SetFanMaxDeviceValidatedInputLength").ValueKind);
        Assert.Equal(JsonValueKind.Array, root.GetProperty("SetFanMaxDryRunBlockedReasons").ValueKind);
        Assert.Equal(JsonValueKind.String, root.GetProperty("SetFanMaxNextRequiredProof").ValueKind);
    }

    [Fact]
    public void ReportModel_HasNoWmiDependencyOrInvocationSurface()
    {
        Type reportType = typeof(HpFanMaxDryRunReport);

        Assert.DoesNotContain(
            reportType.GetMethods(),
            method => method.Name.Contains("Invoke", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(
            reportType.GetMethods().SelectMany(method => method.GetParameters()),
            parameter => parameter.ParameterType.Namespace?.Contains("Management", StringComparison.Ordinal) == true);
    }
}
