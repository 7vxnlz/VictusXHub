using System.Buffers.Binary;
using Xunit;

namespace VictusXHub.Tests.Packaging;

public sealed class VictusXHubIconAssetTests
{
    private static readonly int[] ExpectedSizes = [16, 20, 24, 32, 40, 48, 64, 128, 256];

    [Fact]
    public void ApprovedIcon_IsACompletePngBackedWindowsIcon()
    {
        string root = FindRepositoryRoot();
        string iconPath = Path.Combine(root, "app", "Assets", "VictusXHub.ico");
        byte[] icon = File.ReadAllBytes(iconPath);

        Assert.True(icon.Length > 6);
        Assert.Equal((ushort)0, BinaryPrimitives.ReadUInt16LittleEndian(icon.AsSpan(0, 2)));
        Assert.Equal((ushort)1, BinaryPrimitives.ReadUInt16LittleEndian(icon.AsSpan(2, 2)));
        int count = BinaryPrimitives.ReadUInt16LittleEndian(icon.AsSpan(4, 2));
        Assert.Equal(ExpectedSizes.Length, count);

        var actualSizes = new List<int>();
        for (int index = 0; index < count; index++)
        {
            int entryOffset = 6 + (16 * index);
            int width = icon[entryOffset] == 0 ? 256 : icon[entryOffset];
            int height = icon[entryOffset + 1] == 0 ? 256 : icon[entryOffset + 1];
            int frameLength = checked((int)BinaryPrimitives.ReadUInt32LittleEndian(icon.AsSpan(entryOffset + 8, 4)));
            int frameOffset = checked((int)BinaryPrimitives.ReadUInt32LittleEndian(icon.AsSpan(entryOffset + 12, 4)));

            Assert.Equal(width, height);
            Assert.InRange(frameOffset, 6 + (16 * count), icon.Length - 1);
            Assert.InRange(frameLength, 8, icon.Length - frameOffset);
            Assert.Equal(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }, icon.AsSpan(frameOffset, 8).ToArray());
            actualSizes.Add(width);
        }

        Assert.Equal(ExpectedSizes, actualSizes);
    }

    [Fact]
    public void ApprovedSourceAndObsoleteVariants_HaveExpectedRepositoryState()
    {
        string assets = Path.Combine(FindRepositoryRoot(), "app", "Assets");

        Assert.True(File.Exists(Path.Combine(assets, "VictusXHub.Source.png")));
        Assert.True(File.Exists(Path.Combine(assets, "VictusXHub.ico")));
        Assert.False(File.Exists(Path.Combine(assets, "VictusXHub.Silent.ico")));
        Assert.False(File.Exists(Path.Combine(assets, "VictusXHub.Balanced.ico")));
        Assert.False(File.Exists(Path.Combine(assets, "VictusXHub.Turbo.ico")));
    }

    private static string FindRepositoryRoot()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "VictusXHub.sln")))
                return directory.FullName;

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate the VictusXHub repository root.");
    }
}
