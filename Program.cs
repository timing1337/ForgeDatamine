namespace ForgeDatamine;

internal class Program
{
    static void Main(string[] args)
    {
        using var forge = Forge.Read(@"E:\Downloads\Y8S4_DeepFreeze\datapc64_mtx_set01_bnk_mesh.forge");
        foreach (var file in forge.Files.Values) {
            var extractedFile = forge.ExtractFile(file.uid);
            Console.WriteLine(Convert.ToHexString(extractedFile.Data.ToArray()));
        }
    }
}
