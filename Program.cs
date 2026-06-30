namespace ForgeDatamine;

internal class Program
{
    static void Main(string[] args)
    {
        using var forge = Forge.Read(@"E:\Downloads\Y8S4_DeepFreeze\datapc64.forge");

        var extractedFile = forge.GetContainer(2048);
        foreach (var file in extractedFile.Entries) {
            Console.WriteLine("Found file: UID = {0}, Size = {1}", file.Key, file.Value.Size);
        }
    }
}
