using Libplanet;
using Libplanet.Crypto;

namespace NineChronicles.CLI;

class Program
{
    static async Task Main(string[] args)
    {
        var pk = await Address.GetKey();
        if (await Graphql.GetNextTxNonce(pk.ToAddress()) == 0)
        {
            Console.WriteLine("Account activation required. Please input activation code");
            var activationCode = Console.ReadLine();
            string? txId = await Address.ActivateAccount(pk, activationCode!);
            if (txId is null)
            {
                return;
            }

            string txResult = await Graphql.WaitTxMining(txId);
            if (txResult == "SUCCESS")
            {
                Console.WriteLine("Account activated.");
            }
            else
            {
                Console.WriteLine("Account activation failed. Please try with another activation code");
            }
        }
    }
}
