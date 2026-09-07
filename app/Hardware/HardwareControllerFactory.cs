public static class HardwareControllerFactory
{
    public static AsusACPI CreateDefaultController()
    {
        return CreateAsusController();
    }

    public static AsusACPI CreateAsusController()
    {
        return new AsusACPI();
    }

    public static IHardwareController CreateUnsupportedController()
    {
        return new UnsupportedHardwareController();
    }

    public static IHardwareController CreateHpVictusController()
    {
        return new HpVictusHardwareController();
    }

    public static IHardwareController CreateController(bool useUnsupportedController)
    {
        return useUnsupportedController
            ? CreateUnsupportedController()
            : CreateDefaultController();
    }

    public static IHardwareController CreateController(bool useUnsupportedController, bool useHpVictusController)
    {
        return useHpVictusController
            ? CreateHpVictusController()
            : CreateController(useUnsupportedController);
    }
}


