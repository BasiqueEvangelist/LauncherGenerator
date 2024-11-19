namespace LauncherGenerator;

public interface ITargetTransformer
{
    Task<TransformedTarget> Transform(Target from);
}

public struct TransformedTarget
{
    public Target From;
    public string VersionID;
}

public class AllTransformer : ITargetTransformer
{
    private FabricTransformer fabric = new FabricTransformer();
    private QuiltTransformer quilt = new QuiltTransformer();

    public async Task<TransformedTarget> Transform(Target from)
    {
        switch (from.Transformer)
        {
            case "fabric":
                return await fabric.Transform(from);
            case "quilt":
                return await quilt.Transform(from);
            case "none":
                return new TransformedTarget { From = from, VersionID = from.VersionID };
            default:
                {
                    Log.Error("Invalid transformer " + from.Transformer);
                    Environment.Exit(1);
                    throw new NotImplementedException();
                }
        }
    }
}