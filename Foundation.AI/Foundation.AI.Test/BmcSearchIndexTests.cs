using BMC.AI;
using Foundation.BMC.Database;

namespace Foundation.AI.Test;

/// <summary>
/// Tests for BmcSearchIndex text description builders.
/// These are pure functions — no DB or AI services required.
/// </summary>
public class BmcSearchIndexTests
{
    [Fact]
    public void BuildPartDescription_BasicPart_IncludesTitle()
    {
        var part = new BrickPart
        {
            name = "Brick 2 x 4",
            ldrawTitle = "Brick  2 x  4",
            ldrawPartId = "3001",
            brickCategory = new BrickCategory { name = "Brick", description = "Standard bricks" }
        };

        var desc = BmcSearchIndex.BuildPartDescription(part);

        Assert.Contains("Brick  2 x  4", desc);
        Assert.Contains("Category: Brick", desc);
        Assert.Contains("LDraw ID: 3001", desc);
    }

    [Fact]
    public void BuildPartDescription_GearPart_IncludesTeethAndRatio()
    {
        var part = new BrickPart
        {
            name = "Technic Gear 24 Tooth",
            ldrawTitle = "Technic Gear 24 Tooth",
            ldrawPartId = "3648",
            toothCount = 24,
            gearRatio = 1.0f,
            brickCategory = new BrickCategory { name = "Gear", description = "Technic gears" }
        };

        var desc = BmcSearchIndex.BuildPartDescription(part);

        Assert.Contains("Teeth: 24", desc);
        Assert.Contains("Gear ratio: 1.00", desc);
        Assert.Contains("Category: Gear", desc);
    }

    [Fact]
    public void BuildPartDescription_WithDimensions_IncludesSize()
    {
        var part = new BrickPart
        {
            name = "Beam 5",
            ldrawTitle = "Technic Beam 5",
            ldrawPartId = "32316",
            widthLdu = 160f,
            heightLdu = 20f,
            depthLdu = 20f,
            brickCategory = new BrickCategory { name = "Beam", description = "Technic beams" }
        };

        var desc = BmcSearchIndex.BuildPartDescription(part);

        Assert.Contains("Dimensions: 160 × 20 × 20 LDU", desc);
    }

    [Fact]
    public void BuildPartDescription_WithKeywords_IncludesKeywords()
    {
        var part = new BrickPart
        {
            name = "Slope 45 2 x 2",
            ldrawPartId = "3039",
            keywords = "slope, roof, angle, 45 degrees",
            brickCategory = new BrickCategory { name = "Slope", description = "Angled slope bricks" }
        };

        var desc = BmcSearchIndex.BuildPartDescription(part);

        Assert.Contains("Keywords: slope, roof, angle, 45 degrees", desc);
    }

    [Fact]
    public void BuildPartDescription_FallsBackToName_WhenNoLdrawTitle()
    {
        var part = new BrickPart
        {
            name = "Custom Import Part",
            ldrawTitle = "",
            ldrawPartId = "99999"
        };

        var desc = BmcSearchIndex.BuildPartDescription(part);

        Assert.StartsWith("Custom Import Part", desc);
    }

    [Fact]
    public void BuildSetDescription_BasicSet_IncludesAllFields()
    {
        var set = new LegoSet
        {
            name = "Liebherr Crawler Crane LR 13000",
            setNumber = "42146-1",
            year = 2023,
            partCount = 2883,
            legoTheme = new LegoTheme { name = "Technic", description = "LEGO Technic" }
        };

        var desc = BmcSearchIndex.BuildSetDescription(set);

        Assert.Contains("Liebherr Crawler Crane LR 13000", desc);
        Assert.Contains("42146-1", desc);
        Assert.Contains("2023", desc);
        Assert.Contains("2883", desc);
        Assert.Contains("Theme: Technic", desc);
    }

    [Fact]
    public void BuildSetDescription_NoTheme_StillWorks()
    {
        var set = new LegoSet
        {
            name = "Mystery Set",
            setNumber = "00001-1",
            year = 2020,
            partCount = 100
        };

        var desc = BmcSearchIndex.BuildSetDescription(set);

        Assert.Contains("Mystery Set", desc);
        Assert.Contains("2020", desc);
        Assert.DoesNotContain("Theme:", desc);
    }
}
